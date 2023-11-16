using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IInventoryActionsEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Object;

	public Dictionary<string, InventoryAction> Actions;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	public bool AddAction(string Name, string Display = null, string Command = null, string PreferToHighlight = null, char Key = ' ', bool FireOnActor = false, int Default = 0, int Priority = 0, bool Override = false, bool WorksAtDistance = false, bool WorksTelekinetically = false, bool WorksTelepathically = false, bool AsMinEvent = true, GameObject FireOn = null)
	{
		if (Actions == null)
		{
			Actions = new Dictionary<string, InventoryAction>();
		}
		else if (!Override && Actions.ContainsKey(Name))
		{
			return false;
		}
		InventoryAction inventoryAction = new InventoryAction();
		inventoryAction.Name = Name;
		inventoryAction.Key = Key;
		inventoryAction.Display = Display;
		inventoryAction.Command = Command;
		inventoryAction.PreferToHighlight = PreferToHighlight;
		inventoryAction.Default = Default;
		inventoryAction.Priority = Priority;
		inventoryAction.FireOnActor = FireOnActor;
		inventoryAction.WorksAtDistance = WorksAtDistance;
		inventoryAction.WorksTelekinetically = WorksTelekinetically;
		inventoryAction.WorksTelepathically = WorksTelepathically;
		inventoryAction.AsMinEvent = AsMinEvent;
		inventoryAction.FireOn = FireOn;
		Actions[Name] = inventoryAction;
		return true;
	}

	public override void Reset()
	{
		Actor = null;
		Object = null;
		Actions = null;
		base.Reset();
	}
}
