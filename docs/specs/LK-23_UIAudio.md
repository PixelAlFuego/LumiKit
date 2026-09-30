# LK-23 — Sonidos de interfaz
Estado: ⬜ spec escrita, sin aprobar (Sesión 11): plan pasado por el revisor, APROBADO CON CAMBIOS, con los 8 cambios aplicados · Depende de: LK-10 (`ObjectSelector`), LK-11a/LK-11b (generador), LK-50 (`LumiButton`) · Diseño: GDD §4.7 (líneas 1140-1164), §4.4 (líneas 1022-1031, 1061 y 1080)

## Objetivo
Hover, Click y Select suenan por el grupo UI de `AMX_LumiKit`. `SFX_UI_Error` y `SFX_UI_Transition` no entran (STATE > Dudas abiertas > Audio).

## Antes del código (usuario, en el editor; commits `chore(assets)`)
1. Crear `Assets/LumiKit/Audio/Mixers/AMX_LumiKit.mixer` (planeado) a mano: grupos Master → SFX → UI, UI a −6 dB (GDD líneas 1161-1162).
   **Excepción a D-002, pendiente de aprobar:** `AudioMixerController.CreateMixerControllerAtPath` existe, pero su tipo no aparece entre
   los públicos (unity_reflect, Unity 6000.0.83f1). Crearlo por reflexión sobre API interna queda descartado. Si se aprueba, se anota en DECISIONS.
2. Import de `SFX_UI_Click`, `SFX_UI_Hover` y `SFX_UI_Select`: Decompress On Load, PCM, Preload Audio Data y Force To Mono (GDD líneas 1156-1160).
   Hoy, según sus `.meta`: Vorbis al 100 %, sin preload y en estéreo.
3. **Pendiente de decidir:** las duraciones no cumplen el GDD (líneas 1147-1149). Hover 285 ms (<100), Click 320 ms (<150),
   Select 1851 ms (<200), según las cabeceras WAV. ¿Se recortan o se aceptan?
4. **Pendiente de decidir:** prefijo `MUS_` (STATE > Dudas abiertas). La música no entra en LK-23.

## Archivos
- `Runtime/Scripts/Systems/UIAudioManager.cs` (planeado) · `LumiKit.Systems` · `Singleton<UIAudioManager>`, enum `UISound` en el mismo archivo.
- `Runtime/Scripts/Systems/UISoundTrigger.cs` (planeado) · `LumiKit.Systems` · va en cada `Selectable` de los prefabs del panel.
- `Runtime/Scripts/Systems/UISelectionSound.cs` (planeado) · `LumiKit.Systems` · escucha `ObjectSelector`.
- `Assets/Editor/UIAudioBuilder.cs` (planeado) · dos menús `LumiKit/Audio/…`. Genera `Prefabs/Systems/PRF_UIAudioManager.prefab` (planeado).
- `Assets/Editor/ParameterPanelBuilder.cs` (existe, ✅) · unas 20 líneas, bajo el tope de 30: añade `UISoundTrigger` y fija `_playClick`
  con `SerializedObject`, mediante un método auxiliar.
- Sin tocar: `LumiButton`, los cuatro widgets, `ParameterPanelUI`, `ObjectSelector`, `Core/` y `Utils/`.

## Contrato
**Capas.** Los tres componentes van en `Systems`. Systems conoce a uGUI (`Selectable`) y a Demo (`ObjectSelector`): es hacia abajo (GDD línea 1061).
Ni UI ni Demo conocen Systems. Todo va en el asmdef `LumiKit.Runtime`, sin referencias nuevas.

**`UIAudioManager`.**
- En `Awake`: `base.Awake()`. Si `Instance != this`, sale: la base destruyó el duplicado. Si no, llama a `DontDestroyOnLoad`.
- Un `AudioSource` con `playOnAwake` false, `spatialBlend` 0 y `outputAudioMixerGroup` = UI.
- `Play(UISound)` usa `PlayOneShot`. Con el clip nulo no suena y avisa una sola vez por sonido.
- Un Hover no se repite antes de 80 ms.

**`UISoundTrigger`.** Va en el mismo GameObject que el `Selectable`: `ExecuteEvents` entrega el evento a todos sus componentes.
- Hover con `IPointerEnterHandler`. Calla si `eventData.dragging`, para que arrastrar el canal R por encima de G y B no suene.
- Click con `IPointerClickHandler` (sólo `InputButton.Left`, como `Button` y `Toggle`) y con `ISubmitHandler`. Sólo si `_playClick` está activo.
- Calla también si el `Selectable` no es `IsInteractable()` o si no hay `UIAudioManager`.
- Con clic: Reset (`LumiButton`), opción de enum, muestra de color, presets y toggle.
- Sólo hover: `CreateSlider` (cubre el slider y los canales R/G/B) y el campo del valor. Arrastrar un `Slider` puede disparar `OnPointerClick`.

**`UISelectionSound`.**
- Lleva `[DisallowMultipleComponent]` y `[SerializeField] private ObjectSelector _selector`.
- Se suscribe en `OnEnable` y se da de baja en `OnDisable`. Con `_selector` nulo avisa y no hace nada.
- Suena Select sólo si la selección nueva no es nula y hay `UIAudioManager`. Deseleccionar calla.
  Pasar del cristal a la moneda sí suena. Volver a pulsar el mismo objeto no dispara el evento (`Demo/ObjectSelector.cs:106`).

**`UIAudioBuilder`.**
- *Generar prefab:* aborta si falta el mixer, la ruta `Master/SFX/UI` o alguno de los tres clips, y si el prefab ya existe.
- *Montar en la escena abierta:* instancia el prefab en la raíz y añade `UISelectionSound` al GameObject del `ObjectSelector`.
  Aborta si ya hay un `UIAudioManager`, si no hay `ObjectSelector` o si ya hay un `UISelectionSound`. No guarda la escena.
- API por comprobar con unity_reflect antes de programar: cómo buscar la ruta de grupos con `AudioMixer.FindMatchingGroups`.

## Banco de pruebas
`Assets/_Development/TestBench.unity`, que ya tiene un `AudioListener` en Main Camera. Pasos: regenerar los seis `PRF_*`,
generar `PRF_UIAudioManager` y montar el audio. Queda diferido a LK-17: sobrevivir al cambio de escena sin duplicarse.

## Criterios de aceptación
**Verificador (MCP).** Compila y la consola queda limpia. En TestBench hay un `UIAudioManager` con los clips y el grupo UI,
y un solo `UISelectionSound` con `_selector`. Los prefabs llevan `UISoundTrigger`. Play y Stop sin excepciones.
`git status` sin `.mat` ni fallback.

**Usuario (oído).**
- [ ] Hover: suena una vez por elemento y no suena durante un arrastre. Si el panel aparece bajo el cursor puede sonar una vez: se acepta.
- [ ] Click con el izquierdo en Reset, opción, toggle, muestra y preset. El derecho y el central callan.
- [ ] Enter o Espacio sobre un botón con foco suenan a Click. Arrastrar un slider no suena a Click. Un botón deshabilitado calla.
- [ ] Seleccionar el cristal suena Select; pasar a la moneda también; deseleccionar calla.
- [ ] Silenciar el grupo UI en el mixer calla todo. Sin `UIAudioManager` en la escena: silencio y sin errores.

## Fuera de alcance
Error, Transition, Back, Confirm y un sonido propio para Reset (el Reset suena a Click). Tampoco música, volumen en ajustes ni localización.
