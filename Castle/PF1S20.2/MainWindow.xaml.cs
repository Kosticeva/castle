using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using SharpGL.SceneGraph;

namespace PF1S20._2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        World m_world = null;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                m_world = new World(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Models\\Arrow"), "arrow.obj", System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Models\\Castle"), "Castle OBJ.obj", (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            Console.WriteLine((int)openGLControl.ActualWidth + ", " + (int)openGLControl.ActualHeight);
            m_world.Draw(args.OpenGL);
        }

        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F4: this.Close(); break;
                case Key.Add: break;
                case Key.Subtract: break;
                case Key.K:  break;
                case Key.I:  break;
                case Key.L:  break;
                case Key.J: break;

            }
        }

        private void rightWallButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            /*Double value = 0.0;
            try {
                value = Double.Parse(moveByBox.Text);
            }catch(Exception ee)
            {
                //
            }

            m_world.doTranslateRightWall(value);*/

        }

        private void leftWallButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
           /* Double value = 0.0;
            try
            {
                value = Double.Parse(rotateByBox.Text);
            }
            catch (Exception ee)
            {
                //
            }

            m_world.doRotateLeftWall(value);*/
        }
        
        private void scaleArrowButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            /*Double value = 0.0;
            try
            {
                value = Double.Parse(scaleArrowBox.Text);
            }
            catch (Exception ee)
            {
                //
            }

            m_world.doScaleArrow(value);*/
        }
    }
}
