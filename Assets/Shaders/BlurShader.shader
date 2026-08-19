Shader "Custom/BoxBlur"
{
    // This shader script was modified from https://github.com/Firnox/BlurShader.
    // Thank you for sharing this shader to the public.

    // The modifications include:
    // - blurring a MainTex instead of the camera opaque render texture
    // - using Gaussian blur instead of box blur.
    Properties
    {
        _MainTex("Texture to be blurred", 2D) = "white" {}
        _Blur ("Blur radius", Integer) = 1
        _Scale ("Scale (texel offset)", Range(1, 5)) = 1
        _Sigma ("Gaussian Sigma", Range(0.1, 10)) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                int _Blur;
                float _Scale;
                float _Sigma;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // Calculate the position of the vertex on the screen.
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // float rather than half as accumulation may exceed half precision.
                float4 OUT = 0.0;

                float totalWeight = 0;
                // Calculate the size of a texel in screen space, scaled up.
                float2 texel = _Scale * _MainTex_TexelSize.xy;

                // Ensure blur is at least 1.
                int blur_size = _Blur > 0 ? _Blur : 1;

                float twoSigmaSquared = 2.0 * _Sigma * _Sigma;

                // Iterate over the pixels in our convolution filter.
                for (int i = -blur_size; i <= blur_size; i++)
                {
                    for (int j = -blur_size; j <= blur_size; j++)
                    {
                        int sqrDistance = i * i + j * j;
                        float gaussianWeight = exp(-sqrDistance / twoSigmaSquared);

                        // Simply sum up each pixel.
                        OUT += SAMPLE_TEXTURE2D(
                                      _MainTex,
                                      sampler_MainTex,
                                      IN.uv + (float2(i, j) * texel)) * gaussianWeight;

                        totalWeight += gaussianWeight;
                    }
                }

                // Normalise by the number of points we've sampled, to maintain brightness.
                OUT /= totalWeight;

                return half4(OUT.rgb, 1.0);
            }
            
            ENDHLSL
        }
    }
}