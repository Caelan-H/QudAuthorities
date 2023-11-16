using System;

namespace XRL.World.Parts;

[Serializable]
public class ItemConvertor : IPoweredPart
{
	public const string DERIVATION_CONTEXT = "ItemConvertor";

	public string ConversionTag;

	public string Verb;

	public string Preposition;

	public int Chance = 100;

	public bool AllowRandomMods;

	public ItemConvertor()
	{
		ChargeUse = 500;
		WorksOnInventory = true;
		NameForStatus = "ConversionSystems";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	private bool ProcessItem(GameObject obj)
	{
		string text = obj?.GetPropertyOrTag(ConversionTag);
		if (string.IsNullOrEmpty(text))
		{
			return true;
		}
		if (!Chance.in100())
		{
			return true;
		}
		bool flag = obj.GetIntProperty("StoredByPlayer") > 0;
		GameObject gameObject = null;
		try
		{
			gameObject = ((!AllowRandomMods) ? GameObject.createUnmodified(text) : GameObject.create(text));
		}
		catch (Exception x)
		{
			MetricsManager.LogException("ItemConvertor", x);
		}
		if (gameObject == null)
		{
			return true;
		}
		if (flag)
		{
			obj.SetIntProperty("FromStoredByPlayer", 1);
		}
		obj.SplitFromStack();
		try
		{
			WasDerivedFromEvent.Send(obj, ParentObject, gameObject, "ItemConvertor");
		}
		catch (Exception message)
		{
			MetricsManager.LogError(message);
		}
		try
		{
			DerivationCreatedEvent.Send(gameObject, ParentObject, obj, "ItemConvertor");
		}
		catch (Exception message2)
		{
			MetricsManager.LogError(message2);
		}
		if (!string.IsNullOrEmpty(Verb))
		{
			DidXToYWithZ(Verb, obj, Preposition, gameObject, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, indefiniteDirectObject: true, indefiniteIndirectObject: true);
		}
		obj.ReplaceWith(gameObject);
		obj.CheckStack();
		return false;
	}

	public bool ProcessItems()
	{
		bool num = ForeachActivePartSubjectWhile(ProcessItem, MayMoveAddOrDestroy: true);
		if (!num)
		{
			ConsumeCharge();
		}
		return num;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ProcessItems();
		}
		return base.FireEvent(E);
	}
}
