// Tessellation programs based on this article by Catlike Coding:
// https://catlikecoding.com/unity/tutorials/advanced-rendering/tessellation/

struct vertexInput
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float  TessFactor : TESS;
	float4 texCoord : TEXCOORD0;
	float hasGrass: HASGRASSS;
};

struct vertexInput0
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float4 texCoord : TEXCOORD0;
};

struct vertexOutput
{
	float4 vertex : SV_POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
};

struct TessellationFactors 
{
	float edge[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};



vertexInput vert(vertexInput0 v)
{
	
	vertexInput vi;
	float dist = distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex)) / (32 * 5);
	vi.vertex = v.vertex;
	vi.normal = v.normal;
	vi.tangent = v.tangent;
	float tf = (int)(lerp(5.2, 1.2, clamp(dist, 0.0, 1)));
	vi.hasGrass = tex2Dlod(_GrassSplatMap, float4(v.texCoord.xy, 0.0, 0.0)).r;
	vi.TessFactor = tf;
	return vi;
}

vertexOutput tessVert(vertexInput v)
{
	vertexOutput o;
	// Note that the vertex is NOT transformed to clip
	// space here; this is done in the grass geometry shader.
	o.vertex = v.vertex;
	o.normal = v.normal;
	o.tangent = v.tangent;
	return o;
}

float _TessellationUniform;


TessellationFactors patchConstantFunction(InputPatch<vertexInput, 3> patch)
{
	TessellationFactors tf;

	//Check if the current vertex needs grass
	tf.edge[0] = (0.5 * (patch[1].TessFactor + patch[2].TessFactor)) * patch[1].hasGrass;
	tf.edge[1] = (0.5f * (patch[2].TessFactor + patch[0].TessFactor)) * patch[2].hasGrass;
	tf.edge[2] = (0.5f * (patch[0].TessFactor + patch[1].TessFactor)) * patch[0].hasGrass;
	tf.inside = tf.edge[0] * 1.5 * patch[0].hasGrass;

	return tf;
}

[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]
[UNITY_partitioning("integer")]
[UNITY_patchconstantfunc("patchConstantFunction")]
vertexInput hull (InputPatch<vertexInput, 3> patch, uint id : SV_OutputControlPointID)
{
	return patch[id];
}

[UNITY_domain("tri")]
vertexOutput domain(TessellationFactors factors, OutputPatch<vertexInput, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
{
	vertexInput v;


	#define MY_DOMAIN_PROGRAM_INTERPOLATE(fieldName) v.fieldName = \
		patch[0].fieldName * barycentricCoordinates.x + \
		patch[1].fieldName * barycentricCoordinates.y + \
		patch[2].fieldName * barycentricCoordinates.z;

	MY_DOMAIN_PROGRAM_INTERPOLATE(vertex)
	MY_DOMAIN_PROGRAM_INTERPOLATE(normal)
	MY_DOMAIN_PROGRAM_INTERPOLATE(tangent)

	return tessVert(v);
}