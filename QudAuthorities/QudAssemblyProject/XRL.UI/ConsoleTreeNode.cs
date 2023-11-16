using System.Collections.Generic;

namespace XRL.UI;

public abstract class ConsoleTreeNode<T> where T : ConsoleTreeNode<T>
{
	public string Category;

	public bool Expand;

	public T ParentNode;

	public ConsoleTreeNode(string Category = "", bool Expand = false, T ParentNode = null)
	{
		this.Category = Category;
		this.Expand = Expand;
		this.ParentNode = ParentNode;
	}

	public static int NextVisible(List<T> Nodes, ref int Index, int Mod = 0)
	{
		int i;
		for (i = Index + Mod; i < Nodes.Count; i++)
		{
			T parentNode = Nodes[i].ParentNode;
			if (parentNode == null || parentNode.Expand)
			{
				break;
			}
		}
		if (i != Nodes.Count)
		{
			return Index = i;
		}
		return Index;
	}

	public static int PrevVisible(List<T> Nodes, ref int Index, int Mod = 0)
	{
		int num;
		for (num = Index + Mod; num >= 0; num--)
		{
			T parentNode = Nodes[num].ParentNode;
			if (parentNode == null || parentNode.Expand)
			{
				break;
			}
		}
		if (num != -1)
		{
			return Index = num;
		}
		return Index;
	}
}
