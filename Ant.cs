using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using Microsoft.SqlServer.Server;

namespace OpenTKTutorial
{
    public class Ant: Object
    {
        public static int Count = 0;

        public List<Pheromone> PheromoneTrail = new List<Pheromone>();
        public GridPoint PrevLocation;
        public double Direction; // radians
        public int Id;
        private int _searchBoundaryX;
        public HorizDirection HorizDirection;
        public VertDirection VertDirection;
        public State State;
        public int WanderLevel;
        public double SearchAngle; // radians
        public int Speed;
        public int FocusCountDown; // When an ant finds a pheromone, it moves towards that pheromone for 5 ticks

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

        //-------------------------------------------------------------------
        //
        //

        public Ant(GridPoint gridLocation)
        {
            // Define ant-specific fields
            Type = GridElementType.Ant;
            GridLocation = gridLocation;
            Id = Count++;
            Wandering();

            // Randomly set direction
            int draw1 = Random.Next();
            if (draw1 % 2 == 0) HorizDirection = HorizDirection.East;
            else HorizDirection = HorizDirection.West;
            
            int draw2 = Random.Next();
            if (draw2 % 2 == 0) VertDirection = VertDirection.North;
            else VertDirection = VertDirection.South;

            // Place ant on grid
            Grid.GridData[GridLocation.X, GridLocation.Y].AntId = Id;
            Grid.GridData[GridLocation.X, GridLocation.Y].Type = Type;

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

            Pheromone p = new Pheromone(GridLocation,Id);
            PheromoneTrail.Add(p);
            if (PheromoneTrail.Count > 25)
            {
                // Remove pheromone object from grid
                PheromoneTrail[0].Remove();
                
                // Remove pheromone object from Ant's pheromone trail
                PheromoneTrail.RemoveAt(0);
            }
        }

        //-------------------------------------------------------------------
        //
        //

        public Point GraphToGL(Point p)
        {
            double fracX = (double)p.X / GlControl.Width;
            double fracY = (double)p.Y / GlControl.Height;

            double glPointX = (double)fracX * 2 - 1;
            double glPointY = (double)fracY * 2 - 1;

            return new Point(glPointX, glPointY);
            }

        //-------------------------------------------------------------------
        //
        //

        public void DrawSearchWindow()
        {
            /*
             * Draws an outline of the ant's direction vector.
             */

            int hyp = 50;

            // Boundary A 
            double graphPointXA = hyp * Math.Cos(Direction + SearchAngle) + GridLocation.X;
            double graphPointYA = hyp * Math.Sin(Direction + SearchAngle) + GridLocation.Y;
            Point graphPointA = new Point(graphPointXA, graphPointYA);

            Point glPointA = GraphToGL(graphPointA);
            
            // Boundary B
            double graphPointXB = hyp * Math.Cos(Direction - SearchAngle) + GridLocation.X;
            double graphPointYB = hyp * Math.Sin(Direction - SearchAngle) + GridLocation.Y;
            Point graphPointB = new Point(graphPointXB, graphPointYB);

            Point glPointB = GraphToGL(graphPointB);

            //GL.Begin(PrimitiveType.Lines);
            //GL.Color3(System.Drawing.Color.Black);
            //GL.Vertex2(GlLocation.X, GlLocation.Y);
            //GL.Vertex2(glPointA.X, glPointA.Y);
            //GL.Vertex2(GlLocation.X, GlLocation.Y);
            //GL.Vertex2(glPointB.X, glPointB.Y);
            //GL.End();

            // Set search boundaries
            if (graphPointXA != GridLocation.X) SearchBoundaryX = (int)graphPointXA;
            else SearchBoundaryX = (int)graphPointXB;

            if (graphPointYA != GridLocation.Y) SearchBoundaryY = (int)graphPointYA;
            else SearchBoundaryY = (int)graphPointYB;
        }

        //-------------------------------------------------------------------
        //
        //

        public double ScanSurroundings()
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

            // Loop through grid elements in search window and look for pheromones
            // If no pheromones found, return the default mean (45) angle to move towards
            double goal = 45;
            double targetStrength = 0;
            for (int x = startingX; x < endingX; x++)
            {
                for (int y = startingY; y < endingY; y++)
                {
                    GridElementType xyType = Grid.GridData[x, y].Type;
                    int xyAntId = Grid.GridData[x, y].AntId;
                    double xyStrength = Grid.GridData[x, y].PheromoneStrength;

                    if (xyType == GridElementType.Pheromone && xyAntId != Id)
                    {

                        FollowingPheromone();

                        if (xyStrength > targetStrength) targetStrength = xyStrength;
                        else continue;
                        
                        // Calculate angle to pheromone
                        int changeX = x - GridLocation.X;
                        int changeY = y - GridLocation.Y;
                        
                        if (changeX == 0)
                        {
                            goal = (GridLocation.X < x) ? 180 : 0;
                        }
                        else goal = Math.Atan(Math.Abs(changeY / changeX))*180/Math.PI; //degrees
                    }
                }
            }
            
            // Lost pheromone
            if (goal.Equals(45)) Wandering();

            return goal;
        }

        //-------------------------------------------------------------------
        //
        //

        public void FollowingPheromone()
        {
            State = State.FollowingPheromone;
            Col = System.Drawing.Color.Aqua;
            WanderLevel = 0;
            SearchAngle = Math.PI / 2;
            Speed = 6;
            FocusCountDown = 5;
        }

        //-------------------------------------------------------------------
        //
        //

        public void Wandering()
        {
            State = State.Wandering;
            Col = System.Drawing.Color.DeepPink;
            WanderLevel = 20;
            SearchAngle = Math.PI / 4;
            Speed = 5;
            FocusCountDown = 0;
        }

        //-------------------------------------------------------------------
        //
        //

        public double SampleGaussian(double mean, int stdev)
        {
            /*
             * Generates angle of movement (theta) by Sampling from a Gaussian distribution
             */

            // This requires sampling from a uniform random of (0,1]
            // but random.NextDouble() returns a sample of [0,1).
            double x1 = 1 - Random.NextDouble(); // Correct sampling range
            double x2 = 1 - Random.NextDouble(); // Correct sampling range

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            double y1Adjusted = y1 * stdev + mean; // goal is the "mean" of our distribution
            double theta = (Math.PI / 180) * y1Adjusted; // Convert to radians
            return theta;
        }

        //-------------------------------------------------------------------
        //
        //

        public void Move(double goal = 45)
        {
            /*
             * Captures current location as previous location
             * Generates angle of movement (theta) by Sampling from a Gaussian distribution
             * Moves ant based on angle
             */

            PrevLocation = GridLocation;
            double theta = SampleGaussian(goal, WanderLevel);

            int deltaX = (int)(Speed * Math.Cos(theta));
            int deltaY = (int)(Speed * Math.Sin(theta));

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
            Grid.GridData[GridLocation.X, GridLocation.Y].AntId = Id;
            Grid.GridData[GridLocation.X, GridLocation.Y].Type = Type;
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
            
            double goal = ScanSurroundings();
            Move(goal);
            
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

    public enum State
    {
        Wandering,
        FollowingPheromone,
        FoundFood,
        HeadingHome
    }
}
