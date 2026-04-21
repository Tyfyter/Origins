sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uSaturation;
float uOpacity;
float uTime;
float uRotation;
float uDirection;
float4 uSourceRect;
float4 uLegacyArmorSourceRect;
float2 uTargetPosition;
float2 uScreenPosition;
float2 uScreenResolution;
float2 uImageSize0;
float2 uLegacyArmorSheetSize;
float2 uImageSize1;
float3 uLightSource;
float4 uShaderSpecificData;

const float twopi = 6.28318530718;

texture uTexture;
sampler1D tex = sampler_state {
	texture = <uTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
};
float gauss[3][3] = {
	0.070, 0.116, 0.070,
    0.116, 0.256, 0.116,
    0.070, 0.116, 0.070
};

float4 SmogStorm(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float2 diff = uTargetPosition - (coords * uScreenResolution + uScreenPosition);
	float4 color = float4(0, 0, 0, 0);
	float dx = 2 / uScreenResolution.x;
	float dy = 2 / uScreenResolution.y;
	for (int i = -1; i <= 1; i++) {
		for (int j = -1; j <= 1; j++) {
			color += gauss[i + 1][j + 1] * tex2D(uImage0, float2(coords.x + dx * i, coords.y + dy * j));
		}
	}
	return lerp(
		tex2D(uImage0, coords),
		color,
		smoothstep(0, 16, length(diff) - tex1D(tex, atan2(coords.y - diff.y, coords.x - diff.x) / twopi).r) * 0.75 + 0.25
	);
}

technique Default {
	pass SmogStorm {
		PixelShader = compile ps_3_0 SmogStorm();
	}
}