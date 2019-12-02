using System.Collections.Generic;
using System.Linq;

public static class Extension {

	private static readonly System.Random random = new System.Random();

	public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> sequence) {
		var retArray = sequence.ToArray();

		for (int i = 0; i < retArray.Length - 1; i += 1) {
			int swapIndex = random.Next(i, retArray.Length);
			if (swapIndex != i) {
				var temp = retArray[i];
				retArray[i] = retArray[swapIndex];
				retArray[swapIndex] = temp;
			}
		}

		return retArray;
	}

	public static T RandomOrDefault<T>(this IEnumerable<T> sequence) {
		if (!sequence.Any()) {
			return default(T);
		}

		return sequence.ElementAt(random.Next(sequence.Count()));
	}

}
