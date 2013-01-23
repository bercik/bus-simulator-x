sampler s0;

float4 globalLightColor;

Texture lightmap;
sampler lightmapSampler = sampler_state {
    texture = <lightmap>;
};

float maxColor = 1.0;

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    // TODO: add your pixel shader code here.
	float4 color = tex2D(s0, coords);

	float4 lightColor = tex2D(lightmapSampler, coords);

	float4 finalColor = (color * (globalLightColor + lightColor));

	return finalColor;
}

technique Light
{
    pass Pass1
    {
        // TODO: set renderstates here.

        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
