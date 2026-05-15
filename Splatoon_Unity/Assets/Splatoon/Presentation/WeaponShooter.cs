using UnityEngine;
using Splatoon.Domain;

namespace Splatoon.Presentation
{
    /// <summary>
    /// 武器発射コンポーネント。Player/BOT両用。
    /// Fire()メソッドで弾を生成し、連射インターバルとインク消費を管理。
    /// </summary>
    public class WeaponShooter : MonoBehaviour
    {
        /// <summary>装備中武器データ</summary>
        public WeaponData Weapon;
        /// <summary>弾プレハブ(InkBulletを持つGameObject)</summary>
        public GameObject BulletPrefab;
        /// <summary>発射ピボット(銃口位置、Player子オブジェクト推奨)</summary>
        public Transform MuzzlePoint;
        /// <summary>チームメンバー参照(チーム色判定)</summary>
        public TeamMember Member;

        // 内部状態
        float _nextFireTime;
        float _currentInk = 100f;
        /// <summary>現在のインクタンク残量(0-100)</summary>
        public float CurrentInk { get { return _currentInk; } }

        void Awake()
        {
            if (Member == null) Member = GetComponent<TeamMember>();
            if (MuzzlePoint == null || MuzzlePoint.Equals(null))
            {
                var t = transform.Find("MuzzlePoint");
                if (t != null) MuzzlePoint = t;
            }
        }

        /// <summary>
        /// 発射処理。連射インターバル+インク残量チェック後、弾生成。
        /// </summary>
        /// <param name="aimDirection">発射方向(正規化済推奨)</param>
        /// <summary>1発の発射に伴う多重スプラット(飛沫)弾数。本家のシューター質感再現</summary>
        public int SplashShotCount = 5;
        /// <summary>飛沫弾の散布角度(ランダム円錐の半径相当)</summary>
        public float SplashSpread = 0.15f;

        /// <summary>マズルフラッシュエフェクトプレハブ(任意、なければ生成しない)</summary>
        public ParticleSystem MuzzleFlashPrefab;

        /// <summary>発射時のカメラ揺れ強度(WeaponShooter -> PlayerCamera連携)</summary>
        public float CameraShakeStrength = 0.05f;

        public bool Fire(Vector3 aimDirection)
        {
            // null安全
            if (Weapon == null || BulletPrefab == null) return false;
            if (Member == null) Member = GetComponent<TeamMember>();
            if (Member == null) return false;
            // インターバル判定
            if (Time.time < _nextFireTime) return false;
            // インク不足判定
            if (_currentInk < Weapon.InkConsumePerShot) return false;

            // 銃口位置と方向から弾生成
            Vector3 origin = MuzzlePoint != null ? MuzzlePoint.position : transform.position + Vector3.up * 1.5f;
            Vector3 dirNorm = aimDirection.normalized;

            // 武器カテゴリ別の発射ロジック(WeaponCategoryFireに委譲)
            WeaponCategoryFire.Execute(Weapon, origin, dirNorm, Member.Team, gameObject, BulletPrefab);

            // マズルフラッシュ生成
            SpawnMuzzleFlash(origin, dirNorm);

            // カメラ揺れ(ローカルPlayerの場合のみ)
            if (CameraShakeStrength > 0f && CameraShaker.Instance != null && GetComponent<PlayerController>() != null)
            {
                CameraShaker.Instance.Shake(CameraShakeStrength, 0.08f);
            }

            // 発射SE
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayShoot();

            // インク消費 + 次発射時間
            _currentInk -= Weapon.InkConsumePerShot;
            _nextFireTime = Time.time + Weapon.FireIntervalFrames / 60f;
            return true;
        }

        /// <summary>
        /// 単発の弾生成ヘルパ。位置と方向を受けて InkBullet を初期化。
        /// </summary>
        void SpawnBullet(Vector3 origin, Vector3 dir)
        {
            GameObject go = Instantiate(BulletPrefab, origin, Quaternion.LookRotation(dir));
            var bullet = go.GetComponent<InkBullet>();
            bullet.Initialize(Weapon, origin, dir, Member.Team, gameObject);
        }

        /// <summary>
        /// マズルフラッシュ(銃口閃光)パーティクル生成。本家の発射感再現。
        /// </summary>
        void SpawnMuzzleFlash(Vector3 origin, Vector3 dir)
        {
            // 動的に短命パーティクル生成
            var go = new GameObject("MuzzleFlash");
            go.transform.position = origin;
            go.transform.rotation = Quaternion.LookRotation(dir);

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = 0.08f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
            Color teamCol = Splatoon.Infrastructure.InkPaintManager.Instance != null
                ? Splatoon.Infrastructure.InkPaintManager.Instance.GetTeamColor(Member.Team)
                : Color.white;
            main.startColor = teamCol;
            main.gravityModifier = 0f;
            main.maxParticles = 15;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;
            shape.radius = 0.05f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            var particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader != null)
            {
                var mat = new Material(particleShader);
                mat.SetColor("_BaseColor", teamCol * 3f);
                renderer.material = mat;
            }

            ps.Play();
            Destroy(go, 0.3f);
        }

        /// <summary>
        /// インクタンク回復(イカ潜伏中など、外部から呼び出し)。
        /// </summary>
        public void RestoreInk(float amount)
        {
            _currentInk = Mathf.Clamp(_currentInk + amount, 0f, 100f);
        }

        /// <summary>
        /// 毎フレームインク自然回復。発射してない時のみ。
        /// ヒト形態=2.5%/s、イカ形態=15%/s、敵インク上=回復停止。
        /// </summary>
        void Update()
        {
            // 直前の発射から0.4秒以内は回復しない(本家仕様)
            if (Time.time < _nextFireTime + 0.4f) return;
            if (_currentInk >= 100f) return;

            // プレイヤー判定(イカ形態か)
            var pc = GetComponent<PlayerController>();
            bool isSquid = pc != null && pc.IsSquidForm;

            float regenRate = isSquid ? 25f : 5f; // イカ早い、ヒト遅い
            _currentInk = Mathf.Clamp(_currentInk + regenRate * Time.deltaTime, 0f, 100f);
        }
    }
}
