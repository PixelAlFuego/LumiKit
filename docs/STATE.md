# Estado del proyecto
Actualizado: 2026-09-10 · Sesión 00

## Ahora
- Fase: 0 — Andamiaje
- Tarea activa: ninguna
- Siguiente: LK-09

## Últimas 3 sesiones
| Sesión | Fecha | Tarea | Resultado | Commit |
|---|---|---|---|---|
| 00 | 2026-09-10 | Andamiaje | 🟡 | [LK-00] |

## Pendiente de verificación en Unity
- [ ] Unity regenera los `.meta` de las carpetas y archivos nuevos al recuperar foco.
- [ ] `LumiKit.Runtime.asmdef` y `LumiKit.Editor.asmdef` compilan sin referencias rotas.
      Nombres de ensamblado usados sin poder verificarlos: `Unity.TextMeshPro`,
      `Unity.RenderPipelines.Universal.Runtime`, `Unity.InputSystem`, `UnityEngine.UI`.
- [ ] Project Settings → Editor → Version Control Mode = **Visible Meta Files** (GDD §4.9).
- [ ] Project Settings → Editor → Asset Serialization = **Force Text** (GDD §4.9).

## Dudas abiertas y bloqueos
- Las 5 escenas `00_Splash` … `04_Demo_VFX` ya existen en `Assets/LumiKit/Scenes/`,
  creadas antes de esta sesión. Contenido desconocido. LK-14 y LK-13 deben decidir si
  se pueblan o se regeneran por script. No borrar sin autorización.
- `Assets/TutorialInfo/` y `Assets/Readme.asset` son plantilla de Unity, fuera del pack.
  No se tocan sin que lo pidas.
- Git LFS declarado en `.gitattributes` pero no verificado (`git lfs install`).

## Handoff
Empezar por: LK-09 — leer `docs/specs/LK-09_EffectDefinition.md`.
Archivos a tocar: `Assets/LumiKit/Runtime/Scripts/Core/`, `.../Utils/`.
No tocar: `Assets/LumiKit/Scenes/`, `Assets/TutorialInfo/`, `ProjectSettings/`,
`Packages/manifest.json`, nada de 3D ni VFX.
