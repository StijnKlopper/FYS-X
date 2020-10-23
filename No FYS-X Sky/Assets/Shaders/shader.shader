Shader "Custom/Blend"
{
    Properties {
        _BaseTextures ("Terrain Textures", 2DArray) = "" {}
        _MainTex("Albedo (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 600

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert

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
        sampler2D _MainTex;

        struct Input
        {
            float2 uv_BaseTextures;
            float3 worldPos;
            float3 worldNormal;
            float4 emissionColor;
            float3 zIndex;
        };

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 zIndex = v.vertex;
            //o.uv.xy = (v.vertex.xy + 0.5) * 1;
        }

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
            int b = 1 / IN.uv_BaseTextures.x;
            float3 blendAxes = abs(IN.worldNormal);

            float drawStrength = 1;
            float3 baseColour = baseColours[i] * baseColourStrength[i];
            float3 textureColour = triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i * b) * (1 - baseColourStrength[i]);

            o.Albedo = o.Albedo * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
        }

        ENDCG
    }
    FallBack "Diffuse"
}