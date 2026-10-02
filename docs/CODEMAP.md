# Mapa de código
Una fila por archivo `.cs`, `.shader` o `.shadergraph` que exista en disco. Si no está aquí, no existe.
Se actualiza al cerrar cada tarea, antes del commit.

Reglas de esta tabla:
- No se añade una fila hasta que el archivo esté escrito en disco.
- "Depende de" lista sólo dependencias directas dentro de LumiKit.
- Capa: `Core` · `Demo` · `UI` · `Systems` · `Utils` · `Editor` · `Shader`.
- Dirección permitida: UI → Demo → Core. `Core` no referencia `UI` ni `Demo`.

## Runtime

| Archivo | Capa | Tipo | Responsabilidad | Depende de | LK | Estado |
|---|---|---|---|---|---|---|
| `Core/ParameterType.cs` | Core | enum | `Float, Color, Boolean, Enum`. Orden congelado | — | LK-09 | ✅ |
| `Core/EffectParameter.cs` | Core | clase `[Serializable]` | Un parámetro: propertyName, tipo, rango, defecto. Cachea `PropertyId` | ParameterType | LK-09 | ✅ |
| `Core/EffectDefinition.cs` | Core | ScriptableObject | Catálogo de un efecto. `LumiKit/Effect Definition` | EffectParameter | LK-09 | ✅ |
| `Core/EffectRegistry.cs` | Core | ScriptableObject | Catálogo global. `LumiKit/Effect Registry` | EffectDefinition | LK-09 | ✅ |
| `Core/EffectController.cs` | Core | MonoBehaviour | Estado vivo + aplicación al Renderer. Setters/getters tipados, reset, `SetEffectEnabled`. Al inicializarse avisa de los `propertyName` que el material no declara | EffectDefinition, MaterialPropertyHelper | LK-09 · LK-49 | ✅ |
| `Utils/MaterialPropertyHelper.cs` | Utils | estática | Único punto que escribe el MPB. Declara `_EffectEnabled`, `ConvertColor` y `HasProperty` | — | LK-09 · LK-49 | ✅ |
| `Utils/Singleton.cs` | Utils | clase abstracta | Base genérica de managers. Sin `DontDestroyOnLoad` automático | — | LK-09 | ✅ |
| `Utils/TextInputFocus.cs` | Utils | estática | `IsTyping`: el objeto seleccionado del `EventSystem` es un campo de texto (TMP o uGUI) en edición. Única comprobación de la regla "escribiendo, ningún atajo escucha" | — | LK-52 | ✅ |
| `Demo/DemoCameraController.cs` | Demo | MonoBehaviour | Cámara ortográfica 2D: paneo `WASD` y botón derecho, zoom con rueda acotado, límite `_bounds`. Ignora el ratón sobre UI de `EventSystem`, y WASD calla con `TextInputFocus.IsTyping` | TextInputFocus | LK-12 · LK-52 | ✅ |
| `Demo/ObjectSelector.cs` | Demo | MonoBehaviour | Selección con botón izquierdo: raycast `Physics2D` acotado por `LayerMask`, un objeto activo a la vez, evento `OnSelectionChanged`. Ignora el ratón sobre UI de `EventSystem` | EffectController | LK-10 | ✅ |
| `Demo/ComparisonToggle.cs` | Demo | MonoBehaviour | Comparación con TAB: mientras está pulsado, el seleccionado se ve sin efecto (`SetEffectEnabled(false)`) y al soltar recupera su estado previo. En `LateUpdate`, tras el selector. Calla con `TextInputFocus.IsTyping`. Uno por escena; `OnDisable` restaura. No conoce la UI | ObjectSelector, EffectController, TextInputFocus | LK-24 | ✅ |
| `UI/LumiTheme.cs` | UI | estática | Paleta y medidas de `ui-style.md` más las de toggle, muestra de color, filas, pie y botón, y `Transparent` para el reposo del botón secundario. Único sitio del pack con colores literales. Resolución de diseño 1920×1080 (D-007). Sin fuentes: las hornea el generador en cada texto (LK-22b). `LumiCyanHover` y `LumiCyanPressed` para el primario de `LumiButton`. Ancho del valor del slider a 68 con relleno 4 (LK-52). `MENU_BUTTON_WIDTH` y `MENU_SECTION_GAP`, derivados (LK-13) | — | LK-11a · LK-11b · LK-22a · LK-51 · LK-22b · LK-50 · LK-52 · LK-13 | 🟡 |
| `UI/LumiButton.cs` | UI | MonoBehaviour (`Button`) | Botón con los cuatro niveles del GDD §2.7 (`LumiButtonStyle`, en el mismo archivo): fondo, borde y texto viran por estado con `CrossFadeColor` (150 ms lineales) y la escala baja a 0.98 al presionar. Lleva la cuenta del puntero: pulsado y arrastrado fuera, vuelve a reposo. Propiedad `Style`, que repinta al cambiar (LK-13) | LumiTheme | LK-50 · LK-13 | 🟡 |
| `UI/ParameterPanelUI.cs` | UI | MonoBehaviour | Escucha `OnSelectionChanged`, instancia un widget por parámetro (los cuatro tipos) y vacía y oculta el panel al deseleccionar. `RefreshFromController()` para cambios externos, y el Reset del pie es su primer llamador | EffectController, EffectDefinition, ObjectSelector, los cuatro widgets | LK-11a · LK-11b · LK-22a | ✅ |
| `UI/Widgets/ParameterWidgetBase.cs` | UI | clase abstracta | Contrato común de los widgets: `Initialize(parámetro, controller)`, `Refresh()`, evento `OnValueChanged` | EffectParameter, EffectController | LK-11a | ✅ |
| `UI/Widgets/SliderParameterWidget.cs` | UI | MonoBehaviour | Widget del tipo `Float`: slider acotado al rango y valor en Mono, editable con clic (`TMP_InputField`: coma o punto, Escape no aplica, sin tocar no redondea). Escribe por `SetFloat` | ParameterWidgetBase | LK-11a · LK-52 | ✅ |
| `UI/Widgets/ColorParameterWidget.cs` | UI | MonoBehaviour | Widget del tipo `Color`: muestra desplegable con tres sliders RGB y seis muestras de paleta. Escribe por `SetColor`; no edita el alfa | ParameterWidgetBase, LumiTheme | LK-11b | ✅ |
| `UI/Widgets/ToggleParameterWidget.cs` | UI | MonoBehaviour | Widget del tipo `Boolean`: pista 36×20 y manija que cambia de lado. Escribe por `SetBool` | ParameterWidgetBase, LumiTheme | LK-11b | ✅ |
| `UI/Widgets/EnumParameterWidget.cs` | UI | MonoBehaviour | Widget del tipo `Enum`: un botón por opción de `EnumOptions`, instanciados en runtime. Escribe por `SetEnum` | ParameterWidgetBase, LumiTheme | LK-11b | ✅ |
| `Systems/UIAudioManager.cs` | Systems | MonoBehaviour (`Singleton`) | Audio de interfaz persistente (`DontDestroyOnLoad`). `Play(UISound)` por el grupo UI con `PlayOneShot`; un Hover no se repite antes de 150 ms. Música en bucle por el grupo Music: sólo la arranca la instancia que sobrevive. Enum `UISound` en el mismo archivo, con `Transition` (LK-17) | Singleton | LK-23 · LK-17 | 🟡 |
| `Systems/UISoundTrigger.cs` | Systems | MonoBehaviour | Hover (`IPointerEnterHandler`, calla arrastrando) y Click (izquierdo o Submit, si `_playClick`) de un `Selectable`, en su mismo GameObject. Calla si no es interactuable o no hay manager | UIAudioManager | LK-23 | ✅ |
| `Systems/UISelectionSound.cs` | Systems | MonoBehaviour | Select al elegir un objeto: escucha `OnSelectionChanged`. Deseleccionar calla | UIAudioManager, ObjectSelector | LK-23 | ✅ |
| `Systems/SceneLoader.cs` | Systems | estática | Carga por nombre, asíncrona y en modo Single, con `UISound.Transition`; ignora otra petición mientras carga; `LogError` si la escena no está en el build. `Quit()` con `Application.Quit` | UIAudioManager | LK-17 | 🟡 |
| `Systems/SceneMenu.cs` | Systems | MonoBehaviour | Un `LumiButton` por entrada de una lista (`MenuEntry`: texto, estilo, `MenuAction` y escena), clonando una plantilla inactiva. Ancho fijo o del texto más el padding | SceneLoader, LumiButton, LumiTheme | LK-13 | 🟡 |

Raíz de los anteriores: `Assets/LumiKit/Runtime/Scripts/`.
Sin consumidores todavía: `EffectRegistry` lo usa LK-30. `Singleton` lo usa `UIAudioManager` (LK-23).

## Editor

| Archivo | Tipo | Responsabilidad | Depende de | LK | Estado |
|---|---|---|---|---|---|
| `Assets/Editor/ParameterPanelBuilder.cs` | estática | Genera los seis prefabs del panel (`PRF_ParameterPanel`, `PRF_Widget_Slider/Color/Toggle/Enum/EnumOption`), con su pie y su `LumiButton` secundario, y monta `UI_Root`, `EventSystem` y el panel en la escena abierta. Usa seis de los siete `SPR_UI_*` de `Sprites/UI/` (`SPR_UI_Ring` no) y tres de las cuatro fuentes de `Fonts/` por familia `Display`/`Label`/`Mono` (`Inter-Regular` no), y aborta si falta un obligatorio. Aborta si algún prefab existe; no guarda la escena. El valor del slider es un `TMP_InputField` (LK-52). Cada `Selectable` lleva `UISoundTrigger`; sliders y campo del valor, sin clic (LK-23) | LumiTheme, LumiButton, ParameterPanelUI, los cuatro widgets, ObjectSelector, UISoundTrigger | LK-11a · LK-11b · LK-22a · LK-51 · LK-22b · LK-50 · LK-52 · LK-23 | ✅ |
| `Assets/Editor/UIAudioBuilder.cs` | estática | Genera `PRF_UIAudioManager` (dos `AudioSource`: SFX al grupo UI, música en bucle al grupo Music) y lo monta en la raíz de la escena abierta, con `UISelectionSound` en el `ObjectSelector`. Aborta si falta el mixer (D-011), un grupo o un clip, o si el prefab o el montaje ya existen; no guarda la escena. Sin diálogos: lo lanza también el verificador por MCP. Menú que asigna `SFX_UI_Transition` al prefab existente, en su sitio (LK-17) | UIAudioManager, UISelectionSound, ObjectSelector | LK-23 · LK-17 | 🟡 |
| `Assets/Editor/FontFeatureCleaner.cs` | estática | Quita de las cinco tablas de features de cada `TMP_FontAsset` Static de `Fonts/` todo registro con algún glifo fuera del atlas. Informa por tabla y del peso en disco. Paso obligatorio tras regenerar una fuente o usar "Import Font Features"; repetible | — | LK-22b | ✅ |
| `Assets/Editor/EffectAssetBuilder.cs` | estática | Genera el `MAT_` por defecto y el `EFF_` de cada efecto 2D (Outline 2D y Glow 2D, un menú cada uno). Si existen, los actualiza en su sitio y conserva el GUID; el material toma los valores por defecto del shader. Aborta si falta el shader. Sin diálogos (MCP) | EffectDefinition, EffectParameter, LumiTheme | LK-01 · LK-03 | ✅ |
| `Assets/Editor/DemoSceneBuilder.cs` | estática | Regenera `02_Demo_2D` desde cero y la guarda (D-013): cámara ortográfica desplazada medio panel, `SPR_Crystal` con brillo (`MAT_Glow2D_Default`, `EFF_Glow2D`) y `SPR_RuneCoin` con contorno (`MAT_Outline2D_Default`, `EFF_Outline2D`), los dos con la forma física del sprite en el collider, `Systems` (`ObjectSelector`, `ComparisonToggle`), HUD y audio por `BuildSceneRig` y `MountInScene`, `ControlsHint` temporal y botón "Menú" (LK-13). Aborta en Play, con Prefab Mode o con escenas sin guardar salvo las dos generadas; sin diálogos (MCP) | ParameterPanelBuilder, UIAudioBuilder, MainMenuSceneBuilder, LumiTheme, EffectController, ObjectSelector, ComparisonToggle, DemoCameraController | LK-14 · LK-13 | 🟡 |
| `Assets/Editor/MainMenuSceneBuilder.cs` | estática | Regenera `01_MainMenu` desde cero y la guarda (D-013): cámara, `UI_Root` (título, subtítulo, botones por `SceneMenu`, versión), `EventSystem` y `PRF_UIAudioManager`. `BuildSceneMenu` e `IsGeneratedScene`, internos, los usa también la demo. Sin diálogos (MCP) | SceneMenu, LumiButton, LumiTheme, UISoundTrigger, UIAudioManager | LK-13 | 🟡 |
| `Assets/Editor/LumiButtonEditor.cs` | clase (`ButtonEditor`) | Inspector de `LumiButton`: el de `Button` y debajo `_style`, `_fill`, `_border` y `_label`. Por heredar, el asmdef referencia `UnityEditor.UI`. No se exporta (duda de LK-27) | LumiButton | LK-50 | ✅ |

## Shaders y materiales

| Archivo | Tipo | Propiedades expuestas | Material(es) | LK | Estado |
|---|---|---|---|---|---|
| `Assets/LumiKit/Shaders/2D/SH_Outline2D.shader` | HLSL (`LumiKit/2D/Outline`), pases `Universal2D` y `UniversalForward` | `_OutlineColor` (HDR), `_OutlineWidth` (centésimas de unidad de mundo, a UV por derivadas), `_OutlineMode` (sólido, punteado, animado), `_EffectEnabled` | `Materials/2D/MAT_Outline2D_Default.mat` (generado por `EffectAssetBuilder`, valores del shader) | LK-01 | ✅ |
| `Assets/LumiKit/Shaders/2D/SH_Glow2D.shader` | HLSL (`LumiKit/2D/Glow`), pases `Universal2D` y `UniversalForward`; mundo → UV de `SH_Outline2D` | `_GlowColor` (HDR), `_GlowIntensity`, `_PulseEnabled` (`[ToggleUI]`), `_PulseSpeed`, `_EffectEnabled`. Brillo exterior e interior por alfa difuminado (24 × 4 muestras, radio 0,2 u), borde de luz en huecos | `Materials/2D/MAT_Glow2D_Default.mat` (generado por `EffectAssetBuilder`, valores del shader) | LK-03 | ✅ |

## Assembly definitions

| Archivo | Ensamblado | Plataformas |
|---|---|---|
| `Assets/LumiKit/Runtime/Scripts/LumiKit.Runtime.asmdef` | LumiKit.Runtime | todas |
| `Assets/Editor/LumiKit.Editor.asmdef` | LumiKit.Editor | Editor |

## ScriptableObjects instanciados

| Asset | Tipo | LK | Estado |
|---|---|---|---|
| `Runtime/Data/Effects/EFF_Outline2D.asset` | EffectDefinition, generado por `EffectAssetBuilder`: `_OutlineColor`, `_OutlineWidth` (0-10, 4), `_OutlineMode` (Sólido, Punteado, Animado) | LK-01 | ✅ |
| `Runtime/Data/Effects/EFF_Glow2D.asset` | EffectDefinition, generado por `EffectAssetBuilder`: `_GlowColor` (Lumi Violet), `_GlowIntensity` (0-2, 1,5: capas por encima), `_PulseEnabled` (no), `_PulseSpeed` (0-3, 1) | LK-03 | ✅ |

## Escenas

Las 5 escenas existen (confirmado por el usuario, Sesión 01); `01_MainMenu` y `02_Demo_2D` generadas (LK-13, LK-14), las demás **vacías**.
Se pueblan con generadores de editor (D-002). No se crean escenas nuevas.

| Escena | Construida por | Contenido | LK | Estado |
|---|---|---|---|---|
| `Assets/LumiKit/Scenes/00_Splash.unity` | _(planeado)_ | vacía | — | ⬜ |
| `Assets/LumiKit/Scenes/01_MainMenu.unity` | `MainMenuSceneBuilder`, que la guarda (D-013) | Cámara, `UI_Root` (título, subtítulo, "Demo 2D" y "Salir", versión), `EventSystem`, `PRF_UIAudioManager`. Primera del build (la pone el usuario) | LK-13 | 🟡 |
| `Assets/LumiKit/Scenes/02_Demo_2D.unity` | `DemoSceneBuilder`, que la guarda (D-013) | Cámara, `SPR_Crystal` con brillo y `SPR_RuneCoin` con contorno, `Systems`, `UI_Root` (panel, `ControlsHint` temporal y `MenuButton`), `EventSystem`, `PRF_UIAudioManager` | LK-14 · LK-13 | 🟡 |
| `Assets/LumiKit/Scenes/03_Demo_3D.unity` | — | vacía · fuera del MVP 2D | LK-15 | ⬜ |
| `Assets/LumiKit/Scenes/04_Demo_VFX.unity` | — | vacía · fuera del MVP 2D | LK-16 | ⬜ |

## Prefabs

| Prefab | Generado por | Componentes | LK | Estado |
|---|---|---|---|---|
| `Prefabs/UI/PRF_ParameterPanel.prefab` | `ParameterPanelBuilder` | ParameterPanelUI + Surface (Image) + Header + Content (VerticalLayoutGroup, ContentSizeFitter) + Footer (separador, HorizontalLayoutGroup, `LumiButton` Reset) | LK-11a · LK-22a · LK-51 · LK-50 · LK-23 | ✅ |
| `Prefabs/UI/PRF_Widget_Slider.prefab` | `ParameterPanelBuilder` | SliderParameterWidget + Label + Value (TMP_InputField, Outline, Text Area con RectMask2D) + Slider (riel, relleno, manija) | LK-11a · LK-51 · LK-52 · LK-23 | ✅ |
| `Prefabs/UI/PRF_Widget_Color.prefab` | `ParameterPanelBuilder` | ColorParameterWidget + Swatch (Button) + Expand (3 canales + 6 muestras) | LK-11b · LK-51 · LK-23 | ✅ |
| `Prefabs/UI/PRF_Widget_Toggle.prefab` | `ParameterPanelBuilder` | ToggleParameterWidget + Track (Toggle) + Handle | LK-11b · LK-51 · LK-23 | ✅ |
| `Prefabs/UI/PRF_Widget_Enum.prefab` | `ParameterPanelBuilder` | EnumParameterWidget + Options (HorizontalLayoutGroup) | LK-11b | ✅ |
| `Prefabs/UI/PRF_Widget_EnumOption.prefab` | `ParameterPanelBuilder` | Botón de una opción. Lo instancia `EnumParameterWidget`, no el panel | LK-11b · LK-51 · LK-23 | ✅ |
| `Prefabs/Systems/PRF_UIAudioManager.prefab` | `UIAudioBuilder` | UIAudioManager + dos AudioSource (SFX → grupo UI; música en bucle, `MUS_Ambient_Loop` → grupo Music), `playOnAwake` apagado. `SFX_UI_Transition` asignado en su sitio (LK-17) | LK-23 · LK-17 | 🟡 |
