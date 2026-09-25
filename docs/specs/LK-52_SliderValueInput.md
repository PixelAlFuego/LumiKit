# LK-52 — Valor editable en el slider
Estado: ⬜ spec escrita, pendiente de aprobación (Sesión 10) · Hallazgo de la verificación de LK-22b (usuario) · Depende de: LK-11a (widget `Float`), LK-12 (`DemoCameraController`), LK-22b (JetBrains Mono); va después de LK-50, que cierra la Fase 3 · Diseño: GDD §1.4 regla 5 (línea 83), línea 403 (Border Strong: contorno de campos activos), línea 629 (Unity Editor como referencia de campos numéricos) · uGUI: D-007 · Input: D-006

## Objetivo
Con el panel a 280 px, el riel del slider se queda en menos de 200 px para rangos de cientos o miles de valores: arrastrando no se acierta un número concreto. Clic en el número para escribirlo.
Y una regla para toda la demo: **mientras un campo de texto tenga el foco, ningún atajo de teclado escucha**, con una sola comprobación que consultan todos los scripts, no una por script.

## Archivos
- `Assets/LumiKit/Runtime/Scripts/Utils/TextInputFocus.cs` (planeado) · `LumiKit.Utils`, estática, ~25 líneas. En `Utils` porque la consultan `Demo` hoy y `Systems` mañana, y las dos capas pueden leer `Utils`.
- `Demo/DemoCameraController.cs` (existe, ✅ LK-12) · **no aditiva**, 1 línea: `UpdateKeyboardPan` sale también con `TextInputFocus.IsTyping`. Zoom y paneo con ratón no cambian.
- `UI/Widgets/SliderParameterWidget.cs` (existe, ✅ LK-11a) · aditiva salvo `WriteValueLabel` (~3 líneas): `_valueInput`, `_editOutline` y los manejadores de edición.
- `Assets/Editor/ParameterPanelBuilder.cs` (existe) · no aditiva, ~6 líneas en `BuildSliderWidget`: el `Value` pasa a ser un campo. Método nuevo `CreateValueInput` (~50 líneas) y constante `VALUE_INPUT_CHARACTER_LIMIT` (8). Regenera (D-002) los seis `PRF_*`.
- `.claude/rules/ui-style.md` (existe) · la línea del slider añade "editable con clic (LK-52)", sin subir de 60.
- No se tocan: `Core/`, `ObjectSelector`, `LumiTheme` (todos los tokens existen), los otros tres widgets, ni `EffectDebugTester.cs` (no lee teclado).

## Contrato
**`TextInputFocus.IsTyping`**: `true` si el objeto seleccionado en `EventSystem.current` tiene un `TMP_InputField`, o un `InputField` de uGUI por si el comprador lo usa, con `isFocused`. Sin `EventSystem`, `false`.
Consumidores: WASD hoy; TAB (LK-24), Escape (LK-32) y `H` (GDD línea 137) cuando lleguen. Ningún script repite la comprobación.

**Jerarquía del `Value`** (la construye el generador, D-002):

| Objeto | Componentes | Notas |
|---|---|---|
| `Value` | `Image` (`SPR_UI_Rect_R6`, sliced) + `TMP_InputField` | mismo rect que hoy: arriba a la derecha, 56 px |
| `Value/Outline` | `Image` (`SPR_UI_Rect_R6_Outline`, `BorderStrong`) | apagada; sólo se ve en edición |
| `Value/Text Area` | `RectMask2D` | `textViewport` del campo |
| `Value/Text Area/Text` | `TextMeshProUGUI`: Mono, `TEXT_MONO`, `TextPrimary`, a la derecha | el `_valueLabel` de hoy |

Radio 6 en los dos sprites: el contorno sólo existe a R6.

**Estados.** `ColorBlock` del campo sobre el fondo, con `TRANSITION_SECONDS`:

| Estado | Aspecto |
|---|---|
| Reposo | `Transparent`: igual que hoy |
| Hover | `SurfaceElevated` |
| Edición | `SurfaceElevated`, `Outline` visible, caret `LumiCyan`, selección `BorderStrong` (el GDD no la define: así no se inventa un token) y todo seleccionado al entrar (`onFocusSelectAll`) |

**Edición** (`SliderParameterWidget`):
1. **Validación por `onValidateInput`**, no por `CharacterValidation.Decimal`. Esa sólo acepta el separador decimal del idioma del sistema, coma **o** punto (`TMP_InputField.cs` línea 4201). Se admiten dígitos, un solo separador (`,` o `.`) y un `-` inicial, éste sólo si `MinValue < 0`.
2. **Confirmar:** `onEndEdit` con `wasCanceled == false`. Enter pasa por ahí: `SendOnSubmit` y después `SendOnEndEdit` (líneas 2382 y 4416).
   Perder el foco se escucha además en `onDeselect`, con una guarda para no aplicar dos veces: no está confirmado que en esta versión la deselección lance `onEndEdit`.
3. **Aplicar:**
   - `,` pasa a `.`, `float.TryParse` con `InvariantCulture` y `Parameter.ClampFloat`;
   - el valor se asigna a `_slider.value`, que escribe el controller y lanza `OnValueChanged` por el camino de siempre;
   - texto vacío o inválido: no se aplica nada;
   - en todos los casos se reescribe el número con el formato de hoy (`0.00`, con punto).
4. **Cancelar:** Escape marca `wasCanceled` y, con `restoreOriginalTextOnEscape`, TMP devuelve el texto. No se aplica nada.
5. `WriteValueLabel` escribe con `_valueInput.SetTextWithoutNotify` si hay campo. Si no, en `_valueLabel`, como hoy.
6. Con rango vacío, el campo se bloquea igual que el slider.

El teclado lo lee `TMP_InputField` por dentro; el pack no lee ninguna tecla nueva (D-006).
**Sin confirmar:** que `TMP_InputField` reciba el tecleo con Active Input Handling = Input System. Lo cubre el criterio de `2,5`.

## Banco de pruebas
`Assets/_Development/TestBench.unity`, con `SPR_Crystal` y los parámetros `Float` de `EFF_Debug`. Regenerar: borrar los seis `PRF_*` y el objeto `ParameterPanel`, ejecutar los dos menús `LumiKit/UI/…` y guardar la escena. Para mirar el campo con Scale alto, pausar antes con ⏸ (VERIFICATION, paso 6).

## Criterios de aceptación (verificables en el editor)
- [ ] Compila sin errores ni warnings nuevos.
- [ ] En reposo, el número se ve como en LK-22b. Con el ratón encima, fondo `SurfaceElevated`.
- [ ] Clic en el número: se ve claramente en edición (fondo, contorno `BorderStrong`, caret cian, todo seleccionado) y sigue en JetBrains Mono. Si no se distingue lo bastante, se corrige con un token en `LumiTheme`, nunca a mano.
- [ ] `2,5` + Enter: el número queda en `2.50` y el slider y el efecto se mueven. Con `2.5`, igual.
- [ ] Escribir un valor y hacer clic fuera: se confirma.
- [ ] Escape: vuelve el valor anterior y el efecto no cambia.
- [ ] Por encima del máximo o por debajo del mínimo: se ajusta al límite.
- [ ] Campo vacío, o `-` o `,` sueltos: vuelve el valor anterior. Una letra no entra en el campo.
- [ ] Con el campo en edición y el cursor fuera de la UI, WASD no mueve la cámara. Al confirmar, vuelve a moverla.
- [ ] Arrastrar el slider funciona como antes y el número no baila de ancho.
- [ ] Reset del pie con el campo en edición: gana el Reset.
- [ ] Tras salir de Play, `git status` sin cambios en `MAT_Debug.mat` (D-001) ni en el fallback de LiberationSans.

## Fuera de alcance
- Canales R/G/B, que no muestran número, y el campo hexadecimal del color (GDD línea 601).
- Arrastrar sobre el número para cambiarlo, flechas arriba/abajo y Tab entre campos.
- Formato del número según el idioma: se sigue mostrando con punto (`InvariantCulture`) → LK-19.
- La tecla que cierra la edición (Escape, Tab) puede llegar a un atajo en el mismo frame, según el orden de ejecución. A WASD no le afecta; se resuelve en LK-24 y LK-32 al consumir `IsTyping`.
- `D-010` con la regla de los atajos: se propone al cerrar. DECISIONS está en 147 de 150 líneas.
- Tocar `Core`, `ObjectSelector`, los otros widgets o el `EffectDebugTester`.
