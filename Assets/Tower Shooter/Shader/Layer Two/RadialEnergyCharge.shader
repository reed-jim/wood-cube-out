Shader "UI/Custom/RadialEnergyChargeAAA_v3"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (unused)", 2D) = "white" {}
        _Color ("Background Track Color", Color) = (0.05, 0.08, 0.15, 1.0)
        _RimColor ("Active Charge / Plasma Color", Color) = (0.0, 0.85, 1.0, 1.0)

        [Header(Progress Control)]
        _Power ("Progress (0 - 100)", Range(0.0, 100.0)) = 25.0
        _RotationAngle ("Base Angle Offset (radians)", Float) = 0.0
        _HighSpeedTransition ("High-Speed / Overcharge Mode", Range(0.0, 1.0)) = 0.0

        [Header(Shape)]
        _Layers ("Segment Count", Range(3, 30)) = 16
        _SegmentGap ("Segment Gap Width", Range(0.0, 0.06)) = 0.014
        _Bevel ("Segment Bevel", Range(0.0, 1.0)) = 0.6
        _EventHorizon ("Ring Inner Radius", Range(0.05, 0.4)) = 0.22
        _RingOuter ("Ring Outer Radius", Range(0.3, 0.75)) = 0.60

        [Header(Look)]
        _HeatShimmer ("Plasma Shimmer & Distortion", Range(0.0, 2.0)) = 1.0
        _Sparks ("Embers / Sparks", Range(0.0, 1.0)) = 1.0
        _Bloom ("Fake Bloom Strength", Range(0.0, 2.0)) = 1.0
        _Exposure ("Exposure", Range(0.25, 3.0)) = 1.0
        _Tonemap ("Filmic Tonemap Amount", Range(0.0, 1.0)) = 0.6

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
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha   // premultiplied alpha: low alpha + high rgb = additive glow
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_RadialEnergyChargeAAA"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
                float4 worldPos   : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _ClipRect;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _RimColor;
                float _Power;
                float _RotationAngle;
                float _HighSpeedTransition;
                float _Layers;
                float _SegmentGap;
                float _Bevel;
                float _EventHorizon;
                float _RingOuter;
                float _HeatShimmer;
                float _Sparks;
                float _Bloom;
                float _Exposure;
                float _Tonemap;
            CBUFFER_END

            #define PI 3.14159265359
            #define TWO_PI 6.28318530718

            Varyings vert(Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPos = input.positionOS;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                o.color = input.color;
                return o;
            }

            // ------------------------------------------------------------
            // Helpers (sin-free hashes: stable on mobile / low precision GPUs)
            // ------------------------------------------------------------
            float hash11(float x)
            {
                x = frac(x * 0.1031);
                x *= x + 33.33;
                x *= x + x;
                return frac(x);
            }

            float hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float max3(float3 v) { return max(v.x, max(v.y, v.z)); }

            float2 rot2(float2 p, float a)
            {
                float s, c;
                sincos(a, s, c);
                return float2(c * p.x - s * p.y, s * p.x + c * p.y);
            }

            float vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * f * (f * (f * 6.0 - 15.0) + 10.0); // quintic: no grid creasing
                float a = hash12(i);
                float b = hash12(i + float2(1, 0));
                float c = hash12(i + float2(0, 1));
                float d = hash12(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm4(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                [unroll]
                for (int i = 0; i < 4; i++)
                {
                    v += a * vnoise(p);
                    p = mul(float2x2(1.6, 1.2, -1.2, 1.6), p) + 17.0;
                    a *= 0.5;
                }
                return v / 0.9375;
            }

            float3 ACESFilm(float3 x)
            {
                return saturate((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14));
            }

            // Bright head at a point ON the ring. Everything is built from the 2D offset to
            // that point and masked radially to the tube width, so it can never shoot out of
            // the ring as a ray (the old tip glow only limited the angle, not the radius).
            //   angTip : polar angle of the head in the shader's p-space
            float TipFlare(float2 p, float angTip, float rc, float hw)
            {
                float s, c;
                sincos(angTip, s, c);
                float2 radial = float2(c, s);
                float2 tang = float2(s, -c);                // clockwise direction
                float2 q = p - radial * rc;
                float across = dot(q, radial);
                float along = dot(q, tang);

                float hw2 = hw * hw;
                float blob = exp(-(across * across + along * along) / (hw2 * 0.35));
                float radialMask = exp(-(across * across) / (hw2 * 0.55));
                float ahead = exp(-max(along, 0.0) / 0.025);
                float behind = exp(-max(-along, 0.0) / 0.14);
                float streak = radialMask * lerp(behind, ahead, step(0.0, along));  // follows the ring only
                return blob + streak * 0.6;
            }

            // ------------------------------------------------------------
            // Main gauge
            // ------------------------------------------------------------
            float4 EnergyGauge(float2 p, float px)
            {
                float t = _Time.y;
                float progress = saturate(_Power * 0.01);
                float hs = saturate(_HighSpeedTransition);
                float hasProg = step(0.001, progress);
                float full = step(0.999, progress);

                // Crisp geometry coordinates
                float r = length(p);
                float2 dir = p / max(r, 1e-4);
                float2 tangCW = float2(dir.y, -dir.x);
                float2 pr = rot2(p, _RotationAngle);
                float ang = atan2(pr.y, pr.x);
                float na = 1.0 - frac((ang - PI * 0.5) / TWO_PI);   // 0..1 clockwise from top

                // Distorted coordinates (plasma only)
                float2 sh = float2(vnoise(p * 5.0 + float2(0.0, t * 2.5)),
                                   vnoise(p * 5.0 + float2(4.3, t * 2.2))) - 0.5;
                float2 pd = p + sh * 0.025 * _HeatShimmer * (0.5 + 0.5 * hs);

                // Palette
                float3 baseCol = _RimColor.rgb;
                float3 activeCol = lerp(baseCol, float3(1.0, 0.90, 0.70), hs * 0.5);
                float3 hotCol = lerp(activeCol, float3(1.0, 0.97, 0.90), 0.6);
                float3 deepCol = pow(max(activeCol, 0.001), 1.7) * 1.25;       // saturated edge colour
                float3 trackBase = _Color.rgb;

                // Light (screen space, top-left)
                float3 L = normalize(float3(-0.45, 0.55, 0.70));
                float3 H = normalize(L + float3(0, 0, 1));

                // ---------------- Ring geometry ----------------
                float inner = _EventHorizon;
                float outer = max(_RingOuter, inner + 0.08);
                float rc = 0.5 * (inner + outer);
                float hw = 0.5 * (outer - inner);
                float dBand = abs(r - rc) - hw;
                float band = saturate(0.5 - dBand / px);

                // Segments
                float N = max(round(_Layers), 3.0);
                float cell = na * N;
                float fc = frac(cell);
                float segId = floor(cell);
                float dEdge = min(fc, 1.0 - fc) / N * TWO_PI * r;      // arc length to nearest boundary
                float seg = saturate((dEdge - _SegmentGap * 0.5) / px + 0.5);

                // Beveled segments: tube cross-section + bevel toward each segment end
                float tr = clamp((r - rc) / hw, -1.0, 1.0);
                float hTube = sqrt(saturate(1.0 - tr * tr));
                float bevW = max(_Bevel * 0.035, 1e-4);
                float bev = saturate((dEdge - _SegmentGap * 0.5) / bevW);
                bev = bev * bev * (3.0 - 2.0 * bev);
                float side = (fc < 0.5) ? -1.0 : 1.0;
                float h = hTube * lerp(1.0, 0.35 + 0.65 * bev, saturate(_Bevel * 1.5));
                float3 n = normalize(float3(dir * tr * 0.9 + tangCW * side * (1.0 - bev) * _Bevel * 0.9, max(h, 0.05)));

                // ---------------- Progress ----------------
                float arcToTip = (progress - na) * TWO_PI * r;         // > 0 behind the tip
                float fillEdge = saturate(arcToTip / px + 0.5) * hasProg;
                float fillAmt = lerp(fillEdge, 1.0, hs);
                float softFill = lerp(smoothstep(-0.08, 0.04, arcToTip) * hasProg, 1.0, hs);

                // Comet falloff
                float behindP = saturate(progress - na);
                float trailNormal = exp(-behindP * 4.0);
                float head = frac(t * 0.5);
                float trailHS = exp(-frac(head - na) * 3.0);
                float trail = lerp(trailNormal, trailHS, hs);

                // ---------------- Plasma energy inside the tube ----------------
                float flowSpeed = 1.0 + 2.0 * hs;
                float flow1 = fbm4(rot2(pd, t * 0.9 * flowSpeed) * 7.0 + float2(0.0, -t * 1.5));
                float flow2 = fbm4(rot2(pd, -t * 0.6 * flowSpeed) * 12.0 + 9.0);
                float energy = lerp(0.65, 1.35, flow1) * lerp(0.85, 1.25, flow2);

                // electric ridges
                float ridge = 1.0 - abs(2.0 * flow2 - 1.0);
                energy += pow(ridge, 6.0) * 0.5;

                // pulses travelling along the ring
                float wave = pow(0.5 + 0.5 * sin(na * TWO_PI * 6.0 - t * (3.0 + 4.0 * hs)), 4.0);
                energy *= 1.0 + 0.35 * wave;

                float thd = atan2(pd.y, pd.x);
                float spin = t * 4.0;
                float arms = pow(0.5 + 0.5 * cos(10.0 * (thd - spin * 0.8) + length(pd) * 14.0), 3.0);
                energy *= lerp(1.0, 0.6 + 0.8 * arms, hs);

                float segVar = 1.0 + 0.12 * (hash11(segId + 3.1) - 0.5);

                float3 tubeCol = lerp(deepCol, hotCol, pow(h, 3.0) * 0.9);     // white-hot core, saturated edges
                float fillGain = energy * segVar * lerp(0.55, 1.7, trail) * (1.0 + 1.2 * hs);
                float specF = pow(saturate(dot(n, H)), 30.0) * 0.4;           // glass coat
                float coreLine = pow(h, 10.0) * 0.5 * (0.5 + 0.5 * trail);     // bright centre filament
                float3 fillRGB = (tubeCol * fillGain + hotCol * coreLine + specF) * fillAmt * seg * band;

                // ---------------- Track (dark glass groove) ----------------
                float diff = saturate(dot(n, L));
                float specT = pow(saturate(dot(n, H)), 40.0);
                float rimT = pow(saturate(1.0 - h), 4.0) * saturate(dot(dir, L.xy) * 0.5 + 0.5);
                float ao = lerp(0.55, 1.0, h);
                float ghost = flow1 * 0.06 * (1.0 - fillAmt);                  // faint dormant energy in the groove
                float3 trackRGB = trackBase * (0.30 + 0.65 * diff) * ao
                                + specT * 0.18 * float3(0.7, 0.8, 1.0)
                                + rimT * 0.35 * lerp(trackBase, float3(0.6, 0.8, 1.0), 0.6)
                                + activeCol * ghost * band;
                trackRGB *= (1.0 - fillAmt * 0.8) * seg * band;

                // ---------------- Multi-scale bloom around the band ----------------
                float dOut = max(dBand, 0.0);
                float glowShape = exp(-dOut * 40.0) * 0.8 + exp(-dOut * 16.0) * 0.45 + exp(-dOut * 6.0) * 0.14;
                float3 glowRGB = activeCol * glowShape * softFill * (0.35 + 0.65 * trail) * (0.6 + 1.0 * hs) * _Bloom;
                glowRGB *= (1.0 - band);

                // ---------------- Tip / orbiting head (point lights ON the ring) ----------------
                float angTip = PI * 0.5 - progress * TWO_PI - _RotationAngle;
                float angHead = PI * 0.5 - head * TWO_PI - _RotationAngle;
                float tipG = TipFlare(p, angTip, rc, hw) * hasProg * (1.0 - full) * (1.0 - hs);
                float headG = TipFlare(p, angHead, rc, hw) * hs;
                float3 tipRGB = hotCol * (tipG + headG) * 1.6;

                // ---------------- HUD tick ring ----------------
                float tIn = outer + 0.045;
                float mi = floor(na * 60.0 + 0.5);                             // nearest tick index
                float isMajor = step(abs(mi - 6.0 * round(mi / 6.0)), 0.01);
                float tOut = outer + 0.072 + isMajor * 0.02;
                float tBand = saturate(0.5 - (abs(r - 0.5 * (tIn + tOut)) - 0.5 * (tOut - tIn)) / px);
                float fm = frac(na * 60.0);
                float dm = min(fm, 1.0 - fm) / 60.0 * TWO_PI * r;
                float tMask = saturate(((0.0032 + isMajor * 0.0015) - dm) / px + 0.5);
                float tickA = tBand * tMask;
                float tickLit = lerp(saturate((progress - mi / 60.0) * 60.0 + 0.5) * hasProg, 1.0, hs);
                float tickNear = exp(-abs(progress - mi / 60.0) * 40.0) * (1.0 - hs) * hasProg; // tick next to the tip
                float3 tickDim = lerp(trackBase, float3(0.5, 0.6, 0.8), 0.5) * 0.35;
                float3 tickCol = lerp(tickDim, activeCol * (0.9 + 0.5 * trail) + hotCol * tickNear * 0.8, tickLit);
                float3 ticksRGB = tickCol * tickA;

                // Hairline guide ring + scanner arc (circular only, never a ray)
                float hairR = outer + 0.025;
                float hair = saturate(0.5 - (abs(r - hairR) - 0.5 * px) / px) * 0.5;
                float dScan = frac(na - t * 0.12 + 0.5) - 0.5;
                float scan = exp(-dScan * dScan / 0.003) * (0.3 + 0.7 * hs);
                float3 hairRGB = lerp(tickDim, activeCol, 0.25 + scan) * hair * (0.5 + 1.5 * scan);

                // ---------------- Core orb ----------------
                float coreR = max(inner - 0.04, 0.03);
                float coreMask = saturate(0.5 - (r - coreR) / px);
                float cz = sqrt(saturate(1.0 - (r * r) / (coreR * coreR)));
                float fres = pow(1.0 - cz, 3.0);
                float swirl = fbm4(rot2(pd / coreR, t * (1.5 + 3.0 * hs)) * 2.5 + t * 0.3);
                float coreI = lerp(0.25 + 0.75 * progress, 1.2, hs);
                float pulse = 1.0 + 0.25 * sin(t * lerp(3.0, 16.0, hs)) * _HeatShimmer;
                float3 nC = float3(p / coreR, cz);
                float glint = pow(saturate(dot(nC, H)), 60.0) * 0.5;
                float3 coreRGB = (activeCol * (0.35 + 1.4 * swirl) * (0.35 + 0.9 * pow(cz, 1.5))
                                + hotCol * pow(cz, 6.0) * 1.2
                                + deepCol * fres * 0.9) * coreI * pulse;
                coreRGB = (coreRGB + glint) * coreMask;
                float3 coreGlow = activeCol * exp(-max(r - coreR, 0.0) * 22.0) * coreI * 0.5 * (1.0 - coreMask) * _Bloom;

                // ---------------- Overcharge shockwave ----------------
                float ph = frac(t * 0.9);
                float swR = lerp(coreR, outer + 0.06, ph);
                float sw = exp(-pow((r - swR) / 0.025, 2.0)) * (1.0 - ph) * (1.0 - ph) * hs;
                float3 swRGB = activeCol * sw * 1.6;

                // ---------------- Embers (2D, ring-relative, bounded) ----------------
                float3 sparkRGB = 0.0;
                if (_Sparks > 0.001)
                {
                    float SN = 56.0;
                    float c = na * SN;
                    float id = floor(c);
                    float h1 = hash11(id);
                    float h2 = hash11(id + 17.3);
                    float h3 = hash11(id + 41.7);
                    float exists = step(0.4 - 0.25 * hs, hash11(id + 7.7));
                    float life = frac(t * (0.25 + 0.35 * h2) + h1);
                    float ca = (id + 0.2 + 0.6 * h3) / SN;
                    float rr = outer - 0.02 + life * (0.09 + 0.07 * h1);
                    float wob = sin(t * 3.0 + h1 * 20.0) * 0.012 * life;
                    float dx = (na - ca) * TWO_PI * rr + wob;
                    float dy = r - rr;
                    float sz = 0.005 + 0.005 * h2;
                    float s = exp(-(dx * dx + dy * dy) / (sz * sz));
                    float flick = 0.6 + 0.4 * sin(t * 20.0 * h3 + h1 * 6.0);
                    float fade = (1.0 - life) * smoothstep(0.0, 0.08, life);
                    float alive = lerp(step(ca, progress) * hasProg, 1.0, hs);
                    sparkRGB = hotCol * s * fade * flick * alive * exists * 3.0 * _Sparks;
                }

                // ---------------- Compose ----------------
                float vig = 1.0 - smoothstep(0.78, 0.97, r);
                float3 soft = (glowRGB + swRGB + sparkRGB + coreGlow) * vig;

                // Tip light is local to the tube: confine it to the ring neighbourhood
                float tipZone = exp(-pow(max(abs(r - rc) - hw, 0.0) / 0.05, 2.0));
                soft += tipRGB * tipZone * vig;

                float3 emis = trackRGB + fillRGB + ticksRGB + hairRGB + coreRGB + soft;

                float bodyA = max(max(band * seg * 0.92, tickA * 0.6), max(coreMask * 0.95, hair * 0.4));
                float alpha = saturate(bodyA + max3(soft) * 0.3);

                float edgeFade = 1.0 - smoothstep(0.97, 1.0, r);
                return float4(emis * edgeFade, alpha * edgeFade);
            }

            // ------------------------------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv - 0.5) * 2.0;
                float px = max(fwidth(p.x), 1e-4);   // pixel size in gauge space (assumes a square Image)

                float4 col = EnergyGauge(p, px);

                // Hot highlights bleed toward white
                float mx = max3(col.rgb);
                float ex = saturate((mx - 1.0) / 5.0);
                col.rgb = lerp(col.rgb, mx.xxx, ex * 0.75);

                // Exposure + filmic tonemap (set Tonemap to 0 and use HDR + Bloom if you want the post stack to do it)
                col.rgb *= _Exposure;
                col.rgb = lerp(col.rgb, ACESFilm(col.rgb), _Tonemap);

                // Dither against banding in the long glow gradients
                float dn = hash12(input.positionCS.xy + frac(_Time.y) * 61.0) - 0.5;
                col.rgb += dn / 255.0 * max(col.a, max3(col.rgb));

                // UI vertex colour (premultiplied-correct)
                col.rgb *= input.color.rgb * input.color.a;
                col.a *= input.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                float2 inside = step(_ClipRect.xy, input.worldPos.xy) * step(input.worldPos.xy, _ClipRect.zw);
                col *= inside.x * inside.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return half4(col);
            }
            ENDHLSL
        }
    }
}
