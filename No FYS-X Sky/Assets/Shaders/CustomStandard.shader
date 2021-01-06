Shader "Custom/CustomStandard" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalTex ("Normal Map", 2D) = "bump" {}
		_HeightMapScale ("Height Map Scale", Float) = 1
		_HeightTex ("Height Map", 2D) = "black" {}
		[NoScaleOffset] _HeightBumpTex("Height Normal Map", 2D) = "bump" {}
		_TessellationFactor("Tessellation Factor", Range(1, 64)) = 1
		_TessllationDistance("Tessellation Fade Distance", Float) = 100
		[Toggle(S_TRI_PLANAR)] _UseTriPlanar("Use Tri Planar Mapping", Int) = 0
		_TriPlanarSize("Tri Planar Size", Vector) = (1.0, 1.0, 1.0, 0.0)
	}
	SubShader {
		Pass{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#pragma target 5.0
			#pragma vertex MainVs
			#pragma hull MainHs
			#pragma domain MainDs
			#pragma fragment MainPs
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "/CustomCG.cginc"
			#pragma shader_feature S_TRI_PLANAR
			
			struct VertexIn
			{
				float3 PosL : POSITION;
				float2 TexC : TEXCOORD;
				float3 NormalL : NORMAL;
				float4 TangentL : TANGENT;
			};

			struct VertexOut
			{
				float3 PosL : TESSPOS;
				float2 TexC : TEXCOORD;
				float3 NormalL : NORMAL;
				float4 TangentL : TANGENT;
			};

			struct DomainOut
			{
				float4 PosH : SV_POSITION;
				float3 PosL : POSITION1;
				float2 TexAlbedo : TEXCOORD0;
				float2 TexBump : TEXCOORD1;
				float2 TexHeight : TEXCOORD2;
				float3 NormalW : NORMAL;
				float4 TangentW : TANGENT;
				float3 BinormalW : BINORMAL;
			};

			// ------------------- Main Vertex Shader ------------------- //
			VertexOut MainVs(VertexIn i)
			{
				VertexOut o = (VertexOut)i;

				return o;
			}

			// ------------------- Main Hull Shader ------------------- //
			struct PatchTess
			{
				float EdgeTess[3]   : SV_TessFactor;
				float InsideTess : SV_InsideTessFactor;
			};

			PatchTess ConstantHS(InputPatch<VertexOut, 3> patch, uint patchID : SV_PrimitiveID)
			{
				// tessellation factor calc
				float4 centerW = mul(unity_ObjectToWorld, float4(0, 0, 0, 1.0f));
				float distance = length(centerW.xyz - _WorldSpaceCameraPos.xyz);

				PatchTess pt;
				// multiply tessellation factor by distance between object position and camera
				// this is for dynamic LOD.
				pt.EdgeTess[0] = clamp(_TessellationFactor * (_TessllationDistance / distance), 1.0f, _TessellationFactor);
				pt.EdgeTess[1] = clamp(_TessellationFactor * (_TessllationDistance / distance), 1.0f, _TessellationFactor);
				pt.EdgeTess[2] = clamp(_TessellationFactor * (_TessllationDistance / distance), 1.0f, _TessellationFactor);
				pt.InsideTess = clamp(_TessellationFactor * (_TessllationDistance / distance), 1.0f, _TessellationFactor);

				return pt;
			}

			// hull shader
			[domain("tri")]
			[partitioning("integer")]
			[outputtopology("triangle_cw")]
			[outputcontrolpoints(3)]
			[patchconstantfunc("ConstantHS")]
			VertexOut MainHs(InputPatch<VertexOut, 3> p,
				uint i : SV_OutputControlPointID)
			{
				return p[i];
			}

			// ------------------- Main Domain Shader ------------------- //
			[domain("tri")]
			DomainOut MainDs(PatchTess patchTess,
				float3 domainLocation : SV_DomainLocation,
				const OutputPatch<VertexOut, 3> tri)
			{
				// interpolation tessllation data
				VertexOut i = (VertexOut)0;
				i.PosL = tri[0].PosL * domainLocation.x + tri[1].PosL * domainLocation.y + tri[2].PosL * domainLocation.z;
				i.TexC = tri[0].TexC * domainLocation.x + tri[1].TexC * domainLocation.y + tri[2].TexC * domainLocation.z;
				i.NormalL = tri[0].NormalL * domainLocation.x + tri[1].NormalL * domainLocation.y + tri[2].NormalL * domainLocation.z;
				i.TangentL = tri[0].TangentL * domainLocation.x + tri[1].TangentL * domainLocation.y + tri[2].TangentL * domainLocation.z;

				// call displacement
				i.PosL += Displacement(i.NormalL, TRANSFORM_TEX(i.TexC, _HeightTex), _HeightMapScale);

				// data transfer
				DomainOut o = (DomainOut)0;
				o.PosH = UnityObjectToClipPos(i.PosL);
				o.PosL = i.PosL;
				o.TexAlbedo = TRANSFORM_TEX(i.TexC, _MainTex);
				o.TexBump = TRANSFORM_TEX(i.TexC, _NormalTex);
				o.TexHeight = TRANSFORM_TEX(i.TexC, _HeightTex);
				o.NormalW = UnityObjectToWorldNormal(i.NormalL);
				o.TangentW = float4(UnityObjectToWorldDir(i.TangentL.xyz), i.TangentL.w);
				o.BinormalW = cross(o.NormalW, o.TangentW) * i.TangentL.w;

				// since we displacement the geometry, we need to adjust normal
				float3 normalHeight = UnpackNormal(tex2Dlod(_HeightBumpTex, float4(o.TexHeight, 0, 0))).xyz;
				normalHeight = Vec3TsToWsNormalized(normalHeight, o.NormalW, o.TangentW, o.BinormalW);
				o.TangentW.xyz = normalize(o.TangentW.xyz - (normalHeight* dot(o.TangentW.xyz, normalHeight)));
				o.BinormalW = cross(normalHeight, o.TangentW) * o.TangentW.w;
				o.NormalW = normalHeight;

				return o;
			}

			// ------------------- Main Pixel(Fragment) Shader ------------------- //
			float4 MainPs(DomainOut i) : SV_Target
			{
				#if(S_TRI_PLANAR)
					// get tri planar mapping blend factor
					float3 triPlanarBlend = GetTriPlanarBlend(i.NormalW);
				#endif

				// sample albedo
				float4 albedo = (float4)0;
				#if(S_TRI_PLANAR)
					albedo = SampleTriplanarTexture(_MainTex, i.PosL * _TriPlanarSize, triPlanarBlend);
				#else
					albedo = tex2D(_MainTex, i.TexAlbedo);
				#endif

				// doing bump mapping by using new TBN vectors
				float3 normalW = (float3)0;
				#if(S_TRI_PLANAR)
					normalW = UnpackNormal(SampleTriplanarTexture(_NormalTex, i.PosL * _TriPlanarSize, triPlanarBlend)).xyz;
				#else
					normalW = UnpackNormal(tex2D(_NormalTex, i.TexBump)).xyz;
				#endif
				normalW = Vec3TsToWsNormalized(normalW, i.NormalW, i.TangentW, i.BinormalW);

				// simple light calc
				float4 ambient = float4(0.75, 0.75, 0.75, 0.75);
				float4 lit = SimpleDirectionalLight(normalW) + ambient;
				lit.a = 1.0f;

				return albedo * lit;
			}
			
			ENDCG
		}
	}
	FallBack "Diffuse"
}
