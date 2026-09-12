Shader "NoDestination/ClearSummerSky" {
Properties { _Zenith("Zenith",Color)=(0.04,0.32,0.76,1) _Horizon("Horizon",Color)=(0.53,0.79,0.92,1) }
SubShader { Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"} Cull Off ZWrite Off
Pass { HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct v2f { float4 pos:SV_POSITION; float3 dir:TEXCOORD0;};float4 _Zenith,_Horizon;
v2f vert(float4 p:POSITION){v2f o;o.pos=UnityObjectToClipPos(p);o.dir=p.xyz;return o;}
fixed4 frag(v2f i):SV_Target{float h=saturate(normalize(i.dir).y);return lerp(_Horizon,_Zenith,pow(h,.45));}
ENDHLSL }
}}
