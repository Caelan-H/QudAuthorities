using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using UnityEngine;
using XRL.World;

namespace XRL.EditorFormats.Map;

public class MapFileCell
{
	public bool bSet;

	public List<MapFileObjectBlueprint> Objects = new List<MapFileObjectBlueprint>();

	public void Clear()
	{
		Objects.Clear();
	}

	public HashSet<string> UsedBlueprints(HashSet<string> result)
	{
		if (result == null)
		{
			result = new HashSet<string>();
		}
		foreach (MapFileObjectBlueprint @object in Objects)
		{
			if (!result.Contains(@object.Name))
			{
				result.Add(@object.Name);
			}
		}
		return result;
	}

	public void Render(MapFileCellRender RenderCell, MapFileCellReference cref)
	{
		if (Objects.Count == 0)
		{
			RenderCell.Char = '.';
			RenderCell.Foreground = ConsoleLib.Console.ColorUtility.ColorMap['K'];
			RenderCell.Background = ConsoleLib.Console.ColorUtility.ColorMap['k'];
			return;
		}
		int num = -1;
		Dictionary<string, GameObjectBlueprint> blueprints = GameObjectFactory.Factory.Blueprints;
		foreach (MapFileObjectBlueprint @object in Objects)
		{
			try
			{
				if (!blueprints.TryGetValue(@object.Name, out var value))
				{
					RenderCell.Foreground = new Color(1f, 0f, 0f, 1f);
					RenderCell.Char = 'x';
					break;
				}
				int num2 = Convert.ToInt32(value.GetPartParameter("Render", "RenderLayer", "-1"));
				if (num2 > num)
				{
					num = num2;
					RenderCell.RenderBlueprint(value, cref);
				}
			}
			catch (Exception ex)
			{
				Debug.Log("Exception rendering " + @object.Name + " : " + ex.Message + ex.StackTrace);
			}
		}
	}

	public void ApplyTo(Cell C, bool CheckEmpty = true, Action<Cell> PreAction = null, Action<Cell> PostAction = null, Func<string, Cell, bool> ShouldPlace = null, Func<string, Cell, string> Replace = null, Action<string, Cell> BeforePlacement = null, Action<string, Cell> AfterPlacement = null)
	{
		if (CheckEmpty && Objects.Count == 0)
		{
			return;
		}
		PreAction?.Invoke(C);
		GameObjectFactory factory = GameObjectFactory.Factory;
		foreach (MapFileObjectBlueprint @object in Objects)
		{
			string text = @object.Name;
			if (ShouldPlace != null && !ShouldPlace(text, C))
			{
				continue;
			}
			if (Replace != null)
			{
				text = Replace(text, C);
			}
			if (!factory.Blueprints.TryGetValue(text, out var value))
			{
				MetricsManager.LogError($"Unknown map object {text} at [{C.Y}, {C.Y}]");
				continue;
			}
			BeforePlacement?.Invoke(text, C);
			XRL.World.GameObject gameObject = C.AddObject(factory.CreateObject(value));
			if (!@object.Owner.IsNullOrEmpty() && gameObject.pPhysics != null)
			{
				gameObject.pPhysics.Owner = @object.Owner;
				if (value.HasTag("Immutable") || value.HasTag("ImmutableWhenUnexplored"))
				{
					gameObject.SetIntProperty("ForceMutableSave", 1);
				}
			}
			if (!@object.Part.IsNullOrEmpty())
			{
				try
				{
					Type type = ModManager.ResolveType("XRL.World.Parts." + @object.Part);
					gameObject.AddPart(Activator.CreateInstance(type) as IPart);
					if (value.HasTag("Immutable") || value.HasTag("ImmutableWhenUnexplored"))
					{
						gameObject.SetIntProperty("ForceMutableSave", 1);
					}
				}
				catch (Exception x)
				{
					MetricsManager.LogError($"Error adding {@object.Part} part to {gameObject.Blueprint} at [{C.Y}, {C.Y}]:", x);
				}
			}
			AfterPlacement?.Invoke(text, C);
		}
		PostAction?.Invoke(C);
	}
}
