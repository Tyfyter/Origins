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

float GetShimmerWave(float worldPositionX, float worldPositionY) {
	return sin((((worldPositionX + worldPositionY / 6) / 10) - uTime / 360.0) * 6.2831854820251465);
}
float hue2rgb(float c, float t1, float t2) {
	if (c < 0.0) {
		c += 1.0;
	}
	if (c > 1.0) {
		c -= 1.0;
	}
	if (6.0 * c < 1.0) {
		return t1 + (t2 - t1) * 6.0 * c;
	}
	if (2.0 * c < 1.0) {
		return t2;
	}
	if (3.0 * c < 2.0) {
		return t1 + (t2 - t1) * (2.0 / 3.0 - c) * 6.0;
	}
	return t1;
}
float3 hslToRgb(float Hue, float Saturation, float Luminosity) {
	if (Saturation <= 0) {
		return float3(Luminosity, Luminosity, Luminosity);
	} else {
		float num2 = Luminosity >= 0.5 ? Luminosity + Saturation - Luminosity * Saturation : Luminosity * (1.0 + Saturation);
		float t = 2.0 * Luminosity - num2;
		float c = Hue + 1.0 / 3.0;
		float c3 = Hue - 1.0 / 3.0;
		c = hue2rgb(c, t, num2);
		float c2 = hue2rgb(Hue, t, num2);
		float num4 = hue2rgb(c3, t, num2);
		return float3(c, c2, num4);
	}
}
float4 GetShimmerGlitterColor(float2 worldPosition) {
	float3 color = hslToRgb((((worldPosition.x + worldPosition.y / 6) + uTime / 30.0) / 6.0) % 1, 1, 0.5);
	return float4(color * 0.5, 0);
}
float4 GetShimmerBaseColor(float2 worldPosition) {
	return float4(lerp(float3(0.64705884, 26.0 / 51.0, 14.0 / 15.0), float3(41.0 / 51.0, 41.0 / 51.0, 1.0), 0.1 + GetShimmerWave(worldPosition.x, worldPosition.y) * 0.4), 1);
}
float average(float3 value) {
	return (value.r + value.g + value.b) / 3.0;
}
float defav(float3 value) {
	return (max(max(value.r, value.g), value.b) + min(min(value.r, value.g), value.b)) / 2.0;
}

float4 Shimmer(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	if (color.a == 0.0) return color;
	float3 baseColor = color.rgb;
	color.rgb = average(color.rgb);
	float rainbowFactor = (min(min(baseColor.r, baseColor.g), baseColor.b) + color.r) / 2.0;
	float2 pixelCoords = coords * uImageSize0 - uSourceRect.xy;
	if (uDirection < 0) {
		pixelCoords.x = uSourceRect.z - pixelCoords.x;
	}
	float2 worldPosition = (pixelCoords + uOffset) / 16;
	return lerp(color, color.a, 0) * GetShimmerBaseColor(worldPosition) * 2 + GetShimmerGlitterColor(worldPosition) * clamp(pow(1.0 - round(rainbowFactor * 3.0) / 3.0, 15) * 15, 0, 1) * 0.5;
}

float4 SmoothShimmer(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	if (color.a == 0.0)
		return color;
	float3 baseColor = color.rgb;
	color.rgb = average(color.rgb);
	float rainbowFactor = (min(min(baseColor.r, baseColor.g), baseColor.b) + color.r) / 2.0;
	float2 pixelCoords = coords * uImageSize0 - uSourceRect.xy;
	if (uDirection < 0) {
		pixelCoords.x = uSourceRect.z - pixelCoords.x;
	}
	float2 worldPosition = (pixelCoords + uOffset) / 16;
	return lerp(color, color.a, 0) * GetShimmerBaseColor(worldPosition) * 2 + GetShimmerGlitterColor(worldPosition) * clamp(pow(1.0 - rainbowFactor, 5) * 5, 0, 1) * (1 - pow(1 - color.a, 4));
}

technique Technique1{
	pass Shimmer {
		PixelShader = compile ps_3_0 Shimmer();
	}
	pass SmoothShimmer {
		PixelShader = compile ps_3_0 SmoothShimmer();
	}
}
