# Módulo 3: diagnóstico y recuperación de una red de oficina

Propuesta de alcance y arquitectura. Este documento no implica que las mecánicas ya estén implementadas. Fecha: 20 de septiembre de 2026.

## 1. Experiencia propuesta

El usuario recibe tres reportes de conectividad en una oficina. Su trabajo es observar el síntoma, obtener evidencia con la laptop y las herramientas, corregir la causa y comprobar la recuperación. La meta educativa es distinguir una falla física, una configuración de puerto y un conflicto de direccionamiento.

Recomiendo una práctica guiada con incidentes secuenciales y una evaluación final de toda la red. Duración objetivo de diseño: 20–30 minutos, por ajustar después de pruebas de uso. Los incidentes se activan al iniciar su etapa, con un aviso narrativo; no aparecen silenciosamente después de una reparación. Una variante libre con fallas combinadas queda para una segunda versión.

## 2. Oficina y topología

- Tres escritorios: PC-01 Recepción, PC-02 Administración y PC-03 Reuniones.
- Un rack de pared con switch administrable SW-01 y patch panel PP-01.
- Tres rosetas de pared y canaletas para dar sentido al cableado estructurado.
- Una mesa de servicio junto al rack para apoyar laptop, tester y repuestos.
- Un NPC y cuatro puntos de explicación: entrada, mesa de servicio, rack y escritorio afectado. Mantener zonas de interacción accesibles y recorridos despejados.

```text
Laptop (.100) ─────────────────────────── SW-01 (.2, gestión)
                                            │
              ┌─────────────────────────────┼───────────────────────────┐
              │ P01                         │ P02                       │ P03
            PP-01/01                      PP-01/02                    PP-01/03
              │                             │                           │
         Roseta R01                    Roseta R02                  Roseta R03
              │                             │                           │
         PC-01 (.11)                   PC-02 (.12)                  PC-03 (.13)
```

Red propuesta: `192.168.10.0/24`. Laptop en puerto P08 del switch. Los demás puertos quedan fuera de la práctica. El patch panel y las rosetas son elementos pasivos: tienen identificador físico y continuidad, pero no IP ni comportamiento de switch. El MVP opera en una sola subred y no requiere router, Internet, DNS ni DHCP. Si se agrega un gateway más adelante, tendrá una función educativa explícita.

La estación lógica de la laptop permanece conectada en la mesa de servicio mediante un enlace fijo. La herramienta de la rueda abre y cierra una pantalla ampliada dentro de las zonas de trabajo permitidas; cerrar la UI no desconecta la estación. La pantalla representa el acceso a esa estación, no una nueva conexión de red inalámbrica. Las consultas y permisos siguen dependiendo del enlace real de la estación y del contexto seleccionado.

Los únicos cables interactuables serán los patch cords entre rosetas y PCs, reutilizando los del módulo 2 o una variante. Los tramos del rack, rack–rosetas y conexión de la estación de diagnóstico serán fijos y no desconectables. Sus rutas se editarán mediante puntos o splines; no requieren rigidbodies.

## 3. Incidentes, evidencias y resolución

| Incidente | Evidencia observable | Acción | Condición de validación |
| --- | --- | --- | --- |
| Patch cord de PC-01 roto internamente | Ping sin respuesta; puerto administrativamente habilitado pero enlace caído; tester sin continuidad | Desconectar ambos extremos, probar cable, sustituirlo, etiquetar ambos extremos del reemplazo y reconectar PC-01 a R01 | Cable nuevo íntegro, etiquetas correspondientes en ambos extremos, conexiones correctas, enlace operativo y nuevo ping exitoso |
| Puerto P02 inhabilitado | PC-02 sin respuesta; cable conectado; consulta del switch muestra puerto deshabilitado administrativamente | Acceder al switch y habilitar P02 | P02 habilitado, enlace operativo y nuevo ping a PC-02 exitoso |
| PC-03 con la misma IP de PC-02 | Configuración local muestra la misma dirección en dos equipos; inventario esperado asigna .13 a PC-03 | Abrir la configuración local de PC-03 y asignar .13 con máscara /24 | IP válida y única, máscara correcta y pruebas nuevas hacia PC-02 y PC-03 exitosas |

En el tercer caso PC-02 conserva `.12`; PC-03 comienza también con `.12`. El inventario esperado y la configuración observada son paneles diferentes, con esas etiquetas explícitas. El inventario no revela automáticamente el estado real de dispositivos inaccesibles.

Un ping fallido no demuestra por sí solo una rotura. Tampoco una respuesta a una IP duplicada demuestra que respondió el equipo esperado. Para el simulador propongo un resultado determinista de conflicto cuando dos equipos activos y alcanzables reclamen la misma IP: presentarlo como simplificación didáctica, no como reproducción exacta de todos los sistemas operativos. No simular éxito al equipo deseado mientras siga la ambigüedad. Una duplicidad en un equipo desconectado puede detectarse al inspeccionar su configuración, pero no mediante una observación remota inventada.

El usuario puede consultar información en cualquier momento. Las modificaciones de la práctica guiada se habilitan según el incidente activo. La laptop explica los bloqueos y permite repetir pruebas. Un botón presionado o una respuesta de diagnóstico elegida nunca sustituye la comprobación del estado real.

## 4. Laptop y acceso a la información

Interfaz ampliada con botones grandes, selección con rayo XRI, texto breve y resultados desplazables. Sin teclado virtual obligatorio. Siempre mostrar origen, destino, comando equivalente y resultado de la última ejecución. La vista inicial es un mapa topológico abstracto; ver sección 14 para presentación y anclaje.

| Pantalla / acción | Información | Alcance |
| --- | --- | --- |
| Mi conexión / `ipconfig` | IP, máscara, MAC y estado de interfaz de la laptop | Solo información local |
| Probar conectividad / `ping <IP>` | Destino seleccionado o IP manual opcional, 4 intentos, respuestas y pérdidas simuladas | Desde la laptop conectada; tiempos rotulados como simulados |
| Vecinos / `arp -a` | Asociaciones IP–MAC aprendidas y observaciones de conflicto | Solo evidencia obtenida; no inventario global |
| Administración de SW-01 | Puerto, etiqueta del destino, conexión física, habilitación administrativa y enlace operativo | Sesión de gestión disponible solo si SW-01 es alcanzable |
| Habilitar puerto | Selección de P02 y aplicación de cambio | Acción de gestión del switch, no comando del sistema de la laptop |
| Inventario esperado | Nombres, IP previstas, rosetas y etiquetas de puertos | Documento de referencia de la oficina |
| Bitácora | Consultas, evidencia, cambios y verificaciones | Historial de la sesión |

La configuración IP de cada PC se consulta y modifica en un panel local al interactuar con ese escritorio. La laptop puede registrar esa evidencia, pero no editar mágicamente un equipo inaccesible. Este panel no añade una herramienta a la rueda. Si se quiere que absolutamente todo se haga en la laptop, la alternativa es una conexión de servicio local explícita; queda fuera del MVP.

La UI distingue campos administrativos y operativos: «habilitado» no equivale a «conectado», y «conectado» no equivale a «conectividad IP correcta».

## 5. Herramientas

- **Laptop:** herramienta principal; diagnóstico, administración del switch, inventario y bitácora.
- **Tester:** recomendado como herramienta secundaria para que el incidente físico requiera evidencia. Probar el patch cord aislado y desconectado por ambos extremos; reutilizar el concepto aprendido en el módulo 1.
- **Repuesto de cable:** consumible del escenario en un soporte cercano, no una nueva herramienta de menú.
- **Etiquetadora:** incluida en el alcance base, limitada al cable que se sustituye por rotura. Etiquetar los dos extremos del cable de reemplazo con la identificación del enlace afectado; validar contenido y asociación con el extremo, no solo presencia de texto. El cable retirado mantiene su identidad en la bitácora. Se interpreta «extremos del cable roto» como el enlace reparado: etiquetar únicamente el cable que se desecha no deja identificada la instalación final.

No incorporar crimpado como requisito: aquí la reparación consiste en sustituir el cable defectuoso por uno previamente validado.

## 6. Ping, animación y rayos X

El servicio de simulación resuelve el resultado; la animación reproduce un registro de esa prueba. La lógica no debe esperar a que una esfera colisione con un dispositivo para decidir si hubo comunicación.

Secuencia visual propuesta:

1. Seleccionar destino y ejecutar ping.
2. Resaltar únicamente el recorrido de esa prueba. Usar azul para solicitud y verde para respuesta, con flechas e iconos además del color.
3. Animar ida y vuelta por los segmentos y dispositivos correspondientes. El switch participa como dispositivo activo; el patch panel solo aporta continuidad.
4. Si falla, indicar ausencia de respuesta y el último estado observable. Mostrar rojo con un pulso suave en el punto conocido de interrupción, nunca paquetes atravesando un enlace roto o deshabilitado.
5. Con IP duplicada, mostrar advertencia de direccionamiento, no un cable rojo que sugiera rotura física.

Dos niveles de ayuda: en tutorial, tras obtener evidencia, se puede señalar la causa exacta; en evaluación, mostrar el resultado y la ruta observada sin revelar automáticamente dónde está la falla. Una vista didáctica de una ruta prevista debe etiquetarse como tal: no representa tráfico efectivamente transmitido.

Separar ruta Ethernet/ARP y ping ICMP en el modelo. Si no se resuelve el destino, no dibujar una respuesta ICMP. No hace falta implementar una pila de red completa: basta con reglas explícitas para esta topología, resolución de vecinos y trazas deterministas.

Para rayos X propongo una capa visual independiente sobre cables y paquetes, activable desde la laptop. Cambiar solo el orden de renderizado no garantiza visibilidad a través de objetos: también interviene la prueba de profundidad. Un prototipo puede dibujar el overlay con `ZTest Always` y `ZWrite Off`, preservando la apariencia normal del resto de la oficina. Las variantes y la integración exacta deben validarse en el URP del proyecto y en ambos ojos del visor. Referencias oficiales: [ZTest](https://docs.unity3d.com/6000.0/Documentation/Manual/SL-ZTest.html) y [ZWrite](https://docs.unity3d.com/6000.2/Documentation/Manual/SL-ZWrite.html).

No usar destellos rápidos. Ofrecer resaltado fijo como alternativa, ocultar la ayuda al terminar la prueba y evitar que el overlay tape la UI. Reutilizar pools de paquetes; no generar objetos por cada ping.

## 7. Objetivos y flujo

| ID propuesto | Objetivo en muñeca | Evidencia de finalización |
| --- | --- | --- |
| M03_01 | Preparar la estación de diagnóstico | Laptop conectada y configuración local consultada |
| M03_02 | Comprobar una conexión de referencia | Ping exitoso a SW-01 y lectura del inventario |
| M03_03 | Identificar la falla del cable de Recepción | Prueba fallida registrada y tester confirma defecto del cable correspondiente |
| M03_04 | Restablecer e identificar la conexión de Recepción | Sustitución correcta, ambos extremos del reemplazo etiquetados y ping nuevo exitoso |
| M03_05 | Identificar el estado del puerto de Administración | Ping fallido y consulta de P02 deshabilitado |
| M03_06 | Restablecer el puerto de Administración | Puerto habilitado y ping nuevo exitoso |
| M03_07 | Identificar el conflicto de direcciones | Configuraciones de PC-02 y PC-03 consultadas y coincidencia identificada |
| M03_08 | Corregir la dirección de Reuniones | PC-03 con .13/24 única y verificaciones nuevas de ambos equipos |
| M03_09 | Verificar la red completa | Pruebas a SW-01 y las tres PCs sobre el estado actual; sin incidentes pendientes |

Después se presenta el quiz. Mantener separados «práctica completada», «quiz entregado» y «módulo completado», tomando el módulo 2 como patrón. La política de calificación debe seguir la del proyecto; no introducir un porcentaje nuevo sin revisar el comportamiento compartido.

Una prueba queda ligada a una revisión del estado de red. Cambiar una IP, habilitar un puerto o desconectar un cable invalida resultados anteriores para la validación final. Si el usuario rompe una conexión ya reparada, el historial conserva el objetivo cumplido, pero la comprobación final exige corregir el estado actual.

El tutorial presenta y orienta; no completa objetivos. Sus waypoints llevan al área de trabajo correspondiente. Límite narrativo: una explicación inicial y una instrucción breve por objetivo, sin comentarios automáticos repetitivos. Los resultados se comunican visualmente; ayudas adicionales solo a petición. Antes de implementar diálogos, audios o assets narrativos se redactará un guion en un documento aparte para validación del usuario.

Quiz sugerido: diferencia entre enlace físico y conectividad IP; límite de lo que demuestra ping; puerto deshabilitado frente a cable dañado; identificación de IP duplicada; necesidad de verificar después de reparar. Preguntas y retroalimentación en assets editables.

## 8. Reutilización comprobada en el repositorio

- `Core.Objectives.IObjectiveFlow`, `ObjectiveController`, `ObjectiveData` y `ModuleDefinition`: contratos de objetivos y contenido.
- `Module02FlowController` y `Module02ObjectiveFactory`: patrón de creación y avance con validación del estado.
- `ModuleSceneConfiguration`: composición de referencias de escena.
- `TutorialDirector`, builders/controllers de módulos anteriores, muñeca, rueda y quiz: integración existente que debe verificarse con el nuevo flujo.
- `Shared.Cabling.NetworkPort`: identidad de puertos, conexión física y LED operativo.
- `Shared.Cabling.PatchCableLink`: base para relacionar extremos físicos; añadir adaptación al modelo de red.
- `Shared.Cabling.CableTrafficVisualizer`: pool y recorrido de paquetes sobre cables físicos. Actualmente permite tráfico periódico al habilitar transmisión; requiere separar claramente modo demo y emisión puntual por prueba para este módulo.
- `TerminalPuzzleView` del módulo 2: referencia de interacción XRI, no interfaz de diagnóstico ya resuelta.
- `Assets/GameData/Module03/Module03Tutorial.asset`: existe, con lista de diálogos vacía; revisar su tipo y conservar GUID al integrarlo.

`Module03_Diagnostics` ya contiene recursos de física de cables utilizados desde código compartido. No tratar esa carpeta como vacía ni mover sus scripts/prefabs durante la implementación inicial.

## 9. Arquitectura propuesta

```text
Assets/Scripts/Modules/Module03_Diagnostics/
  Domain/
    NetworkState, DeviceState, PortState, LinkState
    NetworkSimulationService, AddressConflictRules, DiagnosticEvidence
    ProbeResult, ProbeTrace, NetworkRevision
  Objectives/
    Module03Objective, Module03ObjectiveCatalog
  Factories/
    Module03ObjectiveFactory
  Flow/
    Module03FlowController, Module03IncidentCoordinator
    Validation/Module03CompletionRules
  Interaction/
    Laptop/, DeviceInspection/, PortManagement/, CableReplacement/
  Presentation/
    Module03PresentationController
    Laptop/, NetworkVisualization/, Tutorial/, Quiz/
  Module03Manager
  Cable_physics/                 # existente; conservar

Assets/Editor/Modules/Module03_Diagnostics/
  Configuradores de escena y validadores de assets

Assets/Prefabs/Tools/Module03/
Assets/Prefabs/Dispositivos/Modulo3/
Assets/Scenes/Modulo3.unity
Tests/Module03Diagnostics/
```

Son nombres propuestos, no archivos existentes. Domain contiene reglas C# sin UI, GameObjects ni física Unity. Interaction traduce acciones físicas a solicitudes; Flow decide cuándo y cómo se aplican y valida objetivos; Presentation refleja los resultados. Los adaptadores conectan IDs estables del modelo con objetos de escena.

La representación visual nunca modifica el resultado de ping. Un cambio físico actualiza primero NetworkState y su revisión; después se notifican UI, LEDs y objetivos. Si cambia la red durante una animación, cancelar la traza y pedir una nueva prueba, evitando validar un resultado obsoleto.

## 10. ScriptableObjects en Assets/GameData

Mantener la capitalización y las convenciones existentes. Todo contenido configurable del módulo se aloja bajo `Assets/GameData`; no incrustar guion, IP iniciales o condiciones de incidentes en scripts de UI.

```text
Assets/GameData/Module03/
  Definitions/           # tipos ScriptableObject específicos
  Topologies/            # OfficeInitialState.asset
  Validation/            # OfficeTargetRules.asset
  Layouts/               # distribución física, rutas y mapa abstracto
  Catalogs/              # claves de prefab y tipos de dispositivos
  Incidents/             # BrokenCable, DisabledPort, DuplicateIp
  Commands/              # definiciones de acciones y textos de ayuda
  Visualization/         # colores, velocidad, tiempos y opciones de ayuda
  Tutorial/              # contenido adicional si resulta necesario
  Module03Tutorial.asset # conservar asset existente
Assets/GameData/Objectives/Module03/
Assets/GameData/Modules/Module03_Diagnostics.asset
Assets/GameData/Quiz/Module03Quiz.asset
Assets/GameData/Achievements/Module03Completion.asset # ya existe
```

| Dato propuesto | Contenido |
| --- | --- |
| NetworkInitialStateDefinition | IDs de equipos/puertos, MAC, IP y máscara iniciales, enlaces y continuidad de elementos pasivos |
| NetworkTargetDefinition | Restricciones finales por ID y reglas globales; no copia literal de todo el estado |
| NetworkLayoutDefinition / DevicePrefabCatalog | Anclajes físicos, posiciones 2D del mapa, rutas y claves de prefab, separados de las reglas de red |
| NetworkIncidentDefinition | Tipo de falla, ID afectado, cambio inicial, evidencia requerida y corrección esperada |
| DiagnosticCommandDefinition | ID de acción, etiqueta, ayuda, parámetros y contextos permitidos |
| NetworkVisualizationSettings | Colores, tamaño y velocidad de paquetes, duración del resaltado, modo de ayuda |
| Datos existentes de objetivos, tutorial y quiz | Títulos, instrucciones, diálogo, preguntas y retroalimentación |

Al iniciar se construye un estado de ejecución independiente a partir de los assets. Nunca guardar en el ScriptableObject compartido una IP editada, cable reparado, puerto habilitado, caché ARP o progreso de la sesión. Reiniciar debe reconstruir el estado y limpiar evidencias y animaciones.

Validación de configuración: IDs únicos, extremos existentes, correspondencia entre objetos de escena y topología, MAC distintas y objetivos referenciados. La duplicidad IP solo se permite donde el incidente la declara explícitamente. Las referencias a objetos de escena permanecen en el manager/adaptadores, no en assets del proyecto.

## 11. Entregas y criterios de aceptación

1. **Dominio y datos:** configuración inicial, reglas finales, cargador por IDs y validación con diferencias legibles; red sana con tres PCs. Pruebas .NET de ida/vuelta, desconexión, puerto deshabilitado, duplicidad, resultados obsoletos y reinicio. Comprobar que agregar una PC no requiere modificar algoritmos ni UI.
2. **Corte vertical:** pantalla ampliada y mapa abstracto, un destino y un ping visible con ida/vuelta. Herramienta de edición de rutas fijas y prototipo de rayos X en el visor antes de construir la oficina completa.
3. **Incidente físico:** diagnóstico con tester, sustitución, etiquetado de ambos extremos y validación de conexiones. Caso negativo: cable en puerto equivocado o etiqueta incorrecta no completa el objetivo.
4. **Puerto y direcciones:** consulta/gestión del switch, panel local de PCs y conflicto controlado. Caso negativo: habilitar otro puerto o cambiar una IP sin verificar no completa el objetivo.
5. **Flujo completo:** redactar y validar primero el guion en documento aparte; después integrar factory, muñeca, rueda, NPC, waypoints, presentación, reinicio y registro de evidencia. Caso negativo: tutorial o UI no pueden completar una reparación pendiente.
6. **Cierre y QA:** quiz, logro, lobby y comprobación integral. Probar texto legible, entrada XRI, overlay en ambos ojos, cancelación de pruebas y rendimiento en el dispositivo objetivo.

Cada entrega deja una práctica verificable. No fijar duración de desarrollo hasta probar la interacción del cable, la laptop y el overlay en hardware.

## 12. Decisiones recomendadas para comenzar

Adoptar tres PCs iniciales con carga escalable por datos, una subred, rack de pared, laptop con pantalla ampliada y mapa abstracto, tester y etiquetadora limitada al enlace reparado, y tres incidentes secuenciales. Incluir configuración local en cada PC. Usar rayos X como ayuda activable y separar síntoma de causa confirmada. Dejar lanzamiento de paquetes, falla de rendimiento, fallas aleatorias, router, DHCP, DNS y consola de texto libre como ampliaciones.

Mejora de mayor valor después del MVP: un reto final con la misma oficina y otra ubicación de la falla, sin guía paso a paso. Permite comprobar que el usuario aprendió a diagnosticar y no solo a repetir una secuencia.

## 13. Configuración inicial, estado vivo y validación final

La propuesta de dos archivos es adecuada con una mejora: el segundo describe condiciones de éxito, en lugar de exigir igualdad literal con una copia completa de la red. Mantener dos assets principales, `OfficeInitialState.asset` y `OfficeTargetRules.asset`, más referencias a catálogos, presentación e incidentes. MVP significa producto mínimo viable: aquí, la primera versión completa y comprobable del módulo, antes de ampliaciones opcionales.

Flujo: configuración inicial + incidentes de la etapa → estado de ejecución → acciones del usuario → validación por reglas + evidencia reciente → progreso y logros.

### Datos y fuentes de verdad

- **Inicial:** versión del esquema, ID de escenario, dispositivos con ID estable y clave de prefab, interfaces/IP/máscara/MAC, puertos, cables con ID propio, extremos, integridad, etiquetas y enlaces fijos. Las IP pertenecen a interfaces; los elementos pasivos no reciben IP.
- **Actual:** copia C# independiente, modificable durante la sesión. Es la fuente de verdad de la simulación y se puede exportar como snapshot JSON versionado para guardado o depuración. La UI y los GameObjects reflejan este estado.
- **Final esperado:** predicados explícitos: IP exacta cuando el inventario lo exige, dirección única dentro del segmento, máscara correcta, puerto habilitado, cable íntegro entre extremos requeridos, etiquetas correctas y conectividad entre los pares indicados. Identificar el enlace requerido por sus extremos, sin exigir el ID del cable retirado.
- **Derivados:** vecinos físicos a partir de enlaces; rutas y conectividad a partir de estado y reglas. La caché ARP contiene observaciones aprendidas en ejecución. No serializar listas de vecinos redundantes que puedan contradecir los enlaces; una caché inicial solo se admite si un futuro ejercicio lo requiere explícitamente.
- **Presentación:** clave de anclaje/posición física, posición 2D independiente en el mapa y rutas de cable. Catálogo de prefabs como asset separado; los DTO de dominio usan IDs y claves, sin referencias Unity.

Los ScriptableObjects son la fuente editable principal para esta versión. JSON es una representación de intercambio/guardado del mismo esquema, no una segunda configuración que deba mantenerse manualmente sincronizada.

### Carga e instanciación

Validar esquema, IDs y referencias antes de crear objetos. Crear dispositivos desde catálogo o enlazar objetos ya colocados mediante IDs; nunca instanciar y registrar dos veces el mismo nodo. Resolver puertos y extremos en una segunda pasada, construir el grafo y finalmente enlazar vistas, mapa y flujo. Agregar una PC requiere registrar sus datos, puertos, enlace, ubicación y reglas aplicables; no cambiar un arreglo fijo de tres equipos. La ampliación también necesita un puerto libre y espacio físico, que el validador debe comprobar.

Los vecinos y recorridos se recalculan cuando cambia la topología; no mediante búsquedas continuas de GameObjects. La distribución abstracta puede comenzar con posiciones editables y, más adelante, añadir distribución automática sin alterar la simulación.

### Comparación semántica

El validador devuelve una lista de resultados por regla con ID, entidad, valor esperado, observado y motivo. Ignorar orden de listas, posición visual, tiempos, paquetes animados y caché ARP al comparar salud de red. Permitir un cable de reemplazo con ID nuevo. Aplicar restricciones globales a todos los dispositivos relevantes para que una PC recién añadida no quede fuera de controles de unicidad o conectividad por olvido.

Ejemplo de reglas, expresado conceptualmente:

```text
PC-03/eth0: IPv4 = 192.168.10.13, máscara = /24
Segmento oficina: todas las IPv4 asignadas deben ser únicas
SW-01/P02: administrativamente habilitado
R01/PC ↔ PC-01/eth0: cable íntegro y etiquetas esperadas en ambos extremos
Laptop → cada PC requerida: prueba exitosa posterior al último cambio de red
```

Separar «red sana» de «práctica completada»: esta última exige también el tester, etiquetado, evidencias pedagógicas y verificaciones solicitadas. La regla técnica de salud no debe depender de abrir un diálogo.

Con incidentes secuenciales, el estado inicial y las reglas finales no bastan por sí solos: el escenario referencia las modificaciones de cada incidente. Aplicarlas una sola vez al entrar en la etapa, registrarlas como eventos del escenario y marcar qué incidentes se presentaron/resolvieron. No completar el módulo antes de presentar todos los obligatorios, aunque la red inicial esté sana. Guardar también etapa y registro de incidentes para que recargar no reintroduzca una falla reparada.

### Registro y logros

Registrar acciones aceptadas y rechazos relevantes con ID de evento, secuencia, instante, origen (usuario/escenario), entidad afectada, valores anterior/nuevo, resultado y revisión de red. Registrar pruebas y evidencia de tester con identidad del objeto probado. No almacenar cada frame ni cada posición de un paquete.

Los logros se conceden al cumplirse reglas y evidencia, una sola vez por clave de logro y sesión; la persistencia global utiliza el servicio existente. El historial no se revoca si aparece otra falla, pero la salud actual se vuelve a evaluar. Empezar con snapshot de sesión y bitácora; reconstrucción completa por reproducción de eventos no es necesaria para este alcance.

## 14. Mapa abstracto y pantalla ampliada

La pantalla inicial muestra un diagrama lógico inspirado en herramientas de simulación de redes. Nodos y conexiones proceden de los mismos IDs y enlaces del modelo, con posiciones 2D independientes de las coordenadas físicas. Permitir seleccionar un nodo para abrir sus consultas y elegirlo como destino de prueba. Añadir zoom/desplazamiento cuando aumente el número de equipos. Patch panel y rosetas pueden agruparse y desplegarse como detalles físicos.

Separar «topología documentada» de «estado observado»: al inicio se conocen los equipos previstos, pero su salud aparece sin comprobar. Mostrar fecha/revisión de cada observación y marcarla como obsoleta si cambia la red. No convertir el mapa en un revelador automático de todas las fallas. La selección visual de una PC no concede permisos para editarla remotamente.

La herramienta abre un panel en un anclaje de la zona de trabajo (mesa/rack o escritorio), orientado hacia el usuario al abrirse y estable en el espacio después. Priorizar anclajes preparados; si se usa posición relativa al usuario, comprobar espacio libre antes de mostrarlo. Botón para recentrar, distancia y escala ajustables. No pegarlo continuamente a la cabeza ni hacerlo perseguir al usuario. Al salir del área, ocultar o suspender interacción conservando la sesión; al volver se puede reabrir. Evitar solapamiento con objetos manipulables y reservar entrada XRI a la UI visible. En escritorio, el contexto local se indica explícitamente; las pruebas siguen mostrando su origen real.

## 15. Autoría de cables fijos

Recomendación: rutas editables dentro de Unity mediante puntos de control/splines, con generación de malla en Editor y asociación a un ID de enlace. Unity Splines ofrece extrusión de malla sobre curvas; evaluar la versión compatible antes de añadir la dependencia. Referencia: [Spline Extrude](https://docs.unity.cn/Packages/com.unity.splines%402.9/manual/extrude-component.html).

Herramienta mínima: seleccionar extremo A y B, añadir puntos por canaletas, ajustar curvas/radio y regenerar. Reutilizar rutas de canaleta con desplazamientos para varios cables. El trazado sigue siendo dirigido por el diseñador; no prometer búsqueda automática de rutas alrededor de paredes. Conservar curva/polilínea y tabla de distancias para animar paquetes y rayos X, aunque la malla quede precalculada. Sin conectores agarrables, rigidbodies ni regeneración por frame en cables fijos.

Blender es apropiado para rack, canaletas y decoración. Modelar allí todo el cableado sería viable en una escena cerrada, pero cada cambio de topología exigiría retocar geometría y mantener además las rutas de animación. La edición en Unity permite reutilizar la misma ruta para geometría, selección y tráfico.

## 16. Ampliaciones de prioridad baja

**Lanzar el paquete:** después de seleccionar origen/destino, generar una esfera agarrable y lanzarla a una zona amplia de envío de la estación origen. Una recepción válida ejecuta exactamente el mismo comando de prueba que el botón; la fuerza no cambia latencia ni conectividad. Si no entra, permitir recuperar/reintentar sin penalizar conocimiento de redes. Conservar el botón como alternativa. Desde la recepción, el recorrido pasa a animación guiada por la traza, no a física libre entre cables. Implementar después del flujo completo.

**Cable inadecuado y rendimiento:** viable como incidente adicional, con requisitos explícitos de velocidad, capacidad de puertos, longitud/condición del enlace y perfil didáctico. No establecer «categoría menor = ping más lento» como regla universal ni asumir que un tester de continuidad certifica categoría/rendimiento. Una etiqueta de categoría no permite deducir por sí sola velocidad negociada o rendimiento; véase la distinción entre comprobación de cable y parámetros del puerto en [Fluke LinkIQ](https://www.flukenetworks.com/createpdf/en/edocs/linkiqtm-industrial-ethernet-cable-network-tester).

Agregar una prueba de transferencia de tamaño fijo y velocidad de enlace simulada. El usuario puede tener ping exitoso y aun así incumplir el rendimiento requerido. Visualizar menor caudal con menos paquetes por unidad de tiempo o una barra que tarda más; si se ralentiza el movimiento de esferas, rotularlo como escala didáctica, no velocidad real de propagación eléctrica. Validar sustitución por un cable admitido en el perfil y repetir transferencia. No introducir este incidente hasta terminar las tres fallas base.
