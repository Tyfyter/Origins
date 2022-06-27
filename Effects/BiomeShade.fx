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

float4 VoidShade(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	float4 color = tex2D(uImage0, coords);
	float progress = uProgress;
	//progress/=pow(2, 1+(color.r+color.g+color.b)/9);
	//if(color.r>0.9||color.g>0.9||color.b>0.9)progress/=pow(2, 1+(pow(color.r,2)+pow(color.g,2)+pow(color.b,2))*2);
	float prog = 1/(1+progress);
	color.r = pow(color.r, prog);
	color.g = pow(color.g, prog);
	color.b = pow(color.b, prog);
	color.rgb -= 0.5*progress;
	//sampleColor.rgb*=0.5;
	return color;
}

float4 DefiledShade(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	float4 color = tex2D(uImage0, coords);
	float progress = uProgress;
	float median = (min(color.r,min(color.g,color.b))+max(color.r,max(color.g,color.b)))/2;
	color.rgb = lerp(color.rgb,median,progress);
	return color*(sampleColor+(1,1,1,1))/2;
}

float4 RivenShade(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	const float pi = 3.1415926535897932384626433832795;
	const float pisquared = 10;//9.8696044010893586188344909998761;
	const float mult = 1.85;
	float4 color = tex2D(uImage0, coords);
	float progress = uProgress;
	//float time = uTime/3;
	float xval = (coords.x - 0.5) * mult;
	float yval = (coords.y - 0.5) * mult;
	float r = sqrt((xval * xval) + (yval * yval)) * progress;
	float tpi = (atan(yval / xval) + (sin(uTime / pisquared) / pi)) * pisquared;
	//float tCos = cos(uTime) + 0.5;
	//if (isnan(tCos) || isinf(tCos)) {
	//	tCos = 0;
	//}
	//the 1 was supposed to be tCos, but for some reason that makes it disappear about half the time now
	float aval = r + (lerp(1, cos(tpi), sin(tpi) + 0.5) * 0.05) * r;
	aval = clamp(aval, 0, 1) * progress;
	color.rgb = lerp(color.rgb, color.rgb * float3(1, 0.45, 0.15), aval);
	if (aval > 0.75) color.rgb = lerp(color.rgb, 0, (aval - 0.75) * 3);
	return color;
}

technique Technique1{
	pass VoidShade{
		PixelShader = compile ps_2_0 VoidShade();
	}
	pass DefiledShade{
		PixelShader = compile ps_2_0 DefiledShade();
	}
	pass RivenShade{
		PixelShader = compile ps_2_0 RivenShade();
	}
}