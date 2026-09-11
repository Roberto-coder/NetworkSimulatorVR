# Módulo 2 — Sprint 1: infraestructura

## Alcance y estado

Código de configuración y almacenamiento implementado. Compilación verificada con Roslyn y las referencias del proyecto Unity. Falta ejecutar el configurador en Unity, alinear los nuevos sockets con los modelos y comprobar en Play/VR; esto es necesario antes de cerrar el sprint. La escena y los prefabs previamente modificados no se regeneran desde disco.

## Aplicación en Unity

1. Abre `Assets/Scenes/Modulo2.unity`, fuera de Play Mode.
2. Abre **Network Simulator > Module 02 > Infraestructura > Sprint 1 - Puertos y ganchos**. Es distinto de los menús antiguos de exploración.
3. Revisa las cuatro raíces: SW1 (el switch que se usará), PP-A (patch panel), FW1 (firewall), PDU-A (PDU). El socket SW1/Gi01 existente debe ser descendiente de la raíz SW1 elegida.
4. Ajusta la posición mundial de los ganchos junto al rack, a una altura accesible. Cada gancho se separa 42 cm del siguiente; el conjunto necesita aproximadamente 2 m de espacio libre.
5. Pulsa **Crear infraestructura**. La operación tiene Undo, revierte si falla y no permite duplicar un sprint ya creado. No guarda automáticamente la escena.
6. Alinea cada nuevo `Port_*` con su agujero real. La colocación inicial usa el frente local +Z del dispositivo, no reconoce los agujeros del modelo. Conserva el socket Gi01 existente como referencia. Ajusta también `VisibleID`, incluida su rotación cuando sea necesario.
7. Pulsa **Validar inventario**, guarda la escena y prueba en Play/VR.

Los sockets fuera del inventario y los cables de prueba anteriores se desactivan, conservándose en la jerarquía. No se desactivan las interacciones de inspección del dispositivo. Los puertos puramente visuales del modelo no reciben componentes de conexión.

## Tabla fija

| Cable | Origen | Destino | Tipo |
|---|---|---|---|
| C01 | PDU-A/AC01 | SW1/Power | Alimentación |
| C02 | PP-A/01 | SW1/Gi01 | Ethernet |
| C03 | PP-A/02 | SW1/Gi02 | Ethernet |
| C04 | PP-A/03 | SW1/Gi03 | Ethernet |
| C05 | FW1/eth01 | SW1/Gi04 | Ethernet |

`SW1/Console` queda reservado para el cable integrado del dispositivo de consola. No se crea un sexto cable suelto. La tabla identifica la práctica; la validación de las cinco conexiones se integra en el sprint 3.

Los cuatro dispositivos tienen `DeviceIdentity`. Cada puerto tiene `NetworkPort` y un Canvas de ID sin raycaster ni bloqueo de rayos. Se añadió `Power` al final de `NetworkPortKind` para conservar los valores serializados anteriores. Los conectores eléctricos usan bloques provisionales en lugar del visual RJ45; su forma definitiva queda pendiente.

## Ganchos

Cada cable conserva el prefab físico existente. `CableHookStorage` inmoviliza sus cuerpos, coloca los puntos en una U invertida con la longitud de cada tramo conservada, y libera la física al agarrar cualquiera de sus extremos. Los soportes son visuales: la estabilidad inicial no depende de colisiones pequeñas con los ganchos. No se implementa recolgado automático. Reiniciar Play restaura el almacenamiento inicial.

La transmisión automática de demostración queda apagada en estos cinco cables hasta implementar encendido/configuración. Las etiquetas de ambos extremos se harán en el sprint 4; los rótulos de los ganchos no cuentan como etiquetado realizado por el usuario.

## Prueba de aceptación pendiente

- Exactamente 11 puertos activos, 4 IDs de dispositivo, 5 cables y 5 ganchos.
- IDs legibles desde la distancia de manipulación, sin tapar agujeros ni tarjetas de inspección.
- Los cinco cables permanecen almacenados sin caerse ni temblar antes del agarre.
- Tomar cualquiera de los extremos libera todo el cable; soltarlo recupera gravedad y permite volver a agarrarlo.
- No hay explosiones físicas, enganches con el entorno ni solapamiento de cables en reposo.
- Se conserva el montaje/agarre del switch y su socket Gi01 previo.
- Reiniciar Play restaura los cinco cables; el configurador impide duplicados.

Acuerdos posteriores: una etiqueta en cada extremo, cable de consola integrado en todas las versiones y puzzle inicial de cuatro bloques predefinidos. Objetivos, NPC, quiz e insignia se integrarán después de las mecánicas.
