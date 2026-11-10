# Pasos en Unity
Sesión 13 · Diagnóstico del tirón periódico (sin tarea LK). Qué se sabe: `docs/STATE.md` > Dudas abiertas > Tirón periódico. Aquí, cómo se diagnostica.
Este archivo se reemplaza entero en cada tarea que necesite acciones del usuario en el editor de Unity.

De la prueba más barata a la más cara. **Para en cuanto una dé la respuesta.** Ya hecho: el `.exe` entregado, sin tirón (usuario, 2026-11-10). Por eso se empieza sin el puente del MCP.

## Antes de empezar
- Abre el proyecto `LumiKit_Project` con Unity 6000.0.83f1 (rama `develop`).
- **No guardes ninguna escena** (Ctrl+S o `File > Save`) después de pulsar Play. Si Unity pregunta si guardar al cambiar de escena o al cerrar: **Don't Save**.
- Qué mirar en cada Play, durante 20 s: el contorno animado de la runa (derecha) y el pulso del brillo del cristal (izquierda). Tirón = los dos se paran a la vez un instante.

## Paso 1 — Play sin el puente del MCP
Fuente (paquete `com.coplaydev.unity-mcp` 10.2.0, en `Library/PackageCache/com.coplaydev.unity-mcp@045e809812a8/`):
`Editor/MenuItems/MCPForUnityMenu.cs` (menú) y `Editor/Windows/Components/Connection/McpConnectionSection.cs` (botón). El paquete **no** tiene un menú que pare el puente: el menú abre una ventana y el puente se para con un botón de esa ventana.
0. Antes de abrir nada: ¿está abierta la ventana `MCP for Unity`, aunque sea como pestaña acoplada detrás de otra? Anota sí o no.
1. Menú `Window > MCP for Unity > Toggle MCP Window` (atajo Ctrl+Shift+M). Se abre la ventana `MCP for Unity`, pestaña `Connect`.
2. Sección `Server`. **Esperado:** el estado dice `Session Active (LumiKit_Project)` y al lado hay un botón `Disconnect`.
   Si dice `No Session`, el puente ya está parado: anótalo y salta al 4.
3. Clic en `Disconnect`. **Esperado:** el estado pasa a `No Session` y el botón, a `Connect`. No pulses `Stop Server` (para el servidor, no hace falta).
4. Cierra la ventana `MCP for Unity` (clic derecho en su pestaña > `Close Tab`). Abierta, revisa la conexión cada 2,0 s (`Editor/Windows/MCPForUnityEditorWindow.cs`, líneas 574 y 600-615).
5. Ventana `Project` > `Assets > LumiKit > Scenes` > doble clic en `02_Demo_2D` > Play. Mira 20 s (ver "Antes de empezar"). Anota si hay tirón.
6. Play otra vez para parar.
7. Comprueba que no se reconectó solo: Ctrl+Shift+M > pestaña `Connect`. **Esperado:** `No Session`. Al pulsar Play, el paquete sólo reanuda el puente si estaba conectado antes (`Editor/Services/HttpBridgeReloadHandler.cs`, línea 74).
8. Vuelve a conectar: botón `Connect`. **Esperado:** `Session Active (LumiKit_Project)`.

**Si en el 5 NO hay tirón:** la respuesta es el MCP. Para aquí y salta al "Paso 3 — Cierre". **Si lo hay:** sigue con el paso 2.

## Paso 2 — Profiler en el editor
1. Menú `Window > Analysis > Profiler`.
2. Arriba a la izquierda del Profiler, el desplegable del destino: `Play Mode`.
3. Módulo `CPU Usage` (gráfica de arriba). Si no está, menú `Profiler Modules` y márcalo.
4. Botón Record (círculo, arriba) activado.
5. Escena `02_Demo_2D` > Play. Espera ~10 s: en la gráfica aparecen picos que se repiten cada ~2 s.
6. Play otra vez para parar (el Profiler conserva lo grabado).
7. Clic en uno de esos picos. Panel inferior: en el desplegable de la vista (dice `Timeline`), elige `Hierarchy`.
8. Clic en la cabecera de la columna `Time ms` hasta que ordene de mayor a menor. Anota las 5 primeras filas con sus ms, tal como salen.
9. Repite 7 y 8 en otro pico distinto.

## Paso 3 — Cierre
1. Si desconectaste el puente y no lo reconectaste (paso 1.8), hazlo ahora.
2. Cierra Unity (**Don't Save** si pregunta).
3. Consola en la carpeta `LumiKit_Project`: `git status`. **Esperado:** `nothing to commit, working tree clean`. Si sale algún archivo modificado, no lo deshagas: cópialo en el reporte.

## Qué reportar
| Paso | ¿Hay tirón? | Observaciones |
|---|---|---|
| a. `.exe` entregado, contorno 20 s | no | Hecho (usuario, 2026-11-10) |
| 1. Play sin el puente | sí / no | Ventana abierta en 1.0 (sí/no) · estado en 1.2 y en 1.7 |
| 2. Profiler, pico 1 | sí / no | 5 filas: nombre · ms |
| 2. Profiler, pico 2 | sí / no | 5 filas: nombre · ms |

Además: texto de cada error rojo de la consola, si hubo, y la salida completa de `git status`.
