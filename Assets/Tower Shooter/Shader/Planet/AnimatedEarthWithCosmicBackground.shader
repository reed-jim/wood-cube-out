Shader "UI/AnimatedEarthWithCosmicBackground"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)

        [Header(Master Speed)]
        _GlobalSpeed ("Global Animation Speed", Range(0.0, 2.0)) = 0.2
        [Toggle(_LOW_QUALITY)] _LowQuality ("Low Quality (Mobile)", Float) = 0

        [Header(Earth Surface)]
        _OceanColor ("Deep Ocean Color", Color) = (0.01, 0.07, 0.26, 1.0)
        _ShallowOceanColor ("Shallow Coast Color", Color) = (0.03, 0.40, 0.58, 1.0)
        _LandColor ("Land Color", Color) = (0.12, 0.42, 0.10, 1.0)
        _MountainColor ("Mountain/Desert Color", Color) = (0.62, 0.52, 0.30, 1.0)
        _PlanetRadius ("Earth Radius Size", Range(0.1, 0.45)) = 0.28
        _PlanetTilt ("Planet Axis Tilt Angle", Range(-180, 180)) = 45.0
        _AxialPitch ("Axis Pitch Toward Viewer", Range(-60, 60)) = 20.0
        _RotationSpeed ("Earth Rotation Speed", Range(-5.0, 5.0)) = 0.4
        _Turbulence ("Continent Noise Scale", Range(1.0, 10.0)) = 2.8
        _SeaLevel ("Sea Level", Range(0.3, 0.7)) = 0.54
        _BumpStrength ("Terrain Relief Strength", Range(0.0, 3.0)) = 1.2

        [Header(Ocean)]
        _OceanSpecular ("Ocean Sun Glint", Range(0.0, 3.0)) = 1.0
        _OceanGloss ("Ocean Glint Sharpness", Range(8, 256)) = 80
        _CoastFoam ("Coast Foam", Range(0.0, 1.0)) = 0.6

        [Header(Clouds and Atmosphere)]
        _CloudColor ("Cloud Color", Color) = (0.95, 0.98, 1.0, 1.0)
        _CloudCoverage ("Cloud Density", Range(0.0, 1.0)) = 0.48
        _CloudSpeed ("Cloud Drift Speed Factor", Range(1.0, 3.0)) = 1.3
        _CloudShadow ("Cloud Shadow On Ground", Range(0.0, 1.0)) = 0.6
        _AtmosphereColor ("Atmosphere Blue Glow", Color) = (0.28, 0.58, 1.0, 1.0)
        _AtmospherePower ("Atmosphere Glow Sharpness", Range(0.5, 10.0)) = 3.5
        _PulseSpeed ("Atmosphere Pulse Speed", Range(0.0, 5.0)) = 1.5

        [Header(Sun and Night Side)]
        _SunYaw ("Sun Yaw (left/right)", Range(-180, 180)) = -55
        _SunPitch ("Sun Pitch (down/up)", Range(-80, 80)) = 20
        _SunColor ("Sun Color", Color) = (1.0, 0.93, 0.82, 1.0)
        _AmbientColor ("Night Side Ambient", Color) = (0.035, 0.05, 0.11, 1.0)
        _TerminatorSoftness ("Terminator Softness", Range(0.05, 0.8)) = 0.3
        _TwilightColor ("Twilight Color", Color) = (1.0, 0.45, 0.18, 1.0)
        _CityLightColor ("City Light Color", Color) = (1.0, 0.72, 0.35, 1.0)
        _CityLightIntensity ("City Light Intensity", Range(0.0, 4.0)) = 1.2

        [Header(Polar Caps)]
        _PolarCapColor ("Polar Ice Color", Color) = (0.9, 0.95, 1.0, 1.0)
        _PolarCapSize ("Polar Cap Size", Range(0.0, 1.0)) = 0.4

        [Header(Limb)]
        _RimPower ("Limb Darkening Sharpness", Range(1.0, 20.0)) = 5.0
        _MinEdgeBrightness ("Minimum Edge Brightness", Range(0.1, 1.0)) = 0.6

        [Header(Orbital Ring)]
        _RingColor ("Ring Color", Color) = (0.25, 0.85, 1.0, 1.0)
        _RingIntensity ("Ring Intensity (0 = off)", Range(0.0, 3.0)) = 1.0
        _RingRadius ("Ring Radius", Range(0.2, 0.48)) = 0.41
        _RingTilt ("Ring Ellipse Flatness", Range(0.05, 1.0)) = 0.28
        _RingAngle ("Ring Rotation", Range(-180, 180)) = -20
        _RingWidth ("Ring Thickness", Range(0.001, 0.02)) = 0.004

        [Header(Background Gradient Properties)]
        _GradientStart ("Gradient Start", Range(0, 1)) = 0.0
        _GradientEnd ("Gradient End", Range(0, 1)) = 1.0
        _GradientAngle ("Gradient Angle", Range(0, 360)) = 0.0
        _BackgroundBrightness ("Background Brightness", Range(0.0, 2.0)) = 1.0

        [Header(Animated Cosmic Background)]
        _SpaceColor ("Space Dark Base", Color) = (0.006, 0.01, 0.03, 1.0)
        _DustColor ("Cosmic Dust Streak Color", Color) = (0.10, 0.35, 0.60, 1.0)
        _NebulaColorA ("Nebula Color 1", Color) = (0.16, 0.06, 0.34, 1.0)
        _NebulaColorB ("Nebula Color 2", Color) = (0.03, 0.16, 0.36, 1.0)
        _BackgroundSpeed ("Background Drift Speed", Range(0.0, 2.0)) = 0.3
        _StarDensity ("Star Density", Range(0.0, 1.0)) = 0.6
        _StarBrightness ("Star Brightness", Range(0.0, 3.0)) = 1.2
        _MeteorIntensity ("Shooting Star Intensity", Range(0.0, 2.0)) = 0.8
        _Vignette ("Background Vignette", Range(0.0, 1.0)) = 0.5

        [Header(Color Grading)]
        _Exposure ("Exposure", Range(0.2, 3.0)) = 1.0
        _Tonemap ("Filmic Tonemap Amount", Range(0.0, 1.0)) = 0.5
        _Saturation ("Saturation", Range(0.0, 2.0)) = 1.15
        _Contrast ("Contrast", Range(0.5, 1.5)) = 1.05

        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip ("Use UI Alpha Clip", Float) = 0

        // UI Masking properties
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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma shader_feature_local _LOW_QUALITY

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define K_PI  3.14159265
            #define K_TAU 6.28318531

            #if defined(_LOW_QUALITY)
                #define OCT_TERRAIN 3
                #define OCT_CLOUD   3
            #else
                #define OCT_TERRAIN 5
                #define OCT_CLOUD   5
            #endif

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;

                #if defined(UNITY_UI_CLIP_RECT)
                float4 worldPosition : TEXCOORD1;
                #endif
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _GlobalSpeed;

                float4 _OceanColor;
                float4 _ShallowOceanColor;
                float4 _LandColor;
                float4 _MountainColor;
                float _PlanetRadius;
                float _PlanetTilt;
                float _AxialPitch;
                float _RotationSpeed;
                float _Turbulence;
                float _SeaLevel;
                float _BumpStrength;

                float _OceanSpecular;
                float _OceanGloss;
                float _CoastFoam;

                float4 _CloudColor;
                float _CloudCoverage;
                float _CloudSpeed;
                float _CloudShadow;
                float4 _AtmosphereColor;
                float _AtmospherePower;
                float _PulseSpeed;

                float _SunYaw;
                float _SunPitch;
                float4 _SunColor;
                float4 _AmbientColor;
                float _TerminatorSoftness;
                float4 _TwilightColor;
                float4 _CityLightColor;
                float _CityLightIntensity;

                float4 _PolarCapColor;
                float _PolarCapSize;

                float _RimPower;
                float _MinEdgeBrightness;

                float4 _RingColor;
                float _RingIntensity;
                float _RingRadius;
                float _RingTilt;
                float _RingAngle;
                float _RingWidth;

                float _GradientStart;
                float _GradientEnd;
                float _GradientAngle;
                float _BackgroundBrightness;

                float4 _SpaceColor;
                float4 _DustColor;
                float4 _NebulaColorA;
                float4 _NebulaColorB;
                float _BackgroundSpeed;
                float _StarDensity;
                float _StarBrightness;
                float _MeteorIntensity;
                float _Vignette;

                float _Exposure;
                float _Tonemap;
                float _Saturation;
                float _Contrast;

                float4 _ClipRect;
            CBUFFER_END

            // ------------------------------------------------------------------
            // Hashes / noise (sin-free, stable at large coordinates)
            // ------------------------------------------------------------------
            float ShSq(float x) { return x * x; }

            float ShHash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float ShHash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float ShHash13(float3 p3)
            {
                p3 = frac(p3 * 0.1031);
                p3 += dot(p3, p3.zyx + 31.32);
                return frac((p3.x + p3.y) * p3.z);
            }

            float2 ShHash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            // 3D value noise with quintic interpolation, output 0..1
            float ShNoise3(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);
                f = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);

                float n000 = ShHash13(i);
                float n100 = ShHash13(i + float3(1, 0, 0));
                float n010 = ShHash13(i + float3(0, 1, 0));
                float n110 = ShHash13(i + float3(1, 1, 0));
                float n001 = ShHash13(i + float3(0, 0, 1));
                float n101 = ShHash13(i + float3(1, 0, 1));
                float n011 = ShHash13(i + float3(0, 1, 1));
                float n111 = ShHash13(i + float3(1, 1, 1));

                return lerp(lerp(lerp(n000, n100, f.x), lerp(n010, n110, f.x), f.y),
                            lerp(lerp(n001, n101, f.x), lerp(n011, n111, f.x), f.y), f.z);
            }

            // Normalised fBm, output ~0..1
            float ShFbm(float3 p, int oct)
            {
                float v = 0.0, a = 0.5, tot = 0.0;
                for (int i = 0; i < oct; i++)
                {
                    v += a * ShNoise3(p);
                    tot += a;
                    p = p * 2.03 + float3(5.2, 1.3, 3.7);
                    a *= 0.5;
                }
                return v / tot;
            }

            // Ridged multifractal (mountain ranges / dust lanes)
            float ShRidged(float3 p, int oct)
            {
                float v = 0.0, a = 0.5, tot = 0.0;
                for (int i = 0; i < oct; i++)
                {
                    float n = 1.0 - abs(ShNoise3(p) * 2.0 - 1.0);
                    v += a * n * n;
                    tot += a;
                    p = p * 2.1 + float3(3.1, 7.7, 1.9);
                    a *= 0.5;
                }
                return v / tot;
            }

            float2 ShRot2(float2 v, float deg)
            {
                float r = radians(deg);
                float c = cos(r), s = sin(r);
                return float2(v.x * c - v.y * s, v.x * s + v.y * c);
            }

            float3x3 ShRotZ(float a)
            {
                float c = cos(a), s = sin(a);
                return float3x3(c, -s, 0, s, c, 0, 0, 0, 1);
            }
            float3x3 ShRotX(float a)
            {
                float c = cos(a), s = sin(a);
                return float3x3(1, 0, 0, 0, c, -s, 0, s, c);
            }
            float3x3 ShRotY(float a)
            {
                float c = cos(a), s = sin(a);
                return float3x3(c, 0, -s, 0, 1, 0, s, 0, c);
            }

            float3 ShACES(float3 x)
            {
                return saturate((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14));
            }

            // ------------------------------------------------------------------
            // Planet fields (sampled on the unit sphere in planet space)
            // ------------------------------------------------------------------
            float ShElevation(float3 pp, float scale)
            {
                float3 q = pp * scale;
                float3 w = float3(ShNoise3(q * 0.8 + float3(7.1, 2.3, 5.9)),
                                  ShNoise3(q * 0.8 + float3(13.7, 8.1, 1.1)),
                                  ShNoise3(q * 0.8 + float3(3.3, 19.2, 9.4))) - 0.5;
                q += w * 1.0;
                float e = ShFbm(q, OCT_TERRAIN);
                return saturate((e - 0.5) * 1.7 + 0.5);
            }

            float ShCloudDensity(float3 pc, float scale)
            {
                float3 q = pc * scale;
                float3 w = float3(ShNoise3(q * 0.7 + float3(1.7, 4.2, 9.1)),
                                  ShNoise3(q * 0.7 + float3(8.3, 0.4, 2.2)),
                                  ShNoise3(q * 0.7 + float3(5.5, 6.6, 3.1))) - 0.5;
                q += w * 1.2;
                float d = ShFbm(q, OCT_CLOUD);
                return saturate((d - 0.5) * 1.5 + 0.5);
            }

            // ------------------------------------------------------------------
            // Stars / meteors
            // ------------------------------------------------------------------
            float3 ShStarLayer(float2 uv, float scale, float seed, float prob, float twT)
            {
                float2 g = uv * scale;
                float2 id = floor(g);
                float2 f = frac(g) - 0.5;

                float h = ShHash12(id * 1.31 + seed);
                if (h > prob) return float3(0, 0, 0);

                float2 h2 = ShHash22(id + seed * 17.0);
                float2 off = (h2 - 0.5) * 0.7;
                float2 d = f - off;
                float rr = length(d);

                float r = h / max(prob, 1e-4);
                float r3 = r * r * r;
                float size = lerp(0.03, 0.10, r3);

                float core = ShSq(saturate(1.0 - rr / size));
                float halo = exp(-rr * rr / (size * size * 10.0)) * 0.18;
                float tw = 0.6 + 0.4 * sin(twT * (1.2 + 3.0 * h2.x) + h2.y * K_TAU * 8.0);

                float temp = ShHash12(id + seed * 3.7);
                float3 tint = lerp(float3(0.65, 0.78, 1.0), float3(1.0, 0.85, 0.65), temp);
                tint = lerp(tint, float3(1, 1, 1), 0.35);

                float flare = 0.0;
                if (r > 0.92)
                {
                    float2 ad = abs(d);
                    flare = (exp(-ad.x * 45.0) * exp(-ad.y * ad.y * 3000.0) +
                             exp(-ad.y * 45.0) * exp(-ad.x * ad.x * 3000.0)) * 0.5;
                }

                float bright = lerp(0.4, 1.6, r3);
                return tint * (core + halo + flare) * tw * bright;
            }

            float3 ShStarField(float2 uv, float drift, float dens, float twT)
            {
                float3 s = float3(0, 0, 0);
                s += ShStarLayer(uv + float2(drift * 0.010, 0.0),          26.0, 1.0, 0.30 * dens, twT) * 0.7;
                s += ShStarLayer(uv + float2(drift * 0.020, drift * 0.003), 13.0, 2.0, 0.22 * dens, twT) * 1.0;
                s += ShStarLayer(uv + float2(drift * 0.040, drift * 0.006),  6.5, 3.0, 0.12 * dens, twT) * 1.3;
                return s;
            }

            float3 ShMeteor(float2 uv, float t)
            {
                float slot = floor(t);
                float f = frac(t);
                float h = ShHash11(slot * 7.13 + 1.7);
                if (h < 0.55) return float3(0, 0, 0);

                float2 st = float2(ShHash11(slot * 1.71 + 3.1), 0.55 + 0.45 * ShHash11(slot * 3.37 + 1.3));
                float ang = -(0.45 + 0.5 * ShHash11(slot * 5.11 + 8.2));
                float2 dir = float2(cos(ang), sin(ang));

                float2 head = st + dir * (f * 1.4 - 0.2);
                float2 d = uv - head;
                float along = -dot(d, dir);
                float perp = abs(d.x * dir.y - d.y * dir.x);

                float trail = step(0.0, along) * exp(-along / 0.22 * 3.0);
                float w = 0.0028;
                float core = exp(-(perp * perp) / (w * w)) * trail;
                float headGlow = exp(-dot(d, d) / (0.006 * 0.006));
                float life = smoothstep(0.0, 0.12, f) * smoothstep(1.0, 0.65, f);

                return float3(0.85, 0.92, 1.0) * (core + headGlow) * life;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;

                #if defined(UNITY_UI_CLIP_RECT)
                output.worldPosition = input.positionOS;
                #endif

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 cuv = uv - 0.5;
                float dist = length(cuv);
                float aa = max(fwidth(dist), 1e-5);

                float t = _Time.y;
                float bgT = t * _BackgroundSpeed * _GlobalSpeed * 5.0;
                float pulse = sin(t * _PulseSpeed) * 0.5 + 0.5;

                // ---------------- Directional gradient mask (Restored) ----------------
                float angleRad = radians(_GradientAngle);
                float2 gradDir = float2(cos(angleRad), sin(angleRad));
                float gradPos = dot(cuv, gradDir) + 0.5;
                float gradRange = max(_GradientEnd - _GradientStart, 0.0001);
                float bgAlpha = smoothstep(0.0, 1.0, saturate((gradPos - _GradientStart) / gradRange));

                // ---------------- Sun direction (view space, +z toward camera) ----------------
                float yaw = radians(_SunYaw);
                float pit = radians(_SunPitch);
                float3 L = normalize(float3(sin(yaw) * cos(pit), sin(pit), cos(yaw) * cos(pit)));

                // ==================================================================
                // COSMIC BACKGROUND
                // ==================================================================
                float3 q0 = float3(uv * 2.2, bgT * 0.35);
                float2 warp = float2(ShFbm(q0 + float3(0, 0, 5.2), 3),
                                     ShFbm(q0 + float3(1.7, 9.2, 1.3), 3)) - 0.5;

                float n1 = ShFbm(float3(uv * 2.6 + warp * 1.2, bgT * 0.25), 5);
                float n2 = ShFbm(float3(uv * 4.2 - warp * 0.8 + 11.3, bgT * 0.18 + 3.0), 4);

                float3 space = _SpaceColor.rgb;
                space = lerp(space, _NebulaColorA.rgb, smoothstep(0.30, 0.72, n1));
                space = lerp(space, _NebulaColorB.rgb, smoothstep(0.40, 0.78, n2) * 0.85);

                // diagonal dust lanes
                float2 dustUV = ShRot2(cuv, -20.0);
                dustUV.y *= 3.0;
                float dust = ShRidged(float3(dustUV * 2.2 + warp * 0.6, bgT * 0.2), 4);
                float dustShape = smoothstep(0.22, 0.55, dust) * (0.35 + 0.65 * smoothstep(0.30, 0.70, n1));
                space += _DustColor.rgb * dustShape * 0.9;

                // hot nebula cores
                float hot = smoothstep(0.55, 0.85, n1);
                space += lerp(_NebulaColorA.rgb, _DustColor.rgb, 0.5) * hot * 1.2;

                // stars + shooting star
                float3 stars = ShStarField(uv, bgT, _StarDensity, t) * _StarBrightness;
                space += stars * (1.0 - hot * 0.3);

                if (_MeteorIntensity > 0.001)
                    space += ShMeteor(uv, t * 0.2) * _MeteorIntensity;

                space *= _BackgroundBrightness;
                space *= 1.0 - _Vignette * smoothstep(0.2, 0.72, dist);

                // ==================================================================
                // PLANET
                // ==================================================================
                float R = _PlanetRadius;
                float cover = saturate((R - dist) / aa + 0.5);
                float3 planetCol = float3(0, 0, 0);

                if (dist < R + aa)
                {
                    float2 N = cuv / R;
                    N /= max(length(N), 1.0);
                    float z = sqrt(saturate(1.0 - dot(N, N)));
                    float3 nV = float3(N, z);

                    // view space -> planet space (roll, pitch, spin)
                    float spin = t * _RotationSpeed * 0.15;
                    float3x3 M = mul(ShRotY(spin), mul(ShRotX(-radians(_AxialPitch)), ShRotZ(radians(_PlanetTilt))));
                    float3 pp = mul(M, nV);
                    float3 Lp = mul(M, L);
                    float lat = abs(pp.y);

                    float scale = _Turbulence;
                    float sea = _SeaLevel;

                    // ---------- terrain ----------
                    float e = ShElevation(pp, scale);

                    float3 nB = nV;
                    #if !defined(_LOW_QUALITY)
                    {
                        float eps = 0.012;
                        float3 nx = normalize(nV + float3(eps, 0, 0));
                        float3 ny = normalize(nV + float3(0, eps, 0));
                        float ex = ShElevation(mul(M, nx), scale);
                        float ey = ShElevation(mul(M, ny), scale);
                        float h0 = max(e - sea, 0.0);
                        float hx = max(ex - sea, 0.0) - h0;
                        float hy = max(ey - sea, 0.0) - h0;
                        nB = normalize(nV - float3(hx, hy, 0.0) / eps * _BumpStrength * 0.12);
                    }
                    #endif

                    // Clean sharp step for land/ocean border without extra artifact lines
                    float landMask = step(sea, e);
                    float alt = saturate((e - sea) / max(1.0 - sea, 0.01));
                    float ridge = ShRidged(pp * scale * 3.0, 3);
                    float moisture = ShFbm(pp * scale * 1.3 + 31.7, 3);
                    float tnoise = ShNoise3(pp * scale * 2.0 + 5.0);
                    float temp = saturate(1.0 - lat * 1.25 + (tnoise - 0.5) * 0.3 - alt * 0.35);

                    float desert = smoothstep(0.52, 0.36, moisture) * smoothstep(0.30, 0.65, temp);
                    float mount = smoothstep(0.18, 0.50, alt) * smoothstep(0.28, 0.50, ridge);

                    float3 grass = lerp(_LandColor.rgb * 0.55, _LandColor.rgb, smoothstep(0.30, 0.65, moisture));
                    float3 sand = _MountainColor.rgb;
                    float3 rock = lerp(_MountainColor.rgb * 0.55, float3(0.42, 0.40, 0.40), 0.5);

                    float3 land = lerp(grass, sand, desert);
                    land = lerp(land, rock, mount);
                    float snow = smoothstep(0.55, 0.85, alt + (1.0 - temp) * 0.45 - 0.2);
                    land = lerp(land, _PolarCapColor.rgb, snow);
                    land *= 0.9 + 0.2 * ShNoise3(pp * scale * 12.0);

                    // ---------- ocean ----------
                    float depth = saturate((sea - e) / 0.2);
                    float3 oceanA = lerp(_ShallowOceanColor.rgb, _OceanColor.rgb, pow(depth, 0.55));
                    oceanA += _ShallowOceanColor.rgb * exp(-depth * 10.0) * 0.35;

                    // ---------- polar caps ----------
                    float capEdge = 1.0 - _PolarCapSize * 0.5 + (ShNoise3(pp * scale * 3.0 + 9.0) - 0.5) * 0.12;
                    float cap = smoothstep(capEdge, capEdge + 0.07, lat);

                    // ---------- clouds ----------
                    float cloudAngle = spin * (_CloudSpeed - 1.0) + t * _GlobalSpeed * 0.03;
                    float3x3 Rc = ShRotY(cloudAngle);
                    float cs = scale * 1.4;
                    float cd = ShCloudDensity(mul(Rc, pp), cs);
                    float thr = lerp(0.85, 0.35, _CloudCoverage);
                    float cloudMask = smoothstep(thr, thr + 0.2, cd);
                    float cloudThick = smoothstep(thr, thr + 0.4, cd);

                    float cdS = ShCloudDensity(mul(Rc, normalize(pp + Lp * 0.05)), cs);
                    float cloudShadow = smoothstep(thr, thr + 0.2, cdS) * _CloudShadow;

                    // ---------- lighting ----------
                    float ndlG = dot(nV, L);
                    float ndlB = dot(nB, L);
                    float soft = _TerminatorSoftness;
                    float litB = smoothstep(-0.1, soft + 0.2, ndlB);
                    float litG = smoothstep(-0.1, soft + 0.2, ndlG);
                    float diffuse = litB * (0.78 + 0.22 * saturate(ndlB));
                    float sunTerm = diffuse * (1.0 - cloudShadow * 0.65);

                    float3 albedo = lerp(oceanA, land, landMask);
                    albedo = lerp(albedo, _PolarCapColor.rgb, cap * 0.95);
                    float oceanness = (1.0 - landMask) * (1.0 - cap);

                    float3 lightCol = _AmbientColor.rgb + _SunColor.rgb * sunTerm * 1.25;
                    float3 surf = albedo * lightCol;

                    // ocean sun glint (animated ripples) + sky reflection
                    float2 rip = float2(ShNoise3(pp * 24.0 + float3(t * 0.5, 0, 0)),
                                        ShNoise3(pp * 24.0 + float3(0, t * 0.5, 17.3))) - 0.5;
                    float3 nO = normalize(nV + float3(rip, 0.0) * 0.10);
                    float3 Hv = normalize(L + float3(0, 0, 1));
                    float specCore = pow(saturate(dot(nO, Hv)), _OceanGloss);
                    float specSheen = pow(saturate(dot(nV, Hv)), _OceanGloss * 0.08) * 0.12;
                    float spec = (specCore + specSheen) * _OceanSpecular * smoothstep(0.0, 0.2, ndlG);
                    surf += _SunColor.rgb * spec * oceanness * (1.0 - cloudMask);

                    float fres = pow(1.0 - z, 3.0);
                    surf += _AtmosphereColor.rgb * fres * 0.45 * litG * oceanness;

                    // twilight band along the terminator
                    float twi = exp(-ShSq(ndlG / 0.22));
                    surf += albedo * _TwilightColor.rgb * twi * 0.35;

                    // city lights on the night side
                    if (_CityLightIntensity > 0.001)
                    {
                        float night = smoothstep(0.1, -0.25, ndlG);
                        float pop = smoothstep(0.50, 0.72, ShFbm(pp * scale * 3.0 + 77.0, 3));
                        float dots = smoothstep(0.80, 0.95, ShNoise3(pp * 55.0));
                        float flick = 0.75 + 0.25 * sin(t * 2.5 + ShHash13(floor(pp * 55.0)) * K_TAU);
                        float cityMask = pop * dots * flick * landMask
                                       * step(sea + 0.01, e)
                                       * (1.0 - smoothstep(0.68, 0.82, lat))
                                       * (1.0 - mount)
                                       * (1.0 - cloudMask * 0.85);
                        surf += _CityLightColor.rgb * cityMask * night * _CityLightIntensity;
                    }

                    // cloud layer lighting
                    float cloudLit = smoothstep(-0.15, 0.55, ndlG);
                    float3 cloudDark = _CloudColor.rgb * (_AmbientColor.rgb * 1.5 + 0.03);
                    float3 cloudBright = _CloudColor.rgb * (_AmbientColor.rgb + _SunColor.rgb * 1.1);
                    float3 cloudCol = lerp(cloudDark, cloudBright, cloudLit);
                    cloudCol *= lerp(1.0, 0.7, cloudThick * (1.0 - 0.5 * cloudLit));
                    cloudCol += _TwilightColor.rgb * twi * 0.25 * cloudMask;
                    surf = lerp(surf, cloudCol, cloudMask * 0.92);

                    // limb darkening
                    surf *= lerp(_MinEdgeBrightness, 1.0, pow(z, 1.0 / _RimPower));

                    // atmosphere: rim scatter, thin limb line, haze
                    float k = _AtmospherePower + pulse * 0.5;
                    float rim = pow(1.0 - z, k);
                    float sunSide = saturate(ndlG * 0.7 + 0.45);
                    float3 atmCol = lerp(_AtmosphereColor.rgb, _TwilightColor.rgb, saturate(twi * 0.8));
                    surf += atmCol * rim * (0.2 + 0.8 * sunSide) * 0.9;

                    float limbLine = pow(1.0 - z, 12.0);
                    surf += lerp(_AtmosphereColor.rgb, float3(1, 1, 1), 0.55) * limbLine * sunSide * 0.9;
                    surf += _AtmosphereColor.rgb * 0.05 * pow(1.0 - z, 1.5) * litG;

                    planetCol = surf;
                }

                // ---------------- Outer atmosphere glow (sun-side biased) ----------------
                float dOut = max(0.0, dist - R);
                float kk = _AtmospherePower + pulse * 0.5;
                float2 dir = cuv / max(dist, 1e-4);
                float sunSideO = saturate(0.45 + dot(dir, L.xy) * 0.7);
                float atmosO = exp(-dOut * kk * 12.0);
                float glowK = atmosO * (0.65 + pulse * 0.2) * (0.25 + 0.75 * sunSideO);
                float halo = exp(-dOut * 4.0) * 0.18 * sunSideO;
                float3 glowCol = lerp(_AtmosphereColor.rgb, float3(1, 1, 1), atmosO * 0.25);
                float3 glowRGB = glowCol * (glowK + halo) * (1.0 - cover);

                // ---------------- Orbital energy ring (passes behind/in front of planet) ----------------
                float3 ringRGB = float3(0, 0, 0);
                if (_RingIntensity > 0.001)
                {
                    float2 p = ShRot2(cuv, -_RingAngle);
                    float a = _RingRadius;
                    float b = max(a * _RingTilt, 1e-3);
                    float re = length(float2(p.x / a, p.y / b));
                    float2 g = float2(p.x / (a * a), p.y / (b * b)) / max(re, 1e-4);
                    float rd = (re - 1.0) / max(length(g), 1e-4);   // ~distance to ellipse in UV units
                    float w = _RingWidth;
                    float ang = atan2(p.y / b, p.x / a);

                    float core = exp(-(rd * rd) / (w * w));
                    float glow = exp(-abs(rd) / (w * 7.0)) * 0.22;
                    float core2 = exp(-ShSq((abs(rd) - w * 5.5) / (w * 0.7))) * 0.45;
                    float tick = smoothstep(0.5, 0.62, sin(ang * 60.0)) * exp(-ShSq((rd - w * 4.0) / (w * 1.6)));
                    float dash = 0.55 + 0.45 * smoothstep(-0.2, 0.5, sin(ang * 5.0 - t * 0.8));

                    float headPos = frac(t * 0.12);
                    float behind = frac(headPos - ang / K_TAU);
                    float spark = exp(-behind * 12.0) * smoothstep(1.0, 0.96, behind);

                    float shape = core * dash + core * spark * 3.0 + glow * (0.5 + spark) + core2 + tick * 0.5;
                    float3 rc = lerp(_RingColor.rgb, float3(1, 1, 1), saturate(core * 0.5 + spark * 0.5));

                    float frontW = smoothstep(0.08, -0.08, p.y / b);
                    float vis = frontW + (1.0 - frontW) * (1.0 - cover);
                    ringRGB = rc * shape * _RingIntensity * vis;
                }

                // ---------------- Compose (premultiplied with gradient alpha) ----------------
                float alphaP = bgAlpha * (1.0 - cover) + cover;
                float3 addRGB = glowRGB + ringRGB;
                float3 rgbP = space * bgAlpha * (1.0 - cover) + planetCol * cover + addRGB;
                float alphaOut = saturate(alphaP + max(addRGB.r, max(addRGB.g, addRGB.b)));
                float3 col = rgbP / max(alphaOut, 1e-4);

                // ---------------- Grading ----------------
                col *= _Exposure;
                col = lerp(col, ShACES(col), _Tonemap);
                float lum = dot(col, float3(0.299, 0.587, 0.114));
                col = lerp(float3(lum, lum, lum), col, _Saturation);
                col = (col - 0.5) * _Contrast + 0.5;
                col = max(col, 0.0);
                col += (ShHash12(input.positionCS.xy) - 0.5) * (2.0 / 255.0);   // kill banding in dark gradients

                half4 result = half4(col, alphaOut) * input.color;

                // UI Clipping
                #if defined(UNITY_UI_CLIP_RECT)
                float2 inside = step(_ClipRect.xy, input.worldPosition.xy) * step(input.worldPosition.xy, _ClipRect.zw);
                result.a *= inside.x * inside.y;
                #endif

                // Alpha Clip
                #if defined(UNITY_UI_ALPHACLIP)
                clip(result.a - 0.001);
                #endif

                return result;
            }
            ENDHLSL
        }
    }
}