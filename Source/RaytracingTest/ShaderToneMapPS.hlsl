#include "ShaderToneMap.hlsli"

float4 main(in float4 position : SV_Position, in float2 texCoord : TEXCOORD) : SV_Target0
{
    float3 color = g_Texture0.Sample(g_LinearClampSampler, texCoord).rgb;
    float lumAvg = g_Texture1.Load(0).x;

    return float4(color / min(0.001 + lumAvg, 65504) * g_Globals.middleGray, 1.0);
}