using UnityEngine;
using Splatoon.Domain;
using Splatoon.Application;

namespace Splatoon.Presentation
{
    /// <summary>
    /// インク弾の挙動。3ステート物理モデル(Straight→Brake→Free)で飛行。
    /// 着弾でInkPaintServiceに塗装委譲、プレイヤーヒットでダメージ与える。
    /// </summary>
    public class InkBullet : MonoBehaviour
    {
        /// <summary>武器データ(発射時に注入)</summary>
        public WeaponData Weapon;
        /// <summary>所属チーム</summary>
        public TeamId Team;
        /// <summary>速度ベクトル(発射時に設定、内部更新)</summary>
        public Vector3 Velocity;
        /// <summary>発射者(自分自身ヒット回避用)</summary>
        public GameObject Owner;

        // 物理状態
        enum BulletState { Straight, Brake, Free }
        BulletState _state = BulletState.Straight;
        int _stateFrames;
        // 1 DU/frame = m/s換算用係数
        const float DU_PER_FRAME_TO_MS = 60f * PlayerPhysicsConfig.DU_TO_METER;

        /// <summary>
        /// 弾の初期化。発射位置・方向・速度を設定。
        /// </summary>
        public void Initialize(WeaponData weapon, Vector3 origin, Vector3 direction, TeamId team, GameObject owner)
        {
            Weapon = weapon;
            Team = team;
            Owner = owner;
            transform.position = origin;
            // 武器初速をm/sで適用
            Velocity = direction.normalized * weapon.MuzzleVelocity;
            _state = BulletState.Straight;
            _stateFrames = 0;
            // 弾を消去するタイマー(最大寿命2秒)
            Destroy(gameObject, 2f);
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            _stateFrames++;

            // 状態遷移判定
            if (_state == BulletState.Straight && _stateFrames > Weapon.StraightFrames)
            {
                _state = BulletState.Brake;
            }
            else if (_state == BulletState.Brake && Velocity.y < -0.15f * DU_PER_FRAME_TO_MS)
            {
                _state = BulletState.Free;
            }

            // 状態別物理適用
            switch (_state)
            {
                case BulletState.Straight:
                    // 重力なし、定速直進(空気抵抗なし)
                    break;
                case BulletState.Brake:
                    // 空気抵抗 + 重力
                    Velocity *= (1f - Weapon.BrakeAirResistance * dt * 60f); // フレーム単位減衰をdtに変換
                    Velocity.y -= Weapon.BrakeGravity * DU_PER_FRAME_TO_MS * 60f * dt;
                    break;
                case BulletState.Free:
                    Velocity *= (1f - Weapon.FreeAirResistance * dt * 60f);
                    Velocity.y -= Weapon.FreeGravity * DU_PER_FRAME_TO_MS * 60f * dt;
                    break;
            }

            // 移動(レイキャストで貫通防止)
            Vector3 nextPos = transform.position + Velocity * dt;
            float distance = Velocity.magnitude * dt;
            if (Physics.Raycast(transform.position, Velocity.normalized, out RaycastHit hit, distance))
            {
                // ヒット対象が自分自身でなければ着弾処理
                if (hit.collider.gameObject != Owner)
                {
                    OnHit(hit);
                    return;
                }
            }
            transform.position = nextPos;
        }

        /// <summary>
        /// 着弾処理。塗装適用 + 飛沫VFX生成 + プレイヤーヒット時ダメージ + 自身破棄。
        /// </summary>
        void OnHit(RaycastHit hit)
        {
            // 床・壁の塗装
            InkPaintService.PaintAt(hit.point, Weapon.PaintRadius, Team);

            // スペシャル充電(発射者に塗装ポイント加算、半径2乗で塗り面積比例)
            if (Owner != null)
            {
                var ownerSpecial = Owner.GetComponent<Splatoon.Presentation.SpecialAction>();
                if (ownerSpecial != null)
                {
                    float chargeAmount = (Weapon.PaintRadius * Weapon.PaintRadius) * 0.012f;
                    ownerSpecial.AddCharge(chargeAmount);
                }
            }

            // プレイヤーヒット時のダメージ処理(自分自身は除外、敵チームのみ)
            var hitRoot = hit.collider.transform.root;
            var health = hitRoot.GetComponent<PlayerHealth>();
            var hitTeam = hitRoot.GetComponent<TeamMember>();
            if (health != null && hitTeam != null && hitTeam.Team != Team)
            {
                // ダメージ計算: フレーム経過でDamageMax→DamageMin線形補間
                int currentFrame = _stateFrames;
                float dmgT = Mathf.InverseLerp(Weapon.ReduceStartFrame, Weapon.ReduceEndFrame, currentFrame);
                float damage = Mathf.Lerp(Weapon.DamageMax, Weapon.DamageMin, dmgT);
                health.TakeDamage(damage, Team);
            }

            // 着弾飛沫VFX生成
            if (Splatoon.Infrastructure.InkPaintManager.Instance != null)
            {
                Color teamCol = Splatoon.Infrastructure.InkPaintManager.Instance.GetTeamColor(Team);
                SpawnImpactVFX(hit.point, hit.normal, teamCol);
            }

            // 着弾SE(近距離のみ、毎着弾だと音がうるさいので確率制御)
            if (Splatoon.Presentation.ProceduralAudio.Instance != null && UnityEngine.Random.value < 0.15f)
            {
                Splatoon.Presentation.ProceduralAudio.Instance.PlayImpact();
            }

            // TODO: プレイヤーヒット時のダメージ処理(Phase 6で実装)
            Destroy(gameObject);
        }

        /// <summary>
        /// 着弾飛沫VFX生成。短命パーティクルで散布感を演出。
        /// </summary>
        void SpawnImpactVFX(Vector3 pos, Vector3 normal, Color teamColor)
        {
            // 一時GameObjectでParticleSystemを生成し、寿命後に自動破棄
            var vfxGO = new GameObject("InkSplashVFX");
            vfxGO.transform.position = pos + normal * 0.05f;
            vfxGO.transform.rotation = Quaternion.LookRotation(normal);

            var ps = vfxGO.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.35f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
            main.startColor = teamColor;
            main.gravityModifier = 1.2f;
            main.maxParticles = 40;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.05f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(teamColor, 0f), new GradientColorKey(teamColor, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 1f);
            curve.AddKey(0.6f, 0.8f);
            curve.AddKey(1f, 0.2f);
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, curve);

            // 発光マテリアル設定(URP Particles Unlit)
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            var particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader != null)
            {
                var mat = new Material(particleShader);
                mat.SetColor("_BaseColor", teamColor * 2.5f); // HDR強発光
                renderer.material = mat;
            }

            ps.Play();
            // 自動破棄
            Destroy(vfxGO, 0.8f);
        }
    }
}
