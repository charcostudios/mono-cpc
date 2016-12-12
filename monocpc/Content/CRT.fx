// XNACPC - An Amstrad CPC Emulator written in C#, using XNA.
// (c) Gavin Pugh 2011
// Modified to work in monogame
// (c) CharcoStudios 2016

sampler  TextureSampler  : register(s0);

float2 Viewport;
float TextureHeight;
float ScreenHeight;

struct VertexShaderOutput
{
	float4 Position :  POSITION0;
	float4 Color : COLOR0;
	float2 TextureCordinate : TEXCOORD0;
};

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
		float4 color = tex2D(TextureSampler, input.TextureCordinate) *input.Color;

		// Find height of a CPC pixel
		float pix_height = (ScreenHeight / TextureHeight);

		// Divide into scanlines. Use modf() to grab fractional scanline part
		float scanline = (input.TextureCordinate[1] * ScreenHeight) / pix_height;
		int scanline_num;
		float scanline_part = modf(scanline, scanline_num);

		// Right now it's 3 XNA pixels per CPC pixel. So the values of the fractional part are 0.0, 0.3333, and 0.6666
		// Just going brute force for it...
		float blend_val;
		float blend_val_next_line;
		if (scanline_part < 0.4)		//< 0.0
		{
			// Full color for this line.
			blend_val = 1.0;
			blend_val_next_line = 0.0;
		}
		else if (scanline_part < 0.7) //< 0.33
		{
			// 95% color, just a little darker. 5% is mixed from the next line, for a little bleeding.
			blend_val = 0.90;
			blend_val_next_line = 0.05;
		}
		else							//< 0.66
		{
			// 90% color, a little darker again. 15% is mixed from the next line, for a little bleeding.
			blend_val = 0.75;
			blend_val_next_line = 0.15;
		}

		// Grab color from next line
		float2 next_line = input.TextureCordinate;
		next_line[1] = (next_line[1] + (1.0 / TextureHeight));
		float4 next_line_color = tex2D(TextureSampler, next_line) *input.Color;

		// Mix it all
		color[0] = (color[0] * blend_val) + (next_line_color[0] * blend_val_next_line);
		color[1] = (color[1] * blend_val) + (next_line_color[1] * blend_val_next_line);
		color[2] = (color[2] * blend_val) + (next_line_color[2] * blend_val_next_line);

		return color;

}

technique CrtShader
{
	pass Pass1
	{
		PixelShader = compile ps_4_0_level_9_1  PixelShaderFunction();
	}
}
