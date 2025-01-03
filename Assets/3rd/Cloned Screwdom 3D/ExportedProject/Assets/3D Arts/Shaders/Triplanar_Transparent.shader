Shader "KIM/3D/Triplanar Transparent" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Blend ("Blend", Float) = 1
		[Header(Lighting)] _HSV ("Shadow Setting", Vector) = (0,1,0.5,10)
		[Header(Specular)] _SpecularColor ("Specular Color", Vector) = (1,1,1,0.5)
		_Alpha ("Alpha", Range(0, 1)) = 0.5
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}