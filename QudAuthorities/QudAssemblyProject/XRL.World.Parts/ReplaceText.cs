using System;

namespace XRL.World.Parts;

[Serializable]
public class ReplaceText : IPart
{
	public string Variables;

	public string Replacements;

	public bool bReplaceInDisplayName = true;

	public bool bReplaceInDescription = true;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		Replace();
		return base.HandleEvent(E);
	}

	public void Replace()
	{
		string[] array = Variables.Split(',');
		string[] array2 = (string.IsNullOrEmpty(Replacements) ? null : Replacements.Split(','));
		if (bReplaceInDisplayName)
		{
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				if (The.Game.HasStringGameState(text))
				{
					ParentObject.pRender.DisplayName = ParentObject.pRender.DisplayName.Replace(text, The.Game.GetStringGameState(text));
				}
				else if (array2 != null && array2.Length >= i)
				{
					ParentObject.pRender.DisplayName = ParentObject.pRender.DisplayName.Replace(text, array2[i]);
				}
			}
		}
		if (!bReplaceInDescription)
		{
			return;
		}
		Description description = ParentObject.GetPart("Description") as Description;
		for (int j = 0; j < array.Length; j++)
		{
			string text2 = array[j];
			if (The.Game.HasStringGameState(text2))
			{
				description._Short = description._Short.Replace(text2, The.Game.GetStringGameState(text2));
			}
			else if (array2 != null && array2.Length >= j)
			{
				description._Short = description._Short.Replace(text2, array2[j]);
			}
		}
	}
}
