# Módulo 3 · Sprint 8: propuesta de guion y flujo

Estado: borrador para validar antes de integrar diálogos, objetivos y waypoints.

## Alcance

Conectar la práctica existente con presentación del módulo, objetivos en muñeca, NPC, waypoints, progreso y reinicio. Mantener la laptop fija y los Canvas actuales. Tester y etiquetadora permanecen en la rueda. Quiz, logro final y regreso al lobby se reservan para el cierre posterior.

Una explicación inicial y una instrucción breve por objetivo. Los resultados y errores se consultan en las pantallas y herramientas; no generar conversaciones por cada intento fallido. Las instrucciones pueden volver a consultarse en la muñeca.

## Guion propuesto

| Momento | Texto del NPC | Objetivo en muñeca | Waypoint |
| --- | --- | --- | --- |
| Presentación | «Esta oficina tiene problemas de conectividad. Usa la laptop para consultar la red y comprobar tus reparaciones. Puedes revisar la configuración de cada PC en su propia pantalla. Sigue los objetivos de tu muñeca.» | Iniciar diagnóstico | Laptop |
| Incidente 1 | «Comprueba la conexión de PC-01. Si debes sustituir su patch cord, prueba los cables con el tester y etiqueta ambos extremos del reemplazo.» | Diagnosticar, sustituir y etiquetar el patch cord de PC-01; verificar con ping | PC-01; tester/repuesto como ayudas opcionales |
| Incidente 2 | «Revisa el estado de los puertos del switch desde la laptop y restablece el acceso de PC-02.» | Consultar puertos y habilitar el puerto correspondiente a PC-02 | Laptop |
| Incidente 3 | «Comprueba que responde el equipo esperado. Compara las direcciones de PC-02 y PC-03 con el inventario y corrige la configuración necesaria.» | Resolver la IP duplicada y verificar PC-02 y PC-03 | PC-03 después del diagnóstico |
| Comprobación final, sin diálogo adicional | — | Ejecutar nuevos pings a las tres PCs y comprobar la red completa | Laptop |

El waypoint orienta hacia el lugar de trabajo; no completa objetivos ni revela el puerto concreto a habilitar. Para el objetivo de IP puede mostrarse primero la laptop y habilitar la ayuda hacia PC-03 cuando el usuario la solicite.

## Criterios de avance

1. Cable: evidencia del tester sobre el cable retirado y el reemplazo, sustitución entre los puertos correctos, etiquetas en ambos extremos y ping vigente de Laptop a PC-01. Reutilizar CableRepairService.
2. Puerto: consulta de puertos registrada y SW-01/P02 habilitado. Todavía no exigir ping exitoso a PC-02: la IP duplicada puede impedirlo incluso con el puerto corregido.
3. IP: direcciones documentadas correctas, unicidad y nuevos pings exitosos a PC-02 y PC-03 con identidad del respondiente comprobada.
4. Final: todas las reglas técnicas, evidencia de reparación física y pings a las tres PCs posteriores al último cambio de red. La evidencia de ping anterior se invalida cuando cambia la revisión.

Registrar hitos de aprendizaje sin confundirlos con salud actual: una reparación previamente reconocida puede volver a fallar. El cierre siempre reevalúa la red y las evidencias; abrir un panel, visitar un waypoint o terminar un diálogo no acredita una reparación.

## Decisión de secuencia propuesta

Conservar por ahora las tres fallas presentes desde el inicio, como están en OfficeInitialState. Guiar el orden de diagnóstico sin volver a inyectar fallas ni sobrescribir cambios del usuario. Esto permite mostrar cómo habilitar el puerto hace visible el conflicto de IP. Si el usuario corrige algo antes de su etapa, reconocer el estado/evidencia ya obtenidos y no obligarlo a romperlo de nuevo.

Esta propuesta adapta la intención anterior de incidentes secuenciales a la implementación existente: la guía es secuencial; las fallas coexisten. Validar esta decisión junto con el guion antes de integrar el flujo.

## Integración prevista después de validar

Revisar los contratos existentes de ModuleDefinition, fábricas de objetivos, muñeca, secuencias NPC y waypoints de los módulos anteriores. Reutilizarlos con adaptadores hacia la sesión M3. Guardar configuración y textos en ScriptableObjects bajo Assets/GameData. Referencias de escena en componentes, nunca progreso mutable en assets compartidos.

Al reiniciar: restaurar red y conexiones iniciales, limpiar evidencias y progreso, cancelar animaciones y volver a la presentación. La rueda debe ofrecer únicamente las herramientas requeridas.

## Scripts de esta entrega

| Script añadido o modificado | Objetivo o función |
| --- | --- |
| Ninguno | Esta entrega prepara el guion para revisión; no modifica el flujo de ejecución. |

La tabla de scripts concretos y sus funciones se completará al implementar el sprint, después de validar este documento.
