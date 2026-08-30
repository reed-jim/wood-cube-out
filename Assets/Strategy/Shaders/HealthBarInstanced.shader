Shader "Custom/InstancedHealthBar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FillColor ("Health Color", Color) = (0, 0.9, 0.2, 1) // Green
        _DelayColor ("Lazy Catchup Color", Color) = (1, 0.3, 0, 1) // Orange/Red
        _BgColor ("Background Color", Color) = (0.1, 0.1, 0.1, 0.8) // Dark Grey Transparency
        _BorderWidth ("Border Width", Range(0, 0.1)) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing // Enables GPU Instancing support

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Allows instanced access in frag shader
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            fixed4 _FillColor;
            fixed4 _DelayColor;
            fixed4 _BgColor;
            float _BorderWidth;

            // Define Per-Instance Shader Variables
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _HealthPercent) // Visual (Catch-up) HP [0..1]
                UNITY_DEFINE_INSTANCED_PROP(float, _ActualPercent) // Immediate target HP [0..1]
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                // Read per-instance properties uploaded by MaterialPropertyBlock
                float actualHP = UNITY_ACCESS_INSTANCED_PROP(Props, _ActualPercent);
                float visualHP = UNITY_ACCESS_INSTANCED_PROP(Props, _HealthPercent);

                float2 uv = i.uv;

                // 1. Render Dark Border Edge
                if (uv.x < _BorderWidth || uv.x > (1.0 - _BorderWidth) || 
                    uv.y < _BorderWidth || uv.y > (1.0 - _BorderWidth))
                {
                    return float4(0.0, 0.0, 0.0, 0.9); // Black Border
                }

                // 2. Render Green Actual HP
                if (uv.x <= actualHP)
                {
                    return _FillColor;
                }

                // 3. Render Orange/Red "Lazy" Lag Area (between Actual HP and Catchup Visual HP)
                if (uv.x <= visualHP)
                {
                    return _DelayColor;
                }

                // 4. Render Empty Background Slot
                return _BgColor;
            }
            ENDCG
        }
    }
}