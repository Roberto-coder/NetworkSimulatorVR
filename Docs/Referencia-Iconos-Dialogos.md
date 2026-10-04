# Referencia de iconos para los dialogos

Escribe los marcadores tal cual, respetando mayusculas y sin espacios internos.
Funcionan en los dialogos de tutoriales que usa DialogueTextAnimator.

## Por accion (recomendado)
El sprite se resuelve con VRInputManager al comenzar cada dialogo.

| Escribir | Accion | Sprite predeterminado |
|---|---|---|
| `{control:confirmar}` | Avanzar dialogo | B.png |
| `{control:pausa}` | Menu de pausa | X.png |
| `{control:herramientas}` | Rueda de herramientas | Y.png |
| `{control:usar}` | Gatillo de herramienta | R2.png |
| `{control:interactuar}` | Interactuar/abrir dispositivo | A.png |

## Boton fijo
Estos marcadores no cambian al reasignar controles. Sirven para mencionar
un boton concreto o representar cualquiera de los sprites disponibles.

| Escribir | Sprite |
|---|---|
| `{boton:A}` | A.png |
| `{boton:B}` | B.png |
| `{boton:X}` | X.png |
| `{boton:Y}` | Y.png |
| `{boton:L1}` | L1.png |
| `{boton:L2}` | L2.png |
| `{boton:R1}` | R1.png |
| `{boton:R2}` | R2.png |
| `{boton:SL2}` | SL2.png |
| `{boton:SR2}` | SR2.png |

Imagenes: Assets/Recursos/SpritesBotones. Recursos TMP: Assets/Resources/ControlIcons.
El recurso compartido conserva el nombre LobbyControls por compatibilidad.

Ejemplos:
- Presiona {control:confirmar} para continuar.
- Apunta al dispositivo y pulsa {control:interactuar}.
- Puedes usar el gatillo izquierdo {boton:L2} o el derecho {boton:R2}.

Se actualizaron las referencias explicitas a controles en Lobby, Module01,
Module02 y Module03. Los botones de pantalla y del switch conservan sus nombres;
Rayos X tampoco es una referencia al boton X del mando.
No se modificaron clips, IDs ni orden de pasos. Las grabaciones conservan su
contenido aunque cambie el boton asignado. No hay deteccion automatica del
aspecto de otros modelos de mando.
