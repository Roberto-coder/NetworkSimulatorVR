# Módulo 2: añadir páginas a los Info Targets

La paginación ya existe. El componente `RackInfoTarget` señala un asset `RackComponentInfo`; su lista **Sections** contiene las páginas adicionales. No necesitas programar ni duplicar el Canvas para añadir contenido.

## 1. Localizar la ficha correcta

1. Abre `Assets/Scenes/Modulo2.unity` y sal de Play Mode antes de editar.
2. Selecciona el dispositivo en Hierarchy y busca su hijo `InfoTargets`. Selecciona el objeto que tenga el componente **Rack Info Target**. También puedes buscar `t:RackInfoTarget` en Hierarchy.
3. En el componente, revisa **Information**. Haz clic en el asset asignado para localizarlo en Project y selecciónalo para editar su Inspector.
4. Si **Parent Target** tiene una referencia, sigue ese target: la tarjeta usa la información del target padre. Si hay varios niveles, sigue las referencias hasta el último. No crees referencias circulares.

La referencia efectiva de la escena es la fuente de verdad: una instancia puede tener una asignación distinta de su prefab. Si varios targets usan el mismo asset, editarlo cambia la información de todos ellos.

## 2. Añadir páginas desde el Inspector

1. Selecciona el asset `RackComponentInfo`.
2. Busca **Additional pages (optional; page 1 is the overview)** y despliega **Sections**.
3. Pulsa **+** para añadir un elemento, o aumenta **Size** si tu Inspector muestra ese control. Para pasar de una página general a cuatro páginas totales necesitas tres elementos.
4. Despliega el elemento y completa:

| Campo | Contenido |
| --- | --- |
| **Title** | Encabezado de la página, por ejemplo `Conexiones del router`. |
| **Body** | Texto explicativo de esa página. |
| **Image** | Sprite opcional de esa página. |
| **Reference** | Referencia opcional de esa página. |

5. Repite para las páginas que necesites. Revisa todos los campos de los elementos nuevos por si el Inspector ha copiado valores del anterior.
6. Ordena los elementos en la lista en el orden de lectura. Guarda los cambios con **File > Save Project**. Si cambiaste referencias en la escena, guarda también la escena.

**Total de páginas = 1 + cantidad de elementos en Sections.**

| Página visible | Origen |
| --- | --- |
| 1 | Información general del asset. |
| 2 | `Sections > Element 0`. |
| 3 | `Sections > Element 1`. |
| 4 | `Sections > Element 2`. |

La primera página utiliza **Short Description**, **Function**, **Image** y **Standard Reference** del asset. Las siguientes utilizan los campos de su sección. **Display Name**, categoría, altura y aviso de modelo educativo se comparten entre páginas. La imagen general no se reutiliza automáticamente en las secciones: asígnala en cada una donde la necesites.

## 3. Qué asset editar para cada dispositivo

Los assets existentes están en `Assets/GameData/Module02_Devices/Components/`. Confirma siempre **Information** siguiendo el paso 1 antes de editar. Los temas de esta tabla son sugerencias para páginas nuevas, no contenido ya configurado.

| Dispositivo o zona | Asset | Temas sugeridos para Sections |
| --- | --- | --- |
| Rack | `rack_22u.asset` | Unidades U; distribución de equipos; organización. |
| Switch, ficha general | `switch_overview.asset` | Partes; función en la red; montaje. |
| Puertos del switch | `switch_ports.asset` | Identificación de puertos; conexiones; revisión del enlace. |
| Indicadores del switch | `switch_leds.asset` | Lectura de indicadores; estados representados por el simulador. |
| Interruptor del switch | `switch_power_button.asset` | Encendido; comprobaciones. |
| Entrada de alimentación del switch | `switch_power_input.asset` | Identificación; conexión de alimentación. |
| Router | `router.asset` | Interfaces; conexión con otras redes; ubicación. |
| Servidor | `server.asset` | Servicios; conexiones; montaje. |
| Firewall | `firewall.asset` | Filtrado; interfaces; ubicación en la red. |
| Patch panel | `patch_panel.asset` | Numeración; correspondencia de puertos; etiquetado. |
| UPS / PDU | `ups_pdu.asset` | Diferencia entre respaldo y distribución; conexiones. |
| Refrigeración | `cooling.asset` | Flujo de aire; ventilación; distribución. |
| Fibra óptica | `fiber_optics.asset` | Conectores representados; uso; cuidado del cable. |
| Cableado estructurado | `structured_cabling.asset` | Organización; identificación; recorrido. |

El switch tiene varias fichas: añadir páginas en `switch_overview.asset` no las añade a `switch_ports.asset` ni a sus otras zonas.

## 4. Ejemplo: añadir dos páginas al router

Selecciona `router.asset` y añade dos elementos a **Sections**:

```text
Element 0
  Title: Conexiones del router
  Body: Identifica las interfaces del modelo y revisa qué cable está
        conectado a cada una antes de continuar con la práctica.
  Image: [sprite opcional]
  Reference: [opcional]

Element 1
  Title: Revisión de la instalación
  Body: Comprueba que el equipo esté ubicado correctamente y que
        puedas identificar el recorrido de sus conexiones.
  Image: [sprite opcional]
  Reference: [opcional]
```

La tarjeta tendrá tres páginas: información general, conexiones y revisión. Puedes repetir este procedimiento en cada asset de la tabla.

## 5. Imágenes y extensión del texto

Para usar una imagen, selecciónala en Project, configura **Texture Type = Sprite (2D and UI)** y pulsa **Apply**. Arrastra el Sprite al campo **Image** de la sección.

Usa párrafos cortos y reparte explicaciones largas en varias páginas. La vista actual da al texto de las secciones un alto de 255 unidades de UI; el generador de tarjetas configura truncamiento con puntos suspensivos. Añadir mucho texto no genera páginas automáticamente. Comprueba la legibilidad en VR.

## 6. Comprobar el resultado

1. Entra en Play Mode y accede a la exploración del módulo 2.
2. Apunta al target hasta que se abra su tarjeta.
3. Pulsa **Fijar** para mantenerla abierta durante la revisión.
4. Comprueba el contador: dos secciones deben mostrar inicialmente `1 / 3`.
5. Usa **Siguiente** y **Anterior**. Verifica texto, imágenes, referencias y orden. Anterior queda desactivado en la primera página y Siguiente en la última.
6. Cierra la tarjeta y prueba los demás dispositivos, incluyendo las distintas zonas del switch.

## 7. Problemas frecuentes

| Problema | Qué revisar |
| --- | --- |
| Sigue mostrando `1 / 1` | Confirma el asset efectivo en Information y Parent Target; comprueba que Sections tenga elementos. Cierra y vuelve a abrir la tarjeta si editaste mientras estaba visible. |
| Aparece una página vacía | Revisa Title y Body del elemento correspondiente. Los elementos vacíos también cuentan como páginas. |
| Cambia otro dispositivo | Los dos targets pueden compartir asset o Parent Target. Duplica el asset si necesitas contenido independiente y asigna la copia al target correspondiente. |
| No aparece la imagen | Asigna un Sprite en Image de esa sección; la imagen de la página general no se hereda. |
| Se corta el texto | Divide el contenido entre más elementos de Sections. |
| No aparecen controles de navegación | Revisa Next Button, Previous Button y Page Label de InfoCardView, y la referencia View de InfoCardController. |

Si la escena todavía utiliza una tarjeta antigua sin controles, existe el menú **Network Simulator > Module 02 > Sprint 2 - Actualizar tarjetas**. Úsalo únicamente si necesitas reconstruir esos controles: modifica el diseño de la tarjeta y guarda la escena. No hace falta ejecutarlo para añadir páginas a una tarjeta que ya tiene navegación.

## Archivos que implementan este comportamiento

- `Assets/GameData/Module02/Definitions/RackComponentInfo.cs`: información general y lista Sections.
- `Assets/GameData/Module02/Definitions/RackInfoSection.cs`: campos de cada página adicional.
- `Assets/Scripts/Modules/Module02_RackInstallation/Interaction/Inspection/RackInfoTarget.cs`: selección de información y resolución de Parent Target.
- `Assets/Scripts/Modules/Module02_RackInstallation/Presentation/Inspection/InfoCardView.cs`: contenido, contador e imágenes.
- `Assets/Scripts/Modules/Module02_RackInstallation/Presentation/Inspection/InfoCardController.cs`: navegación entre páginas.

Guía verificada contra los scripts y assets del proyecto. La comprobación visual en Play Mode debe realizarse en Unity.
