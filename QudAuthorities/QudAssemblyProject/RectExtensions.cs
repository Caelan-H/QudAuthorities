using UnityEngine;

public static class RectExtensions
{
	public static Matrix4x4 TransformTo(this Transform from, Transform to)
	{
		return to.worldToLocalMatrix * from.localToWorldMatrix;
	}

	public static bool Contains(this Rect source, Rect target)
	{
		if (source.Contains(new Vector2(target.xMin, target.yMin)))
		{
			return source.Contains(new Vector2(target.xMax, target.yMax));
		}
		return false;
	}

	public static Rect RectRelativeTo(this RectTransform transform, Transform to)
	{
		Matrix4x4 matrix4x = transform.TransformTo(to);
		Rect rect = transform.rect;
		Vector3 point = new Vector2(rect.xMin, rect.yMin);
		Vector3 point2 = new Vector2(rect.xMax, rect.yMax);
		point = matrix4x.MultiplyPoint(point);
		point2 = matrix4x.MultiplyPoint(point2);
		rect.xMin = point.x;
		rect.yMin = point.y;
		rect.xMax = point2.x;
		rect.yMax = point2.y;
		return rect;
	}
}
