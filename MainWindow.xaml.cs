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
        private DispatcherTimer _dispatchTimer;
        private Stopwatch _stopwatch = new Stopwatch();

        public Grid Grid;
        public List<Ant> Ants = new List<Ant>();
        public Dictionary<int,Food> Foods = new Dictionary<int,Food>();
        public int Speed = 100;
        public ClickOption ClickOption = ClickOption.AddingAnt;
        public Point Home;

        //-------------------------------------------------------------------
        //
        //

        public MainWindow()
        {
            InitializeComponent(); // Executes/creates everything in the xaml code, including GLControl

            // Set object static fields
            Object.GlControl = GLControlMain;
            Object.Grid = Grid;
            Object.Foods = Foods;

            // Set up dispatch timer
            _defineDispatchTimer();
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

        //-------------------------------------------------------------------
        //
        //

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

        public void DrawHome()
        {
            int sides = 100;
            double radius = 0.15;
            double aspectRatio = (double) Grid.Xsize / Grid.Ysize;

            GL.Begin(PrimitiveType.LineLoop);
            GL.Color3(System.Drawing.Color.Brown);

            for (int i = 0; i < sides; i++)
            {
                double degInRad = i * 2 * Math.PI / sides;
                double x = Home.X + (Math.Cos(degInRad) * radius);
                double y = Home.Y + aspectRatio * (Math.Sin(degInRad) * radius);
                GL.Vertex2(x, y);
            }

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

            DrawHome();
            foreach (Ant a in Ants)
            {
                a.Update();
            }

            foreach (KeyValuePair<int,Food> f in Foods)
            {
                f.Value.Draw();
            }

            //Pheromone p = new Pheromone(new GridPoint(0, 1),123456);
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

            if (ClickOption == ClickOption.AddingAnt)
            {
                Ant ant = new Ant(gridLocation);
                Ants.Add(ant);
            } else if (ClickOption == ClickOption.AddingFood)
            {
                Food food = new Food(gridLocation);
                Foods.Add(food.Id,food);
            }

            UpdateBuffers();
        }
        
        //-------------------------------------------------------------------
        //
        //

        private void GridMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            // Update grid
            Grid = new Grid(GLControlMain.Width, GLControlMain.Height);
            Object.Grid = Grid; // This sets the static Grid feature of the entire Ant class; an Ant does not necessarily needed to be initiated before this is done! 
            Grid.GridData[0, 0].AntId = 7777777;

            // Set up home
            Home = new Point(0.82, -0.70);

            // x and y on GLcontrol are from -1 to 1, thus range is 2
            double fracX = (Home.X + 1) / 2;
            double fracY = (Home.Y + 1) / 2;

            int homeXasGrid = (int)(fracX * Grid.Xsize);
            int homeYasGrid = (int)(fracY * Grid.Ysize);

            Ant.HomeLocation = new GridPoint(homeXasGrid, homeYasGrid);
        }

        //-------------------------------------------------------------------
        //
        //

        private void SliderSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Speed = 1000-100*(int)e.NewValue;
            TextBoxSpeed.Text = e.NewValue.ToString();
            _defineDispatchTimer();
        }

        //-------------------------------------------------------------------
        //
        //

        private void RadioAnt_Click(object sender, RoutedEventArgs e)
        {
            ClickOption = ClickOption.AddingAnt;

        }

        private void RadioFood_Click(object sender, RoutedEventArgs e)
        {
            ClickOption = ClickOption.AddingFood;
        }

        //-------------------------------------------------------------------
        //
        //
    }

    public enum ClickOption
    {
        AddingAnt,
        AddingFood
    }
}
