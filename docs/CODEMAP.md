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
| `Core/EffectController.cs` | Core | MonoBehaviour | Estado vivo + aplicación al Renderer. Setters/getters tipados, reset, `SetEffectEnabled` | EffectDefinition, MaterialPropertyHelper | LK-09 | ✅ |
| `Utils/MaterialPropertyHelper.cs` | Utils | estática | Único punto que escribe el MPB. Declara `_EffectEnabled` y `ConvertColor` | — | LK-09 | ✅ |
| `Utils/Singleton.cs` | Utils | clase abstracta | Base genérica de managers. Sin `DontDestroyOnLoad` automático | — | LK-09 | ✅ |
| `Demo/DemoCameraController.cs` | Demo | MonoBehaviour | Cámara ortográfica 2D: paneo `WASD` y botón derecho, zoom con rueda acotado, límite `_bounds`. Ignora el ratón sobre UI de `EventSystem` | — | LK-12 | ✅ |
| `Demo/ObjectSelector.cs` | Demo | MonoBehaviour | Selección con botón izquierdo: raycast `Physics2D` acotado por `LayerMask`, un objeto activo a la vez, evento `OnSelectionChanged`. Ignora el ratón sobre UI de `EventSystem` | EffectController | LK-10 | ✅ |
| `UI/LumiTheme.cs` | UI | estática | Paleta y medidas de `ui-style.md`. Único sitio del pack con colores literales. Sin fuentes: LK-22 | — | LK-11a | 🟡 |
| `UI/ParameterPanelUI.cs` | UI | MonoBehaviour | Escucha `OnSelectionChanged`, instancia un widget por parámetro y vacía y oculta el panel al deseleccionar. `RefreshFromController()` para cambios externos | EffectController, EffectDefinition, ObjectSelector, ParameterWidgetBase | LK-11a | 🟡 |
| `UI/Widgets/ParameterWidgetBase.cs` | UI | clase abstracta | Contrato común de los widgets: `Initialize(parámetro, controller)`, `Refresh()`, evento `OnValueChanged` | EffectParameter, EffectController | LK-11a | 🟡 |
| `UI/Widgets/SliderParameterWidget.cs` | UI | MonoBehaviour | Widget del tipo `Float`: slider acotado al rango y valor en Mono. Escribe por `SetFloat` | ParameterWidgetBase | LK-11a | 🟡 |

Raíz de los anteriores: `Assets/LumiKit/Runtime/Scripts/`.
Sin consumidores todavía: `Singleton` y `EffectRegistry` los usan `Systems` y LK-30.

## Editor

| Archivo | Tipo | Responsabilidad | Depende de | LK | Estado |
|---|---|---|---|---|---|
| `Assets/Editor/ParameterPanelBuilder.cs` | estática | Genera `PRF_Widget_Slider` y `PRF_ParameterPanel`; monta `UI_Root`, `EventSystem` y el panel en la escena abierta. Aborta si el prefab ya existe y no guarda la escena | LumiTheme, ParameterPanelUI, SliderParameterWidget, ObjectSelector | LK-11a | 🟡 |

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
| `Prefabs/UI/PRF_ParameterPanel.prefab` | `ParameterPanelBuilder` | ParameterPanelUI + Surface (Image) + Header + Content (VerticalLayoutGroup, ContentSizeFitter) | LK-11a | ✅ |
| `Prefabs/UI/PRF_Widget_Slider.prefab` | `ParameterPanelBuilder` | SliderParameterWidget + Label + Value + Slider (riel, relleno, manija) | LK-11a | ✅ |
