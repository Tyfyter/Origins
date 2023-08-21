sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float sum(float3 value) {
	return value.r + value.g + value.b;
}

float4 MaskedPurpleJellyFilter(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage2, coords);
	float2 noiseCoords = fmod(coords * 1.5, float2(1, 1));
	float brightness = sum(color.rgb) / 3;
	color = float4(1.2, 0.6, 1.5, 0) * brightness;
	color *= sum(tex2D(uImage1, noiseCoords).rgb);
	return tex2D(uImage0, coords) + color; // + float4(0.6, 0, 0.8, 0) * color.a;
}

technique Technique1{
	pass MaskedPurpleJellyFilter {
		PixelShader = compile ps_2_0 MaskedPurpleJellyFilter();
	}
}