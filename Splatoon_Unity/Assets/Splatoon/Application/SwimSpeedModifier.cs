using UnityEngine;
using Splatoon.Domain;
using Splatoon.Infrastructure;

namespace Splatoon.Application
{
    /// <summary>
    /// 自軍/敵軍/中立インク上のプレイヤー速度倍率を計算するサービス。
    /// プレイヤー足元から下方向レイキャスト→ヒットしたPaintableSurfaceのCpuMirrorから色サンプル。
    /// </summary>
    public static class SwimSpeedModifier
    {
        /// <summary>自軍インク上の速度倍率(イカ形態)</summary>
        public const float OwnInkSwimMultiplier = 1.5f;
        /// <summary>自軍インク上の速度倍率(ヒト形態)</summary>
        public const float OwnInkHumanMultiplier = 1.1f;
        /// <summary>敵軍インク上の速度倍率</summary>
        public const float EnemyInkMultiplier = 0.35f;
        /// <summary>未塗装での速度倍率</summary>
        public const float NeutralMultiplier = 1.0f;
        /// <summary>塗装判定の最小アルファ値</summary>
        public const float InkAlphaThreshold = 0.3f;

        /// <summary>
        /// プレイヤー足元の床インクを判定して速度倍率を返す。
        /// </summary>
        /// <param name="footPos">プレイヤー足元のワールド座標</param>
        /// <param name="myTeam">自分のチーム</param>
        /// <param name="isSquidForm">イカ形態か</param>
        /// <returns>速度倍率、自軍判定、敵軍判定 のタプル</returns>
        public static (float multiplier, bool isOnOwnInk, bool isOnEnemyInk) GetSpeedMultiplier(Vector3 footPos, TeamId myTeam, bool isSquidForm)
        {
            // レイキャストで下方向の床取得
            if (!Physics.Raycast(footPos + Vector3.up * 0.3f, Vector3.down, out RaycastHit hit, 2f))
                return (NeutralMultiplier, false, false);

            var surface = hit.collider.GetComponent<PaintableSurface>();
            if (surface == null || surface.CpuMirror == null)
                return (NeutralMultiplier, false, false);

            // UV近似計算: BoxColliderはローカル±0.5正規化される
            Vector3 localPoint = surface.transform.InverseTransformPoint(hit.point);
            float u = localPoint.x + 0.5f;
            float v = localPoint.z + 0.5f;
            u = Mathf.Clamp01(u);
            v = Mathf.Clamp01(v);

            Color inkCol = surface.SampleAtUV(new Vector2(u, v));
            if (inkCol.a < InkAlphaThreshold)
                return (NeutralMultiplier, false, false);

            // RGBチャネルでチーム判定(Alpha=R強度、Bravo=B強度を使用)
            bool isOwnInk = false;
            bool isEnemyInk = false;
            if (myTeam == TeamId.Alpha && inkCol.r > inkCol.b + 0.1f) isOwnInk = true;
            else if (myTeam == TeamId.Bravo && inkCol.b > inkCol.r + 0.1f) isOwnInk = true;
            else if (inkCol.r > 0.3f || inkCol.b > 0.3f) isEnemyInk = true;

            if (isOwnInk)
            {
                return (isSquidForm ? OwnInkSwimMultiplier : OwnInkHumanMultiplier, true, false);
            }
            else if (isEnemyInk)
            {
                return (EnemyInkMultiplier, false, true);
            }
            return (NeutralMultiplier, false, false);
        }
    }
}
