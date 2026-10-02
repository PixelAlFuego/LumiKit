# LK-03 — Glow / Inner Glow Shader 2D
Estado: 🟠 en curso (Sesión 12, 2026-10-02). Revisor: APROBADO CON CAMBIOS, 9 aplicados. Usuario: sin demo, violeta, borde de luz en los huecos · Depende de: LK-09, LK-49, LK-01 (`EffectAssetBuilder`, estructura de `SH_Outline2D`), LK-24 (TAB) · Diseño: GDD §3.2 (línea 659), §1.3 (líneas 51-52), §1.7 (línea 156), D-001, D-005, D-010

## Objetivo
Brillo exterior e interior alrededor de la silueta del sprite, con intensidad y pulso opcional, en HLSL para URP (D-010). Objeto de prueba: `SPR_RuneCoin`
(la runa del GDD línea 156). Los dos brillos a la vez, sin modo: la runa tiene huecos y contorno irregular y aprovecha los dos.
**La demo no cambia** (usuario, 2026-10-02): en `02_Demo_2D` la runa sigue con contorno. Sólo shader y assets, probados en TestBench.

## Archivos
- `Assets/LumiKit/Shaders/2D/SH_Glow2D.shader` (planeado) · `Shader "LumiKit/2D/Glow"`. Estructura de `SH_Outline2D` (LK-01): pases `Universal2D` y
  `UniversalForward`, `Core2D.hlsl`, `UnityFlipSprite`, color = vértice × `_Color` × `unity_SpriteColor`, CBUFFER en `HLSLINCLUDE`, `positionWS` en
  Varyings, sólo las `multi_compile` de URP. La conversión mundo → UV se copia de `SH_Outline2D`: un include común obligaría a tocar un ✅.
- `Assets/Editor/EffectAssetBuilder.cs` · añade el menú `LumiKit/Efectos/Generar Glow 2D (LK-03)` y sus tres rutas; el código existente no cambia.
  Mismo comportamiento que Outline: actualiza en su sitio y conserva el GUID; aborta si falta el shader; sin diálogos (MCP).
- `Assets/LumiKit/Materials/2D/MAT_Glow2D_Default.mat` y `Assets/LumiKit/Runtime/Data/Effects/EFF_Glow2D.asset` (planeados, generados).
- Sin tocar: `Core/`, `Utils/`, `Demo/`, `UI/`, `Systems/`, `SH_Outline2D.shader`, `EFF_Outline2D`, `DemoSceneBuilder.cs`, `Assets/LumiKit/Scenes/`,
  TestBench guardada (el montaje de prueba no se guarda), `ProjectSettings/`.

## Contrato — propiedades de `SH_Glow2D`
El nombre de cada propiedad coincide exactamente con el `propertyName` del `EffectParameter`. Sin keywords (`.claude/rules/shaders.md`).

| Propiedad | Declaración | ParameterType | Rango / valores | Defecto | Origen |
|---|---|---|---|---|---|
| `_EffectEnabled` | `Float` | — (no va al panel) | 0 / 1 | 1 | **D-005** |
| `_GlowColor` | `[HDR] Color` | Color | — | Lumi Violet `#8B5CF6` (GDD línea 411; usuario, 2026-10-02) | GDD línea 51 |
| `_GlowIntensity` | `Range(0, 5)` | Float | 0 – 5 (propuesta) | 1,5 (propuesta) | GDD línea 659: sólo "intensidad" |
| `_PulseEnabled` | `[ToggleUI] Float` | Boolean | 0 / 1 | 0 | GDD línea 52 |
| `_PulseSpeed` | `Range(0, 3)` | Float | 0 – 3 pulsos por segundo (propuesta; ≤ 3 destellos/s, WCAG 2.3.1) | 1 (propuesta) | GDD línea 659: sólo "pulso" |
| `_MainTex`, `_Color` | como `Sprite-Unlit-Default` | — | — | — | URP |

`EFF_Glow2D` (`_shader` = `SH_Glow2D`), textos ES / EN. Efecto: "Brillo 2D" / "Glow 2D"; descripción: "Brillo interior y exterior alrededor de la
silueta, con pulso opcional." / "Inner and outer glow around the silhouette, with optional pulse." Parámetros, en este orden: "Color del brillo" /
"Glow color"; "Intensidad" / "Intensity"; "Pulso" / "Pulse"; "Velocidad del pulso" / "Pulse speed". `_EffectEnabled` no genera widget: lo conduce LK-24.

**Máscara.** Alfa difuminado `B`: media ponderada del alfa en 24 direcciones × 4 anillos hasta `GLOW_RADIUS` = 0,2 u, más el centro (97 muestras,
`SAMPLE_TEXTURE2D_LOD` a 0: los sprites no tienen mipmaps). Mundo → UV con la matriz 2×2 de LK-01. Anillos alternos girados media dirección; peso
decreciente con el radio. UV fuera de [0, 1] cuenta como alfa 0. `a` = alfa de la **textura** en `i.uv` (el `centerAlpha` de LK-01), no el del sprite
teñido: con el alfa del `SpriteRenderer` < 1, el halo no se ve a través del cuerpo. Exterior = `B` × saturate(2 (1 − `B`)) × (1 − `a`): **borde de luz**
(usuario): con `B` ≤ 0,5, fuera junto a la silueta, no cambia; en un hueco pequeño rodeado (`B` → 1) el centro se apaga y brilla su borde.
Interior = (1 − `B`) × `a` × 0,5 (`INNER_GLOW_SCALE`: los detalles siguen legibles). Fuerza = `_GlowIntensity` × pulso × `validDeterminant` (|det| casi 0 apaga los dos brillos, sin NaN).
Composición: el interior suma `_GlowColor.rgb` × saturate(interior × fuerza) al sprite; el exterior es una capa bajo el sprite con alfa
saturate(exterior × fuerza) × `_GlowColor.a` × alfa de vértice, mezclada como el contorno de LK-01. Final: `lerp(sprite, conBrillo, _EffectEnabled)`.
Si se ven escalones en el halo, se suben anillos o direcciones (constantes), sin cambiar el contrato.
**Pulso.** `_PulseEnabled` = 1: pulso = lerp(0,3, 1, 0,5 + 0,5 · cos(2π · `_PulseSpeed` · `_Time.y`)); 0: pulso = 1. Velocidad 0 = brillo fijo al máximo.
La fase depende de `_Time.y`: mover la velocidad con el pulso activo hace saltar la fase mientras se arrastra. Aceptado (un shader no guarda estado).
**Sin bloom.** TestBench no tiene Volume y su cámara tiene el post apagado (LK-01), igual que `02_Demo_2D` (LK-14): el HDR se recorta a 1. La intensidad cambia alcance y opacidad del halo.
**Requisitos del sprite** (LK-26): los de LK-01 (Single, Full Rect, sin atlas, Simple) y margen transparente ≥ `GLOW_RADIUS` = 0,2 × PPU texels
(103 a PPU 512). `SPR_RuneCoin` tiene 135 o más. Con menos margen, el halo se corta en el borde de la textura.
**Coste.** 97 lecturas de textura por píxel del rectángulo del sprite (LK-01: 49). Aceptado para la demo de escritorio; optimizar, fuera de alcance.

## Banco de pruebas
`Assets/_Development/TestBench.unity`, montaje del usuario a mano y **sin guardar**: en `SPR_RuneCoin`, material `MAT_Glow2D_Default` y `_definition` del
`EffectController` = `EFF_Glow2D`. `SPR_Crystal` sigue con `EFF_Debug` (el panel cambia de efecto al cambiar de selección). TAB de LK-24 (`ComparisonToggle`,
ya guardado en TestBench) para `_EffectEnabled`. El color del `EffectDebugTester` apunta a `_OutlineColor`: con el brillo avisaría; no se usa.
Al terminar, volver a `MAT_Outline2D_Default` y `EFF_Outline2D`, o cerrar sin guardar. `SPR_Lumi` no está (LK-20): no bloquea.

## Criterios de aceptación
**Verificador (MCP).** El shader compila sin errores ni warnings y declara `_EffectEnabled`. El menú de LK-03 genera los dos assets y relanzarlo no cambia
sus GUID. `EFF_Glow2D` tiene los cuatro `propertyName` exactos y el material usa `LumiKit/2D/Glow`. `_PulseEnabled` con `[ToggleUI]`; sin `[Toggle]`,
`[KeywordEnum]` ni `shader_feature`; `multi_compile`, sólo las de URP. `02_Demo_2D.unity` y `DemoSceneBuilder.cs` sin cambios en git.
`grep -rniE "coplay|mcpforunity" Assets/LumiKit/` da 0. Sin montaje no hay Play ni captura: van al ojo del usuario.

**Usuario (ojo), en TestBench con el montaje.**
- [ ] Play: consola sin avisos de LK-49. Runa con halo violeta suave alrededor; sus huecos, con borde de luz y centro más tenue; sin corte en el borde de la textura ni anillos marcados.
- [ ] Intensidad 0 quita el brillo; 5 lo hace más ancho y opaco. Cambiar el color cambia halo e interior.
- [ ] Pulso: el toggle lo arranca y lo detiene; la velocidad lo acelera; a 0, fijo. Mientras se arrastra la velocidad, el pulso puede saltar (aceptado).
- [ ] TAB con la runa seleccionada: sin brillo mientras se mantiene. Clic en el cristal: su panel; en la runa: "Brillo 2D". Reset devuelve el brillo por defecto.
- [ ] Zoom con la rueda: el halo escala con el sprite. En Play, rotar la runa 45° en Z, Flip X y alfa del `SpriteRenderer` a 0,5: halo uniforme y sin verse a través del cuerpo.
- [ ] Stop: ningún `.mat` modificado en `git status` (D-001). Deshacer el montaje.

## Fuera de alcance
- Bloom del volumen URP. Presets (LK-33). Versión Shader Graph y `SUB_FresnelGlow` (aparcados, D-010). Radio del halo como parámetro.
- Brillo en detalles internos de color (sólo bordes de alfa). Movimiento "flotante" de la runa. Iluminación 2D. Glow en `02_Demo_2D` (decisión del usuario).
