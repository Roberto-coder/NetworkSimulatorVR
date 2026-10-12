# InfoTargets del módulo 2

Contenido actualizado en `Assets/GameData/Module02/Module02_Devices/Components`.
La escena `Assets/Scenes/Modulo2.unity` incluye `ConsoleInfoZone`, una zona de hover
junto a `Port_Console`, y su tarjeta dentro de la revisión del switch.

## Contenido

- Cobre: organización, curvatura, categoría, fuentes de interferencia y separación contextual.
- Fibra: limpieza, curvatura, protección mecánica e inmunidad de la señal a EMI.
- Puertos: RJ45, ranuras SFP, transceptores de cobre y ópticos, y compatibilidad.
- Consola: RJ45, DB-25/DCE, USB, configuración serial, velocidad, PuTTY y AUX.
- Correcciones: reenvío del switch dentro de VLAN, enlaces ascendentes, UPS frente a PDU,
  panel ciego, función pasiva del patch panel y especificaciones dependientes del fabricante.
- Las cifras de puertos, bahías o prestaciones que no se habían verificado se sustituyeron
  por descripciones generales en las tarjetas afectadas.

Las separaciones de unos 30 cm para luminarias y 1.2 m para grandes motores o
transformadores son ejemplos de especificaciones distintas, no una regla universal
ni una distancia impuesta por el simulador. No se presentan 1.5 m como mínimo normativo.
La fibra no necesita esa separación para evitar EMI en su señal, aunque conserva
los requisitos eléctricos, térmicos y mecánicos de instalación.

## Imágenes disponibles y asignadas

| Archivo | Uso |
| --- | --- |
| Switch/PuertosDeRed.jpg | Puertos RJ45 |
| Switch/SFP_module.jpg | SFP de cobre con RJ45; no es un SFP óptico |
| Switch/puerto-de-consola.jpg | Consola RJ45 y AUX, ambos visibles |
| Switch/LEDswitch.jpg | Indicadores |
| Switch/Interruptor.jpg | Interruptor educativo |
| Switch/Switch24ports.jpg | Switch de 24 puertos; no se asignó al modelo educativo de 12 |
| Rack/ups.png | UPS |
| Rack/fiber_patch_panel_1u.png | Panel de fibra |

Se conservaron las referencias existentes de las demás tarjetas.

## Imágenes pendientes

Prioridad alta:

1. **Separación y EMI:** esquema de cables de datos frente a luminarias, motores/ventiladores
   y transformadores; rutas separadas y cruce a 90°. Rotular cualquier distancia como
   ejemplo del contexto correspondiente.
2. **SFP óptico:** primer plano con conectores LC, idealmente junto a un SFP RJ45 y una
   ranura SFP vacía para explicar dónde se inserta. La imagen actual solo muestra cobre.
3. **PuTTY:** captura de Session con Serial/COM/Speed y de Connection > Serial con
   bits, paridad, parada y control de flujo. Usar 9600 y 8N1 como ejemplo, no como universal.
4. **Conectores de consola:** comparación rotulada de RJ45 serial, DB-25 y USB, con
   cable/adaptador de consola. AUX ya aparece en la foto existente.

Faltan imágenes principales para:

- **Switch educativo de 12 puertos:** captura de su modelo real en el simulador.
- **Entrada de alimentación:** primer plano del conector de alimentación del switch.

Conviene reemplazar la imagen de panel usada por **Cableado de fibra** por una foto
de latiguillos ópticos, y agregar comparaciones SC/LC y monomodo/multimodo cuando estén disponibles.
Las páginas sin imagen siguen funcionando como tarjetas de texto; no tienen referencias ficticias.

## Fuentes consultadas

- [Fluke Networks: Residential Cabling Specification](https://www.flukenetworks.com/knowledge-base/cableiq/residential-cabling-specification)
- [UNCP: Communications Cabling Specification 2017](https://www.uncp.edu/sites/default/files/2020-01/27_15_00_UNC_Pembroke_Communications_Cabling_Specification_2017_FINAL.pdf)
- [Cisco: SFP Modules for Gigabit Ethernet Applications](https://www.cisco.com/c/en/us/products/collateral/interfaces-modules/gigabit-ethernet-gbic-sfp-modules/datasheet-c78-366584.pdf)
- [Cisco: Terminal Connection to a Console Port](https://www.cisco.com/c/en/us/support/docs/switches/catalyst-6000-series-switches/10600-9.html)
- [Cisco: Correct Terminal Emulator Settings](https://www.cisco.com/c/en/us/support/docs/storage-networking/management/217970-troubleshoot-and-apply-correct-terminal.html)
- [PuTTY: documentación oficial](https://www.chiark.greenend.org.uk/~sgtatham/putty/docs.html)

## Verificación

Se verificó la lectura YAML de las 23 tarjetas y de la escena, las referencias de
imágenes, la ausencia de IDs duplicados y el enlace de la nueva zona de consola a
su tarjeta, al puerto y a la lista de revisión del switch. Las páginas nuevas tienen
textos breves (hasta 440 caracteres).

Pendiente en Unity: revisar el tamaño/posición de ConsoleInfoZone, apuntar al puerto,
recorrer las páginas y confirmar que la tarjeta se puede marcar como revisada.
Comprobar también legibilidad, imágenes y desbordamiento en el visor VR.
No se ejecutó Play Mode durante esta edición.

## Puesta a tierra y organizador de cables

Se añadieron las tarjetas `rack_grounding.asset` y `cable_organizer.asset`.
El organizador inferior ahora usa su tarjeta propia, en lugar de compartir la de
cableado estructurado. Se conserva su modelo y sus colliders de escena; su lista
de colliders XRI permite descubrir los colliders hijos.

`RackGrounding_Provisional.prefab` contiene un cubo color cobre de 8 cm, un collider
de 9 cm, XRSimpleInteractable, RackInfoTarget y el filtro de selección de hover.
Está bajo RackDevices, en posición local (-0.18, 0.065, -0.015), en la parte inferior.
Se añadió a la lista de elementos requeridos durante la revisión del rack.
El coordinador controla su activación como en los otros elementos.

La tarjeta explica la unión equipotencial, la inspección del terminal y el alcance
del visual provisional. No representa un electrodo real ni valida mediciones.
Fuente: [Panduit: Grounding Busbars](https://www.panduit.com/en/products/grounding-bonding/grounding-busbars-strips/grounding-busbars.html).

Imágenes pendientes: una barra/terminal de puesta a tierra con sus conexiones al
rack, y un organizador horizontal con latiguillos correctamente guiados. Ambas
tarjetas funcionan sin imágenes mientras se agregan esos recursos.

Se verificaron el YAML, los IDs, las referencias entre assets, la jerarquía del
cubo, el collider enlazado a XRI y la inclusión de ambas tarjetas en la revisión.
Hay 25 tarjetas de componentes. Falta comprobar la posición y accesibilidad del
cubo, las interacciones y la confirmación de revisión en Play Mode/VR.
