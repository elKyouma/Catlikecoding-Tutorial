
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<float> _Noise;
	StructuredBuffer<float3> _Positions;
	StructuredBuffer<float3> _Normals;
	float4 _Config;
#endif

void ConfigureProcedural()
{
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    float u = floor(unity_InstanceID * _Config.y + 0.0001);
    float v = unity_InstanceID - u * _Config.x;
	
	unity_ObjectToWorld = 0.0;
    unity_ObjectToWorld._m00_m11_m22 = float3(1,1,1) * _Config.y * _Config.w;
    unity_ObjectToWorld._m03_m13_m23 = _Positions[unity_InstanceID];
    unity_ObjectToWorld._m03_m13_m23 += (_Config.z * _Noise[unity_InstanceID]) * _Normals[unity_InstanceID];
    unity_ObjectToWorld._m30_m31_m32_m33 = float4(0.0, 0.0, 0.0, 1.0);
#endif
}

float3 GetNoiseColor()
{
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		float noise = _Noise[unity_InstanceID];
		return noise < 0.0 ? float3(0.0, 0.0, -noise) : float3(0.0, noise, 0.0);
#else
    return 1.0;
#endif
}

void ShaderGraphFunction_float(float3 In, out float3 Out, out float3 color)
{
    Out = In;
    color = GetNoiseColor();
}

void ShaderGraphFunction_half(half3 In, out half3 Out, out half3 color)
{
    Out = In;
    color = GetNoiseColor();
}