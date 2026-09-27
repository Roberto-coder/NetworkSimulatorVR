# Módulo 3 — Sprint 2: escenario y rutas fijas

## Estado

Configurador de Editor, catálogo, layout, asociación de dispositivos/puertos, rutas Splines y malla tubular implementados. Código nuevo compilado con las referencias de Unity 6000.2.7f2 y Splines 2.8.4. Pasan las 54 comprobaciones del dominio del sprint 1.

Pendiente: ejecutar el configurador y comprobar colocación/Undo/guardado en Unity. No se han modificado las escenas del usuario ni el demo de Splines. La prueba del sprint 1 en el Editor fue confirmada por el usuario.

## Preparación y creación

1. Abrir `Assets/Scenes/Modulo3.unity` fuera de Play.
2. Abrir **Network Simulator > Module 03 > Sprint 2 - Escenario y rutas**.
3. Pulsar **Crear assets y prefabs provisionales faltantes**. Crea `OfficeLayout`, `OfficePrefabCatalog`, material de cable y prefabs geométricos simples. No sustituye assets existentes. Esta operación escribe assets; no forma parte del Undo de escena.
4. En el catálogo, reemplazar los prefabs provisionales por modelos definitivos si se desea. En el layout, ajustar posiciones de dispositivos. La distribución inicial es una cuadrícula de autoría, no una oficina terminada.
5. Para aprovechar modelos colocados en la oficina, arrastrar sus raíces a los campos de dispositivo de la ventana. Deben pertenecer a la escena activa, ser distintos y no estar anidados entre sí. Quedarán asociados dentro de `Module03_Network`, conservando posición mundial.
6. Pulsar **Crear / completar infraestructura (Undo)**. Los campos vacíos usan el catálogo. Se crean solamente dispositivos, anclajes y rutas faltantes. Repetir no debe duplicarlos ni reemplazar lo editado. Una asociación contradictoria se rechaza.

La escena no se guarda automáticamente. La operación de construcción admite Undo y revierte los cambios de escena si falla. Los modelos asociados no pueden tener ya otro `NetworkDeviceBinding`.

## Ajuste de los puertos y cables

- Cada `NetworkDeviceBinding` representa un ID del estado inicial. Debajo se crean `NetworkPortAnchor` con los IDs completos de puerto. Mover estos anclajes hasta los conectores visuales del modelo; sus posiciones iniciales solo sirven de referencia.
- `SW-01/mgmt` es la interfaz lógica de gestión: no requiere cable físico.
- Se generan **7 rutas fijas**: laptop–switch, 3 switch–patch panel y 3 patch panel–roseta. Cada objeto `Fixed_*` contiene `SplineContainer`, `FixedNetworkCable`, `SplineExtrude` y un LineRenderer de respaldo desactivado.
- Seleccionar una ruta y editar sus knots con las herramientas habituales de Splines. Añadir puntos por canaletas/paredes; no mover/escalar la raíz `Fixed_*`. Mantenerla con transform local identidad bajo la raíz de red.
- En `FixedNetworkCable`, pulsar **Ajustar extremos a puertos y reconstruir** después de mover un dispositivo/anclaje. Conserva los puntos intermedios y las tangentes.
- Después de editar la curva, pulsar **Reconstruir visual desde spline** para actualizar malla y tabla de distancias. La regeneración automática por modificación está desactivada; no se recalcula la geometría cada frame.
- Ajustar el radio en Spline Extrude para una ruta existente. `layout.cableRadius` y `routeSamples` aportan valores iniciales a rutas nuevas. El material compartido sí permite cambiar el color globalmente.

La herramienta no calcula caminos alrededor de paredes. El diseñador decide el recorrido; Unity genera la geometría. No hay colisiones ni agarre en los cables fijos. Los patch cords de PC–roseta se incorporan físicamente en el sprint 6; por ahora solo existen en el modelo de datos.

## Validación y guardado

1. Pulsar **Validar infraestructura**. Debe informar IDs, anclajes y rutas correctos.
2. Comprobar visualmente que los 21 anclajes coinciden con los puertos previstos y que los cables siguen las canaletas; el validador comprueba identidad y extremos, no reconoce muebles ni paredes.
3. Pulsar **Guardar posiciones y rutas en layout**. Guarda posiciones/rotaciones/escalas de dispositivos, posiciones de puertos relativas al dispositivo y knots completos de rutas (incluidas tangentes). Conserva las posiciones 2D independientes del mapa futuro.
4. Guardar la escena normalmente.
5. Entrar en Play. `Module03NetworkScene` valida los enlaces de escena y crea una sesión aislada del estado inicial. Todavía no ejecuta ping ni genera tráfico.

El layout guarda datos de reconstrucción, no referencias a objetos de escena. Para reconstruir con idéntica apariencia se debe usar el mismo catálogo de modelos. Guardar posiciones de un modelo asociado no incorpora automáticamente ese modelo al catálogo: asignar su prefab correspondiente allí.

Mantener escala mundial (1,1,1) en `Module03_Network`. Puede trasladarse o rotarse la raíz. Las distancias de ruta se expresan en sus unidades locales. Para una nueva PC, ampliar estado inicial, layout, puertos y catálogo si necesita una clave nueva, y volver a **Crear / completar**.

## Pruebas de aceptación en Unity

- Se conservan entorno, XR rig y demo existentes; no se agrega cámara nueva.
- 9 dispositivos, 21 anclajes y 7 rutas fijas con IDs únicos en la raíz.
- Repetir creación no duplica objetos ni recoloca los existentes.
- Undo revierte la construcción y devuelve los modelos asociados a sus padres anteriores.
- Mover un anclaje provoca error de desalineación; ajustar extremos y reconstruir lo resuelve.
- Editar puntos intermedios cambia la malla sin alterar el ID de cable ni el modelo lógico.
- Guardar el layout y recrear en una escena de prueba con el mismo catálogo conserva rutas y anclajes.
- Entrar/salir de Play no modifica los assets ni introduce rigidbodies en rutas fijas.

## Archivos

- `Assets/GameData/Module03/Definitions/NetworkLayoutAsset.cs` y `NetworkPrefabCatalog.cs`.
- `Assets/Scripts/Modules/Module03_Diagnostics/Interaction/NetworkDeviceBinding.cs` y `NetworkPortAnchor.cs`.
- `Assets/Scripts/Modules/Module03_Diagnostics/Module03NetworkScene.cs`.
- `Assets/Scripts/Modules/Module03_Diagnostics/Presentation/FixedNetworkCable.cs`.
- `Assets/Editor/Modules/Module03_Diagnostics/Module03SceneSetup.cs` y `FixedNetworkCableEditor.cs`.

Los nuevos componentes tienen comentarios sobre límites de responsabilidad, coordenadas, reconstrucción y conservación de objetos. No se han conectado aún al `NetworkPort` físico del módulo 2: esa adaptación corresponde a los patch cords interactuables.


## Scripts añadidos y responsabilidades

Incluye ampliaciones posteriores del sprint; los archivos reutilizados o modificados se indican expresamente.

| Script | Objetivo o función |
| --- | --- |
| `NetworkLayoutAsset.cs` | Guarda colocación, anclajes y rutas spline. |
| `NetworkPrefabCatalog.cs` | Relaciona claves lógicas con prefabs. |
| `NetworkDeviceBinding.cs` | Asocia un GameObject con el ID del dispositivo. |
| `NetworkPortAnchor.cs` | Asocia un transform con el ID del puerto. |
| `Module03NetworkScene.cs` | Valida asociaciones e inicia sesión y simulador. |
| `FixedNetworkCable.cs` | Muestrea el spline y permite recorrerlo por distancia. |
| `FixedNetworkCableEditor.cs` | Herramientas de autoría de rutas. |
| `Module03SceneSetup.cs` | Prepara escenario, assets y enlaces desde el Editor. |
