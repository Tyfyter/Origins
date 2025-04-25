sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
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
float2 uImageSize2;
float2 uOffset;
float uScale;
float uFrameCount;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;
float4 uShaderSpecificData;
const float PI = 3.14159265359;

float4 GetTangelaColor(float2 coords, float2 uImageSize0, float2 uOffset, float4 uSourceRect) {
	float3 puble = float3(0.24, 0.00, 0.44);
	float2 frameCoords = coords * uImageSize0 + uOffset - uSourceRect.xy;
	float perlin = tex2D(uImage2, frameCoords / uImageSize2).r;
	float4 noise = tex2D(uImage1, frameCoords / uImageSize1);
	perlin = (perlin - 0.5) * 2;
	perlin = (pow(abs(perlin), 0.8) * sign(perlin)) / 2 + 0.5;
	
	return lerp(float4(perlin * 3 * puble, 1), noise, pow(perlin * 2, 3));
}

float4 Tangela(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	return GetTangelaColor(coords, uImageSize0, uOffset, uSourceRect) * tex2D(uImage0, coords).a;
}

float quintEaseIn(float x) {
	return x / 3;
}

float4 DefiledLaser(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	coords *= 2;
	coords -= 1;
	float progress = uShaderSpecificData.x;
    
	float laser1 = smoothstep(3, 0.1, abs(coords.y * 3));
	float laser2 = saturate(1 / abs(coords.y * 3));
	laser1 *= laser2;
    //smooth the bloom
	laser1 *= smoothstep(0.5, 0.1, length(coords.y));
    
    //fade in and out
	laser1 *= smoothstep(0.8, 0.1, coords.x);
    
    //more fade in
	float fadeIn = smoothstep(1, 0.8, abs(coords.x));

    
	float4 laserFlames1 = float4(uColor, 1) * 20;

	laserFlames1 *= laser1;
    
    
	float4 edgeFlameAuraThingy = tex2D(uImage3, float2(coords.x - sign(coords.x) > -1 ? uTime : -uTime, coords.y));
	edgeFlameAuraThingy *= lerp(smoothstep(1, 0.4, coords.x / 2), 1, laser1) * smoothstep(0.5, 0.1, abs(coords.y) / 2);
	float4 finalColor = (laser1.xxxx + laserFlames1 + edgeFlameAuraThingy) * fadeIn;
	return GetTangelaColor((coords * 16 + floor(uTime * 5) * 155) / float2(32, 256), uImageSize2, uOffset, float4(0, 0, 1, 1)) * finalColor;
  

}

technique Technique1 {
	pass Tangela {
		PixelShader = compile ps_2_0 Tangela();
	}
	pass DefiledLaser {
		PixelShader = compile ps_3_0 DefiledLaser();
	}
}
