# Convenciones
Fuente: GDD §4.5 (líneas 1085-1123). Aquí sólo lo aplicable; nada se duplica del GDD.

## Prefijos de assets

| Prefijo | Tipo | Ejemplo | Carpeta |
|---|---|---|---|
| `SG_` | Shader Graph | `SG_Outline2D.shadergraph` | `Assets/LumiKit/Shaders/2D/` |
| `SUB_` | Sub Graph | `SUB_NoiseSampler.shadersubgraph` | `Assets/LumiKit/Shaders/SubGraphs/` |
| `MAT_` | Material | `MAT_Toon3D_Default.mat` | `Assets/LumiKit/Materials/2D/` |
| `TEX_` | Textura | `TEX_NoisePerlin_512.png` | `Assets/LumiKit/Textures/Noise/` |
| `SPR_` | Sprite | `SPR_Lumi_512.png` | `Assets/LumiKit/Sprites/` |
| `MDL_` | Modelo 3D | `MDL_Bust_LowPoly.fbx` | `Assets/LumiKit/Models/` |
| `PRF_` | Prefab | `PRF_ParameterPanel.prefab` | `Assets/LumiKit/Prefabs/UI/` |
| `VFX_` | Prefab de partículas | `VFX_FireSmoke.prefab` | `Assets/LumiKit/VFX/Prefabs/` |
| `SFX_` | Efecto de sonido | `SFX_UI_Click.wav` | `Assets/LumiKit/Audio/SFX/` |
| `EFF_` | EffectDefinition (SO) | `EFF_Outline2D.asset` | `Assets/LumiKit/Runtime/Data/Effects/` |
| `LOC_` | Localización | `LOC_Spanish.asset` | `Assets/LumiKit/Runtime/Data/Localization/` |
| `PRE_` | Preset | `PRE_Outline_Neon.asset` | `Assets/LumiKit/Runtime/Data/Presets/` |
| `AMX_` | Audio Mixer | `AMX_LumiKit.mixer` | `Assets/LumiKit/Audio/Mixers/` |

## Código C#

| Elemento | Convención | Ejemplo |
|---|---|---|
| Clases, structs, enums | `PascalCase` | `EffectController` |
| Métodos (públicos y privados) | `PascalCase` | `ApplyParameter()` |
| Campos privados | `_camelCase` | `_currentEffect` |
| Campos serializados | `[SerializeField] private` + `_camelCase` | `[SerializeField] private Renderer _targetRenderer;` |
| Propiedades públicas | `PascalCase` | `public EffectDefinition Definition { get; }` |
| Constantes | `UPPER_SNAKE_CASE` | `MAX_PARAMETERS` |
| `public static readonly` | `PascalCase`, nombre del token que representa | `public static readonly Color LumiCyan` |
| Namespace | `LumiKit.<Capa>` | `namespace LumiKit.Core` |

Capas válidas para el namespace: `Core`, `Demo`, `UI`, `UI.Widgets`, `Systems`, `Utils`, `Editor`.

## Escenas
Prefijo numérico de dos dígitos que coincide con el índice en Build Settings: `NN_Nombre.unity`.
Motivo en GDD §4.5 (línea 1122): el orden alfabético debe igualar el orden de carga.

## Dependencias entre capas
Permitido: `UI` → `Demo` → `Core`. `Systems` notifica por eventos hacia abajo.
Prohibido: que `Core` referencie `UI`, `Demo` o `Systems`. Diagrama en GDD §4.4 (líneas 1021-1061).

## Ensamblados
| Ensamblado | Ubicación del `.asmdef` | Alcance |
|---|---|---|
| `LumiKit.Runtime` | `Assets/LumiKit/Runtime/Scripts/` | runtime + editor |
| `LumiKit.Editor` | `Assets/Editor/` | sólo editor |

Ningún script bajo `Assets/LumiKit/` puede hacer `using UnityEditor;`. Herramientas de
editor van en `Assets/Editor/`, que no se exporta.

## Commits
Formato: `[LK-XX] descripción en imperativo`. Ejemplos en GDD §4.9 (líneas 1247-1250).
Ramas: `main` estable · `develop` integración · `feature/LK-XX-descripcion`.

## Qué se exporta
Sólo `Assets/LumiKit/`. Quedan fuera: `Assets/Editor/`, `Assets/_Development/`,
`Assets/TutorialInfo/`, `Assets/Settings/`. Procedimiento en GDD §4.10 (líneas 1256+).
