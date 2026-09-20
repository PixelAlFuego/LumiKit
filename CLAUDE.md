# LumiKit — Instrucciones de proyecto

@docs/STATE.md

## Qué es esto
Pack comercial de shaders y VFX para Unity 6 LTS + URP 17. Todo lo distribuible vive
bajo `Assets/LumiKit/`. Nada fuera de esa carpeta se exporta.

## Protocolo de sesión
1. Al iniciar ya tienes STATE.md. Resume en 3 líneas: última tarea cerrada, tarea
   activa, siguiente paso. Espera mi confirmación antes de tocar un solo archivo.
2. Antes de trabajar LK-XX: lee `docs/specs/LK-XX_*.md` y `docs/CODEMAP.md`. Nada más.
3. Al cerrar: actualiza STATE.md, BACKLOG.md y CODEMAP.md; DECISIONS.md sólo si hubo
   decisión estructural. Commit `[LK-XX] descripción`.
4. `docs/reference/GDD_v2.md` NUNCA se lee completo. `grep -n` y rango de líneas.

## Mapa de documentación
| Archivo | Léelo cuando |
|---|---|
| docs/BACKLOG.md | Vas a elegir o cerrar una tarea |
| docs/specs/LK-XX_*.md | Vas a implementar esa tarea |
| docs/CODEMAP.md | Vas a tocar código existente |
| docs/CONVENTIONS.md | Vas a crear archivos o assets nuevos |
| docs/DECISIONS.md | Dudas por qué algo está hecho así |
| docs/VERIFICATION.md | Vas a cerrar una tarea |
| docs/reference/GDD_v2.md | Necesitas un dato de diseño puntual (sólo por grep) |
| docs/reference/UI_ART_BRIEF.md | Vas a tocar sprites de UI o el aspecto del generador |

## Integridad del código — no negociable
- Prohibido borrar archivos o bloques de código. Si crees que algo sobra, anótalo en
  STATE.md > Dudas abiertas y pregúntame.
- Edición quirúrgica siempre. Nunca reescribas un archivo completo para cambiar una parte.
- Si una tarea exige modificar más de 30 líneas de código ya existente: detente,
  muéstrame el plan y espera aprobación.
- Prohibido tocar archivos fuera del alcance de la tarea actual, aunque veas errores.
  Repórtalos en STATE.md > Dudas abiertas.
- Prohibido refactorizar, renombrar o "limpiar" sin que yo lo pida explícitamente.
- Nunca edites a mano: `*.meta`, `*.unity`, `*.prefab`, `*.asset`, `ProjectSettings/`,
  `Packages/manifest.json`. Escenas y prefabs se construyen con scripts de editor.
- Git prohibido: `reset --hard`, `checkout -- .`, `clean -fd`, `push --force`, `rebase`.
  Permitido: `status`, `diff`, `log`, `add`, `commit`.

## Honestidad — anti-alucinación
- No puedes ejecutar Unity ni compilar. Nunca escribas "funciona", "probado" o
  "verificado" sobre nada.
- Estados: ⬜ pendiente · 🟠 en curso · 🟡 implementado sin verificar · ✅ verificado
  por el usuario · ⛔ bloqueado.
- Tu tope al terminar de programar es 🟡. Sólo yo autorizo ✅: cuando yo escriba
  "verificado LK-XX", actualiza los docs.
- Antes de escribir una ruta en un doc, confirma con `ls` que existe.
- Si no recuerdas si algo se hizo: `git log --oneline` y CODEMAP.md. Nunca supongas.
- Si no estás seguro de una API de Unity 6 o URP 17, dilo en vez de inventar la firma.
- Si te falta contexto, pregunta. Prohibido asumir requisitos.

## Convenciones rápidas (detalle en docs/CONVENTIONS.md)
- `namespace LumiKit.<Capa>` · campos privados `_camelCase` · `[SerializeField] private`
- Prefijos: SG_ SUB_ MAT_ TEX_ SPR_ MDL_ PRF_ VFX_ SFX_ EFF_ LOC_ PRE_ AMX_
- Escenas: `NN_Nombre.unity`
- Dependencias: UI → Demo → Core. Core NUNCA conoce la UI.

## Alcance actual
MVP 2D únicamente. No trabajes en shaders 3D ni VFX aunque el GDD los describa.
