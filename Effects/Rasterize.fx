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

float4 Rasterize(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float fade = uTime % 3;
	float2 noiseCoords = float2(coords.x * uImageSize0.x, coords.y * uImageSize0.y);
	float4 color2 = tex2D(uImage1, ((noiseCoords / float2(320, 320)) + uWorldPosition) % float2(1, 1));
	float value;
	float fade2 = fade % 1;
	if (fade < 1) {
		value = color2.r > 0.5f ? 1 : 0; //(color2.r*fade2)+(color2.b*(1-fade2));
	} else if (fade < 2) {
		value = color2.g > 0.5f ? 1 : 0; //(color2.g*fade2)+(color2.r*(1-fade2));
	} else {
		value = color2.b > 0.5f ? 1 : 0; //(color2.b*fade2)+(color2.g*(1-fade2));
	}
	value = pow(value, 2);
	//return color2;
	float2 sourceCoords = coords - (uOffset * value / float2(uImageSize0.x, uImageSize0.y));
	float4 color = tex2D(uImage0, sourceCoords);
	float median = (min(color.r, min(color.g, color.b)) + max(color.r, max(color.g, color.b))) / 2;
	color.rgb /= (color.rgb + 0.5f);
	return lerp(color, median, 0.45) * sampleColor;
}

technique Technique1 {
	pass Rasterize {
		PixelShader = compile ps_2_0 Rasterize();
	}
}
