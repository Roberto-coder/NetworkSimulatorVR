# Sprint 9 — Quiz, insignia y cierre del módulo

## Configuración en Unity

Con `Assets/Scenes/Modulo2.unity` abierta, fuera de Play y con el sprint 8 configurado:

`Network Simulator > Module 02 > Infraestructura > Sprint 9 - Quiz y cierre`

El menú crea dos copias independientes a partir de los prefabs existentes:

- `Assets/Prefabs/UI_Components/Quiz/Module02/QuizCanvas_Module02_XRI.prefab`
- `Assets/Prefabs/UI_Components/Quiz/Module02/AnswerOption_Module02_XRI.prefab`

Conservan el diseño del quiz original. Se eliminan sus adaptadores ISDK y se utiliza
TrackedDeviceGraphicRaycaster con el XRUIInputModule que ya tiene Modulo2. La copia
del quiz referencia la copia de respuestas; los originales del módulo 1 no se modifican.
Los prefabs se generan al ejecutar el menú, no antes. Si existen, se reutilizan y
validan sin sobrescribir ajustes visuales posteriores.

También se crean `Module02_Finale` y `QuizSpawner_Module02`. Ajustar la posición y
orientación del spawner para que el panel resulte accesible y quede a un costado del
NPC. Revisar el recorrido entre `waypoint_rack` y `waypoint_quiz`: el follower recorre
segmentos rectos y no evita obstáculos. Guardar la escena después del ajuste.

El menú no duplica el cierre si ya existe. `Modulo2` y `Lobby` deben estar habilitadas
en Build Settings para navegar; Modulo2 estaba deshabilitada y quedó habilitada.

## Datos

Creado `Assets/GameData/Quiz/Module02Quiz.asset`, de tipo QuizData, con 12 preguntas,
cuatro opciones por pregunta y un umbral de aprobación del 70 %. Se conectó a
FinalQuiz de `Assets/GameData/Modules/Module02_RackInstallation.asset`.

Temas: rack, fijación, cinco cables, alimentación, firewall, diez etiquetas,
invalidación de etiquetas, consola, encendido, puzzle, LED/paquetes y cable de consola.
Las preguntas y respuestas se editan en el Inspector del asset.

Se reutiliza `Assets/GameData/Achievements/Module02Completion.asset`, incluida su
imagen: **Especialista de rack y switch**. No se genera otra insignia duplicada.

## Flujo y tutorial

Los nueve objetivos físicos de muñeca conservan su orden. `Module02FlowController`
emite PracticalObjectivesCompleted al terminar el puzzle; IsCompleted permanece
en false hasta entregar el quiz y entonces emite ModuleCompleted.

`Module02FinaleController` recibe la completitud práctica y espera el cierre del
tutorial opcional. El Builder añade el recorrido a waypoint_quiz y la instrucción
final. Al terminar la narración aparece una sola instancia del quiz. Si el tutorial
está desactivado, el quiz aparece directamente al terminar la práctica.

El guion de cierre está en `Assets/GameData/Module02/Module02Tutorial.asset`:

**Antes de moverse:** «Has completado la instalación y configuración del switch.
Ahora acompáñame para realizar el quiz final y repasar lo aprendido.»

**Al llegar:** «Responde todas las preguntas y entrega el quiz. Verás tu puntuación
y recibirás la insignia por completar el módulo. El progreso se guarda en este
dispositivo; después podrás repetir el quiz, reiniciar el módulo o volver al lobby.»

## Resultado y guardado

El usuario debe responder todas las preguntas. Puede revisar y modificar respuestas
antes de entregar. Una entrega repetida por doble clic se ignora.

Como en el módulo 1, cualquier puntuación entregada completa el módulo. El panel
muestra aciertos, porcentaje y aprobación; la insignia reconoce la completitud.

`Module02QuizCompletionController` guarda automáticamente la primera entrega de la
sesión en el slot local activo mediante SaveManager: módulo completado, nueve
objetivos, tiempo desde el inicio de la escena e insignia. Como en el esquema
existente, la puntuación del intento se presenta en UI; no se añadió historial de
notas al archivo. Repetir el quiz no vuelve a sumar el tiempo de la misma sesión.

El guardado escribe un temporal y reemplaza el archivo al terminar. Si ocurre un
error de acceso o escritura, se restaura la copia en memoria, se muestra un aviso
en resultados y aparece **Reintentar guardado**. La insignia se muestra después del
guardado exitoso. Reinicio y salida permanecen bloqueados mientras haya un fallo.

Desde el lobby se reutiliza SaveManager. Al probar Modulo2 directamente se crea si
falta, siguiendo la selección de slot del sistema existente (slot 0 si no hay uno
seleccionado). Las pruebas automatizadas no acceden a partidas reales.

Después del guardado:

- **Repetir quiz:** otro intento sin volver a instalar el switch.
- **Reiniciar módulo:** recarga Modulo2, reinicia la práctica y el NPC; conserva la
  completitud y la insignia guardadas. El flag de depuración Skip Rack Inspection
  sigue respetándose si está activado en la escena.
- **Volver al lobby:** usa SceneTransitionManager y conserva el progreso.

## Verificación

Compilación satisfactoria de runtime y Editor con Roslyn de Unity 6000.2.7f2;
advertencias TMP preexistentes. Pasan 621 aserciones de dominio, secuencia, tutorial,
quiz, completitud y escritura local. Se comprobaron entrega incompleta, cambio de
respuestas, puntuación baja/alta, reinicio de intento y reemplazo de un archivo
temporal propio sin alterar un archivo válido ante un fallo.

No se ejecutaron el configurador ni Play/VR en esta sesión. Verificar en Unity:

1. Ejecutar el menú, ajustar el spawner, guardar; repetir el menú no duplica objetos.
2. Terminar el puzzle: completitud práctica en muñeca y NPC hacia el quiz.
3. Confirmar el cierre con B; usar el ray XRI para seleccionar respuestas y botones.
4. Respuestas incompletas no permiten entregar; anterior conserva las respuestas.
5. Entregar: resultado, insignia y mensaje de guardado local.
6. Repetir el quiz: respuestas vacías; no repite el tiempo guardado de la sesión.
7. Reiniciar módulo: objetos y objetivos desde el inicio, guardado anterior conservado.
8. Volver al lobby: comprobar módulo e insignia en el perfil.
9. Con tutorial desactivado, el quiz sigue apareciendo al terminar la práctica.

La interacción visual completa, el recorrido y el guardado de una partida real
requieren esa prueba en Unity/VR; la compilación y las pruebas de reglas no la sustituyen.
