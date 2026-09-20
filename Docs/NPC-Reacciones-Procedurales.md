# Reacciones del robot y visual del mando XRI

## Configuración preparada

El prefab `Assets/Prefabs/TutorialNPC/InstructorNPC.prefab` usa reacciones por código. No necesitas crear clips, estados ni transiciones nuevas en el Animator.

En `NPCReactionController`:

- `Use Procedural Reactions`: activado.
- `Procedural Motion`: referencia al componente `NPCProceduralIdle` del modelo del robot, ya asignada.
- `Completed Clip` y `Alert Clip`: opcionales. No hace falta asignar un AudioSource aquí; la voz se coordina con `NPCVoiceController`.

En `NPCProceduralIdle`:

- `Floating Root`: raíz visual del robot, ya asignada. No usar el objeto que controla los waypoints ni el panel de diálogo.
- `Happy Duration`: 1.6 segundos.
- `Bounce Height`: 0.18 unidades locales.
- `Sad Duration`: 1.8 segundos.
- `Shake Angle`: 18 grados.
- `Look Down Angle`: 25 grados.

Felicidad combina un giro de 360 grados con dos rebotes, el segundo más pequeño. Tristeza inclina el cuerpo hacia abajo y oscila de lado a lado como un gesto de negación. Al terminar o cancelar, se restaura la posición y rotación originales. Durante la reacción se suspende el reposo procedural y se desactiva temporalmente el Animator para que no sobrescriba la pose; luego recupera su estado de activación anterior.

El tutorial espera a que terminen la reacción y el audio antes del siguiente paso. Si se desea volver a clips en el futuro, desactivar `Use Procedural Reactions` y configurar los estados y triggers `Feliz` y `Triste` del Animator.

## Right Controller Visual

En `Assets/Prefabs/Player/XRI/PlayerVR_XRI_Hands.prefab`, abrir `Systems/ToolManager`. El campo visible `Right Controller Visual` acepta el Transform del objeto `Camera Offset/Right Controller/Right Controller Visual` (arrastrar el GameObject también permite resolver su Transform).

La referencia queda asignada al visual heredado del rig XRI. Solo se oculta ese modelo al equipar una herramienta; no se desactiva `Right Controller`, su seguimiento ni sus interactores. La referencia Meta antigua se conserva oculta para otros rigs. Si no hay referencia explícita, el script busca el visual por su nombre dentro del rig.

## Por qué no arrancaban los tutoriales

La referencia serializada vacía `audioSource` se utilizaba con `?.Stop()`. Unity puede representar un objeto vacío con una referencia administrada que no es null para C#, aunque `objeto == null` sí sea verdadero para Unity. Por eso `?.` no evitaba `UnassignedReferenceException`.

La llamada estaba tanto en `CancelReactions` como en la configuración que se ejecuta antes de `StartTutorial`. Por tanto, podía impedir el arranque, además de fallar al desactivar el componente. Ahora se comprueba explícitamente la referencia con `!= null`. Las referencias del director y de voz que participan en esta inicialización y cancelación utilizan también la comprobación de Unity.

Los tutoriales de las escenas Modulo1 y Modulo2 están habilitados y tienen GameData asignado. Módulo 1 espera ahora un fotograma antes de configurarse para permitir que el flujo inicie sus objetivos, como ya hacía módulo 2.

## Verificación

Compilación de Assembly-CSharp correcta. Pruebas NPCPresentation: 37 comprobaciones, incluyendo una referencia de AudioSource que simula el null especial de Unity y las reacciones procedurales reales con dobles de componentes. Module02Wiring: 621 comprobaciones correctas.

Falta validar visualmente en Play Mode/visor la altura del rebote, el sentido de la inclinación del modelo importado y el ocultamiento del mando al equipar herramientas. Ajustar los campos anteriores si el tamaño u orientación de otro modelo requiere valores distintos.
