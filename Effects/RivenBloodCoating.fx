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

float4 RivenBloodCoating(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float2 worldCoords = (coords - float2(0.5, 0.5)) / uZoom + float2(0.5, 0.5);
	float alpha = tex2D(uImage2, worldCoords).a;
	float2 pixelX = float2(2 / uScreenResolution.x, 0) / uZoom;
	float2 pixelY = float2(0, 2 / uScreenResolution.y) / uZoom;
	float adjacents = tex2D(uImage2, worldCoords + pixelX).a + tex2D(uImage2, worldCoords - pixelX).a + tex2D(uImage2, worldCoords + pixelY).a + tex2D(uImage2, worldCoords - pixelY).a;
	return tex2D(uImage0, coords) + lerp(
		float4(0.35, 1, 0.75, 0.2) * (1 - uIntensity),
		tex2D(uImage1, (coords * uScreenResolution + uScreenPosition) / uImageSize1) * float4(0.394f, 0.879f, 0.912f, 1) * uIntensity,
		(adjacents / (alpha * 3))
	) * alpha;
}

float4 ChineseRivenBloodCoating(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float2 worldCoords = (coords - float2(0.5, 0.5)) / uZoom + float2(0.5, 0.5);
	float alpha = tex2D(uImage2, worldCoords).a;
	float2 pixelX = float2(2 / uScreenResolution.x, 0) / uZoom;
	float2 pixelY = float2(0, 2 / uScreenResolution.y) / uZoom;
	float adjacents = tex2D(uImage2, worldCoords + pixelX).a + tex2D(uImage2, worldCoords - pixelX).a + tex2D(uImage2, worldCoords + pixelY).a + tex2D(uImage2, worldCoords - pixelY).a;
	return tex2D(uImage0, coords) + lerp(
		float4(0.5, 0.5, 0.5, 0.5),
		float4(1, 1, 1, 1),
		(adjacents / (alpha * 3))
	) * alpha;
}

technique Technique1{
	pass RivenBloodCoating {
		PixelShader = compile ps_2_0 RivenBloodCoating();
	}
	pass ChineseRivenBloodCoating {
		PixelShader = compile ps_2_0 ChineseRivenBloodCoating();
	}
}