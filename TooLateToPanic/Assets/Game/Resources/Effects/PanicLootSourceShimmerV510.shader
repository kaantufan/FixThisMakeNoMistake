Shader "Panic/LootSourceShimmerV510"
{
    Properties
    {
        _MainTex ("Source alpha", 2D) = "white" {}
        _Color ("Source tint", Color) = (1,1,1,1)
        _Cutoff ("Alpha cutoff", Float) = .001
        _Cull ("Cull", Float) = 2
    }
    SubShader
    {
        // Every pixel is on the existing source's triangles. No sprites, shells, lights or screen markers.
        Pass
        {
            Cull [_Cull] ZWrite Off ZTest LEqual Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex ShimmerVert
            #pragma fragment ShimmerFrag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST, _Color;
            float _Cutoff, _LootShimmerTimeV510, _LootShimmerReducedV510;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            struct ShimmerInput { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct ShimmerOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screen : TEXCOORD1;
                float eye : TEXCOORD2;
                float3 objectPosition : TEXCOORD3;
                float3 worldNormal : TEXCOORD4;
                float3 world : TEXCOORD5;
            };
            float ShimmerEyeDepth(float raw)
            {
                if (unity_OrthoParams.w > .5)
                {
                    #if defined(UNITY_REVERSED_Z)
                    raw = 1 - raw;
                    #endif
                    return lerp(_ProjectionParams.y, _ProjectionParams.z, raw);
                }
                return LinearEyeDepth(raw);
            }
            ShimmerOutput ShimmerVert(ShimmerInput input)
            {
                ShimmerOutput output;
                output.pos = UnityObjectToClipPos(input.vertex);
                output.screen = ComputeScreenPos(output.pos);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.eye = -UnityObjectToViewPos(input.vertex).z;
                output.objectPosition = input.vertex.xyz;
                output.worldNormal = UnityObjectToWorldNormal(input.normal);
                output.world = mul(unity_ObjectToWorld, input.vertex).xyz;
                return output;
            }
            float4 ShimmerFrag(ShimmerOutput input) : SV_Target
            {
                clip(tex2D(_MainTex, input.uv).a * _Color.a - _Cutoff);
                float scene = ShimmerEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(input.screen)));
                // Explicit camera depth rejection also protects against an incompatible target attachment.
                clip(scene + max(.002, input.eye * .0001) - input.eye);
                float3 viewDirection = unity_OrthoParams.w > .5 ? UNITY_MATRIX_V[2].xyz : normalize(_WorldSpaceCameraPos - input.world);
                float rim = pow(1 - saturate(abs(dot(normalize(input.worldNormal), viewDirection))), 2);
                float coordinate = dot(input.objectPosition, float3(.41, .78, .23));
                float stripe = abs(frac(coordinate - _LootShimmerTimeV510 * .16) - .5);
                float sheen = 1 - smoothstep(.012, .09, stripe);
                float pulse = .5 + .5 * sin(_LootShimmerTimeV510 * 1.6);
                // A persistent gentle wash never blinks out. Reduced motion is fully time-independent.
                float alpha = _LootShimmerReducedV510 > .5 ? .085 + rim * .07 : .055 + rim * .055 + sheen * .09 + pulse * .015;
                return float4(1, .79, .42, alpha);
            }
            ENDCG
        }
    }
    Fallback Off
}
