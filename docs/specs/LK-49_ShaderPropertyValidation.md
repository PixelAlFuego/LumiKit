# LK-49 — Validar `propertyName` contra el shader del material
Estado: ✅ verificado por el usuario 2026-09-18 · Depende de: LK-09 (`EffectController`, `MaterialPropertyHelper`) · Origen: verificación de LK-11b (Sesión 05); no viene del GDD · Decisiones: D-001 (MPB), D-005 (`_EffectEnabled`)

## Objetivo
Al inicializarse, un `EffectController` comprueba qué `propertyName` de su `EffectDefinition` no
declara el material del `Renderer` y lo avisa **una sola vez** por consola, nombrando objeto,
efecto, material y shader.
Motivo: un `MaterialPropertyBlock` que escribe una propiedad inexistente no falla ni avisa, la
descarta. Así se descubrió en LK-11b que el widget de `_BaseColor` movía el valor sin pintar los
sprites, porque `Sprite-Unlit-Default` declara `_Color` y no `_BaseColor`. Es el mismo fallo
silencioso que aparecerá en la Fase 5 cuando el campo Reference de un Shader Graph no coincida con
el `propertyName` del asset, y allí costará mucho más verlo.

## Archivos
- `Assets/LumiKit/Runtime/Scripts/Utils/MaterialPropertyHelper.cs` (existe · ✅ LK-09) · **aditiva**: `HasProperty(Material, string)`, ~12 líneas. No se reescribe nada de lo que ya hay.
- `Assets/LumiKit/Runtime/Scripts/Core/EffectController.cs` (existe · ✅ LK-09) · **una línea insertada** al final de `EnsureInitialized()` más un método privado nuevo `ValidateProperties()`, ~55 líneas con su comentario. Ninguna línea existente cambia de contenido.
- No se tocan: `UI/`, `Demo/`, los prefabs, `EffectDefinition`, `EffectParameter`, el `EffectDebugTester`.

## Contrato

| Miembro | Firma | Comportamiento |
|---|---|---|
| `HasProperty` | `public static bool HasProperty(Material material, string propertyName)` | Envuelve `Material.HasProperty`. Null-safe: material nulo o nombre vacío → `false` |
| `ValidateProperties` | `private void ValidateProperties()` | Junta los `propertyName` que el material no declara y emite un único warning si hay alguno |

1. Se llama al final de `EnsureInitialized()`, que ya está protegido por `_initialized`: una vez por
   componente y por sesión de Play. No hay `Update`, ni comprobación por frame, ni por escritura.
2. Material: `_targetRenderer.sharedMaterial`, **sólo lectura**, que es lo que D-001 permite
   expresamente. No se usa `sharedMaterials`: D-001 prohíbe `renderer.materials` y la variante en
   plural no está autorizada por escrito. Consecuencia asumida: en un `Renderer` con varios
   materiales sólo se valida el primero, **y el aviso lo dice**: "Validado 1 de N materiales".
   El recuento sale de `GetSharedMaterials(List<Material>)` sobre una lista reutilizada, que no
   instancia nada y sólo se usa para contar.
3. Sin `Renderer` o sin material asignado: un warning distinto —no hay material contra el que
   validar— y no se evalúa ninguna propiedad. Sin `EffectDefinition` no hace nada: de eso ya avisa
   `EnsureInitialized`.
4. El texto nombra objeto, efecto, material, shader y la lista de propiedades ausentes, y dice que
   esas escrituras se descartan en silencio. Un solo `Debug.LogWarning`, con el objeto como
   contexto para que al pulsarlo se seleccione en la jerarquía.
5. `_EffectEnabled` (D-005) **no** entra: hoy no existe en ningún shader del pack y saltaría en
   todos los objetos, tapando lo que sí importa. Su comprobación es criterio de cada spec de shader.
6. Sólo existencia, no tipo. Un parámetro `Float` apuntando a una propiedad de color pasa la
   validación: eso es otra comprobación y otra tarea.

## Banco de pruebas
`Assets/_Development/TestBench.unity` con `EFF_Debug.asset` y sus seis parámetros: `_OutlineWidth`
(Float), `_Glow` (Float), `_BaseColor` (Color), `_Pulse` (Boolean), `_OutlineMode` (Enum) y `_Color`
(Color). Nada que montar: el usuario lo dejó listo en la Sesión 05.

| Objeto | Shader del material | Debe avisar de | No debe avisar de |
|---|---|---|---|
| `Marker_Center` · `Marker_NE` · `NW` · `SE` · `SPR_Crystal` · `SPR_RuneCoin` | `Universal Render Pipeline/2D/Sprite-Unlit-Default` (declara `_MainTex` y `_Color`) | `_BaseColor`, `_Glow`, `_OutlineWidth`, `_Pulse`, `_OutlineMode` | `_Color` |
| Cubo de LK-09 (`MAT_Debug.mat`) | URP `Lit`/`Unlit`: declaran `_BaseColor` y además `_Color`, en su bloque `ObsoleteProperties` | `_Glow`, `_OutlineWidth`, `_Pulse`, `_OutlineMode` | `_BaseColor` y `_Color` |
| `Marker_SW` | capa `Default`, sin `EffectController` | nada: no hay componente que valide | — |

`_BaseColor` se queda en `EFF_Debug` como control negativo permanente (decisión del usuario): es lo
que demuestra que la validación detecta el caso real que se escapó en LK-11b.

## Criterios de aceptación (verificables en el editor)
- [x] Compila sin errores ni warnings nuevos.
- [x] Al entrar en Play: **un** aviso por objeto con `EffectController`, no uno por parámetro. Siete avisos, uno por controller.
- [x] `Marker_Center` y los sprites listan exactamente `_BaseColor`, `_Glow`, `_OutlineWidth`, `_Pulse` y `_OutlineMode`; `_Color` no aparece. `Marker_SW`, sin controller, no avisa.
- [x] El cubo de LK-09 lista cuatro: las mismas menos `_BaseColor` y `_Color` (verificado; la previsión de esta spec decía cinco, sin contar con las propiedades obsoletas de URP).
- [ ] Cada aviso nombra objeto, efecto, material y shader, y al pulsarlo selecciona el objeto.
- [ ] Mover sliders y cambiar de selección durante un minuto: la consola no crece.
- [ ] Seleccionar un marcador: el widget de `_Color` sigue pintando el sprite y el de `_BaseColor` sigue sin pintar, ahora con el aviso que lo explica.
- [ ] Un `EffectController` en un objeto sin `Renderer`: avisa de que no hay material, sin excepción.
- [ ] Un objeto con dos materiales en el `Renderer`: el aviso termina en "Validado 1 de 2 materiales"; con uno solo, esa frase no aparece.
- [ ] Salir de Play: `git status` no muestra cambios en `MAT_Debug.mat` ni en los materiales de los sprites (D-001).

## Fuera de alcance
- Comprobar el **tipo** de la propiedad, no sólo el nombre (`Material.HasFloat` / `HasColor`): si hace falta, entra con el primer shader real, en LK-01.
- Validar `_EffectEnabled`: criterio de cada shader (D-005).
- Validar sin entrar en Play, con `OnValidate` o una ventana de auditoría del pack → LK-44.
- Varios materiales por `Renderer`, materiales instanciados y cualquier uso de `renderer.materials`.
- Corregir nada automáticamente: LK-49 sólo avisa. Tampoco toca la UI ni desactiva widgets.
