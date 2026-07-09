Shader "Custom/ShadowComposite"
{
    Properties
    {
        _ShadowMask ("Shadow Mask", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0,0,0,0.5)
    }

    SubShader
    {
        Tags { "Queue"="Transparent+100" }

        Pass
        {
            ZWrite Off
            ZTest LEqual
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _ShadowMask;
            float4 _ShadowColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float mask = tex2D(_ShadowMask, i.uv).a;

                return float4(
                    _ShadowColor.rgb,
                    _ShadowColor.a * mask
                );
            }
            ENDHLSL
        }
    }
}
