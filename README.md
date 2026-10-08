# NetXR Lab · NetworkSimulatorVR

### Aprende redes construyéndolas en realidad virtual.

Proyecto académico desarrollado en **Unity para Meta Quest**, donde el usuario asume el papel de un ingeniero de redes: prepara cables, instala equipos y diagnostica fallas dentro de un laboratorio virtual.

**Unity 6.2 · C# · OpenXR · XR Interaction Toolkit · Meta XR · Firebase**

> 🚧 Proyecto en desarrollo. Las escenas y los sistemas se encuentran en integración y validación en visor.

<!-- IMAGEN 01 — PORTADA
Guardar en Docs/Imagenes/README/01-portada-lobby.png.
Captura horizontal del Lobby desde la vista del jugador: entorno, instructor y panel de módulos.
Cuando exista el archivo, reemplazar el aviso siguiente por:
![Lobby de NetXR Lab con el instructor virtual y el acceso a los módulos](Docs/Imagenes/README/01-portada-lobby.png)
-->
> 📷 **Portada:** vista del Lobby desde el jugador, con el instructor y el acceso a los módulos.

## 🧭 Del concepto a la práctica

NetXR Lab propone un espacio para practicar tareas de redes mediante interacción con dispositivos, herramientas y conexiones. El recorrido aborda conceptos de las primeras tres capas del modelo OSI: **física, enlace de datos y red**.

La experiencia combina instrucciones de un NPC, objetivos por módulo, retroalimentación durante las actividades y evaluaciones de cierre. La simulación de red está orientada al aprendizaje; no pretende reproducir todas las funciones de un equipo comercial.

## 🛠️ Un recorrido en tres módulos

| Etapa | ¿Qué hace el usuario? | Conceptos que practica |
|---|---|---|
| **Lobby · Familiarización** | Aprende los controles, conoce al instructor y explora el museo de dispositivos. | Interacción VR y reconocimiento de equipos. |
| **Módulo 1 · Fabricación de cable** | Prepara un cable de red, organiza sus conductores, realiza el ponchado y verifica el resultado. | Cableado UTP, terminaciones y continuidad. |
| **Módulo 2 · Instalación en rack** | Inspecciona componentes, monta y fija un switch, conecta y etiqueta enlaces y realiza una configuración guiada. | Instalación física, organización del cableado y configuración básica. |
| **Módulo 3 · Diagnóstico y reparación** | Consulta la topología, ejecuta pruebas desde una laptop y corrige incidentes de cableado, puertos y direccionamiento. | Conectividad, ARP, ICMP y diagnóstico de fallas. |

<!-- IMAGEN 02 — MÓDULO 1
Guardar en Docs/Imagenes/README/02-fabricacion-cable.png.
Primer plano de las manos, el cable y la herramienta durante una acción; evitar una mesa vacía.
![Preparación de un cable de red con herramientas interactivas](Docs/Imagenes/README/02-fabricacion-cable.png)
-->
> 📷 **Módulo 1:** primer plano de la preparación o el ponchado del cable.

<!-- IMAGEN 03 — MÓDULO 2
Guardar en Docs/Imagenes/README/03-instalacion-rack.png.
Mostrar el switch instalado, varios cables conectados y al menos una etiqueta legible.
![Switch instalado en rack con conexiones y etiquetas](Docs/Imagenes/README/03-instalacion-rack.png)
-->
> 📷 **Módulo 2:** rack con el switch instalado, cables conectados y etiquetas visibles.

<!-- IMAGEN 04 — MÓDULO 3
Guardar en Docs/Imagenes/README/04-diagnostico-red.png.
Mostrar la laptop con una prueba de conectividad y parte de la red o su visualización de paquetes.
![Diagnóstico de conectividad desde la laptop del laboratorio](Docs/Imagenes/README/04-diagnostico-red.png)
-->
> 📷 **Módulo 3:** una prueba de conectividad en la laptop y su relación con la red del escenario.

## 🤖 Acompañamiento durante la práctica

- **Instructor virtual:** diálogos con voz, desplazamientos por puntos de referencia y reacciones procedurales ante logros y errores.
- **Indicaciones visuales:** iconos de controles integrados en los diálogos y objetivos consultables durante la actividad.
- **Herramientas interactivas:** selección mediante una rueda de herramientas y manipulación de elementos del laboratorio.
- **Evaluación y progreso:** cierre de módulos con evaluaciones e insignias asociadas a la finalización.
- **Audio ajustable:** controles independientes de música y voz del NPC desde el menú de pausa.

<!-- IMAGEN 05 — GUÍA E INTERACCIÓN
Guardar en Docs/Imagenes/README/05-instructor-controles.png.
Encuadrar al robot y un diálogo corto con un icono de botón claramente visible.
![Instructor virtual con indicaciones e iconos de controles](Docs/Imagenes/README/05-instructor-controles.png)
-->
> 📷 **Instructor:** robot y diálogo con un icono de control legible.

## ⚙️ Cómo está construido

| Tecnología o sistema | Aplicación en el proyecto |
|---|---|
| **Unity 6000.2.7f2 + URP** | Escenarios 3D, interfaz y efectos visuales. |
| **C#** | Interacción, objetivos, secuencias de tutorial y lógica de simulación. |
| **OpenXR, XR Interaction Toolkit y Meta XR** | Integración con visor, mandos e interacciones VR. El proyecto conserva componentes Meta OVR y XRI según el módulo. |
| **ScriptableObjects** | Contenido editable: diálogos, audios, objetivos y configuración de módulos. |
| **TextMeshPro** | Diálogos con escritura progresiva e iconos de botones. |
| **Firebase Authentication y Realtime Database** | Autenticación y respaldo remoto del progreso. |
| **Guardado local por cuenta** | Archivos separados por usuario y sincronización manual con Firebase. |

### Decisiones de diseño

**Contenido separado de la lógica.** Los textos y audios del instructor se editan en GameData, facilitando la colaboración entre quienes preparan el contenido y quienes implementan la experiencia.

**Secuencias coordinadas.** Los diálogos, movimientos y reacciones del NPC se organizan en pasos para controlar cuándo puede continuar la práctica.

**Progreso local con respaldo manual.** Completar actividades registra el progreso en el dispositivo. La subida a Firebase se solicita de forma explícita y mantiene los cambios pendientes hasta recibir confirmación.

## 🚧 Estado y próximos pasos

El repositorio contiene el Lobby y los tres módulos, junto con pruebas automatizadas de lógica y regresión. La validación visual, funcional y de rendimiento en el visor continúa como parte del desarrollo.

Los siguientes puntos permanecen abiertos:

- Resolver conflictos de guardado entre dispositivos antes de reemplazar una copia remota.
- Completar pruebas integradas de autenticación y sincronización con Firebase.
- Ajustar legibilidad, interacción y rendimiento a partir de pruebas en VR.

En esta versión, el progreso registra la finalización de módulos; **no incluye reanudar una práctica a mitad de su ejecución**.

## 📂 Estructura del repositorio

```text
Assets/
├── Scenes/        # Menú, Lobby y módulos
├── Scripts/       # Lógica compartida y sistemas por módulo
├── GameData/      # Contenido y configuración mediante ScriptableObjects
├── Prefabs/       # Jugador, dispositivos, herramientas e interfaces
└── Shaders/       # Efectos visuales
Docs/              # Arquitectura, guiones y guías de configuración
Tests/             # Pruebas automatizadas de lógica y regresión
```

## 🚀 Abrir el proyecto

1. Instala **Unity 6000.2.7f2** con soporte de compilación Android.
2. Abre la carpeta del proyecto desde Unity Hub y deja que termine la importación.
3. Comprueba la configuración Android de XR y las dependencias del proyecto.
4. Para utilizar autenticación y respaldo remoto, configura Firebase para tu propia aplicación.
5. Abre `Assets/Scenes/Menu.unity` para revisar el flujo de entrada. Valida la experiencia completa en el visor.

## 📚 Documentación

- [Arquitectura del módulo 2](Docs/Module02-Architecture.md)
- [Configuración del módulo 3](Docs/Module03-GameData.md)
- [Diálogos y audios del NPC](Docs/NPC-Dialogos-GameData.md)
- [Referencia de iconos en diálogos](Docs/Referencia-Iconos-Dialogos.md)
- [Guardado híbrido y estados](Docs/Guardado-Hibrido-Estados.md)
- [Guía de capturas y publicación en LinkedIn](Docs/Guia-Visual-LinkedIn.md)

---

**NetXR Lab** explora cómo llevar la práctica de redes a un entorno inmersivo, desde la preparación de un cable hasta la identificación de una falla de conectividad.
