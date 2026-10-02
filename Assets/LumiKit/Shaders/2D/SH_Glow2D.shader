// LK-03 — Brillo exterior e interior sobre sprite (D-010: HLSL para URP; spec docs/specs/LK-03_Glow2D.md).
// Base: SH_Outline2D (LK-01): mismos pases, mismo color de sprite y misma conversión mundo → UV.
// Sin keywords para los parámetros (.claude/rules/shaders.md): Boolean viaja como Float por
// MaterialPropertyBlock (D-001). Termina en lerp(sprite, conBrillo, _EffectEnabled) (D-005).
Shader "LumiKit/2D/Glow"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}

        // Lumi Violet #8B5CF6 (GDD línea 411).
        [HDR] _GlowColor ("Glow Color", Color) = (0.545, 0.361, 0.965, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.5
        [ToggleUI] _PulseEnabled ("Pulse Enabled", Float) = 0
        // Pulsos por segundo. Tope 3: no más de 3 destellos por segundo (WCAG 2.3.1).
        _PulseSpeed ("Pulse Speed (per second)", Range(0, 3)) = 1
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

        // Muestras del brillo: GLOW_DIRECTIONS direcciones en GLOW_RINGS anillos, más el centro.
        // Si se ven escalones en el halo, se suben; no cambian el contrato.
        #define GLOW_DIRECTIONS 24
        #define GLOW_RINGS 4
        // Radio del halo en unidades de mundo. El sprite necesita al menos este margen transparente.
        #define GLOW_RADIUS 0.2
        // El brillo interior, a la mitad: los detalles del sprite siguen legibles.
        #define INNER_GLOW_SCALE 0.5
        // Valle del pulso: el brillo baja hasta esta fracción y vuelve.
        #define PULSE_MIN 0.3
        // Por debajo de este determinante la conversión mundo → UV no es fiable: sin brillo.
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
            // Fuera de DEBUG_DISPLAY: hace falta para pasar el radio de mundo a UV.
            float3  positionWS  : TEXCOORD1;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        // Sin #ifdef dentro: el SRP Batcher no admite layouts distintos.
        CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half4 _GlowColor;
            float _GlowIntensity;
            float _PulseEnabled;
            float _PulseSpeed;
            float _EffectEnabled;
        CBUFFER_END

        Varyings GlowVertex(Attributes v)
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
        // de la textura se estiraría hacia fuera. LOD 0: los sprites no tienen mipmaps y el halo ya difumina.
        float SampleAlpha(float2 uv)
        {
            float2 inside = step(0.0, uv) * step(uv, 1.0);
            return SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, uv, 0).a * inside.x * inside.y;
        }

        // 1 sin pulso. Con pulso, oscila entre PULSE_MIN y 1; con velocidad 0 se queda en 1.
        // La fase depende de _Time.y: mover la velocidad con el pulso activo la hace saltar (aceptado).
        float Pulse()
        {
            float wave = 0.5 + 0.5 * cos(TWO_PI * _PulseSpeed * _Time.y);
            return _PulseEnabled > 0.5 ? lerp(PULSE_MIN, 1.0, wave) : 1.0;
        }

        half4 GlowFragment(Varyings i) : SV_Target
        {
            half4 sprite = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

            // Mundo → UV con la matriz 2×2 de SH_Outline2D (LK-01):
            // M = [ddx(uv) ddy(uv)] · inversa([ddx(ws.xy) ddy(ws.xy)]).
            // Vale con escala, rotación, flip y zoom. Supone el sprite en el plano XY.
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

            // Alfa de la textura, sin el tinte: con el alfa del SpriteRenderer < 1, el halo
            // exterior no se ve a través del cuerpo.
            float centerAlpha = SampleAlpha(i.uv);

            // Alfa difuminado: media ponderada alrededor, con más peso cerca. Los anillos impares
            // giran media dirección para que las muestras no se alineen en rayos.
            float weightedSum = centerAlpha;
            float totalWeight = 1.0;

            UNITY_UNROLL
            for (int ring = 1; ring <= GLOW_RINGS; ring++)
            {
                float ringRadius = GLOW_RADIUS * ring / GLOW_RINGS;
                float ringWeight = 1.0 - (ring - 0.5) / GLOW_RINGS;
                float angleOffset = (ring % 2) * 0.5;

                UNITY_UNROLL
                for (int k = 0; k < GLOW_DIRECTIONS; k++)
                {
                    float2 direction;
                    sincos((k + angleOffset) * (TWO_PI / GLOW_DIRECTIONS), direction.y, direction.x);
                    float2 offset = mul(uvPerWorld, direction * ringRadius);
                    weightedSum += SampleAlpha(i.uv + offset) * ringWeight;
                    totalWeight += ringWeight;
                }
            }

            float blurred = weightedSum / totalWeight;
            // validDeterminant apaga los dos brillos donde la conversión no es fiable.
            float strength = _GlowIntensity * Pulse() * validDeterminant;

            // Exterior, fuera de la silueta. saturate(2 (1 − B)) es el borde de luz: en un hueco pequeño
            // rodeado de sprite (B → 1) apaga el centro; junto al borde exterior (B ≤ 0,5) no cambia nada.
            float outer = blurred * saturate(2.0 * (1.0 - blurred)) * (1.0 - centerAlpha);
            // Interior, dentro de la silueta, junto a sus bordes y huecos.
            float inner = (1.0 - blurred) * centerAlpha * INNER_GLOW_SCALE;

            float innerAmount = saturate(inner * strength) * _GlowColor.a;
            float outerAlpha = saturate(outer * strength) * _GlowColor.a * i.color.a;

            // El interior suma el color del brillo al sprite.
            float3 lit = sprite.rgb + _GlowColor.rgb * innerAmount;

            // El exterior es una capa bajo el sprite, como el contorno de LK-01.
            float alpha = sprite.a + outerAlpha * (1.0 - sprite.a);
            float3 rgb = (lit * sprite.a + _GlowColor.rgb * outerAlpha * (1.0 - sprite.a)) / max(alpha, 1e-5);
            half4 withGlow = half4(rgb, alpha);

            return lerp(sprite, withGlow, _EffectEnabled);
        }
        ENDHLSL

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex GlowVertex
            #pragma fragment GlowFragment
            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue" = "Transparent" "RenderType" = "Transparent" }

            HLSLPROGRAM
            #pragma vertex GlowVertex
            #pragma fragment GlowFragment
            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE
            ENDHLSL
        }
    }
}
