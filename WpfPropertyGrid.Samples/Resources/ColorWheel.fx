sampler2D opacityMask : register(S0);
float2 center : register(C0);

#define PI 3.141592653

float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float2 vec = center - uv; // vector from center
    float angle = atan2(vec.x, vec.y) / 2 / PI; // normalize radians to 0..1
    float saturation = length(vec) * 2;
    float3 hsv = float3(angle, saturation, 1); // hue is angle
    float3 rgb = hsv2rgb(hsv);
    float a = tex2D(opacityMask, uv).a; // get alpha from opacity mask
    return float4(rgb.x * a, rgb.y * a, rgb.z * a, a); // premultiply alpha
}
