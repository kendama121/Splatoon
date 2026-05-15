using UnityEngine;
using Splatoon.Domain;

namespace Splatoon.Application
{
    /// <summary>
    /// ナワバリバトル(レギュラーマッチ)。床塗装面積の多いチーム勝利。
    /// </summary>
    public class TurfWarMode : MatchModeBase
    {
        public override string ModeName => "Turf War";

        public override void OnMatchStart()
        {
            Winner = TeamId.Neutral;
            IsKnockedOut = false;
        }

        public override void OnMatchUpdate()
        {
            // ナワバリは時間切れまで継続(ノックアウトなし)
        }

        public override TeamId EvaluateFinalWinner()
        {
            if (TurfWarMatchManager.Instance == null || TurfWarMatchManager.Instance.Score == null)
                return TeamId.Neutral;
            float a = TurfWarMatchManager.Instance.Score.TeamRatios[0];
            float b = TurfWarMatchManager.Instance.Score.TeamRatios[1];
            if (a > b) return TeamId.Alpha;
            if (b > a) return TeamId.Bravo;
            return TeamId.Neutral;
        }
    }
}
