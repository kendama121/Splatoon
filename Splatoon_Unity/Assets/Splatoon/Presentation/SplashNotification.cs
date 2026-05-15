using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Splatoon.Presentation
{
    /// <summary>
    /// スプラット通知。やられた時に画面中央に「YOU GOT SPLATTED!」を表示。
    /// やった時に「SPLATTED!」短い表示。
    /// </summary>
    public class SplashNotification : MonoBehaviour
    {
        /// <summary>シングルトン参照</summary>
        public static SplashNotification Instance;

        /// <summary>通知テキスト(Inspectorから設定)</summary>
        public TextMeshProUGUI NotificationText;
        /// <summary>通知背景(Image)</summary>
        public Image BackgroundImage;

        void Awake() { Instance = this; }

        /// <summary>
        /// 通知表示開始。指定秒数後に自動非表示。
        /// </summary>
        public void Show(string message, Color color, float duration = 2f)
        {
            StopAllCoroutines();
            StartCoroutine(ShowSequence(message, color, duration));
        }

        IEnumerator ShowSequence(string msg, Color col, float duration)
        {
            if (NotificationText == null) yield break;
            NotificationText.text = msg;
            NotificationText.color = col;
            NotificationText.gameObject.SetActive(true);
            if (BackgroundImage != null)
            {
                BackgroundImage.color = new Color(col.r * 0.3f, col.g * 0.3f, col.b * 0.3f, 0.7f);
                BackgroundImage.gameObject.SetActive(true);
            }

            // スケールアニメ(0.5→1.0、Punch風)
            float t = 0;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / 0.3f);
                float ease = 1f - Mathf.Pow(1f - p, 3);
                float scale = Mathf.Lerp(0.5f, 1f, ease);
                NotificationText.rectTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            yield return new WaitForSeconds(duration);

            // フェードアウト
            float fadeTime = 0.5f;
            t = 0;
            Color baseTextCol = NotificationText.color;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(1f, 0f, t / fadeTime);
                Color c = baseTextCol; c.a = a; NotificationText.color = c;
                if (BackgroundImage != null)
                {
                    Color bg = BackgroundImage.color; bg.a = a * 0.7f; BackgroundImage.color = bg;
                }
                yield return null;
            }
            NotificationText.gameObject.SetActive(false);
            if (BackgroundImage != null) BackgroundImage.gameObject.SetActive(false);
        }
    }
}
