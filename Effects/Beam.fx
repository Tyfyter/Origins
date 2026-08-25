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
matrix<float, 4, 4> uColorMatrix0;
matrix<float, 4, 4> uColorMatrix1;
matrix<float, 4, 4> uFinalColorMatrix;
matrix<float, 4, 4> uOverbrightMatrix;
float4 uOverbrightMax;

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

float4 StarSoldierLaser(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
	float4 value = tex2D(uImage0, uv)
	+ tex2D(uImage0, uv + uShaderSpecificData.xy) * uShaderSpecificData.z
	+ tex2D(uImage0, uv - uShaderSpecificData.xy) * uShaderSpecificData.z;
	value = mul(uColorMatrix1, value);
	float4 overbrightness = max(value - float4(1, 1, 1, 1), float4(0, 0, 0, 0));
	return mul(uFinalColorMatrix, value - overbrightness) + mul(uOverbrightMatrix, min(overbrightness, uOverbrightMax));
}

technique Technique1 {
	pass Beam {
		PixelShader = compile ps_2_0 Beam();
	}
	pass StarSoldierLaser {
		PixelShader = compile ps_2_0 StarSoldierLaser();
	}
}
