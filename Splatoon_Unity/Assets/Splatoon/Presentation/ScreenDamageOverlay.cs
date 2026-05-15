using UnityEngine;
using UnityEngine.UI;

namespace Splatoon.Presentation
{
    /// <summary>
    /// 画面ダメージオーバーレイ。被弾時に画面端を赤くフラッシュ、HP低下で常時赤め。
    /// </summary>
    public class ScreenDamageOverlay : MonoBehaviour
    {
        /// <summary>シングルトン</summary>
        public static ScreenDamageOverlay Instance;
        /// <summary>赤いVignette風オーバーレイImage</summary>
        public Image OverlayImage;
        /// <summary>追跡対象のPlayerHealth</summary>
        public PlayerHealth LocalHealth;

        float _flashAlpha;

        void Awake() { Instance = this; }

        void Update()
        {
            if (OverlayImage == null) return;

            // フラッシュ減衰
            _flashAlpha = Mathf.Lerp(_flashAlpha, 0f, Time.deltaTime * 3f);

            // HP低い時の常時赤味(HP30以下で徐々に増える)
            float lowHpAlpha = 0f;
            if (LocalHealth != null)
            {
                float hpRatio = LocalHealth.CurrentHP / LocalHealth.MaxHP;
                if (hpRatio < 0.4f)
                {
                    lowHpAlpha = (1f - hpRatio / 0.4f) * 0.3f;
                    // 心拍ぽいパルス
                    lowHpAlpha += Mathf.Sin(Time.time * 6f) * 0.05f;
                }
            }

            float totalAlpha = Mathf.Max(_flashAlpha, lowHpAlpha);
            Color c = OverlayImage.color;
            c.a = totalAlpha;
            OverlayImage.color = c;
        }

        /// <summary>外部から被弾フラッシュを発火</summary>
        public void TriggerFlash(float intensity = 0.5f)
        {
            _flashAlpha = Mathf.Max(_flashAlpha, intensity);
        }
    }
}
