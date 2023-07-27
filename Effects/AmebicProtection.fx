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
float2 Rotate(float2 value, float angle) {
	return float2(value.x * cos(angle) - value.y * sin(angle), value.x * sin(angle) + value.y * cos(angle));
}

float4 AmebicProtection(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float2 realOffset = Rotate(uOffset, uRotation) / uImageSize0;
	realOffset.x *= uDirection;
	float alpha;
	if (IsInBounds(coords + realOffset)) {
		alpha = tex2D(uImage0, coords + realOffset).a > 0 ? 0 : tex2D(uImage0, coords).a;
	} else {
		alpha = 0;
	}

	float2 spriteCoords = float2(round(coords.x), round(coords.y)) * uImageSize0;
	return float4(0, alpha * (sin((uTime + spriteCoords.x + spriteCoords.y * 10) * 2) + 1) * 0.5, alpha * (sin(uTime + spriteCoords.x + spriteCoords.y * 10) + 3) * 0.25, 0);
}

technique Technique1 {
	pass AmebicProtection {
		PixelShader = compile ps_2_0 AmebicProtection();
	}
}
