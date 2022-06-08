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

float4 BlackHole(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	coords-=float2(0.5,0.5);
	return float4(uColor.r,uColor.g,uColor.b,uOpacity-pow(length(coords)/uScale,max(uSaturation,0)));
}

technique Technique1{
	pass BlackHole{
		PixelShader = compile ps_2_0 BlackHole();
	}
}