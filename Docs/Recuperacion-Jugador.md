# Recuperación del jugador

El menú de pausa incluye **Volver al inicio**. Muestra la pantalla de carga existente, recoloca el jugador XR y devuelve el control. Conserva los objetivos, cables y demás objetos de la escena.

También está disponible en el Lobby, que utiliza el mismo PlayerRoot con OVRCameraRig del módulo 1 y hereda el botón del prefab PauseMenu. El punto de retorno corresponde al inicio del Lobby.

El punto de retorno se registra al iniciar la escena en `PauseManager`: posición inicial de la cámara sobre el suelo del rig y orientación inicial del rig. Es compatible con el OVRCameraRig de Meta del módulo 1 y con el XR Origin de los módulos 2 y 3. Al regresar se conserva la altura rastreada de la cabeza, incluida la postura sentada o agachada. En XR se sueltan las selecciones de los interactores y se reinicia la fuerza de caída. En Meta se reinicia la velocidad del FirstPersonLocomotor y se traslada su cápsula junto con la cámara para impedir que devuelva al jugador a la ubicación anterior.

En `PlayerVR_XRI_Hands.prefab` se activó **Use Gravity** del Gravity Provider y se estableció **Step Offset = 0.001** del Character Controller: un milímetro con escala normal. El valor positivo evita la validación que aparecía con cero y limita la subida automática a desniveles mínimos. La gravedad permite volver al suelo. Los colliders del rack y sus posiciones permanecen intactos.

La pantalla utiliza tiempo sin escalar y restaura la locomoción al terminar. El rig se mueve bajo la pantalla opaca, sin cargar nuevamente la escena.

La compilación de los scripts pasó y se verificaron las referencias del nuevo botón. Falta comprobar en Play/VR: intentar caminar contra la base del rack, caer desde una superficie y usar Volver al inicio; comprobar después que el objetivo y las conexiones se conservaron.
