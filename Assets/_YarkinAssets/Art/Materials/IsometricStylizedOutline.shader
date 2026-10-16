Shader "YarkinAssets/IsometricStylizedOutline"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _RimColor ("Isometric Rim Color", Color) = (0.85, 0.95, 1.0, 1.0)
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0
        _OutlineColor ("Interaction Outline Color", Color) = (1.0, 0.34, 0.13, 1.0)
        _OutlineWidth ("Outline Width (m)", Range(0.0, 0.03)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        // Pass 1: Inverted Hull Outline for Proximity / Hover Highlight
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            Cull Front
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _OutlineWidth;
            fixed4 _OutlineColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float3 norm = normalize(v.normal);
                float4 expanded = v.vertex + float4(norm * _OutlineWidth, 0.0);
                o.pos = UnityObjectToClipPos(expanded);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (_OutlineWidth <= 0.0001) discard;
                return _OutlineColor;
            }
            ENDCG
        }

        // Pass 2: Standard Surface with Crisp Isometric Rim Highlight
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _RimColor;
        half _RimPower;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            o.Emission = _RimColor.rgb * pow(rim, _RimPower) * 0.22;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Standard"
}
