using UnityEngine;
using UnityEngine.InputSystem;
using Splatoon.Domain;

namespace Splatoon.Presentation
{
    /// <summary>
    /// プレイヤー操作の中核MonoBehaviour。
    /// マウス&キーボード入力を受け取り、CharacterControllerで移動、ヒト/イカ形態切替を制御。
    /// CLAUDE.md準拠: nullチェック省略、private修飾子省略、日本語コメント。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        /// <summary>物理パラメータ設定(ScriptableObject、Inspectorから割当)</summary>
        public PlayerPhysicsConfig Config;

        /// <summary>カメラのTransform(マウス操作で回転、追従はCinemachineが担当)</summary>
        public Transform CameraTarget;

        /// <summary>ヒト形態モデルのルート(切替時にON/OFF)</summary>
        public GameObject HumanModel;

        /// <summary>イカ形態モデルのルート(切替時にON/OFF)</summary>
        public GameObject SquidModel;

        /// <summary>武器発射コンポーネント(同オブジェクトに付随、自動取得)</summary>
        public WeaponShooter Shooter;

        // 内部状態
        CharacterController _controller;
        Vector2 _moveInput;
        Vector2 _lookInput;
        Vector3 _velocity;
        float _cameraYaw;
        float _cameraPitch;
        bool _isSquidForm;
        bool _isGrounded;
        /// <summary>イカ形態か(WeaponShooterから参照)</summary>
        public bool IsSquidForm { get { return _isSquidForm; } }
        /// <summary>敵インク上にいるか(継続ダメージ・速度低下判定)</summary>
        public bool _isOnEnemyInk;

        // 入力アクション(InputSystem)
        InputAction _moveAction;
        InputAction _lookAction;
        InputAction _fireAction;
        InputAction _subAction;
        InputAction _squidAction;
        InputAction _jumpAction;
        InputAction _specialAction;

        /// <summary>
        /// 初期化処理。CharacterController取得、InputSystemのアクション直接生成。
        /// 後でInputActionAssetに切替予定。
        /// </summary>
        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (Shooter == null) Shooter = GetComponent<WeaponShooter>();

            // 子オブジェクトを名前で自動取得(古い参照が破棄されてた場合の保険)
            if (HumanModel == null || HumanModel.Equals(null))
            {
                var t = transform.Find("HumanModel");
                if (t != null) HumanModel = t.gameObject;
            }
            if (SquidModel == null || SquidModel.Equals(null))
            {
                var t = transform.Find("SquidModel");
                if (t != null) SquidModel = t.gameObject;
            }
            if (CameraTarget == null || CameraTarget.Equals(null))
            {
                CameraTarget = transform.Find("CameraTarget");
            }

            // InputSystemのアクションをコードで生成(MVPは簡易、後でInputActionAsset移行)
            _moveAction = new InputAction("Move", InputActionType.Value);
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            _lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
            _fireAction = new InputAction("Fire", InputActionType.Button, "<Mouse>/leftButton");
            _subAction = new InputAction("Sub", InputActionType.Button, "<Mouse>/rightButton");
            _squidAction = new InputAction("Squid", InputActionType.Button, "<Keyboard>/leftShift");
            _jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
            _specialAction = new InputAction("Special", InputActionType.Button, "<Keyboard>/q");

            // 全アクション有効化
            _moveAction.Enable();
            _lookAction.Enable();
            _fireAction.Enable();
            _subAction.Enable();
            _squidAction.Enable();
            _jumpAction.Enable();
            _specialAction.Enable();

            // カーソルロック(FPS/TPS定番)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// 毎フレーム処理。入力読取→カメラ回転→移動計算→形態切替判定。
        /// </summary>
        void Update()
        {
            // 入力読み取り
            _moveInput = _moveAction.ReadValue<Vector2>();
            _lookInput = _lookAction.ReadValue<Vector2>();

            // カメラYaw/Pitch更新(マウス感度反映)
            _cameraYaw += _lookInput.x * Config.CameraSensitivityX * 0.1f;
            _cameraPitch -= _lookInput.y * Config.CameraSensitivityY * 0.1f * (Config.IsInvertY ? -1f : 1f);
            _cameraPitch = Mathf.Clamp(_cameraPitch, -80f, 80f);
            CameraTarget.rotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0f);

            // 形態切替(Shift長押し中はイカ)
            UpdateForm(_squidAction.IsPressed());

            // 移動計算
            UpdateMovement();

            // ジャンプ
            if (_isGrounded && _jumpAction.WasPressedThisFrame())
            {
                _velocity.y = Config.JumpVelocity;
                // ジャンプSE
                if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayJump();
            }

            // 武器発射(左クリック長押し対応、ヒト形態のみ)
            if (Shooter != null && !_isSquidForm && _fireAction.IsPressed())
            {
                // カメラ前方を狙い、わずかに下方向補正
                Vector3 aimDir = CameraTarget.forward;
                Shooter.Fire(aimDir);
            }

            // サブウェポン投擲(右クリックWasPressedThisFrame)
            if (_subAction.WasPressedThisFrame() && !_isSquidForm)
            {
                var sub = GetComponent<SubWeaponAction>();
                if (sub != null) sub.Throw(CameraTarget.forward);
            }

            // スペシャル発動(Q WasPressedThisFrame)
            if (_specialAction.WasPressedThisFrame() && !_isSquidForm)
            {
                var sp = GetComponent<SpecialAction>();
                if (sp != null) sp.TryActivate();
            }
        }

        /// <summary>
        /// ヒト/イカ形態を切り替える。モデルとコライダーサイズを切替。
        /// </summary>
        void UpdateForm(bool isSquidPressed)
        {
            // 状態変化検出
            if (isSquidPressed == _isSquidForm) return;
            _isSquidForm = isSquidPressed;

            // モデル切替(null安全)
            if (HumanModel != null) HumanModel.SetActive(!_isSquidForm);
            if (SquidModel != null) SquidModel.SetActive(_isSquidForm);

            // CharacterControllerサイズ切替
            if (_isSquidForm)
            {
                _controller.height = Config.SquidColliderHeight;
                _controller.radius = Config.SquidColliderRadius;
                _controller.center = new Vector3(0, Config.SquidColliderHeight * 0.5f, 0);
            }
            else
            {
                _controller.height = Config.HumanColliderHeight;
                _controller.radius = Config.HumanColliderRadius;
                _controller.center = new Vector3(0, Config.HumanColliderHeight * 0.5f, 0);
            }
        }

        /// <summary>
        /// 移動処理。カメラ向き基準でWASD入力を世界空間に変換。
        /// イカ形態はスイム速度、ヒトは走行速度。重力適用。
        /// </summary>
        void UpdateMovement()
        {
            // カメラの水平方向ベクトル取得
            Vector3 forward = CameraTarget.forward;
            Vector3 right = CameraTarget.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            // 入力を世界空間に変換
            Vector3 moveDir = forward * _moveInput.y + right * _moveInput.x;

            // 速度選択(ヒト/イカ)
            float speed = _isSquidForm ? Config.SwimSpeedBase : Config.RunSpeedBase;

            // 自軍/敵軍インク判定で速度倍率を取得→反映
            var teamMember = GetComponent<TeamMember>();
            if (teamMember != null)
            {
                var (multiplier, isOnOwn, isOnEnemy) = Splatoon.Application.SwimSpeedModifier.GetSpeedMultiplier(
                    transform.position, teamMember.Team, _isSquidForm);
                speed *= multiplier;
                // 敵インクで継続ダメージ累積(MVP: HP無いので未反映だが、内部状態として保持)
                _isOnEnemyInk = isOnEnemy;
            }

            // 重力適用
            _isGrounded = _controller.isGrounded;
            if (_isGrounded && _velocity.y < 0f)
            {
                _velocity.y = -2f; // 接地時に軽く下に押し付け
            }
            _velocity.y -= Config.Gravity * Time.deltaTime;

            // 移動実行
            Vector3 finalMove = moveDir * speed + Vector3.up * _velocity.y;
            _controller.Move(finalMove * Time.deltaTime);

            // プレイヤー自身の向きを移動方向に(緩やかに)
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);
            }
        }

        /// <summary>
        /// 後始末: InputAction解放、カーソル復帰。
        /// </summary>
        void OnDestroy()
        {
            _moveAction.Dispose();
            _lookAction.Dispose();
            _fireAction.Dispose();
            _subAction.Dispose();
            _squidAction.Dispose();
            _jumpAction.Dispose();
            _specialAction.Dispose();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
