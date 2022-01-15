using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenTKTutorial
{
    public class Pheromone : Object
    {
        public static double DecayRate = 1.0 / 25; //0.04
        public double Strength = 1;
        public PheromoneType PheromoneType;

        //-------------------------------------------------------------------
        //
        //

        public Pheromone(GridPoint gridLocation, int antId, State antState)
        {
            // Define pheromone-specific fields
            Radius = Radius / 2;
            Type = GridElementType.Pheromone;
            GridLocation = gridLocation;

            
            if (antState == State.FoundFood)
            {
                Col = System.Drawing.Color.FromArgb(0, 255, 0, 0);
                PheromoneType = PheromoneType.MovingTowardsFood;
            }
            else if (antState == State.CarryingFoodHome)
            {
                Col = System.Drawing.Color.FromArgb(0, 0, 0, 255);
                PheromoneType = PheromoneType.CarryingFoodHome;
            }
            else
            {
                Col = System.Drawing.Color.FromArgb(0, 0, 255, 0);
                PheromoneType = PheromoneType.Wandering;
            }
            
            // Place pheromone on grid
            Grid.GridData[GridLocation.X, GridLocation.Y].AntId = antId;
            Grid.GridData[GridLocation.X, GridLocation.Y].Type = Type;
            Grid.GridData[GridLocation.X, GridLocation.Y].PheromoneStrength = Strength;
            Grid.GridData[GridLocation.X, GridLocation.Y].PheromoneType = PheromoneType;
        }

        //-------------------------------------------------------------------
        //
        //

        public void Fade()
        {
            Strength -= DecayRate;
            Grid.GridData[GridLocation.X, GridLocation.Y].PheromoneStrength = Strength;

            if (PheromoneType == PheromoneType.MovingTowardsFood)
            {
                Col = System.Drawing.Color.FromArgb(0, 255, 255 - (int)(Strength * 255), 255 - (int)(Strength * 255));
            }
            else if (PheromoneType == PheromoneType.CarryingFoodHome)
            {
                Col = System.Drawing.Color.FromArgb(0, 255-(int)(Strength * 255), 255 - (int)(Strength * 255), 255);
            }
            else if (PheromoneType == PheromoneType.Wandering)
            {
                Col = System.Drawing.Color.FromArgb(0, 255 - (int)(Strength * 255), 255, 255 - (int)(Strength * 255));
            }
        }

        //-------------------------------------------------------------------
        //
        //

        public void Remove()
        {
            Grid.GridData[GridLocation.X, GridLocation.Y].AntId = 0;
            Grid.GridData[GridLocation.X, GridLocation.Y].Type = GridElementType.NotAssigned;
            Grid.GridData[GridLocation.X, GridLocation.Y].PheromoneStrength = 0;
        }

    }

    //************************************************************************
    //
    //

    public enum PheromoneType
    {
        Wandering,
        CarryingFoodHome,
        MovingTowardsFood
    }
}
