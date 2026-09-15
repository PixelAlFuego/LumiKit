# Estado del proyecto
Actualizado: 2026-09-15 · Sesión 02

## Ahora
- Fase: 2 (Interacción) · LK-12 ✅. LK-24 pasa a la Fase 5, tras LK-01.
- Tarea activa: ninguna
- Siguiente: LK-10

## Últimas 3 sesiones
| Sesión | Fecha | Tarea | Resultado | Commit |
|---|---|---|---|---|
| 00 | 2026-09-10 | Andamiaje | ✅ | dfd6fe7 |
| 01 | 2026-09-11 | LK-09 EffectDefinition | ✅ | b2cd34c |
| 02 | 2026-09-15 | LK-12 DemoCameraController | ✅ | 434636d |

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
- Rama única `main`. `develop` y `feature/LK-XX-*` del GDD §4.9 aún no creadas.

## Handoff
Empezar por: LK-10 — redactar `docs/specs/LK-10_ObjectSelector.md` (planeado) con la plantilla
de `.claude/rules/docs-style.md`, incluida la sección Banco de pruebas.
Ya fijado para LK-10: sólo botón izquierdo (spec LK-12 > Reparto del ratón), raycast por
código sin `Physics2DRaycaster`, e ignorar el ratón sobre la UI (issue conocido → LK-11).
Archivos a tocar: `Assets/LumiKit/Runtime/Scripts/Demo/` (LK-10).
No tocar: `Scripts/Core/` y `Scripts/Utils/` (verificados en LK-09), `Demo/DemoCameraController.cs`
(verificado en LK-12), `Assets/LumiKit/Scenes/`, `ProjectSettings/`, `Packages/manifest.json`,
nada de 3D ni VFX.
