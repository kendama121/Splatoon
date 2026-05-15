using UnityEngine;

namespace Splatoon.Domain
{
    /// <summary>
    /// スペシャルウェポンの設定データ。SpecialActionが参照して発動。
    /// </summary>
    [CreateAssetMenu(fileName = "SpecialData", menuName = "Splatoon/Special Data")]
    public class SpecialData : ScriptableObject
    {
        /// <summary>スペシャルカテゴリ</summary>
        public SpecialCategory Category = SpecialCategory.UltraShot;
        /// <summary>表示名</summary>
        public string DisplayName = "ウルトラショット";
        /// <summary>必要充電ポイント(本家180-220)</summary>
        public int RequiredPoints = 200;
        /// <summary>発動持続時間(秒)</summary>
        public float Duration = 5f;
        /// <summary>影響範囲(m)</summary>
        public float EffectRadius = 5f;
        /// <summary>主弾ダメージ</summary>
        public float MainDamage = 200f;
        /// <summary>塗装拡散範囲(m)</summary>
        public float PaintRadius = 3f;
        /// <summary>発動時の効果色(チーム色で上書き予定)</summary>
        public Color EffectColor = Color.white;
    }
}
