# Módulo 3 · Sprint 7: puerto inhabilitado e IP duplicadas

## Alcance

La laptop fija administra los puertos del switch seleccionado en el minimapa. Cada PC permite cambiar únicamente su propia IPv4 y prefijo desde su Canvas local. Seleccionar otra PC en el mapa no concede acceso para editarla. Los controles se guardan en el prefab común; no se construyen Canvas durante Play.

## Aplicar en Unity

1. Sal de Play y abre la escena del módulo 3 que ya tiene los sprints anteriores aplicados.
2. Ejecuta **Network Simulator > Module 03 > Sprint 7 - Puertos e IP**.
3. Guarda la escena. El configurador añade Administración al prefab `Assets/Prefabs/Dispositivos/Modulo3/DiagnosticScreen.prefab`, actualiza vistas antiguas que aún no tengan estos controles y asigna el asset al controlador existente. Conserva las ubicaciones actuales bajo `_Interactuables`, `_UI` y `_Managers`.
4. Revisa en el prefab el tamaño y la posición del panel con el visor. Los botones se accionan con la interacción UI XR existente; A abre/cierra la pantalla del dispositivo.

El configurador crea `Assets/GameData/Module03/Presentation/NetworkAdministrationSettings.asset` si falta. Ejecutarlo nuevamente reutiliza el asset y los controles existentes.

## Recorrido de prueba

1. Desde la laptop, selecciona PC-02 y ejecuta Ping. Puede responder PC-03 con la dirección duplicada: **Responde otro equipo** no es éxito del incidente.
2. Selecciona SW-01, consulta puertos y abre **Administrar**. Usa **Siguiente puerto** hasta `SW-01/P02`; habilítalo.
3. Vuelve al mapa y repite ping a PC-02. Con ambos equipos accesibles y la misma dirección, aparecerá conflicto de direcciones.
4. Acércate a PC-03, abre su pantalla con A y **Administrar**. Selecciona `192.168.10.13`, luego **Aplicar IP**. El prefijo documentado se aplica automáticamente. Las opciones incorrectas se permiten para practicar diagnóstico; no se aplica automáticamente la respuesta correcta.
5. Vuelve al mapa y realiza pings a PC-02 y PC-03 después del último cambio. **Verificar incidentes** comprueba reglas y evidencia vigente de ambos equipos. El origen de ping sigue siendo la laptop, incluso al consultar desde una pantalla local.
6. Cambia otra dirección o puerto: la evidencia previa deja de ser vigente. Repite consultas tras corregirlo.

También comprobar: intentar editar una PC desde la laptop; seleccionar otro equipo mientras estás físicamente en una PC; salir del alcance con el panel abierto; pausar; repetir el configurador sin duplicar controles.

La administración remota requiere alcanzar la IP de gestión del switch en cada escritura. Si deshabilitas el puerto de la laptop o la interfaz de gestión, ya no puedes reactivarlo por ese mismo canal. En este sprint reinicia Play para recuperar el estado inicial; aún no se simula una consola de administración fuera de banda.

## Datos y validación

- `targetRules`: reglas finales existentes; también proporciona las direcciones documentadas para el minimapa. PC-03 conserva su IP duplicada en el estado inicial, pero el inventario documenta `.13`.
- `addressOptions`: direcciones seleccionables con botones, sin teclado VR. El prefijo procede del inventario documentado (/24 en esta oficina).
- `requiredRuleIds`: `enabled-P02`, `address-PC-02`, `address-PC-03`, `unique-addresses`.
- `verificationDeviceIds`: PC-02 y PC-03. La verificación exige respuesta del puerto del equipo esperado y resultados de la sesión/revisión actuales.

La revisión cambia al modificar la red, no al consultar ni al repetir una escritura sin cambios. Las acciones y rechazos del dominio quedan en la bitácora. Esta verificación corresponde a los incidentes de puerto/IP: no sustituye la evidencia de tester, reemplazo y etiquetas del sprint 6 ni implementa todavía el cierre general del módulo, NPC o quiz.

## Scripts añadidos y modificados

| Script | Estado | Objetivo o función |
| --- | --- | --- |
| `NetworkAdministrationSettings.cs` | Añadido, GameData | Configura propuestas de IP/prefijo, reglas y dispositivos a verificar. |
| `NetworkAdministrationView.cs` | Añadido | Referencias serializadas a los controles del panel fijo y sus selecciones temporales. |
| `Module03AdministrationSetup.cs` | Añadido, Editor | Actualiza el prefab compartido, conecta el asset y prepara las vistas existentes. |
| `DiagnosticCanvasBuilder.cs` | Modificado, Editor | Construye los controles de administración únicamente durante la autoría. |
| `DiagnosticScreenController.cs` | Modificado | Enlaza botones, presenta configuración, aplica acciones y muestra la verificación. |
| `DiagnosticWorkspace.cs` | Modificado, Domain | Restringe edición al contexto local, valida acceso remoto y comprueba evidencia de ping. |
| `NetworkTargetRulesAsset.cs` | Modificado, GameData | Deriva el inventario documentado desde las reglas finales sin modificar el estado inicial. |
| `AdministrationChecks.cs` | Añadido, pruebas | Verifica permisos, conflicto ARP, identidad del respondiente, revisión y separación del inventario. |
| `Program.cs` | Modificado, pruebas | Ejecuta las nuevas comprobaciones junto con las anteriores. |

## Validación realizada

148 comprobaciones de lógica correctas con `dotnet run --project Tests/Module03Diagnostics/Module03Diagnostics.csproj --no-restore`. Compilación de los scripts del módulo y configuradores con referencias Unity correcta; advertencia existente CS0649 del campo serializado `VRInputManager.backend`.

Pendiente en Unity: ejecutar el configurador, guardar prefab/escena y probar interacción y legibilidad en visor. La compilación externa no confirma la ejecución del configurador ni la colocación visual final.


## Simplificación del prefijo

Se retira la edición del prefijo porque no corresponde a los tres incidentes del módulo. La pantalla conserva su consulta. Los prefijos iniciales de la oficina son /24 y aplicar una IP usa el prefijo de las reglas documentadas. Los Canvas anteriores ocultan su botón de prefijo al iniciar; repetir el configurador actualiza también el prefab.

| Script modificado | Función |
| --- | --- |
| `DiagnosticWorkspace.cs` | Aplica la dirección con el prefijo documentado mediante una sobrecarga. |
| `DiagnosticScreenController.cs` | Oculta el selector antiguo y utiliza Aplicar IP. |
| `DiagnosticCanvasBuilder.cs` | Deja de crear el selector de prefijo y migra el prefab existente. |
| `NetworkAdministrationSettings.cs` | Elimina la lista de prefijos seleccionables. |
| `AdministrationChecks.cs` | Comprueba que editar IP restablece el prefijo esperado. |

El origen actual de ping y ARP sigue siendo `DiagnosticUiSettings.sourcePortId` (Laptop/eth0 en el asset). Seleccionar un nodo define el destino. Ping local PC→PC y perfiles de opciones por dispositivo son propuestas de evolución, todavía no implementadas con este ajuste.
