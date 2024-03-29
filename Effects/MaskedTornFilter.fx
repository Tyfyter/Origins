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

float4 MaskedTorn(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 baseColor = tex2D(uImage0, coords);
	coords = (coords - float2(0.5, 0.5)) / uZoom + float2(0.5, 0.5);
	float4 color = tex2D(uImage2, coords);
	float4 mask = tex2D(uImage1, color.rg);
	float maskAlpha = mask.a;
	//return float4(color.rg, 0, 0);
	if (color.a <= 0 || maskAlpha <= 0) return baseColor;
	if (mask.r > color.b) {
		maskAlpha = 0;
	} else if (mask.r + 0.02 > color.b) {
		maskAlpha *= (mask.r - color.b) / 0.02;
	} else {
		maskAlpha = 1;
	}
	float glow = maskAlpha * mask.g;
	float2 spriteCoords = float2(round((color.r * uImageSize1.x) / 2) * 2, round((color.g * uImageSize1.y) / 2) * 2);
	float gloSinG = sin((sin(uTime) + spriteCoords.x * 8 + spriteCoords.y) * 2) + 0.5;
	float gloSinB = sin((uTime + spriteCoords.x + spriteCoords.y * 16)) + 0.5;
	float glowG = glow * (gloSinG + gloSinB * 1 + 1) * 0.4;
	float glowB = glow * (gloSinB + gloSinG * 0.5 + 1) * 0.5;
	float4 glowColor = float4(0, glowG, glowB, 1);
	maskAlpha = glowG + glowB;
	return (baseColor * (1 - maskAlpha)) + (glowColor * maskAlpha); // + float4(0.6, 0, 0.8, 0) * color.a;
}

technique Technique1{
	pass MaskedTornFilter {
		PixelShader = compile ps_3_0 MaskedTorn();
	}
}