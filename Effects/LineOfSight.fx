sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
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
float4 uLightRegion;
float2 uLightOffset;
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
	float2 worldCoords = coords * uScreenResolution + uScreenPosition;
	float2 diff = uTargetPosition - worldCoords;
	float dist = length(diff);
	float losDist = tex1D(tex, atan2(coords.y - diff.y, coords.x - diff.x) / twopi).r;
	float4 color = float4(0, 0, 0, 0);
	float dx = 2 / uScreenResolution.x;
	float dy = 2 / uScreenResolution.y;
	for (int i = -1; i <= 1; i++) {
		for (int j = -1; j <= 1; j++) {
			color += gauss[i + 1][j + 1] * tex2D(uImage0, float2(coords.x + dx * i, coords.y + dy * j));
		}
	}
	float4 light = tex2D(uImage1, coords);
	float brightness = max(max(light.r, light.g), light.b);
	return lerp(
		tex2D(uImage0, coords),
		lerp(
			color,
			light,
			smoothstep(0, 16 * 60 * brightness * max(brightness * brightness, 1), dist - losDist)
		),
		smoothstep(0, 128, dist - losDist) * 0.65 + 0.35
	);
	// * lerp(
	//float4(1, 1, 1, 1),
	//	light,
	//	smoothstep(0, 16 * 20 * (light.r + light.g + light.b), dist)
	//)
}

technique Default {
	pass SmogStorm {
		PixelShader = compile ps_3_0 SmogStorm();
	}
}