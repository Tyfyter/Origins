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

float4 Beam(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
	if (uLoopData.y == 0) {
		uv.x = fmod(uv.x * uLoopData.x, 1);
	} else if (uv.x < uLoopData.y) { // start
		uv.x = (uv.x / uLoopData.y) * uLoopData.x;
	} else if (uv.x > 1 - uLoopData.y) { // end
		uv.x = ((uv.x - (1 - uLoopData.y)) / uLoopData.y + 2) * uLoopData.x;
	} else {
		uv.x = (fmod((uv.x / uLoopData.y - 1), 1) + 1) * uLoopData.x;
		//return float4(uv.x, 0, 0, 1);
	}
	uv = (uv * uShaderSpecificData.zw) + uShaderSpecificData.xy;
	return color * tex2D(uImage0, uv);
}

technique Technique1 {
	pass Beam {
		PixelShader = compile ps_2_0 Beam();
	}
}
