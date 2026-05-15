using UnityEngine;

namespace Splatoon.Infrastructure
{
    /// <summary>
    /// プロシージャルテクスチャ生成。床/壁/Crate/屋根等のディテール用パターンをコードで合成。
    /// 外部画像アセット不要。
    /// </summary>
    public static class ProceduralTextureBuilder
    {
        /// <summary>グリッドタイル(白床+細線+目地ノイズ)</summary>
        public static Texture2D MakeGridTile(int size = 512, int cellPixels = 64, float noiseStrength = 0.06f)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Trilinear;
            tex.anisoLevel = 4;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int cx = x % cellPixels;
                    int cy = y % cellPixels;
                    bool line = cx < 2 || cy < 2;
                    float noise = Mathf.PerlinNoise(x * 0.03f, y * 0.03f) * noiseStrength;
                    float v = line ? 0.55f : (0.92f - noise);
                    pixels[y * size + x] = new Color(v, v, v * 1.02f, 1f);
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            return tex;
        }

        /// <summary>ストライプパターン(屋根や桟橋)</summary>
        public static Texture2D MakeStripes(int size, Color a, Color b, int stripeWidth)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    px[y * size + x] = (x / stripeWidth) % 2 == 0 ? a : b;
            tex.SetPixels(px); tex.Apply();
            return tex;
        }

        /// <summary>木目(縦線+ノイズ)</summary>
        public static Texture2D MakeWoodGrain(int size, Color baseCol, Color darkCol)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float ring = Mathf.Sin(x * 0.04f + Mathf.PerlinNoise(x * 0.01f, y * 0.005f) * 3f);
                    float t = (ring + 1f) * 0.5f;
                    float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.15f;
                    px[y * size + x] = Color.Lerp(baseCol, darkCol, t * 0.7f + noise);
                }
            }
            tex.SetPixels(px); tex.Apply();
            return tex;
        }

        /// <summary>六角ハニカム(ハイランド上面、SF風)</summary>
        public static Texture2D MakeHexPattern(int size, Color baseCol, Color hexCol, float hexSize = 40f)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            var px = new Color[size * size];
            float w = hexSize * Mathf.Sqrt(3f);
            float h = hexSize * 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x;
                    float dy = y;
                    float colF = dx / (w * 0.5f);
                    float rowF = dy / (h * 0.75f);
                    int col = Mathf.RoundToInt(colF);
                    int row = Mathf.RoundToInt(rowF);
                    float cx = col * w * 0.5f;
                    float cy = row * h * 0.75f;
                    if (row % 2 == 1) cx += w * 0.5f;
                    float distance = Vector2.Distance(new Vector2(dx, dy), new Vector2(cx, cy));
                    bool isLine = Mathf.Abs(distance - hexSize * 0.85f) < 1.5f;
                    px[y * size + x] = isLine ? hexCol : baseCol;
                }
            }
            tex.SetPixels(px); tex.Apply();
            return tex;
        }

        /// <summary>金属プレート(リベット付き)</summary>
        public static Texture2D MakeMetalPlate(int size, Color baseCol)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            var px = new Color[size * size];
            int plate = size / 4;
            int rivet = 12;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int px2 = x % plate;
                    int py2 = y % plate;
                    bool plateLine = px2 < 2 || py2 < 2;
                    bool rivetDot = (px2 < rivet || px2 > plate - rivet) && (py2 < rivet || py2 > plate - rivet)
                                    && Vector2.Distance(new Vector2(Mathf.Min(px2, plate - px2), Mathf.Min(py2, plate - py2)), Vector2.zero) < rivet * 0.6f;
                    float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.1f;
                    Color c = baseCol * (0.9f + noise);
                    if (plateLine) c *= 0.55f;
                    if (rivetDot) c = baseCol * 1.3f;
                    px[y * size + x] = c;
                }
            }
            tex.SetPixels(px); tex.Apply();
            return tex;
        }

        /// <summary>都市夜景の窓パターン(発光ビル)</summary>
        public static Texture2D MakeCityWindows(int size, Color wall, Color lit)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            var px = new Color[size * size];
            int wW = 16;
            int wH = 20;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int rx = x % wW;
                    int ry = y % wH;
                    bool isWindow = rx >= 4 && rx <= wW - 4 && ry >= 4 && ry <= wH - 4;
                    // 一部の窓だけ点灯
                    int wid = (x / wW) * 1024 + (y / wH);
                    bool lit2 = isWindow && (wid * 7919 % 100) < 55;
                    px[y * size + x] = lit2 ? lit : wall;
                }
            }
            tex.SetPixels(px); tex.Apply();
            return tex;
        }
    }
}
