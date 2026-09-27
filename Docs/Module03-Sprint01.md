# Módulo 3 — Sprint 1: modelo y configuración de red

## Entrega

Implementados el dominio C# independiente de Unity, los dos ScriptableObjects, assets de oficina y validación semántica. No se modifican escenas ni el demo de Splines existente. Las clases contienen comentarios sobre IDs, copia de estado, continuidad pasiva, registro y comparación por extremos.

La configuración carga **datos de dispositivos** en memoria. La creación/asociación de GameObjects, catálogo de prefabs y disposición física corresponden al sprint 2. Las claves de prefab incluidas son identificadores de autoría; aún no resuelven prefabs.

## Archivos principales

- `Assets/Scripts/Modules/Module03_Diagnostics/Domain/NetworkDefinition.cs`: dispositivos, interfaces/puertos, cables y continuidad interna de elementos pasivos.
- `NetworkDefinitionValidator.cs`: esquema, IDs, referencias, ocupación de puertos, formato de direcciones, MAC y conflictos iniciales declarados.
- `NetworkSession.cs`: copia de ejecución, operaciones de configuración/conexión/etiquetado, vecinos físicos, revisión y bitácora.
- `NetworkTargets.cs`: reglas finales y resultados con ID, esperado, observado y cumplimiento.
- `Assets/GameData/Module03/Definitions/`: tipos ScriptableObject para inicial y reglas finales.
- `Assets/GameData/Module03/Topologies/OfficeInitialState.asset`.
- `Assets/GameData/Module03/Validation/OfficeTargetRules.asset`.
- `Assets/Editor/Modules/Module03_Diagnostics/Module03DataValidation.cs`: inspección desde menú.

## Modelo de la oficina

Incluye 3 PCs, laptop, switch, patch panel y 3 rosetas (9 dispositivos), 21 puertos, 10 cables instalados, un cable de repuesto desconectado y 6 conexiones pasivas internas.

La IP de gestión del switch pertenece a `SW-01/mgmt`, una interfaz lógica sin cable propio. El futuro servicio de red resolverá su acceso desde los puertos del switch. Las rosetas y el patch panel tienen puertos frontales/posteriores y continuidad explícita; no reciben IP ni MAC.

Solo los patch cords PC–roseta y el repuesto son interactuables. Los enlaces de rack y laptop son fijos. Inicialmente está roto `Patch-01`; el resto está sano. Esto respeta el inicio del primer incidente. La introducción secuencial de las otras fallas será responsabilidad del coordinador de incidentes, no del asset final.

Las 10 reglas finales comprueban direcciones únicas por segmento y, para cada PC, IP esperada, puerto del switch habilitado y patch cord íntegro con sus etiquetas. Al cargar, debe quedar pendiente únicamente `patch-PC-01`.

## Uso en Unity

1. Esperar la importación y comprobar que no hay errores de compilación.
2. Revisar los assets inicial/final en Inspector. Los ScriptableObjects también se pueden crear con **Create > Network Simulator > Module 03 > Initial State / Target Rules**.
3. Ejecutar **Network Simulator > Module 03 > Sprint 1 - Validar datos**.
4. La consola debe mostrar 9 reglas cumplidas, una pendiente por el cable roto y el mensaje de configuración válida. No se generan objetos ni se guardan cambios en escena.

Integración desde código, cuando se implemente el manager:

```csharp
NetworkSession session = initialAsset.CreateSession();
session.ConnectCable("Patch-01", "", "");
session.ConnectCable("Replacement-01", "R01/front", "PC-01/eth0");
session.LabelCable("Replacement-01", "R01", "PC-01");
var results = targetAsset.Evaluate(session);
```

No existe un método de usuario para convertir el cable roto en sano: la reparación exige usar el repuesto. Una conexión rechazada queda registrada y no incrementa la revisión. Repetir un valor existente tampoco incrementa la revisión. `Snapshot()` devuelve una copia y `Reset()` restaura la configuración inicial, conserva la bitácora con un evento de reinicio e incrementa la revisión para invalidar evidencias anteriores. Para una partida nueva con bitácora vacía se crea otra sesión.

Las etiquetas permanecen asociadas a los extremos físicos A/B. La regla reconoce orientación inversa del cable y compara cada etiqueta con el puerto correspondiente. El ID del cable retirado no es una condición de éxito.

## Escalabilidad

Para añadir una PC: definir dispositivo, puerto con MAC/IP, puerto disponible en switch, continuidad pasiva necesaria y cables con IDs nuevos. Añadir reglas de IP, puerto y conexión para esa PC. La regla global de unicidad incorpora automáticamente los nuevos puertos del segmento. En el sprint 2 se asignarán ubicación y prefab.

Los IDs son sensibles a mayúsculas y no admiten espacios externos. Las direcciones IPv4 usan cuatro octetos decimales sin ceros iniciales. El alcance de prefijos es /1–/30. Esta entrega valida formato y unicidad; selección de direcciones utilizables, subredes, alcance y resolución de vecinos IP se completarán con el simulador del sprint 3. Los vecinos físicos incluyen cables rotos: vecindad no significa conectividad.

No se admite un archivo de reglas vacío como éxito. Referencias inválidas o tipos de reglas desconocidos se rechazan antes de evaluar. La evaluación no considera posiciones visuales, orden de listas ni identidad específica del repuesto.

## Verificación

- `dotnet run --project Tests/Module03Diagnostics/Module03Diagnostics.csproj`: **54 comprobaciones correctas**.
- Compilación del código nuevo de dominio, GameData y Editor con Roslyn y referencias de **Unity 6000.2.7f2**, C# 9 y .NET Standard 2.1: correcta.
- Pruebas de copia independiente, serialización JSON de DTOs, reglas inválidas, duplicidad, puertos ocupados, reemplazo invertido, etiquetas, reinicio y ampliación a 12 PCs.

Pendiente de comprobación dentro del Editor: importación/deserialización de los assets y ejecución del menú anterior. La compilación aislada no sustituye esa comprobación. No se ha ejecutado Play/VR: este sprint no incorpora interacción de escena.

## Límites para siguientes sprints

«Todas las reglas actuales cumplen» significa que se cumplen las condiciones de configuración implementadas; todavía no equivale a conectividad verificada ni módulo completado. Ping, trazas, evidencia de tester, objetivos de muñeca, logros, guardado persistente y secuencia de incidentes se integrarán en sus sprints. Los DTOs son serializables, pero aún no hay servicio de guardado/carga de partidas.


## Scripts añadidos y responsabilidades

Incluye ampliaciones posteriores del sprint; los archivos reutilizados o modificados se indican expresamente.

| Script | Objetivo o función |
| --- | --- |
| `NetworkDefinition.cs` | Define dispositivos, puertos, cables y continuidad pasiva; copia los datos. |
| `NetworkDefinitionValidator.cs` | Valida IDs, referencias y direcciones. |
| `NetworkSession.cs` | Mantiene estado, revisión y bitácora; aplica cambios. |
| `NetworkTargets.cs` | Evalúa reglas de recuperación contra el estado. |
| `NetworkInitialStateAsset.cs` | ScriptableObject que conserva y crea el estado inicial. |
| `NetworkTargetRulesAsset.cs` | ScriptableObject de condiciones finales. |
| `Module03DataValidation.cs` | Valida los assets desde el Editor. |
