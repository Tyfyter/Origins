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

float4 ProcessMatrix(float4 centerColor, float4 a, float4 b, float4 c, float4 d) {
	int fails = 0;
	centerColor *= 4;
	centerColor -= a * a.a;
	centerColor -= b * b.a;
	centerColor -= c * c.a;
	centerColor -= d * d.a;
	if (centerColor.r < 0) {
		centerColor.r = 0;
	}
	if (centerColor.g < 0) {
		centerColor.g = 0;
	}
	if (centerColor.b < 0) {
		centerColor.b = 0;
	}
	if (centerColor.a < 0) {
		centerColor.a = 0;
	}
	return centerColor;
}

float4 TileOutline(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float offset = uScale / uImageSize0;
	float4 alpha = ProcessMatrix(
		tex2D(uImage0, coords),
		tex2D(uImage0, coords + float2(offset, 0)),
		tex2D(uImage0, coords + float2(-offset, 0)),
		tex2D(uImage0, coords + float2(0, offset)),
		tex2D(uImage0, coords + float2(0, -offset))
	);
	if (sampleColor.r / sampleColor.g > 1.15) {
		alpha.rgb *= sampleColor.rgb * 8;
		//alpha.rgb = alpha.aaa;
	} else {
		alpha.rgb *= sampleColor.rgb * 2;
	}
	return float4(uColor, 0) * alpha * sampleColor;
}

technique Technique1 {
	pass TileOutline {
		PixelShader = compile ps_2_0 TileOutline();
	}
}
