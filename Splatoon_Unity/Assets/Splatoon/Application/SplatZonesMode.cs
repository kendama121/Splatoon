using UnityEngine;
using Splatoon.Domain;
using Splatoon.Infrastructure;

namespace Splatoon.Application
{
    /// <summary>
    /// ガチエリア。中央のエリアを占有率50%以上で確保→自軍カウントを減らす。
    /// 100→0でノックアウト勝利。同点なら長く確保したチーム勝利。
    /// </summary>
    public class SplatZonesMode : MatchModeBase
    {
        public override string ModeName => "Splat Zones";

        /// <summary>エリア中心ワールド座標</summary>
        public Vector3 ZoneCenter = Vector3.zero;
        /// <summary>エリア半径(m)</summary>
        public float ZoneRadius = 2f;
        /// <summary>カウント初期値(本家100)</summary>
        public float InitialCount = 100f;
        /// <summary>占有判定の塗り率閾値</summary>
        public float OwnRatioThreshold = 0.5f;

        /// <summary>Alpha側カウント</summary>
        public float AlphaCount { get; set; }
        /// <summary>Bravo側カウント</summary>
        public float BravoCount { get; set; }
        /// <summary>現在の占有チーム</summary>
        public TeamId Holder { get; set; } = TeamId.Neutral;

        public override void OnMatchStart()
        {
            AlphaCount = InitialCount;
            BravoCount = InitialCount;
            Holder = TeamId.Neutral;
            Winner = TeamId.Neutral;
            IsKnockedOut = false;
        }

        public override void OnMatchUpdate()
        {
            if (IsKnockedOut) return;
            // エリア内塗り率簡易判定: 床PaintableSurfaceからエリアUV範囲をサンプリング
            // MVPでは ScoreCalculator の値をそのまま参照(エリアと床全体の比率)
            float aRatio = 0f, bRatio = 0f;
            if (TurfWarMatchManager.Instance != null && TurfWarMatchManager.Instance.Score != null)
            {
                aRatio = TurfWarMatchManager.Instance.Score.TeamRatios[0];
                bRatio = TurfWarMatchManager.Instance.Score.TeamRatios[1];
            }

            // 占有判定: 片チームが過半数+他チームの2倍以上で占有(簡易)
            if (aRatio > OwnRatioThreshold && aRatio > bRatio * 1.5f) Holder = TeamId.Alpha;
            else if (bRatio > OwnRatioThreshold && bRatio > aRatio * 1.5f) Holder = TeamId.Bravo;
            else Holder = TeamId.Neutral;

            // 占有中チームのカウント減少
            if (Holder == TeamId.Alpha) AlphaCount -= Time.deltaTime * 1f;
            else if (Holder == TeamId.Bravo) BravoCount -= Time.deltaTime * 1f;

            // ノックアウト判定
            if (AlphaCount <= 0) { Winner = TeamId.Alpha; IsKnockedOut = true; }
            else if (BravoCount <= 0) { Winner = TeamId.Bravo; IsKnockedOut = true; }
        }

        public override TeamId EvaluateFinalWinner()
        {
            if (IsKnockedOut) return Winner;
            // 時間切れ: カウントが少ない方が進行多→勝者
            if (AlphaCount < BravoCount) return TeamId.Alpha;
            if (BravoCount < AlphaCount) return TeamId.Bravo;
            return TeamId.Neutral;
        }
    }
}
