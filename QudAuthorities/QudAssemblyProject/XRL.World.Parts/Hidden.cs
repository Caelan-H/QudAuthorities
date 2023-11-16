using System;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Hidden : IPart
{
	public bool _Found;

	public int Difficulty = 15;

	public bool Silent;

	public bool Found
	{
		get
		{
			return _Found;
		}
		set
		{
			if (_Found != value)
			{
				bool flag = false;
				if (!_Found && value)
				{
					flag = true;
				}
				_Found = value;
				if (flag)
				{
					RevealInternal();
				}
			}
		}
	}

	public Hidden()
	{
	}

	public Hidden(int Difficulty = 15, bool Silent = false)
		: this()
	{
		this.Difficulty = Difficulty;
		this.Silent = Silent;
	}

	public Hidden(Hidden source)
		: this()
	{
		Difficulty = source.Difficulty;
		_Found = source._Found;
		Silent = source.Silent;
	}

	public override void Initialize()
	{
		base.Initialize();
		if (!Found)
		{
			if (ParentObject.pRender.CustomRender && ParentObject.HasIntProperty("CustomRenderSources"))
			{
				ParentObject.ModIntProperty("CustomRenderSources", 1);
			}
			ParentObject.pRender.CustomRender = true;
			ParentObject.ModIntProperty("CustomRenderSources", 1);
		}
	}

	public override bool SameAs(IPart p)
	{
		Hidden hidden = p as Hidden;
		if (hidden._Found != _Found)
		{
			return false;
		}
		if (hidden.Difficulty != Difficulty)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID && (ID != GetAdjacentNavigationWeightEvent.ID || _Found))
		{
			if (ID == GetNavigationWeightEvent.ID)
			{
				return !_Found;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (!_Found)
		{
			E.Weight = E.PriorWeight;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		if (!_Found)
		{
			E.Weight = E.PriorWeight;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (!Found)
		{
			ParentObject.pRender.Visible = false;
		}
		else
		{
			ParentObject.pRender.Visible = true;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CustomRender");
		Object.RegisterPartEvent(this, "Searched");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CustomRender")
		{
			if (!Found && E.GetParameter("RenderEvent") is RenderEvent renderEvent && (renderEvent.Lit == LightLevel.Radar || renderEvent.Lit == LightLevel.LitRadar))
			{
				Found = true;
			}
		}
		else if (E.ID == "Searched" && !Found)
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Searcher");
			if (E.GetIntParameter("Bonus") + Stat.Random(1, gameObjectParameter.Stat("Intelligence")) >= Difficulty)
			{
				Found = true;
			}
		}
		return base.FireEvent(E);
	}

	public new void Reveal()
	{
		Found = true;
	}

	public void Hide()
	{
		Found = false;
	}

	public void Reveal(bool Silent = false)
	{
		if (Silent && !Found)
		{
			_Found = true;
			RevealInternal(Silent: true);
		}
		else
		{
			Reveal();
		}
	}

	private void RevealInternal(bool Silent = false)
	{
		if (ParentObject.pRender != null)
		{
			ParentObject.pRender.Visible = true;
			ParentObject.ModIntProperty("CustomRenderSources", -1);
			if (ParentObject.GetIntProperty("CustomRenderSources") <= 0)
			{
				ParentObject.pRender.CustomRender = false;
			}
		}
		if (Visible())
		{
			if (!Silent && !this.Silent)
			{
				DidX("are", "revealed", "!", null, null, null, UseFullNames: false, IndefiniteSubject: true, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
			}
			if (AutoAct.IsActive() && (!ParentObject.HasTag("Creature") || IComponent<GameObject>.ThePlayer.IsRelevantHostile(ParentObject)))
			{
				AutoAct.Interrupt(null, null, ParentObject);
			}
		}
		ParentObject.FireEvent("Found");
	}

	private void HideInternal()
	{
		if (ParentObject.pRender != null)
		{
			ParentObject.pRender.Visible = false;
			ParentObject.ModIntProperty("CustomRenderSources", 1);
			if (ParentObject.GetIntProperty("CustomRenderSources") > 0)
			{
				ParentObject.pRender.CustomRender = true;
			}
		}
		if (Visible())
		{
			if (!Silent)
			{
				DidX("disappear", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
			}
			if (AutoAct.IsActive() && (!ParentObject.HasTag("Creature") || IComponent<GameObject>.ThePlayer.IsRelevantHostile(ParentObject)))
			{
				AutoAct.Interrupt(null, null, ParentObject);
			}
		}
		ParentObject.FireEvent("Hidden");
	}

	public static void CarryOver(GameObject src, GameObject dest)
	{
		if (src.GetPart("Hidden") is Hidden source)
		{
			dest.RemovePart("Hidden");
			dest.AddPart(new Hidden(source));
		}
	}
}
