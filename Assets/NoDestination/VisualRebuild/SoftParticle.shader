Shader "NoDestination/SoftParticle" {
Properties{_BaseColor("Colour",Color)=(.25,.25,.22,.12)}
SubShader{Tags{"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off Pass{
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};float4 _BaseColor;
V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.c=i.c;return o;}
half4 frag(V i):SV_Target{float d=length(i.uv-.5)*2;return half4(_BaseColor.rgb*i.c.rgb,_BaseColor.a*i.c.a*pow(saturate(1-d*d),2));}
ENDHLSL
}}}
