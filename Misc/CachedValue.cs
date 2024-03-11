using System;

namespace Origins.Misc {
	public class FrameCachedValue<T> {
		uint lastGameFrameCount = 0;
		Func<T> GetValueFunc;
		T value;
		public FrameCachedValue(Func<T> getValueFunc) {
			GetValueFunc = getValueFunc;
			lastGameFrameCount = Origins.gameFrameCount;
			value = GetValueFunc();
		}
		public T GetValue() {
			if (lastGameFrameCount != Origins.gameFrameCount) {
				lastGameFrameCount = Origins.gameFrameCount;
				value = GetValueFunc();
			}
			return value;
		}
	}
}
