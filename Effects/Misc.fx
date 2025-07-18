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

bool IsInBounds(float2 value) {
	if (value.x < 0)
		return false;
	if (value.x > 1)
		return false;
	if (value.y < 0)
		return false;
	if (value.y > 1)
		return false;
	return true;
}
bool IfInBounds(float2 value) {
	if (IsInBounds(value))
		return 1;
	return 0;
}

float2 Rotate(float2 value, float angle) {
	return float2(value.x * cos(angle) - value.y * sin(angle), value.x * sin(angle) + value.y * cos(angle));
}

float4 MultiplyRGBA(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float2 realOffset = Rotate(uOffset, uRotation) / uImageSize1;
	float2 maskCoords = (coords - uTargetPosition / uImageSize0) / (uImageSize1 / uImageSize0) - realOffset;
	return tex2D(uImage0, coords) * tex2D(uImage1, maskCoords);
}

float4 NoScreenShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	return tex2D(uImage0, coords);
}

float4 NoArmorShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	return tex2D(uImage0, coords) * sampleColor;
}

float4 Erase(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 value = tex2D(uImage0, coords) - tex2D(uImage1, coords).a * sampleColor;
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
	pass MultiplyRGBA {
		PixelShader = compile ps_2_0 MultiplyRGBA();
	}
	pass NoScreenShader {
		PixelShader = compile ps_2_0 NoScreenShader();
	}
	pass NoArmorShader {
		PixelShader = compile ps_2_0 NoArmorShader();
	}
	pass Erase {
		PixelShader = compile ps_2_0 Erase();
	}
}
