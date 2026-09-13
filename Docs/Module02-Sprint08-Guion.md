# Guion del NPC — módulo 2, sprint 8

Texto editable en `Assets/GameData/Module02/Module02Tutorial.asset` (ScriptableObject).
El Builder organiza los pasos; los diálogos no están escritos dentro del Builder.

## Inicio

**Instructor:** Bienvenido al cuarto de servidores. Hoy inspeccionarás el rack, montarás un switch y prepararás su cableado y configuración. Las herramientas están disponibles desde el inicio; cada una realiza su interacción cuando corresponde. Presiona B para completar el texto o continuar. Acompáñame al rack para comenzar.

**Acción:** orientarse hacia el jugador, desplazarse a `waypoint_rack` y volver a mirar al jugador.

## Inspección del rack (`inspect_rack`)

**Instructor — explicación:** Un rack organiza equipos de red, servidores y alimentación. Existen racks abiertos y gabinetes cerrados; su capacidad vertical se expresa en unidades U. En este entorno, el patch panel concentra puntos de red, el firewall controla el tráfico entre redes y la PDU distribuye alimentación eléctrica.

**Instructor — instrucción:** Acércate al rack y revisa las fichas de sus elementos estáticos. Recorre todas las páginas y pulsa Marcar como revisado. El contador indica cuántas fichas diferentes llevas; abrir la misma varias veces no suma.

**Espera:** finalización real del objetivo. B cierra el diálogo; no completa la tarea.

## Inspección del switch (`inspect_switch`)

**Instructor — explicación:** El switch conecta dispositivos de una red local y reenvía tramas Ethernet. En esta práctica ocupará el espacio señalado entre el patch panel y el firewall. Los IDs visibles de dispositivos y puertos te ayudarán a localizar cada conexión.

**Instructor — instrucción:** Puedes agarrar y girar el switch mientras revisas sus fichas. Identifica el frente, las dos pestañas, la alimentación y el puerto Console. Recorre sus páginas y marca cada ficha como revisada.

**Espera:** finalización real del objetivo. B cierra el diálogo; no completa la tarea.

## Montaje (`mount_switch`)

**Instructor — explicación:** El switch entra en las guías del rack con el frente y sus puertos orientados hacia ti. La guía visual señala la posición de montaje. Sus dos pestañas laterales quedarán al frente para fijar el equipo.

**Instructor — instrucción:** Toma el switch, alinea su orientación con la guía e introdúcelo hasta que quede montado. Si la guía no acepta la posición, ajusta el ángulo antes de continuar.

**Espera:** finalización real del objetivo. B cierra el diálogo; no completa la tarea.

## Fijación (`fasten_switch`)

**Instructor — explicación:** Las pestañas sujetan el switch al rack. Los tornillos ya están colocados: en esta simulación basta mantener la punta del destornillador en cada zona durante unos dos segundos, sin girar físicamente la herramienta.

**Instructor — instrucción:** Selecciona el destornillador en la rueda y completa las dos pestañas. Observa el progreso de cada lado. Al terminar aparecerán los cinco cables para la instalación.

**Espera:** finalización real del objetivo. B cierra el diálogo; no completa la tarea.

## Cableado (`connect_links`)

**Instructor — explicación:** Son cinco conexiones: una de alimentación y cuatro Ethernet. Cada dirección combina el ID del dispositivo y del puerto, por ejemplo SW1/Gi01. La tabla del entorno muestra los pares exactos; los demás puertos del modelo no son interactuables.

**Instructor — instrucción:** Conecta PDU-A/AC01 con SW1/Power; PP-A/01, PP-A/02 y PP-A/03 con SW1/Gi01, Gi02 y Gi03, respectivamente; y FW1/eth01 con SW1/Gi04. Comprueba el contador de cinco conexiones correctas.

**Espera:** finalización real del objetivo. B cierra el diálogo; no completa la tarea.

## Etiquetado (`label_links`)

**Instructor — explicación:** Las etiquetas permiten identificar ambos extremos sin seguir visualmente todo el cable. Su texto indica los puertos conectados. Si cambias una conexión después de etiquetarla, sus etiquetas se invalidan y debes aplicarlas de nuevo.

**Instructor — instrucción:** Selecciona la etiquetadora, acerca su punta a cada extremo y presiona el trigger. Etiqueta ambos extremos de los cinco cables: diez etiquetas en total. Puedes consultar la tabla de conexiones para comprobar los IDs.

**Espera:** finalización real del objetivo. B cierra el diálogo; no completa la tarea.

## Consola (`connect_console`)

**Instructor — explicación:** El puerto Console ofrece acceso de administración al switch. La terminal tiene su propio cable integrado y un único extremo libre. Este cable se suma a los cinco de instalación y no cuenta como un enlace Ethernet.

**Instructor — instrucción:** Selecciona la terminal y conecta su extremo libre a SW1/Console. Conserva la terminal disponible para el puzzle. Si guardas la herramienta, su cable se recoge y tendrás que conectarlo de nuevo para configurar.

**Espera:** finalización real del objetivo. B cierra el diálogo; no completa la tarea.

## Encendido (`power_on`)

**Instructor — explicación:** Antes de encender se vuelve a comprobar el cableado y las etiquetas vigentes. Si falta algo, debes corregirlo. Un encendido correcto bloquea el agarre de los cables de instalación para proteger las conexiones; el cable de consola sigue siendo desmontable.

**Instructor — instrucción:** Pulsa el botón de encendido del switch. Si aparece el aviso de cableado y etiquetado, corrige las conexiones o etiquetas pendientes y vuelve a intentarlo. El LED de alimentación confirma el encendido.

**Espera:** finalización real del objetivo. B cierra el diálogo; no completa la tarea.

## Configuración (`configure_switch`)

**Instructor — explicación:** La consola real utiliza distintos modos de acceso, como privilegiado y configuración global. Aquí los resumimos en cuatro bloques: acceder a configuración, asignar IP de gestión, definir gateway y habilitar puertos. Los valores son predefinidos para centrarnos en la instalación física.

**Instructor — instrucción:** Con el switch encendido y la consola conectada, ordena los cuatro bloques y aplica la configuración. Al habilitar los puertos correctos se encienden sus LED y se representa el envío de paquetes en los cuatro cables Ethernet. La alimentación y la consola no muestran ese efecto.

**Espera:** finalización real del objetivo. B cierra el diálogo; no completa la tarea.

## Cierre

**Instructor:** Has completado los objetivos prácticos: el switch quedó montado, fijado, conectado, etiquetado y configurado. Observa sus indicadores y la animación de paquetes para comprobar los enlaces habilitados. ¡Buen trabajo!

El NPC permanece en el rack. `waypoint_quiz` está reservado para el siguiente sprint.
Las explicaciones de objetivos ya completados se omiten, incluyendo el salto de inspección de depuración.
