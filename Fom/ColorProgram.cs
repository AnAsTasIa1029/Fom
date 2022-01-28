namespace ShadowWinForms
{
    public class ColorProgram : ShaderProgram
    {
        public int a_Color; //а - атрибуты
        public int u_MvpMatrixFromLight; //u - uniform
        public int u_ShadowMap;
        public int a_Normal;
        public int u_ModelMatrix;
        public int u_NormalMatrix;
        public int u_LightColor;
        public int u_LightPosition;
        public int u_AmbientLight; //окружающий свет
        public int u_IsShadowReceiver;
    }
}