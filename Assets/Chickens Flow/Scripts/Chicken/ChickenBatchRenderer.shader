Shader "Instanced/ChickenSpriteAtlas"
{
    Properties
    {
        _MainTex ("Chicken Atlas", 2D) = "white" {}
        _Columns ("Atlas Columns", Float) = 4
        _Rows ("Atlas Rows", Float) = 4
        _FPS ("Animation FPS", Float) = 8
        _Frames ("Frame Count", Float) = 16
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"


            sampler2D _MainTex;

            float _Columns;
            float _Rows;
            float _FPS;
            float _Frames;


            UNITY_INSTANCING_BUFFER_START(Props)

            // x = animation speed offset
            // y = skin/frame start offset
            UNITY_DEFINE_INSTANCED_PROP(float4,_AnimData)

            UNITY_INSTANCING_BUFFER_END(Props)



            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);

                v2f o;


                // billboard toward camera
                float3 worldPos =
                    mul(unity_ObjectToWorld,
                    float4(v.vertex.xyz,1)).xyz;


                float3 camRight =
                    UNITY_MATRIX_V[0].xyz;

                float3 camUp =
                    UNITY_MATRIX_V[1].xyz;


                float3 center =
                    unity_ObjectToWorld._m03_m13_m23;


                float3 size =
                    float3(1,1,1);


                worldPos =
                    center +
                    camRight * v.vertex.x * size.x +
                    camUp    * v.vertex.y * size.y;


                o.pos =
                    UnityWorldToClipPos(worldPos);



                float offset =
                    UNITY_ACCESS_INSTANCED_PROP(
                        Props,
                        _AnimData).x;


                float frame =
                    floor(
                        frac(
                            _Time.y * _FPS + offset
                        )
                        * _Frames
                    );


                float column =
                    fmod(frame,_Columns);


                float row =
                    floor(frame/_Columns);


                o.uv =
                    v.uv;

                o.uv.x =
                    (o.uv.x + column) / _Columns;

                o.uv.y =
                    (o.uv.y + row) / _Rows;


                return o;
            }



            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col =
                    tex2D(_MainTex,i.uv);


                clip(col.a - 0.01);

                return col;
            }

            ENDCG
        }
    }
}