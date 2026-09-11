# Módulo 2 — Sprint 3: cinco conexiones

## Aplicar en Unity

Abre Modulo2 fuera de Play y ejecuta **Network Simulator > Module 02 > Infraestructura > Sprint 3 - Validar cinco conexiones**. Se necesitan los 11 puertos y los cinco cables activos del sprint 1. El configurador comprueba sus IDs y los componentes necesarios antes de editar.

Conserva las posiciones de sockets, cables y dispositivos. Configura las familias y el anclaje de sockets, apaga el tráfico de demostración y añade `Cabling_Sprint03` con una tabla de estado. Puede repetirse sin duplicar el monitor ni reposicionar la tabla existente. Ajusta su posición y orientación para leerla cómodamente y guarda la escena; no se guarda automáticamente.

## Tabla fija

| Origen | Destino | Tipo |
|---|---|---|
| PDU-A/AC01 | SW1/Power | Alimentación |
| PP-A/01 | SW1/Gi01 | Ethernet |
| PP-A/02 | SW1/Gi02 | Ethernet |
| PP-A/03 | SW1/Gi03 | Ethernet |
| FW1/eth01 | SW1/Gi04 | Ethernet |

`Module02ConnectionPlan` contiene la tabla compartida por el monitor y el configurador de ganchos. Cualquiera de los cuatro cables Ethernet puede cubrir cualquiera de los cuatro enlaces Ethernet. La asignación fija corresponde a los puertos, no a un cable físico particular. El orden de sus extremos no importa. `SW1/Console` está fuera del contador y reservado para la herramienta integrada.

## Comportamiento

- Se conecta acercando el extremo al socket y soltándolo; volver a agarrarlo desconecta.
- Alimentación, Ethernet y consola son familias distintas. Se comprueban tanto en el detector cercano como en Connector.CanConnect/Connect. Los conectores antiguos sin NetworkPort/PatchCableLink conservan sus reglas previas.
- Un socket ocupado no se reemplaza por otro cable.
- Un enlace entre puertos Ethernet equivocados puede encajar, pero no cuenta y aparece como `Revisar: origen ↔ destino` en la tabla.
- Los cinco enlaces correctos producen 5/5. Desconectar un extremo o desactivar/eliminar un cable retira su contribución, con actualización cada 0.1 segundos.
- El monitor expone ConnectedCount, IncorrectCount, IsComplete, IsConnectionPresent y StateChanged. IsComplete es un estado reversible, no un evento de finalización permanente.
- El objetivo antiguo Module02CablingObjective se desactiva desde el configurador para no adelantar el flujo. No hay dependencia de montaje, tornillos, NPC u objetivos: la integración final aplicará esa secuencia.

Se restauran los extremos libres a física dinámica antes de encajarlos para que el socket recuerde correctamente ese estado al desconectar. Los conectores preconectados al iniciar evitan crear dos joints para una misma unión.

Las etiquetas de ambos extremos pertenecen al sprint 4. La lógica definitiva de encendido, LED y habilitación de tráfico corresponde a los sprints 5 y 6. Se conserva el visual provisional del cable eléctrico.

## Verificación realizada

- Runtime y editor compilados con Roslyn y las referencias locales de Unity; solo advertencias preexistentes de TextMeshPro.
- 386 comprobaciones de las reglas: pares fijos, inversión de extremos, IDs sin distinción de mayúsculas, extremos faltantes/reconexión, exclusión de Console, destinos cruzados y todas las combinaciones de familias de cable y puertos.
- Repetir las reglas con `dotnet run --project Tests/Module02Wiring/Module02Wiring.csproj --property:RestoreConfigFile=D:/TTRedesVR/NetworkSimulatorVR/Tests/Module02Wiring/NuGet.Config`. Requiere SDK .NET 10 y no usa paquetes externos.

## Prueba física pendiente en Play/VR

1. Completar los cinco enlaces en orden libre, también invirtiendo los extremos de un cable.
2. Comprobar 5/5 y luego retirar un extremo: debe pasar a 4/5 y recuperar 5/5 al reconectar.
3. Conectar PP-A/01 a SW1/Gi02: debe mostrar el enlace a revisar y no completar el requisito PP-A/01 → SW1/Gi01.
4. Intentar conectar el cable eléctrico a Gi01 y Ethernet a Power/Console: no deben encajar.
5. Intentar ocupar un socket ya conectado: la conexión original debe permanecer.
6. Agarrar de nuevo antes de finalizar la conexión pendiente: no debe encajar mientras está en la mano.
7. Confirmar que los extremos desconectados vuelven a tener gravedad y se pueden reutilizar, sin saltos por velocidades residuales.
8. Desactivar un cable completo y verificar que el contador baja. Reiniciar Play restaura el inventario inicial.

El configurador no se ha ejecutado ni se ha validado la interacción física en esta sesión.
