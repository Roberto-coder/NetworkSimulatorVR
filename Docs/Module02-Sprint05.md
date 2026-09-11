# Módulo 2 — Sprint 5: encendido y consola

## Aplicación

Abre Modulo2 fuera de Play y ejecuta **Network Simulator > Module 02 > Infraestructura > Sprint 5 - Encendido y consola**. Requiere SW1/Power y SW1/Console con sus familias correctas. El puerto Power debe estar bajo la raíz del switch con XRGrabInteractable.

Se crea PowerControls_Sprint05 bajo ese switch con un botón XRSimpleInteractable, LED y Module02SwitchPower. Alinea PowerButton y PowerLED con el modelo; se crean controles provisionales para no reemplazar las zonas de inspección existentes. El botón se pulsa mediante selección XRI (selectEntered) con el interactor configurado. También existe Toggle power en el menú contextual de Module02SwitchPower para probar en Play.

El configurador conserva los controles ya creados al repetirlo. Guarda la escena tras ajustar las posiciones. El prefab y la asignación de la consola al tercer espacio de la rueda se guardan al ejecutar el configurador.

## Encendido

Module02SwitchPower verifica el cable físico que llega a SW1/Power, su familia eléctrica y el par exacto PDU-A/AC01 ↔ SW1/Power. Expone HasSupply, IsOn y StateChanged.

SwitchPowerState permite alternar encendido solo con alimentación. Retirar alimentación apaga inmediatamente al actualizar el estado; reconectar no enciende automáticamente. El botón vuelve a ser necesario. El LED asignado refleja IsOn mediante MaterialPropertyBlock.

No se exigen tornillos, etiquetas ni objetivos previos. Los LED referenciados por los NetworkPort de SW1 dejan de reflejar la mera conexión física mediante indicatePhysicalLink=false. La habilitación de interfaces y tráfico se implementará en el siguiente sprint. Si el modelo tiene otros LED animados mediante materiales o scripts independientes, esos visuales requieren asignación/revisión en Unity.

## Consola integrada

ConsoleTerminal_Module02 es un dispositivo provisional equipado en la mano mediante ToolManager. Su pantalla muestra:

- Conecta el cable de consola a SW1/Console.
- Enciende el switch.
- Consola lista / Conexión establecida con SW1.

ConsoleTerminalTool crea un cable ConsoleRj45 corto (aproximadamente 0.9 m) desde ConsoleCable_Integrated. El extremo Start queda conectado a TERM1/Console en el dispositivo y sin agarre. End es el único extremo manipulable: se conecta a SW1/Console con el sistema existente de acercar y soltar. Puede usarse la otra mano mientras la herramienta permanece equipada.

El cable tiene su propia raíz mundial para que el movimiento de la herramienta no transforme todos sus segmentos. El socket integrado mantiene su extremo fijo siguiendo al dispositivo. Al guardar/desactivar la herramienta, se cancela el agarre del extremo, se desconecta y se destruye el cable, recuperando también extremos temporalmente separados de la jerarquía por XRI. Volver a equipar crea un cable nuevo, sin una conexión previa.

IsConnected exige exactamente TERM1/Console ↔ SW1/Console. IsReady añade que el switch esté encendido. StateChanged permite conectar la pantalla de configuración después. El cable de consola no se registra en los cinco cables del panel de instalación ni en las diez etiquetas.

## Verificación realizada y pendiente

Runtime/editor compilados sin errores con las referencias locales de Unity. Pasaron 393 comprobaciones: 386 de cableado y 7 del estado de alimentación/encendido. Las pruebas automáticas no cubren la física de Unity ni XRI.

El configurador no se ha ejecutado en esta sesión. Pruebas pendientes en Play/VR:

1. Pulsar sin cable eléctrico: permanece apagado.
2. Conectar PDU-A/AC01 ↔ SW1/Power, pulsar y observar el LED.
3. Apagar/encender con el botón. Desconectar alimentación mientras está encendido: se apaga.
4. Reconectar alimentación: permanece apagado hasta pulsar.
5. Equipar consola y comprobar que solo End se puede agarrar.
6. Intentar conectarla a Ethernet/Power: no encaja. Conectar a SW1/Console: pantalla según encendido.
7. Apagar el switch o desconectar la consola: IsReady deja de ser verdadero.
8. Guardar la herramienta conectada y también con End agarrado: no deben quedar extremos, cuerpos físicos o conexiones huérfanos.
9. Reequipar: aparece un cable nuevo sin conexión al switch.
10. Ajustar tamaño/orientación de pantalla, controles y longitud del cable según ergonomía y colisiones del entorno.

El puzzle, comandos, LED de interfaces y paquetes quedan para el sprint 6.
