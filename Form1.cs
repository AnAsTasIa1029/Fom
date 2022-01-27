using System;
using ShadowWinForms.Properties;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using Utils;
using OpenTK;
using System.Drawing;

namespace ShadowWinForms
{
    public partial class Form1 : Form
    {
        private GLControl glControl;

        private System.Windows.Forms.Timer timer;
        private DateTime last = DateTime.Now;

        private const int OFFSCREEN_WIDTH = 2048;
        private const int OFFSCREEN_HEIGHT = 2048;



        private const float fovyAngle = 45f * (float)Math.PI / 180f;  //угол обзора
        private const float fovyAngleFromLight = 70f * (float)Math.PI / 180f; //угол обзора от света
        private const float planeAngle = -45f * (float)Math.PI / 180f; // угол плоскости

        private const int LIGHT_X = 10, LIGHT_Y = 70, LIGHT_Z = 50; // положение источника света

        private Vector3 lightPos = new Vector3(LIGHT_X, LIGHT_Y, LIGHT_Z);
        private Vector3 lightColor = new Vector3(1f, 1f, 1f);
        private Vector3 ambientLight = new Vector3(0.2f, 0.2f, 0.2f); //окружающий свет 

        private bool canDraw = false;

        private float angleStep = 40f; // угол поворота (градусы/сек)

        // матрица вида и проекцыы для карты теней
        private Matrix4 viewMatrixFromLight; //вид матрицы от света
        private Matrix4 projMatrixFromLight; //проекции матрицы от света 

        // подготовка матрицы проекции вида для рисования 
        private Matrix4 modelMatrix;
        private Matrix4 viewMatrix;
        private Matrix4 projMatrix;
        private Matrix4 mvpMatrix;
        private Matrix4 normalMatrix;

        private float currentAngle = 0f; // текущий угол поворота

        // Матрица проекции вида модели от источника света (для треугольника?)
        private Matrix4 mvpMatrixFromLight_t;

        // Матрица проекции вида модели от источника света (для плоскости)
        private Matrix4 mvpMatrixFromLight_p;

        // Матрица проекции вида модели от источника света (для цилиндра)
        private Matrix4 mvpMatrixFromLight_h;

        private class FrameBuffer // буфер кадров
        {
            public int id;
            public int texture;
        }
        private FrameBuffer fbo; //объект кадрового буфера

        private class ShaderProgram
        {
            public int id;
            public int a_Position;
            public int u_MvpMatrix;
        }

        private class ShadowProgram : ShaderProgram
        {
        }
        private ShadowProgram shadowProgram = new ShadowProgram();

        private class ColorProgram : ShaderProgram
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
        private ColorProgram colorProgram = new ColorProgram();

        private class Obj
        {
            public VertexBufferObject vertexBuffer;
            public VertexBufferObject colorBuffer;
            public VertexBufferObject normalBuffer;
            public ElementBufferObject indexBuffer;
            public int numIndices;
        }

        private class Cube : Obj { }
        private Cube cube;

        private class Plane : Obj { }
        private Plane plane;

        //private class Cylindr : Obj { }
        //private Cylindr cylindr;

        private class HexagonalPrism: Obj { }
        private HexagonalPrism hexagonalPrism;


        private class VertexBufferObject
        {
            public int id;
            public int num;
            public VertexAttribPointerType type;
        }

        private class ElementBufferObject
        {
            public int id;
            public DrawElementsType type;
        }

        public Form1()
        {

            //Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
            InitializeComponent();

            Text = Resources.titleText;
            groupBoxRotationSpeed.Text = Resources.groupBoxRotationSpeedText;
            labelDegreesInSec.Text = Resources.labelDegreesInSecText;



            textBoxRotationSpeed.Text = angleStep.ToString();

            glControl = new GLControl();
            glControl.Load += GlControl_Load;
            glControl.Paint += GlControl_Paint;
            glControl.Resize += GlControl_Resize;

            glControl.Dock = DockStyle.Fill;
            glControl.MakeCurrent();
            tableLayoutPanel1.Controls.Add(glControl, 0, 0);
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color4.White);



            // Загрузка шейдера из файла 
            string vShadowShaderSource = null;
            string fShadowShaderSource = null;
            string vShaderSource = null;
            string fShaderSource = null;
            ShaderLoader.LoadShader("./Shaders/ShadowVertexShader.glsl", out vShadowShaderSource);
            ShaderLoader.LoadShader("./Shaders/ShadowFragmentShader.glsl", out fShadowShaderSource);
            ShaderLoader.LoadShader("./Shaders/VertexShader.glsl", out vShaderSource);
            ShaderLoader.LoadShader("./Shaders/FragmentShader.glsl", out fShaderSource);
            //    if (vShadowShaderSource == null || fShadowShaderSource == null ||
            //     vShaderSource == null || fShaderSource == null)
            //  {
            //      Logger.Append("Не удалось загрузить шейдер вершин или фрагментов");
            //     return;
            // }

            // Загрузить шейдер из файла и инициализировать шейдеры для создания карты теней
            shadowProgram.id = ShaderLoader.CreateProgram(vShadowShaderSource, fShadowShaderSource);
            if (shadowProgram.id == 0)
            {
                Logger.Append("Не удалось создать программу теней");
                return;
            }
            shadowProgram.a_Position = GL.GetAttribLocation(shadowProgram.id, "a_Position");
            shadowProgram.u_MvpMatrix = GL.GetUniformLocation(shadowProgram.id, "u_MvpMatrix");
            // if (shadowProgram.a_Position < 0 || shadowProgram.u_MvpMatrix < 0)
            //    {
            //       Logger.Append("Не удалось получить местоположение хранилища атрибута или однородной переменной из shadowProgram");
            //      return;
            //  }

            // Инициализация шейдеров для рисования
            colorProgram.id = ShaderLoader.CreateProgram(vShaderSource, fShaderSource);
            // if (colorProgram.id == 0)
            //   {
            //     Logger.Append("Failed to crate a normal program");
            //     return;
            // }
            colorProgram.a_Position = GL.GetAttribLocation(colorProgram.id, "a_Position");
            colorProgram.a_Color = GL.GetAttribLocation(colorProgram.id, "a_Color");
            colorProgram.a_Normal = GL.GetAttribLocation(colorProgram.id, "a_Normal");
            colorProgram.u_MvpMatrix = GL.GetUniformLocation(colorProgram.id, "u_MvpMatrix");
            colorProgram.u_MvpMatrixFromLight = GL.GetUniformLocation(colorProgram.id, "u_MvpMatrixFromLight");
            colorProgram.u_ShadowMap = GL.GetUniformLocation(colorProgram.id, "u_ShadowMap");
            colorProgram.u_ModelMatrix = GL.GetUniformLocation(colorProgram.id, "u_ModelMatrix");
            colorProgram.u_NormalMatrix = GL.GetUniformLocation(colorProgram.id, "u_NormalMatrix");
            colorProgram.u_LightColor = GL.GetUniformLocation(colorProgram.id, "u_LightColor");
            colorProgram.u_LightPosition = GL.GetUniformLocation(colorProgram.id, "u_LightPosition");
            colorProgram.u_AmbientLight = GL.GetUniformLocation(colorProgram.id, "u_AmbientLight");
            colorProgram.u_IsShadowReceiver = GL.GetUniformLocation(colorProgram.id, "u_IsShadowReceiver");
            if (colorProgram.a_Position < 0 || colorProgram.a_Color < 0 ||
                 colorProgram.u_MvpMatrix < 0 || colorProgram.u_MvpMatrixFromLight < 0 ||
                 colorProgram.u_ShadowMap < 0 || colorProgram.u_ModelMatrix < 0 ||
                 colorProgram.u_NormalMatrix < 0 || colorProgram.u_LightColor < 0 ||
                 colorProgram.u_LightPosition < 0 || colorProgram.u_AmbientLight < 0 ||
                 colorProgram.u_IsShadowReceiver < 0)
            {
                Logger.Append("Не удалось получить местоположение хранения атрибута или однородной переменной из normalProgram");
                return;
            }

            // Установка информацию о вершинах
            cube = InitVertexBuffersForCube();
            plane = InitVertexBuffersForPlane();
            // cylindr = InitVertexBuffersForCylinder();

            hexagonalPrism = InitVertexBuffersForHexagonalPrism();

            if (cube == null || plane == null )//|| cylindr == null)
            {
                Logger.Append("Не удалось установить информацию о вершине");
                return;
            }

            // Инициализирование объекта буфера кадров 
            fbo = InitFramebufferObject();
            if (fbo == null)
            {
                Logger.Append("Не удалось инициализировать объект буфера кадров (FBO)");
                return;
            }
            GL.ActiveTexture(TextureUnit.Texture0); // Установка объекта текстуры в текстурный блок
            GL.BindTexture(TextureTarget.Texture2D, fbo.texture);

            // Установка цвета и включение проверки глубины
            GL.ClearColor(Color4.DarkSlateBlue);
            GL.Enable(EnableCap.DepthTest); // проверка глубины 

            viewMatrixFromLight = Matrix4.LookAt(LIGHT_X, LIGHT_Y, LIGHT_Z, 0f, 0f, 0f, 0f, 1f, 0f);
            viewMatrix = Matrix4.LookAt(-5f, 15f, 10f, 0f, 0f, 0f, 0f, 1f, 0f);
            SetProjMatrixes();

            canDraw = true;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = (int)(1000f / 60f);
            timer.Tick += GlControl_Update;
            timer.Start();
        }

        private void GlControl_Update(object sender, EventArgs e)
        {
            currentAngle = Animate(currentAngle);
            glControl.Invalidate();
        }

        private float Animate(float angle)
        {
            DateTime now = DateTime.Now;
            TimeSpan elapsed = now - last;
            last = now;
            // Обновление текущего угола поворота (скорректированный с учетом прошедшего времени)
            float newAngle = angle + (angleStep * (float)elapsed.TotalMilliseconds) / 1000f;
            return (newAngle % 360);
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            if (canDraw)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo.id); // Изменение назначение рисунка на FBO (привязка буфера кадров)
                GL.Viewport(0, 0, OFFSCREEN_WIDTH, OFFSCREEN_HEIGHT); // Установка окна просмотра для FBO
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // Clear FBO

                GL.UseProgram(shadowProgram.id); //Установка шейдера для создания карты теней
                // Отрисовка куба и плоскости (для создания карты теней)
                DrawCube(shadowProgram, cube, viewMatrixFromLight, projMatrixFromLight);
                mvpMatrixFromLight_t = mvpMatrix;
                DrawPlane(shadowProgram, plane, viewMatrixFromLight, projMatrixFromLight);
                mvpMatrixFromLight_p = mvpMatrix;
                //DrawCylinder(shadowProgram, cylindr, viewMatrixFromLight, projMatrixFromLight);
                //mvpMatrixFromLight_c = mvpMatrix;
                DrawHexagonalPrism(shadowProgram, hexagonalPrism, viewMatrixFromLight, projMatrixFromLight);
                mvpMatrixFromLight_h = mvpMatrix;

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); // Изменение назначения рисунка на буфер цветов
                GL.Viewport(0, 0, ClientSize.Width, ClientSize.Height);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.UseProgram(colorProgram.id); // установка шейдера для рисования 
                GL.Uniform1(colorProgram.u_ShadowMap, 0); // Pass 0 because gl.TEXTURE0 is enabled
                // Прорисовка куба и плоскости
                GL.UniformMatrix4(colorProgram.u_MvpMatrixFromLight, false, ref mvpMatrixFromLight_t);
                GL.Uniform1(colorProgram.u_IsShadowReceiver, 0);
                DrawCube(colorProgram, cube, viewMatrix, projMatrix);
                GL.UniformMatrix4(colorProgram.u_MvpMatrixFromLight, false, ref mvpMatrixFromLight_p);
                GL.Uniform1(colorProgram.u_IsShadowReceiver, 1);
                DrawPlane(colorProgram, plane, viewMatrix, projMatrix);
                //GL.UniformMatrix4(colorProgram.u_MvpMatrixFromLight, false, ref mvpMatrixFromLight_c);
                //GL.Uniform1(colorProgram.u_IsShadowReceiver, 0);
                //DrawCylinder(colorProgram, cylindr, viewMatrix, projMatrix);

                GL.UniformMatrix4(colorProgram.u_MvpMatrixFromLight, false, ref mvpMatrixFromLight_h);
                GL.Uniform1(colorProgram.u_IsShadowReceiver, 0);
                DrawHexagonalPrism(colorProgram, hexagonalPrism, viewMatrix, projMatrix);
            }

            glControl.SwapBuffers();
        }

        private void SetNormalMatrix() //установка матрицы нормали
        {
            normalMatrix = modelMatrix;
            normalMatrix.Transpose();
            normalMatrix.Invert();
            GL.UniformMatrix4(colorProgram.u_NormalMatrix, false, ref normalMatrix);
        }

        private void DrawCube(ShaderProgram program, Obj o, Matrix4 viewMatrix, Matrix4 projMatrix)
        {
            // Установка угола поворота для модели матрицы
            modelMatrix = Matrix4.CreateRotationY(currentAngle * (float)Math.PI / 180f);
            Draw(program, o, viewMatrix, projMatrix);
        }

        private void DrawPlane(ShaderProgram program, Obj o, Matrix4 viewMatrix, Matrix4 projMatrix)
        {
            modelMatrix = Matrix4.CreateScale(10f, 10f, 10f); //масштаб 
                                                              //Matrix4.CreateRotationY(planeAngle) *
                                                              //Matrix4.CreateRotationZ(planeAngle);
            Draw(program, o, viewMatrix, projMatrix);
        }
        private void DrawHexagonalPrism(ShaderProgram program, Obj o, Matrix4 viewMatrix, Matrix4 projMatrix)
        {
            modelMatrix = Matrix4.CreateRotationY(currentAngle * (float)Math.PI / 180f);
            Draw(program, o, viewMatrix, projMatrix);
        }
        //private void DrawCylinder(ShaderProgram program, Obj o, Matrix4 viewMatrix, Matrix4 projMatrix)
        //{
        //    // Установка угола поворота для модели матрицы
        //    modelMatrix = Matrix4.CreateRotationY(currentAngle * (float)Math.PI / 180f);
        //    Draw(program, o, viewMatrix, projMatrix);
        //}


        private void Draw(ShaderProgram program, Obj o, Matrix4 viewMatrix, Matrix4 projMatrix)
        {
            InitAttributeVariable(program.a_Position, o.vertexBuffer); //переменная атрибута инициализации 
            if (ReferenceEquals(program.GetType(), typeof(ColorProgram)))
            {
                InitAttributeVariable((program as ColorProgram).a_Color, o.colorBuffer);
                InitAttributeVariable((program as ColorProgram).a_Normal, o.normalBuffer);
                SetNormalMatrix();
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, o.indexBuffer.id);

            // Вычисление карты теней
            mvpMatrix = modelMatrix * viewMatrix * projMatrix;
            GL.UniformMatrix4(program.u_MvpMatrix, false, ref mvpMatrix);

            GL.Uniform3(colorProgram.u_LightPosition, ref lightPos);
            GL.Uniform3(colorProgram.u_LightColor, ref lightColor);
            GL.Uniform3(colorProgram.u_AmbientLight, ref ambientLight);

            GL.DrawElements(PrimitiveType.Triangles, o.numIndices, DrawElementsType.UnsignedInt, 0);

        }

        // Назначение объекта буфера и его включение
        private void InitAttributeVariable(int a_attribute, VertexBufferObject buffer)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.id);
            GL.VertexAttribPointer(a_attribute, buffer.num, buffer.type, false, 0, 0);
            GL.EnableVertexAttribArray(a_attribute);
        }
       
        
        private Plane InitVertexBuffersForPlane()
        {
            // Create face
            //  v1------v0
            //  |        | 
            //  |        |
            //  |        |
            //  v2------v3

            // Vertex coordinates
            float[] vertices = new float[]
            {
                //3.0f, -1.7f, 2.5f, // v0-v1-v2-v3
                //-3.0f, -1.7f, 2.5f,
                //-3.0f, -1.7f, -10.5f,
                //3.0f, -1.7f, -10.5f

                3f, -3f, -3f, // v0-v1-v2-v3
                -3f, -3f, -3f,
                -3f, -3f, 3f,
                3f, -3f, 3f
            };

            // Colors
            float[] colors = new float[]
            {
                1f, 0.6f, 0f,
                1f, 0.6f, 0f,
                1f, 0.6f, 0f,
                1f, 0.6f, 0f
            };

            // Normals
            float[] normals = new float[]
            {
                0f, 1f, 0f,
                0f, 1f, 0f,
                0f, 1f, 0f,
                0f, 1f, 0f
            };

            // Индексы вершин
            int[] indices = new int[]
            {
                0, 1, 2, 0, 2, 3
            };

            Plane o = new Plane(); //Использование объекта Object для совместного возврата нескольких объектов буфера
                                   // Запись информации о вершинах в объект буфера
            o.vertexBuffer = InitArrayBufferForLaterUse(vertices, 3, VertexAttribPointerType.Float);
            o.colorBuffer = InitArrayBufferForLaterUse(colors, 3, VertexAttribPointerType.Float);
            o.normalBuffer = InitArrayBufferForLaterUse(normals, 3, VertexAttribPointerType.Float);
            o.indexBuffer = InitElementArrayBufferForLaterUse(indices, DrawElementsType.UnsignedInt);
            //  if (o.vertexBuffer == null || o.colorBuffer == null ||
            //  o.normalBuffer == null || o.indexBuffer == null)
            // {
            //     return null;
            // }

            o.numIndices = indices.Length;

            // Отвязать объект буфера
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            return o;
        }
       

        private Cube InitVertexBuffersForCube()
        {
            // Create a cube
            //    v6----- v5
            //   /|      /|
            //  v1------v0|
            //  | |     | |
            //  | |v7---|-|v4
            //  |/      |/
            //  v2------v3

            // Vertex coordinates
            float[] vertices = new float[]
            {
                //Prizma(/)
               0,0,0,
                1,0,0,   
                2,1,0,
                1,2,0, 
                0,2,0, 
                -1,1,0,
                0,0,2, 
                1,0,2, 
                2,1,2, 
                1,2,2, 
                0,2,2, 
                -1,1,2

                //1, -1, 2,
                //-1, -1, 2,


                //cube

               
                //2f, 2f, 2f, -2f, 2f, 2f, -2f, -2f, 2f, 2f, -2f, 2f,     // v0-v1-v2-v3 front
                //2f, 2f, 2f, 2f, -2f, 2f, 2f, -2f, -2f, 2f, 2f, -2f,     // v0-v3-v4-v5 right
                //2f, 2f, 2f, 2f, 2f, -2f, -2f, 2f, -2f, -2f, 2f, 2f,     // v0-v5-v6-v1 up
                //-2f, 2f, 2f, -2f, 2f, -2f, -2f, -2f, -2f, -2f, -2f, 2f, // v1-v6-v7-v2 left
                //-2f, -2f, -2f, 2f, -2f, -2f, 2f, -2f, 2f, -2f, -2f, 2f, // v7-v4-v3-v2 down
                //2f, -2f, -2f, -2f, -2f, -2f, -2f, 2f, -2f, 2f, 2f, -2f  // v4-v7-v6-v5 back
            };

            // Colors
            float[] colors = new float[]
            {

                   0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, // v0-v1-v2-v3 front
                0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, // v0-v3-v4-v5 right
                0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, // v0-v5-v6-v1 up
                0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, // v1-v6-v7-v2 left
                0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, // v7-v4-v3-v2 down
                0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0　// v4-v7-v6-v5 back

                //1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, // v0-v1-v2-v3 front
                //1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, // v0-v3-v4-v5 right
                //1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, // v0-v5-v6-v1 up
                //1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, // v1-v6-v7-v2 left
                //1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, // v7-v4-v3-v2 down
                //1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, // v4-v7-v6-v5 back
                //1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0,
                //1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0
            };

            // Normals
            float[] normals = new float[]
            {
               0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f,     // v0-v1-v2-v3 front
                1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f,     // v0-v3-v4-v5 right
                0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f,     // v0-v5-v6-v1 up
                -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, // v1-v6-v7-v2 left
                0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, // v7-v4-v3-v2 down
                0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f  // v4-v7-v6-v5 back
            };

            // Indices of the vertices
            int[] indices = new int[]
            {
                //Cube
              //0, 1, 2, 0, 2, 3,       // front
              //  4, 5, 6, 4, 6, 7,       // right
              //  8, 9, 10, 8, 10, 11,    // up
              //  12, 13, 14, 12, 14, 15, // left
              //  16, 17, 18, 16, 18, 19, // down
              //  20, 21, 22, 20, 22, 23  // back

                    //0, 1, 2, 0, 2, 3,
                    //4, 5, 6, 4, 6, 7,
                    //8, 9, 10, 8, 10, 11,
                    //10, 11, 1, 11, 1, 10,
                    //10, 8, 1, 8, 1, 6, 0, 6, 1, 5, 6, 0,
                    //11, 9, 2, 9, 2, 7, 3, 7, 2, 4, 7 ,3 
                   
                    //Prizma
                    0, 2, 1, 
                    0, 3,2,
                    0,4,3, 
                    0,5,4,
                    6,7,8,
                    6,8,9, 
                    6,9,10, 
                    6,10,11,
                    0,1,6, 
                    1,7,6,
                    1,2,7, 
                    2,8,7,
                    2,3,8, 
                    3,9,8,
                    3,4,9, 
                    4,10,9,
                    4,5,11,
                    4,11,10,
                    5,0,6, 
                    5,6,11

                      //0,1,2,     2,3,0,
                      //                 4,5,6,
                      //                 7,8,9,     7,9,10,
                      //                 11,12,13,  11,13,14,
                      //                 15,16,17



            };

            Cube o = new Cube(); //Использование объекта Object для совместного возврата нескольких объектов буфера

            //  Запись информации о вершинах в объект буфера
            o.vertexBuffer = InitArrayBufferForLaterUse(vertices, 3, VertexAttribPointerType.Float);
            o.colorBuffer = InitArrayBufferForLaterUse(colors, 3, VertexAttribPointerType.Float);
            o.normalBuffer = InitArrayBufferForLaterUse(normals, 3, VertexAttribPointerType.Float);
            o.indexBuffer = InitElementArrayBufferForLaterUse(indices, DrawElementsType.UnsignedInt);
            //  if (o.vertexBuffer == null || o.colorBuffer == null ||
            //       o.normalBuffer == null || o.indexBuffer == null)
            // {
            //      return null;
            // }

            o.numIndices = indices.Length;

            // Отвязать объект буфера
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            return o;
        }
        private HexagonalPrism InitVertexBuffersForHexagonalPrism()
        {
            float[] vertices = new float[]
              {
                //Prizma(/)
               0,0,0,
                1,0,0,
                2,1,0,
                1,2,0,
                0,2,0,
                -1,1,0,
                0,0,2,
                1,0,2,
                2,1,2,
                1,2,2,
                0,2,2,
                -1,1,2

                };

            // Colors
            float[] colors = new float[]
            {

                   0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, // v0-v1-v2-v3 front
                0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, // v0-v3-v4-v5 right
                0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, // v0-v5-v6-v1 up
                0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, // v1-v6-v7-v2 left
                0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, // v7-v4-v3-v2 down
                0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0　// v4-v7-v6-v5 back

                 };

            // Normals
            float[] normals = new float[]
            {
               0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f,     // v0-v1-v2-v3 front
                1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f,     // v0-v3-v4-v5 right
                0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f,     // v0-v5-v6-v1 up
                -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, // v1-v6-v7-v2 left
                0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, // v7-v4-v3-v2 down
                0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f  // v4-v7-v6-v5 back
            };

            int[] indices = new int[]
             {
                 0, 2, 1,
                 0, 3, 2,
                 0, 4, 3,
                 0, 5, 4,
                 6, 7, 8,
                 6, 8, 9,
                 6, 9, 10,
                 6, 10, 11,
                 0, 1, 6,
                 1, 7, 6,
                 1, 2, 7,
                 2, 8, 7,
                 2, 3, 8,
                 3, 9, 8,
                 3, 4, 9,
                 4, 10, 9,
                 4, 5, 11,
                 4, 11, 10,
                 5, 0, 6,
                 5, 6, 11
             };
            HexagonalPrism o = new HexagonalPrism(); //Использование объекта Object для совместного возврата нескольких объектов буфера

            //  Запись информации о вершинах в объект буфера
            o.vertexBuffer = InitArrayBufferForLaterUse(vertices, 3, VertexAttribPointerType.Float);
            o.colorBuffer = InitArrayBufferForLaterUse(colors, 3, VertexAttribPointerType.Float);
            o.normalBuffer = InitArrayBufferForLaterUse(normals, 3, VertexAttribPointerType.Float);
            o.indexBuffer = InitElementArrayBufferForLaterUse(indices, DrawElementsType.UnsignedInt);
            //  if (o.vertexBuffer == null || o.colorBuffer == null ||
            //       o.normalBuffer == null || o.indexBuffer == null)
            // {
            //      return null;
            // }

            o.numIndices = indices.Length;

            // Отвязать объект буфера
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            return o;
        }

            //  private void Cylinder_Click(object sender, EventArgs e)
            //{
            //             double j;
            //        double i;
            //        double k;
            //        GL.Begin(BeginMode.QuadStrip);
            //        for (j = 0; j <= 360; j += 5)
            //        {
            //            GL.Color3(1f, 0f, 0f);
            //            GL.Vertex3(Math.Cos(j), 1, Math.Sin(j));
            //            GL.Color3(1f, 0f, 0f);
            //            GL.Vertex3(Math.Cos(j), -1, Math.Sin(j));
            //        }
            //        GL.End();

            //        for (i = 1; i >= -1; i -= 2)
            //        {
            //            GL.Begin(BeginMode.TriangleFan);
            //            GL.Color3(0f, 0f, 1f);
            //            GL.Vertex3(0, i, 0);

            //            for (k = 0; k <= 360; k += 5)
            //            {
            //                GL.Color3(0f, 0f, 1f);
            //                GL.Vertex3(i * Math.Cos(k), i, Math.Sin(k));

            //            }
            //        }
            //        GL.End();
            //     }










            private VertexBufferObject InitArrayBufferForLaterUse(float[] data, int num, VertexAttribPointerType type)
            {
                // Создание объекта буфера
                VertexBufferObject buffer = new VertexBufferObject();
                GL.GenBuffers(1, out buffer.id);
                // if (buffer.id < 0)
                //{
                //    Logger.Append("Failed to create the buffer object");
                //    return null;
                //}
                // Запись данных в объект буфера
                GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.id);
                GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);

                // Сохранение необходимой информации, чтобы позже назначить объект переменной атрибута
                buffer.num = num;
                buffer.type = type;

                return buffer;
            }

            private ElementBufferObject InitElementArrayBufferForLaterUse(int[] data, DrawElementsType type)
            {
                // Create a buffer object
                ElementBufferObject buffer = new ElementBufferObject();
                GL.GenBuffers(1, out buffer.id);
                //if (buffer.id < 0)
                //{
                //    Logger.Append("Failed to create the buffer object");
                //    return null;
                //}
                //  Запись данных в объект буфера
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffer.id);
                GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);

                buffer.type = type;

                return buffer;
            }

            private FrameBuffer InitFramebufferObject()
            {
                int texture, depthBuffer;

                // Создание объекта буфера кадров (FBO)
                FrameBuffer frameBuffer = new FrameBuffer();
                GL.GenFramebuffers(1, out frameBuffer.id);
                //if (frameBuffer.id < 0)
                //{
                //    Logger.Append("Failed to create frame buffer object");
                //    return null;
                //}

                // Создание текстурного объекта; указать его размер и параметры
                GL.GenTextures(1, out texture);
                //if (texture < 0)
                //{
                //    Logger.Append("Failed to create texture object");
                //    return null;
                //}
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, OFFSCREEN_WIDTH, OFFSCREEN_HEIGHT, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);// glTexImage2D задает двумерную или кубическую текстуру для текущего текстурного блока
                                                                                                                                                                                 //   target(цель) - Задает целевую текстуру активного текстурного блока.
                                                                                                                                                                                 //   level - Задает номер уровня детализации. Уровень 0 - это базовый уровень изображения.
                                                                                                                                                                                 // internalformat (внутренний формат) - Задает внутренний формат текстуры.
                                                                                                                                                                                 //  width (ширина) 
                                                                                                                                                                                 // height (высота)
                                                                                                                                                                                 //border (граница) - задает ширину границы; должно бьть 0
                                                                                                                                                                                 // format - Задает формат данных. Должен соответствовать внутреннему формату.
                                                                                                                                                                                 // type - Задает тип данных
                                                                                                                                                                                 //  data - Указывает указатель на данные изображения в памяти.
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);

                // Создание объекта буфера визуализации; указать его размер и параметры
                GL.GenRenderbuffers(1, out depthBuffer); // Create a renderbuffer object
                                                         //if (depthBuffer < 0)
                                                         //{
                                                         //    Logger.Append("Failed to create renderbuffer object");
                                                         //    return null;
                                                         //}
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent16, OFFSCREEN_WIDTH, OFFSCREEN_HEIGHT); //  GL.RenderbufferStorage -установка хранилища данных, формат и размеры изображения объекта буфера визуализации 
                                                                                                                                                  // target -  задает цель привязки распределения для функции  GL.RenderbufferStorage
                                                                                                                                                  // renderbuffer (буфер визуализации) - задает имя объекта 
                                                                                                                                                  // internal format(внутренний формат) - задает внутренний формат, используемый для изображения объекта
                                                                                                                                                  // width - задает ширину буфера визуализации в пикселях.
                                                                                                                                                  // height - задает высоту буфера визуализации в пикселях.

                // Прикрепление текстуры и объект буфера визуалиизации к FBO
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer.id);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture, 0);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);

                // Проверка настройки FBO
                FramebufferErrorCode e = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (e != FramebufferErrorCode.FramebufferComplete)
                {
                    Logger.Append("Frame buffer object is incomplete: " + e.ToString());
                    return null;
                }

                frameBuffer.texture = texture; // сохранение необходимого объекта

                // Отвязка объекта буфера
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

                return frameBuffer;

           
            }

            //private void textBoxDebugOutput_TextChanged(object sender, EventArgs e)
            //  {
            //     textBoxDebugOutput.SelectionStart = textBoxDebugOutput.Text.Length;
            //     textBoxDebugOutput.ScrollToCaret();
            //  }

            private void textBoxRotationSpeed_TextChanged(object sender, EventArgs e)
            {
                int result;
                if (int.TryParse(textBoxRotationSpeed.Text, out result))
                {
                    angleStep = result;
                }
                else
                {
                    //textBoxDebugOutput.Text += debugMessageCounter++ + " " + Resources.invalidRotationSpeedMessage;
                    //  textBoxDebugOutput.Text += Environment.NewLine;
                    //  textBoxDebugOutput.Text += Environment.NewLine;
                }
            }

        private void Prism_Click(object sender, EventArgs e)
        {
           
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
                SetProjMatrixes();
                glControl.Invalidate();

           
        }

            private void SetProjMatrixes()
            {
                projMatrixFromLight = Matrix4.CreatePerspectiveFieldOfView(fovyAngleFromLight, OFFSCREEN_WIDTH / (float)OFFSCREEN_HEIGHT, 1f, 500f); //создать перспективное поле обзора
                projMatrix = Matrix4.CreatePerspectiveFieldOfView(fovyAngle, ClientSize.Width / (float)ClientSize.Height, 1f, 500f);
            }
        }
    } 
