    Shader "No Interpolation Texture Array Test"
    {
        Properties
        {
            _BaseTextures ("Main Tex Array", 2DArray) = "white" {}
            _MainTex("Albedo (RGB)", 2D) = "white" {}
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
                    int index : TEXCOORD1;
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
                sampler2D _MainTex;
     
                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 c = tex2D(_MainTex, i.uv);
                    
                    return UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3(i.uv, c.rgb.r * 10));;
                }
                ENDCG
            }
        }
    }

