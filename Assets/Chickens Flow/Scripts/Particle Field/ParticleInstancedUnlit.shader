// Shader tối giản, tự khai báo rõ ràng hỗ trợ GPU instancing (KHÔNG dựa vào built-in shader nào có
// hỗ trợ sẵn hay không — "Sprites/Default" đặt enableInstancing=true không báo lỗi nhưng có vẻ
// không thực sự có biến thể instanced, nên Graphics.DrawMeshInstanced không vẽ ra gì).
Shader "MyGame/ParticleInstancedUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" "IgnoreProjector"="True" }
        LOD 100
        Cull Off

        Pass
        {
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            // half chứ không phải fixed: tích t.rgb * 2 * _Color chạm mức 2.0, ngoài dải tin cậy
            // của fixed (~[-2,2] với độ chính xác 1/256).
            half4 _Color;
            half _Cutoff;

            // Cạnh lưới atlas biến thể hạt. Phải khớp ParticleField.GrainGrid.
            #define GRAIN_GRID 4.0

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);

                // _MainTex là atlas GRAIN_GRID x GRAIN_GRID biến thể hạt (bóng khối + vết xước).
                // ParticleRenderer.MakeInstance nhét chỉ số biến thể vào ô m22 của ma trận instance
                // dưới dạng 1..16 — ô đó trống vì game 2D có mọi vertex z = 0, nên cả cột thứ ba
                // của ma trận bị nhân với 0. Nhờ vậy không tốn thêm byte per-instance nào.
                float vid = unity_ObjectToWorld._m22 - 1.0;
                float row = floor(vid / GRAIN_GRID);
                float col = vid - row * GRAIN_GRID;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex) / GRAIN_GRID + float2(col, row) / GRAIN_GRID;
                return o;
            }

            // t.r lưu hệ số nhân độ sáng đã chia cho thang này (RGBA32 chỉ chứa [0..1] nhưng vùng
            // sáng cần vượt 1.0). Phải khớp ParticleRenderer.ShadeEncodeScale.
            #define SHADE_DECODE 2.0

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                half4 t = tex2D(_MainTex, i.uv);
                half a = t.a * _Color.a;
                clip(a - _Cutoff);
                // Trung bình hệ số trên toàn atlas được chuẩn hoá về đúng 1.0, nên hạt trung bình
                // ra chính xác màu palette; chỗ tối/sáng lệch quanh nó.
                half3 baseCol = t.r * (SHADE_DECODE * _Color.rgb);
                // Chấm bóng specular (t.g) pha THẲNG về trắng, không đi qua phép nhân màu ở trên.
                // Bắt buộc phải tách riêng: nếu gộp vào phép nhân R thì highlight chỉ có thể sáng
                // hơn CHÍNH _Color chứ không bao giờ ngả trắng được (nhân đồng đều giữ nguyên hue),
                // khiến hạt trông đục như nhựa mờ thay vì bóng như thuỷ tinh.
                half3 col = lerp(baseCol, half3(1, 1, 1), t.g);
                return half4(col, a);
            }
            ENDCG
        }
    }
}
