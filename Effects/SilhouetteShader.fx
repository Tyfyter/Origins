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


float4 SilhouetteShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{

    return float4(uColor, 1) * tex2D(uImage0,coords).a;
	
}

technique Technique1
{
    pass SilhouetteShader
    {
        PixelShader = compile ps_3_0 SilhouetteShader();
    }
}
