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
