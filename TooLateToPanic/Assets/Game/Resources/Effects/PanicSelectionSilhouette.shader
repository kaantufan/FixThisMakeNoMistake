Shader "Panic/SelectionSilhouette"
{
    Properties
    {
        _MainTex ("Source alpha / selected mask", 2D) = "white" {}
        _Color ("Source tint", Color) = (1,1,1,1)
        _Cutoff ("Alpha cutoff", Float) = .001
        _Cull ("Cull", Float) = 2
        _OutlineColor ("Silhouette", Color) = (1,.48,.16,1)
    }
    SubShader
    {
        // Actual geometry mask; no bounds, extrusion, cloned renderer, or stencil ownership.
        Pass
        {
            Cull [_Cull] ZWrite On ZTest LEqual Blend Off
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex MaskVert
            #pragma fragment MaskFrag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST, _Color;
            float _Cutoff;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            struct MaskInput { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct MaskOutput { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float4 screen : TEXCOORD1; float eye : TEXCOORD2; };
            float EyeDepth(float raw)
            {
                // LinearEyeDepth alone assumes perspective; preparation also uses orthographic.
                if (unity_OrthoParams.w > .5)
                {
                    #if defined(UNITY_REVERSED_Z)
                    raw = 1 - raw;
                    #endif
                    return lerp(_ProjectionParams.y, _ProjectionParams.z, raw);
                }
                return LinearEyeDepth(raw);
            }
            MaskOutput MaskVert(MaskInput input)
            {
                MaskOutput output;
                output.pos = UnityObjectToClipPos(input.vertex);
                output.screen = ComputeScreenPos(output.pos);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.eye = -UnityObjectToViewPos(input.vertex).z;
                return output;
            }
            float MaskFrag(MaskOutput input) : SV_Target
            {
                clip(tex2D(_MainTex, input.uv).a * _Color.a - _Cutoff);
                float scene = EyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(input.screen)));
                clip(scene + max(.002, input.eye * .0001) - input.eye);
                return input.eye;
            }
            ENDCG
        }
        Pass
        {
            Cull Off ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment OutlineFrag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize, _OutlineColor;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float EyeDepth(float raw)
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
            float4 OutlineFrag(v2f_img input) : SV_Target
            {
                // A visible surface wash makes the selected asset itself read as selected.
                if (tex2D(_MainTex, input.uv).r > 0) return float4(_OutlineColor.rgb, .20 * _OutlineColor.a);
                float2 depthUV = input.uv;
                #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0) depthUV.y = 1 - depthUV.y;
                #endif
                float scene = EyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, depthUV));
                float edge = 0;
                float2 pixel = abs(_MainTex_TexelSize.xy);
                [unroll] for (int x = -2; x <= 2; x++)
                [unroll] for (int y = -2; y <= 2; y++)
                {
                    float radius = length(float2(x,y));
                    if (radius == 0 || radius > 2.1) continue;
                    float2 uv = input.uv + float2(x,y) * pixel;
                    if (any(uv < 0) || any(uv > 1)) continue;
                    float source = tex2D(_MainTex, uv).r;
                    // Reject any edge that would be painted onto a nearer occluder.
                    if (source > 0 && source <= scene + max(.002, source * .0001))
                        edge = max(edge, radius <= 1.5 ? 1 : .45);
                }
                return float4(_OutlineColor.rgb, _OutlineColor.a * edge);
            }
            ENDCG
        }
        // Pickup-only reflection cue. Actual triangles clip every glint pixel, with no sprites,
        // lights, trails, beam, selection box, floor marker, or original material mutation.
        Pass
        {
            Cull [_Cull] ZWrite Off ZTest LEqual Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex GatherVert
            #pragma fragment GatherFrag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST, _Color;
            float _Cutoff;
            float4 _PanicGatherScreen;
            float _PanicGatherPhase;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            struct GatherInput { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct GatherOutput { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float4 screen : TEXCOORD1; float eye : TEXCOORD2; float3 world : TEXCOORD3; };
            float GatherEyeDepth(float raw)
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
            GatherOutput GatherVert(GatherInput input)
            {
                GatherOutput output;
                output.pos = UnityObjectToClipPos(input.vertex);
                output.screen = ComputeScreenPos(output.pos);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.eye = -UnityObjectToViewPos(input.vertex).z;
                output.world = mul(unity_ObjectToWorld, input.vertex).xyz;
                return output;
            }
            float4 GatherFrag(GatherOutput input) : SV_Target
            {
                clip(tex2D(_MainTex, input.uv).a * _Color.a - _Cutoff);
                float scene = GatherEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(input.screen)));
                clip(scene + max(.002, input.eye * .0001) - input.eye);
                float pulse = pow(max(0, sin(_Time.y * 1.75 + _PanicGatherPhase)), 12);
                float2 screen = input.screen.xy / input.screen.w;
                float2 delta = (screen - _PanicGatherScreen.xy) * _ScreenParams.xy;
                // Local 2 px core with short soft rays, clipped to visible pickup surfaces.
                float core = exp(-dot(delta, delta) * .3);
                float star = exp(-abs(delta.x) * 2 - abs(delta.y) * .45) + exp(-abs(delta.y) * 2 - abs(delta.x) * .45);
                float stripe = abs(frac(dot(input.world, float3(.83, 2.13, .37)) - _Time.y * .38 + _PanicGatherPhase) - .5);
                float sheen = 1 - smoothstep(.008, .045, stripe);
                float alpha = saturate(pulse * (.018 + sheen * .22 + saturate(core + star) * .70));
                return float4(1, .96, .78, alpha);
            }
            ENDCG
        }
    }
    Fallback Off
}
