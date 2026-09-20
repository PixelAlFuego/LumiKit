# LK-22a — Pie del panel con Reset y `ui-style.md` al día
Estado: 🟡 implementado sin verificar (Sesión 07) · Corte y alcance aprobados por el usuario · Depende de: LK-09 (`ResetToDefaults`), LK-11a (`ParameterPanelUI`, `RefreshFromController`), LK-11b (los cuatro widgets) · Continúa en LK-22b (fuentes TMP) · Diseño: GDD §1.3 Mecánica 5 (líneas 65-67), §2.7 (líneas 547-588), §2.8 (línea 609) · Estilo: `.claude/rules/ui-style.md` · uGUI: D-007

## Objetivo
Dar al panel el pie fijo que pide el GDD (línea 609) con el botón Reset de la Mecánica 5: devuelve el
efecto seleccionado a sus valores por defecto y vuelca esos valores en los widgets. Es el primer
llamador de `RefreshFromController()`, que espera sin consumidor desde LK-11a. Y poner `ui-style.md`
al día con los tamaños reales de `LumiTheme` (Label y Mono a 16, no a 13), condensando para no pasar
de 60 líneas: los topes no se suben (Sesión 05). El botón Copiar es LK-29 y hoy no se crea; el
layout del pie lo deja entrar después sin tocar código.

## Archivos
- `UI/LumiTheme.cs` (existe) · aditiva, ~8 líneas: alto del pie, alturas, paddings y grosor de borde de botón del GDD §2.7. Cada valor derivado, comentado.
- `UI/ParameterPanelUI.cs` (existe) · aditiva, ~30 líneas: `using UnityEngine.UI`, campo `_resetButton`, el controller vivo y el manejador. El contrato de LK-11a no cambia.
- `Assets/Editor/ParameterPanelBuilder.cs` (existe) · aditiva, ~70 líneas: `BuildFooter`, `CreateButton` y tres líneas en `BuildPanel`. **La protección no cambia**: si un prefab existe, aborta.
- `.claude/rules/ui-style.md` (existe) · condensación in situ. Tope 60; sale en 60 o menos.
- Regenera (D-002): `PRF_ParameterPanel.prefab`. Los otros cinco salen idénticos, pero el generador es todo o nada: hay que borrar los seis.
- No se tocan: `Core/`, `Utils/`, `Demo/`, los cuatro widgets, `ParameterWidgetBase.cs`, `EffectDebugTester.cs`, ni `Assets/TextMesh Pro/`.

## Contrato
**`LumiTheme`** — aditiva. Nada de lo que ya hay cambia de valor.

| Constante | Valor | Origen |
|---|---|---|
| `BUTTON_HEIGHT_COMPACT` | 28 | GDD línea 583 |
| `BUTTON_PADDING` · `BUTTON_PADDING_COMPACT` | 16 · 12 | GDD línea 584 |
| `PANEL_FOOTER_HEIGHT` | `BUTTON_HEIGHT + 2 * PANEL_PADDING` = 68 | derivado: el GDD no da alto de pie |
| `BUTTON_BORDER` | 1 | borde del botón secundario (GDD línea 566) |

**`ParameterPanelUI`** — aditiva; nada de lo verificado en LK-11a y LK-11b cambia de comportamiento.
1. `[SerializeField] private Button _resetButton;` bajo una cabecera "Pie". Null → el panel funciona exactamente igual que hoy, sin warning y sin excepción: el pie es opcional.
2. Campo privado `_current`: el `EffectController` que se está mostrando. Lo escribe `Build`, también cuando es null. Es lo único que el pie necesita saber.
3. `OnEnable` añade el listener del botón; `OnDisable` lo quita. Mismo sitio que la suscripción al selector, para que no queden listeners colgando al desactivar el panel.
4. `HandleReset()`: con `_current` a null no hace nada; si no, `_current.ResetToDefaults()` y **después** `RefreshFromController()`. Ese orden y no el inverso: primero el modelo, luego la vista.
5. `_resetButton.interactable` sigue a `_current != null`. Sin selección el pie ya está oculto con `_surface`; el `interactable` es el cinturón sobre los tirantes.

Reset es **botón secundario** del GDD (líneas 561-567), no destructivo: el `#FF5A5A` está reservado al
*reset global* del menú de pausa (línea 577, LK-32). Raíz = borde `BorderStrong`, hijo insertado 1 px =
relleno, y el relleno es el `targetGraphic`. Estados por `ColorBlock` con `fadeDuration =
TRANSITION_SECONDS`: Normal, Presionado y Deshabilitado en `Surface`; Hover en `SurfaceElevated`. El
"Transparente" del GDD (línea 566) se pinta con `Surface`, el color del panel que hay detrás: un
relleno transparente de verdad dejaría ver entera la Image del borde. **Sólo cambia el relleno**; borde
y texto quedan fijos hasta LK-50.

**`ParameterPanelBuilder`** — aditiva.
1. `BuildFooter`: hijo de `Surface`, anclado abajo y a lo ancho, alto `PANEL_FOOTER_HEIGHT`, separador de 1 px `Border` en su borde superior. `HorizontalLayoutGroup` con padding `PANEL_PADDING`, `spacing = SPACING` y `childForceExpandWidth = true`: hoy Reset ocupa los 248 px útiles y el Copiar de LK-29 se reparte el ancho sin tocar nada.
2. `CreateButton`: `UISprite` en `Image.Type.Sliced` en borde y relleno —el radio es el del sprite de Unity, no los 6 px literales del GDD: un radio exacto pide un sprite generado—, etiqueta TMP centrada a `TEXT_LABEL` y `LayoutElement` de alto `BUTTON_HEIGHT`. El separador del pie lleva `ignoreLayout`: sin él, el `HorizontalLayoutGroup` lo pondría en fila junto al botón.
3. `BuildPanel` cablea `_resetButton` en el `SerializedObject` junto a los campos que ya cablea.

**`ui-style.md`** — misma información, menos líneas: paleta agrupada por familia en vez de tabla de 17 filas, Label y Mono a 16 con la nota de la Sesión 05, `SLIDER_VALUE_WIDTH` 56, y un bloque nuevo con la jerarquía de botones del GDD §2.7 y el pie. Ni un token ni una medida se pierden.

## Banco de pruebas
`Assets/_Development/TestBench.unity`, con el HUD de LK-11a y los seis parámetros de `EFF_Debug`.

| Qué | Cómo | Quién |
|---|---|---|
| Regenerar los prefabs | borrar los seis `PRF_*` de `Prefabs/UI/` **y el objeto `ParameterPanel` de la escena** (al borrar el prefab su instancia queda huérfana) y ejecutar los dos menús `LumiKit/UI/…` | usuario |

Sólo el `Color` de `EFF_Debug` tiene confirmación visual en el sprite; los otros cinco se verifican por ida y vuelta del dato, como en LK-11b, y el Reset se ve en los widgets, no en pantalla.
No verificable todavía: las fuentes (LK-22b), el sonido del click (LK-23), que un Float cambie algo en pantalla (LK-01).

## Criterios de aceptación (verificables en el editor)
- [ ] Compila sin errores ni warnings nuevos.
- [ ] Play + clic en `SPR_Crystal`: el pie aparece pegado al fondo del panel, con el separador de 1 px por encima y el botón Reset ocupando el ancho útil.
- [ ] Pasar el ratón por encima del botón lo ilumina y quitarlo lo devuelve, con transición, no de golpe.
- [ ] Mover dos o tres sliders, cambiar el color y el toggle, y pulsar Reset: **los seis widgets vuelven al defecto en el acto**, sin deseleccionar ni reseleccionar.
- [ ] Tras el Reset, el color del sprite vuelve al que tenía al seleccionar: el valor llegó al material y no sólo a la UI.
- [ ] Deseleccionar y volver a seleccionar tras un Reset: los valores siguen siendo los de defecto.
- [ ] Clic en el pie o en el botón: la selección no cambia y la cámara no se panea.
- [ ] A → B → A con un Reset por medio: el Reset actúa sobre el objeto mostrado, nunca sobre el anterior. Consola muda durante todo el recorrido.
- [ ] Tras salir de Play, `git status` no muestra cambios en `MAT_Debug.mat` ni en los materiales de los sprites (D-001).
- [ ] Grep en `Scripts/UI/`: ni un `new Color(` ni un hexadecimal fuera de `LumiTheme.cs`.
- [ ] `ui-style.md` en 60 líneas o menos, con Label y Mono a 16, y sin que falte ningún token de la paleta.

## Fuera de alcance
- Botón Copiar del pie → LK-29 (portapapeles). El layout ya lo admite.
- Componente de botón con la jerarquía completa del GDD §2.7 —primario, secundario, terciario y destructivo, con borde y texto por estado y escala 0.98 al presionar— → **LK-50**, abierta en esta sesión a petición del usuario. Sustituirá al botón de serie que se usa aquí, y este pie es su banco de pruebas.
- Fuentes TMP, logotipo y `Fonts/` → LK-22b y LK-13: hoy todo el HUD sigue con LiberationSans. Animación de 150 ms de la manija del toggle y envoltura del enum, heredadas de LK-11b: siguen abiertas y siguen sin dueño.
- Scroll del panel cuando los widgets pasen del alto útil: hoy `Content` crece hacia abajo y con muchos parámetros llegaría a solaparse con el pie. Con los seis de `EFF_Debug` no ocurre.
- Separadores entre grupos (GDD línea 609), descripción del efecto y colapsado con `‹`. Tocar `Core`, `Demo`, los widgets o el `EffectDebugTester`.
