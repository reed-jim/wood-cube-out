Shader "Custom/URP_UI_EngineCycle"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Outer Glow Color", Color) = (0.1, 0.5, 1.0, 1.0)
        _CoreColor ("Core Pulse Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Progress ("Cycle Progress (0-1)", Range(0.0, 1.0)) = 0.0
        _Frequency ("Ring Frequency", Float) = 15.0
        _SpeedMultiplier ("Auto Speed (Optional)", Float) = 2.0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        Pass
        {
            Name "UIEngineCycle"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _CoreColor;
                float _Progress;
                float _Frequency;
                float _SpeedMultiplier;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Center UV coordinates to range [-0.5, 0.5]
                float2 uv = input.uv - 0.5;

                // Define cylinder width and bounds
                float cylinderWidth = 0.3;
                float distFromCenter = abs(uv.x);
                
                // Sharp cut-off for the cylinder body with smooth anti-aliased edges
                float cylinderMask = smoothstep(cylinderWidth, cylinderWidth - 0.08, distFromCenter);

                // Map progress (0 to 1) into a full 360-degree phase cycle (0 to 2*PI)
                // You can also mix in _Time.y if you want it to animate automatically while playing.
                float cyclePhase = (_Progress * 6.2831853) + (_Time.y * _SpeedMultiplier);

                // Create repeating wave rings moving up/down the cylinder
                float wave = sin((uv.y * _Frequency) - cyclePhase);
                
                // Core intensity based on the wave and distance from center
                float coreGlow = smoothstep(0.15, 0.0, distFromCenter);
                float pattern = 0.5 + 0.5 * wave; // Normalize wave to [0, 1]

                // Combine shape, rings, and core glow
                float intensity = pattern * cylinderMask;
                float4 finalColor = lerp(_Color, _CoreColor, coreGlow * pattern);

                // Fade top and bottom caps slightly for a contained engine look
                float capFade = smoothstep(0.5, 0.3, abs(uv.y));
                intensity *= capFade;

                float alpha = intensity * input.color.a;
                return float4(finalColor.rgb * (1.0 + coreGlow), alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}