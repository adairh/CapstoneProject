// Custom URP Lit Shader
Shader "Custom/GlowingShader"
{
    Properties
    {
        _BaseMap ("Albedo (RGB)", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _EmissionColor ("Emission Color", Color) = (0,0,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        
        Pass
        {
            Name "LitPass"
            Tags { "LightMode"="UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual
            Blend One Zero  // Ensures solid rendering

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float4 tangentWS : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            float4 _BaseColor;
            float _Metallic;
            float _Smoothness;
            float4 _EmissionColor;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;

                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.tangentWS = float4(TransformObjectToWorldDir(IN.tangentOS.xyz), IN.tangentOS.w);
                OUT.viewDirWS = normalize(GetCameraPositionWS() - TransformObjectToWorld(IN.positionOS.xyz));
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Sample base color and normal map
                float4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv));
                float3 normalWS = TransformTangentToWorld(normalTS, half3x3(IN.tangentWS.xyz, cross(IN.tangentWS.xyz, IN.normalWS), IN.normalWS));

                // Lighting calculations
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float NdotL = max(dot(normalWS, lightDir), 0.0);
                float3 diffuse = mainLight.color * albedo.rgb * NdotL;

                // Specular highlights (PBR-based)
                float3 viewDir = normalize(IN.viewDirWS);
                float3 halfDir = normalize(viewDir + lightDir);
                float NdotH = max(dot(normalWS, halfDir), 0.0);
                float specular = pow(NdotH, _Smoothness * 128.0) * _Metallic;
                float3 specularColor = mainLight.color * specular;

                // Ambient lighting (to prevent objects from going completely dark)
                float3 ambient = float3(0.1, 0.1, 0.1) * albedo.rgb;

                // Final color output
                float3 finalColor = diffuse + specularColor + ambient + _EmissionColor.rgb;
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
