Shader "NetworkSimulator/Lobby/Drifting Mist"
{
 Properties {
  _Color("Mist color", Color) = (0.5,0.7,0.75,1)
  _Opacity("Opacity", Range(0,0.4)) = 0.085
  _Scale("Noise scale", Float) = 4
  _Speed("Drift speed", Range(0,0.3)) = 0.025
  _Offset("Surface offset", Float) = 0
 }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
  Pass {
   Tags { "LightMode"="SRPDefaultUnlit" }
   Blend SrcAlpha OneMinusSrcAlpha
   Cull Off ZWrite Off
   HLSLPROGRAM
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

   CBUFFER_START(UnityPerMaterial)
   float4 _Color;
   float _Opacity, _Scale, _Speed, _Offset;
   CBUFFER_END
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 world : TEXCOORD0; float3 normal : TEXCOORD1; float2 uv : TEXCOORD2; UNITY_VERTEX_OUTPUT_STEREO };
            float Hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            Varyings Vert(Attributes v) {
                Varyings o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.world = TransformObjectToWorld(v.positionOS.xyz);
                o.normal = TransformObjectToWorldNormal(v.normalOS);
                o.positionCS = TransformWorldToHClip(o.world + o.normal * _Offset);
                o.uv=v.uv; return o;
            }

   float Noise(float2 p) {
    float2 c=floor(p), f=frac(p); f=f*f*(3-2*f);
    return lerp(lerp(Hash(c),Hash(c+float2(1,0)),f.x),lerp(Hash(c+float2(0,1)),Hash(c+1),f.x),f.y);
   }
   half4 Frag(Varyings i):SV_Target {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float2 p=i.uv*_Scale + _Time.y*_Speed*float2(1,0.4);
    float n=Noise(p)*0.65+Noise(p*2.1-_Time.y*_Speed*0.6)*0.35;
    float2 edge=smoothstep(0,0.18,i.uv)*smoothstep(0,0.18,1-i.uv);
    float facing=smoothstep(0.04,0.22,abs(dot(normalize(i.normal),normalize(GetWorldSpaceViewDir(i.world)))));
    return half4(_Color.rgb,smoothstep(0.25,0.85,n)*edge.x*edge.y*_Opacity*facing);
   }
   ENDHLSL
  }
 }
}
