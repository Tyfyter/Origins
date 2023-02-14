sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 MaskedRasterizeFilter(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	//return tex2D(uImage0, coords) * tex2D(uImage1, coords).a;
	float fade = uTime % 3;
	float2 noiseCoords = float2(coords.x * uScreenResolution.x, coords.y * uScreenResolution.y);
	float4 color2 = tex2D(uImage1, ((noiseCoords / float2(320, 320)) + uScreenPosition) % float2(1, 1));
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
	float4 maskColor = tex2D(uImage2, coords);
	//return color2;
	float2 sourceCoords = coords - ((maskColor.rg * 8 * value) / float2(uScreenResolution.x, uScreenResolution.y));
	float4 color = tex2D(uImage0, sourceCoords);
	//color.rgb /= (color.rgb + 0.5f);
	float median = (min(color.r, min(color.g, color.b)) + max(color.r, max(color.g, color.b))) / 2;
	return lerp(tex2D(uImage0, coords), median, maskColor.a); //lerp(tex2D(uImage0, coords), color, maskColor.a); //

}

technique Technique1{
	pass MaskedRasterizeFilter {
		PixelShader = compile ps_2_0 MaskedRasterizeFilter();
	}
}