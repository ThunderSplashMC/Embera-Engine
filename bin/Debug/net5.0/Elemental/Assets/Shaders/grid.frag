#version 330 core

layout(location = 0) out vec4 color;

in vec2 v_TexCoord;
in vec3 FragPos;

void main()
{
//	vec2 cellCoord = convert_to_cell_coords(FragPos.xy, cellSize);
//	vec2 cutoff = convert_to_cell_coords(vec2(1.0 - lineWidth), cellSize);
//
//	vec2 alpha = step(cutoff, cellCoord);
//
//	color = vec4(0.5, 0.5, 0.5, min(alpha.x, alpha.y) * (1 - FragPos));
    if(fract(v_TexCoord.x / 0.001f) < 0.01f || fract(v_TexCoord.y / 0.001f) < 0.01f) {
        float d = distance((v_TexCoord) * 0.5, FragPos.xz) * 0.03;
        color = vec4(1.0, 1.0, 1.0, 1 - d);
    } else {
            color = vec4(0);
    }
}