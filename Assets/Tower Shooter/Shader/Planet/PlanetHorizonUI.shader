// Unity 6.3 LTS (6000.3) - URP - UI/Canvas Image shader (no textures, fully procedural)
// Holographic planet horizon: wireframe globe, contour/dot-matrix data terrain, scanlines,
// scan sweep, glitch, chromatic limb and a cyan halo. Tuned to sit on a #121726 background.
// Assign to a Material -> use on a UI Image (Sprite = None). Works in Screen Space Overlay / Camera / World Space.
Shader "UI/Planet Horizon Hologram (URP)"
{
    Properties
    {
        [Header(Layout)]
        [Toggle] _AutoAspect ("Auto Aspect (from rect size)", Float) = 1
        _Aspect ("Manual Aspect (W/H)", Float) = 5.25
        _Radius ("Planet Radius (in rect heights)", Range(2, 60)) = 11
        _HorizonHeight ("Horizon Height (0..1 at center)", Range(0, 1.2)) = 0.46
        _PlanetOffsetX ("Planet Center X Offset", Range(-3, 3)) = 0

        [Header(Palette)]
        _BgColor ("Background Color (#121726)", Color) = (0.0706, 0.0902, 0.1490, 1)
        [HDR] _HoloColor ("Hologram Main", Color) = (0.10, 0.80, 1.10, 1)
        [HDR] _HotColor ("Hologram Hot (Rim / Sweep)", Color) = (0.65, 1.15, 1.45, 1)
        [HDR] _DimColor ("Hologram Deep (Fill / Back Side)", Color) = (0.04, 0.32, 0.75, 1)

        [Header(Body)]
        _BodyOpacity ("Body Plate Opacity", Range(0, 1)) = 0.55
        _FillGlow ("Fill Glow", Range(0, 2)) = 0.35
        _FresnelPower ("Fresnel Power", Range(0.5, 8)) = 2.0
        _InnerFalloff ("Depth Falloff (from limb)", Range(0, 20)) = 3.0
        _DepthFloor ("Depth Floor", Range(0, 1)) = 0.18

        [Header(Globe Grid)]
        _Tilt ("View Tilt", Range(-1.5, 1.5)) = 0.9
        _RotSpeed ("Rotation Speed", Range(-0.1, 0.1)) = 0.006
        [IntRange] _LonLines ("Longitude Lines", Range(6, 144)) = 72
        [IntRange] _LatLines ("Latitude Lines", Range(4, 72)) = 36
        [IntRange] _MajorEvery ("Major Line Every N", Range(2, 12)) = 6
        _GridWidth ("Grid Line Width (px)", Range(0.5, 3)) = 1.0
        _GridIntensity ("Grid Intensity", Range(0, 3)) = 0.6
        _BackIntensity ("Back Hemisphere Intensity", Range(0, 1)) = 0.22

        [Header(Data Terrain)]
        _SurfaceSeed ("Seed", Vector) = (3.1, 7.7, 1.3, 0)
        _ContScale ("Continent Scale", Range(1, 40)) = 8
        _SeaLevel ("Sea Level", Range(0.3, 0.7)) = 0.5
        _ContourSpacing ("Contour Spacing (height)", Range(0.005, 0.1)) = 0.03
        _ContourIntensity ("Contour Intensity", Range(0, 3)) = 0.5
        _CoastIntensity ("Coastline Intensity", Range(0, 4)) = 1.2
        [IntRange] _DotCols ("Dot Columns (around globe)", Range(60, 720)) = 360
        _DotSize ("Dot Size", Range(0.05, 0.45)) = 0.2
        _DotIntensity ("Dot Intensity", Range(0, 3)) = 0.7
        _LandFill ("Land Fill", Range(0, 1)) = 0.12
        _OceanDots ("Ocean Dot Level", Range(0, 1)) = 0.1

        [Header(Hologram FX)]
        _ScanDensity ("Scanline Density (per rect height)", Range(20, 400)) = 140
        _ScanStrength ("Scanline Strength", Range(0, 1)) = 0.35
        _ScanSpeed ("Scanline Drift Speed", Range(-2, 2)) = 0.25
        _SweepSpeed ("Sweep Speed (cycles/s)", Range(0, 1)) = 0.18
        _SweepWidth ("Sweep Width", Range(0.01, 0.3)) = 0.05
        _SweepIntensity ("Sweep Intensity", Range(0, 4)) = 1.6
        _Flicker ("Flicker", Range(0, 0.5)) = 0.06
        _GlitchAmount ("Glitch Frequency", Range(0, 1)) = 0.25
        _GlitchStrength ("Glitch Slice Offset", Range(0, 0.05)) = 0.012
        _ChromaShift ("Limb Chromatic Split (px)", Range(0, 6)) = 0.9
        _EdgeBreakup ("Edge Breakup", Range(0, 1)) = 0.35

        [Header(Limb and Halo)]
        _RimWidth ("Rim Line Width (px)", Range(0.5, 4)) = 1.6
        _RimIntensity ("Rim Intensity", Range(0, 8)) = 2.4
        _HaloFalloff ("Halo Falloff", Range(2, 120)) = 26
        _HaloIntensity ("Halo Intensity", Range(0, 6)) = 1.5
        _AtmoThickness ("Halo Thickness", Range(0.02, 0.5)) = 0.2

        [Header(Frame)]
        [Toggle] _FrameOn ("Neon Frame", Float) = 0
        [HDR] _FrameColor ("Frame Color", Color) = (0.2, 0.9, 1.2, 1)
        _FrameWidth ("Frame Width (px)", Range(0.5, 4)) = 1.5
        _FrameGlow ("Frame Glow", Range(0, 2)) = 0.6

        [Header(Output)]
        _Exposure ("Exposure", Range(0.2, 3)) = 1.0
        [Toggle] _SoftClip ("Soft Clip (turn off for HDR + Bloom)", Float) = 1
        [Toggle] _HighQuality ("High Quality", Float) = 1

        // UI required
        [PerRendererData] _MainTex ("Sprite (unused)", 2D) = "white" {}
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
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
            Name "PlanetHorizonHologramUI"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
                float4 posOS      : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _ClipRect;

            float _AutoAspect, _Aspect, _Radius, _HorizonHeight, _PlanetOffsetX;
            float4 _BgColor, _HoloColor, _HotColor, _DimColor;
            float _BodyOpacity, _FillGlow, _FresnelPower, _InnerFalloff, _DepthFloor;
            float _Tilt, _RotSpeed, _LonLines, _LatLines, _MajorEvery, _GridWidth, _GridIntensity, _BackIntensity;
            float4 _SurfaceSeed;
            float _ContScale, _SeaLevel, _ContourSpacing, _ContourIntensity, _CoastIntensity;
            float _DotCols, _DotSize, _DotIntensity, _LandFill, _OceanDots;
            float _ScanDensity, _ScanStrength, _ScanSpeed, _SweepSpeed, _SweepWidth, _SweepIntensity;
            float _Flicker, _GlitchAmount, _GlitchStrength, _ChromaShift, _EdgeBreakup;
            float _RimWidth, _RimIntensity, _HaloFalloff, _HaloIntensity, _AtmoThickness;
            float _FrameOn, _FrameWidth, _FrameGlow;
            float4 _FrameColor;
            float _Exposure, _SoftClip, _HighQuality;

            // ------------------------------------------------------------------ hashing / noise
            float hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float hash13(float3 p3)
            {
                p3 = frac(p3 * 0.1031);
                p3 += dot(p3, p3.zyx + 31.32);
                return frac((p3.x + p3.y) * p3.z);
            }

            float vnoise(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);
                f = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);

                float n000 = hash13(i);
                float n100 = hash13(i + float3(1, 0, 0));
                float n010 = hash13(i + float3(0, 1, 0));
                float n110 = hash13(i + float3(1, 1, 0));
                float n001 = hash13(i + float3(0, 0, 1));
                float n101 = hash13(i + float3(1, 0, 1));
                float n011 = hash13(i + float3(0, 1, 1));
                float n111 = hash13(i + float3(1, 1, 1));

                return lerp(lerp(lerp(n000, n100, f.x), lerp(n010, n110, f.x), f.y),
                            lerp(lerp(n001, n101, f.x), lerp(n011, n111, f.x), f.y), f.z);
            }

            float fbm(float3 p, const int oct)
            {
                float a = 0.5;
                float s = 0.0;
                float norm = 0.0;
                [unroll]
                for (int k = 0; k < oct; k++)
                {
                    s += a * vnoise(p);
                    norm += a;
                    p = p * 2.03 + float3(17.1, 5.3, 11.7);
                    a *= 0.5;
                }
                return s / norm;
            }

            // ------------------------------------------------------------------ helpers
            // Spin + tilt. rot = (cos tilt, sin tilt, cos spin, sin spin)
            float3 ToBody(float3 n, float4 rot)
            {
                float3 b = float3(n.x, n.y * rot.x + n.z * rot.y, -n.y * rot.y + n.z * rot.x);
                return float3(b.x * rot.z + b.z * rot.w, b.y, -b.x * rot.w + b.z * rot.z);
            }

            // Derivative that survives the atan2 seam (needs integer cell counts around the globe)
            float PeriodicFw(float x)
            {
                return min(fwidth(x), fwidth(frac(x + 0.5)));
            }

            // Anti-aliased line at every integer of x, width in pixels. Fades out when lines get denser than the pixel grid.
            float LineAA(float x, float widthPx)
            {
                float f = abs(frac(x + 0.5) - 0.5);
                float w = max(PeriodicFw(x), 1e-6);
                float cov = saturate(widthPx * 0.5 + 0.5 - f / w);
                return cov * saturate(1.0 - w * 1.4);
            }

            float RimAA(float hpx, float widthPx)
            {
                return saturate(widthPx * 0.5 + 0.5 - abs(hpx));
            }

            float UIClip(float2 pos, float4 rect)
            {
                float2 inside = step(rect.xy, pos) * step(pos, rect.zw);
                return inside.x * inside.y;
            }

            // ------------------------------------------------------------------ vertex
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.posOS = IN.positionOS;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            // ------------------------------------------------------------------ fragment
            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float t = _Time.y;
                bool hq = _HighQuality > 0.5;

                float2 fuv = fwidth(uv);

                // aspect: derived from UV derivatives so it matches the Image rect automatically
                float aspectAuto = abs(ddy(uv.y)) / max(abs(ddx(uv.x)), 1e-7);
                float aspect = clamp(lerp(_Aspect, aspectAuto, step(0.5, _AutoAspect)), 0.2, 40.0);

                // ---- glitch bursts (rare, short): horizontal slice displacement of the whole projection
                float seg = floor(t * 6.0);
                float gOn = step(1.0 - _GlitchAmount * 0.25, hash11(seg * 1.37 + 3.1));
                float sId = floor(uv.y * 36.0) + floor(t * 24.0) * 7.0;
                float slice = gOn * step(0.55, hash11(sId)) * (hash11(sId + 11.0) * 2.0 - 1.0);

                float2 p = float2((uv.x - 0.5) * aspect + slice * _GlitchStrength, uv.y);

                // ---- global hologram modulation
                float flick = 1.0 - _Flicker * (0.5 + 0.5 * sin(t * 47.0 + sin(t * 13.0) * 3.0));
                flick *= 1.0 - gOn * 0.3 * hash11(floor(t * 30.0) + 5.0);

                float scanFade = saturate(1.0 - fuv.y * _ScanDensity * 2.5);
                float scanWave = 0.5 + 0.5 * sin((uv.y * _ScanDensity - t * _ScanSpeed * 10.0) * 6.2831853);
                float scanMul = 1.0 - _ScanStrength * scanFade * scanWave;

                float ph = frac(t * _SweepSpeed);
                float sweepT = saturate(ph / 0.65);
                float sweepY = lerp(-_SweepWidth * 2.5, _HorizonHeight + _AtmoThickness * 0.5 + _SweepWidth * 3.0, sweepT);
                float sw = (p.y - sweepY) / _SweepWidth;
                float sweep = exp(-sw * sw);

                float brkRnd = hash12(float2(floor(p.x * 3.0), floor(uv.y * 50.0)) + floor(t * 18.0) * 13.0);
                float brk = 1.0 - _EdgeBreakup * smoothstep(0.6, 1.0, brkRnd);

                // ---- planet geometry
                float R = _Radius;
                float2 c = float2(_PlanetOffsetX, _HorizonHeight - R);
                float2 dv = (p - c) / R;
                float r = length(dv);
                float h = (r - 1.0) * R;                     // signed distance to limb (rect-height units)
                float fw = max(fwidth(h), 1e-5);
                float hp = h / fw;                           // signed distance to limb in pixels (+ = outside)
                float planetMask = saturate(0.5 - hp);

                float depthGlow = lerp(_DepthFloor, 1.0, exp(min(h, 0.0) * _InnerFalloff));
                float spot = 1.0 - 0.45 * pow(saturate(abs(p.x - _PlanetOffsetX) / (aspect * 0.5)), 2.0);

                float ct = cos(_Tilt), st = sin(_Tilt);
                float ang = t * _RotSpeed * 6.2831853;
                float4 rot = float4(ct, st, cos(ang), sin(ang));

                // ---- hologram body (only evaluated on / right next to the planet)
                float3 holo = float3(0, 0, 0);

                [branch]
                if (hp < 2.0)
                {
                    float z = sqrt(saturate(1.0 - r * r));
                    float fres = pow(saturate(1.0 - z), _FresnelPower);

                    // front hemisphere in body space
                    float3 sF = ToBody(float3(dv, z), rot);
                    float lon = atan2(sF.x, sF.z);
                    float lat = asin(clamp(sF.y, -0.9999, 0.9999));
                    float xLon = lon * (_LonLines * 0.15915494);
                    float xLat = lat * (_LatLines * 0.31830989);

                    // wireframe
                    float gMinor = max(LineAA(xLon, _GridWidth), LineAA(xLat, _GridWidth));
                    float gMajor = max(LineAA(xLon / _MajorEvery, _GridWidth * 1.6),
                                       LineAA(xLat / _MajorEvery, _GridWidth * 1.6));
                    float grid = max(gMinor * 0.45, gMajor);
                    float eq = saturate(_GridWidth + 0.5 - abs(lat) / max(fwidth(lat), 1e-6));

                    // data terrain
                    float3 nz = (sF + _SurfaceSeed.xyz) * _ContScale;
                    float cont;
                    if (hq) cont = fbm(nz, 4); else cont = fbm(nz, 3);
                    float fwc = max(fwidth(cont), 0.0015);
                    float landSoft = smoothstep(_SeaLevel - fwc, _SeaLevel + fwc, cont);
                    float coast = saturate(1.1 - abs(cont - _SeaLevel) / fwc);

                    float xC = (cont - _SeaLevel) / _ContourSpacing;
                    float contour = max(LineAA(xC, 1.0) * 0.5, LineAA(xC / 4.0, 1.6)) * landSoft;

                    // dot matrix (staggered rows), brighter on land
                    float kDot = _DotCols * 0.15915494;
                    float u0 = lon * kDot;
                    float v = lat * kDot;
                    float aaD = max(max(PeriodicFw(u0), fwidth(v)), 1e-5);
                    float u = u0 + 0.5 * fmod(floor(v), 2.0);
                    float2 cell = float2(frac(u), frac(v)) - 0.5;
                    float dotM = (1.0 - smoothstep(_DotSize - aaD, _DotSize + aaD, length(cell)))
                               * saturate(1.0 - aaD * 2.2);
                    float polar = saturate((1.35 - abs(lat)) * 4.0);
                    float tw = 1.0;
                    if (hq)
                    {
                        float dr = hash12(floor(float2(u, v)));
                        tw = (0.55 + 0.45 * sin(t * 1.7 + dr * 6.2831853)) * step(0.12, dr);
                    }
                    float dots = dotM * polar * tw * lerp(_OceanDots, 1.0, landSoft);

                    float lineBoost = 1.0 + sweep * _SweepIntensity;

                    float3 surf = _HoloColor.rgb * (grid * _GridIntensity
                                                  + contour * _ContourIntensity
                                                  + dots * _DotIntensity
                                                  + landSoft * _LandFill);
                    surf += _HotColor.rgb * (coast * _CoastIntensity + eq * _GridIntensity * 0.9);
                    surf *= lineBoost;

                    // volumetric fill + sweep glow
                    surf += _DimColor.rgb * (fres * 0.85 + 0.15) * _FillGlow;
                    surf += _HotColor.rgb * sweep * 0.08;

                    // see-through back hemisphere (grid only, dim)
                    if (hq)
                    {
                        float3 sB = ToBody(float3(dv, -z), rot);
                        float lonB = atan2(sB.x, sB.z);
                        float latB = asin(clamp(sB.y, -0.9999, 0.9999));
                        float backGrid = max(LineAA(lonB * (_LonLines * 0.15915494), 1.0),
                                             LineAA(latB * (_LatLines * 0.31830989), 1.0));
                        surf += _DimColor.rgb * backGrid * _BackIntensity * 2.0;
                    }

                    holo = surf * depthGlow * planetMask * lerp(1.0, brk, 0.6);
                }

                // ---- halo above the horizon (bounded band)
                float hOut = max(h, 0.0);
                float atmoBand = saturate(1.0 - hOut / _AtmoThickness);
                float halo = (exp(-hOut * _HaloFalloff) + 0.18 * exp(-hOut * _HaloFalloff * 0.22)) * atmoBand;
                halo *= (1.0 - planetMask);
                float3 haloCol = lerp(_HoloColor.rgb, _HotColor.rgb, exp(-hOut * _HaloFalloff * 2.5));
                float3 haloGlow = haloCol * halo * _HaloIntensity * spot * lerp(1.0, brk, 0.7) * (1.0 + sweep * 0.6);

                // ---- limb rim line (cheap chromatic split on High)
                float3 rimV;
                if (hq)
                {
                    float cs = _ChromaShift * (1.0 + gOn * 2.5);
                    rimV = float3(RimAA(hp - cs, _RimWidth), RimAA(hp, _RimWidth), RimAA(hp + cs, _RimWidth));
                }
                else
                {
                    rimV = RimAA(hp, _RimWidth).xxx;
                }
                float3 rimGlow = rimV * _HotColor.rgb * _RimIntensity * spot * brk * (1.0 + sweep);

                // ---- combine
                float3 glow = (holo + haloGlow + rimGlow) * scanMul * flick * _Exposure;

                if (_FrameOn > 0.5)
                {
                    float2 e = min(uv, 1.0 - uv) / max(fuv, 1e-6);
                    float d = min(e.x, e.y);
                    float f = saturate(_FrameWidth * 0.5 + 0.5 - d) + _FrameGlow * exp(-d * 0.15);
                    glow += _FrameColor.rgb * f * flick;
                }

                if (_SoftClip > 0.5)
                {
                    float3 over = max(glow - 0.7, 0.0);
                    glow = min(glow, 0.7) + 0.3 * (1.0 - exp(-over / 0.3));
                }

                // translucent plate in the background color so the globe sits cleanly on #121726
                float plateA = _BodyOpacity * (0.5 + 0.5 * depthGlow) * planetMask;
                float3 plateRGB = _BgColor.rgb * plateA;

                float3 total = glow * IN.color.rgb * IN.color.a + plateRGB * IN.color.a;

                // dither (kills banding in the dark glow gradients)
                float dn = frac(52.9829189 * frac(dot(IN.positionCS.xy, float2(0.06711056, 0.00583715))));
                float lum = max(total.r, max(total.g, total.b));
                total = max(total + (dn - 0.5) / 255.0 * saturate(lum * 20.0), 0.0);

                half4 outCol = half4(total, plateA * IN.color.a);

                #ifdef UNITY_UI_CLIP_RECT
                float clipM = UIClip(IN.posOS.xy, _ClipRect);
                outCol *= clipM;
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(max(outCol.a, lum) - 0.001);
                #endif

                return outCol;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
