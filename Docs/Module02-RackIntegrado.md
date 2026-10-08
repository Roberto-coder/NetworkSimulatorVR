# Rack integrado del módulo 2

La escena `Assets/Scenes/Modulo2.unity` utiliza el montaje de `_Interactuables/RackDevices`. Las alturas y profundidades se tomaron de los volúmenes de `racksFF.blend`, leído con Blender. El objeto de referencia externo se conserva para comparar el montaje.

| Equipo | Altura del centro en el marco del modelo (m) |
| --- | ---: |
| PDU horizontal / PDU-A | 1.86107 |
| NAS 2U | 1.75076 |
| Panel de fibra | 1.34697 |
| Cableado / organizador superior | 1.28897 |
| Patch panel / PP-A | 1.10205 |
| Switch secundario / SW2 | 1.00182 |
| Router | 0.88407 |
| Organizador inferior provisional | 0.81897 |
| Ventilación | 0.69784 |
| Posición de instalación de SW1 | 0.60896 |
| Servidor 2U | 0.40614 |
| UPS 2U | 0.17606 |

El marco del modelo tiene una altura global inicial de 0.024 m. La PDU vertical se encuentra en el lateral posterior y usa una ficha independiente de 0U. El switch manipulable conserva su posición inicial en el entorno; su guía de inserción se trasladó al espacio de SW1.

## Conexiones

Se conservan los sockets existentes y sus referencias. El antiguo puerto del firewall pertenece ahora al prefab `Switch_static` y tiene identificador `SW2/Gi04`. SW2 es fijo y dispone de ficha informativa de switch. Los puertos adicionales del modelo son ilustrativos.

| Cable | Enlace |
| --- | --- |
| C01 | PDU-A/AC01 ↔ SW1/Power |
| C02 | PP-A/01 ↔ SW1/Gi01 |
| C03 | PP-A/02 ↔ SW1/Gi02 |
| C04 | PP-A/03 ↔ SW1/Gi03 |
| C05 | SW2/Gi04 ↔ SW1/Gi04 |

El plan de validación, las etiquetas visibles y el texto del tutorial usan estos identificadores. Se retiraron dos clips de voz asociados a los pasos modificados; sus textos permanecen disponibles. Esos audios requieren una nueva grabación para recuperar la narración.

Al detectar las cinco conexiones correctas, el coordinador bloquea los extremos y fija los cuerpos físicos de esos cinco cables. El bloqueo ocurre antes del etiquetado y del encendido. Las etiquetas y la consola siguen disponibles. Los conectores bloqueados rechazan la desconexión por agarre, salida del socket o estiramiento; al deshabilitar o destruir los objetos, se liberan las conexiones para limpiar la escena.

## Modelos y fichas

Se añadieron NAS, UPS y PDU horizontal con modelos ya importados, colliders, interacción XR y `RackInfoTarget`. El panel de fibra utiliza el modelo existente. Se añadió un organizador inferior provisional con la ficha de cableado estructurado.

`RackStaticVisualFit` funciona únicamente como herramienta manual del Editor. El menú contextual **Ajustar modelo al volumen del dispositivo** aplica el ajuste con Undo y registra los cambios del prefab. Habilitar objetos o entrar en Play conserva las posiciones y escalas guardadas. Los cambios manuales de medidas deben realizarse fuera de Play Mode y guardarse en la escena.

## Verificación

Se comprobaron los identificadores y las referencias locales de escena y prefabs: 11 puertos únicos, sin extremos `FW1/eth01`. La compilación de los scripts de ejecución y de las herramientas de edición modificadas pasó utilizando las referencias de Unity del proyecto.

La apertura del Editor en modo batch terminó antes de cargar la escena. La revisión visual y de interacción en Play Mode/VR queda pendiente. Con `Modulo2` abierta se puede ejecutar **Network Simulator > Module 02 > Validar rack integrado y segundo switch** para comprobar los vínculos cargados por Unity. Después conviene recorrer las fichas, montar SW1 y probar las cinco conexiones.
