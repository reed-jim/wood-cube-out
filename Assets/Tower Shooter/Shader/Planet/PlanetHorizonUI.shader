// Unity 6.3 LTS (6000.3) - URP - UI/Canvas Image shader (no textures, fully procedural)
// Planet horizon with atmosphere glow, clouds, city lights, stars and neon cyan frame.
// Assign to a Material -> use on a UI Image (Sprite = None). Works in Screen Space Overlay / Camera / World Space.
Shader "UI/Planet Horizon (URP)"
{
    Properties
    {
        [Header(Layout)]
        [Toggle] _AutoAspect ("Auto Aspect (from rect size)", Float) = 1
        _Aspect ("Manual Aspect (W/H)", Float) = 5.25
        _Radius ("Planet Radius (in rect heights)", Range(2, 60)) = 11
        _HorizonHeight ("Horizon Height (0..1 at center)", Range(0, 1.2)) = 0.46
        _PlanetOffsetX ("Planet Center X Offset", Range(-3, 3)) = 0

        [Header(Surface)]
        _SurfaceSeed ("Seed", Vector) = (3.1, 7.7, 1.3, 0)
        _Tilt ("View Tilt", Range(-1.5, 1.5)) = 0.9
        _RotSpeed ("Rotation Speed", Range(-0.1, 0.1)) = 0.004
        _ContScale ("Continent Scale", Range(1, 40)) = 11
        _SeaLevel ("Sea Level", Range(0.3, 0.7)) = 0.5
        [HDR] _OceanDeep ("Ocean Deep", Color) = (0.004, 0.035, 0.17, 1)
        [HDR] _OceanShallow ("Ocean Shallow", Color) = (0.02, 0.22, 0.55, 1)
        [HDR] _LandA ("Land Low", Color) = (0.06, 0.2, 0.22, 1)
        [HDR] _LandB ("Land High", Color) = (0.28, 0.32, 0.3, 1)

        [Header(Clouds)]
        _CloudScale ("Cloud Scale", Range(2, 80)) = 24
        _CloudCover ("Cloud Cover", Range(0.2, 0.8)) = 0.46
        _CloudSpeed ("Cloud Speed", Range(0, 0.1)) = 0.012
        [HDR] _CloudColor ("Cloud Color", Color) = (0.75, 0.9, 1.15, 1)

        [Header(Lighting)]
        _SunDir ("Sun Direction", Vector) = (-0.35, 0.55, 0.75, 0)
        _SunIntensity ("Sun Intensity", Range(0, 4)) = 1.6
        _Ambient ("Ambient", Range(0, 1)) = 0.35
        _Terminator ("Terminator Softness", Range(0.05, 1.5)) = 0.5
        _SpecPower ("Ocean Specular Power", Range(8, 400)) = 140
        [HDR] _CityColor ("City Lights", Color) = (1.6, 0.85, 0.35, 1)
        _CityAmount ("City Lights Amount", Range(0, 3)) = 1.0

        [Header(Atmosphere)]
        [HDR] _AtmoColor ("Atmosphere Color", Color) = (0.12, 0.45, 1.2, 1)
        [HDR] _AtmoCoreColor ("Atmosphere Core (Limb)", Color) = (0.55, 0.9, 1.6, 1)
        _AtmoPower ("Fresnel Power", Range(0.5, 8)) = 2.4
        _AtmoIntensity ("Inner Scatter Intensity", Range(0, 4)) = 1.3
        _HaloFalloff ("Halo Falloff", Range(2, 120)) = 22
        _HaloIntensity ("Halo Intensity", Range(0, 6)) = 1.8
        _RimSharp ("Rim Sharpness", Range(20, 600)) = 160
        _RimIntensity ("Rim Intensity", Range(0, 8)) = 2.6
        _AtmoThickness ("Atmosphere Thickness", Range(0.02, 0.5)) = 0.18

        [Header(Output)]
        _Exposure ("Exposure", Range(0.2, 3)) = 1.0

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
            Name "PlanetHorizonUI"

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
            float4 _SurfaceSeed;
            float _Tilt, _RotSpeed, _ContScale, _SeaLevel;
            float4 _OceanDeep, _OceanShallow, _LandA, _LandB;
            float _CloudScale, _CloudCover, _CloudSpeed;
            float4 _CloudColor;
            float4 _SunDir;
            float _SunIntensity, _Ambient, _Terminator, _SpecPower;
            float4 _CityColor;
            float _CityAmount;
            float4 _AtmoColor, _AtmoCoreColor;
            float _AtmoPower, _AtmoIntensity, _HaloFalloff, _HaloIntensity, _RimSharp, _RimIntensity, _AtmoThickness;
            float _Exposure;

            // ------------------------------------------------------------------ noise
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

            float fbm(float3 p, int oct)
            {
                float a = 0.5;
                float s = 0.0;
                float norm = 0.0;
                for (int k = 0; k < oct; k++)
                {
                    s += a * vnoise(p);
                    norm += a;
                    p = p * 2.03 + float3(17.1, 5.3, 11.7);
                    a *= 0.5;
                }
                return s / norm;
            }

            float3 ACESFitted(float3 x)
            {
                return saturate((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14));
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

                // aspect: derived from UV derivatives so it matches the Image rect automatically
                float aspectAuto = abs(ddy(uv.y)) / max(abs(ddx(uv.x)), 1e-7);
                float aspect = lerp(_Aspect, aspectAuto, step(0.5, _AutoAspect));
                aspect = clamp(aspect, 0.2, 40.0);

                float2 p = float2((uv.x - 0.5) * aspect, uv.y);

                // ---- planet geometry
                float R = _Radius;
                float2 c = float2(_PlanetOffsetX, _HorizonHeight - R);
                float2 dv = (p - c) / R;
                float r = length(dv);
                float h = (r - 1.0) * R;                       // signed distance to limb (rect-height units)
                float fw = max(fwidth(h), 1e-5);
                float planetMask = 1.0 - smoothstep(-fw, fw, h);

                float3 L = normalize(_SunDir.xyz);
                float2 limbDir = dv / max(r, 1e-4);
                float limbLit = saturate(dot(limbDir, L.xy) * 0.5 + 0.8);

                float3 atmoCol = _AtmoColor.rgb;
                float3 coreCol = _AtmoCoreColor.rgb;

                float3 col = float3(0, 0, 0);
                float alphaMask = planetMask;

                // ---- atmosphere halo (strictly bounded above the horizon)
                float hOut = max(h, 0.0);
                float atmoBand = saturate(1.0 - (hOut / _AtmoThickness)); // fades out quickly above horizon
                
                if (atmoBand > 0.0)
                {
                    float halo = (exp(-hOut * _HaloFalloff) + 0.18 * exp(-hOut * _HaloFalloff * 0.22)) * atmoBand;
                    float3 haloCol = lerp(atmoCol, coreCol, exp(-hOut * _HaloFalloff * 2.5));
                    
                    col += haloCol * halo * _HaloIntensity * limbLit;
                    alphaMask = saturate(alphaMask + halo * 1.5);
                }

                // ---- planet surface
                if (planetMask > 0.0)
                {
                    float z = sqrt(saturate(1.0 - r * r));
                    float3 n = float3(dv, z);
                    float3 V = float3(0, 0, 1);

                    // body-space position: tilt then spin
                    float ct = cos(_Tilt), st = sin(_Tilt);
                    float3 b = float3(n.x, n.y * ct + n.z * st, -n.y * st + n.z * ct);
                    float ang = t * _RotSpeed * 6.2831853;
                    float ca = cos(ang), sa = sin(ang);
                    float3 sp = float3(b.x * ca + b.z * sa, b.y, -b.x * sa + b.z * ca) + _SurfaceSeed.xyz;

                    // terrain
                    float cont = fbm(sp * _ContScale, 5);
                    float land = smoothstep(_SeaLevel - 0.015, _SeaLevel + 0.015, cont);
                    float depth = saturate((_SeaLevel - cont) * 4.0);
                    float3 ocean = lerp(_OceanShallow.rgb, _OceanDeep.rgb, pow(depth, 0.6));
                    float elev = saturate((cont - _SeaLevel) * 4.0);
                    float3 ground = lerp(_LandA.rgb, _LandB.rgb, elev * (0.6 + 0.4 * vnoise(sp * 40.0)));
                    float3 albedo = lerp(ocean, ground, land);

                    // clouds (domain-warped, drifting)
                    float3 cp = sp * _CloudScale + float3(t * _CloudSpeed * 6.0, 0.0, t * _CloudSpeed * 2.0);
                    float warp = fbm(cp * 0.35 + 9.0, 3);
                    float cl = fbm(cp + warp * 1.8, 5);
                    float cloud = smoothstep(_CloudCover, _CloudCover + 0.22, cl);
                    float clShadow = smoothstep(_CloudCover, _CloudCover + 0.25,
                                      fbm(cp + warp * 1.8 + float3(L.x, L.y, 0) * -0.35, 3));

                    // lighting
                    float ndl = dot(n, L);
                    float day = smoothstep(0.0, 1.0, saturate((ndl + _Terminator) / (1.0 + _Terminator)));
                    float3 H = normalize(L + V);
                    float spec = pow(saturate(dot(n, H)), _SpecPower) * (1.0 - land) * (1.0 - cloud) * day;
                    float fresnelSea = pow(1.0 - saturate(dot(n, V)), 4.0) * (1.0 - land) * 0.6;

                    float3 lit = albedo * (day * _SunIntensity * (1.0 - clShadow * 0.45) + _Ambient);
                    lit += atmoCol * fresnelSea * (0.3 + day);
                    lit += float3(1.0, 0.95, 0.85) * spec * 2.0;

                    // clouds shading
                    float3 cloudLit = _CloudColor.rgb * (day * _SunIntensity * 0.75 + _Ambient * 1.1);
                    lit = lerp(lit, cloudLit, cloud * 0.9);

                    // night-side city lights
                    float clusters = smoothstep(0.55, 0.78, fbm(sp * 16.0, 3));
                    float dots = smoothstep(0.62, 0.9, vnoise(sp * 110.0));
                    float lights = land * clusters * dots * (1.0 - day) * (1.0 - cloud * 0.8);
                    lit += _CityColor.rgb * lights * _CityAmount;

                    // atmospheric inner scatter (fresnel)
                    float fres = pow(saturate(1.0 - z), _AtmoPower);
                    float scatterLight = 0.25 + 0.75 * saturate(ndl * 0.5 + 0.7);
                    lit += lerp(atmoCol, coreCol, fres) * fres * _AtmoIntensity * scatterLight;
                    lit = lerp(lit, lit + atmoCol * 0.15, 1.0 - z);

                    col = lerp(col, lit, planetMask);
                }

                // ---- limb rim line
                float rim = exp(-abs(h) * _RimSharp);
                rim += 0.35 * exp(-max(-h, 0.0) * _RimSharp * 0.18);
                col += coreCol * rim * _RimIntensity * limbLit * 0.5;
                alphaMask = saturate(alphaMask + rim * _RimIntensity);

                // ---- output: tonemap + dither
                col *= _Exposure;
                col = ACESFitted(col);
                float dn = frac(52.9829189 * frac(dot(IN.positionCS.xy, float2(0.06711056, 0.00583715))));
                col += (dn - 0.5) / 255.0 * planetMask;

                half4 outCol = half4(col * IN.color.rgb * IN.color.a, alphaMask * IN.color.a);

                #ifdef UNITY_UI_CLIP_RECT
                outCol.a *= UIClip(IN.posOS.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(outCol.a - 0.001);
                #endif

                return outCol;
            }
            ENDHLSL
        }
    }
    FallBack Off
}