using System;
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
        public Point Position;
        public Object Object;

        public GridElement(Point p)
        {
            Type = GridElementType.NotAssigned;
            Position = p;
        }

    }

    public enum GridElementType
    {
        NotAssigned,
        Ant,
        Pheromone
    }
}
