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
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

float4 TileOutline(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float offset = uScale / uImageSize0;
	float alpha;
	alpha = min(4 - (tex2D(uImage0, coords + float2(offset, 0)).a
	+ tex2D(uImage0, coords + float2(-offset, 0)).a
	+ tex2D(uImage0, coords + float2(0, offset)).a
	+ tex2D(uImage0, coords + float2(0, -offset)).a)
	, 1);
	
	return float4(0.5, 0.0625, 0, 0) * alpha;
}

technique Technique1 {
	pass TileOutline {
		PixelShader = compile ps_2_0 TileOutline();
	}
}
