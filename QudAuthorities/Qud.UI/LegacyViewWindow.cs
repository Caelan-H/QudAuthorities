using QupKit;

namespace Qud.UI;

public class LegacyViewWindow<T> : WindowBase where T : BaseView, new()
{
	protected T baseView;

	protected BaseView Previous;

	public override void Init()
	{
		baseView = new T();
		baseView.AttachTo(base.gameObject);
		baseView.OnCreate();
	}

	public override void Show()
	{
		Previous = LegacyViewManager.Instance.ActiveView;
		Previous?.Leave();
		LegacyViewManager.Instance.ActiveView = baseView;
		baseView.Enter();
		base.Show();
	}

	public override void Hide()
	{
		if (Previous != null)
		{
			LegacyViewManager.Instance.ActiveView = Previous;
			Previous.Enter();
		}
		baseView.Leave();
		base.Hide();
	}
}
