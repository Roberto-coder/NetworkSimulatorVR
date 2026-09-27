using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

namespace Modules.Module03_Diagnostics.Presentation
{
    /// <summary>
    /// Ejemplo visual: dibuja una spline y mueve cuatro esferas sobre ella.
    /// No simula Ethernet ni completa objetivos del módulo.
    /// </summary>
    [RequireComponent(typeof(SplineContainer))]
    public sealed class SplinePacketDemo : MonoBehaviour
    {
        // Referencias configuradas en la escena. El asset aporta valores iniciales;
        // los controles de Play cambian variables locales, no el asset compartido.
        [SerializeField] private SplinePacketDemoSettings settings;
        [SerializeField] private Shader visualizationShader;
        [SerializeField] private bool useDemoCamera = true;
        private SplineContainer route;
        private GameObject generated;
        private Camera demoCamera;
        private LineRenderer cable;
        private Transform source, destination, failureMarker;
        // Materiales propios para liberarlos al cerrar la demo y cuatro esferas
        // reutilizables: repetir un ping no vuelve a crear objetos.
        private readonly List<Material> materials = new();
        private readonly Transform[] packets = new Transform[4];
        private readonly Material[] packetMaterials = new Material[4];
        // points contiene posiciones mundiales; distances, la distancia acumulada
        // desde el origen hasta cada muestra de la curva.
        private Vector3[] points;
        private float[] distances;
        // Aunque se llama elapsed, acumula DISTANCIA (deltaTime * speed), no segundos.
        private float length, elapsed, speed;
        // dirty indica que se debe volver a muestrear la spline antes de animar.
        private bool active, paused, broken, xray, dirty;
        private Matrix4x4 previousMatrix;
        private string status = "Listo";
        private Material cableMaterial, failureMaterial;

        // Entrada: crear los objetos, calcular el recorrido y lanzar la primera prueba.
        private void OnEnable()
        {
            route = GetComponent<SplineContainer>();
            if (settings == null || visualizationShader == null)
            {
                Debug.LogError("Spline demo needs settings and its visualization shader.", this);
                enabled = false;
                return;
            }
            speed = settings.packetSpeed;
            BuildPresentation();
            RebuildRoute();
            Spline.Changed += OnSplineChanged;
            SendPing();
        }

        // El evento es global a Splines; solo reaccionamos a nuestra primera spline.
        // Se difiere la reconstrucción a Update para no hacerla dentro del evento.
        private void OnSplineChanged(Spline spline, int knot, SplineModification modification)
        {
            if (spline == route.Spline) dirty = true;
        }

        // Parte del modo normal: material con color fijo, prueba de profundidad
        // y cola opaca. SetDepth ajusta después el cable y los paquetes.
        private Material MakeMaterial(Color color)
        {
            var material = new Material(visualizationShader);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            material.renderQueue = 2000;
            materials.Add(material);
            return material;
        }

        // Crea geometría de demostración sin colisiones: su movimiento es calculado,
        // no depende de Rigidbody. La posición recibida es local al objeto generado.
        private Transform Shape(string label, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = label;
            obj.transform.SetParent(generated.transform, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            var collider = obj.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
            obj.GetComponent<Renderer>().sharedMaterial = material;
            return obj.transform;
        }

        // Todos los objetos temporales quedan bajo un padre para retirarlos juntos.
        // La spline ya existe en la escena; aquí solo construimos su presentación.
        private void BuildPresentation()
        {
            generated = new GameObject("Demo visuals (runtime only)");
            generated.transform.SetParent(transform, false);
            cableMaterial = MakeMaterial(settings.cableColor);
            cable = new GameObject("Cable sampled from SplineContainer").AddComponent<LineRenderer>();
            cable.transform.SetParent(generated.transform, false);
            cable.sharedMaterial = cableMaterial;     
            cable.widthMultiplier = settings.cableWidth;
            cable.numCornerVertices = 6;
            cable.numCapVertices = 6;
            // EvaluatePosition del contenedor devuelve coordenadas mundiales.
            cable.useWorldSpace = true;
            cable.shadowCastingMode = ShadowCastingMode.Off;
            source = Shape("A - Origen", PrimitiveType.Cube, Vector3.zero, Vector3.one * 0.45f,
                MakeMaterial(new Color(0.08f, 0.65f, 0.85f)));
            destination = Shape("B - Destino", PrimitiveType.Cube, Vector3.zero, Vector3.one * 0.45f,
                MakeMaterial(new Color(0.2f, 0.8f, 0.4f)));
            failureMaterial = MakeMaterial(settings.failureColor);
            failureMarker = Shape("Interrupcion simulada", PrimitiveType.Sphere, Vector3.zero,
                Vector3.one * 0.32f, failureMaterial);
            Shape("Pared para comparar oclusion", PrimitiveType.Cube, new Vector3(0, 1.7f, 1.8f),
                new Vector3(2.1f, 3.4f, 0.18f), MakeMaterial(new Color(0.26f, 0.3f, 0.38f)));
            for (int i = 0; i < packets.Length; i++)
            {
                packetMaterials[i] = MakeMaterial(settings.requestColor);
                packets[i] = Shape("Packet " + (i + 1), PrimitiveType.Sphere, Vector3.zero,
                    Vector3.one * settings.packetDiameter, packetMaterials[i]);
                packets[i].gameObject.SetActive(false);
            }
            demoCamera = new GameObject("Spline demo camera").AddComponent<Camera>();
            demoCamera.transform.SetParent(generated.transform, false);
            demoCamera.transform.localPosition = new Vector3(0, 5, -10);
            demoCamera.transform.LookAt(transform.TransformPoint(new Vector3(0, 1.3f, 2)));
            demoCamera.clearFlags = CameraClearFlags.SolidColor;
            demoCamera.backgroundColor = new Color(0.025f, 0.035f, 0.06f);
            // Esta cámara se dibuja después de las originales sin desactivarlas.
            demoCamera.depth = 100;
            demoCamera.fieldOfView = 45;
            demoCamera.enabled = useDemoCamera;
            SetXray(false);
        }

        // Muestrea la curva una vez al iniciar o al editarla/mover su transform.
        // Cancela el envío anterior para no mostrar paquetes sobre una ruta obsoleta.
        private void RebuildRoute()
        {
            active = false;
            foreach (var packet in packets) packet.gameObject.SetActive(false);
            dirty = false;
            previousMatrix = route.transform.localToWorldMatrix;
            int count = Mathf.Clamp(settings.samples, 32, 512);
            points = new Vector3[count + 1];
            distances = new float[count + 1];
            length = 0;
            if (route.Spline == null || route.Spline.Count < 2)
            {
                cable.positionCount = 0;
                source.gameObject.SetActive(false);
                destination.gameObject.SetActive(false);
                failureMarker.gameObject.SetActive(false);
                status = "La ruta necesita al menos dos puntos.";
                return;
            }
            // El parámetro t va de 0 a 1, pero NO representa distancia uniforme.
            // Sumamos las longitudes de las muestras para construir otra referencia.
            for (int i = 0; i <= count; i++)
            {
                points[i] = route.EvaluatePosition(i / (float)count);
                if (i > 0) length += Vector3.Distance(points[i - 1], points[i]);
                distances[i] = length;
            }
            cable.positionCount = points.Length;
            cable.SetPositions(points);
            source.gameObject.SetActive(true);
            destination.gameObject.SetActive(true);
            source.position = points[0];
            destination.position = points[count];
            // Posición arbitraria para demostrar un fallo conocido, no diagnosticado.
            failureMarker.position = AtDistance(length * 0.56f);
            failureMarker.gameObject.SetActive(broken);
            status = "Ruta actualizada. Ejecuta un nuevo ping.";
        }

        // Convierte metros recorridos a posición sobre la polilínea muestreada.
        // Así la velocidad es aproximadamente constante aunque los knots no lo sean.
        private Vector3 AtDistance(float distance)
        {
            int index = Array.BinarySearch(distances, Mathf.Clamp(distance, 0, length));
            if (index >= 0) return points[index];
            // BinarySearch devuelve un número negativo si no hay coincidencia exacta;
            // ~index recupera el índice de inserción y ubica el tramo que nos contiene.
            index = Mathf.Clamp(~index, 1, points.Length - 1);
            float span = distances[index] - distances[index - 1];
            return Vector3.Lerp(points[index - 1], points[index],
                span > 0.00001f ? (distance - distances[index - 1]) / span : 0);
        }

        // Reinicia la animación usando las mismas cuatro esferas. Una ruta vacía
        // no puede iniciar una prueba; aquí no se consulta ninguna IP real.
        public void SendPing()
        {
            if (length < 0.001f) return;
            elapsed = 0;
            active = true;
            paused = false;
            status = broken ? "Solicitud hacia interrupcion simulada..." : "Azul: solicitud / verde: respuesta";
            foreach (var packet in packets) packet.gameObject.SetActive(false);
        }

        // Unity llama Update cada frame. La pausa detiene el avance, pero permite
        // detectar cambios de ruta y cancelar una prueba que ya no corresponde.
        private void Update()
        {
            if (dirty || previousMatrix != route.transform.localToWorldMatrix) RebuildRoute();
            if (!active || paused) return;
            elapsed += Time.deltaTime * speed;
            // Sin fallo se recorre ida + vuelta; con fallo solo hasta la interrupción.
            float end = broken ? length * 0.56f : length * 2;
            for (int i = 0; i < packets.Length; i++)
            {
                // Separación de 0.65 metros entre paquetes, no retraso en segundos.
                float travelled = elapsed - i * 0.65f;
                bool visible = travelled >= 0 && travelled < end;
                packets[i].gameObject.SetActive(visible);
                if (!visible) continue;
                bool reply = !broken && travelled > length;
                // Al regresar, la distancia desde A disminuye hasta cero.
                packets[i].position = AtDistance(reply ? length * 2 - travelled : travelled);
                packetMaterials[i].SetColor("_BaseColor", reply ? settings.replyColor : settings.requestColor);
            }
            // Esperamos al último paquete, que empezó después de los anteriores.
            if (elapsed >= end + (packets.Length - 1) * 0.65f)
            {
                active = false;
                status = broken ? "Sin respuesta: 4/4 detenidos en la interrupcion." : "Ping visual completo: 4/4 respuestas.";
            }
        }

        // Solo cambia cable, paquetes y marcador; la pared conserva profundidad.
        private void SetXray(bool value)
        {
            xray = value;
            SetDepth(cableMaterial);
            SetDepth(failureMaterial);
            foreach (var material in packetMaterials) SetDepth(material);
        }

        // LessEqual respeta objetos delante; Always dibuja aunque exista una pared.
        // ZWrite=0 evita que el overlay escriba profundidad y oculte otros elementos.
        private void SetDepth(Material material)
        {
            material.SetFloat("_ZTest", (float)(xray ? CompareFunction.Always : CompareFunction.LessEqual));
            material.SetFloat("_ZWrite", 0);
            material.renderQueue = xray ? 3100 : 3000;
        }

        // Panel IMGUI para ratón en NoVR. Unity puede llamarlo varias veces por frame;
        // la animación avanza en Update, mientras aquí se leen las acciones de UI.
        private void OnGUI()
        {
            float scale = Mathf.Clamp(Screen.height / 800f, 0.65f, 1.5f);
            Matrix4x4 old = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            GUILayout.BeginArea(new Rect(16, 16, 370, 335), GUI.skin.box);
            GUILayout.Label("SPLINE / PAQUETES - DEMO NoVR");
            GUILayout.Label("A (azul) → B (verde). Simulacion visual.");
            GUILayout.Label(status);
            if (GUILayout.Button("Enviar 4 paquetes / repetir ping", GUILayout.Height(32))) SendPing();
            if (GUILayout.Button(paused ? "Continuar" : "Pausar", GUILayout.Height(28))) paused = !paused;
            bool nextBroken = GUILayout.Toggle(broken, "Simular interrupcion al 56% de la ruta");
            if (nextBroken != broken)
            {
                broken = nextBroken;
                failureMarker.gameObject.SetActive(broken && length > 0);
                SendPing();
            }
            bool nextXray = GUILayout.Toggle(xray, "Rayos X: ver cable y paquetes tras la pared");
            if (nextXray != xray) SetXray(nextXray);
            useDemoCamera = GUILayout.Toggle(useDemoCamera, "Usar camara de demostracion");
            demoCamera.enabled = useDemoCamera;
            GUILayout.Label($"Velocidad visual: {speed:F1} m/s | Ruta: {length:F1} m");
            speed = GUILayout.HorizontalSlider(speed, 0.2f, 6f);
            GUILayout.Label("Editar recorrido: salir de Play, seleccionar\nSplinePacketDemo y mover sus knots con Splines.");
            GUILayout.EndArea();
            // No dejar nuestra escala aplicada a otros paneles IMGUI de la escena.
            GUI.matrix = old;
        }

        // Previsualización en Scene fuera de Play: dibuja la ruta sin crear objetos.
        private void OnDrawGizmos()
        {
            if (Application.isPlaying) return;
            var container = GetComponent<SplineContainer>();
            if (container == null || container.Spline == null || container.Spline.Count < 2) return;
            Gizmos.color = Color.cyan;
            Vector3 previous = container.EvaluatePosition(0);
            Gizmos.DrawSphere(previous, 0.15f);
            for (int i = 1; i <= 100; i++)
            {
                Vector3 position = container.EvaluatePosition(i / 100f);
                Gizmos.DrawLine(previous, position);
                previous = position;
            }
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(previous, 0.15f);
        }

        // Liberar objetos/materiales creados en ejecución y dejar de escuchar el
        // evento estático para evitar referencias a una demo que ya no está activa.
        private void OnDisable()
        {
            Spline.Changed -= OnSplineChanged;
            if (generated != null) Destroy(generated);
            foreach (var material in materials) if (material != null) Destroy(material);
            materials.Clear();
            active = false;
        }
    }
}
