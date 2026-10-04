# Prueba de LED y neblina del Lobby

En la escena Lobby, _Enviroment/LobbyProd/Lightseffect:
- FakeServerLights busca el objeto importado Servers bajo LobbyProd.
- Genera overlays de sus mallas al entrar en Play, sin collider ni sombras.
- El shader dibuja puntos en superficies verticales; no agrega luces reales.
- El Quad existente conserva posicion y escala, ahora con ServerMist y collider desactivado.

## Probar
1. Abrir Lobby y entrar en Play; mirar los servidores por las ventanas.
2. Para previsualizar LED en edicion: menu contextual de FakeServerLights > Rebuild server LED preview.
   La geometria temporal no se guarda y se regenera en Play.
3. Si se renombra Servers en el modelo, asignar Server Root manualmente o cambiar Server Object Name.

## Ajustar
Material Assets/Recursos/LobbyUI/Materials/ServerLEDs.mat:
- LED green / LED cyan: colores.
- Intensity: brillo visual (no ilumina otras superficies).
- Activity speed: frecuencia de actividad independiente por punto.
- Columns / Rows per meter: densidad del patron.
- Dot radius: tamano del punto en cada celda.
- Surface offset: separacion minima de la malla para evitar parpadeo de profundidad.

Material Assets/Recursos/LobbyUI/Materials/ServerMist.mat:
- Opacity: empieza en 0.085; aumentar con moderacion.
- Noise scale: tamano de las nubes.
- Drift speed: movimiento lento.
- Mist color: tono del humo.
Mover/escalar el Quad permite situar la neblina a la altura deseada.

Para desactivar LED, deshabilitar FakeServerLights. Para desactivar neblina,
deshabilitar el MeshRenderer del Quad. El material original FakeLight_LED se conserva.

## Alcance y limites
Es una prueba decorativa: puntos en caras verticales de los gabinetes y una capa
horizontal de neblina con bordes difuminados. No es humo volumetrico ni ilumina
el entorno. No requiere Bloom. Materiales compartidos, sin clones por LED.
La malla de servidores se dibuja otra vez para los puntos; revisar su coste en
el visor si tiene muchos poligonos. La neblina usa una sola capa transparente.

Validacion: revisar compilacion C# y referencias serializadas. La compilacion de
shaders y el aspecto final requieren importar y probar en Unity/visor.
