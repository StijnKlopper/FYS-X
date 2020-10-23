Shader "Custom/Biome Tilemap"
{
    Properties
    {
        // Biome splat map for texturing
        _SplatMap1("_SplatMap1", 2D) = "white" {}
        _SplatMap2("_SplatMap2", 2D) = "white" {}
        _SplatMap3("_SplatMap3", 2D) = "white" {}
        _BaseTextures("Terrain Textures", 2DArray) = "" {}

    // Want to use this 2DArray that will have 16 textures...
    //_TerrainTextures("Terrain Textures", 2DArray) = "white" {}


    // As opposed to 4 specific textures
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
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

        // compile shader into multiple variants, with and without shadows
        // (we don't care about any lightmaps yet, so skip these variants)
        #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
        // shadow helper functions and macros
        #include "AutoLight.cginc"

            sampler2D _SplatMap1;
            sampler2D _SplatMap2;
            sampler2D _SplatMap3;
            float4 _SplatMap1_ST;

            UNITY_DECLARE_TEX2DARRAY(_BaseTextures);

            struct VertexData
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            

            struct Interpolators
            {
                float4 pos : SV_POSITION;
                SHADOW_COORDS(1)
                fixed3 diff : COLOR0;
                fixed3 ambient : COLOR1;
                float2 uv : TEXCOORD0;
                float2 uvSplat : TEXCOORD2;
                
            };

            Interpolators vert(VertexData v)
            {
                Interpolators i;
                i.pos = UnityObjectToClipPos(v.position);
                i.uv = TRANSFORM_TEX(v.uv, _SplatMap1);
                i.uvSplat = v.uv;

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
                float4 splat = tex2D(_SplatMap1, i.uvSplat);
                float4 splat2 = tex2D(_SplatMap2, i.uvSplat);
                float4 splat3 = tex2D(_SplatMap3, i.uvSplat);
                float rgba[12];

                rgba[0] = splat.r;
                rgba[1] = splat.g;
                rgba[2] = splat.b;
                rgba[3] = splat.a;

                rgba[4] = splat2.r;
                rgba[5] = splat2.g;
                rgba[6] = splat2.b;
                rgba[7] = splat2.a;

                rgba[8]  = splat3.r;
                rgba[9]  = splat3.g;
                rgba[10] = splat3.b;
                rgba[11] = splat3.a;

                float4 Albedo = float4(0, 0, 0, 0);

                for (float index = 0; index < 12; index++) {
                    Albedo = Albedo + UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3 (i.uv, index)) * rgba[index];
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
