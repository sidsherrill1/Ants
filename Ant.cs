using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenTKTutorial
{
    public class Ant: Object
    {
        public List<Pheromone> PheromoneTrail = new List<Pheromone>();
        public GridPoint PrevLocation;
        public double Direction; // radians
        private int _searchBoundaryX;

        public int SearchBoundaryX
        {
            get { return _searchBoundaryX; }
            set
            {
                if (value <0) _searchBoundaryX = 0;
                else if (value > Grid.Xsize) _searchBoundaryX = Grid.Xsize;
                else _searchBoundaryX = value;
            }
        }

        private int _searchBoundaryY;

        public int SearchBoundaryY
        {
            get { return _searchBoundaryY;}
            set
            {
                if (value < 0) _searchBoundaryY = 0;
                else if (value > Grid.Ysize) _searchBoundaryY = Grid.Ysize;
                else _searchBoundaryY = value;
            }
        }
        
        public Ant(GridPoint gridLocation) : base(gridLocation)
        {
            Col = System.Drawing.Color.FromArgb(100,249,24,241);
            Type = GridElementType.Ant;
        }

        //-------------------------------------------------------------------
        //
        //

        public void DropPheromone()
        {
            /*
             * Drops a pheromone where the ant currently is before the ant moves to it's next location.
             * Defines a new pheromone object, adds it to the grid, adds it to this Ant's path.
             */

            
            Pheromone p = new Pheromone(GridLocation);
            PheromoneTrail.Add(p);
            if (PheromoneTrail.Count > 25)
            {
                // Remove pheromone object from grid
                int oldPx = PheromoneTrail[0].GridLocation.X;
                int oldPy = PheromoneTrail[0].GridLocation.Y;
                Grid.GridData[oldPx, oldPy].Object = null;
                Grid.GridData[oldPx, oldPy].Type = GridElementType.NotAssigned;

                // Remove pheromone object from trail
                PheromoneTrail.RemoveAt(0);
            }
        }

        //-------------------------------------------------------------------
        //
        //                                                                                              
        public void DrawSearchWindow()
        {
            /*
             * Draws an outline of the ant's direction vector.
             */

            int hyp = 20;

            // Boundary A 
            //int graphPointXa = hyp * (int)Math.Cos(Direction + Math.PI / 4) + GridLocation.X;
            //int graphPointYa = hyp * (int)Math.Sin(Direction + Math.PI / 4) + GridLocation.Y;
            double graphPointXa = hyp * Math.Cos(Direction + Math.PI / 4) + GridLocation.X;
            double graphPointYa = hyp * Math.Sin(Direction + Math.PI / 4) + GridLocation.Y;

            double fracXa = (double)graphPointXa / GlControl.Width;
            double fracYa = (double)graphPointYa / GlControl.Height;

            double glPointXa = (double)fracXa * 2 - 1;
            double glPointYa = (double)fracYa * 2 - 1;

            // Boundary B
            //int graphPointXb = hyp * (int)Math.Cos(Direction - Math.PI / 4) + GridLocation.X;
            //int graphPointYb = hyp * (int)Math.Sin(Direction - Math.PI / 4) + GridLocation.Y;
            double graphPointXb = hyp * Math.Cos(Direction - Math.PI / 4) + GridLocation.X;
            double graphPointYb = hyp * Math.Sin(Direction - Math.PI / 4) + GridLocation.Y;

            double fracXb = (double)graphPointXb / GlControl.Width;
            double fracYb = (double)graphPointYb / GlControl.Height;

            double glPointXb = (double)fracXb * 2 - 1;
            double glPointYb = (double)fracYb * 2 - 1;

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(System.Drawing.Color.Black);
            GL.Vertex2(GlLocation.X, GlLocation.Y);
            GL.Vertex2(glPointXa, glPointYa);
            GL.Vertex2(GlLocation.X, GlLocation.Y);
            GL.Vertex2(glPointXb, glPointYb);
            GL.End();

            // Set search boundaries
            if (graphPointXa != GridLocation.X) SearchBoundaryX = (int)graphPointXa;
            else SearchBoundaryX = (int)graphPointXb;

            if (graphPointYa != GridLocation.Y) SearchBoundaryY = (int)graphPointYa;
            else SearchBoundaryY = (int)graphPointYb;
        }

        //-------------------------------------------------------------------
        //
        //

        public double? ScanSurroundings()
        {
            int startingX;
            int endingX;
            int startingY;
            int endingY;

            if (GridLocation.X < SearchBoundaryX)
            {
                startingX = GridLocation.X;
                endingX = SearchBoundaryX;
            }
            else
            {
                startingX = SearchBoundaryX;
                endingX = GridLocation.X;
            }

            if (GridLocation.Y < SearchBoundaryY)
            {
                startingY = GridLocation.Y;
                endingY = SearchBoundaryY;
            }
            else
            {
                startingY = SearchBoundaryY;
                endingY = GridLocation.Y;
            }

            for (int x = startingX; x < endingX; x++)
            {
                for (int y = startingY; y < endingY; y++)
                {
                    if (Grid.GridData[x, y].Type == GridElementType.Pheromone)
                    {
                        // Change ant color to signify identified pheromone
                        Col = System.Drawing.Color.Aqua;

                        // Calculate angle to pheromone
                        int changeX = x - GridLocation.X;
                        int changeY = y - GridLocation.Y;
                        double newMean = Math.Atan(Math.Abs(changeY / changeX)); //radians
                        return newMean;
                    }
                }
            }

            return null;
        }

        //-------------------------------------------------------------------
        //
        //

        public void Move(double goal = 45, int stdev = 20)
        {
            /*
             * Returns radian angle for next move based on a Gaussian distribution
             */

            // Capture current location as previous location and remove ant from previous location on grid
            PrevLocation = GridLocation;
            Grid.GridData[GridLocation.X, GridLocation.Y].Object = null;
            Grid.GridData[GridLocation.X, GridLocation.Y].Type = GridElementType.NotAssigned;

            // Generate angle of movement (theta) by Sampling from a Gaussian distribution
            // This requires sampling from a uniform random of (0,1]
            // but random.NextDouble() returns a sample of [0,1).
            double x1 = 1 - Random.NextDouble(); // Correct sampling range
            double x2 = 1 - Random.NextDouble(); // Correct sampling range

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            double y1Adjusted = y1 * stdev + goal; // goal is the "mean" of our distribution
            double theta = (Math.PI / 180) * y1Adjusted; // Convert to radians

            int hyp = 5;
            int deltaX = (int)(hyp * Math.Cos(theta));
            int deltaY = (int)(hyp * Math.Sin(theta));

            int adjDeltaX = 0;
            switch (HorizDirection)
            {
                case HorizDirection.East:
                    if (GridLocation.X + deltaX >= Grid.Xsize - 1)
                    {
                        adjDeltaX = Grid.Xsize - 1;
                        HorizDirection = HorizDirection.West;
                    }
                    else adjDeltaX = GridLocation.X + deltaX;
                    break;

                case HorizDirection.West:
                    if (GridLocation.X - deltaX <= 0)
                    {
                        adjDeltaX = 0;
                        HorizDirection = HorizDirection.East;
                    }
                    else adjDeltaX = GridLocation.X - deltaX;
                    break;
            }

            int adjDeltaY = 0;
            switch (VertDirection)
            {
                case VertDirection.North:
                    if (GridLocation.Y + deltaY >= Grid.Ysize - 1)
                    {
                        adjDeltaY = Grid.Ysize - 1;
                        VertDirection = VertDirection.South;
                    }
                    else adjDeltaY = GridLocation.Y + deltaY;
                    break;
                case VertDirection.South:
                    if (GridLocation.Y - deltaY <= 0)
                    {
                        adjDeltaY = 0;
                        VertDirection = VertDirection.North;
                    }
                    else adjDeltaY = GridLocation.Y - deltaY;
                    break;
            }

            // Update new location and add ant back to grid
            GridLocation = new GridPoint(adjDeltaX, adjDeltaY);
            Grid.GridData[GridLocation.X, GridLocation.Y].Object = this;
            Grid.GridData[GridLocation.X, GridLocation.Y].Type = GridElementType.Ant;
        }

        //-------------------------------------------------------------------
        //
        //

        public void UpdateDirection()
        {
            /*
             * Updates the Direction field of the Ant based on it's new and previous *grid* locations.
             * This field is used to define the search window that the ant looks in for food.
             */

            double changeX = GridLocation.X - PrevLocation.X;
            double changeY = GridLocation.Y - PrevLocation.Y;
            double newDirection = Math.Atan(Math.Abs(changeY / changeX)); //radians

            if (changeX >= 0 && changeY >= 0) // Q1
            {
                Direction = newDirection;
            }
            else if (changeX <= 0 && changeY >= 0) // Q2
            {
                Direction = Math.PI - newDirection;
            }
            else if (changeX <= 0 && changeY <= 0) // Q3
            {
                Direction = Math.PI + newDirection;
            }
            else if (changeX >= 0 && changeY <= 0) // Q4
            {
                Direction = 2 * Math.PI - newDirection;
            }
            else
            {
                throw new Exception("Error in UpdateDirection: not capturing part of direction spectrum.");
            }

            TextBoxDir.Text = Direction.ToString();
        }

        

        //-------------------------------------------------------------------
        //
        //

        public void Update()
        {
            /*
             * Updates ant by dropping a new pheromone, moving the ant, and drawing it
             */


            DropPheromone();
            DrawSearchWindow();
            double? goal = ScanSurroundings();
            
            if (goal.HasValue) Move((double)goal,5);
            else Move();
            
            UpdateDirection();
            
            // Draw pheromone trail
            foreach (Pheromone p in PheromoneTrail)
            {
                p.Fade();
                p.Draw();
            }

            // Draw ant
            Draw();
        }

    }

    //******************************************************************************
    //
    //

    public enum HorizDirection
    {
        East,
        West,
    }

    public enum VertDirection
    {
        North,
        South
    }
}
