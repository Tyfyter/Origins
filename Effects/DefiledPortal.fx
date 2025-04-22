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
float4 uShaderSpecificData;
float2 uLoopData;
const float PI = 3.14159265359;
float arctan2(float2 offset)
{
	return offset.y / (sqrt(offset.x * offset.x + offset.y * offset.y) + offset.x);
}
float mod(float x)
{
	return x - floor(x);
}

float4 DefiledPortal(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{

    float2 NormalUV = coords;
    NormalUV -= 0.5;
    NormalUV *= 2.;
    float2 pixelatedUV = round(NormalUV * (16.)) / 16.;
    float2 playerPos = float2(uShaderSpecificData.z,uShaderSpecificData.w);
    float d = length(pixelatedUV);
    float angle = atan2(pixelatedUV.y, pixelatedUV.x);
    float2 VortexUV = float2(sin(angle + d * 5 + floor(uTime * 5)), (d));
    float circle = saturate((0.5 - distance(float2(0.5 * 2.0 - 1.0, 0.5 * 2.0 - 1.0), NormalUV)));
    float4 finalCol = tex2D(uImage1, VortexUV) * lerp(uColor, uSecondaryColor, saturate(VortexUV.x)).rgbr * smoothstep(1, 0., d);
    finalCol = round(finalCol * (16)) / 16;
    
    
    
    return finalCol * 3 - circle;
}

technique Technique1
{
    pass DefiledPortal
    {
        PixelShader = compile ps_3_0 DefiledPortal();
    }
}
