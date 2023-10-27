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
float2 uCoordinateSize;
float2 uOffset;
float uScale;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

float4 CoordinateMask(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	if (tex2D(uImage0, coords).a <= 0) return float4(0, 0, 0, 0);
	coords = ((uWorldPosition - uOffset) * 0.18 + ((coords * uImageSize0 - uSourceRect.xy) * uScale)) / uCoordinateSize; //
	return float4(fmod(coords.x + uColor.r, 1), fmod(coords.y + uColor.g, 1), uColor.b, 1);
}

float4 Transparency(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	return float4(0, 0, 0, 0);
}

technique Technique1 {
	pass CoordinateMask {
		PixelShader = compile ps_2_0 CoordinateMask();
	}
	pass Transparency {
		PixelShader = compile ps_2_0 Transparency();
	}
}
