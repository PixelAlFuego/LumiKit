---
paths: ["docs/**","CLAUDE.md"]
---

# Estilo de documentación

1. Sólo `CLAUDE.md` se carga siempre. Es un enrutador, no un manual.
2. El único import `@` permitido es `docs/STATE.md`. Cada import extra es coste permanente.
3. Todo lo demás se lee bajo demanda. Reglas contextuales en `.claude/rules/` con `paths:`.
4. Cero duplicación. Si el dato ya está en el GDD, se referencia por sección y rango de
   líneas; no se copia.
5. Tablas y listas, nunca prosa larga. Sin adjetivos, sin justificaciones.
6. Toda ruta escrita en un doc debe existir en disco. Si es futura: `(planeado)`.
7. Al superar el tope se condensa o se archiva; no se deja crecer.
8. Los topes no se suben. Al llegar al tope se condensan entradas antiguas, sin borrar
   ninguna. Excepción histórica y cerrada: Sesión 02 (docs-style 30→40, DECISIONS 100→150).

## Topes (líneas)
CLAUDE.md 120 · STATE 100 · BACKLOG 80 · CODEMAP 120 · CONVENTIONS 80 · DECISIONS 150 ·
VERIFICATION 60 · specs/LK-XX 80 · rules: unity-assets 40, ui-style 60, shaders 40, docs-style 40.

## Plantilla de spec — `docs/specs/LK-XX_Nombre.md`
Cabecera `# LK-XX — Nombre` y línea `Estado · Depende de · Diseño (GDD §, líneas)`.
Secciones en orden: Objetivo · Archivos · Contrato · Banco de pruebas · Criterios de
aceptación · Fuera de alcance. En shaders, Contrato = tabla de propiedades del grafo.
**Banco de pruebas** — obligatoria desde LK-12: escena, objetos y componentes con los que
se verifica, y qué criterios no se pueden verificar aún y a qué `LK-XX` quedan diferidos.

## Archivado
STATE.md guarda 3 sesiones; la cuarta se resume en una línea en
`docs/archive/sesiones_AAAA-QN.md`. En DECISIONS.md sólo se archivan las anuladas,
en `docs/archive/decisiones_AAAA.md`; las vigentes nunca.

## Nunca
- Escribir "funciona", "probado" o "verificado" en un doc.
- Marcar ✅ sin autorización explícita del usuario.
- Añadir a CODEMAP.md un archivo que no exista en disco.
