using System;
using System.Linq;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class AddsRep : IActivePart
{
	public string Faction = "";

	public int Value;

	public bool AppliedBonus;

	public AddsRep()
	{
		WorksOnEquipper = true;
	}

	public AddsRep(string _Faction)
		: this()
	{
		Faction = _Faction;
	}

	public AddsRep(string _Faction, int _Value)
		: this(_Faction)
	{
		Value = _Value;
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		AddsRep obj = base.DeepCopy(Parent, MapInv) as AddsRep;
		obj.AppliedBonus = false;
		return obj;
	}

	public override bool SameAs(IPart p)
	{
		AddsRep addsRep = p as AddsRep;
		if (addsRep.Faction != Faction)
		{
			return false;
		}
		if (addsRep.Value != Value)
		{
			return false;
		}
		if (addsRep.AppliedBonus != AppliedBonus)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		CheckApplyBonus(null, UseCharge: true);
	}

	public override void TenTurnTick(long TurnNumber)
	{
		CheckApplyBonus(null, UseCharge: true, 10);
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		CheckApplyBonus(null, UseCharge: true, 100);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BootSequenceDoneEvent.ID && ID != BootSequenceInitializedEvent.ID && ID != CellChangedEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != EquippedEvent.ID && ID != UnequippedEvent.ID && ID != GetShortDescriptionEvent.ID && (!WorksOnCarrier || ID != TakenEvent.ID))
		{
			if (WorksOnCarrier)
			{
				return ID == DroppedEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Faction.StartsWith("*allvisiblefactions:"))
		{
			Extensions.AppendRules(text: "+" + Convert.ToInt32(Faction.Split(':')[1]) + " reputation with every faction", SB: E.Postfix);
		}
		else if (Faction == "*alloldfactions")
		{
			int value = Value;
			E.Postfix.AppendRules("+" + value + " reputation with every faction that existed during the time of the sultanate");
		}
		else
		{
			string[] array = Faction.Split(',');
			foreach (string text2 in array)
			{
				string factionName = text2;
				int num = Value;
				if (text2.Contains(':'))
				{
					string[] array2 = text2.Split(':');
					if (array2.Length >= 3 && array2[2].Contains("hidden"))
					{
						continue;
					}
					factionName = array2[0];
					num = Convert.ToInt32(array2[1]);
				}
				E.Postfix.AppendRules(((num >= 0) ? "+" : "") + num + " reputation with " + XRL.World.Faction.getFormattedName(factionName));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CheckApplyBonus(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		CheckApplyBonus(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		CheckApplyBonus(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DroppedEvent E)
	{
		CheckApplyBonus(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceInitializedEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public static AddsRep AddModifier(GameObject go, string Faction)
	{
		AddsRep addsRep = go.GetPart<AddsRep>();
		if (addsRep != null)
		{
			string[] array = Faction.Split(',');
			foreach (string text in array)
			{
				if (addsRep.Faction != "")
				{
					addsRep.Faction += ",";
				}
				if (text.Contains(':'))
				{
					string[] array2 = text.Split(':');
					string text2 = array2[0];
					int num = Convert.ToInt32(array2[1]);
					if (num == addsRep.Value && array2.Length <= 2)
					{
						addsRep.Faction += text2;
					}
					else
					{
						addsRep.Faction += text;
					}
					if (addsRep.AppliedBonus)
					{
						XRLCore.Core.Game.PlayerReputation.modify(text2, num, null, null, silent: true, transient: true);
					}
				}
				else
				{
					addsRep.Faction += text;
					if (addsRep.AppliedBonus)
					{
						XRLCore.Core.Game.PlayerReputation.modify(text, addsRep.Value, null, null, silent: true, transient: true);
					}
				}
			}
		}
		else
		{
			addsRep = new AddsRep(Faction);
			go.AddPart(addsRep);
		}
		return addsRep;
	}

	public static AddsRep AddModifier(GameObject go, string Faction, int Value)
	{
		AddsRep addsRep = go.GetPart<AddsRep>();
		if (addsRep != null)
		{
			string[] array = Faction.Split(',');
			foreach (string text in array)
			{
				if (addsRep.Faction != "")
				{
					addsRep.Faction += ",";
				}
				if (text.Contains(':'))
				{
					string[] array2 = text.Split(':');
					string text2 = array2[0];
					int num = Convert.ToInt32(array2[1]);
					if (num == addsRep.Value && array2.Length <= 2)
					{
						addsRep.Faction += text2;
					}
					else
					{
						addsRep.Faction += text;
					}
					if (addsRep.AppliedBonus)
					{
						XRLCore.Core.Game.PlayerReputation.modify(text2, num, null, null, silent: true, transient: true);
					}
				}
				else
				{
					if (Value == addsRep.Value)
					{
						addsRep.Faction += text;
					}
					else
					{
						AddsRep addsRep2 = addsRep;
						addsRep2.Faction = addsRep2.Faction + text + ":" + Value;
					}
					if (addsRep.AppliedBonus)
					{
						XRLCore.Core.Game.PlayerReputation.modify(text, Value, null, null, silent: true, transient: true);
					}
				}
			}
		}
		else
		{
			addsRep = new AddsRep(Faction, Value);
			go.AddPart(addsRep);
		}
		return addsRep;
	}

	private void ApplyBonus(GameObject who)
	{
		if (AppliedBonus || who == null || !who.IsPlayer() || !IsObjectActivePartSubject(who))
		{
			return;
		}
		if (Faction.StartsWith("*allvisiblefactions:"))
		{
			int delta = Convert.ToInt32(Faction.Split(':')[1]);
			foreach (string visibleFactionName in Factions.getVisibleFactionNames())
			{
				XRLCore.Core.Game.PlayerReputation.modify(visibleFactionName, delta, null, null, silent: true, transient: true);
			}
		}
		else if (Faction == "*alloldfactions")
		{
			int value = Value;
			foreach (Faction item in Factions.loop())
			{
				if (item.Visible && item.Old)
				{
					The.Game.PlayerReputation.modify(item, value, null, null, silent: true, transient: true);
				}
			}
		}
		else
		{
			string[] array = Faction.Split(',');
			foreach (string text in array)
			{
				string faction = text;
				int delta2 = Value;
				if (text.Contains(':'))
				{
					string[] array2 = text.Split(':');
					faction = array2[0];
					delta2 = Convert.ToInt32(array2[1]);
				}
				XRLCore.Core.Game.PlayerReputation.modify(faction, delta2, null, null, silent: true, transient: true);
			}
		}
		AppliedBonus = true;
	}

	private void UnapplyBonus()
	{
		if (!AppliedBonus)
		{
			return;
		}
		if (Faction.StartsWith("*allvisiblefactions:"))
		{
			int num = Convert.ToInt32(Faction.Split(':')[1]);
			foreach (string visibleFactionName in Factions.getVisibleFactionNames())
			{
				XRLCore.Core.Game.PlayerReputation.modify(visibleFactionName, -num, null, null, silent: true, transient: true);
			}
		}
		else if (Faction == "*alloldfactions")
		{
			int value = Value;
			foreach (Faction item in Factions.loop())
			{
				if (item.Visible && item.Old)
				{
					The.Game.PlayerReputation.modify(item, -value, null, null, silent: true, transient: true);
				}
			}
		}
		else
		{
			string[] array = Faction.Split(',');
			foreach (string text in array)
			{
				string faction = text;
				int num2 = Value;
				if (text.Contains(':'))
				{
					string[] array2 = text.Split(':');
					faction = array2[0];
					num2 = Convert.ToInt32(array2[1]);
				}
				XRLCore.Core.Game.PlayerReputation.modify(faction, -num2, null, null, silent: true, transient: true);
			}
		}
		AppliedBonus = false;
	}

	public void CheckApplyBonus(GameObject who = null, bool UseCharge = false, int MultipleCharge = 1)
	{
		if (who == null)
		{
			who = GetActivePartFirstSubject();
		}
		else if (IsObjectActivePartSubject(who))
		{
			who.RegisterPartEvent(this, "DominationStarted");
			who.RegisterPartEvent(this, "DominationEnded");
		}
		else
		{
			who.UnregisterPartEvent(this, "DominationStarted");
			who.UnregisterPartEvent(this, "DominationEnded");
		}
		if (AppliedBonus)
		{
			if (IsDisabled(UseCharge, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, MultipleCharge, null, UseChargeIfUnpowered: false, 0L))
			{
				UnapplyBonus();
			}
		}
		else if (IsReady(UseCharge, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, MultipleCharge, null, UseChargeIfUnpowered: false, 0L))
		{
			ApplyBonus(who);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DominationEnded")
		{
			if (!E.HasFlag("Metempsychosis"))
			{
				UnapplyBonus();
			}
		}
		else if (E.ID == "DominationStarted")
		{
			CheckApplyBonus();
		}
		return base.FireEvent(E);
	}
}
