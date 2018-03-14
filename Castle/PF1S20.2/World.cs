using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Cameras;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PF1S20._2
{
    public class World : IDisposable
    {

        private AssimpScene castle_scene;
        private AssimpScene arrow_scene;

        private float m_xRotation = 0.0f;
        private float m_yRotation = 0.0f;

        private int m_width;
        private int m_height;


        private float[] m_spotPosition = { -20.0f, 25.0f, -50.0f };
        private float[] m_reflPosition = { 0.0f, 25.0f, -50.0f };
        private Sphere lightBulb;

        private enum TextureObjects { Grass = 0, Path, Walls };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;
        private uint[] m_textures = null;
        private string[] m_textureFiles = { "..//..//Textures//grass-texture.jpeg",
                                            "..//..//Textures//pavement-texture.jpg",
                                            "..//..//Textures//chain-link-fence-texture.jpg" };

        //kamera

        private float m_eyeX = 0.0f;
        private float m_eyeY = 5.0f;
        private float m_eyeZ = 10.0f;

        private float m_centerX = 0.0f;
        private float m_centerY = 0.0f;
        private float m_centerZ = 0.0f;

        private float m_upX = 0.0f;
        private float m_upY = 10.0f;
        private float m_upZ = 10.0f;

        private double rightWallTranslation = 0.0;
        private double leftWallRotation = 0.0;
        private double arrowScale = 0.0;


        public AssimpScene Castle_Scene
        {
            get { return castle_scene; }
            set { castle_scene = value; }
        }

        public AssimpScene Arrow_Scene
        {
            get { return arrow_scene; }
            set { arrow_scene = value; }
        }

        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }


        public World(String scenePath_arrow, String sceneFile_arrow, String scenePath_castle, String sceneFile_castle, int width, int height, OpenGL gl)
        {
            this.castle_scene = new AssimpScene(scenePath_castle, sceneFile_castle, gl);
            this.arrow_scene = new AssimpScene(scenePath_arrow, sceneFile_arrow, gl);
            this.m_width = width;
            this.m_height = height;
            m_textures = new uint[m_textureCount];
        }

        ~World()
        {
            this.Dispose(false);
        }


        public void Initialize(OpenGL gl)
        {
            //gl.LookAt(m_eyeX, m_eyeY, m_eyeZ, m_centerX, m_centerY, m_centerZ, m_upX, m_upY, m_upZ);
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1f, 0f, 0f);
            gl.ShadeModel(OpenGL.GL_FLAT);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.Enable(OpenGL.GL_DEPTH_TEST);

            SetupLighting(gl);


            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);
                
                Bitmap image = new Bitmap(m_textureFiles[i]);
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

                image.UnlockBits(imageData);
                image.Dispose();
            }





            lightBulb = new Sphere();
            lightBulb.CreateInContext(gl);
            lightBulb.Radius = 1f;

            arrow_scene.LoadScene();
            arrow_scene.Initialize();

            castle_scene.LoadScene();
            castle_scene.Initialize();
        }

        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(60, (double)m_width / m_height, 1, 20000);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
        }

        private void SetupLighting(OpenGL gl)
        {
            float[] global_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);


            //tackasti izvor svetlosti
            float[] light0pos = new float[] { 0.0f, 10.0f, -10.0f, 1.0f };
            float[] light0ambient = new float[] { 0.4f, 0.4f, 0.4f, 1.0f };
            float[] light0diffuse = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] light0specular = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);


            //reflektorski izvor svetlosti
            float[] light1pos = new float[] { 0.0f, 10.0f, -10.0f, 1.0f };
            float[] light1ambient = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] light1diffuse = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] light1specular = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light1pos);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, light1ambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, light1diffuse);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, light1specular);
            
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 45.0f);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_EXPONENT, 5.0f);

            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_LIGHT1);

            //Ukljuci color tracking mehanizam
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);

            // Podesi na koje parametre materijala se odnose pozivi glColor funkcije
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            gl.Enable(OpenGL.GL_NORMALIZE);
        }

        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            
            gl.Perspective(60, (double)m_width / m_height, 1, 20000);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.Viewport(0, 0, m_width, m_height);

            //            gl.LookAt(m_eyeX, m_eyeY, m_eyeZ, m_centerX, m_centerY, m_centerZ, m_upX, m_upY, m_upZ);

            //tackasti izvor svetlosti

            gl.PushMatrix();
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, m_spotPosition);
            gl.Translate(m_spotPosition[0], m_spotPosition[1], m_spotPosition[2]);
            gl.Color(1.0f, 1.0f, 1.0f);
            lightBulb.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, m_reflPosition);
            gl.Translate(m_reflPosition[0], m_reflPosition[1], m_reflPosition[2]);
            gl.Color(1.0f, 1.0f, 1.0f);
            lightBulb.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            DrawArrow(gl);

            DrawCastle(gl);

            DrawWalls(gl);
            
            DrawSurface(gl);

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho2D(-m_width / 2, m_width / 2, -m_height / 2, m_height / 2);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.Viewport(m_width / 2, 0, m_width / 2, m_height / 2);

            Draw3DText(gl);
            
            gl.Flush();
        }

        public void DrawArrow(OpenGL gl) {

            //3d model strele   +++
            gl.PushMatrix();
            //gl.Translate(0.0, -10.0, 0.0);
            gl.Translate(-1.5f, 0.5f, -2.0f);
            gl.Scale(0.7f, 0.7f, 0.7f);
            //gl.Scale(arrowScale, arrowScale, arrowScale);
            arrow_scene.Draw();
            gl.PopMatrix();
        }

        public void DrawCastle(OpenGL gl) {

            //3d model zamka    +++
            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -30.0f);
            gl.Rotate(m_xRotation + 5.0f, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
            gl.Scale(0.3f, 0.3f, 0.3f);
            castle_scene.Draw();
        }

        public void DrawWalls(OpenGL gl) {

            //desni zid         +++
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Walls]);

            gl.PushMatrix();
            Cube right_wall = new Cube();
            gl.Translate(40, 5, -5);
            gl.Translate(rightWallTranslation, 0, 0);
            gl.Scale(1, 5.0, 55.0);
            right_wall.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            //levi zid           +++
            gl.PushMatrix();
            Cube left_wall = new Cube();
            gl.Translate(-40, 5, -5);
            gl.Rotate(leftWallRotation, 0, 1, 0);
            gl.Scale(1.0, 5.0, 55.0);
            left_wall.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
        }

        public void DrawSurface(OpenGL gl) {
            //podloga

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Grass]);
            
            gl.Begin(OpenGL.GL_QUADS);
            
            //gl.PointSize(20.0f);
            gl.PolygonMode(SharpGL.Enumerations.FaceMode.FrontAndBack, SharpGL.Enumerations.PolygonMode.Filled);

            //gl.Color(0.7f, 1.0f, 0.5f);
            gl.Normal(0.0f, 1.0f, -5.0f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(40.0f, 0.0f, -60.0f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(40.0f, 0.0f, 50.0f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(-40.0f, 0.0f, 50.0f);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(-40.0f, 0.0f, -60.0f);

            gl.End();

            //staza
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Path]);

            gl.Begin(OpenGL.GL_QUADS);

          //  gl.PointSize(20.0f);
            gl.PolygonMode(SharpGL.Enumerations.FaceMode.FrontAndBack, SharpGL.Enumerations.PolygonMode.Filled);

            gl.Color(0.5f, 0.5f, 0.5f);
            gl.Normal(0.0f, 1.0f, 30.0f);
            gl.TexCoord(1.0, 0.0);
            gl.Vertex(10.0f, 0.1f, 10.0f);
            gl.TexCoord(1.0, 1.0);
            gl.Vertex(10.0f, 0.1f, 50.0f);
            gl.TexCoord(0.0, 1.0);
            gl.Vertex(-10.0f, 0.1f, 50.0f);
            gl.TexCoord(0.0, 0.0);
            gl.Vertex(-10.0f, 0.1f, 10.0f);

            gl.End();

            gl.PopMatrix();
        }

        public void Draw3DText(OpenGL gl) {

            gl.PushMatrix();
            gl.Translate(-250.0, 60.0f, 0.0f);
            gl.Scale(30.0f, 30.0f, 30.0f);
            gl.Color(1.0f, 0.0f, 0.0f);
            gl.DrawText3D("Verdana Bold", 14.0f, 1f, 0.1f,
               "Predmet: Racunarska grafika");

            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-250.0f, 35.0f, 0.0f);
            gl.Scale(30.0f, 30.0f, 30.0f);
            gl.Color(1.0f, 0.0f, 0.0f);
            gl.DrawText3D("Verdana Bold", 14.0f, 1f, 0.1f,
                "Sk. god: 2017/18.");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-250.0f, 10.0f, 0.0f);
            gl.Scale(30.0f, 30.0f, 30.0f);
            gl.Color(1.0f, 0.0f, 0.0f);
            gl.DrawText3D("Verdana Bold", 14.0f, 1f, 0.1f,
                "Ime: Jelena");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-250.0f, -15.0f, 0.0f);
            gl.Scale(30.0f, 30.0f, 30.0f);
            gl.Color(1.0f, 0.0f, 0.0f);
            gl.DrawText3D("Verdana Bold", 14.0f, 1f, 0.1f,
                 "Prezime: Kostic");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-250.0f, -40.0f, 0.0f);
            gl.Scale(30.0f, 30.0f, 30.0f);
            gl.Color(1.0f, 0.0f, 0.0f);
            gl.DrawText3D("Verdana Bold", 14f, 1f, 0.1f,
                 "Sifra zad: 20.2");
            gl.PopMatrix();
        }

        public void doTranslateRightWall(double distance) {
            rightWallTranslation = distance;
        }

        public void doRotateLeftWall(double angle) {
            leftWallRotation = angle;
        }

        public void doScaleArrow(double scale) {
            arrowScale = scale;
        }

        public void doAnimate() { }

        public void doRotateCameraX() { }

        public void doRotateCameraY() { }

        public void doZoomCamera(bool direction) { }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                castle_scene.Dispose();
                arrow_scene.Dispose();
                //gl.DeleteTextures(m_textureCount, m_textures);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
