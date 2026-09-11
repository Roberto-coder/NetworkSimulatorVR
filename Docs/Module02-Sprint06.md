# Módulo 2 — Sprint 6: puzzle de terminal

## Configuración

En Modulo2, fuera de Play, ejecuta **Network Simulator > Module 02 > Infraestructura > Sprint 6 - Puzzle de terminal**. Requiere los cinco cables, cuatro puertos Ethernet SW1 y los componentes del sprint 5. Guarda la escena después.

El configurador actualiza el prefab existente de consola: amplía Screen y DeviceBody provisional, sitúa el mensaje de conexión en la cabecera y añade TerminalPuzzleView. La apariencia usa negro, texto verde y botones verde muy oscuro. Los paneles y listeners se crean al iniciar la herramienta en Play. El Canvas usa TrackedDeviceGraphicRaycaster, por lo que necesita el EventSystem y la entrada UI XRI ya configurados en Modulo2.

Los LED existentes asignados a NetworkPort se conservan. Si falta esa referencia, se crea OperationalLED como hijo del puerto. Ajusta su posición o asigna el renderer del LED real del modelo. No se añaden colliders a los indicadores.

## Puzzle

La pantalla muestra los bloques solo cuando ConsoleTerminalTool.IsReady es verdadero. Selecciona un bloque de la izquierda y después un destino de la derecha. Asignar el mismo bloque a otro destino lo mueve; reemplazar un destino libera el bloque anterior. No se arrastra ni se escriben comandos.

Orden correcto:

1. Acceder a configuración.
2. Asignar IP de gestión: 192.168.10.2 /24.
3. Definir gateway: 192.168.10.1.
4. Habilitar puertos: Gi01–Gi04.

Los valores son apoyo visual predefinido; no configuran una pila IP ni una CLI real. Aplicar configuración comprueba la secuencia completa. Si falla, se indica el primer paso a revisar; se permiten reintentos. Si acierta, se bloquean los controles y se habilitan las interfaces en el estado de la simulación.

## Estado y red

- SwitchConfigurationPuzzle contiene las reglas puras, comentadas y comprobadas sin Unity.
- Module02SwitchConfiguration mantiene el puzzle en el switch, no en la herramienta. Guardar o desconectar la consola conserva la configuración (también el progreso parcial).
- Apagar el switch o perder alimentación reinicia todos los bloques y apaga los enlaces. Hace falta resolver nuevamente al volver a encender.
- Place/Apply verifican la consola y alimentación actuales antes de modificar el estado, incluso si se llama a estos métodos sin pulsar los botones.
- Cada cable Ethernet se coteja con Module02ConnectionPlan. Solo los cuatro enlaces correctos, con switch encendido y configuración aplicada, encienden su LED y transmiten paquetes.
- Retirar/cambiar un cable detiene inmediatamente su efecto al actualizar los enlaces; el resto continúa. Reponer correctamente el enlace reactiva su efecto mientras el switch siga configurado.
- Alimentación y consola no muestran paquetes. SetTransmissionEnabled(false) retira también los paquetes que ya estaban en tránsito.

Se conservan las mecánicas independientes: montaje, tornillos y etiquetas todavía no bloquean el puzzle. La integración de objetivos, NPC, quiz e insignia queda para el sprint final.

## Verificación

Runtime/editor compilan sin errores; permanecen advertencias preexistentes de TextMeshPro. Pasaron **429 comprobaciones** de reglas de cableado, alimentación y puzzle, incluidas las 24 permutaciones de bloques, duplicados, reemplazo, índices inválidos, corrección, bloqueo tras resolver y reinicio.

Pendiente en Unity/VR (el menú y el render de la interfaz no se ejecutaron en esta sesión):

1. Aplicar el configurador, ajustar LED y guardar escena.
2. Encender y conectar consola: aparece el puzzle; antes solo debe verse el estado de conexión.
3. Comprobar selección UI con el rayo de la otra mano, legibilidad y tamaño físico de pantalla.
4. Probar secuencia incorrecta y corregir siguiendo la pista.
5. Resolver con enlaces Ethernet conectados: observar los cuatro LED y paquetes.
6. Retirar un enlace y reconectarlo; repetir con un destino equivocado.
7. Guardar/reabrir consola: continúa resuelto y la red no se detiene.
8. Apagar/reconectar alimentación: se reinicia el puzzle, LED y paquetes apagados hasta volver a configurarlo.

La compilación y las pruebas puras no sustituyen estas comprobaciones visuales y físicas.
