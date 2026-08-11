Shader "Spine/SpineWithHighlight" {
    Properties {
        _Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
        [NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
        [Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
        [HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default

        // Highlight, Dissolve, and Flashing Properties
        [Header(Highlight Settings)]
        [HDR] _HighlightColor ("Highlight Color", Color) = (1, 1, 1, 1)
        _HighlightAmount ("Highlight Amount", Range(0, 1)) = 1.0

        [Header(Flashing Settings)]
        _FlashSpeed ("Flash Speed", Float) = 5.0
        _FlashIntensity ("Flash Intensity", Range(0, 1)) = 0.5

        [Header(Local Vertical Dissolve Settings)]
        _MinY ("Mesh Min Local Y", Float) = -1.0
        _MaxY ("Mesh Max Local Y", Float) = 1.0
        _DissolveProgress ("Dissolve Progress", Range(0, 1)) = 0.0
        _DissolveEdgeWidth ("Edge Width / Softness", Range(0.001, 0.5)) = 0.05
        [HDR] _DissolveEdgeColor ("Edge Color", Color) = (2, 0.5, 0, 1)
        [Toggle(_INVERT_DISSOLVE_DIRECTION)] _InvertDirection ("Dissolve Top to Bottom", Float) = 0

        // Outline properties are drawn via custom editor.
        [HideInInspector] _OutlineWidth("Outline Width", Range(0,8)) = 3.0
        [HideInInspector] _OutlineColor("Outline Color", Color) = (1,1,0,1)
        [HideInInspector] _OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
        [HideInInspector] _ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
        [HideInInspector] _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
        [HideInInspector][MaterialToggle(_USE8NEIGHBOURHOOD_ON)] _Use8Neighbourhood("Sample 8 Neighbours", Float) = 1
        [HideInInspector] _OutlineOpaqueAlpha("Opaque Alpha", Range(0,1)) = 1.0
        [HideInInspector] _OutlineMipLevel("Outline Mip Level", Range(0,3)) = 0
    }

    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

        Fog { Mode Off }
        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Lighting Off

        Stencil {
            Ref[_StencilRef]
            Comp[_StencilComp]
            Pass Keep
        }

        Pass {
            Name "Normal"

            CGPROGRAM
            #pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
            #pragma shader_feature _ _INVERT_DISSOLVE_DIRECTION
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "CGIncludes/Spine-Common.cginc"

            sampler2D _MainTex;
            
            // Uniforms
            fixed4 _HighlightColor;
            fixed _HighlightAmount;
            fixed _FlashSpeed;
            fixed _FlashIntensity;
            float _MinY;
            float _MaxY;
            fixed _DissolveProgress;
            fixed _DissolveEdgeWidth;
            fixed4 _DissolveEdgeColor;

            struct VertexInput {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 vertexColor : COLOR;
            };

            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float localYNormalized : TEXCOORD1;
                float4 vertexColor : COLOR;
            };

            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                // Normalize vertex local Y position relative to bounds [_MinY, _MaxY]
                float normalizedY = (v.vertex.y - _MinY) / max(0.0001, (_MaxY - _MinY));
                o.localYNormalized = saturate(normalizedY);

                o.vertexColor = PMAGammaToTargetSpace(v.vertexColor);
                return o;
            }

            float4 frag (VertexOutput i) : SV_Target {
                float4 texColor = tex2D(_MainTex, i.uv);

                #if defined(_STRAIGHT_ALPHA_INPUT)
                texColor.rgb *= texColor.a;
                #endif

                // Base spine color calculation
                float4 finalColor = texColor * i.vertexColor;

                // --- Highlight & Dissolve Logic ---
                if (_HighlightAmount > 0.0) {
                    float verticalY = i.localYNormalized;

                    #if defined(_INVERT_DISSOLVE_DIRECTION)
                    verticalY = 1.0 - verticalY;
                    #endif

                    // Remap progress to clear the entire height including edge width bounds
                    float progress = lerp(-_DissolveEdgeWidth, 1.0 + _DissolveEdgeWidth, 1 - _DissolveProgress);

                    // Compute smooth transitions and edge glow
                    float highlightMask = smoothstep(progress, progress + _DissolveEdgeWidth, verticalY);
                    
                    // Edge band calculation
                    float edgeBand = smoothstep(progress - _DissolveEdgeWidth, progress, verticalY) - highlightMask;

                    // --- Flashing Logic ---
                    // Creates a ping-pong sine wave from 0 to 1 based on time and speed
                    float pingPongTime = 0.5 * sin(_Time.y * _FlashSpeed) + 0.5;
                    
                    // Remaps the ping-pong to [1.0 - Intensity, 1.0] to never go completely black
                    float flashMultiplier = lerp(1.0 - _FlashIntensity, 1.0, pingPongTime);

                    // Apply the flash to the Highlight color itself (supports HDR glowing colors)
                    float3 finalHighlightColor = _HighlightColor.rgb * flashMultiplier;

                    // Blend original RGB into the Flashing Highlight Color
                    // Highlight color must be scaled by PMA Alpha (finalColor.a)
                    float3 highlightedRGB = lerp(finalColor.rgb, finalHighlightColor * finalColor.a, _HighlightAmount);
                    
                    // Add edge glow
                    highlightedRGB += _DissolveEdgeColor.rgb * edgeBand * finalColor.a;

                    // Apply the vertical mask
                    finalColor.rgb = lerp(finalColor.rgb, highlightedRGB, highlightMask);
                }

                return finalColor;
            }
            ENDCG
        }

        Pass {
            Name "Caster"
            Tags { "LightMode"="ShadowCaster" }
            Offset 1, 1
            ZWrite On
            ZTest LEqual

            Fog { Mode Off }
            Cull Off
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            fixed _Cutoff;

            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float4 uvAndAlpha : TEXCOORD1;
            };

            VertexOutput vert (appdata_base v, float4 vertexColor : COLOR) {
                VertexOutput o;
                o.uvAndAlpha = v.texcoord;
                o.uvAndAlpha.a = vertexColor.a;
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }

            float4 frag (VertexOutput i) : SV_Target {
                fixed4 texcol = tex2D(_MainTex, i.uvAndAlpha.xy);
                clip(texcol.a * i.uvAndAlpha.a - _Cutoff);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    CustomEditor "SpineShaderWithOutlineGUI"
}
