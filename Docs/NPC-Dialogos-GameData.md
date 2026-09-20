# Diálogos del NPC

Cada escena disponible tiene asignado su GameData en el campo `data` del controlador del tutorial. El asset del módulo 3 está vacío y preparado para cuando se implemente su tutorial.

| Módulo | Script que inicia el tutorial | GameData |
| --- | --- | --- |
| Lobby | `Assets/Scripts/Modules/Lobby/Presentation/Tutorial/LobbyTutorialController.cs` | `Assets/GameData/Lobby/LobbyTutorial.asset` |
| 1 | `Assets/Scripts/Modules/Module01_CableMaking/Presentation/Tutorial/Module01TutorialController.cs` | `Assets/GameData/Module01/Module01Tutorial.asset` |
| 2 | `Assets/Scripts/Modules/Module02_RackInstallation/Presentation/Tutorial/Module02TutorialController.cs` | `Assets/GameData/Module02/Module02Tutorial.asset` |
| 3 | Pendiente de implementación | `Assets/GameData/Module03/Module03Tutorial.asset` |

Los archivos `LobbyTutorialBuilder.cs`, `Module01TutorialBuilder.cs` y `Module02TutorialBuilder.cs`, en las mismas carpetas de sus controladores, conservan el orden de los pasos y los movimientos. Los textos se editan en el asset. No cambiar los identificadores de entradas existentes: los builders y las reacciones los utilizan para localizar cada diálogo.

## Audio desde el Inspector

- `clip`: arrastrar un AudioClip importado en Unity.
- `resourcesPath`: alternativa para escribir una ruta relativa a una carpeta `Resources`, sin extensión. Por ejemplo, `Assets/Resources/Audio/Lobby/Intro.wav` se referencia como `Audio/Lobby/Intro`.
- El clip directo tiene prioridad. Si no se resuelve ninguno, el tutorial sigue mostrando texto. El lobby conserva además sus identificadores del catálogo de voces anterior como respaldo y su clip de introducción ya asignado.
- En módulo 2, cada explicación e instrucción tiene su propio campo `explanationAudio` / `instructionAudio`; la introducción, conclusión e instrucción del quiz también tienen campos separados. Se conserva el esquema de texto original para mantener compatibilidad con las herramientas de configuración del módulo.

## Recordatorios y avisos

En módulo 1 están en la lista `dialogues`; en módulo 2, en `reactions`. Los IDs `reminder_...` corresponden a objetivos y los IDs `alert_...` a avisos de interacción. Los reproduce `Assets/Scripts/Presentacion/NPC/NPCReactionController.cs`.

Los avisos del módulo 1 admiten `{objective}` y `{end}` como sustituciones de texto. Un audio grabado no sustituye esas palabras automáticamente: utilizar una grabación adecuada al aviso. Los mensajes originales de validación y los textos de ObjectiveData se conservan como respaldo para usos ajenos a estos tutoriales.

La presentación compartida usa `Assets/Scripts/Presentacion/Tutorial/DialogStep.cs`, `GuidedObjectiveStep.cs` y `Assets/Scripts/Presentacion/NPC/NPCDialogueController.cs`; la voz de los pasos se reproduce con `NPCVoiceController.cs`. Al confirmar o completar un objetivo se detiene su narración.

## Espera de reacciones y audio

El director espera las reacciones pendientes antes de iniciar cada paso y antes de finalizar el tutorial. Los pasos guiados también esperan antes de una nueva instrucción. La felicidad al completar un objetivo cierra primero el diálogo anterior; si ya se está ejecutando una alerta, espera su finalización.

Las reacciones usan los estados `Feliz` y `Triste` de la capa 0. Se espera la entrada al estado y su salida completa, incluidas las transiciones. El Animator vuelve a Idle al final del clip. Los campos `animationStartTimeout` y `animationTimeout` evitan bloquear el tutorial si un estado no inicia o no sale; por defecto son 1 y 30 segundos de juego.

Diálogos y reacciones comparten `NPCVoiceController`. Confirmar, saltar la escritura, ocultar o reemplazar un diálogo corta su voz. La limpieza de una sesión anterior no detiene el audio de una sesión nueva. Un mensaje transitorio espera a que termine su audio si dura más que el tiempo visible; puede saltarse por confirmación. Saltar su texto no recorta la animación de una reacción.

## Validación

- Compilación de Assembly-CSharp con las referencias generadas por Unity: correcta; únicamente dos advertencias existentes de TextMesh Pro obsoleto.
- Pruebas Module02Wiring: 621 comprobaciones correctas.
- Pruebas NPCPresentation: 30 comprobaciones de audio, reacciones y secuencia con reloj y componentes de Unity simulados.
- Verificación de textos originales del lobby y módulo 1, IDs únicos y referencias de GameData en escenas: correcta.
- Pendiente comprobar la reproducción audible en Play Mode/visor con los clips definitivos.
