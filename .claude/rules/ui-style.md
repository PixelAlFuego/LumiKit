---
paths: ["Assets/LumiKit/Runtime/Scripts/UI/**","Assets/LumiKit/Prefabs/UI/**"]
---

# Estilo de interfaz
Fuente: GDD Parte 2 (líneas 393-621). Valores literales, no aproximar.
Los valores viven en `LumiTheme.cs`; aquí están para consultarlos sin abrir el código.
Donde esta tabla y `LumiTheme` discrepen, manda `LumiTheme`: ahí está el motivo comentado.

## Paleta
Superficies · Void `#0D0F14` (fondo de aplicación y de escena) · Surface `#151922` (paneles,
barras, contenedores) · SurfaceElevated `#1E2430` (tarjetas, hover de listas) ·
Border `#2A3242` (bordes por defecto, separadores) · BorderStrong `#3D4759` (énfasis, campo activo).
Acentos · LumiCyan `#00E5D4` (primario: acción principal, selección activa) ·
LumiMagenta `#FF3D9A` (secundario: acentos, estados alternativos) ·
LumiViolet `#8B5CF6` (terciario: categorización).
Señales · SignalAmber `#FFB020` (advertencia, valor fuera de rango) ·
SignalGreen `#3DD68C` (confirmación) · SignalRed `#FF5A5A` (error, acción destructiva).
Texto · TextPrimary `#F2F5FA` (títulos, valores) · TextSecondary `#A0AABA` (etiquetas,
descripciones) · TextMuted `#5C6678` (placeholder, deshabilitado) ·
TextOnAccent `#0D0F14` (texto sobre cian o magenta).

Reglas: un solo acento primario por pantalla · neón ≤10% de la superficie ·
texto sobre acento siempre oscuro · sin degradados en UI · contraste mínimo 4.5:1.

## Tipografía
Nivel · familia · tamaño · peso:
Space Grotesk — Display 42 Bold · H1 28 Medium · H2 20 Medium.
Inter — H3 16 Medium · Body 14 Regular · Label **16** Medium · Caption 12 Regular.
JetBrains Mono — Mono **16** Regular.

Label y Mono van a 16 y no a los 13 del GDD (líneas 456-458): a 13 no se leen con la resolución
de diseño de 1920×1080. Cerrado por el usuario en la Sesión 05 (D-007). El resto de la escala no
se toca. Las familias todavía no existen en disco: todo el HUD va con LiberationSans hasta LK-22b.
Interlineado 1.5 (1.2 en títulos) · sentence case, nunca ALL CAPS salvo ES/EN/2D/3D ·
mínimo absoluto 12 px · +0.02em de tracking en ≤12 px.

## Medidas
Botón: alto 36 (compacto 28, el de dentro de un panel) · padding 16 (compacto 12) · radio 6 ·
separación 8 · transición 150 ms ease-out · escala al presionar 0.98 · borde 1.
Jerarquía (GDD §2.7, líneas 547-577) — primario: fondo cian, texto `#0D0F14`, sin borde ·
secundario: fondo transparente, texto `#F2F5FA`, borde 1 px `#3D4759`; en hover fondo `#1E2430`
y borde cian; al presionar fondo `#151922` y texto cian · terciario: como el secundario sin borde ·
destructivo: el secundario con borde y texto en `#FF5A5A`, sólo para reset global e irreversibles.
Slider: riel 4 px `#2A3242`, relleno `#00E5D4`, manija ⌀16 `#F2F5FA` con borde 2 px cian,
valor a la derecha en Mono, ancho fijo 56 px (el GDD dice 48: con Mono a 16, "10.00" no cabe).
Color: botón 36×24, radio 4, borde 1 px `#3D4759`.
Toggle: pista 36×20 radio completo · off `#2A3242`+`#5C6678` · on `#00E5D4`+`#F2F5FA`.
Panel de parámetros: ancho 280, fondo `#151922`, borde izq. 1 px `#2A3242`, cabecera 48 px,
padding 16, pie fijo de 68 (derivado: botón estándar más el padding arriba y abajo) con Reset
y Copiar, separador de 1 px por encima.
Tooltip: fondo `#0D0F14` 90%, borde 1 px cian, radio 4, padding 6×10, offset de cursor 12.

## Código
Namespace `LumiKit.UI` / `LumiKit.UI.Widgets`. La UI lee `Core` y `Demo`; `Core` nunca
conoce la UI. Colores y medidas se leen de `LumiTheme`, nunca literales sueltos.
Ningún widget conoce un efecto concreto: se configura desde el `EffectParameter` (D-003).
Los prefabs de UI se generan por script de editor (D-002).
