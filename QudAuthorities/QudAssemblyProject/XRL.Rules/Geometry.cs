using System;
using System.Collections.Generic;
using Genkit;
using UnityEngine;
using XRL.World;

namespace XRL.Rules;

public class Geometry
{
	public static int Distance(Point P1, Point P2)
	{
		int num = P1.X - P2.X;
		int num2 = P1.Y - P2.Y;
		return (int)Math.Sqrt(num * num + num2 * num2);
	}

	public static int Distance(int X1, int Y1, int X2, int Y2)
	{
		int num = X1 - X2;
		int num2 = Y1 - Y2;
		return (int)Math.Sqrt(num * num + num2 * num2);
	}

	public static int Distance(int X1, int Y1, XRL.World.GameObject obj)
	{
		int num = X1 - obj.pPhysics.CurrentCell.X;
		int num2 = Y1 - obj.pPhysics.CurrentCell.Y;
		return (int)Math.Sqrt(num * num + num2 * num2);
	}

	public static bool TestAngleTo(Location2D fulcrum, Location2D from, Location2D to, float MaxAngle)
	{
		Vector2 from2 = new Vector2(from.x - fulcrum.x, from.y - fulcrum.y);
		Vector2 to2 = new Vector2(to.x - fulcrum.x, to.y - fulcrum.y);
		if (Vector2.Angle(from2, to2) <= MaxAngle)
		{
			return true;
		}
		float num = 0.25f;
		Vector2 from3 = new Vector2((float)(from.x - fulcrum.x) - num, (float)(from.y - fulcrum.y) - num);
		to2 = new Vector2((float)(to.x - fulcrum.x) - num, (float)(to.y - fulcrum.y) - num);
		if (Vector2.Angle(from3, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 from4 = new Vector2((float)(from.x - fulcrum.x) + num, (float)(from.y - fulcrum.y) - num);
		to2 = new Vector2((float)(to.x - fulcrum.x) + num, (float)(to.y - fulcrum.y) - num);
		if (Vector2.Angle(from4, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 from5 = new Vector2((float)(from.x - fulcrum.x) - num, (float)(from.y - fulcrum.y) + num);
		to2 = new Vector2((float)(to.x - fulcrum.x) - num, (float)(to.y - fulcrum.y) + num);
		if (Vector2.Angle(from5, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 from6 = new Vector2((float)(from.x - fulcrum.x) + num, (float)(from.y - fulcrum.y) + num);
		to2 = new Vector2((float)(to.x - fulcrum.x) + num, (float)(to.y - fulcrum.y) + num);
		if (Vector2.Angle(from6, to2) <= MaxAngle)
		{
			return true;
		}
		return false;
	}

	public static List<Location2D> GetCone(Location2D source, Location2D target, int Length, int Angle, List<Location2D> result = null)
	{
		if (result == null)
		{
			result = new List<Location2D>();
		}
		else
		{
			result.Clear();
		}
		if (source.Distance(target) == 0)
		{
			result.Add(source);
			return result;
		}
		foreach (Location2D item in source.YieldAdjacent(Length))
		{
			if (item.x < 80 && item.y < 25 && item.Distance(source) <= Length && TestAngleTo(source, item, target, (float)Angle / 2f))
			{
				result.Add(item);
			}
		}
		return result;
	}
}
