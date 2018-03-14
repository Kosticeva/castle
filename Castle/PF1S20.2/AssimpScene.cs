using Assimp;
using SharpGL;
using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PF1S20._2
{
    public class AssimpScene : IDisposable
    {
        private Assimp.Scene m_scene;
        private OpenGL gl;
        private DisplayList lista;
        private String m_scenePath;
        private String m_sceneFileName;

        public Assimp.Scene Scene
        {
            get { return m_scene; }
            private set { m_scene = value; }
        }


        public AssimpScene(String scenePath, String sceneFileName, OpenGL gl)
        {
            this.m_scenePath = scenePath;
            this.m_sceneFileName = sceneFileName;
            this.gl = gl;
        }
        ~AssimpScene()
        {
            this.Dispose(false);
        }

        public void Draw()
        {
            lista.Call(gl);
        }
        public void LoadScene()
        {
            AssimpImporter importer = new AssimpImporter();

            LogStream logstream = new LogStream(delegate (String msg, String userData)
            {
                Console.WriteLine(msg);
            });
            importer.AttachLogStream(logstream);

            m_scene = importer.ImportFile(Path.Combine(m_scenePath, m_sceneFileName));

            // Oslobadjanje resursa koriscenih za ucitavanje podataka o sceni.
            importer.Dispose();
        }

        public void Initialize()
        {
            lista = new DisplayList();
            lista.Generate(gl);
            lista.New(gl, DisplayList.DisplayListMode.Compile);
            RenderNode(m_scene.RootNode);
            lista.End(gl);
        }

        private void RenderNode(Node node)
        {
            gl.PushMatrix();

            float[] matrix = new float[16] { node.Transform.A1, node.Transform.B1, node.Transform.C1, node.Transform.D1, node.Transform.A2, node.Transform.B2, node.Transform.C2, node.Transform.D2, node.Transform.A3, node.Transform.B3, node.Transform.C3, node.Transform.D3, node.Transform.A4, node.Transform.B4, node.Transform.C4, node.Transform.D4 };
            gl.MultMatrix(matrix);

            if (node.HasMeshes)
            {
                foreach (int meshIndex in node.MeshIndices)
                {
                    Mesh mesh = m_scene.Meshes[meshIndex];

                    bool hasColors = mesh.HasVertexColors(0);
                    uint brojPoli = mesh.Faces[0].IndexCount;

                    foreach (Assimp.Face face in mesh.Faces)
                    {
                        switch (face.IndexCount)
                        {
                            case 1:
                                gl.Begin(OpenGL.GL_POINTS);
                                break;
                            case 2:
                                gl.Begin(OpenGL.GL_LINES);
                                break;
                            case 3:
                                gl.Begin(OpenGL.GL_TRIANGLES);
                                break;
                            default:
                                gl.Begin(OpenGL.GL_POLYGON);
                                break;
                        }

                        for (int i = 0; i < face.IndexCount; i++)
                        {
                            uint vertexIndex = face.Indices[i];

                            if (hasColors)
                                gl.Color(mesh.GetVertexColors(0)[vertexIndex].R, mesh.GetVertexColors(0)[vertexIndex].G, mesh.GetVertexColors(0)[vertexIndex].B, mesh.GetVertexColors(0)[vertexIndex].A);
                            else
                            {
                                if (vertexIndex % 2 == 0)
                                    gl.Color(0.3f, 0.3f, 0.3f);
                                else
                                    gl.Color(0.4f, 0.4f, 0.4f);
                            }

                            gl.Vertex(mesh.Vertices[vertexIndex].X, mesh.Vertices[vertexIndex].Y, mesh.Vertices[vertexIndex].Z);
                        }
                        gl.End();
                    }
                }
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                RenderNode(node.Children[i]);
            }
            gl.PopMatrix();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lista.Delete(gl);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
