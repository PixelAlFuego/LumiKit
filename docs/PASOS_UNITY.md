# Pasos en Unity
Sesión 13 · Criterios sin confirmar de LK-13, LK-14 y LK-24. Qué se verifica: `docs/STATE.md` > Pendiente de verificación. Aquí, cómo.
Este archivo se reemplaza entero en cada tarea que necesite acciones del usuario en el editor de Unity.

## Antes de empezar
- Abre el proyecto `LumiKit_Project` con Unity 6000.0.83f1 (rama `develop`).
- **No guardes ninguna escena** (Ctrl+S o `File > Save`) después de pulsar Play. Si Unity pregunta si guardar al cambiar de escena o al cerrar: **Don't Save**.
- Consola de Unity: `Window > General > Console`. Anota el texto de cualquier línea roja (error).

## Paso 1 — LK-14: la esquina transparente no selecciona (editor)
1. Ventana `Project` > `Assets > LumiKit > Scenes` > doble clic en `02_Demo_2D`.
2. Botón Play (▶, arriba al centro). **Esperado:** fondo liso oscuro; a la izquierda un cristal con brillo; a la derecha una runa (moneda) con contorno cian; suena música.
3. Clic en el centro del cristal. **Esperado:** suena un clic de selección y se abre el panel de parámetros con el título "Brillo 2D".
4. Clic en el fondo vacío, lejos de los dos objetos. **Esperado:** el panel se cierra.
5. Clic en una esquina del cristal donde no hay dibujo (la imagen es un cuadrado con las esquinas transparentes). **Esperado:** no suena nada y no se abre ningún panel.
6. Repite 3 a 5 con la runa. Título esperado en el 3: "Contorno 2D".
7. Botón Play otra vez para parar.

## Paso 2 — Build de Windows
1. Si conservas la carpeta del build entregado el 2026-10-02, usa **ese** build y salta al paso 3. No compiles uno nuevo.
2. Sólo si ya no la tienes: `File > Build Profiles`. Selecciona la plataforma de Windows (si no es la activa: `Switch Platform`).
   En `Scene List` deben estar marcadas, en este orden: `Assets/LumiKit/Scenes/01_MainMenu` (primera) y `Assets/LumiKit/Scenes/02_Demo_2D`. No cambies nada más.
3. Botón `Build` > elige una carpeta nueva **fuera** de la carpeta del proyecto. **Esperado:** termina sin errores rojos en la consola.
   Al compilar, Unity puede reescribir `Assets/Settings/*.asset` y `ProjectSettings/GraphicsSettings.asset`: no lo deshagas, sale en el paso 6.

## Paso 3 — LK-13: arranque y "Salir" (build)
1. Doble clic en el `.exe` de la carpeta del build. **Esperado:** arranca en el menú: título, subtítulo, botones "Demo 2D" y "Salir" y número de versión.
2. Clic en "Salir". **Esperado:** la aplicación se cierra sola, sin ventana colgada ni mensaje de error.

## Paso 4 — LK-14: build a 16:10 y 16:9 (build)
1. Explorador de Windows > carpeta del build > clic en la barra de dirección > escribe `cmd` y Enter. Se abre una consola en esa carpeta.
2. **16:10.** Escribe (cambia `LumiKit.exe` por el nombre real del `.exe`):
   `LumiKit.exe -screen-fullscreen 0 -screen-width 1440 -screen-height 900`
   **Esperado:** se abre en ventana de 1440×900. Unity exige una resolución que el monitor soporte; si la ventana sale de otro tamaño, anota cuál.
3. En la ventana: "Demo 2D". **Esperado:** suena la transición, carga la demo y la música sigue sin cortarse.
4. Comprueba en la demo: a. Lo esperado en el paso 1.2. b. Clic en el cristal: "Brillo 2D"; en la runa: "Contorno 2D".
   c. Esquina transparente de cada uno: no selecciona. d. Runa seleccionada y TAB mantenido: sin contorno; al soltar, vuelve. Clic en vacío cierra el panel.
   e. Sin mover la cámara, el panel no tapa la runa y nada sale recortado por los bordes.
5. Botón "Menú" (arriba a la izquierda) > "Salir".
6. **16:9.** Repite 2 a 5 con `-screen-width 1600 -screen-height 900`.

## Paso 5 — LK-24: TAB con el efecto apagado por el tester (editor)
El tester es un recuadro gris arriba a la izquierda de la vista `Game`. Lo dibuja el componente `EffectDebugTester`, que se ve en el `Inspector`
al seleccionar el objeto `Cube de Prueba` en la ventana `Hierarchy`. Su primera línea dice `LK-10 · seleccionado: <nombre>` (`ninguno` sin selección).
**Ojo:** el botón del tester usa OnGUI, el sistema de interfaz antiguo de Unity, que no pasa por el `EventSystem` (issue de la Sesión 02): el clic lo
atraviesa y deselecciona la moneda. Sin selección, TAB no hace nada y la prueba no valdría. Por eso, tras cada botón, se vuelve a seleccionar la moneda.
1. Ventana `Project` > `Assets > _Development` > doble clic en `TestBench` > Play.
2. Clic en la moneda (`SPR_RuneCoin` en `Hierarchy`; es la que tiene contorno cian). **Esperado:** el tester dice `seleccionado: SPR_RuneCoin`.
3. Clic en el botón "Efecto: ON — pulsa para apagar". **Esperado:** el contorno desaparece; el botón pasa a "Efecto: OFF — pulsa para encender".
4. Clic otra vez en la moneda. **Esperado:** `seleccionado: SPR_RuneCoin`, sigue sin contorno.
5. Pulsa y suelta TAB. **Esperado:** sigue sin contorno.
6. Clic en "Efecto: OFF — pulsa para encender". **Esperado:** vuelve el contorno.
7. Clic otra vez en la moneda; mantén TAB. **Esperado:** sin contorno mientras lo mantienes; al soltar, vuelve.
8. Play otra vez para parar. No guardes la escena.

## Paso 6 — Cierre
1. Cierra Unity (**Don't Save** si pregunta).
2. Consola en la carpeta `LumiKit_Project`: `git status`. **Esperado:** `nothing to commit, working tree clean`.

## Qué reportar
- Una tabla: paso (1.3, 1.5, 3.2, 4.4c…) · OK o FALLO · qué viste, si falló.
- Build usado: el entregado el 2026-10-02 o uno nuevo.
- Tamaño real de las dos ventanas del paso 4.
- Texto de cada error rojo de la consola, si hubo.
- Salida completa de `git status`.
