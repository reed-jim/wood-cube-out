Shader "Custom/InstancedShadow"
{
    Properties
    {
        _MainTex ("Shadow Texture", 2D) = "white" {}
        _Color ("Shadow Color", Color) = (0,0,0,0.5)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Cutoff;

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);

                // Texture alpha controls shadow transparency
                float alpha = tex.a * _Color.a;

                // Remove almost-transparent pixels
                clip(alpha - _Cutoff);

                return fixed4(
                    _Color.rgb,
                    alpha
                );
            }

            ENDCG
        }
    }
}