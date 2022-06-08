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
float uThreshold0;
float uThreshold1;
float4 uShaderSpecificData;

float4 RedFade(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	float4 color = tex2D(uImage0, coords);
	float2 center = (coords.x-0.5,coords.y-0.5);
	float sinr = sin(uRotation);
	float cosr = cos(uRotation);
	float2 rotCoords = ((center.x*cosr-center.y*sinr), (center.x*sinr+center.y*cosr))+(0.5,0.5);
	float4 color2 = tex2D(uImage1, rotCoords);//tex2D(uImage1, float2((round((coords.x*100+uOffset.x)/3)/300)%1, (round((coords.y*100+uOffset.y)/3)/300)%1));
	/*if(color2.x>uThreshold0){
		color.w*=1;
	}else{
		if(color2.x>uThreshold1){
			color.w*=0.25f;
		}else{
			color.w*=0;
		}
	}*/
	if(color2.x>uThreshold0){
		if(color2.x>uThreshold1){
			color.w*= 1;
		}else{
			color.w*= (color2.x-uThreshold0)/(uThreshold1-uThreshold0);
		}
	}else{
		color.w = 0;
	}
	//color.w = color.w*color2.x;
	return color*sampleColor;
	}

technique Technique1{
	pass RedFade{
		PixelShader = compile ps_2_0 RedFade();
	}
}