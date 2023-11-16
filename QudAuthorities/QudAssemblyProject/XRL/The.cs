using System.Threading;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.World.Conversations;

namespace XRL;

public static class The
{
	public static XRLCore Core => XRLCore.Core;

	public static XRLGame Game => Core?.Game;

	public static long CurrentTurn => XRLCore.CurrentTurn;

	public static GameObject Player => Game?.Player?.Body;

	public static Cell PlayerCell => Player?.CurrentCell;

	public static ParticleManager ParticleManager => XRLCore.ParticleManager;

	public static ActionManager ActionManager => Game?.ActionManager;

	public static ZoneManager ZoneManager => Game?.ZoneManager;

	public static Zone ActiveZone => ZoneManager?.ActiveZone;

	public static GraveyardCell Graveyard => Game?.Graveyard;

	public static SynchronizationContext UiContext => GameManager.Instance.uiSynchronizationContext;

	public static SynchronizationContext GameContext => GameManager.Instance.gameThreadSynchronizationContext;

	public static GameObject Speaker => XRL.World.Conversation.Speaker ?? ConversationUI.Speaker;

	public static Dialogue Conversation => ConversationUI.CurrentDialogue;
}
