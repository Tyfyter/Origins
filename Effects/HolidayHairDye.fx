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
	float2 baseCoords = float2(0.5, (uSourceRect.y + 16) / uImageSize0.y);
	float2 offsetCoords = (coords - baseCoords) * uImageSize0;
	float len = length(offsetCoords) / 32;
	float angle = atan2(offsetCoords.y, offsetCoords.x);
	float cosine = cos(angle);
	float sine = sin(angle);
	//offsetCoords += float2(offsetCoords.x * cosine - offsetCoords.y * sine, offsetCoords.x * sine + offsetCoords.y * cosine) * len * 0.5 * sin(uTime + len);
	//offsetCoords += float2(sin(uTime + offsetCoords.x * 0.25) * max(abs(offsetCoords.x) - 8, 0) * 0.25, cos(-uTime + offsetCoords.y * 0.25) * max(abs(offsetCoords.y) - 8, 0) * 0.25);
	
	coords = (offsetCoords / uImageSize0) + baseCoords;
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
	float star = (pow(max(starColor.b, 0), 4) - 0.1) * 4;
	if (star < 0) star = 0;
	if (star > 0.7) star = 0.7;
	//return float4(star * 0.64, star * 0.7, star, 1) * color.a * baseColor.a;
	
	//
	return color * baseColor + float4(star * 0.64, star * 0.7, star, 0) * baseColor.a; // * baseColor
}

float4 BorderedDyeBase(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 baseColor = tex2D(uImage0, coords);
	float4 color = float4(0.149, 0.616, 0.808, 1);

	if (baseColor.r > 0.36 || !EmptyAdj(coords, float2(2, 2) / uImageSize0)) {
		
	}
	return color * baseColor;
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
	pass Default {
		PixelShader = compile ps_3_0 Identity();
	}
}