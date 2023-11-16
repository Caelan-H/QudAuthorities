using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.World;

namespace XRL.EditorFormats.Map;

public struct MapFileRegion
{
	public readonly int width;

	public readonly int height;

	private MapFileCell[,] cells;

	public MapFileCell this[int x, int y]
	{
		get
		{
			return cells[x, y];
		}
		set
		{
			cells[x, y] = value;
		}
	}

	[Obsolete("This version is read only, please update to using [x,y]")]
	public MapFileCell[] this[int x]
	{
		get
		{
			MapFileCell[] array = new MapFileCell[height];
			for (int i = 0; i < height; i++)
			{
				array[i] = cells[x, i];
			}
			return array;
		}
	}

	public MapFileRegion(int width = 80, int height = 25)
	{
		this.width = width;
		this.height = height;
		cells = new MapFileCell[width, height];
	}

	public bool HasBlueprintInCell(int x, int y, string bpname, string paintWith = "")
	{
		if (x >= 0 && x < width && y >= 0 && y < height && cells[x, y] != null)
		{
			return cells[x, y].Objects.FindCount(delegate(MapFileObjectBlueprint bp)
			{
				if (bp.Name == bpname)
				{
					return true;
				}
				if (paintWith == null)
				{
					return false;
				}
				string tag = GameObjectFactory.Factory.Blueprints[bp.Name].GetTag("PaintWith", null);
				return (tag == paintWith || tag == "*" || (paintWith == "*" && tag != null)) ? true : false;
			}) > 0;
		}
		return false;
	}

	public MapFileRegion FlippedVertical()
	{
		MapFileRegion result = new MapFileRegion(width, height);
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				result.cells[i, height - j - 1] = cells[i, j];
			}
		}
		return result;
	}

	public MapFileRegion Clone()
	{
		MapFileRegion result = new MapFileRegion(width, height);
		foreach (MapFileCellReference item in AllCells())
		{
			MapFileCell orCreateCellAt = result.GetOrCreateCellAt(item.x, item.y);
			foreach (MapFileObjectBlueprint @object in item.cell.Objects)
			{
				orCreateCellAt.Objects.Add(new MapFileObjectBlueprint(@object));
			}
		}
		return result;
	}

	public MapFileRegion GetRegion(Rect rect)
	{
		if (rect.xMin < 0f || rect.xMax >= (float)width || rect.yMin < 0f || rect.yMax >= (float)height)
		{
			throw new ArgumentOutOfRangeException("rect", rect, "rect is outside the bounds of this region");
		}
		MapFileRegion result = new MapFileRegion((int)rect.width + 1, (int)rect.height + 1);
		for (int i = (int)rect.xMin; i <= (int)rect.xMax; i++)
		{
			for (int j = (int)rect.yMin; j <= (int)rect.yMax; j++)
			{
				result.cells[i - (int)rect.xMin, j - (int)rect.yMin] = cells[i, j];
			}
		}
		return result;
	}

	public MapFileRegion FlippedHorizontal()
	{
		MapFileRegion result = new MapFileRegion(width, height);
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				result.cells[width - i - 1, j] = cells[i, j];
			}
		}
		return result;
	}

	public IEnumerable<MapFileCellReference> FindCellsWithObjectBlueprint(string blueprint)
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (cells[x, y] != null && cells[x, y].Objects.Find((MapFileObjectBlueprint o) => o.Name == blueprint) != null)
				{
					yield return new MapFileCellReference(this, x, y, cells[x, y]);
				}
			}
		}
	}

	public IEnumerable<MapFileCellReference> FindCellsWithObjectBlueprint(MapFileObjectBlueprint blueprint)
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (cells[x, y] != null && cells[x, y].Objects.Find((MapFileObjectBlueprint o) => o == blueprint) != null)
				{
					yield return new MapFileCellReference(this, x, y, cells[x, y]);
				}
			}
		}
	}

	public IEnumerable<MapFileCellReference> AllCells(Predicate<MapFileCellReference> filter = null)
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (cells[x, y] != null)
				{
					MapFileCellReference mapFileCellReference = new MapFileCellReference(this, x, y, cells[x, y]);
					if (filter == null || filter(mapFileCellReference))
					{
						yield return mapFileCellReference;
					}
				}
			}
		}
	}

	public IEnumerable<MapFileObjectReference> AllObjects(Predicate<MapFileObjectBlueprint> filter = null)
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (cells[x, y] == null)
				{
					continue;
				}
				foreach (MapFileObjectBlueprint @object in cells[x, y].Objects)
				{
					if (filter == null || filter(@object))
					{
						yield return new MapFileObjectReference(this, x, y, cells[x, y], @object);
					}
				}
			}
		}
	}

	/// <summary>Copy the contents of the region to the section of this region matching the upper left x,y corner</summary>
	public void SetRegion(MapFileRegion other, int x0 = 0, int y0 = 0)
	{
		for (int i = x0; i < width && i - x0 < other.width; i++)
		{
			for (int j = y0; j < height && j - y0 < other.height; j++)
			{
				cells[i, j] = other.cells[i - x0, j - y0];
			}
		}
	}

	public int MaxX(bool requireObjects = false)
	{
		for (int num = width - 1; num >= 0; num--)
		{
			for (int i = 0; i < height; i++)
			{
				if (cells[num, i] != null && (!requireObjects || cells[num, i].Objects.Count > 0))
				{
					return num;
				}
			}
		}
		return -1;
	}

	/// <summary>The y position of the bottom most in use cell, -1 if no cells are in use</summary><param>requireObjects: when true, the cell found must have at least one object in it</param>
	public int MaxY(bool requireObjects = false)
	{
		for (int num = height - 1; num >= 0; num--)
		{
			for (int i = 0; i < width; i++)
			{
				if (cells[i, num] != null && (!requireObjects || cells[i, num].Objects.Count > 0))
				{
					return num;
				}
			}
		}
		return -1;
	}

	public MapFileCell GetOrCreateCellAt(int x, int y)
	{
		if (x < 0 || x >= width)
		{
			throw new ArgumentOutOfRangeException("x");
		}
		if (y < 0 || y >= height)
		{
			throw new ArgumentOutOfRangeException("y");
		}
		if (cells[x, y] == null)
		{
			cells[x, y] = new MapFileCell();
		}
		return cells[x, y];
	}

	public void FillEmptyCells()
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (cells[i, j] == null)
				{
					cells[i, j] = new MapFileCell();
				}
			}
		}
	}
}
