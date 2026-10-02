# LK-24 — Comparación antes / después (TAB)
Estado: 🟡 implementado, pendiente de verificar en el editor (Sesión 12). Revisor: APROBADO CON CAMBIOS, 8 aplicados; programar sin bloqueantes, autorizado por el usuario · Depende de: LK-09 (`SetEffectEnabled`, `IsEffectEnabled`), LK-10 (`ObjectSelector`), LK-52 (`TextInputFocus`), LK-01 (`_EffectEnabled` en `SH_Outline2D`) · Diseño: GDD §1.3 Mecánica 3 (líneas 57-59), §1.11 (línea 250), D-001, D-005, D-006

## Objetivo
Mientras el usuario mantiene `TAB`, el objeto seleccionado se ve sin efecto; al soltar, el efecto vuelve. Sin tocar el material (D-001):
`EffectController.SetEffectEnabled` escribe `_EffectEnabled` por `MaterialPropertyBlock` (D-005).

## Archivos
- `Assets/LumiKit/Runtime/Scripts/Demo/ComparisonToggle.cs` (planeado) · `LumiKit.Demo`. Nuevo. Único archivo de código de la tarea.
- Sin tocar: `Core/`, `Utils/`, `ObjectSelector.cs`, `DemoCameraController.cs`, `UI/`, `Systems/`, los generadores de `Assets/Editor/`, `EffectDebugTester.cs`.
- Montaje en TestBench: Claude por MCP (`manage_components`, permitido en TestBench); guarda el usuario (el hook impide guardar por MCP).
  `TestBench.unity` no entra en el commit de la tarea: el montaje vive sólo en el editor abierto y, si Unity se cierra sin guardar, se pierde.
  En `02_Demo_2D` lo monta el generador de LK-14.

## Contrato
MonoBehaviour suelto: `[AddComponentMenu("LumiKit/Comparison Toggle")]`, `[DisallowMultipleComponent]`, sin `RequireComponent`. Va en el objeto del `ObjectSelector`.
**Uno por escena:** con dos, el segundo guardaría el efecto ya apagado y no volvería nunca. Una referencia estática al activo: el segundo avisa y se desactiva.

| Campo serializado | Tipo | Defecto | Uso |
|---|---|---|---|
| `_selector` | ObjectSelector | null | Si es null, `GetComponent<ObjectSelector>()` en `Awake`. Sin ninguno: warning una vez y el componente se desactiva |

Sin miembros públicos. Input (D-006): `Keyboard.current.tabKey.isPressed`, con comprobación de null. Sin sonido: GDD §4.7 no lo define.
No llama a `ParameterPanelUI.RefreshFromController`: Demo no conoce la UI y `SetEffectEnabled` no cambia el valor de ningún widget.

Comportamiento. Cada `LateUpdate` (tras el `Update` del `ObjectSelector`: el objeto recién elegido no se ve con efecto ni un frame) calcula el objetivo deseado y, si cambia, pasa del actual al nuevo:
1. Deseado = `_selector.Selected` si hay teclado, `tabKey.isPressed` y `!TextInputFocus.IsTyping`; si no, null.
2. Deseado ≠ actual: restaura el actual (si sigue vivo); guarda `IsEffectEnabled` del deseado y lo apaga con `SetEffectEnabled(false)`; actual = deseado.
3. Restaurar = `SetEffectEnabled(estadoGuardado)`, no `true`: si el efecto ya estaba apagado (tester), soltar TAB no lo enciende.
   Un `SetEffectEnabled` externo durante la comparación se pierde al soltar; el botón del tester muestra "OFF" mientras TAB está pulsado.
4. TAB pulsado y cambio de selección: el anterior vuelve y el nuevo se ve sin efecto. Deseleccionar: el anterior vuelve.
5. Con TAB pulsado, empezar a escribir en un campo de texto termina la comparación (LK-52: escribiendo, ningún atajo escucha). TMP ignora Tab en un
   campo de una línea. Al confirmar (Enter o clic fuera) con TAB aún pulsado, la comparación empieza al frame siguiente sin volver a pulsar: aceptado.
6. `OnDisable` (componente apagado, escena descargada, salir de Play) restaura el actual y lo pone a null. Objeto destruido: nada (comparación de Unity con null).
7. Sin selección, TAB no hace nada. Sin teclado: nada y sin warning. Cursor sobre la UI: TAB funciona igual (GDD §1.4 regla 5 es sólo para WASD).
8. Editar parámetros o pulsar Reset durante la comparación: los `Set*` y `ResetToDefaults` no escriben `_EffectEnabled`, así que el efecto
   sigue oculto y al soltar aparece con los valores nuevos.
9. Un shader sin `_EffectEnabled` (`Sprite-Unlit-Default` con `EFF_Debug`): la escritura se ignora en silencio (D-005). Sin error ni aviso.

## Banco de pruebas
`Assets/_Development/TestBench.unity`, tal como quedó LK-01 (commit `a633f0f`):
- `SPR_RuneCoin`: `MAT_Outline2D_Default` y `EFF_Outline2D`. Objeto de la prueba.
- `SPR_Crystal`: `Sprite-Unlit-Default` con `EFF_Debug`, sin `_EffectEnabled`. Control negativo del punto 9.
- `ComparisonToggle` en el objeto del `ObjectSelector`, montado por Claude por MCP, con `_selector` vacío (prueba el `GetComponent`).
- Panel (LK-11) para el punto 8; campo del valor del slider (LK-52) para el 5; `EffectDebugTester` en "Cube de Prueba",
  apuntando a la moneda desde LK-01, para el 3.

## Criterios de aceptación
**Verificador (MCP).** Compila sin errores ni warnings nuevos. `ComparisonToggle` montado en TestBench; en Play no avisa de selector
ausente. Play y Stop sin excepciones. En `Assets/LumiKit/`, sin `UnityEngine.Input`, `InputAction` ni `InputSystem.actions` (D-006),
y `ComparisonToggle.cs` sin `.material` ni `.materials` (D-001). Tras Play, `git status` sin `.mat` modificado.

**Usuario (ojo).**
- [ ] Moneda seleccionada: mantener TAB quita el contorno; soltar lo devuelve, en el mismo frame y sin parpadeo.
- [ ] Sin selección, TAB no hace nada. Cristal seleccionado y TAB: nada visible, consola limpia.
- [ ] TAB pulsado y clic en el cristal: la moneda recupera el contorno. TAB pulsado y clic en vacío: igual.
- [ ] TAB pulsado y clic en la moneda: pierde el contorno sin soltar y volver a pulsar.
- [ ] TAB pulsado y grosor movido en el panel: sigue sin contorno; al soltar vuelve con el grosor nuevo. Igual con Reset.
- [ ] Escribiendo en el campo del valor del slider, TAB no quita el contorno.
- [ ] Efecto apagado con el tester; pulsar y soltar TAB: sigue apagado. Encenderlo con el tester: vuelve.
- [ ] Sin confirmar: TAB pulsado y clic fuera de la ventana Game; soltar TAB y volver: la moneda tiene contorno. El paquete reinicia el teclado al
      perder el foco (`ResetAndDisableNonBackgroundDevices`), pero que un clic en otro panel del editor quite el foco es código nativo.

## Fuera de alcance
- Botón "Comparar" en pantalla (GDD línea 59) y recordatorio de teclas (GDD línea 137): aparcados en `docs/MVP_SCOPE.md`.
- Comparar todos los objetos a la vez · sonido · mando · máquina de estados `DEMO_*` (GDD §1.11).
