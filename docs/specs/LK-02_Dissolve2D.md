# LK-02 — Dissolve Shader 2D
Estado: ⬜ · Depende de: LK-09, LK-01 · Diseño: GDD §3.2 (línea 658), §1.7 (línea 155)

> **Spec parcial.** Sólo el contrato de propiedades, sembrado en la Sesión 01 para fijar
> D-005. El resto (nodos del grafo, criterios completos) se redacta al abrir la Fase 5.

## Objetivo
Disolución progresiva con textura de ruido y borde emisivo. Objeto demo: Cristal 2D.

## Propiedades del grafo `SG_Dissolve2D`
El `Reference` debe coincidir exactamente con el `propertyName` del `EffectParameter`.
Sin Keywords: Boolean y Enum van como Float (ver `.claude/rules/shaders.md`).

| Reference | Tipo SG | ParameterType | Rango / valores | Defecto | Origen |
|---|---|---|---|---|---|
| `_EffectEnabled` | Float | — (no expuesto en UI) | 0 / 1 | 1 | **D-005** |
| `_DissolveAmount` | Float | Float | 0.0 – 1.0 | 0 | GDD línea 658 |
| `_NoiseTexture` | Texture2D | — (no expuesto en UI) | — | `TEX_NoisePerlin_512` | GDD línea 658 |
| `_EdgeColor` | Color (Mode = HDR) | Color | — | por definir | GDD línea 658 |
| `_EdgeWidth` | Float | Float | 0.0 – 0.2 | por definir | GDD línea 658 |

`_EffectEnabled` no genera widget: lo conduce LK-24 (TAB), no el panel de parámetros.
`_NoiseTexture` no se expone: las texturas no tienen widget en el MVP.
Usa el subgrafo `SUB_NoiseSampler` (GDD línea 828).

## Criterios de aceptación (verificables en el editor)
- [ ] El grafo expone las propiedades con el `Reference` exacto de la tabla.
- [ ] Ninguna propiedad está declarada como Keyword.
- [ ] El grafo termina en `Lerp(base, conEfecto, _EffectEnabled)`.
- [ ] Con `_EffectEnabled = 0` el sprite se ve íntegro aunque `_DissolveAmount` sea alto.
      Éste es el caso que descartó la opción "escribir valores neutros" en D-005.
- [ ] Resto de criterios: por redactar en la Fase 5.

## Fuera de alcance
- Generador procedural de la textura de ruido — entra con la Fase 5 (LK-02).
- Presets — LK-33.
