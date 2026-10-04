# Iconos de controles: prueba en Lobby

Edita Assets/GameData/Lobby/LobbyTutorial.asset con estos marcadores:

- {control:confirmar}: boton de avanzar dialogo (B por defecto).
- {control:herramientas}: rueda de herramientas (Y por defecto).
- {control:pausa}: menu de pausa (X por defecto).
- {control:usar}: gatillo de herramienta (R2 por defecto).

Ejemplo: Presiona {control:confirmar} para continuar.

Solo se modificaron los textos del Lobby que mencionaban B, Y y R2. No se
agrego un dialogo de pausa ni se cambiaron clips de voz o IDs.

VRInputManager.GetControlIcon consulta los botones Meta OVR o las rutas OpenXR
configuradas. El texto se resuelve al comenzar cada dialogo. Las rutas no
reconocidas aparecen como texto, evitando mostrar un boton incorrecto.
Esto representa los iconos Quest suministrados; no detecta automaticamente
la apariencia de otros modelos de mando. Las grabaciones no cambian si se
reasigna un boton: habra que actualizarlas si mencionan su nombre anterior.

Los TMP Sprite Assets estan en Assets/Resources/ControlIcons y referencian las
imagenes originales de Assets/Recursos/SpritesBotones. LobbyControls agrupa
los iconos como fallbacks. Cada imagen mantiene su material y proporcion;
TMP adapta la altura al texto. Para ajustar alineacion o tamano fino, modifica
Bearing Y y Scale en la tabla de glifos del asset correspondiente.

DialogueTextAnimator convierte los marcadores antes de calcular los caracteres
visibles. Por eso el efecto de escritura y Skip muestran cada sprite completo.
Los dialogos sin marcadores siguen usando su texto habitual.

Pendiente verificacion visual en Unity/visor: bienvenida con B, herramientas
con Y/R2, saltar escritura, y cambiar Confirm Button a One para comprobar A.
