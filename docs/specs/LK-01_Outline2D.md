# LK-01 — Outline Shader 2D
Estado: 🟡 implementado, pendiente de verificar en el editor (Sesión 12). Plan aprobado por el usuario con 4 decisiones y el requisito de margen; revisor: APROBADO CON CAMBIOS, 9 aplicados · Depende de: LK-09, LK-49 · Diseño: GDD §3.2 (línea 657), §1.3 (líneas 48-53), §1.7 (línea 154), D-005, D-010

## Objetivo
Contorno configurable sobre sprite, con color, grosor y modo, en HLSL para URP (D-010). Objeto demo: Lumi (LK-20); mientras no llegue, `SPR_RuneCoin`.
El grosor no depende de la resolución de la textura: el mismo valor se ve igual con un sprite de 1024 px o de 64 px.

## Archivos
- `Assets/LumiKit/Shaders/2D/SH_Outline2D.shader` (planeado) · `Shader "LumiKit/2D/Outline"`. Estructura de `Sprite-Unlit-Default` de URP 17.0.4:
  pases `Universal2D` y `UniversalForward`, `Core2D.hlsl`, `UnityFlipSprite`, color = vértice × `_Color` × `unity_SpriteColor`. Sin `DEBUG_DISPLAY`.
  Toda propiedad en `CBUFFER_START(UnityPerMaterial)`, mismo orden en los dos pases. `positionWS` en Varyings, fuera de `#if DEBUG_DISPLAY`.
  Las únicas `multi_compile` son las de URP: instancing y `SKINNED_SPRITE`.
- `Assets/Editor/EffectAssetBuilder.cs` (planeado) · menú `LumiKit/Efectos/Generar Outline 2D (LK-01)`: crea los dos assets siguientes con
  `SerializedObject` (D-002); si ya existen, los actualiza en su sitio y conserva el GUID (TestBench y LK-14 los referencian). Aborta si falta
  el shader. Sin diálogos: lo lanza el verificador por MCP. LK-03 y LK-02 añaden aquí su menú.
- `Assets/LumiKit/Sprites/SPR_RuneCoin.png.meta` y `SPR_Crystal.png.meta` · cambiados por el usuario en el Inspector: Mesh Type a Full Rect,
  permanente (Sesión 12). La prueba de Max Size 256 se deshace después (valor actual: 2048).
- `Assets/LumiKit/Materials/2D/MAT_Outline2D_Default.mat` (planeado, generado) · `Assets/LumiKit/Runtime/Data/Effects/EFF_Outline2D.asset` (planeado, generado).
- Sin tocar: `Core/`, `Utils/` (salvo el permiso del criterio de Linear), `Demo/`, `UI/`, `Systems/`, `EffectDebugTester.cs`.

## Contrato — propiedades de `SH_Outline2D`
El nombre de cada propiedad coincide exactamente con el `propertyName` del `EffectParameter`. Sin keywords (`.claude/rules/shaders.md`).

| Propiedad | Declaración | ParameterType | Rango / valores | Defecto | Origen |
|---|---|---|---|---|---|
| `_EffectEnabled` | `Float` | — (no va al panel) | 0 / 1 | 1 | **D-005** |
| `_OutlineColor` | `[HDR] Color` | Color | — | Lumi Cyan `#00E5D4` (GDD línea 409) | GDD línea 657 |
| `_OutlineWidth` | `Range(0, 10)` | Float | 0 – 10 | 4 | GDD línea 50 |
| `_OutlineMode` | `[Enum(Solid,0,Dotted,1,Animated,2)] Float` | Enum | 0 sólido · 1 punteado · 2 animado | 0 | GDD línea 53 |
| `_MainTex`, `_Color` | como `Sprite-Unlit-Default` | — | — | — | URP |

`EFF_Outline2D` (`_shader` = `SH_Outline2D`), textos ES / EN. Efecto: "Contorno 2D" / "Outline 2D"; descripción: "Contorno de color alrededor de la
silueta del sprite." / "Colored outline around the sprite silhouette." Parámetros: "Color del contorno" / "Outline color"; "Grosor" / "Width";
"Modo" / "Mode", con `_enumOptions` = Sólido, Punteado, Animado (una sola lista; la UI muestra el ES). `_EffectEnabled` no genera widget: lo conduce LK-24.

**Grosor en unidades de mundo** (usuario, Sesión 12). `_OutlineWidth` se mide en centésimas de unidad: 1 = 0,01 u = 1 px de un sprite a PPU 100, el valor por
defecto de Unity. En el fragment, cada dirección en mundo pasa a UV con la matriz 2×2 completa
`M = [ddx(uv) ddy(uv)] · inversa([ddx(ws.xy) ddy(ws.xy)])`, no con un cociente escalar: así vale con sprites no cuadrados y escala no uniforme.
Supone el sprite en el plano XY. Si |det| es casi 0, el contorno se apaga en ese fragmento (sin NaN).
No se usa `_MainTex_TexelSize`: el grosor no cambia con la resolución ni con el PPU, y aguanta rotación, escala, flip y batching. Escala con el zoom, como el sprite.

**Máscara.** 24 direcciones × 2 anillos (r y r/2) = 48 muestras: máximo alfa vecino × (1 − alfa propio). Una muestra con UV fuera de [0, 1] cuenta
como alfa 0. Con grosor 10, una punta más fina que ~13 texels puede dar un borde ondulado: se acepta.
El sprite se pinta encima del contorno; el alfa del contorno se multiplica por el del color de vértice. Final: `lerp(sprite, conContorno, _EffectEnabled)`.
**Modos.** Punteado: la máscara × 24 trazos fijos por ángulo polar alrededor del centro de la UV. Animado: esos trazos avanzan con `_Time.y`, velocidad
fija, como un marcador de selección (usuario, Sesión 12; el pulso de brillo es de LK-03 y no se duplica).
**Requisitos del sprite, documentados en el pack (LK-26):** Sprite Mode Single; Mesh Type = Full Rect; sin Sprite Atlas; Draw Mode Simple.
Margen transparente ≥ 0,1 u = 0,1 × PPU texels (52 a PPU 512, 26 a PPU 256): con Full Rect el contorno sólo se dibuja dentro del margen de la
textura y se corta si lo supera. `SPR_Crystal` y `SPR_RuneCoin`: Full Rect y 135 texels o más de margen.

## Banco de pruebas
`Assets/_Development/TestBench.unity`, sin Volume y con el post de la cámara apagado. El usuario: en el marcador `SPR_RuneCoin`, material
`MAT_Outline2D_Default` y `EffectController` con `EFF_Outline2D`. `SPR_Crystal` sigue con `EFF_Debug` (control negativo de LK-49); sólo para el
criterio del margen se le pone el mismo material y se deshace. `SetEffectEnabled` se dispara con
`EffectDebugTester` (en "Cube de Prueba"), que tiene `_controller` fijo: el usuario lo apunta al `EffectController` de `SPR_RuneCoin` y pone
`_colorProperty` = `_OutlineColor`. Diferido a LK-20: los valores por defecto se ajustan cuando llegue Lumi (el generador actualiza en su sitio).

## Criterios de aceptación
**Verificador (MCP).** El shader compila sin errores ni warnings y declara `_EffectEnabled` (D-005; LK-49 no lo comprueba). El menú genera
los dos assets, y relanzarlo no cambia sus GUID. `EFF_Outline2D` tiene los tres `propertyName` exactos y el material usa `LumiKit/2D/Outline`.
En Play con el marcador, LK-49 no avisa de propiedades ausentes.
Sin `[Toggle]`, `[KeywordEnum]` ni `shader_feature`; las `multi_compile`, sólo las de URP. `git status` sin `.mat` modificado tras Play (D-001).

**Usuario (ojo).**
- [ ] Contorno Lumi Cyan alrededor de la silueta, sin cortes; grosor 0 lo quita y 10 lo engrosa.
- [ ] Margen: con el grosor al máximo, el contorno de `SPR_Crystal` y de `SPR_RuneCoin` no se corta en el borde de la textura.
- [ ] Resolución: con Max Size 256 en el import del sprite, el grosor en pantalla no cambia (el sprite se ve más borroso). Volver a 2048.
- [ ] Zoom con la rueda: el contorno escala con el sprite. Rotar el marcador 45°, activar Flip X o escalarlo a X = 2: el contorno sigue uniforme.
- [ ] Punteado muestra trazos; Animado, trazos que avanzan. Sólido vuelve al contorno continuo.
- [ ] **Diferido desde LK-09:** `SetEffectEnabled(false)` deja el sprite sin contorno; `true` lo devuelve.
- [ ] **Diferido desde LK-09:** color en Linear, medido. Condiciones: post de la cámara apagado, ningún Volume activo, modo Sólido, grosor alto.
      Un píxel del centro del trazo (no del borde suavizado) en una captura de `Temp/Captures` da `#00E5D4` ±2 por canal. Si falla, la
      corrección va sólo en `MaterialPropertyHelper.ConvertColor`, con permiso del usuario ya dado (Sesión 12) para esa función y nada más de `Utils/`.

## Fuera de alcance
- Presets `PRE_Outline_*` (LK-33). Versión Shader Graph (aparcada, D-010). Comparación con TAB (LK-24).
- Soporte de Sprite Atlas y de mallas Tight. Contorno de selección del GDD (línea 42). Iluminación 2D (sprite unlit).
