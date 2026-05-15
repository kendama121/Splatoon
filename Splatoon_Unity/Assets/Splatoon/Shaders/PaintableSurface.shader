Shader "Splatoon/PaintableSurface"
{
    // 塗装可能サーフェスの表示シェーダ(URP Lit互換)
    // ベース色 + ベースグリッド + 塗装テクスチャの3層合成
    // ネオン感を出すためインク部分にEmission+Smoothness boost
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.85, 0.85, 0.85, 1)
        _SplatTex ("Splat Texture", 2D) = "black" {}
        _Smoothness ("Smoothness", Range(0,1)) = 0.4
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _InkGloss ("Ink Wet Gloss", Range(0,1)) = 0.92
        _InkEmission ("Ink Emission Strength", Range(0,5)) = 1.5
        _GridScale ("Grid Scale (cells per meter)", Float) = 0.5
        _GridStrength ("Grid Strength", Range(0,1)) = 0.25
        _GridColor ("Grid Line Color", Color) = (0.5, 0.55, 0.6, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float2 uvSplat : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _Smoothness;
                float _Metallic;
                float _InkGloss;
                float _InkEmission;
                float _GridScale;
                float _GridStrength;
                float4 _GridColor;
            CBUFFER_END

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_SplatTex); SAMPLER(sampler_SplatTex);

            Varyings Vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs pIn = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = pIn.positionCS;
                o.positionWS = pIn.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.uvSplat = v.uv1;
                return o;
            }

            // グリッド線計算(ワールド座標基準でステージ感)
            float ComputeGridLine(float3 wpos)
            {
                // 水平面のグリッド(XZ平面、_GridScaleで密度調整)
                float2 gridCoord = wpos.xz * _GridScale;
                float2 grid = abs(frac(gridCoord) - 0.5);
                float lineFactor = 1.0 - smoothstep(0.45, 0.5, max(grid.x, grid.y));
                return lineFactor;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                half4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                half4 splat = SAMPLE_TEXTURE2D(_SplatTex, sampler_SplatTex, i.uvSplat);

                // グリッド線をベース色に適用(塗装がある場所は薄く)
                float gridLine = ComputeGridLine(i.positionWS);
                half3 withGrid = lerp(baseCol.rgb, _GridColor.rgb, gridLine * _GridStrength);

                // ベース色とインク色を塗装マスクで補間
                half splatMask = splat.a;
                half3 albedo = lerp(withGrid, splat.rgb, splatMask);

                // 軽い世界ノイズで床に微細凹凸(視覚的)
                float microNoise = frac(sin(dot(i.positionWS.xz, float2(94.7, 67.1))) * 1234.5) * 0.04;
                albedo *= (1.0 - microNoise);

                // URP PBR材質設定
                SurfaceData s = (SurfaceData)0;
                s.albedo = albedo;
                s.alpha = 1;
                s.normalTS = half3(0, 0, 1);
                s.metallic = _Metallic;
                // インク部分は強い光沢で湿った感じ
                s.smoothness = lerp(_Smoothness, _InkGloss, splatMask);
                s.occlusion = 1;
                // インク部分は強い自発光でネオン感
                s.emission = splat.rgb * splatMask * _InkEmission;

                InputData inp = (InputData)0;
                inp.positionWS = i.positionWS;
                inp.normalWS = NormalizeNormalPerPixel(i.normalWS);
                inp.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
                inp.shadowCoord = TransformWorldToShadowCoord(i.positionWS);

                return UniversalFragmentPBR(inp, s);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma vertex VertShadow
            #pragma fragment FragShadow
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            float4 VertShadow(float4 positionOS : POSITION, float3 normalOS : NORMAL) : SV_POSITION
            {
                float3 posWS = TransformObjectToWorld(positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(posWS, normalWS, _LightDirection));
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return positionCS;
            }
            half4 FragShadow() : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
