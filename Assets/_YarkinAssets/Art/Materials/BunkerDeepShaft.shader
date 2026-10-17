Shader "YarkinAssets/BunkerDeepShaft"
{
    Properties
    {
        _Color ("Surface Albedo Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _MainTex ("Swatch Atlas (RGB)", 2D) = "white" {}
        _AbyssColor ("Deep Abyss Color", Color) = (0.004, 0.008, 0.016, 1.0)
        _RimGlowColor ("Subterranean Rim Tint", Color) = (0.16, 0.24, 0.34, 1.0)
        _MouthHeight ("Well Mouth Local Y", Float) = 0.02
        _DarknessFalloff ("Depth Darkness Falloff", Float) = 1.15
        _AbyssFloorDepth ("Total Abyss Depth", Float) = 1.95
    }
    SubShader
    {
        // Renders right after BunkerPortalMask (2001) has cleared the depth buffer inside Stencil == 17
        Tags { "Queue"="Geometry+2" "RenderType"="Opaque" "DisableBatching"="True" }

        Pass
        {
            Name "DEEP_SHAFT_FORWARD"
            Tags { "LightMode"="ForwardBase" }
            ZTest LEqual
            ZWrite On
            Cull Back

            Stencil
            {
                Ref 17
                Comp Equal
                Pass Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _AbyssColor;
            fixed4 _RimGlowColor;
            float _MouthHeight;
            float _DarknessFalloff;
            float _AbyssFloorDepth;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 relWorldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float3 worldOrigin = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0)).xyz;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.relWorldPos = o.worldPos - worldOrigin;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 albedo = tex2D(_MainTex, i.uv).rgb * _Color.rgb;
                float3 N = normalize(i.worldNormal);
                float3 V = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);

                // Depth below well mouth (in world meters relative to WellShaft_Deep pivot at mouth)
                float depthBelowMouth = max(0.0, _MouthHeight - i.relWorldPos.y);

                // Overhead daylight/room light spilling into the circular mouth from above
                float3 lightDir = normalize(float3(0.25, 0.92, -0.28));
                float ndotl = saturate(dot(N, lightDir) * 0.55 + 0.45);

                // Crisp metallic highlight on ladder rungs and cast-iron shaft rib edges
                float3 H = normalize(lightDir + V);
                float spec = pow(saturate(dot(N, H)), 18.0) * 0.35;
                float grazingRim = pow(1.0 - saturate(dot(N, V)), 2.4) * 0.20;

                // Exponential depth attenuation into pitch-black underground well
                float depthAtten = exp(-depthBelowMouth * _DarknessFalloff);
                float abyssFade = saturate(1.0 - smoothstep(_AbyssFloorDepth * 0.62, _AbyssFloorDepth, depthBelowMouth));
                float visibility = depthAtten * abyssFade;

                // Subtle radial vignette near the center of the deep shaft
                float radialDist = length(i.relWorldPos.xz);
                float wallFactor = smoothstep(0.15, 0.56, radialDist);
                visibility *= lerp(0.80, 1.0, wallFactor);

                float3 litSurface = albedo * (ndotl * 1.18) + _RimGlowColor.rgb * (spec + grazingRim);
                float3 finalRgb = lerp(_AbyssColor.rgb, litSurface, saturate(visibility));

                return fixed4(finalRgb, 1.0);
            }
            ENDCG
        }
    }
    FallBack Off
}
