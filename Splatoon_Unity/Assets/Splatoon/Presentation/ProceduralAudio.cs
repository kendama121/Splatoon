using UnityEngine;

namespace Splatoon.Presentation
{
    /// <summary>
    /// プロシージャル音響。AudioClipをコード合成で生成し、SE再生。
    /// 発射音(ノイズ短音)、着弾音(低音バースト)、ジャンプ音(上昇ピッチ)等を提供。
    /// </summary>
    public class ProceduralAudio : MonoBehaviour
    {
        /// <summary>シングルトン参照</summary>
        public static ProceduralAudio Instance;

        AudioSource _audioSource;
        AudioClip _shootClip;
        AudioClip _impactClip;
        AudioClip _jumpClip;
        AudioClip _winClip;

        void Awake()
        {
            Instance = this;
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 0f; // 2D
            _audioSource.volume = 0.3f;
            _audioSource.playOnAwake = false;

            // クリップ生成(発射、着弾、ジャンプ、勝利ファンファーレ)
            _shootClip = GenerateShootClip();
            _impactClip = GenerateImpactClip();
            _jumpClip = GenerateJumpClip();
            _winClip = GenerateWinClip();
        }

        /// <summary>発射音(短いノイズスパイク)</summary>
        public void PlayShoot()
        {
            if (_shootClip != null) _audioSource.PlayOneShot(_shootClip, 0.4f);
        }

        /// <summary>着弾音(低音ポップ)</summary>
        public void PlayImpact()
        {
            if (_impactClip != null) _audioSource.PlayOneShot(_impactClip, 0.3f);
        }

        /// <summary>ジャンプ音(上昇スイープ)</summary>
        public void PlayJump()
        {
            if (_jumpClip != null) _audioSource.PlayOneShot(_jumpClip, 0.5f);
        }

        /// <summary>勝利ファンファーレ</summary>
        public void PlayWin()
        {
            if (_winClip != null) _audioSource.PlayOneShot(_winClip, 0.8f);
        }

        /// <summary>短いノイズスパイク(発射SE)生成</summary>
        AudioClip GenerateShootClip()
        {
            int sampleRate = 44100;
            float duration = 0.08f;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                float envelope = Mathf.Exp(-t * 25f); // 急速減衰
                float noise = (UnityEngine.Random.value - 0.5f) * 2f;
                data[i] = noise * envelope;
            }
            var clip = AudioClip.Create("Shoot", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>低音ポップ(着弾SE)生成</summary>
        AudioClip GenerateImpactClip()
        {
            int sampleRate = 44100;
            float duration = 0.18f;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                float envelope = Mathf.Exp(-t * 12f);
                float freq = 200f - t * 100f; // 周波数低下スイープ
                float sine = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate));
                float noise = (UnityEngine.Random.value - 0.5f) * 0.5f;
                data[i] = (sine + noise * 0.3f) * envelope * 0.7f;
            }
            var clip = AudioClip.Create("Impact", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>上昇スイープ(ジャンプSE)生成</summary>
        AudioClip GenerateJumpClip()
        {
            int sampleRate = 44100;
            float duration = 0.25f;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                float envelope = Mathf.Exp(-t * 6f);
                float freq = 300f + t * 600f; // 周波数上昇
                float sine = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate));
                data[i] = sine * envelope * 0.6f;
            }
            var clip = AudioClip.Create("Jump", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>勝利ファンファーレ(C-E-G-C上昇和音)</summary>
        AudioClip GenerateWinClip()
        {
            int sampleRate = 44100;
            float duration = 1.2f;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];
            // C4, E4, G4, C5 各0.3秒ずつ
            float[] freqs = { 261.6f, 329.6f, 392.0f, 523.3f };
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                int noteIdx = Mathf.Clamp((int)(t / 0.3f), 0, freqs.Length - 1);
                float noteT = (t % 0.3f) / 0.3f;
                float envelope = Mathf.Sin(noteT * Mathf.PI); // 山なり包絡
                float sine = Mathf.Sin(2f * Mathf.PI * freqs[noteIdx] * t);
                float harmonic = Mathf.Sin(2f * Mathf.PI * freqs[noteIdx] * 2f * t) * 0.3f;
                data[i] = (sine + harmonic) * envelope * 0.5f;
            }
            var clip = AudioClip.Create("Win", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
