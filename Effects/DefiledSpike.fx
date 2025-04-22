sampler2D uImage0 : register(s0);
sampler2D uImage1 : register(s1);
sampler2D uImage2 : register(s2);
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
float4 uShaderSpecificData;
float2 uLoopData;
const float PI = 3.14159265359;
const float SpikeSegmentHeight = 32;
const float SpikeSegmentWidth = 34;
const float SpikeTextureLength = 289;

float arctan2(float2 offset)
{
	return offset.y / (sqrt(offset.x * offset.x + offset.y * offset.y) + offset.x);
}
float mod(float x)
{
	return x - floor(x);
}
float2 Rotate(float2 uv, float amount)
{
	float2 uv2 = uv;
	float s = sin(amount);
	float c = cos(amount);
	uv2.x = (uv.x * c) + (uv.y * -s);
	uv2.y = (uv.x * s) + (uv.y * c);

	return uv2;
    
}


float4 DefiledSpike(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{

    float progress = uShaderSpecificData.x;
    float length = uShaderSpecificData.y;
    float aspectRatio = (length / SpikeSegmentHeight) / SpikeSegmentHeight;
    float SpikeRatio = (length / SpikeTextureLength) / SpikeSegmentHeight;

	
    float2 SpikeUV = float2(coords.x * SpikeRatio, coords.y);
	

    SpikeUV.x -= progress * SpikeRatio;
	
    float spikeEdge = (progress - (0.2));
	
    if (spikeEdge > coords.x)
        return tex2D(uImage2, float2(frac(coords.x * aspectRatio), coords.y));
    else
    {
        SpikeUV = Rotate(SpikeUV, -PI / 2);
        return tex2D(uImage1, float2(SpikeUV.x, SpikeUV.y));

    }
	
}

technique Technique1
{
	pass DefiledSpike
	{
		PixelShader = compile ps_3_0 DefiledSpike();
	}
}
