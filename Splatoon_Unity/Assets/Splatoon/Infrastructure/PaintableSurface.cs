using UnityEngine;
using UnityEngine.Rendering;

namespace Splatoon.Infrastructure
{
    /// <summary>
    /// 塗装可能サーフェス。各塗装対象オブジェクトにアタッチ。
    /// 専用RenderTextureを保持し、表示用マテリアルへ送信する。
    /// CPUミラー(低解像度Texture2D)を一定間隔で更新→足元色サンプリングに使用。
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class PaintableSurface : MonoBehaviour
    {
        /// <summary>塗装マスクテクスチャの解像度(2の累乗推奨)</summary>
        public int TextureSize = 1024;
        /// <summary>表示用マテリアル(Splatoon/PaintableSurface)のテンプレート</summary>
        public Material PaintableMaterialTemplate;
        /// <summary>未塗装時のベース色(灰色推奨)</summary>
        public Color BaseColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        /// <summary>CPUミラー解像度(色サンプリング用)</summary>
        public int CpuMirrorSize = 128;
        /// <summary>CPUミラー更新間隔(秒)</summary>
        public float CpuMirrorUpdateInterval = 0.25f;

        /// <summary>塗装マスク本体(表示シェーダが参照)</summary>
        [HideInInspector] public RenderTexture MaskRT;
        /// <summary>シーム埋め前の中間バッファ</summary>
        [HideInInspector] public RenderTexture MaskSupport;
        /// <summary>CPU側ミラー(色サンプリング用、低解像度)</summary>
        public Texture2D CpuMirror { get; set; }

        /// <summary>このサーフェスのRenderer参照</summary>
        public Renderer Renderer { get; set; }

        // 内部
        Material _runtimeMaterial;
        RenderTexture _downsampleRT;
        float _nextMirrorUpdate;

        /// <summary>
        /// 初期化。RenderTexture確保、マテリアルインスタンス生成。
        /// </summary>
        void Awake()
        {
            Renderer = GetComponent<Renderer>();

            // 塗装用RenderTexture(ARGB32、シーンサイズ依存)
            MaskRT = new RenderTexture(TextureSize, TextureSize, 0, RenderTextureFormat.ARGB32);
            MaskRT.filterMode = FilterMode.Bilinear;
            MaskRT.wrapMode = TextureWrapMode.Clamp;
            MaskRT.Create();
            MaskSupport = new RenderTexture(TextureSize, TextureSize, 0, RenderTextureFormat.ARGB32);
            MaskSupport.filterMode = FilterMode.Bilinear;
            MaskSupport.wrapMode = TextureWrapMode.Clamp;
            MaskSupport.Create();

            // ダウンサンプル用RT + CPUミラー
            _downsampleRT = new RenderTexture(CpuMirrorSize, CpuMirrorSize, 0, RenderTextureFormat.ARGB32);
            _downsampleRT.filterMode = FilterMode.Bilinear;
            _downsampleRT.Create();
            CpuMirror = new Texture2D(CpuMirrorSize, CpuMirrorSize, TextureFormat.RGBA32, false);
            CpuMirror.filterMode = FilterMode.Bilinear;
            CpuMirror.wrapMode = TextureWrapMode.Clamp;

            // 表示マテリアルのインスタンス生成
            _runtimeMaterial = new Material(PaintableMaterialTemplate);
            _runtimeMaterial.SetTexture("_SplatTex", MaskRT);
            _runtimeMaterial.SetColor("_BaseColor", BaseColor);
            Renderer.material = _runtimeMaterial;
        }

        /// <summary>
        /// 毎フレーム、定期的にCPUミラーを更新(AsyncGPUReadbackで非同期)。
        /// </summary>
        void Update()
        {
            if (Time.time < _nextMirrorUpdate) return;
            _nextMirrorUpdate = Time.time + CpuMirrorUpdateInterval;
            // ダウンサンプル
            Graphics.Blit(MaskRT, _downsampleRT);
            // 非同期読み戻し
            AsyncGPUReadback.Request(_downsampleRT, 0, TextureFormat.RGBA32, OnMirrorReadback);
        }

        /// <summary>
        /// GPU→CPU読み戻し完了時、Texture2D CpuMirrorに反映。
        /// </summary>
        void OnMirrorReadback(AsyncGPUReadbackRequest req)
        {
            if (req.hasError || CpuMirror == null) return;
            var data = req.GetData<Color32>();
            CpuMirror.SetPixelData(data, 0);
            CpuMirror.Apply(false, false);
        }

        /// <summary>
        /// 指定UV(0-1)の塗装色をCPUミラーからサンプリング。
        /// </summary>
        public Color SampleAtUV(Vector2 uv)
        {
            if (CpuMirror == null) return Color.clear;
            return CpuMirror.GetPixelBilinear(uv.x, uv.y);
        }

        /// <summary>
        /// 後始末。RenderTexture解放。
        /// </summary>
        void OnDestroy()
        {
            if (MaskRT != null) MaskRT.Release();
            if (MaskSupport != null) MaskSupport.Release();
            if (_downsampleRT != null) _downsampleRT.Release();
            if (CpuMirror != null) Destroy(CpuMirror);
        }
    }
}
