Shader "Custom/URP_UISpotlight"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _DimColor ("Dim Color (RGB) & Intensity (A)", Color) = (0, 0, 0, 0.75)
        
        _TargetRect ("Target Rect (Screen UV)", Vector) = (0.3, 0.3, 0.7, 0.7)
        _CornerRadius ("Corner Radius", Range(0, 0.5)) = 0.05
        _EdgeSoftness ("Edge Softness", Range(0.001, 0.1)) = 0.01
        _Progress ("Transition Progress", Range(0, 1)) = 0.0
        _FadeAlpha ("Fade Alpha", Range(0, 1)) = 1.0
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "UISpotlightPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float2 screenUV : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _DimColor;
                float4 _TargetRect;
                float _CornerRadius;
                float _EdgeSoftness;
                float _Progress;
                float _FadeAlpha;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                float4 screenPos = ComputeScreenPos(o.pos);
                o.screenUV = screenPos.xy / screenPos.w;
                
                o.color = v.color;
                return o;
            }

            float roundedBoxSDF(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Interpolate rect from full screen (0,0,1,1) down to the target rect
                float4 currentRect = lerp(float4(0, 0, 1, 1), _TargetRect, _Progress);
                
                // Start with 0 radius at full-screen (progress 0) to prevent corner distortion
                float currentRadius = lerp(0.0, _CornerRadius, _Progress);

                // Account for screen aspect ratio to prevent corner distortion
                float aspect = _ScreenParams.x / _ScreenParams.y;

                float2 center = (currentRect.xy + currentRect.zw) * 0.5;
                float2 size = (currentRect.zw - currentRect.xy) * 0.5;

                float2 p = i.screenUV - center;
                p.x *= aspect;
                size.x *= aspect;

                float dist = roundedBoxSDF(p, size, currentRadius);
                float mask = smoothstep(-_EdgeSoftness, _EdgeSoftness, dist);

                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;
                
                float4 finalColor = lerp(float4(0, 0, 0, 0), _DimColor, mask);
                finalColor.a *= _FadeAlpha;

                return finalColor * texColor.a;
            }
            ENDHLSL
        }
    }
}