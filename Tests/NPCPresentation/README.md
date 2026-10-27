# Coordinación de presentación del NPC

Ejecutar `dotnet run --project Tests/NPCPresentation/NPCPresentation.csproj` desde la raíz del proyecto.

Compila los controladores reales de diálogo, voz, reacciones y tutorial, con dobles deterministas de Unity para controlar el tiempo. Comprueba reemplazo de diálogos, salto durante escritura, cancelación, duración de audio, espera de felicidad/tristeza, último objetivo, recordatorios y recuperación ante estados de animación atascados.

Los dobles no validan la reproducción audible ni las transiciones visuales del Animator de Unity. La comprobación final en Play Mode/visor debe cubrir:

1. Completar un objetivo mientras suena una instrucción: se corta la voz, se ejecuta Feliz y después empieza el siguiente paso.
2. Provocar un error: Triste termina antes del próximo paso, incluso al saltar el texto de alerta.
3. Completar el último objetivo: el quiz espera la reacción final.
4. Saltar la escritura o cerrar un diálogo: el audio se detiene sin afectar al diálogo siguiente.
5. Reemplazar un recordatorio, detener el tutorial o desactivar el NPC: no queda audio ni una reacción pendiente.
6. En Modulo1, Modulo2 y Modulo3, completar cada objetivo práctico: ambos mandos reciben un pulso y el NPC muestra «¡Bien hecho! Has completado el objetivo.» junto con Feliz. La instrucción siguiente espera a que termine la felicitación.
7. En cada traslado de los tres módulos, comprobar que el modelo mira hacia el recorrido y vuelve a mirar al jugador al llegar. Repetir con el jugador situado a un lado del recorrido.

La suite también comprueba el envío de un pulso a cada mando, el texto de felicitación durante Feliz y el cambio/restauración de mirada en MoveNpcStep, incluida su cancelación. El movimiento y la mirada usan dobles; su orientación real y los pulsos físicos requieren visor.

La felicitación admite una línea `objective_completed` en los datos de diálogos de cada módulo (texto, audio y voz heredada). Sin esa línea utiliza el mensaje compartido y `completedClip`, si está asignado. El prefab actual no tiene un clip de felicitación asignado; el mensaje predeterminado es texto.
