using UnityEngine;
using Splatoon.Application;
using Splatoon.Infrastructure;
using Splatoon.Domain;

namespace Splatoon.Presentation
{
    /// <summary>
    /// 試合終了時の演出。勝者発表時にスローモーション+花火パーティクル+勝利チーム色の花火を散布。
    /// TurfWarMatchManagerの状態を監視し、終了瞬間に1度だけ発動。
    /// </summary>
    public class MatchEndEffect : MonoBehaviour
    {
        /// <summary>スローモーション継続時間(秒、Time.timeScale=0.3)</summary>
        public float SlowMotionDuration = 3f;
        /// <summary>花火の上昇高さ</summary>
        public float FireworkHeight = 8f;
        /// <summary>花火パーティクルの粒子数</summary>
        public int FireworkParticles = 200;

        // 内部
        bool _isTriggered;
        bool _wasActive = true;

        void Update()
        {
            if (TurfWarMatchManager.Instance == null) return;
            var mgr = TurfWarMatchManager.Instance;

            // 試合がアクティブ→非アクティブに遷移した瞬間に発動
            if (_wasActive && !mgr.IsMatchActive && !_isTriggered)
            {
                _isTriggered = true;
                StartCoroutine(PlayEndSequence());
            }
            _wasActive = mgr.IsMatchActive;
        }

        System.Collections.IEnumerator PlayEndSequence()
        {
            // スローモーション
            float originalTimeScale = Time.timeScale;
            Time.timeScale = 0.35f;

            // 花火を勝者色で打ち上げ
            var mgr = TurfWarMatchManager.Instance;
            Color winnerColor = InkPaintManager.Instance != null
                ? InkPaintManager.Instance.GetTeamColor(mgr.Winner)
                : Color.white;
            SpawnFirework(new Vector3(-4f, FireworkHeight, 0f), winnerColor);
            SpawnFirework(new Vector3(4f, FireworkHeight, 0f), winnerColor);
            SpawnFirework(new Vector3(0f, FireworkHeight + 2f, 0f), winnerColor);

            yield return new WaitForSecondsRealtime(SlowMotionDuration);
            Time.timeScale = originalTimeScale;
        }

        /// <summary>
        /// 花火パーティクル生成。爆発状に上方向へ広がる。
        /// </summary>
        void SpawnFirework(Vector3 pos, Color color)
        {
            var go = new GameObject("Firework");
            go.transform.position = pos;

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 1.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 12f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startColor = color;
            main.gravityModifier = 0.5f;
            main.maxParticles = FireworkParticles;
            main.useUnscaledTime = true; // スローモ無視

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, FireworkParticles) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var colOverLife = ps.colorOverLifetime;
            colOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color * 2f, 0f), new GradientColorKey(color, 0.7f), new GradientColorKey(color * 0.3f, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) }
            );
            colOverLife.color = grad;

            // トレイル
            var trails = ps.trails;
            trails.enabled = true;
            trails.lifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            trails.minVertexDistance = 0.2f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            var particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader != null)
            {
                var mat = new Material(particleShader);
                mat.SetColor("_BaseColor", color * 3f);
                renderer.material = mat;
                renderer.trailMaterial = mat;
            }

            ps.Play();
            Destroy(go, 3f);
        }
    }
}
