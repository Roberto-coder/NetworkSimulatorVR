# Estructura del módulo 2

La integración posterior del NPC se configura con el menú del
[sprint 8](Module02-Sprint08.md), que conecta el tutorial opcional descrito aquí.

Se reutiliza la división del módulo 1. La reorganización conserva los GUID de los
scripts movidos, los IDs de objetivos y los campos serializados del coordinador.
No requiere ejecutar de nuevo el configurador del sprint 7 ni reemplazar objetos
de la escena que ya funciona. Unity debe terminar de importar los cambios antes de Play.

```text
Assets/Scripts/Modules/Module02_RackInstallation/
  Domain/
    Module02ConnectionPlan.cs
    SwitchPowerState.cs
    SwitchConfigurationPuzzle.cs
  Factories/
    Module02ObjectiveFactory.cs
  Objectives/
    Module02ObjectiveCatalog.cs
    Module02Objective.cs
    Module02CablingObjective.cs
  Flow/
    Module02FlowController.cs
    Module02SequenceCoordinator.cs
    Validation/Module02SequenceRules.cs
  Interaction/
    Inspection/                   # detección de fichas y targets
    ...                           # herramientas, puertos y montaje físico
  Presentation/
    Module02PresentationController.cs
    Module02SequencePresenter.cs
    Inspection/                   # tarjeta, navegación y área del puntero
    Console/TerminalPuzzleView.cs
    Installation/RackInsertionGuide.cs
    Tutorial/
      Module02TutorialBuilder.cs
      Module02TutorialController.cs
  Module02Manager.cs

Assets/Editor/Modules/Module02_RackInstallation/
  Module02SequenceViewBuilder.cs
  ...                             # configuradores de los sprints

Assets/GameData/Module02/
  Definitions/                    # tipos ScriptableObject y contenido de fichas
  Module02Tutorial.asset
```

Los assets existentes de dispositivos, objetivos y definición del módulo se
conservan en sus carpetas actuales bajo `Assets/GameData`: `Module02_Devices`,
`Module02_Sequence`, `Objectives` y `Modules`. No se crea otra carpeta `Gamedata`
con distinta capitalización. Las referencias existentes siguen siendo válidas.

## Responsabilidades

- **Domain:** reglas y estados sin MonoBehaviour ni UI; las pruebas de .NET los
  ejecutan sin Unity.
- **Objectives:** catálogo ordenado de los nueve IDs y clases de objetivo.
  `Module02Objective` mantiene el ciclo común; no se duplican nueve clases idénticas.
  `Module02CablingObjective` conserva compatibilidad con escenas de mecánicas aisladas.
- **Factories:** crea objetivos a partir de ObjectiveData.
- **Flow:** coordina las condiciones reales, el avance secuencial, los spawners y
  el bloqueo de cables. `Validation` decide qué acción corresponde a cada fase.
- **Interaction:** agarre, detección, herramientas y respuesta física.
- **Presentation:** muestra el estado. `Module02SequencePresenter` formatea el
  contador, controla el botón y muestra rechazos. El coordinador conserva las dos
  referencias serializadas de UI como conexión compatible con la escena existente,
  pero delega su manejo al presenter.
- **Editor:** crea objetos de escena. `Module02SequenceViewBuilder` construye Canvas
  y botón; el configurador del sprint se encarga de conectarlos al flujo.
- **GameData:** contenido editable y definiciones de datos. Títulos e instrucciones
  pertenecen a ObjectiveData. La introducción y el cierre opcionales están en
  `Module02Tutorial.asset`.

Los tipos puros usan namespaces Domain, Factories y Flow.Validation. Los componentes
y datos trasladados conservan sus namespaces anteriores además de sus GUID para
evitar cambios innecesarios en la serialización y en referencias externas.

## Tutorial reutilizable

En el módulo 1, el Builder construye pasos narrativos: no construye un Canvas.
El módulo 2 ahora sigue esa misma separación:

1. `Module02TutorialBuilder` compone introducción, movimiento opcional al rack,
   instrucciones de los objetivos y cierre.
2. `Module02TutorialController` conecta datos, flujo y director, y lo inicia.
3. El `TutorialDirector` compartido ejecuta los pasos existentes.

El director y `GuidedObjectiveStep` dependen de `Core.Objectives.IObjectiveFlow`,
implementado por los flujos de los módulos 1 y 2. Para el módulo 3 bastará implementar
ese contrato y crear su Builder/Controller. La interfaz solo permite observar progreso;
el tutorial no puede completar objetivos físicos.

La única modificación al flujo del módulo 1 es declarar esa interfaz. Su lógica
de objetivos, quiz y reacciones no cambia. `NPCReactionController` aún conserva sus
reacciones específicas del módulo 1; no se conectó automáticamente al módulo 2.

El tutorial del módulo 2 es opcional y **no se agregó al NPC de la escena** en esta
reorganización. Para conectarlo cuando se trabaje el NPC: añadir
`Module02TutorialController`, asignar Manager, un TutorialDirector con su controlador
de diálogo, el asset `Assets/GameData/Module02/Module02Tutorial.asset` y, opcionalmente,
el waypoint del rack. No compartir el director simultáneamente con otro tutorial.
El guion básico lee Description de los ObjectiveData existentes; puede ampliarse
después sin mezclar narrativa con permisos o aparición de objetos.

## Verificación

Compilan runtime y Editor con Roslyn de Unity 6000.2.7f2; solo advertencias TMP
preexistentes. Pasan las 598 aserciones de dominio y permisos tras actualizar rutas
y namespaces del proyecto de pruebas. Se verifican los GUID de scripts trasladados.

Pendiente en Unity/VR: confirmar que las fichas, el agarre durante inspección, los
objetivos de muñeca, los spawners y el encendido conservan su comportamiento.
El nuevo tutorial opcional requiere sus referencias de NPC antes de probarlo.
