using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace Origins.Core;
public class KeyframeSet<T>() : IKeyframeSet, IEnumerable<KeyframeSet<T>.Keyframe> {
	public T start;
	public List<Keyframe> keyframes = [];
	public float Duration => keyframes.Count == 0 ? 0 : keyframes[^1].Time;
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
	public void Insert(int index, Keyframe keyframe) => keyframes.Insert(index, keyframe);
	public void InsertAtFrame(float time) {
		for (int i = 0; i < keyframes.Count; i++) {
			if (keyframes[i].Time >= time) {
				if (keyframes[i].Time > time) {
					Insert(i, new(
						time,
						GetValue(time),
						keyframes[i].Interpolation.Clone()
					));
				}
				return;
			}
		}
	}
	public void GetAtIndex(int index) => keyframes.RemoveAt(index);
	public void RemoveAtIndex(int index) => keyframes.RemoveAt(index);
	IEnumerator<Keyframe> IEnumerable<Keyframe>.GetEnumerator() => keyframes.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)keyframes).GetEnumerator();

	public void DrawEditorUI(SpriteBatch spriteBatch, KeyframeAnimation animation, bool isSelected, float currentTime) {
		if (isSelected && Main.mouseRight && Main.mouseRightRelease && animation.CurrentTimeline.Contains(Main.MouseScreen)) {
			float time = animation.ScreenPosToTimeline(Main.MouseScreen.X);
			if (!animation.usesSubframes) time = MathF.Round(time);
			InsertAtFrame(time);
		}
		Span<Keyframe> keyframes = CollectionsMarshal.AsSpan(this.keyframes);
		for (int i = 0; i < keyframes.Length; i++) {
			ref Keyframe keyframe = ref keyframes[i];
			Vector2 pos = animation.timelinePos with { X = animation.TimelineToScreenPos(keyframe.Time) };
			if (isSelected && keyframe.Time == currentTime) {
				DrawDiamond(
					spriteBatch,
					pos,
					14,
					Color.Orange
				);
				DrawDiamond(
					spriteBatch,
					pos,
					12,
					Color.Black
				);
				IInterpolation inter = keyframe.Interpolation;
				Vector2 iPos = new((Main.screenWidth + KeyframeAnimation.TimelineWidth) * 0.5f + 8, 4);
				TextSnippet[] snippets = new TextSnippet[1];
				snippets[0] = new();
				int depth = -1;
				int removeDepth = -1;
				while (inter is not null) {
					depth++;
					snippets[0].Text = inter.ToString();
					ChatManager.DrawColorCodedStringWithShadow(
						spriteBatch,
						FontAssets.ItemStack.Value,
						snippets,
						iPos,
						0,
						Vector2.Zero,
						Vector2.One,
						out int hoveredSnippet
					);
					if (hoveredSnippet != -1 && Main.mouseRight && Main.mouseRightRelease && inter is IStackedInterpolation stacked) {
						removeDepth = depth;
					}
					iPos.Y += 16;
					inter = (inter as IStackedInterpolation)?.Base;
				}
			}
			DrawDiamond(
				spriteBatch,
				pos,
				8,
				Color.Orange
			);
		}
		static void DrawDiamond(SpriteBatch spriteBatch, Vector2 position, int size, Color color) {
			Rectangle frame = new(0, 0, size, size);
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				position,
				frame,
				color,
				MathHelper.PiOver4,
				frame.Size() * 0.5f,
				1,
				SpriteEffects.None,
			0);
		}
	}

	public record struct Keyframe(float Time, T Target, IInterpolation Interpolation) {
		public readonly T GetValue(float time, float prevTime, T prevValue) => Interpolation.Interpolate(prevValue, Target, Utils.GetLerpValue(prevTime, Time, time));
	}
	public interface IInterpolation {
		public IInterpolation Clone() => this;
		public T Interpolate(T prevValue, T nextValue, float progress);
	}
	public interface IStackedInterpolation : IInterpolation {
		public IInterpolation Base { get; }
		public float ModifyProgress(float progress);
		T IInterpolation.Interpolate(T prevValue, T nextValue, float progress) => Base.Interpolate(prevValue, nextValue, ModifyProgress(progress));
	}
	public delegate void GizmoDrawer(SpriteBatch spriteBatch, ref T value);
	public static GizmoDrawer DrawGizmo;
}
public interface IKeyframeSet {
	public void DrawEditorUI(SpriteBatch spriteBatch, KeyframeAnimation animation, bool isSelected, float currentTime);
	public float Duration { get; }
}
public static class KeyframeTypes {
	public static ExponentialInterpolation<T> WithExponent<T>(this KeyframeSet<T>.IInterpolation self, float exponent) =>
		new(self) { exponent = exponent };
	public static KeyframeSet<T>.Keyframe Step<T>(int onFrame, T value) => new(onFrame, value, new StepInterpolation<T>());
	public readonly struct StepInterpolation<T> : KeyframeSet<T>.IInterpolation {
		public readonly T Interpolate(T prevValue, T nextValue, float progress) => prevValue;
		public readonly override string ToString() => "Stepped";
	}
	public class ExponentialInterpolation<T>(KeyframeSet<T>.IInterpolation @base) : KeyframeSet<T>.IStackedInterpolation {
		public KeyframeSet<T>.IInterpolation Base { get; set; } = @base;
		public float exponent = 2;
		public float ModifyProgress(float progress) => float.Pow(progress, exponent);
		public KeyframeSet<T>.IInterpolation Clone() => new ExponentialInterpolation<T>(Base.Clone()) { exponent = exponent };
		public override string ToString() => $"Exponential({exponent})";
	}
	public record struct SinEaseInInterpolation<T>(KeyframeSet<T>.IInterpolation Base) : KeyframeSet<T>.IStackedInterpolation {
		public readonly float ModifyProgress(float progress) => 1 - MathF.Cos(progress * MathHelper.PiOver2);
		public readonly override string ToString() => "Sinusoidal Ease In";
	}
	public record struct SinEaseOutInterpolation<T>(KeyframeSet<T>.IInterpolation Base) : KeyframeSet<T>.IStackedInterpolation {
		public readonly float ModifyProgress(float progress) => MathF.Sin(progress * MathHelper.PiOver2);
		public readonly override string ToString() => "Sinusoidal Ease Out";
	}
	public record struct SinEaseBothInterpolation<T>(KeyframeSet<T>.IInterpolation Base) : KeyframeSet<T>.IStackedInterpolation {
		public readonly float ModifyProgress(float progress) => (MathF.Cos(progress * MathHelper.PiOver2) - 1) * -0.5f;
		public readonly override string ToString() => "Sinusoidal Ease In/Out";
	}
}
