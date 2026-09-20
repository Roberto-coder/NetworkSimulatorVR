# Controles centralizados y audios

## Configuración
En PlayerVR_XRI_Hands, selecciona Systems > VRInputManager. Backend = OpenXR.
Las cinco rutas OpenXR se editan aquí, fuera de Play Mode. Los campos Meta OVR
solo se usan cuando Backend = MetaOVR; ese sigue siendo el valor por defecto para
los prefabs antiguos. Debe existir un solo gestor activo por escena.

| Acción | Control Quest | Binding |
|---|---|---|
| Pausa | X izquierdo | <XRController>{LeftHand}/primaryButton |
| Rueda de herramientas | Y izquierdo, mantener y soltar | <XRController>{LeftHand}/secondaryButton |
| Confirmar diálogo | B derecho | <XRController>{RightHand}/secondaryButton |
| Etiquetadora | Gatillo derecho | <XRController>{RightHand}/trigger |
| Agacharse | Clic joystick derecho | <XRController>{RightHand}/primary2DAxisClick |

En Editor y Development Build, Escape pausa y Enter confirma.
PauseManager y RadialMenuController consultan el gestor compartido. Los adaptadores
anteriores de diálogo, rueda, etiquetadora y agachado conservan su lectura independiente
como respaldo cuando no existe gestor. Con el gestor OpenXR, configura los bindings
centrales: las referencias de acciones de esos adaptadores no sustituyen sus bindings.
Movimiento, giro y agarre siguen configurados en las acciones de XRI del rig.
El gestor vive en Systems, fuera del objeto Locomotion que se desactiva al pausar.

## Audios
Legacy Voice ID es la clave del catálogo anterior NPCVoiceCatalog. La prioridad es:
Audio.Clip, Audio.ResourcesPath y finalmente Legacy Voice ID si no se resolvió un clip.
Puede quedar vacío cuando asignas un clip directamente.

Usa nombres estables por significado, por ejemplo:
- m02_rack_presentacion.wav
- m02_rack_instalacion_instruccion.wav
- m02_cableado_conexion_error.wav

No incluyas el número del paso ni renumeres al insertar o quitar líneas.
Si cambia la grabación de la misma línea, reemplaza el archivo conservando su .meta.
Si cambia el significado, crea un nombre nuevo. Es preferible asignar Audio.Clip:
Unity mantiene la referencia al mover o renombrar el asset dentro del editor.
El ID interno del diálogo puede estar referenciado por código; no lo cambies solo
para que coincida con el nombre del audio.
ResourcesPath es relativo a una carpeta Resources, sin extensión, y sí requiere
actualización cuando cambia la ruta. Ejemplo: Audio/NPC/Module02/m02_rack_presentacion.

## Verificación
Compilación de Assembly-CSharp con las referencias de Unity: sin errores.
Pruebas existentes: 621 comprobaciones de módulo 2 y 37 de presentación NPC.
Estas pruebas no simulan los mandos OpenXR. Pendiente validar en Play Mode/visor:
X abre y cierra pausa; Y abre y selecciona una sola vez; B avanza una sola vez;
gatillo imprime una etiqueta y R3 alterna postura una sola vez.
