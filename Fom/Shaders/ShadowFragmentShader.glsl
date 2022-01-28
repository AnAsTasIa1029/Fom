#version 120

precision mediump float;

void main()
{
    const vec4 bitShift = vec4(1.0, 256.0, 256.0 * 256.0, 256.0 * 256.0 * 256.0);
    const vec4 bitMask = vec4(1.0/256.0, 1.0/256.0, 1.0/256.0, 0.0);

    // Calculate the value stored into each byte
    vec4 rgbaDepth = fract(gl_FragCoord.z * bitShift);

    // Cut off the value which do not fit in 8 bits
    rgbaDepth -= rgbaDepth.gbaa * bitMask;

    gl_FragColor = rgbaDepth;    
}
