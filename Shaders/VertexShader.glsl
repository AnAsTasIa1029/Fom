attribute vec4 a_Position;
attribute vec4 a_Color;
attribute vec4 a_Normal;

uniform mat4 u_MvpMatrix;
uniform mat4 u_MvpMatrixFromLight;
uniform mat4 u_ModelMatrix;
uniform mat4 u_NormalMatrix;

varying vec3 v_Position;
varying vec4 v_PositionFromLight;
varying vec4 v_Color;
varying vec3 v_Normal;

void main()
{
    gl_Position = u_MvpMatrix * a_Position;

    // Calculate the vertex position in the world coordinate
    v_Position = vec3(u_ModelMatrix * a_Position);

    v_PositionFromLight = u_MvpMatrixFromLight * a_Position;
    v_Normal = normalize(vec3(u_NormalMatrix * a_Normal));
    v_Color = a_Color;
}
