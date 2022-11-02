namespace Qud.UI;

public class SingletonWindowBase<T> : WindowBase where T : class, new()
{
	public static T instance;

	public override void Init()
	{
		instance = this as T;
	}
}
