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
        public static double DecayRate = 1.0 / 25;
        public double Strength;
        

        public Pheromone(GridPoint gridLocation) : base(gridLocation)
        {
            Col = System.Drawing.Color.FromArgb(0, 255, 0);
            Radius = Radius / 2;
            Strength = 1;
            Type = GridElementType.Pheromone;
        }

        public void Fade()
        {
            Strength -= DecayRate;
            Col = System.Drawing.Color.FromArgb(0, (int)Strength * 255, 0);
        }
    }

}
