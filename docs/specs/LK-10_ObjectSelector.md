# LK-10 — Selección de objetos por raycast (ObjectSelector)
Estado: 🟡 implementado, sin verificar en el editor · Depende de: LK-09 (`EffectController`) · Diseño: GDD §1.3 Mecánica 1 (líneas 38-42), §1.4 regla 1 (línea 79), §4.4 (líneas 1044, 1072), §4.8 (línea 1178)

## Objetivo
Clic izquierdo sobre un objeto con `EffectController` lo convierte en el objeto activo; sólo uno a
la vez; clic en vacío deselecciona; el cambio se notifica por evento, para que LK-11 no sondee.
Raycast por código con `Physics2D`, sin `Physics2DRaycaster` (usuario, Sesión 03). Derecho y rueda son de LK-12.

## Archivos
- `Assets/LumiKit/Runtime/Scripts/Demo/ObjectSelector.cs` (planeado) · `LumiKit.Demo`. Nuevo.
- `Assets/_Development/Tests/EffectDebugTester.cs` (existe) · edición aditiva: se suscribe al evento y muestra el nombre. Desechable, fuera del pack y de CODEMAP.
- No se tocan: `LumiKit.Runtime.asmdef` (ya referencia `Unity.InputSystem`), `Core`, `Utils`, `DemoCameraController.cs`.

## Contrato
MonoBehaviour suelto (sin `RequireComponent`). Se monta en un objeto de sistemas, no en la cámara.

| Campo serializado | Tipo | Defecto | Uso |
|---|---|---|---|
| `_camera` | Camera | null | Cámara del raycast. Si es null, `Camera.main` en `Awake` |
| `_selectableLayers` | LayerMask | Everything | Capas que ve el raycast. En TestBench: sólo `Selectable` |

| Miembro público (lo consume LK-11) | Firma | Comportamiento |
|---|---|---|
| `Selected` | `public EffectController Selected { get; private set; }` | Objeto activo. `null` si no hay. `Selected.gameObject` da el objeto; `Selected.Definition`, el efecto |
| `OnSelectionChanged` | `public event Action<EffectController> OnSelectionChanged` | **Vía única de notificación**: se dispara sólo cuando cambia y pasa el nuevo valor, `null` al deseleccionar. Reclicar el objeto ya activo no lo dispara |
| `ClearSelection()` | `public void ClearSelection()` | Deselecciona por código. Dispara el evento si había selección |

Input (D-006): `Mouse.current.leftButton.wasPressedThisFrame` y `Mouse.current.position`, en `Update`. Si `Mouse.current` es null, no actúa.

Comportamiento:
1. Al pulsar el izquierdo: `Physics2D.GetRayIntersection(_camera.ScreenPointToRay(pos),
   Mathf.Infinity, _selectableLayers)`. No `OverlapPoint`: ésta tiene en cuenta la Z del collider,
   así que entre dos sprites a distinta profundidad gana el de delante.
2. Impacto con `EffectController` → pasa a activo (GDD línea 40). `GetComponent` en el GameObject
   del collider, ni padres ni hijos: los prefabs demo lo llevan en la raíz (GDD línea 1178).
3. Impacto en un collider de la capa **sin** `EffectController` → deselecciona, igual que un clic en
   vacío. Sin warning: el clic es demasiado frecuente para avisar por consola.
4. Sin impacto → deselecciona. Seleccionar otro deselecciona el anterior (GDD §1.4 regla 1). Sin multiselección.
5. Cursor sobre UI: el clic se ignora. `EventSystem.current` **puede ser null** (TestBench no tiene
   UI): se comprueba antes de `IsPointerOverGameObject()` o el primer clic lanza `NullReferenceException`.
6. Sin cámara (`_camera` null y sin `Camera.main`): warning y el componente se desactiva.
7. `Physics2D` no ve colliders 3D: un objeto con `BoxCollider` nunca se selecciona.

## Banco de pruebas
`Assets/_Development/TestBench.unity`, estado y capa `Selectable` confirmados por el usuario (Sesión
03). Falta añadir un GameObject vacío `Selector` con `ObjectSelector` (`_camera` = Main Camera ·
`_selectableLayers` = sólo `Selectable`).

| Objeto | Posición | Capa | Collider | `EffectController` |
|---|---|---|---|---|
| Marker_Center | (0,0,0) | Selectable | BoxCollider2D | sí, `EFF_Debug` |
| Marker_NE · NW · SE | (10,6,0) · (-10,6,0) · (10,-6,0) | Selectable | BoxCollider2D | sí, `EFF_Debug` |
| Marker_SW | (-10,-6,0) | Default | BoxCollider2D | no · control negativo de la máscara |
| SPR_Crystal · SPR_RuneCoin | (3,0,0) · (-3,0,0) | Selectable | PolygonCollider2D | sí, `EFF_Debug` |
| Cubo de LK-09 | (6,3,0) | Default | 3D | sí · control negativo de `Physics2D`. Lleva el `EffectDebugTester`, al que se le asigna el `Selector` en el Inspector |

No verificable todavía: contorno cian (línea 42) → LK-01 · sonido `SFX_UI_Select` (línea 1149) →
LK-23 · clic ignorado sobre UI (punto 5) → LK-11: no hay panel ni `EventSystem`, y el `OnGUI` del
tester tampoco pasa por él (issue conocido, STATE.md).

## Criterios de aceptación (verificables en el editor)
- [ ] Compila sin errores ni warnings nuevos en la consola.
- [ ] Buscar `Input.Get`, `Input.mouse` e `InputAction` en `Assets/LumiKit/` no da resultados (D-006).
- [ ] Play: clic sobre Marker_Center → la línea del tester muestra su nombre y el contador sube a 1.
- [ ] Clic en zona vacía → la línea pasa a "ninguno" y el contador sube.
- [ ] Clic sobre SPR_Crystal con Marker_Center activo → la línea cambia a SPR_Crystal.
- [ ] Reclicar SPR_Crystal, ya activo → el nombre no cambia y **el contador no sube**.
- [ ] Clic dentro del rectángulo de SPR_Crystal pero fuera de su silueta → pasa a "ninguno".
- [ ] Clic sobre Marker_SW (capa `Default`, fuera de la máscara) → "ninguno"; su nombre no sale nunca.
- [ ] Clic sobre el cubo de LK-09 (collider 3D) → "ninguno"; su nombre no sale nunca.
- [ ] Primer clic de la sesión sin `EventSystem` en la escena → no salta `NullReferenceException`.
- [ ] Panear con el botón derecho de un objeto a otro, y usar la rueda → la línea nunca cambia.
- [ ] Salir de Play y volver a entrar: arranca en "ninguno", contador a 0.
- [ ] (opcional) SPR_Crystal movido sobre Marker_Center: el clic en la zona solapada elige siempre el mismo, sin alternar.

## Fuera de alcance
- Contorno y `_EffectEnabled` — LK-01 · panel — LK-11 · sonido — LK-23 · contador — LK-30.
- `Select(EffectController)` por código, multiselección, hover, doble clic, táctil.
- Raycast 3D y `Physics.Raycast`: fuera del MVP 2D.
- Que el `EffectDebugTester` edite el seleccionado: sigue en su `EffectController` fijo del Inspector.
