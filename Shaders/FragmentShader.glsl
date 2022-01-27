#version 120

precision mediump float;

uniform vec3 u_LightColor;
uniform vec3 u_LightPosition;
uniform vec3 u_AmbientLight;
uniform int u_IsShadowReceiver;
uniform sampler2D u_ShadowMap;

varying vec3 v_Position;
varying vec4 v_PositionFromLight;
varying vec4 v_Color;
varying vec3 v_Normal;

// Recalculate the z value from the rgba
float unpackDepth(const in vec4 rgbaDepth)
{
    const vec4 bitShift = vec4(1.0, 1.0/256.0, 1.0/(256.0*256.0), 1.0/(256.0*256.0*256.0));
       
    // Use dot() since the calculations is same
    float depth = dot(rgbaDepth, bitShift);

    return depth;
}

void main()
{
    vec3 shadowCoord = (v_PositionFromLight.xyz/v_PositionFromLight.w)/2.0 + 0.5;
    vec4 rgbaDepth = texture2D(u_ShadowMap, shadowCoord.xy);
    float depth = unpackDepth(rgbaDepth); // Recalculate the z value from the rgba
    float visibility = (shadowCoord.z > depth + 0.0015) ? 0.7 : 1.0;

    // Normalize the normal because it is interpolated and
    // not 1.0 in length any more
    vec3 normal = normalize(v_Normal);

    // Calculate the light direction and make its length 1
    vec3 lightDirection = normalize(u_LightPosition - v_Position);

    // The dot product of the light direction and the
    // orientation of a surface (the normal)
    float nDotL = max(dot(lightDirection, normal), 0.0);

    vec3 diffuse = u_LightColor * v_Color.rgb * nDotL;
    vec3 ambient = u_AmbientLight * v_Color.rgb;

    if (u_IsShadowReceiver == 1)
    {
        gl_FragColor = vec4((diffuse + ambient) * visibility, v_Color.a);
    }
    else
    {
        gl_FragColor = vec4(diffuse + ambient, v_Color.a);
    }
}
