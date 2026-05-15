Shader "Splatoon/TexturePainter"
{
    // インク塗装用ペインターシェーダ(URP対応・スプラット形状不規則化版)
    // 半径方向のノイズ歪みでスプラトゥーン特有のギザギザ塗り痕を生成
    Properties
    {
        _BrushWorldPos ("Brush World Position", Vector) = (0,0,0,0)
        _BrushRadius ("Brush Radius", Float) = 0.5
        _BrushHardness ("Brush Hardness", Range(0,1)) = 0.9
        _BrushColor ("Brush Color", Color) = (1,0.5,0,1)
        _BrushSeed ("Brush Seed", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZTest Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            float3 _BrushWorldPos;
            float _BrushRadius;
            float _BrushHardness;
            float4 _BrushColor;
            float _BrushSeed;

            // 簡易疑似ランダム(0-1)
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // 2D ノイズ(値補間)
            float noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            Varyings Vert(Attributes v)
            {
                Varyings o;
                float2 uv = v.uv1.xy;
                o.positionCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);
                o.positionCS.y *= _ProjectionParams.x;
                o.worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.uv1 = uv;
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                // ブラシ中心からの相対座標
                float3 rel = i.worldPos - _BrushWorldPos;
                float dist = length(rel.xz); // 水平方向距離

                // 方向ベクトル(angle計算用)
                float2 dir = (dist > 0.001) ? normalize(rel.xz) : float2(1, 0);
                float angle = atan2(dir.y, dir.x);

                // 角度方向のノイズで半径を歪ませる→花びら/水滴の様な不規則境界
                // 複数周波数のサイン合成でフラクタル感
                float radialNoise = sin(angle * 6.0 + _BrushSeed * 1.3) * 0.18;
                radialNoise += sin(angle * 13.0 + _BrushSeed * 2.7) * 0.10;
                radialNoise += sin(angle * 27.0 - _BrushSeed * 4.1) * 0.05;

                // 周辺位置のノイズも加算(ジェル感)
                float worldNoise = noise2D(i.worldPos.xz * 8.0 + _BrushSeed) * 0.10;
                radialNoise += worldNoise;

                float effectiveRadius = _BrushRadius * (1.0 + radialNoise);

                // 距離マスク(エッジ硬さ調整)
                float mask = 1.0 - smoothstep(effectiveRadius * _BrushHardness, effectiveRadius, dist);

                // 中心に向けて少し色を濃く
                mask = saturate(mask);

                return half4(_BrushColor.rgb, mask);
            }
            ENDHLSL
        }
    }
}
