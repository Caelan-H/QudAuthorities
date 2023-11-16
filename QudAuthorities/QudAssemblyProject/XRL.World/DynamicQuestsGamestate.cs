using System;
using System.Collections.Generic;
using XRL.Core;

namespace XRL.World;

[Serializable]
[GamestateSingleton("dynamicQuestsGamestate")]
public class DynamicQuestsGamestate : IGamestateSingleton, IObjectGamestateCustomSerializer
{
	public long nextId;

	public Dictionary<string, Quest> quests = new Dictionary<string, Quest>();

	public static DynamicQuestsGamestate instance => XRLCore.Core.Game.GetObjectGameState("dynamicQuestsGamestate") as DynamicQuestsGamestate;

	public IGamestateSingleton GameLoad(SerializationReader reader)
	{
		nextId = reader.ReadInt64();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			quests.Add(reader.ReadString(), Quest.Load(reader));
		}
		return this;
	}

	public void GameSave(SerializationWriter writer)
	{
		writer.Write(nextId);
		writer.Write(quests.Count);
		foreach (KeyValuePair<string, Quest> quest in quests)
		{
			writer.Write(quest.Key);
			quest.Value.Save(writer);
		}
	}

	public static void addQuest(Quest quest)
	{
		instance.quests.Add(quest.ID, quest);
	}

	public void init()
	{
	}

	public void worldBuild()
	{
	}
}
