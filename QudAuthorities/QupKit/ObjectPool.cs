using System.Collections.Generic;

namespace QupKit;

public class ObjectPool<T> where T : new()
{
	private static Queue<T> Pool = new Queue<T>();

	public static T Checkout()
	{
		lock (Pool)
		{
			if (Pool.Count > 0)
			{
				return Pool.Dequeue();
			}
			return new T();
		}
	}

	public static void Return(T item)
	{
		lock (Pool)
		{
			Pool.Enqueue(item);
		}
	}
}
