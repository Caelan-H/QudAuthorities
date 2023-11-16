namespace XRL.World;

public interface IHackingSifrahHandler
{
	void HackingResultSuccess(GameObject who, GameObject obj, HackingSifrah game);

	void HackingResultExceptionalSuccess(GameObject who, GameObject obj, HackingSifrah game);

	void HackingResultPartialSuccess(GameObject who, GameObject obj, HackingSifrah game);

	void HackingResultFailure(GameObject who, GameObject obj, HackingSifrah game);

	void HackingResultCriticalFailure(GameObject who, GameObject obj, HackingSifrah game);
}
