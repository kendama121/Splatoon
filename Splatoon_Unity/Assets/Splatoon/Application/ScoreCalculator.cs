using UnityEngine;
using UnityEngine.Rendering;
using Splatoon.Infrastructure;

namespace Splatoon.Application
{
    /// <summary>
    /// 塗装スコア集計。AsyncGPUReadbackで非同期にMaskRTを読み取り、
    /// 各チャネル(R/G/B/A)の平均で各チーム塗装率を算出。
    /// 毎フレーム実行は重いので1秒間隔推奨。
    /// </summary>
    public class ScoreCalculator
    {
        /// <summary>各チームの塗装率(0-1、配列インデックス=TeamId int値)</summary>
        public float[] TeamRatios = new float[4];
        /// <summary>未塗装の割合(0-1)</summary>
        public float UnpaintedRatio = 1f;

        // 内部
        RenderTexture _downsampledRT;
        const int DOWNSAMPLE_SIZE = 64; // 64×64まで縮小して集計

        /// <summary>
        /// 集計用RT初期化。
        /// </summary>
        public void Initialize()
        {
            _downsampledRT = new RenderTexture(DOWNSAMPLE_SIZE, DOWNSAMPLE_SIZE, 0, RenderTextureFormat.ARGB32);
            _downsampledRT.useMipMap = false;
            _downsampledRT.filterMode = FilterMode.Bilinear;
            _downsampledRT.Create();
        }

        /// <summary>
        /// PaintableSurfaceから塗装率を計算してリクエスト送信。
        /// </summary>
        public void RequestUpdate(PaintableSurface surface)
        {
            if (surface == null || surface.MaskRT == null) return;
            // 縮小コピー
            Graphics.Blit(surface.MaskRT, _downsampledRT);
            // 非同期読み戻し
            AsyncGPUReadback.Request(_downsampledRT, 0, TextureFormat.RGBA32, OnReadback);
        }

        /// <summary>
        /// GPUからCPUへ読み戻し完了時のコールバック。
        /// </summary>
        void OnReadback(AsyncGPUReadbackRequest req)
        {
            if (req.hasError) return;
            var data = req.GetData<Color32>();
            int count = data.Length;
            if (count == 0) return;

            // 各チャネル累積
            long sumR = 0, sumG = 0, sumB = 0, sumA = 0;
            for (int i = 0; i < count; i++)
            {
                var c = data[i];
                sumR += c.r;
                sumG += c.g;
                sumB += c.b;
                sumA += c.a;
            }

            // 平均化(0-1)
            float total = count * 255f;
            // チーム識別はAチャネル(塗装マスク)とRGB(色)を組合せ
            // MVPでは各チームのRGBチャネルから直接識別
            TeamRatios[0] = (sumR / total);  // 仮: R強度=Alpha(オレンジ)
            TeamRatios[1] = (sumG / total);  // 仮: G強度=Bravo(青系...青はBチャネルだが簡略)
            TeamRatios[2] = 0f;
            TeamRatios[3] = 0f;

            // 簡易方式: アルファ=塗装率、RGB比でチーム分配
            float paintedRatio = sumA / total;
            UnpaintedRatio = 1f - paintedRatio;
        }

        /// <summary>
        /// 後始末。
        /// </summary>
        public void Dispose()
        {
            if (_downsampledRT != null) _downsampledRT.Release();
        }
    }
}
