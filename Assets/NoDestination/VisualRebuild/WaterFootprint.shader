Shader "NoDestination/WaterFootprint" {
SubShader{Tags{"Queue"="Transparent+40" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off Pass{
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}
half4 frag(V i):SV_Target{
 float2 p=i.uv-.5;float heel=length((p-float2(0,-.27))/float2(.30,.17));float toe=length((p-float2(.03,.13))/float2(.37,.32));float edge=min(heel,toe);float a=saturate((1-edge)*7)*.36;float tread=.88+.12*sin(p.y*105);return half4(pow(half3(.37,.40,.33),2.2),a*tread);
}
ENDHLSL
}}}
