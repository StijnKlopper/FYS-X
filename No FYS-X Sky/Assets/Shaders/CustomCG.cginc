sampler2D _MainTex;
float4 _MainTex_ST;

sampler2D _NormalTex;
float4 _NormalTex_ST;

float _HeightMapScale;
sampler2D _HeightTex;
float4 _HeightTex_ST;
sampler2D _HeightBumpTex;

int _TessellationFactor;
float _TessllationDistance;

float3 _TriPlanarSize;

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
float3 Vec3TsToWs(float3 vVectorTs, float3 vNormalWs, float3 vTangentUWs, float3 vTangentVWs)
{
	float3x3 TBNMatrix = float3x3(vTangentUWs, vTangentVWs, vNormalWs);

	float3 result = mul(vVectorTs, TBNMatrix);
	return result; // Return without normalizing
}

float3 Vec3TsToWsNormalized(float3 vVectorTs, float3 vNormalWs, float3 vTangentUWs, float3 vTangentVWs)
{
	return normalize(Vec3TsToWs(vVectorTs.xyz, vNormalWs.xyz, vTangentUWs.xyz, vTangentVWs.xyz));
}

float4 SimpleDirectionalLight(float3 vNormal)
{
	float3 lightDir = -_WorldSpaceLightPos0.xyz;
	float NDotL = dot(lightDir, vNormal);

	if (NDotL <= 0.0f)
		return 0.0f;

	return NDotL * _LightColor0;
}

float3 Displacement(float3 vNormal, float2 uv, float heightScale)
{
	float4 heightData = tex2Dlod(_HeightTex, float4(uv, 0, 0));
	heightData.xyz = normalize(heightData) * vNormal * heightScale;

	return heightData;
}

float3 GetTriPlanarBlend(float3 _wNorm)
{
	// in wNorm is the world-space normal of the fragment
	float3 blending = abs(_wNorm);
	blending = (blending - 0.2) * 7;
	blending = normalize(max(blending, 0.00001)); // Force weights to sum to 1.0
	float b = (blending.x + blending.y + blending.z);
	blending /= float3(b, b, b);
	return blending;
}

float4 SampleTriplanarTexture(sampler2D tex, float3 worldPos, float3 blending)
{
	float4 xaxis = tex2D(tex, worldPos.yz).rgba;
	float4 yaxis = tex2D(tex, worldPos.xz).rgba;
	float4 zaxis = tex2D(tex, worldPos.xy).rgba;
	float4 final = xaxis * blending.x + yaxis * blending.y + zaxis * blending.z;

	return final;
}