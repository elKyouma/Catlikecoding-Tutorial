#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<float3x4> _Matrices;
#endif

void ConfigureProcedural()
{
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	float3x4 m = _Matrices[unity_InstanceID];
	unity_ObjectToWorld._m00_m01_m02_m03 = m._m00_m01_m02_m03;
	unity_ObjectToWorld._m10_m11_m12_m13 = m._m10_m11_m12_m13;
	unity_ObjectToWorld._m20_m21_m22_m23 = m._m20_m21_m22_m23;
	unity_ObjectToWorld._m30_m31_m32_m33 = float4(0.0, 0.0, 0.0, 1.0);
#endif
}

float4 _BaseColor;

float4 GetFractalColor () {
	return _BaseColor;
}

void ShaderGraphFunction_float(float3 In, out float3 Out, out float4 FractalColor)
{
    Out = In;
    float4 col = GetFractalColor();
	
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	col /= (unity_InstanceID+1)*0.001;
#endif
	
    FractalColor = col;
}

void ShaderGraphFunction_half(half3 In, out half3 Out, out half4 FractalColor)
{
    Out = In;
    half4 col = GetFractalColor();
	
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	col /= (unity_InstanceID+1)*0.001;
#endif
	
    FractalColor = col;
}