Shader "BOXOPHOBIC/Polyverse Skies/Standard" {
	Properties {
		[StyledBanner(Polyverse Skies Standard)] _Banner ("< Banner >", Float) = 1
		[StyledCategory(Background, 5, 10)] _BackgroundCat ("[ Background Cat ]", Float) = 1
		[KeywordEnum(Colors,Cubemap,Combined)] _BackgroundMode ("Background Mode", Float) = 0
		[Space(10)] _SkyColor ("Sky Color", Vector) = (0.4980392,0.7450981,1,1)
		_EquatorColor ("Equator Color", Vector) = (1,0.747,0,1)
		_GroundColor ("Ground Color", Vector) = (0.4980392,0.497,0,1)
		_EquatorHeight ("Equator Height", Range(0, 1)) = 0.5
		_EquatorSmoothness ("Equator Smoothness", Range(0.01, 1)) = 0.5
		[Space(10)] [StyledTextureSingleLine] [NoScaleOffset] _BackgroundCubemap ("Background Cubemap", Cube) = "black" {}
		[Space(10)] _BackgroundExposure ("Background Exposure", Range(0, 8)) = 1
		[StyledCategory(Pattern)] _PatternCat ("[ Pattern Cat ]", Float) = 1
		[Toggle(_ENABLEPATTERNOVERLAY_ON)] _EnablePatternOverlay ("Enable Pattern Overlay", Float) = 0
		[Space(10)] [StyledTextureSingleLine] [NoScaleOffset] _PatternCubemap ("Pattern Cubemap", Cube) = "black" {}
		[Space(10)] _PatternContrast ("Pattern Contrast", Range(0, 1)) = 0.5
		[StyledCategory(Stars)] _StarsCat ("[ Stars Cat ]", Float) = 1
		[Toggle(_ENABLESTARS_ON)] _EnableStars ("Enable Stars", Float) = 0
		[Space(10)] [StyledTextureSingleLine] [NoScaleOffset] _StarsCubemap ("Stars Cubemap", Cube) = "white" {}
		[Space(10)] _StarsIntensity ("Stars Intensity", Range(0, 5)) = 2
		[IntRange] _StarsLayer ("Stars Layers", Range(1, 3)) = 2
		_StarsSize ("Stars Size", Range(0, 0.99)) = 0.5
		_StarsSunMask ("Stars Sun Mask", Range(0, 1)) = 0
		_StarsHeightMask ("Stars Height Mask", Range(0, 1)) = 0
		[StyledToggle] _StarsBottomMask ("Stars Bottom Mask", Float) = 0
		[Space(10)] [Toggle(_ENABLESTARSTWINKLING_ON)] _EnableStarsTwinkling ("Enable Stars Twinkling", Float) = 0
		[Space(10)] _TwinklingContrast ("Twinkling Contrast", Range(0, 2)) = 1
		_TwinklingSpeed ("Twinkling Speed", Float) = 0.05
		[Space(10)] [Toggle(_ENABLESTARSROTATION_ON)] _EnableStarsRotation ("Enable Stars Rotation", Float) = 0
		[IntRange] [Space(10)] _StarsRotation ("Stars Rotation", Range(0, 360)) = 360
		[StyledVector(18)] _StarsRotationAxis ("Stars Rotation Axis", Vector) = (0,1,0,0)
		_StarsRotationSpeed ("Stars Rotation Speed", Float) = 0.5
		[StyledCategory(Sun)] _SunCat ("[ Sun Cat ]", Float) = 1
		[Toggle(_ENABLESUN_ON)] _EnableSun ("Enable Sun", Float) = 0
		[Space(10)] [StyledTextureSingleLine] [NoScaleOffset] _SunTexture ("Sun Texture", 2D) = "black" {}
		[Space(10)] _SunColor ("Sun Color", Vector) = (1,1,1,1)
		_SunSize ("Sun Size", Range(0.1, 1)) = 0.5
		_SunIntensity ("Sun Intensity", Range(0, 10)) = 1
		[StyledCategory(Moon)] _MoonCat ("[ Moon Cat ]", Float) = 1
		[Toggle(_ENABLEMOON_ON)] _EnableMoon ("Enable Moon", Float) = 0
		[Space(10)] [StyledTextureSingleLine] [NoScaleOffset] _MoonTexture ("Moon Texture", 2D) = "black" {}
		[Space(10)] _MoonColor ("Moon Color", Vector) = (1,1,1,1)
		_MoonSize ("Moon Size", Range(0.1, 1)) = 0.5
		_MoonIntensity ("Moon Intensity", Range(0, 10)) = 1
		[StyledCategory(Clouds)] _CloudsCat ("[ Clouds Cat ]", Float) = 1
		[Toggle(_ENABLECLOUDS_ON)] _EnableClouds ("Enable Clouds", Float) = 0
		[Space(10)] [StyledTextureSingleLine] [NoScaleOffset] _CloudsCubemap ("Clouds Cubemap", Cube) = "black" {}
		[Space(10)] _CloudsHeight ("Clouds Height", Range(-0.5, 0.5)) = 0
		_CloudsLightColor ("Clouds Light Color", Vector) = (1,1,1,1)
		_CloudsShadowColor ("Clouds Shadow Color", Vector) = (0.4980392,0.7450981,1,1)
		[Space(10)] [Toggle(_ENABLECLOUDSROTATION_ON)] _EnableCloudsRotation ("Enable Clouds Rotation", Float) = 0
		[Space(10)] _CloudsRotation ("Clouds Rotation", Range(0, 360)) = 1
		[StyledVector(18)] _CloudsRotationAxis ("Clouds Rotation Axis", Vector) = (0,1,0,0)
		_CloudsRotationSpeed ("Clouds Rotation Speed", Float) = 0.5
		[StyledCategory(Fog)] _FogCat ("[ Fog Cat ]", Float) = 1
		[Toggle(_ENABLEBUILTINFOG_ON)] _EnableBuiltinFog ("Enable Fog", Float) = 0
		[StyledMessage(Info, The fog color is controlled by the fog color set in the Lighting panel., _EnableBuiltinFog, 1, 10, 0)] _EnableFogMessage ("EnableFogMessage", Float) = 0
		[Space(10)] _FogHeight ("Fog Height", Range(0, 1)) = 0
		_FogSmoothness ("Fog Smoothness", Range(0.01, 1)) = 0
		_FogFill ("Fog Fill", Range(0, 1)) = 0
		[StyledCategory(Skybox Settings)] _SkyboxCat ("[ Skybox Cat ]", Float) = 1
		_SkyboxOffset ("Skybox Offset", Range(-1, 1)) = 0
		_SkyboxRotation ("Skybox Rotation", Range(0, 360)) = 0
		[StyledVector(18)] _SkyboxRotationAxis ("Skybox Rotation Axis", Vector) = (0,1,0,0)
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
	Fallback "False"
	//CustomEditor "PolyverseSkiesShaderGUI"
}