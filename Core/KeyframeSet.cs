using System.Collections;
using System.Collections.Generic;
using Terraria;

namespace Origins.Core;
public class KeyframeSet<T>() : IEnumerable<KeyframeSet<T>.Keyframe> {
	public T start;
	public List<Keyframe> keyframes = [];
	public KeyframeSet(T startValue) : this() => start = startValue;
	public T GetValue(float time) {
		float prevTime = 0;
		T prevValue = start;
		for (int i = 0; i < keyframes.Count; i++) {
			if (time == prevTime) return prevValue;
			Keyframe keyframe = keyframes[i];
			if (time < keyframe.Time) return keyframe.GetValue(time, prevTime, prevValue);
			prevTime = keyframe.Time;
			prevValue = keyframe.Target;
		}
		return prevValue;
	}
	public void Add(Keyframe keyframe) => keyframes.Add(keyframe);
	IEnumerator<Keyframe> IEnumerable<Keyframe>.GetEnumerator() => keyframes.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)keyframes).GetEnumerator();
	public record struct Keyframe(float Time, T Target, Interpolation Interpolation) {
		public readonly T GetValue(float time, float prevTime, T prevValue) => Interpolation(prevValue, Target, Utils.GetLerpValue(prevTime, Time, time));
	}
	public delegate T Interpolation(T prevValue, T nextValue, float progress);
}
public static class KeyframeTypes {
	public static KeyframeSet<T>.Interpolation WithExponent<T>(this KeyframeSet<T>.Interpolation self, float exponent) =>
		(prevValue, nextValue, progress) => self(prevValue, nextValue, float.Pow(progress, exponent));
	public static KeyframeSet<T>.Keyframe Step<T>(int onFrame, T value) => new(onFrame, value, DoStep);
	static T DoStep<T>(T prevValue, T nextValue, float progress) => prevValue;
}
