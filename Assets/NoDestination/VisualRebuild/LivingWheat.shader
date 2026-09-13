Shader "NoDestination/LivingWheat" {
Properties { _BaseColor("Warmth",Color)=(1,0.94,0.70,1) _BaseMap("Blender wheat stand",2D)="white"{} _Cutoff("Cutout",Range(0,1))=.3 }
SubShader { Tags {"RenderType"="TransparentCutout" "Queue"="AlphaTest" "RenderPipeline"="UniversalPipeline"} Cull Off
Pass { Tags {"LightMode"="UniversalForward"}
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
struct A {float4 positionOS:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};struct V {float4 pos:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;float fog:TEXCOORD1;float worldX:TEXCOORD2;float2 worldXZ:TEXCOORD3;};float4 _BaseColor;float _Cutoff;float _RealityBlend;float4 _FieldHouseCentre,_FieldPath;
V vert(A i){V o;float3 p=TransformObjectToWorld(i.positionOS.xyz);float2 trail=_FieldPath.zw-_FieldPath.xy;float tu=saturate(dot(p.xz-_FieldPath.xy,trail)/max(dot(trail,trail),.001));float2 ts=normalize(float2(trail.y,-trail.x)+.0001);float tb=sin(tu*3.14159)*sin(tu*11)*1.05;float td=distance(p.xz,_FieldPath.xy+trail*tu+ts*tb);if(dot(trail,trail)>1)p.y=-.75+(p.y+.75)*lerp(.085,1,smoothstep(.4,1.5,td));float weight=i.uv.y*i.uv.y;float wind=sin(p.x*.54+p.z*.23+_Time.y*1.25)+.35*sin(p.z*.83-_Time.y*1.8);p.x+=wind*.07*weight;p.z+=cos(p.x*.6+_Time.y)*.035*weight;p.y=lerp(p.y,-.72+sin(p.z*.6+_Time.y*.35)*.013,_RealityBlend);o.worldX=p.x;o.worldXZ=p.xz;o.pos=TransformWorldToHClip(p);o.color=i.color;o.uv=i.uv;o.fog=ComputeFogFactor(o.pos.z);return o;}
half4 frag(V i):SV_Target {clip(.999-_RealityBlend);clip(abs(i.worldX)-2.85);
 float2 houseDelta=abs(i.worldXZ-_FieldHouseCentre.xy);if(_FieldHouseCentre.z>0)clip(max(houseDelta.x-_FieldHouseCentre.z,houseDelta.y-_FieldHouseCentre.w));
 float2 pathDelta=_FieldPath.zw-_FieldPath.xy;float u=saturate(dot(i.worldXZ-_FieldPath.xy,pathDelta)/max(dot(pathDelta,pathDelta),.001));
 float2 sideways=normalize(float2(pathDelta.y,-pathDelta.x)+.0001);float bend=sin(u*3.14159)*sin(u*11)*1.05;
 float width=.70+.12*sin(u*31)+.10*sin(i.worldXZ.x*4+i.worldXZ.y*2.7);
 if(dot(pathDelta,pathDelta)>1)clip(lerp(i.color.r-.98,1,smoothstep(.40,1.2,distance(i.worldXZ,_FieldPath.xy+pathDelta*u+sideways*bend))));
 half4 tex=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);clip(tex.a-_Cutoff);half3 c=tex.rgb*_BaseColor.rgb*i.color.rgb*lerp(.59,1.1,saturate(i.uv.y*1.4));c=lerp(c,half3(.78,.75,.60),_RealityBlend);c=MixFog(c,i.fog);return half4(c,1);}
ENDHLSL
}
}}


