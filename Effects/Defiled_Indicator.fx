sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
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

const float PI = 3.14159265359;
float arctan2(float2 offset)
{
    return offset.y / (sqrt(offset.x * offset.x + offset.y * offset.y) + offset.x);
}
float mod(float x)
{
    return x - floor(x);
}

float4 Defiled_Indicator(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float3 puble = float3(0.24, 0.00, 0.44);
    float3 pink = float3(0.95, 0.75, 0.89);
    float4 noise = tex2D(uImage1,coords);
    
    noise.rgb *= pink;
    return noise;
}

technique Technique1
{
    pass Defiled_Indicator
    {
        PixelShader = compile ps_3_0 Defiled_Indicator();
    }
}
