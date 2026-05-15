using UnityEngine;
using System.Collections;
using Splatoon.Domain;
using Splatoon.Infrastructure;
using Splatoon.Application;

namespace Splatoon.Presentation
{
    /// <summary>
    /// サブウェポン投擲。R(右クリック)でボム投擲→放物線飛行→着弾爆発+塗装。
    /// </summary>
    public class SubWeaponAction : MonoBehaviour
    {
        public SubWeaponData Equipped;
        public TeamMember Member;
        public Transform AimSource;
        public WeaponShooter Shooter;
        public float NextUseTime;

        void Awake()
        {
            if (Member == null) Member = GetComponent<TeamMember>();
            if (Shooter == null) Shooter = GetComponent<WeaponShooter>();
            if (AimSource == null)
            {
                var t = transform.Find("CameraTarget");
                if (t != null) AimSource = t;
                else AimSource = transform;
            }
        }

        public bool Throw(Vector3 aimDirection)
        {
            if (Equipped == null) return false;
            if (Time.time < NextUseTime) return false;
            if (Shooter != null && Shooter.CurrentInk < Equipped.InkCost) return false;

            // インク消費
            if (Shooter != null) Shooter.RestoreInk(-Equipped.InkCost);

            // ボム生成(球体)
            var bomb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bomb.name = "SubBomb";
            UnityEngine.Object.DestroyImmediate(bomb.GetComponent<Collider>());
            Vector3 origin = AimSource.position + AimSource.forward * 0.5f + Vector3.up * 0.3f;
            bomb.transform.position = origin;
            bomb.transform.localScale = Vector3.one * 0.3f;

            Color teamCol = InkPaintManager.Instance != null
                ? InkPaintManager.Instance.GetTeamColor(Member.Team)
                : Color.white;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", teamCol);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", teamCol * 3f);
            bomb.GetComponent<MeshRenderer>().sharedMaterial = mat;

            // 起爆コルーチン
            Vector3 initialVel = (aimDirection.normalized + Vector3.up * 0.4f) * Equipped.ThrowForce;
            StartCoroutine(BombFly(bomb, initialVel, teamCol));

            NextUseTime = Time.time + 1.0f;
            return true;
        }

        IEnumerator BombFly(GameObject bomb, Vector3 velocity, Color teamCol)
        {
            float fuse = Equipped.FuseTime;
            float elapsed = 0f;
            while (elapsed < fuse && bomb != null)
            {
                elapsed += Time.deltaTime;
                velocity.y -= Equipped.Gravity * Time.deltaTime;
                Vector3 next = bomb.transform.position + velocity * Time.deltaTime;
                // 地面衝突チェック
                if (Physics.Raycast(bomb.transform.position, velocity.normalized, out RaycastHit hit, velocity.magnitude * Time.deltaTime))
                {
                    bomb.transform.position = hit.point + hit.normal * 0.1f;
                    velocity = Vector3.zero; // 着地で静止
                    yield return new WaitForSeconds(0.2f); // 着地後少し待つ
                    break;
                }
                bomb.transform.position = next;
                // 発光パルス
                float pulse = 1f + Mathf.Sin(elapsed * 25f) * 0.3f;
                bomb.transform.localScale = Vector3.one * 0.3f * pulse;
                yield return null;
            }

            if (bomb == null) yield break;

            // 爆発: 塗装+VFX+ダメージ
            Vector3 explodePos = bomb.transform.position;
            InkPaintService.PaintAt(explodePos, Equipped.ExplosionRadius, Member.Team);

            // 周囲のキャラにダメージ
            Collider[] hits = Physics.OverlapSphere(explodePos, Equipped.ExplosionRadius);
            foreach (var col in hits)
            {
                var ph = col.GetComponent<PlayerHealth>();
                if (ph == null) ph = col.GetComponentInParent<PlayerHealth>();
                if (ph == null) continue;
                var tm = ph.GetComponent<TeamMember>();
                if (tm != null && tm.Team == Member.Team) continue;
                float distance = Vector3.Distance(col.transform.position, explodePos);
                float dmg = Mathf.Lerp(Equipped.Damage, Equipped.Damage * 0.3f, distance / Equipped.ExplosionRadius);
                ph.TakeDamage(dmg, Member.Team);
            }

            // VFX
            SpawnExplosionVFX(explodePos, teamCol, Equipped.ExplosionRadius);
            if (CameraShaker.Instance != null) CameraShaker.Instance.Shake(0.2f, 0.3f);

            Destroy(bomb);
        }

        void SpawnExplosionVFX(Vector3 pos, Color col, float radius)
        {
            var go = new GameObject("BombVFX");
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 14f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
            main.startColor = col;
            main.gravityModifier = 1.2f;
            main.maxParticles = 200;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 150) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.2f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            var sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (sh != null)
            {
                var mat = new Material(sh);
                mat.SetColor("_BaseColor", col * 4f);
                renderer.material = mat;
            }
            ps.Play();
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayImpact();
            Destroy(go, 2.5f);
        }
    }
}
