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

float4 LaserLink(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 uv = coords * 2. - 1.;
    float laserLength = uShaderSpecificData.x;
    float3 laserMainColor = uColor;
    float3 laserSecondaryColor = uSecondaryColor;
    float halfStar = abs(1 / uv.y) * 0.1;
    float fadeOut = smoothstep(0.9, 0.1, length(uv.y));
    halfStar *= fadeOut;
    float4 laserMain = halfStar * laserMainColor.rgbr;
    float4 laserSecondary = halfStar * fadeOut * laserSecondaryColor.rgbr;
    
    return lerp(laserMain,laserSecondary,laserMain.r);

}



technique Technique1
{
    pass LaserLink
    {
        PixelShader = compile ps_3_0 LaserLink();
    }
}
