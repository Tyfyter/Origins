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
float4 uShaderSpecificData;
const float PI = 3.14159265359;

float quintEaseIn(float x)
{
    return x / 3;
}

float4 DefiledLaser(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    coords *= 2;
    coords -= 1;
    float progress = uShaderSpecificData.x;
    float3 purble = float3(0.24, 0.00, 0.22);
    float3 green = float3(0.2, 0.2, 0);
    
    //float3 puble = float3(0.24, 0.00, 0.44);
    //float2 frameCoords = coords - uSourceRect.xy;
    //float perlin = tex2D(uImage1, frameCoords).r;
    //float4 noise = tex2D(uImage2, frameCoords);
    //perlin = (perlin - 0.5) * 2;
    //perlin = (pow(abs(perlin), 0.8) * sign(perlin)) / 2 + 0.5;
	
    //return lerp(float4(perlin * 3 * puble, 1), noise, pow(perlin * 2, 3));
    // John's intended laser below
    
    float laser1 = smoothstep(3, 0.1, abs(coords.y * 3));
    float laser2 = saturate(1 / abs(coords.y * 3));
    laser1 *= laser2;
    //smooth the bloom
    laser1 *= smoothstep(0.5, 0.1, length(coords.y));
    
    //fade in and out
    laser1 *= smoothstep(0.8, 0.1, coords.x);
    
    //more fade in
    float fadeIn = smoothstep(1, 0.8, abs
    (coords.x));
    
    float laserProgressFade = (progress);

    
    float4 laserFlames1 = float4(uColor, 1) * 20;

    laserFlames1 *= laser1;
    
    
    float4 edgeFlameAuraThingy = tex2D(uImage1, float2(coords.x - sign(coords.x) > -1 ? uTime : -uTime, coords.y)) * lerp(green.rgbr, purble.rgbr, sin((coords.x - uTime) * 3) * 5);
    edgeFlameAuraThingy *= lerp(smoothstep(1, 0.4, coords.x / 2), 1, laser1) * smoothstep(0.5, 0.1, abs(coords.y) / 2);
    float4 tvStatic = tex2D(uImage2, (coords * 16 + floor(uTime * 5) * 155) / 16);
    tvStatic = (floor(tvStatic * 4) / 4);
    float4 finalColor = (laser1.xxxx * lerp(purble.rgbr, green.rgbr, sin((coords.x - uTime) * 3) * 5) + laserFlames1 + edgeFlameAuraThingy) * fadeIn;
    return lerp(finalColor, tvStatic, laser1);
  

}

technique Technique1
{
    pass DefiledLaser
    {
        PixelShader = compile ps_3_0 DefiledLaser();
    }
}
