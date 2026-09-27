# Módulo 3 — Sprint 4: laptop fija y Canvas de escena

## Diseño vigente

La laptop permanece fija en la oficina. No aparece en la rueda, no se equipa y no se crea un prefab de herramienta. El configurador deshabilita XRGrabInteractable y fija los Rigidbody de la laptop. La rueda conserva sus otras herramientas.

Solo las PCs y la laptop tienen Canvas world-space guardados en la escena, editables antes de Play (cuatro en la oficina actual). Switch, patch panel y rosetas permanecen como nodos del mapa, sin pantalla local ni interacción A. A solo muestra/oculta ese Canvas: no lo instancia, mueve, rota, escala ni orienta hacia la cámara. Se eliminó Recentrar. Los indicadores de A también son objetos preexistentes y fijos.

En PC se consulta la configuración local; en laptop se abre el mapa. Elegir un nodo del mapa selecciona destino sin cambiar contexto local. Todos los pings parten de la laptop. El switch se consulta desde esas pantallas y requiere conectividad de gestión desde la laptop; los elementos pasivos solo aparecen en el mapa.

## Preparar la escena

1. Abrir Modulo3 fuera de Play, con infraestructura validada.
2. Abrir **Network Simulator > Module 03 > Sprint 4 - Pantalla y botón A**.
3. Asignar red, cámara XR y origen del rayo derecho. El canvas de rueda es opcional y solo bloquea aperturas simultáneas; no se solicita ToolManager.
4. Pulsar **Crear Canvas fijos y zonas en escena (Undo)**. Es una herramienta de autoría exclusiva del Editor: crea y conecta las jerarquías una vez, no se ejecuta en el juego. No guarda automáticamente la escena.
5. Bajo Diagnostic workspace aparecerá Canvas_Laptop, Canvas_PC-01, etc., cada uno con mapa, resultados desplazables y seis botones.
6. Activar temporalmente los Canvas en Inspector para editar posición, orientación, escala, fuentes, colores y tamaños. Colocarlos donde deben permanecer, visibles desde la zona y sin atravesar paredes. Al iniciar Play se ocultan hasta pulsar A.
7. Ajustar Diagnostic hit zone y el indicador fijo de A de cada dispositivo. DiagnosticInteractionTarget.screen debe apuntar a su vista exclusiva; focusHint y hitZone tienen referencias independientes.
8. Guardar la escena. Gatillo opera botones XRI; A abre/cierra; E sirve en Editor/desarrollo. B y la rueda conservan sus bindings.

También se puede diseñar el Canvas manualmente: añadir DiagnosticScreenView, asignar título, salida, contenido desplazable, seis botones en orden (Ping, ARP, Configuración local, Puertos del switch, Bitácora, Cerrar) y nodos con ID, botón y texto. Referenciarlo desde el target. No hace falta ejecutar el builder si la jerarquía y referencias ya están preparadas.

El controlador conecta callbacks en ejecución; no añadir eventos manuales duplicados de comandos/nodos. Los eventos independientes de sonido/efectos pueden conservarse.

## Datos y mantenimiento

DiagnosticUiSettings vive en Assets/GameData/Module03/Presentation. Contiene origen de ping, alcance, textos y escala inicial de autoría. El binding A pertenece a VRInputManager (Interact Binding / Interact Button). La escala del asset no recoloca Canvas existentes en Play: editar sus transforms en escena.

El mapa se prepara desde topología y layout durante la autoría. Sus nodos/enlaces quedan editables. Si se agregan dispositivos después, actualizar representaciones y referencias en Editor; no se generan nodos al iniciar.

Se retiraron los scripts de acceso a laptop y registro en la rueda. No se modifica ToolManager. Si ya se hubiese creado un asset UI con siete comandos en una prueba anterior, ajustar su lista a los seis comandos vigentes, sin Recentrar.

## Entrada y consultas

Se necesita un único EventSystem con XRUIInputModule y el rayo configurado para UI. Se crea EventSystem solo si no existe; uno incompatible provoca un aviso.

Interaction Mask incluye zonas y obstáculos: el primer impacto bloquea interacción a través de paredes. Modal Blockers admite paneles de pausa/tutorial. También se cierra al pausar con timeScale=0, abrir la rueda configurada o abandonar el alcance. Cerrar conserva la sesión.

Las observaciones comienzan sin comprobar y quedan obsoletas al cambiar la revisión. Configuración local funciona en una PC aislada. No hay escritura de IP ni habilitación desde esta UI todavía. Bitácora muestra las últimas 30 acciones; ARP, vecinos aprendidos. Los enlaces grises representan la topología documentada sin revelar la falla inicial.

## Pruebas en Unity pendientes

- Antes de Play, todos los Canvas y botones existen en Hierarchy y pueden editarse.
- Abrir/cerrar con A desde distintas posiciones no cambia el transform del Canvas.
- No aparecen nuevos Canvas, nodos, indicadores ni herramientas al entrar en Play.
- Laptop no se agarra ni aparece como nueva opción en la rueda.
- A sobre PC abre su Canvas; seleccionar otra PC no cambia la consulta local.
- A sobre Laptop abre mapa; ping a PC-01 falla mientras el cable esté roto.
- Probar rayos, botones y arrastre de resultados en el visor.
- Alejarse o abrir un modal oculta la vista conservando la sesión.
- Guardar y reabrir la escena conserva referencias y posiciones.

## Verificación realizada

Compilación de runtime y Editor con referencias de Unity/XRI/TMP correcta. Las **109 comprobaciones de dominio** siguen pasando. Pendiente aplicar el configurador y probar en Unity; no se modificó la escena del usuario.

DiagnosticScreenController gestiona entrada, contexto y visibilidad. DiagnosticScreenView guarda referencias a UI. DiagnosticCanvasBuilder, ubicado exclusivamente bajo Assets/Editor, ayuda a construirla antes de Play. El dominio DiagnosticWorkspace no cambia.

## Corrección de escenas creadas con la versión anterior

En la misma ventana del sprint 4, asignar la red y pulsar **Desactivar pantallas sobrantes de infraestructura (Undo)**. Oculta los Canvas e indicadores del switch, patch panel y rosetas, y deshabilita sus targets de interacción. No elimina objetos ni altera los Canvas editados de las PCs/laptop. Guardar la escena. También se excluyen esas interacciones al iniciar Play aunque aún no se haya aplicado la limpieza.

Las nuevas construcciones generan exclusivamente Canvas para tipos Computer y DiagnosticStation; añadir PCs conserva la escalabilidad sin crear pantallas para infraestructura.

## Corrección de entrada y bloqueo por rueda

DiagnosticScreenController utiliza VRInputManager.InteractPressed: A en OpenXR y Meta OVR; E en Editor/desarrollo con OpenXR. B conserva Confirm. No hay una segunda InputAction de diagnóstico.

Radial Canvas puede referenciar RadialSelection o su Canvas hijo. Si hay RadialMenuController, se consulta IsOpen: el contenedor activo no implica un menú abierto. Esto corrige el bloqueo permanente observado en Modulo3.

La rueda requiere herramientas. Sin ModuleDefinition se puede configurar ToolManager > Available Tools Override, usando las herramientas autorizadas para el módulo. Una entrada con nombre Mano vacía y prefab nulo permite probar abrir/seleccionar/cerrar antes de integrar tester y etiquetadora. La laptop no se agrega. Una lista vacía muestra un aviso al intentar abrir la rueda.

Prueba de regresión en visor: con la rueda cerrada, apuntar a PC-02 dentro de 2.5 m debe mostrar el hint; A abre y vuelve a cerrar su Canvas fijo. Configurar una entrada de prueba en Available Tools Override, mantener Y para abrir la rueda y soltar para cerrarla; el diagnóstico debe bloquearse sólo mientras está abierta. Repetir con laptop y verificar que B no abre diagnóstico. No hace falta agregar un collider al jugador.

## Prefab compartido y mapa con sprites

En Network Simulator > Module 03 > Sprint 4 - Pantalla y botón A, asignar Red y Datos UI y pulsar **Migrar Canvas existentes a prefab compartido (Undo)** fuera de Play. La primera ejecución crea `Assets/Prefabs/Dispositivos/Modulo3/DiagnosticScreen.prefab`; las siguientes reutilizan el prefab sin sobrescribir las ediciones de Prefab Mode.

Se migran sólo PCs y laptop. Cada instancia mantiene posición, rotación y escala de la pantalla previa, y recibe la cámara del controlador. Los Canvas anteriores quedan desactivados con sufijo `(respaldo)`; Undo recupera referencias y objetos. Guardar la escena después de revisar. Los cambios al asset prefab no se revierten con el Undo de escena.

El prefab contiene iconos desde `Assets/Recursos/Modulo3/UI`, nombre debajo, estado en un texto independiente y enlaces abstractos dibujados detrás. Varios cables entre el mismo par de dispositivos se representan con una línea. Los resultados están debajo del mapa y los seis comandos al pie. Las posiciones del mapa y sprites iniciales se guardan en `DiagnosticUiSettings.mapNodes`, dentro de Assets/GameData. Se usan al crear el prefab; después, editar su diseño en Prefab Mode para propagarlo a todas las instancias. Cambiar esos datos no reconstruye el prefab existente automáticamente.

No se instancia ni mueve UI en Play. Un prefab evita duplicar la autoría del diseño, pero siguen existiendo cuatro instancias fijas independientes. La migración no elimina zonas, hints ni cables físicos. Al ampliar la topología, añadir los nodos y sus bindings al prefab; los Canvas no se regeneran durante ejecución.

Verificación: compilación del código runtime/Editor y 109 comprobaciones de dominio aprobadas. Pendiente ejecutar la migración en Unity y verificar distribución, lectura y selección con el visor. Comprobar que editar un icono en Prefab Mode afecta las cuatro instancias y que Undo restaura las referencias anteriores.


## Scripts añadidos y responsabilidades

Incluye ampliaciones posteriores del sprint; los archivos reutilizados o modificados se indican expresamente.

| Script | Objetivo o función |
| --- | --- |
| `DiagnosticWorkspace.cs` | Separa contexto local y destino; organiza consultas y observaciones. |
| `DiagnosticUiSettings.cs` | Guarda textos, alcance y nodos gráficos en GameData. |
| `DiagnosticInteractionTarget.cs` | Asocia dispositivo, zona, hint y pantalla fija. |
| `DiagnosticScreenView.cs` | Referencias de Canvas, botones, nodos, estados y contornos. |
| `DiagnosticScreenController.cs` | Detecta apuntado/A, ejecuta consultas y actualiza selección. |
| `DiagnosticCanvasBuilder.cs` | Construye UI únicamente durante autoría. |
| `DiagnosticScreenPrefabSetup.cs` | Crea prefab compartido y migra pantallas con respaldo. |
| `Module03DiagnosticUiSetup.cs` | Ventana de configuración y migración del sprint 4. |
| `VRInputManager.cs (modificado)` | Centraliza A en OpenXR/OVR. |
| `RadialMenuController.cs (modificado)` | Expone visibilidad real para bloquear la pantalla. |
