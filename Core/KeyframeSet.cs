using Microsoft.Extensions.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using static Origins.Core.KeyframeTypes;

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
	public void InsertAtFrame(float time, InterpolationStack Interpolation) {
		for (int i = 0; i < keyframes.Count; i++) {
			if (keyframes[i].Time >= time) {
				if (keyframes[i].Time > time) {
					Insert(i, new(
						time,
						GetValue(time),
						Interpolation
					));
				}
				return;
			}
		}
		Insert(keyframes.Count, new(
			time,
			GetValue(time),
			Interpolation
		));
	}
	public void GetAtIndex(int index) => keyframes.RemoveAt(index);
	public void RemoveAtIndex(int index) => keyframes.RemoveAt(index);
	IEnumerator<Keyframe> IEnumerable<Keyframe>.GetEnumerator() => keyframes.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)keyframes).GetEnumerator();
	public string Export() {
		StringBuilder builder = new("new(");
		builder.Append(ExportValue(start));
		builder.Append(')');
		builder.Append(" {\n");
		for (int i = 0; i < keyframes.Count; i++) {
			builder.Append("\t\t");
			builder.Append(keyframes[i].Export());
			if (i + 1 < keyframes.Count) builder.Append(", \n");
			else builder.Append('\n');
		}
		builder.Append("\t}");
		return builder.ToString();
	}
	public string ExportType() => $"KeyframeSet<{typeof(T).Name}>";
	public void DrawEditorUI(SpriteBatch spriteBatch, KeyframeAnimation animation, bool isSelected, float currentTime) {
		if (isSelected && Main.mouseRight && Main.mouseRightRelease && animation.CurrentTimeline.Contains(Main.MouseScreen)) {
			float time = animation.ScreenPosToTimeline(Main.MouseScreen.X);
			if (!animation.usesSubframes) time = MathF.Round(time);
			InsertAtFrame(time);
		}
		Span<Keyframe> keyframes = CollectionsMarshal.AsSpan(this.keyframes);
		bool showCreationSidebar = isSelected;
		for (int i = 0; i < keyframes.Length; i++) {
			ref Keyframe keyframe = ref keyframes[i];
			Vector2 pos = animation.timelinePos with { X = animation.TimelineToScreenPos(keyframe.Time) };
			if (isSelected && keyframe.Time == currentTime) {
				showCreationSidebar = false;
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
				DrawSidebar(spriteBatch, animation, keyframe);
				int g = 0;
				DrawGizmo?.Invoke(spriteBatch, ref keyframe.Target, ref animation.draggingGizmo, ref g, animation.gizmoBasePosition);
			}
			DrawDiamond(
				spriteBatch,
				pos,
				8,
				Color.Orange
			);
		}
		if (showCreationSidebar) DrawKeyframeCreationSidebar(spriteBatch, animation, currentTime);
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
		static void ShowSwap(SpriteBatch spriteBatch, KeyframeAnimation animation, int index, Vector2 iPos, ref int moveTo, int width) {
			if (animation.draggingInterpolation && animation.modifyingInterpolation != index && animation.modifyingInterpolation != index - 1 && Main.mouseY > iPos.Y - 8 && Main.mouseY < iPos.Y + 8) {
				spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					iPos,
					new(0, 0, width, 2),
					Color.White
				);
				if (!Main.mouseLeft) {
					moveTo = index;
					if (animation.modifyingInterpolation < index) moveTo--;
					animation.draggingInterpolation = false;
				}
			}
		}

		static void DrawSidebar(SpriteBatch spriteBatch, KeyframeAnimation animation, Keyframe keyframe) {
			InterpolationStack stack = keyframe.Interpolation;
			Vector2 iPos = new((Main.screenWidth + KeyframeAnimation.TimelineWidth) * 0.5f + 8, 4);
			TextSnippet[] snippets = [
				new(),
				new(" "),
				new("X", Color.Red)
			];
			float width = -1;
			int moveSelectedTo = -1;
			Max(ref width, FontAssets.ItemStack.Value.MeasureString(stack.BaseInterpolation.ToString()).X);
			if (!stack.BaseInterpolation.IsModifiable) {
				spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					iPos - Vector2.One * 2,
					new(0, 0, (int)width + 4, (stack.Count + 1) * 16 + 4),
					new Color(90, 90, 105)
				);
				goto skipModifiers;
			}
			for (int i = stack.Count - 1; i >= 0; i--) Max(ref width, FontAssets.ItemStack.Value.MeasureString(stack[i].ToString()).X);
			width += 18;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				iPos - Vector2.One * 2,
				new(0, 0, (int)width + 4, (stack.Count + 1) * 16 + 4),
				new Color(90, 90, 105)
			);
			for (int i = stack.Count - 1; i >= 0; i--) {
				snippets[0].Text = stack[i].ToString();
				snippets[0].Color = (animation.modifyingInterpolation == i) ? Color.Goldenrod : Color.White;
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
				ShowSwap(spriteBatch, animation, i + 1, iPos, ref moveSelectedTo, (int)width);
				if (Main.mouseLeft && Main.mouseLeftRelease && hoveredModifier != -1) {
					Main.mouseLeftRelease = false;
					switch (hoveredModifier) {
						case 0:
						animation.modifyingInterpolation = i.OrXIf(animation.modifyingInterpolation, -1);
						animation.draggingInterpolation = true;
						break;
						case 2:
						stack.RemoveAt(i);
						break;
					}
				}
				iPos.Y += 16;
			}
			ShowSwap(spriteBatch, animation, 0, iPos, ref moveSelectedTo, (int)width);
			skipModifiers:
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
			if (Main.mouseLeft && Main.mouseLeftRelease && hoveredBase != -1) animation.modifyingInterpolation = -2;
			width = -1;
			iPos.Y += 16 + 8;
			snippets[0].Color = Color.White;
			if (animation.modifyingInterpolation == -2) {
				width = FontAssets.ItemStack.Value.MeasureString(CreateLinear is not null ? "Interpolated" : "Stepped").X;
				spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					iPos + new Vector2(-2, -2),
					new(0, 0, (int)width + 4, (1 + (CreateLinear is not null).ToInt()) * 16 + 4),
					new Color(90, 90, 105)
				);
				hoveredBase = -1;
				if (CreateLinear is not null) {
					snippets[0].Text = "Interpolated";
					ChatManager.DrawColorCodedStringWithShadow(
						spriteBatch,
						FontAssets.ItemStack.Value,
						snippets,
						iPos,
						0,
						Vector2.Zero,
						Vector2.One,
						out hoveredBase
					);
					iPos.Y += 16;
				}
				snippets[0].Text = "Stepped";
				ChatManager.DrawColorCodedStringWithShadow(
					spriteBatch,
					FontAssets.ItemStack.Value,
					snippets,
					iPos,
					0,
					Vector2.Zero,
					Vector2.One,
					out int hoveredStep
				);
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					if (hoveredBase != -1) stack.BaseInterpolation = CreateLinear();
					else if (hoveredStep != -1) stack.BaseInterpolation = new KeyframeTypes.StepInterpolation<T>();
				}
			} else if (stack.BaseInterpolation.IsModifiable) {
				for (int i = KeyframeModifiers.modifierTypes.Count - 1; i >= 0; i--) Max(ref width, FontAssets.ItemStack.Value.MeasureString(KeyframeModifiers.modifierTypes[i].name).X);
				spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					iPos + new Vector2(-2, -2),
					new(0, 0, (int)width + 4, KeyframeModifiers.modifierTypes.Count * 16 + 4),
					new Color(90, 90, 105)
				);
				for (int i = KeyframeModifiers.modifierTypes.Count - 1; i >= 0; i--) {
					snippets[0].Text = KeyframeModifiers.modifierTypes[i].name;
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
					if (Main.mouseLeft && Main.mouseLeftRelease && hoveredModifier != -1) {
						if (animation.modifyingInterpolation == -1) stack.Add(KeyframeModifiers.modifierTypes[i].Item2());
						else stack[animation.modifyingInterpolation] = KeyframeModifiers.modifierTypes[i].Item2();
					}
					iPos.Y += 16;
				}
			}
			if (stack.BaseInterpolation is not StepInterpolation<T>) {
				iPos.Y += 8;
				spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					iPos,
					new(0, 0, 150, 150),
					Color.Black
				);
				Rectangle fram = new(0, 0, 1, 150);
				Func<float, float> modifyProgress = animation.modifyingInterpolation > 0 ? stack[animation.modifyingInterpolation].ModifyProgress : stack.ModifyProgress;
				for (int i = 0; i < 150; i++) {
					fram.Height = (int)(150 - stack.ModifyProgress(i / 150f) * 150);
					spriteBatch.Draw(
						TextureAssets.MagicPixel.Value,
						iPos + new Vector2(i, 0),
						fram,
						new Color(90, 90, 105)
					);
				}
				iPos.Y += 150 + 4;
			}
			if (animation.modifyingInterpolation > 0) {

			}
		}
	}
	void DrawKeyframeCreationSidebar(SpriteBatch spriteBatch, KeyframeAnimation animation, float time) {
		Vector2 iPos = new((Main.screenWidth + KeyframeAnimation.TimelineWidth) * 0.5f + 8, 4);
		TextSnippet[] snippets = [new()];
		snippets[0].Color = Color.White;
		float width = FontAssets.ItemStack.Value.MeasureString((CreateLinear is not null ? "Interpolated" : "Stepped")).X;
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			iPos + new Vector2(-2, -2),
			new(0, 0, (int)width + 4, (2 + (CreateLinear is not null).ToInt()) * 16 + 4),
			new Color(90, 90, 105)
		);
		snippets[0].Text = "Insert:";
		ChatManager.DrawColorCodedStringWithShadow(
			spriteBatch,
			FontAssets.ItemStack.Value,
			snippets,
			iPos,
			0,
			Vector2.Zero,
			Vector2.One,
			out _
		);
		iPos.Y += 16;
		int hoveredBase = -1;
		if (CreateLinear is not null) {
			snippets[0].Text = "Interpolated";
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.ItemStack.Value,
				snippets,
				iPos,
				0,
				Vector2.Zero,
				Vector2.One,
				out hoveredBase
			);
			iPos.Y += 16;
		}
		snippets[0].Text = "Stepped";
		ChatManager.DrawColorCodedStringWithShadow(
			spriteBatch,
			FontAssets.ItemStack.Value,
			snippets,
			iPos,
			0,
			Vector2.Zero,
			Vector2.One,
			out int hoveredStep
		);
		if (Main.mouseLeft && Main.mouseLeftRelease) {
			if (hoveredBase != -1) InsertAtFrame(time, new() { BaseInterpolation = CreateLinear() });
			else if (hoveredStep != -1) InsertAtFrame(time, new() { BaseInterpolation = new StepInterpolation<T>() });
		}
	}

	public record struct Keyframe(float Time, T Target, InterpolationStack Interpolation) {
		public T Target = Target;
		public Keyframe(float time, T target, IInterpolation interpolation) : this(time, target, new InterpolationStack() { BaseInterpolation = interpolation }) { }
		public readonly T GetValue(float time, float prevTime, T prevValue) => Interpolation.Interpolate(prevValue, Target, Utils.GetLerpValue(prevTime, Time, time));
		public readonly string Export() {
			StringBuilder builder = new("new(");
			builder.Append(Time);
			builder.Append(Time == (int)Time ? ", " : "f, ");
			builder.Append(ExportValue(Target));
			builder.Append(", ");
			builder.Append(Interpolation.Export());
			builder.Append(')');
			return builder.ToString();
		}
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
		public float ModifyProgress(float progress) {
			Span<IInterpolationModifier> modifiers = CollectionsMarshal.AsSpan(this);
			for (int i = 0; i < modifiers.Length; i++) progress = modifiers[i].ModifyProgress(progress);
			return progress;
		}
		public T Interpolate(T prevValue, T nextValue, float progress) =>
			BaseInterpolation.Interpolate(prevValue, nextValue, ModifyProgress(progress));
		public InterpolationStack Clone() => new(this) {
			baseInterpolation = BaseInterpolation.Clone()
		};
		public override string ToString() => baseInterpolation is StepInterpolation<T> ?
			$"new StepInterpolation<{typeof(T).Name}>()" :
			$"[{string.Join(", ", this.Select(i => i.Export()))}]";
		public string Export() => ToString();
	}
	public interface IInterpolation {
		public bool IsModifiable => true;
		public T Interpolate(T prevValue, T nextValue, float progress);
		public IInterpolation Clone() => this;
	}
	public delegate void GizmoDrawer(SpriteBatch spriteBatch, ref T value, ref int draggingGizmo, ref int gizmoIndex, Vector2 offset);
	public static GizmoDrawer DrawGizmo;
	public static Func<IInterpolation> CreateLinear;
	public static Func<T, string> ExportValue;
	public interface ITypeHandler : IAutoload<ITypeHandler.Loader> {
		public abstract static void DrawGizmo(SpriteBatch spriteBatch, ref T value, ref int draggingGizmo, ref int gizmoIndex, Vector2 offset);
		public abstract static IInterpolation Linear { get; }
		public abstract static string Export(T value);
		class Loader : IAutoloader {
			static readonly MethodInfo getDrawer = typeof(Loader).GetMethod("GetDrawer");
			static readonly MethodInfo getExporter = typeof(Loader).GetMethod("GetExporter");
			static void IAutoloader.Autoload(Mod mod, Type type) {
				KeyframeSet<T>.DrawGizmo = (GizmoDrawer)getDrawer.MakeGenericMethod(type).Invoke(null, []);
				ExportValue = (Func<T, string>)getExporter.MakeGenericMethod(type).Invoke(null, []);
				CreateLinear = type.GetInterfaceProperty(nameof(Linear), false).GetMethod.CreateDelegate<Func<IInterpolation>>();
			}
			public static GizmoDrawer GetDrawer<THandler>() where THandler : ITypeHandler => THandler.DrawGizmo;
			public static Func<T, string> GetExporter<THandler>() where THandler : ITypeHandler => THandler.Export;
		}
	}
}
public interface IKeyframeSet {
	public void DrawEditorUI(SpriteBatch spriteBatch, KeyframeAnimation animation, bool isSelected, float currentTime);
	public float Duration { get; }
	public string Export();
	public string ExportType();
}
public interface IInterpolationModifier : IMustBeStruct, IAutoload<IInterpolationModifier.Loader> {
	public float ModifyProgress(float progress);
	public string Export() => $"new {GetType().Name}()";
	class Loader : IAutoloader {
		static void IAutoloader.Autoload(Mod mod, Type type) {
			Func<IInterpolationModifier> func = CreateDefault(type);
			KeyframeModifiers.modifierTypes.Add((func().ToString(), func));
		}

		static Func<IInterpolationModifier> CreateDefault(Type type) {
			DynamicMethod method = new($"default({type})", typeof(IInterpolationModifier), [], true);
			ILGenerator gen = method.GetILGenerator();
			gen.DeclareLocal(type);

			gen.Emit(OpCodes.Ldloca_S, 0);
			gen.Emit(OpCodes.Initobj, type);
			gen.Emit(OpCodes.Ldloc_0);
			gen.Emit(OpCodes.Box, type);
			gen.Emit(OpCodes.Ret);

			return method.CreateDelegate<Func<IInterpolationModifier>>();
		}
	}
}
public static class KeyframeTypes {
	public static KeyframeSet<T>.Keyframe Step<T>(int onFrame, T value) => new(onFrame, value, new StepInterpolation<T>());
	public readonly struct StepInterpolation<T> : KeyframeSet<T>.IInterpolation {
		public bool IsModifiable => false;
		public readonly T Interpolate(T prevValue, T nextValue, float progress) => prevValue;
		public readonly override string ToString() => "Stepped";
	}
	public readonly struct FloatInterpolation : KeyframeSet<float>.IInterpolation, KeyframeSet<float>.ITypeHandler {
		static KeyframeSet<float>.IInterpolation KeyframeSet<float>.ITypeHandler.Linear => new FloatInterpolation();
		public readonly float Interpolate(float prevValue, float nextValue, float progress) => float.Lerp(prevValue, nextValue, progress);
		public static string Export(float value) => $"{value}f";
		static void KeyframeSet<float>.ITypeHandler.DrawGizmo(SpriteBatch spriteBatch, ref float value, ref int draggingGizmo, ref int gizmoIndex, Vector2 offset) {
			throw new NotImplementedException();
		}
	}
	public readonly struct Vec2Interpolation : KeyframeSet<Vector2>.IInterpolation, KeyframeSet<Vector2>.ITypeHandler {
		static KeyframeSet<Vector2>.IInterpolation KeyframeSet<Vector2>.ITypeHandler.Linear => new Vec2Interpolation();
		public readonly Vector2 Interpolate(Vector2 prevValue, Vector2 nextValue, float progress) => Vector2.Lerp(prevValue, nextValue, progress);
		static Vector2 oldMousePos;
		public static void DrawGizmo(SpriteBatch spriteBatch, ref Vector2 value, ref int draggingGizmo, ref int gizmoIndex, Vector2 offset) {
			Rectangle rect = new Rectangle(0, 0, 16, 16).Recentered(value + offset);
			DrawAALine(rect.TopLeft(), default, rect.Width, 2);
			DrawAALine(rect.TopLeft(), default, 2, rect.Height);
			DrawAALine(rect.BottomRight(), Vector2.One, rect.Width, 2);
			DrawAALine(rect.BottomRight(), Vector2.One, 2, rect.Height);
			if (draggingGizmo == gizmoIndex) value += Main.MouseScreen - oldMousePos;
			if (Main.mouseLeft && Main.mouseLeftRelease && rect.Contains(Main.MouseScreen)) draggingGizmo = gizmoIndex;
			oldMousePos = Main.MouseScreen;
			static void DrawAALine(Vector2 pos, Vector2 origin, int width, int height) {
				Main.spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					pos,
					new Rectangle(0, 0, 1, 1),
					Color.White,
					0,
					origin,
					new Vector2(width, height),
					0,
				0);
			}
			gizmoIndex++;
		}
		public static string Export(Vector2 value) => value == Vector2.Zero ? "Vector2.Zero" : $"new({value.X}f, {value.Y}f)";
	}
	static readonly Polygon rotationHandle = new(
		new(-1, -1),
		new(1, -1),
		new(1, 1),
		new(-1, 1)
	);
	static float rotationHandleOffset;
	public static void DrawRotationGizmo(SpriteBatch spriteBatch, ref float value, ref int draggingGizmo, ref int gizmoIndex, Vector2 pos, float size) {
		pos = pos.Floor();
		Vector2 end = (pos + (value - MathHelper.PiOver2).ToRotationVector2() * size).Floor();
		spriteBatch.DrawLine(Color.White, pos + Main.screenPosition, end + Main.screenPosition);

		Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
		Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
		Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
		Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		rotationHandle.ResetPositions().Scale(4).Translate(end).Draw();
		if (draggingGizmo == gizmoIndex) value = (Main.MouseScreen - pos).ToRotation() + rotationHandleOffset;
		if (Main.mouseLeft && Main.mouseLeftRelease && rotationHandle.Contains(Main.MouseScreen)) {
			rotationHandleOffset = value - (Main.MouseScreen - pos).ToRotation();
			draggingGizmo = gizmoIndex;
		}
		gizmoIndex++;
	}
	public static void DrawAALine(Vector2 pos, Vector2 origin, int width, int height) {
		Main.spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			pos,
			new Rectangle(0, 0, 1, 1),
			Color.White,
			0,
			origin,
			new Vector2(width, height),
			0,
		0);
	}
}
public static class KeyframeModifiers {
	public static List<(string name, Func<IInterpolationModifier>)> modifierTypes = [];
	public record struct Exponential(float Exponent = 2) : IInterpolationModifier {
		public readonly float ModifyProgress(float progress) => float.Pow(progress, Exponent);
		public readonly override string ToString() => Exponent == 0 ? "Exponent" : $"Exponent({Exponent})";
		readonly string IInterpolationModifier.Export() => $"new {nameof(Exponential)}({Exponent}f)";
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
		public readonly float ModifyProgress(float progress) => (MathF.Cos(progress * MathHelper.Pi) - 1) * -0.5f;
		public readonly override string ToString() => "Sinusoidal Ease In/Out";
	}
}
public interface IAnimatableSpriteFrame<TSelf> : KeyframeSet<TSelf>.ITypeHandler where TSelf : struct, IAnimatableSpriteFrame<TSelf> {
	public static abstract Texture2D Texture { get; }
	public static abstract int FrameCount { get; }
	public int Frame { get; set; }
	static KeyframeSet<TSelf>.IInterpolation KeyframeSet<TSelf>.ITypeHandler.Linear { get; } = new Interpolation();
	public readonly struct Interpolation : KeyframeSet<TSelf>.IInterpolation {
		readonly TSelf KeyframeSet<TSelf>.IInterpolation.Interpolate(TSelf prevValue, TSelf nextValue, float progress) =>
			prevValue with { Frame = (int)float.Round(float.Lerp(prevValue.Frame, prevValue.Frame, progress)) };
		public readonly override string ToString() => "Linear";
	}
	static void KeyframeSet<TSelf>.ITypeHandler.DrawGizmo(SpriteBatch spriteBatch, ref TSelf value, ref int draggingGizmo, ref int gizmoIndex, Vector2 offset) {
		Vector2 pos = new(0, Main.screenHeight);
		Rectangle frame = TSelf.Texture.Frame(verticalFrames: TSelf.FrameCount, frameY: 0);
		Vector2 origin = new(0, frame.Height);
		Rectangle selectedFrame = default;
		Rectangle hoveredFrame = default;
		int hoveredIndex = -1;
		for (int i = 0; i < TSelf.FrameCount; i++) {
			Main.spriteBatch.Draw(
				TSelf.Texture,
				pos,
				frame,
				Color.White,
				0,
				origin,
				Vector2.One,
				0,
			0);
			Rectangle current = new((int)pos.X, (int)(pos.Y - origin.Y), frame.Width, frame.Height);
			if (i == value.Frame) selectedFrame = current;
			else if (current.Contains(Main.MouseScreen)) {
				hoveredFrame = current;
				hoveredIndex = i;
			}
			pos.X += frame.Width + 4;
			if (pos.X + frame.Width > Main.screenWidth) {
				pos.X = 0;
				pos.Y -= frame.Height + 4;
			}
			frame.Y += frame.Height;
		}
		DrawAALine(selectedFrame.TopLeft(), default, selectedFrame.Width, 2);
		DrawAALine(selectedFrame.TopLeft(), default, 2, selectedFrame.Height);
		DrawAALine(selectedFrame.BottomRight(), Vector2.One, selectedFrame.Width, 2);
		DrawAALine(selectedFrame.BottomRight(), Vector2.One, 2, selectedFrame.Height);
		if (hoveredIndex != -1) {
			DrawAALine(hoveredFrame.TopLeft(), default, hoveredFrame.Width, 2);
			DrawAALine(hoveredFrame.TopLeft(), default, 2, hoveredFrame.Height);
			DrawAALine(hoveredFrame.BottomRight(), Vector2.One, hoveredFrame.Width, 2);
			DrawAALine(hoveredFrame.BottomRight(), Vector2.One, 2, hoveredFrame.Height);
			if (Main.mouseLeft && Main.mouseLeftRelease) value.Frame = hoveredIndex;
		}
	}
	static string KeyframeSet<TSelf>.ITypeHandler.Export(TSelf value) => value.Frame.ToString();
	public static abstract implicit operator TSelf(int value);
	public static abstract implicit operator int(TSelf value);
}