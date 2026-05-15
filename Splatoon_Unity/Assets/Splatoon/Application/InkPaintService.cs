using UnityEngine;
using Splatoon.Domain;
using Splatoon.Infrastructure;

namespace Splatoon.Application
{
    /// <summary>
    /// インク塗装のユースケース実装。
    /// 弾着弾やローラー振り等から呼び出され、レイキャストでPaintableSurfaceを特定し塗装。
    /// </summary>
    public static class InkPaintService
    {
        /// <summary>
        /// ワールド座標+法線方向に半径rでインクを塗布する。
        /// 内部でレイキャストし当該PaintableSurfaceへPaint実行。
        /// </summary>
        /// <param name="hitPoint">着弾ワールド座標</param>
        /// <param name="radius">塗装半径(m)</param>
        /// <param name="team">所属チーム</param>
        /// <returns>塗装成功フラグ</returns>
        public static bool PaintAt(Vector3 hitPoint, float radius, TeamId team)
        {
            if (InkPaintManager.Instance == null) return false;

            // 着弾点付近のPaintableSurfaceを全方位短距離レイで探索
            Collider[] hits = Physics.OverlapSphere(hitPoint, radius * 1.5f);
            bool isPainted = false;
            foreach (var col in hits)
            {
                var surface = col.GetComponent<PaintableSurface>();
                if (surface == null) continue;
                InkPaintManager.Instance.Paint(surface, hitPoint, radius, team);
                isPainted = true;
            }
            return isPainted;
        }

        /// <summary>
        /// プレイヤー足元のインク色を取得(自軍/敵軍/中立判定用)。
        /// CPU側に同期されたマスクから読み取り(MVPは簡易、AsyncGPUReadback化は後で)。
        /// </summary>
        /// <param name="footPos">プレイヤー足元ワールド座標</param>
        /// <param name="surface">床のPaintableSurface参照</param>
        /// <returns>足元の色(透明=未塗装)</returns>
        public static Color SampleInkColor(Vector3 footPos, PaintableSurface surface)
        {
            if (surface == null) return Color.clear;
            // 簡易実装: レイキャストでUV取得 → MaskRTから1ピクセル読み取り
            if (!Physics.Raycast(footPos + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f)) return Color.clear;
            if (hit.collider.gameObject != surface.gameObject) return Color.clear;
            // RenderTextureはCPU直接読みできないため、ReadPixelsで毎回読むのは重い
            // MVPはとりあえず未塗装扱い(Phase 3.5でAsyncGPUReadback実装)
            return Color.clear;
        }
    }
}
