# LK-09 — Arquitectura de EffectDefinition
Estado: ⬜ · Depende de: — · Diseño: GDD §4.3 (líneas 1001-1016), §4.4 (líneas 1065-1081), §1.3 Mecánica 2 (líneas 44-56)

## Objetivo
Definir en datos qué parámetros expone un efecto y aplicarlos a un `Renderer` vía
`MaterialPropertyBlock`, sin que ningún código de UI conozca un efecto concreto.

## Archivos
| Ruta | Acción |
|---|---|
| `Assets/LumiKit/Runtime/Scripts/Core/ParameterType.cs` | crear |
| `Assets/LumiKit/Runtime/Scripts/Core/EffectParameter.cs` | crear |
| `Assets/LumiKit/Runtime/Scripts/Core/EffectDefinition.cs` | crear |
| `Assets/LumiKit/Runtime/Scripts/Core/EffectRegistry.cs` | crear |
| `Assets/LumiKit/Runtime/Scripts/Core/EffectController.cs` | crear |
| `Assets/LumiKit/Runtime/Scripts/Utils/MaterialPropertyHelper.cs` | crear |
| `Assets/LumiKit/Runtime/Scripts/Utils/Singleton.cs` | crear |
| `docs/CODEMAP.md`, `docs/BACKLOG.md`, `docs/STATE.md` | modificar |

Namespace: `LumiKit.Core` y `LumiKit.Utils`. Ninguno referencia `UI` ni `Demo`.

## Contrato

`ParameterType` — enum. Orden exacto: `Float`, `Color`, `Boolean`, `Enum`. No se altera
tras crear el primer `.asset`: cambiaría el valor serializado.

`EffectParameter` — `[System.Serializable]`, clase, no MonoBehaviour.

| Campo | Tipo | Notas |
|---|---|---|
| displayNameEs / displayNameEn | string | etiqueta del widget |
| propertyName | string | debe coincidir con el `Reference` del Shader Graph |
| type | ParameterType | determina el widget |
| minValue / maxValue | float | sólo `Float` |
| defaultFloat / defaultColor / defaultBool / defaultEnumIndex | float / Color / bool / int | valor de reset |
| enumOptions | string[] | sólo `Enum` |

Expone el ID de propiedad cacheado (`Shader.PropertyToID`), calculado una vez, no por frame.

`EffectDefinition` — ScriptableObject con `[CreateAssetMenu]` bajo `LumiKit/`. Campos:
nombre y descripción bilingües, `Shader` asociado, `EffectParameter[]`. Sin setters.

`EffectRegistry` — ScriptableObject. Lista de `EffectDefinition`, consulta por índice y
por nombre. Lo consume el contador de exploración (LK-30).

`EffectController` — MonoBehaviour. `[SerializeField] private Renderer _targetRenderer;`
y `[SerializeField] private EffectDefinition _definition;`.

| Miembro | Forma | Responsabilidad |
|---|---|---|
| Definition | propiedad pública de sólo lectura | expone la definición activa |
| SetFloat / SetColor / SetBool / SetEnum | público, `(string propertyName, valor)` | escribe en el block y lo aplica |
| GetCurrentValue | público | valor actual para que el widget se inicialice |
| ResetToDefaults | público | Mecánica 5 (GDD línea 67) |
| SetEffectEnabled | público, `(bool)` | Mecánica 3, comparación TAB (GDD línea 59) |
| OnValueChanged | evento | notifica a la UI sin conocerla |

`MaterialPropertyHelper` (`LumiKit.Utils`) — estática. Centraliza la escritura en el
`MaterialPropertyBlock` y la conversión bool→float. Único punto que toca el block.

`Singleton<T>` (`LumiKit.Utils`) — MonoBehaviour genérica base. `DontDestroyOnLoad` no
automático: lo decide la subclase.

## Criterios de aceptación (verificables en el editor)
- [ ] Compila sin errores ni warnings nuevos en la consola.
- [ ] `Create → LumiKit → Effect Definition` genera un `.asset` y el Inspector muestra
      la lista de parámetros (min/max sólo relevantes en `Float`).
- [ ] `EffectController` sobre un objeto con Renderer: mover un valor por código en Play
      Mode cambia el aspecto del objeto.
- [ ] Al salir de Play Mode, el `.mat` usado **no** aparece modificado en disco (D-001).
- [ ] `ResetToDefaults()` devuelve todos los valores a los del `EffectDefinition`.
- [ ] `SetEffectEnabled(false)` muestra el objeto sin efecto; `true` lo restaura.
- [ ] Buscar `renderer.material` en `Assets/LumiKit/` no devuelve resultados.

## Fuera de alcance
- Widgets, panel y cualquier archivo bajo `Scripts/UI/` — eso es LK-11.
- Raycast y selección de objetos — LK-10.
- Crear los `.asset` `EFF_*` concretos y los shaders — LK-01, LK-02, LK-03.
- Localización real: por ahora dos strings en el SO, sin `LocalizationManager` (LK-19).
- Serialización a JSON o portapapeles — LK-29, LK-46.
