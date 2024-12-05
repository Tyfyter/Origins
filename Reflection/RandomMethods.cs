using System;
using Terraria.Utilities;
using PegasusLib;
using PegasusLib.Reflection;
using System.Reflection;

namespace Origins.Reflection {
	public static class RandomMethods<T> {
		static FastFieldInfo<WeightedRandom<T>, double> _totalWeight;
		public static FastFieldInfo<WeightedRandom<T>, double> TotalWeight => _totalWeight ??= new("_totalWeight", BindingFlags.Public | BindingFlags.NonPublic);
	}
	public static class RandomMethods {
		public static T Pop<T>(this WeightedRandom<T> rand) {
			if (rand.needsRefresh) {
				rand.CalculateTotalWeight();
			}
			double num = rand.random.NextDouble();
			num *= RandomMethods<T>.TotalWeight.GetValue(rand);
			if (num <= 0) {
				rand.elements.Clear();
				return default;
			}
			for (int i = 0; i < rand.elements.Count; i++) {
				Tuple<T, double> element = rand.elements[i];
				if (num > element.Item2) {
					num -= element.Item2;
					continue;
				}
				rand.elements.RemoveAt(i);
				rand.needsRefresh = true;
				return element.Item1;
			}
			return default;
		}
	}
}
