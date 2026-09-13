Shader "NoDestination/ImpossibleWindow" {
Properties{_Mode("World",Float)=1 _Delay("Delay",Float)=0}
SubShader{Tags{"Queue"="Transparent+20" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off Pass{
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;};float _Mode,_Delay,_WindowFracture;
V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);return o;}
half4 frag(V i):SV_Target{
 float3 ray=normalize(i.w-_WorldSpaceCameraPos);float a=saturate((_WindowFracture-_Delay)*2);half3 c;
 if(_Mode<1.5)c=half3(.30,.32,.30)+ray.y*.02;
 else {c=lerp(half3(.82,.78,.64),half3(.75,.69,.50),pow(saturate(ray.y),.45));if(ray.y<0)c=half3(.82,.78,.64)-min(.14,abs(ray.y))*.25;c-=exp(-abs(ray.y)*1400)*.025;}
 if(ray.y<0&&_Mode>1.5){float2 water=ray.xz/max(.025,-ray.y);c+=sin(water.y*6+sin(water.x*2)+_Time.y*.2)*.012*saturate(-ray.y*8);}
 return half4(pow(max(c,0),2.2),a);
}
ENDHLSL
}}}
