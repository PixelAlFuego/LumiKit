# LK-01 — Outline Shader 2D
Estado: ⬜ · Depende de: LK-09 · Diseño: GDD §3.2 (línea 657), §1.3 (líneas 48-53), §1.7 (línea 154)

> **Spec parcial.** Sólo el contrato de propiedades, sembrado en la Sesión 01 para fijar
> D-005. El resto (nodos del grafo, criterios completos) se redacta al abrir la Fase 5.

## Objetivo
Contorno configurable sobre sprite, con color, grosor y modo. Objeto demo: Lumi.

## Propiedades del grafo `SG_Outline2D`
El `Reference` debe coincidir exactamente con el `propertyName` del `EffectParameter`.
Sin Keywords: Boolean y Enum van como Float (ver `.claude/rules/shaders.md`).

| Reference | Tipo SG | ParameterType | Rango / valores | Defecto | Origen |
|---|---|---|---|---|---|
| `_EffectEnabled` | Float | — (no expuesto en UI) | 0 / 1 | 1 | **D-005** |
| `_OutlineColor` | Color (Mode = HDR) | Color | — | por definir | GDD línea 657 |
| `_OutlineWidth` | Float | Float | 0.0 – 10.0 | por definir | GDD línea 50 |
| `_OutlineMode` | Float (índice) | Enum | 0 sólido · 1 punteado · 2 animado | 0 | GDD línea 53 |

`_EffectEnabled` no genera widget: lo conduce LK-24 (TAB), no el panel de parámetros.

## Criterios de aceptación (verificables en el editor)
- [ ] El grafo expone las 4 propiedades con el `Reference` exacto de la tabla.
- [ ] Ninguna propiedad está declarada como Keyword.
- [ ] El grafo termina en `Lerp(base, conEfecto, _EffectEnabled)`.
- [ ] **Diferido desde LK-09:** `EffectController.SetEffectEnabled(false)` muestra el
      sprite sin contorno; `true` lo restaura. Es la primera vez que se puede probar,
      porque en LK-09 todavía no existía ningún shader con `_EffectEnabled`.
- [ ] **Diferido desde LK-09:** verificación visual del color en espacio Linear. Si
      `_OutlineColor` se ve lavado respecto al valor elegido en la UI, la corrección va
      en `MaterialPropertyHelper.ConvertColor` y en ningún otro sitio.
- [ ] Resto de criterios: por redactar en la Fase 5.

## Fuera de alcance
- `EFF_Outline2D.asset` y `MAT_Outline2D_Default.mat` — se crean con esta tarea, pero
  su configuración final depende del sprite de LK-20.
- Presets `PRE_Outline_*` — LK-33.
