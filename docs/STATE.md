# Estado del proyecto
Actualizado: 2026-09-11 · Sesión 01

## Ahora
- Fase: 1 cerrada → 2 (Interacción)
- Tarea activa: ninguna
- Siguiente: LK-10

## Últimas 3 sesiones
| Sesión | Fecha | Tarea | Resultado | Commit |
|---|---|---|---|---|
| 00 | 2026-09-10 | Andamiaje | ✅ | dfd6fe7 |
| 01 | 2026-09-11 | LK-09 EffectDefinition | ✅ | b2cd34c |

## Pendiente de verificación en Unity
- [ ] (nada)

## Entorno confirmado
- Unity 6000.0.83f1 · URP 17.0.4 · Input System 1.19.0 · uGUI 2.0.0.
- Remoto `origin` = https://github.com/PixelAlFuego/LumiKit.git
- **Push siempre requiere autorización del usuario.** Claude commitea; no sube.
- Los Shader Graph los construye el usuario. Claude entrega la especificación del grafo.
- D-001 confirmado en la práctica: tras salir de Play Mode, `MAT_Debug.mat` sin cambios
  en `git status` y el Mesh Renderer sin "(Instance)".

## Dudas abiertas y bloqueos
- **Diferidos a LK-01, no cumplidos:** verificación visual de `SetEffectEnabled` y del
  color en espacio Linear. Los criterios viven en `docs/specs/LK-01_Outline2D.md`.
- **Borrar al cerrar la Fase 5:** `Assets/_Development/` entero — `EffectDebugTester.cs`,
  `EFF_Debug.asset`, `MAT_Debug.mat`, `TestBench.unity`. Desechable, fuera del pack y
  deliberadamente ausente de CODEMAP y BACKLOG. Lo reemplaza LK-11.
- `Assets/Settings/DefaultVolumeProfile.asset` cambió solo (migración de Unity al
  importar) y entró en el commit `2cc98ff` sin revisión. Fuera del alcance de LK-09.
- `EffectController.cs` salió de 316 líneas, más de las ~140 estimadas en el plan.
- `Assets/TutorialInfo/` y `Assets/Readme.asset` son plantilla de Unity, fuera del pack.
- Rama única `main`. `develop` y `feature/LK-XX-*` del GDD §4.9 aún no creadas.

## Handoff
Empezar por: LK-10 — falta redactar `docs/specs/LK-10_ObjectSelector.md`.
Archivos a tocar: `Assets/LumiKit/Runtime/Scripts/Demo/`.
No tocar: `Scripts/Core/` y `Scripts/Utils/` (cerrados y verificados en LK-09),
`Assets/LumiKit/Scenes/`, `ProjectSettings/`, `Packages/manifest.json`, nada de 3D ni VFX.
