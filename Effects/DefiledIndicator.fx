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
float2 uLoopData;
const float PI = 3.14159265359;
float arctan2(float2 offset) {
	return offset.y / (sqrt(offset.x * offset.x + offset.y * offset.y) + offset.x);
}
float mod(float x) {
	return x - floor(x);
}

float pingpong(float x) {
	x %= 1;
	if (x < 0)
		x += 1;

	if (x >= 0.5)
		return 2 - x * 2;

	return x * 2;
}

float4 DefiledIndicator(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {

	coords -= float2(0.5, 0.5);

	float progress = uShaderSpecificData.y;
    
	float4 noise = tex2D(uImage1, float2(coords.x - uTime * 2, coords.y));

    
    
	noise.rgb *= uColor;
    
	float4 noise2 = tex2D(uImage1, float2(coords.x + 0.25 - uTime * 2, coords.y));
    
	float4 outlines = smoothstep(0.6, 0.9, abs(coords.y * 2)) * 15;
	float fadeInAndOut = smoothstep(0.5, 0.1, 3 * abs(coords.x) - 0.95);
	return ((noise * noise2.r + outlines) * fadeInAndOut) * pingpong(progress) * sampleColor;
}

technique Technique1 {
	pass DefiledIndicator {
		PixelShader = compile ps_3_0 DefiledIndicator();
	}
}
