---
paths: ["Assets/LumiKit/Shaders/**","Assets/LumiKit/Materials/**"]
---

# Shaders y materiales

## Prohibido — keywords (límite de Unity, no preferencia)
Un `MaterialPropertyBlock` **no puede activar keywords de shader**. Los keywords son
variantes de compilación y sólo se activan con `Material.EnableKeyword()`, que modifica
el material → prohibido por D-001.

| En el `EffectDefinition` | En Shader Graph | Nunca |
|---|---|---|
| `ParameterType.Boolean` | propiedad **Float** 0/1 | ❌ Boolean Keyword |
| `ParameterType.Enum` | propiedad **Float** como índice + `Comparison`/`Branch` | ❌ Enum Keyword |

Un keyword expuesto compila sin error y el widget de UI no hace nada. Fallo silencioso.

## Obligatorio — `_EffectEnabled` (D-005)
Todo grafo del pack expone `Float _EffectEnabled` y termina en
`Lerp(base, conEfecto, _EffectEnabled)`. Sin ella, la comparación con TAB no funciona.

## D-001 — nunca se modifica el material
Prohibido: `renderer.material`, `renderer.materials`, `material.SetFloat`. Permitido:
`renderer.sharedMaterial` sólo en lectura. Todo pasa por `MaterialPropertyHelper`.

## Propiedades
El `Reference` de la propiedad en Shader Graph debe coincidir **exactamente** con el
`propertyName` del `EffectParameter`. Un typo falla en silencio.
Las propiedades `Color` van en `Mode = HDR`.
Referencia por defecto: `_NombreEnPascalCase`.

## Formato y nombres
`.shadergraph` y `.shadersubgraph` son JSON con GUIDs: **no se escriben a mano**.
Los construye el usuario en la ventana de Shader Graph; Claude entrega la
especificación (nodos, conexiones, propiedades) en tabla.
`SG_` grafo · `SUB_` subgrafo · `MAT_` material. Detalle en docs/CONVENTIONS.md.

## Alcance actual
Sólo 2D: LK-01 Outline, LK-03 Glow, LK-02 Dissolve. Registrar en CODEMAP.md. Estado 🟡.
