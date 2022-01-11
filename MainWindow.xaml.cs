using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTKTutorial
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    
    
    public partial class MainWindow : Window 
    {
        public Grid Grid;
        public List<Ant> Ants = new List<Ant>();
        public int Speed = 100;
        private DispatcherTimer _dispatchTimer;
        private Stopwatch _stopwatch = new Stopwatch();


        //-------------------------------------------------------------------
        //
        //

        public MainWindow()
        {
            InitializeComponent(); // Executes/creates everything in the xaml code, including GLControl

            Object.GlControl = GLControlMain;
            Object.TextBoxDir = TextBoxDir;

            _defineDispatchTimer();

            // Define function that will be called after initialization
            _dispatchTimer.Tick += _dispatchTimer_Tick;

        }

        //-------------------------------------------------------------------
        //
        //

        // TODO: ask dad about this!!!
        private void _defineDispatchTimer()
        {
            _dispatchTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100), //100 milliseconds = 0.1 seconds
                IsEnabled = true,
            };
        }

        private void _dispatchTimer_Tick(object sender, EventArgs e)
        {
            // Display, reset, start timer
            TextBoxTimer.Text = _stopwatch.ElapsedMilliseconds.ToString();
            _stopwatch.Reset();
            _stopwatch.Start();
            

            UpdateBuffers();
        }


        //-------------------------------------------------------------------
        //
        //

        public void DrawGrid()
        {
            
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(System.Drawing.Color.Gray);
                GL.Vertex2(0, -1);
                GL.Vertex2(0, 1);
                GL.Vertex2(-1, 0);
                GL.Vertex2(1, 0);
            GL.End();
        }

        //-------------------------------------------------------------------
        //
        //

        public void UpdateBuffers()
        {
            GLControlMain.SwapBuffers(); // Flip the buffer
            GL.ClearColor(System.Drawing.Color.Transparent);
            GL.Clear(ClearBufferMask.ColorBufferBit); // Clear the buffer - overwriting each pixel to be blank

            DrawGrid();
            foreach (Ant a in Ants)
            {
                a.Update();
            }

            GL.Flush(); // Flush == Execute what's in the buffer
        }

        //-------------------------------------------------------------------
        //
        //

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GL.Viewport(0, 0, GLControlMain.Width, GLControlMain.Height);

        }

        //-------------------------------------------------------------------
        //
        //
        
        private void GLControl_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int x = e.X;
            int y = GLControlMain.Height - e.Y;
            GridPoint gridLocation = new GridPoint(x, y);
            Ant ant = new Ant(gridLocation);
            Ants.Add(ant);
            UpdateBuffers();
        }

        private void GridMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int x = GLControlMain.Width;
            int y = GLControlMain.Height;
            Grid = new Grid(x, y);
            Object.Grid = Grid; // This sets the static Grid feature of the entire Ant class; an Ant does not necessarily needed to be initiated before this is done! 
        }

        private void SliderSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Speed = 1000-100*(int)e.NewValue;
            TextBoxSpeed.Text = e.NewValue.ToString();
            _defineDispatchTimer();
        }
    }
}
