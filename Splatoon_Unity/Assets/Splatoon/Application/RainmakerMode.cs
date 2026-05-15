using UnityEngine;
using Splatoon.Domain;
using Splatoon.Presentation;

namespace Splatoon.Application
{
    /// <summary>
    /// ガチホコ。中央のホコをバリア破壊→拾い上げ→敵ゴール設置。
    /// MVP簡略版: ホコ保持側のカウントが減少、最終ゴール接近距離で勝敗。
    /// </summary>
    public class RainmakerMode : MatchModeBase
    {
        public override string ModeName => "Rainmaker";
        /// <summary>ホコTransform</summary>
        public Transform Rainmaker;
        /// <summary>Alphaゴール</summary>
        public Vector3 AlphaGoal = new Vector3(-8, 0, 0);
        /// <summary>Bravoゴール</summary>
        public Vector3 BravoGoal = new Vector3(8, 0, 0);
        /// <summary>初期カウント</summary>
        public float InitialCount = 100f;
        /// <summary>ホコ持ち捕捉範囲</summary>
        public float PickupRadius = 1.5f;

        public float AlphaCount { get; set; }
        public float BravoCount { get; set; }
        public TeamId Holder { get; set; } = TeamId.Neutral;

        Vector3 _spawnPos;

        public override void OnMatchStart()
        {
            AlphaCount = InitialCount;
            BravoCount = InitialCount;
            Holder = TeamId.Neutral;
            Winner = TeamId.Neutral;
            IsKnockedOut = false;
            if (Rainmaker != null) _spawnPos = Rainmaker.position;
        }

        public override void OnMatchUpdate()
        {
            if (IsKnockedOut || Rainmaker == null) return;

            // ホコ拾得検知
            if (Holder == TeamId.Neutral)
            {
                Collider[] hits = Physics.OverlapSphere(Rainmaker.position, PickupRadius);
                foreach (var h in hits)
                {
                    var tm = h.GetComponent<TeamMember>();
                    if (tm == null) tm = h.GetComponentInParent<TeamMember>();
                    if (tm != null) { Holder = tm.Team; break; }
                }
            }

            // ホコ持ち側のゴール接近度でカウント計算
            if (Holder == TeamId.Alpha)
            {
                float dist = Vector3.Distance(Rainmaker.position, BravoGoal);
                AlphaCount = 100f - (16f - dist) * 6f; // 16=最大距離→100、近づくほどカウント減少
            }
            else if (Holder == TeamId.Bravo)
            {
                float dist = Vector3.Distance(Rainmaker.position, AlphaGoal);
                BravoCount = 100f - (16f - dist) * 6f;
            }

            if (AlphaCount <= 0) { Winner = TeamId.Alpha; IsKnockedOut = true; }
            else if (BravoCount <= 0) { Winner = TeamId.Bravo; IsKnockedOut = true; }
        }

        public override TeamId EvaluateFinalWinner()
        {
            if (IsKnockedOut) return Winner;
            if (AlphaCount < BravoCount) return TeamId.Alpha;
            if (BravoCount < AlphaCount) return TeamId.Bravo;
            return TeamId.Neutral;
        }
    }
}
