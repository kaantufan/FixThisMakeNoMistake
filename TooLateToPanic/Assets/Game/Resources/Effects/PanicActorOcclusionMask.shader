Shader "Panic/ActorOcclusionMask"
{
 SubShader {
  Tags { "Queue"="Transparent+19" "RenderType"="Transparent" }
  Pass {
   ZWrite Off ZTest LEqual Cull Back ColorMask 0
   Stencil { Ref 64 ReadMask 64 WriteMask 64 Comp Always Pass Replace }
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   float4 vert(float4 vertex:POSITION):SV_POSITION { return UnityObjectToClipPos(vertex); }
   fixed4 frag():SV_Target { return 0; }
   ENDCG
  }
 }
 Fallback Off
}
