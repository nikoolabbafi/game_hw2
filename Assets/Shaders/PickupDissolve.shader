Shader "NightDrive/PickupDissolve"
{
    Properties
    {
        _BaseMap ("Original Texture", 2D) = "white" {}
        _BaseColor ("Original Color", Color) = (1,1,1,1)
        [HDR] _EdgeColor ("Glowing Edge", Color) = (4,2,0.1,1)
        _Dissolve ("Dissolve", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 localPos : TEXCOORD1; };
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _EdgeColor;
                float _Dissolve;
            CBUFFER_END
            float Hash(float3 p) { return frac(sin(dot(p,float3(12.9898,78.233,37.719)))*43758.5453); }
            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = TRANSFORM_TEX(i.uv,_BaseMap);
                o.localPos = i.positionOS.xyz;
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                float noise = Hash(floor(i.localPos * 38.0));
                clip(noise - _Dissolve);
                half4 color = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv) * _BaseColor;
                half edge = 1.0-smoothstep(_Dissolve,_Dissolve+0.13,noise);
                color.rgb += edge * _EdgeColor.rgb;
                color.a *= 1.0-_Dissolve;
                return color;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
