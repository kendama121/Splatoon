using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Splatoon.Infrastructure;
using Splatoon.Domain;
using Splatoon.Application;

namespace Splatoon.Presentation
{
    /// <summary>
    /// マップ画面表示。Tab長押しで全画面マップ展開、塗り状況+プレイヤー位置+スポーン位置可視化。
    /// マウスクリックで地点を選択しスーパージャンプ先指定。
    /// </summary>
    public class MapView : MonoBehaviour
    {
        /// <summary>シングルトン参照</summary>
        public static MapView Instance;

        /// <summary>マップRoot Panel(Tab中のみ表示)</summary>
        public GameObject MapPanel;
        /// <summary>塗装マップを表示するRawImage</summary>
        public RawImage MapImage;
        /// <summary>プレイヤーアイコン(オレンジ)</summary>
        public RectTransform PlayerMarker;
        /// <summary>BOTアイコン(青)</summary>
        public RectTransform BotMarker;
        /// <summary>Alphaスポーンアイコン</summary>
        public RectTransform AlphaSpawnMarker;
        /// <summary>Bravoスポーンアイコン</summary>
        public RectTransform BravoSpawnMarker;
        /// <summary>選択中のジャンプ先マーカー</summary>
        public RectTransform JumpTargetMarker;
        /// <summary>マップ案内テキスト</summary>
        public TextMeshProUGUI HintText;

        /// <summary>マップに対応するワールド座標範囲(中心0,0、半幅幅 ±X、±Z)</summary>
        public Vector2 WorldExtents = new Vector2(11f, 6f);

        /// <summary>追跡対象のPaintableSurface(床)</summary>
        public PaintableSurface GroundSurface;
        /// <summary>プレイヤーTransform参照</summary>
        public Transform PlayerTransform;
        /// <summary>BotTransform参照</summary>
        public Transform BotTransform;
        /// <summary>AlphaスポーンTransform</summary>
        public Transform AlphaSpawnTransform;
        /// <summary>BravoスポーンTransform</summary>
        public Transform BravoSpawnTransform;
        /// <summary>スーパージャンプアクション連携</summary>
        public SuperJumpAction SuperJump;

        // 内部
        bool _isOpen;
        InputAction _mapAction;
        InputAction _confirmAction;
        Vector3 _selectedTargetWorldPos;
        bool _hasTarget;

        void Awake()
        {
            Instance = this;

            // Tabキーでマップ表示
            _mapAction = new InputAction("Map", InputActionType.Button, "<Keyboard>/tab");
            _mapAction.Enable();
            // Enterまたは左クリックで確定
            _confirmAction = new InputAction("Confirm", InputActionType.Button, "<Keyboard>/enter");
            _confirmAction.AddBinding("<Mouse>/leftButton");
            _confirmAction.Enable();
        }

        void OnDestroy()
        {
            _mapAction?.Dispose();
            _confirmAction?.Dispose();
        }

        void Update()
        {
            // Tab長押し中マップ表示
            bool mapPressed = _mapAction.IsPressed();
            if (mapPressed != _isOpen)
            {
                _isOpen = mapPressed;
                if (MapPanel != null) MapPanel.SetActive(_isOpen);
                Cursor.lockState = _isOpen ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = _isOpen;
            }

            if (!_isOpen) return;

            // 塗装テクスチャ更新
            if (MapImage != null && GroundSurface != null && GroundSurface.MaskRT != null)
            {
                MapImage.texture = GroundSurface.MaskRT;
            }

            // マーカー位置更新
            UpdateMarker(PlayerMarker, PlayerTransform);
            UpdateMarker(BotMarker, BotTransform);
            UpdateMarker(AlphaSpawnMarker, AlphaSpawnTransform);
            UpdateMarker(BravoSpawnMarker, BravoSpawnTransform);

            // マウス位置でジャンプ先選択
            HandleTargetSelection();
        }

        /// <summary>
        /// マーカーをワールド座標→マップImage上のUV位置に変換配置。
        /// </summary>
        void UpdateMarker(RectTransform marker, Transform target)
        {
            if (marker == null || target == null || MapImage == null) return;

            // ワールド座標→[0,1]正規化(中心0,0、範囲±WorldExtents)
            float u = (target.position.x + WorldExtents.x) / (WorldExtents.x * 2f);
            float v = (target.position.z + WorldExtents.y) / (WorldExtents.y * 2f);
            u = Mathf.Clamp01(u);
            v = Mathf.Clamp01(v);

            // MapImageのRect内に配置
            var mapRT = MapImage.rectTransform;
            float w = mapRT.rect.width;
            float h = mapRT.rect.height;
            marker.anchoredPosition = new Vector2(
                (u - 0.5f) * w,
                (v - 0.5f) * h
            );
        }

        /// <summary>
        /// マウス位置でジャンプ先を選択。クリックで決定→SuperJumpへ伝達。
        /// </summary>
        void HandleTargetSelection()
        {
            if (MapImage == null) return;

            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(MapImage.rectTransform, Mouse.current.position.ReadValue(), null, out localPoint))
                return;

            var rect = MapImage.rectTransform.rect;
            if (!rect.Contains(localPoint)) return;

            // ローカル座標→[0,1]→ワールド座標
            float u = (localPoint.x / rect.width) + 0.5f;
            float v = (localPoint.y / rect.height) + 0.5f;
            float worldX = (u - 0.5f) * WorldExtents.x * 2f;
            float worldZ = (v - 0.5f) * WorldExtents.y * 2f;
            _selectedTargetWorldPos = new Vector3(worldX, 0.3f, worldZ);
            _hasTarget = true;

            // ジャンプ先マーカー表示
            if (JumpTargetMarker != null)
            {
                JumpTargetMarker.gameObject.SetActive(true);
                JumpTargetMarker.anchoredPosition = localPoint;
            }

            // ヒント表示
            if (HintText != null)
            {
                HintText.text = "[CLICK]/[ENTER] = SUPER JUMP!";
            }

            // クリックで確定発動
            if (_confirmAction.WasPressedThisFrame())
            {
                if (SuperJump != null && PlayerTransform != null)
                {
                    SuperJump.Execute(PlayerTransform, _selectedTargetWorldPos);
                }
                // マップ閉じる
                _isOpen = false;
                if (MapPanel != null) MapPanel.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
