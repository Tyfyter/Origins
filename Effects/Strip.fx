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
float4 uSourceRect0;
float4 uSourceRect1;
float4 uAlphaMatrix0;
float4 uAlphaMatrix1;

float4 SapphireAura(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
	float upi = uv.x * 3.14159265359 * 2;
	float time = uTime;
	uv.y += sin((time + upi) * 7) * 0.05f - sin((time * 0.5f + upi) * 4) * 0.025f;
	float4 tex = tex2D(uImage0, uv);
	tex.a *= tex.r;
	return color * tex;
}

float4 ApplyAlphaMatrix(float4 color, float4 _matrix) {
	return float4(color.r, color.g, color.b, min(color.r * _matrix.r + color.g * _matrix.g + color.b * _matrix.b + color.a * _matrix.a, 1));
}

float4 Framed(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
	return color * ApplyAlphaMatrix(tex2D(uImage0, (uv * uSourceRect0.zw) + uSourceRect0.xy), uAlphaMatrix0);
}

float4 AnimatedTrail(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
	return color * ApplyAlphaMatrix(tex2D(uImage0, (uv * uSourceRect0.zw) + uSourceRect0.xy), uAlphaMatrix0) * ApplyAlphaMatrix(tex2D(uImage1, (uv * uSourceRect1.zw) + uSourceRect1.xy), uAlphaMatrix1).a;
}

float4 LaserBlade(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
	float factor = 1.0 - ((abs(uv.y - 0.5f) + uv.x * 0.45) * (pow(uv.x, 0.5)) * 2);
	return color * ApplyAlphaMatrix(tex2D(uImage0, uv + float2(uTime.x * uSaturation, 0)), uAlphaMatrix0) * factor * uOpacity;
}

float2 ApplySourceRect(float2 uv, float4 sourceRect) {
	return uSourceRect0.xy + uv * uSourceRect0.zw;
}

float4 Identity(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
	return color * ApplyAlphaMatrix(tex2D(uImage0, ApplySourceRect(uv, uSourceRect0)), uAlphaMatrix0) * uOpacity;
}

technique Technique1 {
	pass SapphireAura {
		PixelShader = compile ps_2_0 SapphireAura();
	}
	pass Framed {
		PixelShader = compile ps_2_0 Framed();
	}
	pass AnimatedTrail {
		PixelShader = compile ps_2_0 AnimatedTrail();
	}
	pass LaserBlade {
		PixelShader = compile ps_2_0 LaserBlade();
	}
	pass Identity {
		PixelShader = compile ps_2_0 Identity();
	}
}
