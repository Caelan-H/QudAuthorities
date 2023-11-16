using System;

namespace XRL.World;

[Serializable]
public class QuestStep
{
	public bool Finished;

	public string ID;

	public string Name;

	public string Text;

	public int XP;

	public override string ToString()
	{
		return ID + " n=" + Name + " t=" + Text + " xp=" + XP + " finished=" + Finished;
	}
}
