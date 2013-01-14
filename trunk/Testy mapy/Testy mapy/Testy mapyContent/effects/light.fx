sampler s0;

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    // TODO: add your pixel shader code here.
	float4 color = tex2D(s0, coords);

	float4 light = float4(1, 1, 1, 1);

    return (color * light);
}

technique Light
{
    pass Pass1
    {
        // TODO: set renderstates here.

        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
