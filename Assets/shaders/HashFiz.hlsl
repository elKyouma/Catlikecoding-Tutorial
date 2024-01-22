
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<uint> _Hashes;
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
    unity_ObjectToWorld._m03_m13_m23 += (((_Hashes[unity_InstanceID] >> 24 & 255) * 1.0 / 255 - 0.5)*_Config.y * _Config.z)*_Normals[unity_InstanceID];
    unity_ObjectToWorld._m30_m31_m32_m33 = float4(0.0, 0.0, 0.0, 1.0);
#endif
}

void ShaderGraphFunction_float(float3 In, out float3 Out, out float3 color)
{
    Out = In;
    float3 newColor = float3(1,1,1);
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    uint rgb = _Hashes[unity_InstanceID];
    newColor = float3(rgb & 255, rgb >> 8 & 255, rgb >> 16 & 255) * 1.0 / 255; 
#endif
    color = newColor;
}

void ShaderGraphFunction_half(half3 In, out half3 Out, out half3 color)
{
    Out = In;
    half3 newColor = half3(1, 1, 1);
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    uint rgb = _Hashes[unity_InstanceID];
    newColor = half3(rgb & 255, rgb >> 8 & 255, rgb >> 16 & 255) * 1.0 / 255; 
#endif
    color = newColor;
}