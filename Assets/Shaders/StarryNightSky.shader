Shader "NightDrive/StarryNightSky"
{
    Properties
    {
        _ZenithColor ("Zenith", Color) = (0.015, 0.024, 0.075, 1)
        _HorizonColor ("Horizon", Color) = (0.065, 0.09, 0.18, 1)
        [HDR] _StarColor ("Stars", Color) = (1.4, 1.7, 2.0, 1)
        _MoonDirection ("Moon Direction", Vector) = (0.48, 0.22, 0.85, 0)
        _MoonSize ("Moon Angular Radius", Range(0.005,0.08)) = 0.025
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 vertex : SV_POSITION; float3 direction : TEXCOORD0; };
            float4 _ZenithColor, _HorizonColor, _StarColor, _MoonDirection;
            float _MoonSize;
            float Hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.direction = v.vertex.xyz;
                return o;
            }
            half4 frag(v2f i) : SV_Target
            {
                float3 d = normalize(i.direction);
                float horizon = pow(saturate(1.0 - max(d.y,0.0)), 4.0);
                float3 color = lerp(_ZenithColor.rgb, _HorizonColor.rgb, horizon);
                float2 uv = float2(atan2(d.x,d.z)/6.2831853+0.5, asin(clamp(d.y,-1.0,1.0))/3.14159265+0.5);
                float2 grid = uv * float2(700.0,350.0);
                float2 cell = floor(grid);
                float2 offset = float2(Hash(cell+17.0),Hash(cell+43.0))*0.6+0.2;
                float distanceToStar = length(frac(grid)-offset);
                float aa = max(fwidth(distanceToStar),0.015);
                float stars = (1.0-smoothstep(0.06,0.06+aa,distanceToStar))*step(0.996,Hash(cell));
                stars *= smoothstep(0.01,0.18,d.y);
                color += stars * _StarColor.rgb;
                float angle = acos(clamp(dot(d,normalize(_MoonDirection.xyz)),-1.0,1.0));
                float moon = 1.0-smoothstep(_MoonSize*0.9,_MoonSize,angle);
                color += moon*float3(1.5,1.75,2.0) + exp(-angle*28.0)*float3(0.035,0.05,0.09);
                color *= lerp(0.15,1.0,smoothstep(-0.2,0.0,d.y));
                return half4(color,1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}
