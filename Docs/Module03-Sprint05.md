# Sprint 5 — Traza visual de ping y rayos X

## Alcance

El botón Ping conserva el cálculo síncrono de NetworkSimulationService y publica el resultado mediante ProbeExecuted. ProbeTracePresenter reproduce exclusivamente ProbeResult.Trace, sin escribir en NetworkSession ni usar colisiones para decidir el resultado. La prueba y bitácora existen aunque se cancele o no pueda mostrarse la animación.

- ARP ámbar; EchoRequest azul; EchoReply verde, con texto de fase, intento y dirección (no depender sólo del color).
- Un único paquete y LineRenderer reutilizados; ambos se preparan en Editor. No se crean objetos por ping.
- Splines de cables fijos en dirección correcta; conexiones internas y patch cords aún sin ruta física se representan mediante líneas abstractas entre anclajes.
- ARP se reproduce por ramas secuenciales del árbol calculado por BFS. Es una visualización didáctica, no una reproducción simultánea de todos los broadcasts.
- Velocidad en metros/segundo de presentación: no representa RTT, latencia ni capacidad del cable.
- Sin respuesta válida: tramo rojo persistente del cable de salida del origen y texto del resultado. No se colorea un cable remoto como culpable sin evidencia. No hay parpadeo.
- Cancelación al repetir Ping, cerrar pantalla, alejarse (cierra pantalla), abrir la rueda (cierra pantalla), pausar, desactivar presenter o cambiar la sesión/revisión. El rojo final permanece al cerrar con A, alejarse o abrir la rueda; se limpia al ejecutar otro comando. Un nuevo ping debe comprobar el estado actual.
- Las rutas desalineadas se omiten y el texto informa cuántos tramos no se visualizaron. No se corrigen ni reposicionan automáticamente.

## Aplicación en Unity

1. Fuera de Play, abrir la escena de módulo 3 con el sprint 4 configurado.
2. Ejecutar **Network Simulator > Module 03 > Sprint 5 - Animación de ping**.
3. Revisar `Probe trace visualization` y guardar la escena. La herramienta añade controles al prefab compartido, si existe, y a pantallas anteriores que no los tengan; no reconstruye la topología ni cambia transforms de los Canvas.
4. El configurador crea `Assets/GameData/Module03/Presentation/ProbeVisualSettings.asset` y `ProbeOverlay.mat`. Ajustar velocidad, colores, dimensiones, duración del resultado y `Animate Motion`. Desactivarlo deja el resaltado de segmentos sin movimiento de esfera.
5. En Play, abrir una pantalla con A, seleccionar un destino con IP y pulsar Ping. El resultado textual aparece de inmediato; la traza se reproduce después.
6. Alternar **Rayos X: ON/OFF** mientras se anima. Sólo cambia el material de la visualización, no el de cables/paredes. Se usa el shader URP existente con ZTest Always/LessEqual y ZWrite Off; cola 2990. Probar que no tapa la UI y que funciona en ambos ojos.

El botón de configuración puede repetirse sin duplicar el presenter ni los controles. Respeta los ajustes existentes del ScriptableObject. Undo revierte los cambios de escena; los assets/prefab guardados son cambios persistentes y no forman parte del Undo de escena.

## Pruebas de aceptación pendientes en Unity/visor

- Ping a SW-01: solicitud/respuesta visibles; segundo ping puede omitir ARP por caché.
- PC-01 con cable roto: ninguna trama atraviesa ese cable; se mantiene una sección roja de salida en origen hasta otro comando. Las ramas ARP hacia otros destinos sí pueden verse.
- Después de reparar desde el dominio: ida/vuelta por las rutas de los puertos correctos.
- Un cambio de revisión durante reproducción cancela el overlay y pide repetir ping.
- Un nuevo ping reemplaza la reproducción; no aumenta el número de esferas, líneas o materiales.
- Pausa, cierre con A y desactivación limpian el overlay. Reactivar el presenter restaura sus suscripciones.
- Rayos X revela el segmento a través de paredes; OFF respeta oclusión. Comprobar ambos ojos y UI en hardware.
- Con ruta fija desalineada: advertencia de tramos omitidos sin bloquear el resultado lógico.
- Verificar controles en prefab compartido y en Canvas antiguos; ningún Canvas se construye durante Play.

## Verificación realizada

Compilación C# runtime/Editor con referencias de Unity correcta; 109 comprobaciones de dominio aprobadas. El dominio no cambió. Sin ejecución del Editor/visor desde esta sesión: configuración de escena, compilación del shader en hardware y aceptación visual pendientes.

La esfera arrojable, interacción de reemplazo del cable, tester y etiquetadora se mantienen para los siguientes sprints. Este sprint no introduce diálogos ni guion.


## Scripts añadidos y responsabilidades

Incluye ampliaciones posteriores del sprint; los archivos reutilizados o modificados se indican expresamente.

| Script | Objetivo o función |
| --- | --- |
| `ProbeVisualSettings.cs` | Configura velocidad, colores, tamaños y tramo rojo persistente. |
| `ProbeTracePresenter.cs` | Reproduce la traza, maneja rayos X y cancela evidencia obsoleta. |
| `Module03ProbeVisualSetup.cs` | Prepara renderers, material, controles y contornos en Editor. |
| `DiagnosticScreenController.cs (modificado)` | Publica ProbeExecuted y CommandExecuting; activa contorno del destino. |
| `DiagnosticScreenView.cs (modificado)` | Añade referencias de rayos X, estado de traza y contorno. |
| `DiagnosticCanvasBuilder.cs (modificado)` | Añade controles y contornos a vistas existentes. |
| `SplineDemoUnlit.shader (reutilizado)` | Permite oclusión normal/rayos X con ZTest y ZWrite. |


## Ajuste: contorno de selección y fallo persistente

Volver a ejecutar el configurador del sprint 5 fuera de Play para añadir `UnityEngine.UI.Outline` al fondo de cada nodo, tanto en el prefab como en Canvas previos. El contorno incluye sprite, nombre y estado. No usa el Outline 3D del proyecto. Cambiar de destino desactiva el contorno anterior; ya no se añade el símbolo junto al texto.

`failureSectionLength` (0.8 m) y `failureLineWidth` (0.03 m) se ajustan en ProbeVisualSettings. El rojo es una anotación de prueba fallida en la salida del origen, no evidencia de que ese cable esté roto. Si falta cable/anclaje/ruta válida se informa por texto y no se inventa una sección. No se anima tráfico al dibujar esta anotación.

Ping, ARP, Configuración local, Puertos, Bitácora y Cerrar limpian la señal mediante CommandExecuting. Elegir otro destino y alternar Rayos X no ejecutan una consulta y la conservan. Cerrar con A conserva el rojo final para inspeccionarlo. Pausa, cambio de revisión o desactivación del componente retiran evidencia obsoleta. Las animaciones aún en curso sí se cancelan al cerrar pantalla.

Pruebas: seleccionar dos nodos sucesivos (un único contorno); ejecutar ping fallido y esperar más de resultSeconds (rojo persiste); cerrar con A (persiste); ejecutar ARP (se limpia); cambiar revisión (se limpia); ping exitoso posterior recupera ancho/color normales. Verificación en visor pendiente.


## Jerarquía organizada por responsabilidad

Se admite `Module03_Network` bajo `_Interactuables`, `Diagnostic workspace` bajo `_UI` y `Probe trace visualization` bajo `_Managers`. El configurador busca componentes en la escena activa, incluyendo inactivos, y los relaciona mediante `networkScene`, `network` y `screenController`. Los nombres no se usan para resolver esas asociaciones.

Se reutiliza el presenter existente, se completan sus renderers si faltan y se actualizan las pantallas referenciadas por DiagnosticInteractionTarget. No se requiere que UI o managers sean hijos de la red. Los dispositivos/puertos/cables siguen bajo Module03_Network porque delimitan su inventario. Si se crea un presenter nuevo, se coloca bajo la raíz `_Managers` cuando existe. Los objetos existentes mantienen su padre y transform.

| Script modificado | Objetivo o función del ajuste |
| --- | --- |
| Module03ProbeVisualSetup.cs | Buscar controlador/presenter en la escena por referencia; actualizar vistas de targets y evitar duplicados. |
| DiagnosticScreenPrefabSetup.cs | Migrar pantallas aunque el controlador esté fuera de la jerarquía de red. |
| Module03DiagnosticUiSetup.cs | Detectar workspace existente en otra raíz y evitar crear otro. |

Aplicación: salir de Play, ejecutar nuevamente el menú del sprint 5 y guardar la escena. Repetir debe conservar un solo presenter y los mismos padres. Comprobar contorno de selección, sección roja persistente y botón de rayos X. Compilación C# y 109 comprobaciones de dominio correctas; ejecución del configurador en Unity pendiente.
