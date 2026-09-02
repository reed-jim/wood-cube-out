Shader "Custom/2D/ParallaxSpaceBackground"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)

        [Header(Space Base)]
        _SpaceColor ("Space BG Color", Color) = (0.02, 0.02, 0.05, 1.0)

        [Header(Global Motion)]
        _GlobalDirection ("Direction Vector (X, Y)", Vector) = (1.0, 0.5, 0, 0)
        _GlobalSpeed ("Global Motion Speed", Float) = 0.05

        [Header(Nebula Layer)]
        _NebulaColor ("Nebula Color", Color) = (0.4, 0.1, 0.6, 1.0)
        _NebulaScale ("Nebula Scale", Float) = 2.0
        _NebulaParallax ("Nebula Parallax Multiplier", Range(0, 2)) = 0.2
        _NebulaScroll ("Nebula Internal Morph", Vector) = (0.01, 0.005, 0, 0)

        [Header(Distant Stars Layer)]
        _StarColor1 ("Distant Star Color", Color) = (0.8, 0.9, 1.0, 1.0)
        _StarScale1 ("Distant Star Density", Float) = 30.0
        _StarParallax1 ("Distant Star Parallax", Range(0, 2)) = 0.5
        _StarThreshold1 ("Distant Star Threshold", Range(0.8, 0.999)) = 0.97

        [Header(Near Stars Layer)]
        _StarColor2 ("Near Star Color", Color) = (1.0, 0.8, 0.6, 1.0)
        _StarScale2 ("Near Star Density", Float) = 15.0
        _StarParallax2 ("Near Star Parallax", Range(0, 2)) = 1.0
        _StarThreshold2 ("Near Star Threshold", Range(0.8, 0.999)) = 0.95
        _TwinkleSpeed ("Twinkle Speed", Float) = 3.0
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
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _SpaceColor;

            float2 _GlobalDirection;
            float _GlobalSpeed;

            fixed4 _NebulaColor;
            float _NebulaScale;
            float _NebulaParallax;
            float2 _NebulaScroll;

            fixed4 _StarColor1;
            float _StarScale1;
            float _StarParallax1;
            float _StarThreshold1;

            fixed4 _StarColor2;
            float _StarScale2;
            float _StarParallax2;
            float _StarThreshold2;
            float _TwinkleSpeed;

            // Pseudo-random noise function
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Simple 2D Value Noise for Nebulas
            float valueNoise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);

                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            // Layered Noise (FBM) for smooth cloud formations
            float fbm(float2 st)
            {
                float value = 0.0;
                float amplitude = 0.5;
                for (int k = 0; k < 3; k++)
                {
                    value += amplitude * valueNoise(st);
                    st *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                // Base motion vector driven internally by time
                float2 baseMotion = normalize(_GlobalDirection) * (_Time.y * _GlobalSpeed);

                // 1. Base Space Background
                fixed4 finalColor = _SpaceColor;

                // 2. Nebula Layer (Motion + Internal Cloud Morphing)
                float2 nebulaUV = (uv + baseMotion * _NebulaParallax) * _NebulaScale + (_Time.y * _NebulaScroll);
                float nebulaIntensity = fbm(nebulaUV);
                nebulaIntensity = pow(nebulaIntensity, 2.0); // Sharpen clouds
                finalColor.rgb += _NebulaColor.rgb * nebulaIntensity * _NebulaColor.a;

                // 3. Distant Stars Layer
                float2 starUV1 = (uv + baseMotion * _StarParallax1) * _StarScale1;
                float starRand1 = hash21(floor(starUV1));
                if (starRand1 > _StarThreshold1)
                {
                    float starBrightness = (starRand1 - _StarThreshold1) / (1.0 - _StarThreshold1);
                    finalColor.rgb += _StarColor1.rgb * starBrightness;
                }

                // 4. Near Stars Layer (Fastest Motion + Twinkle)
                float2 starUV2 = (uv + baseMotion * _StarParallax2) * _StarScale2;
                float2 gridID2 = floor(starUV2);
                float starRand2 = hash21(gridID2);

                if (starRand2 > _StarThreshold2)
                {
                    float starBrightness = (starRand2 - _StarThreshold2) / (1.0 - _StarThreshold2);
                    // Add procedural twinkle over time based on cell ID
                    float twinkle = sin(_Time.y * _TwinkleSpeed + starRand2 * 6.28) * 0.5 + 0.5;
                    finalColor.rgb += _StarColor2.rgb * starBrightness * twinkle;
                }

                finalColor.a = 1.0;
                return finalColor * IN.color;
            }
            ENDCG
        }
    }
}
