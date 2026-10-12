Shader "UI/AugmentQualityShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Quality ("Quality (0:Common, 1:Uncommon, 2:Rare, 3:Epic, 4:Legendary, 5:Mythic)", Int) = 0
        
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15
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
            Comp[_StencilOp]
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
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            int _Quality;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.texcoord = v.texcoord;
                o.color = v.color;
                return o;
            }

            // Simple pseudo-random hash generator for sparkles
            float random(float2 st) 
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float4 frag(v2f i) : SV_Target
            {
                // Sample base texture
                float4 texColor = _MainTex.Sample(sampler_MainTex, i.texcoord);
                float alpha = texColor.a * i.color.a;
                
                // Discard black background pixels
                float luminance = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
                if (luminance < 0.1 || alpha <= 0.05) 
                {
                    return float4(0, 0, 0, 0); 
                }

                float time = _Time.y;
                float2 uv = i.texcoord;
                float3 finalColor = float3(1, 1, 1);

                // --- 0: COMMON (Industrial Scanline Grid) ---
                if (_Quality == 0)
                {
                    float3 baseCol = float3(0.48, 0.54, 0.60);
                    float scanlines = sin(uv.y * 80.0) * 0.08 + 0.92;
                    finalColor = baseCol * scanlines;
                }
                // --- 1: UNCOMMON (Bio-Organic Vertical Pulse) ---
                else if (_Quality == 1)
                {
                    float3 baseCol = float3(0.09, 0.80, 0.34);
                    float pulse = sin(uv.y * 10.0 - time * 4.0) * 0.25 + 0.75;
                    finalColor = baseCol * pulse;
                }
                // --- 2: RARE (Crystalline Diamond Facets) ---
                else if (_Quality == 2)
                {
                    float3 baseCol = float3(0.0, 0.635, 1.0);
                    float facet = abs(sin((uv.x + uv.y) * 12.0) * cos((uv.x - uv.y) * 12.0));
                    finalColor = lerp(baseCol * 0.7, baseCol * 1.5, facet);
                }
                // --- 3: EPIC (Swirling Energy Vortex) ---
                else if (_Quality == 3)
                {
                    float3 baseCol = float3(0.59, 0.22, 1.0);
                    float2 center = uv - 0.5;
                    float dist = length(center);
                    float angle = atan2(center.y, center.x);
                    float vortex = sin(angle * 5.0 + time * 3.0 - dist * 15.0) * 0.5 + 0.5;
                    finalColor = lerp(baseCol * 0.6, baseCol * 1.4, vortex);
                }
                // --- 4: LEGENDARY (Sharp Metallic Anisotropic Specular Gleam) ---
                else if (_Quality == 4)
                {
                    float3 baseCol = float3(1.0, 0.635, 0.0);
                    float specularLine = pow(frac(uv.x * 1.5 - time * 0.6), 16.0) * 4.0;
                    finalColor = baseCol + float3(0.5, 0.4, 0.1) * specularLine;
                }
                // --- 5: MYTHIC (Cosmic Starfield & Prismatic Aurora Sparkle) ---
                else if (_Quality == 5)
                {
                    float3 baseCol = float3(1.0, 0.08, 0.42);
                    // Shifting aurora background wave
                    float aurora = sin(uv.x * 6.0 + time * 2.0) * cos(uv.y * 6.0 - time * 1.5) * 0.5 + 0.5;
                    float3 auroraMix = lerp(baseCol, float3(0.8, 0.2, 1.0), aurora);
                    
                    // Dynamic sparkling star field nodes
                    float sparkleGrid = random(floor(uv * 20.0 + time));
                    float star = step(0.94, sparkleGrid);
                    
                    finalColor = lerp(auroraMix, float3(1.0, 1.0, 1.0), star);
                }

                return float4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}