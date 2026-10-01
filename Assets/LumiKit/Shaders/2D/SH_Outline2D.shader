// LK-01 — Contorno 2D sobre sprite (D-010: HLSL para URP; spec docs/specs/LK-01_Outline2D.md).
// Base: Sprite-Unlit-Default de URP 17.0.4, mismos pases y mismo color de sprite, sin DEBUG_DISPLAY.
// Sin keywords para los parámetros (.claude/rules/shaders.md): Boolean y Enum viajan como Float por
// MaterialPropertyBlock (D-001). Termina en lerp(sprite, conContorno, _EffectEnabled) (D-005).
Shader "LumiKit/2D/Outline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}

        // Lumi Cyan #00E5D4 (GDD línea 409).
        [HDR] _OutlineColor ("Outline Color", Color) = (0, 0.898, 0.831, 1)
        // Centésimas de unidad de mundo: 1 = 0,01 u = 1 px de un sprite a PPU 100.
        _OutlineWidth ("Outline Width (0.01 u)", Range(0, 10)) = 4
        [Enum(Solid,0,Dotted,1,Animated,2)] _OutlineMode ("Outline Mode", Float) = 0
        _EffectEnabled ("Effect Enabled", Float) = 1

        // Legado de Sprite-Unlit-Default, por el mismo motivo: fallback al shader de sprite clásico.
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        // Código común a los dos pases: así el CBUFFER tiene el mismo orden en ambos (SRP Batcher).
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

        // Muestras del contorno: OUTLINE_DIRECTIONS direcciones en dos anillos, r y r/2.
        #define OUTLINE_DIRECTIONS 24
        // _OutlineWidth está en centésimas de unidad de mundo.
        #define WIDTH_TO_WORLD 0.01
        // Trazos de los modos Punteado y Animado, por vuelta completa alrededor del centro de la UV.
        #define DASH_COUNT 24.0
        // Modo Animado: trazos que avanzan por segundo.
        #define DASH_SPEED 1.5
        // Por debajo de este determinante la conversión mundo → UV no es fiable: sin contorno.
        #define MIN_DETERMINANT 1e-12

        struct Attributes
        {
            float3 positionOS   : POSITION;
            float4 color        : COLOR;
            float2 uv           : TEXCOORD0;
            UNITY_SKINNED_VERTEX_INPUTS
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4  positionCS  : SV_POSITION;
            half4   color       : COLOR;
            float2  uv          : TEXCOORD0;
            // Fuera de DEBUG_DISPLAY: hace falta para pasar el grosor de mundo a UV.
            float3  positionWS  : TEXCOORD1;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        // Sin #ifdef dentro: el SRP Batcher no admite layouts distintos.
        CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half4 _OutlineColor;
            float _OutlineWidth;
            float _OutlineMode;
            float _EffectEnabled;
        CBUFFER_END

        Varyings OutlineVertex(Attributes v)
        {
            Varyings o = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            UNITY_SKINNED_VERTEX_COMPUTE(v);

            SetUpSpriteInstanceProperties();
            v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
            o.positionCS = TransformObjectToHClip(v.positionOS);
            o.positionWS = TransformObjectToWorld(v.positionOS);
            o.uv = v.uv;
            o.color = v.color * _Color * unity_SpriteColor;
            return o;
        }

        // Alfa de la textura en uv. Fuera de [0, 1] cuenta como transparente: con Wrap Clamp, el borde
        // de la textura se estiraría hacia fuera. Gradiente explícito: mismo mip que el píxel propio.
        float SampleAlpha(float2 uv, float2 dUVdx, float2 dUVdy)
        {
            float2 inside = step(0.0, uv) * step(uv, 1.0);
            return SAMPLE_TEXTURE2D_GRAD(_MainTex, sampler_MainTex, uv, dUVdx, dUVdy).a * inside.x * inside.y;
        }

        // Trazos por ángulo polar alrededor del centro de la UV. En modo Animado avanzan con el tiempo.
        float DashMask(float2 uv)
        {
            float2 fromCenter = uv - 0.5;
            float turn = atan2(fromCenter.y, fromCenter.x) / TWO_PI + 0.5;
            float scroll = _OutlineMode > 1.5 ? _Time.y * DASH_SPEED : 0.0;
            // DASH_COUNT es entero: frac no salta donde atan2 da la vuelta.
            return step(0.5, frac(turn * DASH_COUNT - scroll));
        }

        half4 OutlineFragment(Varyings i) : SV_Target
        {
            half4 sprite = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

            // Mundo → UV con la matriz 2×2 completa de derivadas de pantalla:
            // M = [ddx(uv) ddy(uv)] · inversa([ddx(ws.xy) ddy(ws.xy)]).
            // Vale con sprites no cuadrados, escala no uniforme, rotación, flip y batching, y no
            // depende de la resolución de la textura. Supone el sprite en el plano XY.
            float2 dUVdx = ddx(i.uv);
            float2 dUVdy = ddy(i.uv);
            float2 dWSdx = ddx(i.positionWS.xy);
            float2 dWSdy = ddy(i.positionWS.xy);

            float determinant = dWSdx.x * dWSdy.y - dWSdy.x * dWSdx.y;
            float validDeterminant = abs(determinant) > MIN_DETERMINANT ? 1.0 : 0.0;
            float safeDeterminant = validDeterminant > 0.5 ? determinant : 1.0;

            float2x2 uvPerScreen = float2x2(dUVdx.x, dUVdy.x, dUVdx.y, dUVdy.y);
            float2x2 screenPerWorld = float2x2(dWSdy.y, -dWSdy.x, -dWSdx.y, dWSdx.x) / safeDeterminant;
            float2x2 uvPerWorld = mul(uvPerScreen, screenPerWorld);

            float radius = _OutlineWidth * WIDTH_TO_WORLD;
            float centerAlpha = SampleAlpha(i.uv, dUVdx, dUVdy);
            float dilated = centerAlpha;

            UNITY_UNROLL
            for (int k = 0; k < OUTLINE_DIRECTIONS; k++)
            {
                float2 direction;
                sincos(k * (TWO_PI / OUTLINE_DIRECTIONS), direction.y, direction.x);
                float2 offset = mul(uvPerWorld, direction * radius);
                dilated = max(dilated, SampleAlpha(i.uv + offset, dUVdx, dUVdy));
                dilated = max(dilated, SampleAlpha(i.uv + offset * 0.5, dUVdx, dUVdy));
            }

            // Capa del contorno: la silueta dilatada. Grosor 0 no tiñe el borde suavizado del sprite.
            float outlineAlpha = dilated * _OutlineColor.a * i.color.a * validDeterminant;
            outlineAlpha *= _OutlineWidth > 0.0 ? 1.0 : 0.0;
            outlineAlpha *= _OutlineMode > 0.5 ? DashMask(i.uv) : 1.0;

            // El sprite se pinta encima del contorno: lo visible del contorno es dilatado × (1 − alfa propio).
            float alpha = sprite.a + outlineAlpha * (1.0 - sprite.a);
            float3 rgb = (sprite.rgb * sprite.a + _OutlineColor.rgb * outlineAlpha * (1.0 - sprite.a)) / max(alpha, 1e-5);
            half4 withOutline = half4(rgb, alpha);

            return lerp(sprite, withOutline, _EffectEnabled);
        }
        ENDHLSL

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment
            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue" = "Transparent" "RenderType" = "Transparent" }

            HLSLPROGRAM
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment
            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE
            ENDHLSL
        }
    }
}
