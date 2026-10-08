Shader "UI/PlanetAugmentBadge"
{
    Properties
    {
        [PerRendererData] _MainTex ("UI Texture", 2D) = "white" {}
        _Color ("UI Color", Color) = (1,1,1,1)

        _PlanetColor ("Planet", Color) = (0.15,0.55,1,1)
        _PlanetColor2 ("Planet Dark", Color) = (0.02,0.12,0.35,1)
        _GlowColor ("Glow", Color) = (0.3,0.8,1,1)

        _PlanetType ("Planet Type", Range(0,4)) = 0
        _OrbitSpeed ("Orbit Speed", Range(-5,5)) = 1
        _PulseSpeed ("Pulse Speed", Range(0,5)) = 1
        _Glow ("Glow", Range(0,3)) = 1
        _SurfaceDetail ("Surface Detail", Range(0,1)) = .5
        _Border ("Border", Range(.01,.15)) = .045
        _Roundness ("Roundness", Range(.02,.5)) = .16
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct A { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct V { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_ST,_Color,_PlanetColor,_PlanetColor2,_GlowColor;
            float _PlanetType,_OrbitSpeed,_PulseSpeed,_Glow,_SurfaceDetail,_Border,_Roundness;

            V vert(A v)
            {
                V o;
                o.vertex=TransformObjectToHClip(v.vertex.xyz);
                o.uv=TRANSFORM_TEX(v.uv,_MainTex);
                o.color=v.color*_Color;
                return o;
            }

            float hash(float2 p)
            {
                return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);
            }

            float noise(float2 p)
            {
                float2 i=floor(p),f=frac(p);
                f=f*f*(3-2*f);
                return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),
                            lerp(hash(i+float2(0,1)),hash(i+float2(1,1)),f.x),f.y);
            }

            float roundBox(float2 p,float2 b,float r)
            {
                float2 q=abs(p)-b+r;
                return length(max(q,0))+min(max(q.x,q.y),0)-r;
            }

            float ring(float2 p,float radius,float width)
            {
                float d=abs(length(p)-radius);
                return 1-smoothstep(width,width*2,d);
            }

            half4 frag(V i):SV_Target
            {
                float2 uv=i.uv;
                float2 p=uv-.5;
                p.x*=3.333;

                // Badge
                float bd=roundBox(p,float2(1.66,.5),_Roundness);
                float badge=1-smoothstep(0,.015,bd);
                float edge=1-smoothstep(_Border,_Border+.015,abs(bd));

                // Subtle animated badge background
                float t=_Time.y;
                float bgPulse=.92+sin(t*_PulseSpeed)*.04;
                float3 bg=lerp(_PlanetColor2.rgb*.5,_PlanetColor2.rgb,bgPulse);

                // Planet position
                float2 pp=p-float2(-1.05,0);
                float planetR=.27;
                float pd=length(pp);
                float planetMask=1-smoothstep(planetR,planetR-.015,pd);

                // Planet surface
                float2 surfaceUV=pp/planetR;
                float n=noise(surfaceUV*5+float2(t*.03,0));
                float surface=saturate(n*_SurfaceDetail+.5);

                float3 planet=lerp(_PlanetColor2.rgb,_PlanetColor.rgb,surface);

                // Planet type variation
                if(_PlanetType<.5) // Earth
                {
                    float land=noise(surfaceUV*7+float2(.0,t*.02));
                    planet=lerp(planet,_PlanetColor.rgb,land*.45);
                }
                else if(_PlanetType<1.5) // Mars
                {
                    float dust=noise(surfaceUV*9+t*.01);
                    planet+=dust*.12;
                }
                else if(_PlanetType<2.5) // Proxima
                {
                    float flare=pow(saturate(sin(t*2.5+surfaceUV.x*8)),8);
                    planet+=_GlowColor.rgb*flare*.3;
                }
                else if(_PlanetType<3.5) // Gas Giant
                {
                    float bands=sin(surfaceUV.y*18+n*3);
                    planet+=_GlowColor.rgb*smoothstep(.4,.9,bands)*.15;
                }
                else // Exotic
                {
                    planet+=_GlowColor.rgb*n*.3;
                }

                // Planet lighting
                float3 normal=float3(pp/planetR,sqrt(max(0,1-pow(pd/planetR,2))));
                float light=saturate(dot(normal,normalize(float3(-.5,.4,1))));
                planet*=.65+.55*light;

                // Rotating orbit
                float a=t*_OrbitSpeed;
                float2 op=float2(cos(a)*pp.x+sin(a)*pp.y,-sin(a)*pp.x+cos(a)*pp.y);

                float orbit=ring(op,.38,.018);

                // Orbit particle
                float particleAngle=t*_OrbitSpeed*1.8;
                float2 particle=float2(cos(particleAngle),sin(particleAngle))*.38;
                float particleGlow=1-smoothstep(.025,.055,length(op-particle));

                // Planet glow
                float planetGlow=1-smoothstep(.25,.5,pd);
                float pulse=.8+.2*sin(t*_PulseSpeed);

                // Small moving energy streak
                float streak=exp(-pow((uv.x-frac(t*.08))/.035,2))*.15;

                float3 col=bg;
                col+=_GlowColor.rgb*planetGlow*_Glow*.12;
                col+=_GlowColor.rgb*orbit*_Glow*.7;
                col+=_GlowColor.rgb*particleGlow*_Glow*1.5;
                col+=_GlowColor.rgb*streak;

                col=lerp(col,planet,planetMask);
                col+=_GlowColor.rgb*edge*.5*_Glow;

                float4 tex=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                return half4(col,badge*tex.a*i.color.a);
            }
            ENDHLSL
        }
    }
}