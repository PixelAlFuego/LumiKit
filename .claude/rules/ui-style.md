---
paths: ["Assets/LumiKit/Runtime/Scripts/UI/**","Assets/LumiKit/Prefabs/UI/**"]
---

# Estilo de interfaz
Fuente: GDD Parte 2 (líneas 393-621). Valores literales, no aproximar.

## Paleta
| Token | Hex | Uso |
|---|---|---|
| Void | `#0D0F14` | Fondo de aplicación y de escena |
| Surface | `#151922` | Paneles, barras, contenedores |
| SurfaceElevated | `#1E2430` | Tarjetas, hover de listas |
| Border | `#2A3242` | Bordes por defecto, separadores |
| BorderStrong | `#3D4759` | Bordes de énfasis, campo activo |
| LumiCyan | `#00E5D4` | Primario: acción principal, selección activa |
| LumiMagenta | `#FF3D9A` | Secundario: acentos, estados alternativos |
| LumiViolet | `#8B5CF6` | Terciario: categorización |
| SignalAmber | `#FFB020` | Advertencia, valor fuera de rango |
| SignalGreen | `#3DD68C` | Confirmación |
| SignalRed | `#FF5A5A` | Error, acción destructiva |
| TextPrimary | `#F2F5FA` | Títulos, valores |
| TextSecondary | `#A0AABA` | Etiquetas, descripciones |
| TextMuted | `#5C6678` | Placeholder, deshabilitado |
| TextOnAccent | `#0D0F14` | Texto sobre cian o magenta |

Reglas: un solo acento primario por pantalla · neón ≤10% de la superficie ·
texto sobre acento siempre oscuro · sin degradados en UI · contraste mínimo 4.5:1.

## Tipografía
| Nivel | Familia | Tamaño | Peso |
|---|---|---|---|
| Display | Space Grotesk | 42 | Bold |
| H1 | Space Grotesk | 28 | Medium |
| H2 | Space Grotesk | 20 | Medium |
| H3 | Inter | 16 | Medium |
| Body | Inter | 14 | Regular |
| Label | Inter | 13 | Medium |
| Caption | Inter | 12 | Regular |
| Mono | JetBrains Mono | 13 | Regular |

Interlineado 1.5 (1.2 en títulos) · sentence case, nunca ALL CAPS salvo ES/EN/2D/3D ·
mínimo absoluto 12 px · +0.02em de tracking en ≤12 px.

## Medidas
Botón: alto 36 (compacto 28) · padding 16 (compacto 12) · radio 6 · separación 8 ·
transición 150 ms ease-out · escala al presionar 0.98.
Slider: riel 4 px `#2A3242`, relleno `#00E5D4`, manija ⌀16 `#F2F5FA` con borde 2 px cian,
valor a la derecha en Mono, ancho fijo 48 px.
Color: botón 36×24, radio 4, borde 1 px `#3D4759`.
Toggle: pista 36×20 radio completo · off `#2A3242`+`#5C6678` · on `#00E5D4`+`#F2F5FA`.
Panel de parámetros: ancho 280, fondo `#151922`, borde izq. 1 px `#2A3242`,
cabecera 48 px, padding 16, pie fijo con Reset y Copiar.
Tooltip: fondo `#0D0F14` 90%, borde 1 px cian, radio 4, padding 6×10, offset de cursor 12.

## Código
Namespace `LumiKit.UI` / `LumiKit.UI.Widgets`. La UI lee `Core` y `Demo`; `Core` nunca
conoce la UI. Colores y medidas se leen de `LumiTheme` (LK-22), nunca literales sueltos.
Ningún widget conoce un efecto concreto: se configura desde el `EffectParameter` (D-003).
Los prefabs de UI se generan por script de editor (D-002).
