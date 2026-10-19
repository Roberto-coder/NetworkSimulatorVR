# Distribución del perfil

El perfil conserva el contenido original: identidad, selección de módulo, detalle
de progreso e insignias. Las filas adicionales de resumen permanecen ocultas.

Se amplió el avatar a 180 px y las insignias a 100 px. El detalle del módulo tiene
alto preferido de 240 px, tipografía mayor y espacio flexible entre sus líneas.
La separación entre secciones aumentó a 12 px para ocupar mejor el alto disponible.

La distribución está guardada en Lobby.unity y en LobbyProfilePanelSetup.
ProfilePanelPresenter respeta los altos ampliados al actualizar el contenido.

Compilación C# comprobada con referencias Unity. Pendiente: revisión visual en
Unity/visor con la escala real del canvas.
