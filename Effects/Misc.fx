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
float uScaleY;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;
matrix<float, 4, 4> uImageMatrix1;

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
float2 Transform(float2 position, matrix<float, 4, 4> _matrix) {
	return float2(
		(position.x * _matrix._11) + (position.y * _matrix._21) + _matrix._41,
		(position.x * _matrix._12) + (position.y * _matrix._22) + _matrix._42
	);
}
float4 MultiplyRGBA(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float2 realOffset = Rotate(uOffset, uRotation) / uImageSize1;
	float2 maskCoords = (Transform(coords * uImageSize0 - uTargetPosition, uImageMatrix1) / uImageSize0) / (uImageSize1 / uImageSize0) - realOffset;
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

float4 Muddle(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float2 muddleMapCoords = fmod(fmod(coords * uOffset + uWorldPosition / uImageSize1, float2(1, 1)) + float2(1, 1), float2(1, 1));
	
	float xOffset = tex2D(uImage1, muddleMapCoords).r;
	float2 offset = xOffset * float2(sin(xOffset + uTime * uScale), sin(xOffset + uTime * uScaleY));
	float step2 = tex2D(uImage1, muddleMapCoords + float2(offset.x, 0)).r;
	float step3 = tex2D(uImage1, muddleMapCoords + float2(0, offset.y)).r;
	//return float4(xOffset, step2, step3, 1);
	return tex2D(uImage0, coords + float2(step2, step3) * uTargetPosition) * sampleColor;
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
	pass Muddle {
		PixelShader = compile ps_2_0 Muddle();
	}
}
