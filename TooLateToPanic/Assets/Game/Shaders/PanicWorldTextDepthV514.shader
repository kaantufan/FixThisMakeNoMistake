Shader "Panic/WorldTextDepthV514"
{
    Properties
    {
        _MainTex ("Font alpha atlas", 2D) = "white" {}
        _Color ("Text tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        ZTest LEqual
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // LegacyRuntime's atlas can have black RGB. Only its glyph coverage
                // belongs in the output; vertex/material tint owns visible lettering.
                fixed coverage = tex2D(_MainTex, i.uv).a;
                return fixed4(i.color.rgb, i.color.a * coverage);
            }
            ENDCG
        }
    }
    Fallback Off
}
