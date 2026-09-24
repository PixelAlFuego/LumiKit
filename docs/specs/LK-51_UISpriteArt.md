# LK-51 — Arte de interfaz: sprites propios del pack
Estado: ✅ verificado en Unity 6000.0.83f1 (usuario, Sesión 10; plan aprobado por el usuario) · Tarea nueva, no una ampliación de LK-50 (decisión del usuario, Sesión 08) · Depende de: LK-22a · Independiente de LK-50: aquello es comportamiento, esto es arte · Diseño: GDD §2.6 (líneas 510-545), §2.7 (líneas 580-588), §2.8 (líneas 593-621) · Detalle de dibujo: `docs/reference/UI_ART_BRIEF.md`

## Objetivo
Quitar de la UI los dos sprites de Unity —`UI/Skin/UISprite.psd` y `UI/Skin/Knob.psd`, que el generador pone en once `Image` (diez por
`CreateImage` y la de la opción de enum)— y ponerle arte propio: esquinas redondeadas, cápsula, círculo y contorno. Es lo único que
`LumiTheme` no puede dar. Con el contorno se cierra además la deuda de LK-22a: el botón secundario con **relleno transparente** en reposo.
Los siete PNG ya están en disco (usuario, Sesión 09): los dos tiempos de la Sesión 08 se quedan en uno.

## Archivos
- `Assets/LumiKit/Sprites/UI/` (existe, la creó el usuario) · los siete `SPR_UI_*.png`. En sus `.meta`: Single, PPU 300, Full Rect, sin mipmaps, compresión None sin overrides por plataforma, tamaños y bordes del brief.
- `UI/LumiTheme.cs` (existe) · aditiva, ~5 líneas: `RADIUS_SMALL` (4, GDD líneas 601 y 621) y `Transparent` (ver Contrato).
- `Assets/Editor/ParameterPanelBuilder.cs` (existe) · **no aditiva**, ~28 líneas cambiadas y ~45 nuevas: siete rutas en lugar de `SPRITE_UI` y `SPRITE_KNOB`, `RequireSprites()`, `CreateImage` cargando del proyecto, las once líneas que pasan sprite, el `ColorBlock` y el multiplicador de `CreateButton`, y tres comentarios que dejan de ser ciertos.
- Regenera (D-002): los seis `PRF_*` de `Prefabs/UI/` y el `ParameterPanel` de `TestBench.unity`, como en LK-22a.
- No se tocan: `Core/`, `Utils/`, `Demo/`, los scripts de `UI/` salvo `LumiTheme`, `EffectDebugTester.cs`, ni `TMP Settings.asset`.

## Contrato
**Dónde vive la referencia.** Horneada en el prefab por el generador, con `AssetDatabase.LoadAssetAtPath` sobre `Image.sprite`. `LumiTheme`
es estática y no serializa un `Sprite`, así que las rutas van en el generador, como las fuentes de LK-22b. En runtime, ni una búsqueda (D-002).

**`RequireSprites()`** — al principio de `GeneratePrefabs`, tras `HasDefaultFont()` y antes de mirar los prefabs existentes. Carga cada
ruta como `Sprite`, así que un PNG importado como `Default` cuenta como ausente. Si falta alguno de los cinco obligatorios: un error en
consola **por cada ruta**, diálogo remitiendo a esta spec y **aborta sin escribir ni un prefab**. `SPR_UI_Track` es opcional: si no está,
el riel se queda macizo y sólo se avisa con un `Debug.Log`.

**Reparto de los sprites sobre las once `Image`:**

| Sprite | Dónde se usa | Sustituye a |
|---|---|---|
| `SPR_UI_Rect_R6` | relleno del botón del pie, fondo de opción de enum | `UISprite` |
| `SPR_UI_Rect_R6_Outline` | borde del botón del pie | `UISprite` |
| `SPR_UI_Rect_R4` | muestra de color y las seis muestras de paleta | `UISprite` |
| `SPR_UI_Pill` | pista del toggle | `UISprite` |
| `SPR_UI_Circle` | manija del toggle; manija del slider: círculo cian debajo y núcleo claro encima | `Knob` |
| `SPR_UI_Ring` | sin uso en el generador desde la Sesión 10 (hilo oscuro entre aro y núcleo). Pieza suelta del kit, sigue en el pack | — |
| `SPR_UI_Track` (opcional) | riel y relleno del slider | `UISprite` |

Los sprites son blancos con la forma en el alfa; el color lo sigue poniendo `Image.color` desde `LumiTheme` (D-007). Aparte del
relleno del botón en reposo (abajo), **si algo cambia de color en esta tarea, es un fallo**.

**Botón del pie.** Misma estructura que en LK-22a: raíz = contorno (`BorderStrong`), hijo insertado 1 px = relleno (`targetGraphic`). Dos cambios:
1. El relleno lleva `pixelsPerUnitMultiplier = RADIUS / (RADIUS − BUTTON_BORDER)` = 1.2: su radio baja de 6 a 5 y casa con el interior
   del contorno, sin cuña de fondo en las esquinas (`Image.cs` línea 727; radio = borde en texels / (PPU/100 × multiplicador)).
2. `ColorBlock`: `normal`, `selected` y `disabled` pasan de `Surface` a `LumiTheme.Transparent`; `highlighted` y `pressed` no cambian.
   `Selectable` tiñe también el alfa (`Selectable.cs` línea 1090). `Transparent` es `Surface` con alfa 0, no negro: el fundido a hover no oscurece.
No espera a LK-50: cuando llegue, su tabla hereda estos tokens.

## Banco de pruebas
`Assets/_Development/TestBench.unity`, con el HUD y el pie ya verificados en LK-22a.

| Paso | Qué | Quién |
|---|---|---|
| 1 | Renombrar en el Project `SPR_UI_Circle` y `SPR_UI_Track` (p. ej. `_x` al final), ejecutar `LumiKit/UI/Generar prefabs del panel` y devolverles el nombre. Desde el Project, no desde el explorador: el `.meta` viaja con el PNG | usuario |
| 2 | Borrar los seis `PRF_*` y el objeto `ParameterPanel` de la escena, y ejecutar los dos menús `LumiKit/UI/…` | usuario |
| 3 | Play, clic en `SPR_Crystal` y repasar los criterios con el Game view a 1920×1080 | usuario |

No se prueba generando sin `SPR_UI_Track`: el paso 1 sólo ve el aviso. Sprite nulo = `Image` maciza, como ya son `Surface` y `Separator`.

## Criterios de aceptación (verificables en el editor)
- [ ] Compila sin errores ni warnings nuevos.
- [ ] Paso 1: aborta, error nombrando `SPR_UI_Circle`, diálogo "Faltan 1 de los 5", sólo un aviso por `SPR_UI_Track`, y `git status` limpio en `Prefabs/UI/`.
- [ ] Paso 2: ningún `.prefab` apunta ya a los sprites de Unity: `grep -c f000000000000000` da 0 en los seis (hoy suma 28). El YAML no guarda el nombre `UISprite`: guarda el fileID, 10905 (`UISprite`) y 10913 (`Knob`).
- [ ] Play + clic en `SPR_Crystal`: opciones de enum y muestras con esquinas redondeadas, pista del toggle en cápsula, manijas redondas.
- [ ] Reset en reposo: sólo el contorno de 1 px, esquinas redondas, centro transparente. En hover el relleno llega al contorno sin hueco en las esquinas.
- [ ] Manija del slider: círculo cian macizo con el núcleo claro encima, 2 px de cian alrededor y sin hilo oscuro entre los dos (con el aro de la Sesión 09, el hilo se veía).
- [ ] **Ningún otro color ha cambiado** respecto a LK-22a.
- [ ] Estirar el botón del pie a lo ancho en el Inspector: las esquinas **no se deforman**.
- [ ] La cápsula del toggle no se deforma con sus 36×20, y sus extremos siguen siendo semicírculos.
- [ ] A 1920×1080 no se ve pixelado ni borroso en ningún borde.
- [ ] Tras salir de Play, `git status` sin cambios en `MAT_Debug.mat` (D-001).

## Fuera de alcance
- Comportamiento del botón —texto y borde virando por estado, escala 0.98— → **LK-50**. Son dos frentes separados y ninguno espera al otro.
- Iconografía Lucide / Tabler (GDD línea 473): no es un 9-slice, es un set entero y otra tarea.
- Logotipo y splash (GDD línea 373) → LK-13. Fuentes → LK-22b.
- Empaquetar los siete en un `SpriteAtlas`: se puede hacer después sin tocar el generador.
- El borde de 1 px de la muestra de color (`ui-style.md` > Medidas > Color): hoy no existe y esta tarea no lo añade.
- `LumiSkin` como ScriptableObject, para que el comprador cambie el arte sin regenerar: más alcance del necesario hoy, anotado en `UI_ART_BRIEF.md` como bifurcación.
- Tocar `Core`, `Demo`, los widgets o el `EffectDebugTester`.
