Shader "NoDestination/ClearSummerSky" {
Properties { _Zenith("Zenith",Color)=(0.04,0.32,0.76,1) _Horizon("Horizon",Color)=(0.53,0.79,0.92,1) }
SubShader { Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"} Cull Off ZWrite Off
Pass { HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct v2f { float4 pos:SV_POSITION; float3 dir:TEXCOORD0;};float4 _Zenith,_Horizon;float _HorizonFracture,_RealityBlend;
v2f vert(float4 p:POSITION){v2f o;o.pos=UnityObjectToClipPos(p);o.dir=p.xyz;return o;}
fixed4 frag(v2f i):SV_Target{float3 dir=normalize(i.dir);float offset=sin(dir.x*17+dir.z*9)*.024*_HorizonFracture*exp(-abs(dir.y)*22);float h=saturate(dir.y+offset);float4 c=lerp(_Horizon,_Zenith,pow(h,.45));c.rgb-=exp(-abs(dir.y+offset)*1400)*.02*_RealityBlend;return c;}
ENDHLSL }
}}
