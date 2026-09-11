# LumiKit — Shader & VFX Pack
## Guía de Avance No. 2 — Refinamiento del Diseño y Planificación de la Producción

**Programa:** Tecnología en Desarrollo de Videojuegos — SENA
**Municipio:** Popayán, Cauca
**Fecha:** Julio 2026
**Versión del documento:** 2.0

---

# PARTE 1: GAME DESIGN DOCUMENT — VERSIÓN 2

## 1.1 Nota sobre la naturaleza del producto

LumiKit es un **pack de assets con entorno interactivo de demostración**, no un videojuego convencional. Por esta razón, las secciones del GDD que tradicionalmente describen elementos de gameplay competitivo (progresión, recompensas, personajes) se han adaptado al contexto de una **herramienta interactiva de exploración**, manteniendo la estructura documental exigida pero traduciendo cada concepto a su equivalente funcional dentro del producto.

Esta adaptación se explicita en cada sección correspondiente para evitar ambigüedad durante la evaluación.

---

## 1.2 Ajustes derivados de la retroalimentación

> ⚠️ **Nota para el aprendiz:** completa o ajusta esta tabla con la retroalimentación real que recibiste de tu instructor. Los ítems listados son refinamientos técnicos identificados durante la revisión interna del Avance 1.

| # | Observación / Ajuste | Acción tomada en la versión 2 |
|---|---|---|
| A1 | El flujo de navegación entre zonas no estaba definido | Se documenta el diagrama completo de estados y navegación (sección 1.11) |
| A2 | Faltaba especificar cómo el usuario percibe su avance en la demo | Se define el sistema de exploración y descubrimiento de efectos (sección 1.9) |
| A3 | La arquitectura del proyecto no estaba planificada | Se documenta arquitectura completa de carpetas, escenas y scripts (Parte 4) |
| A4 | No se había definido la identidad visual del producto | Se elabora la guía gráfica completa (Parte 2) |
| A5 | Las escenas de demostración estaban planteadas como una sola escena monolítica | Se reestructura en **escenas independientes por zona**, permitiendo que el usuario final modifique o elimine zonas sin afectar el resto del pack |
| A6 | No se contemplaba retroalimentación sonora en la interfaz | Se incorporan efectos de sonido básicos de UI al alcance del PMV |

---

## 1.3 Mecánicas refinadas

### Mecánica 1 — Selección de objeto (Object Picking)

El usuario hace clic izquierdo sobre cualquier objeto marcado como demostrable dentro de la escena. El sistema lanza un raycast desde la cámara hacia el punto del cursor; si impacta un objeto con el componente `EffectController`, ese objeto se convierte en el **objeto activo**.

**Retroalimentación al usuario:** el objeto activo recibe un contorno de selección color cian neón, el panel lateral se puebla con sus parámetros configurables y se reproduce el sonido `sfx_select`.

### Mecánica 2 — Ajuste de parámetros en tiempo real

El panel lateral genera dinámicamente los controles correspondientes al efecto del objeto activo. Cada tipo de parámetro tiene su control asociado:

| Tipo de parámetro | Control de UI | Ejemplo |
|---|---|---|
| `Float` | Slider horizontal con valor numérico | Grosor del outline (0.0 – 10.0) |
| `Color` | Selector de color con muestra | Color del brillo |
| `Boolean` | Interruptor (toggle) | Activar animación de pulso |
| `Enum` | Botones segmentados | Tipo de outline: sólido / punteado / animado |

Cualquier cambio se aplica **inmediatamente** al material del objeto mediante `MaterialPropertyBlock`, sin recompilación y sin modificar el material original en disco.

### Mecánica 3 — Comparación antes / después

El usuario mantiene presionada la tecla `TAB` o el botón "Comparar" en pantalla, y el efecto se desactiva temporalmente mostrando el objeto en su estado base. Al soltar, el efecto vuelve. Permite valorar el impacto visual real del shader.

### Mecánica 4 — Navegación libre en el espacio

Cámara en primera persona controlada con `WASD` para desplazamiento y mouse para rotación. En las zonas 2D la cámara es ortográfica con desplazamiento restringido a los ejes X e Y; en la zona 3D es en perspectiva con movimiento libre.

### Mecánica 5 — Restablecimiento de valores

Botón `Reset` por efecto que devuelve todos los parámetros a sus valores por defecto, definidos en el `EffectDefinition` correspondiente. Existe también un `Reset global` en el menú de pausa.

### Mecánica 6 — Copia de configuración

Al terminar de ajustar un efecto, el usuario presiona "Copiar valores" y el sistema copia al portapapeles los valores actuales en formato legible, listos para replicar la configuración en su propio proyecto. **Esta es la mecánica que conecta la demo con el uso real del pack.**

---

## 1.4 Reglas del sistema

Como producto de exploración libre, LumiKit no tiene condiciones de victoria o derrota. Sus reglas son restricciones de comportamiento del sistema:

1. Solo puede haber **un objeto activo** a la vez. Seleccionar un nuevo objeto deselecciona el anterior.
2. Los parámetros están **acotados** por valores mínimos y máximos definidos en el `EffectDefinition`, impidiendo configuraciones que rompan el shader.
3. Los cambios del usuario son **volátiles por sesión**: al salir de la escena, los efectos vuelven a sus valores por defecto. No se sobrescriben los materiales originales del pack.
4. Cada zona de demostración es **independiente**: eliminar o modificar una escena no afecta el funcionamiento de las demás.
5. La navegación con `WASD` se **deshabilita** mientras el cursor está sobre el panel de UI, evitando que el usuario mueva la cámara al arrastrar un slider.
6. El idioma seleccionado (ES/EN) se aplica **globalmente** y persiste entre sesiones mediante `PlayerPrefs`.

---

## 1.5 Gameplay Loop (Ciclo de interacción)

El ciclo central que el usuario repite durante toda la experiencia:

```
    ┌─────────────────────────────────────────────────┐
    │                                                 │
    ▼                                                 │
[1] EXPLORAR                                          │
    El usuario navega la escena y observa             │
    los objetos disponibles                           │
    │                                                 │
    ▼                                                 │
[2] SELECCIONAR                                       │
    Hace clic sobre un objeto demostrable             │
    → El panel muestra sus parámetros                 │
    │                                                 │
    ▼                                                 │
[3] EXPERIMENTAR                                      │
    Ajusta sliders, colores y toggles                 │
    → Ve el resultado en tiempo real                  │
    │                                                 │
    ▼                                                 │
[4] COMPARAR                                          │
    Presiona TAB para ver el antes/después            │
    → Valora el impacto real del efecto               │
    │                                                 │
    ▼                                                 │
[5] EVALUAR / CAPTURAR                                │
    Decide si el efecto le sirve                      │
    → Copia los valores para su proyecto              │
    │                                                 │
    └─────────────────────────────────────────────────┘
              Repite con el siguiente efecto
```

**Duración estimada del ciclo completo por efecto:** 1 a 3 minutos.
**Duración estimada de la sesión completa (8 efectos):** 12 a 20 minutos.

---

## 1.6 HUD (Interfaz durante la interacción)

El HUD de LumiKit está diseñado bajo el principio de **mínima obstrucción del viewport**, ya que el contenido visual es el producto mismo.

| Zona del HUD | Ubicación | Contenido | Comportamiento |
|---|---|---|---|
| **Barra superior** | Superior, full width, 48px | Botón volver al menú, nombre de la zona actual, pestañas 2D/3D/VFX, botón configuración | Fija, siempre visible |
| **Panel de parámetros** | Lateral derecho, 280px | Nombre del efecto activo, descripción breve, controles dinámicos, botones Reset y Copiar | Se despliega al seleccionar un objeto; colapsable con botón `‹` |
| **Indicador de controles** | Inferior izquierda | Recordatorio de teclas: `WASD` mover · `Clic` seleccionar · `TAB` comparar | Se desvanece a los 10 segundos; reaparece con la tecla `H` |
| **Contador de exploración** | Superior derecha | `Efectos explorados: 3 / 8` | Se actualiza al seleccionar un efecto por primera vez |
| **Etiqueta flotante** | Sobre el objeto bajo el cursor | Nombre del efecto (ej: "Dissolve Shader") | Aparece en hover, desaparece al salir |
| **Toast de confirmación** | Inferior centro | Mensajes breves ("Valores copiados", "Efecto restablecido") | Aparece 2 segundos y se desvanece |

**Regla de diseño del HUD:** el área central del viewport (el 60% central de la pantalla) permanece siempre libre de elementos de interfaz.

---

## 1.7 Personajes (Objetos de demostración)

En LumiKit no existen personajes jugables ni NPCs. El rol de "personaje" lo cumplen los **objetos de demostración**: elementos visuales diseñados específicamente para exhibir cada shader de la mejor manera posible.

### Objetos de la Zona 2D

| Objeto | Efecto que demuestra | Justificación del diseño |
|---|---|---|
| **Lumi** (sprite mascota) | Outline Shader | Personaje de contorno simple y silueta clara, ideal para evidenciar el borde. Actúa como mascota informal del pack |
| **Cristal 2D** | Dissolve Shader | Forma geométrica con área amplia y uniforme donde la disolución progresiva es claramente legible |
| **Runa flotante** | Glow / Inner Glow | Sprite con detalles internos y contorno irregular que aprovecha tanto el brillo interior como el exterior |

**Lumi** es un sprite bidimensional de estilo *flat* con silueta redondeada, cuerpo de color oscuro y detalle luminoso central. Su diseño prioriza la legibilidad del contorno sobre la complejidad, y funciona como elemento de identidad visual del producto.

### Objetos de la Zona 3D

| Objeto | Efecto que demuestra | Justificación del diseño |
|---|---|---|
| **Cabeza / busto low-poly** | Toon / Cel Shader | Superficie curva con planos variados que evidencia claramente las bandas de sombreado |
| **Estanque circular** | Water Shader | Superficie plana horizontal amplia, ideal para observar ondas procedurales y transparencia |
| **Prop de apoyo (roca, columna)** | Ambos (comparación) | Objeto secundario que permite contrastar el shader aplicado y sin aplicar |

### Objetos de la Zona VFX

| Objeto | Efecto que demuestra |
|---|---|
| **Antorcha / brasero** | Fire & Smoke Particle System |
| **Pedestal de invocación** | Magic Sparkles Particle System |
| **Diana de impacto** | Hit Flash / Impact Effect |

---

## 1.8 Escenarios

Cada zona de demostración es una **escena independiente de Unity**, diseñada como un espacio neutro que no compite visualmente con los efectos.

### Escena `02_Demo_2D`

Espacio bidimensional con cámara ortográfica. Fondo degradado plano en tonos oscuros (`#0D0F14` → `#151922`) con una retícula técnica sutil que refuerza la identidad de herramienta profesional. Los tres objetos 2D se disponen horizontalmente sobre una línea base común, separados por espaciado uniforme. Iluminación plana sin sombras.

### Escena `03_Demo_3D`

Espacio tridimensional cerrado tipo estudio fotográfico. Suelo oscuro mate con retícula técnica, sin paredes visibles (fondo negro degradado). Iluminación de tres puntos: luz principal direccional cálida, luz de relleno fría de baja intensidad y contraluz cian que refuerza la paleta neón. Los objetos se sitúan sobre plataformas circulares elevadas.

### Escena `04_Demo_VFX`

Espacio abierto oscuro, deliberadamente más vacío que las anteriores para que las partículas destaquen. Suelo con retícula tenue. Cada emisor de partículas está sobre un pedestal con un botón físico de activación al frente. Iluminación ambiental muy baja para maximizar el contraste de las partículas emisivas.

### Escenas de soporte

| Escena | Descripción |
|---|---|
| `00_Splash` | Pantalla de carga con logotipo centrado sobre fondo oscuro. Duración 2 segundos con transición automática |
| `01_MainMenu` | Menú principal sobre fondo con partículas ambientales suaves en cian y magenta |
| `05_Credits` | Pantalla informativa con datos del pack, créditos y enlaces externos |

---

## 1.9 Progresión del usuario

Adaptación del concepto de "progresión del jugador" al contexto de una herramienta de exploración.

La progresión en LumiKit es **de descubrimiento**, no de habilidad ni de dificultad. Se estructura en tres niveles:

**Nivel 1 — Exploración inicial:** el usuario recorre las zonas y descubre qué efectos contiene el pack. El contador `Efectos explorados: X / 8` registra cada efecto seleccionado por primera vez.

**Nivel 2 — Experimentación:** el usuario manipula parámetros y comprende el rango de posibilidades de cada shader. No hay métrica formal; la retroalimentación es puramente visual.

**Nivel 3 — Apropiación:** el usuario copia configuraciones concretas para llevarlas a su propio proyecto. Este es el objetivo final del producto y marca el fin del recorrido.

**Registro de progreso:** el estado de exploración se guarda en `PlayerPrefs` y persiste entre sesiones. Al completar los 8 efectos se desbloquea un mensaje de cierre en la pantalla de créditos.

---

## 1.10 Sistema de recompensa

Adaptación del concepto de "recompensa" al contexto de una herramienta interactiva.

LumiKit no otorga recompensas materiales ni puntuaciones. Su sistema de gratificación es de **retroalimentación inmediata y reconocimiento de progreso**:

| Tipo de recompensa | Implementación | Momento en que ocurre |
|---|---|---|
| **Recompensa visual inmediata** | El efecto se aplica instantáneamente al mover un slider | Cada interacción con un parámetro |
| **Retroalimentación sonora** | Sonidos sutiles de confirmación en selección, copia y reset | Acciones puntuales de UI |
| **Reconocimiento de descubrimiento** | El contador avanza y el efecto queda marcado como explorado | Primera selección de cada efecto |
| **Confirmación de utilidad** | Toast "Valores copiados al portapapeles" | Al usar la función copiar |
| **Cierre de recorrido** | Mensaje de agradecimiento y enlace a la documentación al explorar los 8 efectos | Al completar 8/8 |

El principio de diseño es que **la recompensa es el resultado visual mismo**: el usuario obtiene satisfacción al ver el efecto funcionando, no al recibir un premio artificial.

---

## 1.11 Estados del juego y flujo de navegación

### Estados del sistema

| Estado | Descripción | Transiciones posibles |
|---|---|---|
| `SPLASH` | Pantalla de carga inicial con logotipo | → `MAIN_MENU` (automático, 2s) |
| `MAIN_MENU` | Menú principal de selección de zona | → `LOADING`, `SETTINGS`, `CREDITS`, `EXIT` |
| `SETTINGS` | Panel de configuración | → estado anterior (retorno) |
| `LOADING` | Carga asíncrona de escena de demostración | → `DEMO_ACTIVE` |
| `DEMO_ACTIVE` | Exploración libre, sin objeto seleccionado | → `DEMO_SELECTED`, `PAUSED`, `MAIN_MENU` |
| `DEMO_SELECTED` | Objeto activo con panel de parámetros abierto | → `DEMO_ACTIVE` (deseleccionar), `DEMO_COMPARING` |
| `DEMO_COMPARING` | Efecto desactivado temporalmente (TAB presionado) | → `DEMO_SELECTED` (al soltar) |
| `PAUSED` | Menú de pausa sobre la escena (tecla ESC) | → `DEMO_ACTIVE`, `SETTINGS`, `MAIN_MENU` |
| `CREDITS` | Pantalla de información del pack | → `MAIN_MENU` |
| `EXIT` | Cierre de la aplicación | — |

### Diagrama de flujo de navegación

```
                    ┌──────────┐
                    │  SPLASH  │
                    └────┬─────┘
                         │ auto 2s
                         ▼
    ┌──────────────► ┌──────────────┐ ◄─────────────┐
    │                │  MAIN_MENU   │               │
    │      ┌─────────┴──────┬───────┴────────┐      │
    │      ▼                ▼                ▼      │
    │ ┌──────────┐    ┌──────────┐    ┌──────────┐  │
    │ │ SETTINGS │    │ CREDITS  │    │   EXIT   │  │
    │ └────┬─────┘    └────┬─────┘    └──────────┘  │
    │      └───────────────┘                        │
    │                                               │
    │              (selección de zona)              │
    │                      │                        │
    │                      ▼                        │
    │              ┌──────────────┐                 │
    │              │   LOADING    │                 │
    │              └──────┬───────┘                 │
    │                     ▼                         │
    │            ┌─────────────────┐                │
    │      ┌────►│   DEMO_ACTIVE   │────────────────┘
    │      │     └────┬────────┬───┘   (volver al menú)
    │      │          │        │ ESC
    │      │  clic en │        ▼
    │      │  objeto  │   ┌──────────┐
    │      │          │   │  PAUSED  │──► SETTINGS
    │      │          │   └────┬─────┘
    │      │          │        │ reanudar
    │      │          │        └────────┐
    │      │          ▼                 │
    │      │  ┌──────────────────┐      │
    │      └──┤  DEMO_SELECTED   │◄─────┘
    │  (deselec) └────┬─────▲────┘
    │                 │ TAB │ soltar
    │                 ▼     │
    │        ┌─────────────────┐
    │        │ DEMO_COMPARING  │
    │        └─────────────────┘
    │
    └── (cambio de zona vía pestañas 2D/3D/VFX → LOADING)
```

---

## 1.12 Requerimientos técnicos

### Requerimientos de desarrollo

| Componente | Especificación |
|---|---|
| Motor | Unity 6 LTS (6000.0.x) |
| Render Pipeline | Universal Render Pipeline (URP) 17.x |
| Lenguaje | C# (.NET Standard 2.1) |
| Sistema de shaders | Shader Graph 17.x |
| Sistema de partículas | Built-in Particle System (Shuriken) |
| Sistema de UI | Unity UI (uGUI) + TextMeshPro |
| Control de versiones | Git + GitHub (con `.gitignore` de Unity) |
| Modelado 3D | Blender 4.x |
| Edición gráfica | Adobe Photoshop CC / Illustrator CC |

### Requerimientos mínimos de ejecución (usuario final)

| Componente | Mínimo | Recomendado |
|---|---|---|
| Sistema operativo | Windows 10 (64 bits) | Windows 11 (64 bits) |
| Procesador | Intel Core i3 6ª gen / AMD equivalente | Intel Core i5 8ª gen o superior |
| Memoria RAM | 4 GB | 8 GB |
| Tarjeta gráfica | Compatible con DirectX 11, 2 GB VRAM | GPU dedicada, 4 GB VRAM |
| Almacenamiento | 500 MB libres | 1 GB libres |
| Resolución | 1280 × 720 | 1920 × 1080 |

### Requerimientos de integración (para el desarrollador que usa el pack)

- Unity 6 LTS o superior
- Proyecto configurado con Universal Render Pipeline
- Shader Graph instalado (viene incluido con URP)
- TextMeshPro (solo si se importa la escena demo)

### Objetivos de rendimiento

| Métrica | Objetivo |
|---|---|
| Framerate en escenas demo | ≥ 60 FPS en hardware recomendado |
| Framerate mínimo aceptable | ≥ 30 FPS en hardware mínimo |
| Tiempo de carga entre escenas | < 3 segundos |
| Tamaño final del `.unitypackage` | < 150 MB |
| Draw calls por escena demo | < 150 |

---
---

# PARTE 2: GUÍA GRÁFICA

## 2.1 Concepto de identidad visual

**Dirección visual: técnico oscuro con acentos neón.**

LumiKit se presenta como una herramienta profesional para desarrolladores, no como un producto de entretenimiento. La base oscura cumple dos funciones simultáneas: comunica seriedad técnica —el lenguaje visual que los desarrolladores reconocen de sus propios editores— y maximiza el contraste de los efectos luminosos, que son el producto real.

Los acentos neón (cian y magenta) representan la luz y el color que los shaders producen. Se usan con extrema disciplina: **el neón nunca es fondo, siempre es señal.** Un exceso de color saturado en la interfaz competiría visualmente con los efectos que el producto vende.

**Principio rector: la interfaz desaparece, el efecto brilla.**

---

## 2.2 Logotipo

### Construcción

El logotipo combina un **isotipo geométrico** y un **logotipo tipográfico**.

**Isotipo:** un hexágono de contorno cian con un núcleo luminoso central en degradado cian → magenta. El hexágono remite a la geometría de las mallas 3D y a los nodos de Shader Graph; el núcleo luminoso representa la emisión de luz.

**Logotipo tipográfico:** la palabra "LumiKit" en Space Grotesk Medium, con "Lumi" en blanco y "Kit" en cian neón, sin espacio entre ambas partes.

### Variantes obligatorias

| Variante | Uso |
|---|---|
| Horizontal (isotipo + texto) | Cabeceras, documentación, página de itch.io |
| Vertical (isotipo sobre texto) | Splash screen, presentaciones |
| Isotipo solo | Icono de aplicación, favicon, marca de agua |
| Monocromo blanco | Sobre fondos claros o de color |
| Monocromo negro | Sobre fondos claros, documentación impresa |

### Reglas de uso

- **Área de protección:** margen libre equivalente a la altura del hexágono en los cuatro lados.
- **Tamaño mínimo:** 32 px de altura para el isotipo; 120 px de ancho para la versión horizontal.
- **Prohibido:** deformar proporciones, rotar, aplicar sombras o contornos adicionales, cambiar los colores de la paleta, colocar sobre fondos de bajo contraste.

---

## 2.3 Paleta cromática

### Colores base (superficies oscuras)

| Nombre | Hex | Uso |
|---|---|---|
| Void | `#0D0F14` | Fondo profundo de escenas, fondo de aplicación |
| Surface | `#151922` | Paneles, barras, contenedores principales |
| Surface Elevated | `#1E2430` | Tarjetas, elementos elevados, hover de listas |
| Border | `#2A3242` | Bordes por defecto, separadores, retículas |
| Border Strong | `#3D4759` | Bordes de énfasis, contorno de campos activos |

### Colores de acento (neón)

| Nombre | Hex | Uso |
|---|---|---|
| Lumi Cyan | `#00E5D4` | Color primario de marca. Acciones principales, selección activa, contorno de objeto seleccionado |
| Lumi Magenta | `#FF3D9A` | Color secundario. Acentos, estados activos alternativos, degradado del logotipo |
| Lumi Violet | `#8B5CF6` | Color terciario. Categorización, elementos de apoyo |
| Signal Amber | `#FFB020` | Advertencias, valores fuera de rango recomendado |
| Signal Green | `#3DD68C` | Confirmaciones, operaciones exitosas |
| Signal Red | `#FF5A5A` | Errores, acciones destructivas (reset global) |

### Colores de texto

| Nombre | Hex | Uso |
|---|---|---|
| Text Primary | `#F2F5FA` | Títulos, valores numéricos, texto principal |
| Text Secondary | `#A0AABA` | Etiquetas, descripciones, texto de apoyo |
| Text Muted | `#5C6678` | Placeholders, metadatos, texto deshabilitado |
| Text on Accent | `#0D0F14` | Texto sobre fondos cian o magenta sólidos |

### Reglas de aplicación cromática

1. **Un solo acento primario por pantalla.** Si el cian marca la selección activa, ningún otro elemento compite con él.
2. **El neón nunca cubre áreas grandes.** Máximo el 10% de la superficie visible de la interfaz.
3. **Texto sobre acento siempre oscuro.** Nunca blanco sobre cian o magenta; el contraste es insuficiente.
4. **Sin degradados en la interfaz.** Los degradados se reservan exclusivamente para el logotipo y los fondos de escena.
5. **Contraste mínimo 4.5:1** entre texto y fondo en todos los casos (cumplimiento WCAG AA).

---

## 2.4 Tipografía

### Familias tipográficas

| Familia | Uso | Pesos utilizados | Licencia |
|---|---|---|---|
| **Space Grotesk** | Títulos, logotipo, nombres de efectos | Medium (500), Bold (700) | SIL Open Font License |
| **Inter** | Interfaz general, etiquetas, descripciones, botones | Regular (400), Medium (500) | SIL Open Font License |
| **JetBrains Mono** | Valores numéricos, nombres de propiedades de shader, código | Regular (400) | SIL Open Font License |

Las tres familias son gratuitas y de licencia abierta, lo que permite su distribución dentro del pack sin restricciones legales.

### Escala tipográfica

| Nivel | Familia | Tamaño | Peso | Uso |
|---|---|---|---|---|
| Display | Space Grotesk | 42 px | Bold | Nombre del producto en splash |
| H1 | Space Grotesk | 28 px | Medium | Títulos de pantalla |
| H2 | Space Grotesk | 20 px | Medium | Nombre del efecto activo |
| H3 | Inter | 16 px | Medium | Subtítulos, encabezados de sección |
| Body | Inter | 14 px | Regular | Descripciones, texto general |
| Label | Inter | 13 px | Medium | Etiquetas de parámetros, botones |
| Caption | Inter | 12 px | Regular | Ayudas, metadatos, notas |
| Mono | JetBrains Mono | 13 px | Regular | Valores numéricos, propiedades técnicas |

### Reglas tipográficas

- **Interlineado:** 1.5 para texto corrido, 1.2 para títulos.
- **Espaciado entre letras:** 0 por defecto; +0.02em en textos de 12px o menores para mejorar legibilidad.
- **Capitalización:** sentence case en toda la interfaz. Nunca ALL CAPS excepto en etiquetas de estado de 3 caracteres o menos (ES / EN / 2D / 3D).
- **Tamaño mínimo absoluto:** 12 px. Ningún texto por debajo de este valor.

---

## 2.5 Iconografía

### Sistema de iconos

**Familia:** Lucide Icons (o Tabler Icons como alternativa). Ambas son de estilo *outline*, licencia MIT y ofrecen consistencia geométrica.

**Especificaciones técnicas:**

| Propiedad | Valor |
|---|---|
| Estilo | Outline exclusivamente (nunca relleno) |
| Grosor de trazo | 1.5 px constante |
| Terminaciones | Redondeadas (round cap, round join) |
| Grilla base | 24 × 24 px |
| Tamaños permitidos | 16 px (inline), 20 px (botones), 24 px (navegación), 32 px (destacados) |
| Color por defecto | `#A0AABA` (Text Secondary) |
| Color activo / hover | `#00E5D4` (Lumi Cyan) |

### Inventario de iconos del proyecto

| Función | Icono | Ubicación |
|---|---|---|
| Volver | `arrow-left` | Barra superior |
| Menú | `menu-2` | Barra superior |
| Configuración | `settings` | Barra superior, menú de pausa |
| Restablecer | `refresh` | Panel de parámetros |
| Copiar valores | `copy` | Panel de parámetros |
| Comparar | `eye` / `eye-off` | Panel de parámetros |
| Colapsar panel | `chevron-right` | Panel de parámetros |
| Zona 2D | `square` | Pestañas de navegación |
| Zona 3D | `box` | Pestañas de navegación |
| Zona VFX | `sparkles` | Pestañas de navegación |
| Documentación | `file-text` | Créditos |
| Enlace externo | `external-link` | Créditos |
| Cerrar | `x` | Diálogos, paneles |
| Confirmar | `check` | Toasts de confirmación |
| Reproducir efecto | `player-play` | Zona VFX |
| Ayuda | `help-circle` | Indicador de controles |

---

## 2.6 Estilo de ilustración

### Elementos 2D

**Estilo:** *flat* geométrico con siluetas simples y contornos limpios. Sin degradados internos, sin texturas complejas, sin detalles menores a 4 px.

**Justificación técnica:** los shaders de outline y glow requieren siluetas legibles para producir resultados visibles. Un sprite con contornos irregulares o detalles finos degradaría la percepción del efecto.

**Especificaciones:**

| Propiedad | Valor |
|---|---|
| Resolución de sprites | 512 × 512 px |
| Formato | PNG-24 con canal alfa |
| Pixels Per Unit (Unity) | 100 |
| Paleta interna del sprite | Máximo 4 colores planos |
| Grosor mínimo de detalle | 4 px |

### Elementos 3D

**Estilo:** *low-poly* estilizado con superficies planas y siluetas claras.

| Propiedad | Valor |
|---|---|
| Presupuesto de polígonos | 500 – 2.000 triángulos por objeto |
| Topología | Quads limpios, sin n-gons |
| Texturas | 1024 × 1024 px máximo; preferencia por materiales de color plano |
| UVs | Desplegadas sin solapamiento, escala uniforme |
| Escala | 1 unidad Unity = 1 metro |
| Orientación | Eje Z hacia adelante, pivote en la base del objeto |

### Fondos de escena

Degradado vertical sutil de `#0D0F14` a `#151922`, con retícula técnica superpuesta: líneas de 1 px en color `#2A3242` con opacidad al 20%, espaciadas cada 64 px. La retícula refuerza la lectura de "entorno de trabajo técnico" sin generar ruido visual.

---

## 2.7 Diseño de botones

### Jerarquía de botones

**Botón primario** — acción principal de la pantalla. Máximo uno por vista.

| Estado | Fondo | Texto | Borde |
|---|---|---|---|
| Normal | `#00E5D4` | `#0D0F14` | Ninguno |
| Hover | `#33EBDD` | `#0D0F14` | Ninguno |
| Presionado | `#00C4B6` | `#0D0F14` | Ninguno |
| Deshabilitado | `#1E2430` | `#5C6678` | Ninguno |

**Botón secundario** — acciones alternativas.

| Estado | Fondo | Texto | Borde |
|---|---|---|---|
| Normal | Transparente | `#F2F5FA` | `1px #3D4759` |
| Hover | `#1E2430` | `#F2F5FA` | `1px #00E5D4` |
| Presionado | `#151922` | `#00E5D4` | `1px #00E5D4` |
| Deshabilitado | Transparente | `#5C6678` | `1px #2A3242` |

**Botón terciario (ghost)** — acciones de baja jerarquía.

| Estado | Fondo | Texto |
|---|---|---|
| Normal | Transparente | `#A0AABA` |
| Hover | `#1E2430` | `#F2F5FA` |
| Presionado | `#151922` | `#00E5D4` |

**Botón destructivo** — reset global, acciones irreversibles. Idéntico al secundario pero con borde y texto en `#FF5A5A`.

### Especificaciones geométricas

| Propiedad | Valor |
|---|---|
| Altura estándar | 36 px |
| Altura compacta | 28 px (dentro de paneles) |
| Padding horizontal | 16 px (estándar), 12 px (compacto) |
| Radio de esquina | 6 px |
| Espaciado entre botones | 8 px |
| Transición de estado | 150 ms, ease-out |
| Escala al presionar | 0.98 |

---

## 2.8 Componentes de interfaz

### Slider de parámetro

Riel de 4 px de altura en `#2A3242`, con relleno de progreso en `#00E5D4`. Manija circular de 16 px de diámetro con fondo `#F2F5FA` y borde de 2 px en `#00E5D4`. El valor numérico se muestra a la derecha en JetBrains Mono 13 px, alineado a la derecha con ancho fijo de 48 px para evitar saltos de layout al cambiar de valor.

### Selector de color

Botón rectangular de 36 × 24 px con radio de 4 px que muestra el color actual como relleno, con borde de 1 px en `#3D4759`. Al hacer clic despliega un panel con rueda de color, campo hexadecimal en JetBrains Mono y seis muestras predefinidas de la paleta.

### Interruptor (toggle)

Pista de 36 × 20 px con radio completo. Estado apagado: fondo `#2A3242`, manija `#5C6678` a la izquierda. Estado encendido: fondo `#00E5D4`, manija `#F2F5FA` a la derecha. Transición de 150 ms.

### Panel de parámetros

Contenedor de 280 px de ancho, fondo `#151922`, borde izquierdo de 1 px en `#2A3242`. Cabecera de 48 px con el nombre del efecto en Space Grotesk 20 px. Cuerpo con padding de 16 px y separadores de 1 px entre grupos de parámetros. Pie fijo con los botones Reset y Copiar.

### Pestañas de navegación (2D / 3D / VFX)

Contenedor segmentado con fondo `#151922` y radio de 6 px. Pestaña inactiva: texto `#A0AABA`, sin fondo. Pestaña activa: fondo `#1E2430`, texto `#00E5D4`, con una línea inferior de 2 px en `#00E5D4`.

### Toast de notificación

Contenedor flotante de altura 40 px, fondo `#1E2430`, borde de 1 px en `#3D4759`, radio de 6 px. Icono de 16 px a la izquierda seguido del mensaje en Inter 14 px. Aparece con desplazamiento vertical de 8 px y desvanecimiento en 200 ms; permanece 2 segundos; desaparece con la animación inversa.

### Etiqueta flotante (tooltip de objeto)

Fondo `#0D0F14` con opacidad 90%, borde de 1 px en `#00E5D4`, radio de 4 px, padding de 6 × 10 px. Texto en Inter 13 px color `#F2F5FA`. Sigue la posición del cursor con un desplazamiento de 12 px.

---

## 2.9 Referencias visuales

| Referencia | Qué se toma de ella |
|---|---|
| **Unity Editor (tema oscuro)** | Jerarquía de paneles, densidad de información, comportamiento de sliders y campos numéricos |
| **Blender 4.x** | Uso de acentos de color sobre base neutra oscura, tratamiento del viewport 3D |
| **Shader Graph** | Estética de nodos, uso del cian como color de conexión activa |
| **Figma (tema oscuro)** | Diseño de paneles laterales de propiedades, tratamiento de selectores de color |
| **Linear.app** | Disciplina cromática, uso mínimo de acento sobre superficie oscura, tipografía de interfaz |
| **Tron: Legacy (dirección de arte)** | Referencia conceptual del neón como luz emisiva sobre oscuridad, no como color decorativo |

---
---

# PARTE 3: BACKLOG DE FUNCIONALIDADES

## 3.1 Criterios de priorización

Cada funcionalidad se clasifica en tres niveles:

- **Esencial (MVP)** — sin esta funcionalidad el producto no cumple su propuesta de valor. Debe estar completa el 17 de diciembre de 2026.
- **Deseable** — mejora significativamente la experiencia pero el producto funciona sin ella. Se implementa si el cronograma lo permite.
- **Futura** — planificada para versiones posteriores a la entrega de la etapa práctica.

El identificador `LK-XX` permite referenciar cada ítem en el control de versiones y en los avances siguientes.

---

## 3.2 Funcionalidades esenciales (MVP)

| ID | Funcionalidad | Categoría | Estimación | Prioridad |
|---|---|---|---|---|
| LK-01 | Outline Shader 2D funcional con color, grosor y modo | Shader | 5 días | Crítica |
| LK-02 | Dissolve Shader 2D con textura de ruido y borde emisivo | Shader | 5 días | Crítica |
| LK-03 | Glow / Inner Glow Shader 2D con intensidad y pulso | Shader | 4 días | Crítica |
| LK-04 | Toon / Cel Shader 3D con bandas configurables | Shader | 6 días | Crítica |
| LK-05 | Water Shader 3D con ondas procedurales y transparencia | Shader | 6 días | Crítica |
| LK-06 | Sistema de partículas Fire & Smoke | VFX | 3 días | Crítica |
| LK-07 | Sistema de partículas Magic Sparkles | VFX | 3 días | Crítica |
| LK-08 | Sistema de partículas Hit Flash / Impact | VFX | 3 días | Crítica |
| LK-09 | Arquitectura de `EffectDefinition` (ScriptableObject) | Sistema | 4 días | Crítica |
| LK-10 | Sistema de selección de objetos por raycast | Sistema | 2 días | Crítica |
| LK-11 | Panel de parámetros con generación dinámica de controles | UI | 6 días | Crítica |
| LK-12 | Controlador de cámara (ortográfica 2D y perspectiva 3D) | Sistema | 3 días | Crítica |
| LK-13 | Escena de menú principal con navegación | UI | 3 días | Crítica |
| LK-14 | Escena `02_Demo_2D` construida y poblada | Escena | 3 días | Crítica |
| LK-15 | Escena `03_Demo_3D` construida y poblada | Escena | 3 días | Crítica |
| LK-16 | Escena `04_Demo_VFX` construida y poblada | Escena | 3 días | Crítica |
| LK-17 | Sistema de carga asíncrona entre escenas | Sistema | 2 días | Alta |
| LK-18 | Pantalla de configuración (calidad, resolución, idioma) | UI | 3 días | Alta |
| LK-19 | Sistema de localización ES / EN | Sistema | 4 días | Alta |
| LK-20 | Objetos de demostración 2D (Lumi, cristal, runa) | Arte | 4 días | Alta |
| LK-21 | Objetos de demostración 3D low-poly en Blender | Arte | 5 días | Alta |
| LK-22 | Identidad visual aplicada a toda la UI | Arte | 4 días | Alta |
| LK-23 | Efectos de sonido básicos de interfaz | Audio | 2 días | Media |
| LK-24 | Función de comparación antes / después (TAB) | Sistema | 2 días | Media |
| LK-25 | Pantalla de créditos e información del pack | UI | 2 días | Media |
| LK-26 | Documentación técnica bilingüe (instalación y uso) | Documentación | 5 días | Crítica |
| LK-27 | Exportación y validación del `.unitypackage` | Distribución | 2 días | Crítica |
| LK-28 | Publicación de la página del producto en itch.io | Distribución | 2 días | Crítica |

**Total estimado del MVP:** aproximadamente 96 días de trabajo efectivo.

---

## 3.3 Funcionalidades deseables

| ID | Funcionalidad | Justificación | Estimación |
|---|---|---|---|
| LK-29 | Función "Copiar valores al portapapeles" | Conecta la demo con el uso real del pack; alto valor percibido | 2 días |
| LK-30 | Contador de exploración (`Efectos explorados: X / 8`) | Refuerza la sensación de recorrido completo | 1 día |
| LK-31 | Etiqueta flotante con nombre del efecto en hover | Mejora la descubribilidad de los objetos interactivos | 1 día |
| LK-32 | Menú de pausa (ESC) dentro de las escenas demo | Comodidad de navegación | 2 días |
| LK-33 | Presets de configuración por efecto (3 por shader) | Muestra el rango de posibilidades sin que el usuario experimente a ciegas | 3 días |
| LK-34 | Toasts de confirmación de acciones | Retroalimentación clara de operaciones puntuales | 1 día |
| LK-35 | Persistencia de configuración en `PlayerPrefs` | Evita reconfigurar idioma y calidad en cada sesión | 1 día |
| LK-36 | Escena `_Playground` con todos los efectos juntos | Permite ver combinaciones de efectos | 2 días |
| LK-37 | Video de demostración para la página de itch.io | Aumenta significativamente la conversión de descargas | 3 días |

---

## 3.4 Funcionalidades futuras (post-entrega)

| ID | Funcionalidad | Versión objetivo |
|---|---|---|
| LK-38 | Port completo de los shaders a Godot 4.x | v2.0 |
| LK-39 | Publicación en Unity Asset Store | v1.1 |
| LK-40 | Publicación en Fab (Epic Games) | v2.0 |
| LK-41 | Soporte para Built-in Render Pipeline | v1.2 |
| LK-42 | Soporte para HDRP | v2.1 |
| LK-43 | Ampliación a 15+ efectos (pack extendido) | v2.0 |
| LK-44 | Editor Window personalizada dentro de Unity | v2.0 |
| LK-45 | Versión WebGL de la demo para navegador | v1.2 |
| LK-46 | Exportación de configuraciones como archivo `.json` | v2.0 |
| LK-47 | Optimización específica para plataformas móviles | v2.1 |
| LK-48 | Documentación en formato video (tutoriales) | v1.1 |

---
---

# PARTE 4: ARQUITECTURA DEL PROYECTO

## 4.1 Principio arquitectónico fundamental

La decisión estructural más importante de LumiKit es la **separación estricta entre el contenido distribuible y el contenido de desarrollo interno**.

Todo lo que el usuario final recibirá vive dentro de una única carpeta raíz: `Assets/LumiKit/`. Todo lo demás —herramientas de desarrollo, pruebas, recursos temporales, borradores— vive fuera de ella y **nunca se incluye en la exportación**.

Esta separación resuelve tres problemas críticos de forma anticipada:

1. **Exportación limpia:** seleccionar `Assets/LumiKit/` y exportar produce un `.unitypackage` correcto sin necesidad de revisar archivo por archivo.
2. **Requisito de Unity Asset Store:** la tienda exige que todo el contenido de un paquete resida bajo una única carpeta raíz con el nombre del producto. Cumplir esto desde el día uno evita una reestructuración completa antes de publicar.
3. **Sin colisiones en el proyecto del usuario:** al importar el pack, el desarrollador recibe una sola carpeta identificable que puede mover, renombrar o eliminar sin afectar su propio proyecto.

---

## 4.2 Estructura completa de carpetas

```
LumiKit_Project/
│
├── Assets/
│   │
│   ├── LumiKit/                          ◄── CONTENIDO DISTRIBUIBLE
│   │   │
│   │   ├── Runtime/
│   │   │   ├── Scripts/
│   │   │   │   ├── Core/
│   │   │   │   │   ├── EffectDefinition.cs
│   │   │   │   │   ├── EffectParameter.cs
│   │   │   │   │   ├── ParameterType.cs
│   │   │   │   │   ├── EffectController.cs
│   │   │   │   │   └── EffectRegistry.cs
│   │   │   │   │
│   │   │   │   ├── Demo/
│   │   │   │   │   ├── ObjectSelector.cs
│   │   │   │   │   ├── DemoCameraController.cs
│   │   │   │   │   ├── OrbitCameraController.cs
│   │   │   │   │   ├── VFXTrigger.cs
│   │   │   │   │   └── ComparisonToggle.cs
│   │   │   │   │
│   │   │   │   ├── UI/
│   │   │   │   │   ├── ParameterPanelUI.cs
│   │   │   │   │   ├── Widgets/
│   │   │   │   │   │   ├── ParameterWidgetBase.cs
│   │   │   │   │   │   ├── SliderWidget.cs
│   │   │   │   │   │   ├── ColorWidget.cs
│   │   │   │   │   │   ├── ToggleWidget.cs
│   │   │   │   │   │   └── EnumWidget.cs
│   │   │   │   │   ├── MainMenuUI.cs
│   │   │   │   │   ├── SettingsUI.cs
│   │   │   │   │   ├── PauseMenuUI.cs
│   │   │   │   │   ├── ToastNotifier.cs
│   │   │   │   │   ├── HoverLabelUI.cs
│   │   │   │   │   └── ExplorationCounterUI.cs
│   │   │   │   │
│   │   │   │   ├── Systems/
│   │   │   │   │   ├── GameStateManager.cs
│   │   │   │   │   ├── SceneLoader.cs
│   │   │   │   │   ├── LocalizationManager.cs
│   │   │   │   │   ├── SettingsManager.cs
│   │   │   │   │   ├── UIAudioManager.cs
│   │   │   │   │   └── ExplorationTracker.cs
│   │   │   │   │
│   │   │   │   ├── Utils/
│   │   │   │   │   ├── Singleton.cs
│   │   │   │   │   ├── ClipboardHelper.cs
│   │   │   │   │   └── MaterialPropertyHelper.cs
│   │   │   │   │
│   │   │   │   └── LumiKit.Runtime.asmdef
│   │   │   │
│   │   │   └── Data/
│   │   │       ├── Effects/
│   │   │       │   ├── EFF_Outline2D.asset
│   │   │       │   ├── EFF_Dissolve2D.asset
│   │   │       │   ├── EFF_Glow2D.asset
│   │   │       │   ├── EFF_Toon3D.asset
│   │   │       │   ├── EFF_Water3D.asset
│   │   │       │   ├── EFF_FireSmoke.asset
│   │   │       │   ├── EFF_MagicSparkles.asset
│   │   │       │   └── EFF_HitFlash.asset
│   │   │       │
│   │   │       ├── Localization/
│   │   │       │   ├── LOC_Spanish.asset
│   │   │       │   └── LOC_English.asset
│   │   │       │
│   │   │       └── Presets/
│   │   │           ├── PRE_Outline_Neon.asset
│   │   │           ├── PRE_Outline_Soft.asset
│   │   │           └── ...
│   │   │
│   │   ├── Shaders/
│   │   │   ├── 2D/
│   │   │   │   ├── SG_Outline2D.shadergraph
│   │   │   │   ├── SG_Dissolve2D.shadergraph
│   │   │   │   └── SG_Glow2D.shadergraph
│   │   │   │
│   │   │   ├── 3D/
│   │   │   │   ├── SG_Toon3D.shadergraph
│   │   │   │   └── SG_Water3D.shadergraph
│   │   │   │
│   │   │   └── SubGraphs/
│   │   │       ├── SUB_NoiseSampler.shadersubgraph
│   │   │       ├── SUB_FresnelGlow.shadersubgraph
│   │   │       └── SUB_WaveDisplacement.shadersubgraph
│   │   │
│   │   ├── Materials/
│   │   │   ├── 2D/
│   │   │   │   ├── MAT_Outline2D_Default.mat
│   │   │   │   ├── MAT_Dissolve2D_Default.mat
│   │   │   │   └── MAT_Glow2D_Default.mat
│   │   │   │
│   │   │   ├── 3D/
│   │   │   │   ├── MAT_Toon3D_Default.mat
│   │   │   │   └── MAT_Water3D_Default.mat
│   │   │   │
│   │   │   └── Demo/
│   │   │       ├── MAT_Floor_Grid.mat
│   │   │       ├── MAT_Pedestal.mat
│   │   │       └── MAT_Background.mat
│   │   │
│   │   ├── Textures/
│   │   │   ├── Noise/
│   │   │   │   ├── TEX_NoisePerlin_512.png
│   │   │   │   ├── TEX_NoiseVoronoi_512.png
│   │   │   │   └── TEX_NoiseClouds_512.png
│   │   │   │
│   │   │   ├── Gradients/
│   │   │   │   ├── TEX_RampToon_128.png
│   │   │   │   └── TEX_RampFire_128.png
│   │   │   │
│   │   │   ├── Particles/
│   │   │   │   ├── TEX_Spark_128.png
│   │   │   │   ├── TEX_Smoke_256.png
│   │   │   │   └── TEX_Flare_256.png
│   │   │   │
│   │   │   └── Demo/
│   │   │       ├── TEX_GridFloor_1024.png
│   │   │       └── TEX_BackgroundGradient_1024.png
│   │   │
│   │   ├── Sprites/
│   │   │   ├── SPR_Lumi_512.png
│   │   │   ├── SPR_Crystal_512.png
│   │   │   └── SPR_Rune_512.png
│   │   │
│   │   ├── Models/
│   │   │   ├── MDL_Bust_LowPoly.fbx
│   │   │   ├── MDL_Pond.fbx
│   │   │   ├── MDL_Rock.fbx
│   │   │   ├── MDL_Pedestal.fbx
│   │   │   ├── MDL_Torch.fbx
│   │   │   └── MDL_Target.fbx
│   │   │
│   │   ├── VFX/
│   │   │   ├── Prefabs/
│   │   │   │   ├── VFX_FireSmoke.prefab
│   │   │   │   ├── VFX_MagicSparkles.prefab
│   │   │   │   └── VFX_HitFlash.prefab
│   │   │   │
│   │   │   └── Materials/
│   │   │       ├── MAT_VFX_Fire.mat
│   │   │       ├── MAT_VFX_Smoke.mat
│   │   │       ├── MAT_VFX_Spark.mat
│   │   │       └── MAT_VFX_Flash.mat
│   │   │
│   │   ├── Prefabs/
│   │   │   ├── Demo/
│   │   │   │   ├── PRF_DemoObject_Lumi.prefab
│   │   │   │   ├── PRF_DemoObject_Crystal.prefab
│   │   │   │   ├── PRF_DemoObject_Rune.prefab
│   │   │   │   ├── PRF_DemoObject_Bust.prefab
│   │   │   │   ├── PRF_DemoObject_Pond.prefab
│   │   │   │   └── PRF_Pedestal.prefab
│   │   │   │
│   │   │   ├── UI/
│   │   │   │   ├── PRF_ParameterPanel.prefab
│   │   │   │   ├── PRF_Widget_Slider.prefab
│   │   │   │   ├── PRF_Widget_Color.prefab
│   │   │   │   ├── PRF_Widget_Toggle.prefab
│   │   │   │   ├── PRF_Widget_Enum.prefab
│   │   │   │   ├── PRF_TopBar.prefab
│   │   │   │   ├── PRF_Toast.prefab
│   │   │   │   └── PRF_HoverLabel.prefab
│   │   │   │
│   │   │   └── Systems/
│   │   │       └── PRF_LumiKitManagers.prefab
│   │   │
│   │   ├── Scenes/
│   │   │   ├── 00_Splash.unity
│   │   │   ├── 01_MainMenu.unity
│   │   │   ├── 02_Demo_2D.unity
│   │   │   ├── 03_Demo_3D.unity
│   │   │   ├── 04_Demo_VFX.unity
│   │   │   └── 05_Credits.unity
│   │   │
│   │   ├── Audio/
│   │   │   ├── SFX/
│   │   │   │   ├── SFX_UI_Click.wav
│   │   │   │   ├── SFX_UI_Hover.wav
│   │   │   │   ├── SFX_UI_Select.wav
│   │   │   │   ├── SFX_UI_Back.wav
│   │   │   │   ├── SFX_UI_Confirm.wav
│   │   │   │   └── SFX_UI_Reset.wav
│   │   │   │
│   │   │   └── Mixers/
│   │   │       └── AMX_LumiKit.mixer
│   │   │
│   │   ├── Fonts/
│   │   │   ├── SpaceGrotesk-Medium SDF.asset
│   │   │   ├── Inter-Regular SDF.asset
│   │   │   ├── Inter-Medium SDF.asset
│   │   │   ├── JetBrainsMono-Regular SDF.asset
│   │   │   └── Licenses/
│   │   │       └── OFL.txt
│   │   │
│   │   ├── Settings/
│   │   │   ├── URP_LumiKit_Renderer.asset
│   │   │   └── URP_LumiKit_PipelineAsset.asset
│   │   │
│   │   └── Documentation/
│   │       ├── README.txt
│   │       ├── Guia_de_Instalacion_ES.pdf
│   │       ├── Installation_Guide_EN.pdf
│   │       ├── Guia_de_Uso_ES.pdf
│   │       ├── Usage_Guide_EN.pdf
│   │       ├── CHANGELOG.txt
│   │       └── LICENSE.txt
│   │
│   ├── _Development/                     ◄── NO SE EXPORTA
│   │   ├── Tests/
│   │   │   ├── TestScenes/
│   │   │   └── PlayModeTests/
│   │   ├── WIP/
│   │   │   ├── Shaders_Borradores/
│   │   │   └── Modelos_Borradores/
│   │   ├── References/
│   │   │   └── Moodboards/
│   │   └── Screenshots/
│   │
│   ├── Editor/                           ◄── NO SE EXPORTA
│   │   ├── LumiKitExporter.cs
│   │   ├── EffectDefinitionEditor.cs
│   │   └── LumiKit.Editor.asmdef
│   │
│   ├── Plugins/
│   │   └── TextMesh Pro/
│   │
│   └── Settings/
│
├── Packages/
│   └── manifest.json
│
├── ProjectSettings/
│
├── .gitignore
├── .gitattributes
└── README.md
```

---

## 4.3 Justificación de las decisiones estructurales

### ¿Por qué `Runtime/` y `Editor/` separados?

Unity compila los scripts de `Editor/` únicamente dentro del editor y los excluye de la build final. Si un script de editor quedara mezclado con los de runtime, la compilación de la build fallaría con errores de referencia a `UnityEditor`. La separación con **Assembly Definitions** (`.asmdef`) hace esta frontera explícita y reduce significativamente el tiempo de recompilación durante el desarrollo.

### ¿Por qué escenas separadas por zona?

Cuatro razones concretas:

1. **Modificabilidad para el usuario final:** el desarrollador que importe el pack puede eliminar `04_Demo_VFX.unity` si solo le interesan los shaders, sin romper nada.
2. **Tiempo de carga:** cada escena carga únicamente sus propios assets, manteniendo el uso de memoria bajo.
3. **Control de versiones:** las escenas de Unity generan conflictos de merge muy difíciles de resolver. Escenas pequeñas y separadas reducen drásticamente ese riesgo.
4. **Trabajo incremental:** permite terminar y validar una zona completa antes de empezar la siguiente, alineándose con el cronograma por fases del proyecto.

### ¿Por qué `EffectDefinition` como ScriptableObject?

Esta es la decisión de diseño más importante del sistema. Un `EffectDefinition` es un archivo de datos que describe **qué parámetros tiene un efecto** sin contener lógica de interfaz.

La consecuencia práctica: el panel de parámetros no tiene código específico para cada shader. Lee el `EffectDefinition` del objeto seleccionado y **genera automáticamente** los controles correspondientes.

Agregar un noveno efecto al pack no requiere tocar ni una línea de código de UI: se crea un nuevo `.asset`, se configuran sus parámetros desde el inspector y el sistema lo integra solo. Esto convierte el ítem LK-43 del backlog (ampliación a 15+ efectos) en una tarea de contenido y no de programación.

### ¿Por qué `MaterialPropertyBlock` en lugar de modificar el material?

Modificar directamente un material en tiempo de ejecución dentro del editor de Unity **sobrescribe el archivo `.mat` en disco de forma permanente**. Si el usuario mueve un slider en la demo, el material del pack quedaría alterado para siempre.

`MaterialPropertyBlock` aplica los valores únicamente al renderizador de esa instancia específica, sin tocar el asset original. Además evita la creación de instancias de material duplicadas, que es una fuente común de fugas de memoria en Unity.

---

## 4.4 Arquitectura de scripts

### Diagrama de dependencias

```
┌────────────────────────────────────────────────────┐
│                    SYSTEMS                         │
│  (persistentes, DontDestroyOnLoad, singletons)     │
│                                                    │
│  GameStateManager ── SceneLoader                   │
│  LocalizationManager   SettingsManager             │
│  UIAudioManager        ExplorationTracker          │
└──────────────────┬─────────────────────────────────┘
                   │ notifica eventos
                   ▼
┌────────────────────────────────────────────────────┐
│                      UI                            │
│                                                    │
│  ParameterPanelUI ──► instancia ──► Widgets        │
│       ▲                          (Slider, Color,   │
│       │                           Toggle, Enum)    │
│       │ recibe EffectController                    │
└───────┼────────────────────────────────────────────┘
        │
┌───────┴────────────────────────────────────────────┐
│                     DEMO                           │
│                                                    │
│  ObjectSelector ──raycast──► EffectController      │
│  DemoCameraController                              │
│  VFXTrigger                                        │
└───────────────────┬────────────────────────────────┘
                    │ lee
                    ▼
┌────────────────────────────────────────────────────┐
│                     CORE                           │
│                                                    │
│  EffectController ──► EffectDefinition (SO)        │
│         │                    │                     │
│         │                    └──► EffectParameter[]│
│         │                                          │
│         └──► MaterialPropertyBlock ──► Renderer    │
└────────────────────────────────────────────────────┘
```

**Regla de dependencia:** las capas superiores conocen a las inferiores, nunca al revés. `Core` no sabe que existe la UI; la UI se comunica con `Core` a través de eventos y referencias explícitas. Esto permite que un desarrollador use los shaders del pack sin importar nada de la interfaz.

### Descripción de los scripts principales

| Script | Tipo | Responsabilidad |
|---|---|---|
| `EffectDefinition` | ScriptableObject | Define el nombre, descripción bilingüe, shader asociado y lista de parámetros configurables de un efecto |
| `EffectParameter` | Clase serializable | Describe un parámetro individual: nombre de propiedad del shader, tipo, valores mínimo/máximo/por defecto |
| `ParameterType` | Enum | `Float`, `Color`, `Boolean`, `Enum` |
| `EffectController` | MonoBehaviour | Se adjunta a cada objeto demostrable. Aplica los valores al renderizador vía `MaterialPropertyBlock` y expone métodos de reset y comparación |
| `EffectRegistry` | ScriptableObject | Catálogo central de todos los `EffectDefinition` del pack. Usado por el contador de exploración |
| `ObjectSelector` | MonoBehaviour | Lanza el raycast desde la cámara, gestiona el objeto activo y notifica a la UI |
| `DemoCameraController` | MonoBehaviour | Movimiento WASD + rotación con mouse. Se desactiva cuando el cursor está sobre la UI |
| `ParameterPanelUI` | MonoBehaviour | Lee el `EffectDefinition` activo e instancia dinámicamente los widgets correspondientes |
| `ParameterWidgetBase` | Clase abstracta | Contrato común de todos los widgets: `Initialize(param)` y evento `OnValueChanged` |
| `GameStateManager` | Singleton | Máquina de estados finita que controla las transiciones descritas en la sección 1.11 |
| `SceneLoader` | Singleton | Carga asíncrona de escenas con pantalla de transición |
| `LocalizationManager` | Singleton | Devuelve la cadena correspondiente al idioma activo a partir de una clave |
| `SettingsManager` | Singleton | Lee y escribe configuración en `PlayerPrefs`; aplica calidad y resolución |
| `UIAudioManager` | Singleton | Reproduce los efectos de sonido de interfaz a través del Audio Mixer |
| `ExplorationTracker` | Singleton | Registra qué efectos ha visitado el usuario y persiste el estado |

---

## 4.5 Convenciones de nomenclatura

Aplicar estas convenciones desde el primer archivo evita reorganizaciones costosas más adelante.

### Prefijos de assets

| Prefijo | Tipo de asset | Ejemplo |
|---|---|---|
| `SG_` | Shader Graph | `SG_Outline2D.shadergraph` |
| `SUB_` | Sub Graph de shader | `SUB_NoiseSampler.shadersubgraph` |
| `MAT_` | Material | `MAT_Toon3D_Default.mat` |
| `TEX_` | Textura | `TEX_NoisePerlin_512.png` |
| `SPR_` | Sprite | `SPR_Lumi_512.png` |
| `MDL_` | Modelo 3D | `MDL_Bust_LowPoly.fbx` |
| `PRF_` | Prefab | `PRF_ParameterPanel.prefab` |
| `VFX_` | Prefab de partículas | `VFX_FireSmoke.prefab` |
| `SFX_` | Efecto de sonido | `SFX_UI_Click.wav` |
| `EFF_` | EffectDefinition (SO) | `EFF_Outline2D.asset` |
| `LOC_` | Datos de localización | `LOC_Spanish.asset` |
| `PRE_` | Preset de configuración | `PRE_Outline_Neon.asset` |
| `AMX_` | Audio Mixer | `AMX_LumiKit.mixer` |

### Convenciones de código C#

| Elemento | Convención | Ejemplo |
|---|---|---|
| Clases, structs, enums | `PascalCase` | `EffectController` |
| Métodos públicos | `PascalCase` | `ApplyParameter()` |
| Métodos privados | `PascalCase` | `UpdateMaterialBlock()` |
| Campos privados | `_camelCase` | `_currentEffect` |
| Campos serializados | `[SerializeField] private` + `_camelCase` | `[SerializeField] private Renderer _targetRenderer;` |
| Propiedades públicas | `PascalCase` | `public EffectDefinition Definition { get; }` |
| Constantes | `UPPER_SNAKE_CASE` | `MAX_PARAMETERS` |
| Espacio de nombres | `LumiKit.<Capa>` | `namespace LumiKit.Core` |

### Convenciones de escenas

Prefijo numérico de dos dígitos que refleja el orden en el Build Settings: `00_`, `01_`, `02_`… Esto garantiza que el orden alfabético de la carpeta coincida con el orden de carga real, evitando errores de índice al usar `SceneManager.LoadScene(int)`.

---

## 4.6 Organización de recursos gráficos

| Categoría | Ubicación | Formato | Resolución | Configuración de importación |
|---|---|---|---|---|
| Sprites de demostración | `Sprites/` | PNG-24 + alfa | 512 × 512 | Sprite (2D and UI), PPU 100, filtro Bilinear, compresión None |
| Texturas de ruido | `Textures/Noise/` | PNG-8 escala de grises | 512 × 512 | Default, Wrap Repeat, compresión Normal Quality |
| Rampas de color | `Textures/Gradients/` | PNG-24 | 128 × 8 | Default, Wrap Clamp, filtro Bilinear, **sin compresión** (crítico para toon shading) |
| Texturas de partículas | `Textures/Particles/` | PNG-24 + alfa | 128–256 px | Default, Alpha Is Transparency activado |
| Texturas de entorno | `Textures/Demo/` | PNG-24 | 1024 × 1024 | Default, compresión Normal Quality |
| Iconos de UI | `Textures/UI/` | PNG-24 + alfa | 24 × 24 base | Sprite (2D and UI), Mesh Type Full Rect |

**Regla de resolución:** ninguna textura del pack supera 1024 × 1024 px. El objetivo de tamaño final del `.unitypackage` (< 150 MB) depende directamente del cumplimiento de esta regla.

---

## 4.7 Organización de recursos sonoros

El PMV incluye únicamente efectos de sonido de interfaz, sin música de fondo.

| Archivo | Momento de reproducción | Duración objetivo |
|---|---|---|
| `SFX_UI_Hover.wav` | Cursor entra en un botón o elemento interactivo | < 100 ms |
| `SFX_UI_Click.wav` | Clic en botón de la interfaz | < 150 ms |
| `SFX_UI_Select.wav` | Selección de un objeto de demostración | < 200 ms |
| `SFX_UI_Back.wav` | Retroceder o cerrar un panel | < 150 ms |
| `SFX_UI_Confirm.wav` | Confirmación de acción (copiar valores, guardar) | < 300 ms |
| `SFX_UI_Reset.wav` | Restablecimiento de parámetros | < 250 ms |

**Especificaciones técnicas:**

| Propiedad | Valor |
|---|---|
| Formato de origen | WAV, 44.1 kHz, 16 bits, mono |
| Load Type en Unity | Decompress On Load (archivos cortos) |
| Compresión | PCM (sin compresión, dado el tamaño mínimo) |
| Ruta del Audio Mixer | `Master → SFX → UI` |
| Volumen por defecto del grupo | −6 dB |

**Origen de los recursos:** sonidos generados con herramientas libres (`sfxr`, `Bfxr`) o descargados bajo licencia CC0 desde Freesound. La licencia de cada archivo debe documentarse en `Documentation/LICENSE.txt`.

---

## 4.8 Prefabs y objetos reutilizables

### Prefabs de demostración

Cada objeto demostrable es un prefab con esta jerarquía estándar:

```
PRF_DemoObject_[Nombre]
├── (Transform raíz)
├── [MeshRenderer / SpriteRenderer]
├── [Collider]                    ← requerido para el raycast de selección
├── [EffectController]            ← referencia al EffectDefinition
└── HoverAnchor (child empty)     ← punto de anclaje de la etiqueta flotante
```

Esta estructura uniforme permite que `ObjectSelector` trate a todos los objetos de forma idéntica, sin importar si son 2D, 3D o emisores de partículas.

### Prefabs de interfaz

Los widgets de parámetros son prefabs independientes que `ParameterPanelUI` instancia bajo demanda. Cada uno hereda de `ParameterWidgetBase` y expone el mismo contrato, lo que permite añadir nuevos tipos de parámetro sin modificar el panel.

### Prefab de sistemas persistentes

`PRF_LumiKitManagers.prefab` contiene todos los singletons del proyecto y se instancia una única vez en la escena `00_Splash` con `DontDestroyOnLoad`. Incluye un control de duplicados: si al cargar una escena ya existe una instancia, la nueva se destruye automáticamente.

Esto permite ejecutar cualquier escena de forma aislada desde el editor durante el desarrollo —los managers se auto-instancian si no existen— sin necesidad de pasar siempre por el splash.

---

## 4.9 Configuración de control de versiones

### `.gitignore` — entradas obligatorias

```
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]serSettings/
*.csproj
*.unityproj
*.sln
*.user
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db
.vs/
.idea/
*.apk
*.aab
*.unitypackage
*.app
sysinfo.txt
crashlytics-build.properties
```

### Configuración obligatoria del proyecto

| Ajuste | Valor | Justificación |
|---|---|---|
| `Edit → Project Settings → Editor → Version Control Mode` | **Visible Meta Files** | Sin esto, Git no rastrea los archivos `.meta` y se pierden todas las referencias entre assets |
| `Edit → Project Settings → Editor → Asset Serialization` | **Force Text** | Los archivos binarios de escena son imposibles de fusionar; el formato texto permite resolver conflictos |
| `.gitattributes` con Git LFS | `*.psd`, `*.fbx`, `*.wav`, `*.png` | Evita que el repositorio crezca sin control por versiones binarias acumuladas |

### Estrategia de ramas

| Rama | Propósito |
|---|---|
| `main` | Versiones estables y publicables únicamente |
| `develop` | Rama de integración del trabajo en curso |
| `feature/LK-XX-descripcion` | Una rama por ítem del backlog |

**Formato de mensajes de commit:**
```
[LK-01] Implementa Outline Shader con parámetro de grosor
[LK-11] Corrige generación duplicada de widgets al cambiar de objeto
```

Referenciar el ID del backlog en cada commit permite trazar directamente qué funcionalidad se trabajó en cada momento, lo cual es material de sustentación.

---

## 4.10 Procedimiento de exportación del paquete

1. Verificar que ningún asset dentro de `Assets/LumiKit/` referencie archivos externos a esa carpeta. Usar `Assets → Select Dependencies` para confirmarlo.
2. Ejecutar la escena `00_Splash` y recorrer el flujo completo verificando que no se lancen excepciones en consola.
3. Confirmar que `Documentation/` contenga la versión final de las cuatro guías, el `README.txt`, el `CHANGELOG.txt` y el `LICENSE.txt`.
4. Clic derecho sobre `Assets/LumiKit/` → `Export Package…`.
5. En el diálogo, verificar que **Include dependencies** esté activado y que no aparezca ningún archivo de `_Development/` o `Editor/` en la lista.
6. Exportar como `LumiKit_v1.0.unitypackage`.
7. **Validación obligatoria:** crear un proyecto Unity 6 URP completamente nuevo, importar el `.unitypackage` y verificar que las escenas se abran sin errores de compilación ni materiales rosados.
8. Solo tras superar el paso 7, publicar en itch.io.

> El paso 7 es innegociable. La causa más frecuente de valoraciones negativas en asset packs es que el paquete funcione en el proyecto del autor pero falle al importarse en un proyecto limpio.

---

*Documento elaborado como parte de la Guía de Avance No. 2 — Etapa Práctica*
*Programa: Tecnología en Desarrollo de Videojuegos — SENA*
*Popayán, Cauca — Julio 2026*
*Versión del documento: 2.0*
