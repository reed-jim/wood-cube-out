Shader "UI/Custom/AAAContinuousBeam"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        [HDR] _Color ("Beam Base Color", Color) = (0.15, 0.65, 1.0, 1.0)
        [HDR] _RimColor ("Arcane Edge Glow", Color) = (0.0, 0.95, 0.85, 1.0)
        [HDR] _HotColor ("Hot Core Color", Color) = (1.0, 0.97, 0.90, 1.0)

        [Header(Spell FX Control)]
        _Power ("Effect Power (0 - 100)", Range(0.0, 100.0)) = 50.0
        _Progress ("Energy Comet Position (0 - 1, Top to Bottom)", Range(0.0, 1.0)) = 0.0
        _Speed ("Flow Speed", Float) = 4.5
        _BeamWidth ("Beam Outer Glow Width", Range(0.01, 0.25)) = 0.08
        _CoreWidth ("Top & Bottom Spread Horizontal Width", Range(0.01, 1.0)) = 0.35
        _Distortion ("Hextech Distortion", Range(0.0, 1.0)) = 0.5
        _Exposure ("Exposure", Range(0.2, 4.0)) = 1.4
        _Aspect ("Image Aspect (Width / Height)", Float) = 1.0

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
            Name "UI_AAAContinuousBeam"
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
                float _BeamWidth;
                float _CoreWidth;
                float _Distortion;
                float _Exposure;
                float _Aspect;
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
            // Hash / noise functions
            // ------------------------------------------------------------------
            float hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

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
            // Fragment
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

                // ============================ WIDE HORIZONTAL SPREAD GRADIENT ============================
                float hGradient = sin(input.uv.x * 3.14159265);
                hGradient = pow(hGradient, 0.55);

                // ============================ TOP -> BOTTOM ENERGY FLOW ============================
                float along = saturate(0.5 - pp.y); // 0 at top, 1 at bottom
                float env = 1.0; 

                float w = _BeamWidth * lerp(0.6, 1.4, p);
                float x = pp.x;

                float amp = _Distortion * lerp(0.008, 0.06, p) * env;
                float n = fbm(float2(along * 5.0 - t * 0.7, t * 0.2));
                float warp = ((n - 0.5) * 2.2 + sin(along * 20.0 - t * 3.0) * 0.35) * amp;
                float d = abs(x - warp);

                float seg = vnoise(float2(along * 10.0 - t * 1.5, _Time.y * 2.0));
                float dropout = lerp(smoothstep(0.35, 0.65, seg), 1.0, smoothstep(0.05, 0.45, p));

                float downwardWave = pow(0.5 + 0.5 * sin((along * 22.0) - (t * 5.0)), 5.0);
                float band = pow(0.5 + 0.5 * sin(along * 26.0 - t * 4.0), 6.0);
                float bandMod = 1.0 + (band + downwardWave * 1.8) * lerp(0.3, 1.5, p);

                float flick = 0.88 + 0.12 * sin(_Time.y * 37.0 + along * 40.0);
                float core = gaussian(d, w * 0.5) * flick * dropout * bandMod;
                float glow = pow(lorentz(d, w * 1.3), 1.5) * (0.6 + 0.4 * dropout);
                float halo = exp(-d / (w * (2.8 + 3.5 * p)));

                float helixOn = smoothstep(0.2, 0.6, p);
                float hr = w * 1.5 * helixOn * env;
                float ph = along * 30.0 - t * 4.0;
                float s1 = warp + sin(ph) * hr;
                float s2 = warp - sin(ph) * hr;
                float sw = max(w * 0.28, aaX * 1.5);
                float strands = (gaussian(x - s1, sw) * (0.65 + 0.35 * cos(ph))
                               + gaussian(x - s2, sw) * (0.65 - 0.35 * cos(ph))) * helixOn * dropout;

                float ca = smoothstep(0.4, 1.0, p) * w * 0.35;
                float fR = max(gaussian(abs(x - warp + ca), w * 0.9) - gaussian(d, w * 0.9), 0.0);
                float fB = max(gaussian(abs(x - warp - ca), w * 0.9) - gaussian(d, w * 0.9), 0.0);
                float3 fringe = float3(fR, 0.0, fB) * smoothstep(0.4, 1.0, p);

                // Manual energy comet driven by _Progress slider
                float dd = frac(_Progress) - along;
                float cometProfile = dd >= 0.0 ? exp(-dd * 7.0) : exp(dd * 70.0);
                float comet = cometProfile * (gaussian(d, w * 1.8) * 0.9 + halo * 0.5) * lerp(0.6, 2.0, p);

                float bolts = 0.0;
                float boltAmt = smoothstep(0.25, 0.9, p);
                float boltW = max(w * 0.09, aaX * 1.2);
                [unroll]
                for (int i = 0; i < 2; i++)
                {
                    float seed = i * 13.7;
                    float tick = floor(_Time.y * (14.0 + i * 5.0));
                    float g = hash21(float2(tick, seed));
                    float vis = step(lerp(1.1, 0.3, boltAmt), g);
                    float j1 = (vnoise(float2(along * 26.0 + seed + tick * 3.7, tick)) - 0.5) * 2.0;
                    float j2 = (vnoise(float2(along * 70.0 + seed, tick * 1.3)) - 0.5) * 2.0;
                    float bx = warp + (j1 * w * 2.6 + j2 * w * 0.7) * env;
                    float bd = abs(x - bx);
                    bolts += (gaussian(bd, boltW) + 0.15 * lorentz(bd, boltW * 4.0)) * vis;
                }
                bolts *= boltAmt * env;

                float sparks = 0.0;
                float dens = lerp(0.15, 0.9, p);
                float prox = exp(-d / (w * (2.5 + 3.0 * p)));
                [unroll]
                for (int L = 0; L < 3; L++)
                {
                    float sc = 14.0 + L * 9.0;
                    float2 q = float2(x, -pp.y) * sc - float2(0.0, t * (1.2 + 0.5 * L));
                    float2 cell = floor(q);
                    float2 f = frac(q) - 0.5;
                    float2 r = hash22(cell + L * 31.7);
                    float2 c = (r - 0.5) * 0.6;
                    c.x += sin(_Time.y * 2.0 + r.y * 6.2831) * 0.15;
                    float dist = length(f - c);
                    float rad = lerp(0.08, 0.16, r.x);
                    float sp = exp(-(dist * dist) / (rad * rad));
                    float alive = step(hash21(cell + L * 7.1 + 5.3), dens);
                    float tw = 0.5 + 0.5 * sin(_Time.y * (4.0 + r.x * 6.0) + r.y * 6.2831);
                    sparks += sp * alive * tw * (1.0 - L * 0.2);
                }
                sparks *= prox * lerp(0.6, 1.5, p);

                // ============================ TOP & BOTTOM HORIZONTAL SPREAD (STRONGER AT CENTER) ============================
                float distFromTop    = abs(pp.y - 0.5);
                float distFromBottom = abs(pp.y + 0.5);
                
                float topPulse = 0.85 + 0.15 * sin(_Time.y * 12.0);
                float botPulse = 0.85 + 0.15 * cos(_Time.y * 10.0);

                // Thin vertical height (tight to the surface), but horizontally spread and concentrated heavily at the center (x = 0)
                float verticalFalloffTop = exp(-distFromTop * 45.0);
                float verticalFalloffBot = exp(-distFromBottom * 45.0);
                
                float horizontalSpread = gaussian(x, _CoreWidth); // Strong at center (x=0), spreading outwards

                float topSpread = verticalFalloffTop * horizontalSpread * topPulse;
                float botSpread = verticalFalloffBot * horizontalSpread * botPulse;

                float3 coresHdr = ((topSpread * _HotColor.rgb * 5.0 + topSpread * _RimColor.rgb * 2.5)
                                 + (botSpread * _HotColor.rgb * 5.0 + botSpread * _RimColor.rgb * 2.5)) * (1.4 + p * 0.8) * hGradient;

                // ============================ COMPOSE HDR BEAM ============================
                float3 tintMid = lerp(_Color.rgb, _RimColor.rgb, p);
                float3 hot = _HotColor.rgb;
                float brightness = lerp(0.6, 2.4, pe);

                float3 beamHdr = core * hot * 2.4
                               + glow * _RimColor.rgb * 1.1
                               + halo * tintMid * 0.55
                               + strands * lerp(_RimColor.rgb, hot, 0.5) * 1.6
                               + bolts * lerp(hot, _RimColor.rgb, 0.3) * 3.0
                               + sparks * lerp(hot, _RimColor.rgb, 0.5) * 2.0
                               + comet * lerp(hot, _RimColor.rgb, 0.25) * 1.8
                               + fringe * _RimColor.rgb * 0.8;
                beamHdr *= brightness * pulseBeat;

                float edgeFadeY = smoothstep(0.0, 0.05, 0.5 - abs(pp.y));
                float3 hdr = beamHdr * _Exposure * edgeFadeY * hGradient + coresHdr * _Exposure;

                // Filmic tonemap with highlight desaturation
                float3 ldr = 1.0 - exp(-hdr);
                float lum = dot(hdr, float3(0.2126, 0.7152, 0.0722));
                ldr = lerp(ldr, (1.0 - exp(-lum)).xxx, saturate(lum * 0.15));

                // Dither to prevent gradient banding
                ldr += (hash21(input.positionCS.xy) - 0.5) / 255.0;
                ldr = max(ldr, 0.0);

                float a = saturate(max(ldr.r, max(ldr.g, ldr.b))) * hGradient;

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