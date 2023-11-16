using System;
using Qud.API;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CurableFungalInfection : IPart
{
	[Obsolete("save compat")]
	public int Placeholder;

	private GameObject Equipped => ParentObject.Equipped;

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Equipped");
		Object.RegisterPartEvent(this, "Unequipped");
		Object.RegisterPartEvent(this, "AppliedLiquidCovered");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AppliedLiquidCovered")
		{
			if (Equipped == null)
			{
				return true;
			}
			if (IComponent<GameObject>.TheGame.GetStringGameState("FungalCureLiquid") == "")
			{
				IComponent<GameObject>.TheCore.GenerateFungalCure();
			}
			string key = "gel";
			string stringGameState = IComponent<GameObject>.TheGame.GetStringGameState("FungalCureLiquid");
			LiquidVolume parameter = E.GetParameter<LiquidVolume>("Liquid");
			if (!parameter.ComponentLiquids.ContainsKey(key))
			{
				return true;
			}
			if (!parameter.ComponentLiquids.ContainsKey(stringGameState))
			{
				return true;
			}
			if (!Equipped.HasEffect("FungalCureQueasy"))
			{
				IComponent<GameObject>.EmitMessage(Equipped, Equipped.Poss("skin itches furiously."), FromDialog: false, UsePopup: true);
			}
			else if (ParentObject.Blueprint.Equals("PaxInfection"))
			{
				IComponent<GameObject>.EmitMessage(Equipped, ParentObject.DisplayNameOnlyDirect + " is immune to conventional treatments.", FromDialog: false, UsePopup: true);
			}
			else
			{
				Cure();
			}
		}
		else if (E.ID == "Eating")
		{
			string stringGameState2 = IComponent<GameObject>.TheGame.GetStringGameState("FungalCureWorm");
			if (string.IsNullOrEmpty(stringGameState2))
			{
				IComponent<GameObject>.TheCore.GenerateFungalCure();
				stringGameState2 = IComponent<GameObject>.TheGame.GetStringGameState("FungalCureWorm");
			}
			GameObject gameObjectParameter = E.GetGameObjectParameter("Food");
			if (gameObjectParameter != null && gameObjectParameter.Blueprint == stringGameState2 && Equipped != null && !Equipped.HasEffect("FungalCureQueasy"))
			{
				Equipped.ApplyEffect(new FungalCureQueasy(100));
			}
		}
		else if (E.ID == "Equipped")
		{
			E.GetGameObjectParameter("EquippingObject").RegisterPartEvent(this, "Eating");
		}
		else if (E.ID == "Unequipped")
		{
			E.GetGameObjectParameter("UnequippingObject").UnregisterPartEvent(this, "Eating");
		}
		return base.FireEvent(E);
	}

	public bool Cure()
	{
		GameObject equipped = Equipped;
		string text = ParentObject.EquippedOn()?.GetOrdinalName() ?? "body";
		if (!ParentObject.Destroy(null, Silent: true))
		{
			return false;
		}
		if (equipped != null)
		{
			IComponent<GameObject>.EmitMessage(equipped, "The infected crust of skin on " + equipped.poss(text) + " loosens and breaks away.", FromDialog: false, UsePopup: true);
			if (equipped.IsPlayer())
			{
				JournalAPI.AddAccomplishment("To the dismay of fungi everywhere, you cured the " + ParentObject.DisplayNameOnlyStripped + " infection on your " + text + ".", "Bless the " + Calendar.getDay() + " of " + Calendar.getMonth() + ", when =name= dissolved a sham alliance with the treacherous fungi by curing the " + ParentObject.DisplayNameOnlyStripped + " infection on " + equipped.GetPronounProvider().PossessiveAdjective + " " + text + ".", "general", JournalAccomplishment.MuralCategory.BodyExperienceNeutral, JournalAccomplishment.MuralWeight.Medium, null, -1L);
			}
		}
		return true;
	}
}
