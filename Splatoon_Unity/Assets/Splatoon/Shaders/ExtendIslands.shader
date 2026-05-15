Shader "Splatoon/ExtendIslands"
{
    // UVシーム埋め用 8-tap dilation シェーダ(URP対応)
    // 塗装マスクのUV境界部分に黒い線が出るのを防ぐ
    Properties { _MainTex ("Texture", 2D) = "white" {} }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZTest Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 ts = _MainTex_TexelSize.xy;
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                if (c.a >= 1) return c;

                half4 maxC = c;
                // 8方向サンプリングで最大値取得 → 塗装エッジを外側へ伸ばす
                maxC = max(maxC, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + ts * float2(-1,-1)));
                maxC = max(maxC, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + ts * float2( 0,-1)));
                maxC = max(maxC, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + ts * float2( 1,-1)));
                maxC = max(maxC, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + ts * float2(-1, 0)));
                maxC = max(maxC, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + ts * float2( 1, 0)));
                maxC = max(maxC, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + ts * float2(-1, 1)));
                maxC = max(maxC, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + ts * float2( 0, 1)));
                maxC = max(maxC, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + ts * float2( 1, 1)));
                return maxC;
            }
            ENDHLSL
        }
    }
}
