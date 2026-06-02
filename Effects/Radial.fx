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
float2 uImageOffset1;
matrix<float, 4, 4> uColorMatrix0;
matrix<float, 4, 4> uColorMatrix1;

const float PI = 3.1415926535897932384626433832795;
const float TAU = 6.283185307179586476925286766559;
float4 TrenchmakerLaserHit(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
	float2 diff = uv - uOffset;
	float2 rt = float2(atan2(diff.y, diff.x) / TAU + 0.5, length(diff) / uScale);
	return color * (mul(uColorMatrix0, tex2D(uImage0, rt)) * mul(uColorMatrix1, tex2D(uImage1, rt + uImageOffset1)));
}

technique Technique1 {
	pass TrenchmakerLaserHit {
		PixelShader = compile ps_2_0 TrenchmakerLaserHit();
	}
}
