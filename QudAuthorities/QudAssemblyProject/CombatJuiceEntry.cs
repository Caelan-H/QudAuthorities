public class CombatJuiceEntry
{
	public long turn;

	public float duration;

	public float t;

	public bool async;

	public static GameManager gameManager => GameManager.Instance;

	public virtual bool canStart()
	{
		return true;
	}

	public virtual void start()
	{
	}

	public virtual void update()
	{
	}

	public virtual void finish()
	{
	}
}
