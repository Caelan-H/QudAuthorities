using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;
using XRL.World.Tinkering;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tinkering_Disassemble : BaseSkill
{
	private static List<string> ModNames = new List<string>();

	private static StringBuilder ModNamesSB = new StringBuilder();

	public static void Init()
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AllowInventoryStackEvent.ID && ID != AutoexploreObjectEvent.ID && ID != OwnerGetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AllowInventoryStackEvent E)
	{
		if (WantToDisassemble(E.Item))
		{
			return false;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if ((!E.Want || E.FromAdjacent == null) && WantToDisassemble(E.Item))
		{
			E.Want = true;
			E.FromAdjacent = "DisassembleAll";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OwnerGetInventoryActionsEvent E)
	{
		if (CanBeConsideredScrap(E.Object) && !TinkeringHelpers.ConsiderStandardScrap(E.Object) && E.Actor.IsPlayer() && E.Object.Understood())
		{
			if (CheckScrapToggle(E.Object))
			{
				E.AddAction("Toggle Scrap", "stop treating these as scrap", "ToggleScrap", "scrap", 'S', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
			}
			else
			{
				E.AddAction("Toggle Scrap", "treat these as scrap", "ToggleScrap", "scrap", 'S', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ToggleScrap" && E.Actor.IsPlayer())
		{
			ToggleScrap(E.Item);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Took");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Took")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (WantToDisassemble(gameObjectParameter))
			{
				InventoryActionEvent.Check(gameObjectParameter, ParentObject, gameObjectParameter, "DisassembleAll", Auto: true);
			}
		}
		return base.FireEvent(E);
	}

	private string ModProfile(GameObject obj)
	{
		ModNames.Clear();
		int i = 0;
		for (int count = obj.PartsList.Count; i < count; i++)
		{
			if (obj.PartsList[i] is IModification modification)
			{
				ModNames.Add(modification.Name);
			}
		}
		if (ModNames.Count > 0)
		{
			if (ModNames.Count > 1)
			{
				ModNames.Sort();
				ModNamesSB.Clear();
				foreach (string modName in ModNames)
				{
					ModNamesSB.Append('+').Append(modName);
				}
				return ModNamesSB.ToString();
			}
			return "+" + ModNames[0];
		}
		return "";
	}

	private string ToggleKey(GameObject obj)
	{
		string text = "ScrapToggle_" + obj.GetTinkeringBlueprint() + ModProfile(obj);
		if (obj.GetPart("Tinkering_Mine") is Tinkering_Mine tinkering_Mine)
		{
			text = ((tinkering_Mine.Timer > 0) ? (text + "/AsBomb") : (text + "/AsMine"));
		}
		return text;
	}

	public static bool CanBeConsideredScrap(GameObject obj)
	{
		if (!obj.IsReal)
		{
			return false;
		}
		if (!(obj.GetPart("TinkerItem") is TinkerItem tinkerItem))
		{
			return false;
		}
		if (!tinkerItem.CanDisassemble)
		{
			return false;
		}
		if (obj.IsCreature)
		{
			return false;
		}
		if (obj.GetPart("Tinkering_Mine") is Tinkering_Mine tinkering_Mine && tinkering_Mine.Armed)
		{
			return false;
		}
		if (obj.HasTag("BaseObject"))
		{
			return false;
		}
		return true;
	}

	public bool CheckScrapToggle(GameObject obj)
	{
		return IComponent<GameObject>.TheGame.GetBooleanGameState(ToggleKey(obj));
	}

	private void SetScrapToggle(GameObject obj, bool flag)
	{
		if (flag)
		{
			IComponent<GameObject>.TheGame.SetBooleanGameState(ToggleKey(obj), Value: true);
		}
		else
		{
			IComponent<GameObject>.TheGame.RemoveBooleanGameState(ToggleKey(obj));
		}
	}

	private void ToggleScrap(GameObject obj)
	{
		SetScrapToggle(obj, !CheckScrapToggle(obj));
	}

	public bool ConsiderScrap(GameObject obj)
	{
		if (!TinkeringHelpers.ConsiderStandardScrap(obj))
		{
			return CheckScrapToggle(obj);
		}
		return true;
	}

	public bool WantToDisassemble(GameObject obj)
	{
		if (!ParentObject.IsPlayer())
		{
			return false;
		}
		if (!Options.AutoDisassembleScrap2)
		{
			return false;
		}
		if (!GameObject.validate(ref obj))
		{
			return false;
		}
		if (!CanBeConsideredScrap(obj))
		{
			return false;
		}
		if (!ConsiderScrap(obj))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(obj.Owner))
		{
			return false;
		}
		if (obj.IsImportant())
		{
			return false;
		}
		if (obj.IsInStasis())
		{
			return false;
		}
		return true;
	}

	public override bool AddSkill(GameObject GO)
	{
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		return base.RemoveSkill(GO);
	}
}
