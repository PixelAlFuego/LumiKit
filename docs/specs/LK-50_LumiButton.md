# LK-50 — Componente de botón con la jerarquía del GDD §2.7
Estado: ⬜ spec escrita, pendiente de aprobación (Sesión 08) · Depende de: LK-22a (su pie es el banco de pruebas) · Independiente de LK-51: esto es comportamiento, no arte · Diseño: GDD §2.7 (líneas 547-588) · Estilo: `.claude/rules/ui-style.md` > Medidas > Jerarquía · uGUI: D-007

## Objetivo
Un `LumiButton` con los cuatro niveles del GDD —primario, secundario, terciario y destructivo— en los
que **el texto y el borde también viran por estado**, no sólo el fondo, y con la escala 0.98 al
presionar. Sustituye al `Button` de serie que LK-22a dejó en el pie del panel, que es lo que hoy sólo
cambia el relleno. **No necesita ni un sprite**: los 16 pares de color y la escala salen de
`LumiTheme`. Lo que sigue necesitando arte —esquinas y contorno de verdad— es LK-51, y va aparte.

## Archivos
- `Assets/LumiKit/Runtime/Scripts/UI/LumiButton.cs` (planeado) · `LumiKit.UI`.
- `UI/LumiTheme.cs` (existe) · aditiva, ~4 líneas: `LumiCyanHover` (`#33EBDD`) y `LumiCyanPressed` (`#00C4B6`), que son del GDD (líneas 555-556) y todavía no están. `Transparent` ya existe: lo añadió LK-51.
- `Assets/Editor/LumiButtonEditor.cs` (planeado) · `LumiKit.Editor`, ~15 líneas: `[CustomEditor(typeof(LumiButton))]` que llama a `base.OnInspectorGUI()` y dibuja debajo los cuatro campos nuevos. Sin él no se ven; ver Fuera de alcance.
- `Assets/Editor/ParameterPanelBuilder.cs` (existe) · aditiva, ~15 líneas en `CreateButton`: monta `LumiButton` en vez de `Button`, le cablea borde, relleno y etiqueta, y le pasa el nivel. El resto del generador no se toca.
- `UI/ParameterPanelUI.cs` (existe) · **no se toca**. `LumiButton` hereda de `Button`, así que el campo `_resetButton` sigue siendo válido y `onClick` sigue funcionando igual.
- No se tocan: `Core/`, `Utils/`, `Demo/`, los cuatro widgets, `EffectDebugTester.cs`, ni los prefabs a mano (D-002: se regeneran).

## Contrato
**`LumiButton : Button`.** Hereda para no reimplementar navegación, `interactable` ni `onClick`.
Serializa `_style` (enum `LumiButtonStyle`: `Primary`, `Secondary`, `Tertiary`, `Destructive`),
`_fill` (Image), `_border` (Image) y `_label` (TMP_Text). Los tres últimos son opcionales: un nivel
sin borde deja `_border` a null y el componente no se entera.

1. `transition = Transition.None`. El `ColorBlock` de uGUI sólo sabe teñir **un** gráfico, y aquí hay tres. El pintado lo lleva entero el componente.
2. Override de `DoStateTransition(SelectionState, bool)` —existe, `Selectable.cs` línea 655—, que para cada estado resuelve los tres colores del nivel y los aplica con `Graphic.CrossFadeColor(color, TRANSITION_SECONDS, true, true)`, que es la misma API que usa `Selectable` por dentro. De ahí salen los 150 ms del GDD (línea 587) en fondo, borde y texto a la vez.
3. **`_fill.color`, `_border.color` y `_label.color` se quedan en blanco** en el prefab. `CrossFadeColor` escribe en el `CanvasRenderer`, que multiplica con el color del `Graphic`: si el `Graphic` no es blanco, el color que se ve no es el pedido. Es el mismo motivo por el que el relleno de LK-22a ya es blanco.
4. Con `instant == true` (al habilitar, al perder el foco) se aplica sin desvanecido: `CrossFadeColor` con duración 0.
5. Escala: `transform.localScale` a `PRESS_SCALE` en `Pressed` y a 1 en el resto, **sin interpolar**. Suavizarla pide un `Update` o una corrutina y no compensa para una pulsación. Escalar la raíz no descoloca el `HorizontalLayoutGroup` del pie: el layout mide el rect, no la escala.
6. Los 16 pares de color no se copian aquí: viven en `ui-style.md` > Medidas > Jerarquía, que a su vez los referencia del GDD §2.7 (docs-style, regla 4). Al implementar se leen de ahí y se escriben una sola vez en una tabla estática dentro de `LumiButton`, con los tokens de `LumiTheme`, nunca con hexadecimales.

**El nivel secundario ya es transparente en Normal** desde LK-51: el borde es `SPR_UI_Rect_R6_Outline`,
un contorno con el centro vacío, y el relleno usa `LumiTheme.Transparent` (`Surface` con alfa 0) con
`pixelsPerUnitMultiplier` 1.2. La tabla de esta tarea hereda esos tokens. Lo que añade es que el
**borde** y el **texto** viren.

## Banco de pruebas
`Assets/_Development/TestBench.unity`, con el HUD y el pie ya verificados en LK-22a.

| Qué | Cómo | Quién |
|---|---|---|
| Regenerar los prefabs | borrar los seis `PRF_*` de `Prefabs/UI/` **y el objeto `ParameterPanel` de la escena**, y ejecutar los dos menús `LumiKit/UI/…` | usuario |
| Ver los cuatro niveles, no sólo el secundario | el pie sólo monta uno. Para los otros tres, cambiar `_style` en el Inspector del botón `Reset` de la escena —visible gracias a `LumiButtonEditor`— y volver a entrar en Play. La escena no se guarda con ese cambio | usuario |

El botón Reset del pie es `Secondary`. `Primary`, `Tertiary` y `Destructive` no tienen todavía ningún
sitio propio en la UI: llegan con LK-13, LK-18, LK-25 y LK-32. Se verifican a mano como dice la tabla.

## Criterios de aceptación (verificables en el editor)
- [ ] Compila sin errores ni warnings nuevos.
- [ ] Play + selección: el pie se ve igual que en LK-51 en reposo. Nada ha empeorado.
- [ ] Pasar el ratón por el botón: **fondo, borde y texto cambian a la vez**, con desvanecido, no de golpe.
- [ ] Mantener pulsado: el texto y el borde se ponen cian y el botón se encoge un pelo. Al soltar, vuelve.
- [ ] El Reset sigue reiniciando los seis widgets: el `onClick` heredado no se ha roto.
- [ ] Deseleccionar: el botón se deshabilita y se ve apagado, sin quedarse en el color del hover.
- [ ] Sacar el ratón del botón mientras se mantiene pulsado: no se queda encogido ni con el color de presionado.
- [ ] Con `_style` en `Primary`: fondo cian y texto oscuro. En `Destructive`: borde y texto rojos.
- [ ] Un `LumiButton` con `_border` o `_label` a null no lanza excepción: pinta lo que tenga.
- [ ] Grep en `Scripts/UI/`: ni un `new Color(` ni un hexadecimal fuera de `LumiTheme.cs`.
- [ ] Tras salir de Play, `git status` sin cambios en `MAT_Debug.mat` ni en `TestBench.unity`.

## Fuera de alcance
- Esquinas redondeadas y contorno con centro transparente: ya los pone **LK-51** (✅, Sesión 10). Esta tarea no toca sprites. Iconos: otra tarea (GDD línea 473).
- Suavizar la escala a lo largo de los 150 ms: pediría un `Update` por botón.
- Sonido de hover y de click → LK-23, que ya tiene los `.wav` en `Assets/LumiKit/Audio/SFX/`.
- Aplicar el componente fuera del pie: barra superior, menú, pausa y créditos → LK-13, LK-18, LK-25, LK-32.
- **El comprador no verá los campos en el Inspector.** `ButtonEditor` está declarado `[CustomEditor(typeof(Button), true)]` (`ButtonEditor.cs` línea 5), así que dibuja también las clases hijas y se come los cuatro campos nuevos. `LumiButtonEditor` lo arregla aquí, pero vive en `Assets/Editor/`, que **no se exporta**: el pack no tiene hoy dónde poner un script de editor propio. Anotado en STATE > Dudas abiertas; se decide en LK-27.
- Tocar `Core`, `Demo`, los widgets o el `EffectDebugTester`.
