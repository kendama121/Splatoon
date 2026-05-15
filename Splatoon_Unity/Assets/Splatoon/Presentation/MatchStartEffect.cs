using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Splatoon.Application;

namespace Splatoon.Presentation
{
    /// <summary>
    /// 試合開始演出。3秒カウントダウン+「GO!」スプラッシュを表示。
    /// 開始前はMatchManagerを一時停止して操作不可にする。
    /// </summary>
    public class MatchStartEffect : MonoBehaviour
    {
        /// <summary>カウントダウン表示用テキスト</summary>
        public TextMeshProUGUI CountdownText;
        /// <summary>「TURF WAR」タイトル表示</summary>
        public TextMeshProUGUI TitleText;
        /// <summary>カウントダウン秒数</summary>
        public int CountdownSeconds = 3;

        IEnumerator Start()
        {
            // MatchManager一時停止
            if (TurfWarMatchManager.Instance != null) TurfWarMatchManager.Instance.IsMatchActive = false;

            // タイトル表示「TURF WAR」
            if (TitleText != null)
            {
                TitleText.gameObject.SetActive(true);
                TitleText.text = "TURF WAR";
                TitleText.color = new Color(1f, 0.85f, 0.1f, 1f);
                yield return AnimateTextScale(TitleText, 0.5f, 1.6f, 0.4f);
                yield return new WaitForSeconds(0.8f);
                yield return AnimateTextFade(TitleText, 1f, 0f, 0.3f);
                TitleText.gameObject.SetActive(false);
            }

            // カウントダウン3→2→1
            if (CountdownText != null)
            {
                CountdownText.gameObject.SetActive(true);
                for (int i = CountdownSeconds; i >= 1; i--)
                {
                    CountdownText.text = i.ToString();
                    CountdownText.color = (i == 1) ? new Color(1f, 0.5f, 0f, 1f) : Color.white;
                    yield return AnimateTextScale(CountdownText, 2.0f, 0.9f, 0.4f);
                    yield return new WaitForSeconds(0.55f);
                }

                // 「GO!」表示
                CountdownText.text = "GO!";
                CountdownText.color = new Color(1f, 0.85f, 0.1f, 1f);
                yield return AnimateTextScale(CountdownText, 0.4f, 2.4f, 0.35f);
                yield return new WaitForSeconds(0.5f);
                yield return AnimateTextFade(CountdownText, 1f, 0f, 0.3f);
                CountdownText.gameObject.SetActive(false);
            }

            // MatchManager開始
            if (TurfWarMatchManager.Instance != null)
            {
                TurfWarMatchManager.Instance.IsMatchActive = true;
                TurfWarMatchManager.Instance.RemainingTime = TurfWarMatchManager.Instance.MatchDuration;
            }
        }

        /// <summary>テキストのスケールを補間アニメ</summary>
        IEnumerator AnimateTextScale(TextMeshProUGUI text, float from, float to, float duration)
        {
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                float ease = 1f - Mathf.Pow(1f - p, 3); // EaseOutCubic
                float scale = Mathf.Lerp(from, to, ease);
                text.rectTransform.localScale = Vector3.one * scale;
                yield return null;
            }
            text.rectTransform.localScale = Vector3.one * to;
        }

        /// <summary>テキストのアルファ値を補間アニメ</summary>
        IEnumerator AnimateTextFade(TextMeshProUGUI text, float from, float to, float duration)
        {
            float t = 0;
            Color baseCol = text.color;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                Color c = baseCol;
                c.a = Mathf.Lerp(from, to, p);
                text.color = c;
                yield return null;
            }
            Color cf = baseCol;
            cf.a = to;
            text.color = cf;
        }
    }
}
