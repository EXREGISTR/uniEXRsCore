using System;
using System.Collections.Generic;

namespace EXRCore.Utils {
	public static class ListsExtenssions {
		private static readonly Random random = new();
		
		public static T GetRandomItem<T>(this IList<T> collection) => GetRandomItem(collection, random);
		public static T GetRandomItem<T>(this IList<T> collection, Random random) {
			return collection[random.Next(0, collection.Count - 1)];
		}
	}
}