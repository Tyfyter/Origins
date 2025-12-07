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

float2 Rotate(float2 uv, float amount)
{
    float2 uv2 = uv;
    float s = sin(amount);
    float c = cos(amount);
    uv2.x = (uv.x * c) + (uv.y * -s);
    uv2.y = (uv.x * s) + (uv.y * c);

    return uv2;
    
}


float4 FireShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 uv = coords * 2;
    uv -= 1;
    float offset = uShaderSpecificData.y;
    float4 noise = tex2D(uImage1, coords * 2 + offset + float2(0, uTime) * 0.78);
    float4 noiseMask = tex2D(uImage1, Rotate(coords,3.1415) + offset + float2(0.25, 0.25) - float2(0, uTime) * 0.2);
    float3 shineColor = uColor;
    float3 smokeColor = uSecondaryColor;
    float progress = uShaderSpecificData.x;
    float d = length(uv);
    float noiseAlphaFade = smoothstep(0.6 * lerp(1,0,progress), 0.0, d);
    
    // Fire Shader Slop
    float3 fireCol = shineColor;

    
    noise.a = noiseMask.r;
    noise *= noiseAlphaFade;
    
    noise.rgb *= lerp(fireCol,smokeColor, progress);
    
    noise.a *= 0.5;
    
    return noise;
    
	
}

technique Technique1
{
    pass FireShader
    {
        PixelShader = compile ps_3_0 FireShader();
    }
}
