using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKTutorial
{
    public class Food : Object
    {
        public static int FoodCount = 0;
        public int Id;

        public Food(GridPoint gridPoint)
        {
            FoodCount++;
            
            GridLocation = gridPoint;
            Type = GridElementType.Food;
            Col = System.Drawing.Color.MediumPurple;
            Id = FoodCount;

            Grid.GridData[GridLocation.X, GridLocation.Y].Type = Type;
            Grid.GridData[GridLocation.X, GridLocation.Y].FoodId = Id;
            Draw();
        }

        public void MoveFood(GridPoint antLocation, double antDirectionAngle)
        {
            if (antDirectionAngle >= 0 && antDirectionAngle < Math.PI / 2)
            {

                GridLocation = new GridPoint(antLocation.X + 1, antLocation.Y + 1);
            }
            else if (antDirectionAngle >= Math.PI / 2 && antDirectionAngle < Math.PI)
            {
                GridLocation = new GridPoint(antLocation.X - 1, antLocation.Y + 1);
            }
            else if (antDirectionAngle >= Math.PI && antDirectionAngle < 3*Math.PI/2)
            {
                GridLocation = new GridPoint(antLocation.X - 1, antLocation.Y - 1);
            }
            else if (antDirectionAngle >= 3*Math.PI / 2 && antDirectionAngle < 2* Math.PI)
            {
                GridLocation = new GridPoint(antLocation.X + 1, antLocation.Y - 1);
            }
        }
    }

    enum FoodState
    {
        NotAssigned,
        BeingCarried
    }
}
