# Evidencia de conservación de la copia local

La política implementada está en `Docs/Guardado-Hibrido-Estados.md`: al restaurar
sesión, si existe cualquier slot con `needsCloudSync=true`, se conserva el archivo
local completo. Sin pendientes locales y con respuesta remota se importa Firebase.
No se comparan fechas o revisiones ni se combinan partidas. Cargar un slot no
consulta Firebase; la descarga ocurre al iniciar/restaurar sesión.

## Preparar una diferencia real

1. Usar una cuenta Firebase de prueba, con una partida existente. La cuenta debug
   no sirve para esta prueba. Partir de una copia previamente sincronizada.
2. Capturar la tarjeta original y, si está disponible, su `saveData` en Firebase.
3. En Play, seleccionar la partida y ejecutar **Network Simulator > Pruebas >
   Preparar diferencia local con respaldo**. La herramienta modifica solo el título
   añadiendo `PRUEBA LOCAL - ` y marca `needsCloudSync=true`. No cambia módulos,
   logros, desbloqueos ni el servidor. Crea antes un respaldo `.evidencia-*.bak`
   junto al archivo y muestra ambas rutas en Console.
4. Capturar el JSON local con el título modificado y la marca pendiente, y el dato
   remoto original. Ocultar UID, correo y credenciales en el documento.
5. Detener Play y volver a iniciar sesión con la misma cuenta y conexión disponible.
   Reabrir el panel de partidas para reconstruir sus tarjetas con los datos actuales.
6. Capturar la tarjeta con `PRUEBA LOCAL - ` y el estado pendiente. Cargar la partida
   y comprobar que mantiene su progreso. No pulsar **Sincronizar todo** durante
   la prueba: esa acción escribiría la copia modificada en Firebase.

La tarjeta muestra el número de partida y, debajo, el campo `moduleTitle` del
guardado. El marcador se ve en esa segunda línea; no cambia el nombre del módulo
en el selector de módulos ni los títulos del tutorial.

Resultado esperado: se conserva el archivo local pendiente aunque Firebase
devuelva otra copia. La interfaz actual informa el estado pendiente en las tarjetas;
no existe un modal de conflicto entre versiones. Una captura de este caso demuestra
la política de conservación local, no una resolución multidispositivo completa.

## Recuperar el original

Con Play detenido, copiar el respaldo `.bak` sobre su archivo `save_UID.json`
correspondiente. Conservar el respaldo y volver a entrar con la misma cuenta.
Si se subió por accidente el marcador, restaurar primero el respaldo y revisar
la copia antes de realizar otra sincronización explícita.

No se han tomado capturas del visor ni se ha ejecutado esta prueba contra Firebase
automáticamente. Las capturas deben proceder de la sesión real descrita arriba.
