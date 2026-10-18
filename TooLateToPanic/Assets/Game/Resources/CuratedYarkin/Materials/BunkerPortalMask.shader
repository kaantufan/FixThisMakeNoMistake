Shader "YarkinAssets/BunkerPortalMask"
{
    Properties
    {
        _AbyssColor ("Abyss Base Color", Color) = (0.004, 0.008, 0.016, 1.0)
    }
    SubShader
    {
        // Renders immediately after opaque geometry (Floor at 2000, Bunker Collar/Lid at 2000)
        Tags { "Queue"="Geometry+1" "RenderType"="Opaque" "DisableBatching"="True" }

        // PASS 1: Stamp Stencil = 17 wherever the well mouth disc is visible (not occluded by closed lid or front collar, but in front of y=0 floor)
        Pass
        {
            Name "PORTAL_STENCIL_STAMP"
            ColorMask 0
            ZWrite Off
            ZTest LEqual
            Cull Back

            Stencil
            {
                Ref 17
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }

        // PASS 2: Clear hardware depth buffer to Far Clip Plane inside Stencil == 17 so underground shaft (y < 0) renders through solid y=0 floors
        Pass
        {
            Name "PORTAL_DEPTH_CLEAR"
            ZTest Always
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

            fixed4 _AbyssColor;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            struct fragOut
            {
                fixed4 color : SV_Target;
                float depth : SV_Depth;
            };

            fragOut frag(v2f i)
            {
                fragOut o;
                o.color = _AbyssColor;
                #if defined(UNITY_REVERSED_Z)
                o.depth = 0.00001f;
                #else
                o.depth = 0.99999f;
                #endif
                return o;
            }
            ENDCG
        }
    }
    FallBack Off
}
