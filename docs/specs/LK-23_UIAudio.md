# LK-23 — Sonidos de interfaz
Estado: ✅ verificado por el usuario (Sesión 12): suenan hover, clic, selección y música; bucle sin huecos y equilibrio correcto · Depende de: LK-10 (`ObjectSelector`), LK-11a/LK-11b (generador), LK-50 (`LumiButton`) · Diseño: GDD §4.7 (líneas 1140-1164), §4.4 (líneas 1022-1031, 1061 y 1080)

## Objetivo
Hover, Click y Select suenan por el grupo UI de `AMX_LumiKit`. `MUS_Ambient_Loop` suena en bucle por el grupo Music y no se corta
ni se reinicia al cambiar de escena: el menú de LK-13 cargará la demo. `SFX_UI_Error` y `SFX_UI_Transition` no entran (STATE > Dudas abiertas > Audio).
Desviaciones del GDD decididas por el usuario: música (línea 1142, "sin música de fondo"), grupos (línea 1161, `Master → SFX → UI`) y duraciones (punto 3).

## Antes del código (usuario, en el editor; commits `chore(assets)`)
1. `Assets/LumiKit/Audio/Mixers/AMX_LumiKit.mixer`, creado a mano (D-011, excepción a D-002): grupos `UI` y `Music` hijos de `Master`,
   sin efectos ni snapshots extra. En disco: un snapshot, sólo Attenuation, UI a −6 dB (GDD línea 1162), Master y Music a 0 dB.
2. Import de `SFX_UI_Click`, `SFX_UI_Hover` y `SFX_UI_Select`: Decompress On Load, PCM, Preload Audio Data y Force To Mono (GDD líneas 1156-1160).
   `MUS_Ambient_Loop.mp3` ya está en Streaming, Vorbis al 70 % y sin preload (STATE > Dudas abiertas > Música en MP3).
3. Duraciones, límite del usuario (el GDD, líneas 1147-1149, queda como objetivo ideal): Hover ≤150 ms (ideal <100), Click ≤200 ms (<150),
   Select ≤400 ms (<200). Recortes del usuario, según la cabecera WAV: Hover 38, Click 106 y Select 215 ms; Select, 15 ms sobre el ideal.

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
- En `Awake`: `base.Awake()`. Si `Instance != this`, sale: la base destruyó el duplicado. Si no, llama a `DontDestroyOnLoad` y arranca la música.
- Dos `AudioSource`, ambos con `playOnAwake` false y `spatialBlend` 0: SFX con `outputAudioMixerGroup` = UI, y música con `loop` true,
  `MUS_Ambient_Loop` y el grupo Music. Sólo la instancia que sobrevive llama a `Play`: el duplicado de la escena nueva nunca suena,
  y la música no se corta ni se reinicia. `DontDestroyOnLoad` exige que el manager esté en la raíz de la escena.
- `Play(UISound)` usa `PlayOneShot`. Con el clip nulo no suena y avisa una sola vez por sonido. Música nula: silencio y un aviso.
- Un Hover no se repite antes de 150 ms (`HOVER_MIN_INTERVAL`, con `Time.unscaledTime`): el tope de duración del Hover. Así dos no se
  solapan y recorrer las opciones del enum no suena a ráfaga.

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
- *Generar prefab:* aborta si falta el mixer, el grupo `Master/UI` o `Master/Music`, alguno de los tres clips o la música, y si el prefab ya existe.
- *Montar en la escena abierta:* instancia el prefab en la raíz y añade `UISelectionSound` al GameObject del `ObjectSelector`.
  Aborta si ya hay un `UIAudioManager`, si no hay `ObjectSelector` o si ya hay un `UISelectionSound`. No guarda la escena.
- API por comprobar con unity_reflect antes de programar: cómo buscar la ruta de grupos con `AudioMixer.FindMatchingGroups`.

## Banco de pruebas
`Assets/_Development/TestBench.unity`, que ya tiene un `AudioListener` en Main Camera. Pasos: regenerar los seis `PRF_*`,
generar `PRF_UIAudioManager` y montar el audio. Queda diferido a LK-17: sobrevivir al cambio de escena sin duplicarse,
y la música sin cortarse ni reiniciarse.

## Criterios de aceptación
**Verificador (MCP).** Compila y la consola queda limpia. En TestBench hay un `UIAudioManager` con los clips y el grupo UI,
su fuente de música en bucle con `MUS_Ambient_Loop` y el grupo Music, y un solo `UISelectionSound` con `_selector`. Los prefabs
llevan `UISoundTrigger`. Duraciones dentro del límite (punto 3). Play y Stop sin excepciones. `git status` sin `.mat` ni fallback.

**Usuario (oído).**
- [ ] Hover: suena una vez por elemento y no suena durante un arrastre. Si el panel aparece bajo el cursor puede sonar una vez: se acepta.
  Recorrer rápido las opciones del enum no suena a ráfaga.
- [ ] Click con el izquierdo en Reset, opción, toggle, muestra y preset. El derecho y el central callan.
- [ ] Enter o Espacio sobre un botón con foco suenan a Click. Arrastrar un slider no suena a Click. Un botón deshabilitado calla.
- [ ] Seleccionar el cristal suena Select; pasar a la moneda también; deseleccionar calla.
- [ ] La música suena en bucle desde Play, sin silencio audible al volver a empezar. Si lo hay (relleno del MP3), el usuario la exporta a OGG.
- [ ] Silenciar UI en el mixer calla los tres sonidos y no la música; silenciar Music calla sólo la música. Sin `UIAudioManager`: silencio y sin errores.

## Fuera de alcance
Error, Transition, Back, Confirm y un sonido propio para Reset (el Reset suena a Click). Tampoco volumen en ajustes ni localización.
