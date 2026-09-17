# Estado del proyecto
Actualizado: 2026-09-17 · Sesión 04

## Ahora
- Fase: 3 (Interfaz) abierta. LK-11 partida en BACKLOG: LK-11a (hoy) y LK-11b (Color, Toggle, Enum).
- Tarea activa: LK-11a 🟡 implementado, sin abrir Unity. Spec: `docs/specs/LK-11a_ParameterPanelUI.md`.
- Siguiente: verificar LK-11a en el editor; después LK-11b, que reusa `ParameterWidgetBase`.

## Últimas 3 sesiones
| Sesión | Fecha | Tarea | Resultado | Commit |
|---|---|---|---|---|
| 01 | 2026-09-11 | LK-09 EffectDefinition | ✅ | b2cd34c |
| 02 | 2026-09-15 | LK-12 DemoCameraController | ✅ | 434636d |
| 03 | 2026-09-16 | LK-10 ObjectSelector | ✅ | 6d4da8d |

Sesión 00 archivada en `docs/archive/sesiones_2026-Q3.md`.

## Pendiente de verificación en Unity
LK-11a. Antes: importar TMP Essential Resources, añadir 2 parámetros `Float` a `EFF_Debug.asset`
y ejecutar los dos menús `LumiKit/UI/…`. Criterios completos en la spec.
- [ ] Compila sin errores ni warnings nuevos, `LumiKit.Editor` sin referencias en rojo y los textos se leen.
- [ ] Ejecutar el menú de generación por segunda vez: error y ningún `.prefab` tocado.
- [ ] Sin selección: el panel no se ve. Clic en `SPR_Crystal`: cabecera + un slider por `Float`, en su valor por defecto.
- [ ] Arrastrar un slider: el valor numérico sigue al arrastre y la consola queda muda.
- [ ] **Arrastrar un slider no panea la cámara ni cambia la selección** (cierra el issue de la Sesión 02).
- [ ] Clic sobre el fondo del panel: la selección no cambia.
- [ ] Mover un slider, deseleccionar y reseleccionar: vuelve con el valor movido.
- [ ] Clic en vacío: el panel se vacía y se oculta. A → B → A: sin widgets duplicados.
- [ ] El `Color` de `EFF_Debug` no genera widget y avisa una vez por reconstrucción.
- [ ] El `EffectDebugTester` sigue funcionando y `MAT_Debug.mat` no cambia tras salir de Play (D-001).
- [ ] (opcional) `Refresh from controller` en el menú contextual del panel tras un `ResetToDefaults()` del tester.

## Entorno confirmado
- Unity 6000.0.83f1 · URP 17.0.4 · Input System 1.19.0 · uGUI 2.0.0 · 2D Sprite 1.0.0.
- Active Input Handling = Input System Package (New) (`activeInputHandler: 1`), confirmado
  por el usuario. Única API y forma de lectura: D-006.
- Remoto `origin` = https://github.com/PixelAlFuego/LumiKit.git
- **Push siempre requiere autorización del usuario.** Claude commitea; no sube.
- Los Shader Graph los construye el usuario. Claude entrega la especificación del grafo.
- D-001 confirmado en la práctica: tras salir de Play Mode, `MAT_Debug.mat` sin cambios
  en `git status` y el Mesh Renderer sin "(Instance)".

## Dudas abiertas y bloqueos
- **Diferidos a LK-01, no cumplidos:** verificación visual de `SetEffectEnabled` y del
  color en espacio Linear. Los criterios viven en `docs/specs/LK-01_Outline2D.md`.
- **Borrar al cerrar la Fase 5:** de `Assets/_Development/`, sólo `EffectDebugTester.cs`,
  `EFF_Debug.asset` y `MAT_Debug.mat`. Desechables, fuera del pack y deliberadamente
  ausentes de CODEMAP y BACKLOG. Los reemplaza LK-11.
- **`Assets/_Development/TestBench.unity` no se borra:** es el banco de pruebas permanente
  y crece con cada tarea (sección "Banco de pruebas" de cada spec). No se exporta.
- `Assets/Settings/DefaultVolumeProfile.asset` cambió solo (migración de Unity al
  importar) y entró en el commit `2cc98ff` sin revisión. Fuera del alcance de LK-09.
- `EffectController.cs` salió de 316 líneas, más de las ~140 estimadas en el plan.
- `Assets/TutorialInfo/` y `Assets/Readme.asset` son plantilla de Unity, fuera del pack.
  También `Assets/InputSystem_Actions.inputactions`: es el asset de acciones de proyecto
  (`EditorBuildSettings.asset`). D-006 prohíbe usarlo desde el pack.
- **Issue de la Sesión 02, ejecutable desde LK-11a:** ya hay `Canvas` y `EventSystem`, así que
  `IsPointerOverUI` de LK-10 y LK-12 por fin se puede probar; es criterio de LK-11a. Lo que sigue
  abierto es el `OnGUI` del `EffectDebugTester`, que no pasa por `EventSystem` y no bloquea el
  ratón: se cierra al retirar el tester, al cerrar la Fase 3.
- **`Assets/TextMesh Pro/` (4 MB) apareció durante la Sesión 04**, importado por el usuario: el
  panel lo necesita. Queda **sin commitear y sin `.gitignore`**: decisión del usuario si entra al
  repo. Fuera de `Assets/LumiKit/`, no se exporta, pero sí es dependencia del proyecto del
  comprador: documentar en LK-26 junto a la de Input System.
- **Prefabs de UI aún no existen en disco:** los crea el usuario con el menú de LK-11a, por eso no
  están en CODEMAP. Se añaden al verificar.
- `Assets/_Development/SPR_Crystal.png` y `SPR_RuneCoin.png`: borradores del usuario, no del
  pack. No van a CODEMAP ni se mueven a `Assets/LumiKit/`. Se usan como marcadores en TestBench.
- **Nombres y posiciones de TestBench:** la spec de LK-12 escribió `Marker_Crystal` en (-3,0,0) y
  `Marker_RuneCoin` en (3,0,0). El estado real, confirmado por el usuario en la Sesión 03, es
  `SPR_Crystal` en (3,0,0) y `SPR_RuneCoin` en (-3,0,0). La spec de LK-10 usa los nombres reales.
  La de LK-12 está cerrada y no se toca: decisión del usuario.
- `ProjectSettings/TagManager.asset` entró en el cierre de LK-10 con la capa `Selectable` (índice
  6) que creó el usuario. Unity aprovechó para migrarlo a `serializedVersion: 3` y borrar las
  entradas de capa vacías del final. Migración del editor, no revisada línea a línea.
- Rama única `main`. `develop` y `feature/LK-XX-*` del GDD §4.9 aún no creadas.

## Handoff
Empezar por: verificar LK-11a con la checklist de arriba. Si pasa, LK-11b (Color, Toggle, Enum y el
pie con Reset y Copiar); su spec está por redactar.
Para LK-11b: heredar de `ParameterWidgetBase` y devolver su `SupportedType`; el panel sólo necesita
un campo de prefab nuevo y un `case` en `PrefabFor`. El botón Reset llama a
`EffectController.ResetToDefaults()` y luego a `ParameterPanelUI.RefreshFromController()`, que ya
existe. Nombres de clase: D-008.
No tocar: `Scripts/Core/` y `Scripts/Utils/` (verificados en LK-09), `Demo/` (LK-10 y LK-12
verificados), `EffectDebugTester.cs` hasta cerrar la Fase 3, `Assets/LumiKit/Scenes/`,
`ProjectSettings/`, `Packages/manifest.json`, nada de 3D ni VFX.
