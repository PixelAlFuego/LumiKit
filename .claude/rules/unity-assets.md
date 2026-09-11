---
paths: ["**/*.unity","**/*.prefab","**/*.asset","**/*.meta"]
---

# Assets serializados de Unity

Estos archivos son YAML con GUIDs. Un error no falla al guardar: corrompe referencias
en silencio.

## Prohibido
- Editarlos a mano, con Edit o con Write. Sin excepción.
- Crear un `.meta` a mano. Unity lo genera al recuperar el foco.
- Cambiar un GUID, un `fileID` o un bloque `m_Component`.
- Borrar o renombrar un `.meta` sin su archivo, o al revés.
- Mover estos archivos desde el shell. El movimiento se hace desde el editor de Unity.

## Permitido
- Leerlos para entender una referencia existente.
- Generarlos ejecutando un script de editor de `Assets/Editor/` (D-002).

## Cómo se crean
Escribe un script en `Assets/Editor/` que use la API del editor y lo produzca.
El script es la fuente de verdad y se versiona; el asset es su salida.

Si el usuario pide una escena o un prefab: entrega el generador, no el YAML.

## Al terminar
Estos archivos nunca pasan de 🟡. Sólo el usuario confirma que la escena abre bien.
Registrar el generador en CODEMAP.md > Editor, y el asset en la tabla que corresponda.
