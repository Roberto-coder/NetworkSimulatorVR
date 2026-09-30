# Flujo completo de la simulación de ping

Describe el código actual de NetworkSimulationService.Ping, no una pila de red del sistema operativo. La conectividad se calcula antes de animar. BFS descubre alcance en el modelo; no sustituye ARP real ni implementa STP.

## Datos que utiliza

| Nombre real | Tipo / contenido | Función |
| --- | --- | --- |
| network | NetworkDefinition | Snapshot de la sesión para esta consulta. |
| ports | Dictionary<string, PortDefinition> | ID de puerto → configuración. |
| graph | Dictionary<string, List<Edge>> | ID de puerto → conexiones operativas bidireccionales. |
| visited | HashSet<string> | Puertos ya descubiertos por BFS; evita ciclos. |
| queue | Queue<string> | Puertos pendientes de explorar. |
| parents | Dictionary<string, Edge> | Puerto descubierto → arista por la que se llegó por primera vez. |
| flood | List<Edge> | Árbol de descubrimiento BFS usado para representar difusión ARP. |
| reachable | PortDefinition[] | Puertos visitados que tienen IPv4 utilizable y MAC válida. |
| neighbours | Dictionary<string, Dictionary<string, NeighbourObservation>> | Puerto de origen → tabla IP → observación ARP. Persiste entre consultas de la misma revisión. |
| cache | Dictionary<string, NeighbourObservation> | Referencia a neighbours[source.id] para la consulta. |
| candidates | PortDefinition[] | Interfaces alcanzables que reclaman destinationIp. |
| request | List<Edge> | Camino hasta el destino reconstruido por Path usando parents. |
| trace | List<ProbeTraceStep> | Fases ARP/ICMP y tramos que después reproduce la presentación. |

## 1. Entrada, validación y grafo

```mermaid
flowchart TD
 A["UI: CommandExecuting limpia la traza anterior"] --> B["DiagnosticWorkspace.PingSelected: ExpectedAddress del destino"]
 B --> C["Ping: count entre 1 y 10"]
 C -->|Inválido| EX["Lanzar ArgumentOutOfRangeException; UI muestra mensaje"]
 C -->|Válido| D["RefreshCache: si cambió Revision, neighbours.Clear; actualizar cacheRevision"]
 D --> E["network = session.Snapshot; trace = lista vacía; ports = diccionario por ID; buscar source"]
 E --> F{"Origen con IP utilizable y MAC válida?"}
 F -->|No| F1["Finish: InvalidSource"]
 F -->|Sí| G{"source.enabled?"}
 G -->|No| G1["Finish: SourceDisabled"]
 G -->|Sí| H{"Destino IPv4 unicast?"}
 H -->|No| H1["Finish: InvalidDestination"]
 H -->|Sí| I{"Misma subred según máscara del origen?"}
 I -->|No| I1["Finish: NoRoute; sin gateway"]
 I -->|Sí| J{"Destino distinto de red y broadcast?"}
 J -->|No| H1
 J -->|Sí| K["graph: lista de aristas vacía por cada puerto"]
 K --> L["Link: cables intactos + continuidad pasiva + pares de puertos del switch"]
 L --> M["Añadir aristas en ambos sentidos sólo con extremos conectados, habilitados y mismo segmentId"]
 M --> N["Ejecutar BFS: diagrama 2"]
```

El for anidado del switch usa `i` y `j = i + 1`: enumera cada pareja una sola vez. `Link` crea ambas direcciones; no hace falta repetir la pareja invertida. No representa aprendizaje de tabla MAC.

## 2. BFS y reconstrucción

```mermaid
flowchart TD
 A["parents vacío; visited contiene source.id; queue contiene source.id; flood vacío"] --> B{"queue.Count > 0?"}
 B -->|Sí| C["Dequeue: tomar puerto; ordenar graph del puerto por To y después Id"]
 C --> D{"Queda una edge por revisar?"}
 D -->|No| B
 D -->|Sí| E{"visited.Add de edge.To devuelve true?"}
 E -->|No: ya conocido| D
 E -->|Sí: nuevo| F["parents[edge.To] = edge; flood.Add(edge); queue.Enqueue(edge.To)"]
 F --> D
 B -->|No| G["reachable = puertos visitados con IP utilizable y MAC válida"]
 G --> H["Continuar con conflictos y ARP: diagrama 3"]
 P["Path(to), cuando se solicita un camino"] --> Q{"to distinto de source.id?"}
 Q -->|Sí| R["edge = parents[to]; path.Add(edge); to = edge.From"]
 R --> Q
 Q -->|No| S["path.Reverse; devolver camino origen → destino"]
```

`parents` guarda por dónde se llegó; `visited` guarda a quién se llegó. El orden estable hace reproducibles las pruebas. Se recorre todo el componente alcanzable, no sólo hasta encontrar el destino: esto permite detectar respuestas ARP contradictorias.

## 3. ARP, conflictos e ICMP

```mermaid
flowchart TD
 A["reachable calculado"] --> B{"Más de un reachable con IP del origen?"}
 B -->|Sí| C["trace agrega ArpRequest de flood; Finish AddressConflict"]
 B -->|No| D{"destinationIp == source.ipv4?"}
 D -->|Sí| E["Finish Success local; sin probar cable ni generar tráfico"]
 D -->|No| F["Obtener o crear neighbours[source.id]; asignar cache"]
 F --> G{"cache contiene destinationIp?"}
 G -->|No| H["trace agrega ArpRequest de flood"]
 G -->|Sí| I["Omitir nueva solicitud ARP"]
 H --> J["candidates = reachable con IP destinationIp"]
 I --> J
 J --> K{"candidates.Length == 0?"}
 K -->|Sí| L["Finish AddressUnresolved"]
 K -->|No| M{"No estaba en cache?"}
 M -->|Sí| N["Por cada candidato ordenado: Path inverso; agregar ArpReply a trace"]
 M -->|No| O{"Hay más de un candidato?"}
 N --> O
 O -->|Sí| P["Finish AddressConflict; sin ICMP a destino ambiguo"]
 O -->|No| Q["destination = candidates[0]; cache[destinationIp] = NeighbourObservation"]
 Q --> R["request = Path(destination.id)"]
 R --> S["canReply: origen en subred del destino y dirección utilizable con su máscara"]
 S --> T["Repetir count: agregar EchoRequest de request; si canReply, EchoReply en reversa"]
 T --> U{"canReply?"}
 U -->|Sí| V["Finish Success"]
 U -->|No| W["Finish ReplyUnavailable"]
```

ARP asocia IP a MAC/interfaz, no calcula rutas de Internet. Aquí la caché sólo aprende un candidato único. En el modelo, la IP del origen duplicada se comprueba antes del ping local. Una IP duplicada en un equipo no alcanzable no aparece como conflicto remoto.

## 4. Resultado, historial y presentación

```mermaid
flowchart TD
 A["Toda salida Finish"] --> B["Crear ProbeResult: SessionId, Revision, origen, destino, responder, Status, count, copia de trace"]
 B --> C["session.RecordProbe: registrar consulta en History"]
 C --> D["Workspace: LastProbe = result; observations[SelectedDeviceId] = result"]
 D --> E["UI muestra resultado inmediato y actualiza nodos"]
 E --> F["ProbeExecuted publica resultado al presenter"]
 F --> G{"Resultado sigue vigente?"}
 G -->|No| Z["No reproducir evidencia obsoleta"]
 G -->|Sí| H["Por cada paso: resolver ports/routes; muestrear spline o tramo abstracto"]
 H --> I["Actualizar overlay y esfera reutilizados; fase y dirección en texto"]
 I --> J{"La red cambió, pausa o nuevo comando?"}
 J -->|Sí| Z
 J -->|No| K{"Quedan pasos?"}
 K -->|Sí| H
 K -->|No| L{"Status Success?"}
 L -->|Sí| M["Ocultar al terminar resultSeconds"]
 L -->|No| N["Resaltar sección del cable de origen en rojo, si existe ruta válida"]
 N --> O["Conservar hasta otra consulta; también limpiar si cambia revisión, pausa o se desactiva presenter"]
```

La sección roja es una anotación de fallo, no un tramo por el que pasó una trama. Cerrar con A conserva ese estado final; cerrar una animación todavía en curso la cancela. Rayos X sólo cambia profundidad de renderizado; no modifica `graph`, `neighbours`, `trace` ni el resultado.

## Scripts implicados

| Script | Función |
| --- | --- |
| NetworkSimulationService.cs | Flujo principal, BFS, ARP, ICMP y caché. |
| NetworkSession.cs | Snapshot, revisión y registro del resultado. |
| ProbeResult.cs | Resultado y traza inmutables expuestos a presentación. |
| DiagnosticWorkspace.cs | Selección de destino y observaciones por equipo. |
| DiagnosticScreenController.cs | Comandos, resultados y eventos de presentación. |
| ProbeTracePresenter.cs | Animación y señal persistente sin modificar dominio. |

Limitaciones deliberadas: sin STP, tabla MAC del switch, tormentas, temporizadores ARP, pérdida aleatoria ni sockets reales. `Sent` cuenta intentos solicitados; `Received` vale count sólo en Success. El modelo no produce éxito parcial.


## Refactorización de legibilidad

El algoritmo anterior conserva el mismo orden y resultados. `Ping` ahora delega en métodos con una responsabilidad:

| Método actual | Función |
| --- | --- |
| ValidatePing | Validar origen, habilitación, destino y subred antes de construir conexiones. |
| BuildOperationalGraph | Crear graph con enlaces habilitados y cables íntegros. |
| TraverseBreadthFirst | Devolver Traversal con Parents, Visited y Flood; queue sigue siendo local al recorrido. |
| FindAddressablePorts | Filtrar reachable por IP/MAC válidas. |
| ReconstructPath | Reconstruir el camino que antes calculaba la función local Path. |
| CanReply | Comprobar subred/máscara de retorno. |
| AppendEchoAttempts | Registrar intentos ICMP y respuestas cuando hay retorno. |

En los diagramas anteriores, `parents`, `visited` y `flood` corresponden ahora a `traversal.Parents`, `traversal.Visited` y `traversal.Flood`; son los mismos datos agrupados en Traversal. No se añadieron hilos ni se cambiaron las reglas ARP.
