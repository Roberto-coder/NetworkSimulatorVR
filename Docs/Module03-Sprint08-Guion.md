> Documento histórico. La revisión vigente del orden, objetivos y ubicación de diálogos está en [Tutorial secuencial](Module03-TutorialSequence.md). El guion Word actualizado es `Guiones_Modulos_M3.docx`.

# Módulo 3 · Sprint 8: guion de diagnóstico de red

Estado: versión 2 aprobada. Integración del sprint 8 preparada; consultar Module03-Sprint08.md para aplicar y verificar en Unity.

## Intención pedagógica

El estudiante asume el papel de ingeniero de redes: observar síntomas, reunir evidencia, proponer una corrección y verificarla. El NPC acompaña la práctica sin resolverla por el estudiante. Los conceptos aparecen cuando ayudan a interpretar una acción concreta.

Se conserva la laptop fija, las pantallas locales de las PCs y las herramientas actuales. No se promete administración remota de PCs. La consola actual permite ping local desde PCs; las verificaciones del flujo se solicitan desde la laptop para mantener un origen común. La selección del mapa elige un destino; la administración del switch se abre con Administrar.

## Ritmo y presentación

- Introducción dividida en cuatro intervenciones breves, ligadas a la llegada y al primer uso de cada control; no reproducirlas todas al entrar.
- Una instrucción principal por objetivo. Consultable nuevamente en muñeca, sin exigir escucharla para manipular objetos.
- Conceptos y pistas opcionales, con subtítulos y posibilidad de omitir/repetir. Ofrecer ayuda ante dificultad; no atribuir desconocimiento por un solo intento fallido.
- Una sola voz a la vez. No interrumpir una instrucción, una lectura de resultados o el uso del tester. Cancelar ayudas pendientes si dejan de ser pertinentes.
- Cada ayuda automática, si se habilita, se reproduce una vez por sesión; dejar al menos 30 segundos entre ellas. Preferir una invitación discreta a escucharla. Pausar voz y temporizadores con el módulo.
- Duración orientativa: 10–18 segundos por intervención; ajustar al grabar. No bloquear el progreso por una locución. Las evidencias, no los diálogos, acreditan los objetivos.

## 1. Bienvenida e introducción a la interfaz

### IN-01 · Bienvenida — al iniciar

> Bienvenido al tercer módulo. Como ingeniero de redes, tu responsabilidad será identificar y resolver los problemas de conectividad de esta oficina. Consulta la configuración de los equipos, comprueba tus hipótesis y verifica cada reparación. Encontrarás los objetivos en la pantalla de tu muñeca.

Waypoint: laptop. Esta es la única intervención que se ofrece antes de iniciar la exploración.

### IN-02 · Topología y acceso — al abrir la laptop por primera vez

> Este diagrama muestra los equipos y sus conexiones; su distribución es aproximada. Selecciona un equipo para consultar su dirección documentada o elegirlo como destino de ping. Para gestionar el switch, selecciónalo y pulsa Administrar. La configuración de cada PC se modifica en su propia pantalla.

Nota de interacción: acercarse y apuntar al dispositivo; A abre o cierra su pantalla. Mantener esta ayuda como indicación visual breve, sin otro diálogo obligatorio.

### IN-03 · Rayos X — ayuda al primer uso

> Rayos X permite ver las rutas a través de los objetos. Al ejecutar un ping, observa la animación del intercambio de mensajes. Es una representación didáctica: su velocidad está reducida para que puedas seguir el recorrido.

No presentar esta visualización como captura de tráfico real ni como medida de latencia. El botón controla visibilidad; el comando inicia la prueba.

### IN-04 · Bitácora — ayuda al primer acceso

> La bitácora conserva las consultas y los cambios realizados durante la práctica. Úsala para revisar qué hiciste y relacionarlo con los resultados. Después de modificar la red, repite las pruebas: una respuesta anterior puede haber quedado desactualizada.

## 2. Objetivos y narrativa principal

| ID | Momento | Diálogo principal | Texto en muñeca / orientación |
| --- | --- | --- | --- |
| OB-01 | Incidente de cable | «PC-01 presenta un problema de conectividad. Examina su enlace con la roseta y utiliza el tester para comprobar el patch cord. Si confirmas un daño, prueba el reemplazo, instálalo y etiqueta ambos extremos. Después, verifica la conexión desde la laptop.» | Diagnosticar y reparar el enlace de PC-01. Waypoint a PC-01; tester y repuesto como ayudas. |
| OB-02 | Incidente de puerto | «Ahora revisa el acceso de PC-02. Desde la laptop, consulta los puertos del switch e identifica cuál corresponde a su conexión. Comprueba si está habilitado y corrige su estado cuando sea necesario.» | Revisar y habilitar el puerto de PC-02. Waypoint a laptop. |
| OB-03 | Incidente de dirección | «Continúa con PC-02 y PC-03. Compara sus direcciones con el inventario y comprueba qué equipo responde. Si encuentras una dirección duplicada, corrígela desde la pantalla de la PC correspondiente y repite las pruebas.» | Resolver la IP duplicada y verificar ambas PCs. Laptop primero; ayuda hacia PC-03 bajo petición. |
| OB-04 | Verificación final | «Antes de cerrar el diagnóstico, comprueba la conectividad de las tres PCs. Asegúrate de obtener respuestas de los equipos esperados después del último cambio. Una reparación termina cuando puedes demostrar que funciona.» | Ejecutar nuevos pings a las tres PCs. Waypoint a laptop. |

No anunciar que el módulo completo terminó aquí: quiz, logro y regreso al lobby pertenecen al cierre posterior. Los éxitos intermedios pueden indicarse con texto breve en muñeca, sin otra conversación.

## 3. Ayudas conceptuales opcionales

Estas cápsulas no forman una segunda clase obligatoria. Presentar solo la pertinente, en el momento de uso o a petición del estudiante.

| ID / contexto | Texto propuesto |
| --- | --- |
| AY-01 · Configuración local | «La IP identifica una interfaz dentro de una red IP. La máscara permite determinar qué direcciones pertenecen a su subred. La MAC identifica la interfaz en el enlace Ethernet. Para localizar el equipo en la oficina, consulta también el inventario y las etiquetas.» |
| AY-02 · Primer ping / ¿qué comprueba? | «Ping envía una solicitud de eco ICMP y espera una respuesta. Te ayuda a comprobar la comunicación IP, pero un fallo no identifica por sí solo la causa: revisa el enlace, el estado del puerto y la configuración.» |
| AY-03 · Recorrido y capas | «En esta red, los mensajes ICMP viajan dentro de paquetes IP, transportados en tramas Ethernet y transmitidos como señales por el cable. Así relacionamos la capa de red, la de enlace y la física: capas tres, dos y uno.» |
| AY-04 · Primera consulta ARP | «Para enviar una trama a otra interfaz de esta red local IPv4, el equipo necesita conocer su MAC. ARP permite obtenerla a partir de la IP. Esa asociación se guarda temporalmente en una tabla que puedes consultar.» |
| AY-05 · Repetición de ping / caché ARP | «ARP no necesita repetir la consulta para cada ping. Si existe una asociación válida en la caché, puede reutilizarla. En esta simulación, los cambios de red invalidan esas observaciones para que las siguientes pruebas reflejen el nuevo estado.» |
| AY-06 · Tester | «Desconecta ambos extremos del cable y colócalos en el tester. Compara los indicadores de ambos lados. Aquí comprobamos continuidad; esta prueba no certifica por sí sola la categoría ni el rendimiento del cable.» |
| AY-07 · Elección del reemplazo | «En esta oficina usaremos patch cords de hasta cinco metros entre la roseta y la PC. El reemplazo previsto mide cuatro metros: colócalo con holgura, sin tensarlo ni obstruir el paso.» |
| AY-08 · Etiquetado | «Identifica ambos extremos del reemplazo según los puntos que conecta. Las etiquetas permiten seguir el enlace durante el mantenimiento sin depender de la posición del cable.» |
| AY-09 · Puerto deshabilitado | «Un cable conectado no garantiza comunicación. Un puerto puede estar deshabilitado administrativamente. Revisa su estado antes de sustituir componentes que podrían estar funcionando correctamente.» |
| AY-10 · IP duplicada | «En esta misma red, dos interfaces no deben usar la misma dirección IPv4. Un conflicto puede provocar respuestas inconsistentes. Comprueba la configuración local y la identidad del equipo que respondió; no te bases solo en que el ping tuvo respuesta.» |

## 4. Pistas progresivas ante dificultad

Ofrecer con “¿Necesitas una pista?” y reproducir únicamente si se solicita. No revelar directamente P02 o la IP correcta en la primera ayuda.

| Situación | Primera pista | Segunda pista, bajo petición |
| --- | --- | --- |
| PC-01 sigue sin responder | «Separa el diagnóstico físico del lógico. ¿Has comprobado el cable fuera de la instalación?» | «Prueba el cable retirado y el reemplazo. Revisa después ambos encajes y las etiquetas.» |
| No localiza el puerto | «Sigue en el diagrama el enlace desde PC-02 hacia el switch.» | «Consulta la lista de puertos y compara su identificación con el enlace documentado.» |
| Habilitó el puerto, pero no resuelve la conexión | «Puedes haber corregido una falla y seguir observando otra. Revisa ahora las direcciones de PC-02 y PC-03.» | «Compara la IP local de cada PC con su dirección documentada y comprueba si se repite alguna.» |
| La validación permanece pendiente | «¿Las pruebas se realizaron después del último cambio?» | «Repite los pings requeridos y confirma que responde cada equipo esperado.» |

## 5. Notas de rigor académico — no locutar

**Longitud del patch cord.** No afirmar sin matices que cualquier cable roseta–PC mayor de 5 m incumple universalmente una norma. La referencia técnica de Fluke describe 5 m como recomendación para cordones y distingue canal de enlace permanente. En el guion se fija el máximo de 5 m como criterio de diseño de esta oficina. Si se desea decir “por normativa”, debe identificarse la norma, edición y configuración de canal aplicables y revisar su cláusula correspondiente. No atribuir una falla automática de ping a superar esa longitud. Fuente: [Fluke Networks, canal y enlace permanente](https://www.flukenetworks.com/blog/cabling-chronicles/dumb-thing-1-specifying-channel-testing-when-installing-permanent-links).

**Ping e ICMP.** La solicitud y respuesta de eco permiten comprobar respuesta IP; no prueban que todos los servicios funcionen. La explicación por capas describe encapsulación, no que todos los dispositivos del recorrido procesen las tres capas. Fuente: [RFC 792, ICMP](https://www.rfc-editor.org/rfc/rfc792.html).

**ARP.** Resuelve asociaciones de direcciones de protocolo a direcciones Ethernet y mantiene una tabla. No calcula rutas ni utiliza ICMP para resolver direcciones. La cápsula se limita a esta LAN IPv4; no generalizar a destinos remotos ni a IPv6. Fuente: [RFC 826, ARP](https://www.rfc-editor.org/rfc/rfc826.html).

**Simulación y realidad.** La invalidación global de caché por revisión, la detección determinista del conflicto y la animación ralentizada son simplificaciones de este módulo. No presentarlas como comportamiento universal de sistemas operativos. La MAC, a veces llamada dirección física, no codifica la ubicación geográfica ni el escritorio del dispositivo. La relación con la ubicación se obtiene del inventario y etiquetado.

**Alcance de evaluación.** La longitud de 5 m es información de diseño en este sprint, no una nueva condición automática de aprobación. Las ayudas no sustituyen evidencia ni añaden objetivos ocultos. No se cambia el prefijo: se consulta el /24 documentado.

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
| Consultar `Module03-Sprint08.md` | Contiene la tabla de scripts implementados y las pruebas de integración. |

El texto aprobado se almacena en `Module03FlowSettings.asset`; los clips de voz quedan disponibles para asignación.
