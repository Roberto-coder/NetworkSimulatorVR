> Documento histórico. La revisión vigente del orden, objetivos y ubicación de diálogos está en [Tutorial secuencial](Module03-TutorialSequence.md). El guion Word actualizado es `Guiones_Modulos_M3.docx`.

# Módulo 3 · Sprint 8: flujo guiado y objetivos

## Resultado

Integra cuatro objetivos de práctica con el sistema compartido de objetivos/muñeca, el guion aprobado, el instructor existente, dos waypoints editables, la rueda y un panel fijo de ayuda. No concede todavía el logro final ni inicia quiz o lobby.

Los datos están en `Assets/GameData/Module03/Presentation/Module03FlowSettings.asset`, con 26 intervenciones: 4 introducciones, 4 instrucciones de objetivo, 10 cápsulas y 8 pistas. Cada entrada permite asignar un clip o ruta Resources. No se generaron grabaciones de voz; sin clip funciona con texto/subtítulos.

La definición es `Assets/GameData/ModulesManagers/Module03_Diagnostics.asset`; sus cuatro ObjectiveData se guardan en `Assets/GameData/Objectives/Module03`. El configurador copia desde la rueda existente los prefabs/iconos del tester y etiquetadora del sprint 6.

## Aplicar en Unity

1. Fuera de Play, abre Modulo3 con los sprints anteriores aplicados y espera la importación de scripts/assets.
2. Ejecuta **Network Simulator > Module 03 > Sprint 8 - Flujo guiado y objetivos**.
3. Guarda la escena. Se crea `Module03 guided flow` bajo `_Managers` si existe; el panel fijo queda bajo `_UI`. Se reutiliza un instructor único o se instancia `InstructorNPC.prefab` en Editor.
4. Coloca el instructor sobre el suelo real y ajusta los puntos Laptop / PC-01 dentro de `M3 waypoints`. Las ubicaciones generadas son aproximaciones; el movimiento sigue segmentos rectos, no evita muebles automáticamente. Ajusta ambos puntos para dejar recorridos libres.
5. Ajusta `M3 guidance panel` cerca de la laptop a una posición legible en visor. Se guarda en escena, no se crea ni reposiciona durante Play.
6. Comprueba las referencias de muñeca del PlayerVR existente y que la rueda ofrezca tester y etiquetadora. La laptop permanece fija.

Repetir el menú selecciona el flujo existente sin duplicar el NPC, los controles ni recolocar ajustes manuales. Para cambios posteriores editar sus referencias en Inspector. El configurador no borra controladores de otros módulos: exige retirarlos del instructor antes de reutilizarlo.

## Interacción narrativa

- Bienvenida inicial; instrucciones según etapa, sin bloquear la manipulación ni completar objetivos.
- Introducción al mapa al abrir la laptop; rayos X y bitácora al primer uso. Las introducciones contextuales se encolan y se espacian al menos 30 segundos; cerrar la consola descarta las que estén pendientes.
- El panel de ayuda muestra el concepto seleccionado y ofrece Siguiente ayuda, Escuchar ayuda, Repetir instrucción, Omitir diálogo y Reiniciar práctica.
- Los conceptos y pistas se reproducen bajo petición; no hay recordatorios automáticos insistentes.
- Solo una intervención a la vez. B confirma el diálogo mediante VRInputManager; Omitir lo cierra. Pausa detiene avance del flujo, temporizadores y voz.
- NPC se dirige al punto de PC-01 en la etapa física y al de laptop en las demás. No depende de llegar al waypoint para aprobar.

El texto completo aprobado se mantiene en [el guion](Module03-Sprint08-Guion.md). El asset es la fuente editable de ejecución; cambiar el documento no sobrescribe automáticamente un asset ya editado.

## Evidencia y secuencia

| Etapa | Condición real de avance |
| --- | --- |
| Cable | CableRepairService verifica tester del retirado/reemplazo, sustitución, etiquetas y ping vigente desde laptop. |
| Puerto | Consulta de puertos mediante gestión alcanzable registrada después del reinicio, y puerto configurado habilitado. No exige resolver todavía la IP duplicada. |
| IP | Reglas de direcciones y unicidad satisfechas, pings vigentes desde laptop a PC-02 y PC-03, con respondiente correcto. |
| Red completa | Todas las reglas finales, evidencia actual del incidente físico y pings a las tres PCs posteriores al último cambio. |

Conserva las tres fallas iniciales y reconoce reparaciones anticipadas sin reinyectar daños. Solo evalúa la etapa activa y no usa conversaciones como evidencia. Las comprobaciones se ejecutan cada 0.2 segundos; no generan pings ocultos para aprobar. La lista de muñeca conserva hitos completados; si se altera la red después del último hito, el panel muestra que la salud actual debe comprobarse otra vez. El futuro cierre deberá comprobar `NetworkVerified`, además de los hitos.

Reiniciar cierra la consola, desequipa la herramienta, restaura la sesión y permite al adaptador físico restaurar conexiones. Se limpian evidencias y objetivos; se cancela voz/movimiento y vuelve la bienvenida. Los assets no guardan progreso mutable.

La consola actual permite pings con origen local desde PCs; esas pruebas siguen disponibles. Esta práctica exige laptop como origen común de sus evidencias finales.

## Scripts añadidos y modificados

| Script | Estado | Objetivo o función |
| --- | --- | --- |
| `Module03FlowSettings.cs` | Añadido, GameData | Definición de módulo, reglas, IDs de verificación y texto/audio de cada intervención. |
| `DiagnosticProgressEvidence.cs` | Añadido, Domain | Conserva pruebas observadas; comprueba origen, identidad, sesión/revisión y consultas de switch posteriores a reset. |
| `Module03ObjectiveFactory.cs` | Añadido, Factories | Crea objetivos que validan sus condiciones incluso ante Complete directo. |
| `Module03FlowController.cs` | Añadido, Flow | Reutiliza ObjectiveController e IObjectiveFlow; coordina los cuatro hitos y el reinicio. |
| `Module03GuidedFlow.cs` | Añadido, Flow | Integra estado de red, reparación, evidencia, definición, rueda y muñeca. |
| `Module03GuidancePresenter.cs` | Añadido, Presentation | Cola única de narración, ayudas manuales, controles, pausa, estado visible y movimiento del instructor. |
| `Module03GuidedFlowSetup.cs` | Añadido, Editor | Conecta datos y escena, reutiliza NPC y crea panel/waypoints editables. |
| `NetworkSession.cs` | Modificado | Registra consulta válida de puertos como evidencia específica. |
| `DiagnosticWorkspace.cs` | Modificado | Registra esa consulta solo después de comprobar acceso a gestión. |
| `DiagnosticScreenController.cs` | Modificado | Expone apertura y uso de comandos para ayudas contextuales. |
| `WristObjectivesPresenter.cs` | Modificado, compartido | Lee el flujo de M3 conservando soporte de los módulos existentes. |
| `GuidedFlowChecks.cs` | Añadido, pruebas | Casos negativos de evidencia, orden de etapas, bypass de Complete y reinicio. |
| `Program.cs`, `UnityStubs.cs`, proyecto de pruebas | Modificados | Incluyen flujo/factory reales y contrato mínimo de ModuleDefinition para prueba sin Unity. |

## Validación

184 comprobaciones correctas con `dotnet run --project Tests/Module03Diagnostics/Module03Diagnostics.csproj --no-restore`. Compilación externa de los scripts de runtime y Editor con referencias Unity correcta; advertencias CS0649 de campos serializados.

No se ha ejecutado el configurador ni probado en visor desde esta sesión. La compilación no valida la colocación, el funcionamiento del prefab del instructor o las referencias serializadas de la escena.

Comprobar en Play:

1. Bienvenida, cuatro objetivos en muñeca y herramientas correctas; las instrucciones no bloquean acciones.
2. Map/bitácora/rayos X ofrecen sus introducciones una sola vez; ayudas manuales no se superponen con la voz actual.
3. Tester, reemplazo y etiquetas incorrectos no completan etapa 1; repetir ping correcto sí cuando se cumplen todas las condiciones.
4. Puerto equivocado o gestión inaccesible no completan etapa 2. Puerto correcto avanza aunque exista conflicto IP.
5. Ping exitoso desde otra PC no acredita la verificación desde laptop. Respuesta de otro equipo tampoco acredita.
6. Cambiar un dato después de hacer ping invalida la evidencia anterior. Antes del cierre repetir los tres pings.
7. Reiniciar durante diálogo, prueba del tester y animación de ping; comprobar conexiones iniciales, rueda, muñeca y ausencia de audio duplicado.
8. Pausar/reanudar y comprobar B/A/Y; revisar panel en ambos ojos y recorridos del instructor sin obstáculos.
9. Volver a módulos anteriores y comprobar que la muñeca mantiene sus objetivos habituales.


## Revisión: tutorial opcional sin panel de ayudas

Esta revisión sustituye el panel de selección de ayudas descrito anteriormente. Las nuevas escenas no lo generan; las escenas anteriores lo ocultan en Play a través de sus referencias conservadas. Si deseas quitarlo también de la jerarquía, puedes eliminar únicamente `M3 guidance panel` bajo `_UI`; los campos ya no son necesarios para el flujo.

En `_Managers/Module03 guided flow`, componente **Module03GuidancePresenter**, desactiva **Tutorial Enabled** para practicar sin bienvenida, diálogos, recordatorios ni desplazamientos guiados del NPC. Los objetivos, la muñeca y las condiciones de reparación siguen activos. No desactivar Module03GuidedFlow para omitir tutorial.

Los complementos se presentan al usar ping, ARP, configuración, bitácora o rayos X, y durante los objetivos. Los recordatorios usan la primera pista de cada etapa cada 75 segundos sin otra intervención, ajustable en **Reminder Interval**. B permite avanzar/ocultar el diálogo con el controlador compartido. Pausa detiene los temporizadores. Los textos permanecen en Module03FlowSettings.

### Muñeca en PlayerVR_XRI_Hands

En los archivos guardados inspeccionados no existe instancia del prefab ObjectivesCanva en Modulo3/PlayerVR_XRI_Hands. El prefab fuente está en `Assets/Prefabs/UI_Components/Objectives/ObjectivesCanva.prefab`; el antiguo PlayerRoot sí lo referencia.

Selecciona en la jerarquía el anclaje que sigue la mano o controlador izquierdo y ejecuta **Network Simulator > Module 03 > Instalar objetivos en anclaje de muñeca seleccionado**. Crea `ObjectivesCanva` como hijo del anclaje seleccionado y asigna cámara, muñeca y Canvas. Guarda la escena. Si ya existe un controlador, lo selecciona para revisar sus referencias sin duplicar objetos.

Ajusta el panel al dorso de la mano y comprueba que **Local Display Normal** apunta hacia los ojos cuando lo miras. La apertura requiere que el panel mire a la cámara y que la muñeca esté dentro del ángulo de visión. Activa **Debug Logs** para comprobar referencias. El controlador ahora oculta con CanvasGroup cuando vive dentro del propio Canvas, manteniendo Update activo para poder volver a abrirlo.

| Script modificado | Función |
| --- | --- |
| Module03GuidancePresenter.cs | Tutorial opcional, complementos y recordatorios sin botones de ayudas. |
| Module03GuidedFlowSetup.cs | No genera el panel y añade instalación explícita del Canvas en el anclaje seleccionado. |
| WristMenuController.cs | Evita desactivar su propio Update y permite configurar referencias del anclaje. |

Compilación externa correcta; comprobar en visor el gesto, orientación y legibilidad de la muñeca, y tutorial desactivado con objetivos aún funcionales.


## Corrección del estado inicial y acreditación del cable

Se restauró OfficeInitialState usado por Modulo3: SW-01/P02 deshabilitado, PC-02 y PC-03 con 192.168.10.12/24, duplicidad declarada para ambas interfaces. Patch-01 conserva la rotura. Las reglas finales no cambian: PC-03 debe terminar en .13 y P02 habilitado. Salir y volver a Play para cargar el asset actualizado; Reset de una sesión ya abierta reutiliza su copia inicial anterior.

El primer objetivo exige terminar la secuencia del tester para el cable retirado y para el reemplazo, instalar el reemplazo correcto, etiquetar según la orientación real y hacer ping desde Laptop/eth0 a PC-01 después de los cambios. Un ping de PC-01 hacia sí misma no comprueba el patch cord.

Tras ping a PC-01, la consola muestra el requisito pendiente. También se expone Progress Status en Module03GuidedFlow. No se ha confirmado en visor cuál de estas condiciones bloqueó el intento reportado.

Se eliminó la dependencia de IsComplete respecto al texto de Status. Reaplicar una etiqueta sin cambio ya no elimina la evidencia vigente. El estado tipado CableRepairProgress conserva las validaciones de tester, identidad del reemplazo, etiquetas, respondiente y revisión.

| Script modificado | Función |
| --- | --- |
| CableRepairService.cs | Progreso tipado, motivos precisos e idempotencia del etiquetado. |
| CableRepairController.cs | Explica el requisito pendiente en el resultado del ping a PC-01. |
| Module03GuidedFlow.cs | Expone Progress Status en Inspector para depurar la etapa activa. |
| RepairChecks.cs | Comprueba reparación con otras fallas activas, avance al puerto y etiqueta repetida. |

188 comprobaciones correctas y compilación externa con referencias Unity correcta. Pendiente repetir el recorrido físico en Play/visor.
