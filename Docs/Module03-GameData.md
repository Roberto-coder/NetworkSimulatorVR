# GameData del módulo 3

Los ScriptableObjects se editan en Assets/GameData/Module03. El estado runtime se copia: no se modifican los assets para reparar la red.

| Script añadido | Asset / ubicación | Objetivo o función |
| --- | --- | --- |
| NetworkInitialStateAsset.cs | Topologies/OfficeInitialState.asset | Dispositivos, IP, puertos, cables y fallas de inicio. |
| NetworkTargetRulesAsset.cs | Validation/OfficeTargetRules.asset | Condiciones semánticas de recuperación. |
| NetworkLayoutAsset.cs | Layouts/OfficeLayout.asset | Posiciones físicas, anclajes y splines. |
| NetworkPrefabCatalog.cs | Catalogs/ | Claves de dispositivo y prefabs asociados. |
| DiagnosticUiSettings.cs | Presentation/DiagnosticUiSettings.asset | Alcance/textos y autoría inicial del mapa con sprites. |
| ProbeVisualSettings.cs | Presentation/ProbeVisualSettings.asset | Colores, velocidad didáctica, material y sección roja. |

El prefab compartido se guarda fuera de GameData, en Assets/Prefabs/Dispositivos/Modulo3. Cambiar mapNodes no reconstruye automáticamente el prefab: su diseño posterior se edita en Prefab Mode. Los bindings A/B pertenecen a VRInputManager.

| Campo visual | Función |
| --- | --- |
| failureSectionLength | Longitud máxima del tramo rojo de salida (metros). |
| failureLineWidth | Grosor de la señal roja; al menos el ancho normal. |
| resultSeconds | Duración final para éxito; no limita el rojo de fallo. |
| animateMotion | Desactiva desplazamiento de esfera conservando resaltado. |
| xRayInitially | Estado inicial de la prueba de profundidad del overlay. |

Cada nuevo documento de sprint/GameData debe incluir una tabla de scripts añadidos/modificados y su función, además de configuración y pruebas.


## Sprint 6: reparación física

| Script añadido | Asset / ubicación | Objetivo o función |
| --- | --- | --- |
| CableRepairSettings.cs | Presentation/CableRepairSettings.asset | Regla del primer incidente, prefabs y radios de tester/etiquetadora. |

`incident` contiene faultyCableId, portA/B, labelAtA/B, probeSource, destinationIp y responderPort. Los modelos de cable/socket se reutilizan del módulo 2. Tester y etiquetadora se guardan como prefabs de módulo 3; la laptop continúa fija y fuera de la rueda.


`CableRepairSettings.patchCordPrefab` apunta ahora a `Assets/Prefabs/Dispositivos/Modulo3/PatchCord_M03.prefab`, generado por Module03CableRepairSetup a partir del RJ45 de módulo 2. Todos los patch cords usan instancias vinculadas; sus IDs y estado permanecen independientes.


## Ampliación tester XR

| Script modificado | Configuración | Función |
| --- | --- | --- |
| CableRepairSettings.cs | testerLedSeconds, testerLedOn, testerLedOff | Duración por LED y colores de ambas filas. |
| Module03XrTesterBuilder.cs | Tester_XR_M03.prefab | Copia visual del tester M1 con snaps XRI y LEDs asignados. |

El radio testerRadius es legado; el snap se ajusta mediante el SphereCollider de cada socket del prefab. Las herramientas se ubican en Assets/Prefabs/Tools/Module03; se conservan referencias a assets movidos por GUID.


## Sprint 7 · Administración de puertos e IP

El menú **Network Simulator > Module 03 > Sprint 7 - Puertos e IP** crea o reutiliza `Assets/GameData/Module03/Presentation/NetworkAdministrationSettings.asset`. Sus opciones de dirección son propuestas editables, no respuestas aplicadas automáticamente. `requiredRuleIds` y `verificationDeviceIds` determinan las reglas y pings necesarios. Deben contener IDs existentes y al menos una entrada; una configuración vacía no acredita el incidente.

| Script añadido/modificado | Función |
| --- | --- |
| `NetworkAdministrationSettings.cs` (añadido) | ScriptableObject con opciones de dirección, reglas finales y equipos a verificar. El prefijo se obtiene del inventario documentado; ya no existe prefixOptions. |
| `NetworkTargetRulesAsset.cs` (modificado) | Copia la topología y aplica las direcciones esperadas solo al inventario documentado. Conserva intactas las fallas iniciales. |
| `Module03AdministrationSetup.cs` (añadido) | Crea/asigna el asset, valida las reglas contra la red y actualiza controles fijos del prefab. |

Guía de aplicación, tabla completa de scripts y pruebas: [Sprint 7](Module03-Sprint07.md).


## Sprint 8 · Flujo guiado

| Asset | Función |
| --- | --- |
| `Module03/Presentation/Module03FlowSettings.asset` | Referencias a definición/reglas, fuente de pings, equipos requeridos y Module03Tutorial.asset. |
| `ModulesManagers/Module03_Diagnostics.asset` | Secuencia de seis objetivos, herramientas, quiz e insignia. |
| `Objectives/Module03/OB-01.asset` a `OB-04.asset` | Títulos, instrucciones y descripción para progreso de práctica. |

| Script añadido/modificado | Función |
| --- | --- |
| `Module03FlowSettings.cs` (nuevo) | Esquema de configuración narrativa y verificación, sin estado mutable de sesión. |
| `Module03GuidedFlowSetup.cs` (nuevo) | Conecta los assets con la escena y copia las herramientas existentes a la definición. |

Los clips aún no están asignados: puede usarse el texto y agregar audios desde Inspector. Los textos y audios ahora viven en `Module03Tutorial.asset`; `Module03FlowSettings` sólo lo referencia. Tabla completa de scripts y aplicación: [Sprint 8](Module03-Sprint08.md).


Revisión de presentación del sprint 8: los textos IN/OB/AY/PI se reutilizan como diálogos contextuales y recordatorios; ya no se presenta un catálogo de ayudas. Tutorial Enabled y Reminder Interval se configuran en Module03GuidancePresenter de escena; el progreso permanece independiente del tutorial.


### Física de patch cords tras mover dispositivos

| Script modificado | Parámetros / función |
| --- | --- |
| CableRepairSettings.cs | patchCordLength (4 m), maximumStretchFraction (0.08), springDamping (5): longitud, tolerancia por tramo y amortiguación, usados al iniciar/reiniciar M3. |
| RepairPatchCord.cs | slackDirection en la instancia de escena para orientar la holgura sin cambiar la topología. |

Aplicar en Editor con Recolocar patch cords desde sockets. Ver sección final del documento Sprint06 para límites y comprobaciones de física.


Estado inicial corregido: OfficeInitialState mantiene SW-01/P02 apagado y PC-02/PC-03 con .12/24, declaradas en allowedDuplicatePortIds. Las reglas finales conservan .13 para PC-03. Cambios al asset requieren una sesión nueva (salir/entrar Play). La tabla de scripts de acreditación está en la revisión final de Sprint08.


## Sprint 9 · Quiz y cierre

Module03Quiz.asset en Assets/GameData/Quiz contiene ocho preguntas y porcentaje formativo de 70. El configurador asigna el quiz y la insignia existente Module03Completion a Module03_Diagnostics. No se guardan resultados de ejecución en los ScriptableObjects.

| Script añadido/modificado | Función |
| --- | --- |
| Module03FinaleSetup.cs (nuevo) | Asigna Final Quiz y Completion Achievement, conservando objetivos y herramientas. |
| Module03FinaleController.cs (nuevo) | Consume configuración del módulo para evaluación y persistencia local. |
| Module03GuidedFlow.cs (modificado) | Expone validación inmediata para el cierre. |

Aplicación y criterios en [Sprint 9](Module03-Sprint09.md).

## Revisión narrativa secuencial

Fuente única de diálogos: `Module03/Module03Tutorial.asset` (M3D01–26 y M3R01–07). Se añaden objetivos IN-01/IN-02 antes de los cuatro OB existentes. Fallas en OfficeInitialState, soluciones en OfficeTargetRules. La tabla completa de scripts añadidos/modificados, montaje y permisos está en [Tutorial secuencial](Module03-TutorialSequence.md).

El orden narrativo se construye ahora en `Presentation/Tutorial/Module03TutorialBuilder.cs`, utilizando `TutorialSequence`/`TutorialStep` y el evento `TutorialDirector.TutorialCompleted`. Los textos siguen en el mismo asset; no hay migración de diálogos. Véase la tabla de scripts y diagnóstico del quiz en [Tutorial secuencial](Module03-TutorialSequence.md#builder-y-activación-del-quiz).
