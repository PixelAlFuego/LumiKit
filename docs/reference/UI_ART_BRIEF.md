# Brief de arte de interfaz
Lista de sprites que el usuario dibuja para que el generador monte la UI con arte propio en vez de
con `UI/Skin/UISprite.psd` y `UI/Skin/Knob.psd` de Unity. Escrito en la Sesión 07.
Nada de esto está implementado. La tarea a la que pertenece **está sin decidir**: o amplía LK-50 o
es una tarea nueva. D-002 no cambia: los prefabs los sigue generando `ParameterPanelBuilder`.

## Lo que ya se puede hacer sin dibujar nada
Con `LumiTheme` sola, tal y como está hoy, se llega a todo esto:
- Los 15 colores de la paleta y toda la escala tipográfica.
- Rectángulos macizos, reglas de 1 px, separadores, el borde izquierdo del panel.
- Todas las medidas: anchos, altos, padding, separación, resolución de diseño.
- Hover, presionado y deshabilitado con desvanecido de 150 ms (`ColorBlock` de uGUI).
- Opacidad, como el 90 % del fondo del tooltip: es alfa en un `Color`.

Lo que **no** se puede sin sprite: **esquinas redondeadas, círculos e iconos**. Nada más.
Los degradados tampoco, pero el GDD ya los prohíbe en UI, así que no se pierde nada.
El texto y el borde virando por estado, y la escala 0.98 al presionar, **tampoco necesitan sprite**:
necesitan un componente de botón, que es LK-50. Son dos frentes independientes.

## Sprites a dibujar
Todo a **3×** (3 px de textura = 1 px de interfaz a 1920×1080). Seis archivos, más uno opcional.

| Archivo | Textura | Borde 9-slice (L,T,R,B) | Da como resultado | Sustituye a |
|---|---|---|---|---|
| `SPR_UI_Rect_R6.png` | 40×40 | 18, 18, 18, 18 | rect. macizo, radio 6 | `UISprite` en botón y opción de enum |
| `SPR_UI_Rect_R6_Outline.png` | 40×40 | 18, 18, 18, 18 | mismo contorno, trazo de 3 px de textura (= 1 px), centro transparente | el apaño de borde+relleno de LK-22a |
| `SPR_UI_Rect_R4.png` | 28×28 | 12, 12, 12, 12 | rect. macizo, radio 4 | `UISprite` en muestra de color y muestras de paleta |
| `SPR_UI_Pill.png` | 62×60 | 30, 0, 30, 0 | cápsula de 20 px de alto, radio completo | `UISprite` en la pista del toggle |
| `SPR_UI_Circle.png` | 48×48 | **sin 9-slice** | círculo ⌀16 | `Knob` en manija de slider y de toggle |
| `SPR_UI_Ring.png` | 48×48 | **sin 9-slice** | aro ⌀16, trazo de 6 px de textura (= 2 px) | **pieza suelta del kit**: sigue en el pack, pero el generador no la usa desde la Sesión 10 (hilo oscuro entre aro y núcleo; la manija del slider es `SPR_UI_Circle` cian con el núcleo encima) |
| `SPR_UI_Track.png` (opcional) | 14×12 | 6, 0, 6, 0 | riel de 4 px con las puntas redondeadas | `UISprite` en el riel del slider |

El riel es opcional porque a 4 px de alto un radio de 2 casi no se ve. Si no lo dibujas, se queda
con esquinas rectas y no desentona.

**La cápsula sólo se estira a lo ancho**, por eso su borde superior e inferior es 0: los 60 px de
textura son los 20 px de alto fijos del toggle, y sólo los 2 px centrales se estiran.

### Cómo dibujarlos
1. **En blanco puro (`#FFFFFF`) y con la forma en el canal alfa.** El color se lo pone `Image.color`
   desde `LumiTheme` en tiempo de generación. Si horneas color en el PNG, la paleta deja de mandar
   y se rompe D-007. Es la regla que más importa de esta lista.
2. Antialias en el alfa, sí. Sombra, brillo o degradado, no: ni el GDD los usa ni se pueden tintar.
3. El trazo de los dos contornos (`_Outline`, `_Ring`) va **hacia dentro**, no centrado en el borde:
   si sobresale, el 9-slice lo recorta.
4. Nada de márgenes transparentes de más alrededor de la forma: el borde del 9-slice se mide desde
   el píxel 0 de la textura.

### Por qué 3× y PPU 300
uGUI dibuja el borde de un 9-slice como `borde_en_píxeles_de_textura / (PPU_del_sprite / 100)`
(`Image.cs`, líneas 751 y 1064, del paquete `com.unity.ugui` que hay en disco). Con PPU 300 y un
borde de 18, sale un radio de 6 px de interfaz, que es el del GDD (línea 585), y la textura tiene
resolución de sobra para una pantalla 4K. Si prefieres dibujar a 1×, divide todos los números entre
tres y pon PPU 100: el resultado es el mismo tamaño, pero se ve blando en pantallas grandes.

## Ajustes de importación
Los mismos en los siete. Los pone el usuario en el Inspector.

| Ajuste | Valor | Por qué |
|---|---|---|
| Texture Type | Sprite (2D and UI) | |
| Sprite Mode | Single | |
| Pixels Per Unit | **300** | lo de arriba |
| Mesh Type | **Full Rect** | `Tight` rompe el 9-slice. Es el error más común |
| Generate Physics Shape | off | la UI no colisiona |
| Wrap Mode | Clamp · Filter Mode | Bilinear | |
| Compression | **None** | un borde redondeado enseña los artefactos de compresión; pesan poco |
| Alpha Is Transparency | on | si no, el antialias del borde sale con halo |
| sRGB (Color Texture) | on | |
| Generate Mip Maps | **off** | la UI va a escala fija |

El **borde del 9-slice no está en el importador**: se marca en `Sprite Editor` (botón del Inspector),
en los campos `Border L / T / R / B`, con los valores de la tabla de sprites. Sin eso el sprite se
estira entero y las esquinas se deforman.

## Qué habría que añadir al código
- **`LumiTheme`**: sólo medidas. `RADIUS` (6) ya existe; faltaría `RADIUS_SMALL` (4, muestra de color
  y tooltip, GDD líneas 601 y 621). Nada más: las medidas de cápsula y círculo ya están.
- **`ParameterPanelBuilder`**: las siete rutas como `const string`, un `RequireSprites()` que aborte
  nombrando las que falten —igual que `HasDefaultFont()` y que el `RequireFonts()` de LK-22b— y
  cambiar las 10 llamadas a `CreateImage` para que pasen el sprite del pack en vez del de Unity.
- **`LumiTheme` no puede guardar los `Sprite`**: es una clase estática y no serializa assets. Por eso
  las rutas van en el generador, que los resuelve con `AssetDatabase` y los hornea en el prefab.
  Es el mismo patrón que las fuentes de LK-22b, y mantiene D-002 y D-007.
- Carpeta: `Assets/LumiKit/Sprites/UI/` (planeada; `Sprites/` existe y está vacía). Prefijo `SPR_`
  según CONVENTIONS.md.

**Bifurcación, para decidir cuando toque:** lo anterior hornea los sprites en el prefab, así que
cambiar el arte obliga a regenerar. La alternativa es un ScriptableObject `LumiSkin` con las
referencias, que el comprador podría cambiar sin regenerar nada. Es más alcance y no hace falta para
el MVP, pero como argumento de venta ("reskinea el pack") no es poca cosa.

## Consecuencia buena que conviene no perder
Con `SPR_UI_Rect_R6_Outline` el botón secundario pasa a tener un contorno de verdad con el centro
transparente. Eso cierra la deuda que LK-22a dejó anotada en STATE: hoy el relleno va en `Surface`
en vez de transparente porque por detrás hay una `Image` de borde que se vería entera.

## Lo que este brief no cubre
- Iconografía Lucide / Tabler (GDD línea 473): son otro tipo de asset y otra tarea.
- Logotipo (GDD línea 373) y splash → LK-13.
- Fuentes → LK-22b, que sigue su camino y no depende de nada de aquí.
- Empaquetar los sprites en un `SpriteAtlas`: se puede hacer después sin tocar el generador.
