# Consola y LEDs de puertos del módulo 3

## Asignar el LED de una PC o del switch

1. Selecciona el objeto del puerto que contiene `NetworkPortAnchor` y comprueba su `Port Id` (por ejemplo `PC-01/eth0` o `SW-01/P01`).
2. Arrastra el `MeshRenderer` del LED a `Link Led` y elige `Connected Color` / `Disconnected Color`.
3. En Play, comprueba que conectar ambos extremos enciende el LED y retirar cualquiera lo apaga.

El renderer debe corresponder al LED, no al modelo completo. El material debe admitir `_BaseColor` (URP) o `_Color`. Se utiliza `MaterialPropertyBlock`, sin duplicar materiales. No es necesario añadir otro controlador.

`Module03NetworkScene` actualiza los LEDs después de sincronizar los cables. La continuidad atraviesa patch panels y rosetas, pero termina en el siguiente dispositivo activo: no depende de que otro puerto del switch tenga enlace. Un puerto deshabilitado, cable roto o extremo suelto apaga el indicador. Una IP incorrecta puede impedir ping sin apagar el enlace físico. Los campos `Link Led` ya asignados en sockets `NetworkPort` también reciben el estado operacional; basta asignar el renderer en uno de los dos componentes.

## Resultados y ARP

- `ipconfig /all`: configuración del dispositivo abierto.
- `ping <IP>`: respuestas y resumen de enviados/recibidos/perdidos; no inventa latencias ni TTL.
- `arp -a`: tabla IP–MAC aprendida por esa interfaz mediante resolución ARP, normalmente durante ping. Puede haber entrada ARP aunque falle la respuesta ICMP. No precarga el inventario.
- Al cambiar la revisión de red se invalida la caché ARP del simulador; vuelve a ejecutar ping para aprender entradas.
- La bitácora muestra las últimas 30 acciones de toda la sesión, con hora UTC, resultado y comando o acción legible.
- La administración del switch utiliza gestión LAN. No se indica COM porque este módulo no implementa una conexión serie.

## Comprobar reparación

El botón antes llamado **Verificar incidentes** comprueba las reglas de IP/puertos y que existan pings vigentes hacia los equipos requeridos. No modifica la red ni sustituye la comprobación de continuidad/etiquetado del cable.

## Diagnostic workspace

`_UI/Diagnostic workspace` contiene `DiagnosticScreenController`: atiende la interacción A, abre/cierra las pantallas, conecta botones, mantiene el origen local y solicita las trazas/rayos X. Es necesario incluso cuando las pantallas se encuentran bajo `M03 Diagnostic Screens`. No eliminarlo mientras tenga ese componente y sus referencias.
