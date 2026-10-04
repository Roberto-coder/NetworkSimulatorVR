Shader "NetworkSimulator/Lobby/Server LEDs"
{
 Properties {
  [HDR] _Color("LED green", Color) = (0.1,1,0.45,1)
  [HDR] _Secondary("LED cyan", Color) = (0.05,0.6,1,1)
  _Intensity("Intensity", Range(0,5)) = 1.5
  _Speed("Activity speed", Range(0,8)) = 1.8
  _Columns("Columns per meter", Float) = 7
  _Rows("Rows per meter", Float) = 7
  _Radius("Dot radius (cell)", Range(0.02,0.3)) = 0.065
  _Offset("Surface offset (meters)", Float) = 0.003
 }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest+10" }
  Pass {
   Tags { "LightMode"="SRPDefaultUnlit" }
   Cull Back ZWrite Off
   HLSLPROGRAM
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

   CBUFFER_START(UnityPerMaterial)
   float4 _Color, _Secondary;
   float _Intensity, _Speed, _Columns, _Rows, _Radius, _Offset;
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

   half4 Frag(Varyings i) : SV_Target {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    clip(0.35-abs(normalize(i.normal).y));
    float horizontal = abs(i.normal.z)>abs(i.normal.x) ? i.world.x : i.world.z;
    float2 grid=float2(horizontal*_Columns,i.world.y*_Rows);
    float2 cell=floor(grid);
    float seed=Hash(cell+floor(i.world.xz*0.1)*17);
    clip(seed-0.48);
    float d=length(frac(grid)-float2(0.75,0.5));
    clip(_Radius-d);
    float beat=step(0.38,Hash(cell+floor(_Time.y*_Speed*(0.6+seed))*11.7));
    float brightness=lerp(0.07,1.0,beat);
    return half4(lerp(_Color.rgb,_Secondary.rgb,step(0.7,seed))*_Intensity*brightness,1);
   }
   ENDHLSL
  }
 }
}
