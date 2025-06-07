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
float ndot(float2 a, float2 b)
{
    return a.x * b.x - a.y * b.y;
}
float DiamondShape(in float2 p, in float2 b)
{
    p = abs(p);
    float h = clamp(ndot(b - 2.0 * p, b) / dot(b, b), -1.0, 1.0);
    float d = length(p - 0.5 * b * float2(1.0 - h, 1.0 + h));
    return d * sign(p.x * b.y + p.y * b.x - b.x * b.y);
}
float2 CircleShape(float2 uv, float r)
{
    return length(uv) - r;

}
float4 ShimmerConstructSDF(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    coords -= .5;
    coords *= 2.;
    float2 repeatedUV = coords;
    float diamond = (DiamondShape(repeatedUV, 0.4 + 0.3 * cos(uTime * 3 + +float2(0.0, 5))));
    float circle = (CircleShape(repeatedUV, lerp(0.5, 0.7, sin(uTime))));
    float shape = lerp(diamond, circle, saturate(sin(uTime)));
    float glow = smoothstep(0.4,0,shape) * (0.02 / saturate(shape));
    float outline = step(step(shape, shape + 1), 0.0);
    float3 outlineColorWithGlow = step(0, shape) * uColor * glow;
    float d = length(repeatedUV);
    float angle = atan2(repeatedUV.y, repeatedUV.x);
    float2 VortexUV = float2(sin(angle + d * 5 - uTime), d);
    float4 noise = tex2D(uImage1, VortexUV - float2(0, uTime) * 0.3 + uTargetPosition);
    float4 noise2 = tex2D(uImage1, VortexUV + float2(0, uTime) * 0.5 + float2(0.3, 0.3) );
    float4 starTexture = tex2D(uImage2, repeatedUV + uWorldPosition);
    noise += noise2;
    noise *= smoothstep(0.,0.7,d);
    noise = floor(noise * 2) / 2;
    float repeatShape = noise * step(shape, 0);
    repeatShape = saturate(repeatShape);
    float3 shimmerGrad = lerp(uColor * 1.25, uSecondaryColor, saturate(VortexUV.xxx));
    float starShape = clamp(1 - abs(((repeatedUV.x * 0.5) * (repeatedUV.y * 0.5f)) * ( 1000 + 100 * cos(uTime * 10))),
    0, 1);
    starShape += starShape * 3;
    starShape *= smoothstep(0., 0.4, 1 / (d * 300) * 5);
    float4 finalColor = float4(repeatShape * shimmerGrad, 0)
    + float4(outlineColorWithGlow * shimmerGrad, outline)
    + float3(starShape * shimmerGrad).rgbb;
    return finalColor;
}

technique Technique1
{
    pass ShimmerConstructSDF
    {
        PixelShader = compile ps_3_0 ShimmerConstructSDF();
    }
}
