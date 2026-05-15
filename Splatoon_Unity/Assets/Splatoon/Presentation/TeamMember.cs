using UnityEngine;
using Splatoon.Domain;

namespace Splatoon.Presentation
{
    /// <summary>
    /// プレイヤー・BOTのチーム所属管理。
    /// 武器の弾色・スコア集計時の識別に使用。
    /// </summary>
    public class TeamMember : MonoBehaviour
    {
        /// <summary>所属チーム</summary>
        public TeamId Team = TeamId.Alpha;

        /// <summary>表示用にチーム色のマテリアルを子Renderer全てに適用</summary>
        public bool ApplyTeamColorToRenderers = true;

        void Start()
        {
            if (!ApplyTeamColorToRenderers) return;
            if (Splatoon.Infrastructure.InkPaintManager.Instance == null) return;

            Color teamCol = Splatoon.Infrastructure.InkPaintManager.Instance.GetTeamColor(Team);
            // 子Rendererに対しチーム色のマテリアル適用(MVP簡易、後で改善)
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                var mat = new Material(r.sharedMaterial);
                mat.SetColor("_BaseColor", teamCol);
                mat.color = teamCol;
                r.material = mat;
            }
        }
    }
}
