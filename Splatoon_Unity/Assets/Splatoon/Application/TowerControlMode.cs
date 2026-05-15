using UnityEngine;
using Splatoon.Domain;
using Splatoon.Presentation;

namespace Splatoon.Application
{
    /// <summary>
    /// ガチヤグラ。中央のヤグラに乗ると敵ゴール方向へ自動移動。
    /// 100カウントを敵ゴールまで進める。
    /// </summary>
    public class TowerControlMode : MatchModeBase
    {
        public override string ModeName => "Tower Control";
        /// <summary>ヤグラTransform</summary>
        public Transform Tower;
        /// <summary>ヤグラ移動速度(unit/秒)</summary>
        public float TowerSpeed = 0.8f;
        /// <summary>Alphaゴール座標</summary>
        public Vector3 AlphaGoal = new Vector3(-8, 0, 0);
        /// <summary>Bravoゴール座標</summary>
        public Vector3 BravoGoal = new Vector3(8, 0, 0);
        /// <summary>初期カウント</summary>
        public float InitialCount = 100f;

        public float AlphaCount { get; set; }
        public float BravoCount { get; set; }
        public TeamId CurrentRider { get; set; } = TeamId.Neutral;

        Vector3 _initialTowerPos;

        public override void OnMatchStart()
        {
            AlphaCount = InitialCount;
            BravoCount = InitialCount;
            CurrentRider = TeamId.Neutral;
            Winner = TeamId.Neutral;
            IsKnockedOut = false;
            if (Tower != null) _initialTowerPos = Tower.position;
        }

        public override void OnMatchUpdate()
        {
            if (IsKnockedOut || Tower == null) return;

            // ヤグラ上の搭乗プレイヤー検出(範囲内のTeamMember)
            Collider[] hits = Physics.OverlapSphere(Tower.position + Vector3.up * 1f, 1.5f);
            int alphaOn = 0, bravoOn = 0;
            foreach (var h in hits)
            {
                var tm = h.GetComponent<TeamMember>();
                if (tm == null) tm = h.GetComponentInParent<TeamMember>();
                if (tm == null) continue;
                if (tm.Team == TeamId.Alpha) alphaOn++;
                else if (tm.Team == TeamId.Bravo) bravoOn++;
            }

            if (alphaOn > 0 && bravoOn == 0) CurrentRider = TeamId.Alpha;
            else if (bravoOn > 0 && alphaOn == 0) CurrentRider = TeamId.Bravo;
            else CurrentRider = TeamId.Neutral;

            // 移動方向(乗ってるチームの敵ゴール側)
            if (CurrentRider == TeamId.Alpha)
            {
                Vector3 dir = (BravoGoal - Tower.position).normalized;
                Tower.position += dir * TowerSpeed * Time.deltaTime;
                AlphaCount -= Time.deltaTime * 2f;
            }
            else if (CurrentRider == TeamId.Bravo)
            {
                Vector3 dir = (AlphaGoal - Tower.position).normalized;
                Tower.position += dir * TowerSpeed * Time.deltaTime;
                BravoCount -= Time.deltaTime * 2f;
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
