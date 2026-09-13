# Sprint 7: integración secuencial de la práctica

La organización posterior del código y la separación de tutorial/presentación se
describen en [Module02-Architecture.md](Module02-Architecture.md). Los configuradores
están ahora en `Assets/Editor/Modules/Module02_RackInstallation`; el menú no cambia.

## Configurar en Unity

Con `Assets/Scenes/Modulo2.unity` abierta y fuera de Play, ejecutar
`Network Simulator > Module 02 > Infraestructura > Sprint 7 - Flujo secuencial`.
Requiere los sprints anteriores configurados, cinco cables activos con etiquetas,
las fichas del rack y del switch, y un único conjunto de controladores.

El configurador crea `Module02_Sequence`, nueve ObjectiveData en
`Assets/GameData/Module02_Sequence`, cinco prefabs propios de cable y sus ObjectSpawner.
Conserva los cables originales desactivados como plantillas. No modifica el prefab
base compartido. Genera un botón «Marcar como revisado» en la tarjeta y un Canvas
`SequenceStatus`; ajustar su posición y orientación, revisar las listas de fichas
del coordinador y guardar la escena. Los assets generados se guardan al ejecutar
el menú; la escena queda pendiente de guardar. El Undo de escena no elimina los assets.

Si ya existe un coordinador, el menú no duplica la configuración. No volver a
ejecutar configuradores anteriores sin revisar sus efectos sobre esta escena integrada.

## Objetivos

1. Inspeccionar el rack y sus elementos estáticos.
2. Inspeccionar el switch.
3. Montar el switch.
4. Asegurar sus dos pestañas.
5. Conectar cinco enlaces: alimentación y cuatro Ethernet.
6. Etiquetar ambos extremos: diez etiquetas.
7. Conectar la consola.
8. Encender el switch.
9. Configurar el switch mediante el puzzle existente.

`Module02ObjectiveFactory` crea los objetivos a partir de ObjectiveData.
`Module02FlowController` conserva el avance secuencial y consulta una condición
de completitud del coordinador antes de aceptar TryCompleteCurrent. La factory
crea el objetivo genérico existente; las condiciones físicas viven en
`Module02SequenceCoordinator`, no en nueve clases de objetivo nuevas.

## Herramientas e interacciones

Las tres herramientas permanecen disponibles: no se modifica availableTools.
Como en el módulo 1, se comprueba la acción antes de modificar el estado.
El agarre del switch se habilita durante su inspección (segundo objetivo) para poder
moverlo y girarlo mientras se revisan sus fichas. El montaje se habilita en el tercer objetivo; atornillado en el
cuarto, cableado en el quinto, etiquetado en el sexto, consola en el séptimo y
encendido en el octavo. Configuración requiere el noveno y las condiciones previas
que ya valida la consola.

Se permite corregir cableado y etiquetas hasta el encendido, aunque esos objetivos
ya estén completados. El botón de encendido vuelve a consultar el estado real de
las cinco conexiones y diez etiquetas. Si no corresponde, muestra:
«Realice el cableado y etiquetado correctamente antes de continuar».
El encendido correcto deshabilita el agarre de los diez extremos de instalación,
sin desactivar sus Connector. La consola conserva su cable desmontable e integrado.
Los LED de enlace y paquetes siguen dependiendo de aplicar la configuración.

## Registro de instancias

Al completar la fijación, `SpawnAndRegister` ejecuta una sola vez:

1. `ObjectSpawner.Spawn()` para cada cable y consulta `CurrentInstance`.
2. Obtiene su `PatchCableLink` y `CableLabelPair`, verificando cinco cables distintos.
3. Entrega esas referencias mediante `SetCables` a `Module02CablingState`,
   `Module02LabelingState` y `Module02SwitchConfiguration`.

Son referencias a los objetos recién creados en Play, no al prefab ni a la plantilla
inactiva. No se copia progreso ni se buscan cables por nombre. Los monitores empiezan
vacíos y después observan las conexiones y etiquetas de esas mismas instancias.
No ejecutar Respawn manual de un spawner durante la práctica: el registro corresponde
a la generación inicial. Para reiniciar esta versión, reiniciar Play o recargar la escena.

## Fichas y depuración

Se cuentan IDs de ficha únicos por grupo. Varios colliders de una misma ficha no
incrementan el total. Se deben visitar todas sus páginas y pulsar «Marcar como revisado».
Esto verifica navegación y confirmación, no comprensión o tiempo de lectura.
Al cambiar de grupo se cierran las tarjetas anteriores y se deshabilitan sus targets.

En `Module02_Sequence > Module02SequenceCoordinator`, activar **Skip Rack Inspection**
antes de Play para iniciar directamente en el objetivo 2. Internamente completa los
IDs requeridos del primer grupo y usa el avance normal. Solo funciona en Unity Editor
o Development Build; no omite la inspección del switch ni los objetivos siguientes.
Mantener el coordinador activo durante la sesión; desactivarlo bloquea las acciones
integradas. Para reiniciar sus suscripciones y progreso, recargar la escena.

## Comprobación manual pendiente

- Inicio normal: contador de fichas sin duplicados, confirmación tras visitar páginas,
  transición al switch y habilitación posterior de la zona de instalación.
- Inicio con Skip Rack Inspection: objetivo 2 activo inmediatamente.
- Herramientas disponibles desde el inicio, sin poder realizar acciones adelantadas.
- Dos pestañas completas: aparecen cinco cables una sola vez, con referencias nuevas
  en los tres monitores; los cables originales permanecen inactivos.
- Desconectar tras etiquetar invalida etiquetas; se puede reparar antes de encender.
- Cableado o etiquetas incompletos impiden encender y muestran el mensaje solicitado.
- Encendido correcto bloquea agarre de cables de instalación; consola aún desmontable.
- Puzzle aplicado: LED y tráfico Ethernet, práctica completada. Reiniciar Play limpia progreso.

## Validación y alcance

Compilación de runtime y Editor con Roslyn de Unity satisfactoria (advertencias TMP
preexistentes). Pasan 598 aserciones de reglas de cableado, alimentación, puzzle y
secuencia en Tests/Module02Wiring. No equivalen a pruebas de integración XR.
El menú y la interacción Play/VR no se ejecutaron desde esta sesión.

`PracticalCompleted` deja el punto de conexión para el siguiente sprint.
NPC, guion de diálogos, quiz con prefab compatible, resultados, insignia y guardado
local todavía no se implementan aquí.
