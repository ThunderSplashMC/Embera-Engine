#version 330 core

out vec4 FragColor;

in vec2 texCoords;

uniform sampler2D S_RENDERED_TEXTURE;
uniform vec2 S_RESOLUTION;

void main() {

    vec2 uv = gl_FragCoord.xy / S_RESOLUTION;
    uv *=  1.0 - uv.yx;   //vec2(1.0)- uv.yx; -> 1.-u.yx; Thanks FabriceNeyret !
    
    float vig = uv.x*uv.y * 15.0; // multiply with sth for intensity
    
    vig = pow(vig, 0.25); // change pow for modifying the extend of the  vignette

    
    FragColor = vec4(vig) * texture(S_RENDERED_TEXTURE, texCoords);

}