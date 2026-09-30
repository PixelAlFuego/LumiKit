---
paths: ["Assets/LumiKit/Shaders/**","Assets/LumiKit/Materials/**"]
---

# Shaders y materiales

## Prohibido — keywords (límite de Unity, no preferencia)
Un `MaterialPropertyBlock` **no puede activar keywords de shader**. Los keywords son
variantes de compilación y sólo se activan con `Material.EnableKeyword()`, que modifica
el material → prohibido por D-001.

| En el `EffectDefinition` | En HLSL (MVP) | En Shader Graph | Nunca |
|---|---|---|---|
| `ParameterType.Boolean` | **Float** 0/1 con `[ToggleUI]` | propiedad **Float** 0/1 | ❌ `[Toggle]`, Boolean Keyword |
| `ParameterType.Enum` | **Float** índice (`[Enum(...)]`) + comparación en código | propiedad **Float** como índice + `Comparison`/`Branch` | ❌ `[KeywordEnum]`, `multi_compile`/`shader_feature`, Enum Keyword |

Un keyword expuesto compila sin error y el widget de UI no hace nada. Fallo silencioso.

## Obligatorio — `_EffectEnabled` (D-005)
Todo shader del pack, HLSL o grafo, expone `Float _EffectEnabled` y termina en
`lerp(base, conEfecto, _EffectEnabled)` (nodo `Lerp` en grafo). Sin ella, el TAB no funciona.

## D-001 — nunca se modifica el material
Prohibido: `renderer.material`, `renderer.materials`, `material.SetFloat`. Permitido:
`renderer.sharedMaterial` sólo en lectura. Todo pasa por `MaterialPropertyHelper`.

## Propiedades
El nombre de la propiedad (`Properties` en HLSL, `Reference` en grafo) debe coincidir
**exactamente** con el `propertyName` del `EffectParameter`. Un typo falla en silencio.
`Color` en HDR: `[HDR]` en HLSL, `Mode = HDR` en grafo. Nombre por defecto: `_NombreEnPascalCase`.

## Formato y nombres
**MVP (D-010):** Claude escribe los shaders 2D en HLSL, `.shader` en `Assets/LumiKit/Shaders/2D/`. Mismos
pases que `Sprite-Unlit-Default` de URP: `Universal2D` y `UniversalForward` (el proyecto usa Universal Renderer).
Shader Graph (aparcado, Asset Store): `.shadergraph` y `.shadersubgraph` son JSON con GUIDs, **no se escriben
a mano**; los construye el usuario con la especificación de Claude (nodos, conexiones, propiedades) en tabla.
`SG_` grafo · `SUB_` subgrafo · `MAT_` material · `.shader`: prefijo pendiente (LK-01). Detalle en docs/CONVENTIONS.md.

## Alcance actual
Sólo 2D: LK-01 Outline, LK-03 Glow, LK-02 Dissolve. Registrar en CODEMAP.md. Estado 🟡.
