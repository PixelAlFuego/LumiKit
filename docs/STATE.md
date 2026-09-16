# Estado del proyecto
Actualizado: 2026-09-16 · Sesión 03

## Ahora
- Fase: 2 (Interacción) cerrada · LK-12 ✅ · LK-10 ✅. LK-24 pasa a la Fase 5, tras LK-01.
- Tarea activa: ninguna
- Siguiente: LK-11 (Fase 3), que consume `ObjectSelector.OnSelectionChanged`.

## Últimas 3 sesiones
| Sesión | Fecha | Tarea | Resultado | Commit |
|---|---|---|---|---|
| 01 | 2026-09-11 | LK-09 EffectDefinition | ✅ | b2cd34c |
| 02 | 2026-09-15 | LK-12 DemoCameraController | ✅ | 434636d |
| 03 | 2026-09-16 | LK-10 ObjectSelector | ✅ | 6d4da8d |

Sesión 00 archivada en `docs/archive/sesiones_2026-Q3.md`.

## Pendiente de verificación en Unity
- [ ] (nada)

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
- **Issue conocido → LK-11:** paneo (LK-12) y selección (LK-10) deben ignorar el ratón con
  el puntero sobre la UI. `EffectDebugTester` dibuja con `OnGUI`, que no pasa por
  `EventSystem`: hoy su panel no bloquea el ratón. Se cierra cuando LK-11 lo sustituya.
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
Empezar por: LK-11 — redactar `docs/specs/LK-11_ParameterPanelUI.md` (planeado). Abre la Fase 3.
Para LK-11: la selección se consume por `OnSelectionChanged`, nunca sondeando `Selected` cada frame.
`Selected.Definition` da el `EffectDefinition` con el que poblar el panel. LK-11 trae el
`EventSystem` con `InputSystemUIInputModule` (D-006) y cierra dos issues conocidos: el ratón sobre
la UI en LK-10 y LK-12, y el `OnGUI` del `EffectDebugTester`, al que sustituye.
No tocar: `Scripts/Core/` y `Scripts/Utils/` (verificados en LK-09), `Demo/DemoCameraController.cs`
(verificado en LK-12), `Assets/LumiKit/Scenes/`, `ProjectSettings/`, `Packages/manifest.json`,
nada de 3D ni VFX.
