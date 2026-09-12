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
struct A {float4 positionOS:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};struct V {float4 pos:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;float fog:TEXCOORD1;float worldX:TEXCOORD2;};float4 _BaseColor;float _Cutoff;
V vert(A i){V o;float3 p=TransformObjectToWorld(i.positionOS.xyz);float weight=i.uv.y*i.uv.y;float wind=sin(p.x*.54+p.z*.23+_Time.y*1.25)+.35*sin(p.z*.83-_Time.y*1.8);p.x+=wind*.07*weight;p.z+=cos(p.x*.6+_Time.y)*.035*weight;o.worldX=p.x;o.pos=TransformWorldToHClip(p);o.color=i.color;o.uv=i.uv;o.fog=ComputeFogFactor(o.pos.z);return o;}
half4 frag(V i):SV_Target {clip(abs(i.worldX)-2.85);half4 tex=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);clip(tex.a-_Cutoff);half3 c=tex.rgb*_BaseColor.rgb*i.color.rgb*lerp(.59,1.1,saturate(i.uv.y*1.4));c=MixFog(c,i.fog);return half4(c,1);}
ENDHLSL
}
}}

