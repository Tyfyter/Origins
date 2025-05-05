sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uProgress;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float2 uScreenPosition;
float2 uScreenResolution;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uOffset;
float2 uZoom;
float uScale;
float4 uShaderSpecificData;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

float4 TestingShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float2 pos = (coords * uScreenResolution) + uScreenPosition;
	float factor = uProgress - pow(length(pos - uTargetPosition) / uScale, max(uSaturation, 0));
	return tex2D(uImage0, coords) * clamp(1 - factor, 0, 1);
}

technique Technique1 {
	pass TestingShader {
		PixelShader = compile ps_2_0 TestingShader();
	}
}