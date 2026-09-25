# Mapa de código
Una fila por archivo `.cs` o `.shadergraph` que exista en disco. Si no está aquí, no existe.
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
| `Demo/DemoCameraController.cs` | Demo | MonoBehaviour | Cámara ortográfica 2D: paneo `WASD` y botón derecho, zoom con rueda acotado, límite `_bounds`. Ignora el ratón sobre UI de `EventSystem` | — | LK-12 | ✅ |
| `Demo/ObjectSelector.cs` | Demo | MonoBehaviour | Selección con botón izquierdo: raycast `Physics2D` acotado por `LayerMask`, un objeto activo a la vez, evento `OnSelectionChanged`. Ignora el ratón sobre UI de `EventSystem` | EffectController | LK-10 | ✅ |
| `UI/LumiTheme.cs` | UI | estática | Paleta y medidas de `ui-style.md` más las de toggle, muestra de color, filas, pie y botón, y `Transparent` para el reposo del botón secundario. Único sitio del pack con colores literales. Resolución de diseño 1920×1080 (D-007). Sin fuentes: las hornea el generador en cada texto (LK-22b). `LumiCyanHover` y `LumiCyanPressed` para el primario de `LumiButton` | — | LK-11a · LK-11b · LK-22a · LK-51 · LK-22b · LK-50 | 🟡 |
| `UI/LumiButton.cs` | UI | MonoBehaviour (`Button`) | Botón con los cuatro niveles del GDD §2.7 (`LumiButtonStyle`, en el mismo archivo): fondo, borde y texto viran por estado con `CrossFadeColor` (150 ms lineales) y la escala baja a 0.98 al presionar. Lleva la cuenta del puntero: pulsado y arrastrado fuera, vuelve a reposo | LumiTheme | LK-50 | 🟡 |
| `UI/ParameterPanelUI.cs` | UI | MonoBehaviour | Escucha `OnSelectionChanged`, instancia un widget por parámetro (los cuatro tipos) y vacía y oculta el panel al deseleccionar. `RefreshFromController()` para cambios externos, y el Reset del pie es su primer llamador | EffectController, EffectDefinition, ObjectSelector, los cuatro widgets | LK-11a · LK-11b · LK-22a | ✅ |
| `UI/Widgets/ParameterWidgetBase.cs` | UI | clase abstracta | Contrato común de los widgets: `Initialize(parámetro, controller)`, `Refresh()`, evento `OnValueChanged` | EffectParameter, EffectController | LK-11a | ✅ |
| `UI/Widgets/SliderParameterWidget.cs` | UI | MonoBehaviour | Widget del tipo `Float`: slider acotado al rango y valor en Mono. Escribe por `SetFloat` | ParameterWidgetBase | LK-11a | ✅ |
| `UI/Widgets/ColorParameterWidget.cs` | UI | MonoBehaviour | Widget del tipo `Color`: muestra desplegable con tres sliders RGB y seis muestras de paleta. Escribe por `SetColor`; no edita el alfa | ParameterWidgetBase, LumiTheme | LK-11b | ✅ |
| `UI/Widgets/ToggleParameterWidget.cs` | UI | MonoBehaviour | Widget del tipo `Boolean`: pista 36×20 y manija que cambia de lado. Escribe por `SetBool` | ParameterWidgetBase, LumiTheme | LK-11b | ✅ |
| `UI/Widgets/EnumParameterWidget.cs` | UI | MonoBehaviour | Widget del tipo `Enum`: un botón por opción de `EnumOptions`, instanciados en runtime. Escribe por `SetEnum` | ParameterWidgetBase, LumiTheme | LK-11b | ✅ |

Raíz de los anteriores: `Assets/LumiKit/Runtime/Scripts/`.
Sin consumidores todavía: `Singleton` y `EffectRegistry` los usan `Systems` y LK-30.

## Editor

| Archivo | Tipo | Responsabilidad | Depende de | LK | Estado |
|---|---|---|---|---|---|
| `Assets/Editor/ParameterPanelBuilder.cs` | estática | Genera los seis prefabs del panel (`PRF_ParameterPanel`, `PRF_Widget_Slider/Color/Toggle/Enum/EnumOption`), con su pie y su `LumiButton` secundario, y monta `UI_Root`, `EventSystem` y el panel en la escena abierta. Usa seis de los siete `SPR_UI_*` de `Sprites/UI/` (`SPR_UI_Ring` no) y tres de las cuatro fuentes de `Fonts/` por familia `Display`/`Label`/`Mono` (`Inter-Regular` no), y aborta si falta un obligatorio. Aborta si algún prefab existe; no guarda la escena | LumiTheme, LumiButton, ParameterPanelUI, los cuatro widgets, ObjectSelector | LK-11a · LK-11b · LK-22a · LK-51 · LK-22b · LK-50 | 🟡 |
| `Assets/Editor/FontFeatureCleaner.cs` | estática | Quita de las cinco tablas de features de cada `TMP_FontAsset` Static de `Fonts/` todo registro con algún glifo fuera del atlas. Informa por tabla y del peso en disco. Paso obligatorio tras regenerar una fuente o usar "Import Font Features"; repetible | — | LK-22b | ✅ |
| `Assets/Editor/LumiButtonEditor.cs` | clase (`ButtonEditor`) | Inspector de `LumiButton`: el de `Button` y debajo `_style`, `_fill`, `_border` y `_label`. Por heredar, el asmdef referencia `UnityEditor.UI`. No se exporta (duda de LK-27) | LumiButton | LK-50 | 🟡 |

## Shaders y materiales

| Archivo | Tipo | Propiedades expuestas | Material(es) | LK | Estado |
|---|---|---|---|---|---|
| _(vacío)_ | | | | | |

## Assembly definitions

| Archivo | Ensamblado | Plataformas |
|---|---|---|
| `Assets/LumiKit/Runtime/Scripts/LumiKit.Runtime.asmdef` | LumiKit.Runtime | todas |
| `Assets/Editor/LumiKit.Editor.asmdef` | LumiKit.Editor | Editor |

## ScriptableObjects instanciados

| Asset | Tipo | LK | Estado |
|---|---|---|---|
| _(vacío)_ | | | |

## Escenas

Las 5 escenas existen y están **vacías** (confirmado por el usuario, Sesión 01).
Se pueblan con generadores de editor (D-002). No se crean escenas nuevas.

| Escena | Construida por | Contenido | LK | Estado |
|---|---|---|---|---|
| `Assets/LumiKit/Scenes/00_Splash.unity` | _(planeado)_ | vacía | — | ⬜ |
| `Assets/LumiKit/Scenes/01_MainMenu.unity` | _(planeado)_ | vacía | LK-13 | ⬜ |
| `Assets/LumiKit/Scenes/02_Demo_2D.unity` | _(planeado)_ | vacía | LK-14 | ⬜ |
| `Assets/LumiKit/Scenes/03_Demo_3D.unity` | — | vacía · fuera del MVP 2D | LK-15 | ⬜ |
| `Assets/LumiKit/Scenes/04_Demo_VFX.unity` | — | vacía · fuera del MVP 2D | LK-16 | ⬜ |

## Prefabs

| Prefab | Generado por | Componentes | LK | Estado |
|---|---|---|---|---|
| `Prefabs/UI/PRF_ParameterPanel.prefab` | `ParameterPanelBuilder` | ParameterPanelUI + Surface (Image) + Header + Content (VerticalLayoutGroup, ContentSizeFitter) + Footer (separador, HorizontalLayoutGroup, botón Reset) | LK-11a · LK-22a · LK-51 | ✅ |
| `Prefabs/UI/PRF_Widget_Slider.prefab` | `ParameterPanelBuilder` | SliderParameterWidget + Label + Value + Slider (riel, relleno, manija) | LK-11a · LK-51 | ✅ |
| `Prefabs/UI/PRF_Widget_Color.prefab` | `ParameterPanelBuilder` | ColorParameterWidget + Swatch (Button) + Expand (3 canales + 6 muestras) | LK-11b · LK-51 | ✅ |
| `Prefabs/UI/PRF_Widget_Toggle.prefab` | `ParameterPanelBuilder` | ToggleParameterWidget + Track (Toggle) + Handle | LK-11b · LK-51 | ✅ |
| `Prefabs/UI/PRF_Widget_Enum.prefab` | `ParameterPanelBuilder` | EnumParameterWidget + Options (HorizontalLayoutGroup) | LK-11b | ✅ |
| `Prefabs/UI/PRF_Widget_EnumOption.prefab` | `ParameterPanelBuilder` | Botón de una opción. Lo instancia `EnumParameterWidget`, no el panel | LK-11b · LK-51 | ✅ |
