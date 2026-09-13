Shader "NoDestination/WornFootpath" {
Properties {_BaseMap("Soil",2D)="white"{}}
SubShader {Tags{"Queue"="Transparent-10" "RenderPipeline"="UniversalPipeline"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Pass {
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);float4 _FieldPath;
struct A{float4 p:POSITION;float3 n:NORMAL;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float top:TEXCOORD1;float fog:TEXCOORD2;};
V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.top=TransformObjectToWorldNormal(i.n).y;o.fog=ComputeFogFactor(o.p.z);return o;}
half4 frag(V i):SV_Target{
 clip(i.top-.5);float2 d=_FieldPath.zw-_FieldPath.xy;float u=saturate(dot(i.w.xz-_FieldPath.xy,d)/max(dot(d,d),.001));float2 side=normalize(float2(d.y,-d.x)+.0001);
 float bend=sin(u*3.14159)*sin(u*11)*1.05;float across=distance(i.w.xz,_FieldPath.xy+d*u+side*bend);
 float width=.59+.11*sin(u*31)+.07*sin(i.w.x*4+i.w.z*2.7);float wear=1-smoothstep(width*.25,width+.2,across);
 float patches=.46+.21*sin(i.w.z*1.3+sin(i.w.x*2.7));half3 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.w.xz*.33).rgb*half3(.72,.65,.48);
 return half4(MixFog(c,i.fog),wear*patches);
}
ENDHLSL
}}}
