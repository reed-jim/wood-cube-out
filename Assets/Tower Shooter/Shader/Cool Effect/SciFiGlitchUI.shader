Shader "URP/UI/TiktokGlitch"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Progress ("Transition Progress (0=Glitch, 1=Normal)", Range(0, 1)) = 0.0
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.5
        _Speed ("Glitch Speed", Float) = 10.0
        
        // UI Mask / Canvas Required Properties
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
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
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_GlitchPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.HLSL"

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            
            float _Progress;
            float _GlitchIntensity;
            float _Speed;

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.worldPosition = v.vertex;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y * _Speed;

                // Effective glitch fades out completely as _Progress goes from 0 -> 1
                float glitchWeight = _GlitchIntensity * (1.0 - _Progress);

                // --- 1. GLITCH EFFECT (Slices & RGB Split) ---
                float sliceID = floor(uv.y * 20.0);
                float sliceNoise = random(float2(sliceID, floor(time)));
                
                if (sliceNoise < glitchWeight * 0.4)
                {
                    float shift = (random(float2(time, sliceID)) - 0.5) * glitchWeight * 0.5;
                    uv.x += shift;
                }

                // RGB Split
                float rgbSplit = glitchWeight * 0.05 * random(float2(time, 1.0));

                float r = _MainTex.Sample(sampler_MainTex, uv + float2(rgbSplit, 0.0)).r;
                float g = _MainTex.Sample(sampler_MainTex, uv).g;
                float b = _MainTex.Sample(sampler_MainTex, uv - float2(rgbSplit, 0.0)).b;
                float a = _MainTex.Sample(sampler_MainTex, uv).a;

                float4 glitchColor = float4(r, g, b, a);

                // Subtle Scanline during glitch
                float scanline = sin(uv.y * 200.0 + time * 10.0) * 0.08 * glitchWeight;
                glitchColor.rgb -= scanline;

                // --- 2. CLEAN NORMAL SAMPLE ---
                float4 cleanColor = _MainTex.Sample(sampler_MainTex, i.uv);

                // Blend from glitch to normal state
                float4 finalColor = lerp(glitchColor, cleanColor, _Progress);

                // --- 3. "WOW, IT'S FIXED!" FINISH EFFECT ---
                // Triggers when _Progress passes 0.75 towards 1.0
                float fixFactor = saturate((_Progress - 0.75) / 0.25);

                if (fixFactor > 0.0)
                {
                    // A. Global Restore Light Burst (Flash)
                    float burst = sin(fixFactor * 3.14159) * 0.4;
                    
                    // B. Clean Cyan / Blue Tech Glow Tint
                    float3 techGlow = float3(0.2, 0.8, 1.0) * burst;

                    // C. Scanning Restore Line moving top to bottom
                    float linePos = 1.0 - fixFactor; // Sweeps down from 1 to 0
                    float scanLine = smoothstep(0.04, 0.0, abs(i.uv.y - linePos));
                    float3 scanLineColor = float3(0.5, 1.0, 1.0) * scanLine * 0.8;

                    finalColor.rgb += techGlow + scanLineColor;
                }

                // Apply Canvas UI Vertex Color Tinting
                finalColor *= i.color;

                return finalColor;
            }
            ENDHLSL
        }
    }
}