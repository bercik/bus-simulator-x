sampler s0;
float4 gradientColor;

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    // TODO: add your pixel shader code here.
    float4 color = tex2D(s0, coords);

	if (!any(color)) return color;

	color.a = gradientColor.a;

	float halfStep = gradientColor.rgb * .3;

	if (coords.x < .5)
		color.rgb = gradientColor.rgb - halfStep + (halfStep * coords.x * 2);
	else
		color.rgb = gradientColor.rgb - (halfStep * (coords.x - .5) * 2);

	return color;
}

technique Gradient
{
    pass Pass1
    {
        // TODO: set renderstates here.

        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}