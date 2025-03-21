using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Vintage")]
public class CC_Vintage : CC_LookupFilter
{
	public enum Filter
	{
		None,
		F1977,
		Aden,
		Amaro,
		Brannan,
		Crema,
		Earlybird,
		Hefe,
		Hudson,
		Inkwell,
		Kelvin,
		LoFi,
		Ludwig,
		Mayfair,
		Nashville,
		Perpetua,
		Rise,
		Sierra,
		Slumber,
		Sutro,
		Toaster,
		Valencia,
		Walden,
		Willow,
		XProII
	}

	public Filter filter;

	protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (filter == Filter.None)
		{
			lookupTexture = null;
		}
		else
		{
			lookupTexture = Resources.Load<Texture2D>("Instagram/" + filter);
		}
		base.OnRenderImage(source, destination);
	}
}
