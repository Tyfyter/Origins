using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Reflection {
	public static class RandomMethods<T> {
		public class Loader : ReflectionLoader {
			public override Type ParentType => typeof(RandomMethods<T>);
		}
		static RandomMethods() {
			new Loader().Load(Origins.instance);
		}
		public static FastFieldInfo<WeightedRandom<T>, double> _totalWeight;
	}
	public static class RandomMethods {
		public static T Pop<T>(this WeightedRandom<T> rand) {
			if (rand.needsRefresh) {
				rand.CalculateTotalWeight();
			}
			double num = rand.random.NextDouble();
			num *= RandomMethods<T>._totalWeight.GetValue(rand);
			if (num <= 0) {
				rand.elements.Clear();
				return default(T);
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
			return default(T);
		}
	}
}
