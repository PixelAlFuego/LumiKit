# LK-52 — Valor editable en el slider
Estado: ✅ verificado en Unity 6000.0.83f1 (usuario, Sesión 11; plan aprobado con ocho ajustes del usuario) · Hallazgo de la verificación de LK-22b (usuario) · Depende de: LK-11a (widget `Float`), LK-12 (`DemoCameraController`), LK-22b (JetBrains Mono); va después de LK-50, que cierra la Fase 3 · Diseño: GDD §1.4 regla 5 (línea 83), línea 403 (Border Strong: contorno de campos activos), línea 629 (Unity Editor como referencia de campos numéricos) · uGUI: D-007 · Input: D-006

## Objetivo
Con el panel a 280 px, el riel del slider se queda en menos de 200 px para rangos de cientos o miles de valores: arrastrando no se acierta un número concreto. Clic en el número para escribirlo.
Y una regla para toda la demo: **mientras un campo de texto tenga el foco, ningún atajo de teclado escucha**, con una sola comprobación que consultan todos los scripts, no una por script.

## Archivos
- `Assets/LumiKit/Runtime/Scripts/Utils/TextInputFocus.cs` (nuevo) · `LumiKit.Utils`, estática, ~25 líneas. En `Utils` porque la consultan `Demo` hoy y `Systems` mañana, y las dos capas pueden leer `Utils`.
- `Demo/DemoCameraController.cs` (existe, ✅ LK-12) · **no aditiva**, 1 línea: `UpdateKeyboardPan` sale también con `TextInputFocus.IsTyping`. Zoom y paneo con ratón no cambian.
- `UI/Widgets/SliderParameterWidget.cs` (existe, ✅ LK-11a) · aditiva salvo `WriteValueLabel` (~10 líneas): `_valueInput`, `_editOutline` y los manejadores de edición.
- `UI/LumiTheme.cs` (existe) · no aditiva, 2 líneas: `SLIDER_VALUE_WIDTH` pasa de 56 a 68, con su comentario. Constante nueva `SLIDER_VALUE_PADDING` (4).
- `Assets/Editor/ParameterPanelBuilder.cs` (existe) · no aditiva, ~10 líneas en `BuildSliderWidget`: el `Value` pasa a ser un campo. Método nuevo `CreateValueInput` (~50 líneas) y constante `VALUE_INPUT_CHARACTER_LIMIT` (8). Regenera (D-002) los seis `PRF_*`.
- `.claude/rules/ui-style.md` (existe) · la línea del slider añade "editable con clic (LK-52)" y el ancho nuevo, sin subir de 60.
- No se tocan: `Core/`, `ObjectSelector`, `ParameterPanelUI`, los otros tres widgets, ni `EffectDebugTester.cs` (no lee teclado).

## Contrato
**`TextInputFocus.IsTyping`**: `true` si el objeto seleccionado en `EventSystem.current` tiene un `TMP_InputField`, o un `InputField` de uGUI por si el comprador lo usa, con `isFocused`. Sin `EventSystem`, `false`. Consumidores: WASD hoy; TAB (LK-24), Escape (LK-32) y `H` (GDD línea 137) cuando lleguen; ninguno repite la comprobación.

**Jerarquía del `Value`** (la construye el generador, D-002):

| Objeto | Componentes | Notas |
|---|---|---|
| `Value` | `Image` (`SPR_UI_Rect_R6`, sliced, en blanco: la tiñe el `ColorBlock`) + `TMP_InputField` | arriba a la derecha, `SLIDER_VALUE_WIDTH` × 24 |
| `Value/Outline` | `Image` (`SPR_UI_Rect_R6_Outline`, `BorderStrong`) | apagada; sólo se ve en edición |
| `Value/Text Area` | `RectMask2D` | `textViewport` del campo; `SLIDER_VALUE_PADDING` a cada lado |
| `Value/Text Area/Text` | `TextMeshProUGUI`: Mono, `TEXT_MONO`, `TextPrimary`, a la derecha | el `_valueLabel` de hoy |

Radio 6 en los dos sprites: el contorno sólo existe a R6. Ancho 68 = 6 caracteres de Mono a 16 px (9.6 px cada uno: 57.6) + 1 del caret + 2 × 4 de relleno, redondeado a múltiplo de 4: cabe `-10.00`. Con 7 o más la máscara corta (aviso pendiente: STATE).

**Estados.** `ColorBlock` del campo sobre el fondo, con `TRANSITION_SECONDS`:

| Estado | Aspecto |
|---|---|
| Reposo | `Transparent`: igual que hoy |
| Hover | `SurfaceElevated` |
| Edición | `SurfaceElevated`, `Outline` visible, caret `LumiCyan`, selección `BorderStrong` (el GDD no la define: así no se inventa un token) y todo seleccionado al entrar (`onFocusSelectAll`) |

**Edición** (`SliderParameterWidget`):
1. **Validación por `onValidateInput`**, no por `CharacterValidation.Decimal`: ésa sólo acepta el separador decimal del idioma del sistema (`TMP_InputField.cs` línea 4201). Se admiten dígitos y un solo separador (`,` o `.`). El `-`, sólo como primer carácter y sólo si `MinValue < 0`; delante de un `-` no entra nada. TMP pasa el texto sin la parte seleccionada (`Append`).
2. **Confirmar: sólo `onEndEdit`**, sin `onDeselect`. Enter, perder el foco y `OnDisable` pasan por `DeactivateInputField` → `ReleaseSelection` → `SendOnEndEdit` (líneas 2382, 4478 y 1237 → 4416), con `resetOnDeActivation`, que el generador fija a `true`.
   `onSelect` enciende el `Outline`; `onDeselect` y `onEndEdit` lo apagan. Al terminar, el widget suelta la selección (`SetSelectedGameObject(null)`, salvo con `alreadySelecting` o el campo inactivo): si no, `selectedColor` deja el fondo como en hover. Mismo coste de foco con teclado que `LumiButton` (STATE).
3. **Aplicar:**
   - `,` pasa a `.`, `float.TryParse` con `InvariantCulture` y `Parameter.ClampFloat`;
   - el valor se asigna a `_slider.value`, que escribe el controller y lanza `OnValueChanged` por el camino de siempre;
   - texto vacío, inválido o sin tocar (entrar y salir no redondea el valor), o sin `Controller` (objeto destruido): no se aplica nada;
   - en todos los casos se reescribe el valor actual del controller con el formato de hoy (`0.00`, con punto).
4. **Escape nunca aplica.** TMP marca `wasCanceled` (línea 2280), salta `onSubmit` (2381) y devuelve el texto original antes de `onEndEdit` (4437). El generador fija `restoreOriginalTextOnEscape = true`, sin fiarse del defecto, y el manejador mira `wasCanceled` antes de aplicar.
5. `WriteValueLabel` escribe con `_valueInput.SetTextWithoutNotify` si hay campo, pero no con el campo en edición (`isFocused`): un cambio desde fuera no pisa lo escrito. Sin campo, en `_valueLabel`, como hoy.
6. Con rango vacío, el campo se bloquea igual que el slider.
7. **Cambio de objeto a media edición.** `Clear` destruye los widgets con `Destroy`, al final del frame, y el panel no escucha su `OnValueChanged`. Sea cual sea el orden entre `ObjectSelector` y el `EventSystem`, `onEndEdit` llega al widget viejo, cuyo `Controller` es el objeto que se editaba, nunca el nuevo. Clic en la escena vacía: igual, por `OnDisable` al ocultarse el panel.

El teclado lo lee `TMP_InputField` por dentro; el pack no lee ninguna tecla nueva (D-006). **Sin confirmar:** que reciba el tecleo con Active Input Handling = Input System; lo cubre el criterio de `2,5`.

## Banco de pruebas
`Assets/_Development/TestBench.unity`, con `SPR_Crystal` y los parámetros `Float` de `EFF_Debug`. Regenerar: borrar los seis `PRF_*` y el objeto `ParameterPanel`, ejecutar los dos menús `LumiKit/UI/…` y guardar la escena. Para mirar el campo con Scale alto, pausar antes con ⏸ (VERIFICATION, paso 6). Signo y `-10.00`: mínimo de `_OutlineWidth` a -10 en el Inspector sólo para la prueba, y deshacerlo (`git status` sin `EFF_Debug.asset`).

## Criterios de aceptación (verificables en el editor)
- [ ] Compila sin errores ni warnings nuevos.
- [ ] En reposo, el número se ve como en LK-22b, 4 px más a la izquierda por el relleno. Con el ratón encima, fondo `SurfaceElevated`.
- [ ] Clic en el número: se ve claramente en edición (fondo, contorno `BorderStrong`, caret cian, todo seleccionado) y sigue en JetBrains Mono. Si no se distingue lo bastante, se corrige con un token en `LumiTheme`, nunca a mano.
- [ ] `2,5` + Enter: el número queda en `2.50` y el slider y el efecto se mueven. Con `2.5`, igual.
- [ ] Escribir un valor y hacer clic fuera: se confirma.
- [ ] Escape: vuelve el valor anterior y el efecto no cambia. Tras Enter o Escape, el fondo vuelve a reposo.
- [ ] Por encima del máximo o por debajo del mínimo: se ajusta al límite.
- [ ] Campo vacío, o `-` o `,` sueltos: vuelve el valor anterior. Una letra no entra en el campo.
- [ ] Con mínimo ≥ 0 el `-` no entra; con mínimo -10, sólo al principio, y `-10.00` se ve entero.
- [ ] Editar el cristal y, sin confirmar, clic en la moneda: el valor escrito queda en el cristal, la moneda no cambia y la consola no da errores.
- [ ] Con el campo en edición, `Refresh from controller` (menú contextual de `ParameterPanelUI`): lo escrito no cambia.
- [ ] Con el campo en edición y el cursor fuera de la UI, WASD no mueve la cámara. Al confirmar, vuelve a moverla.
- [ ] Arrastrar el slider funciona como antes y el número no baila de ancho.
- [ ] Reset del pie con el campo en edición: gana el Reset.
- [ ] Tras salir de Play, `git status` sin cambios en `MAT_Debug.mat` (D-001) ni en el fallback de LiberationSans.

## Fuera de alcance
- Canales R/G/B (sin número) y campo hexadecimal del color (GDD línea 601); arrastrar sobre el número, flechas arriba/abajo y Tab entre campos.
- Formato del número según el idioma: se sigue mostrando con punto (`InvariantCulture`) → LK-19.
- La tecla que cierra la edición (Escape, Tab) puede llegar a un atajo en el mismo frame, según el orden de ejecución. A WASD no le afecta; se resuelve en LK-24 y LK-32 al consumir `IsTyping`.
- `D-010` con la regla de los atajos: se propone al cerrar. DECISIONS está en 147 de 150 líneas.
- Tocar `Core`, `ObjectSelector`, los otros widgets o el `EffectDebugTester`.
