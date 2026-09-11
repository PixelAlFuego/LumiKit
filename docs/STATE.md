# Estado del proyecto
Actualizado: 2026-09-10 · Sesión 01

## Ahora
- Fase: 1 — Núcleo de datos
- Tarea activa: LK-09 🟡 (7 archivos escritos, sin verificar en Unity)
- Siguiente: LK-10

## Últimas 3 sesiones
| Sesión | Fecha | Tarea | Resultado | Commit |
|---|---|---|---|---|
| 00 | 2026-09-10 | Andamiaje | ✅ | dfd6fe7 |
| 01 | 2026-09-10 | LK-09 EffectDefinition | 🟡 | b2cd34c |

## Pendiente de verificación en Unity
Banco de pruebas: material URP de stock (`Universal Render Pipeline/Unlit`) + `_BaseColor`,
con un `EFF_Debug.asset` desechable en `Assets/_Development/`. No hay shader del pack aún.
Disparador: `Assets/_Development/Tests/EffectDebugTester.cs` (OnGUI, 3 sliders RGB +
botones de efecto y reset). El `EFF_Debug.asset` debe declarar un parámetro `Color`
con PropertyName exactamente `_BaseColor`.

- [ ] Compila sin errores ni warnings nuevos.
- [ ] `Create → LumiKit → Effect Definition` y `→ Effect Registry` generan el `.asset`.
- [ ] Inspector del `.asset`: la lista de parámetros se edita bien.
- [ ] `EffectController` sobre un objeto con Renderer: cambiar `_BaseColor` por código en
      Play Mode altera el objeto.
- [ ] Al salir de Play Mode el `.mat` **no** aparece modificado en disco (D-001).
- [ ] `ResetToDefaults()` restaura los valores por defecto.

## Entorno confirmado
- Unity 6000.0.83f1 · URP 17.0.4 · Input System 1.19.0 · uGUI 2.0.0.
- Remoto `origin` = https://github.com/PixelAlFuego/LumiKit.git
- **Push siempre requiere autorización del usuario.** Claude commitea; no sube.
- Los Shader Graph los construye el usuario. Claude entrega la especificación del grafo.

## Dudas abiertas y bloqueos
- **Color en espacio Linear, sin resolver.** No está confirmado si
  `MaterialPropertyBlock.SetColor` aplica la conversión gamma→lineal. La corrección, si
  hace falta, va sólo en `MaterialPropertyHelper.ConvertColor`. Diferido a LK-01.
- **`SetEffectEnabled` no verificable aún.** Escribe `_EffectEnabled`, pero ningún shader
  la declara todavía. Criterio diferido a `docs/specs/LK-01_Outline2D.md`.
- **Borrar al cerrar la Fase 5:** `Assets/_Development/Tests/EffectDebugTester.cs` y el
  `EFF_Debug.asset`. Código desechable, fuera del pack, deliberadamente ausente de
  CODEMAP y BACKLOG. Lo reemplaza el panel de parámetros real (LK-11).
- `EffectController.cs` salió de 316 líneas, más de las ~140 estimadas en el plan.
- `Assets/TutorialInfo/` y `Assets/Readme.asset` son plantilla de Unity, fuera del pack.
- Rama única `main`. `develop` y `feature/LK-XX-*` del GDD §4.9 aún no creadas.

## Handoff
Empezar por: LK-10 — falta redactar `docs/specs/LK-10_ObjectSelector.md`.
Archivos a tocar: `Assets/LumiKit/Runtime/Scripts/Demo/`.
No tocar: `Scripts/Core/` (cerrado en LK-09), `Assets/LumiKit/Scenes/`,
`ProjectSettings/`, `Packages/manifest.json`, nada de 3D ni VFX.
