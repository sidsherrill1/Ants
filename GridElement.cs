﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKTutorial
{
    
    public class GridElement
    {
        public GridElementType Type;
        public int AntId; // Used by Pheromone objects
        public double PheromoneStrength; // Used by Pheromone objects
        public PheromoneType PheromoneType; // Used by Ant objects
        public int FoodId; // TODO: figure out how to reference objects instead of doing this

        public GridElement()
        {
            Type = GridElementType.NotAssigned;
        }

    }

    public enum GridElementType
    {
        NotAssigned,
        Ant,
        Pheromone,
        Food
    }
}
