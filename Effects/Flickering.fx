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
float4 uShaderSpecificData;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

float4 Flickering(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
    float2 pos = coords * uImageSize0 + uWorldPosition;
	float factor = uOpacity - pow(length(pos - uTargetPosition) / uScale, max(uSaturation, 0));
    return tex2D(uImage0, coords) * clamp(1 - factor, 0, 1) * float4(uColor, factor);
}

technique Technique1{
	pass Flickering{
		PixelShader = compile ps_2_0 Flickering();
	}
}