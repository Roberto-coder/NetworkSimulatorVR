# Módulo 2 — Sprint 4: etiquetado

## Aplicación

En Modulo2, fuera de Play, ejecuta **Network Simulator > Module 02 > Infraestructura > Sprint 4 - Etiquetado**. Requiere los cinco cables activos de los sprints anteriores.

El configurador añade CableLabelPair a cada cable y CableEndpointLabel a Start y End, con sus respectivos LabelVisual ocultos. Registra el prefab provisional LabelMaker_Module02 en el segundo espacio de la rueda y crea Labeling_Sprint04. Conserva las etiquetas existentes al repetir el menú. Guarda la escena después de aplicarlo; la definición de herramientas y el prefab se guardan desde el configurador.

Para ajustar visuales, activa temporalmente LabelVisual en edición y modifica posición, rotación y tamaño. Las etiquetas son Canvas sin colliders ni raycaster, anclados a los extremos rígidos y separados de los segmentos flexibles. En Play comienzan ocultas. La herramienta tiene geometría provisional sustituible y un Transform Nozzle editable.

## Interacción

1. Conecta los dos extremos del cable.
2. Selecciona Etiquetadora en la rueda.
3. Acerca la boquilla al conector, dentro de 4.5 cm, y pulsa el trigger derecho.
4. Se muestra una etiqueta en ese extremo con los IDs reales de ambos puertos. Repite en el otro extremo.

Una pulsación solo afecta al extremo detectado más cercano. Repetir sobre un extremo ya etiquetado no crea objetos adicionales. El texto de la herramienta confirma el resultado o indica que falta acercarse/conectar ambos extremos.

LabelMakerTool usa Input System, con binding por defecto `<XRController>{RightHand}/triggerPressed`. Trigger Action permite asignar otra acción existente del proyecto. Solo escucha mientras está equipada/activa. En Play también puede probarse la impresión con el menú contextual Print nearest label del componente.

## Validaciones y estado

- CableEndpointLabel exige un propietario activo y un enlace completo compatible antes de mostrar la etiqueta.
- CableLabelPair construye texto desde los IDs actuales. Desconectar, cambiar un puerto/ID, desactivar el cable o perder la compatibilidad invalida ambas etiquetas. Reconectar requiere volver a etiquetar, incluso si se usan los mismos puertos.
- Los datos no se resuelven buscando al padre al imprimir: se conserva una referencia explícita al cable, porque XRI separa el extremo de la jerarquía al agarrarlo.
- Se puede etiquetar un enlace Ethernet equivocado: la etiqueta describe la conexión real. Eso no lo convierte en correcto; Module02CablingState sigue validando la tabla fija.
- Module02LabelingState expone LabeledEndCount (0–10), FullyLabeledCableCount (0–5), IsComplete y StateChanged. Los contadores también se muestran como campos en el Inspector durante Play.
- IsComplete indica diez etiquetas vigentes en cinco cables; la integración final deberá combinarlo con el estado del cableado correcto.
- No se exigen montaje, atornillado ni encendido para etiquetar. Tampoco se completa un objetivo automáticamente.

## Verificación

Runtime y editor compilados sin errores con Roslyn y referencias locales de Unity. Pendiente aplicar el menú y probar en VR:

- Cable con un solo extremo conectado: impresión rechazada.
- Cable completo: una etiqueta por pulsación, texto correcto, ambos extremos independientes.
- Pulsar repetidamente: el contador no sube de dos por cable.
- Desconectar cualquiera de los extremos: ambas etiquetas se ocultan y el contador baja.
- Reconectar/cambiar destino: nueva impresión con IDs actuales, sin recuperar etiquetas anteriores.
- Guardar y volver a equipar la etiquetadora: el trigger solo actúa con la herramienta activa.
- Confirmar alcance, orientación y legibilidad, sin tapar los puertos ni bloquear el agarre.
- Etiquetar diez extremos y comprobar los contadores de Labeling_Sprint04.

El configurador y las pruebas físicas no se han ejecutado en esta sesión.
