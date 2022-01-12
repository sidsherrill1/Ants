using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK;
using System.Windows.Controls;

namespace OpenTKTutorial
{
    public class Object
    {
        public static Grid Grid; // Set after InitializeComponent(), within GridMain_SizeChanged;
        public static GLControl GlControl; // Set after InitializeComponent();
        public static Random Random = new Random();

        public double Radius = 0.02;
        public int Sides = 20;

        public System.Drawing.Color Col;
        public GridElementType Type;

        private Point _glLocation;
        public Point GlLocation
        {
            get
            {
                return _glLocation;
            }

            set
            {
                double x = (double)value.X;
                double y = (double)value.Y;

                double fracX = x / GlControl.Width;
                double fracY = y / GlControl.Height;

                double finalX = (double)fracX * 2 - 1;
                double finalY = (double)fracY * 2 - 1;

                _glLocation = new Point(finalX, finalY); //x and y are from -1 to 1
            }
        }
        
        private GridPoint _gridLocation;
        public virtual GridPoint GridLocation
        {
            get
            {
                return _gridLocation;
            }

            set
            {
                _gridLocation = value;
                GlLocation = new Point(value.X,value.Y);
            }
        }

        //-------------------------------------------------------------------
        //
        //

        public void Draw()
        {

            /*
             * Method for drawing object based on object fields.
             * Note: All drawing operations using GL.Vertex2(x,y) are based on the GL window frame (x: -1,1, y: -1,1).
             */

            GL.Begin(PrimitiveType.Polygon);
            GL.Color3(Col);

            for (int i = 0; i < Sides; i++)
            {
                double degInRad = i * 2 * Math.PI / Sides;
                double x = GlLocation.X + (Math.Cos(degInRad) * Radius);
                double y = GlLocation.Y + (Math.Sin(degInRad) * Radius);
                GL.Vertex2(x, y);
            }
            GL.End();
        }

        //-------------------------------------------------------------------
        //
        //

        public void Draw(System.Drawing.Color col, GridPoint gridPoint, int sides, double radius)
        {
            /*
             * Overload for Draw method given a specified color, GridPoint location, number of sides, and radius.
             * Used for drawing...?
             */

            double fracX = (double)gridPoint.X / GlControl.Width;
            double fracY = (double)gridPoint.Y / GlControl.Height;

            double glPointX = (double)fracX * 2 - 1;
            double glPointY = (double)fracY * 2 - 1;

            GL.Begin(PrimitiveType.Polygon);
            GL.Color3(col);

            for (int i = 0; i < sides; i++)
            {
                double degInRad = i * 2 * Math.PI / sides;
                double x = glPointX + (Math.Cos(degInRad) * radius);
                double y = glPointY + (Math.Sin(degInRad) * radius);
                GL.Vertex2(x, y);
            }
            GL.End();
        }

    }

    
    //******************************************************************************
    //
    //

    public struct GridPoint
    {
        public int X;
        public int Y;

        public GridPoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

}
