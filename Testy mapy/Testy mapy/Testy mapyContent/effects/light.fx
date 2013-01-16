sampler s0;

float4 globalLightColor;

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    // TODO: add your pixel shader code here.
	float4 color = tex2D(s0, coords);

    return (color * globalLightColor);
}

technique Light
{
    pass Pass1
    {
        // TODO: set renderstates here.

        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
