# Guía visual para el README y LinkedIn

## Dónde guardar las capturas

Usa `Docs/Imagenes/README/`. El README incluye comentarios HTML con la línea
Markdown lista para cada imagen. Después de guardar la captura:

1. Busca `IMAGEN 01`, `IMAGEN 02`, etc. en el código del README.
2. Copia la línea `![...](...)` fuera del comentario HTML.
3. Retira el aviso visible `📷` correspondiente.

No hay enlaces activos a imágenes inexistentes. Los comentarios no se muestran
en la vista renderizada de GitHub.

| Archivo | Posición | Qué capturar |
|---|---|---|
| `01-portada-lobby.png` | Bajo la introducción | Plano amplio del Lobby desde el jugador, instructor y panel de módulos. |
| `02-fabricacion-cable.png` | Tras la tabla de módulos | Manos y herramienta actuando sobre el cable; que se entienda la acción. |
| `03-instalacion-rack.png` | Después del módulo 1 | Switch montado, cableado y etiquetas. |
| `04-diagnostico-red.png` | Después del módulo 2 | Laptop con un resultado real de prueba y red del escenario. |
| `05-instructor-controles.png` | Tras las funciones de acompañamiento | Robot y diálogo breve con sprite de botón legible. |

## Cómo conseguir capturas claras

- Usa Game View o una captura de un solo ojo del visor; evita la vista estereoscópica doble.
- Para el README, mantén un formato horizontal uniforme; 1920 × 1080 es una buena base de trabajo.
- Oculta gizmos, consola e Inspector: la imagen principal debe mostrar la experiencia del usuario.
- Evita paneles cortados y texto muy pequeño. Acércate al elemento que quieres explicar.
- No muestres correos, contraseñas, tokens ni datos personales de cuentas de prueba.
- Captura el estado real del proyecto. No presentes renders o montajes como gameplay.

## Propuesta de video corto

Un video de 30–45 segundos puede mostrar el recorrido sin enseñar todas las pantallas:

1. Lobby e instructor (5 s).
2. Preparación del cable (8 s).
3. Instalación o conexión en rack (8 s).
4. Prueba de conectividad y reparación (10 s).
5. Cierre con el nombre del proyecto y el enlace al repositorio (4 s).

Añade subtítulos breves para que se entienda sin audio. Si una función aún no está
validada, preséntala como trabajo en desarrollo.

## Texto base para LinkedIn

Estoy trabajando en NetXR Lab, un proyecto académico de realidad virtual para
practicar redes de computadoras dentro de un laboratorio interactivo.

El recorrido abarca la fabricación de un cable, la instalación de equipos en rack
y el diagnóstico de fallas de conectividad. Un instructor virtual acompaña las
actividades con diálogos, audio e indicaciones visuales.

El proyecto utiliza Unity, C#, OpenXR, XR Interaction Toolkit, Meta XR y Firebase.
Uno de los retos ha sido coordinar la interacción VR con las secuencias del tutorial
y mantener el progreso local con sincronización manual a la nube.

Actualmente seguimos integrando y validando la experiencia en visor. Comparto un
avance del recorrido y algunas decisiones técnicas del desarrollo.

[Agregar enlace al repositorio o demo]
[Agregar nombres del equipo y el aporte concreto de cada integrante, si corresponde]

¿Qué actividad de redes te gustaría practicar en un entorno como este?

#RealidadVirtual #Unity #RedesDeComputadoras #DesarrolloDeSoftware #TecnologiaEducativa

Antes de publicar, adapta “estoy trabajando” a “estamos desarrollando” si publicas
como equipo e identifica tu contribución real. No atribuyas a una sola persona
el trabajo colectivo ni incluyas resultados de aprendizaje que no se hayan medido.
