using UnityEngine;
using UnityEngine.Rendering;
using Splatoon.Domain;

namespace Splatoon.Infrastructure
{
    /// <summary>
    /// インク塗装システムの中核Singleton。
    /// 各PaintableSurfaceに対してCommandBufferでブラシ描画を実行。
    /// Mix and Jam方式(MIT)をURP対応に移植した実装。
    /// </summary>
    public class InkPaintManager : MonoBehaviour
    {
        /// <summary>インスタンス参照(シーン内に1つだけ存在)</summary>
        public static InkPaintManager Instance;

        /// <summary>ペインターシェーダ(Splatoon/TexturePainter)</summary>
        public Shader TexturePainterShader;
        /// <summary>シーム埋めシェーダ(Splatoon/ExtendIslands)</summary>
        public Shader ExtendIslandsShader;

        // 内部状態
        Material _paintMaterial;
        Material _extendIslandsMaterial;
        CommandBuffer _commandBuffer;

        // チーム色定義(ScriptableObjectに移行予定、MVPはハードコード)
        /// <summary>チーム別の塗装色マッピング</summary>
        public Color[] TeamColors = new Color[]
        {
            new Color(1.0f, 0.5f, 0.0f, 1.0f), // Alpha: オレンジ
            new Color(0.2f, 0.4f, 1.0f, 1.0f), // Bravo: 青
            new Color(0.2f, 1.0f, 0.4f, 1.0f), // Charlie: 緑
            new Color(1.0f, 0.2f, 0.8f, 1.0f), // Delta: ピンク
        };

        /// <summary>
        /// 初期化。マテリアル生成、CommandBuffer確保。
        /// </summary>
        void Awake()
        {
            Instance = this;
            _paintMaterial = new Material(TexturePainterShader);
            _extendIslandsMaterial = new Material(ExtendIslandsShader);
            _commandBuffer = new CommandBuffer { name = "InkPaint" };
        }

        /// <summary>
        /// チームIDから対応する塗装色を取得。
        /// </summary>
        public Color GetTeamColor(TeamId team)
        {
            int idx = (int)team;
            if (idx < 0 || idx >= TeamColors.Length) return Color.gray;
            return TeamColors[idx];
        }

        /// <summary>
        /// 指定サーフェスのワールド座標に塗装を実行。
        /// </summary>
        /// <param name="surface">塗装対象のサーフェス</param>
        /// <param name="worldPos">塗装中心のワールド座標</param>
        /// <param name="radius">塗装半径(m)</param>
        /// <param name="team">塗装チーム</param>
        /// <param name="hardness">エッジの硬さ(0-1、1で完全硬エッジ)</param>
        public void Paint(PaintableSurface surface, Vector3 worldPos, float radius, TeamId team, float hardness = 0.95f)
        {
            Color color = GetTeamColor(team);
            Paint(surface, worldPos, radius, color, hardness);
        }

        /// <summary>
        /// 指定サーフェスへ任意色で塗装。
        /// </summary>
        public void Paint(PaintableSurface surface, Vector3 worldPos, float radius, Color color, float hardness = 0.95f)
        {
            _paintMaterial.SetVector("_BrushWorldPos", worldPos);
            _paintMaterial.SetFloat("_BrushRadius", radius);
            _paintMaterial.SetFloat("_BrushHardness", hardness);
            _paintMaterial.SetColor("_BrushColor", color);
            // スプラットごとに異なるシード→形状を毎回変化させる(ギザギザの方向ランダム化)
            _paintMaterial.SetFloat("_BrushSeed", Time.time * 17.371f + Random.value * 100f);

            _commandBuffer.Clear();
            // Step1: maskSupportへペイント描画(メッシュをUV2展開して描画)
            _commandBuffer.SetRenderTarget(surface.MaskSupport);
            _commandBuffer.DrawRenderer(surface.Renderer, _paintMaterial, 0);
            // Step2: シーム埋め適用してメインmaskへ
            _commandBuffer.Blit(surface.MaskSupport, surface.MaskRT, _extendIslandsMaterial);
            Graphics.ExecuteCommandBuffer(_commandBuffer);
        }
    }
}
