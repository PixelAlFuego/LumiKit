# LK-09 — Arquitectura de EffectDefinition
Estado: ✅ verificado por el usuario 2026-09-11 · Depende de: — · Diseño: GDD §4.3 (líneas 1001-1016), §4.4 (líneas 1065-1081), §1.3 Mecánica 2 (líneas 44-56)

## Objetivo
Definir en datos qué parámetros expone un efecto y aplicarlos a un `Renderer` vía
`MaterialPropertyBlock`, sin que ningún código de UI conozca un efecto concreto.

## Archivos
7 archivos bajo `Assets/LumiKit/Runtime/Scripts/`, listados en `docs/CODEMAP.md` > Runtime.
Namespaces `LumiKit.Core` y `LumiKit.Utils`. Ninguno referencia `UI` ni `Demo`.

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
| GetFloat / GetColor / GetBool / GetEnum | público, `(string propertyName)` | valor actual para que el widget se inicialice |
| ResetToDefaults | público | Mecánica 5 (GDD línea 67) |
| SetEffectEnabled | público, `(bool)` | escribe `_EffectEnabled` (D-005). Mecánica 3, TAB |
| OnValueChanged | `event Action<string>` | notifica a la UI sin conocerla |

Cuatro getters tipados en vez de un `GetCurrentValue`: sin boxing y sin tipo nuevo. El
estado vivo se guarda en un struct privado dentro de `EffectController`.

`MaterialPropertyHelper` (`LumiKit.Utils`) — estática. Único punto que toca el block:
centraliza escritura, conversión bool→float y `ConvertColor`. Declara
`EFFECT_ENABLED_PROPERTY` (D-005): el nombre vive sólo aquí.

`Singleton<T>` (`LumiKit.Utils`) — MonoBehaviour genérica base. `DontDestroyOnLoad` no
automático: lo decide la subclase.

## Criterios de aceptación (verificables en el editor)
Banco de pruebas: no hay shader del pack todavía. Material URP de stock
(`Universal Render Pipeline/Unlit`) + `_BaseColor`, con un `EFF_Debug.asset` desechable
en `Assets/_Development/`, que no se exporta.

- [x] Compila sin errores ni warnings nuevos en la consola.
- [x] `Create → LumiKit → Effect Definition` genera un `.asset` y el Inspector muestra
      la lista de parámetros (min/max sólo relevantes en `Float`).
- [x] `EffectController` sobre un objeto con Renderer: mover `_BaseColor` por código en
      Play Mode cambia el aspecto del objeto.
- [x] Al salir de Play Mode, el `.mat` usado **no** aparece modificado en disco (D-001).
- [x] `ResetToDefaults()` devuelve todos los valores a los del `EffectDefinition`.
- [x] Buscar `renderer.material` en `Assets/LumiKit/` no devuelve resultados.

Dos criterios quedan **diferidos a `docs/specs/LK-01_Outline2D.md`**, no cumplidos:
`SetEffectEnabled` (ningún shader declara `_EffectEnabled` aún) y el color en espacio
Linear (verificación visual).

## Fuera de alcance
- Widgets, panel y cualquier archivo bajo `Scripts/UI/` — LK-11. Raycast — LK-10.
- Los `.asset` `EFF_*` del pack y los shaders — LK-01, LK-02, LK-03.
- Localización real: por ahora dos strings en el SO, sin `LocalizationManager` (LK-19).
- Serialización a JSON o portapapeles — LK-29, LK-46.
