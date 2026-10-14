Shader "Custom/URP_UIDigitalGlitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LongPressProgress ("Long Press Progress", Range(0, 1)) = 0
        _GlitchTrigger ("Glitch Trigger", Range(0, 1)) = 0
        _Tint ("Effect Tint", Color) = (0.0, 1.0, 0.5, 1.0)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "DigitalGlitchPass"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Tint;
                float _LongPressProgress;
                float _GlitchTrigger;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            // Procedural fine 2D circle particles moving downward dynamically
            float MovingDigitalCircles(float2 uv)
            {
                // High frequency grid for small, proportionate PC elements
                float2 gridScale = float2(90.0, 50.0);
                float2 grid = uv * gridScale;
                
                float2 cellID = floor(grid);
                float2 cellUV = frac(grid) - 0.5; // Center UV inside each cell (-0.5 to 0.5)

                // Unique downward fall speed per column
                float speed = frac(sin(cellID.x * 12.9898) * 43758.5453) * 6.0 + 3.0;
                
                // Subtracting time makes the circular points stream downwards continuously
                float drop = frac((cellID.y / gridScale.y) - _Time.y * speed * 0.15);

                // Small, crisp 2D circle distance check
                float radius = 0.25;
                float dist = length(cellUV);
                float circle = 1.0 - smoothstep(radius - 0.08, radius + 0.08, dist);

                // Random brightness variation per cell
                float randomFade = frac(sin(cellID.x * 78.233 + cellID.y * 12.989) * 43758.5453);
                
                return circle * (randomFade > 0.3 ? 1.0 : 0.2);
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 centeredUV = uv - 0.5;
                float dist = length(centeredUV) * 1.414;

                // 1. Moving 2D circle particles active while holding
                float particles = MovingDigitalCircles(uv) * _LongPressProgress * 0.9;
                float centerClearMask = smoothstep(0.12 * _LongPressProgress, 0.3 * _LongPressProgress + 0.1, dist);

                // 2. Release shockwave ring
                float waveRadius = _GlitchTrigger * 1.2;
                float waveRing = (smoothstep(waveRadius - 0.06, waveRadius, dist) - smoothstep(waveRadius, waveRadius + 0.06, dist)) * _GlitchTrigger;

                // 3. Glitch lines on release
                float slice = step(0.96, sin(uv.y * 90.0 + _GlitchTrigger * 35.0)) * _GlitchTrigger;
                
                // Chromatic aberration shift on release
                float aberration = _GlitchTrigger * 0.02;
                float rChan = _MainTex.Sample(sampler_MainTex, uv + float2(aberration, 0)).r;
                float bChan = _MainTex.Sample(sampler_MainTex, uv - float2(aberration, 0)).b;

                // Combine effects
                float effectIntensity = (particles * (1.0 - centerClearMask)) + (waveRing * 3.0) + (slice * 1.5);
                
                float3 finalRgb = _Tint.rgb * effectIntensity;
                finalRgb.r += rChan * _GlitchTrigger * 0.3;
                finalRgb.b += bChan * _GlitchTrigger * 0.3;

                // Master alpha calculation to guarantee total transparency when idle
                float masterMultiplier = saturate(_LongPressProgress + _GlitchTrigger);
                float finalAlpha = saturate(effectIntensity + (waveRing * 0.6) + (slice * 0.4)) * masterMultiplier;

                return float4(finalRgb, finalAlpha);
            }
            ENDHLSL
        }
    }
}