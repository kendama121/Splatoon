using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Splatoon.Application;

namespace Splatoon.Presentation
{
    /// <summary>
    /// HUD表示管理(本家風レイアウト対応版)。
    /// タイマー・塗り進捗バー(AnimatedScoreBar)・インクタンク・スペシャルゲージ・武器情報・結果表示。
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        /// <summary>残り時間表示</summary>
        public TextMeshProUGUI TimerText;
        /// <summary>タイマー背景(残り30秒で赤に)</summary>
        public Image TimerBackground;
        /// <summary>塗りバー連動コンポーネント</summary>
        public AnimatedScoreBar ScoreBar;
        /// <summary>インクタンク残量Image(Vertical Fill)</summary>
        public Image InkTankFill;
        /// <summary>インクタンク%テキスト</summary>
        public TextMeshProUGUI InkPercentText;
        /// <summary>スペシャルゲージImage(Radial Fill)</summary>
        public Image SpecialGaugeFill;
        /// <summary>武器名表示</summary>
        public TextMeshProUGUI WeaponNameText;
        /// <summary>追跡対象プレイヤーのWeaponShooter</summary>
        public WeaponShooter LocalShooter;
        /// <summary>勝敗結果表示テキスト(MatchEndEffect等から制御)</summary>
        public TextMeshProUGUI ResultText;
        /// <summary>結果表示の背景パネル</summary>
        public Image ResultPanel;

        bool _matchEnded;
        float _resultAnimTime;

        void Update()
        {
            if (TurfWarMatchManager.Instance == null) return;
            var mgr = TurfWarMatchManager.Instance;

            // タイマー(残り時間 m:ss)
            int sec = Mathf.Max(0, Mathf.CeilToInt(mgr.RemainingTime));
            int m = sec / 60;
            int s = sec % 60;
            if (TimerText != null)
            {
                TimerText.text = $"{m}:{s:00}";
                // 残り30秒で赤化、10秒で点滅
                if (sec <= 30)
                {
                    Color col = (sec <= 10 && Mathf.Sin(Time.time * 12f) > 0)
                        ? new Color(1f, 0.2f, 0.2f, 1f)
                        : new Color(1f, 0.5f, 0.3f, 1f);
                    TimerText.color = col;
                }
                else
                {
                    TimerText.color = Color.white;
                }
            }
            if (TimerBackground != null && sec <= 30)
            {
                TimerBackground.color = new Color(0.4f, 0f, 0f, 0.7f);
            }

            // 塗り進捗バーへ値受け渡し
            if (ScoreBar != null)
            {
                ScoreBar.AlphaRatio = mgr.Score.TeamRatios[0];
                ScoreBar.BravoRatio = mgr.Score.TeamRatios[1];
            }

            // インクタンク
            if (LocalShooter != null)
            {
                float inkRatio = LocalShooter.CurrentInk / 100f;
                if (InkTankFill != null) InkTankFill.fillAmount = inkRatio;
                if (InkPercentText != null) InkPercentText.text = $"{LocalShooter.CurrentInk:F0}";
            }

            // スペシャルゲージ(MVPは塗りスコアで仮充電、本家は塗りポイントから)
            if (SpecialGaugeFill != null && LocalShooter != null)
            {
                // 仮:Alpha塗り率を1.5倍まで=スペシャルゲージ
                float specialCharge = Mathf.Clamp01(mgr.Score.TeamRatios[0] * 2f);
                SpecialGaugeFill.fillAmount = specialCharge;
            }

            // 武器名表示
            if (WeaponNameText != null && LocalShooter != null && LocalShooter.Weapon != null)
            {
                WeaponNameText.text = LocalShooter.Weapon.DisplayName;
            }

            // 結果表示
            if (ResultText != null)
            {
                if (!mgr.IsMatchActive && mgr.RemainingTime <= 0f)
                {
                    if (!_matchEnded)
                    {
                        _matchEnded = true;
                        _resultAnimTime = 0f;
                    }
                    _resultAnimTime += Time.unscaledDeltaTime;

                    ResultText.gameObject.SetActive(true);
                    bool isAlphaWin = mgr.Winner == Splatoon.Domain.TeamId.Alpha;
                    ResultText.text = isAlphaWin ? "YOU WIN!" : "YOU LOSE!";
                    ResultText.color = isAlphaWin ? new Color(1f, 0.85f, 0.1f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);

                    // スケールパンチアニメ
                    float scale = 1f + Mathf.Max(0f, 1f - _resultAnimTime * 2f) * 0.5f;
                    ResultText.rectTransform.localScale = Vector3.one * scale;

                    if (ResultPanel != null)
                    {
                        ResultPanel.gameObject.SetActive(true);
                        Color bg = isAlphaWin ? new Color(1f, 0.5f, 0f, 0.6f) : new Color(0.2f, 0.4f, 1f, 0.6f);
                        ResultPanel.color = bg;
                    }
                }
                else
                {
                    ResultText.gameObject.SetActive(false);
                    if (ResultPanel != null) ResultPanel.gameObject.SetActive(false);
                    _matchEnded = false;
                }
            }
        }
    }
}
