Shader "NetworkSimulator/Rack Airflow"
{
    Properties
    {
        _Speed("Speed", Range(0, 2)) = 0.35
        _Repeat("Arrow count", Range(1, 16)) = 7
        _Opacity("Opacity", Range(0, 1)) = 0.65
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float _Speed, _Repeat, _Opacity;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv; output.color = input.color;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float x = frac(input.uv.x * _Repeat - _Time.y * _Speed);
                float y = abs(input.uv.y - 0.5);
                float shaft = step(0.12, x) * step(x, 0.53) * step(y, 0.11);
                float head = step(0.48, x) * step(x, 0.88) * step(y, (0.88-x)*0.95);
                float fade = smoothstep(0, 0.08, input.uv.x) * smoothstep(0, 0.08, 1-input.uv.x);
                return half4(input.color.rgb, max(shaft,head) * fade * _Opacity);
            }
            ENDHLSL
        }
    }
}
