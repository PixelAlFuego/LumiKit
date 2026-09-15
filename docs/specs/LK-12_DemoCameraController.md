# LK-12 — Cámara ortográfica 2D (DemoCameraController)
Estado: 🟡 implementado, pendiente de verificar en el editor · Depende de: — · Diseño: GDD §1.3 Mecánica 4 (línea 63), §1.4 regla 5 (línea 83), §3.2 (línea 668), §4.4 (línea 1073)

## Objetivo
Mover la cámara ortográfica de la demo 2D con `WASD` y arrastrando con botón derecho, sólo
en X e Y y dentro de un límite, con zoom de rueda acotado. Botón derecho y zoom: decisión
del usuario en la Sesión 02 (el zoom no está en el GDD).

## Archivos
`Assets/LumiKit/Runtime/Scripts/Demo/DemoCameraController.cs` (planeado) · `LumiKit.Demo`.
Único archivo nuevo. `LumiKit.Runtime.asmdef` ya referencia `Unity.InputSystem`: no se toca.

## Contrato
MonoBehaviour con `[RequireComponent(typeof(Camera))]`. Sin miembros públicos.

| Campo serializado | Tipo | Defecto | Uso |
|---|---|---|---|
| `_panSpeed` | float | 5 | unidades de mundo por segundo con `WASD` |
| `_bounds` | Rect | x -10 · y -6 · w 20 · h 12 | zona permitida para el centro de la cámara |
| `_zoomStep` | float | 0.5 | cambio de `orthographicSize` por cada frame con rueda |
| `_minOrthographicSize` | float | 2 | zoom más cercano |
| `_maxOrthographicSize` | float | 10 | zoom más lejano |

`OnValidate` fuerza mínimo ≥ 0.1 y máximo ≥ mínimo.
Input (D-006): `Keyboard.current` (`wKey` `aKey` `sKey` `dKey`) y `Mouse.current`
(`rightButton`, `position`, `scroll`), leídos en `Update`. Si uno es null, esa parte no actúa.

Comportamiento:
1. Sólo cambian X e Y de la posición y `orthographicSize`. Z y rotación nunca.
2. Paneo 1:1: el punto del mundo bajo el cursor al pulsar el botón derecho sigue bajo el cursor.
3. Zoom hacia el centro de la pantalla. Rueda hacia delante acerca (Size baja). Sólo cuenta
   el signo de `scroll.y`, no su magnitud. Size siempre dentro de [mínimo, máximo].
4. Tras cada movimiento, el centro se ajusta dentro de `_bounds`.
5. Cursor sobre UI (`EventSystem.current.IsPointerOverGameObject()`, en `Update`): ni `WASD`,
   ni inicio de paneo, ni zoom. Un paneo ya empezado sigue hasta soltar. Sin `EventSystem`
   no hay bloqueo. La cámara no lleva `Physics2DRaycaster`: con él, los sprites contarían como UI.
6. Si la `Camera` no es ortográfica: un warning por consola y el componente se desactiva.

### Reparto del ratón con LK-10
Botón derecho y rueda: LK-12. Botón izquierdo: LK-10; este archivo nunca lo lee.
Sin API pública ni orden de ejecución entre ambos.

## Banco de pruebas
Escena `Assets/_Development/TestBench.unity`. Montaje a mano en el editor, como en LK-09.

| Objeto | Componentes | Configuración |
|---|---|---|
| Main Camera | Camera, `DemoCameraController` | Projection Orthographic · Size 5 · posición (0, 0, -10) |
| Marker_Center | SpriteRenderer con sprite Square de Unity | posición (0, 0, 0) |
| Marker_NE · NW · SE · SW | SpriteRenderer con sprite Square de Unity | esquinas de `_bounds`: (±10, ±6, 0) |
| Marker_Crystal · Marker_RuneCoin | SpriteRenderer con `Assets/_Development/SPR_Crystal.png` · `SPR_RuneCoin.png` | (-3, 0, 0) y (3, 0, 0), para juzgar el zoom |

No verificable todavía:
- Ratón ignorado sobre la UI (regla 5) → diferido a LK-11: no hay panel ni `EventSystem`.
  Incluye el issue conocido del `OnGUI` de `EffectDebugTester` (ver STATE.md).

## Criterios de aceptación (verificables en el editor)
- [ ] Compila sin errores ni warnings nuevos en la consola.
- [ ] Buscar `Input.Get`, `Input.mouse` e `InputAction` en `Assets/LumiKit/` no da resultados (D-006).
- [ ] Play: `WASD` mueve la cámara en X/Y. Z y rotación no cambian.
- [ ] Botón derecho + arrastrar: el marcador bajo el cursor se queda bajo el cursor.
- [ ] Dirección del arrastre: cursor hacia arriba → la escena sube con él; hacia la derecha →
      va a la derecha. Si va al revés, es el fallo de signo.
- [ ] Botón izquierdo, con clic o arrastrando: la cámara no se mueve.
- [ ] Rueda hacia delante acerca y hacia atrás aleja. Size nunca sale del mínimo y máximo.
- [ ] Ni con `WASD` ni paneando sale el centro de `_bounds`: en el tope, el marcador de esa
      esquina queda en el centro de la pantalla.
- [ ] Mínimo mayor que máximo en el Inspector: se corrige solo.
- [ ] Con Projection = Perspective: un warning al entrar en Play y la cámara no se mueve.

## Fuera de alcance
- Zoom hacia el cursor. Flechas, gamepad y táctil. Suavizado o inercia.
- Cámara 3D y `OrbitCameraController` (GDD línea 762): fuera del MVP 2D.
- `ObjectSelector` — LK-10 · `ComparisonToggle` (TAB) — LK-24 · escena `02_Demo_2D` — LK-14.
