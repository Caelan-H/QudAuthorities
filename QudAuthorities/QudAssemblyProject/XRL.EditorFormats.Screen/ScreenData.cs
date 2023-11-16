using System;
using System.Collections.Generic;

namespace XRL.EditorFormats.Screen;

[Serializable]
public class ScreenData
{
	public Cell[,] Cells;

	public int Rows;

	public int Columns;

	public List<string> Properties = new List<string>();

	public ScreenData(int Width, int Height)
	{
		Rows = Height;
		Columns = Width;
		ResizeCells();
	}

	public void Clear()
	{
		Cell[,] cells = Cells;
		foreach (Cell cell in cells)
		{
			cell.Char = ' ';
			cell.Background = 'k';
			cell.Foreground = 'k';
		}
	}

	public void ResizeCells()
	{
		Cell[,] cells = Cells;
		Cells = new Cell[Columns, Rows];
		if (cells != null)
		{
			for (int i = 0; i < Columns; i++)
			{
				for (int j = 0; j < Rows; j++)
				{
					if (i < cells.GetUpperBound(0) && j < cells.GetUpperBound(1))
					{
						Cells[i, j] = cells[i, j];
					}
					else
					{
						Cells[i, j] = new Cell();
					}
				}
			}
			return;
		}
		for (int k = 0; k < Columns; k++)
		{
			for (int l = 0; l < Rows; l++)
			{
				Cells[k, l] = new Cell();
			}
		}
	}
}
