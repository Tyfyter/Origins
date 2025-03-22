sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
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
float2 uImageSize2;
float2 uOffset;
float uScale;
float uFrameCount;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

float4 Tangela(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float3 puble = float3(0.24, 0.00, 0.44);
	float4 color = tex2D(uImage0, coords);
	float2 frameCoords = coords * uImageSize0 - uSourceRect.xy;
	float perlin = tex2D(uImage2, frameCoords / uImageSize2).r;
	float4 noise = tex2D(uImage1, frameCoords / uImageSize1);
	perlin = (perlin - 0.5) * 2;
	perlin = (pow(abs(perlin), 0.8) * sign(perlin)) / 2 + 0.5;
	
	return lerp(float4(perlin * puble, 1), noise, pow(perlin * 1.5, 2)) * color.a;
}

technique Technique1{
	pass Tangela {
		PixelShader = compile ps_2_0 Tangela();
	}
}
