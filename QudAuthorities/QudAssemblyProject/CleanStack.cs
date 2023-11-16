using System;
using System.Collections.Generic;

[Serializable]
public class CleanStack<T>
{
	public List<T> Items = new List<T>(100);

	public int Count => Items.Count;

	public void Clear()
	{
		Items.Clear();
	}

	public bool Contains(T Item)
	{
		if (Item == null)
		{
			for (int i = 0; i < Items.Count; i++)
			{
				if (Items[i] == null)
				{
					return true;
				}
			}
		}
		else
		{
			for (int j = 0; j < Items.Count; j++)
			{
				if (Items[j] != null && Items[j].Equals(Item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public T Peek()
	{
		if (Items.Count == 0)
		{
			return default(T);
		}
		return Items[Items.Count - 1];
	}

	public void Push(T Item)
	{
		Items.Add(Item);
	}

	public T Pop()
	{
		T result = Items[Items.Count - 1];
		Items.RemoveAt(Items.Count - 1);
		return result;
	}
}
