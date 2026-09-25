# Estado del proyecto
Actualizado: 2026-09-24 · Sesión 10 (en curso) · se cortó por un apagón y se reanudó con auditoría, sin pérdidas

## Ahora
- **Fase 3 (Interfaz) cerrada** (Sesión 10): LK-51, LK-22b y LK-50 ✅. Empieza la Fase 4.
- Tarea activa: ninguna.
- Siguiente: **LK-52** (valor editable en el slider, hallazgo de la verificación de LK-22b): spec escrita, sin aprobar. Después, LK-23.

## Últimas 3 sesiones
| Sesión | Fecha | Tarea | Resultado | Commit |
|---|---|---|---|---|
| 08 | 2026-09-20 | Cierre de LK-22a + specs de LK-50 y LK-51 | ✅ | 6947ac8 |
| 09 | 2026-09-22 | Música a MP3 + LK-51 sprites del pack, verificación parcial | 🟡 | 12d21e6 · 58f9c9c · bb94a11 |
| 10 | 2026-09-24 | Apagón y auditoría · LK-22b limpiador y fuentes · manija y cierre de LK-51 · LK-50 · spec de LK-52 | ✅ LK-51 · ✅ LK-22b · ✅ LK-50 · Fase 3 cerrada | 658d8fa · deef7b1 · f4b5b11 · 9b4832a · 81920ba · 37596da · 3c865e5 · cb27f92 · 6f85bb2 · este commit |

Sesiones 00 a 07 archivadas en `docs/archive/sesiones_2026-Q3.md`.

## Pendiente de verificación en Unity
Nada. LK-50 cerrada (Sesión 10) y, con ella, la Fase 3.

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
- **Audio, para LK-23.** En el repositorio desde la Sesión 08 (por LFS): `Audio/SFX/` con `SFX_UI_Click`, `SFX_UI_Hover`,
  `SFX_UI_Select`, `SFX_UI_Error` y `SFX_UI_Transition`, y `Audio/Music/MUS_Ambient_Loop.mp3`. **`SFX_UI_Error` y
  `SFX_UI_Transition` no entran en LK-23**: el primero espera a que haya avisos en pantalla y el segundo al cambio de
  escena de LK-17. LK-23 engancha sólo los otros tres.
- **Música en MP3, decisión del usuario (Sesión 09).** `MUS_Ambient_Loop` pasa de WAV (64 MB) a MP3 (4 MB), con Load Type
  Streaming, Vorbis al 70 % y sin preload (leído en el `.meta`). En WAV no bajaba de 5 MB y recortar el bucle lo rompía.
  Cierra la nota de los 64 MB. `*.mp3` va por LFS (`.gitattributes`). Cambia el GUID, pero nada referenciaba el `.wav`.
- **`MUS_` no existe como prefijo** en la tabla de CONVENTIONS.md ni en CLAUDE.md, aunque el archivo ya use ese nombre.
  Se decide en LK-23: o se añade el prefijo a la tabla, o la música se nombra de otra forma. No lo toco sin que me lo pidas.
- **Diferidos a LK-01, no cumplidos:** verificación visual de `SetEffectEnabled` y del color en
  espacio Linear. Los criterios viven en `docs/specs/LK-01_Outline2D.md`.
- **Botón del pie, deuda de LK-22a:** cerrada por LK-51 (contorno `SPR_UI_Rect_R6_Outline` y `LumiTheme.Transparent`), vista por el usuario.
- **`LumiButton` pinta `Selected` como reposo** (usuario, Sesión 10; con el cursor encima, como hover): así un usuario de teclado o mando
  no ve qué botón tiene el foco. El GDD no define ese estado. Se decide antes de publicar.
- **El pack no tiene dónde poner un script de editor propio.** `Assets/Editor/` no se exporta (CONVENTIONS) y ningún script bajo
  `Assets/LumiKit/` puede hacer `using UnityEditor`. Sale a la luz con LK-50: su `LumiButtonEditor` funcionará aquí pero el comprador
  no verá los campos del componente en el Inspector. Segunda herramienta, `FontFeatureCleaner` (LK-22b): D-009 da los `.ttf` al comprador, pero si regenera sin él sus fuentes vuelven a pesar 20 MB o más. Haría falta un tercer asmdef sólo-editor dentro del pack. Se decide en LK-27.
- **Borrar al cerrar la Fase 5:** de `Assets/_Development/`, sólo `EffectDebugTester.cs`, `EFF_Debug.asset` y `MAT_Debug.mat`.
  Desechables, fuera del pack, deliberadamente ausentes de CODEMAP y BACKLOG. El panel (LK-11) sustituyó al tester como UI de parámetros,
  pero no se retira: es el único disparador de `SetEffectEnabled`, que hará falta en la Fase 5 (corrección del usuario, Sesión 10).
- **`Assets/_Development/TestBench.unity` no se borra:** banco de pruebas permanente, crece con
  cada tarea (sección "Banco de pruebas" de cada spec). No se exporta.
- `Assets/Settings/DefaultVolumeProfile.asset` cambió solo (migración de Unity al importar) y entró en el commit `2cc98ff` sin revisión. Fuera del alcance de LK-09.
- `EffectController.cs` salió de 316 líneas, más de las ~140 estimadas en el plan.
- `Assets/TutorialInfo/` y `Assets/Readme.asset` son plantilla de Unity, fuera del pack. También
  `Assets/InputSystem_Actions.inputactions`, asset de acciones del proyecto: D-006 prohíbe usarlo desde el pack.
- **Issue de la Sesión 02:** sigue abierto el `OnGUI` del `EffectDebugTester`, que no pasa por `EventSystem` y no bloquea el ratón. Se cierra al retirar el tester, al cerrar la Fase 5.
- **`Assets/TextMesh Pro/` (4 MB) entra al repositorio** (usuario, Sesión 05): los prefabs la referencian por GUID. Documentar en LK-26.
- **`LiberationSans SDF - Fallback.asset` se reescribe solo:** es dinámico y guarda los glifos que le piden. Con la prueba visual de las fuentes
  (Sesión 10) ganó 25 caracteres. No se commitea; lo revierte el usuario. Con LK-22b el HUD ya no se lo pide: no cambió tras Play (usuario, Sesión 10).
- **Tamaño de texto por encima del GDD:** cerrado. Label y Mono a 16 px (GDD: 13) con la resolución de diseño en 1920×1080; anotado en `LumiTheme`, en D-007 y ya en `ui-style.md` (LK-22a).
- **`EFF_Debug.asset` tiene seis parámetros** y dos son deliberadamente distintos: `_Color` existe
  en el shader de los sprites y `_BaseColor` **no**. `_BaseColor` se queda como control negativo
  permanente de LK-49. Los marcadores usan `Sprite-Unlit-Default` (`_MainTex` y `_Color`) y el cubo, un shader URP
  que declara `_BaseColor` **y también `_Color`** en `ObsoleteProperties` (`Unlit.shader` línea 28): por eso reporta cuatro ausentes y no cinco.
- **`MAT_Debug.mat` usa `Universal Render Pipeline/Unlit`** (usuario, Sesión 06; en disco, GUID
  `650dd952…`): sin iluminación, lo acordado en LK-09 para poder cerrar el criterio de Linear.
- **Materiales de los sprites, para la Fase 5:** los marcadores comparten el material por defecto `Sprite-Unlit-Default`.
  Cada efecto necesitará el suyo (`MAT_` en `Assets/LumiKit/Materials/2D/`) o tocar un parámetro en uno los cambiará todos. Entra con LK-01.
- **Los topes no se suben** (usuario, Sesión 05): al llegar al tope se condensa (Sesión 07: `ui-style.md`, D-001 a D-003, por D-009).
- `Assets/_Development/SPR_Crystal.png` y `SPR_RuneCoin.png`: borradores del usuario, no del pack.
  No van a CODEMAP ni se mueven a `Assets/LumiKit/`. Se usan como marcadores en TestBench.
- **Nombres y posiciones de TestBench:** la spec de LK-12 escribió `Marker_Crystal` en (-3,0,0) y `Marker_RuneCoin` en (3,0,0). El estado
  real, confirmado en la Sesión 03, es `SPR_Crystal` en (3,0,0) y `SPR_RuneCoin` en (-3,0,0). LK-10 usa los nombres reales; LK-12 no se toca.
- `ProjectSettings/TagManager.asset` entró en el cierre de LK-10 con la capa `Selectable` (índice 6) que creó el usuario. Unity aprovechó
  para migrarlo a `serializedVersion: 3` y borrar las entradas de capa vacías del final. Migración del editor, no revisada línea a línea.
- **Enmienda pendiente a D-001:** autorizar la lectura de `sharedMaterials` (en plural) para validar todos los materiales de un `Renderer`,
  no sólo el primero. No instancia copias, que es lo que D-001 prohíbe, pero no está en su lista de permitidos. Hoy LK-49 valida el
  primero y cuenta el resto con `GetSharedMaterials`. Se decide cuando un objeto del pack lleve más de un material.
- **Referencias a "LK-22" a secas que el corte deja obsoletas** y no se han tocado, por estar en specs cerradas o fuera del alcance de hoy:
  `LK-11a` (líneas 18, 59, 78, 79), `LK-11b` (9, 39, 44, 74-76) y los widgets Color/Enum/Toggle. La de `ParameterPanelBuilder` la quitó LK-51.
  Casi todas apuntan a LK-50 o a LK-22b; se corrigen si alguna vez toca abrir ese archivo.
- **`ENUM_OPTION_HEIGHT` (28) y `BUTTON_HEIGHT_COMPACT` (28) son el mismo número del GDD con dos
  nombres** en `LumiTheme`. No lo unifico sin que me lo pidas. LK-50 no lo hizo: las opciones de enum no son `LumiButton`.
- Rama única `main`. `develop` y `feature/LK-XX-*` del GDD §4.9 aún no creadas.

## Handoff
**Fase 3 cerrada (Sesión 10).** LK-52 tiene spec escrita y sin aprobar: toca `Utils/` (archivo nuevo), `DemoCameraController` y el
  widget de Float, con permiso de su spec. Regenerar una fuente: atlas → `Import Font Features` → `FontFeatureCleaner` (spec de LK-22b).
No tocar: `Core/` y `Utils/` (LK-09, LK-49), `Demo/` (LK-10, LK-12), los cuatro widgets,
`EffectDebugTester.cs` hasta la Fase 5, `Assets/LumiKit/Scenes/`, `ProjectSettings/`,
`Packages/manifest.json`, nada de 3D ni VFX.
