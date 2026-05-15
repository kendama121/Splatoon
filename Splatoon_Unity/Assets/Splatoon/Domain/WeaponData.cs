using UnityEngine;

namespace Splatoon.Domain
{
    /// <summary>
    /// 武器の挙動パラメータ。本家GameParameterTable.json由来の内部値を保持。
    /// MVPはスプラシューター1本のみだが、全11カテゴリに拡張可能な汎用設計。
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Splatoon/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        // 基本情報
        /// <summary>武器表示名(例: スプラシューター)</summary>
        public string DisplayName = "スプラシューター";
        /// <summary>武器カテゴリ</summary>
        public WeaponCategory Category = WeaponCategory.Shooter;

        // ダメージ・射程
        /// <summary>1発あたりの最大ダメージ。スプラシュ = 36</summary>
        public float DamageMax = 36f;
        /// <summary>1発あたりの最小ダメージ(減衰後)</summary>
        public float DamageMin = 18f;
        /// <summary>ダメージ減衰開始フレーム数</summary>
        public int ReduceStartFrame = 8;
        /// <summary>ダメージ減衰終了フレーム数</summary>
        public int ReduceEndFrame = 30;
        /// <summary>最大射程(m)。スプラシュ ≒ 0.86m × 60F = 必要 = 約2.8m相当(DU換算で約11DU)</summary>
        public float MaxRangeMeters = 2.8f;

        // 弾道(3ステートモデル: Straight → Brake → Free)
        /// <summary>弾の初速(m/s)。本家1.493 DU/frame × 60 × 0.0525 ≒ 4.7 m/s</summary>
        public float MuzzleVelocity = 4.7f;
        /// <summary>Straight状態のフレーム数(直進・重力なし)</summary>
        public int StraightFrames = 4;
        /// <summary>Brake状態の空気抵抗(1フレームあたり減速率)</summary>
        [Range(0f, 1f)] public float BrakeAirResistance = 0.36f;
        /// <summary>Brake状態の重力(DU/frame²)</summary>
        public float BrakeGravity = 0.07f;
        /// <summary>Free状態の空気抵抗</summary>
        [Range(0f, 1f)] public float FreeAirResistance = 0.02f;
        /// <summary>Free状態の重力(DU/frame²)</summary>
        public float FreeGravity = 0.016f;

        // 連射・精度
        /// <summary>連射フレーム数(60fps基準)。スプラシュ = 6F = 0.1秒</summary>
        public int FireIntervalFrames = 6;
        /// <summary>地上ブレ角度(度)</summary>
        public float SpreadGroundDegrees = 6f;
        /// <summary>空中ブレ角度(度、ジャンプ中は精度低下)</summary>
        public float SpreadAirDegrees = 18f;

        // 塗り
        /// <summary>1発の着弾時塗り半径(m)</summary>
        public float PaintRadius = 0.35f;
        /// <summary>飛沫の塗り半径(主弾より小、複数生成)</summary>
        public float SplashPaintRadius = 0.12f;

        // 当たり判定
        /// <summary>プレイヤーへの当たり判定半径(m)</summary>
        public float HitRadiusPlayer = 0.18f;
        /// <summary>環境への当たり判定半径(m)</summary>
        public float HitRadiusEnvironment = 0.16f;

        // インク消費
        /// <summary>1発のインク消費(%)。スプラシュ = 0.92%</summary>
        public float InkConsumePerShot = 0.92f;

        // スペシャル
        /// <summary>スペシャル必要塗りポイント</summary>
        public int SpecialPointsRequired = 200;

        // 弾道可視化(VFX)
        /// <summary>弾の見た目色(チーム色で上書き予定、デフォルト白)</summary>
        public Color BulletDefaultColor = Color.white;
    }
}
