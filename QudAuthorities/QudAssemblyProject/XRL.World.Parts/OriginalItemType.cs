using System;
using ConsoleLib.Console;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class OriginalItemType : IPart
{
	public string Article;

	public string DisplayNameOnlyDirect;

	public string BaseDisplayName;

	public string ShortDisplayName;

	public string DisplayName;

	[FieldSaveVersion(255)]
	public string UnitName;

	public override void Initialize()
	{
		base.Initialize();
		Article = ParentObject.a;
		DisplayNameOnlyDirect = ParentObject.DisplayNameOnlyDirect;
		BaseDisplayName = ParentObject.BaseDisplayName;
		ShortDisplayName = ParentObject.ShortDisplayName;
		DisplayName = ParentObject.DisplayName;
		UnitName = ParentObject.GetUnitName();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		string text;
		if (!string.IsNullOrEmpty(UnitName) && !UnitName.StartsWith(DisplayName))
		{
			text = Grammar.A(UnitName);
		}
		else
		{
			text = UnitName ?? DisplayNameOnlyDirect;
			if (!string.IsNullOrEmpty(Article))
			{
				text = ((Article == "a " || Article == "an " || Article == "a" || Article == "an") ? Grammar.A(text) : ((!Article.EndsWith(" ")) ? (Article + " " + text) : (Article + text)));
			}
		}
		text = ColorUtility.CapitalizeExceptFormatting(text);
		E.Base.Insert(0, text + ". ");
		return base.HandleEvent(E);
	}
}
