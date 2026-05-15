using UnityEngine;
using Splatoon.Domain;

namespace Splatoon.Presentation
{
    /// <summary>
    /// 簡易BOT。ランダムにステージを歩き回り、定期的に発射。
    /// MVPの相手プレイヤーとして使用。後でNavMesh + 状況判断に拡張可能。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SimpleBot : MonoBehaviour
    {
        /// <summary>物理設定</summary>
        public PlayerPhysicsConfig Config;
        /// <summary>武器</summary>
        public WeaponShooter Shooter;
        /// <summary>歩行範囲(中心からの半径)</summary>
        public float WanderRadius = 6f;
        /// <summary>目標再選択間隔(秒)</summary>
        public float RetargetInterval = 3f;
        /// <summary>発射間隔ランダム範囲</summary>
        public Vector2 FireInterval = new Vector2(0.5f, 1.5f);

        CharacterController _cc;
        Vector3 _targetPos;
        float _retargetTimer;
        float _nextFireTime;
        Vector3 _velocity;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            PickNewTarget();
        }

        void Update()
        {
            // 目標再選択
            _retargetTimer += Time.deltaTime;
            if (_retargetTimer >= RetargetInterval)
            {
                _retargetTimer = 0f;
                PickNewTarget();
            }

            // 目標方向へ移動
            Vector3 toTarget = _targetPos - transform.position;
            toTarget.y = 0f;
            Vector3 moveDir = toTarget.normalized;
            float speed = Config.RunSpeedBase * 0.7f; // BOTは少し遅め

            // 重力適用
            if (_cc.isGrounded && _velocity.y < 0f) _velocity.y = -2f;
            _velocity.y -= Config.Gravity * Time.deltaTime;

            Vector3 finalMove = moveDir * speed + Vector3.up * _velocity.y;
            _cc.Move(finalMove * Time.deltaTime);

            // 移動方向に向く
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.deltaTime);
            }

            // 発射(BOT前方+下5度の方向)
            if (Time.time >= _nextFireTime && Shooter != null)
            {
                Vector3 aim = transform.forward + Vector3.down * 0.08f;
                Shooter.Fire(aim.normalized);
                _nextFireTime = Time.time + Random.Range(FireInterval.x, FireInterval.y);
            }
        }

        /// <summary>
        /// 新しい目標地点をランダム選択。
        /// </summary>
        void PickNewTarget()
        {
            Vector2 randomCircle = Random.insideUnitCircle * WanderRadius;
            _targetPos = new Vector3(randomCircle.x, transform.position.y, randomCircle.y);
        }
    }
}
