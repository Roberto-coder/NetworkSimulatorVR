# Sprint 8 — NPC y waypoints

## Instalar en la escena

Abrir `Assets/Scenes/Modulo2.unity` fuera de Play y ejecutar:

`Network Simulator > Module 02 > Infraestructura > Sprint 8 - NPC y waypoints`

El configurador requiere el flujo del sprint 7 y una MainCamera activa. Instancia
`Assets/Prefabs/TutorialNPC/InstructorNPC.prefab` o reutiliza el instructor ya presente
si tiene un único director compatible. No modifica el prefab original.

Genera `WaypointsController` con `Waypoints_Creator`, igual que el módulo 1, y un
`Waypoint Holder` con tres puntos:

- `waypoint_inicio`: aparición del instructor al comenzar Play.
- `waypoint_rack`: destino para explicar y acompañar los nueve objetivos.
- `waypoint_quiz`: reservado para el próximo sprint; todavía no se recorre.

Revisar posiciones, altura y la ruta libre entre inicio y rack, y guardar la escena.
El follower existente mueve al NPC por un segmento recto: no utiliza NavMesh ni evita
obstáculos. Las posiciones generadas son una aproximación al costado del rack.
Mover `waypoint_inicio` actualiza el inicio en el siguiente Play; no hace falta mover
también el NPC. El menú no duplica ni reposiciona objetos si ya existe un
`Module02TutorialController`; para ajustar después, usar el Inspector.

## Referencias y comportamiento

`Module02TutorialController` conecta Manager, TutorialDirector, Start Waypoint,
Rack Waypoint, Reaction Controller y el asset de guion. El prefab ya aporta diálogo,
animación y movimiento. Su Canvas de diálogo se orienta hacia MainCamera.

`Module02NpcInput` confirma con B (`RightHand/secondaryButton`); en Editor o
Development Build también acepta Enter. La primera pulsación completa la escritura;
la siguiente cierra el diálogo. Se desactiva la lectura OVR del diálogo solo en esta
instancia para evitar doble confirmación. El módulo 1 conserva su entrada original.

El NPC mira al jugador durante la introducción, sigue la dirección de movimiento
hasta el rack y vuelve a mirar al jugador al llegar. Para cada objetivo muestra una
explicación y una instrucción. Cerrar la instrucción no completa el objetivo: espera
el estado real del flujo. Los objetivos completados se omiten, incluso si se
completaron durante una explicación o mediante Skip Rack Inspection.

Las herramientas, spawners y objetivos de muñeca mantienen su flujo existente; la
narración no bloquea el progreso físico. Desactivar el tutorial cancela narración y
movimiento, sin marcar objetivos. Reiniciar Play para iniciar otra sesión completa.

`NPCReactionController` ahora observa IObjectiveFlow. Conserva el validador de errores
del módulo 1 y recibe los rechazos del coordinador del módulo 2. Los recordatorios
usan ReminderDialogue (o Description) e intervalo del ObjectiveData; no interrumpen
un diálogo activo. El configurador conecta Feliz/Triste solo si esos triggers existen
en el Animator y no se había elegido otra reacción. No se generan grabaciones de voz.

El director emite TutorialCompleted al terminar naturalmente. Cancelar el tutorial
no emite completitud. El cierre actual termina en el rack y no anuncia un quiz disponible.

## Guion editable

Todos los textos de este sprint están en
`Assets/GameData/Module02/Module02Tutorial.asset` (ScriptableObject): Introduction,
Completion y nueve entradas identificadas por objectiveId, con Explanation e Instruction.

`Module02TutorialBuilder` ordena los pasos; `Module02GuidedTutorialStep` consulta el
progreso antes y durante la explicación. Si falta una entrada o su instrucción,
utiliza Description del ObjectiveData como instrucción.

El guion completo está en [Module02-Sprint08-Guion.md](Module02-Sprint08-Guion.md).

## Comprobación

Runtime y Editor compilan con Roslyn de Unity 6000.2.7f2, con advertencias TMP
preexistentes. Pasan 606 aserciones: las 598 anteriores y ocho verificaciones nuevas
de corutinas reales del tutorial, usando dobles de Unity/UI fuera del Editor.
Comprueban omisión de objetivos completados, avance físico durante explicación,
confirmación sin completar y liberación de suscripciones al completar o cancelar.

No se ejecutó el menú ni Play/VR en esta sesión. Comprobación manual pendiente:

1. Ejecutar el configurador y guardar; repetirlo no duplica NPC ni waypoints.
2. Inicio normal: NPC en inicio, diálogo visible, B completa texto y luego avanza.
3. Llegada al rack: mira al jugador; revisar ruta y altura sin atravesar objetos.
4. Cerrar instrucciones: objetivo sigue pendiente hasta realizar la interacción.
5. Activar Skip Rack Inspection: omite la explicación del primer objetivo.
6. Completar objetivos con diálogo abierto: la narración no queda bloqueada en uno anterior.
7. Probar un rechazo de encendido y su recordatorio; comprobar herramientas y muñeca.
8. Completar puzzle: mensaje de cierre, sin activar un quiz ni conceder insignia.
9. Desactivar el tutorial en movimiento: se detiene sin completar objetivos.

La integración del quiz, resultados, insignia y guardado continúa en el siguiente sprint.
