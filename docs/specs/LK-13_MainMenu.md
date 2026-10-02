# LK-13 + LK-17 — Menú de inicio mínimo y carga entre escenas
Estado: 🟡 implementado, pendiente de verificar en el editor (Sesión 12, 2026-10-02). Verificador: todos los técnicos (la primera pasada la paró TestBench sin guardar; capturas en `Temp/Captures/LK13_*`). Revisor: APROBADO CON CAMBIOS, 6 aplicados. Tope 1,5 h. Entra al MVP por la entrega (usuario) · Depende de: LK-14 (`DemoSceneBuilder`), LK-23 (audio), LK-50 (`LumiButton`), LK-03 · Diseño: GDD §1.8 (línea 199), §3.2 (líneas 669, 673), D-002, D-006, D-007, D-013

## Objetivo
`01_MainMenu` generada por script: título, subtítulo, "Demo 2D" y "Salir"; la demo vuelve al menú con un botón. La música suena desde el menú y no se
corta, reinicia ni duplica. Sin fondo de partículas, sin pantalla de carga, sin ajustes ni créditos.

## Archivos
- `Runtime/Scripts/Systems/SceneLoader.cs` (planeado) · estática, `LumiKit.Systems`. `Load(sceneName)`: si hay una carga en curso, nada; si la escena no está
  en la lista del build (`Application.CanStreamedLevelBeLoaded`), `LogError` y nada; si no, `UISound.Transition` (si `UIAudioManager.HasInstance`; es
  persistente y el sonido sobrevive al cambio) y `LoadSceneAsync` en modo `Single`. La bandera de carga se enciende sólo si la operación no es null y se
  apaga en `AsyncOperation.completed`; también se reinicia con `RuntimeInitializeOnLoadMethod(SubsystemRegistration)`. `Quit()`: `Application.Quit()`, sin
  `UnityEditor`; en el editor no hace nada.
- `Runtime/Scripts/Systems/SceneMenu.cs` (planeado) · `LumiKit.Systems` (Systems está por encima de UI, GDD §4.4, como `UISoundTrigger`). En `Awake` crea
  un botón por entrada de `_entries` clonando `_buttonTemplate` (inactivo, `flexibleWidth` 0) dentro de `_container`: fija `Style` y texto, activa, y después
  mide. Entrada (`MenuEntry`, mismo archivo): texto, `LumiButtonStyle`, acción (`LoadScene` / `Quit`) y escena. Ancho: `_buttonWidth`, o, con 0, el del texto
  más `LumiTheme.BUTTON_PADDING` a cada lado. Reciclable: cambiar la lista en el Inspector cambia los botones.
- `UI/LumiButton.cs` (✅ LK-50) · **sólo se añade** la propiedad `Style` (lectura y escritura; al escribir repinta al instante). Nada existente cambia.
- `UI/LumiTheme.cs` (✅) · se añaden `MENU_BUTTON_WIDTH` = `PANEL_WIDTH − 2 × PANEL_PADDING` (248) y `MENU_SECTION_GAP` = `4 × SPACING` (32).
- `Systems/UIAudioManager.cs` (✅ LK-23) · se añaden `UISound.Transition` (al final del enum) y `_transitionClip`.
- `Assets/Editor/UIAudioBuilder.cs` (✅) · `GeneratePrefab` asigna también `SFX_UI_Transition`; menú nuevo `LumiKit/Audio/Añadir sonido de transición (LK-17)`:
  lo asigna al `PRF_UIAudioManager` existente con `LoadPrefabContents` / `SaveAsPrefabAsset` / `UnloadPrefabContents` (D-002). Repetible.
- `Assets/Editor/MainMenuSceneBuilder.cs` (planeado) · menú `LumiKit/Escenas/Generar 01_MainMenu (LK-13)`. Mismas comprobaciones, borrado de raíces y
  guardado que `DemoSceneBuilder` (D-013, ampliada en `docs/DECISIONS.md`). `internal static BuildSceneMenu(...)` lo reutiliza la demo.
- `Assets/Editor/DemoSceneBuilder.cs` (✅ LK-14) · `SPR_Crystal` pasa a `MAT_Glow2D_Default` + `EFF_Glow2D`; `SPR_RuneCoin` sigue con contorno. Botón
  "Menú" arriba a la izquierda (`BuildSceneMenu`, una entrada Tertiary → `01_MainMenu`), último hijo de `UI_Root`; `CheckMounted` lo comprueba. Escape no se usa.
  `CanOpenScene` deja pasar sucias las **dos** escenas generadas (las dos quedan marcadas por el lienzo tras generarse); el menú hace lo mismo.
- `Assets/LumiKit/Scenes/01_MainMenu.unity` y `02_Demo_2D.unity` · salida de los generadores; los dos entran en el commit.
- Sin tocar: `Core/`, `Utils/`, `Demo/`, `ParameterPanelBuilder`, shaders, `ProjectSettings/` (la lista del build la pone el usuario).

## Contrato — `01_MainMenu`
| Raíz | Contenido |
|---|---|
| `Main Camera` | Ortográfica, Solid Color `Void`, post apagado, `AudioListener`. Sin controlador |
| `UI_Root` | `Canvas` Overlay, `CanvasScaler` 1920×1080 match height (D-007). `MenuColumn` centrado (`VerticalLayoutGroup`): "LumiKit" (`SpaceGrotesk-Medium`, `TEXT_DISPLAY`, `TextPrimary`), subtítulo "Shaders 2D para Unity 6 y URP" (`Inter-Medium`, `TEXT_H3`, `TextSecondary`), hueco `MENU_SECTION_GAP` y `Buttons` (`SceneMenu`, ancho `MENU_BUTTON_WIDTH`): "Demo 2D" Primary → `02_Demo_2D`, "Salir" Secondary → Quit. `Version` "v0.1 MVP" abajo a la derecha a `PANEL_PADDING` (Caption: `Inter-Regular`, `TEXT_CAPTION`, +0,02 em, `TextSecondary`: `TextMuted` sobre `Void` no llega a 4,5:1). Display va en `SpaceGrotesk-Medium`: el pack no trae la Bold del GDD |
| `EventSystem` | `InputSystemUIInputModule` (D-006) |
| `PRF_UIAudioManager` | Instancia del prefab en la raíz (DontDestroyOnLoad). Sin `ObjectSelector`, así que no se usa `MountInScene` |

Cada botón lleva `UISoundTrigger` con clic (LK-23): hover y clic suenan. Textos con `raycastTarget` = false.
**Música.** El `UIAudioManager` del menú sobrevive; el de la escena que llega es un duplicado que la base destruye en su `Awake` antes de tocar la música
(LK-23). Cada cambio de escena deja el aviso de duplicado de `Singleton` (`[LumiKit] Ya existe una instancia…`): esperado, no es un error.

## Banco de pruebas
`01_MainMenu` y `02_Demo_2D` regeneradas. El usuario pone en la lista del build `01_MainMenu` (activada, primera) y `02_Demo_2D`: sin ellas,
`SceneLoader` da `LogError` y no carga. Salir sólo se prueba en el build.

## Criterios de aceptación
**Verificador (MCP).** Compila sin errores ni warnings nuevos. Menús en orden: transición, `01_MainMenu`, `02_Demo_2D`, sin errores. Raíz de `01_MainMenu`:
`Main Camera`, `UI_Root`, `EventSystem`, `PRF_UIAudioManager`. `02_Demo_2D` con la misma raíz que en LK-14; `SPR_Crystal` con `EFF_Glow2D`, `SPR_RuneCoin`
con `EFF_Outline2D`. Play en `01_MainMenu`: un `UIAudioManager`, botones "Demo 2D" y "Salir" creados; Stop sin excepciones. Sin `using UnityEditor` en
`Assets/LumiKit/` (el comentario de `ColorParameterWidget.cs:12` no cuenta); sin `.mat` modificado; `LiberationSans SDF - Fallback.asset` sin cambios; `grep -rniE "coplay|mcpforunity" Assets/LumiKit/` da 0.

**Usuario (ojo).**
- [ ] Play desde `01_MainMenu`: título, subtítulo, dos botones y versión, legibles y centrados; suena la música. Hover y clic suenan.
- [ ] "Demo 2D": suena la transición y carga la demo; la música sigue sin cortarse ni reiniciarse.
- [ ] Demo: cristal con brillo, runa con contorno; "Menú" arriba a la izquierda vuelve al menú.
- [ ] Menú → demo → menú → demo varias veces: música continua y única, un solo `UIAudioManager` en `DontDestroyOnLoad`, consola sin errores.
- [ ] Build: arranca en el menú; "Salir" cierra la aplicación.

## Fuera de alcance
Fondo de partículas (GDD línea 199), pantalla de carga, ajustes, créditos, navegación con teclado o mando, Escape, estados `MAIN_MENU` del GDD §1.11, `00_Splash`.
