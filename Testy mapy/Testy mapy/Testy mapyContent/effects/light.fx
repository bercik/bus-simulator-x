sampler s0;

float4 globalLightColor;

Texture firstLightmap;
sampler firstLightmapSampler = sampler_state {
    texture = <firstLightmap>;
};
Texture secondLightmap;
sampler secondLightmapSampler = sampler_state {
    texture = <secondLightmap>;
};

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    // TODO: add your pixel shader code here.
	float4 color = tex2D(s0, coords);
	float4 firstLightColor = tex2D(firstLightmapSampler, coords);
	float4 secondLightColor = tex2D(secondLightmapSampler, coords);

	float4 finalColor = color * (globalLightColor + firstLightColor + secondLightColor);

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
