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

float4 SapphireAura(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
	float upi = uv.x * 3.14159265359 * 2;
	float time = uTime;
	uv.y += sin((time + upi) * 7) * 0.05f - sin((time * 0.5f + upi) * 4) * 0.025f;
	float4 tex = tex2D(uImage0, uv);
	tex.a *= tex.r;
	return color * tex;
}

technique Technique1 {
	pass SapphireAura {
		PixelShader = compile ps_2_0 SapphireAura();
	}
}
