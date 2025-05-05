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

float4 AntiGrayDye(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords) * sampleColor;
	float median = (min(color.r, min(color.g, color.b)) + max(color.r, max(color.g, color.b))) / 2;
	float saturation = max(abs(color.r - median), max(abs(color.g - median), abs(color.b - median)));
	color.rgb = lerp(median, color.rgb, 1.1 + (1 - saturation) * 0.5);
	return color;
}

technique Technique1{
	pass AntiGrayDye {
		PixelShader = compile ps_3_0 AntiGrayDye();
	}
}
