# Alcance del MVP — modo sprint
Rama `sprint/mvp` · Entrega: viernes 2026-10-02 por la tarde · Punto seguro: tag `v0.3-pre-mcp`
**Recorte por enfermedad (usuario, Sesión 12):** el 2026-10-02 es el último día de trabajo.

## Dentro
| Tarea | Qué | Tiempo | Se para a (+50 %) | ¿Se recorta? |
|---|---|---|---|---|
| LK-23 | Sonidos de interfaz · ✅ | 2,5 h | 3 h 45 | Nunca |
| LK-01 | Outline 2D · ✅ | 3 h | 4 h 30 | Nunca |
| LK-24 | Comparación antes / después (TAB) · 2026-10-02 | 1,5 h | 2 h 15 | Nunca |
| LK-14 | Escena `02_Demo_2D` · 2026-10-02; el build arranca en ella | 2,5 h | 3 h 45 | Nunca |
| LK-03 | Glow 2D · sólo si LK-14 queda cerrada antes de las 11:00 del 2026-10-02 | 2 h | 3 h | Sí |
| LK-20 | Sprites: Cristal y Runa definitivos; Lumi si llega | usuario | — | Lumi no bloquea LK-14 |
| LK-13 + LK-17 | Menú de inicio mínimo y `SceneLoader` · 2026-10-02, prioridad máxima | 1,5 h (tope) | 1,5 h | No |

El 2026-10-02: LK-24 + LK-14 = 4 h (6 h con el +50 %). LK-03 suma 2 h si entra.

## Entregables
- Build de Windows: arranca en `01_MainMenu` (LK-13); "Demo 2D" lleva a `02_Demo_2D` (LK-14). El orden del build lo pone el usuario.
- LK-27: el `.unitypackage` se exporta el 2026-10-02; la prueba de importación en un proyecto vacío pasa a la semana del 2026-10-05.
- Resolución: 16:9 y 16:10 (LK-14).
- LK-37: vídeo corto. Lo graba el usuario.

## Fuera
- 3D y VFX · localización (LK-19) · portapapeles (LK-29) · menú de pausa (LK-32) · notificaciones (LK-34).
- `LumiSkin` · asmdef de editor dentro del pack · accesibilidad del foco · refactors.
- Recortado por enfermedad (usuario, Sesión 12): LK-02 Dissolve 2D. LK-13 + LK-17 vuelven, en versión mínima (usuario, 2026-10-02): la entrega exige menú.

## Recorte
- LK-02, ya recortada. LK-03 entró por decisión del usuario (✅). LK-13 + LK-17, mínimos, con tope de 1,5 h.
- Nunca: LK-23, LK-01, LK-24, LK-14.
- Una tarea que supera su tiempo en un 50 % se para y se recorta.

## Aparcado
Toda idea nueva va aquí en una línea, sin implementarla.
- Versiones Shader Graph de LK-01, LK-03 y LK-02, para la versión de Asset Store (D-010).
- Botón "Comparar" en pantalla (GDD línea 59) e indicador de controles del GDD (línea 137: se desvanece, tecla H). El MVP lleva un texto fijo temporal en `02_Demo_2D` (LK-14), que se quitará.
- 4:3: la composición de `02_Demo_2D` no se garantiza (usuario, Sesión 12).
- Halo de LK-03 sin capas a intensidad alta: fundido o difuminado entre anillos, o más muestras; probarlo y devolver el máximo del `EFF` a 5.
