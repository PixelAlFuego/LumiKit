# LK-51 — Arte de interfaz: sprites propios del pack
Estado: ⬜ spec escrita, pendiente de aprobación (Sesión 08) · Tarea nueva, no una ampliación de LK-50 (decisión del usuario, Sesión 08) · Depende de: LK-22a · Independiente de LK-50: aquello es comportamiento, esto es arte · Diseño: GDD §2.6 (líneas 510-545), §2.7 (líneas 580-588), §2.8 (líneas 593-621) · Detalle de dibujo: `docs/reference/UI_ART_BRIEF.md`

## Objetivo
Quitar de la UI los dos sprites de Unity —`UI/Skin/UISprite.psd` y `UI/Skin/Knob.psd`, que cubren las diez `Image` con sprite que monta hoy el generador— y ponerle arte propio:
esquinas redondeadas de verdad, cápsula, círculo y contorno. Es lo único que `LumiTheme` no puede dar por sí sola.
Se hace en dos tiempos, porque los PNG los dibuja el usuario y el código no depende de ellos: **primero el soporte en el generador**, que sin sprites
en disco aborta con un mensaje claro; **después** los siete archivos y la regeneración. Entre medias el proyecto compila y el HUD se ve como hoy.

## Archivos
- `Assets/LumiKit/Sprites/UI/` (planeada; `Sprites/` existe y está vacía) · los siete `SPR_UI_*.png`, **los dibuja el usuario**. Nombres, tamaños y bordes: `UI_ART_BRIEF.md` > Sprites a dibujar. Prefijo `SPR_` por CONVENTIONS.md.
- `UI/LumiTheme.cs` (existe) · aditiva, 2 líneas: `RADIUS_SMALL` (4), el radio de la muestra de color y del tooltip (GDD líneas 601 y 621). `RADIUS` (6) ya está y el resto de medidas también.
- `Assets/Editor/ParameterPanelBuilder.cs` (existe) · **no aditiva**: siete `const string` de ruta, un `RequireSprites()` y las diez llamadas a `CreateImage` pasando el sprite del pack. Es una línea por llamada; ninguna cambia de forma.
- Regenera (D-002): los seis `PRF_*` de `Prefabs/UI/`. Cambian todos: todos llevan al menos una `Image`.
- No se tocan: `Core/`, `Utils/`, `Demo/`, los scripts de `UI/` salvo `LumiTheme`, `EffectDebugTester.cs`, ni `TMP Settings.asset`.

## Contrato
**Dónde vive la referencia.** Horneada en el prefab por el generador, con `AssetDatabase.LoadAssetAtPath`
sobre `Image.sprite`. `LumiTheme` es estática y no puede serializar un `Sprite`, así que las rutas van
en el generador, igual que las fuentes de LK-22b. En runtime no hay ni una búsqueda. Mantiene D-002.

**`RequireSprites()`** — se ejecuta al principio de `GeneratePrefabs`, junto a `HasDefaultFont()`:
si falta alguno de los obligatorios, error en consola **nombrando una por una las rutas que faltan**,
diálogo remitiendo a esta spec, y **aborta sin escribir ni un prefab**. Misma política que ya tiene el
generador para los prefabs existentes y para los TMP Essential Resources. `SPR_UI_Track` es opcional:
si no está, el riel se queda macizo y sólo se avisa con un `Debug.Log`, sin abortar.

**Reparto de los sprites sobre las diez `Image`:**

| Sprite | Dónde se usa | Sustituye a |
|---|---|---|
| `SPR_UI_Rect_R6` | relleno del botón del pie, fondo de opción de enum | `UISprite` |
| `SPR_UI_Rect_R6_Outline` | borde del botón del pie | `UISprite` |
| `SPR_UI_Rect_R4` | muestra de color y las seis muestras de paleta | `UISprite` |
| `SPR_UI_Pill` | pista del toggle | `UISprite` |
| `SPR_UI_Circle` | manija del toggle y núcleo de la manija del slider | `Knob` |
| `SPR_UI_Ring` | aro cian de la manija del slider | `Knob` |
| `SPR_UI_Track` (opcional) | riel y relleno del slider | `UISprite` |

Los sprites se dibujan en blanco con la forma en el alfa y el color se lo sigue poniendo
`Image.color` desde `LumiTheme` (D-007). El generador **no** toca ningún color al cambiar de sprite:
si algo cambia de color en esta tarea, es un fallo.

**Lo que esto arregla.** Con `SPR_UI_Rect_R6_Outline` el borde del botón pasa a ser un contorno con el centro transparente, así que el relleno ya puede ser
transparente de verdad en el estado Normal del botón secundario: cierra la deuda que LK-22a dejó anotada en STATE y que LK-50 tampoco resuelve.
Si LK-50 ya está hecha cuando esto entre, es cambiar un token de su tabla; si no, se queda esperándola.

## Banco de pruebas
`Assets/_Development/TestBench.unity`, con el HUD y el pie ya verificados en LK-22a.
Los dos tiempos se verifican por separado.

| Paso | Qué | Quién |
|---|---|---|
| 1 | Con el soporte programado y **la carpeta vacía**: ejecutar `LumiKit/UI/Generar prefabs del panel` y comprobar que aborta nombrando las seis rutas que faltan, sin tocar nada | usuario |
| 2 | Dibujar los siete PNG siguiendo `UI_ART_BRIEF.md`, a 3× y en blanco con alfa | usuario |
| 3 | Importarlos con los ajustes del brief. **El borde del 9-slice no está en el importador**: se marca en `Sprite Editor` > `Border L/T/R/B` | usuario |
| 4 | Borrar los seis `PRF_*` y el objeto `ParameterPanel` de la escena, y ejecutar los dos menús `LumiKit/UI/…` | usuario |

Tras el paso 1 el proyecto se queda utilizable: el generador aborta, pero los prefabs que ya existen siguen en disco y el HUD sigue funcionando. Los cuatro pasos no tienen que ser el mismo día.

## Criterios de aceptación (verificables en el editor)
- [ ] Compila sin errores ni warnings nuevos.
- [ ] Carpeta vacía: el menú aborta, la consola **nombra una por una** las rutas que faltan, y `git status` queda limpio en `Prefabs/UI/`.
- [ ] Con seis de los siete y sin `SPR_UI_Track`: genera igual, avisando sólo del opcional.
- [ ] Con los siete: los seis prefabs se generan y no queda ninguna referencia a `UISprite.psd` ni a `Knob.psd` (buscar "UISprite" y "Knob" en los `.prefab`).
- [ ] Play + clic en `SPR_Crystal`: botón del pie y opciones de enum con esquinas redondeadas, pista del toggle en cápsula, manijas redondas. Nada cuadrado que debiera ser redondo.
- [ ] **Ningún color ha cambiado** respecto a LK-22a: el sprite sólo aporta forma.
- [ ] Estirar el botón del pie a lo ancho en el Inspector: las esquinas **no se deforman** (`Mesh Type = Full Rect` y borde marcado en el Sprite Editor).
- [ ] La cápsula del toggle no se deforma con sus 36×20, y sus extremos siguen siendo semicírculos.
- [ ] A 1920×1080 no se ve pixelado ni borroso en ningún borde, y los siete `.png` tienen `Pixels Per Unit = 300`, `Mesh Type = Full Rect` y `Generate Mip Maps` apagado.
- [ ] Tras salir de Play, `git status` sin cambios en `MAT_Debug.mat` (D-001).

## Fuera de alcance
- Comportamiento del botón —texto y borde virando por estado, escala 0.98— → **LK-50**. Son dos frentes separados y ninguno espera al otro.
- Iconografía Lucide / Tabler (GDD línea 473): no es un 9-slice, es un set entero y otra tarea.
- Logotipo y splash (GDD línea 373) → LK-13. Fuentes → LK-22b.
- Empaquetar los siete en un `SpriteAtlas`: se puede hacer después sin tocar el generador.
- `LumiSkin` como ScriptableObject, para que el comprador cambie el arte sin regenerar: más alcance del necesario hoy, anotado en `UI_ART_BRIEF.md` como bifurcación.
- Tocar `Core`, `Demo`, los widgets o el `EffectDebugTester`.
