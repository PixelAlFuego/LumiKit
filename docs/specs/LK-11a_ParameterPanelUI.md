# LK-11a — Panel de parámetros y widget Float (ParameterPanelUI)
Estado: 🟠 en curso · Depende de: LK-09 (`EffectController`), LK-10 (`OnSelectionChanged`) · Continúa en LK-11b (Color, Toggle, Enum) · Diseño: GDD §1.3 Mecánica 2 (líneas 46-56), §1.6 (línea 136), §2.8 (líneas 595-611), §4.4 (líneas 1074-1075), §4.8 (línea 1187) · Estilo: `.claude/rules/ui-style.md` · Nombre del widget: D-008

## Objetivo
Seleccionar un objeto puebla el panel con un widget por parámetro de su `EffectDefinition`; deseleccionar lo vacía y lo oculta. Sólo el tipo `Float` (slider): Color, Boolean y Enum se omiten sin romper el panel.
La UI escribe sólo vía `EffectController` (D-001) y ningún widget conoce un efecto concreto (D-003). uGUI + TextMeshPro, no UI Toolkit (D-006). Trae el `Canvas` y el `EventSystem` que faltaban: el `IsPointerOverUI` de LK-10 y LK-12 pasa a ser ejecutable por primera vez.

## Archivos
Nuevos bajo `Assets/LumiKit/Runtime/Scripts/UI/` (planeados): `LumiTheme.cs` (estática; paleta y medidas de ui-style.md, único sitio del pack con colores literales) y `ParameterPanelUI.cs` · `LumiKit.UI`;
`Widgets/ParameterWidgetBase.cs` y `Widgets/SliderParameterWidget.cs` · `LumiKit.UI.Widgets`.
- `Assets/Editor/ParameterPanelBuilder.cs` (planeado) · `LumiKit.Editor`. Genera los prefabs y monta el HUD en la escena abierta.
- `Assets/Editor/LumiKit.Editor.asmdef` (existe) · edición aditiva: añade `Unity.InputSystem` a `references`. `InputSystemUIInputModule` vive en ese ensamblado, namespace `UnityEngine.InputSystem.UI` (comprobado en el paquete 1.19.0).
- Genera (D-002): `Assets/LumiKit/Prefabs/UI/PRF_Widget_Slider.prefab` y `PRF_ParameterPanel.prefab`.
- No se tocan: `Core/`, `Utils/`, `Demo/`, `LumiKit.Runtime.asmdef` (ya referencia `UnityEngine.UI` y `Unity.TextMeshPro`) ni `EffectDebugTester.cs`, red de seguridad hasta el cierre de la Fase 3.

## Contrato
`LumiTheme` — estática. Colores `public static readonly Color` en PascalCase con el nombre del token (`LumiTheme.LumiCyan`), vía `Color32(0xRR,0xGG,0xBB,0xFF)`; medidas y tamaños de texto `const UPPER_SNAKE_CASE` (`PANEL_WIDTH = 280f`). Regla en CONVENTIONS.md.
Ni un valor inventado: todos salen de ui-style.md. Sin fuentes: los `TMP_FontAsset` del GDD (línea 934) no existen en disco → LK-22.

`ParameterWidgetBase` — `abstract : MonoBehaviour`. Serializa `_label` (`TMP_Text`).
- `public void Initialize(EffectParameter, EffectController)`: guarda ambos; si `Type != SupportedType`, warning y el widget queda inerte; escribe la etiqueta y llama a `OnInitialize()`.
- `protected abstract ParameterType SupportedType { get; }` · `protected abstract void OnInitialize()`: la subclase cablea su control desde `Parameter` y termina llamando a `OnRefresh()`.
- `public void Refresh()` → `protected abstract void OnRefresh()`: relee el valor vivo de `Controller` y lo vuelca en el control **sin** notificar ni reescribir el controller. Inerte si el widget no se inicializó.
- `Parameter` y `Controller`: `protected { get; private set; }`. `public event Action<ParameterWidgetBase> OnValueChanged`: lo dispara la subclase tras escribir; sin consumidores hoy (LK-29, LK-34).
- Etiqueta = `DisplayNameEs`; si está vacía, `PropertyName`. Idioma fijo ES → LK-19.

`SliderParameterWidget` — `SupportedType = Float`. Serializa `_slider` (`Slider`) y `_valueLabel` (`TMP_Text`).
1. `OnInitialize`: `minValue`/`maxValue` del parámetro, `wholeNumbers = false`, listener añadido aquí y quitado en `OnDestroy`; termina en `OnRefresh()`.
2. `OnRefresh`: `_slider.SetValueWithoutNotify(Controller.GetFloat(PropertyName))` (existe en ugui 2.0.0, `Slider.cs` línea 279) y refresca la etiqueta. Sin bandera manual de supresión.
3. Al mover: `Controller.SetFloat(PropertyName, v)` → etiqueta con `ToString("0.00", InvariantCulture)` → `OnValueChanged`.
4. `MaxValue <= MinValue` (asset mal formado): un warning y `interactable = false`.

`ParameterPanelUI` — en la raíz del prefab. Sin `Update`: nunca sondea `Selected`.

| Campo serializado | Tipo | Uso |
|---|---|---|
| `_selector` | ObjectSelector | Obligatorio. Null → warning y se desactiva. Sin búsqueda automática |
| `_surface` | GameObject | Contenedor visual. `SetActive(false)` cuando no hay selección (GDD línea 136) |
| `_content` | RectTransform | Padre de los widgets. `VerticalLayoutGroup` + `ContentSizeFitter` |
| `_effectNameLabel` | TMP_Text | Cabecera. `Definition.DisplayNameEs`; si está vacío, el nombre del asset |
| `_floatWidgetPrefab` | SliderParameterWidget | Prefab del widget `Float`. Los otros tres tipos → LK-11b |

1. `OnEnable` suscribe y construye una vez con `_selector.Selected`; `OnDisable` desuscribe y limpia.
2. Toda reconstrucción empieza destruyendo los hijos de `_content` en recorrido inverso: es el bug del GDD línea 1249. Los widgets vivos se guardan en una lista propia.
3. `public void RefreshFromController()` + `[ContextMenu]`: llama a `Refresh()` en cada widget vivo. Sin caller hoy; lo consumen LK-24 (`SetEffectEnabled`) y el botón Reset de LK-11b (`ResetToDefaults`), que cambian el controller por fuera del panel.
4. Sin selección, sin `Definition` o con 0 parámetros → `_content` vacío y `_surface` inactivo.
5. Parámetro de un tipo sin prefab → se omite, con un único warning por reconstrucción y sin hueco en el layout.

## Banco de pruebas
`Assets/_Development/TestBench.unity`, con lo ya montado en LK-10 y LK-12.

| Qué | Cómo | Quién |
|---|---|---|
| TMP Essential Resources | `Window > TextMeshPro > Import TMP Essential Resources`. Sin ellos los textos no tienen fuente. Importados por el usuario en la Sesión 04 (`Assets/TextMesh Pro/`, sin commitear) | usuario, una vez |
| Los dos prefabs de UI | menú `LumiKit/UI/Generar prefabs del panel (LK-11a)`. Si alguno de los dos ya existe: error con la ruta y **aborta sin escribir nada**; para regenerar, borrar el prefab a mano | script |
| `UI_Root` (Canvas 1920×1080, match height) + `EventSystem` con `InputSystemUIInputModule` + instancia del panel con `_selector` asignado | menú `LumiKit/UI/Montar HUD en la escena abierta (LK-11a)`: reutiliza lo que ya exista, registra Undo y **no guarda la escena** | script |
| `EFF_Debug.asset`: dos parámetros `Float` nuevos (`_OutlineWidth` 0–10 defecto 2 · `_Glow` 0–1 defecto 0.25) junto al `Color` existente | Inspector | usuario |

No verificable todavía: que un `Float` cambie algo en pantalla → LK-01 (no existe ningún shader del pack y escribir una propiedad inexistente en el MPB se ignora en silencio) · fuentes, radios y transiciones exactos → LK-22 · sonido de click → LK-23 · pie con Reset y Copiar, colapsado con `‹`, scroll y descripción del efecto → LK-11b.

## Criterios de aceptación (verificables en el editor)
- [ ] Compila sin errores ni warnings nuevos, `LumiKit.Editor` no muestra referencias en rojo y los textos del panel se leen (TMP importado, sin fuente rosa ni texto invisible).
- [ ] Ejecutar el menú de generación por segunda vez: error nombrando el prefab existente, y `git status` no muestra cambios en los `.prefab`.
- [ ] Sin selección: `_surface` inactivo y el viewport libre de panel.
- [ ] Play + clic en `SPR_Crystal` → panel visible, cabecera con el nombre de `EFF_Debug` y un slider por cada `Float`, cada uno en su valor por defecto y no en 0.
- [ ] Arrastrar un slider: la etiqueta numérica sigue al arrastre y la consola queda muda.
- [ ] **Arrastrar un slider no panea la cámara ni cambia la selección** — cierra el issue arrastrado desde la Sesión 02.
- [ ] Clic sobre el fondo del panel, fuera de todo widget → la selección no cambia.
- [ ] Mover un slider, deseleccionar y volver a seleccionar → vuelve con el valor movido, no con el defecto: la escritura llegó al `EffectController`.
- [ ] Clic en vacío → el panel se vacía y se oculta. A → B → A: el panel nunca acumula widgets duplicados.
- [ ] El `Color` de `EFF_Debug` no genera widget y la consola avisa una sola vez por reconstrucción.
- [ ] El `EffectDebugTester` sigue en pantalla y sus sliders siguen cambiando el color.
- [ ] Tras salir de Play, `git status` no muestra cambios en `MAT_Debug.mat` (D-001). Grep en `Scripts/UI/`: ni un `new Color(` o hex fuera de `LumiTheme.cs`, ni `Input.Get`, ni `renderer.material`.
- [ ] (opcional, prueba de `RefreshFromController`) Asignar `SPR_Crystal` al `_controller` del tester. Play: mover un slider, pulsar `ResetToDefaults()` en el tester —el panel no se entera— y luego `Refresh from controller` en el menú contextual del componente → los sliders vuelven a los valores por defecto.

## Fuera de alcance
- Widgets `Color`, `Boolean` y `Enum` y sus prefabs → LK-11b: el selector de color del GDD (línea 599: rueda, campo hex y seis muestras) y los botones segmentados del Enum (línea 52) son cada uno una pantalla propia. El panel los omite sin romperse y el `Color` sigue cubierto por el tester.
- Pie con Reset y Copiar, colapsado, scroll, descripción y separadores → LK-11b y LK-22. Barra superior, contador, etiqueta flotante y toasts → LK-13, LK-30, LK-31, LK-34.
- Localización (texto fijo en ES) → LK-19 · sonidos → LK-23 · fuentes del GDD → LK-22.
  Que el panel se entere solo de un cambio externo: `RefreshFromController()` existe, pero nadie lo llama hasta LK-24 y LK-11b. Tocar `Core`, `Demo` o el tester.
