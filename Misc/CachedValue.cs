using Origins.Items.Other.Consumables;
using System;

namespace Origins.Misc {
	public class FrameCachedValue<T> {
		uint lastGameFrameCount = 0;
		readonly Func<T> GetValueFunc;
		T value;
		public FrameCachedValue(Func<T> getValueFunc) {
			GetValueFunc = getValueFunc;
			try {
				value = GetValueFunc();
				lastGameFrameCount = Origins.gameFrameCount;
			} catch (Exception) { }
		}
		public T GetValue() => Value;
		public T Value {
			get {
				if (lastGameFrameCount != Origins.gameFrameCount) {
					lastGameFrameCount = Origins.gameFrameCount;
					value = GetValueFunc();
				}
				return value;
			}
		}
		public static implicit operator FrameCachedValue<T>(Func<T> getValueFunc) => new(getValueFunc);
	}
}
