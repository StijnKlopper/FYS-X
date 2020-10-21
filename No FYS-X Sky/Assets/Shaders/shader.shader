Shader "Custom/Blend"
{
    Properties {
        _BaseTextures ("Terrain Textures", 2DArray) = "" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 600

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 4.0
        const static int maxLayerCount = 10;
        const static float epsilon = 1E-4;


        float3 baseColours[maxLayerCount];
        float baseStartHeights[maxLayerCount];
        float baseBlends[maxLayerCount];
        float baseColourStrength[maxLayerCount];
        float baseTextureScales[maxLayerCount];

        UNITY_DECLARE_TEX2DARRAY(_BaseTextures);

        struct Input
        {
            float2 uv_BaseTextures;
            float3 worldPos;
            float3 worldNormal;
        };

        // use triplanar mapping to prevent texture stretching across various surface
        float3 triplanar(float3 worldPos, float scale, float3 blendAxes, float index) {
            float3 scaledWorldPos = worldPos / scale;

            float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, index)) * blendAxes.x;
            float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, index)) * blendAxes.y;
            float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, index)) * blendAxes.z;
            return xProjection + yProjection + zProjection;
        }

        // display texture based on index in a texture array that is stored via a vertice value.
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            int i = IN.uv_BaseTextures.y;
            float3 blendAxes = abs(IN.worldNormal);

            float drawStrength = 1;
            float3 baseColour = baseColours[i] * baseColourStrength[i];
            float3 textureColour = triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1 - baseColourStrength[i]);

            o.Albedo = o.Albedo * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
        }

        ENDCG
    }
    FallBack "Diffuse"
}