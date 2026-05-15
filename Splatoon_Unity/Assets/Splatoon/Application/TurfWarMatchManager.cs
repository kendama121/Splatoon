using UnityEngine;
using Splatoon.Domain;
using Splatoon.Infrastructure;

namespace Splatoon.Application
{
    /// <summary>
    /// ナワバリバトル試合進行管理。
    /// 3分タイマー、スコア集計、勝敗判定を行う。
    /// </summary>
    public class TurfWarMatchManager : MonoBehaviour
    {
        /// <summary>シングルトン参照</summary>
        public static TurfWarMatchManager Instance;

        /// <summary>試合時間(秒)。本家ナワバリは180秒</summary>
        public float MatchDuration = 180f;
        /// <summary>集計対象の床PaintableSurface参照</summary>
        public PaintableSurface GroundSurface;
        /// <summary>スコア更新間隔(秒)</summary>
        public float ScoreUpdateInterval = 1f;

        // 公開状態
        /// <summary>残り時間(秒)</summary>
        public float RemainingTime { get; set; }
        /// <summary>試合進行フラグ</summary>
        public bool IsMatchActive { get; set; }
        /// <summary>勝者(試合終了後に設定)</summary>
        public TeamId Winner { get; set; } = TeamId.Neutral;
        /// <summary>スコア集計</summary>
        public ScoreCalculator Score { get; set; }

        // 内部
        float _scoreTimer;

        void Awake()
        {
            Instance = this;
            Score = new ScoreCalculator();
            Score.Initialize();
        }

        void Start()
        {
            StartMatch();
        }

        /// <summary>
        /// 試合開始。
        /// </summary>
        public void StartMatch()
        {
            RemainingTime = MatchDuration;
            IsMatchActive = true;
            Winner = TeamId.Neutral;
        }

        void Update()
        {
            if (!IsMatchActive) return;

            // タイマー進行
            RemainingTime -= Time.deltaTime;
            if (RemainingTime <= 0f)
            {
                EndMatch();
                return;
            }

            // スコア定期更新
            _scoreTimer += Time.deltaTime;
            if (_scoreTimer >= ScoreUpdateInterval)
            {
                _scoreTimer = 0f;
                Score.RequestUpdate(GroundSurface);
            }
        }

        /// <summary>
        /// 試合終了処理。スコア最終確認 + 勝者判定。
        /// </summary>
        public void EndMatch()
        {
            IsMatchActive = false;
            RemainingTime = 0f;
            // 最終スコアで勝者判定
            float maxRatio = -1f;
            for (int i = 0; i < Score.TeamRatios.Length; i++)
            {
                if (Score.TeamRatios[i] > maxRatio)
                {
                    maxRatio = Score.TeamRatios[i];
                    Winner = (TeamId)i;
                }
            }
            Debug.Log($"[Match End] Winner: {Winner}, Ratios: A={Score.TeamRatios[0]:P1} B={Score.TeamRatios[1]:P1}");
        }

        void OnDestroy()
        {
            if (Score != null) Score.Dispose();
        }
    }
}
