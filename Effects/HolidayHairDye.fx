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
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;
/*float2 uMin;
float2 uMax;*/
const float pi = 3.1415926535897931;

bool EmptyAdj(float2 coords, float2 unit) {
	return tex2D(uImage0, coords - unit * float2(1, 0)).a == 0 
	|| tex2D(uImage0, coords - unit * float2(0, 1)).a == 0
	|| tex2D(uImage0, coords + unit * float2(1, 0)).a == 0
	|| tex2D(uImage0, coords + unit * float2(0, 1)).a == 0;
}

float4 SummerSolstace(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	/*
	float2 baseCoords = float2(0.5, (uSourceRect.y + 16) / uImageSize0.y);
	float2 offsetCoords = (coords - baseCoords) * uImageSize0;
	float len = length(offsetCoords) / 32;
	float angle = atan2(offsetCoords.y, offsetCoords.x);
	float cosine = cos(angle);
	float sine = sin(angle);
	offsetCoords += float2(offsetCoords.x * cosine - offsetCoords.y * sine, offsetCoords.x * sine + offsetCoords.y * cosine) * len * 0.5 * sin(uTime + len);
	offsetCoords += float2(sin(uTime + offsetCoords.x * 0.25) * max(abs(offsetCoords.x) - 8, 0) * 0.25, cos(-uTime + offsetCoords.y * 0.25) * max(abs(offsetCoords.y) - 8, 0) * 0.25);
	
	coords = (offsetCoords / uImageSize0) + baseCoords;
	//*/
	float4 baseColor = tex2D(uImage0, coords);
	float4 color = float4(0.969, 0.607, 0.100, 0.369);
	if (baseColor.a > 0) {
		if (baseColor.r > 0.36 || !EmptyAdj(coords, float2(2, 2) / uImageSize0)) {
			color = float4(0.945, 0.891, 0.786, 0.5);
		}
		baseColor.r = pow(baseColor.r, 0.5);
		baseColor.g = pow(baseColor.g, 0.5);
		baseColor.b = pow(baseColor.b, 0.5);
		baseColor *= 1.5;
		if (baseColor.r > 1)
			baseColor.r = 1;
		if (baseColor.g > 1)
			baseColor.g = 1;
		if (baseColor.b > 1)
			baseColor.b = 1;
		color *= baseColor;
	} else {
		color = float4(0, 0, 0, 0);
		float pixeledX = coords.x * uImageSize0.x / 2;
		float pixeledY = coords.y * uImageSize0.y / 2;
		float pixelY = 2 / uImageSize0.y;
		float offsetAmount = (((((floor(pixeledX) * 7) % 5) + uTime * 0.66) * 7) % 3.3);
		if (offsetAmount > 2 || coords.y * uImageSize0.y + offsetAmount > uSourceRect.y + uSourceRect.w) {
			return float4(0, 0, 0, 0);
		}
		baseColor = tex2D(uImage0, coords + float2(0, pixelY * offsetAmount));
		if (baseColor.a <= 0) {
			return float4(0, 0, 0, 0);
		} else {
			color = float4(0.969, 0.507, 0.100, 0.369);
			if (tex2D(uImage0, coords + float2(0, pixelY * (offsetAmount - 1))).a <= 0) {
				float dist = max(min(abs(pixeledX - (floor(pixeledX) + 0.5)) * 2.5, abs(pixeledY - ceil(pixeledY))) - 0.1, 0);
				//color.r = 0;
				color *= pow(1 - dist, 2);
			}
		}
	}
	
	return color;
}

float4 WinterSolstace(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 baseColor = tex2D(uImage0, coords);
	float4 color = float4(0.557 * 0.5, 0.441 * 0.5, 0.769 * 0.5, 0.769) * 0.5;

	if (baseColor.r > 0.36 || !EmptyAdj(coords, float2(2, 2) / uImageSize0)) {
		color = float4(0.191, 0.186, 0.745, 1);
	}
	
	baseColor.r = pow(baseColor.r, 0.5);
	baseColor.g = pow(baseColor.g, 0.5);
	baseColor.b = pow(baseColor.b, 0.5);
	baseColor = ((baseColor * 1.5) + (baseColor * sampleColor * 1.5)) * 0.5;
	if (baseColor.r > 1) baseColor.r = 1;
	if (baseColor.g > 1) baseColor.g = 1;
	if (baseColor.b > 1) baseColor.b = 1;
	
	float frameY = (coords.y * uImageSize0.y - uSourceRect.y);
	float frameX = 0;
	if (uDirection < 0) {
		frameX = uImageSize0.x * (1 - coords.x);
	} else {
		frameX = uImageSize0.x * coords.x;
	}
	float2 absoluteCoords = (float2(frameX, frameY)) + float2(uTime, -uTime) * 3 + uTargetPosition * 0.05;
	float4 starColor = tex2D(uImage1, fmod((absoluteCoords % uImageSize1) / uImageSize1, float2(1, 1)));
	float starSubCycle = (uTime % 1);
	float starCycle = uTime % 3;
	starSubCycle *= pow(starSubCycle, 0.5);
	float val;
	if (starCycle < 1) {
		val = starColor.r * (1 - starSubCycle) + starColor.g * starSubCycle;
	} else if (starCycle < 2) {
		val = starColor.g * (1 - starSubCycle) + starColor.b * starSubCycle;
	} else {
		val = starColor.b * (1 - starSubCycle) + starColor.r * starSubCycle;
	}
	float star = (pow(max(val, 0), 3) - 0.1) * 4;
	if (star < 0) star = 0;
	if (star > 0.7) star = 0.7;
	//return float4(star * 0.64, star * 0.7, star, 1) * color.a * baseColor.a;
	
	//
	return color * baseColor + float4(star * 0.64, star * 0.7, star, 0) * baseColor.a; // * baseColor
}
const float3 RefXYZ = float3(95.047, 100.000, 108.883);
static double PivotXyz(float n) {
	float i = pow(n, 1 / 3);
	return n > 0.008856 ? i : 7.787 * n + 16 / 116;
}
static double PivotRgb(float n) {
	return (n > 0.04045 ? pow((n + 0.055) / 1.055, 2.4) : n / 12.92) * 100;
}
float3 RGBToXYZ(float3 rgb) {
	float r = PivotRgb(rgb.r);
	float g = PivotRgb(rgb.g);
	float b = PivotRgb(rgb.b);
	
	return float3(r * 0.4124 + g * 0.3576 + b * 0.1805, r * 0.2126 + g * 0.7152 + b * 0.0722, r * 0.0193 + g * 0.1192 + b * 0.9505);
}
float3 XYZToRGB(float3 xyz) {
	float x = xyz.x / 100;
	float y = xyz.y / 100;
	float z = xyz.z / 100;

	float r = x * 3.2406 + y * -1.5372 + z * -0.4986;
	float g = x * -0.9689 + y * 1.8758 + z * 0.0415;
	float b = x * 0.0557 + y * -0.2040 + z * 1.0570;

	return float3(
		r > 0.0031308 ? 1.055 * pow(r, 1 / 2.4) - 0.055 : 12.92 * r,
		g > 0.0031308 ? 1.055 * pow(g, 1 / 2.4) - 0.055 : 12.92 * g,
		b > 0.0031308 ? 1.055 * pow(b, 1 / 2.4) - 0.055 : 12.92 * b
	);
}
float3 XYZToLaB(float3 xyz) {
	xyz /= RefXYZ;
	float x = PivotXyz(xyz.x);
	float y = PivotXyz(xyz.y);
	float z = PivotXyz(xyz.z);

	return float3(max(0, 116 * y - 16), 500 * (x - y), 200 * (y - z));
}
float3 LaBToXYZ(float3 lab) {
	float y = (lab[0] + 16) / 116.0;
	float x = lab[1] / 500.0 + y;
	float z = y - lab[2] / 200.0;

	y = pow(y, 3) > 0.008856 ? pow(y, 3) : (y - 16 / 116) / 7.787;
	x = pow(x, 3) > 0.008856 ? pow(x, 3) : (x - 16 / 116) / 7.787;
	z = pow(z, 3) > 0.008856 ? pow(z, 3) : (z - 16 / 116) / 7.787;
	return float3(x, y, z) * RefXYZ;
}
float3 LaBToLCh(float3 lab) {
	float h = atan2(lab[2], lab[1]);
	if (h > 0) {
		h = (h / pi) * 180;
	} else {
		h = 360 - (abs(h) / pi) * 180;
	}
	lab.yz = float2(length(lab.yz), h % 360);
	return lab;
}
float3 LChToLaB(float3 lch) {
	float hRadians = (pi * lch[2]) / 180.0;
	lch.yz = float2(cos(hRadians) * lch[1], sin(hRadians) * lch[1]);
	return lch;
}
float3 RGBToLCh(float3 rgb) {
	return LaBToLCh(XYZToLaB(RGBToXYZ(rgb)));
}
float3 LChToRGB(float3 rgb) {
	return XYZToRGB(LaBToXYZ(LChToLaB(rgb)));
}

float4 AutismAwareness(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	if (color.r > 0.36 || !EmptyAdj(coords, float2(2, 2) / uImageSize0)) {
		color.rgb *= 0.99;
	}
	//color *= sampleColor;
	float3 convertedColor = RGBToLCh(color.rgb);
	convertedColor[1] = 100;
	float2 dist = coords * uImageSize0 - uSourceRect.xy;
	convertedColor[2] = dist.x * 25 + dist.y * 5 + uTime * 50;
	
	return float4(LChToRGB(convertedColor), color.a) * color * sampleColor;
}

float4 BorderedDyeBase(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 baseColor = tex2D(uImage0, coords);
	float4 color = float4(0.149, 0.616, 0.808, 1);

	if (baseColor.r > 0.36 || !EmptyAdj(coords, float2(2, 2) / uImageSize0)) {
		
	}
	return color * baseColor;
}

float4 Overlay(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 baseColor = tex2D(uImage0, coords);
	float4 overlay = tex2D(uImage1, coords);
	baseColor.rgb *= uColor;
	overlay.rgb *= uSecondaryColor;
	return (baseColor * (1 - overlay.a) + overlay) * sampleColor;
}

float4 Textured(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 baseColor = tex2D(uImage0, coords);
	float4 overlay = tex2D(uImage1, coords);
	overlay *= baseColor.a;
	overlay.rgb *= baseColor.rgb;
	baseColor.rgb *= uColor;
	overlay.rgb *= uSecondaryColor;
	return (baseColor * (1 - overlay.a) + overlay) * sampleColor;
}

float4 Identity(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	return tex2D(uImage0, coords) * sampleColor;
}

technique Technique1 {
	pass SummerSolstace {
		PixelShader = compile ps_3_0 SummerSolstace();
	}
	pass WinterSolstace {
		PixelShader = compile ps_3_0 WinterSolstace();
	}
	pass AutismAwareness {
		PixelShader = compile ps_3_0 AutismAwareness();
	}
	pass Overlay {
		PixelShader = compile ps_3_0 Overlay();
	}
	pass Textured {
		PixelShader = compile ps_3_0 Textured();
	}
	pass Default {
		PixelShader = compile ps_3_0 Identity();
	}
}