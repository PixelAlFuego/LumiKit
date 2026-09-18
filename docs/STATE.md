# Estado del proyecto
Actualizado: 2026-09-18 · Sesión 06 (cerrada)

## Ahora
- Fase: 3 (Interfaz). LK-11a ✅, LK-11b ✅ y LK-49 ✅ verificados en TestBench.
- Tarea activa: ninguna.
- Siguiente: LK-22 — identidad visual, fuentes y pie del panel. Cierra la Fase 3.

## Últimas 3 sesiones
| Sesión | Fecha | Tarea | Resultado | Commit |
|---|---|---|---|---|
| 04 | 2026-09-17 | LK-11a ParameterPanelUI + widget Float | ✅ | 2284703 |
| 05 | 2026-09-18 | LK-11b widgets Color, Toggle y Enum | ✅ | 871edb6 |
| 06 | 2026-09-18 | LK-49 validación de `propertyName` | ✅ | dd7140a |

Sesiones 00 a 03 archivadas en `docs/archive/sesiones_2026-Q3.md`.

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
- **Issue de la Sesión 02, ejecutable desde LK-11a:** ya hay `Canvas` y `EventSystem`, así que
  `IsPointerOverUI` de LK-10 y LK-12 por fin se puede probar; es criterio de LK-11a. Lo que sigue
  abierto es el `OnGUI` del `EffectDebugTester`, que no pasa por `EventSystem` y no bloquea el
  ratón: se cierra al retirar el tester, al cerrar la Fase 3.
- **`Assets/TextMesh Pro/` (4 MB) entra al repositorio** (usuario, Sesión 05): los prefabs de UI
  referencian esas fuentes por GUID. No se ignora ni se exporta, pero es dependencia del proyecto
  del comprador: documentar en LK-26 junto a la de Input System.
- **Tamaño de texto por encima del GDD:** Label y Mono a 16 px en vez de los 13 del GDD (líneas
  451-457), con la resolución de diseño fijada en 1920×1080. Cerrado por el usuario en la Sesión 05
  y anotado en `LumiTheme` y en D-007. `ui-style.md` sigue con los 13 px y está en 60/60 líneas: la
  corrección espera a que toque condensarlo, en LK-22.
- **`EFF_Debug.asset` tiene seis parámetros** y dos son deliberadamente distintos: `_Color` existe
  en el shader de los sprites y `_BaseColor` **no**. `_BaseColor` se queda como control negativo
  permanente de LK-49. Los marcadores usan `Sprite-Unlit-Default` (`_MainTex` y `_Color`) y el cubo,
  un shader URP que declara `_BaseColor` **y también `_Color`**, en su bloque `ObsoleteProperties`
  (`Unlit.shader` línea 28): por eso el cubo reporta cuatro ausentes y no cinco.
- **`MAT_Debug.mat` pasa a `Universal Render Pipeline/Unlit`** (usuario, Sesión 06): sin
  iluminación, que es lo acordado en LK-09 para poder cerrar el criterio de espacio Linear.
  **Todavía no está en disco**: el `.mat` conserva el GUID de `Lit.shader` (`933532a4…`); el de
  `Unlit.shader` es `650dd952…`. Falta guardar el proyecto en Unity y commitear el asset.
- **Materiales de los sprites, para la Fase 5:** los marcadores comparten el material por defecto
  de Unity `Sprite-Unlit-Default`. Cada efecto necesitará el suyo (`MAT_` en
  `Assets/LumiKit/Materials/2D/`) o tocar un parámetro en uno los cambiará todos. Entra con LK-01.
- **Los topes no se suben** (usuario, Sesión 05): al llegar al tope se condensa. Regla 8 de
  `docs-style.md` actualizada; D-004 condensada para hacer sitio a D-007.
- `Assets/_Development/SPR_Crystal.png` y `SPR_RuneCoin.png`: borradores del usuario, no del
  pack. No van a CODEMAP ni se mueven a `Assets/LumiKit/`. Se usan como marcadores en TestBench.
- **Nombres y posiciones de TestBench:** la spec de LK-12 escribió `Marker_Crystal` en (-3,0,0) y
  `Marker_RuneCoin` en (3,0,0). El estado real, confirmado por el usuario en la Sesión 03, es
  `SPR_Crystal` en (3,0,0) y `SPR_RuneCoin` en (-3,0,0). La spec de LK-10 usa los nombres reales.
  La de LK-12 está cerrada y no se toca: decisión del usuario.
- `ProjectSettings/TagManager.asset` entró en el cierre de LK-10 con la capa `Selectable` (índice
  6) que creó el usuario. Unity aprovechó para migrarlo a `serializedVersion: 3` y borrar las
  entradas de capa vacías del final. Migración del editor, no revisada línea a línea.
- **Enmienda pendiente a D-001:** autorizar la lectura de `sharedMaterials` (en plural) para
  validar todos los materiales de un `Renderer`, no sólo el primero. No instancia copias, que es
  lo que D-001 prohíbe, pero no está en su lista de permitidos. Hoy LK-49 valida el primero y
  cuenta el resto con `GetSharedMaterials`. Se decide cuando un objeto del pack lleve más de un
  material; hasta entonces no se toca D-001.
- Rama única `main`. `develop` y `feature/LK-XX-*` del GDD §4.9 aún no creadas.

## Handoff
Empezar por: LK-22, que cierra la Fase 3. Tres frentes: las fuentes del GDD como `TMP_FontAsset` en
`Assets/LumiKit/Fonts/` (hoy todo el HUD va con LiberationSans, la de TMP), `ui-style.md` al día con
los tamaños reales —está en 60/60 líneas, toca condensar, no subir el tope— y el pie del panel con
Reset, que ya tiene `RefreshFromController()` esperando desde LK-11a.
Antes de empezar: guardar en Unity el cambio de shader de `MAT_Debug` y commitear el `.mat`.
No tocar: `Core/` y `Utils/` (verificados en LK-09 y LK-49), `Demo/` (LK-10 y LK-12),
`EffectDebugTester.cs` hasta cerrar la Fase 3, `Assets/LumiKit/Scenes/`, `ProjectSettings/`,
`Packages/manifest.json`, nada de 3D ni VFX.
