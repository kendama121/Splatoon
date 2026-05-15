using UnityEngine;
using Splatoon.Domain;

namespace Splatoon.Application
{
    /// <summary>
    /// ガチアサリ。床のアサリを集めパワーアサリ作成→敵バリア破壊→ゴール投入。
    /// MVP簡略版: 床全体に散布されたアサリを拾ってカウント加算。
    /// </summary>
    public class ClamBlitzMode : MatchModeBase
    {
        public override string ModeName => "Clam Blitz";
        /// <summary>初期カウント</summary>
        public float InitialCount = 100f;
        /// <summary>アサリスポーン地点群(将来配置)</summary>
        public Vector3[] ClamSpawnPoints;

        public float AlphaScore { get; set; }
        public float BravoScore { get; set; }

        public override void OnMatchStart()
        {
            AlphaScore = 0f;
            BravoScore = 0f;
            Winner = TeamId.Neutral;
            IsKnockedOut = false;
        }

        public override void OnMatchUpdate()
        {
            // MVP実装: 塗装の進行で代替スコア(本格実装は別途)
            if (TurfWarMatchManager.Instance != null && TurfWarMatchManager.Instance.Score != null)
            {
                AlphaScore = TurfWarMatchManager.Instance.Score.TeamRatios[0] * 1000f;
                BravoScore = TurfWarMatchManager.Instance.Score.TeamRatios[1] * 1000f;
            }

            if (AlphaScore >= InitialCount) { Winner = TeamId.Alpha; IsKnockedOut = true; }
            else if (BravoScore >= InitialCount) { Winner = TeamId.Bravo; IsKnockedOut = true; }
        }

        public override TeamId EvaluateFinalWinner()
        {
            if (IsKnockedOut) return Winner;
            if (AlphaScore > BravoScore) return TeamId.Alpha;
            if (BravoScore > AlphaScore) return TeamId.Bravo;
            return TeamId.Neutral;
        }
    }
}
