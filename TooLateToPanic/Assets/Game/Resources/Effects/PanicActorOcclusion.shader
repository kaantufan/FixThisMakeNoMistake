Shader "Panic/ActorOcclusion"
{
 Properties { _Color ("Occluded actor cue", Color) = (1,.73,.27,.66) _MinimumY ("Ground contact exclusion", Float) = 0 }
 SubShader {
  Tags { "Queue"="Transparent+20" "RenderType"="Transparent" }
  Pass {
   Name "CUE"
   ZWrite Off ZTest Greater Cull Back
   Stencil { Ref 64 ReadMask 64 WriteMask 0 Comp NotEqual Pass Keep }
   Blend SrcAlpha OneMinusSrcAlpha
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   fixed4 _Color;float _MinimumY;
   struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; };
   struct v2f { float4 position:SV_POSITION;float worldY:TEXCOORD0; };
   v2f vert(appdata v) { v2f o;o.position=UnityObjectToClipPos(v.vertex);o.worldY=mul(unity_ObjectToWorld,v.vertex).y;return o; }
   fixed4 frag(v2f i):SV_Target { clip(i.worldY-_MinimumY);return _Color; }
   ENDCG
  }
 }
 Fallback Off
}
