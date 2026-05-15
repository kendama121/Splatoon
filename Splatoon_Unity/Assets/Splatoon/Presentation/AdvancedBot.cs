using UnityEngine;
using Splatoon.Domain;
using Splatoon.Application;

namespace Splatoon.Presentation
{
    /// <summary>
    /// 高度BOT。視界内の敵を検出して狙う、距離による接近/後退、定期的なスペシャル使用。
    /// SimpleBotの上位版として置き換え可能。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class AdvancedBot : MonoBehaviour
    {
        /// <summary>物理設定</summary>
        public PlayerPhysicsConfig Config;
        /// <summary>武器発射</summary>
        public WeaponShooter Shooter;
        /// <summary>スペシャル発動</summary>
        public SpecialAction Special;
        /// <summary>所属チーム</summary>
        public TeamMember Member;
        /// <summary>敵検出範囲(m)</summary>
        public float DetectRange = 15f;
        /// <summary>射撃発動距離(m)</summary>
        public float FireRange = 8f;
        /// <summary>離れすぎ判定(接近開始)</summary>
        public float TooFarRange = 10f;
        /// <summary>近すぎ判定(後退開始)</summary>
        public float TooCloseRange = 3f;
        /// <summary>歩行ランダム範囲</summary>
        public float WanderRadius = 5f;
        /// <summary>目標再選択間隔(秒)</summary>
        public float RetargetInterval = 3f;
        /// <summary>発射間隔ランダム範囲</summary>
        public Vector2 FireInterval = new Vector2(0.3f, 0.8f);
        /// <summary>スペシャル発動充電率閾値</summary>
        public float SpecialThreshold = 0.95f;

        CharacterController _cc;
        Vector3 _targetPos;
        Transform _enemyTarget;
        float _retargetTimer;
        float _nextFireTime;
        Vector3 _velocity;
        float _nextSpecialChargeTime;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (Shooter == null) Shooter = GetComponent<WeaponShooter>();
            if (Special == null) Special = GetComponent<SpecialAction>();
            if (Member == null) Member = GetComponent<TeamMember>();
            PickNewTarget();
        }

        void Update()
        {
            // CharacterController無効化中は動かない(スプラット中・スーパージャンプ中)
            if (_cc == null || !_cc.enabled) return;
            // スプラット中も動かない
            var ph = GetComponent<PlayerHealth>();
            if (ph != null && ph.IsSplatted) return;

            // 敵検出
            FindEnemyTarget();

            // 目標位置決定
            Vector3 desiredPos;
            if (_enemyTarget != null)
            {
                float dist = Vector3.Distance(transform.position, _enemyTarget.position);
                Vector3 toEnemy = (_enemyTarget.position - transform.position).normalized;
                if (dist > TooFarRange) desiredPos = transform.position + toEnemy * 2f; // 接近
                else if (dist < TooCloseRange) desiredPos = transform.position - toEnemy * 2f; // 後退
                else desiredPos = transform.position; // 維持
            }
            else
            {
                _retargetTimer += Time.deltaTime;
                if (_retargetTimer >= RetargetInterval) { _retargetTimer = 0f; PickNewTarget(); }
                desiredPos = _targetPos;
            }

            // 移動
            Vector3 dirH = desiredPos - transform.position;
            dirH.y = 0f;
            Vector3 moveDir = dirH.normalized;
            float speed = Config.RunSpeedBase * 0.85f;

            if (_cc.isGrounded && _velocity.y < 0f) _velocity.y = -2f;
            _velocity.y -= Config.Gravity * Time.deltaTime;

            Vector3 finalMove = moveDir * speed + Vector3.up * _velocity.y;
            _cc.Move(finalMove * Time.deltaTime);

            // 向き(敵がいれば敵方向、無ければ移動方向)
            Vector3 lookDir = _enemyTarget != null
                ? (_enemyTarget.position - transform.position)
                : moveDir;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.deltaTime);
            }

            // 発射(敵が射程内のみ)
            if (_enemyTarget != null && Time.time >= _nextFireTime && Shooter != null)
            {
                float d = Vector3.Distance(transform.position, _enemyTarget.position);
                if (d <= FireRange)
                {
                    Vector3 aim = (_enemyTarget.position + Vector3.up * 0.8f - transform.position).normalized;
                    Shooter.Fire(aim);
                    _nextFireTime = Time.time + Random.Range(FireInterval.x, FireInterval.y);
                }
            }

            // スペシャル発動(充電満タン+敵がいる時)
            if (Special != null && Special.Charge >= SpecialThreshold && _enemyTarget != null && Time.time >= _nextSpecialChargeTime)
            {
                Special.TryActivate();
                _nextSpecialChargeTime = Time.time + 10f;
            }
            // 塗りで充電(MVPは時間経過で充電)
            if (Special != null) Special.AddCharge(Time.deltaTime * 0.04f);
        }

        /// <summary>視界内の敵プレイヤー検出</summary>
        void FindEnemyTarget()
        {
            _enemyTarget = null;
            float bestDist = DetectRange;
            // シーン内のTeamMember全部取得
            var allMembers = Object.FindObjectsByType<TeamMember>(FindObjectsSortMode.None);
            foreach (var m in allMembers)
            {
                if (m == Member) continue;
                if (m.Team == Member.Team) continue;
                // スプラット中の敵は除外
                var ph = m.GetComponent<PlayerHealth>();
                if (ph != null && ph.IsSplatted) continue;
                float d = Vector3.Distance(transform.position, m.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    _enemyTarget = m.transform;
                }
            }
        }

        void PickNewTarget()
        {
            Vector2 randomCircle = Random.insideUnitCircle * WanderRadius;
            _targetPos = new Vector3(randomCircle.x, transform.position.y, randomCircle.y);
        }
    }
}
