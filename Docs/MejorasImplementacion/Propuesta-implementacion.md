# Material para ampliar el capítulo 4. Implementación

Revisión del 22 de octubre de 2026. Basada en el PDF entregado y en los scripts y assets actuales del repositorio. Los textos propuestos pueden incorporarse al documento fuente; el PDF original y la aplicación no se modificaron. Las figuras F-A a F-E son esquemas de elaboración propia; no son capturas de Unity. La numeración es provisional y debe sustituirse por títulos automáticos al integrarlas.

## 1. Ubicación de las incorporaciones

Las páginas indicadas son las impresas en el documento; entre paréntesis aparece la página del archivo PDF.

| Apartado actual | Incorporación recomendada | Material |
| --- | --- | --- |
| 4.7.2, p. 161 (190) | Después de la tabla de objetivos, explicar el estado de cada extremo y sus transiciones. | Figura F-B y tabla de transiciones. |
| 4.7.3, pp. 161–162 (190–191) | Añadir diseño del rack, inventario, montaje, colliders y conexiones. Puede dividirse en subapartados sin renumerar todavía el capítulo. | Figura F-E, inventario del rack y plan C01–C05. |
| 4.7.4, pp. 162–164 (191–193) | Antes de explicar los objetivos, presentar la red de oficina; después, explicar el grafo operacional y BFS. | Figura F-C, inventarios y Figura F-D. |
| 4.8.1, p. 165 (194) | Complementar la Figura 72, que actualmente explica componentes, con el detalle geométrico de puntos y joints. | Figura F-A, parámetros y guía para captura real. |
| 4.8.3, pp. 166–167 (195–196) | Actualizar las adaptaciones y distinguir las variantes de interacción. | Correcciones de la sección 7 de este archivo. |

El capítulo ya tiene una Figura 72: conviene conservarla como vista de arquitectura y añadir el detalle de puntos inmediatamente después, o sustituirla si se desea reducir el número de figuras.

## 2. Física de cables: texto para incorporar en 4.8.1

La Figura F-A muestra la diferencia entre los puntos físicos, las uniones y los segmentos visibles del cable. Cada punto es un objeto con un componente `Rigidbody`; los puntos consecutivos y los cuerpos de los extremos se enlazan mediante `SpringJoint`. La unión es un componente de uno de los objetos y referencia al cuerpo vecino mediante `connectedBody`; no es un segmento geométrico adicional. Los objetos denominados `connector0` y `Part_i_Conn` representan visualmente los tramos del cable y se actualizan a partir de las posiciones de los cuerpos. Estos tramos deben distinguirse de la clase `Connector`, empleada para los plugs y sockets.

En la configuración base de `PhysicCable.SetSpirng`, el parámetro `springForce` se asigna a `SpringJoint.spring` con un valor predeterminado de 200. Este parámetro controla la rigidez de la unión; no significa que el cable reciba una fuerza constante de 200 N. El valor de `damper` se establece en 0.2 y contribuye a reducir la oscilación relativa de los cuerpos. Ambos anclajes se sitúan en el origen local de sus respectivos objetos y se desactiva su configuración automática. Las distancias mínima y máxima se igualan a `space`, que define la separación de referencia. Esta configuración emplea un resorte y, por tanto, no constituye por sí sola un límite rígido de estiramiento.

La apariencia del cable se actualiza en `Update`: cada tramo visible se coloca en el punto medio de dos cuerpos consecutivos, se orienta mediante `LookRotation` y modifica su escala longitudinal según la distancia que los separa. Por ello, la continuidad visual y la simulación física son responsabilidades relacionadas pero distintas. La conexión de un extremo a un puerto utiliza `Connector` y un `FixedJoint`, mientras que las uniones internas continúan describiendo la flexibilidad del cable.

El proyecto también incorpora configuraciones específicas. `PlaceBetween` redistribuye los cuerpos entre dos posiciones y recalcula las distancias de las uniones. Cuando se invoca `LimitStretch`, se modifica el amortiguamiento y se añaden `ConfigurableJoint` con límites de traslación; además, se incrementan las iteraciones del solucionador. Estos ajustes deben describirse como adaptaciones y documentarse con los valores de la instancia utilizada, pues no equivalen a los valores base de `SetSpirng`.

**Pie propuesto:** Figura F-A. Puntos físicos, uniones elásticas y tramos visuales de un cable del simulador. Las líneas en zigzag representan `SpringJoint` y las líneas gruesas representan la geometría visible. Fuente: elaboración propia a partir de `PhysicCable` y `Connector`.

### Tabla de parámetros verificables

| Elemento | Valor o regla verificada | Interpretación y alcance |
| --- | --- | --- |
| `springForce` → `spring.spring` | 200, predeterminado del script | Rigidez del resorte; comprobar overrides del prefab y de la escena. |
| `spring.damper` | 0.2 en `SetSpirng` | Amortiguamiento base; `LimitStretch` puede cambiarlo. |
| `autoConfigureConnectedAnchor` | `false` | Los anclajes se asignan explícitamente. |
| `anchor`, `connectedAnchor` | `(0,0,0)` | Orígenes locales de ambos cuerpos. |
| `minDistance`, `maxDistance` | `space` en `SetSpirng` | Distancia de referencia; `PlaceBetween` puede recalcularla por tramo. |
| `numberOfPoints` | 3 por defecto; 16 en `PatchCord_M03.prefab` | Puntos internos; con dos extremos, N puntos internos producen N+1 tramos. |
| `space` | 0.3 por defecto; 0.05 en `PatchCord_M03.prefab` | Separación configurada, expresada en unidades del mundo de Unity. |
| `size` | 0.3 por defecto; 0.012 en `PatchCord_M03.prefab` | Escala transversal del tramo; no asumir un diámetro físico sin revisar la malla. |
| `minBrakeTime` | 1 s por defecto y en `PatchCord_M03.prefab` | Tiempo de exceso de longitud antes de desconectar en la comprobación base. |
| `brakeLengthMultiplier` | 2 por defecto | El umbral base se calcula en `Start` con `space × numberOfPoints × multiplier + 2`; `LimitStretch` lo sustituye. |
| `LimitStretch` | fracción limitada a 0–0.25; proyección 0.02; iteraciones ≥12 y ≥4 | Adaptación opcional; no afirmar que todas las instancias la utilizan. |

Los valores del prefab son datos serializados, no mediciones durante la ejecución. No deben presentarse como parámetros universales ni como resultados de calibración experimental. El amortiguamiento del `Rigidbody` tampoco es el mismo parámetro que `SpringJoint.damper`.

### Referencia de Internet solicitada

La documentación histórica de Unity incluye una imagen del Inspector de `Spring Joint`:

- [Página oficial de Unity 2018.4 que contiene la imagen](https://docs.unity3d.com/2018.4/Documentation/Manual/class-SpringJoint.html).
- [Archivo PNG de la imagen, enlazado por esa página](https://docs.unity3d.com/2018.4/Documentation/uploads/Main/Inspector-SpringJoint.png).
- [Referencia de propiedades para Unity 6.2, versión del proyecto](https://docs.unity3d.com/6000.2/Documentation/Manual/class-SpringJoint.html).
- [Repositorio original Cable-physics](https://github.com/Hrober0/Cable-physics).

La página histórica sirve para ilustrar el componente. Sus valores y su interfaz pertenecen a otra versión y no son evidencia de la configuración del prototipo. El enlace PNG se obtuvo del HTML oficial; el visor web no pudo previsualizar directamente ese archivo. Si el enlace directo no abre, utilizar la página que lo contiene.

Referencia bibliográfica sugerida, adaptando el número al estilo del documento: Unity Technologies, «Spring Joint», *Unity User Manual 2018.4*. [En línea]. Disponible en la página oficial enlazada. [Consulta: 22-oct-2026]. Para explicar las propiedades de la versión empleada, agregar la referencia del manual de Unity 6.2. Pie: «Inspector del componente Spring Joint. Fuente: Unity Technologies, Unity User Manual 2018.4». No atribuir esa captura a elaboración propia.

### Capturas reales que conviene añadir

1. En una copia de trabajo o una sesión de inspección sin guardar cambios, abrir el prefab del cable elegido. Activar Gizmos y seleccionar un `Point`. Encuadrar el cable de lado con varios puntos visibles y el Inspector de `Rigidbody` y `Spring Joint`.
2. Hacer una segunda captura del objeto raíz con `PhysicCable`: deben leerse `Number Of Points`, `Space`, `Size`, `Spring Force` y los parámetros de desconexión. Un único Inspector del joint no muestra todos esos campos.
3. En ejecución, capturar el mismo punto para comprobar si se modificaron distancias, amortiguamiento o límites. Registrar prefab/escena, versión de Unity y si la captura corresponde a Edit Mode o Play Mode.
4. Añadir flechas discretas a «Point / Rigidbody», «SpringJoint», «Connected Body» y «tramo visual». El `Joint` puede señalarse en el Inspector aunque su gizmo no se distinga en la escena.

**Pie propuesto para la captura propia:** Configuración del cable [nombre de prefab] y de una unión interna en Unity 6000.2.7f2, en [Edit/Play Mode]. Fuente: captura del proyecto. Sustituir los corchetes únicamente después de tomarla. No se ha fabricado una captura del Editor para este paquete.

## 3. Módulo 1: máquina de estados del cable

### Texto para incorporar en 4.7.2

El estado de fabricación se almacena de manera independiente para los extremos izquierdo y derecho. `CableStateController` utiliza un diccionario cuya clave es `CableEnd` y cuyo valor es `CableState`; de esta forma, es posible que un extremo ya esté ponchado mientras el otro permanezca entero. La Figura F-B representa la secuencia que se aplica a cualquiera de los dos extremos. Los cinco estados implementados son `Whole`, `Peeled`, `Rj45Disordered`, `Rj45Ordered` y `Rj45Crimped`.

La acción de pelar transforma un extremo entero en uno pelado. Posteriormente, la interacción de tomar y soltar el extremo pelado prepara el ordenamiento y lo lleva a `Rj45Disordered`. La validación correcta de los ocho hilos conforme a T568B permite avanzar a `Rj45Ordered`; una disposición incorrecta mantiene el estado y permite corregir el puzzle. La crimpadora solo puede completar el ponchado de un extremo ordenado. Cada transición se somete tanto a los permisos del objetivo activo como a las condiciones del estado actual.

`TryAdvance` acepta únicamente el estado siguiente de la enumeración. No implementa saltos ni retrocesos. Los componentes de interacción complementan esta restricción al comprobar la acción, el extremo y el orden de los hilos antes de solicitar el cambio. Tras una transición válida, `StateChanged` informa del nuevo estado y los eventos de la mecánica permiten actualizar objetivos y presentación.

Cuando ambos extremos alcanzan `Rj45Crimped`, `CanTest` se vuelve verdadero. La práctica exige además conectar los dos extremos al tester y ejecutar la acción de prueba. La conexión al tester y el resultado de validación pertenecen al flujo del módulo: no existen estados adicionales `Connected`, `Tested` o `Validated` en esta enumeración. La comprobación implementada confirma las condiciones de fabricación y conexión y reproduce una respuesta didáctica; no constituye una medición eléctrica independiente hilo por hilo.

**Pie propuesto:** Figura F-B. Máquina de estados de fabricación por extremo y condiciones de validación del cable completo. Fuente: elaboración propia a partir de `CableStateController`, los componentes de estado y `TesterTool`.

| Estado de origen | Acción y condiciones | Estado de destino | Resultado si se rechaza |
| --- | --- | --- | --- |
| `Whole` | Pelar el extremo esperado por el objetivo activo. | `Peeled` | Conservar estado; informar rechazo cuando lo emite el validador. |
| `Peeled` | Preparar el puzzle mediante la interacción de tomar y soltar; permiso `PrepareOrder`. | `Rj45Disordered` | Conservar estado. |
| `Rj45Disordered` | Validar ocho hilos en el orden requerido; permiso `Order`. | `Rj45Ordered` | Permanecer en el puzzle y corregir los hilos. |
| `Rj45Ordered` | Aplicar crimpadora al extremo autorizado. | `Rj45Crimped` | Conservar estado. |
| Ambos extremos `Rj45Crimped` | Dos extremos conectados al tester y objetivo `validate_cable`; ejecutar prueba. | Sin cambio en `CableState` | No emitir validación si faltan condiciones. |

La secuencia didáctica normal termina primero el extremo izquierdo y luego el derecho. Esa secuencia la impone `ModuleActionValidator` y el orden de objetivos; no es una dependencia estructural entre las dos máquinas. Las opciones `initialLeftState` e `initialRightState` permiten iniciar estados de depuración y deben quedar en `Whole` para demostrar el flujo normal.

## 4. Módulo 2: diseño del rack, interacción y conexiones

### Texto para incorporar

El rack integra modelos de equipos, objetos de interacción XR, volúmenes de colisión y puntos de conexión identificados. Estos elementos cumplen funciones diferentes: el modelo comunica la apariencia del dispositivo; los colliders delimitan sus zonas físicas o de interacción; los componentes XR gestionan la manipulación; y `DeviceIdentity` y `NetworkPort` proporcionan identificadores utilizados por las validaciones. Por ello, la presencia visual de un puerto no implica que ese puerto participe en la práctica.

La distribución implementada se encuentra en `_Interactuables/RackDevices` de `Modulo2`. Las alturas y profundidades se ajustaron con referencia a los volúmenes del modelo `racksFF.blend`. SW1 es el equipo que manipula e instala el estudiante, mientras que SW2 permanece fijo y proporciona el enlace Ethernet adicional `SW2/Gi04`. El conjunto incluye elementos de energía, comunicaciones, organización, ventilación y almacenamiento. Algunos tienen funciones de reconocimiento mediante fichas y otros intervienen directamente en los objetivos. El esquema de rack de 22U adjunto puede presentarse como propuesta de distribución, pero requiere actualizarse para representar la integración actual antes de llamarlo «rack implementado».

### Inventario del rack implementado

Las alturas proceden de `Docs/Module02-RackIntegrado.md` y están expresadas en el marco del modelo; no son coordenadas mundiales ni números de unidad U. La altura global inicial de ese marco es 0.024 m. No convertir estos valores a posiciones U sin verificar el origen y los pasos de montaje del modelo.

| Elemento | Cantidad | Altura de referencia del centro (m) | Participación en la práctica |
| --- | ---: | ---: | --- |
| PDU horizontal, `PDU-A` | 1 | 1.86107 | Alimentación mediante `PDU-A/AC01`. |
| NAS 2U | 1 | 1.75076 | Modelo y ficha informativa; no participa en C01–C05. |
| Panel de fibra | 1 | 1.34697 | Reconocimiento del componente; no participa en C01–C05. |
| Organizador superior | 1 | 1.28897 | Organización y reconocimiento del cableado. |
| Patch panel UTP, `PP-A` | 1 | 1.10205 | Puertos `/01`, `/02`, `/03` usados por el plan. |
| Switch fijo, `SW2` | 1 | 1.00182 | Enlace `SW2/Gi04` hacia SW1. |
| Router | 1 | 0.88407 | Modelo de reconocimiento; no interviene en C01–C05. |
| Organizador inferior provisional | 1 | 0.81897 | Representación y ficha de cableado estructurado. |
| Bandeja de ventilación | 1 | 0.69784 | Representación de infraestructura y reconocimiento. |
| Switch manipulable, `SW1` | 1 | 0.60896, posición de instalación | Montaje, fijación, conexiones, consola y configuración. |
| Servidor 2U | 1 | 0.40614 | Reconocimiento; no participa en C01–C05. |
| UPS 2U | 1 | 0.17606 | Modelo y ficha; no participa en C01–C05. |
| PDU vertical 0U | 1 | Lateral posterior | Ficha independiente; no confundir con PDU-A. |

Respecto a la imagen adjunta, deben revisarse especialmente el firewall, que ya no aporta un puerto activo `FW1`; la posición de SW1; la incorporación de NAS y PDU horizontal; y las reservas o paneles ciegos. Conservar una vista de diseño y otra de la escena final permite explicar los cambios sin afirmar que ambas son idénticas.

### Montaje y colliders

La inserción se implementa como un desplazamiento guiado entre `EntryPose` e `InstalledPose`. `RackInsertionTrigger` detecta colliders candidatos y delega en `RackInsertionSlot.Consider`. La captura requiere que el dispositivo esté autorizado, que se encuentre agarrado, que corresponda al objetivo de montaje y que respete la tolerancia lateral y angular. Tras la captura, `RackInsertionGrabTransformer` proyecta la pose sobre el recorrido del riel. Al alcanzar el progreso de finalización, el switch queda instalado, se estabiliza su cuerpo y se deshabilita el agarre del equipo.

La instalación sobre el riel y la fijación son condiciones distintas. `RackFasteningAssembly` exige que el equipo correcto esté instalado y que las dos zonas de tornillo hayan sido completadas. Cada `RackScrewZone` utiliza un `SphereCollider` como trigger y acumula tiempo cuando detecta una punta de destornillador utilizable durante el objetivo permitido. Si se interrumpe la interacción antes de finalizar, el tiempo de esa fijación se reinicia. Se trata de una aproximación didáctica del atornillado basada en permanencia; no simula rosca ni torque.

| Volumen o componente | Función | Validación asociada |
| --- | --- | --- |
| Collider sólido del switch y `XRGrabInteractable` | Delimitar el volumen físico y de agarre. | Dispositivo manipulable y permiso de agarre. |
| Trigger de inserción | Detectar candidatos en la entrada del rack. | Tipo de objeto aceptado, agarre activo, etapa, distancia y orientación. |
| `SphereCollider` trigger de tornillo | Detectar proximidad de la punta de destornillador. | Equipo instalado, herramienta utilizable, etapa y tiempo continuo. |
| Collider del plug | Permitir contacto y agarre del extremo. | Interacción XR y estado del conector. |
| Trigger/socket del puerto | Detectar o seleccionar un plug candidato. | Tipo de puerto, ocupación, compatibilidad y permiso del flujo. |
| Volumen de objetivo informativo | Permitir enfocar un equipo y consultar su ficha. | Target identificado y contenido informativo asignado. |

Los triggers detectan presencia; no sustituyen la validación de la operación ni funcionan como una barrera sólida. Para la figura conviene mostrar el modelo con transparencia y superponer volúmenes de distinto trazo, con una leyenda que separe collider sólido, trigger de inserción y zona de tornillo.

Valores predeterminados de los scripts: tolerancia lateral 0.12, ángulo 25°, progreso de instalación 0.96 y permanencia de tornillo 2 s. Antes de publicarlos como valores efectivos, comprobar los campos serializados de la escena: estos parámetros pueden sobrescribirse. Debe justificarse su elección mediante facilidad de manipulación y precisión requerida, o indicar que se ajustaron de forma empírica sin atribuirles una optimización no medida.

### Mecanismo de conexiones

En los puertos configurados para XRI, `Module02NetworkSocket` extiende `XRSocketInteractor` y filtra candidatos durante hover y selección. Comprueba que existan el puerto y el plug, que el cable tenga propietario y que el tipo de cable coincida con `NetworkPort.Kind`. El cálculo de la pose de unión compensa la diferencia entre el pivote de agarre y la punta real del conector, de modo que ambos extremos del cable puedan alinearse correctamente con el socket. Después de la selección espera un fotograma para permitir que XRI restaure el estado físico del agarre anterior y entonces solicita `Connector.Connect`.

En los puertos que no contienen un `XRSocketInteractor`, `CableEndSocketDetector` ofrece otra vía de conexión. Mantiene un conjunto de puertos cercanos mediante triggers, busca el compatible más próximo y trata de conectarlo cuando se libera el extremo. El detector excluye los puertos administrados por sockets XR, por lo que ambas vías no intentan efectuar el mismo snap. Esta variante no debe describirse como el mecanismo único del módulo 2.

Antes de efectuar la unión, `Connector.CanConnect` comprueba que los extremos sean distintos, estén disponibles, tengan tipos Male/Female complementarios y cumplan las reglas de color cuando estas estén activas. `CablePortCompatibility` añade la coincidencia del tipo de cable y puerto y consulta los permisos del módulo. La unión física se alinea en `connectionPoint` y utiliza un `FixedJoint`; `PatchCableLink` registra los puertos relacionados para que la evaluación consulte la conexión lógica.

La compatibilidad local no equivale a completar el plan de instalación. Una conexión puede pertenecer al tipo correcto y, aun así, unir puertos diferentes de los exigidos. `Module02CablingState` compara las conexiones registradas con `Module02ConnectionPlan`; el objetivo requiere los cinco enlaces previstos. El emparejamiento acepta ambos sentidos y comprueba también el tipo de cable y de los dos puertos.

| Cable | Origen | Destino | Tipo |
| --- | --- | --- | --- |
| C01 | `PDU-A/AC01` | `SW1/Power` | Alimentación |
| C02 | `PP-A/01` | `SW1/Gi01` | Ethernet RJ45 |
| C03 | `PP-A/02` | `SW1/Gi02` | Ethernet RJ45 |
| C04 | `PP-A/03` | `SW1/Gi03` | Ethernet RJ45 |
| C05 | `SW2/Gi04` | `SW1/Gi04` | Ethernet RJ45 |

Al completar los cinco enlaces, el coordinador bloquea las conexiones y fija los cuerpos de los cables antes de comenzar el etiquetado. Las etiquetas continúan disponibles y se comprueban en ambos extremos, hasta completar diez. La conexión de consola es independiente de esos cinco enlaces. Para encender el switch se comprueba el cableado y el etiquetado; la configuración final utiliza su propio puzzle. De esta manera, ocupación, compatibilidad, cumplimiento del plan, etiquetas y encendido son comprobaciones diferentes.

**Pie propuesto:** Figura F-E. Detección, validación y registro de una conexión en el módulo 2. Se distinguen la compatibilidad del puerto y la validación de los cinco enlaces del plan. Fuente: elaboración propia.

## 5. Módulo 3: topología e inventario de red

### Texto para incorporar antes de los objetivos

La práctica utiliza una red de oficina perteneciente al segmento `office`. La Figura F-C representa la laptop de diagnóstico, un switch, un patch panel, tres rosetas y tres computadoras. La laptop se conecta directamente a `SW-01/P08`; cada computadora se enlaza con un puerto del switch a través de un cable de rack, una continuidad pasiva del patch panel, un tramo horizontal, una continuidad pasiva de la roseta y un patch cord de usuario. La continuidad interna de los elementos pasivos forma parte del modelo lógico y no representa un cable adicional del inventario.

El diseño separa la topología física del estado operacional. Al inicio, `Patch-01` está averiado, `SW-01/P02` se encuentra deshabilitado y PC-02 y PC-03 comparten la dirección `192.168.10.12/24`. Las conexiones siguen presentes como parte del diseño, pero el grafo utilizado para determinar la conectividad excluye los cables no íntegros y los enlaces cuyos extremos están deshabilitados. El conflicto IP se detecta al evaluar interfaces alcanzables; no se elimina una arista únicamente porque sus equipos tengan una dirección duplicada.

El modelo representa una LAN sin gateway y no realiza encaminamiento entre subredes. La interfaz `SW-01/mgmt` dispone de una dirección de administración; esa dirección no se asigna a cada puerto Ethernet del switch. Los puertos del panel y de las rosetas tampoco necesitan IP para proporcionar continuidad.

**Pie propuesto:** Figura F-C. Topología física por puertos de la red de oficina y fallas iniciales del módulo 3. Los enlaces continuos representan cables; las continuidades pasivas se muestran con línea discontinua. El bloque del switch resume la comunicación interna entre sus puertos habilitados. Fuente: elaboración propia a partir de `OfficeInitialState.asset`.

### Inventario de dispositivos e interfaces

| Elemento | Cantidad | Interfaces modeladas | IPv4 inicial | IPv4 objetivo o función |
| --- | ---: | --- | --- | --- |
| `Laptop` | 1 | `eth0` | 192.168.10.100/24 | Origen del diagnóstico. |
| `SW-01` | 1 | `P01`, `P02`, `P03`, `P08`, `mgmt` | mgmt: 192.168.10.2/24 | P01/P02/P03 habilitados; P08 habilitado inicialmente. |
| `PP-01` | 1 | `front01`–`front03`, `back01`–`back03` | No aplica | Tres continuidades internas front/back. |
| `R01`, `R02`, `R03` | 3 | `back`, `front` por roseta | No aplica | Tres continuidades internas back/front. |
| `PC-01` | 1 | `eth0` | 192.168.10.11/24 | 192.168.10.11/24. |
| `PC-02` | 1 | `eth0` | 192.168.10.12/24 | 192.168.10.12/24. |
| `PC-03` | 1 | `eth0` | 192.168.10.12/24, duplicada | 192.168.10.13/24. |

Totales de la definición inicial: 9 dispositivos, 21 puertos, 11 registros de cable y 6 continuidades pasivas. Diez cables tienen sus extremos asignados; el undécimo, `Replacement-01`, es un repuesto desconectado. De los diez conectados, uno está averiado. Solo se modelan los puertos indicados, aunque la malla del switch represente más conectores.

### Inventario de enlaces

| Cable | Puerto A | Puerto B | Estado inicial | Manipulación |
| --- | --- | --- | --- | --- |
| `LaptopLink` | `SW-01/P08` | `Laptop/eth0` | Íntegro | Fijo |
| `Rack-01` | `SW-01/P01` | `PP-01/front01` | Íntegro | Fijo |
| `Horizontal-01` | `PP-01/back01` | `R01/back` | Íntegro | Fijo |
| `Patch-01` | `R01/front` | `PC-01/eth0` | Averiado | Interactivo |
| `Rack-02` | `SW-01/P02` | `PP-01/front02` | Íntegro; no conduce por P02 deshabilitado | Fijo |
| `Horizontal-02` | `PP-01/back02` | `R02/back` | Íntegro | Fijo |
| `Patch-02` | `R02/front` | `PC-02/eth0` | Íntegro | Interactivo |
| `Rack-03` | `SW-01/P03` | `PP-01/front03` | Íntegro | Fijo |
| `Horizontal-03` | `PP-01/back03` | `R03/back` | Íntegro | Fijo |
| `Patch-03` | `R03/front` | `PC-03/eth0` | Íntegro | Interactivo |
| `Replacement-01` | Sin asignar | Sin asignar | Íntegro | Repuesto interactivo |

No atribuir categorías comerciales, longitudes reales, velocidad de enlace ni modelos de fabricante: esos datos no están definidos por este inventario lógico. Si se desean añadir, obtenerlos de los assets o especificarlos explícitamente como decisiones de diseño.

## 6. Módulo 3: BFS y simulación de ping

### Texto para incorporar después de la topología

`NetworkSimulationService` construye un grafo operacional no dirigido cuyos vértices son identificadores de puertos. Las aristas representan tres tipos de relación: cables íntegros conectados, continuidades pasivas y comunicación interna entre puertos del switch. Para añadir una arista, ambos extremos deben estar habilitados y pertenecer al mismo segmento. En el switch se examinan todas las parejas de puertos y se añade la relación en ambos sentidos. El bloque de conmutación mostrado en la topología es una simplificación visual de estas relaciones, no un vértice adicional del algoritmo.

La búsqueda en anchura comienza en la interfaz de origen, normalmente `Laptop/eth0`. Se utiliza una cola FIFO para los puertos pendientes, un conjunto `Visited` para evitar visitas repetidas y un diccionario `Parents` para registrar la arista por la que se descubrió cada puerto. La colección `Flood` conserva las aristas del árbol de descubrimiento. Los vecinos se ordenan por identificador de destino y después por identificador de arista, de manera que el recorrido sea reproducible.

El algoritmo explora todo el componente alcanzable y no se detiene al encontrar la primera interfaz que responde a la IP consultada. Esto permite identificar varios equipos alcanzables que reclamen la misma dirección. Si existe un único destino válido, `ReconstructPath` retrocede por `Parents` hasta el origen y revierte la lista para obtener el camino de ida. La presentación utiliza los tramos resultantes para representar las fases ARP e ICMP.

La Figura F-D muestra los niveles de descubrimiento para la red reparada y un ejemplo de reconstrucción hacia PC-01. Los niveles cuentan aristas del modelo: incluyen continuidad pasiva y comunicación interna del switch. No son valores de TTL, saltos de router ni tiempos de propagación. BFS obtiene un camino con el menor número de aristas del grafo no ponderado; no calcula la ruta más rápida ni la de menor longitud física.

La simulación utiliza ese alcance para decidir si la dirección está sin resolver, si presenta conflicto o si es posible producir solicitudes y respuestas ICMP. La representación visual ocurre después del cálculo y no determina el éxito del ping. La caché de vecinos pertenece a cada interfaz de origen y se invalida cuando cambia la revisión de la red. BFS no implementa un protocolo real de encaminamiento, aprendizaje MAC, STP ni una pila ARP/ICMP del sistema operativo.

**Pie propuesto:** Figura F-D. Niveles de descubrimiento de BFS desde `Laptop/eth0` en la red reparada y reconstrucción del camino hacia `PC-01/eth0`. Las capas corresponden a aristas del modelo lógico. Fuente: elaboración propia a partir de `TraverseBreadthFirst` y `ReconstructPath`.

### Secuencia breve del algoritmo

```text
Agregar origen a Visited y a la cola.
Mientras la cola no esté vacía:
    current = retirar el primer puerto.
    Para cada arista saliente en el orden definido:
        Si el destino ya está en Visited, continuar.
        Agregar destino a Visited.
        Parents[destino] = arista.
        Agregar arista a Flood y destino a la cola.
Al terminar, filtrar interfaces alcanzables con IPv4 y MAC válidas.
Resolver la consulta de dirección y sus conflictos.
Si existe un único destino, reconstruir camino mediante Parents.
```

### Ejemplo comprobable con los datos del módulo

| Nivel BFS, red reparada | Puertos descubiertos |
| --- | --- |
| 0 | `Laptop/eth0` |
| 1 | `SW-01/P08` |
| 2 | `SW-01/P01`, `SW-01/P02`, `SW-01/P03`, `SW-01/mgmt` |
| 3 | `PP-01/front01`, `PP-01/front02`, `PP-01/front03` |
| 4 | `PP-01/back01`, `PP-01/back02`, `PP-01/back03` |
| 5 | `R01/back`, `R02/back`, `R03/back` |
| 6 | `R01/front`, `R02/front`, `R03/front` |
| 7 | `PC-01/eth0`, `PC-02/eth0`, `PC-03/eth0` |

El camino a PC-01 contiene siete aristas: `Laptop/eth0 → SW-01/P08 → SW-01/P01 → PP-01/front01 → PP-01/back01 → R01/back → R01/front → PC-01/eth0`. En el estado reparado, la última arista corresponde a `Replacement-01`, pues `Patch-01` se retira.

En el estado inicial, la rama 1 se alcanza hasta `R01/front` y no llega a PC-01. La rama 2 queda aislada del origen por P02, aunque sus cables estén íntegros. PC-03 sí es alcanzable; por ello, la IP duplicada no produce dos candidatos remotos mientras PC-02 permanezca aislada. Después de habilitar P02, las dos interfaces con `192.168.10.12` resultan alcanzables y la consulta puede detectar el conflicto. La reparación física y la habilitación de puerto cambian las aristas operativas; la corrección IP cambia la resolución de direcciones.

En la construcción, conectar por parejas los p puertos del switch tiene un costo cuadrático en p. Para un grafo ya construido, BFS es lineal en vértices y aristas si no se ordenan los vecinos. Esta implementación además ordena cada lista de adyacencia: conviene mencionar ese costo si se hace un análisis de complejidad, en lugar de atribuir O(V+E) a todo el servicio de ping sin matices.

## 7. Correcciones puntuales antes de integrar las figuras

| Ubicación | Hallazgo | Corrección |
| --- | --- | --- |
| 4.7.3, p. 162 | El texto dice que el bloqueo ocurre después del encendido. | Sustituir por bloqueo al completar los cinco enlaces, antes de etiquetado, consola y encendido. Lo hacen `Update` y `OnCablingChanged`. |
| Fragmento de reglas de 4.7.3 | `Label` incluye `!cablesLocked`. | Actualizar: `Label => step >= 5 && step <= 7`; se etiqueta con los cables ya bloqueados. |
| Imagen de rack adjunta | Incluye firewall y una distribución distinta. | Presentarla como diseño previo o actualizarla contra la escena actual; no conservar `FW1/eth01` como puerto operativo. |
| Tabla 91, `RebuildConfiguredPoints` | Dice que las herramientas crean dimensiones mediante `SendMessage`. | Describir los métodos públicos `ConfigureDimensions` y `RebuildConfiguredPoints`; este último llama directamente a `UpdatePoints`. |
| Tabla 91, compatibilidad | Da a entender que el filtro comprueba el puerto exacto del plan. | Separar compatibilidad local (tipo, ocupación, permisos) del cumplimiento global de `Module02ConnectionPlan`. |
| 4.8.1 | Presenta resortes y amortiguamiento como garantía general de estabilidad. | Explicar que 200 y 0.2 son ajustes base; documentar overrides, límites y resultados observados por variante. |
| 4.8.1 | «Ruptura» puede sugerir daño persistente del cable. | Aclarar que el exceso de longitud desconecta extremos; no equivale a la falla lógica `intact=false` del módulo 3. |
| 4.8.3 | Mezcla los estados `Whole/Peeled/Crimped/Tested` con los de fabricación. | Identificar a qué componente pertenece cada enumeración; la máquina M1 tiene cinco estados y no incluye `Tested`. |
| 4.8, código compartido | Puede interpretarse que todo el código físico está en `Shared/Cabling`. | Aclarar que el código base físico está en `Modules/Module03_Diagnostics/Cable_physics/Scripts` y conserva `HPhysic`; `Shared/Cabling` integra puertos y vínculos reutilizables. |
| Índice del PDF | Hay entradas con «¡Error! Marcador no definido.». | Reparar referencias cruzadas y actualizar todos los campos en el documento fuente antes de exportar. |
| Tablas y figuras nuevas | Cambian numeración y referencias existentes. | Insertar títulos y referencias automáticas; evitar numeración manual. |

## 8. Mejoras priorizadas para toda la sección de implementación

Son mejoras de documentación y evidencia. No se han ejecutado nuevas pruebas funcionales ni mediciones de rendimiento como parte de esta revisión.

| Prioridad | Apartado | Mejora concreta | Evidencia o entrega esperada |
| --- | --- | --- | --- |
| Alta | 4.7.2, M1 | Incorporar máquina por extremo y distinguirla del flujo de nueve objetivos. | F-B, tabla de transiciones y ejemplo de rechazo sin cambio de estado. |
| Alta | 4.7.3, M2 | Actualizar rack y describir roles de cada equipo. | Inventario, vista frontal actual y comparación con diseño previo. |
| Alta | 4.7.3, M2 | Explicar collider sólido, trigger, riel, fijación y socket. | Capturas de Scene View con Gizmos y una leyenda. |
| Alta | 4.7.3, M2 | Corregir cuándo se bloquea el cableado y separar validaciones. | C01–C05, diez etiquetas y reglas actuales. |
| Alta | 4.7.4, M3 | Presentar red completa, puertos, continuidades y fallas. | F-C e inventarios de dispositivos y enlaces. |
| Alta | 4.7.4, M3 | Explicar BFS con un caso de la propia topología. | F-D, niveles, camino reconstruido y efecto de cada falla. |
| Alta | 4.8 | Complementar arquitectura con puntos y joints. | F-A y dos capturas reales de Inspector. |
| Alta | 4.8 | Separar parámetros base, del prefab y de ejecución. | Tabla con procedencia, unidad, valor y efecto observado. |
| Alta | Todo el capítulo | Corregir referencias rotas y afirmaciones desactualizadas. | Índices y referencias cruzadas regenerados; revisión contra una versión concreta del proyecto. |
| Media | 4.1, arquitectura | Justificar patrones con problemas concretos del prototipo. | Un ejemplo antes/decisión/efecto por patrón; distinguir enum + controlador de una implementación clásica State por objetos. |
| Media | 4.2, ScriptableObjects | Mostrar un asset real y cómo llega a un componente de escena. | Inspector legible y esquema de referencias, incluyendo datos del módulo 3 y su quiz. |
| Media | 4.3, UI y escenas | Explicar qué bloquea interacción en carga, pausa y diálogos. | Recorrido de una acción, estados de interfaz y captura con textos legibles. |
| Media | 4.4, prefabs | Diferenciar prefab, instancia y referencias serializadas. | Jerarquía de un prefab representativo y tabla de responsabilidades; evitar una lista exhaustiva sin explicación. |
| Media | 4.5, modelos | Documentar autoría, adaptación y función de cada grupo de modelos. | Tabla de procedencia/licencia y vistas con escala; incluir pivotes y separación visual/física cuando afecten al montaje. |
| Media | 4.6, entrada | Relacionar botón, mano, evento, acción y contexto. | Tabla de controles por módulo; indicar backend Meta/OpenXR y condiciones de vibración. |
| Media | 4.7, objetivos | Diferenciar permiso de acción, condición de completitud y feedback. | Un caso válido y uno rechazado por módulo, con condición que se evaluó. |
| Media | 4.9, puzzle | Mostrar un orden incorrecto y su corrección. | Captura del puzzle y regla de ocho posiciones; explicar T568B usado en la práctica frente a T568A disponible en datos. |
| Media | 4.10, herramientas | Explicar selección, equipamiento y aplicación sobre un target. | Secuencia de una herramienta que incluya proximidad, interfaz requerida y rechazo. |
| Media | 4.11, NPC | Separar guía narrativa, autorización de acciones y reacción. | Secuencia objetivo–evento–diálogo; documentar diálogos sin audio y condiciones de cancelación. |
| Media | 4.12, quiz | Precisar aprobación, reintento y relación con práctica completada. | Caso de aprobación y de reintento; cómo se conserva o sustituye el mejor resultado según el código. |
| Media | 4.13, persistencia | Hacer visible el comportamiento sin conexión y los conflictos de sincronización. | Diagrama local/nube, ejemplo de guardado y tabla de decisiones; usar datos ficticios. |
| Media | 4.14, autenticación | Explicar creación de sesión, error de acceso y retorno al menú. | Diagrama de secuencia y mensajes de error; evitar mostrar credenciales reales. |
| Media | 4.15, insignias | Relacionar condiciones de otorgamiento, persistencia y perfil. | Tabla por módulo, ejemplo de desbloqueo y repetición sin duplicación. |
| Media | 4.16, ajustes | Indicar valores persistentes, alcance entre escenas y momento de aplicación. | Matriz ajuste–servicio–persistencia–efecto visible. |
| Media | 4.17, audio | Relacionar buses del mixer, prioridad de voces, pausa y espacialización. | Captura del mixer y ejemplo de señal espacial; evitar atribuir claridad o mejora sin evaluación. |
| Media | Rendimiento transversal | Explicar decisiones para Quest 2 con mediciones reales. | Versión y dispositivo, escena, FPS/tiempo de frame, draw calls y costo de físicas; distinguir Editor de dispositivo. |
| Media | Pruebas y capítulo 5 | Conectar cada regla relevante con evidencia reproducible. | Matriz requisito–clase–prueba–resultado y limitaciones; no convertir una prueba de dominio en evidencia de interacción VR. |
| Baja | Mantenimiento | Reducir tablas que solo repiten nombres de scripts. | Conservar clases clave en el texto y trasladar catálogos extensos a anexo. |
| Baja | Presentación | Homogeneizar idioma, títulos, ortografía y tamaños de figura. | Usar un término principal para ponchado/crimpado; definir siglas; corregir «Tabla 82..» y espacios en títulos. |

## 9. Archivos y trazabilidad

Las figuras se entregan en PNG para inserción y SVG para edición, dentro de `figuras/`. El SVG conserva el texto como texto. Los inventarios del módulo 3 también se entregan en CSV UTF-8 con BOM para abrirlos en Excel.

| Evidencia | Fuente del proyecto |
| --- | --- |
| Estados y guardas M1 | `Assets/Scripts/Modules/Module01_CableMaking/Domain/Cable/CableState.cs`, `CableStateController.cs` y `CableStates/`; `Flow/Validation/ModuleActionValidator.cs`; `Interaction/TesterTool.cs`. |
| Parámetros y generación física | `Assets/Scripts/Modules/Module03_Diagnostics/Cable_physics/Scripts/PhysicCable.cs`; `Assets/Prefabs/Dispositivos/Modulo3/PatchCord_M03.prefab`. |
| Unión y compatibilidad | `Cable_physics/Scripts/Connector.cs`; `Assets/Scripts/Shared/Cabling/CablePortCompatibility.cs`, `CableEndSocketDetector.cs` y `PatchCableLink.cs`. |
| Rack e integración | `Docs/Module02-RackIntegrado.md`; `Assets/Scenes/Modulo2.unity`; `Assets/Editor/Module02RackIntegration.cs`. |
| Montaje y fijación | `Assets/Scripts/Modules/Module02_RackInstallation/Interaction/RackInsertionSlot.cs`, `RackInsertionTrigger.cs`, `RackFasteningAssembly.cs`, `RackScrewZone.cs`. |
| Socket XR y plan | `Interaction/Module02NetworkSocket.cs`; `Domain/Module02ConnectionPlan.cs`; `Flow/Module02SequenceCoordinator.cs`; `Flow/Validation/Module02SequenceRules.cs`. |
| Topología, IP y enlaces | `Assets/GameData/Module03/Topologies/OfficeInitialState.asset`; `Assets/GameData/Module03/Validation/OfficeTargetRules.asset`. |
| Construcción del grafo y BFS | `Assets/Scripts/Modules/Module03_Diagnostics/Domain/NetworkSimulationService.cs`; contexto adicional en `Docs/Module03-SimulationFlow.md`. |

El generador `generar_figuras.py` reproduce las figuras y los inventarios. El inventario y los niveles BFS se derivan de la definición actual; el caso reparado sustituye Patch-01 por Replacement-01, habilita P02 y corrige PC-03 a .13. Los demás esquemas sintetizan los scripts revisados. Revisar nuevamente si cambia la implementación antes de integrar el material definitivo.
