using System.Collections.Generic;
using NUnit.Framework;
using XRL.World;

namespace XRL.UI;

public class ObjectFinderTests
{
	public class TestContext : ObjectFinder.Context
	{
		public TestContext(ObjectFinder finder)
			: base(finder)
		{
		}

		public override string GetDisplayName()
		{
			return "TestContext";
		}
	}

	[TestCase(new object[] { })]
	public void TestCaseOne()
	{
		ObjectFinder objectFinder = ObjectFinder.Reset();
		ObjectFinder.Context context = new TestContext(objectFinder);
		List<GameObject> list = new List<GameObject>
		{
			new GameObject(),
			new GameObject(),
			new GameObject()
		};
		list[0].DisplayName = "One";
		list[1].DisplayName = "two";
		list[2].DisplayName = "a definitely unexpected three";
		objectFinder.UpdateContext(context, list);
		objectFinder.UpdateFilter();
		List<GameObject> list2 = new List<GameObject>(objectFinder.readItems());
		Assert.AreEqual(list[2].DisplayName, list2[0].DisplayName);
	}
}
