# Módulo 3 · Sprint 9: quiz, logro y regreso al lobby

## Resultado

Cierre integrado con QuizController, QuizView, SaveManager y SceneTransitionManager compartidos. El Canvas es una instancia fija creada en Editor, editable bajo `_UI`; no se construye ni se reposiciona en Play. El cierre es independiente de Tutorial Enabled.

Requiere los cuatro hitos y la validación inmediata de todas las reglas/evidencias vigentes. Los objetivos completados por sí solos no permiten abrir ni entregar el quiz con una red nuevamente averiada. Si cambia antes del guardado, se oculta el quiz; tras reparar y verificar se abre un intento nuevo. La guía recupera su estado habilitado anterior.

## Aplicar

1. Fuera de Play, abre Modulo3 con el sprint 8 preparado.
2. Ejecuta **Network Simulator > Module 03 > Sprint 9 - Quiz y cierre**.
3. Ajusta `_UI/QuizCanvas_Module03` a una ubicación cómoda y libre de obstáculos. Está desactivado en edición por defecto; puedes activarlo para colocarlo y volver a ocultarlo.
4. Revisa `_Managers/Module03 finale`: Flow, Quiz Root, Quiz, Actions y Guidance. Si el flujo no está bajo `_Managers`, el cierre se crea junto a él.
5. Guarda la escena y verifica que Modulo3 y Lobby estén habilitados en Build Settings. El configurador no modifica la lista de escenas de compilación.

El menú reutiliza la configuración existente sin crear otro cierre. Crea el prefab `Assets/Prefabs/UI_Components/Quiz/Module03/QuizCanvas_Module03_XRI.prefab` copiando la presentación XR del módulo 2 si aún no existe. Mantiene la vista de botones Module02QuizActionsView, que no contiene lógica de objetivos ni guardado. No modifica el prefab del módulo 2; sus opciones de respuesta compartidas siguen siendo dependencias visuales.

## Quiz y criterio de finalización

`Assets/GameData/Quiz/Module03Quiz.asset` contiene ocho preguntas de cuatro opciones: significado de ping, ARP, puerto deshabilitado, IP duplicada, continuidad, verificación desde laptop, vigencia de resultados y rayos X. Índice de respuesta correcto de base cero. La referencia de aprobación formativa es 70 %, editable en QuizData.

Siguiendo módulo 2, la insignia acredita práctica verificada y entrega completa del quiz; no exige superar el porcentaje. Se muestra calificación y se permite repetir el quiz. No se agregaron textos nuevos al NPC ni una conversación obligatoria de cierre.

El configurador asigna Final Quiz y la insignia existente Module03Completion a Module03_Diagnostics. Se guarda mediante TryCompleteModuleLocally: completitud, objetivos y tiempo, sin añadir persistencia de respuestas/puntuación que el servicio actual no contempla. El tiempo sigue el criterio existente de tiempo real desde la carga de escena, incluyendo pausas.

Reintentar guardado no duplica el registro en una entrega ya guardada. El logro se muestra después de confirmar el guardado local. Volver al lobby y reiniciar módulo se habilitan tras guardar; una escena no disponible genera un mensaje en la vista. Repetir el quiz no suma nuevamente el tiempo de la sesión. El progreso ya persistido se conserva al recargar.

## Scripts añadidos y modificados

| Script | Estado | Objetivo o función |
| --- | --- | --- |
| Module03FinaleController.cs | Añadido, Presentation | Apertura del quiz con validación vigente, entrega, guardado, insignia, reintento y navegación. |
| Module03FinaleSetup.cs | Añadido, Editor | Copia/reutiliza prefab XR, asigna datos de cierre y coloca un Canvas fijo en escena. |
| Module03GuidedFlow.cs | Modificado | CanFinishPractice revalida estado y evidencia inmediatamente, sin usar un indicador desactualizado. |
| QuizController / QuizView | Reutilizados sin cambios | Preguntas, selección, resultados y repetición del quiz. |
| Module02QuizActionsView | Reutilizado sin cambios | Botones de reinicio, reintento de guardado y estado de salida. |
| SaveManager / SceneTransitionManager | Reutilizados sin cambios | Persistencia local y transición entre escenas. |

## Validación realizada y pendiente

Compilación externa de runtime/Editor correcta con referencias Unity; advertencias existentes de campos serializados. 193 comprobaciones de dominio/flujo anteriores siguen aprobadas. Comprobada estructura del nuevo asset: ocho preguntas, cuatro opciones e índices válidos. Estas pruebas no ejecutan el nuevo Canvas, la escritura de progreso ni las transiciones de escena.

Pruebas manuales requeridas en Unity/visor:

1. Con tutorial activado y desactivado: el quiz aparece solo después de verificar toda la práctica.
2. Cambio de IP/cable tras último ping impide abrir/cerrar el módulo; repetir las pruebas habilita un intento nuevo.
3. Todas las preguntas requieren respuesta antes de entregar; comprobar nota baja, repetición y calificación final.
4. Entregar dos veces/repetir quiz no duplica tiempo ni logro guardados dentro de la sesión.
5. Fallo de guardado muestra reintento y bloquea salida; recuperación permite continuar.
6. Reinicio de práctica cancela el intento abierto. Reiniciar módulo desde resultados recarga Modulo3 conservando progreso persistido.
7. Regreso al lobby refleja completitud/insignia; escena deshabilitada muestra mensaje.
8. Legibilidad en ambos ojos, interacción XR, panel fijo y ausencia de voz superpuesta al quiz.
