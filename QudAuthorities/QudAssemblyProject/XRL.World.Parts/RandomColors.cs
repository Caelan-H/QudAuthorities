using System;
using System.Reflection;
using UnityEngine;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class RandomColors : IPart
{
	public string MainColor;

	public string TileColor;

	public string DetailColor;

	public string BackgroundColor;

	public string ReferencePart = "Render";

	public string MainColorField = "ColorString";

	public string TileColorField = "TileColor";

	public string DetailColorField = "DetailColor";

	public bool MainColorFieldIsForegroundBackground = true;

	public bool TileColorFieldIsForegroundBackground = true;

	public bool DetailColorFieldIsForegroundBackground;

	public bool SetColorStringToDetailColor;

	[FieldSaveVersion(223)]
	public bool PairDetailWithForeground;

	public override bool SameAs(IPart p)
	{
		RandomColors randomColors = p as RandomColors;
		if (randomColors.MainColor != MainColor)
		{
			return false;
		}
		if (randomColors.TileColor != TileColor)
		{
			return false;
		}
		if (randomColors.DetailColor != DetailColor)
		{
			return false;
		}
		if (randomColors.BackgroundColor != BackgroundColor)
		{
			return false;
		}
		if (randomColors.ReferencePart != ReferencePart)
		{
			return false;
		}
		if (randomColors.MainColorField != MainColorField)
		{
			return false;
		}
		if (randomColors.TileColorField != TileColorField)
		{
			return false;
		}
		if (randomColors.DetailColorField != DetailColorField)
		{
			return false;
		}
		if (randomColors.MainColorFieldIsForegroundBackground != MainColorFieldIsForegroundBackground)
		{
			return false;
		}
		if (randomColors.TileColorFieldIsForegroundBackground != TileColorFieldIsForegroundBackground)
		{
			return false;
		}
		if (randomColors.DetailColorFieldIsForegroundBackground != DetailColorFieldIsForegroundBackground)
		{
			return false;
		}
		if (randomColors.SetColorStringToDetailColor != SetColorStringToDetailColor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (MainColor == null && TileColor == null && BackgroundColor == null && DetailColor == null)
		{
			MetricsManager.LogError("no colors specified for " + ParentObject.DebugName);
			return base.HandleEvent(E);
		}
		string text = MainColor;
		string text2 = TileColor;
		string text3 = DetailColor;
		string text4 = BackgroundColor;
		int num = -1;
		if (text != null)
		{
			if (text.Contains(","))
			{
				string[] array = text.Split(',');
				num = Stat.Rand.Next(0, array.Length);
				text = array[num];
			}
			if (text == "bright")
			{
				text = Crayons.GetRandomColor();
			}
			if (text == "all")
			{
				text = Crayons.GetRandomColorAll();
			}
			if (MainColorFieldIsForegroundBackground)
			{
				text = "&" + text;
			}
		}
		if (text2 != null)
		{
			if (text2.Contains(","))
			{
				string[] array2 = text2.Split(',');
				num = Stat.Rand.Next(0, array2.Length);
				text2 = array2[num];
			}
			if (text2 == "bright")
			{
				text2 = Crayons.GetRandomColor();
			}
			if (text2 == "all")
			{
				text2 = Crayons.GetRandomColorAll();
			}
			if (TileColorFieldIsForegroundBackground)
			{
				text2 = "&" + text2;
			}
		}
		if (text4 != null)
		{
			if (text4.Contains(","))
			{
				text4 = text4.Split(',').GetRandomElement();
			}
			if (text4 == "bright")
			{
				text4 = Crayons.GetRandomColor();
			}
			if (text4 == "all")
			{
				text4 = Crayons.GetRandomColorAll();
			}
		}
		if (text3 != null)
		{
			if (text3.Contains(","))
			{
				string[] array3 = text3.Split(',');
				if (!PairDetailWithForeground || num < 0 || num >= array3.Length)
				{
					num = Stat.Rand.Next(0, array3.Length);
				}
				text3 = array3[num];
			}
			if (text3 == "bright")
			{
				text3 = Crayons.GetRandomColor();
			}
			if (text3 == "all")
			{
				text3 = Crayons.GetRandomColorAll();
			}
			if (DetailColorFieldIsForegroundBackground)
			{
				text3 = "&" + text3;
			}
		}
		if (ReferencePart == "Render" && MainColorField == "ColorString" && DetailColorField == "DetailColor" && MainColorFieldIsForegroundBackground && !DetailColorFieldIsForegroundBackground)
		{
			Render pRender = ParentObject.pRender;
			if (pRender == null)
			{
				Debug.LogError("no Render part in " + ParentObject.DisplayNameOnly);
				return true;
			}
			if (text != null)
			{
				pRender.ColorString = text;
			}
			if (text4 != null)
			{
				pRender.ColorString = pRender.ColorString.Split('^')[0] + "^" + text4;
			}
			if (text2 != null)
			{
				pRender.TileColor = text2;
			}
			if (text3 != null)
			{
				pRender.DetailColor = text3;
			}
			if (SetColorStringToDetailColor)
			{
				pRender.ColorString = "&" + pRender.DetailColor;
			}
		}
		else
		{
			IPart part = ParentObject.GetPart(ReferencePart);
			if (part == null)
			{
				Debug.LogError("no " + ReferencePart + " part in " + ParentObject.DisplayNameOnly);
				return true;
			}
			Type type = part.GetType();
			if (text != null)
			{
				FieldInfo field = type.GetField(MainColorField);
				if (field != null)
				{
					field.SetValue(part, text);
				}
				else
				{
					PropertyInfo property = type.GetProperty(MainColorField);
					if (property != null)
					{
						property.SetValue(part, text, null);
					}
					else
					{
						Debug.LogError("no " + MainColorField + " field or property in " + ReferencePart + " part in " + ParentObject.DisplayNameOnly);
					}
				}
			}
			if (text2 != null)
			{
				FieldInfo field2 = type.GetField(TileColorField);
				if (field2 != null)
				{
					field2.SetValue(part, text2);
				}
				else
				{
					PropertyInfo property2 = type.GetProperty(TileColorField);
					if (property2 != null)
					{
						property2.SetValue(part, text2, null);
					}
					else
					{
						Debug.LogError("no " + TileColorField + " field or property in " + ReferencePart + " part in " + ParentObject.DisplayNameOnly);
					}
				}
			}
			if (text3 != null)
			{
				FieldInfo field3 = type.GetField(DetailColorField);
				if (field3 != null)
				{
					field3.SetValue(part, text3);
				}
				else
				{
					PropertyInfo property3 = type.GetProperty(DetailColorField);
					if (property3 != null)
					{
						property3.SetValue(part, text3, null);
					}
					else
					{
						Debug.LogError("no " + DetailColorField + " field or property in " + ReferencePart + " part in " + ParentObject.DisplayNameOnly);
					}
				}
			}
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}
}
