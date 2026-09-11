# Módulo 2 — Sprint 2: montaje y fijación

## De qué Transform depende la inserción

Antes, RackInsertionSlot comparaba la rotación mundial de la raíz con XRGrabInteractable con EntryPose.rotation. No comparaba el eje Z con el recorrido. Eso permitía un desfase de 180 grados: la posición se guiaba hacia InstalledPose, pero la rotación se exigía en sentido contrario.

Ahora:

- EntryPose.position → InstalledPose.position define la dirección de entrada y el recorrido del pivote del dispositivo.
- EntryPose.up define qué lado debe quedar arriba.
- RackInsertionGrabTransformer > Local Insertion Euler define el marco de entrada relativo al objeto que tiene XRGrabInteractable.
- (0, 0, 0): el +Z local del objeto entra en el rack y +Y queda arriba. Es el ajuste para el switch 2U descrito.
- (0, 180, 0): el modelo entra por su -Z local.

La captura, el seguimiento, el estado colocado y el reinicio usan la misma orientación. InstalledPose.rotation no determina la rotación del switch. Los pivotes EntryPose e InstalledPose siguen correspondiendo al pivote de la raíz del objeto, no al conector ni al frente de la malla. Si inspeccionas el eje de un hijo visual, puede ser distinto al de la raíz con XRGrabInteractable.

## Retención y giro residual

Al llegar al final se cancela el agarre desactivando XRGrabInteractable y DESPUÉS se fija el Rigidbody. Se refuerza el estado cinemático, sin gravedad y en la pose del riel en FixedUpdate y LateUpdate para evitar que la restauración de física de XRI al soltar deshaga el encaje. También se sostiene el switch si se suelta a mitad del riel; se puede volver a agarrar para terminar de introducirlo.

Fuera del riel se cancela la velocidad angular residual un frame después de soltar. El configurador además desactiva Throw On Detach y establece angularDamping al menos en 2. La gravedad sigue funcionando. Si el modelo aún gira por penetración de colliders, se debe revisar su geometría de colisión; este ajuste no sustituye esa revisión.

El switch colocado queda retenido aunque todavía falte atornillar. No se permite sacarlo manualmente en esta versión. Reset installation es la vía de reinicio y reinicia ambas pestañas.

## Configuración en Unity

1. Abre Modulo2 fuera de Play Mode y espera a que compile.
2. Ejecuta **Network Simulator > Module 02 > Infraestructura > Sprint 2 - Montaje y destornillador**.
3. El configurador usa la única RackInsertionSlot de la escena y sus dispositivos aceptados. Conserva los ajustes existentes de EntryPose, InstalledPose, colliders y Local Insertion Euler. No vuelve a crear las pestañas que ya existen.
4. En cada switch, ajusta Fastening_Sprint02/LeftTab y RightTab al centro de cada pestaña. Las posiciones iniciales suponen un frente de 19 pulgadas en -Z y deben alinearse con el modelo real. El radio de cada zona es de 2.2 cm.
5. Guarda la escena. El prefab del destornillador y su asignación al primer espacio de ModuleDefinition se guardan al ejecutar el configurador. Hay Undo para los cambios de escena; el prefab nuevo es un archivo generado aparte.
6. Prueba la rueda y ajusta la orientación/posición visual del prefab si es necesario. ToolManager lo instancia en su anclaje de mano existente. La herramienta usa geometría provisional y una punta trigger de 7 mm a 17 cm del anclaje.

No hace falta ejecutar de nuevo el configurador antiguo de exploración/inserción para recibir las correcciones del código de RackInsertionSlot.

## Atornillado

RackFasteningAssembly.CanFasten solo se activa si ese mismo switch está colocado en la ranura. RackScrewZone cuenta dos segundos continuos con la punta de una herramienta Screwdriver dentro. El mango, la mano o cualquier otra herramienta no cuentan. Retirar o guardar la herramienta antes de terminar reinicia el progreso de esa pestaña; una pestaña completada permanece fijada. El texto muestra porcentaje y luego Fijado; el tornillo cambia a verde.

FastenedCount e IsSecured exponen el resultado. Secured se emite una sola vez al completar las dos pestañas. No se conecta todavía al flujo de objetivos, NPC, quiz o insignia. AudioSource es opcional: se puede asignar un clip, pero el configurador no incorpora audio.

## Verificación

Compilación de runtime/editor con el compilador y referencias locales de Unity. Falta validación en Play/VR:

- Introducir el switch 2U por +Z sin tener que girarlo 180 grados.
- Rechazar la orientación opuesta; comprobar que el ajuste Y=180 permite otro modelo con ejes inversos.
- Soltar a mitad del riel, volver a agarrar y completar el recorrido.
- Llegar al verde manteniendo pulsado el agarre, mover la mano y soltar: el switch debe permanecer fijo.
- Repetir con dos manos cuando el interactuable permita selección múltiple.
- Soltar fuera del riel: no debe conservar giro de lanzamiento.
- Acercar el destornillador antes del montaje: no debe avanzar.
- Completar cada pestaña por separado, interrumpir a mitad, guardar la herramienta y volver a equiparla.
- Tras dos pestañas verdes, comprobar IsSecured y que no se vuelve a emitir Secured.
- Ejecutar Reset installation en Play: desaparece la fijación, vuelve la guía y el switch puede montarse otra vez.
