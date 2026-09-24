# LK-22b — Fuentes TMP del pack y sus licencias
Estado: ⬜ alcance pendiente de aprobación (Sesión 07) · Depende de: LK-22a (pie y botón ya construidos), LK-11a/LK-11b (los seis prefabs) · Diseño: GDD §2.4 (líneas 435-458), árbol de carpetas (líneas 931-938) · Estilo: `.claude/rules/ui-style.md` · uGUI: D-007

## Objetivo
Sustituir LiberationSans —la fuente de los TMP Essential Resources, que hoy pinta todo el HUD— por las tres familias del GDD, horneadas en los prefabs del pack. Cierra la Fase 3.
Claude no puede crear un `TMP_FontAsset`: el Font Asset Creator es una ventana del editor. El usuario descarga los `.ttf` y genera los cuatro `.asset`; Claude cambia el generador para consumirlos y aborta si falta alguno, igual que aborta hoy sin los TMP Essential Resources.
Las tres son SIL OFL 1.1, que permite redistribuirlas dentro de un producto comercial (GDD línea 445); sus condiciones se cumplen aquí, no al exportar.

## Archivos
- `Assets/LumiKit/Fonts/` (existe, vacía) · cuatro `TMP_FontAsset` (planeados) con los nombres literales del GDD líneas 932-936: `SpaceGrotesk-Medium SDF.asset`, `Inter-Regular SDF.asset`, `Inter-Medium SDF.asset`, `JetBrainsMono-Regular SDF.asset`. **Los genera el usuario.**
- `Assets/LumiKit/Fonts/Licenses/` (existe, vacía) · `OFL-SpaceGrotesk.txt`, `OFL-Inter.txt`, `OFL-JetBrainsMono.txt` (planeados). El GDD dibuja un único `OFL.txt`; son tres porque cada familia trae su línea de copyright y esa línea no se fusiona ni se reescribe.
- `Assets/LumiKit/Fonts/Source/` (planeada) · los `.ttf` originales, que **sí entran en el pack**: desviación del árbol del GDD decidida por el usuario y anotada en **D-009**. No pesan en el build, sólo en el `.unitypackage`.
- `Assets/Editor/ParameterPanelBuilder.cs` (existe) · **no aditiva**: `CreateText` gana un parámetro de familia y las ~10 llamadas existentes lo pasan. Único punto que toca líneas ya escritas, y es una línea por llamada.
- `Assets/Editor/FontFeatureCleaner.cs` (existe) · limpiador de features fuera del atlas. Herramienta de desarrollo, fuera del pack (duda abierta en STATE, LK-27).
- `.claude/rules/ui-style.md` (existe) · una celda: la tabla de tipografía pasa a apuntar a `Assets/LumiKit/Fonts/`, sin subir de las 60 líneas en que LK-22a la deja. Regenera (D-002): los seis `PRF_*` de `Prefabs/UI/`; cambian todos, porque todos llevan texto.
- No se tocan: `Core/`, `Utils/`, `Demo/`, los scripts de `UI/`, `Assets/TextMesh Pro/` ni `TMP Settings.asset` — la fuente por defecto del proyecto se queda como está; el pack no la impone.

## Contrato
**Dónde vive la referencia.** En los prefabs, horneada por el generador con `AssetDatabase.LoadAssetAtPath` sobre `TMP_Text.font`: `LumiTheme` es estática y no serializa assets, y `Resources.Load` no entra en el pack. En runtime no hay ni una búsqueda de fuente.

**`ParameterPanelBuilder`**
1. Cuatro constantes de ruta, una por `.asset`, bajo `Assets/LumiKit/Fonts/`.
2. `RequireFonts()` al principio de `GeneratePrefabs`, junto a `HasDefaultFont()`: si falta alguna, error **nombrando las rutas que faltan**, diálogo remitiendo a esta spec, y aborta sin escribir nada.
3. `CreateText(nombre, padre, familia, tamaño, color, alineación)`. `familia` es un enum local del editor (`Display`, `Label`, `Mono`), no un `string`: una familia mal escrita debe ser error de compilación, no un texto invisible.

| Texto | Familia | Tamaño |
|---|---|---|
| Cabecera del panel (nombre del efecto) | SpaceGrotesk-Medium | `TEXT_H2` 20 |
| Etiqueta de widget, opción de enum, botón del pie | Inter-Medium | `TEXT_LABEL` 16 |
| Valor del slider, letras R/G/B del color | JetBrainsMono-Regular | `TEXT_MONO` 16 |

`Inter-Regular` se genera pero **hoy no lo usa ningún texto del panel**: es el nivel Body del GDD (línea 455) y entra con descripciones y toasts (LK-25, LK-31, LK-34). Se genera ahora para no volver a abrir el Font Asset Creator; su criterio es que importe, no que se vea.

**Licencias.** OFL 1.1 §2 permite empaquetar, incrustar, redistribuir y **vender** las fuentes junto al software. Aplican tres condiciones: (a) redistribuir el texto íntegro de la licencia con su línea de copyright — por eso los tres `.txt`; (b) no venderlas por sí solas, cosa que LumiKit no hace; (c) §5, una versión modificada no puede usar el *Reserved Font Name* si la licencia declara uno, y un atlas SDF es un cambio de formato que cae en esa definición.
Por eso, **antes de generar, mirar la primera línea de cada `OFL.txt`**: si dice `with Reserved Font Name`, ese `.asset` se renombra sin el nombre de la familia; si no, se deja el del GDD. Lo comprueba el usuario. Resumen por familia → LK-26.

## Banco de pruebas
`Assets/_Development/TestBench.unity`. Antes de que Claude toque nada, el usuario:

| Paso | Qué | Dónde |
|---|---|---|
| 1 | Descargar los `.ttf` **estáticos**, no los variables, y dejarlos en `Fonts/Source/` (D-009): `SpaceGrotesk-Medium`, `Inter-Regular`, `Inter-Medium`, `JetBrainsMono-Regular`. Google Fonts entrega estáticos; los repos de Inter y Space Grotesk entregan además variables, que el Font Asset Creator no hornea bien | fuera de Unity |
| 2 | Guardar el `OFL.txt` de cada familia y mirar su línea de copyright | `Fonts/Licenses/` |
| 3 | Importar los `.ttf` y generar un `.asset` por cada uno | `Window > TextMeshPro > Font Asset Creator` |
| 4 | Guardar los cuatro `.asset` con los nombres del GDD | `Assets/LumiKit/Fonts/` |
| 5 | Borrar los seis `PRF_*` y el objeto `ParameterPanel` de la escena, y ejecutar los dos menús `LumiKit/UI/…` | después de que Claude programe |

Ajustes del Font Asset Creator, **los mismos cuatro veces**: un ratio padding/tamaño distinto entre fuentes da grosores de trazo distintos en la misma línea.

| Ajuste | Valor |
|---|---|
| Sampling Point Size | Custom Size **70** (rango sano en latino 70-90; 70 cabe en un atlas de 1024) |
| Padding | **7** — ratio del 10 %, el mismo en las cuatro |
| Packing Method | Optimum · Atlas Resolution **1024 × 1024** |
| Character Set | Custom Range: `32-126,160-255,8211-8212,8216-8221,8226,8230` — ASCII, el latín-1 del español (ñ á é í ó ú ü ¿ ¡) y los signos tipográficos |
| Render Mode | **SDFAA** · Get Kerning Pairs ✔ |

Si sale *atlas too small*, subir a 1024 × 2048 antes que bajar el Padding: bajarlo rompe el ratio. Tras guardar, en el Inspector de cada `.asset`: `Scale = 1` (un 0.9 heredado del import descuadra todos los tamaños en px de `LumiTheme`) y `Atlas Population Mode = Static`.
**Obligatorio tras cada regeneración**, en este orden: generar atlas → clic derecho en el `.asset` > `Import Font Features` → `LumiKit > Fuentes > Limpiar features fuera del atlas (LK-22b)`. El creador se deja pares (Inter-Medium: AV y To); el limpiador salta las fuentes que no son `Static`.
Más de 5 MB tras limpiar: **aviso de revisión, no fallo**. Se comprueba que el peso sea de pares útiles (Inter-Medium: 5,4 MB con 7.779 pares, aceptado en la Sesión 10).
Pesos de referencia: los de `ls`. El "Disco antes" del informe mide el archivo en disco, e `Import Font Features` no guarda al importar: Inter-Regular salió 4,40 → 4,40 MB habiendo quitado 479.403 registros.

## Criterios de aceptación (verificables en el editor)
- [ ] Compila sin errores ni warnings nuevos.
- [ ] Ejecutar el menú de generación con una fuente fuera de `Fonts/`: error nombrando la ruta que falta y **ningún prefab escrito** (`git status` limpio en `Prefabs/UI/`).
- [ ] Con las cuatro en su sitio, los seis prefabs se generan y el HUD ya no usa LiberationSans.
- [ ] Play + clic en `SPR_Crystal`: la cabecera se lee en Space Grotesk, las etiquetas en Inter y el valor del slider en JetBrains Mono, las tres distinguibles entre sí.
- [ ] Arrastrar un slider de 0 a 10: el valor no baila de ancho ni salta de sitio (Mono con ancho fijo 56).
- [ ] Una etiqueta con acento y con ñ (`DisplayNameEs` de `EFF_Debug`, p. ej. "Difuminado pequeño") se ve entera, sin cuadrados ni huecos, y la consola no avisa de glifos ausentes.
- [ ] Con un TextMeshPro temporal en TestBench, que no se guarda en la escena, cada una de las cuatro fuentes muestra `Áéíóú Ññ ¿¡ «» — … AV To Wa 0123` sin glifos vacíos y con el kerning visible en AV, To y Wa, salvo JetBrains Mono: es monoespaciada y no tiene kerning.
- [ ] Ningún texto sale rosa ni invisible. Los cuatro `.asset` con `Scale = 1` y `Atlas Population Mode = Static`, `Fonts/Source/` con los cuatro `.ttf` y `Fonts/Licenses/` con los tres `.txt`.
- [ ] El nombre de cada `.asset` respeta lo que diga su línea de copyright sobre el Reserved Font Name.
- [ ] Tras salir de Play, `git status` no muestra cambios en `MAT_Debug.mat` (D-001).

## Fuera de alcance
- Logotipo "LumiKit" (GDD línea 373) y el nivel Display a 42 px: son del splash y del menú → LK-13. Por eso no se genera `SpaceGrotesk-Bold`, que el árbol del GDD tampoco lista.
- Iconografía Lucide / Tabler (GDD línea 473): no es una fuente TMP, es otra tarea. Fallbacks dinámicos, CJK y localización: el pack es ES/EN → LK-19.
- `Documentation/LICENSE.txt` con el resumen de licencias del pack → LK-26, junto a las de audio y a las dependencias de Input System y TextMesh Pro. Ahí se documenta también que el comprador seguirá necesitando los TMP Essential Resources por el shader y los ajustes de TMP, aunque las fuentes vengan dentro.
- Tocar `Core`, `Demo`, los scripts de `UI/`, `TMP Settings.asset` o el `EffectDebugTester`.
