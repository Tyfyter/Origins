using CalamityMod.Items.Accessories;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
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
				InterpolationStack stack = keyframe.Interpolation;
				Vector2 iPos = new((Main.screenWidth + KeyframeAnimation.TimelineWidth) * 0.5f + 8, 4);
				TextSnippet[] snippets = [
					new(),
					new(" "),
					new("X", Color.Red)
				];
				int moveSelectedTo = -1;
				for (int j = stack.Count - 1; j >= 0; j--) {
					snippets[0].Text = stack[j].ToString();
					snippets[0].Color = (animation.modifyingInterpolation == j) ? Color.Goldenrod : Color.White;
					ChatManager.DrawColorCodedStringWithShadow(
						spriteBatch,
						FontAssets.ItemStack.Value,
						snippets,
						iPos,
						0,
						Vector2.Zero,
						Vector2.One,
						out int hoveredModifier
					);
					ShowSwap(spriteBatch, animation, j + 1, iPos, ref moveSelectedTo);
					if (Main.mouseLeft && Main.mouseLeftRelease) {
						switch (hoveredModifier) {
							case 0:
							animation.modifyingInterpolation = j;
							animation.dragging = true;
							break;
							case 2:
							stack.RemoveAt(j);
							break;
						}
					}
					iPos.Y += 16;
				}
				ShowSwap(spriteBatch, animation, 0, iPos, ref moveSelectedTo);
				Array.Resize(ref snippets, 1);
				snippets[0].Text = stack.BaseInterpolation.ToString();
				snippets[0].Color = (animation.modifyingInterpolation == -2) ? Color.Goldenrod : Color.White;
				ChatManager.DrawColorCodedStringWithShadow(
					spriteBatch,
					FontAssets.ItemStack.Value,
					snippets,
					iPos,
					0,
					Vector2.Zero,
					Vector2.One,
					out int hoveredBase
				);
				if (moveSelectedTo != -1) {
					IInterpolationModifier toMove = stack[animation.modifyingInterpolation];
					stack.RemoveAt(animation.modifyingInterpolation);
					stack.Insert(moveSelectedTo, toMove);
					animation.modifyingInterpolation = moveSelectedTo;
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
		static void ShowSwap(SpriteBatch spriteBatch, KeyframeAnimation animation, int index, Vector2 iPos, ref int moveTo) {
			if (animation.dragging && animation.modifyingInterpolation != index && animation.modifyingInterpolation != index - 1 && Main.mouseY > iPos.Y - 8 && Main.mouseY < iPos.Y + 8) {
				spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					iPos,
					new(0, 0, 100, 2),
					Color.White
				);
				if (!Main.mouseLeft) {
					moveTo = index;
					if (animation.modifyingInterpolation < index) moveTo--;
					animation.dragging = false;
				}
			}
		}
	}

	public record struct Keyframe(float Time, T Target, InterpolationStack Interpolation) {
		public Keyframe(float time, T target, IInterpolation interpolation) : this(time, target, new InterpolationStack() { BaseInterpolation = interpolation }) { }
		public readonly T GetValue(float time, float prevTime, T prevValue) => Interpolation.Interpolate(prevValue, Target, Utils.GetLerpValue(prevTime, Time, time));
	}
	public class InterpolationStack : List<IInterpolationModifier> {
		IInterpolation baseInterpolation = CreateLinear?.Invoke();
		public ref IInterpolation BaseInterpolation {
			get {
				baseInterpolation ??= CreateLinear?.Invoke();
				return ref baseInterpolation;
			}
		}
		public InterpolationStack() : base() { }
		InterpolationStack(IEnumerable<IInterpolationModifier> collection) : base(collection) { }
		public T Interpolate(T prevValue, T nextValue, float progress) {
			Span<IInterpolationModifier> modifiers = CollectionsMarshal.AsSpan(this);
			for (int i = 0; i < modifiers.Length; i++) progress = modifiers[i].ModifyProgress(progress);
			return BaseInterpolation.Interpolate(prevValue, nextValue, progress);
		}
		public InterpolationStack Clone() => new(this) {
			baseInterpolation = BaseInterpolation.Clone()
		};
	}
	public interface IInterpolation {
		public T Interpolate(T prevValue, T nextValue, float progress);
		public IInterpolation Clone() => this;
	}
	public delegate void GizmoDrawer(SpriteBatch spriteBatch, ref T value);
	public static GizmoDrawer DrawGizmo;
	public static Func<IInterpolation> CreateLinear;
	public interface ITypeHandler : IAutoload<ITypeHandler.Loader> {
		public abstract static void DrawGizmo(SpriteBatch spriteBatch, ref T value);
		public abstract static IInterpolation Linear { get; }
		class Loader : IAutoloader {
			static void IAutoloader.Autoload(Mod mod, Type type) {
				KeyframeSet<T>.DrawGizmo = type.GetMethod(nameof(DrawGizmo)).CreateDelegate<GizmoDrawer>();
				CreateLinear = type.GetProperty(nameof(Linear)).GetMethod.CreateDelegate<Func<IInterpolation>>();
			}
		}
	}
}
public interface IKeyframeSet {
	public void DrawEditorUI(SpriteBatch spriteBatch, KeyframeAnimation animation, bool isSelected, float currentTime);
	public float Duration { get; }
}
public interface IInterpolationModifier : IMustBeStruct {
	public float ModifyProgress(float progress);
}
public static class KeyframeTypes {
	public static KeyframeSet<T>.Keyframe Step<T>(int onFrame, T value) => new(onFrame, value, new StepInterpolation<T>());
	readonly struct StepInterpolation<T> : KeyframeSet<T>.IInterpolation {
		public readonly T Interpolate(T prevValue, T nextValue, float progress) => prevValue;
		public readonly override string ToString() => "Stepped";
	}
}
public static class KeyframeModifiers {
	public record struct Exponent(float exponent = 2) : IInterpolationModifier {
		public float exponent = exponent;
		public readonly float ModifyProgress(float progress) => float.Pow(progress, exponent);
		public readonly override string ToString() => $"Exponential({exponent})";
	}
	public record struct SinEaseIn : IInterpolationModifier {
		public readonly float ModifyProgress(float progress) => 1 - MathF.Cos(progress * MathHelper.PiOver2);
		public readonly override string ToString() => "Sinusoidal Ease In";
	}
	public record struct SinEaseOut : IInterpolationModifier {
		public readonly float ModifyProgress(float progress) => MathF.Sin(progress * MathHelper.PiOver2);
		public readonly override string ToString() => "Sinusoidal Ease Out";
	}
	public record struct SinEaseBoth : IInterpolationModifier {
		public readonly float ModifyProgress(float progress) => (MathF.Cos(progress * MathHelper.PiOver2) - 1) * -0.5f;
		public readonly override string ToString() => "Sinusoidal Ease In/Out";
	}
}
