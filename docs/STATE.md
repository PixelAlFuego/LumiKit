# Estado del proyecto
Actualizado: 2026-09-18 · Sesión 06

## Ahora
- Fase: 3 (Interfaz) en curso. LK-11a ✅ y LK-11b ✅ verificados en TestBench.
- Tarea activa: LK-49 🟡 implementado, sin abrir Unity. Spec: `docs/specs/LK-49_ShaderPropertyValidation.md`.
- Siguiente: tras LK-49, LK-22 (identidad visual, fuentes y pie del panel) cierra la Fase 3.

## Últimas 3 sesiones
| Sesión | Fecha | Tarea | Resultado | Commit |
|---|---|---|---|---|
| 03 | 2026-09-16 | LK-10 ObjectSelector | ✅ | 6d4da8d |
| 04 | 2026-09-17 | LK-11a ParameterPanelUI + widget Float | ✅ | 2284703 |
| 05 | 2026-09-18 | LK-11b widgets Color, Toggle y Enum | ✅ | 871edb6 |

Sesiones 00 a 02 archivadas en `docs/archive/sesiones_2026-Q3.md`.

## Pendiente de verificación en Unity
LK-49. Nada que montar: el banco quedó listo en la Sesión 05. Criterios completos en la spec.
- [ ] Compila sin errores ni warnings nuevos.
- [ ] Play: un aviso por objeto con `EffectController`, no uno por parámetro ni por frame.
- [ ] `Marker_Center` lista `_BaseColor`, `_Glow`, `_OutlineWidth`, `_Pulse` y `_OutlineMode`; `_Color` no sale.
- [ ] El cubo lista las mismas menos `_BaseColor`, y en su lugar `_Color`.
- [ ] El aviso nombra objeto, efecto, material y shader, y al pulsarlo selecciona el objeto.
- [ ] Un minuto moviendo sliders y cambiando de selección: la consola no crece.
- [ ] Objeto sin `Renderer`: avisa de que no hay material, sin excepción.
- [ ] Con dos materiales en el `Renderer`, el aviso termina en "Validado 1 de 2 materiales".
- [ ] Salir de Play: `git status` sin cambios en `MAT_Debug.mat` ni en los materiales de los sprites (D-001).

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
- **`EFF_Debug.asset` tiene seis parámetros** y dos de ellos son deliberadamente distintos:
  `_Color` existe en el shader de los sprites y `_BaseColor` **no**. El usuario deja `_BaseColor`
  como control negativo permanente de LK-49. Los marcadores usan
  `Universal Render Pipeline/2D/Sprite-Unlit-Default` (declara `_MainTex` y `_Color`) y el cubo de
  LK-09 usa `Universal Render Pipeline/Unlit` (declara `_BaseColor`).
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
Empezar por: verificar LK-49 con la checklist de arriba. Es la primera vez que se toca `Core/`
desde LK-09: si algo chirría, el cambio es puramente aditivo (118 líneas, ninguna existente
modificada) y se revierte sin arrastrar nada.
Después, LK-22 cierra la Fase 3: fuentes del GDD como `TMP_FontAsset` en `Assets/LumiKit/Fonts/`,
`ui-style.md` al día con los tamaños reales (está en 60/60 líneas, toca condensar, no subir el tope)
y el pie del panel con Reset, que ya tiene `RefreshFromController()` esperando.
No tocar: `Scripts/UI/` ni sus prefabs salvo lo que pida LK-22 (LK-11a y LK-11b verificados),
`Demo/` (LK-10 y LK-12), `EffectDebugTester.cs` hasta cerrar la Fase 3, `Assets/LumiKit/Scenes/`,
`ProjectSettings/`, `Packages/manifest.json`, nada de 3D ni VFX.
