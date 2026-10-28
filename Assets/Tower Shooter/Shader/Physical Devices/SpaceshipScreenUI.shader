// ============================================================================
//  UI/SpaceshipScreen  -  Unity URP, HLSL, for a UI Image (no sprite / no texture)
//
//  Reference: modern flagship displays (iPad Pro / Tesla center screen):
//    - machined titanium-style frame with chamfered edges + brushed finish
//    - ultra-thin black glass bezel
//    - flat true-black OLED panel with a soft glass reflection
//
//  Layout (all sizes are in "height units": 1.0 = full Image height)
//    [ outer chamfered frame ] -> [ black glass bezel ] -> [ screen ]
//    Bottom frame ("chin") is thicker and contains a recessed socket for your
//    power-button Image. A thin status ring around the socket glows
//    (breathing amber when off, steady cyan when on).
//
//  _Power drives the screen:
//    0        : dark glass (off)
//    0..0.18  : bright horizontal line grows from the center
//    0.16-0.56: line opens vertically into the boot panel
//    0.62-1.0 : a scan bar sweeps top->bottom, revealing transparency
//    1        : screen area is fully transparent (alpha 0) -> your gameplay
//               image/RawImage placed BEHIND this Image shows through.
//
//  Setup: Image -> Image Type "Simple", Source Image "None", Material = this
//  shader. Draw order: gameplay (behind)  ->  this Image (in front).
//  Place your power-button Image over the socket (see _BtnCenter / _BtnHalfSize).
// ============================================================================
Shader "UI/SpaceshipScreen"
{
    Properties
    {
        [Header(State)]
        _Power ("Power (0 = off, 1 = on / transparent)", Range(0, 1)) = 0
        _Aspect ("Aspect W/H (0 = auto detect)", Float) = 0

        [Header(Layout in height units)]
        _OuterRadius ("Outer corner radius", Range(0, 0.5)) = 0.07
        _FrameWidth ("Frame width (sides and top)", Range(0.004, 0.08)) = 0.022
        _Bezel ("Black glass bezel width", Range(0.002, 0.05)) = 0.016
        _ChinHeight ("Bottom frame height (button area)", Range(0.02, 0.3)) = 0.12
        _ScreenRadius ("Screen corner radius", Range(0, 0.2)) = 0.035
        _Chamfer ("Frame chamfer width", Range(0.001, 0.02)) = 0.007

        [Header(Power button socket)]
        _BtnCenter ("Socket center (UV)", Vector) = (0.5, 0.06, 0, 0)
        _BtnHalfSize ("Well half size (height units)", Vector) = (0.04, 0.04, 0, 0)
        _BtnCorner ("Well corner radius", Range(0, 0.1)) = 0.04
        _BtnRim ("Recessed rim width", Range(0.002, 0.03)) = 0.01

        [Header(Frame material)]
        _FrameColor ("Frame base color", Color) = (0.50, 0.51, 0.54, 1)
        _EnvTop ("Studio light top", Color) = (0.90, 0.95, 1.00, 1)
        _EnvBottom ("Studio light bottom", Color) = (0.02, 0.02, 0.03, 1)
        _Shininess ("Edge specular sharpness", Range(5, 200)) = 60
        _Brushed ("Brushed amount", Range(0, 1)) = 0.35
        _BrushScale ("Brushed line density", Float) = 700

        [Header(Glass and screen)]
        _BezelColor ("Bezel glass color", Color) = (0.008, 0.008, 0.010, 1)
        _ScreenOffColor ("Screen off color", Color) = (0.010, 0.012, 0.016, 1)
        _GlareStrength ("Glass reflection (screen off)", Range(0, 1)) = 0.25
        _OnGlare ("Glass reflection (screen on)", Range(0, 1)) = 0.0
        _AccentColor ("Boot accent color", Color) = (0.10, 0.80, 1.00, 1)
        _ScanDensity ("Scanline density", Float) = 1200

        [Header(Status ring)]
        _StatusOffColor ("Status ring off", Color) = (1.0, 0.40, 0.08, 1)
        _StatusOnColor ("Status ring on", Color) = (0.10, 0.85, 1.0, 1)

        // UI system properties
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "RenderPipeline" = "UniversalPipeline"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha     // premultiplied alpha output
        ColorMask [_ColorMask]

        Pass
        {
            Name "SpaceshipScreenUI"

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Power;
                float _Aspect;
                float _OuterRadius;
                float _FrameWidth;
                float _Bezel;
                float _ChinHeight;
                float _ScreenRadius;
                float _Chamfer;
                float4 _BtnCenter;
                float4 _BtnHalfSize;
                float _BtnCorner;
                float _BtnRim;
                float4 _FrameColor;
                float4 _EnvTop;
                float4 _EnvBottom;
                float _Shininess;
                float _Brushed;
                float _BrushScale;
                float4 _BezelColor;
                float4 _ScreenOffColor;
                float _GlareStrength;
                float _OnGlare;
                float4 _AccentColor;
                float _ScanDensity;
                float4 _StatusOffColor;
                float4 _StatusOnColor;
                float4 _ClipRect;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float4 worldPos   : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.worldPos = IN.positionOS;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            // ------------------------------------------------------------------
            //  Helpers
            // ------------------------------------------------------------------
            float pow2(float x) { return x * x; }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Signed distance to a rounded box (negative inside)
            float sdRoundBox(float2 p, float2 h, float r)
            {
                float2 q = abs(p) - h + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            // Analytic gradient (outward direction) of the rounded box SDF
            float2 gradRoundBox(float2 p, float2 h, float r)
            {
                float2 s = float2(p.x >= 0.0 ? 1.0 : -1.0, p.y >= 0.0 ? 1.0 : -1.0);
                float2 q = abs(p) - h + r;
                float2 g;
                if (q.x > 0.0 || q.y > 0.0)
                    g = normalize(max(q, 0.0));
                else
                    g = (q.x > q.y) ? float2(1, 0) : float2(0, 1);
                return g * s;
            }

            float Cov(float d, float aa) { return saturate(0.5 - d / aa); }
            // 1 when x < edge, smooth transition of width aa
            float Band(float x, float edge, float aa) { return 1.0 - smoothstep(edge - aa, edge + aa, x); }

            float3 EnvMap(float3 r)
            {
                float sky = smoothstep(-0.8, 0.9, r.y);
                float3 env = lerp(_EnvBottom.rgb, _EnvTop.rgb, sky);
                float box1 = exp(-pow2((r.x + 0.5) * 2.5)) * smoothstep(0.2, 0.8, r.y);
                float box2 = exp(-pow2((r.x - 0.7) * 3.0)) * smoothstep(-0.2, 0.5, r.y) * 0.6;
                return env + (box1 + box2) * float3(1.0, 1.0, 1.05);
            }

            float3 ShadeMetal(float3 n, float3 baseCol)
            {
                float3 V = float3(0, 0, 1);
                float3 L = normalize(float3(-0.5, 0.6, 0.8));
                float3 H = normalize(L + V);
                float3 r = reflect(-V, n);
                float spec = pow(saturate(dot(n, H)), _Shininess);
                return baseCol * (0.15 + EnvMap(r) * 1.2) + spec * 0.6;
            }

            // Soft diagonal glass reflection bands
            float Glare(float2 p)
            {
                float u = dot(p, normalize(float2(0.8, 0.6)));
                float b1 = exp(-pow2((u - 0.10) / 0.10)) * 0.5;
                float b2 = exp(-pow2((u + 0.30) / 0.025)) * 0.6;
                float b3 = exp(-pow2((u - 0.45) / 0.015)) * 0.3;
                return saturate(b1 + b2 + b3);
            }

            // Boot panel: grid, scanlines, loader rings, HUD brackets
            float3 BootContent(float2 ps, float2 sh, float t)
            {
                float2 s = ps / sh;
                float minH = min(sh.x, sh.y);
                float2 q = ps / minH;
                float r = length(q);
                float3 acc = _AccentColor.rgb;
                float2 sv = s * float2(0.75, 0.95);
                float vign = saturate(1.0 - dot(sv, sv) * 0.6);

                float3 col = float3(0.0, 0.012, 0.025) + acc * 0.05 * vign;

                // grid
                float2 g = ps * 28.0;
                float2 gd = abs(frac(g - 0.5) - 0.5) / max(fwidth(g), 1e-4);
                float grid = 1.0 - saturate(min(gd.x, gd.y));
                col += acc * grid * 0.10 * vign;

                // loader rings
                float ang = atan2(q.y, q.x) / 6.2831853 + 0.5;
                float prog = smoothstep(0.22, 0.60, t);
                float ring1 = 1.0 - smoothstep(0.006, 0.012, abs(r - 0.42));
                ring1 *= lerp(0.15, 1.0, step(ang, prog));
                float dash = step(0.5, frac(ang * 36.0 - _Time.y * 0.5));
                float ring2 = (1.0 - smoothstep(0.003, 0.006, abs(r - 0.55))) * dash * 0.7;
                float core = exp(-r * r * 40.0) * (0.5 + 0.5 * sin(_Time.y * 6.0)) * 0.6;
                col += acc * (ring1 * 0.9 + ring2 * 0.6 + core);

                // HUD corner brackets
                float2 c = sh - 0.02 - abs(ps);
                float inX = step(0.0, c.x) * step(c.x, 0.006);
                float inY = step(0.0, c.y) * step(c.y, 0.006);
                float brk = inX * step(0.0, c.y) * step(c.y, 0.07) + inY * step(0.0, c.x) * step(c.x, 0.07);
                col += acc * saturate(brk) * 0.8;

                // rolling bar, scanlines, flicker
                float roll = exp(-pow2((frac(-s.y * 0.5 + _Time.y * 0.3) - 0.5) * 6.0)) * 0.08;
                col += acc * roll;
                float scan = 0.88 + 0.12 * sin(ps.y * _ScanDensity);
                float fl = 0.92 + 0.08 * hash21(float2(floor(_Time.y * 24.0), 1.7));
                return saturate(col * scan * fl);
            }

            float UIClip(float2 position, float4 clipRect)
            {
                float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
                return inside.x * inside.y;
            }

            // ------------------------------------------------------------------
            //  Fragment
            // ------------------------------------------------------------------
            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Aspect ratio of the Image rect (derivatives of a linear uv are exact)
                float autoAspect = abs(ddy(uv.y)) / max(abs(ddx(uv.x)), 1e-8);
                float aspect = clamp((_Aspect > 0.001) ? _Aspect : autoAspect, 0.25, 8.0);
                float aa = max(abs(ddx(uv.y)) + abs(ddy(uv.y)), 1e-5);

                // Centered coordinates in height units (y up, 1.0 = Image height)
                float2 p = (uv - 0.5) * float2(aspect, 1.0);
                float tt = saturate(_Power);

                // ---------------- shapes ----------------
                float2 oh = float2(aspect * 0.5, 0.5);
                float rO = min(_OuterRadius, min(oh.x, oh.y));
                float dO = sdRoundBox(p, oh, rO);

                float side = _FrameWidth + _Bezel;
                float topI = _FrameWidth + _Bezel;
                float botI = _ChinHeight + _Bezel;
                float2 sh = float2(max(oh.x - side, 0.02), max((1.0 - topI - botI) * 0.5, 0.02));
                float2 sc = float2(0.0, (botI - topI) * 0.5);
                float rS = min(_ScreenRadius, min(sh.x, sh.y));
                float dS = sdRoundBox(p - sc, sh, rS);
                float dB = sdRoundBox(p - sc, sh + _Bezel, rS + _Bezel);

                float2 bc = (_BtnCenter.xy - 0.5) * float2(aspect, 1.0);
                float2 bh = max(_BtnHalfSize.xy, 0.005);
                float rW = min(_BtnCorner, min(bh.x, bh.y));
                float dW = sdRoundBox(p - bc, bh, rW);
                float dR = sdRoundBox(p - bc, bh + _BtnRim, rW + _BtnRim);

                float mO = Cov(dO, aa);
                float mB = Cov(dB, aa);
                float mS = Cov(dS, aa);
                float mW = Cov(dW, aa);

                // ---------------- frame normal (chamfers + socket rim) ----------------
                float2 gO = gradRoundBox(p, oh, rO);
                float2 gB = gradRoundBox(p - sc, sh + _Bezel, rS + _Bezel);
                float2 gR = gradRoundBox(p - bc, bh + _BtnRim, rW + _BtnRim);

                float e = -dO;
                float tiltO = Band(e, _Chamfer, aa) * lerp(0.4, 1.3, saturate(1.0 - e / _Chamfer));
                float tiltB = Band(dB, _Chamfer * 0.7, aa) * (1.0 - Band(dB, 0.0, aa)) * 0.9;
                float tiltR = Band(dR, 0.0, aa) * (1.0 - Band(dW, 0.0, aa));

                float2 nxy = gO * tiltO - gB * tiltB - gR * tiltR;
                float3 n = normalize(float3(nxy, 1.0));

                // brushed finish: fine horizontal streaks perturb the normal
                float b = hash21(float2(floor(p.y * _BrushScale), floor(p.x * 8.0))) - 0.5;
                n = normalize(n + float3(0.0, b * _Brushed * 0.25, 0.0));

                float3 baseCol = _FrameColor.rgb * (0.85 + 0.3 * uv.y);
                float3 metal = ShadeMetal(n, baseCol);

                // contact shadows: frame next to bezel, rim next to the well
                float occl = lerp(0.45, 1.0, smoothstep(0.0, 0.006, dB));
                float ao = lerp(0.35, 1.0, smoothstep(0.0, 0.005, dW));
                metal *= occl * ao;

                // ---------------- housing ----------------
                float glare = Glare(p);
                float3 bezelRGB = _BezelColor.rgb + glare * _GlareStrength * 0.35;
                float3 wellRGB = float3(0.012, 0.012, 0.015);

                float3 housing = metal;
                housing = lerp(housing, bezelRGB, mB);
                housing = lerp(housing, wellRGB, mW);

                // status ring around the socket
                float onK = smoothstep(0.0, 0.12, tt);
                float3 ledCol = lerp(_StatusOffColor.rgb, _StatusOnColor.rgb, onK);
                float breathe = 0.55 + 0.45 * sin(_Time.y * 2.5);
                float ledK = lerp(breathe * 0.6, 1.0, onK);
                float ledLine = 1.0 - smoothstep(0.0, 0.0022, abs(dR + 0.0025));
                float ledGlow = exp(-abs(dR) / 0.014) * 0.25;
                housing += ledCol * (ledLine + ledGlow) * ledK * (1.0 - mW) * (1.0 - mB);

                float4 housingPM = float4(housing * mO, mO);

                // ---------------- screen ----------------
                float2 ps = p - sc;
                float2 s = ps / sh;
                float3 acc = _AccentColor.rgb;

                float tA = smoothstep(0.0, 0.18, tt);
                float tB = smoothstep(0.16, 0.56, tt);
                float tC = smoothstep(0.62, 1.0, tt);

                float3 interior = _ScreenOffColor.rgb;

                // A: horizontal line grows from the center
                float len = tA;
                float lineX = saturate((len - abs(s.x)) / max(0.2 * len, 1e-3));
                float dy = abs(ps.y);
                float lcore = 1.0 - smoothstep(0.0, 0.003 * (1.0 + tA), dy);
                float lglow = exp(-dy / 0.012);
                float lineI = lineX * (lcore * 1.5 + lglow * 0.6) * (1.0 - tB);

                // B: line opens vertically into the boot panel
                float openH = tB;
                float opened = (1.0 - smoothstep(openH, openH + 0.03, abs(s.y))) * saturate(tB * 40.0);
                float edgeG = exp(-abs(abs(s.y) - openH) / 0.025) * saturate(tB * 40.0)
                              * (1.0 - smoothstep(0.85, 1.0, tB));

                float3 boot = BootContent(ps, sh, tt);
                interior = lerp(interior, boot, opened);
                interior += float3(0.75, 0.95, 1.0) * lineI;
                interior += float3(0.70, 0.95, 1.0) * edgeG * 0.8;

                // C: scan bar sweeps top -> bottom, everything above becomes transparent
                float sweepY = lerp(1.25, -1.25, tC);
                float reveal = smoothstep(sweepY - 0.02, sweepY + 0.02, s.y);
                float intA = 1.0 - reveal;
                float4 scr = float4(interior * intA, intA);

                float barI = exp(-abs(s.y - sweepY) / 0.035) * saturate(tC * 30.0)
                             * (1.0 - smoothstep(0.94, 1.0, tC));
                float barA = saturate(barI);
                float3 barC = lerp(acc, float3(1, 1, 1), 0.6);
                scr = float4(barC * barA, barA) + scr * (1.0 - barA);

                // glass reflection on top of the screen
                float gl = glare * lerp(_GlareStrength, _OnGlare, tC);
                scr = float4(float3(gl, gl, gl), gl) + scr * (1.0 - gl);

                // ---------------- compose ----------------
                float4 col = housingPM * (1.0 - mS) + scr * mS;
                col *= IN.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                col *= UIClip(IN.worldPos.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}
