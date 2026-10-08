using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
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
public class KeyframeSet<T>() : IKeyframeSet, IEnumerable<KeyframeSet<T>.Keyframe> where T : IEquatable<T> {
	#region Animating
	public T start;
	public List<Keyframe> keyframes = [];
	public float Duration => keyframes.Count == 0 ? 0 : keyframes[^1].Time;
	public KeyframeSet(T startValue, Vector2 gizmoOffset = default) : this() {
		start = startValue;
		this.gizmoOffset = gizmoOffset;
	}
	public T CalculateValue(float time) {
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
		IInterpolation baseInterpolation = Interpolatable ? CreateLinear?.Invoke() : new StepInterpolation<T>();
		public ref IInterpolation BaseInterpolation {
			get {
				baseInterpolation ??= Interpolatable ? CreateLinear?.Invoke() : new StepInterpolation<T>();
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
	#endregion
	#region Editing
	public T currentValue;
	T resetValue;
	//TODO: "child of" offset
	public Vector2 gizmoOffset;
	float lastProcessedTime = -1;
	int updateKeyframeAnimIndex = -1;
	float updateKeyframeAnim = 0;
	public static Color ModifiedKeyframeColor { get; } = new(120, 71, 222);
	private ref Keyframe this[int index] => ref CollectionsMarshal.AsSpan(keyframes)[index];
	public T GetCurrentValue(float time) {
		if (!lastProcessedTime.TrySet(time)) return currentValue;
		return currentValue = CalculateValue(time);
	}
	public void Insert(int index, Keyframe keyframe) => Do(new CreateKeyframe(this, index, keyframe));
	protected int InsertAtCurrentFrame(float currentFrame, InterpolationStack Interpolation = null) {
		if (currentFrame == 0) {
			Do(new ModifyStartValue(this, GetCurrentValue(currentFrame)));
			return -1;
		}
		for (int i = 0; i < keyframes.Count; i++) {
			if (keyframes[i].Time >= currentFrame) {
				if (keyframes[i].Time > currentFrame) {
					Insert(i, new(
						currentFrame,
						GetCurrentValue(currentFrame),
						Interpolation ?? keyframes[i].Interpolation.Clone()
					));
				}
				return i;
			}
		}
		Insert(keyframes.Count, new(
			currentFrame,
			GetCurrentValue(currentFrame),
			Interpolation ?? new()
		));
		return keyframes.Count - 1;
	}
	public void RemoveAtIndex(int index) {
		keyframes.RemoveAt(index);
		if (updateKeyframeAnimIndex == index) updateKeyframeAnimIndex = -1;
	}

	IEnumerator<Keyframe> IEnumerable<Keyframe>.GetEnumerator() => keyframes.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)keyframes).GetEnumerator();
	public float PrevKeyframeTime(float currentTime) {
		for (int i = keyframes.Count - 1; i >= 0; i--) {
			Keyframe keyframe = keyframes[i];
			if (currentTime > keyframe.Time) return keyframe.Time;
		}
		return float.NaN;
	}
	public float NextKeyframeTime(float currentTime) {
		for (int i = 0; i < keyframes.Count; i++) {
			Keyframe keyframe = keyframes[i];
			if (currentTime < keyframe.Time) return keyframe.Time;
		}
		return float.NaN;
	}
	public string Export() {
		StringBuilder builder = new("new(");
		builder.Append(ExportValue(start));
		if (gizmoOffset != default) {
			builder.Append(", ");
			builder.Append(KeyframeSet<Vector2>.ExportValue(gizmoOffset));
		}
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
	void AnimateKeyframeUpdate(int i, int dir = 1) {
		updateKeyframeAnimIndex = i;
		updateKeyframeAnim = dir;
	}
	public void DrawEditorUI(SpriteBatch spriteBatch, KeyframeAnimation animation, bool isSelected, float currentTime) {
		if (isSelected) {
			if (Keybindings.EditUndo.JustPressed) Undo();
			else if (Keybindings.EditRedo.JustPressed) Redo();
			else if (Keybindings.DeleteAllKeyframes.JustPressed && this.keyframes.Count > 0) Do(new DeleteAllKeyframes(this));
		}

		if (history.Length != DebugConfig.Instance.HistoryLength) history = new(DebugConfig.Instance.HistoryLength);
		if (future.Length != DebugConfig.Instance.HistoryLength) future = new(DebugConfig.Instance.HistoryLength);

		if (updateKeyframeAnimIndex != -1 && MathUtils.LinearSmoothing(ref updateKeyframeAnim, 0, 1f / 20)) updateKeyframeAnimIndex = -1;
		Span<Keyframe> keyframes = CollectionsMarshal.AsSpan(this.keyframes);
		bool showCreationSidebar = isSelected;
		if (isSelected) DrawGizmo?.Invoke(spriteBatch, ref currentValue, ref animation.gizmoTracker, animation.gizmoBasePosition);
		int deleteIndex = -1;
		if (lastProcessedTime != currentTime) GetCurrentValue(currentTime);

		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			new Rectangle((int)animation.TimelineToScreenPos(0) - 1, (int)animation.timelinePos.Y - 6, 2, 12),
			isSelected && currentTime == 0 && !start.Equals(currentValue) ? ModifiedKeyframeColor : Color.Orange
		);

		for (int i = 0; i < keyframes.Length; i++) {
			float rotation = i == updateKeyframeAnimIndex ? new KeyframeModifiers.SinEaseBoth().ModifyProgress(updateKeyframeAnim.Abs(out int animDir)) * -MathHelper.PiOver2 * animDir : 0;
			ref Keyframe keyframe = ref keyframes[i];
			Vector2 pos = animation.timelinePos with { X = animation.TimelineToScreenPos(keyframe.Time) };
			Color color = Color.Orange;
			if (isSelected && keyframe.Time == currentTime) {
				showCreationSidebar = false;
				if (!currentValue.Equals(keyframe.Target)) color = ModifiedKeyframeColor;
				DrawDiamond(
					spriteBatch,
					pos,
					14,
					color,
					rotation
				);
				DrawDiamond(
					spriteBatch,
					pos,
					12,
					Color.Black,
					-rotation
				);
				DrawSidebar(spriteBatch, animation, keyframes, i, ref animation.gizmoTracker);
				if (Keybindings.InsertKeyframe.JustPressed) Do(new SetKeyframeTarget(this, i, currentValue));
				if (Keybindings.DeleteKeyframe.JustPressed) deleteIndex = i;
			}
			DrawDiamond(
				spriteBatch,
				pos,
				8,
				color,
				rotation
			);
		}
		if (showCreationSidebar) {
			DrawKeyframeCreationSidebar(spriteBatch, animation, currentTime);
			if (Keybindings.InsertKeyframe.JustPressed) InsertAtCurrentFrame(currentTime);
		}
		if (deleteIndex != -1) Do(new DeleteKeyframe(this, deleteIndex));
		if (animation.gizmoTracker.JustSelected) resetValue = currentValue;
		else if (Keybindings.CancelGizmo.JustPressed && animation.gizmoTracker.Cancel()) currentValue = resetValue;
		static void DrawDiamond(SpriteBatch spriteBatch, Vector2 position, int size, Color color, float rotation) {
			Rectangle frame = new(0, 0, size, size);
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				position,
				frame,
				color,
				MathHelper.PiOver4 + rotation,
				frame.Size() * 0.5f,
				1,
				SpriteEffects.None,
			0);
		}
	}

	void DrawSidebar(SpriteBatch spriteBatch, KeyframeAnimation animation, Span<Keyframe> keyframes, int keyframeIndex, ref GizmoTracker gizmoTracker) {
		ref Keyframe keyframe = ref keyframes[keyframeIndex];
		float width = -1;
		InterpolationStack stack = keyframe.Interpolation;
		Vector2 iPos = new((Main.screenWidth + KeyframeAnimation.TimelineWidth) * 0.5f + 8, 4);
		TextSnippet[] snippets = [
			new("Update", new(107, 53, 219)),
			new(" "),
			new("Reset", Color.Red)
		];
		if (!currentValue.Equals(keyframe.Target)) {
			width = ChatManager.GetStringSize(FontAssets.ItemStack.Value, snippets, Vector2.One).X;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				iPos - Vector2.One * 2,
				new(0, 0, (int)width + 4, 16 + 4),
				new Color(90, 90, 105)
			);
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.ItemStack.Value,
				snippets,
				iPos,
				0,
				Vector2.Zero,
				Vector2.One,
				out int hoveredAction
			);
			if (Main.mouseLeft && Main.mouseLeftRelease && hoveredAction != -1) {
				Main.mouseLeftRelease = false;
				switch (hoveredAction) {
					case 0:
					Do(new SetKeyframeTarget(this, keyframeIndex, currentValue));
					break;
					case 2:
					currentValue = keyframe.Target;
					break;
				}
			}
			iPos.Y += 16 + 8;
			width = -1;
		}
		snippets[0].Color = Color.White;
		snippets[^1].Text = "X";
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
		if (moveSelectedTo != -1 && animation.modifyingInterpolation != -1) {
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
		if (animation.modifyingInterpolation >= stack.Count) animation.modifyingInterpolation = -1;
		if (stack.BaseInterpolation is not StepInterpolation<T>) {
			iPos.Y += 8;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				iPos,
				new(0, 0, 150, 150),
				Color.Black
			);
			Rectangle fram = new(0, 0, 1, 150);
			Func<float, float> modifyProgress = animation.modifyingInterpolation >= 0 ? stack[animation.modifyingInterpolation].ModifyProgress : stack.ModifyProgress;
			for (int i = 0; i < 150; i++) {
				fram.Height = (int)(150 - modifyProgress(i / 150f) * 150);
				spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					iPos + new Vector2(i, 0),
					fram,
					new Color(90, 90, 105)
				);
			}
			if (animation.modifyingInterpolation >= 0) {
				Span<IInterpolationModifier> modifiers = CollectionsMarshal.AsSpan(keyframe.Interpolation);
				modifiers[animation.modifyingInterpolation].DrawGizmo(spriteBatch, iPos, ref animation.gizmoTracker);
			}
			iPos.Y += 150 + 4;
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
			if (hoveredBase != -1) InsertAtCurrentFrame(time, new() { BaseInterpolation = CreateLinear() });
			else if (hoveredStep != -1) InsertAtCurrentFrame(time, new() { BaseInterpolation = new StepInterpolation<T>() });
		}
	}
	public void Do(UndoStep step, bool clearFuture = true) {
		if (clearFuture) future.Clear();
		step.Do();
		history.Push(step);
		lastProcessedTime = -1;
	}
	public void Undo() {
		if (DebugConfig.Instance.HistoryLength == 0) {
			Main.NewText("History is disabled");
			return;
		}
		if (!history.TryPop(out UndoStep step)) {
			Main.NewText("Nothing left to undo");
			return;
		}
		step.Undo();
		future.Push(step);
		lastProcessedTime = -1;
	}
	public void Redo() {
		if (DebugConfig.Instance.HistoryLength == 0) {
			Main.NewText("History is disabled");
			return;
		}
		if (!future.TryPop(out UndoStep step)) {
			Main.NewText("Nothing left to redo");
			return;
		}
		Do(step, false);
	}
	class CreateKeyframe(KeyframeSet<T> set, int index, Keyframe keyframe) : UndoStep {
		public override string Text => $"Create Keyframe: {keyframe}";
		public override void Do() {
			set.keyframes.Insert(index, keyframe with { Interpolation = keyframe.Interpolation.Clone() });
			set.AnimateKeyframeUpdate(index);
		}

		public override void Undo() => set.RemoveAtIndex(index);
	}
	class SetKeyframeTarget(KeyframeSet<T> set, int index, T value) : UndoStep {
		readonly T oldValue = set[index].Target;
		public override string Text => $"Set Keyframe Value: {value}";
		public override void Do() {
			set[index].Target = value;
			set.AnimateKeyframeUpdate(index);
		}

		public override void Undo() {
			set[index].Target = oldValue;
			set.AnimateKeyframeUpdate(index, -1);
		}
	}
	class DeleteKeyframe(KeyframeSet<T> set, int index) : UndoStep {
		readonly Keyframe keyframe = set[index];
		public override string Text => $"Delete Keyframe: {keyframe}";
		public override void Do() => set.RemoveAtIndex(index);
		public override void Undo() => set.keyframes.Insert(index, keyframe with { Interpolation = keyframe.Interpolation.Clone() });
	}
	class ModifyStartValue(KeyframeSet<T> set, T newValue) : UndoStep {
		readonly T oldValue = set.start;
		public override string Text => $"Modify Start Value: {newValue}";
		public override void Do() => set.start = newValue;
		public override void Undo() => set.start = oldValue;
	}
	class DeleteAllKeyframes(KeyframeSet<T> set) : UndoStep {
		readonly List<Keyframe> oldValue = set.keyframes;
		public override string Text => $"Delete All Keyframes";
		public override void Do() => set.keyframes = [];
		public override void Undo() => set.keyframes = oldValue;
	}
	UndoStep.StepBuffer history = new(10);
	UndoStep.StepBuffer future = new(10);
	public delegate void GizmoDrawer(SpriteBatch spriteBatch, ref T value, ref GizmoTracker gizmoTracker, Vector2 offset);
	public static GizmoDrawer DrawGizmo;
	public static bool Interpolatable = true;
	public static Func<IInterpolation> CreateLinear;
	public static Func<T, string> ExportValue;
	public interface ITypeHandler : IAutoload<ITypeHandler.Loader> {
		public virtual static bool Interpolatable => true;
		public abstract static void DrawGizmo(SpriteBatch spriteBatch, ref T value, ref GizmoTracker gizmoTracker, Vector2 offset);
		public abstract static IInterpolation Linear { get; }
		public abstract static string Export(T value);
		class Loader : IAutoloader {
			static readonly MethodInfo getDrawer = typeof(Loader).GetMethod("GetDrawer");
			static readonly MethodInfo getExporter = typeof(Loader).GetMethod("GetExporter");
			static readonly MethodInfo getInterpolatable = typeof(Loader).GetMethod("GetInterpolatable");
			static void IAutoloader.Autoload(Mod mod, Type type) {
				KeyframeSet<T>.DrawGizmo = (GizmoDrawer)getDrawer.MakeGenericMethod(type).Invoke(null, []);
				ExportValue = (Func<T, string>)getExporter.MakeGenericMethod(type).Invoke(null, []);
				KeyframeSet<T>.Interpolatable = (bool)getInterpolatable.MakeGenericMethod(type).Invoke(null, []);
				if (KeyframeSet<T>.Interpolatable) CreateLinear = type.GetInterfaceProperty(nameof(Linear), false).GetMethod.CreateDelegate<Func<IInterpolation>>();
			}
			public static GizmoDrawer GetDrawer<THandler>() where THandler : ITypeHandler => THandler.DrawGizmo;
			public static Func<T, string> GetExporter<THandler>() where THandler : ITypeHandler => THandler.Export;
			public static bool GetInterpolatable<THandler>() where THandler : ITypeHandler => THandler.Interpolatable;
		}
	}
	#endregion
}
public interface IKeyframeSet {
	public float Duration { get; }
	public void DrawEditorUI(SpriteBatch spriteBatch, KeyframeAnimation animation, bool isSelected, float currentTime);
	public float PrevKeyframeTime(float currentTime);
	public float NextKeyframeTime(float currentTime);
	public string Export();
	public string ExportType();
}
public interface IInterpolationModifier : IMustBeStruct, IAutoload<IInterpolationModifier.Loader> {
	public float ModifyProgress(float progress);
	public string Export() => $"new {GetType().Name}()";
	public void DrawGizmo(SpriteBatch spriteBatch, Vector2 pos, ref GizmoTracker gizmoTracker) { }
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
	public static KeyframeSet<T>.Keyframe Step<T>(int onFrame, T value) where T : IEquatable<T> => new(onFrame, value, new StepInterpolation<T>());
	public readonly struct StepInterpolation<T> : KeyframeSet<T>.IInterpolation where T : IEquatable<T> {
		public bool IsModifiable => false;
		public readonly T Interpolate(T prevValue, T nextValue, float progress) => prevValue;
		public readonly override string ToString() => "Stepped";
	}
	/*public readonly struct FloatInterpolation : KeyframeSet<float>.IInterpolation, KeyframeSet<float>.ITypeHandler {
		static KeyframeSet<float>.IInterpolation KeyframeSet<float>.ITypeHandler.Linear => new FloatInterpolation();
		public readonly float Interpolate(float prevValue, float nextValue, float progress) => float.Lerp(prevValue, nextValue, progress);
		public static string Export(float value) => $"{value}f";
		static void KeyframeSet<float>.ITypeHandler.DrawGizmo(SpriteBatch spriteBatch, ref float value, ref GizmoTracker gizmoTracker, Vector2 offset) {
			throw new NotImplementedException();
		}
	}*/
	public readonly struct Vec2Interpolation : KeyframeSet<Vector2>.IInterpolation, KeyframeSet<Vector2>.ITypeHandler {
		static KeyframeSet<Vector2>.IInterpolation KeyframeSet<Vector2>.ITypeHandler.Linear => new Vec2Interpolation();
		public readonly Vector2 Interpolate(Vector2 prevValue, Vector2 nextValue, float progress) => Vector2.Lerp(prevValue, nextValue, progress);
		public static void DrawGizmo(SpriteBatch spriteBatch, ref Vector2 value, ref GizmoTracker gizmoTracker, Vector2 offset) {
			Vector2 scale = GizmoZoom;
			if (gizmoTracker.IsCurrent && Main.MouseScreen != oldMousePos) value += (Main.MouseScreen - oldMousePos) / scale;
			Rectangle rect = new Rectangle(0, 0, (int)(16 * scale.X), (int)(16 * scale.Y)).Recentered(value * scale + offset);
			bool isHovering = rect.Contains(Main.MouseScreen) || gizmoTracker.IsCurrent;
			Color color = Color.White;
			if (!isHovering) color *= 0.5f;
			DrawAALine(rect.TopLeft(), default, rect.Width, 2, color);
			DrawAALine(rect.TopLeft(), default, 2, rect.Height, color);
			DrawAALine(rect.BottomRight(), Vector2.One, rect.Width, 2, color);
			DrawAALine(rect.BottomRight(), Vector2.One, 2, rect.Height, color);
			gizmoTracker.CheckSetCurrent(isHovering, Keybindings.GrabGizmo);
			static void DrawAALine(Vector2 pos, Vector2 origin, int width, int height, Color color) {
				Main.spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					pos,
					new Rectangle(0, 0, 1, 1),
					color,
					0,
					origin,
					new Vector2(width, height),
					0,
				0);
			}
			gizmoTracker.Advance();
		}
		public static string Export(Vector2 value) => value == Vector2.Zero ? "Vector2.Zero" : $"new({value.X}f, {value.Y}f)";
	}
	public readonly struct BoolInterpolation : KeyframeSet<bool>.ITypeHandler {
		static bool KeyframeSet<bool>.ITypeHandler.Interpolatable => false;
		static KeyframeSet<bool>.IInterpolation KeyframeSet<bool>.ITypeHandler.Linear => throw new NotImplementedException();
		static string KeyframeSet<bool>.ITypeHandler.Export(bool value) => value ? "true" : "false";
		static readonly Polygon handle = new(
			new(-1, -1),
			new(1, -1),
			new(1, 1),
			new(-1, 1)
		);
		public static void DrawGizmo(SpriteBatch spriteBatch, ref bool value, ref GizmoTracker gizmoTracker, Vector2 offset) {
			handle.ResetVertices().Scale(8).Rotate(MathHelper.PiOver4).Translate(offset);
			bool isHovering = handle.Contains(Main.MouseScreen);

			handle.MultiplyColor(0.5f + isHovering.Mul(0.5f)).DrawOutline();
			if (value) handle.Scale(6f / 8, offset).Draw(KeyframeAnimation.primitiveBatch);

			if (Main.mouseLeft && Main.mouseLeftRelease && isHovering) value = !value;
		}
	}
	static readonly Polygon rotationHandle = new(
		new(-1, -1),
		new(1, -1),
		new(1, 1),
		new(-1, 1)
	);
	static float rotationHandleOffset;
	public static void DrawRotationGizmo(SpriteBatch spriteBatch, ref float value, ref GizmoTracker gizmoTracker, Vector2 pos, float size) {
		float scale = GizmoZoom.X;
		pos = pos.Floor();
		if (gizmoTracker.IsCurrent && Main.MouseScreen != oldMousePos) value = (Main.MouseScreen - pos).ToRotation() + rotationHandleOffset;
		Vector2 end = (pos + (value - MathHelper.PiOver2).ToRotationVector2() * (size * scale)).Floor();
		spriteBatch.DrawLine(Color.White, pos + Main.screenPosition, end + Main.screenPosition);

		rotationHandle.ResetPositions().Scale(4 * scale).Rotate(value).Translate(end);
		bool isHovering = rotationHandle.Contains(Main.MouseScreen) || gizmoTracker.IsCurrent;
		Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
		rotationHandle.ResetColors().MultiplyColor(0.5f + isHovering.Mul(0.5f)).Draw(KeyframeAnimation.primitiveBatch);
		Main.pixelShader.CurrentTechnique.Passes[0].Apply();

		if (gizmoTracker.CheckSetCurrent(isHovering, Keybindings.RotateGizmo)) {
			rotationHandleOffset = value - (Main.MouseScreen - pos).ToRotation();
		}
		gizmoTracker.Advance();
	}
	static float scaleHandleStart;
	static float scaleHandleOffset;
	public static void DrawScaleGizmo(SpriteBatch spriteBatch, ref float value, ref GizmoTracker gizmoTracker, Vector2 pos, float size) {
		const float handle_thickness = 2;
		float distSQ = (Main.MouseScreen - pos).LengthSquared();
		if (gizmoTracker.IsCurrent && Main.MouseScreen != oldMousePos) value = scaleHandleStart * (float.Sqrt(distSQ) / scaleHandleOffset);

		bool isHovering = gizmoTracker.IsCurrent || (distSQ >= (size - handle_thickness).Square() && distSQ <= (size + handle_thickness).Square());
		ShaderCircle.Draw(pos, size + handle_thickness, Color.White * (0.5f + isHovering.Mul(0.5f)), size - handle_thickness);

		if (gizmoTracker.CheckSetCurrent(isHovering, Keybindings.ScaleGizmo)) {
			scaleHandleStart = value;
			scaleHandleOffset = float.Sqrt(distSQ);
		}
		gizmoTracker.Advance();
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
	public static Vector2 GizmoZoom => new(Main.ForcedMinimumZoom * MathHelper.Clamp(Main.GameZoomTarget, 1f, 2f));
	public static Vector2 oldMousePos;
}
public static class KeyframeModifiers {
	public static List<(string name, Func<IInterpolationModifier>)> modifierTypes = [];
	public record struct Exponential(float Exponent = 2) : IInterpolationModifier {
		public readonly float ModifyProgress(float progress) => float.Pow(progress, Exponent);
		public readonly override string ToString() => Exponent == 0 ? "Exponent" : $"Exponent({Exponent})";
		readonly string IInterpolationModifier.Export() => $"new {nameof(Exponential)}({Exponent}f)";
		static readonly Polygon handle = new(
			new(-1, 0),
			new(0, -1),
			new(1, 0),
			new(0, 1)
		);
		static Vector2 handleOffset;
		static float handleX = 0.5f;
		static Vector2 lastMouse;
		public void DrawGizmo(SpriteBatch spriteBatch, Vector2 pos, ref int draggingGizmo, ref int gizmoIndex) {
			Vector2 mouse = (Main.MouseScreen - pos) / 150;
			float handleY = 1 - float.Pow(handleX, Exponent);
			handle.ResetPositions().Scale(6).Translate(pos + new Vector2(handleX, handleY) * 150);
			bool isHovering = handle.Contains(Main.MouseScreen) || draggingGizmo == gizmoIndex;
			handle.ResetColors().MultiplyColor(0.5f + isHovering.Mul(0.5f)).Draw(KeyframeAnimation.primitiveBatch);
			if (draggingGizmo == gizmoIndex && lastMouse.TrySet(mouse)) {
				handleX = mouse.X + handleOffset.X;
				Clamp(ref handleX, 0, 1);
				Exponent = float.Log(1 - (mouse.Y - handleOffset.Y), handleX);
			}
			if (Main.mouseLeft && Main.mouseLeftRelease) {
				handleOffset = new Vector2(handleX, handleY) - mouse;
				draggingGizmo = gizmoIndex;
			}
			gizmoIndex++;
		}
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
public struct GizmoTracker() {
	int usingGizmo = -1;
	int currentIndex;
	bool usingMouseDown;
	public bool JustSelected { readonly get; private set; }
	public readonly bool IsCurrent => usingGizmo == currentIndex;
	void SetCurrent() {
		usingGizmo = currentIndex;
		usingMouseDown = Main.mouseLeft;
	}
	public bool CheckSetCurrent(bool isHovered, ModKeybind keybind) {
		if (isHovered) Main.LocalPlayer.mouseInterface = true;
		if (IsCurrent) {
			if ((Main.mouseLeft && Main.mouseLeftRelease) || (keybind?.JustPressed ?? false)) usingGizmo = -1;
		} else if ((isHovered && Main.mouseLeft && Main.mouseLeftRelease) || (keybind?.JustPressed ?? false)) {
			SetCurrent();
			JustSelected = true;
			return true;
		}
		return false;
	}
	public void ResetCurrent() => currentIndex = 0;
	public void Advance() => currentIndex++;
	public void Update() {
		if (usingMouseDown && !Main.mouseLeft) usingGizmo = -1;
		JustSelected = false;
	}
	public bool Cancel() {
		if (usingGizmo != -1) {
			usingGizmo = -1;
			return true;
		}
		return false;
	}
}
public interface IAnimatableSpriteFrame<TSelf> : KeyframeSet<TSelf>.ITypeHandler, IEquatable<TSelf>, IEqualityOperators<TSelf, TSelf, bool> where TSelf : struct, IAnimatableSpriteFrame<TSelf> {
	public static abstract Texture2D Texture { get; }
	public static abstract int FrameCount { get; }
	public int Frame { get; set; }
	bool IEquatable<TSelf>.Equals(TSelf other) => ((TSelf)this) == other;
	static KeyframeSet<TSelf>.IInterpolation KeyframeSet<TSelf>.ITypeHandler.Linear { get; } = new Interpolation();
	public readonly struct Interpolation : KeyframeSet<TSelf>.IInterpolation {
		readonly TSelf KeyframeSet<TSelf>.IInterpolation.Interpolate(TSelf prevValue, TSelf nextValue, float progress) =>
			prevValue with { Frame = (int)float.Round(float.Lerp(prevValue.Frame, prevValue.Frame, progress)) };
		public readonly override string ToString() => "Interpolated";
	}
	static void KeyframeSet<TSelf>.ITypeHandler.DrawGizmo(SpriteBatch spriteBatch, ref TSelf value, ref GizmoTracker gizmoTracker, Vector2 offset) {
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
public record struct PosRotScale(Vector2 Position, float Rotation, float Scale) : KeyframeSet<PosRotScale>.ITypeHandler {
	public PosRotScale() : this(default, 0, 1) { }
	public Vector2 Position = Position;
	public float Rotation = Rotation;
	public float Scale = Scale;
	public static KeyframeSet<PosRotScale>.IInterpolation Linear { get; } = new Interpolation();
	public readonly struct Interpolation : KeyframeSet<PosRotScale>.IInterpolation {
		readonly PosRotScale KeyframeSet<PosRotScale>.IInterpolation.Interpolate(PosRotScale prevValue, PosRotScale nextValue, float progress) =>
			new(
				Vector2.Lerp(prevValue.Position, nextValue.Position, progress),
				float.Lerp(prevValue.Rotation, nextValue.Rotation, progress),
				float.Lerp(prevValue.Scale, nextValue.Scale, progress)
			);
		public readonly override string ToString() => "Interpolated";
	}
	public static PosRotScale operator +(PosRotScale a, PosRotScale b) => new(a.Position + b.Position, a.Rotation + b.Rotation, a.Scale * b.Scale);
	public static void DrawGizmo(SpriteBatch spriteBatch, ref PosRotScale value, ref GizmoTracker gizmoTracker, Vector2 offset) {
		KeyframeTypes.Vec2Interpolation.DrawGizmo(spriteBatch, ref value.Position, ref gizmoTracker, offset);
		KeyframeTypes.DrawRotationGizmo(spriteBatch, ref value.Rotation, ref gizmoTracker, value.Position * GizmoZoom + offset, 32);
		KeyframeTypes.DrawScaleGizmo(spriteBatch, ref value.Scale, ref gizmoTracker, value.Position * GizmoZoom + offset, 20);
	}
	public static string Export(PosRotScale value) => value == new PosRotScale() ? "new()" : $"new({KeyframeTypes.Vec2Interpolation.Export(value.Position)}, {value.Rotation}f, {value.Scale}f)";
}
public abstract class UndoStep {
	public abstract string Text { get; }
	public abstract void Do();
	public abstract void Undo();
	public struct StepBuffer(int size) {
		public readonly int Length => buffer?.Length ?? 0;
		readonly UndoStep[] buffer = size == 0 ? null : new UndoStep[size];
		int wrapIndex = 0;
		int currentIndex = 0;
		public void Push(UndoStep step) => buffer[IncrementCurrent()] = step;
		public bool TryPop([MaybeNullWhen(false)] out UndoStep step) {
			if (currentIndex == wrapIndex) {
				step = default;
				return false;
			}
			step = buffer[currentIndex];
			currentIndex--;
			if (currentIndex < 0) currentIndex += buffer.Length;
			return true;
		}
		public void Clear() {
			currentIndex = wrapIndex;
			Array.Clear(buffer);
		}
		int IncrementCurrent() {
			currentIndex++;
			currentIndex %= buffer.Length;
			if (currentIndex == wrapIndex) {
				wrapIndex++;
				wrapIndex %= buffer.Length;
			}
			return currentIndex;
		}
	}
}