using UnityEngine;

namespace Splatoon.Domain
{
    /// <summary>
    /// プレイヤーの物理パラメータ設定。
    /// 本家Splatoonの数値(Inkipedia/Leanny GameParameterTable由来)を初期値として保持。
    /// 1 DU(距離単位) = 0.0525m, FPS基準 = 60。
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerPhysicsConfig", menuName = "Splatoon/Player Physics Config")]
    public class PlayerPhysicsConfig : ScriptableObject
    {
        // 単位系定数
        /// <summary>距離単位(DU)からメートルへの換算係数</summary>
        public const float DU_TO_METER = 0.0525f;
        /// <summary>本家フレームレート基準</summary>
        public const float REFERENCE_FPS = 60f;

        // 移動速度関連(m/s換算済み)
        /// <summary>ヒト形態の基本走行速度(m/s)。本家0.96 DU/frame換算 = 3.024 m/s</summary>
        public float RunSpeedBase = 3.024f;
        /// <summary>イカ形態の基本スイム速度(m/s)。本家1.92 DU/frame換算 = 6.048 m/s</summary>
        public float SwimSpeedBase = 6.048f;
        /// <summary>イカ形態の最大スイム速度(m/s)。Swim Speed Up極限値</summary>
        public float SwimSpeedMax = 7.560f;
        /// <summary>敵インク上での移動速度倍率(0.0〜1.0)。本家は約0.25倍</summary>
        [Range(0f, 1f)] public float EnemyInkSpeedMultiplier = 0.25f;
        /// <summary>射撃中の移動速度倍率(本家0.72/0.96 ≒ 0.75)</summary>
        [Range(0f, 1f)] public float FiringMoveSpeedMultiplier = 0.75f;

        // ジャンプ・重力
        /// <summary>ジャンプ初速(m/s)。実測調整必要、初期値はSplatoonっぽい軽さ</summary>
        public float JumpVelocity = 5.0f;
        /// <summary>重力(m/s²)。Unity標準値</summary>
        public float Gravity = 9.81f;
        /// <summary>壁登り時の上昇速度(m/s)</summary>
        public float WallClimbSpeed = 4.0f;

        // HP・ダメージ
        /// <summary>HP最大値。本家は100固定</summary>
        public float MaxHP = 100f;
        /// <summary>ヒト形態の自然回復速度(HP/秒)</summary>
        public float RegenHumanoid = 12.5f;
        /// <summary>イカ形態(自軍インク内)の回復速度(HP/秒)。本家は実質即時回復</summary>
        public float RegenSwim = 100f;
        /// <summary>敵インク踏みのダメージレート(HP/秒)</summary>
        public float EnemyInkDamagePerSecond = 30f;
        /// <summary>敵インク累積ダメージ上限(本家S2基準)</summary>
        public float EnemyInkDamageCap = 40f;

        // インクタンク
        /// <summary>インクタンク最大容量(%)</summary>
        public float InkTankMax = 100f;
        /// <summary>ヒト形態のインク自然回復(%/秒)。要実測調整</summary>
        public float InkRegenHumanoid = 2.5f;
        /// <summary>イカ形態(自軍インク内)のインク回復(%/秒)。要実測調整</summary>
        public float InkRegenSwim = 15f;

        // リスポーン
        /// <summary>スプラットアニメーション時間(秒)</summary>
        public float SplatAnimDuration = 0.5f;
        /// <summary>キラーカム表示時間(秒)</summary>
        public float SplatCamDuration = 6.0f;
        /// <summary>リスポーン降下アニメ時間(秒)</summary>
        public float RespawnDescendDuration = 2.0f;
        /// <summary>ノーアビリティ時の合計リスポーン時間(秒)</summary>
        public float TotalRespawnDuration = 8.5f;

        // イカロール・イカノボリ
        /// <summary>イカロールの無敵フレーム数(60fps基準、推定値)</summary>
        public int SquidRollIFrames = 6;
        /// <summary>イカロールのアーマー量(単発吸収ダメージ)</summary>
        public float SquidRollArmor = 100f;
        /// <summary>イカノボリ最大チャージ時間(秒)</summary>
        public float SquidSurgeMaxCharge = 1.0f;
        /// <summary>イカノボリ最大上昇高度(m)</summary>
        public float SquidSurgeMaxHeight = 5.0f;

        // スーパージャンプ
        /// <summary>イカ形態からのスーパージャンプチャージ時間(秒)</summary>
        public float SuperJumpChargeSquid = 0.8f;
        /// <summary>ヒト形態からのスーパージャンプチャージ時間(秒)。+0.35秒余分</summary>
        public float SuperJumpChargeHuman = 1.15f;
        /// <summary>スーパージャンプ着地後の硬直時間(秒)</summary>
        public float SuperJumpLandingLag = 0.5f;

        // カメラ
        /// <summary>カメラ感度(横)。本家-5〜+5の内部マッピング</summary>
        public float CameraSensitivityX = 1.0f;
        /// <summary>カメラ感度(縦)。横の70%程度</summary>
        public float CameraSensitivityY = 0.7f;
        /// <summary>Y軸反転フラグ</summary>
        public bool IsInvertY = false;
        /// <summary>通常時のカメラFOV(度)</summary>
        public float NormalFOV = 60f;
        /// <summary>スコープ半チャージ時のFOV(度)</summary>
        public float ScopeHalfFOV = 35f;
        /// <summary>スコープフルチャージ時のFOV(度)</summary>
        public float ScopeFullFOV = 24f;

        // プレイヤー判定
        /// <summary>ヒト形態のCharacterController半径(m)。本家R0.7DU≒0.037mだがゲーム的に拡大</summary>
        public float HumanColliderRadius = 0.4f;
        /// <summary>ヒト形態のCharacterController高さ(m)</summary>
        public float HumanColliderHeight = 1.65f;
        /// <summary>イカ形態のCharacterController半径(m)</summary>
        public float SquidColliderRadius = 0.35f;
        /// <summary>イカ形態のCharacterController高さ(m)</summary>
        public float SquidColliderHeight = 0.7f;
    }
}
