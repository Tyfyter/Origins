sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uOffset;
float uScale;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;
matrix<float, 4, 4> uImageMatrix1;
matrix<float, 4, 4> uColorMatrix0;
float4 uColorAddition0;
matrix<float, 4, 4> uColorMatrix1;
float4 uColorAddition1;

float2 Rotate(float2 value, float angle) {
	return float2(value.x * cos(angle) - value.y * sin(angle), value.x * sin(angle) + value.y * cos(angle));
}
float2 Transform(float2 position, matrix<float, 4, 4> _matrix) {
	return float2(
		(position.x * _matrix._11) + (position.y * _matrix._21) + _matrix._41,
		(position.x * _matrix._12) + (position.y * _matrix._22) + _matrix._42
	);
}
float4 Transform(float4 color, matrix<float, 4, 4> _matrix) {
	return float4(
		(color.r * _matrix._11) + (color.g * _matrix._21) + (color.b * _matrix._31) + (color.a * _matrix._41),
		(color.r * _matrix._12) + (color.g * _matrix._22) + (color.b * _matrix._32) + (color.a * _matrix._42),
		(color.r * _matrix._13) + (color.g * _matrix._23) + (color.b * _matrix._33) + (color.a * _matrix._43),
		(color.r * _matrix._14) + (color.g * _matrix._24) + (color.b * _matrix._34) + (color.a * _matrix._44)
	);
}

float4 MatrixCombine(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float2 realOffset = Rotate(uOffset, uRotation) / uImageSize1;
	float2 maskCoords = (Transform(coords * uImageSize0 - uTargetPosition, uImageMatrix1) / uImageSize0) / (uImageSize1 / uImageSize0) - realOffset;
	float4 value = (Transform(tex2D(uImage0, coords), uColorMatrix0) + uColorAddition0) + (Transform(tex2D(uImage1, maskCoords), uColorMatrix1) + uColorAddition1);
	if (value.r < 0)
		value.r = 0;
	if (value.g < 0)
		value.g = 0;
	if (value.b < 0)
		value.b = 0;
	if (value.a < 0)
		value.a = 0;
	return value;
}

technique Technique1 {
	pass MatrixCombine {
		PixelShader = compile ps_2_0 MatrixCombine();
	}
}
