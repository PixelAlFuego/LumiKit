# LK-14 — Escena `02_Demo_2D`
Estado: ✅ verificado por el usuario (Sesión 12, 2026-10-02): arranca con música, objetos desplazados y texto legible; consola limpia; selección, panel, grosor, color, tres modos y TAB; cada objeto conserva sus valores con el material compartido y Reset sólo afecta al seleccionado; cámara, hover y clic; el texto no bloquea clics. Lista del build del usuario en el cierre (`EditorBuildSettings.asset`). Verificador: 7 de 7 técnicos en la segunda pasada (la primera halló `_definition` vacío y la escena sucia). Revisor: APROBADO CON CAMBIOS, 12 menores aplicados; el bloqueante, resuelto por el usuario (opción A, D-013). Usuario: cámara desplazada, texto de controles, 16:9 y 16:10 · Depende de: LK-01 (`MAT_Outline2D_Default`, `EFF_Outline2D`), LK-10, LK-11a/b (`ParameterPanelBuilder`), LK-12, LK-23 (`UIAudioBuilder`), LK-24 (`ComparisonToggle`), LK-20 (sprites) · Diseño: GDD §1.8 (líneas 182-184), §1.7 (líneas 150-156), §2 paleta (línea 399), D-001, D-002, D-006, D-007, D-013

## Objetivo
Generar por script la escena que se enseña y con la que arranca el build de Windows: `SPR_Crystal` y `SPR_RuneCoin` con contorno, selección,
panel, cámara, comparación con TAB y audio. Nada de pruebas: ni `EffectDebugTester`, ni cubo, ni marcadores, ni `EFF_Debug`/`MAT_Debug`.

## Archivos
- `Assets/Editor/DemoSceneBuilder.cs` (planeado) · menú `LumiKit/Escenas/Generar 02_Demo_2D (LK-14)`. Nuevo. Sin diálogos: lo lanza el verificador por MCP.
- `Assets/LumiKit/Scenes/02_Demo_2D.unity` (existe; sólo la `Main Camera` en perspectiva y la `Directional Light` de plantilla) · salida del generador (D-002).
- Reutiliza sin tocarlos: `ParameterPanelBuilder.BuildSceneRig()` (`UI_Root`, `EventSystem` con `InputSystemUIInputModule`, panel con su `_selector`)
  y `UIAudioBuilder.MountInScene()` (`PRF_UIAudioManager` y `UISelectionSound` en el `ObjectSelector`). Los dos son `public static`, actúan
  sobre la escena activa y buscan el `ObjectSelector`, que por eso se crea antes.
- Sin tocar: `Assets/LumiKit/Runtime/`, los demás generadores, TestBench, `ProjectSettings/` (la lista del build la pone el usuario).

## Contrato — el generador
1. **Comprueba antes de tocar nada.** Si algo falla: `Debug.LogError` con el motivo y no hace nada. No en Play; ninguna escena abierta con
   cambios sin guardar (abrir otra los perdería), salvo la propia `02_Demo_2D`, que se regenera igual; ningún prefab abierto en Prefab Mode (`OpenScene` podría sacar un diálogo y colgar el MCP);
   existen `SPR_Crystal`, `SPR_RuneCoin` (los dos con `GetPhysicsShapeCount()` > 0), `MAT_Outline2D_Default`, `EFF_Outline2D`,
   `PRF_ParameterPanel` y `PRF_UIAudioManager`; existe la capa `Selectable`.
2. **Abre `02_Demo_2D`** (`OpenScene`, `Single`) y destruye sus objetos raíz, también los de plantilla: regenerar parte de cero y un cambio a mano se pierde (D-002).
3. **Construye**, con valores por `SerializedObject` en los campos privados:

| Objeto | Componentes y valores iniciales |
|---|---|
| `Main Camera` (tag `MainCamera`) | `Camera` ortográfica, Size 2,5, en (x, 0, −10) con x = medio ancho del panel en mundo = `PANEL_WIDTH / REFERENCE_HEIGHT × Size` (0,65 u; con match = height no depende del aspecto): la composición queda centrada en la zona libre a la izquierda del panel (usuario, Sesión 12). Solid Color `LumiTheme.Void` (`#0D0F14`). `UniversalAdditionalCameraData` añadido por el generador con `renderPostProcessing` = false (si lo añade el Inspector al seleccionar la cámara, ensucia la escena). `AudioListener` (sin él no suena nada). `DemoCameraController`: `_bounds` (−4, −3, 8, 6), Size 1,5 a 5 |
| `SPR_Crystal` en (−1,5, 0, 0) · `SPR_RuneCoin` en (1,5, 0, 0) | Capa `Selectable`. `SpriteRenderer` con su sprite y `MAT_Outline2D_Default`. `PolygonCollider2D` con la forma física del sprite: `pathCount` = `GetPhysicsShapeCount()` y cada camino con `GetPhysicsShape` y `SetPath` (firmas del motor, sin comprobar en paquetes). `EffectController`: `_targetRenderer` = su `SpriteRenderer`, `_definition` = `EFF_Outline2D` |
| `Systems` | `ObjectSelector` (`_camera` = `Main Camera`, `_selectableLayers` = sólo `Selectable`) y `ComparisonToggle` (LK-24) con `_selector` asignado a ese `ObjectSelector` |
| `UI_Root`, `EventSystem`, panel | `ParameterPanelBuilder.BuildSceneRig()` |
| `PRF_UIAudioManager` | `UIAudioBuilder.MountInScene()` |
| `ControlsHint`, primer hijo de `UI_Root` | **Temporal** (usuario, Sesión 12; se quitará): `TextMeshProUGUI` fijo "Clic: seleccionar · Clic derecho: mover · Rueda: zoom · TAB: comparar", `Inter-Medium SDF` (Label, "·" en el atlas) a `TEXT_LABEL`, `TextSecondary`, abajo a la izquierda a `PANEL_PADDING`, sin entrar bajo el panel. `raycastTarget` = false: si no, bloquearía clic, rueda y paneo |

4. **Comprueba** lo que los dos métodos sólo avisan si falla: `ParameterPanelUI` con `_selector` asignado, `UIAudioManager` y `UISelectionSound`; y `ControlsHint`. Si falta algo: `LogError` y no guarda.
5. **Guarda** (opción A del usuario, D-013): `EditorSceneManager.SaveScene(scene)` en su sitio (el GUID no cambia), comprueba el `bool` y su último
   log dice que guardó (los de los métodos reutilizados dicen "NO se ha guardado"). Tras generar, no deshacer con Ctrl+Z (el borrado de raíces no
   está en Undo): relanzar. Al terminar, el editor se queda en `02_Demo_2D`, marcada como modificada: el lienzo da tamaño a su
   `RectTransform` raíz en la primera actualización del editor, ya guardada (valores que maneja el `Canvas`; inofensivo).
   Los assets se cargan después de `OpenScene`: cargados antes, la primera prueba guardó `_definition` vacío. El paso 4 lo comprueba.

Los dos sprites comparten `MAT_Outline2D_Default` y cada uno lleva sus valores en su `MaterialPropertyBlock` (D-001): editar uno no cambia el otro.
Las cifras de cámara y posiciones son de partida: con Size 2,5, los dos sprites (2 u cada uno) quedan a la izquierda del panel (280 px de 1920).

**Build (el usuario).** Lista de escenas del build: añadir `02_Demo_2D` y desmarcar `01_MainMenu` (`00_Splash` no está); `02_Demo_2D` pasa a ser la
primera. **Resolución:** el MVP soporta 16:9 y 16:10; 4:3, aparcado (`docs/MVP_SCOPE.md`). Se arranca sin menú (LK-13 y LK-17 fuera del MVP). El índice 0 no coincide con el prefijo `02` (CONVENTIONS > Escenas): se acepta para el MVP.
Salir de la build: cerrar la ventana (sin menú de pausa, LK-32 fuera).

## Banco de pruebas
La propia `02_Demo_2D` y el build de Windows. TestBench no se toca y sigue con sus marcadores y el tester. Antes de lanzar el generador,
TestBench guardada (el paso 1 aborta si no). `SPR_Lumi` no está (LK-20): no bloquea.

## Criterios de aceptación
**Verificador (MCP).** Compila sin errores ni warnings nuevos. El menú genera la escena sin errores. Raíz: `Main Camera`, `SPR_Crystal`,
`SPR_RuneCoin`, `Systems`, `UI_Root`, `EventSystem`, `PRF_UIAudioManager` y nada más; sin errores ni warnings `[LumiKit]`. Relanzarlo deja la misma raíz
y `02_Demo_2D.unity.meta` sin cambios en git. `02_Demo_2D.unity` no contiene ningún GUID de un `.meta` de `Assets/_Development/` ni `052faaac…`
(`InputSystem_Actions`). Los dos `PolygonCollider2D` con al menos un camino. En Play: consola sin errores ni avisos de LK-49; Stop sin excepciones; ningún `.mat`
modificado. Captura de la vista Game en `Temp/Captures`. `grep -rniE "coplay|mcpforunity" Assets/LumiKit/` da 0.

**Usuario (ojo).**
- [ ] Play: fondo liso oscuro, cristal a la izquierda y runa a la derecha, los dos con contorno cian; suena la música.
- [ ] Clic en cada uno: suena Select y se abre "Contorno 2D". Cambiar el grosor de la runa no cambia el del cristal.
- [ ] Clic en una esquina transparente de cada sprite: no selecciona (el collider sigue la silueta, no el rectángulo Full Rect).
- [ ] TAB con un objeto seleccionado: sin contorno mientras se mantiene. Clic en vacío cierra el panel.
- [ ] WASD, botón derecho y rueda mueven la cámara sin perder los objetos; con la cámara en reposo el panel no tapa la runa.
- [ ] Hover y clic de los controles del panel suenan.
- [ ] Texto de controles abajo a la izquierda, discreto y legible; con el cursor encima, clic, rueda y paneo siguen funcionando.
- [ ] Build de Windows con la lista de arriba: arranca directo en `02_Demo_2D` y repite los cuatro primeros puntos.

## Fuera de alcance
- Fondo degradado y retícula (GDD líneas 184 y 543): fondo liso `Void`. Lumi (LK-20). Glow en la runa: lo decide LK-03 si entra.
- Prefabs `PRF_DemoObject_*` y `HoverAnchor` (GDD §4.8) · barra superior, indicador de controles que se desvanece (el texto fijo lo sustituye en el MVP) y contador (GDD §1.6) · `PRF_LumiKitManagers` · 4:3.
- Menú, carga entre escenas y `00_Splash` (LK-13, LK-17, fuera del MVP) · cambiar la lista del build por script.
