# Módulo 3 — Tutorial secuencial y presentación

Esta revisión sustituye la cola narrativa del sprint 8 por una secuencia con seis objetivos y recorridos explícitos. No cambia las posiciones de las PCs, los Canvas ni las rutas de cable de la escena.

## Dónde se definen los datos

| Archivo | Responsabilidad |
| --- | --- |
| `Assets/GameData/Module03/Topologies/OfficeInitialState.asset` | Fallas iniciales: `Patch-01.intact = false`, `SW-01/P02.enabled = false`, PC-02 y PC-03 con `192.168.10.12/24`. `Module03NetworkScene.initialState` carga una copia al crear la sesión. |
| `Assets/GameData/Module03/Validation/OfficeTargetRules.asset` | Reglas de solución, incluida PC-03 con `.13`, puerto habilitado, continuidad, etiquetas e IPs únicas. |
| `Assets/GameData/Module03/Presentation/CableRepairSettings.asset` | Incidente de cable, IDs de sus extremos, etiquetas y origen/destino exigidos para verificar. |
| `Assets/GameData/Module03/Module03Tutorial.asset` | Única fuente narrativa: 26 diálogos `M3D01…26` y 7 recordatorios `M3R01…07`, con speaker, clip/Resources y legacyVoiceId. |
| `Assets/GameData/Module03/Presentation/Module03FlowSettings.asset` | Referencias a tutorial, módulo, reglas e IDs; ya no contiene copias de los diálogos. |
| `Assets/GameData/ModulesManagers/Module03_Diagnostics.asset` | Orden de seis objetivos y herramientas. Conserva quiz e insignia existentes. |
| `Assets/GameData/Objectives/Module03/IN-01.asset`, `IN-02.asset` | Objetivos nuevos de apertura de laptop y primer ping. Los cuatro `OB` conservan sus IDs/GUIDs. |

Cambiar el asset inicial no altera una sesión ya creada: salir de Play y entrar de nuevo. `RestartPractice()` restaura la copia inicial de esa sesión.

## Montaje en Unity

1. Fuera de Play, abre **Modulo3** y ejecuta **Network Simulator → Module 03 → Actualizar recorrido y presentación del tutorial**. Si aún no tienes flujo, usa **Sprint 8 - Flujo guiado y objetivos**, que también llama al nuevo configurador.
2. Selecciona `_Managers/Module03 guided flow`. Revisa `Module03GuidedFlow.guidance` y `Module03PresentationController.replacement`: debe referenciar la instancia existente `Replacement-01`, registrada en `CableRepairController.cables`. Déjala activa en Editor para inicializar su física; el presenter oculta la reserva al comenzar.
3. En `Module03GuidancePresenter`, acomoda los cinco waypoints. El configurador crea solamente los que faltan y conserva tus posiciones existentes. Las posiciones nuevas son propuestas de edición, no navegación automática que detecte paredes.
4. Mantén el repuesto en un lugar accesible cerca de PC-01. Se presenta activando su única instancia; no se instancia otro cable con el mismo ID. Después de verificar PC-01, el averiado se desactiva al soltarlo de manos/tester. Se reactiva al reiniciar, en lugar de destruir un binding requerido por la sesión.
5. Guarda la escena. Los Canvas continúan siendo objetos fijos preparados en Editor. El configurador no crea pantallas durante Play.

| Campo del Inspector | Posición y uso |
| --- | --- |
| Initial Waypoint | Frente al jugador, con espacio libre para el NPC. |
| Laptop Entry Waypoint | `Waypoint_laptop1`, esquina/entrada del cuarto. |
| Laptop Waypoint | `Waypoint_laptop2`, junto a la laptop dentro del cuarto. Conserva el waypoint anterior. |
| Cable Waypoint | `Waypoint_PC01`, frente al puesto de PC-01. Conserva el waypoint anterior. |
| Quiz Waypoint | Junto al Canvas fijo del quiz, sin taparlo. |

Recorridos: inicial → laptop1 → laptop2; laptop2 → laptop1 → PC01; PC01 → laptop1 → laptop2; laptop2 → laptop1 → quiz. Cada tramo espera la llegada del NPC. No se vuelve a ordenar ir a laptop2 cuando el NPC ya está allí.

## Orden y acciones habilitadas

El orden se lee en `Module03GuidancePresenter.RunSequence()`. `Say`, `Move` y `WaitFor` separan locución, desplazamiento y espera de evidencia. B confirma los diálogos; completar una instrucción permite continuar sin una confirmación adicional. Confirmar un diálogo **no completa** el objetivo. Los recordatorios aparecen solamente durante las esperas.

| Objetivo visible | Evidencia para avanzar | Desbloqueo |
| --- | --- | --- |
| 1 Abrir terminal | Evento de apertura de la laptop con A; otra PC no cuenta. | Apertura tras introducción del cuarto. |
| 2 Primer diagnóstico | Ping desde Laptop/eth0 hacia PC-01 en sesión/revisión actual, aunque falle. | Ping tras explicar interfaz y ARP. |
| 3 Reparar cable | Prueba del averiado y del repuesto, instalación, dos etiquetas y ping vigente desde laptop hacia PC-01. | Agarre de cables, tester y etiquetadora al llegar a PC-01. Repuesto visible. |
| 4 Habilitar puerto | Consulta del switch y P02 habilitado. | Mutaciones de puertos tras su explicación. |
| 5 Resolver IP duplicada | Reglas de IP correctas y pings vigentes desde laptop a PC-02 y PC-03. | Edición local de IP tras explicar el conflicto. |
| 6 Verificar red | Todas las reglas y pings vigentes a las tres PCs. | Cierre; el quiz espera el recorrido del NPC si hay tutorial. |

Las consultas de datos/ARP/bitácora y Rayos X no requieren desbloqueos adicionales una vez abierta la terminal. Los permisos quedan acumulados: se puede repetir una reparación anterior. El filtro de selección bloquea las manos, pero conserva los sockets XR para no provocar desconexiones al bloquear una etapa.

**Tutorial Enabled = false:** se omiten diálogos y recorridos; permanecen los seis objetivos y el orden de desbloqueo por evidencia. El quiz no espera al NPC. Si faltan waypoints con tutorial activado, se registra un error y se libera la práctica sin narración en vez de dejar un bloqueo imposible. Aplicar el configurador antes de validar el tutorial.

## Scripts añadidos o modificados

| Script | Cambio | Objetivo o función |
| --- | --- | --- |
| `Domain/DiagnosticTutorialProgress.cs` | Añadido | Evidencia de apertura/primer ping y política de desbloqueo independiente de Unity. Rechaza otra sesión, revisión, origen o destino. |
| `Presentation/Module03PresentationController.cs` | Añadido | Visibilidad de repuesto/averiado y filtro XR de agarre que conserva sockets. |
| `Editor/…/Module03TutorialSetup.cs` | Añadido | Migra referencias y crea los waypoints faltantes sin reemplazar posiciones existentes. |
| `Presentation/Module03GuidancePresenter.cs` | Modificado | Orden completo de narración, recorridos, esperas, recordatorios y tutorial opcional. |
| `Flow/Module03GuidedFlow.cs` | Modificado | Integra los dos objetivos iniciales, evidencia, permisos y reinicio. |
| `Flow/Module03FlowController.cs` | Modificado | Admite seis objetivos y conserva compatibilidad con datos anteriores de cuatro. |
| `GameData/…/Module03FlowSettings.cs` | Modificado | Referencia a NPCDialogueData en lugar de diálogos incrustados. |
| `Presentation/DiagnosticScreenController.cs` | Modificado | Bloquea apertura/ping/cambios de puerto/IP según etapa; comunica rechazo en la consola. |
| `Interaction/CableRepairTool.cs`, `XRCableTester.cs` | Modificados | Comprueban permiso antes de iniciar prueba o activar etiqueta. |
| `Interaction/CableRepairController.cs` | Modificado | Reactiva cables retirados antes de restaurar conexiones al reiniciar. |
| `Presentation/Module03FinaleController.cs` | Modificado | Espera final de recorrido si hay tutorial; mantiene la validación real de la red. |
| `Editor/…/Module03GuidedFlowSetup.cs` | Modificado | Valida los seis objetivos y el nuevo asset; reaplicar migra el flujo existente. |

Rutas runtime relativas a `Assets/Scripts/Modules/Module03_Diagnostics`; GameData bajo `Assets/GameData/Module03/Definitions`; Editor bajo `Assets/Editor/Modules/Module03_Diagnostics`.

## Guion y precisiones académicas

`Guiones_Modulos_M3.docx` conserva las páginas del Lobby y módulos 1/2 del documento aportado, y desarrolla el apartado Módulo 3 con el mismo formato. Los textos locutados coinciden con los IDs de `Module03Tutorial.asset`; contiene instrucciones no locutadas de espera, desbloqueos y recorrido. El original externo no se modifica.

- ARP resuelve IP a MAC en el enlace; no determina dónde está físicamente una PC. Referencia: [RFC 826](https://www.rfc-editor.org/rfc/rfc826.html).
- El ping de verificación permanece laptop → PC-01, consistente con la evidencia del incidente. ICMP/IP/Ethernet se distinguen en la explicación: [RFC 792](https://www.rfc-editor.org/rfc/rfc792.html).
- El modelo convencional de canal es 100 m incluyendo 90 m permanentes y 10 m de latiguillos en conjunto. Los 5 m del puesto se expresan como criterio de la práctica, no como límite universal aislado. Referencia: [Fluke Networks](https://www.flukenetworks.com/blog/cabling-chronicles/top-10-cable-testing-mistakes).
- El cierre indica guardado al entregar el quiz, que coincide con `Module03FinaleController`, con reintento si falla. No promete que volver al Lobby por sí mismo guarde.

## Validación

Compilación externa con Roslyn de Unity sin errores; 802 comprobaciones de dominio/flujo, incluyendo la matriz de permisos con/sin narración, evidencias inválidas, seis objetivos y reinicio. No equivale a una prueba XR ni a ejecutar el configurador dentro del Editor.

Prueba manual pendiente: abrir laptop tras introducción, ping fallido, recorrer la entrada en L sin chocar, intentar agarrar antes de la etapa (los sockets deben seguir conectados), completar tester/etiquetas/ping, soltar averiado, habilitar P02, corregir PC-03, repetir tres pings y llegar al quiz. Repetir sin tutorial y con `RestartPractice()`. Los audios siguen sin asignarse; el guion funciona con texto.
