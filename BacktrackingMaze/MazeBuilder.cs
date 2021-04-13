using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BacktrackingMaze
{
    public class MazeBuilder
    {
        private int cellWidth;
        public int FieldWidth { get; private set; }
        public int FieldHeight { get; private set; }
        private char[,] Field;
        private Grid grid;

        public MazeBuilder(int size, int cellWidth)
        {
            this.cellWidth = cellWidth;
            FieldHeight = size * cellWidth + 1;
            FieldWidth = FieldHeight;
            Field = new char[FieldWidth, FieldHeight];
            grid = new Grid(size, size);
        }

        public char[,] GetMap()
        {
            int[,] cells = grid.Generate();
            Paint(cells);
            return Field;
        }

        public StringBuilder GetStringMap()
        {
            Field = GetMap();
            StringBuilder builder = new StringBuilder();
            for (int y = 0; y < FieldHeight; y++)
            {
                for (int x = 0; x < FieldWidth; x++)
                {
                    builder.Append(Field[x, y]);
                }
                builder.AppendLine();
            }
            return builder;
        }

        public void Paint(int[,] cells)
        {
            char s;
            for (int k = 1; k < cells.GetLength(0) * cellWidth; k += cellWidth)
            {
                for (int m = 1; m < cells.GetLength(1) * cellWidth; m += cellWidth)
                {
                    var directions = (Directions)cells[k / cellWidth, m / cellWidth];
                    for (int j = k; j < k + cellWidth - 1; j++)
                    {
                        for (int i = m; i < m + cellWidth - 1; i++)
                        {
                            Field[i, j] = '.';
                            if (!directions.HasFlag(Directions.S))
                                s = '#';
                            else
                                s = '.';
                            Field[i, k + cellWidth - 1] = s; // South (bottom) wall

                            if (!directions.HasFlag(Directions.E))
                                s = '#';
                            else
                                s = '.';

                            Field[m + cellWidth - 1, j] = s; // East (right) wall

                            /*if (directions.HasFlag(Directions.S) &&
                                directions.HasFlag(Directions.E))
                                s = '.';
                            else
                            */
                            s = '#';
                            Field[m + cellWidth - 1, k + cellWidth - 1] = s; //Corners
                        }
                    }
                }
            }

            for (int k = 0; k <= cells.GetLength(0) * cellWidth; k++)
            {
                Field[0, k] = '#';
            }

            for (int m = 0; m <= cells.GetLength(1) * cellWidth; m++)
            {
                Field[m, 0] = '#';
            }

            bool finishPlaced = false;
            for (int j = cells.GetLength(1) * (cellWidth - 1) + 1; j < cells.GetLength(1) * cellWidth; j++)
            {
                for (int i = cells.GetLength(0) * (cellWidth - 1) + 1; i < cells.GetLength(0) * cellWidth; i++)
                {
                    if(Field[i, j] == '.')
                    {
                        Field[i, j] = 'F';
                        finishPlaced = true;
                    }     
                }
            }

            if (!finishPlaced)
                Field[cells.GetLength(0) * cellWidth - 2, cells.GetLength(1) * cellWidth - 2] = 'F';
                    
        }
    }
}
