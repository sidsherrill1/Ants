using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKTutorial
{
    public class Food : Object
    {
        public Food(GridPoint gridPoint)
        {
            GridLocation = gridPoint;
            Type = GridElementType.Food;
            Col = System.Drawing.Color.MediumPurple;

            Grid.GridData[GridLocation.X, GridLocation.Y].Type = Type;
            Draw();
        }
    }
}
