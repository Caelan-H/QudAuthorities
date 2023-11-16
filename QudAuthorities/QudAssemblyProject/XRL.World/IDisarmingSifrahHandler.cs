namespace XRL.World;

public interface IDisarmingSifrahHandler
{
	void DisarmingResultSuccess(GameObject who, GameObject obj);

	void DisarmingResultExceptionalSuccess(GameObject who, GameObject obj);

	void DisarmingResultPartialSuccess(GameObject who, GameObject obj);

	void DisarmingResultFailure(GameObject who, GameObject obj);

	void DisarmingResultCriticalFailure(GameObject who, GameObject obj);
}
