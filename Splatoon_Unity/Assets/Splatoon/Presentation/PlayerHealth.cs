using UnityEngine;
using System.Collections;
using Splatoon.Domain;
using Splatoon.Infrastructure;
using Splatoon.Application;

namespace Splatoon.Presentation
{
    /// <summary>
    /// プレイヤーHP・ダメージ受け・スプラット・リスポーン処理。
    /// 敵弾でダメージ→HP0でスプラット演出→リスポーン地点に復帰。
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        /// <summary>最大HP(本家100)</summary>
        public float MaxHP = 100f;
        /// <summary>ヒト形態の自然回復(HP/秒)</summary>
        public float RegenHumanoid = 12.5f;
        /// <summary>イカ形態(自軍インク)の高速回復(HP/秒)</summary>
        public float RegenSwim = 100f;
        /// <summary>敵インク踏みダメージ(HP/秒)</summary>
        public float EnemyInkDamagePerSecond = 30f;
        /// <summary>リスポーン地点</summary>
        public Transform SpawnPoint;
        /// <summary>所属チーム(TeamMember参照)</summary>
        public TeamMember Member;
        /// <summary>スプラットアニメ時間(秒)</summary>
        public float SplatAnimDuration = 0.5f;
        /// <summary>キラーカム時間(秒)</summary>
        public float SplatCamDuration = 3f;
        /// <summary>リスポーン降下時間(秒)</summary>
        public float RespawnDescendDuration = 2f;

        /// <summary>現在HP</summary>
        public float CurrentHP { get; set; }
        /// <summary>スプラット状態</summary>
        public bool IsSplatted { get; set; }

        // 内部
        CharacterController _cc;
        PlayerController _pc;
        Vector3 _initialSpawnPos;
        float _regenDelayUntil;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _pc = GetComponent<PlayerController>();
            if (Member == null) Member = GetComponent<TeamMember>();
            CurrentHP = MaxHP;
            _initialSpawnPos = SpawnPoint != null ? SpawnPoint.position : transform.position;
        }

        void Update()
        {
            if (IsSplatted) return;

            // 敵インクダメージ判定(PlayerControllerが_isOnEnemyInkで持つ)
            if (_pc != null && _pc._isOnEnemyInk)
            {
                TakeDamage(EnemyInkDamagePerSecond * Time.deltaTime, TeamId.Neutral);
                _regenDelayUntil = Time.time + 1.5f;
            }
            else if (Time.time > _regenDelayUntil)
            {
                // 自然回復
                bool isSquid = _pc != null && IsSquidForm();
                float regenRate = isSquid ? RegenSwim : RegenHumanoid;
                CurrentHP = Mathf.Min(MaxHP, CurrentHP + regenRate * Time.deltaTime);
            }
        }

        bool IsSquidForm()
        {
            // PlayerController内のSquidModelがアクティブか判定
            if (_pc == null) return false;
            var sm = _pc.SquidModel;
            return sm != null && sm.activeSelf;
        }

        /// <summary>
        /// ダメージを受ける。HP0でスプラット発動。
        /// </summary>
        public void TakeDamage(float amount, TeamId attacker)
        {
            if (IsSplatted) return;
            CurrentHP -= amount;
            _regenDelayUntil = Time.time + 1.5f;

            // 自プレイヤーなら画面赤フラッシュ発動
            if (_pc != null && ScreenDamageOverlay.Instance != null)
            {
                float intensity = Mathf.Clamp01(amount / 50f) * 0.6f;
                ScreenDamageOverlay.Instance.TriggerFlash(intensity);
            }

            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                StartCoroutine(SplatAndRespawn(attacker));
            }
        }

        IEnumerator SplatAndRespawn(TeamId attacker)
        {
            IsSplatted = true;

            // スプラット演出: インク爆発VFX + キャラ非表示
            if (InkPaintManager.Instance != null)
            {
                Color teamCol = InkPaintManager.Instance.GetTeamColor(attacker == TeamId.Neutral ? Member.Team : attacker);
                SpawnSplatVFX(transform.position + Vector3.up * 0.5f, teamCol);
                // 周囲塗装
                InkPaintService.PaintAt(transform.position, 1.8f, attacker == TeamId.Neutral ? Member.Team : attacker);
            }

            // スプラット通知(自分=「YOU GOT SPLATTED!」、他=「SPLATTED!」)
            if (SplashNotification.Instance != null && _pc != null && _pc.enabled)
            {
                SplashNotification.Instance.Show("YOU GOT SPLATTED!", new Color(0.9f, 0.2f, 0.2f, 1f), SplatCamDuration);
            }

            // KillFeed通知(攻撃者・犠牲者の名前で)
            if (KillFeed.Instance != null)
            {
                string victimName = gameObject.name;
                string attackerName = (attacker == TeamId.Neutral) ? "ENVIRONMENT" : attacker.ToString();
                KillFeed.Instance.AddLog(attackerName, victimName, attacker == TeamId.Neutral ? Member.Team : attacker);
            }

            // 移動・操作無効化(BOT系も含む)
            if (_cc != null) _cc.enabled = false;
            if (_pc != null) _pc.enabled = false;
            // BOT動作も停止(SimpleBot/AdvancedBot両方)
            var sb = GetComponent<SimpleBot>();
            if (sb != null) sb.enabled = false;
            var ab = GetComponent<AdvancedBot>();
            if (ab != null) ab.enabled = false;

            // モデル非表示(null安全)
            if (_pc != null)
            {
                if (_pc.HumanModel != null) _pc.HumanModel.SetActive(false);
                if (_pc.SquidModel != null) _pc.SquidModel.SetActive(false);
            }

            // スプラットアニメ時間
            yield return new WaitForSeconds(SplatAnimDuration);
            // キラーカム時間
            yield return new WaitForSeconds(SplatCamDuration);

            // リスポーン地点へ移動
            Vector3 spawnPos = SpawnPoint != null ? SpawnPoint.position : _initialSpawnPos;
            transform.position = spawnPos + Vector3.up * 3f; // 上空から降下

            // 降下アニメ
            float elapsed = 0f;
            while (elapsed < RespawnDescendDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / RespawnDescendDuration;
                transform.position = Vector3.Lerp(spawnPos + Vector3.up * 3f, spawnPos, t);
                yield return null;
            }
            transform.position = spawnPos;

            // 復活(BOT系も再有効化)
            if (_pc != null)
            {
                if (_pc.HumanModel != null) _pc.HumanModel.SetActive(true);
                if (_pc.SquidModel != null) _pc.SquidModel.SetActive(false);
            }
            if (_cc != null) _cc.enabled = true;
            if (_pc != null) _pc.enabled = true;
            var sb2 = GetComponent<SimpleBot>();
            if (sb2 != null) sb2.enabled = true;
            var ab2 = GetComponent<AdvancedBot>();
            if (ab2 != null) ab2.enabled = true;
            CurrentHP = MaxHP;
            IsSplatted = false;
        }

        /// <summary>
        /// スプラット時の派手な爆発VFX生成。
        /// </summary>
        void SpawnSplatVFX(Vector3 pos, Color teamColor)
        {
            var go = new GameObject("SplatVFX");
            go.transform.position = pos;

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 12f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
            main.startColor = teamColor;
            main.gravityModifier = 1.8f;
            main.maxParticles = 120;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 100) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            var particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader != null)
            {
                var mat = new Material(particleShader);
                mat.SetColor("_BaseColor", teamColor * 3f);
                renderer.material = mat;
            }

            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayImpact();

            ps.Play();
            Destroy(go, 2f);
        }
    }
}
