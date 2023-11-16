using System.Collections.Generic;

namespace XRL.UI.Framework;

public class ProxyNavigationContext : NavigationContext
{
	public NavigationContext proxyTo;

	public override bool disabled
	{
		get
		{
			return proxyTo?.disabled ?? base.disabled;
		}
		set
		{
			if (proxyTo != null)
			{
				proxyTo.disabled = value;
			}
			else
			{
				base.disabled = value;
			}
		}
	}

	public override IEnumerable<NavigationContext> children
	{
		get
		{
			if (proxyTo != null)
			{
				yield return proxyTo;
			}
			foreach (NavigationContext child in base.children)
			{
				yield return child;
			}
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (proxyTo != null && !base.currentEvent.handled && !base.currentEvent.cancelled && (NavigationContext)base.currentEvent.data["to"] == this)
		{
			proxyTo.Activate();
			base.currentEvent.Cancel();
		}
	}
}
