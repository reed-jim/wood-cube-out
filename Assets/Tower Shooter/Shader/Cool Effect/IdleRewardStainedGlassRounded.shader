Shader "Custom/URP/IdleRewardStainedGlassRounded"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        
        [Header(Crystal Voronoi Settings)]
        _CellSize ("Crystal Cell Size", Range(5.0, 50.0)) = 18.0
        _RefractionStrength ("Refraction Distortion", Range(0.0, 0.05)) = 0.015
        _EdgeWidth ("Lead Line Width", Range(0.01, 0.1)) = 0.03
        _EdgeColor ("Lead Line Color", Color) = (0.05, 0.03, 0.02, 1.0)
        
        [Header(Glass Tint Sparkle)]
        _GlassTint ("Stained Glass Tint", Color) = (0.9, 0.7, 0.2, 0.6)
        _Speed ("Crystal Shimmer Speed", Float) = 0.5

        [Header(Rounded Corners)]
        _CornerRadius ("Corner Radius", Range(0.0, 0.5)) = 0.1
        _CornerSmoothness ("Corner Smoothness", Range(0.001, 0.05)) = 0.005

        [Header(Transition Control)]
        _Transition ("Transition Progress", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float4 screenPos    : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _EdgeColor;
                float4 _GlassTint;
                float _CellSize;
                float _RefractionStrength;
                float _EdgeWidth;
                float _Speed;
                float _CornerRadius;
                float _CornerSmoothness;
                float _Transition;
            CBUFFER_END

            float2 Hash22(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453123);
            }

            float2 Voronoi(float2 uv)
            {
                float2 n = floor(uv);
                float2 f = frac(uv);
                float m1 = 8.0;
                float m2 = 8.0;

                [unroll]
                for (int y = -1; y <= 1; y++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 g = float2(float(x), float(y));
                        float2 o = Hash22(n + g);
                        o = 0.5 + 0.5 * sin(_Time.y * _Speed + o * 6.2831);
                        float2 r = g + o - f;
                        float d = dot(r, r);

                        if (d < m1)
                        {
                            m2 = m1;
                            m1 = d;
                        }
                        else if (d < m2)
                        {
                            m2 = d;
                        }
                    }
                }
                return float2(sqrt(m1), sqrt(m2));
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }

            // Corrected Rounded Box SDF using UI Rect aspect ratio
            float RoundedBoxSDF(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 screenUV = input.screenPos.xy / input.screenPos.w;

                // -------------------------------------------------------------
                // 1. Aspect-Ratio Corrected Rounded Corners
                // -------------------------------------------------------------
                // Calculate UI Image aspect ratio from screen pixel size or derivative scales
                // Alternatively, we derive aspect ratio using screen width/height mapping or DDX/DDY.
                // Using ddx/ddy ensures it maps precisely to whatever RectTransform size the Image has.
                float2 duvdx = ddx(uv);
                float2 duvdy = ddy(uv);
                float pixelWidth = length(duvdx);
                float pixelHeight = length(duvdy);
                
                // Get the local aspect ratio of the UI element rect
                float rectAspect = (pixelHeight > 0.0 && pixelWidth > 0.0) ? (pixelHeight / pixelWidth) : 1.0;

                // Center UVs and normalize based on rect dimensions
                float2 centeredUV = (uv - 0.5);
                if (rectAspect > 1.0)
                    centeredUV.x *= rectAspect;
                else
                    centeredUV.y /= rectAspect;

                // Box dimensions matching the scaled space (-0.5 to 0.5 box)
                float2 boxSize = float2(0.5, 0.5);
                if (rectAspect > 1.0) boxSize.x *= rectAspect;
                else boxSize.y /= rectAspect;

                float boxDist = RoundedBoxSDF(centeredUV, boxSize, _CornerRadius);
                float cornerMask = 1.0 - smoothstep(0.0, _CornerSmoothness, boxDist);

                if (cornerMask <= 0.0)
                    return float4(0, 0, 0, 0);

                // -------------------------------------------------------------
                // 2. Voronoi Crystal Cell Calculation
                // -------------------------------------------------------------
                float screenAspect = _ScreenParams.x / _ScreenParams.y;
                float2 scaledUV = uv * float2(screenAspect, 1.0) * _CellSize;

                float2 voronoiDist = Voronoi(scaledUV);
                float crystalEdge = voronoiDist.y - voronoiDist.x;

                // -------------------------------------------------------------
                // 3. Chromatic Dispersion & Refraction
                // -------------------------------------------------------------
                float2 randomOffset = (Hash22(floor(scaledUV)) - 0.5) * _RefractionStrength * (1.0 - _Transition);
                
                float r = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + randomOffset + float2(0.002, 0)).r;
                float g = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + randomOffset).g;
                float b = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + randomOffset - float2(0.002, 0)).b;
                
                float4 refractedBG = float4(r, g, b, 1.0);

                float cellID = Hash22(floor(scaledUV)).x;
                float4 cellTint = lerp(_GlassTint, _GlassTint * 1.3, cellID);
                float4 glassColor = refractedBG * cellTint;

                float leadMask = smoothstep(0.0, _EdgeWidth, crystalEdge);
                float4 finalGlass = lerp(_EdgeColor, glassColor, leadMask);

                // -------------------------------------------------------------
                // 4. Geometric Shard Shatter Transition
                // -------------------------------------------------------------
                float shardDissolveThreshold = _Transition * 1.3;
                if (cellID < shardDissolveThreshold)
                {
                    float4 pureImage = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                    return float4(pureImage.rgb, pureImage.a * cornerMask);
                }

                return finalGlass * (input.color * cornerMask);
            }
            ENDHLSL
        }
    }
}