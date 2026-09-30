# Sprint 6 — Reparación de cable y etiquetado

## Entrega

Integra los patch cords RJ45 de módulo 2 con NetworkSession. Se prepara el incidente físico de PC-01: retirar cable roto, comprobar continuidad, probar repuesto, instalarlo, etiquetar ambos extremos y verificar un ping nuevo desde Laptop.

El gestor publica CompletionChanged y muestra un panel fijo de estado. La integración con factories/objetivos de muñeca, NPC y quiz corresponde al sprint de flujo. No se añade guion ni se completan objetivos existentes por presionar un botón.

## Scripts añadidos y modificados

| Script | Objetivo o función |
| --- | --- |
| Domain/CableRepairService.cs | Define CableRepairDefinition y exige evidencias de tester, sustitución, etiquetas orientadas y ping vigente. |
| Domain/NetworkSession.cs (modificado) | Registra pruebas de continuidad aceptadas/rechazadas sin cambiar revisión de topología. |
| GameData/Module03/Definitions/CableRepairSettings.cs | ScriptableObject de incidente, prefabs y radios de herramientas. |
| Interaction/RepairPatchCord.cs | Vincula ID lógico con PatchCableLink y extremos físicos A/B; guarda textos de etiquetas. |
| Interaction/CableRepairController.cs | Sincroniza encajes con la sesión, restaura conexiones iniciales/reset y publica cumplimiento. |
| Interaction/CableRepairTool.cs | Entrada de gatillo; delega en XRCableTester o aplica etiqueta elegida al extremo cercano. |
| Editor/Module03CableRepairSetup.cs | Configurador repetible, sockets, cables, soporte, herramientas y referencias; Undo de escena. |
| Shared/Cabling/PatchCableLink.cs (reutilizado) | Observa conectores físicos y expone los puertos conectados. |
| Shared/Cabling/CableEndSocketDetector.cs (reutilizado) | Desconecta al agarrar un plug y conecta al soltarlo sobre un socket compatible. |
| Cable_physics/Scripts/Connector.cs y PhysicCable.cs (reutilizados) | Encaje, joints y geometría física del cable del módulo 2. |
| Tests/Module03Diagnostics/RepairChecks.cs | Casos positivos y negativos de evidencia y reparación. |

## GameData y prefabs

El menú crea `Assets/GameData/Module03/Presentation/CableRepairSettings.asset`. Allí se editan ID del cable averiado, puertos, etiquetas, origen/destino/responder de la verificación, referencias de prefabs y radios. No se codifican datos del incidente en los botones.

- Patch cord predeterminado: `Assets/GameData/Module02/Module02_Sequence/InstallationCable_02.prefab` (Ethernet, no InstallationCable_01 de alimentación).
- Socket: `Assets/Prefabs/Tools/Module02/Cabling/NetworkPortSocket_RJ45.prefab`.
- Herramientas generadas: `Assets/Prefabs/Dispositivos/Modulo3/Tester_M03.prefab` y `LabelMaker_M03.prefab`.

Se prepara un único prefab PatchCord_M03 a partir del cable RJ45 del módulo 2, retirando su etiquetado/demo y añadiendo la identidad del módulo 3. Todos los cables de la escena permanecen vinculados al nuevo prefab. Los originales no se editan. Las herramientas tienen cuerpos geométricos provisionales, display y lógica específica; se pueden sustituir sus visuales en Prefab Mode. El tester es una interacción didáctica de proximidad, no una simulación de ocho LED/pines ni un medidor de categoría/rendimiento.

## Aplicación en Unity

1. Salir de Play y abrir Modulo3 con sprint 4 configurado y un único ToolManager.
2. Abrir **Network Simulator > Module 03 > Sprint 6 - Reparación y etiquetado**.
3. Asignar Red; dejar Configuración vacía para crear el asset predeterminado, o asignar uno propio.
4. Pulsar **Crear / completar sprint 6 (Undo)**.
5. Revisar los sockets creados bajo NetworkPortAnchor: centro y orientación de la boca deben corresponder al modelo. Se crean sólo los seis puertos de patch cords; el rack conserva rutas fijas.
6. Revisar los cuatro cables (tres instalados y repuesto), soporte del repuesto y panel Repair status cerca de PC-01. Ajustar el soporte a una mesa real si hace falta. La distribución inicial de puntos es provisional y requiere prueba de física en visor.
7. Guardar la escena. Los Canvas/textos se crean exclusivamente en Editor. _Interactuables/_UI/_Managers se conservan; el gestor nuevo va bajo _Managers si existe.

Repetir no recrea cables/sockets/gestor ni reemplaza sus transforms. No usarlo para regenerar un cable ya editado. Los assets generados no forman parte del Undo de escena.

## Prueba del incidente

1. Desde la laptop, ping a PC-01: comprobar falta de respuesta.
2. Con mano vacía, tomar y retirar los dos plugs de Patch-01. Soltarlo fuera de los sockets.
3. Mantener Y, elegir Tester XR M3 y soltar Y. Soltar un plug libre en Master y el otro en Remote. Pulsar gatillo derecho y esperar la secuencia LED. Sólo una punta, cables distintos o retirar una punta durante la secuencia no registra prueba válida.
4. Retirar las puntas del tester y probar el repuesto del mismo modo: continuidad correcta al terminar los LEDs.
5. Elegir mano vacía y conectar el repuesto entre R01/front y PC-01/eth0. El orden Start/End puede invertirse. No reconectar el cable roto.
6. Equipar Etiquetadora M3. Usar sus botones de pantalla para elegir etiqueta del extremo A o B del incidente (R01 / PC-01 por defecto); puede usarse el rayo izquierdo para seleccionar en la herramienta sostenida a la derecha. Acercar Tip al plug correspondiente (radio 0.12 m) y pulsar gatillo derecho. Aplicar por separado a ambos extremos. El display muestra el texto seleccionado.
7. Ejecutar nuevamente ping a PC-01. El panel debe indicar **Incidente físico resuelto: reparación y ping verificados.**

El dominio no cambia intact=false del cable viejo: sustituir consiste en conectar otro ID íntegro. Las etiquetas permanecen asociadas a los extremos físicos; mover el cable no las reinterpreta automáticamente. Una etiqueta incorrecta se sobrescribe imprimiendo la correcta. El botón de etiquetado no revela por sí mismo la rotura.

## Evidencia y cancelación

- Sólo una prueba con ambos extremos lógicamente desconectados registra continuidad válida. El adaptador exige dos plugs distintos del mismo cable retenidos en los snaps XR durante toda la secuencia.
- Se exige comprobar el cable averiado y el repuesto concreto instalado.
- El cable averiado debe quedar completamente retirado.
- Deben coincidir puertos y etiquetas, incluso con cable invertido.
- Se requiere ping de la fuente configurada a la IP esperada y respuesta del puerto esperado, de la misma sesión y revisión.
- Tester/etiquetado posterior obliga a verificar de nuevo. Un cambio de red invalida el ping y la animación del sprint 5.
- Reset descarta evidencias y restaura conexiones lógicas/físicas instaladas; el repuesto suelto no se recoloca en su soporte.

## Validación

Compilación C# runtime/Editor con referencias de Unity realizada. Pruebas de dominio cubren cable conectado/parcial, cable desconocido, sustitución invertida, etiquetas incorrectas, repuesto sin tester, ping previo, otra sesión/origen, retirada posterior y Reset. Ejecución del configurador, agarre/suelta XRI, física, selección de herramientas y legibilidad en visor pendientes.

Antes de aceptar el sprint: verificar que al retirar un plug cae el enlace en la laptop, que no se puede ocupar un socket con dos cables, que las rutas fijas no se desconectan y que completar el incidente no modifica el estado de los otros dos incidentes.


## Un único prefab para todos los patch cords

Volver a ejecutar **Crear / completar sprint 6 (Undo)** fuera de Play. El configurador crea `Assets/Prefabs/Dispositivos/Modulo3/PatchCord_M03.prefab` sólo si no existe y lo asigna a `CableRepairSettings.patchCordPrefab`. Migra los cables existentes en el mismo objeto con ConvertToPrefabInstance: conserva referencias del gestor, IDs, transformaciones, etiquetas y ajustes manuales. Los nuevos cables usan InstantiatePrefab sin desempaquetarse.

Editar modelos, materiales y componentes comunes en Prefab Mode. Los valores distintos de cada cable se conservan como overrides; por tanto un valor sobrescrito en una instancia no recibe ese mismo cambio del prefab. Revertir únicamente el override deseado para volver a heredar. No aplicar IDs, conexiones ni colocaciones particulares al prefab común. La integridad del cable sigue definida por su ID en NetworkSession.

| Script modificado | Objetivo o función |
| --- | --- |
| Module03CableRepairSetup.cs | Crear plantilla compartida una vez, convertir cables existentes y guardar los ajustes de cada instancia. |

La migración de escena permite Undo. La creación del asset no se revierte con el Undo de escena. Si un cable está dentro de otro prefab contenedor, el configurador informa el caso antes de desempaquetar ese contenedor. Repetir la operación omite cables ya vinculados y no sobrescribe el asset compartido.

Validación: compilación con Unity 6000.2.7f2 correcta; comprobación visual de los enlaces a prefab y Undo pendiente en Unity. Probar que editar un material común afecta los cuatro cables y que cada uno conserva cableId, etiquetas y extremos. API utilizada: [ConvertToPrefabInstanceSettings, documentación Unity](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/converttoprefabinstancesettings).


## Tester XR con doble snap y refactorización

Ejecutar de nuevo **Crear / completar sprint 6 (Undo)**. Se crea `Assets/Prefabs/Tools/Module03/Tester_XR_M03.prefab` a partir de las mallas y los 16 LEDs del tester del módulo 1. Se eliminan de la copia sus validadores/sockets Meta y se agregan dos RepairTesterSocket de XRI. El original de módulo 1 y el antiguo Tester_M03 no se sobrescriben. La configuración y el slot existente de la rueda pasan al tester XR. El prefab XR existente se respeta en posteriores ejecuciones.

En el proyecto actual la carpeta movida se llama **Module03**, no Modulo03. La referencia a LabelMaker_M03 se conserva; nuevos prefabs de herramientas se crean en esa carpeta. Los patch cords continúan en su propio prefab compartido.

| Script añadido/modificado | Función |
| --- | --- |
| RepairTesterSocket.cs (nuevo) | XRSocketInteractor que sólo acepta plugs libres de los cables registrados. No agrega NetworkPort ni conectividad LAN. |
| XRCableTester.cs (nuevo) | Verifica ambos snaps, anima LEDs y registra evidencia sólo al completar una prueba estable. |
| Module03XrTesterBuilder.cs (nuevo) | Copia la presentación de M1, reemplaza interacción Meta por XR y conserva asignación de LEDs. |
| CableRepairTool.cs | Delega el tester al componente XR; mantiene selección e impresión de etiquetas. |
| CableRepairSettings.cs | Añade duración y colores de LEDs en GameData; testerRadius queda como legado. |
| Module03CableRepairSetup.cs | Genera la versión XR, actualiza el slot de rueda y respeta herramientas movidas a Tools/Module03. |
| NetworkSimulationService.cs | Extrae validación, grafo, BFS, reconstrucción de camino y generación ICMP en métodos privados. |
| DiagnosticScreenController.cs | Separa recopilación/validación de vistas, callbacks, pantalla abierta, apuntado y ejecución de ping. Conserva nombres serializados. |

El tester anima la fila Master en secuencia; Remote responde si el cable es íntegro. El dominio sólo conoce continuidad global: Remote apagado indica fallo general, no identifica qué conductor está roto. Se cancela sin conceder evidencia si se retira/cambia un plug, se cambia la revisión, se pausa o se guarda la herramienta. Los colores se aplican con MaterialPropertyBlock, sin crear materiales por LED/prueba.

Ajustar los transforms `Attach plug here` en Prefab Mode para orientar los plugs del patch cord: las posiciones parten de MasterSocket/RemoteSocket del modelo M1, pero el modelo de plug XR es distinto. No asignar Connected To del Connector a estos snaps; XRI mantiene el acoplamiento.

La etiquetadora M2 puede reutilizar su modelo, boquilla y presentación. Su LabelMakerTool/CableLabelPair no se reutiliza directamente porque consulta el coordinador M2 y genera otro formato de etiqueta. Mantener CableRepairTool en la versión M3 conserva la selección de R01/PC-01 y la validación por extremo; no activar ambos controladores de impresión en la misma herramienta.

Pruebas XR pendientes: uno/dos snaps, plugs del mismo cable en cualquier orden, cables distintos, retirada durante LED, pausa, desequipar y repetir. Verificar que desequipar libera los plugs sin destruirlos, que el tester no aparece en neighbours, y que sólo la secuencia terminada registra CableTest. La compilación C# y pruebas de dominio no sustituyen estas comprobaciones en visor.


## Ajuste de colocación y longitud de patch cords

En `CableRepairSettings`, `Patch Cord Length` define la longitud nominal en metros (4 por defecto, suponiendo 1 unidad Unity = 1 m). No es una afirmación sobre un estándar de cableado. Los resortes pueden estirarse durante la simulación.

Fuera de Play, coloca los `NetworkPortAnchor` y sus sockets en las rosetas/PC. El punto exacto de encaje es `Connector > Connection Point` (`ConnectionPosition`). Ejecuta **Network Simulator > Module 03 > Recolocar patch cords desde sockets** y guarda la escena. Esta acción admite Undo, conserva IDs y conexiones lógicas, y actualiza también las instancias existentes. El repuesto conserva la posición manual de sus dos extremos. Si los extremos están separados más de la longitud configurada, ese cable no se recoloca: acerca los puertos.

La curva inicial distribuye la holgura lateralmente, sin hundir los puntos bajo los extremos. Revisa que no atraviese paredes/muebles y que el repuesto quede sobre su soporte. La recolocación no detecta obstáculos. Para que la gravedad no lleve el cable bajo el suelo durante Play, el suelo debe tener Collider no trigger y su capa debe colisionar con la de los puntos físicos del cable. No uses `Reset points` después de recolocar: ese botón genera una línea en el eje forward del prefab.

| Script modificado | Función del ajuste |
| --- | --- |
| `CableRepairSettings.cs` | Expone longitud nominal configurable, por defecto 4 m. |
| `Module03CableRepairSetup.cs` | Recoloca puntos físicos por orden desde sockets, calcula holgura lateral, ajusta resortes y actualiza tramos visuales en Editor. |

Compilación externa con referencias Unity correcta. Pendiente ejecutar la recolocación y comprobar colisiones en la escena/visor.


## Sockets XR de red y colisiones

Ejecutar fuera de Play **Network Simulator > Module 03 > Agregar sockets XR a patch cords** y guardar la escena. Añade `RepairNetworkSocket` (derivado de XRSocketInteractor) a los NetworkPort de la red. Es idempotente y admite Undo. Radio inicial de captura: 0.06 unidades; ajustar a escala real. El Attach Transform del socket queda en su punto de conexión; revisar alineación con el Attach Transform del XRGrabInteractable del plug en visor.

Los puntos `Point0` y `Part_X_Point` necesitan Collider no trigger y Rigidbody dinámico; Start/End necesitan Collider sólido además de su trigger de detección. `Part_X_Conn` son visuales, no cuerpos físicos. El suelo requiere Collider sólido y capas que colisionen con los puntos. Las esferas discretas no garantizan contacto continuo de toda la malla visual entre puntos. Revisar tamaño de colliders, escala, separación de puntos y detección continua si caen a gran velocidad.

La selección por socket ya no se interpreta como retirada manual. El detector antiguo omite los puertos administrados por XR para evitar conexiones duplicadas. El snap conserva Connector/NetworkPort para que la simulación observe el cable conectado. No añadir un segundo XRSocketInteractor estándar sobre el mismo puerto.

| Script añadido/modificado | Objetivo |
| --- | --- |
| `RepairNetworkSocket.cs` (nuevo) | Snap XR filtrado por compatibilidad y puente a la unión física/lógica existente. |
| `CableEndSocketDetector.cs` | Diferencia socket de mano y omite conexión automática antigua en puertos XR. |
| `Module03CableRepairSetup.cs` | Menú para añadir sockets XR a puertos existentes. |
| `PhysicCable.cs` | Avisa con longitud medida y límite cuando desconecta por estiramiento. |

Compilación externa correcta. Pendiente en Play: insertar/retirar ambos plugs, esperar sin tocar, tomar de nuevo desde el socket, comprobar bitácora y conectar el cable al tester. Si aparece el aviso de estiramiento, revisar geometría y colisiones antes de aumentar el límite: el socket no corrige una cadena que atraviesa el suelo.
