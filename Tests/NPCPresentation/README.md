# Coordinación de presentación del NPC

Ejecutar `dotnet run --project Tests/NPCPresentation/NPCPresentation.csproj` desde la raíz del proyecto.

Compila los controladores reales de diálogo, voz, reacciones y tutorial, con dobles deterministas de Unity para controlar el tiempo. Comprueba reemplazo de diálogos, salto durante escritura, cancelación, duración de audio, espera de felicidad/tristeza, último objetivo, recordatorios y recuperación ante estados de animación atascados.

Los dobles no validan la reproducción audible ni las transiciones visuales del Animator de Unity. La comprobación final en Play Mode/visor debe cubrir:

1. Completar un objetivo mientras suena una instrucción: se corta la voz, se ejecuta Feliz y después empieza el siguiente paso.
2. Provocar un error: Triste termina antes del próximo paso, incluso al saltar el texto de alerta.
3. Completar el último objetivo: el quiz espera la reacción final.
4. Saltar la escritura o cerrar un diálogo: el audio se detiene sin afectar al diálogo siguiente.
5. Reemplazar un recordatorio, detener el tutorial o desactivar el NPC: no queda audio ni una reacción pendiente.
