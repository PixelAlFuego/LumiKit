# Decisiones estructurales
Una entrada por decisión que condiciona código futuro. No se borran: si una decisión
se revierte, se añade una entrada nueva que la anula y se marca la anterior `Anulada por D-XXX`.

Formato: ID · fecha · decisión · motivo · irreversible · alcance.

---

## D-001 — Los parámetros se aplican con `MaterialPropertyBlock`
Fecha: 2026-09-10 · Sesión 00 · Irreversible: **sí**

**Decisión.** `EffectController` escribe los valores en un `MaterialPropertyBlock` y lo
aplica al `Renderer`. Nunca se asigna ni modifica un `Material` en runtime.

**Motivo.** Modificar un `.mat` en runtime dentro del editor lo sobrescribe en disco de
forma permanente. El daño no se revierte con Undo ni al salir de Play Mode.

**Alcance.** Todo `Core`, todo widget de UI que escriba un parámetro, todo shader del pack.
Prohibido: `renderer.material`, `renderer.materials`, `material.SetFloat` y equivalentes.
Permitido: `renderer.sharedMaterial` sólo en lectura.

**Referencia.** GDD §4.3 (líneas 1009-1016).

---

## D-002 — Escenas y prefabs se generan con scripts de editor
Fecha: 2026-09-10 · Sesión 00 · Irreversible: **no**

**Decisión.** Escenas y prefabs se construyen desde scripts en `Assets/Editor/`,
no escribiendo YAML a mano ni editando el `.unity` / `.prefab` con un editor de texto.

**Motivo.** El YAML de Unity depende de GUIDs. Un error de referencia no falla al
guardar: corrompe la escena de forma silenciosa y se descubre horas después.

**Alcance.** LK-13, LK-14, LK-17, LK-22 y cualquier tarea que produzca `.unity`,
`.prefab`, `.asset` o `.meta`. El script generador queda versionado; el artefacto
generado se puede regenerar.

**Consecuencia.** Es reversible porque basta con volver a ejecutar el generador.
Por eso el generador, no la escena, es la fuente de verdad.

---

## D-003 — `EffectDefinition` es un ScriptableObject y la UI se genera desde él
Fecha: 2026-09-10 · Sesión 00 · Irreversible: **sí**

**Decisión.** Cada efecto se describe en un `EffectDefinition` (ScriptableObject) con su
lista de `EffectParameter`. `ParameterPanelUI` lee esa lista e instancia los widgets.

**Motivo.** Añadir un efecto nuevo no debe requerir tocar código de interfaz. Sin esto,
cada shader nuevo obliga a editar el panel, y el panel se convierte en un `switch` que
crece con cada efecto.

**Alcance.** LK-09 define el contrato. LK-11 lo consume. Ningún widget puede conocer un
efecto concreto: se configura sólo desde el `EffectParameter` que recibe.

**Consecuencia.** Cambiar la forma de `EffectParameter` después de crear los `.asset`
rompe la serialización de todos ellos. El contrato se cierra en LK-09.

**Referencia.** GDD §4.3 (líneas 1001-1008) y §4.4 (líneas 1065-1081).

---

## D-004 — El GDD se renombró a `docs/reference/GDD_v2.md`
Fecha: 2026-09-10 · Sesión 00 · Irreversible: **no**

**Decisión.** `LumiKit_Avance2_GDDv2_Arquitectura.md` pasó a llamarse `GDD_v2.md`.

**Motivo.** Toda ruta escrita en un doc debe existir en disco. El nombre corto es el que
referencian CLAUDE.md y el resto de la documentación.

**Alcance.** Sólo el nombre del archivo. El contenido no se tocó (1274 líneas).
