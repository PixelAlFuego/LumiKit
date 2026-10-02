# Decisiones estructurales
Una entrada por decisión que condiciona código futuro. No se borran: si una decisión
se revierte, se añade una entrada nueva que la anula y se marca la anterior `Anulada por D-XXX`.

Formato: ID · fecha · decisión · motivo · irreversible · alcance.

---

## D-001 — Los parámetros se aplican con `MaterialPropertyBlock`
Fecha: 2026-09-10 · Sesión 00 · Irreversible: **sí** · Condensada en la Sesión 07
**Decisión.** `EffectController` escribe los valores en un `MaterialPropertyBlock` y lo aplica al
`Renderer`; nunca se asigna ni modifica un `Material` en runtime. **Motivo:** modificar un `.mat` en
runtime dentro del editor lo sobrescribe en disco de forma permanente, y el daño no se revierte con
Undo ni al salir de Play Mode. **Alcance:** todo `Core`, todo widget de UI que escriba un parámetro,
todo shader del pack. **Prohibido:** `renderer.material`, `renderer.materials`, `material.SetFloat` y
equivalentes. **Permitido:** `renderer.sharedMaterial` sólo en lectura.
**Referencia:** GDD §4.3 (líneas 1009-1016).

---

## D-002 — Escenas y prefabs se generan con scripts de editor
Fecha: 2026-09-10 · Sesión 00 · Irreversible: **no** · Condensada en la Sesión 07
**Decisión.** Escenas y prefabs se construyen desde scripts en `Assets/Editor/`, nunca escribiendo
YAML a mano ni editando el `.unity` / `.prefab` con un editor de texto. **Motivo:** el YAML de Unity
depende de GUIDs y un error de referencia no falla al guardar, corrompe la escena en silencio y se
descubre horas después. **Alcance:** LK-13, LK-14, LK-17, LK-22a y cualquier tarea que produzca
`.unity`, `.prefab`, `.asset` o `.meta`. Reversible porque basta volver a ejecutar el generador: la
fuente de verdad es el generador versionado, no el artefacto generado.

---

## D-003 — `EffectDefinition` es un ScriptableObject y la UI se genera desde él
Fecha: 2026-09-10 · Sesión 00 · Irreversible: **sí** · Condensada en la Sesión 07
**Decisión.** Cada efecto se describe en un `EffectDefinition` (ScriptableObject) con su lista de
`EffectParameter`; `ParameterPanelUI` lee esa lista e instancia los widgets. **Motivo:** añadir un
efecto no debe obligar a tocar código de interfaz, o el panel acaba siendo un `switch` que crece con
cada shader. **Alcance:** LK-09 define el contrato y LK-11 lo consume; ningún widget puede conocer
un efecto concreto, se configura sólo desde el `EffectParameter` que recibe. **Consecuencia:**
cambiar la forma de `EffectParameter` tras crear los `.asset` rompe la serialización de todos, así
que el contrato se cierra en LK-09. **Referencia:** GDD §4.3 (líneas 1001-1008) y §4.4 (1065-1081).

---

## D-005 — El efecto se apaga con la propiedad `_EffectEnabled`
Fecha: 2026-09-10 · Sesión 01 · Irreversible: **sí** · Condensada en la Sesión 11
**Decisión.** Todo Shader Graph del pack expone una propiedad `Float` llamada `_EffectEnabled` (0 = sin
efecto, 1 = con efecto) y termina en `Lerp(colorBase, colorConEfecto, _EffectEnabled)`;
`EffectController.SetEffectEnabled` la escribe por `MaterialPropertyBlock`. **Motivo:** D-001 prohíbe
modificar el material y un `MaterialPropertyBlock` no puede activar keywords; apagar el efecto con
"valores neutros" no sirve (en un Dissolve al 0% el borde emisivo puede seguir visible). **Alcance:**
grafos de LK-01, LK-02 y LK-03 (en HLSL por D-010); comparación con TAB (LK-24).
`MaterialPropertyHelper.EFFECT_ENABLED_PROPERTY` es el único sitio donde vive el nombre.
**Consecuencia:** un shader sin `_EffectEnabled` compila y no falla: la escritura se ignora en silencio
y el TAB no hace nada. Es criterio de aceptación de cada shader. Keywords: `.claude/rules/shaders.md`.

---

## D-004 — El GDD se renombró a `docs/reference/GDD_v2.md`
Fecha: 2026-09-10 · Sesión 00 · Irreversible: **no** · Condensada en la Sesión 05
**Decisión.** `LumiKit_Avance2_GDDv2_Arquitectura.md` → `GDD_v2.md`: toda ruta escrita en un doc
debe existir en disco, y el nombre corto es el que referencian CLAUDE.md y el resto de la
documentación. Sólo cambió el nombre; el contenido (1274 líneas) no se tocó.

---

## D-006 — El input se lee sólo con Input System
Fecha: 2026-09-14 · Sesión 02 · Irreversible: **no** · Condensada en la Sesión 11
**Decisión.** Active Input Handling = `Input System Package (New)`: `activeInputHandler: 1` en
`ProjectSettings/ProjectSettings.asset`, confirmado por el usuario (Sesión 02). Todo el pack lee input con
`UnityEngine.InputSystem` y con ninguna otra API. Forma única: `Keyboard.current` y `Mouse.current` leídos
en `Update`, con comprobación de null; sin `InputAction` creadas en código ni assets `.inputactions`.
**Motivo:** con ese valor, la API antigua `UnityEngine.Input` lanza excepción en runtime; una sola API evita
dos formas de leer el mismo ratón (LK-10, LK-12, LK-24). **Alcance:** todo script bajo `Assets/LumiKit/`.
**Prohibido:** `UnityEngine.Input` (`Input.GetKey`, `Input.mousePosition`…), `StandaloneInputModule`,
`InputSystem.actions` y `Assets/InputSystem_Actions.inputactions` (son del proyecto, no se exportan).
**Obligatorio:** el `EventSystem` de toda escena del pack usa `InputSystemUIInputModule`. **Consecuencia:**
el comprador necesita `com.unity.inputsystem` y Active Input Handling en `New` o `Both`; se documenta en LK-26.

---

## D-008 — Los widgets de parámetros se llaman `<Tipo>ParameterWidget`
Fecha: 2026-09-17 · Sesión 04 · Irreversible: **no** · Condensada en la Sesión 11
**Decisión.** El widget de `Float` es `SliderParameterWidget`, no `SliderWidget` como escribe el árbol de archivos del GDD §4.2 (línea 770). Misma
regla para los que faltan (LK-11b): `ColorParameterWidget`, `ToggleParameterWidget` y `EnumParameterWidget`. **Motivo:** `SliderWidget` nombra el
control de interfaz; `SliderParameterWidget` nombra lo que la clase es: el widget de un `EffectParameter`, que hereda de `ParameterWidgetBase` y se
configura sólo desde la `EffectDefinition` (D-003). El pack va a tener sliders que no editan parámetros de efecto —volumen y calidad en LK-18— y el
nombre corto los mezclaría en `Widgets/`. **Alcance:** LK-11a y LK-11b. Los prefabs mantienen el nombre del GDD: `PRF_Widget_Slider.prefab`. El árbol
del GDD (líneas 768-773) no se reescribe: la fuente de verdad de los nombres de clase es CODEMAP.md.

---

## D-007 — La UI del pack es uGUI + TextMeshPro, con la paleta y las medidas en LumiTheme
Fecha: 2026-09-17 · Sesión 04, verificada en la 05 · Irreversible: **no** · Condensada en la Sesión 12
**Decisión.** Toda la interfaz se construye con uGUI (`Canvas`, `Image`, `Slider`, `TextMeshProUGUI`), no con UI Toolkit. Colores, tamaños y medidas
salen de `LumiKit.UI.LumiTheme`; ni un literal suelto. **Motivo:** D-006 obliga a `InputSystemUIInputModule`, módulo de `EventSystem` y por tanto de
uGUI: es lo que hace que `IsPointerOverGameObject` bloquee cámara y selección (LK-10, LK-12); UI Toolkit en runtime no pasa por `EventSystem`. Y un
único sitio para la paleta evita que LK-22a sea una caza de hexadecimales por todo el pack. **Alcance:** LK-11a/b, LK-13, LK-18, LK-22a/b, LK-25,
LK-30 a LK-34, LK-50 y todo prefab de `Assets/LumiKit/Prefabs/UI/` (D-002). La resolución de diseño es **1920×1080** (usuario, Sesión 05):
`CanvasScaler` en `ScaleWithScreenSize` con match = height y esa referencia, y el Game view igual; sin fijarla, la escala del panel depende del tamaño
de la ventana y un tamaño de texto no es comparable entre sesiones. **Consecuencia:** depende de `com.unity.ugui` y de los TMP Essential Resources
(`Assets/TextMesh Pro/`, en el repo desde la Sesión 05): los prefabs referencian esas fuentes por GUID. Se documenta en LK-26.

---

## D-009 — Los `.ttf` originales entran en el pack, en `Fonts/Source/`
Fecha: 2026-09-20 · Sesión 07 · Irreversible: **no** · Condensada en la Sesión 12
**Decisión.** `Assets/LumiKit/Fonts/` lleva los cuatro `TMP_FontAsset` y, además, los `.ttf` originales en `Fonts/Source/` (planeado). El árbol del
GDD (líneas 931-938) sólo dibuja los `.asset` y un `OFL.txt`: desviación deliberada, decidida por el usuario en la Sesión 07. **Motivo:** dos. El
comprador puede regenerar los atlas —a otro tamaño de muestreo, con otro juego de caracteres o para otra resolución de diseño— sin volver a buscar la
fuente. Y refuerza el cumplimiento de la OFL 1.1: se redistribuye el Font Software completo junto a su licencia, no sólo un atlas derivado de él.
**Alcance:** LK-22b y LK-27 (exportación). `Fonts/Licenses/` lleva un `.txt` por familia y no uno solo, porque cada una trae su propia línea de
copyright. Coste ≈1 MB en el `.unitypackage`; en el build no pesa: un `TMP_FontAsset` estático no referencia el `.ttf` en runtime.

---

## D-010 — Los shaders 2D del MVP se escriben en HLSL
Fecha: 2026-09-30 · Sesión 11 · Irreversible: **no**
**Decisión.** Para el MVP, Claude escribe los shaders 2D (LK-01, LK-03, LK-02) en HLSL para URP: archivos
`.shader` en `Assets/LumiKit/Shaders/2D/`. Cumplen D-005 igual que un grafo: `Float _EffectEnabled` y
`lerp(base, conEfecto, _EffectEnabled)`. Desviación del GDD §1.12 (línea 313, "Shader Graph 17.x") y del
árbol §4.2 (líneas 817-820): las versiones Shader Graph quedan aparcadas para la versión de Asset Store
(`docs/MVP_SCOPE.md` > Aparcado). **Motivo:** decisión del usuario (Sesión 11); desbloquea LK-01.
**Alcance:** LK-01, LK-02, LK-03 y `.claude/rules/shaders.md`. Keywords, D-001 y nombres de propiedad, sin cambios.

---

## D-011 — Excepción a D-002: el Audio Mixer lo crea el usuario
Fecha: 2026-09-30 · Sesión 11 · Irreversible: **no**
**Decisión.** `AMX_LumiKit.mixer` no sale de un generador: lo crea el usuario en el editor, con los grupos `UI` y `Music`
hijos de `Master`, sin efectos ni snapshots extra. El código sólo lo lee. **Motivo:** Unity no tiene API pública para
crear un `AudioMixer`; `AudioMixerController` es interno y usarlo por reflexión queda descartado. **Alcance:** LK-23.
Un cambio de grupos lo hace el usuario a mano y se anota en `docs/specs/LK-23_UIAudio.md`.

---

## D-012 — Los sprites de demostración no llevan sufijo de resolución
Fecha: 2026-09-30 · Sesión 12 · Irreversible: **no**
**Decisión.** `SPR_Crystal.png` y `SPR_RuneCoin.png` en `Assets/LumiKit/Sprites/`, a 1024×1024 con PPU 512 y sin sufijo
de resolución; `SPR_Lumi` (planeado) sigue la misma regla. El GDD decía `SPR_*_512.png`, 512×512 y PPU 100: corregido en
§2.6 (líneas 522 y 524), §4.2 (866-868), §4.5 (1097) y §4.6 (1130). **Motivo:** decisión del usuario; la resolución ya
cambió una vez (de 512 a 1024) y con sufijo cada cambio obliga a renombrar. **Alcance:** LK-20 y LK-14. Las `TEX_` conservan el sufijo.

---

## D-013 — Excepción: `DemoSceneBuilder` y `MainMenuSceneBuilder` guardan su escena
Fecha: 2026-10-01 · Sesión 12 · Irreversible: **no** · Ampliada el 2026-10-02 a `01_MainMenu` (usuario, LK-13)
**Decisión.** Los generadores de LK-14 y LK-13 guardan ellos mismos la escena que reconstruyen, con `EditorSceneManager.SaveScene` en su sitio (el GUID no
cambia). Excepción acotada a `DemoSceneBuilder` (`Assets/LumiKit/Scenes/02_Demo_2D.unity`) y `MainMenuSceneBuilder` (`01_MainMenu.unity`): los demás generadores siguen dejando la escena
sin guardar, y por MCP las escenas las guarda, crea y borra el usuario (hook). **Motivo:** decisión del usuario (Sesión 12): un solo paso para
regenerar las escenas que se enseñan. **Alcance:** LK-14, LK-13. Regenerar borra lo que se haya hecho a mano en esa escena (D-002).
