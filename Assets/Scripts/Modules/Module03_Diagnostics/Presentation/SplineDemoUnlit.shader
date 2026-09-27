Shader "NetworkSimulator/SplineDemoUnlit"
{
    // Shader URP de color plano: no necesita luces para que se vea la demostración.
    // SplinePacketDemo modifica estas propiedades en los materiales que crea.
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
        // 4 = LessEqual (oclusión normal); 8 = Always (rayos X).
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Depth test", Float) = 4
        [Toggle] _ZWrite ("Write depth", Float) = 1
    }
    SubShader
    {
        // El script ajusta la cola: 2000 para pared/extremos, 3000 para tráfico
        // normal y 3100 para rayos X. Este pase devuelve color sin mezcla alfa.
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            // Consultar profundidad y escribirla son decisiones independientes.
            // La pared escribe; el cable y los paquetes solo consultan o ignoran.
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; UNITY_VERTEX_OUTPUT_STEREO };
            // Agrupa propiedades del material para el SRP Batcher de URP.
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END
            Varyings Vert(Attributes input)
            {
                Varyings output;
                // Macros para instancias y salida estéreo; no implican QA en visor.
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                // Convierte vértices locales a coordenadas de proyección de cámara.
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            // Todos los píxeles de la superficie usan el color asignado por el script.
            half4 Frag(Varyings input) : SV_Target { return _BaseColor; }
            ENDHLSL
        }
    }
}
