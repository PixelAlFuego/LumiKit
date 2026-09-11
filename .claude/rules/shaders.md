---
paths: ["Assets/LumiKit/Shaders/**","Assets/LumiKit/Materials/**"]
---

# Shaders y materiales

## Regla dura — D-001
Los parámetros se aplican con `MaterialPropertyBlock`. Nunca se modifica un material.
Prohibido: `renderer.material`, `renderer.materials`, `material.SetFloat` y equivalentes.
Permitido: `renderer.sharedMaterial` sólo en lectura.
Motivo: modificar un `.mat` en runtime dentro del editor lo sobrescribe en disco.

## Nombres
`SG_` Shader Graph · `SUB_` Sub Graph · `MAT_` Material. Detalle en docs/CONVENTIONS.md.
Ubicación: `Shaders/2D/`, `Shaders/SubGraphs/`, `Materials/2D/`.

## Formato
`.shadergraph` y `.shadersubgraph` son JSON con GUIDs: **no se escriben a mano**.
Se construyen en la ventana de Shader Graph. Lo que Claude entrega es la especificación
del grafo: nodos, conexiones y propiedades expuestas, en tabla.

## Propiedades expuestas
Toda propiedad que la UI vaya a mover necesita su fila en el `EffectDefinition` (LK-09).
El `Reference` de la propiedad en Shader Graph debe coincidir exactamente con el
`propertyName` del `EffectParameter`. Un typo aquí falla en silencio.

Referencia por defecto: `_NombreEnPascalCase`.

## Alcance actual
Sólo 2D: LK-01 Outline, LK-03 Glow, LK-02 Dissolve. No trabajar shaders 3D.

## Al terminar
Registrar en CODEMAP.md > Shaders con sus propiedades expuestas. Estado 🟡.
