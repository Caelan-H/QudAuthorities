using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using XRL.UI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World;

public class PointOfInterest
{
	public GameObject Object;

	public string _DisplayName;

	public string Explanation;

	public string Key;

	public string Preposition = "at";

	public int Radius;

	public int Order;

	public Location2D Location;

	public IRenderable Icon;

	private static List<string> DisplayItems = new List<string>();

	public string DisplayName
	{
		get
		{
			if (_DisplayName == null)
			{
				if (Object != null)
				{
					_DisplayName = Object.DisplayName;
				}
				if (_DisplayName == null && !string.IsNullOrEmpty(Explanation))
				{
					_DisplayName = Explanation;
					Explanation = null;
				}
				if (_DisplayName == null)
				{
					_DisplayName = "?";
				}
			}
			return _DisplayName;
		}
		set
		{
			_DisplayName = value;
		}
	}

	public string GetDisplayName(GameObject who)
	{
		DisplayItems.Clear();
		string text = DisplayName;
		if (!string.IsNullOrEmpty(Explanation))
		{
			DisplayItems.Add(Explanation);
		}
		string text2 = null;
		if (who != null)
		{
			if (IsAt(who))
			{
				text2 = "here";
			}
			else if (Location != null)
			{
				text2 = who.DescribeDirectionToward(Location, General: true, Short: true);
			}
			else if (Object != null)
			{
				text2 = who.DescribeDirectionToward(Object, General: true, Short: true);
			}
		}
		if (!string.IsNullOrEmpty(text2))
		{
			DisplayItems.Add(text2);
		}
		if (DisplayItems.Count > 0)
		{
			text = text + " [" + string.Join(", ", DisplayItems.ToArray()) + "]";
		}
		return text;
	}

	public string GetSentenceName(GameObject who)
	{
		string text = DisplayName;
		if (Object != null)
		{
			text = Object.the + text;
		}
		else if (!text.StartsWith("the ", StringComparison.InvariantCultureIgnoreCase) && !text.StartsWith("a ", StringComparison.InvariantCultureIgnoreCase) && !text.StartsWith("an ", StringComparison.InvariantCultureIgnoreCase))
		{
			text = "the " + text;
		}
		return text;
	}

	public bool IsAt(GameObject who, int Radius)
	{
		return GetDistanceTo(who) <= Radius;
	}

	public bool IsAt(GameObject who)
	{
		return IsAt(who, GetAppropriateRadius(who));
	}

	public int GetAppropriateRadius(GameObject who)
	{
		if (Radius >= 0)
		{
			return Radius;
		}
		if (Object != null && (Object.IsCreature || Object.ConsiderSolidFor(who)))
		{
			return 1;
		}
		return 0;
	}

	public int GetDistanceTo(GameObject who)
	{
		if (!(Location != null))
		{
			return who.DistanceTo(Object);
		}
		return who.DistanceTo(Location);
	}

	public IRenderable GetIcon()
	{
		object obj = Icon;
		if (obj == null)
		{
			GameObject @object = Object;
			if (@object == null)
			{
				return null;
			}
			obj = @object.RenderForUI();
		}
		return (IRenderable)obj;
	}

	public bool NavigateTo(GameObject who)
	{
		if (!GameObject.validate(ref who))
		{
			return false;
		}
		int appropriateRadius = GetAppropriateRadius(who);
		if (IsAt(who, appropriateRadius))
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You are already " + Preposition + " " + GetSentenceName(who) + ".");
			}
			return false;
		}
		if (who.IsPlayer())
		{
			string text = ((appropriateRadius > 0) ? ("P" + appropriateRadius + ":") : "M");
			if (Location != null)
			{
				AutoAct.Setting = text + Location.x + "," + Location.y;
				who.ForfeitTurn();
			}
			else if (Object != null)
			{
				AutoAct.Setting = text + Object.id;
				who.ForfeitTurn();
			}
			else
			{
				Popup.ShowFail("Somehow there seems to be no location for " + GetSentenceName(who) + ".");
			}
		}
		else if (Location != null)
		{
			who.pBrain?.PushGoal(new MoveTo(Location, careful: true, overridesCombat: false, appropriateRadius));
		}
		else if (Object != null)
		{
			who.pBrain?.PushGoal(new MoveTo(Object, careful: true, overridesCombat: false, appropriateRadius));
		}
		return true;
	}

	public static int Compare(PointOfInterest a, PointOfInterest b)
	{
		int num = a.Order.CompareTo(b.Order);
		if (num != 0)
		{
			return num;
		}
		int num2 = ColorUtility.CompareExceptFormattingAndCase(a.DisplayName, b.DisplayName);
		if (num2 != 0)
		{
			return num2;
		}
		return a.GetDistanceTo(The.Player).CompareTo(b.GetDistanceTo(The.Player));
	}
}
