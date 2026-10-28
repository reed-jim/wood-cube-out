// ============================================================================
//  UI/SpaceshipPlatformSolid  -  Unity URP, HLSL, UI Image (no sprite/texture)
//
//  Reference: Modern aerospace solid physical mounting plate (SpaceX / F-35 panel):
//    - Fully solid physical metal platform (outer border and inner surface use the exact same brushed metal)
//    - LED light strip precisely positioned around the outer border/edge perimeter (from previous version)
//
//  Setup: Image -> Image Type "Simple", Source Image "None", Material = this shader.
// ============================================================================
Shader "UI/SpaceshipPlatformSolid"
{
    Properties
    {
        [Header(State)]
        _Power ("Power / Engagement State", Range(0, 1)) = 1.0
        _Aspect ("Aspect W/H (0 = auto detect)", Float) = 0

        [Header(Chassis Geometry)]
        _OuterRadius ("Outer Corner Radius", Range(0, 0.5)) = 0.10
        _OuterChamfer ("Outer Edge Chamfer Width", Range(0.001, 0.05)) = 0.012

        [Header(Material Finish)]
        _ChassisColor ("Metal Base Color", Color) = (0.28, 0.30, 0.35, 1)
        _EnvTop ("Lighting Top Accent", Color) = (0.88, 0.94, 1.00, 1)
        _EnvBottom ("Lighting Bottom Accent", Color) = (0.03, 0.04, 0.06, 1)
        _Shininess ("Specular Sharpness", Range(5, 200)) = 80
        _Brushed ("Brushed Anodized Intensity", Range(0, 1)) = 0.40
        _BrushScale ("Brushed Line Density", Float) = 850

        [Header(Outer Border LED Strip)]
        _LedOffColor ("LED Standby Color", Color) = (0.95, 0.35, 0.05, 1)
        _LedOnColor ("LED Active Color", Color) = (0.10, 0.85, 1.00, 1)
        _LedWidth ("LED Strip Width", Range(0.001, 0.02)) = 0.005

        // UI system properties
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "RenderPipeline" = "UniversalPipeline"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha     // premultiplied alpha output
        ColorMask [_ColorMask]

        Pass
        {
            Name "SpaceshipPlatformSolidUI"

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Power;
                float _Aspect;
                float _OuterRadius;
                float _OuterChamfer;
                float4 _ChassisColor;
                float4 _EnvTop;
                float4 _EnvBottom;
                float _Shininess;
                float _Brushed;
                float _BrushScale;
                float4 _LedOffColor;
                float4 _LedOnColor;
                float _LedWidth;
                float4 _ClipRect;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float4 worldPos   : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.worldPos = IN.positionOS;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            float pow2(float x) { return x * x; }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float sdRoundBox(float2 p, float2 h, float r)
            {
                float2 q = abs(p) - h + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            float2 gradRoundBox(float2 p, float2 h, float r)
            {
                float2 s = float2(p.x >= 0.0 ? 1.0 : -1.0, p.y >= 0.0 ? 1.0 : -1.0);
                float2 q = abs(p) - h + r;
                float2 g;
                if (q.x > 0.0 || q.y > 0.0)
                    g = normalize(max(q, 0.0));
                else
                    g = (q.x > q.y) ? float2(1, 0) : float2(0, 1);
                return g * s;
            }

            float Cov(float d, float aa) { return saturate(0.5 - d / aa); }
            float Band(float x, float edge, float aa) { return 1.0 - smoothstep(edge - aa, edge + aa, x); }

            float3 EnvMap(float3 r)
            {
                float sky = smoothstep(-0.8, 0.9, r.y);
                float3 env = lerp(_EnvBottom.rgb, _EnvTop.rgb, sky);
                float box1 = exp(-pow2((r.x + 0.4) * 2.5)) * smoothstep(0.2, 0.8, r.y);
                float box2 = exp(-pow2((r.x - 0.6) * 3.0)) * smoothstep(-0.2, 0.5, r.y) * 0.5;
                return env + (box1 + box2) * float3(1.0, 1.0, 1.05);
            }

            float3 ShadeMetal(float3 n, float3 baseCol)
            {
                float3 V = float3(0, 0, 1);
                float3 L = normalize(float3(-0.4, 0.7, 0.7));
                float3 H = normalize(L + V);
                float3 r = reflect(-V, n);
                float spec = pow(saturate(dot(n, H)), _Shininess);
                return baseCol * (0.22 + EnvMap(r) * 1.1) + spec * 0.5;
            }

            float UIClip(float2 position, float4 clipRect)
            {
                float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
                return inside.x * inside.y;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                float autoAspect = abs(ddy(uv.y)) / max(abs(ddx(uv.x)), 1e-8);
                float aspect = clamp((_Aspect > 0.001) ? _Aspect : autoAspect, 0.25, 8.0);
                float aa = max(abs(ddx(uv.y)) + abs(ddy(uv.y)), 1e-5);

                // Centered coordinates in height units
                float2 p = (uv - 0.5) * float2(aspect, 1.0);
                float tt = saturate(_Power);

                // --- Geometry Layout ---
                float2 oh = float2(aspect * 0.5, 0.5);
                float rO = min(_OuterRadius, min(oh.x, oh.y));
                float dO = sdRoundBox(p, oh, rO);
                float mO = Cov(dO, aa);

                // --- Uniform Physical Metal Shading ---
                float2 gO = gradRoundBox(p, oh, rO);
                float eO = -dO;
                float tiltO = Band(eO, _OuterChamfer, aa) * lerp(0.5, 1.25, saturate(1.0 - eO / _OuterChamfer));
                
                float3 n = normalize(float3(gO * tiltO, 1.0));

                // Continuous brushed metal grain across the entire faceplate surface
                float b = hash21(float2(floor(p.y * _BrushScale), floor(p.x * 12.0))) - 0.5;
                n = normalize(n + float3(0.0, b * _Brushed * 0.3, 0.0));

                float3 baseMetalCol = _ChassisColor.rgb * (0.8 + 0.4 * uv.y);
                float3 metalSurface = ShadeMetal(n, baseMetalCol);

                // --- Outer Border LED Strip (positioned along the outer perimeter frame edge) ---
                float3 ledCol = lerp(_LedOffColor.rgb, _LedOnColor.rgb, tt);
                float breathe = 0.6 + 0.4 * sin(_Time.y * 3.0);
                float ledPower = lerp(breathe * 0.5, 1.2, tt);

                float ledLine = 1.0 - smoothstep(0.0, _LedWidth, abs(dO + 0.012));
                float ledGlow = exp(-abs(dO + 0.012) / 0.025) * 0.35;

                // --- Composite Surface Layering ---
                float3 finalColor = metalSurface;
                finalColor += ledCol * (ledLine + ledGlow) * ledPower;

                float4 col = float4(finalColor * mO, mO);
                col *= IN.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                col *= UIClip(IN.worldPos.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}