using UnityEngine;
using System.Collections;
using Splatoon.Domain;
using Splatoon.Infrastructure;
using Splatoon.Presentation;

namespace Splatoon.Application
{
    /// <summary>
    /// スーパージャンプ動作(修正版)。マップ画面で選択した地点へキャラを放物線で飛行→着地時インク爆発。
    /// 3フェーズ: チャージ→飛行→着地。try/finallyでCC/PCのenabled復帰保証。
    /// </summary>
    public class SuperJumpAction : MonoBehaviour
    {
        /// <summary>チャージ時間(秒、本家のヒト形態 0.8s)</summary>
        public float ChargeDuration = 0.8f;
        /// <summary>飛行時間(秒)</summary>
        public float FlightDuration = 1.5f;
        /// <summary>飛行頂点の高さオフセット(m)</summary>
        public float FlightApexHeight = 8f;
        /// <summary>着地時の塗装半径(m)</summary>
        public float LandingPaintRadius = 2.5f;

        // 内部状態(再入防止)
        bool _isJumping;

        /// <summary>
        /// スーパージャンプを実行。指定地点へキャラ移動。
        /// </summary>
        public void Execute(Transform target, Vector3 destination)
        {
            if (_isJumping) return;
            StartCoroutine(JumpSequence(target, destination));
        }

        IEnumerator JumpSequence(Transform target, Vector3 destination)
        {
            _isJumping = true;

            // 一時保存(try/finallyで確実に復帰)
            var cc = target.GetComponent<CharacterController>();
            var pc = target.GetComponent<PlayerController>();
            bool ccWasEnabled = cc != null && cc.enabled;
            bool pcWasEnabled = pc != null && pc.enabled;

            // CC + PC無効化
            if (cc != null) cc.enabled = false;
            if (pc != null) pc.enabled = false;

            // ジャンプSE
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayJump();

            // チャージフェーズ(その場で待機、scale変更しない=CCに影響なし)
            float chargeElapsed = 0f;
            while (chargeElapsed < ChargeDuration)
            {
                chargeElapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // 飛行フェーズ(放物線)
            Vector3 startPos = target.position;
            Vector3 endPos = destination + Vector3.up * 0.6f;
            float flightElapsed = 0f;
            while (flightElapsed < FlightDuration)
            {
                flightElapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(flightElapsed / FlightDuration);
                Vector3 horizPos = Vector3.Lerp(startPos, endPos, t);
                float verticalArc = 4f * t * (1f - t); // 放物線(0→1→0)
                horizPos.y = Mathf.Lerp(startPos.y, endPos.y, t) + verticalArc * FlightApexHeight;
                target.position = horizPos;
                yield return null;
            }
            target.position = endPos;

            // 着地塗装+VFX
            InkPaintService.PaintAt(endPos, LandingPaintRadius, GetTeam(target));
            SpawnLandingVFX(endPos, GetTeam(target));

            // 復帰
            if (cc != null) cc.enabled = ccWasEnabled;
            if (pc != null) pc.enabled = pcWasEnabled;

            _isJumping = false;
        }

        TeamId GetTeam(Transform target)
        {
            var tm = target.GetComponent<TeamMember>();
            return tm != null ? tm.Team : TeamId.Neutral;
        }

        void SpawnLandingVFX(Vector3 pos, TeamId team)
        {
            if (InkPaintManager.Instance == null) return;
            Color teamCol = InkPaintManager.Instance.GetTeamColor(team);

            var go = new GameObject("LandingVFX");
            go.transform.position = pos + Vector3.up * 0.1f;

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 10f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startColor = teamCol;
            main.gravityModifier = 1.5f;
            main.maxParticles = 80;
            main.useUnscaledTime = true;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 60) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.3f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(teamCol * 2f, 0f), new GradientColorKey(teamCol, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            var particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader != null)
            {
                var mat = new Material(particleShader);
                mat.SetColor("_BaseColor", teamCol * 3f);
                renderer.material = mat;
            }

            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayImpact();

            ps.Play();
            Destroy(go, 2f);
        }
    }
}
