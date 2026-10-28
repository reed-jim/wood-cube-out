Shader "UI/URP/EnergyBorderAAALevel"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        [HDR] _Color ("Border Base Color", Color) = (0.15, 0.65, 1.0, 1.0)
        [HDR] _RimColor ("Arcane Edge Glow", Color) = (0.0, 0.95, 0.85, 1.0)
        [HDR] _HotColor ("Hot Core Color", Color) = (1.0, 0.97, 0.90, 1.0)

        [Header(Energy FX Control)]
        _Power ("Effect Power (0 - 100)", Range(0.0, 100.0)) = 75.0
        _Progress ("Energy Comet Position (0 - 1)", Range(0.0, 1.0)) = 0.0
        _Speed ("Flow Speed", Float) = 4.0
        _BorderWidth ("Border Width", Range(0.00, 0.25)) = 0.08
        _Distortion ("Hextech Distortion", Range(0.0, 1.0)) = 0.4
        _Exposure ("Exposure", Range(0.2, 4.0)) = 1.4
        _Aspect ("Image Aspect (Width / Height)", Float) = 1.0
        _CornerRadius ("Corner Radius", Range(0.0, 0.5)) = 0.15

        // Stencil / UI Canvas properties
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
            "RenderPipeline"="UniversalPipeline"
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
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_EnergyBorderAAALevel"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma shader_feature_local _USEUIALPHACLIP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _RimColor;
                float4 _HotColor;
                float _Power;
                float _Progress;
                float _Speed;
                float _BorderWidth;
                float _Distortion;
                float _Exposure;
                float _Aspect;
                float _CornerRadius;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            // ------------------------------------------------------------------
            // Hash / Noise Functions (Stable & Sin-Free)
            // ------------------------------------------------------------------
            float hash21(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float2 hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            float vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                float2x2 m = float2x2(0.8, -0.6, 0.6, 0.8);
                [unroll]
                for (int i = 0; i < 4; i++)
                {
                    v += a * vnoise(p);
                    p = mul(m, p) * 2.02 + 17.1;
                    a *= 0.5;
                }
                return v / 0.9375;
            }

            float gaussian(float d, float w)
            {
                float q = d / w;
                return exp(-q * q);
            }

            float lorentz(float d, float w)
            {
                float q = d / w;
                return 1.0 / (1.0 + q * q);
            }

            // ------------------------------------------------------------------
            // Rounded Box SDF
            // ------------------------------------------------------------------
            float roundedBoxSdf(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            // Parameterize perimeter position (0 to 1 around the rounded box loop)
            float getPerimeterCoord(float2 p, float2 b, float r)
            {
                return frac(p.x * 0.5 + p.y * 0.5 + 0.5);
            }

            // ------------------------------------------------------------------
            // Fragment Shader
            // ------------------------------------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv - 0.5;
                float2 pp = float2(uv.x * _Aspect, uv.y);

                float aaX = max(fwidth(pp.x), 1e-4);

                float p = saturate(_Power * 0.01);
                float pe = pow(p, 1.6);
                float t = _Time.y * _Speed;
                float pulseBeat = 0.88 + 0.12 * sin(_Time.y * lerp(2.0, 10.0, p));

                // Box dimensions expanded to fill the entire UI Image bounds (0.5 half-extent)
                float2 boxSize = float2(0.5 * _Aspect, 0.5);
                float sdBox = roundedBoxSdf(pp, boxSize, _CornerRadius);

                // Smoothly clip pixels outside the full rounded box bounds
                float cornerMask = smoothstep(0.002, -0.002, sdBox);

                // Border band distance field
                float targetDist = _BorderWidth * 0.5;
                float d = abs(sdBox - (-targetDist)) - targetDist; 
                
                // Track perimeter coordinate for fluid flow
                float along = getPerimeterCoord(pp, boxSize, _CornerRadius) + t * 0.1;

                // Organic noise warp along the border
                float amp = _Distortion * 0.05;
                float n = fbm(float2(along * 8.0 - t * 0.4, t * 0.1));
                float warp = (n - 0.5) * amp;
                d = abs(d - warp);

                // Dropouts & energetic surges
                float seg = vnoise(float2(along * 12.0 - t, _Time.y * 2.0));
                float dropout = lerp(smoothstep(0.35, 0.65, seg), 1.0, smoothstep(0.05, 0.45, p));
                float band = pow(0.5 + 0.5 * sin(along * 30.0 - t * 2.5), 6.0);
                float bandMod = 1.0 + band * lerp(0.3, 1.2, p);

                float w = 0.03 * lerp(0.8, 1.4, p);
                float flick = 0.88 + 0.12 * sin(_Time.y * 37.0 + along * 40.0);
                
                // Energy layers (Core, Glow, Halo)
                float core = gaussian(d, w * 0.5) * flick * dropout * bandMod;
                float glow = pow(lorentz(d, w * 1.3), 1.5) * (0.6 + 0.4 * dropout);
                float halo = exp(-d / (w * 3.0));

                // Energy Comet travelling along the border loop
                float dd = frac(_Progress + t * 0.05) - frac(along);
                float cometProfile = dd >= 0.0 ? exp(-dd * 8.0) : exp(dd * 50.0);
                float comet = cometProfile * (gaussian(d, w * 1.5) * 0.9 + halo * 0.5) * 1.8;

                // Crackling lightning bolts on the border
                float bolts = 0.0;
                float boltAmt = smoothstep(0.25, 0.9, p);
                float boltW = max(w * 0.1, aaX * 1.2);
                [unroll]
                for (int i = 0; i < 2; i++)
                {
                    float seed = i * 13.7;
                    float tick = floor(_Time.y * (10.0 + i * 4.0));
                    float g = hash21(float2(tick, seed));
                    float vis = step(lerp(1.1, 0.3, boltAmt), g);
                    float j1 = (vnoise(float2(along * 20.0 + seed + tick * 3.0, tick)) - 0.5) * 2.0;
                    float bx = warp + j1 * w * 1.5;
                    float bd = abs(d - bx);
                    bolts += (gaussian(bd, boltW) + 0.15 * lorentz(bd, boltW * 4.0)) * vis;
                }
                bolts *= boltAmt;

                // Particle sparks floating around the border
                float sparks = 0.0;
                float dens = lerp(0.15, 0.9, p);
                float prox = exp(-d / (w * 2.5));
                [unroll]
                for (int L = 0; L < 2; L++)
                {
                    float sc = 12.0 + L * 8.0;
                    float2 q = pp * sc - float2(0.0, t * (0.4 + 0.3 * L));
                    float2 cell = floor(q);
                    float2 f = frac(q) - 0.5;
                    float2 r = hash22(cell + L * 31.7);
                    float2 c = (r - 0.5) * 0.5;
                    float dist = length(f - c);
                    float rad = lerp(0.08, 0.15, r.x);
                    float sp = exp(-(dist * dist) / (rad * rad));
                    float alive = step(hash21(cell + L * 7.1 + 5.3), dens);
                    float tw = 0.5 + 0.5 * sin(_Time.y * 5.0 + r.y * 6.28);
                    sparks += sp * alive * tw;
                }
                sparks *= prox * 1.2;

                // NEW: Center fade factor. 
                // sdBox is negative inside. We create a multiplier that drops significantly 
                // as we move away from the border towards the center.
                float centerFade = saturate((sdBox + _BorderWidth * 1.5) / (-_BorderWidth * 1.5));
                // Keep a very faint ambient energy in the core (e.g., 0.05), or set to 0.0 for pitch black center
                centerFade = lerp(0.02, 1.0, centerFade); 

                // Compose HDR color matching League/Arcane quality
                float3 tintMid = lerp(_Color.rgb, _RimColor.rgb, p);
                float3 hot = _HotColor.rgb;
                float brightness = lerp(0.8, 2.2, pe);

                float3 hdr = (core * hot * 2.4
                            + glow * _RimColor.rgb * 1.2
                            + halo * tintMid * 0.6
                            + bolts * hot * 3.0
                            + sparks * hot * 2.0
                            + comet * _RimColor.rgb * 2.0) * cornerMask * centerFade;

                hdr *= brightness * pulseBeat * _Exposure;

                // Filmic tonemapping + highlight desaturation
                float3 ldr = 1.0 - exp(-hdr);
                float lum = dot(hdr, float3(0.2126, 0.7152, 0.0722));
                ldr = lerp(ldr, (1.0 - exp(-lum)).xxx, saturate(lum * 0.15));

                // Dither to remove gradient banding
                ldr += (hash21(input.positionCS.xy) - 0.5) / 255.0;
                ldr = max(ldr, 0.0);

                float a = saturate(max(ldr.r, max(ldr.g, ldr.b))) * cornerMask;

                half4 col = half4(ldr, a);
                col.rgb *= input.color.rgb * input.color.a;
                col.a *= input.color.a;

                #ifdef _USEUIALPHACLIP_ON
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}