namespace Genkit;

public static class Hash
{
	public static int String(string read)
	{
		ulong num = 3074457345618258791uL;
		for (int i = 0; i < read.Length; i++)
		{
			num += read[i];
			num *= 3074457345618258799L;
		}
		return (int)(num >> 32);
	}
}
