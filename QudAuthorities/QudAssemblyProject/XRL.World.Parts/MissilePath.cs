using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class MissilePath
{
	public List<Cell> Path = new List<Cell>();

	public float Angle;

	public float Cover;

	public float x0;

	public float y0;

	public float x1;

	public float y1;
}
