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
float uFrameCount;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;
float4 uFullColor;

float3 HSVToRGB(float3 hsv) {
	float C = hsv[2] * hsv[1];
	float X = C * (1 - abs((hsv[0] % 2) - 1));
	float m = hsv[2] - C;
	float3 rgbOff = float3(0, 0, 0);
	float s = hsv[0];
	if (s < 1) {
		rgbOff.r = C;
		rgbOff.g = X;
	} else if (s < 2) {
		rgbOff.g = C;
		rgbOff.r = X;
	} else if (s < 3) {
		rgbOff.g = C;
		rgbOff.b = X;
	} else if (s < 4) {
		rgbOff.b = C;
		rgbOff.g = X;
	} else if (s < 5) {
		rgbOff.b = C;
		rgbOff.r = X;
	} else {
		rgbOff.r = C;
		rgbOff.b = X;
	}
	return rgbOff + float3(m, m, m);
}

float4 InvertAnimate(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	coords = uWorldPosition + coords * uImageSize0;
	color.rgb = float3(1.2, 1.2, 1.2) - color.rgb * HSVToRGB(float3(fmod((coords.x + coords.y * 2) / 16 + uTime * 4 + color.r * 3, 6), 1, 1)) * float3(1, 10, 10);
	return color * uFullColor;
}

technique Technique1{
	pass InvertAnimate {
		PixelShader = compile ps_3_0 InvertAnimate();
	}
}
