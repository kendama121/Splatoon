using UnityEngine;
using Splatoon.Domain;

namespace Splatoon.Application
{
    /// <summary>
    /// ガチマッチ4種共通の基底クラス。StateパターンでTurfWarMatchManagerから利用。
    /// </summary>
    public abstract class MatchModeBase : MonoBehaviour
    {
        /// <summary>モード名(表示用)</summary>
        public abstract string ModeName { get; }
        /// <summary>勝者(ノックアウト判定後)</summary>
        public TeamId Winner { get; set; } = TeamId.Neutral;
        /// <summary>ノックアウト判定(true=即終了)</summary>
        public bool IsKnockedOut { get; set; }

        /// <summary>モード初期化</summary>
        public abstract void OnMatchStart();
        /// <summary>毎フレーム更新</summary>
        public abstract void OnMatchUpdate();
        /// <summary>終了時の最終勝者決定</summary>
        public abstract TeamId EvaluateFinalWinner();
    }
}
