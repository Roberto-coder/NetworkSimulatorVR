# Acceso a módulos y tutorial del Lobby

El acceso se calcula con `ModuleAccessPolicy` y el progreso de la partida activa:

- M1 está disponible desde el inicio.
- Completar M1 habilita M2; completar M2 habilita M3.
- Empezar un módulo o tener una insignia por sí solos no habilita el siguiente.
- `SessionContext.IsDebugSession` habilita los tres sin alterar los registros de completitud.
- El botón de la tarjeta, las transiciones por nombre/índice y el perfil consultan la misma regla.
- Las pestañas bloqueadas del perfil siguen siendo consultables para mostrar su estado.

Se reconocen los IDs reales `M1_`, `module_02_rack_installation` y `module03`,
además de los alias históricos M2_/M3_ y los nombres de escena.

## Tutorial

`SaveFile.startedTutorialIds` registra la primera visita por cuenta. Se conserva al
crear otra partida y se serializa también en la copia enviada a Firebase.
Los saves antiguos con tutorial completado o módulos completados se consideran
usuarios que ya visitaron el simulador. Debug omite el arranque automático.

El inicio se registra antes de ejecutar la secuencia: un tutorial interrumpido no
arranca automáticamente en cada regreso. Se puede volver a ejecutar con el botón.
La completitud continúa guardándose por partida en `completedTutorialIds`.

Repetir tutorial solicita recargar Lobby mediante `SceneTransitionManager`.
La recarga reconstruye el rig en la posición inicial de la escena y reinicia al NPC.
La petición de repetición se consume una vez y está asociada a la cuenta local.
La secuencia espera a que desaparezca la pantalla de carga antes de empezar.
No se eliminan el progreso ni las insignias.

## Validación

- `dotnet run --project Tests/SaveSafety/SaveSafety.csproj --no-restore`: 36 comprobaciones aprobadas.
- Compilación C# de 300 fuentes con las referencias del proyecto Unity: sin errores.
- Pendiente de verificación en Play Mode/VR:
  1. Cuenta nueva: M1 disponible, M2/M3 bloqueados en selector y perfil; tutorial automático.
  2. Regresar al Lobby o reiniciar: el tutorial no arranca otra vez.
  3. Completar y guardar M1: M2 disponible, M3 bloqueado.
  4. Completar y guardar M2: M3 disponible.
  5. Cuenta de debug: tres módulos disponibles y sin tutorial automático.
  6. Repetir tutorial desde el panel: carga, jugador en inicio y secuencia desde el primer diálogo.
