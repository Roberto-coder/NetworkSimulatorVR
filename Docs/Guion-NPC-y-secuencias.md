# Guion del NPC y secuencias de los tutoriales

Network Simulator VR · Lobby y módulos 1 a 3 · 15 de septiembre de 2026

Guion de referencia para el equipo de contenido, audio y desarrollo. Presenta el texto del Instructor y las acciones del tutorial en su orden de ejecución, con los identificadores que permiten localizar cada intervención en GameData. Los pasos de movimiento, mirada y espera son acotaciones y no se locutan.

## Cómo leer el guion

Los pasos numerados corresponden a la secuencia principal. Las condiciones junto a cada paso indican cuándo puede omitirse o qué necesita para terminar. Los recordatorios y las alertas aparecen al final de cada módulo, porque se activan por eventos durante la práctica y no forman una secuencia fija.

- **Diálogo informativo:** explica un concepto, una función o el contexto de la actividad.

- **Diálogo de instrucciones:** indica una acción o el uso de controles.

- **Diálogo de bienvenida:** inicia el encuentro con el Instructor.

- **Diálogo de transición:** conecta dos zonas o fases de la experiencia.

- **Diálogo de cierre:** despide al usuario o cierra una fase.

- **Diálogo de recordatorio:** recuerda el objetivo durante la práctica.

- **Diálogo de alerta:** responde a una interacción rechazada.

- **Acción del NPC:** movimiento, cambio de mirada o colocación inicial.

- **Espera de sistema:** pausa temporal o espera de una condición.

Las categorías son indicadores editoriales para el equipo; no cambian la lógica del juego. Cada texto se conserva tal como está en su GameData. Un mismo diálogo puede combinar información e instrucciones, y se mantiene como una sola intervención para respetar su referencia de audio.

**Controles de avance:** el guion del lobby y del módulo 2 indica B. La confirmación solo avanza la narración; no completa las tareas prácticas. La duración del audio no determina por sí sola el avance.

## Lobby Recorrido de bienvenida

**Escena:** `Assets/Scenes/Lobby.unity`.

**Guion editable:** `Assets/GameData/Lobby/LobbyTutorial.asset`.

**Controlador:** `Assets/Scripts/Modules/Lobby/Presentation/Tutorial/LobbyTutorialController.cs`.

**Construcción de pasos:** `Assets/Scripts/Modules/Lobby/Presentation/Tutorial/LobbyTutorialBuilder.cs`.

### Inicio y condiciones generales

El flujo del lobby comienza y se configuran las herramientas. Si el tutorial ya está guardado como completado y `forceTutorial` no está activado, se omite el recorrido y se completa su objetivo. En otro caso se ejecuta la secuencia siguiente. La opción de repetir tutorial vuelve a solicitar su ejecución; no inicia una segunda secuencia si ya hay una en marcha.

Se requieren el director y los waypoints del panel principal, museo y final, además del GameData.

### Paso 01 Mirada

**Acción del NPC:** orientar la mirada al jugador. **Step:** `LookAtStep` con modo `Player`. Cambia el modo de mirada y cede un fotograma; no espera un tiempo de animación.

### Paso 02 Diálogo de bienvenida e instrucciones

**Diálogo de bienvenida e instrucciones** · Instructor

> Hola, soy tu asistente. Presiona el botón B para avanzar el diálogo.

**Referencia:** `lobby_intro_controls` · `DialogueStep`.

**Avance:** Confirmar el diálogo. Durante la escritura, la primera confirmación completa el texto; la siguiente permite continuar. La voz se detiene al cerrar el diálogo.

### Paso 03 Diálogo informativo

**Diálogo informativo** · Instructor

> Como ingeniero de los laboratorios de red, te acompañaré en tu aprendizaje, comenzando por la capa física del modelo OSI.

**Referencia:** `lobby_intro_role` · `DialogueStep`.

**Avance:** Confirmar el diálogo. Durante la escritura, la primera confirmación completa el texto; la siguiente permite continuar. La voz se detiene al cerrar el diálogo.

### Paso 04 Diálogo de instrucciones

**Diálogo de instrucciones** · Instructor

> Presiona Y para abrir la rueda, apunta a la herramienta que quieras y usa R2 para utilizarla.

**Referencia:** `lobby_tools` · `DialogueStep`.

**Avance:** Confirmar el diálogo. Durante la escritura, la primera confirmación completa el texto; la siguiente permite continuar. La voz se detiene al cerrar el diálogo.

### Paso 05 Diálogo de instrucciones

**Diálogo de instrucciones** · Instructor

> Al girar la muñeca izquierda verás el menú de objetivos de cada módulo. En el Lobby tu único objetivo es completar este tutorial.

**Referencia:** `lobby_objectives` · `DialogueStep`.

**Avance:** Confirmar el diálogo. Durante la escritura, la primera confirmación completa el texto; la siguiente permite continuar. La voz se detiene al cerrar el diálogo.

### Paso 06 Diálogo de transición

**Diálogo de transición** · Instructor

> Sígueme para comenzar el recorrido.

**Referencia:** `lobby_follow_me` · `DialogueStep`.

**Avance:** Confirmar el diálogo. Durante la escritura, la primera confirmación completa el texto; la siguiente permite continuar. La voz se detiene al cerrar el diálogo.

### Paso 07 Mirada

**Acción del NPC:** orientar la mirada en la dirección del movimiento. **Step:** `LookAtStep` con modo `MovementDirection`. Cambia el modo de mirada y cede un fotograma; no espera un tiempo de animación.

### Paso 08 Movimiento

**Acción del NPC:** desplazarse hacia panel principal. Waypoint del controlador: `mainPanelWaypoint`. Objeto asignado en escena: `Waypoint_MainPanel`.

**Step:** `MoveNpcStep`. **Avance:** esperar el evento `DestinationReached`, es decir, la llegada del NPC al destino. No hay un paso adicional que espere a que el jugador llegue.

### Paso 09 Mirada

**Acción del NPC:** orientar la mirada al jugador. **Step:** `LookAtStep` con modo `Player`. Cambia el modo de mirada y cede un fotograma; no espera un tiempo de animación.

### Paso 10 Diálogo de instrucciones

**Diálogo de instrucciones** · Instructor

> En este panel puedes gestionar tu almacenamiento. Guarda tu progreso cada vez que completes un módulo.

**Referencia:** `lobby_storage` · `DialogueStep`.

**Avance:** Confirmar el diálogo. Durante la escritura, la primera confirmación completa el texto; la siguiente permite continuar. La voz se detiene al cerrar el diálogo.

### Paso 11 Diálogo informativo

**Diálogo informativo** · Instructor

> También puedes consultar tu perfil, insignias y módulos completados.

**Referencia:** `lobby_profile` · `DialogueStep`.

**Avance:** Confirmar el diálogo. Durante la escritura, la primera confirmación completa el texto; la siguiente permite continuar. La voz se detiene al cerrar el diálogo.

### Paso 12 Diálogo informativo

**Diálogo informativo** · Instructor

> Desde aquí podrás desbloquear módulos nuevos o repetir los que ya completaste.

**Referencia:** `lobby_modules` · `DialogueStep`.

**Avance:** Confirmar el diálogo. Durante la escritura, la primera confirmación completa el texto; la siguiente permite continuar. La voz se detiene al cerrar el diálogo.

### Paso 13 Mirada

**Acción del NPC:** orientar la mirada en la dirección del movimiento. **Step:** `LookAtStep` con modo `MovementDirection`. Cambia el modo de mirada y cede un fotograma; no espera un tiempo de animación.

### Paso 14 Movimiento

**Acción del NPC:** desplazarse hacia museo. Waypoint del controlador: `museumAreaWaypoint`. Objeto asignado en escena: `Waypoint_MuseumArea`.

**Step:** `MoveNpcStep`. **Avance:** esperar el evento `DestinationReached`, es decir, la llegada del NPC al destino. No hay un paso adicional que espere a que el jugador llegue.

### Paso 15 Mirada

**Acción del NPC:** orientar la mirada al jugador. **Step:** `LookAtStep` con modo `Player`. Cambia el modo de mirada y cede un fotograma; no espera un tiempo de animación.

### Paso 16 Diálogo informativo

**Diálogo informativo** · Instructor

> En el museo, los dispositivos con contorno amarillo pueden manipularse para conocer su información y características.

**Referencia:** `lobby_museum_devices` · `DialogueStep`.

**Avance:** Confirmar el diálogo. Durante la escritura, la primera confirmación completa el texto; la siguiente permite continuar. La voz se detiene al cerrar el diálogo.

### Paso 17 Diálogo de instrucciones

**Diálogo de instrucciones** · Instructor

> Puedes interactuar con ellos usando el trigger de los controles o mediante gestos de hand tracking.

**Referencia:** `lobby_museum_controls` · `DialogueStep`.

**Avance:** Confirmar el diálogo. Durante la escritura, la primera confirmación completa el texto; la siguiente permite continuar. La voz se detiene al cerrar el diálogo.

### Paso 18 Diálogo de cierre

**Diálogo de cierre** · Instructor

> Buena suerte. Volveré a acompañarte más adelante en otros módulos.

**Referencia:** `lobby_farewell` · `DialogueStep`.

**Avance:** Confirmar el diálogo. Durante la escritura, la primera confirmación completa el texto; la siguiente permite continuar. La voz se detiene al cerrar el diálogo.

### Paso 19 Mirada

**Acción del NPC:** orientar la mirada en la dirección del movimiento. **Step:** `LookAtStep` con modo `MovementDirection`. Cambia el modo de mirada y cede un fotograma; no espera un tiempo de animación.

### Paso 20 Movimiento

**Acción del NPC:** desplazarse hacia posición final. Waypoint del controlador: `finalWaypoint`. Objeto asignado en escena: `Waypoint_Final`.

**Step:** `MoveNpcStep`. **Avance:** esperar el evento `DestinationReached`, es decir, la llegada del NPC al destino. No hay un paso adicional que espere a que el jugador llegue.

### Paso 21 Mirada

**Acción del NPC:** orientar la mirada al jugador. **Step:** `LookAtStep` con modo `Player`. Cambia el modo de mirada y cede un fotograma; no espera un tiempo de animación.

### Fin de la secuencia principal

El director emite `TutorialCompleted`. El lobby completa el objetivo del tutorial y lo registra en el guardado local. El último cambio de mirada deja al NPC mirando al jugador desde la posición final.

Este builder no programa recordatorios ni alertas de práctica.

## Módulo 1 Preparación de un cable de red

**GameData:** `NetworkSimulatorVR/Assets/GameData/Module01/Module01Tutorial.asset`.

**Controlador:** `NetworkSimulatorVR/Assets/Scripts/Modules/Module01_CableMaking/Presentation/Tutorial/Module01TutorialController.cs`.

**Construcción de pasos:** `NetworkSimulatorVR/Assets/Scripts/Modules/Module01_CableMaking/Presentation/Tutorial/Module01TutorialBuilder.cs`.

### Paso 01 Espera

**Espera:** 5 segundos.

### Paso 02 Diálogo de bienvenida

**Diálogo de bienvenida**

**Instructor:**

> Bienvenido al laboratorio.

### Paso 03 Diálogo informativo

**Diálogo informativo**

**Instructor:**

> En este módulo aprenderás a preparar, armar y comprobar un cable de red.

### Paso 04 Movimiento

**Acción del NPC:** desplazarse hacia la zona del cable. Waypoint del controlador: `cableWaypoint`.

**Avance:** esperar a que el NPC llegue al destino.

### Paso 05 Diálogo informativo

**Diálogo informativo**

**Instructor:**

> El pelado consiste en retirar cuidadosamente una parte de la cubierta exterior sin dañar los conductores internos.

### Paso 06 Diálogo de instrucciones

**Diálogo de instrucciones**

**Instructor:**

> Selecciona la peladora, apunta al extremo izquierdo del cable y realiza la acción para retirar la cubierta.

**Espera:** completar este objetivo para continuar.

### Paso 07 Diálogo informativo

**Diálogo informativo**

**Instructor:**

> Ahora debes ordenar los ocho conductores siguiendo la norma T568B, respetando la posición asignada a cada color.

### Paso 08 Diálogo de instrucciones

**Diálogo de instrucciones**

**Instructor:**

> Toma el extremo izquierdo preparado, abre el puzzle y coloca cada conductor en el orden T568B.

**Espera:** completar este objetivo para continuar.

### Paso 09 Diálogo informativo

**Diálogo informativo**

**Instructor:**

> El ponchado fija los conductores dentro del conector RJ45 y crea el contacto eléctrico con sus terminales.

### Paso 10 Diálogo de instrucciones

**Diálogo de instrucciones**

**Instructor:**

> Selecciona la ponchadora y úsala sobre el conector del extremo izquierdo.

**Espera:** completar este objetivo para continuar.

### Paso 11 Diálogo informativo

**Diálogo informativo**

**Instructor:**

> El segundo extremo necesita la misma preparación antes de ordenar sus conductores.

### Paso 12 Diálogo de instrucciones

**Diálogo de instrucciones**

**Instructor:**

> Usa la peladora para retirar la cubierta del extremo derecho.

**Espera:** completar este objetivo para continuar.

### Paso 13 Movimiento

**Acción del NPC:** desplazarse hacia la zona del puzzle. Waypoint del controlador: `puzzleWaypoint`.

**Avance:** esperar a que el NPC llegue al destino.

### Paso 14 Diálogo informativo

**Diálogo informativo**

**Instructor:**

> Repite el orden T568B en el extremo derecho. Ambos conectores deben conservar exactamente la misma distribución.

### Paso 15 Diálogo de instrucciones

**Diálogo de instrucciones**

**Instructor:**

> Ordena los conductores del extremo derecho dentro del puzzle.

**Espera:** completar este objetivo para continuar.

### Paso 16 Diálogo informativo

**Diálogo informativo**

**Instructor:**

> Con los conductores ya ordenados, falta asegurar el segundo conector.

### Paso 17 Diálogo de instrucciones

**Diálogo de instrucciones**

**Instructor:**

> Usa la ponchadora sobre el conector del extremo derecho.

**Espera:** completar este objetivo para continuar.

### Paso 18 Diálogo informativo

**Diálogo informativo**

**Instructor:**

> Para comprobar el cable, primero conecta ambos extremos en los puertos correspondientes del tester.

### Paso 19 Diálogo de instrucciones

**Diálogo de instrucciones**

**Instructor:**

> Selecciona el tester y conecta los dos extremos del cable.

**Espera:** completar este objetivo para continuar.

### Paso 20 Diálogo informativo

**Diálogo informativo**

**Instructor:**

> El tester verificará la continuidad y el orden de los ocho conductores para detectar conexiones incorrectas.

### Paso 21 Diálogo de instrucciones

**Diálogo de instrucciones**

**Instructor:**

> Inicia la prueba y revisa el resultado del cable.

**Espera:** completar este objetivo para continuar.

### Paso 22 Diálogo de cierre de práctica y transición

**Diálogo de cierre de práctica y transición**

**Instructor:**

> Has completado todos los objetivos prácticos. Ahora realizarás un breve quiz para repasar el módulo.

### Paso 23 Movimiento

**Acción del NPC:** desplazarse hacia la zona del quiz. Waypoint del controlador: `quizWaypoint`.

**Avance:** esperar a que el NPC llegue al destino.

### Paso 24 Diálogo de instrucciones del quiz

**Diálogo de instrucciones del quiz**

**Instructor:**

> Responde todas las preguntas y entrega el quiz. Cualquier resultado completará el módulo.

### Fin de la secuencia principal

El director emite `TutorialCompleted`. El flujo solicita el quiz al terminar la práctica. Entregarlo completa el módulo, independientemente de la puntuación.

### Recordatorios durante la práctica

Se muestran durante la práctica para recordar el objetivo actual.

#### Pelar el extremo izquierdo

**Diálogo de recordatorio**

**Instructor:**

> Recuerda usar la peladora sobre el extremo izquierdo del cable.

#### Ordenar el extremo izquierdo

**Diálogo de recordatorio**

**Instructor:**

> Ordena los conductores del extremo izquierdo siguiendo la secuencia T568B.

#### Ponchar el extremo izquierdo

**Diálogo de recordatorio**

**Instructor:**

> Usa la ponchadora para fijar el conector RJ45 del extremo izquierdo.

#### Pelar el extremo derecho

**Diálogo de recordatorio**

**Instructor:**

> Retira la cubierta del extremo derecho con la peladora.

#### Ordenar el extremo derecho

**Diálogo de recordatorio**

**Instructor:**

> Repite la secuencia T568B en los conductores del extremo derecho.

#### Ponchar el extremo derecho

**Diálogo de recordatorio**

**Instructor:**

> Usa la ponchadora para fijar el conector RJ45 del extremo derecho.

#### Conectar el tester

**Diálogo de recordatorio**

**Instructor:**

> Conecta ambos extremos RJ45 en los puertos correspondientes del tester.

#### Comprobar el cable

**Diálogo de recordatorio**

**Instructor:**

> Inicia la prueba del tester para validar la continuidad y el orden del cable.

### Alertas de interacción

Se muestran cuando una interacción es incorrecta.

**Disparador:** La herramienta o acción no corresponde al objetivo actual.

**Diálogo de alerta**

**Instructor:**

> Esa herramienta o acción no corresponde ahora. Objetivo actual: {objective}.

**Disparador:** Se intenta trabajar el extremo incorrecto del cable.

**Diálogo de alerta**

**Instructor:**

> Ese no es el extremo indicado. Trabaja primero el extremo {end}.

**Disparador:** Se reporta un orden de conductores incorrecto.

**Diálogo de alerta**

**Instructor:**

> El orden de los conductores no coincide con la norma T568B. Revisa los colores e inténtalo nuevamente.

**Variables:** `{objective}` indica el objetivo actual; `{end}`, el extremo izquierdo o derecho.

### Reacción al completar un objetivo

**Acción del NPC:** activar la animación y el sonido de finalización, si están configurados. No añade diálogo.

## Módulo 2 Instalación de un switch en rack

**GameData:** `NetworkSimulatorVR/Assets/GameData/Module02/Module02Tutorial.asset`.

**Controlador:** `NetworkSimulatorVR/Assets/Scripts/Modules/Module02_RackInstallation/Presentation/Tutorial/Module02TutorialController.cs`.

**Construcción de pasos:** `NetworkSimulatorVR/Assets/Scripts/Modules/Module02_RackInstallation/Presentation/Tutorial/Module02TutorialBuilder.cs`.

### Inicio

**Acción del NPC:** colocarse en el waypoint de inicio antes de comenzar.

### Paso 01 Mirada

**Acción del NPC:** orientar la mirada al jugador.

### Paso 02 Diálogo de bienvenida e instrucciones

**Diálogo de bienvenida e instrucciones**

**Instructor:**

> Bienvenido al cuarto de servidores. Hoy inspeccionarás el rack, montarás un switch y prepararás su cableado y configuración. Las herramientas están disponibles desde el inicio; cada una realiza su interacción cuando corresponde. Presiona B para completar el texto o continuar. Acompáñame al rack para comenzar.

### Paso 03 Mirada

**Acción del NPC:** orientar la mirada en la dirección del movimiento.

### Paso 04 Movimiento

**Acción del NPC:** desplazarse hacia la zona del rack. Waypoint del controlador: `rackWaypoint`.

**Avance:** esperar a que el NPC llegue al destino.

### Paso 05 Mirada

**Acción del NPC:** orientar la mirada al jugador.

### Paso 06 Inspeccionar el rack

**Diálogo informativo**

**Instructor:**

> Un rack organiza equipos de red, servidores y alimentación. Existen racks abiertos y gabinetes cerrados; su capacidad vertical se expresa en unidades U. En este entorno, el patch panel concentra puntos de red, el firewall controla el tráfico entre redes y la PDU distribuye alimentación eléctrica.

**Diálogo de instrucciones**

**Instructor:**

> Acércate al rack y revisa las fichas de sus elementos estáticos. Recorre todas las páginas y pulsa Marcar como revisado. El contador indica cuántas fichas diferentes llevas; abrir la misma varias veces no suma.

**Espera:** completar este objetivo para continuar.

### Paso 07 Inspeccionar el switch

**Diálogo informativo**

**Instructor:**

> El switch conecta dispositivos de una red local y reenvía tramas Ethernet. En esta práctica ocupará el espacio señalado entre el patch panel y el firewall. Los IDs visibles de dispositivos y puertos te ayudarán a localizar cada conexión.

**Diálogo de instrucciones**

**Instructor:**

> Puedes agarrar y girar el switch mientras revisas sus fichas. Identifica el frente, las dos pestañas, la alimentación y el puerto Console. Recorre sus páginas y marca cada ficha como revisada.

**Espera:** completar este objetivo para continuar.

### Paso 08 Montar el switch

**Diálogo informativo**

**Instructor:**

> El switch entra en las guías del rack con el frente y sus puertos orientados hacia ti. La guía visual señala la posición de montaje. Sus dos pestañas laterales quedarán al frente para fijar el equipo.

**Diálogo de instrucciones**

**Instructor:**

> Toma el switch, alinea su orientación con la guía e introdúcelo hasta que quede montado. Si la guía no acepta la posición, ajusta el ángulo antes de continuar.

**Espera:** completar este objetivo para continuar.

### Paso 09 Fijar el switch

**Diálogo informativo**

**Instructor:**

> Las pestañas sujetan el switch al rack. Los tornillos ya están colocados: en esta simulación basta mantener la punta del destornillador en cada zona durante unos dos segundos, sin girar físicamente la herramienta.

**Diálogo de instrucciones**

**Instructor:**

> Selecciona el destornillador en la rueda y completa las dos pestañas. Observa el progreso de cada lado. Al terminar aparecerán los cinco cables para la instalación.

**Espera:** completar este objetivo para continuar.

### Paso 10 Conectar enlaces

**Diálogo informativo**

**Instructor:**

> Son cinco conexiones: una de alimentación y cuatro Ethernet. Cada dirección combina el ID del dispositivo y del puerto, por ejemplo SW1/Gi01. La tabla del entorno muestra los pares exactos; los demás puertos del modelo no son interactuables.

**Diálogo de instrucciones**

**Instructor:**

> Conecta PDU-A/AC01 con SW1/Power; PP-A/01, PP-A/02 y PP-A/03 con SW1/Gi01, Gi02 y Gi03, respectivamente; y FW1/eth01 con SW1/Gi04. Comprueba el contador de cinco conexiones correctas.

**Espera:** completar este objetivo para continuar.

### Paso 11 Etiquetar enlaces

**Diálogo informativo**

**Instructor:**

> Las etiquetas permiten identificar ambos extremos sin seguir visualmente todo el cable. Su texto indica los puertos conectados. Si cambias una conexión después de etiquetarla, sus etiquetas se invalidan y debes aplicarlas de nuevo.

**Diálogo de instrucciones**

**Instructor:**

> Selecciona la etiquetadora, acerca su punta a cada extremo y presiona el trigger. Etiqueta ambos extremos de los cinco cables: diez etiquetas en total. Puedes consultar la tabla de conexiones para comprobar los IDs.

**Espera:** completar este objetivo para continuar.

### Paso 12 Conectar la consola

**Diálogo informativo**

**Instructor:**

> El puerto Console ofrece acceso de administración al switch. La terminal tiene su propio cable integrado y un único extremo libre. Este cable se suma a los cinco de instalación y no cuenta como un enlace Ethernet.

**Diálogo de instrucciones**

**Instructor:**

> Selecciona la terminal y conecta su extremo libre a SW1/Console. Conserva la terminal disponible para el puzzle. Si guardas la herramienta, su cable se recoge y tendrás que conectarlo de nuevo para configurar.

**Espera:** completar este objetivo para continuar.

### Paso 13 Encender el switch

**Diálogo informativo**

**Instructor:**

> Antes de encender se vuelve a comprobar el cableado y las etiquetas vigentes. Si falta algo, debes corregirlo. Un encendido correcto bloquea el agarre de los cables de instalación para proteger las conexiones; el cable de consola sigue siendo desmontable.

**Diálogo de instrucciones**

**Instructor:**

> Pulsa el botón de encendido del switch. Si aparece el aviso de cableado y etiquetado, corrige las conexiones o etiquetas pendientes y vuelve a intentarlo. El LED de alimentación confirma el encendido.

**Espera:** completar este objetivo para continuar.

### Paso 14 Configurar el switch

**Diálogo informativo**

**Instructor:**

> La consola real utiliza distintos modos de acceso, como privilegiado y configuración global. Aquí los resumimos en cuatro bloques: acceder a configuración, asignar IP de gestión, definir gateway y habilitar puertos. Los valores son predefinidos para centrarnos en la instalación física.

**Diálogo de instrucciones**

**Instructor:**

> Con el switch encendido y la consola conectada, ordena los cuatro bloques y aplica la configuración. Al habilitar los puertos correctos se encienden sus LED y se representa el envío de paquetes en los cuatro cables Ethernet. La alimentación y la consola no muestran ese efecto.

**Espera:** completar este objetivo para continuar.

### Paso 15 Cierre de práctica y transición

**Diálogo de cierre de práctica y transición**

**Instructor:**

> Has completado la instalación y configuración del switch. Ahora acompáñame para realizar el quiz final y repasar lo aprendido.

### Paso 16 Mirada

**Acción del NPC:** orientar la mirada en la dirección del movimiento.

### Paso 17 Movimiento

**Acción del NPC:** desplazarse hacia la zona del quiz. Waypoint del controlador: `quizWaypoint`.

**Avance:** esperar a que el NPC llegue al destino.

### Paso 18 Mirada

**Acción del NPC:** orientar la mirada al jugador.

### Paso 19 Instrucciones del quiz

**Diálogo de instrucciones del quiz**

**Instructor:**

> Responde todas las preguntas y entrega el quiz. Verás tu puntuación y recibirás la insignia por completar el módulo. El progreso se guarda en este dispositivo; después podrás repetir el quiz, reiniciar el módulo o volver al lobby.

### Fin de la secuencia principal

El director emite `TutorialCompleted`. El quiz aparece cuando el tutorial termina. Entregarlo completa el módulo.

### Recordatorios durante la práctica

Se muestran durante la práctica para recordar el objetivo actual.

#### Inspeccionar el rack

**Diálogo de recordatorio**

**Instructor:**

> Recorre y confirma las fichas estáticas.

#### Inspeccionar el switch

**Diálogo de recordatorio**

**Instructor:**

> Recorre y confirma las fichas del switch.

#### Montar el switch

**Diálogo de recordatorio**

**Instructor:**

> Introduce el switch en la posición indicada.

#### Fijar el switch

**Diálogo de recordatorio**

**Instructor:**

> Fija las dos pestañas con el destornillador.

#### Conectar los enlaces

**Diálogo de recordatorio**

**Instructor:**

> Conecta alimentación y cuatro enlaces Ethernet según la tabla.

#### Etiquetar los enlaces

**Diálogo de recordatorio**

**Instructor:**

> Aplica las diez etiquetas en conexiones correctas.

#### Conectar la consola

**Diálogo de recordatorio**

**Instructor:**

> Conecta el cable integrado a SW1/Console.

#### Encender el switch

**Diálogo de recordatorio**

**Instructor:**

> Completa cableado y etiquetas antes de encender.

#### Configurar el switch

**Diálogo de recordatorio**

**Instructor:**

> Ordena y aplica los cuatro bloques de configuración.

### Alertas de interacción

Se muestran cuando una interacción es incorrecta.

**Disparador:** La secuencia rechaza una acción que aún no corresponde.

**Diálogo de alerta**

**Instructor:**

> Esta acción aún no corresponde al objetivo actual.

**Disparador:** El encendido no supera la comprobación de cableado y etiquetado.

**Diálogo de alerta**

**Instructor:**

> Realice el cableado y etiquetado correctamente antes de continuar

### Reacción al completar un objetivo

**Acción del NPC:** activar la animación y el sonido de finalización, si están configurados. No añade diálogo.

## Módulo 3 Diagnóstico

**Estado:** tutorial pendiente de implementación. `Assets/GameData/Module03/Module03Tutorial.asset` contiene una lista `dialogues` vacía. No hay diálogos, waypoints ni steps de tutorial definidos para transcribir en este módulo.

## Referencias para contenido y audio

Cada diálogo tiene un espacio para `AudioClip` o una ruta relativa a `Resources`, sin extensión. En módulo 2, la explicación y la instrucción de cada objetivo tienen audios separados. Los IDs y nombres de campo indicados en este documento permiten asociar cada grabación con su texto. La primera intervención del lobby conserva un clip asignado; los campos restantes pueden completarse en el Inspector.

La narración principal la ejecutan `TutorialDirector`, `DialogueStep` y `GuidedObjectiveStep`; el panel lo presenta `NPCDialogueController` y la voz la reproduce `NPCVoiceController`. Los recordatorios, alertas y reacciones de finalización pasan por `NPCReactionController`.

**Scripts compartidos:**

- `Assets/Scripts/Presentacion/Tutorial/TutorialDirector.cs`

- `Assets/Scripts/Presentacion/Tutorial/DialogStep.cs`

- `Assets/Scripts/Presentacion/Tutorial/GuidedObjectiveStep.cs`

- `Assets/Scripts/Presentacion/Tutorial/MoveNpcStep.cs`

- `Assets/Scripts/Presentacion/Tutorial/LookAtStep.cs`

- `Assets/Scripts/Presentacion/Tutorial/WaitSecondStep.cs`

- `Assets/Scripts/Presentacion/NPC/NPCReactionController.cs`

Las rutas son relativas a la raíz del proyecto. Las condiciones e identificadores corresponden al estado del proyecto en la fecha del documento.
