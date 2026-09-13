Shader "NoDestination/FieldContinuum" {
Properties {_BaseColor("Distant wheat",Color)=(.7,.55,.28,1)}
SubShader{Tags{"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"} Pass{
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float fog:TEXCOORD1;};float4 _BaseColor;float _WorldTravel,_RealityBlend;
V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.fog=ComputeFogFactor(o.p.z);return o;}
half4 frag(V i):SV_Target{
 float2 uv=i.w.xz+float2(0,_WorldTravel);float distanceToEye=distance(i.w,_WorldSpaceCameraPos);
 float rows=sin(uv.x*5+sin(uv.y*.6))*sin(uv.y*7);float swath=sin(uv.x*.061+uv.y*.043+sin(uv.x*.01))*sin(uv.y*.021);
 float fine=rows*lerp(.08,0,saturate(distanceToEye/180));half3 c=_BaseColor.rgb*(.93+fine+swath*.065);
 c=lerp(c,half3(.79,.76,.62),_RealityBlend);return half4(MixFog(c,i.fog),1);
}
ENDHLSL
}}}
