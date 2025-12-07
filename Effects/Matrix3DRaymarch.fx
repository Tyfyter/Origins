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

float sdSphere(float3 p, float s)
{
    return (length(p) - s);
}

float sdSpike(float3 p, float s)
{
    p = abs(p);
    float m = p.x + p.y + p.z - s;
    float3 q;
    if (3.0 * p.x < m)
        q = p.xyz;
    else if (3.0 * p.y < m)
        q = p.yzx;
    else if (3.0 * p.z < m)
        q = p.zxy;
    else
        return m * 0.57735027;
    
    float k = clamp(0.5 * (q.z - q.y + s), 0.0, s);
    return length(float3(q.x, q.y - s + k, q.z - k)); 

}

float sdHexPrism(float3 p, float2 h)
{
    float3 k = float3(-0.8660254, 0.5, 0.57735);
    p = abs(p);
    p.xy -= 2.0 * min(dot(k.xy, p.xy), 0.0) * k.xy;
    float2 d = float2(
       length(p.xy - float2(clamp(p.x, -k.z * h.x, k.z * h.x), h.x)) * sign(p.y - h.x),
       p.z - h.y);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}
float sdBox(float3 p, float3 b)
{
    float3 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
}
float3 rotateZ(float3 p,float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    float3 p2 = p;
    p2.y = (p.x * c) + (p.y * -s);
    p2.x = (p.x * s) + (p.y * c);

    return p2;

}
float3 rotateX(float3 p, float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    float3 p2 = p;
    p2.y = (p.z * c) + (p.y * -s);
    p2.z = (p.z * s) + (p.y * c);

    return p2;

}

float mutliLerp(float value1, float value2, float value3, float t)
{
    float value = 0;
    if(t < 0.5)
    {
        value = lerp(value1, value2, (t) * 2);

    }
    else
    {
        value = lerp(value2, value3, (t - 0.5) * 2);

    }
    return value;

}

float map(float3 p)
{
    p = rotateX(p,uTime);
    p = rotateZ(p, uTime);
    
    float shape = sdSpike(p, 3);
   
    return shape;
}
// the outlines, i forgot how it works
float3 normal(float3 p, float s)
{

    float2 off = float2(s,0);
    float3 n = float3(map(p + off.xyy).x, map(p + off.yxy).x, map(p + off.xyy).x) -
    float3(map(p - off.xyy).x, map(p - off.yxy).x, map(p - off.xyy).x);
    return normalize(n);

}

float4 EscapeTheMatrix(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 uv = coords * 2. - 1;
    float3 rayOrigin = float3(0, 0, -10);
    float3 rayDir = normalize(float3(uv.x, uv.y, 1));
    float t = 0;
    float3 col = uColor;
    //raymarching
    for (int i = 0; i < 25; i++)
    {
        float3 p = rayOrigin + rayDir * t;
        float d = map(p);
        t += d;
        if (abs(d) < 0.01)
        {
            float edge = 0.005 * t;
            float edgeAmount = length(normal(p, 0.15) - normal(p, edge));
            col += smoothstep(0, .1, edgeAmount) * uSecondaryColor;

            
        }


    }   

    // clear the bg and only draw the shape at full brightness
    float4 finalShape = float4(col, 1) / lerp(1, 0, t / 60);
    float bloom = smoothstep(0.1, 1, 1/length(uv) * 0.15);
    return lerp(bloom * uSecondaryColor.rgbr, finalShape, finalShape.r);
    
}



technique Technique1
{
    pass EscapeTheMatrix
    {
        PixelShader = compile ps_3_0 EscapeTheMatrix();
    }
}
