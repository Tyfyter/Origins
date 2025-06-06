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
float2 uTimeScale;

float4 VoidShade(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	float progress = uProgress;
	if (progress < 0) progress *= 0.25;
	//progress/=pow(2, 1+(color.r+color.g+color.b)/9);
	//if(color.r>0.9||color.g>0.9||color.b>0.9)progress/=pow(2, 1+(pow(color.r,2)+pow(color.g,2)+pow(color.b,2))*2);
		float prog = 1 / (1 + progress);
	color.r = pow(color.r, prog);
	color.g = pow(color.g, prog);
	color.b = pow(color.b, prog);
	color.rgb -= 0.5 * progress;
	//sampleColor.rgb*=0.5;
	return color;
}

float Select(float3 a, float i) {
	if (i < 1.0)
		return a.r;
	if (i < 2.0)
		return a.g;
	return a.b;
}

float4 DefiledShade(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float select = uTime * uTimeScale;
	select = select - floor(select / 3.0) * 3.0;
	//coords.x += (Select(tex2D(uImage1, float2(0, Select(tex2D(uImage1, float2(uTime, 0)).rgb, select) * 5) + coords.y).rgb, select) - 0.5, select) * uIntensity;
	coords.x += Select(tex2D(uImage1, float2(coords.y, coords.x) * 10).rgb, select) * uIntensity;
	coords.y += Select(tex2D(uImage1, coords * 10).rgb, select) * uIntensity; //(Select(tex2D(uImage1, float2(0, Select(tex2D(uImage1, coords * 10).rgb, select) * 5) + coords.x).rgb, select) - 0.5, select) * uIntensity; // float2(uTime, uTime) +
	float4 color = tex2D(uImage0, coords);
	
	float progress = uProgress;
	if (progress < 0)
		progress = 0;
	float median = (min(color.r, min(color.g, color.b)) + max(color.r, max(color.g, color.b))) / 2;
	median += (Select(tex2D(uImage1, float2(median, 0) + coords * float2(1, 4)).rgb, select) - 0.5) * uOpacity * (sampleColor + (1, 1, 1, 1)) / 2; //
	color.rgb = lerp(color.rgb, median, progress * (1 - tex2D(uImage2, coords).a));
	return color;
}

float4 RivenShade_Old(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	const float pi = 3.1415926535897932384626433832795;
	const float halfpi = 1.5707963267948966192313216916398;
	const float pisquared = 10; //9.8696044010893586188344909998761;
	const float mult = 1.85;
	float4 color = tex2D(uImage0, coords);
	//float time = uTime/3;
	float xval = (coords.x - 0.5) * mult;
	float yval = (coords.y - 0.5) * mult;
	float r = sqrt((xval * xval) + (yval * yval)) * uProgress;
	float tpi = (atan(yval / xval) + (sin(uTime / pisquared) / pi)) * pisquared;
	float tCos = sin(uTime + halfpi) + 0.5;
	//the 1 was supposed to be tCos, but for some reason that makes it disappear about half the time now
	float aval = r + (lerp(tCos, sin(tpi + halfpi), sin(tpi) + 0.5) * 0.05) * r;
	
	aval = clamp(aval, 0, 1) * uProgress;
	bool high = aval > 0.75;
	color.rgb = lerp(color.rgb, color.rgb * float3(1, 0.45, 0.15), aval);
	//if (aval > 0.75) color.rgb = lerp(color.rgb, 0, (aval - 0.75) * 3);
	
	return color;
}

technique Technique1 {
	pass VoidShade {
		PixelShader = compile ps_2_0 VoidShade();
	}
	pass DefiledShade {
		PixelShader = compile ps_3_0 DefiledShade();
	}
}