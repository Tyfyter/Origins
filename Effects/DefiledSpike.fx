sampler2D uImage0 : register(s0);
sampler2D uImage1 : register(s1);
sampler2D uImage2 : register(s2);
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
const float SpikeSegmentHeight = 32;
const float SpikeSegmentWidth = 34;
const float SpikeTextureLength = 289;


float2 Rotate(float2 uv, float amount) {
	float2 uv2 = uv;
	float s = sin(amount);
	float c = cos(amount);
	uv2.x = (uv.x * c) + (uv.y * -s);
	uv2.y = (uv.x * s) + (uv.y * c);

	return uv2;
    
}


float4 DefiledSpike(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	return tex2D(uImage1, float2(coords.x - (uShaderSpecificData.x), coords.y)) * sampleColor;

}

technique Technique1 {
	pass DefiledSpike {
		PixelShader = compile ps_3_0 DefiledSpike();
	}
}
