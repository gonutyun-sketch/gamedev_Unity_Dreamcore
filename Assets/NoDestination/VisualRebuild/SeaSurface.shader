Shader "NoDestination/SeaSurface" {
Properties {_BaseColor("Water",Color)=(.65,.65,.53,1)}
SubShader{Tags{"Queue"="Geometry+501" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"} Blend SrcAlpha OneMinusSrcAlpha ZWrite On Cull Off Pass{
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float fog:TEXCOORD1;};float4 _BaseColor;float _RealityBlend;
V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.fog=ComputeFogFactor(o.p.z);return o;}
half4 frag(V i):SV_Target{
 float alpha=saturate(_RealityBlend*1.4);clip(alpha-.001);
 float3 view=normalize(_WorldSpaceCameraPos-i.w);float horizon=pow(1-saturate(view.y),.38);
 float ripple=sin(i.w.x*.71+sin(i.w.z*.24+_Time.y*.18))*sin(i.w.z*.53-_Time.y*.13);
 half3 c=lerp(_BaseColor.rgb,pow(half3(.82,.78,.64),2.2),horizon);
 float distanceFade=1-smoothstep(25,140,distance(i.w,_WorldSpaceCameraPos));
 float fine=sin(i.w.z*14+sin(i.w.x*3.1+_Time.y*.23)*1.8-_Time.y*.35);
 c+=(ripple*.014+pow(saturate(fine),12)*.018)*distanceFade;
 return half4(MixFog(c,i.fog),alpha);
}
ENDHLSL
}}}
