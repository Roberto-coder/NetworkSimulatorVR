# Avisos de conexión en Lobby y Menu

Prefab: `Assets/Prefabs/UI_Components/GlobalUI/SyncStatusDialog.prefab`.
Está como hijo del Canvas del panel principal del Lobby y de los canvas Login,
Registro, MainMenu y selección de partidas de Menu. Hereda escala e interacción
del anfitrión. No tiene Canvas propio, Distance ni seguimiento del jugador.
Se retiró la instancia persistente de GlobalSystems.

## Layout e icono

- ConnectionPanel: VerticalLayoutGroup y ContentSizeFitter vertical.
- Actions: HorizontalLayoutGroup para Reintentar y Cerrar.
- Textos, icono y botones: LayoutElement para tamaños y expansión.
- WiFiIcon: Image con Preserve Aspect, altura preferida de 72 px.

Para añadir Wi-Fi: abrir el prefab, asignar un sprite al Image de WiFiIcon y activar
ese GameObject. Empieza desactivado para no dejar un hueco vacío. Al activarlo,
el panel aumenta su altura. El mensaje usa el alto preferido de TMP y un mínimo
de 120 px para adaptarse al contenido.

## Operaciones

- Sincronizar requiere sesión Firebase y conexión; ofrece reintento ante error.
  Mientras hay una subida pendiente, Reintentar está deshabilitado.
- Login normal, Google y registro comprueban conexión y muestran el aviso en el
  canvas activo. Reintentar ejecuta la operación correspondiente, no una subida.
- La cuenta debug conserva su acceso local, sin Firebase.
- La consulta de progreso tras login/restauración distingue un error de una
  respuesta correcta sin save remoto. Ante fallo usa la copia local y ofrece
  reintentar la descarga.
- Cargar partida selecciona el slot del archivo local: no necesita Internet ni
  descarga datos. La nube se consulta al iniciar/restaurar sesión; las subidas
  siguen siendo explícitas al pulsar Sincronizar.
- RestoreSessionData mantiene los cambios locales pendientes ante una descarga.
- Cerrar no elimina progreso ni cancela una subida. Tras 20 segundos sin
  confirmación de subida se muestra espera sin permitir una subida duplicada.

## Verificación y pruebas APK

Compilación con referencias Unity y 36 comprobaciones de guardado aprobadas.
Estructura YAML y referencias de prefab/escenas revisadas.

Pendiente en Unity/visor y APK:

1. Login y registro sin Wi-Fi: aviso en su panel; reconectar y reintentar.
2. Con sesión restaurada y copia local: cargar partida sin red.
3. En Lobby: Sincronizar sin red, reconectar y Reintentar hasta confirmar.
4. Asignar y activar WiFiIcon: comprobar disposición sin solapamiento.

La interfaz está guardada en el prefab. El controlador conecta eventos y cambia
textos/visibilidad; no crea objetos de interfaz en tiempo de ejecución.

## Aviso modal y simulación con Link

Al abrir el aviso se ocultan las ramas de UI hermanas dentro de su canvas anfitrión.
No se desactiva el canvas completo ni sus superficies de interacción Meta/XRI.
Al cerrar, cancelar, confirmar éxito o desactivar el canvas se restaura el estado
previo de cada rama, sin activar pantallas que estaban ocultas. Reintentar login
restaura primero los campos originales para reutilizar sus valores.

En el Editor, activar **Network Simulator > Pruebas > Simular falta de conexión**.
Luego intentar login/registro normal o Sincronizar con una sesión Firebase. El menú
muestra una marca mientras la simulación está habilitada. Desactivarlo antes de
Reintentar para usar la red real. No desconecta Link y no se incluye en el APK.
La cuenta debug mantiene su excepción local y no sirve para probar subidas Firebase.

Pruebas: `Tests/SyncDialog` verifica 13 casos del aviso modal y la restauración de
pantallas. `Tests/SaveSafety` conserva sus 36 comprobaciones aprobadas.

La ocultación excluye los RectTransform con PointableCanvas: las superficies
ISDK_RayCanvasInteraction/ISDK_PokeCanvasInteraction deben mantenerse activas.
Los Image de Reintentar y Cerrar tienen Raycast Target activado, y las instancias
del aviso permanecen en el plano Z=0 del canvas anfitrión para la interacción Meta.
