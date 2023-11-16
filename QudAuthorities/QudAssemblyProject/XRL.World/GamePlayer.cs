using System;
using System.Collections.Generic;
using XRL.Messages;
using XRL.UI;
using XRL.World.Conversations;

namespace XRL.World;

[Serializable]
public class GamePlayer
{
	[NonSerialized]
	public GameObject _Body;

	public MessageQueue Messages = new MessageQueue();

	[Obsolete("save compat")]
	public Dictionary<string, bool> ConversationNodesVisited = new Dictionary<string, bool>();

	[Obsolete("save compat")]
	public Dictionary<string, bool> ConversationItemsGiven = new Dictionary<string, bool>();

	public GameObject Body
	{
		get
		{
			return _Body;
		}
		set
		{
			if (value != null)
			{
				Cell currentCell = value.CurrentCell;
				if (currentCell != null && currentCell != The.Graveyard)
				{
					_Body = value;
					The.ZoneManager.SetActiveZone(currentCell.ParentZone.ZoneID);
					The.ActionManager.AddActiveObject(_Body);
					The.ZoneManager.ProcessGoToPartyLeader();
				}
				value.pRender.RenderLayer = 100;
				value.pBrain.Goals.Clear();
				AbilityManager.UpdateFavorites();
			}
			else
			{
				_Body = value;
			}
		}
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.WriteObject(this);
		Writer.WriteGameObject(_Body);
		Writer.Write(ConversationNodesVisited);
		Writer.Write(ConversationItemsGiven);
		Choice.StoreHashes();
	}

	public static GamePlayer Load(SerializationReader Reader)
	{
		GamePlayer obj = (GamePlayer)Reader.ReadObject();
		obj._Body = Reader.ReadGameObject("player body");
		obj.ConversationNodesVisited = Reader.ReadDictionary<string, bool>();
		obj.ConversationItemsGiven = Reader.ReadDictionary<string, bool>();
		return obj;
	}
}
