sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uRotation1;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uOffset;
float uScale;
float uFrameCount;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;
float4 uAlphaMatrix0;
float4 uAlphaMatrix1;

const float PI = 3.14159265359;
const float2 center = float2(0.5, 0.5);

float4 ApplyAlphaMatrix(float4 color, float4 _matrix) {
	return float4(color.r, color.g, color.b, min(color.r * _matrix.r + color.g * _matrix.g + color.b * _matrix.b + color.a * _matrix.a, 1));
}
float2 Rotate(float2 value, float angle) {
	return float2(value.x * cos(angle) - value.y * sin(angle), value.x * sin(angle) + value.y * cos(angle));
}
float PowKeepSign(float value, float exponent) {
	int sig = sign(value);
	return pow(value * sig, exponent) * sig;
}

float4 DoRing(float2 coords) {
	float2 offset = (coords - center) * uImageSize0 * uScale;
	float distance = length(offset);
	float v = clamp(abs(distance - uSaturation) / uOpacity + 0.5, 0, 1);
	
	float4 color = ApplyAlphaMatrix(tex2D(uImage0, float2((atan2(offset.y, offset.x) / (PI * 2) + 0.5) % 1, v)), uAlphaMatrix0);
	return color * ApplyAlphaMatrix(tex2D(uImage1, Rotate(coords - center, uRotation1 * 2) + center), uAlphaMatrix1);
}

float4 MakeRing(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	return DoRing(coords) * sampleColor;
}

technique Technique1 {
	pass MakeRing {
		PixelShader = compile ps_3_0 MakeRing();
	}
}
