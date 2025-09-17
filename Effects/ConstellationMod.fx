sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float2 uWorldPosition;
float3 uColor;
float3 uSecondaryColor;
float uOpacity; // Noise distortion factor
float uSaturation; // Desired edge width IN PIXELS
float uTime;
float4 uSourceRect;
float3 uLightSource;
float2 uImageSize0; // Use this for pixel conversion
float2 uImageSize1;
float4 uShaderSpecificData;
float4 uNodePositions;

float saturate(float x) {
    return clamp(x, 0.0, 1.0);
}
float distanceToLine(float2 p, float2 a, float2 b) {
    float2 ab = b - a;
    float2 ap = p - a;
    float denom = dot(ab, ab);
    if (denom == 0.0) { 
        return length(ap);
    }
    float t = saturate(dot(ap, ab) / denom);
    float2 closest = a + t * ab;
    return length(p - closest);
}

float4 ConstellationMod(float2 coords : TEXCOORD0) : COLOR0
{
    float2 screenPosition = uShaderSpecificData.xy;
    float2 uResolution = uShaderSpecificData.zw;
    float2 speed = uColor.xy;

    float2 pcoords = coords * uResolution;
    float2 node0 = uNodePositions.xy * uResolution;
    float2 node1 = uNodePositions.zw * uResolution;
    
    float2 tileUV = frac(pcoords / uImageSize0 - screenPosition);
    float3 tex = tex2D(uImage0, tileUV).xyz;

    float2 ab = node1 - node0;
    float2 dir = normalize(ab);
    float2 perp = float2(-dir.y, dir.x);

    float2 noiseTexture = coords + speed * uTime * 10.;
    float noise = tex2D(uImage1, noiseTexture / 3.).x - 0.5;

    float2 distortedCoords = coords + perp * noise * (uOpacity / uResolution);

    float mdist = distanceToLine(distortedCoords * uResolution, node0, node1);
    float close = step(mdist, uSaturation);

    return float4(tex, 1.0) * close;
}

technique Technique1 {
    pass ConstellationMod {
        PixelShader = compile ps_3_0 ConstellationMod();
    }
}