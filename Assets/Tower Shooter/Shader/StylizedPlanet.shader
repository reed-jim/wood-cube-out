Shader "UI/PlanetSubtleStyle"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _UITint ("UI Accent Color", Color) = (0.0, 0.89, 1.0, 1.0)       // Cyan #00E5FF
        _DarkTint ("Shadow UI Color", Color) = (0.04, 0.06, 0.11, 1.0)    // Dark Slate #0B101D
        
        _StylizeAmount ("Style Intensity", Range(0.0, 1.0)) = 0.25        // Keeps image mostly natural
        _Contrast ("Contrast Boost", Range(0.5, 2.0)) = 1.15
        _Saturation ("Saturation", Range(0.0, 2.0)) = 1.1
        
        _RimColor ("Rim Outline Color", Color) = (0.0, 0.89, 1.0, 1.0)
        _RimPower ("Rim Softness", Range(0.5, 8.0)) = 3.0
        _RimIntensity ("Rim Strength", Range(0.0, 2.0)) = 0.5
        
        _AlphaCutoff ("Space Alpha Cutoff", Range(0.0, 0.2)) = 0.05
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "CanShaderCanvas"="True" 
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _UITint;
            fixed4 _DarkTint;
            float _StylizeAmount;
            float _Contrast;
            float _Saturation;
            
            fixed4 _RimColor;
            float _RimPower;
            float _RimIntensity;
            float _AlphaCutoff;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            float3 AdjustContrast(float3 color, float contrast)
            {
                return saturate(((color - 0.5) * contrast) + 0.5);
            }

            float3 AdjustSaturation(float3 color, float sat)
            {
                float lum = dot(color, float3(0.299, 0.587, 0.114));
                return saturate(lerp(float3(lum, lum, lum), color, sat));
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. Sample original image
                fixed4 originalCol = tex2D(_MainTex, IN.texcoord) * IN.color;
                float lum = dot(originalCol.rgb, float3(0.299, 0.587, 0.114));

                // 2. Adjust contrast and saturation (keeps details sharp)
                float3 enhancedCol = AdjustContrast(originalCol.rgb, _Contrast);
                enhancedCol = AdjustSaturation(enhancedCol, _Saturation);

                // 3. Subtle color grading to fit UI palette
                float3 uiShadowGrad = lerp(_DarkTint.rgb, _UITint.rgb, lum);
                float3 subtleStylized = lerp(enhancedCol, enhancedCol * uiShadowGrad * 1.5, _StylizeAmount);

                // 4. Circular Rim Glow Effect (Vector-like border)
                float2 uvCentered = IN.texcoord - float2(0.5, 0.5);
                float dist = length(uvCentered) * 2.0; // Normalized distance from center (0 to 1)
                float rim = pow(saturate(dist), _RimPower) * _RimIntensity;
                float3 finalColor = subtleStylized + (_RimColor.rgb * rim);

                // 5. Alpha Cutoff for Space Background
                float alpha = (lum < _AlphaCutoff) ? 0.0 : originalCol.a;

                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
    }
}
