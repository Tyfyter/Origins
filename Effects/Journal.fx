sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uSaturation;
float uOpacity;
float uTime;
float uRotation;
float uDirection;
float4 uSourceRect;
float4 uLegacyArmorSourceRect;
float2 uTargetPosition;
float2 uWorldPosition;
float2 uImageSize0;
float2 uLegacyArmorSheetSize;
float2 uImageSize1;
float3 uLightSource;
float4 uShaderSpecificData;

float4 Drawing(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float2 pixel = 2 / uImageSize0;
	float4 textureHere = tex2D(uImage0, coords);
	float4 value =
	tex2D(uImage0, coords + pixel * float2(0, -1)) + tex2D(uImage0, coords + pixel * float2(0, -1)) + tex2D(uImage0, coords + pixel * float2(1, -1)) +
	tex2D(uImage0, coords + pixel * float2(1, 0)) + textureHere * -8 + tex2D(uImage0, coords + pixel * float2(1, 0)) +
	tex2D(uImage0, coords + pixel * float2(-1, 1)) + tex2D(uImage0, coords + pixel * float2(0, 1)) + tex2D(uImage0, coords + pixel * float2(1, 1));
	return float4(uColor, 1) * pow((abs(value.r) + abs(value.g) + abs(value.b) + value.a * 3) / 6, uSaturation) * (textureHere.a + 1) * 0.5;
}
float4 LightnessToTransparency(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	float median = (min(color.r, min(color.g, color.b)) + max(color.r, max(color.g, color.b))) / 2;
	return float4(0, 0, 0, 1 - median) * color.a;
}

technique Default {
	pass Drawing {
		PixelShader = compile ps_3_0 Drawing();
	}
	pass LightnessToTransparency {
		PixelShader = compile ps_3_0 LightnessToTransparency();
	}
}