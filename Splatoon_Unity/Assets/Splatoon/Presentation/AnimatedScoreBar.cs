using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Splatoon.Presentation
{
    /// <summary>
    /// 塗り進捗バーのアニメーション付き表示。
    /// 中央分割で両チームのバーが中央から左右に伸びる本家レイアウト。
    /// </summary>
    public class AnimatedScoreBar : MonoBehaviour
    {
        /// <summary>Alphaチーム塗り%(0-1)</summary>
        public float AlphaRatio = 0f;
        /// <summary>Bravoチーム塗り%(0-1)</summary>
        public float BravoRatio = 0f;

        /// <summary>Alphaバー(中央→左へfill)</summary>
        public Image AlphaBar;
        /// <summary>Bravoバー(中央→右へfill)</summary>
        public Image BravoBar;
        /// <summary>Alpha塗り%テキスト</summary>
        public TextMeshProUGUI AlphaPercent;
        /// <summary>Bravo塗り%テキスト</summary>
        public TextMeshProUGUI BravoPercent;
        /// <summary>リードチームの「LEAD!」表示</summary>
        public TextMeshProUGUI LeadText;
        /// <summary>バーアニメ速度</summary>
        public float AnimSpeed = 5f;

        // 内部
        float _alphaDisplay;
        float _bravoDisplay;

        void Update()
        {
            // 補間アニメ
            _alphaDisplay = Mathf.Lerp(_alphaDisplay, AlphaRatio, AnimSpeed * Time.deltaTime);
            _bravoDisplay = Mathf.Lerp(_bravoDisplay, BravoRatio, AnimSpeed * Time.deltaTime);

            // バー反映(中央から左へAlpha、中央から右へBravo)
            if (AlphaBar != null) AlphaBar.fillAmount = _alphaDisplay * 2f; // 半分割合だが見栄えで2倍
            if (BravoBar != null) BravoBar.fillAmount = _bravoDisplay * 2f;

            // %テキスト
            if (AlphaPercent != null) AlphaPercent.text = $"{_alphaDisplay * 100f:F1}";
            if (BravoPercent != null) BravoPercent.text = $"{_bravoDisplay * 100f:F1}";

            // リード表示(差5%以上で表示)
            if (LeadText != null)
            {
                float diff = _alphaDisplay - _bravoDisplay;
                if (Mathf.Abs(diff) > 0.05f)
                {
                    LeadText.text = diff > 0 ? "ALPHA LEAD!" : "BRAVO LEAD!";
                    LeadText.color = diff > 0 ? new Color(1f, 0.5f, 0f, 1f) : new Color(0.2f, 0.4f, 1f, 1f);
                    LeadText.gameObject.SetActive(true);
                }
                else
                {
                    LeadText.gameObject.SetActive(false);
                }
            }
        }
    }
}
