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
float3 RGBToHSV(float3 rgb) {
	float Cmax = max(max(rgb.r, rgb.g), rgb.b);
	float Cmin = min(min(rgb.r, rgb.g), rgb.b);
	float delta = Cmax - Cmin;
	float3 hsv = float3(0, 0, Cmax);
	if (delta <= 0) {
		hsv.x = 0;
	} else if (Cmax <= rgb.r) {
		hsv.x = ((rgb.g - rgb.b) / delta) % 6;
		//if (hsv.x >= 6) hsv.x -= 6;
	} else if (Cmax <= rgb.g) {
		hsv.x = ((rgb.b - rgb.r) / delta + 2);
	} else {
		hsv.x = ((rgb.r - rgb.g) / delta + 4);
	}
	if (Cmax <= 0) {
		hsv.y = 0;
	} else {
		hsv.y = delta / Cmax;
	}
	return hsv;
}

float4 InvertAnimate(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	coords = uWorldPosition + coords * uImageSize0;
	color.rgb = float3(1.2, 1.2, 1.2) - color.rgb * HSVToRGB(float3(fmod((coords.x + coords.y * 2) / 16 + uTime * 4 + color.r * 3, 6), 1, 1)) * float3(1, 10, 10);
	return color * uFullColor;
}

float4 Mask(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	float saturation = max(max(color.r, color.g), color.b) - min(min(color.r, color.g), color.b);
	float lightness = max(max(color.r, color.g), color.b);
	float keepLightness = clamp((saturation * 2 - lightness) * 10, 0, 1);
	//return float4(lerp((1 - lightness) / lightness, 1, keepLightness), lerp((1 - lightness) / lightness, 1, keepLightness), lerp((1 - lightness) / lightness, 1, keepLightness), 1);
	color.rgb *= lerp((1 - lightness) / lightness, 1, keepLightness);
	//color.rgb *= (1 - lightness) / lightness;
	return color * uFullColor * tex2D(uImage1, coords).a;
}

float4 SimpleMask(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	return tex2D(uImage0, coords) * tex2D(uImage1, coords).a;
}

technique Technique1 {
	pass InvertAnimate {
		PixelShader = compile ps_3_0 InvertAnimate();
	}
	pass Mask {
		PixelShader = compile ps_3_0 Mask();
	}
	pass SimpleMask {
		PixelShader = compile ps_3_0 SimpleMask();
	}
}
