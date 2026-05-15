using UnityEngine;
using Splatoon.Domain;
using Splatoon.Infrastructure;
using Splatoon.Application;

namespace Splatoon.Presentation
{
    /// <summary>
    /// スペシャル発動処理。スペシャルゲージ満タン時にQで発動。
    /// カテゴリ別に異なるエフェクト+塗装+ダメージを生成する。
    /// </summary>
    public class SpecialAction : MonoBehaviour
    {
        /// <summary>装備中スペシャル</summary>
        public SpecialData Equipped;
        /// <summary>所属チーム</summary>
        public TeamMember Member;
        /// <summary>カメラ方向(発射方向用)</summary>
        public Transform AimSource;
        /// <summary>スペシャルゲージ充電量(0-1)</summary>
        public float Charge = 0f;
        /// <summary>充電完了フラグ</summary>
        public bool IsCharged { get { return Charge >= 1f; } }

        void Awake()
        {
            // null安全: 自動取得(参照が壊れた場合の保険)
            if (Member == null || Member.Equals(null)) Member = GetComponent<TeamMember>();
            if (AimSource == null || AimSource.Equals(null))
            {
                var t = transform.Find("CameraTarget");
                if (t != null) AimSource = t;
                else AimSource = transform;
            }
        }

        /// <summary>
        /// 塗りで充電(InkBulletの着弾等から呼び出し)。
        /// </summary>
        public void AddCharge(float amount)
        {
            Charge = Mathf.Clamp01(Charge + amount);
        }

        /// <summary>
        /// 発動。Q押下時に呼び出し。
        /// </summary>
        public bool TryActivate()
        {
            if (Equipped == null || !IsCharged) return false;
            // AimSource null安全
            if (AimSource == null) AimSource = transform.Find("CameraTarget") ?? transform;
            if (Member == null) Member = GetComponent<TeamMember>();
            if (Member == null) return false;
            Charge = 0f;

            Color teamCol = InkPaintManager.Instance != null
                ? InkPaintManager.Instance.GetTeamColor(Member.Team)
                : Color.white;

            switch (Equipped.Category)
            {
                case SpecialCategory.UltraShot:
                    SpawnUltraShot(teamCol);
                    break;
                case SpecialCategory.MultiMissile:
                    SpawnMultiMissile(teamCol);
                    break;
                case SpecialCategory.InkStorm:
                    SpawnInkStorm(teamCol);
                    break;
                case SpecialCategory.BooyahBomb:
                    SpawnBooyahBomb(teamCol);
                    break;
                case SpecialCategory.KillerWail:
                    SpawnKillerWail(teamCol);
                    break;
                case SpecialCategory.UltraStamp:
                    SpawnUltraStamp(teamCol);
                    break;
                case SpecialCategory.TripleTornado:
                    SpawnTripleTornado(teamCol);
                    break;
                case SpecialCategory.Reefslider:
                    SpawnReefslider(teamCol);
                    break;
                case SpecialCategory.UltraStomp:
                    SpawnUltraStomp(teamCol);
                    break;
                case SpecialCategory.CrabTank:
                    SpawnCrabTank(teamCol);
                    break;
                default:
                    SpawnGenericEffect(teamCol);
                    break;
            }

            if (CameraShaker.Instance != null) CameraShaker.Instance.Shake(0.3f, 0.4f);
            return true;
        }

        // ============== 各スペシャル発動エフェクト(MVP簡略版) ==============

        /// <summary>ウルトラショット: 前方へ巨大インク弾を発射</summary>
        void SpawnUltraShot(Color col)
        {
            Vector3 origin = AimSource.position + AimSource.forward * 1f;
            Vector3 dir = AimSource.forward;
            // 巨大弾を9方向(3列×3段、螺旋風)
            for (int i = 0; i < 9; i++)
            {
                float h = ((i % 3) - 1) * 0.15f;
                float v = ((i / 3) - 1) * 0.1f;
                Vector3 d = (dir + AimSource.right * h + AimSource.up * v).normalized;
                ShootBigInk(origin, d, col, 2.5f, 18f);
            }
            // スペシャル発動時 大型VFX
            SpawnExplosionVFX(origin, col, 3f);
        }

        /// <summary>マルチミサイル: 上方ミサイル一斉発射(MVP: 5発)</summary>
        void SpawnMultiMissile(Color col)
        {
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            for (int i = 0; i < 15; i++) // 5発→15発
            {
                Vector3 target = transform.position + new Vector3(
                    UnityEngine.Random.Range(-18f, 18f),
                    0.3f,
                    UnityEngine.Random.Range(-12f, 12f));
                StartCoroutine(LaunchMissile(origin, target, col, i * 0.08f));
            }
        }

        /// <summary>アメフラシ: 範囲にインク雨を3秒間降らせる</summary>
        void SpawnInkStorm(Color col)
        {
            // 3地点で同時に雨
            for (int i = 0; i < 3; i++) {
                Vector3 spot = transform.position + AimSource.forward * 5f + new Vector3((i-1) * 4f, 0, (i-1) * 3f);
                spot.y = 0.3f;
                StartCoroutine(RainInk(spot, col, 8f));
            }
        }

        /// <summary>ナイスダマ: 自身周囲を爆発塗装</summary>
        void SpawnBooyahBomb(Color col)
        {
            Vector3 pos = transform.position;
            // 巨大3重爆発
            InkPaintService.PaintAt(pos, 7f, Member.Team);
            SpawnExplosionVFX(pos, col, 7f);
            // 周辺ダメージ
            Collider[] hits = Physics.OverlapSphere(pos, 7f);
            foreach (var c in hits) {
                var ph = c.GetComponentInParent<PlayerHealth>();
                if (ph == null) continue;
                var tm = ph.GetComponent<TeamMember>();
                if (tm == null || tm.Team == Member.Team) continue;
                ph.TakeDamage(380f, Member.Team);
            }
            // 連鎖VFX
            for (int i = 0; i < 8; i++) {
                float ang = i * 45f * Mathf.Deg2Rad;
                Vector3 p = pos + new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * 4f;
                SpawnExplosionVFX(p, col, 2.5f);
                InkPaintService.PaintAt(p, 3f, Member.Team);
            }
        }

        /// <summary>メガホンレーザー: 前方扇形塗装</summary>
        void SpawnKillerWail(Color col)
        {
            // 12発扇形+全方位ホップソナー風波動
            Vector3 origin = transform.position + Vector3.up * 1f;
            for (int i = 0; i < 12; i++)
            {
                float angle = (i - 5.5f) * 8f;
                Vector3 dir = Quaternion.Euler(0, angle, 0) * AimSource.forward;
                ShootBigInk(origin, dir, col, 0.9f, 18f);
            }
            // 波動Ring
            StartCoroutine(SoundRing(origin, col));
        }
        System.Collections.IEnumerator SoundRing(Vector3 origin, Color col) {
            for (int wave = 0; wave < 5; wave++) {
                yield return new WaitForSeconds(0.15f);
                SpawnExplosionVFX(origin + Vector3.up * 0.5f, col, 1.5f + wave);
            }
        }

        /// <summary>ウルトラハンコ: 前方を地面塗装</summary>
        void SpawnUltraStamp(Color col)
        {
            // 20マス突進+ジャイアントスタンプVFX
            Vector3 start = transform.position;
            Vector3 forward = AimSource.forward;
            forward.y = 0; forward.Normalize();
            for (int i = 1; i <= 20; i++)
            {
                Vector3 pos = start + forward * i * 0.8f;
                pos.y = 0.3f;
                InkPaintService.PaintAt(pos, 1.5f, Member.Team);
                if (i % 3 == 0) SpawnExplosionVFX(pos, col, 1.8f);
            }
        }

        /// <summary>トリプルトルネード: 3地点に竜巻塗装</summary>
        void SpawnTripleTornado(Color col)
        {
            // 3地点に巨大竜巻+周辺塗装
            for (int i = 0; i < 3; i++)
            {
                Vector3 pos = transform.position + new Vector3(
                    UnityEngine.Random.Range(-15f, 15f), 0.3f,
                    UnityEngine.Random.Range(-10f, 10f));
                InkPaintService.PaintAt(pos, 5f, Member.Team);
                SpawnExplosionVFX(pos, col, 5f);
                // 螺旋VFX
                StartCoroutine(SpiralTornado(pos, col));
            }
        }
        System.Collections.IEnumerator SpiralTornado(Vector3 center, Color col) {
            for (int h = 0; h < 8; h++) {
                float t = h * 0.1f;
                yield return new WaitForSeconds(0.08f);
                float ang = h * 0.7f;
                Vector3 p = center + new Vector3(Mathf.Cos(ang) * 1.5f, h * 0.5f, Mathf.Sin(ang) * 1.5f);
                SpawnExplosionVFX(p, col, 1f);
            }
        }

        /// <summary>サメライド: 前方へ突撃→大爆発</summary>
        void SpawnReefslider(Color col)
        {
            Vector3 forward = AimSource.forward;
            forward.y = 0; forward.Normalize();
            Vector3 endPos = transform.position + forward * 12f;
            endPos.y = 0.3f;
            // 12マス長距離突撃ルート塗装
            for (int i = 1; i <= 12; i++)
            {
                Vector3 p = transform.position + forward * i;
                p.y = 0.3f;
                InkPaintService.PaintAt(p, 2.0f, Member.Team);
                if (i % 2 == 0) SpawnExplosionVFX(p, col, 1.5f);
            }
            // 着地で巨大爆発
            InkPaintService.PaintAt(endPos, 3.5f, Member.Team);
            SpawnExplosionVFX(endPos, col, 3.5f);
        }

        /// <summary>ウルトラチャクチ: 自分の真下を巨大塗装</summary>
        void SpawnUltraStomp(Color col)
        {
            Vector3 pos = transform.position;
            InkPaintService.PaintAt(pos, 3.5f, Member.Team);
            SpawnExplosionVFX(pos, col, 3.5f);
        }

        /// <summary>カニタンク: 前方に連続塗装弾(8連)</summary>
        void SpawnCrabTank(Color col)
        {
            // 30発バースト+カノン砲(大型2発)
            Vector3 origin = transform.position + Vector3.up * 1f;
            for (int i = 0; i < 30; i++)
            {
                Vector3 dir = AimSource.forward + UnityEngine.Random.insideUnitSphere * 0.2f;
                ShootBigInk(origin, dir.normalized, col, 0.6f, 16f);
            }
            // カノン砲(巨大弾2発)
            ShootBigInk(origin, AimSource.forward, col, 2.5f, 14f);
            ShootBigInk(origin + Vector3.up * 0.5f, AimSource.forward, col, 2.5f, 14f);
        }

        /// <summary>汎用エフェクト(未実装スペシャルの代替)</summary>
        void SpawnGenericEffect(Color col)
        {
            Vector3 pos = transform.position + AimSource.forward * 3f;
            pos.y = 0.3f;
            InkPaintService.PaintAt(pos, 3f, Member.Team);
            SpawnExplosionVFX(pos, col, 3f);
        }

        // ============== ヘルパー ==============

        void ShootBigInk(Vector3 origin, Vector3 dir, Color col, float radius, float speed)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "SpecialInkBall";
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.position = origin;
            go.transform.localScale = Vector3.one * (radius * 0.5f);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", col);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", col * 4f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var travel = go.AddComponent<SpecialProjectile>();
            travel.Velocity = dir * speed;
            travel.PaintRadius = radius;
            travel.Team = Member.Team;
            travel.TeamColor = col;
        }

        System.Collections.IEnumerator LaunchMissile(Vector3 origin, Vector3 target, Color col, float delay)
        {
            yield return new WaitForSeconds(delay);
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.position = origin;
            go.transform.localScale = Vector3.one * 0.3f;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", col);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", col * 3f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            float t = 0; float dur = 1.2f;
            Vector3 mid = (origin + target) * 0.5f + Vector3.up * 6f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float p = t / dur;
                // 2次ベジエ放物線
                Vector3 a = Vector3.Lerp(origin, mid, p);
                Vector3 b = Vector3.Lerp(mid, target, p);
                go.transform.position = Vector3.Lerp(a, b, p);
                yield return null;
            }
            InkPaintService.PaintAt(target, 2.5f, Member.Team);
            SpawnExplosionVFX(target, col, 2.5f);
            Destroy(go);
        }

        System.Collections.IEnumerator RainInk(Vector3 center, Color col, float duration)
        {
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                if (UnityEngine.Random.value < 0.2f)
                {
                    Vector3 drop = center + UnityEngine.Random.insideUnitSphere * 3f;
                    drop.y = 0.3f;
                    InkPaintService.PaintAt(drop, 0.8f, Member.Team);
                }
                yield return null;
            }
        }

        void SpawnExplosionVFX(Vector3 pos, Color col, float radius)
        {
            var go = new GameObject("SpecialVFX");
            go.transform.position = pos + Vector3.up * 0.1f;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 14f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
            main.startColor = col;
            main.gravityModifier = 1.0f;
            main.maxParticles = 150;
            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 120) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = radius * 0.4f;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            var sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (sh != null)
            {
                var mat = new Material(sh);
                mat.SetColor("_BaseColor", col * 3.5f);
                renderer.material = mat;
            }
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayImpact();
            Destroy(go, 2.5f);
        }
    }

    /// <summary>
    /// スペシャル弾の物理(着弾で塗装+VFX)。
    /// </summary>
    public class SpecialProjectile : MonoBehaviour
    {
        public Vector3 Velocity;
        public float PaintRadius = 2f;
        public TeamId Team;
        public Color TeamColor;

        void Awake() { Destroy(gameObject, 3f); }

        void FixedUpdate()
        {
            Vector3 nextPos = transform.position + Velocity * Time.fixedDeltaTime;
            if (Physics.Raycast(transform.position, Velocity.normalized, out RaycastHit hit, Velocity.magnitude * Time.fixedDeltaTime))
            {
                InkPaintService.PaintAt(hit.point, PaintRadius, Team);
                if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayImpact();
                Destroy(gameObject);
                return;
            }
            // 重力
            Velocity.y -= 9.8f * Time.fixedDeltaTime;
            transform.position = nextPos;
        }
    }
}
