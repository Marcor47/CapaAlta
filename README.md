# Capa Alta

Videojuego educativo de plataformas 2D para reforzar habilidades socioemocionales y comunicativas en las sesiones de tutoría de 6.° grado de primaria.

Proyecto de tesis de pregrado en Ingeniería Informática, Pontificia Universidad Católica del Perú (PUCP).
**Autor:** Marco Rodriguez · **Asesor:** Johan Paul Baldeón Medrano

---

## Sobre el juego

Theo, un joven de 18 años que no creció con su padre, recorre los cuatro senderos de la montaña Capa Alta (tierra, vegetación, roca y nieve) para llegar al pueblo donde vive su padre. En el camino encuentra cartas que los habitantes lanzan desde la montaña para que el viento se lleve sus conflictos emocionales, y conoce a otros viajeros con los que debe decidir cómo relacionarse.

Las decisiones del jugador, las relaciones que construye y las cartas que recolecta determinan cómo termina el reencuentro con su padre.

> **Estado:** prototipo demo desarrollado como proyecto de tesis. Los cuatro capítulos, el módulo alumno y el módulo docente son jugables; la narrativa completa y pulida del clímax y de los finales queda fuera del alcance de la tesis.

### Mecánicas principales

| Mecánica | Descripción |
|---|---|
| Cartas (M1, M4) | Cartas del padre (principales, una por capítulo) y de otros pobladores (secundarias, tres por capítulo) que el jugador recolecta y lee. |
| Árbol de decisiones (M2, M3) | Diálogos por rondas con 3 opciones; la relación con cada personaje (-1 / 0 / +1) influye en capítulos posteriores. |
| Motivación (M6) | Estamina que consume el dash y el salto; se recarga sola y más rápido al sentarse. |
| Regulación emocional (M9) | Baja con caídas y situaciones difíciles; se recupera meditando con la libreta o llamando a mamá. En 0 reduce el campo de visión. |
| Libreta de Theo (M7) | Menú principal con selección de capítulos y rejugabilidad. |
| Respuesta epistolar (M8) | El alumno voltea cada carta recolectada, escribe una reflexión y la vincula con una o más de las 13 habilidades socioemocionales (HSE) del MINEDU. Ver [Respuesta epistolar](#respuesta-epistolar). |
| Registro docente (M10) | El profesor ve el avance, las decisiones, las relaciones con los personajes y las reflexiones de cada alumno, y una vista agregada del grupo. Ver [Dashboard docente](#dashboard-docente). |
| Teléfono satelital (M12) | Mamá da tutoriales, consejos y apoyo emocional durante el viaje; con su llamada se desbloquean el dash y la herramienta de cada capítulo. |
| Manzanas (M13) | Ítem que mejora la relación con un personaje después de completar su diálogo. |

### Capítulos y herramientas

| Capítulo | Escenario | Herramienta nueva |
|---|---|---|
| 1 · Árido | Tierra | Teléfono satelital (tutorial) y dash |
| 2 · Frondoso | Vegetación | Onda |
| 3 · Rocoso | Roca | Gancho |
| 4 · Helado | Nieve | Pinchos (piolet) + doble salto |

Las herramientas solo funcionan mientras están equipadas en la mochila.

---

## Respuesta epistolar

Es el instrumento digital de expresión emocional del juego. Cada carta recolectada queda en el Compendio y puede responderse en cualquier momento:

1. El alumno abre la carta y la voltea para ver su reverso.
2. Escribe su reflexión en **TU REFLEXIÓN**.
3. Marca al menos una de las 13 HSE (por ejemplo, Empatía o Autoestima). El panel de ayuda incluye un glosario de las 13 habilidades.
4. El botón **GUARDAR** solo se habilita cuando hay texto y al menos una HSE. Al guardar vuelve al panel de cartas y la carta pierde su indicador de pendiente.

Otras reglas:

- La respuesta puede editarse después y se guarda con la carta (`cardID`, `reflectionText`, `linkedHSE`).
- Responder las cartas **no** es requisito para avanzar de capítulo.
- Las cartas pendientes se señalan con un indicador en el Compendio, en la pestaña de cartas y en cada carta.
- Al terminar un capítulo se muestra un aviso con las cartas por responder de ese capítulo y las que quedan en otros capítulos.

---

## Dashboard docente

Desde la cuenta de profesor se abren dos paneles con las mismas pestañas (Resumen, Cap.1 a Cap.4 y HSE):

- **Panel individual del alumno:** estado de cada capítulo, cartas recogidas y respondidas, decisiones en los diálogos, relación con cada personaje, la reflexión escrita y las HSE vinculadas a cada carta respondida, y cuántas reflexiones relacionó con cada una de las 13 HSE.
- **Panel de grupo:** alumnos al día y pendientes, alumnos que completaron cada capítulo, cartas del grupo, relación del grupo con los personajes, las dos HSE más vinculadas a cada carta y cuántos alumnos relacionaron cada HSE.

Las visualizaciones son descriptivas: las barras de HSE cuentan reflexiones o alumnos y no miden el nivel de la habilidad.

---

## Controles

| Acción | Tecla |
|---|---|
| Moverse | `A` / `D` o `←` / `→` |
| Saltar | `Espacio` o `↑` |
| Dash | `Shift` izquierdo |
| Interactuar (hablar, leer una carta, entregar una manzana, terminar el capítulo) | `E` |
| Abrir la mochila | `E` (mantener) |
| Sentarse / abrir la libreta | `S` o `↓` (mantener 1 s) |
| Usar la herramienta equipada | Click izquierdo (mantener para cargar) |
| Trepar con los pinchos | Click izquierdo sostenido + `W` / `S`; `Espacio` para saltar |
| Cancelar la herramienta | Click derecho |
| Desequipar | `Q` |
| Avanzar un diálogo | Click izquierdo |
| Cerrar una carta | `E` o `Espacio` |
| Pausa | `Esc` |

Estas mismas instrucciones están disponibles dentro del juego, en el botón de ayuda del menú principal.

---

## Requisitos

- **Unity 6 (6000.4.1f1)** con Universal Render Pipeline 2D
- Módulo **WebGL Build Support** (para exportar)

## Cómo abrir el proyecto

1. Clonar el repositorio.
2. Abrirlo desde Unity Hub con la versión **6000.4.1f1**.
3. Abrir `Assets/_Scenes/LoginScene` y presionar Play.
4. Para compilar, verificar que en **File → Build Profiles** estén agregadas `LoginScene` y `Cap1` a `Cap4`.

## Configuración técnica

| Parámetro | Valor |
|---|---|
| Plataforma de exportación | WebGL + IL2CPP |
| Resolución | 960 × 540 |
| FPS | 60 (`vSyncCount = 0`) |
| Cámara | Cinemachine, Orthographic Size 2.8125 (escala de píxel 3×) |
| Sprites | Filter Mode Point; personajes 32×32 px, tiles 16×16 px |
| Persistencia | Local por alumno vía `PlayerPrefs` |

## Estructura del proyecto

```
Assets/
├── _Scenes/            LoginScene, Cap1 … Cap4
└── Scripts/
    ├── Player/         Control de Theo
    ├── Mechanics/      Cartas, árbol de decisiones, herramientas, inventario,
    │                   regulación emocional, checkpoints, peligros, capítulos
    ├── Systems/        Cuentas de profesor y alumno, progreso, ajustes del curso
    ├── UI/             Login y dashboard docente, diálogos, lector de cartas,
    │                   compendio, pausa, barras de estado, inventario
    ├── Managers/       Gestores globales
    └── Shared/         Utilidades compartidas
```

---

## Cuentas

El juego tiene dos roles:

- **Profesor:** crea y elimina cuentas de alumno, habilita capítulos para todo el grupo y consulta el dashboard: avance de cada alumno, relaciones con los personajes, cartas recolectadas y respondidas, habilidades que relacionó con sus cartas, y una vista agregada del grupo.
- **Alumno:** juega los capítulos habilitados, responde las cartas que recolecta y consulta su compendio de cartas y personajes.

> La cuenta de profesor es fija y los datos se guardan localmente en el navegador. Es adecuado para el uso en aula previsto en la tesis, no es un sistema de autenticación seguro.

## Limitaciones y consideraciones de implementación

- **Persistencia local.** Todo el progreso (decisiones, cartas, reflexiones y habilidades vinculadas) se guarda con `PlayerPrefs`, que en WebGL vive en el navegador del dispositivo. No se comparte entre dispositivos ni navegadores y se borra si se limpian los datos del sitio. Por eso el dashboard docente solo muestra a los alumnos que jugaron en el mismo navegador. El juego no envía datos a ningún servidor.
- **Autenticación.** La cuenta de profesor es fija y las contraseñas no están protegidas. Es adecuado para el uso en aula previsto en la tesis, no para un despliegue abierto.
- **Alcance del prototipo.** Es un prototipo demo: la versión completa y pulida del videojuego queda fuera del alcance de la tesis. La prueba de usabilidad con un estudiante real es una verificación inicial del flujo de experiencia, no una validación representativa.
- **Alcance de las visualizaciones.** Las habilidades socioemocionales (HSE) son un constructo subjetivo y la propuesta no incluye un instrumento validado para medir su nivel. El dashboard describe lo que el alumno hizo o eligió; **no mide el desarrollo de las HSE**. Está pensado como apoyo para conversar con el alumno y planificar la tutoría.
- **Datos personales y consentimiento.** Capa Alta está dirigido a estudiantes menores de edad, y sus reflexiones y HSE vinculadas son datos emocionales, considerados sensibles por el Reglamento de la Ley N.° 29733 (D. S. N.° 016-2024-JUS). El sistema no gestiona el consentimiento: **la institución educativa es responsable de obtener la autorización de los padres o apoderados antes de implementar el juego con sus estudiantes**. Se recomienda crear las cuentas con alias o códigos en lugar de nombres completos.

## Trabajos futuros

- Almacenamiento en un servidor con autenticación por institución, para que el profesor vea a todos sus alumnos desde cualquier dispositivo. Incluye un plan de instalación y despliegue: hosting del WebGL, una API y una base de datos.
- Prueba piloto con grupos completos de 6.° de primaria para evaluar el efecto sobre el involucramiento y las HSE.
- Visualizaciones con evolución por bimestre, comparación entre capítulos y alumnos, filtros por habilidad y exportación de reportes.
- Versión final del videojuego: narrativa completa, ilustración y audio para todas las cartas, y cierre de la historia.
- Adaptación a otros grados y ciclos, y a dispositivos móviles y equipos de bajo rendimiento.

## Créditos y licencias

- Habilidades socioemocionales: *Orientaciones para el desarrollo del Programa de Habilidades Socioemocionales – V ciclo* (Ministerio de Educación del Perú, 2022).
- Arte (sprites, escenarios, interfaz y cartas): creado por el autor del proyecto.

### Audio

Todos los audios provienen de [OpenGameArt.org](https://opengameart.org). Los autores indicados no exigen atribución; se listan como cortesía.

| Uso | Recurso | Autor | Licencia |
|---|---|---|---|
| Balbuceos | [huh?](https://opengameart.org/content/huh) | TinyWorlds | CC0 |
| Salto y dash | [Slime jump effect](https://opengameart.org/content/slime-jump-effect) | KobatoGames | CC0 |
| Pisadas | [42 Snow and Gravel Footsteps](https://opengameart.org/content/42-snow-and-gravel-footsteps) | Corsica_S (publicado por qubodup) | CC0 |
| Botones | [8bit Menu Highlight](https://opengameart.org/content/8bit-menu-highlight) | Fupi | CC0 |
| Música del menú | [Rpg Vipraphone And Piano](https://opengameart.org/content/rpg-vipraphone-and-piano) | Tozan | CC0 |
| Música de los capítulos | [peaceful theme](https://opengameart.org/content/peaceful-theme) | pebonius | CC0 |