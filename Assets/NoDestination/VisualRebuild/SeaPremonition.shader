Shader "NoDestination/SeaPremonition" {
SubShader{Tags{"Queue"="Transparent+30" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off Pass{
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};float _PuddleReveal;
V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}
half4 frag(V i):SV_Target{
 float2 uv=i.uv;float edge=saturate((.5-abs(uv.x-.5))*15)*saturate((.5-abs(uv.y-.5))*9);
 float pool1=1-smoothstep(.6,1,length((uv-float2(.42,.28))*float2(2.8,5.5)));
 float pool2=1-smoothstep(.65,1,length((uv-float2(.57,.66))*float2(3.7,4.1)));
 float pools=max(pool1,pool2);float h=uv.y-.53+sin(uv.x*16+_Time.y*.2)*.002;
 half3 c=pow(half3(.63,.61,.47),2.2);c-=exp(-abs(h)*180)*.035;
 c+=sin(uv.y*320+sin(uv.x*19)+_Time.y*.4)*.004;
 return half4(c,edge*pools*_PuddleReveal*.46);
}
ENDHLSL
}}}
