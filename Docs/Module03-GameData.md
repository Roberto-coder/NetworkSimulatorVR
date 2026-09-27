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
