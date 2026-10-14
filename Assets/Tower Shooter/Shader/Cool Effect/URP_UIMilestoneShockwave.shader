Shader "Custom/URP_UIMilestoneShockwave"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture (Unused)", 2D) = "white" {}
        _Color ("Shockwave Color", Color) = (0.3, 0.85, 1, 1)
        _SecondaryColor ("Secondary Wave Color", Color) = (1, 1, 1, 1)
        _Progress ("Progress", Range(0, 1)) = 0
        _Thickness ("Ring Thickness", Range(0.005, 0.15)) = 0.035
        _Glow ("Glow Strength", Range(0, 5)) = 1.5
        _Aspect ("Aspect Ratio", Float) = 1
        _MaxRadius ("Maximum Radius", Float) = 0.8 // Lower max radius slightly to allow room for glow
        _Scale ("Inner UV Scale", Range(1, 3)) = 1.3 // Remaps coordinates inside mesh
        _Alpha ("Alpha", Range(0, 1)) = 1
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderPipeline" = "UniversalPipeline"
            "CanUseSpriteAtlas" = "True"
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _SecondaryColor;
            float _Progress;
            float _Thickness;
            float _Glow;
            float _Aspect;
            float _MaxRadius;
            float _Scale;
            float _Alpha;
            float4 _ClipRect;

            v2f vert(appdata v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float ring(float distanceToCenter, float radius, float thickness)
            {
                float d = abs(distanceToCenter - radius);
                return 1.0 - smoothstep(thickness * 0.5, thickness, d);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Scale UV coordinates from center to shrink shockwave relative to RectTransform
                float2 p = (i.uv - 0.5) * _Scale;
                p.x *= max(_Aspect, 0.001);

                float dist = length(p);
                float progress = saturate(_Progress);

                float radius1 = progress * _MaxRadius;
                float radius2 = saturate(progress - 0.22) / 0.78 * _MaxRadius;

                float ring1 = ring(dist, radius1, _Thickness);
                float ring2 = ring(dist, radius2, _Thickness * 0.55)
                            * (1.0 - smoothstep(0.0, 0.22, progress));

                float glow1 = exp(-abs(dist - radius1) / max(_Thickness * 2.5, 0.001)) * _Glow;
                float glow2 = exp(-abs(dist - radius2) / max(_Thickness * 1.8, 0.001)) * _Glow * 0.5;

                // Soft edge mask to smoothly fade out remaining glow before reaching UV boundaries
                float2 edgeDist = min(i.uv, 1.0 - i.uv);
                float edgeFade = smoothstep(0.0, 0.08, min(edgeDist.x, edgeDist.y));

                float fade = 1.0 - smoothstep(0.65, 1.0, progress);
                float alpha = saturate((ring1 + ring2 * 0.75 + glow1 * 0.25 + glow2 * 0.15) * fade) * edgeFade;

                float3 color = lerp(_Color.rgb, _SecondaryColor.rgb, saturate(ring2 * 0.8 + glow2 * 0.25));

                fixed4 result;
                result.rgb = color * i.color.rgb;
                result.a = alpha * _Alpha * _Color.a * i.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(result.a - 0.001);
                #endif

                return result;
            }
            ENDCG
        }
    }
}