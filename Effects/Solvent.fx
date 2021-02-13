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
float4 uSourceRect2;

float4 Dissolve(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
	float4 color = tex2D(uImage0, coords);
	float fade = uTime%3;
	float frameY = (coords.y*uSecondaryColor.b-uSecondaryColor.r)/uSecondaryColor.g;
	float2 noiseCoords = float2(coords.x, frameY); ///uImageSize1;*uSecondaryColor.b
	float2 offset = float2(0.5+sin(uTime), 0.5+cos(uTime))*0.1;
	float4 color2 = tex2D(uImage1, noiseCoords+offset);
	float value;
	float fade2 = fade%1;
	if(fade<1) {
		value = (color2.r*fade2)+(color2.b*(1-fade2));
	}else if(fade<2) {
		value = (color2.g*fade2)+(color2.r*(1-fade2));
	}else {
		value = (color2.b*fade2)+(color2.g*(1-fade2));
	}
	value = pow(value, 2);
	if(value < 0.1){
		color.rb *= 0.25;
		color.g *= 0.3;
	}else if(value<0.15){
		color.rb *= 0.5;
		color.g *= 0.55;
	}else{
		color.rb *= 0.95;
	}
	return color;
}

technique Technique1{
	pass Dissolve {
		PixelShader = compile ps_2_0 Dissolve();
	}
}