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
float uFrameCount;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

float4 PowerLotusFairy(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	float median = (min(color.r, min(color.g, color.b)) + max(color.r, max(color.g, color.b))) / 2;
	float saturation = max(abs(median - color.r), max(abs(median - color.g), abs(median - color.b))) * 2;
	
	float2 spriteCoords = (float2(coords.x, coords.y) * uImageSize0 - uSourceRect.xy) * 0.01;
	return lerp(
		float4(1, 1, 1, 1),
		float4(0, (sin((uTime + spriteCoords.x + spriteCoords.y * 10) * 2) + 1) * 0.5, (sin(uTime + spriteCoords.x + spriteCoords.y * 10) + 3) * 0.25, 1),
		saturation
	) * float4(median, median, median, 1) * color.a;
}

technique Technique1{
	pass PowerLotusFairy {
		PixelShader = compile ps_2_0 PowerLotusFairy();
	}
}
