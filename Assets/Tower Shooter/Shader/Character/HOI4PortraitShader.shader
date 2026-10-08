Shader "UI/ProceduralCharacterPortrait"
{
    Properties
    {
        _SkinColor ("Skin", Color) = (0.72,0.48,0.34,1)
        _SkinLight ("Skin Light", Color) = (0.86,0.64,0.48,1)
        _HairColor ("Hair", Color) = (0.08,0.055,0.04,1)
        _HairLight ("Hair Light", Color) = (0.16,0.11,0.075,1)
        _UniformColor ("Uniform", Color) = (0.12,0.16,0.18,1)
        _UniformLight ("Uniform Light", Color) = (0.22,0.25,0.25,1)
        _EyeColor ("Eyes", Color) = (0.08,0.11,0.10,1)
        _MouthColor ("Mouth", Color) = (0.32,0.12,0.11,1)
        _PortraitScale ("Portrait Scale", Range(0.8,1.2)) = 1
        _FaceWidth ("Face Width", Range(0.7,1.3)) = 1
        _HairAmount ("Hair Amount", Range(0,1)) = 0.65
        _Gender ("Face Shape", Range(0,1)) = 0.5
        _Seed ("Character Seed", Range(0,100)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _SkinColor,_SkinLight,_HairColor,_HairLight,_UniformColor,_UniformLight,_EyeColor,_MouthColor;
            float _PortraitScale,_FaceWidth,_HairAmount,_Gender,_Seed;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex=UnityObjectToClipPos(v.vertex);
                o.uv=v.uv;
                return o;
            }

            float sdEllipse(float2 p,float2 r)
            {
                return length(p/r)-1.0;
            }

            float sdBox(float2 p,float2 b)
            {
                float2 d=abs(p)-b;
                return length(max(d,0))+min(max(d.x,d.y),0);
            }

            float shape(float d,float softness)
            {
                return 1.0-smoothstep(-softness,softness,d);
            }

            float hash(float n)
            {
                return frac(sin(n*127.1)*43758.5453);
            }

            fixed4 frag(v2f i):SV_Target
            {
                float2 uv=i.uv;
                float2 p=uv*2.0-1.0;
                p.y*=1.05;
                p/=_PortraitScale;

                float seed=hash(_Seed*17.31);

                float3 col=float3(0,0,0);
                float alpha=0;

                // Shoulders / upper body
                float shoulderCurve=0.52+0.10*sin(p.x*3.2);
                float bodyMask=shape((abs(p.x)*0.82+pow(max(0,-p.y+0.18),2)*0.18)-shoulderCurve,0.015);
                bodyMask*=step(-0.98,p.y);
                float3 bodyCol=lerp(_UniformColor.rgb,_UniformLight.rgb,saturate(0.35-p.y*0.35));
                col=lerp(col,bodyCol,bodyMask);
                alpha=max(alpha,bodyMask);

                // Neck
                float2 neckP=p-float2(0,0.02);
                float neck=shape(sdBox(neckP,float2(0.16,0.30)),0.012);
                col=lerp(col,_SkinColor.rgb,neck);
                alpha=max(alpha,neck);

                // Uniform collar
                float2 lp=p-float2(-0.13,0.13);
                float2 rp=p-float2(0.13,0.13);
                float collarL=shape(sdBox(lp,float2(0.115,0.28)),0.008);
                float collarR=shape(sdBox(rp,float2(0.115,0.28)),0.008);
                float collar= max(collarL,collarR);
                float collarCut=shape(sdBox(p-float2(0,0.20),float2(0.055,0.20)),0.01);
                collar*=1-collarCut;
                col=lerp(col,_UniformLight.rgb,collar);
                alpha=max(alpha,collar);

                // Face
                float2 faceP=p-float2(0,0.38);
                float faceWidth=0.37*_FaceWidth;
                float faceHeight=0.48;
                float faceShape=sdEllipse(faceP,float2(faceWidth,faceHeight));
                float face=shape(faceShape,0.012);
                float chinCut=smoothstep(-0.10,0.22,faceP.y);
                face*=lerp(0.88,1.0,chinCut);

                float3 faceCol=lerp(_SkinColor.rgb,_SkinLight.rgb,saturate(0.5-faceP.y*0.75));
                col=lerp(col,faceCol,face);
                alpha=max(alpha,face);

                // Ear shapes
                float earL=shape(sdEllipse(p-float2(-faceWidth,0.38),float2(0.075,0.13)),0.01);
                float earR=shape(sdEllipse(p-float2(faceWidth,0.38),float2(0.075,0.13)),0.01);
                float ears=max(earL,earR);
                col=lerp(col,_SkinColor.rgb,ears);
                alpha=max(alpha,ears);

                // Hair mass
                float hairWidth=faceWidth+0.055;
                float hairTop=shape(sdEllipse(p-float2(0,0.66),float2(hairWidth,0.48)),0.015);
                float hairLower=smoothstep(0.55,0.25,p.y);
                float hair=hairTop*hairLower*_HairAmount;

                // Side hair
                float sideL=shape(sdBox(p-float2(-hairWidth+0.035,0.47),float2(0.085,0.34)),0.012);
                float sideR=shape(sdBox(p-float2(hairWidth-0.035,0.47),float2(0.085,0.34)),0.012);
                hair=max(hair,max(sideL,sideR)*_HairAmount);

                float3 hairCol=lerp(_HairColor.rgb,_HairLight.rgb,saturate(0.35+p.y*0.5));
                col=lerp(col,hairCol,hair);
                alpha=max(alpha,hair);

                // Hair highlight / painted strand
                float2 hp=p-float2(-0.08,0.78);
                float highlight=shape(sdEllipse(hp,float2(0.10,0.035)),0.015)*hair;
                col=lerp(col,_HairLight.rgb,highlight*0.65);

                // Eyebrows
                float browL=shape(sdBox(p-float2(-0.145,0.51),float2(0.085,0.018)),0.008);
                float browR=shape(sdBox(p-float2(0.145,0.51),float2(0.085,0.018)),0.008);
                float brows=max(browL,browR);
                col=lerp(col,_HairColor.rgb,brows);
                alpha=max(alpha,brows);

                // Eye whites
                float eyeL=shape(sdEllipse(p-float2(-0.145,0.445),float2(0.073,0.042)),0.012);
                float eyeR=shape(sdEllipse(p-float2(0.145,0.445),float2(0.073,0.042)),0.012);
                float eyes=max(eyeL,eyeR);
                col=lerp(col,float3(0.90,0.86,0.76),eyes);
                alpha=max(alpha,eyes);

                // Pupils
                float pupilL=shape(sdEllipse(p-float2(-0.145,0.445),float2(0.027,0.032)),0.008);
                float pupilR=shape(sdEllipse(p-float2(0.145,0.445),float2(0.027,0.032)),0.008);
                float pupils=max(pupilL,pupilR);
                col=lerp(col,_EyeColor.rgb,pupils);

                // Nose
                float2 noseP=p-float2(0,0.35);
                float nose=shape(sdEllipse(noseP,float2(0.055,0.105)),0.012);
                float noseShadow=shape(sdEllipse(noseP-float2(0.025,0),float2(0.035,0.085)),0.012);
                col=lerp(col,_SkinLight.rgb,nose*0.28);
                col=lerp(col,_SkinColor.rgb,noseShadow*0.18);

                // Mouth
                float2 mouthP=p-float2(0,0.205);
                float mouth=shape(sdEllipse(mouthP,float2(0.095,0.026)),0.009);
                col=lerp(col,_MouthColor.rgb,mouth);

                // Mouth highlight
                float lipHighlight=shape(sdEllipse(mouthP+float2(0,0.012),float2(0.070,0.009)),0.006);
                col=lerp(col,_SkinLight.rgb,lipHighlight*0.30);

                // Face shadow for painted illustration look
                float cheekL=shape(sdEllipse(p-float2(-0.25,0.31),float2(0.11,0.17)),0.02);
                float cheekR=shape(sdEllipse(p-float2(0.25,0.31),float2(0.11,0.17)),0.02);
                float cheek=max(cheekL,cheekR)*face;
                col=lerp(col,_SkinColor.rgb,cheek*0.15);

                // Uniform buttons
                float button1=shape(sdEllipse(p-float2(0,0.02),float2(0.018,0.018)),0.005);
                float button2=shape(sdEllipse(p-float2(0,-0.10),float2(0.018,0.018)),0.005);
                float buttons=max(button1,button2)*bodyMask;
                col=lerp(col,float3(0.65,0.58,0.38),buttons);

                // Shoulder insignia
                float insigniaL=shape(sdBox(p-float2(-0.37,0.00),float2(0.055,0.035)),0.008);
                float insigniaR=shape(sdBox(p-float2(0.37,0.00),float2(0.055,0.035)),0.008);
                float insignia=max(insigniaL,insigniaR)*bodyMask;
                col=lerp(col,float3(0.65,0.55,0.30),insignia);

                // Subtle painted edge shading
                float edge=saturate(abs(p.x)*0.65+max(0,0.42-p.y)*0.12);
                col*=1.0-edge*0.16;

                // Transparent outside character
                alpha=saturate(alpha);
                return fixed4(col,alpha);
            }
            ENDCG
        }
    }
}