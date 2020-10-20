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

        const static int maxLayerCount = 8;
        const static float epsilon = 1E-4;

        int layerCount;
        float3 baseColours[maxLayerCount];
        float baseStartHeights[maxLayerCount];
        float baseBlends[maxLayerCount];
        float baseColourStrength[maxLayerCount];
        float baseTextureScales[maxLayerCount];

        float minHeight;
        float maxHeight;

        float4 regions[20];
        float4 biomes[20];

        float textureIndexByBiome[11 * 11];

        sampler2D map;


        UNITY_DECLARE_TEX2DARRAY(_BaseTextures);

        struct Input
        {
            float2 uv_BaseTextures;
            float3 worldPos;
            float3 worldNormal;
            float4 color;
        };

        float inverseLerp(float a, float b, float value) {
            return saturate((value-a)/(b-a));
        }

        float3 triplanar(float3 worldPos, float scale, float3 blendAxes, float index) {
                float3 scaledWorldPos = worldPos / scale;

                float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, index)) * blendAxes.x ;
			    float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, index)) * blendAxes.y ;
			    float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, index)) * blendAxes.z ;
                return xProjection + yProjection + zProjection;
            
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
            float3 blendAxes = abs(IN.worldNormal);
            blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

            int i = IN.uv_BaseTextures.xy;

            float drawStrength = 1 ;
            float3 baseColour = baseColours[IN.uv_BaseTextures.y] * baseColourStrength[IN.uv_BaseTextures.y];
            float3 textureColour = triplanar(IN.worldPos, baseTextureScales[IN.uv_BaseTextures.y], blendAxes, i) * (1-baseColourStrength[IN.uv_BaseTextures.y]);

            o.Albedo = o.Albedo * (1-drawStrength) + (baseColour + textureColour) * drawStrength;

        }

        ENDCG
    }
    FallBack "Diffuse"
}