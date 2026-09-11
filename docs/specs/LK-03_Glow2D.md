# LK-03 — Glow / Inner Glow Shader 2D
Estado: ⬜ · Depende de: LK-09 · Diseño: GDD §3.2 (línea 659), §1.3 (línea 52), §1.7 (línea 156)

> **Spec parcial.** Sólo el contrato de propiedades, sembrado en la Sesión 01 para fijar
> D-005. El resto (nodos del grafo, criterios completos) se redacta al abrir la Fase 5.

## Objetivo
Brillo interior y exterior con intensidad y pulso animado. Objeto demo: Runa flotante.

## Propiedades del grafo `SG_Glow2D`
El `Reference` debe coincidir exactamente con el `propertyName` del `EffectParameter`.
Sin Keywords: Boolean y Enum van como Float (ver `.claude/rules/shaders.md`).

| Reference | Tipo SG | ParameterType | Rango / valores | Defecto | Origen |
|---|---|---|---|---|---|
| `_EffectEnabled` | Float | — (no expuesto en UI) | 0 / 1 | 1 | **D-005** |
| `_GlowColor` | Color (Mode = HDR) | Color | — | por definir | GDD línea 51 |
| `_GlowIntensity` | Float | Float | 0.0 – 5.0 | por definir | GDD línea 659 |
| `_PulseEnabled` | **Float 0/1** | Boolean | 0 / 1 | 0 | GDD línea 52 |
| `_PulseSpeed` | Float | Float | 0.0 – 5.0 | por definir | GDD línea 659 |

`_PulseEnabled` es el caso que motivó la prohibición de keywords: como Boolean Keyword el
toggle de la UI no haría nada y el fallo sería silencioso. Va como Float 0/1.
`_EffectEnabled` no genera widget: lo conduce LK-24 (TAB), no el panel de parámetros.
Usa el subgrafo `SUB_FresnelGlow` (GDD línea 828).

## Criterios de aceptación (verificables en el editor)
- [ ] El grafo expone las propiedades con el `Reference` exacto de la tabla.
- [ ] `_PulseEnabled` es una propiedad Float, **no** un Boolean Keyword.
- [ ] El toggle de `_PulseEnabled` en la UI arranca y detiene el pulso en Play Mode.
- [ ] El grafo termina en `Lerp(base, conEfecto, _EffectEnabled)`.
- [ ] Resto de criterios: por redactar en la Fase 5.

## Fuera de alcance
- Post-proceso Bloom del volumen URP — no forma parte de este shader.
- Presets — LK-33.
