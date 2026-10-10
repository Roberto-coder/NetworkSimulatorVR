# Recuperación del jugador

El menú de pausa incluye **Volver al inicio**. Muestra la pantalla de carga existente, recoloca el jugador XR y devuelve el control. Conserva los objetivos, cables y demás objetos de la escena.

El punto de retorno se registra al iniciar la escena en `PauseManager`: posición inicial de la cámara sobre el suelo del XR Origin y orientación inicial del rig. Al regresar se conserva la altura rastreada de la cabeza, incluida la postura sentada o agachada. Se sueltan los objetos agarrados y se reinicia la fuerza de caída antes de recolocar el jugador.

En `PlayerVR_XRI_Hands.prefab` se activó **Use Gravity** del Gravity Provider y se estableció **Step Offset = 0.001** del Character Controller: un milímetro con escala normal. El valor positivo evita la validación que aparecía con cero y limita la subida automática a desniveles mínimos. La gravedad permite volver al suelo. Los colliders del rack y sus posiciones permanecen intactos.

La pantalla utiliza tiempo sin escalar y restaura la locomoción al terminar. El rig se mueve bajo la pantalla opaca, sin cargar nuevamente la escena.

La compilación de los scripts pasó y se verificaron las referencias del nuevo botón. Falta comprobar en Play/VR: intentar caminar contra la base del rack, caer desde una superficie y usar Volver al inicio; comprobar después que el objetivo y las conexiones se conservaron.
