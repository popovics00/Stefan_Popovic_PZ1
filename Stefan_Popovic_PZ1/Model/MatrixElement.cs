using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stefan_Popovic_PZ1.Model
{
    public enum Previous { Left, Right, Up, Down };

    public enum FieldType { Obstacle, Visited, Line, Free };

    public enum positionLook { straight, horizontally, Lposition, upsidedownL, othersideL, totalRotL, plus, Tposition, upsidedownT, clockwise90T, notclockwise90T }


    public class MatrixElement
    {
        public FieldType Type;
        public int Step;
        public positionLook Paint;
        public positionLook Paint1;
        public Tuple<int, int> Id;
        public Tuple<int, int> PreviousStep;
        public Previous Direction;

        public List<long> listId;

        public MatrixElement()
        {
            listId = new List<long>();
        }
    }
}
