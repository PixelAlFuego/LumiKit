# LK-11b — Widgets Color, Toggle y Enum
Estado: 🟠 en curso · Alcance aprobado por el usuario (Sesión 05) · Depende de: LK-11a (`ParameterWidgetBase`, `ParameterPanelUI`, `ParameterPanelBuilder`) · Diseño: GDD §1.3 Mecánica 2 (líneas 50-53), §2.8 (líneas 595-607), §4.2 (líneas 901-904) · Estilo: `.claude/rules/ui-style.md` · Nombres: D-008 · uGUI: D-007

## Objetivo
Cerrar los tres `ParameterType` que faltan para que el panel deje de omitir parámetros. El contrato
de LK-11a no cambia: `ParameterPanelUI` sólo gana un campo de prefab y un `case` por tipo.
Unity no trae selector de color en runtime (`ColorPicker` vive en `UnityEditor` y no se puede
exportar): el widget de Color son tres sliders RGB más las seis muestras de la paleta, desplegables
bajo la muestra. Sin rueda ni campo hexadecimal (GDD línea 599): eso es una pantalla propia, LK-22.

## Archivos
Nuevos en `Assets/LumiKit/Runtime/Scripts/UI/Widgets/` · `LumiKit.UI.Widgets` (planeados):
`ColorParameterWidget.cs`, `ToggleParameterWidget.cs` y `EnumParameterWidget.cs`.
- `UI/ParameterPanelUI.cs` (existe) · aditiva, ~10 líneas: campos `_colorWidgetPrefab`, `_toggleWidgetPrefab`, `_enumWidgetPrefab` y sus tres `case` en `PrefabFor`.
- `UI/LumiTheme.cs` (existe) · aditiva: pista del toggle (36×20), muestra de color (36×24) y altos desplegados. Valores del GDD.
- `Assets/Editor/ParameterPanelBuilder.cs` (existe) · aditiva: cuatro constructores y cuatro rutas más en la comprobación de existencia. **La protección no cambia**: si un prefab existe, aborta.
- Genera (D-002): `PRF_Widget_Color`, `PRF_Widget_Toggle`, `PRF_Widget_Enum` y `PRF_Widget_EnumOption` en `Assets/LumiKit/Prefabs/UI/`.
- No se tocan: `Core/`, `Utils/`, `Demo/`, `ParameterWidgetBase.cs`, `SliderParameterWidget.cs`, `EffectDebugTester.cs`.

## Contrato
Los tres heredan de `ParameterWidgetBase`: `OnInitialize` cablea y termina en `OnRefresh()`;
`OnRefresh` lee del `Controller` y escribe el control sin notificar; al cambiar, escriben por el
setter tipado y llaman a `RaiseValueChanged()`. Los listeners se quitan en `OnDestroy`.

**`ColorParameterWidget`** — `SupportedType = Color`. Serializa `_swatchButton` (Button), `_swatchImage` (Image), `_expandRoot` (GameObject), `_redSlider` · `_greenSlider` · `_blueSlider` (Slider), `_paletteButtons` (Button[]) y `_layout` (LayoutElement).
1. Plegado: etiqueta a la izquierda y muestra de 36×24 a la derecha. La muestra es el botón que despliega.
2. Desplegado: tres sliders 0–1 (R, G, B) y una fila de seis muestras de `LumiTheme` (cian, magenta, violeta, ámbar, verde y `TextPrimary`). `_layout.preferredHeight` alterna entre los dos altos y el `VerticalLayoutGroup` del panel recoloca el resto solo.
3. Escribe `Controller.SetColor(PropertyName, new Color(r, g, b, Parameter.DefaultColor.a))`. El alfa no se edita: ningún shader del pack lo usa todavía.
4. Una muestra de la paleta escribe el `Image.color` de su propio botón (el tint de `Button` no altera ese valor) y refresca los tres sliders.
5. Sin HDR ni intensidad: `EffectParameter` no la tiene, y `MaterialPropertyHelper.ConvertColor` sigue pendiente de verificación visual (diferido a LK-01).
6. **El desplegable no sobrevive a un cambio de selección**: el panel destruye los widgets y los
   vuelve a instanciar, así que el Color vuelve plegado. Decidido, no descubierto: recordarlo
   obligaría al panel a guardar estado por parámetro, y el panel no guarda nada (LK-11a). Al
   reseleccionar el mismo objeto pasa igual, porque también reconstruye.

**`ToggleParameterWidget`** — `SupportedType = Boolean`. Serializa `_toggle` (Toggle), `_track` (Image), `_handle` (RectTransform) y `_handleImage` (Image).
1. `OnRefresh` usa `SetIsOnWithoutNotify` (existe en ugui 2.0.0, `Toggle.cs` línea 255) y repinta.
2. Al cambiar: `Controller.SetBool(...)`. Apagado: pista `Border`, manija `TextMuted` a la izquierda. Encendido: pista `LumiCyan`, manija `TextPrimary` a la derecha.
3. La manija salta de lado sin transición. Los 150 ms de ui-style necesitan animación → LK-22.

**`EnumParameterWidget`** — `SupportedType = Enum`. Serializa `_optionsRoot` (RectTransform con `HorizontalLayoutGroup`) y `_optionPrefab` (el botón de una opción).
1. `OnInitialize` vacía `_optionsRoot` e instancia un botón por entrada de `Parameter.EnumOptions`, capturando su índice. `EnumOptions` vacío: warning y widget inerte, sin botones.
2. Al pulsar: `Controller.SetEnum(PropertyName, índice)` y repinta: activo `LumiCyan` + texto `TextOnAccent`; el resto `SurfaceElevated` + `TextSecondary`.
3. Los botones se reparten el ancho a partes iguales. Con más de cuatro opciones el texto se aprieta: ni se envuelve ni hay scroll horizontal (LK-22).

## Banco de pruebas
`Assets/_Development/TestBench.unity`, con el HUD montado en LK-11a.

| Qué | Cómo | Quién |
|---|---|---|
| `EFF_Debug.asset`: hecho en la Sesión 05 — `_BaseColor` (Color, blanco), `_Pulse` (Boolean, off) y `_OutlineMode` con las tres opciones. **Falta cambiarle el Type a `Enum`**: está guardado como `Float` (`_type: 0`) y saldría como slider | Inspector | usuario |
| Regenerar los prefabs | borrar los `PRF_*` de `Prefabs/UI/` **y el objeto `ParameterPanel` de la escena** (al borrar el prefab su instancia queda huérfana), y ejecutar los dos menús `LumiKit/UI/…` | usuario |

Con `_BaseColor` de vuelta, el warning del `EffectDebugTester` desaparece y el color **sí se ve** en
el material URP Unlit: es el único de los tres con confirmación visual. `_Pulse` y `_OutlineMode` no
cambian nada en pantalla —no existe ningún shader del pack (LK-01) y un MPB no activa keywords
(D-005)—: se verifican por ida y vuelta del dato, deseleccionando y volviendo a seleccionar.

## Criterios de aceptación (verificables en el editor)
- [ ] Compila sin errores ni warnings nuevos.
- [ ] Clic en `SPR_Crystal`: el panel muestra los cinco parámetros con su widget y ya no avisa de tipos omitidos.
- [ ] La muestra de color abre y cierra el desplegable, y las filas de debajo se recolocan sin solaparse.
- [ ] Mover R, G o B cambia el color del sprite en el acto y repinta la muestra del widget.
- [ ] Pulsar una muestra de la paleta: el sprite toma ese color y los tres sliders saltan a sus valores.
- [ ] El toggle cambia de color y la manija cambia de lado.
- [ ] En el enum sólo hay un botón activo, y es el último pulsado.
- [ ] Deseleccionar y volver a seleccionar: color, toggle y enum vuelven con lo elegido, no con el defecto.
- [ ] `ResetToDefaults()` en el tester y luego `Refresh from controller` en el panel: los cinco widgets vuelven al defecto.
- [ ] A → B → A: ni widgets duplicados ni botones de enum acumulados.
- [ ] Arrastrar un slider RGB no panea la cámara ni cambia la selección; el tester sigue funcionando.
- [ ] Tras salir de Play, `git status` no muestra cambios en `MAT_Debug.mat` (D-001).

## Fuera de alcance
- Rueda de color, campo hexadecimal, alfa e intensidad HDR → LK-22 y LK-01.
- Animación de 150 ms del toggle, envoltura del enum y scroll del panel → LK-22.
- Pie del panel con Reset y Copiar → LK-22 (necesita el estilo de botón) y LK-29 (portapapeles).
  Confirmado por el usuario en la Sesión 05 y ya movido en BACKLOG.
- Localización: las opciones del enum salen tal cual están en el asset → LK-19.
- Tocar `Core`, `Demo`, `ParameterWidgetBase`, `SliderParameterWidget` o el `EffectDebugTester`.
