Shader "Custom/TerrainShader"
{
    Properties
    {
        // Biome textures and Biome splat map
        _BaseTextures("Terrain Textures", 2DArray) = "" {}
        _SplatMaps("Splat Maps", 2DArray) = "" {}
    }

        SubShader
    {
        Tags{ "RenderType" = "Opaque" "Queue" = "Geometry"}
        LOD 150

        Pass
        {
            Tags {"LightMode" = "ForwardBase"}

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            float4 _SplatMaps_ST;
            
            // Amount of current textures used for terrain: Update in case more texture are added to the terrain
            const static int layerCount = 12;
            const static int rgbaChannels = 4;

            float baseTextureScales[layerCount];

            UNITY_DECLARE_TEX2DARRAY(_BaseTextures);
            UNITY_DECLARE_TEX2DARRAY(_SplatMaps);

            struct VertexData
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Interpolators
            {
                float4 pos : SV_POSITION;
                float4 worldPos : POSITION1;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float2 uvSplat : TEXCOORD2;
                fixed3 diff : COLOR0;
                fixed3 ambient : COLOR1;
                SHADOW_COORDS(1)
            };

            Interpolators vert(VertexData v)
            {
                Interpolators i;
                i.pos = UnityObjectToClipPos(v.position);
                i.worldPos = v.position;
                i.uv = TRANSFORM_TEX(v.uv, _SplatMaps);
                i.uvSplat = v.uv;
                i.normal = v.normal;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                i.diff = nl * _LightColor0.rgb;
                i.ambient = ShadeSH9(half4(worldNormal, 1));
                // compute shadows data
                TRANSFER_SHADOW(i)

                return i;
            }


            float4 frag(Interpolators i) : SV_TARGET
            {

                float rgba[layerCount];
                
                // Map the various splatmaps RGBA channels to an array for
                for (int index = 0; index < layerCount / rgbaChannels; index++) {
                    float4 splat = UNITY_SAMPLE_TEX2DARRAY(_SplatMaps, float3(i.uvSplat, index));

                    rgba[0 + rgbaChannels * index] = splat.r;
                    rgba[1 + rgbaChannels * index] = splat.g;
                    rgba[2 + rgbaChannels * index] = splat.b;
                    rgba[3 + rgbaChannels * index] = splat.a;
                }

                float4 Albedo = float4(0, 0, 0, 0);

                half3 blendWeights = pow(abs(i.normal), 1);
                blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);

                // Loop through every texture in array and determine the draw power via the RGBA channels of the splatmapArray
                for (float index = 0; index < layerCount; index++) {

                    // Triplanar mapping to prevent textures from stretching across xyz surfaces
                    float4 AlbedoY = UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3 (i.worldPos.xz / baseTextureScales[index], index)) * rgba[index];
                    float4 AlbedoX = UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3 (i.worldPos.zy / baseTextureScales[index], index)) * rgba[index];
                    float4 AlbedoZ = UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3 (i.worldPos.xy / baseTextureScales[index], index)) * rgba[index];

                    Albedo = Albedo + AlbedoY * blendWeights.y + AlbedoX * blendWeights.x + AlbedoZ * blendWeights.z;
                }

                fixed shadow = SHADOW_ATTENUATION(i);
                fixed3 lighting = i.diff * shadow + i.ambient;

                Albedo.rgb *= lighting;

                return
                    Albedo;
            }

            ENDCG
        }
    }

            Fallback "VertexLit"
}
