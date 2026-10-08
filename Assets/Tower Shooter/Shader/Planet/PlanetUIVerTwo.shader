Shader "UI/URP/PlanetGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Planet Archetype)]
        [KeywordEnum(Terrestrial, GasGiant, CrateredMoon, LavaVolcanic)] _PLANET_TYPE("Planet Type", Float) = 0

        [Header(Base Colors)]
        _BaseColor ("Base Color (Ocean/Crust/Gas)", Color) = (0.05, 0.2, 0.5, 1)
        _DetailColor ("Detail Color (Land/Bands/Magma)", Color) = (0.1, 0.6, 0.2, 1)
        _SecondaryColor ("Secondary Color (Polar Ice/Crater Rims)", Color) = (0.8, 0.9, 1.0, 1)
        _AtmosphereColor ("Atmosphere Glow Color", Color) = (0.2, 0.7, 1.0, 1)

        [Header(Terrain Details)]
        _PlanetRadius ("Planet Radius", Range(0.1, 0.48)) = 0.38
        _RotationSpeed ("Rotation Speed", Range(-2.0, 2.0)) = 0.2
        _NoiseScale ("Noise / Detail Scale", Range(0.5, 20.0)) = 3.5
        _LandThreshold ("Land / Pattern Cutoff", Range(0.0, 1.0)) = 0.5
        _BandStretch ("Gas Band Horizontal Stretch", Range(1.0, 20.0)) = 10.0
        _CraterDensity ("Crater Density", Range(1.0, 15.0)) = 6.0

        [Header(Cloud Layer)]
        _CloudColor ("Cloud Color", Color) = (1, 1, 1, 0.8)
        _CloudSpeed ("Cloud Speed Multiplier", Range(-3.0, 3.0)) = 1.3
        _CloudDensity ("Cloud Cover Density", Range(0.0, 1.0)) = 0.45

        [Header(Emissive Glow)]
        _EmissiveColor ("Lava / City Lights Color", Color) = (1.0, 0.3, 0.0, 1)
        _EmissionPower ("Lava / Emission Intensity", Range(0.0, 10.0)) = 0.0

        [Header(Glow Border)]
        _GlowThickness ("Glow Thickness", Range(0.01, 0.2)) = 0.08
        _GlowPower ("Glow Falloff Power", Range(1.0, 8.0)) = 3.0

        [Header(Lighting)]
        _LightDir ("Light Direction (XY)", Vector) = (0.5, 0.5, 1.0, 0.0)

        // Canvas UI Masking
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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #pragma shader_feature_local _PLANET_TYPE_TERRESTRIAL _PLANET_TYPE_GASGIANT _PLANET_TYPE_CRATEREDMOON _PLANET_TYPE_LAVAVOLCANIC
            #pragma shader_feature_local _USEUIALPHACLIP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float4 worldPosition: TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _BaseColor;
                float4 _DetailColor;
                float4 _SecondaryColor;
                float4 _AtmosphereColor;
                float4 _CloudColor;
                float4 _EmissiveColor;
                float4 _ClipRect;
                float4 _LightDir;
                float _PlanetRadius;
                float _RotationSpeed;
                float _NoiseScale;
                float _LandThreshold;
                float _BandStretch;
                float _CraterDensity;
                float _CloudSpeed;
                float _CloudDensity;
                float _EmissionPower;
                float _GlowThickness;
                float _GlowPower;
            CBUFFER_END

            // --- NOISE HELPERS ---
            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float noise(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(lerp(hash(i + float3(0,0,0)), hash(i + float3(1,0,0)), f.x),
                                 lerp(hash(i + float3(0,1,0)), hash(i + float3(1,1,0)), f.x), f.y),
                            lerp(lerp(hash(i + float3(0,0,1)), hash(i + float3(1,0,1)), f.x),
                                 lerp(hash(i + float3(0,1,1)), hash(i + float3(1,1,1)), f.x), f.y), f.z);
            }

            float fbm(float3 p)
            {
                float val = 0.0;
                float amp = 0.5;
                for(int i = 0; i < 4; i++)
                {
                    val += amp * noise(p);
                    p *= 2.02;
                    amp *= 0.5;
                }
                return val;
            }

            // Voronoi for Moon Craters
            float voronoi(float3 p)
            {
                float3 b = floor(p);
                float3 r = frac(p);
                float minDist = 1.0;

                for(int z = -1; z <= 1; z++)
                for(int y = -1; y <= 1; y++)
                for(int x = -1; x <= 1; x++)
                {
                    float3 cell = float3(x, y, z);
                    float3 offset = float3(hash(b + cell), hash(b + cell + 13.1), hash(b + cell + 31.7));
                    float3 distVec = cell + offset - r;
                    float d = length(distVec);
                    minDist = min(minDist, d);
                }
                return minDist;
            }

            float UnityGet2DClipping(float2 position, float4 clipRect)
            {
                float2 inside = step(clipRect.xy, position) * step(position, clipRect.zw);
                return inside.x * inside.y;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.worldPosition = input.positionOS;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 p = input.uv - float2(0.5, 0.5);
                float dist = length(p);

                float r = _PlanetRadius;
                float glowOuter = r + _GlowThickness;

                half4 finalColor = half4(0, 0, 0, 0);

                if (dist < r)
                {
                    // Calculate Hemisphere Normal
                    float z = sqrt(r * r - dist * dist);
                    float3 sphereNormal = normalize(float3(p.x, p.y, z));

                    // Rotate coordinates over time
                    float angle = _Time.y * _RotationSpeed;
                    float cosA = cos(angle);
                    float sinA = sin(angle);
                    float3 rotNormal = float3(
                        sphereNormal.x * cosA - sphereNormal.z * sinA,
                        sphereNormal.y,
                        sphereNormal.x * sinA + sphereNormal.z * cosA
                    );

                    float3 surfaceColor = _BaseColor.rgb;
                    float emissionMask = 0.0;

                    #if defined(_PLANET_TYPE_GASGIANT)
                        // Gas Giant: Horizontal Streaked Noise with Swirls
                        float3 gasPos = rotNormal;
                        gasPos.x *= _BandStretch;
                        float n = fbm(gasPos * _NoiseScale);
                        float band = sin(rotNormal.y * 15.0 + n * 4.0) * 0.5 + 0.5;
                        surfaceColor = lerp(_BaseColor.rgb, _DetailColor.rgb, band);
                        
                        // Add Great Red Spot storm storm swirl
                        float storm = smoothstep(0.25, 0.0, length(rotNormal.xy - float2(0.3, -0.2)));
                        surfaceColor = lerp(surfaceColor, _SecondaryColor.rgb, storm);

                    #elif defined(_PLANET_TYPE_CRATEREDMOON)
                        // Cratered Moon: Voronoi Impact Basins
                        float v = voronoi(rotNormal * _CraterDensity);
                        float craterRim = smoothstep(0.1, 0.3, v) - smoothstep(0.3, 0.4, v);
                        float craterFloor = smoothstep(0.0, 0.15, v);
                        
                        surfaceColor = lerp(_DetailColor.rgb, _BaseColor.rgb, craterFloor);
                        surfaceColor += craterRim * _SecondaryColor.rgb * 0.5;

                    #elif defined(_PLANET_TYPE_LAVAVOLCANIC)
                        // Lava Planet: Dark Crust Cracks + Emissive Magma Fissures
                        float n = fbm(rotNormal * _NoiseScale);
                        float crust = smoothstep(_LandThreshold - 0.05, _LandThreshold + 0.05, n);
                        surfaceColor = lerp(_SecondaryColor.rgb, _BaseColor.rgb, crust);
                        
                        // Fissure Lines
                        float crack = smoothstep(0.48, 0.52, abs(n - _LandThreshold));
                        emissionMask = (1.0 - crack) * _EmissionPower;

                    #else
                        // Terrestrial Planet (Default Continents + Ocean)
                        float n = fbm(rotNormal * _NoiseScale);
                        float landMask = smoothstep(_LandThreshold - 0.02, _LandThreshold + 0.02, n);
                        
                        // Add Polar Ice Caps
                        float iceCap = smoothstep(0.7, 0.9, abs(rotNormal.y));
                        
                        surfaceColor = lerp(_BaseColor.rgb, _DetailColor.rgb, landMask);
                        surfaceColor = lerp(surfaceColor, _SecondaryColor.rgb, iceCap);
                    #endif

                    // Light calculation
                    float3 lightDir = normalize(_LightDir.xyz);
                    float diff = max(0.05, dot(sphereNormal, lightDir)); // Ambient minimum

                    // Add Top Cloud Layer (Terrestrial & Gas Giants)
                    #if defined(_PLANET_TYPE_TERRESTRIAL) || defined(_PLANET_TYPE_GASGIANT)
                        float cloudAngle = _Time.y * _RotationSpeed * _CloudSpeed;
                        float3 cloudRot = float3(
                            sphereNormal.x * cos(cloudAngle) - sphereNormal.z * sin(cloudAngle),
                            sphereNormal.y,
                            sphereNormal.x * sin(cloudAngle) + sphereNormal.z * cos(cloudAngle)
                        );
                        float cloudNoise = fbm(cloudRot * (_NoiseScale * 1.5) + float3(1.2, 3.4, 5.6));
                        float cloudMask = smoothstep(1.0 - _CloudDensity, 1.0 - _CloudDensity + 0.1, cloudNoise);
                        
                        // Apply Cloud Shadow & Color
                        surfaceColor = lerp(surfaceColor, surfaceColor * 0.4, cloudMask * 0.5); // Shadow
                        surfaceColor = lerp(surfaceColor, _CloudColor.rgb, cloudMask * _CloudColor.a);
                    #endif

                    // Combine Lighting & Night-side Emission
                    float3 litRGB = surfaceColor * diff;
                    float3 emissiveRGB = _EmissiveColor.rgb * emissionMask;
                    
                    // Fresnel Rim Atmosphere Reflection
                    float rim = pow(1.0 - max(0.0, sphereNormal.z), 2.5);
                    float3 atmosphereRim = _AtmosphereColor.rgb * rim * _AtmosphereColor.a;

                    finalColor = half4(litRGB + emissiveRGB + atmosphereRim, 1.0);
                }
                else if (dist < glowOuter)
                {
                    // Outer Atmosphere Glow Ring
                    float glowFactor = (dist - r) / _GlowThickness;
                    float alphaGlow = pow(1.0 - glowFactor, _GlowPower);
                    finalColor = half4(_AtmosphereColor.rgb, alphaGlow * _AtmosphereColor.a);
                }

                finalColor *= input.color;

                #ifdef UNITY_UI_ALPHACLIP
                    finalColor.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                    clip(finalColor.a - 0.001);
                #else
                    finalColor.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                #endif

                return finalColor;
            }
            ENDHLSL
        }
    }
}
