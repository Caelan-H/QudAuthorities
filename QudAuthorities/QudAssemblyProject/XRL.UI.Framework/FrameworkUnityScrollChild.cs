namespace XRL.UI.Framework;

public class FrameworkUnityScrollChild : FrameworkContext
{
	public virtual ScrollChildContext scrollContext
	{
		get
		{
			return (ScrollChildContext)context;
		}
		set
		{
			context = value;
		}
	}
}
