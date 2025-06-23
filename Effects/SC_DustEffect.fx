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

float4 SC_DustEffect(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    coords *= 2;
    coords -= 1;
    float d = length(coords);
    float angle = atan2(coords.y, coords.x);
    float2 VortexUV = float2(sin(angle + d * 5 - uTime), d);
    float starShape = clamp(1 - abs(((coords.x * 0.5) * (coords.y * 0.5f)) * (1000 + 100 * cos(uTime * 10))),
    0, 1);
    starShape += starShape * 3;
    starShape *= smoothstep(0., 0.4, 1 / (d * 300) * 5);
    return starShape + float4(uColor, 1) * smoothstep(0.24,0,d);
}

technique Technique1
{
    pass SC_DustEffect
    {
        PixelShader = compile ps_3_0 SC_DustEffect();
    }
}
