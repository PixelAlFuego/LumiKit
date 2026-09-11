# Mapa de código
Una fila por archivo `.cs` o `.shadergraph` que exista en disco. Si no está aquí, no existe.
Se actualiza al cerrar cada tarea, antes del commit.

Reglas de esta tabla:
- No se añade una fila hasta que el archivo esté escrito en disco.
- "Depende de" lista sólo dependencias directas dentro de LumiKit.
- Capa: `Core` · `Demo` · `UI` · `Systems` · `Utils` · `Editor` · `Shader`.
- Dirección permitida: UI → Demo → Core. `Core` no referencia `UI` ni `Demo`.

## Runtime

| Archivo | Capa | Tipo | Responsabilidad | Depende de | LK | Estado |
|---|---|---|---|---|---|---|
| _(vacío)_ | | | | | | |

## Editor

| Archivo | Tipo | Responsabilidad | Depende de | LK | Estado |
|---|---|---|---|---|---|
| _(vacío)_ | | | | | |

## Shaders y materiales

| Archivo | Tipo | Propiedades expuestas | Material(es) | LK | Estado |
|---|---|---|---|---|---|
| _(vacío)_ | | | | | |

## Assembly definitions

| Archivo | Ensamblado | Plataformas |
|---|---|---|
| `Assets/LumiKit/Runtime/Scripts/LumiKit.Runtime.asmdef` | LumiKit.Runtime | todas |
| `Assets/Editor/LumiKit.Editor.asmdef` | LumiKit.Editor | Editor |

## ScriptableObjects instanciados

| Asset | Tipo | LK | Estado |
|---|---|---|---|
| _(vacío)_ | | | |

## Escenas

| Escena | Construida por | Contenido | LK | Estado |
|---|---|---|---|---|
| `Assets/LumiKit/Scenes/00_Splash.unity` | — | preexistente, sin auditar | — | ⬜ |
| `Assets/LumiKit/Scenes/01_MainMenu.unity` | — | preexistente, sin auditar | LK-13 | ⬜ |
| `Assets/LumiKit/Scenes/02_Demo_2D.unity` | — | preexistente, sin auditar | LK-14 | ⬜ |
| `Assets/LumiKit/Scenes/03_Demo_3D.unity` | — | preexistente, sin auditar | LK-15 | ⬜ |
| `Assets/LumiKit/Scenes/04_Demo_VFX.unity` | — | preexistente, sin auditar | LK-16 | ⬜ |

## Prefabs

| Prefab | Generado por | Componentes | LK | Estado |
|---|---|---|---|---|
| _(vacío)_ | | | | |
