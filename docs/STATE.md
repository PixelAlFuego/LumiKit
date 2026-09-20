# Estado del proyecto
Actualizado: 2026-09-20 · Sesión 08 (cerrada)

## Ahora
- Fase: 3 (Interfaz). **LK-22a ✅** verificada por el usuario en la Sesión 08: el Reset reinicia los
  seis widgets y se refleja en pantalla al instante.
- Tarea activa: ninguna. Dos specs escritas y **pendientes de aprobación**: LK-50 (componente de
  botón) y LK-51 (arte de interfaz, tarea nueva por decisión del usuario, no una ampliación de LK-50).
- Siguiente: el usuario aprueba el plan y se programa el soporte de sprites de LK-51 y luego LK-50.
  Los siete PNG los dibuja él. LK-22b (fuentes) sigue abierta y sin bloquear a nadie.

## Últimas 3 sesiones
| Sesión | Fecha | Tarea | Resultado | Commit |
|---|---|---|---|---|
| 06 | 2026-09-18 | LK-49 validación de `propertyName` | ✅ | dd7140a |
| 07 | 2026-09-20 | LK-22a pie del panel con Reset | ✅ | 6161b99 |
| 08 | 2026-09-20 | Cierre de LK-22a + specs de LK-50 y LK-51 | ✅ | este commit |

Sesiones 00 a 05 archivadas en `docs/archive/sesiones_2026-Q3.md`.

## Pendiente de verificación en Unity
Nada. LK-22a se cerró en la Sesión 08 y no hay ninguna tarea 🟡.

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
- **Audio, para LK-23.** Ya en el repositorio desde la Sesión 08: `Audio/SFX/` con `SFX_UI_Click`, `SFX_UI_Hover`,
  `SFX_UI_Select`, `SFX_UI_Error` y `SFX_UI_Transition`, y `Audio/Music/MUS_Ambient_Loop.wav`. **`SFX_UI_Error` y
  `SFX_UI_Transition` no entran en LK-23**: el primero espera a que haya avisos en pantalla y el segundo al cambio de
  escena de LK-17. LK-23 engancha sólo los otros tres. Los ajustes de importación de los `.wav` están sin revisar.
- **`MUS_` no existe como prefijo** en la tabla de CONVENTIONS.md ni en CLAUDE.md, aunque el archivo ya use ese nombre.
  Se decide en LK-23: o se añade el prefijo a la tabla, o la música se nombra de otra forma. No lo toco sin que me lo pidas.
- **Diferidos a LK-01, no cumplidos:** verificación visual de `SetEffectEnabled` y del color en
  espacio Linear. Los criterios viven en `docs/specs/LK-01_Outline2D.md`.
- **Botón del pie, deuda consciente:** el estado Normal del botón secundario del GDD es "Transparente" (línea 566), pero el relleno va en
  `Surface` porque por detrás está la Image del borde y transparente de verdad se vería entera. **La cierra LK-51**, con el sprite de
  contorno; LK-50 no puede. Se ve igual, así que no corre prisa.
- **El pack no tiene dónde poner un script de editor propio.** `Assets/Editor/` no se exporta (CONVENTIONS) y ningún script bajo
  `Assets/LumiKit/` puede hacer `using UnityEditor`. Sale a la luz con LK-50: su `LumiButtonEditor` funcionará aquí pero el comprador
  no verá los campos del componente en el Inspector. Haría falta un tercer asmdef sólo-editor dentro del pack. Se decide en LK-27.
- **Borrar al cerrar la Fase 5:** de `Assets/_Development/`, sólo `EffectDebugTester.cs`, `EFF_Debug.asset` y `MAT_Debug.mat`.
  Desechables, fuera del pack, deliberadamente ausentes de CODEMAP y BACKLOG. Los reemplaza LK-11.
- **`Assets/_Development/TestBench.unity` no se borra:** banco de pruebas permanente, crece con
  cada tarea (sección "Banco de pruebas" de cada spec). No se exporta.
- `Assets/Settings/DefaultVolumeProfile.asset` cambió solo (migración de Unity al importar) y entró en el commit `2cc98ff` sin revisión. Fuera del alcance de LK-09.
- `EffectController.cs` salió de 316 líneas, más de las ~140 estimadas en el plan.
- `Assets/TutorialInfo/` y `Assets/Readme.asset` son plantilla de Unity, fuera del pack. También
  `Assets/InputSystem_Actions.inputactions`, asset de acciones del proyecto: D-006 prohíbe usarlo desde el pack.
- **Issue de la Sesión 02:** sigue abierto el `OnGUI` del `EffectDebugTester`, que no pasa por `EventSystem` y no bloquea el ratón. Se cierra al retirar el tester, al cerrar la Fase 3.
- **`Assets/TextMesh Pro/` (4 MB) entra al repositorio** (usuario, Sesión 05): los prefabs de UI
  referencian esas fuentes por GUID. Dependencia del proyecto del comprador: documentar en LK-26.
- **Tamaño de texto por encima del GDD:** cerrado. Label y Mono a 16 px (GDD: 13) con la resolución de diseño en 1920×1080; anotado en `LumiTheme`, en D-007 y ya en `ui-style.md` (LK-22a).
- **`EFF_Debug.asset` tiene seis parámetros** y dos son deliberadamente distintos: `_Color` existe
  en el shader de los sprites y `_BaseColor` **no**. `_BaseColor` se queda como control negativo
  permanente de LK-49. Los marcadores usan `Sprite-Unlit-Default` (`_MainTex` y `_Color`) y el cubo, un shader URP
  que declara `_BaseColor` **y también `_Color`** en `ObsoleteProperties` (`Unlit.shader` línea 28): por eso reporta cuatro ausentes y no cinco.
- **`MAT_Debug.mat` usa `Universal Render Pipeline/Unlit`** (usuario, Sesión 06; en disco, GUID
  `650dd952…`): sin iluminación, lo acordado en LK-09 para poder cerrar el criterio de Linear.
- **Materiales de los sprites, para la Fase 5:** los marcadores comparten el material por defecto `Sprite-Unlit-Default`.
  Cada efecto necesitará el suyo (`MAT_` en `Assets/LumiKit/Materials/2D/`) o tocar un parámetro en uno los cambiará todos. Entra con LK-01.
- **Los topes no se suben** (usuario, Sesión 05): al llegar al tope se condensa. En la Sesión 07 se
  condensaron `ui-style.md` (60→58), D-001, D-002 y D-003 para hacer sitio a D-009.
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
  `LK-11a` (líneas 18, 59, 78, 79), `LK-11b` (9, 39, 44, 74-76), los widgets Color/Enum/Toggle y `ParameterPanelBuilder` línea 244.
  Casi todas apuntan a LK-50 o a LK-22b; se corrigen si alguna vez toca abrir ese archivo.
- **`ENUM_OPTION_HEIGHT` (28) y `BUTTON_HEIGHT_COMPACT` (28) son el mismo número del GDD con dos
  nombres** en `LumiTheme`. No lo unifico sin que me lo pidas; lo natural es hacerlo en LK-50.
- Rama única `main`. `develop` y `feature/LK-XX-*` del GDD §4.9 aún no creadas.

## Handoff
Todo lo de LK-22a está cerrado y commiteado: los seis `PRF_*`, `TestBench.unity` y los seis audios.
Sobre la mesa, **dos specs sin aprobar** y ningún código escrito:
- `docs/specs/LK-51_UISpriteArt.md` — arte de interfaz. Se hace en dos tiempos: primero el soporte en
  el generador (`RequireSprites()`, rutas, `RADIUS_SMALL`), que sin sprites aborta nombrando lo que
  falta; después los siete PNG que dibuja el usuario. El detalle de dibujo está en
  `docs/reference/UI_ART_BRIEF.md` y la spec no lo duplica.
- `docs/specs/LK-50_LumiButton.md` — comportamiento del botón. No necesita ni un sprite y se verifica
  con el pie que ya existe. Independiente de LK-51: ninguna espera a la otra.
LK-22b (fuentes) sigue abierta, bloqueada sólo por los cuatro `TMP_FontAsset`.
No tocar: `Core/` y `Utils/` (LK-09, LK-49), `Demo/` (LK-10, LK-12), los cuatro widgets,
`EffectDebugTester.cs` hasta cerrar la Fase 3, `Assets/LumiKit/Scenes/`, `ProjectSettings/`,
`Packages/manifest.json`, nada de 3D ni VFX.
