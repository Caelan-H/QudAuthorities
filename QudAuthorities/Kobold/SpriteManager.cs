using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XRL;

namespace Kobold;

[HasModSensitiveStaticCache]
public static class SpriteManager
{
	private static GameObject _BaseSpritePrefab;

	private static Shader[] Shaders;

	private static GameObject _BaseSplitSpritePrefab;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<exTextureInfo, Sprite> unitySpriteMap;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, exTextureInfo> InfoMap;

	private static GameObject CloneSpritePrefab()
	{
		if (_BaseSpritePrefab == null)
		{
			_BaseSpritePrefab = Resources.Load("KoboldBaseSprite") as GameObject;
			UnityEngine.Object.DontDestroyOnLoad(_BaseSpritePrefab);
		}
		return UnityEngine.Object.Instantiate(_BaseSpritePrefab);
	}

	public static Shader GetShaderMode(int n)
	{
		if (Shaders == null)
		{
			Shaders = new Shader[2];
			Shaders[0] = Shader.Find("Kobold/Alpha Blended Dual Color");
			Shaders[1] = Shader.Find("Kobold/Alpha Blended Truecolor");
		}
		return Shaders[n];
	}

	private static GameObject CloneSplitSpritePrefab()
	{
		if (_BaseSplitSpritePrefab == null)
		{
			_BaseSplitSpritePrefab = Resources.Load("KoboldBaseSlicedSprite") as GameObject;
			UnityEngine.Object.DontDestroyOnLoad(_BaseSplitSpritePrefab);
		}
		return UnityEngine.Object.Instantiate(_BaseSplitSpritePrefab);
	}

	public static Sprite GetUnitySprite(string path)
	{
		exTextureInfo textureInfo = GetTextureInfo(path);
		if (textureInfo == null)
		{
			Debug.LogError("Unknown sprite: " + path);
			return null;
		}
		return GetUnitySprite(textureInfo);
	}

	public static Sprite GetUnitySprite(exTextureInfo info)
	{
		if (unitySpriteMap == null)
		{
			unitySpriteMap = new Dictionary<exTextureInfo, Sprite>();
		}
		if (unitySpriteMap.ContainsKey(info))
		{
			return unitySpriteMap[info];
		}
		Texture2D texture = info.texture;
		Texture2D texture2D = new Texture2D(info.width, info.height, TextureFormat.ARGB32, mipChain: false);
		texture2D.filterMode = FilterMode.Point;
		Color[] pixels = texture.GetPixels(info.x, info.y, info.width, info.height, 0);
		texture2D.SetPixels(pixels);
		texture2D.Apply();
		Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 40f);
		unitySpriteMap.Add(info, sprite);
		return sprite;
	}

	public static void SetSprite(GameObject Sprite, string Path)
	{
		Sprite.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path);
	}

	public static Vector2i GetSpriteSize(string Path)
	{
		exTextureInfo textureInfo = GetTextureInfo(Path);
		Debug.Log(Path + " " + textureInfo.rawWidth + "x" + textureInfo.rawWidth + "   " + textureInfo.trim_x + "x" + textureInfo.trim_y);
		return new Vector2i(textureInfo.width, textureInfo.height);
	}

	public static exTextureInfo GetTextureInfo(string Path)
	{
		if (Path == null)
		{
			return null;
		}
		if (InfoMap == null)
		{
			MemoryHelper.GCCollect();
			InfoMap = new Dictionary<string, exTextureInfo>();
			KoboldDatabaseScriptable koboldDatabaseScriptable = Resources.Load("KoboldDatabase") as KoboldDatabaseScriptable;
			if (koboldDatabaseScriptable == null)
			{
				UnityEngine.Object[] array = Resources.LoadAll("TextureInfo", typeof(exTextureInfo));
				for (int j = 0; j < array.Length; j++)
				{
					exTextureInfo exTextureInfo = (exTextureInfo)array[j];
					if (exTextureInfo != null)
					{
						try
						{
							InfoMap.Add(exTextureInfo.name.ToLower(), exTextureInfo);
						}
						catch (Exception ex)
						{
							Debug.Log("Error adding - " + exTextureInfo.name.ToLower() + " ... " + ex.Message);
						}
					}
				}
			}
			else
			{
				string[] koboldTextureInfos = koboldDatabaseScriptable.koboldTextureInfos;
				foreach (string text in koboldTextureInfos)
				{
					if (string.IsNullOrEmpty(text))
					{
						Debug.LogWarning("Info in koboldTextureInfos is null");
						continue;
					}
					try
					{
						InfoMap.Add(text, null);
					}
					catch (Exception x)
					{
						MetricsManager.LogException("SpriteManager", x);
					}
				}
			}
			ModManager.ForEachFileIn("Textures", delegate(string f, ModInfo i)
			{
				if (i.IsEnabled && f.ToLower().EndsWith(".png"))
				{
					Texture2D texture2D = new Texture2D(i.TextureConfiguration.TextureWidth, i.TextureConfiguration.TextureHeight);
					byte[] data = File.ReadAllBytes(f);
					texture2D.LoadImage(data);
					texture2D.filterMode = FilterMode.Point;
					f = "assets_content_" + f.Substring(f.ToLower().IndexOf("textures")).Replace('\\', '_').Replace('/', '_');
					f = f.ToLower();
					exTextureInfo exTextureInfo2 = ScriptableObject.CreateInstance<exTextureInfo>();
					exTextureInfo2.texture = texture2D;
					exTextureInfo2.width = texture2D.width;
					exTextureInfo2.height = texture2D.height;
					exTextureInfo2.x = 0;
					exTextureInfo2.y = 0;
					exTextureInfo2.ShaderMode = i.TextureConfiguration.ShaderMode;
					if (InfoMap.ContainsKey(f))
					{
						InfoMap[f] = exTextureInfo2;
					}
					else
					{
						InfoMap.Add(f, exTextureInfo2);
					}
					string key = f.ToLower().Replace(".png", ".bmp");
					if (InfoMap.ContainsKey(key))
					{
						InfoMap[key] = exTextureInfo2;
					}
					else
					{
						InfoMap.Add(key, exTextureInfo2);
					}
					key = f.ToLower().Replace(".png", "");
					if (InfoMap.ContainsKey(key))
					{
						InfoMap[key] = exTextureInfo2;
					}
					else
					{
						InfoMap.Add(key, exTextureInfo2);
					}
				}
			});
		}
		exTextureInfo value = null;
		if (InfoMap.TryGetValue(Path, out value))
		{
			if (value == null)
			{
				value = Resources.Load("TextureInfo/" + Path) as exTextureInfo;
				InfoMap[Path] = value;
			}
			return value;
		}
		if (InfoMap.TryGetValue(Path.ToLower().Replace('/', '_').Replace('\\', '_'), out value))
		{
			if (value == null)
			{
				value = Resources.Load("TextureInfo/" + Path.ToLower().Replace('/', '_').Replace('\\', '_')) as exTextureInfo;
				InfoMap[Path.ToLower().Replace('/', '_').Replace('\\', '_')] = value;
			}
			InfoMap.Add(Path, value);
			return value;
		}
		if (InfoMap.TryGetValue("assets_content_textures_" + Path.ToLower().Replace('/', '_').Replace('\\', '_'), out value))
		{
			if (value == null)
			{
				value = Resources.Load("TextureInfo/assets_content_textures_" + Path.ToLower().Replace('/', '_').Replace('\\', '_')) as exTextureInfo;
				InfoMap["assets_content_textures_" + Path.ToLower().Replace('/', '_').Replace('\\', '_')] = value;
			}
			InfoMap.Add(Path, value);
			return value;
		}
		Debug.Log("Couldn't find " + Path);
		return null;
	}

	public static GameObject CreateEmptySprite()
	{
		return CloneSpritePrefab();
	}

	public static GameObject CreateSplitSprite(string Path)
	{
		GameObject gameObject = CloneSplitSpritePrefab();
		gameObject.GetComponent<ex3DSprite2>().anchor = Anchor.MidCenter;
		gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
		gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
		return gameObject;
	}

	public static GameObject CreateCollidableSprite(string Path, Anchor _Anchor, bool bReusable = false)
	{
		GameObject gameObject = CloneSpritePrefab();
		gameObject.GetComponent<ex3DSprite2>().anchor = _Anchor;
		gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
		gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
		gameObject.GetComponent<ex3DSprite2>().bCollide = true;
		return gameObject;
	}

	public static GameObject CreateCollidableSprite(string Path, bool bReusable = false)
	{
		GameObject gameObject = CloneSpritePrefab();
		gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
		gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
		gameObject.GetComponent<ex3DSprite2>().bCollide = true;
		return gameObject;
	}

	public static GameObject CreateSprite(string Path, Anchor _Anchor, bool bReusable = false)
	{
		GameObject gameObject = CloneSpritePrefab();
		gameObject.GetComponent<ex3DSprite2>().anchor = _Anchor;
		gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
		gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
		return gameObject;
	}

	public static GameObject CreateSprite(string Path, Color Foreground, bool bReusable = false)
	{
		GameObject gameObject = CloneSpritePrefab();
		gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
		gameObject.GetComponent<ex3DSprite2>().color = Foreground;
		gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
		return gameObject;
	}

	public static GameObject CreateSprite(string Path, Color Foreground, Color Background, bool bReusable = false)
	{
		GameObject gameObject = CloneSpritePrefab();
		gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
		gameObject.GetComponent<ex3DSprite2>().color = Foreground;
		gameObject.GetComponent<ex3DSprite2>().backcolor = Background;
		return gameObject;
	}

	public static GameObject CreateSprite(string Path, bool bReusable = false)
	{
		GameObject gameObject = CloneSpritePrefab();
		gameObject.GetComponent<ex3DSprite2>().textureInfo = GetTextureInfo(Path.Replace('\\', '_').Replace('/', '_').ToLower());
		gameObject.GetComponent<ex3DSprite2>().backcolor = new Color(0f, 0f, 0f, 1f);
		return gameObject;
	}
}
