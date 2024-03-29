using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.ZoneBuilders.Utility;

public class FloatNoiseMap
{
	public int Width;

	public int Height;

	public float CutoffDepth;

	public float[,] Seeds;

	public float[,] Noise;

	public float[,] Areas;

	public float nAreas;

	public Dictionary<float, List<FloatNoiseMapNode>> AreaNodes;

	public FloatNoiseMap Copy(int x1, int y1, int x2, int y2)
	{
		FloatNoiseMap floatNoiseMap = new FloatNoiseMap();
		floatNoiseMap.Width = x2 - x1 + 1;
		floatNoiseMap.Height = y2 - y1 + 1;
		floatNoiseMap.Seeds = new float[floatNoiseMap.Width, floatNoiseMap.Height];
		floatNoiseMap.Areas = new float[floatNoiseMap.Width, floatNoiseMap.Height];
		if (Noise != null)
		{
			floatNoiseMap.Noise = new float[floatNoiseMap.Width, floatNoiseMap.Height];
			for (int i = x1; i <= x2; i++)
			{
				for (int j = y1; j <= y2; j++)
				{
					floatNoiseMap.Noise[i - x1, j - y1] = Noise[x1, y1];
				}
			}
		}
		return floatNoiseMap;
	}

	public FloatNoiseMap()
	{
	}

	public FloatNoiseMap(int _Width, int _Height, float MaxDepth, int SectorsWide, int SectorsHigh, int SeedsPerSector, float MinSeedDepth, float MaxSeedDepth, float BaseNoise, int FilterPasses, int BorderWidth, float _CutoffDepth, List<FloatNoiseMapNode> ExtraNodes)
	{
		CreateFloatNoiseMap(_Width, _Height, MaxDepth, SectorsWide, SectorsHigh, SeedsPerSector, MinSeedDepth, MaxSeedDepth, BaseNoise, FilterPasses, BorderWidth, _CutoffDepth, ExtraNodes, -1, DoAreas: true);
	}

	public FloatNoiseMap(int _Width, int _Height, float MaxDepth, int SectorsWide, int SectorsHigh, int SeedsPerSector, float MinSeedDepth, float MaxSeedDepth, float BaseNoise, int FilterPasses, int BorderWidth, float _CutoffDepth, List<FloatNoiseMapNode> ExtraNodes, int iFilterBorder)
	{
		CreateFloatNoiseMap(_Width, _Height, MaxDepth, SectorsWide, SectorsHigh, SeedsPerSector, MinSeedDepth, MaxSeedDepth, BaseNoise, FilterPasses, BorderWidth, _CutoffDepth, ExtraNodes, iFilterBorder, DoAreas: true);
	}

	public void FastQuadPlasma(int Width, int Height, int MinStartHeight, int MaxStartHeight, float RoughnessMin, float RoughnessMax)
	{
		if (Width % 2 == 0)
		{
			Width++;
		}
		if (Height % 2 == 0)
		{
			Height++;
		}
		if (Width < Height)
		{
			Height = Width;
		}
		if (Height < Width)
		{
			Height = Width;
		}
		Noise = new float[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Noise[i, j] = float.MaxValue;
			}
		}
		Stack<Box> stack = new Stack<Box>();
		Noise[0, 0] = Stat.Random(MinStartHeight, MaxStartHeight);
		Noise[Width - 1, 0] = Stat.Random(MinStartHeight, MaxStartHeight);
		Noise[0, Height - 1] = Stat.Random(MinStartHeight, MaxStartHeight);
		Noise[Width - 1, Height - 1] = Stat.Random(MinStartHeight, MaxStartHeight);
		stack.Push(new Box(0, 0, Width - 1, Height - 1));
		while (stack.Count > 0)
		{
			Box box = stack.Pop();
			if (box.Width > 1)
			{
				if (Noise[box.MidX, box.y1] == float.MaxValue)
				{
					Noise[box.MidX, box.y1] = (Noise[box.x1, box.y1] + Noise[box.x2, box.y1]) / 2f + Stat.Random(RoughnessMin, RoughnessMax);
				}
				if (Noise[box.MidX, box.y2] == float.MaxValue)
				{
					Noise[box.MidX, box.y2] = (Noise[box.x1, box.y2] + Noise[box.x2, box.y2]) / 2f + Stat.Random(RoughnessMin, RoughnessMax);
				}
				if (Noise[box.x1, box.MidY] == float.MaxValue)
				{
					Noise[box.x1, box.MidY] = (Noise[box.x1, box.y1] + Noise[box.x1, box.y2]) / 2f + Stat.Random(RoughnessMin, RoughnessMax);
				}
				if (Noise[box.x2, box.MidY] == float.MaxValue)
				{
					Noise[box.x2, box.MidY] = (Noise[box.x2, box.y1] + Noise[box.x2, box.y2]) / 2f + Stat.Random(RoughnessMin, RoughnessMax);
				}
				if (Noise[box.MidX, box.MidY] == float.MaxValue)
				{
					Noise[box.MidX, box.MidY] = (Noise[box.x2, box.y1] + Noise[box.x2, box.y2] + Noise[box.x1, box.y1] + Noise[box.x1, box.y2]) / 4f + Stat.Random(RoughnessMin, RoughnessMax);
				}
				if (box.Width > 2)
				{
					stack.Push(new Box(box.x1, box.y1, box.MidX, box.MidY));
					stack.Push(new Box(box.MidX, box.y1, box.x2, box.MidY));
					stack.Push(new Box(box.x1, box.MidY, box.MidX, box.y2));
					stack.Push(new Box(box.MidX, box.MidY, box.x2, box.y2));
				}
			}
		}
	}

	public void CreateFloatNoiseMap(int _Width, int _Height, float MaxDepth, int SectorsWide, int SectorsHigh, int SeedsPerSector, float MinSeedDepth, float MaxSeedDepth, float BaseNoise, int FilterPasses, int BorderWidth, float _CutoffDepth, List<FloatNoiseMapNode> ExtraNodes, int FilterBorder, bool DoAreas)
	{
		if (MinSeedDepth > MaxSeedDepth)
		{
			MaxSeedDepth = MinSeedDepth;
		}
		CutoffDepth = _CutoffDepth;
		Width = _Width;
		Height = _Height;
		Noise = new float[Width, Height];
		Seeds = new float[Width, Height];
		Areas = new float[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Noise[i, j] = 0f;
				Seeds[i, j] = 0f;
			}
		}
		for (int k = BorderWidth + FilterPasses; k < Width - BorderWidth - FilterPasses; k++)
		{
			for (int l = BorderWidth + FilterPasses; l < Height - BorderWidth - FilterPasses; l++)
			{
				Noise[k, l] = Stat.Random(0f, BaseNoise);
				Seeds[k, l] = 0f;
			}
		}
		for (int m = 0; m < SectorsWide; m++)
		{
			for (int n = 0; n < SectorsHigh; n++)
			{
				int num = m * (Width / SectorsWide);
				int num2 = (m + 1) * (Width / SectorsWide);
				int num3 = n * (Height / SectorsHigh);
				int num4 = (n + 1) * (Height / SectorsHigh);
				if (FilterBorder == -1)
				{
					if (num < BorderWidth + FilterPasses)
					{
						num = BorderWidth + FilterPasses;
					}
					if (num3 < BorderWidth + FilterPasses)
					{
						num3 = BorderWidth + FilterPasses;
					}
					if (num2 > Width - 1 - BorderWidth - FilterPasses)
					{
						num2 = Width - 1 - BorderWidth - FilterPasses;
					}
					if (num4 > Height - 1 - BorderWidth - FilterPasses)
					{
						num4 = Height - 1 - BorderWidth - FilterPasses;
					}
				}
				else
				{
					if (num < BorderWidth + FilterBorder)
					{
						num = BorderWidth + FilterBorder;
					}
					if (num3 < BorderWidth + FilterBorder)
					{
						num3 = BorderWidth + FilterBorder;
					}
					if (num2 > Width - 1 - BorderWidth - FilterBorder)
					{
						num2 = Width - 1 - BorderWidth - FilterBorder;
					}
					if (num4 > Height - 1 - BorderWidth - FilterBorder)
					{
						num4 = Height - 1 - BorderWidth - FilterBorder;
					}
				}
				for (float num5 = 0f; num5 < (float)SeedsPerSector; num5 += 1f)
				{
					int num6 = Stat.Random(num, num2);
					int num7 = Stat.Random(num3, num4);
					Noise[num6, num7] = Stat.Random(MinSeedDepth, MaxSeedDepth);
					Seeds[num6, num7] += 1f;
				}
			}
		}
		if (ExtraNodes != null)
		{
			foreach (FloatNoiseMapNode ExtraNode in ExtraNodes)
			{
				if (ExtraNode.x > 0 && ExtraNode.y > 0 && ExtraNode.x < Width - 1 && ExtraNode.y < Height - 1)
				{
					if (ExtraNode.depth == -1f)
					{
						Noise[ExtraNode.x, ExtraNode.y] = MaxSeedDepth;
					}
					else
					{
						Noise[ExtraNode.x, ExtraNode.y] = ExtraNode.depth;
					}
					Seeds[ExtraNode.x, ExtraNode.y] += 1f;
				}
			}
		}
		float[,] array = new float[3, 3]
		{
			{ 1f, 3f, 1f },
			{ 3f, 6f, 3f },
			{ 1f, 3f, 1f }
		};
		for (int num8 = 0; num8 < FilterPasses; num8++)
		{
			float[,] array2 = new float[Width, Height];
			for (int num9 = 0; num9 < Width; num9++)
			{
				for (int num10 = 0; num10 < Height; num10++)
				{
					float num11 = 0f;
					array2[num9, num10] = 0f;
					for (int num12 = 0; num12 < 3; num12++)
					{
						for (int num13 = 0; num13 < 3; num13++)
						{
							if (num9 + (num12 - 1) >= 0 && num9 + (num12 - 1) < Width && num10 + (num13 - 1) >= 0 && num10 + (num13 - 1) < Height)
							{
								array2[num9, num10] += Noise[num9 + (num12 - 1), num10 + (num13 - 1)] * array[num12, num13];
								num11 += array[num12, num13];
							}
						}
					}
					array2[num9, num10] /= num11;
				}
			}
			Noise = array2;
		}
		AreaNodes = new Dictionary<float, List<FloatNoiseMapNode>>();
		if (DoAreas)
		{
			FillAreas();
		}
	}

	private void FillAreas()
	{
		nAreas = 0f;
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Areas[i, j] = -1f;
			}
		}
		for (int k = 0; k < Width; k++)
		{
			for (int l = 0; l < Height; l++)
			{
				if (Noise[k, l] > 1f && Areas[k, l] == -1f)
				{
					FillArea(k, l, nAreas);
					nAreas += 1f;
				}
			}
		}
		for (int m = 0; (float)m < nAreas; m++)
		{
			AreaNodes[m] = new List<FloatNoiseMapNode>();
		}
		for (int n = 0; n < Height; n++)
		{
			for (int num = 0; num < Width; num++)
			{
				if (Areas[num, n] != -1f)
				{
					AreaNodes[Areas[num, n]].Add(new FloatNoiseMapNode(num, n, Noise[num, n]));
				}
			}
		}
	}

	private void FillArea(int x, int y, float a)
	{
		if (x >= 0 && x < Width && y >= 0 && y < Height && !(Noise[x, y] <= CutoffDepth) && Areas[x, y] == -1f)
		{
			Areas[x, y] = a;
			FillArea(x - 1, y - 1, a);
			FillArea(x, y - 1, a);
			FillArea(x + 1, y - 1, a);
			FillArea(x - 1, y, a);
			FillArea(x + 1, y, a);
			FillArea(x - 1, y + 1, a);
			FillArea(x, y + 1, a);
			FillArea(x + 1, y + 1, a);
		}
	}

	public void Draw()
	{
		for (int i = 0; i < Noise.GetUpperBound(0) + 1; i++)
		{
			for (int j = 0; j < Noise.GetUpperBound(1) + 1; j++)
			{
				if (Noise[i, j] > CutoffDepth)
				{
					Popup._ScreenBuffer.Goto(i, j);
					Popup._ScreenBuffer.Write(49);
				}
				else
				{
					Popup._ScreenBuffer.Goto(i, j);
					Popup._ScreenBuffer.Write(32);
				}
				if (Areas[i, j] != -1f)
				{
					Popup._ScreenBuffer.Goto(i, j);
					Popup._ScreenBuffer.Write((short)(float)(int)Areas[i, j].ToString()[0]);
				}
				else
				{
					Popup._ScreenBuffer.Goto(i, j);
					Popup._ScreenBuffer.Write(32);
				}
				if (Seeds[i, j] > 0f)
				{
					Popup._ScreenBuffer.Goto(i, j);
					Popup._ScreenBuffer.Write("&W" + (char)(48f + Seeds[i, j]));
				}
			}
		}
		Popup._TextConsole.DrawBuffer(Popup._ScreenBuffer);
		Keyboard.getch();
	}
}
