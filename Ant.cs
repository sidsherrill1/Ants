using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        public static GridPoint HomeLocation;

        public List<Pheromone> PheromoneTrail = new List<Pheromone>();
        public GridPoint PrevLocation;
        public double Target; // radians
        public int Id;
        public State State;
        public int WanderLevel; // This is used as the stdev used when generating a new next move by sampling from a Gaussian distribution
        public double SearchAngle; // radians; Defines the angular range the ant will look for food/pheromones in
        public int Speed;
        public GridPoint FoodLocation;
        public double _directionAngle; // radians
        public Food Food;

        public double DirectionAngle
        {
            get { return _directionAngle; }
            set
            {
                if (value < 0) _directionAngle = value + 2*Math.PI;
                else if (value > 2 * Math.PI) _directionAngle = value - 2 * Math.PI;
                else _directionAngle = value;
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
            DirectionAngle = Random.NextDouble() * (2 * Math.PI);

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

            Pheromone p = new Pheromone(GridLocation,Id,State);
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

        public Point GraphToGL(GridPoint p)
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
            int graphPointXA = (int)(hyp * Math.Cos(DirectionAngle + SearchAngle/2) + GridLocation.X);
            int graphPointYA = (int)(hyp * Math.Sin(DirectionAngle + SearchAngle/2) + GridLocation.Y);
            GridPoint graphPointA = new GridPoint(graphPointXA, graphPointYA);

            Point glPointA = GraphToGL(graphPointA);

            // Boundary B
            int graphPointXB = (int)(hyp * Math.Cos(DirectionAngle - SearchAngle/2) + GridLocation.X);
            int graphPointYB = (int)(hyp * Math.Sin(DirectionAngle - SearchAngle/2) + GridLocation.Y);
            GridPoint graphPointB = new GridPoint(graphPointXB, graphPointYB);

            Point glPointB = GraphToGL(graphPointB);

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(System.Drawing.Color.Black);
            GL.Vertex2(GlLocation.X, GlLocation.Y);
            GL.Vertex2(glPointA.X, glPointA.Y);
            GL.Vertex2(GlLocation.X, GlLocation.Y);
            GL.Vertex2(glPointB.X, glPointB.Y);
            GL.End();

        }

        //-------------------------------------------------------------------
        //
        //

        public double ScanSurroundings()
        {
            /*
             * Scans surroundings for food or pheromones
             * If found, returns angle (in radians) to move towards, based on surroundings
             * If no food or pheromones are found, returns the default mean angle (45)
             */

            HashSet<GridPoint> set = new HashSet<GridPoint>();
            int magnitude = 50;
            for (int m = 1; m <= magnitude; m++)
            {
                double scanAngleInterval = Math.PI / 64; // TODO: tinker with this value
                for (double angle = DirectionAngle - SearchAngle / 2; angle <= DirectionAngle + SearchAngle / 2; angle = angle + scanAngleInterval)
                {
                    int x = (int)(m * Math.Cos(angle)) + GridLocation.X;
                    int y = (int)(m * Math.Sin(angle)) + GridLocation.Y;

                    // Check to make sure we're not trying to search out of bounds
                    if (x < 0 || x >= Grid.Xsize) continue;
                    if (y < 0 || y >= Grid.Ysize) continue;

                    GridPoint p = new GridPoint(x, y);
                    set.Add(p);
                }
            }

            double angleToTarget = DirectionAngle;
            double targetStrength = 0;
            bool foundTarget = false;

            foreach (GridPoint p in set)
            {
                GridElementType xyType = Grid.GridData[p.X, p.Y].Type;
                int xyAntId = Grid.GridData[p.X, p.Y].AntId;

                if (xyType == GridElementType.Food)
                {
                    GridPoint fLocation = new GridPoint(p.X, p.Y);
                    angleToTarget = CalculateAngleToTarget(fLocation);

                    FoundFood(fLocation);
                    return angleToTarget;
                }

                // If ant sees a pheromone that's not it's own
                if (xyType == GridElementType.Pheromone && xyAntId != Id)
                {
                    PheromoneType xyPheromoneType = Grid.GridData[p.X, p.Y].PheromoneType;
                    switch (xyPheromoneType)
                    {
                        case PheromoneType.Wandering:
                        {
                            // Target the strongest wandering pheromone in search window
                            double xyStrength = Grid.GridData[p.X, p.Y].PheromoneStrength;
                            if (xyStrength > targetStrength) targetStrength = xyStrength;
                            else break;

                            GridPoint pLocation = new GridPoint(p.X, p.Y);
                            angleToTarget = CalculateAngleToTarget(pLocation);
                            foundTarget = true;

                            FollowingPheromone();
                            break;
                        }
                        case PheromoneType.MovingTowardsFood:
                        {
                            GridPoint pLocation = new GridPoint(p.X, p.Y);
                            angleToTarget = CalculateAngleToTarget(pLocation);
                            return angleToTarget;
                        }

                    }
                }
            }

            if (!foundTarget) Wandering();
            return angleToTarget;
        }

        //-------------------------------------------------------------------
        //
        //

        public double CalculateAngleToTarget(GridPoint target)
        {
            /*
             * Returns angle to target (in radians)
             */

            int changeX = target.X - GridLocation.X;
            int changeY = target.Y - GridLocation.Y;

            double angleToTarget = Math.Atan2(changeY, changeX); // radians

            return angleToTarget;
        }

        
        //-------------------------------------------------------------------
        //
        //

        public double SampleGaussian(double mean, int stdev)
        {
            /*
             * Generates angle of movement (theta) by Sampling from a Gaussian distribution
             */

            // Convert to degrees
            mean = 180/Math.PI * mean;

            // This requires sampling from a uniform random of (0,1]
            // but random.NextDouble() returns a sample of [0,1).
            double x1 = 1 - Random.NextDouble();
            double x2 = 1 - Random.NextDouble();

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            double y1Adjusted = y1 * stdev + mean;

            // Convert to radians
            double theta = (Math.PI / 180) * y1Adjusted;
            
            return theta;
        }

        //-------------------------------------------------------------------
        //
        //

        public void EatFood()
        {
            /*
             * Removes food from static list of food
             */

            int foodId = Grid.GridData[FoodLocation.X, FoodLocation.Y].FoodId;
            Foods.Remove(foodId);
            Wandering();
        }

        //-------------------------------------------------------------------
        //
        //

        public void PickUpFood()
        {
            /*
             * Sets food object and sets state to be CarryingFoodHome
             */

            int foodId = Grid.GridData[FoodLocation.X, FoodLocation.Y].FoodId;
            Food = Foods[foodId];
            CarryingFoodHome();
        }

        //-------------------------------------------------------------------
        //
        //

        public void DropOffFood()
        {
            /*
             * Removes food from static list of food
             */

            Food = null;
            Wandering();
        }

        //-------------------------------------------------------------------
        //
        //


        public void Move(double angleToTarget)
        {
            /*
             * Captures current location as previous location
             * Generates angle of movement (theta) by Sampling from a Gaussian distribution
             * Moves ant based on angle
             */

            // If we've been moving towards food, check and see if we're close enough to either eat it or pick it up
            if (State == State.FoundFood)
            {
                if (Math.Abs(GridLocation.X - FoodLocation.X) <= 5 && (Math.Abs(GridLocation.Y - FoodLocation.Y) <= 5))
                {
                    // Remove food
                    //Eat();

                    // Pick up food
                    PickUpFood();

                    // Update location to be the food's previous location, and update fields of grid element for ant's new position
                    GridLocation = FoodLocation;
                    Grid.GridData[GridLocation.X, GridLocation.Y].AntId = Id;
                    Grid.GridData[GridLocation.X, GridLocation.Y].Type = Type;
                    return;
                }
            }
            // If we've been taking food home, check to see if we're hoe
            else if (State == State.CarryingFoodHome)
            {
                if (Math.Abs(GridLocation.X - HomeLocation.X) <= 5 && (Math.Abs(GridLocation.Y - HomeLocation.Y) <= 5))
                {
                    // Drop off food
                    DropOffFood();

                    // Update location to be the food's previous location, and update fields of grid element for ant's new position
                    GridLocation = HomeLocation;
                    Grid.GridData[GridLocation.X, GridLocation.Y].AntId = Id;
                    Grid.GridData[GridLocation.X, GridLocation.Y].Type = Type;
                    return;
                }
            }
            
            PrevLocation = GridLocation;
            double theta = SampleGaussian(angleToTarget, WanderLevel);

            int deltaX = (int) (Speed * Math.Cos(theta));
            int deltaY = (int) (Speed * Math.Sin(theta));

            int newX = GridLocation.X + deltaX;
            if (newX >= Grid.Xsize) newX = Grid.Xsize - 1;
            else if (newX <= 0) newX = 0;

            int newY = GridLocation.Y + deltaY;
            if (newY >= Grid.Ysize) newY = Grid.Ysize - 1;
            else if (newY <= 0) newY = 0;

            // Update new location and add ant back to grid
            GridLocation = new GridPoint(newX, newY);
            

            // Update fields of grid element for ant's new position
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
             * This field is used to define the search window that the ant looks in for food/pheromones.
             */

            double changeX = GridLocation.X - PrevLocation.X;
            double changeY = GridLocation.Y - PrevLocation.Y;
            DirectionAngle = Math.Atan2(changeY, changeX); // radians

            // If ant is on a boundary, rotate it
            if (GridLocation.X == 0 || GridLocation.X == Grid.Xsize-1)
            {
                if (DirectionAngle >= 0 && DirectionAngle < Math.PI / 2) DirectionAngle += Math.PI / 2; // If moving towards Q1, rotate counterclockwise 90 deg
                else if (DirectionAngle >= Math.PI / 2 && DirectionAngle < Math.PI) DirectionAngle -= Math.PI / 2; // If moving towards Q2, rotate clockwise 90 deg
                else if (DirectionAngle >= Math.PI && DirectionAngle < 3 * Math.PI / 2) DirectionAngle += Math.PI / 2; // If moving towards Q3, rotate counterclockwise 90 deg
                else if (DirectionAngle >= 3 * Math.PI / 2 && DirectionAngle < 2 * Math.PI) DirectionAngle -= Math.PI / 2; // If moving towards Q4, rotate clockwise 90 deg
                else throw new Exception("Revisit UpdateDirection method - not accounting for some DirectionAngles.");
            }
            
            if (GridLocation.Y == 0 || GridLocation.Y == Grid.Ysize - 1)
            {
                if (DirectionAngle >= 0 && DirectionAngle < Math.PI / 2) DirectionAngle -= Math.PI / 2; // If moving towards Q1, rotate clockwise 90 deg
                else if (DirectionAngle >= Math.PI / 2 && DirectionAngle < Math.PI) DirectionAngle += Math.PI / 2; // If moving towards Q2, rotate counterclockwise 90 deg
                else if (DirectionAngle >= Math.PI && DirectionAngle < 3*Math.PI / 2) DirectionAngle -= Math.PI / 2; // If moving towards Q3, rotate clockwise 90 deg
                else if (DirectionAngle >= 3 * Math.PI / 2 && DirectionAngle < 2*Math.PI) DirectionAngle += Math.PI / 2; // If moving towards Q4, rotate counterclockwise 90 deg
                else throw new Exception("Revisit UpdateDirection method - not accounting for some DirectionAngles.");
            }

        }

        //-------------------------------------------------------------------
        //
        //

        public void CarryingFoodHome()
        {
            State = State.CarryingFoodHome;
            Col = System.Drawing.Color.Yellow;
            WanderLevel = 10;
            SearchAngle = 0;
            Speed = 6;
        }

        //-------------------------------------------------------------------
        //
        //

        public void FoundFood(GridPoint fLocation)
        {
            State = State.FoundFood;
            Col = System.Drawing.Color.DeepPink;
            WanderLevel = 0;
            SearchAngle = Math.PI / 4;
            Speed = 10;
            FoodLocation = fLocation;
        }

        //-------------------------------------------------------------------
        //
        //

        public void FollowingPheromone()
        {
            State = State.FollowingPheromone;
            Col = System.Drawing.Color.HotPink;
            WanderLevel = 10;
            SearchAngle = Math.PI / 3;
            Speed = 6;
        }

        //-------------------------------------------------------------------
        //
        //

        public void Wandering()
        {
            State = State.Wandering;
            Col = System.Drawing.Color.LightPink;
            WanderLevel = 10;
            SearchAngle = Math.PI / 2;
            Speed = 5;
        }

        //-------------------------------------------------------------------
        //
        //

        public void Update()
        {
            /*
             * Updates ant by way of the following events:
             *  If ant is wandering: drop pheromone, scan surroundings, move, update direction of movement, draw ant and its pheromones
             *  If ant has found food: drop pheromone, move towards food, draw ant and its pheromones
             */

            DropPheromone();
            if (State != State.CarryingFoodHome) DrawSearchWindow();

            double angleToTarget;
            switch (State)
            {
                case State.FoundFood:
                    angleToTarget = CalculateAngleToTarget(FoodLocation);
                    break;
                case State.CarryingFoodHome:
                    angleToTarget = CalculateAngleToTarget(HomeLocation);
                    break;
                default:
                    angleToTarget = ScanSurroundings();
                    break;
            }
        
            Move(angleToTarget);

            if (State == State.CarryingFoodHome) Food.MoveFood(GridLocation,DirectionAngle);

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

    public enum State
    {
        Wandering,
        FollowingPheromone,
        FoundFood,
        CarryingFoodHome
    }
}
