    Shader "No Interpolation Texture Array Test"
    {
        Properties
        {
            _BaseTextures ("Main Tex Array", 2DArray) = "white" {}
        }
        SubShader
        {
            Pass
            {
                Tags {}
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
     
                struct appdata {
                    float4 vertex : POSITION;
                    float3 uv : TEXCOORD0;
                };
     
                struct v2f {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    nointerpolation int index : TEXCOORD1;
                };
     
                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv.xy;
                    o.index = v.uv.y;
                    return o;
                }
     
                UNITY_DECLARE_TEX2DARRAY(_BaseTextures);
     
                fixed4 frag(v2f i) : SV_Target
                {
                    return UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3(1, 1, i.index + 1));
                }
                ENDCG
            }
        }
    }

