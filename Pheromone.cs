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

        //-------------------------------------------------------------------
        //
        //

        public Pheromone(GridPoint gridLocation, int antId)
        {
            // Define pheromone-specific fields
            Col = System.Drawing.Color.FromArgb(0,0, 255, 0);
            Radius = Radius / 2;
            Type = GridElementType.Pheromone;
            GridLocation = gridLocation;

            // Place pheromone on grid
            Grid.GridData[GridLocation.X, GridLocation.Y].AntId = antId;
            Grid.GridData[GridLocation.X, GridLocation.Y].Type = Type;
            Grid.GridData[GridLocation.X, GridLocation.Y].PheromoneStrength = Strength;

            // Add to static list of all pheromones
        }

        //-------------------------------------------------------------------
        //
        //

        public void Fade()
        {
            Strength -= DecayRate;
            Col = System.Drawing.Color.FromArgb(0, 255-(int)(Strength * 255), 255, 255-(int)(Strength * 255));
            Grid.GridData[GridLocation.X, GridLocation.Y].PheromoneStrength = Strength;
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

}
