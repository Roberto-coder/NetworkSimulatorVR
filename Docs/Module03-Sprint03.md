# Módulo 3 — Sprint 3: simulación de diagnóstico

## Entrega y verificación

Implementados `NetworkSimulationService`, resultados inmutables, traza ARP/ICMP, tabla de vecinos por origen y revisión de evidencia. `Module03NetworkScene.Diagnostics` expone el servicio asociado a su sesión después de una inicialización válida.

- **94 comprobaciones .NET correctas**, incluyendo las 54 del sprint 1.
- Compilación del código de dominio, adaptadores de escena y Editor con las referencias de Unity 6000.2.7f2/Splines 2.8.4: correcta.
- Pendiente: ejecutar la ventana de diagnóstico dentro de Unity. Las pruebas no sustituyen la importación del Editor.
- No se modificaron escenas, assets iniciales ni el demo de Splines. La laptop y la visualización conectada al diagnóstico corresponden a los siguientes sprints.

## Prueba en Unity

Abrir **Network Simulator > Module 03 > Sprint 3 - Diagnóstico**. No requiere Play. Seleccionar `OfficeInitialState` y pulsar **Crear / reiniciar sesión desde asset**. Esta ventana mantiene una copia independiente; sus botones no modifican los GameObjects ni los assets.

Mantener el origen `Laptop/eth0`:

| Paso | Acción / destino | Resultado esperado |
| --- | --- | --- |
| 1 | Ping a `192.168.10.2` | Success, 4/4: interfaz de gestión del switch |
| 2 | Ping a `192.168.10.11` | AddressUnresolved: el cable roto impide resolver ARP |
| 3 | Sustituir Patch-01 por repuesto; repetir .11 | Success, 4/4 |
| 4 | Inhabilitar P02; ping a `192.168.10.12` | AddressUnresolved |
| 5 | Habilitar P02; repetir .12 | Success, 4/4 |
| 6 | Duplicar IP de PC-03 (.12); ping a .12 | AddressConflict, sin ICMP exitoso |
| 7 | Restaurar IP de PC-03 (.13); ping a .12 y .13 | Success en ambos |
| 8 | Ping a `10.0.0.1` | NoRoute: no existe gateway en este alcance |
| 9 | Cambiar un puerto o una IP tras una prueba | Resultado anterior rotulado como obsoleto |
| 10 | Crear/reiniciar sesión | Restaurar cable roto inicial y tabla ARP vacía |

La tabla muestra vecinos aprendidos por el origen seleccionado. Repetir un ping sin modificar la red reutiliza la entrada ARP y muestra solo ICMP. Un cambio de revisión limpia la tabla de forma conservadora.

Los botones de incidentes son controles de prueba con IDs de la oficina de ejemplo; no son el flujo educativo ni el tutorial. Para una topología distinta se usa el servicio con sus IDs. `NetworkSimulationService` no contiene IDs fijos de dispositivos.

## Reglas de simulación

1. Validar interfaz origen, IPv4 unicast, máscara y destino. El alcance usa /1–/30 y no admite origen/destino de red, broadcast, multicast ni loopback.
2. Verificar que el destino está en la subred del origen. No hay enrutamiento ni gateway implícito.
3. Construir el grafo operacional: cables íntegros y conectados, continuidad pasiva declarada, puertos habilitados y mismo segmento.
4. Los switches conectan internamente sus puertos del mismo segmento, incluida su interfaz de gestión. Las PCs no reenvían entre interfaces. Las rosetas y patch panels solo conducen por sus conexiones internas explícitas.
5. Recorrer el grafo mediante BFS estable. La solicitud ARP usa un árbol de difusión acotado; las respuestas regresan al origen.
6. Sin respuesta ARP, devolver AddressUnresolved sin inventar recorrido ICMP. Un ping no identifica por sí solo un cable roto frente a un puerto inhabilitado.
7. Dos candidatos alcanzables con la misma IP producen AddressConflict. Un equipo aislado no revela mágicamente su IP. También se detecta conflicto alcanzable de la IP origen.
8. Con vecino único, emitir la traza EchoRequest y EchoReply de cada intento. Si la máscara del destino impide el retorno, registrar EchoRequest sin EchoReply y devolver ReplyUnavailable.

Un ping a la IP del propio origen es una respuesta local sin recorrido físico y no demuestra que el cable funcione. Un resultado expone el puerto que realmente respondió, para que los objetivos futuros puedan comprobar identidad y no solo igualdad de IP.

## Contrato para la laptop y la animación

```csharp
var result = scene.Diagnostics.Ping("Laptop/eth0", "192.168.10.11");
bool usableEvidence = scene.Diagnostics.IsCurrent(result)
    && result.Status == ProbeStatus.Success
    && result.ResponderPort == "PC-01/eth0";
var arp = scene.Diagnostics.GetNeighbours("Laptop/eth0");
```

`ProbeResult` contiene ID de sesión interno, revisión, origen, IP destino, puerto respondiente, estado, mensaje, intentos/respuestas y una colección de tramos de solo lectura. Cada tramo identifica fase, tipo de enlace, ID, puertos origen/destino y número de intento ICMP (1–4 por defecto; 0 para ARP).

`Sent` representa los intentos solicitados por el comando, no una cuenta de tramas Ethernet realmente emitidas. Puede haber 0 respuestas y solo ARP en la traza. La cantidad aceptada es 1–10. No se inventan valores de latencia; el tiempo de reproducción se definirá en datos de presentación.

Los tramos de tipo Cable se relacionan con `FixedNetworkCable.CableId` o, más adelante, el patch cord interactuable. PassiveContinuity y SwitchForwarding conectan anclajes de puertos dentro del dispositivo. La difusión puede ramificarse: no unir todos los pasos ARP como si fueran una única polilínea.

Antes de reproducir o validar una traza, comprobar `IsCurrent`. Se comprueban sesión y revisión: dos sesiones con el mismo contador no comparten evidencia. Cambiar la red no modifica el resultado anterior; lo vuelve obsoleto. El reinicio también invalida evidencia y vecinos.

Ping queda registrado en la bitácora sin cambiar la revisión de la red. Por ahora las pruebas y la UI conservan sus resultados; el servicio aún no concede objetivos ni logros automáticamente.

## Límites didácticos

Simulación determinista de una LAN, no pila Ethernet/IP completa. La misma configuración produce el mismo resultado y recorrido aunque se reordenen las listas del asset. No se implementan pérdidas aleatorias, latencias reales, TTL, STP, aprendizaje de tabla MAC, tormentas por bucles, DHCP, DNS, enrutamiento ni caducidad temporal de ARP. Los ciclos se recorren una vez para impedir bucles infinitos.

La detección de IP duplicada es deliberadamente determinista y explícita; sistemas operativos reales pueden mostrar respuestas intermitentes o de otro equipo. La tabla ARP se invalida al cambiar cualquier revisión, incluso al etiquetar un cable: es una simplificación conservadora.

## Pruebas automatizadas

Ejecutar `dotnet run --project Tests/Module03Diagnostics/Module03Diagnostics.csproj`.

La suite cubre las tres fallas y su recuperación; continuidad pasiva y gestión del switch; identidad y sentido de las trazas; resolución/caché ARP; origen inválido o deshabilitado; red/broadcast/multicast/loopback; IP inexistente; máscara asimétrica; duplicidad aislada y alcanzable; separación de segmentos; hosts sin reenvío; reinicio; bitácora; escalabilidad a 12 PCs; ciclos; independencia del orden de las listas y número de intento.


## Scripts añadidos y responsabilidades

Incluye ampliaciones posteriores del sprint; los archivos reutilizados o modificados se indican expresamente.

| Script | Objetivo o función |
| --- | --- |
| `NetworkSimulationService.cs` | Construye grafo operativo, ejecuta BFS, ARP e ICMP simulados. |
| `ProbeResult.cs` | Define resultado, fases, traza y observaciones ARP. |
| `Module03DiagnosticsWindow.cs` | Permite probar la simulación desde el Editor. |


## Refactorización de legibilidad

| Script modificado | Objetivo o función |
| --- | --- |
| `NetworkSimulationService.cs` | Refactor sin cambio de reglas: ValidatePing, BuildOperationalGraph, TraverseBreadthFirst, ReconstructPath y AppendEchoAttempts. |

Se conserva el comportamiento probado por las 129 comprobaciones de dominio. Las pruebas de interacción deben repetirse en Unity.
