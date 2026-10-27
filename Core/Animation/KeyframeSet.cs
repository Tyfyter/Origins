using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Graphics.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using static Origins.Core.Animation.KeyframeAnimation;
using static Origins.Core.Animation.KeyframeTypes;

namespace Origins.Core.Animation;
public class KeyframeSet<T>() : AKeyframeSet<KeyframeSet<T>.Keyframe> where T : IEquatable<T> {
	public override bool CanLoop => true;
	#region Animating
	public T start;
	public InterpolationStack loopInterpolation;
	public KeyframeSet(T startValue, AGizmoOffset gizmoOffset = null) : this(startValue, (false, null), gizmoOffset) { }
	public KeyframeSet(T startValue, (bool looped, InterpolationStack interpolation) loopData, AGizmoOffset gizmoOffset = null) : this() {
		start = startValue;
		(looped, loopInterpolation) = loopData;
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
		if (looped) return (loopInterpolation ??= []).Interpolate(prevValue, start, Utils.GetLerpValue(prevTime, Animation.totalLength + 1, time));
		return prevValue;
	}
	public record struct Keyframe(float Time, T Target, InterpolationStack Interpolation) : IKeyframe<Keyframe> {
		public T Target = Target;
		public Keyframe(float time, T target, IInterpolation interpolation) : this(time, target, new InterpolationStack() { BaseInterpolation = interpolation }) { }
		public Keyframe(float time, T target) : this(time, target, new InterpolationStack()) { }
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
		public readonly Keyframe CleanMutableRefs() => this with { Interpolation = Interpolation.Clone() };
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
			$"Stepped" :
			$"[{string.Join(", ", this.Select(i => i.Export()))}]";
		public string Export() => ToString();
		public static implicit operator InterpolationStack(KeyframeData.StepInterpolationToken token) => new() { baseInterpolation = new StepInterpolation<T>() };
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
	readonly AGizmoOffset gizmoOffset;
	public static Color ModifiedKeyframeColor { get; } = new(120, 71, 222);
	public T GetCurrentValue(float time) {
		if (!lastProcessedTime.TrySet(time)) return currentValue;
		return currentValue = CalculateValue(time);
	}
	void Insert(int index, Keyframe keyframe) => Do(new CreateKeyframe(this, index, keyframe));
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
	public override string Export(string indent) {
		StringBuilder builder = new("new(");
		builder.Append(ExportValue(start));
		if (looped) builder.Append($", (true, {(loopInterpolation ?? []).Export()})");
		if (gizmoOffset is not null) {
			builder.Append(", ");
			builder.Append(gizmoOffset.Export());
		}
		builder.Append(')');
		builder.Append(" {\n");
		for (int i = 0; i < keyframes.Count; i++) {
			builder.Append('\t');
			builder.Append(indent);
			builder.Append(keyframes[i].Export());
			if (i + 1 < keyframes.Count) builder.Append(",\n");
			else builder.Append('\n');
		}
		builder.Append(indent);
		builder.Append('}');
		return builder.ToString();
	}
	public override string ExportType() => $"KeyframeSet<{typeof(T).Name}>";
	public GizmoOffset GizmoOffset(float time) {
		GizmoOffset offset = gizmoOffset?.GetOffset(time) ?? default;
		(GetCurrentValue(time) as IKeyframeOffsetModifier)?.ModifyGizmoOffset(ref offset);
		return offset;
	}
	static readonly TextSnippet[] threeSnippets = [
		new(),
		new(" "),
		new("", Color.Red)
	];
	static readonly TextSnippet[] oneSnippet = [
		new()
	];
	void DrawSidebar(SpriteBatch spriteBatch, int keyframeIndex, ref GizmoTracker gizmoTracker) {
		Span<Keyframe> keyframes = CollectionsMarshal.AsSpan(this.keyframes);
		ref Keyframe keyframe = ref keyframes[keyframeIndex];
		Vector2 iPos = new((Main.screenWidth + KeyframeAnimation.TimelineWidth) * 0.5f + 8, 4);
		threeSnippets[0].Text = "Update";
		threeSnippets[0].Color = new(107, 53, 219);
		threeSnippets[2].Text = "Reset";
		if (!currentValue.Equals(keyframe.Target)) {
			float width = ChatManager.GetStringSize(FontAssets.ItemStack.Value, threeSnippets, Vector2.One).X;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				iPos - Vector2.One * 2,
				new(0, 0, (int)width + 4, 16 + 4),
				new Color(90, 90, 105)
			);
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.ItemStack.Value,
				threeSnippets,
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
		}
		DrawInterpolationModifierEditor(spriteBatch, keyframe.Interpolation, iPos, ref gizmoTracker, keyframeIndex);
	}
	void DrawInterpolationModifierEditor(SpriteBatch spriteBatch, InterpolationStack stack, Vector2 iPos, ref GizmoTracker gizmoTracker, int keyframeIndex = -1) {
		threeSnippets[0].Color = Color.White;
		threeSnippets[2].Text = "X";
		int moveSelectedTo = -1;
		float width = -1;
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
			threeSnippets[0].Text = stack[i].ToString();
			threeSnippets[0].Color = Animation.modifyingInterpolation == i ? Color.Goldenrod : Color.White;
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.ItemStack.Value,
				threeSnippets,
				iPos,
				0,
				Vector2.Zero,
				Vector2.One,
				out int hoveredModifier
			);
			ShowSwap(spriteBatch, Animation, i + 1, iPos, ref moveSelectedTo, (int)width);
			if (Main.mouseLeft && Main.mouseLeftRelease && hoveredModifier != -1) {
				Main.mouseLeftRelease = false;
				switch (hoveredModifier) {
					case 0:
					Animation.modifyingInterpolation = i.OrXIf(Animation.modifyingInterpolation, -1);
					Animation.draggingInterpolation = true;
					break;
					case 2:
					Do(new RemoveInterpolationModifier(stack, i, this, keyframeIndex));
					break;
				}
			}
			iPos.Y += 16;
		}
		ShowSwap(spriteBatch, Animation, 0, iPos, ref moveSelectedTo, (int)width);
		skipModifiers:
		oneSnippet[0].Text = stack.BaseInterpolation.ToString();
		oneSnippet[0].Color = Animation.modifyingInterpolation == -2 ? Color.Goldenrod : Color.White;
		ChatManager.DrawColorCodedStringWithShadow(
			spriteBatch,
			FontAssets.ItemStack.Value,
			oneSnippet,
			iPos,
			0,
			Vector2.Zero,
			Vector2.One,
			out int hoveredBase
		);
		if (moveSelectedTo != -1 && Animation.modifyingInterpolation != -1) {
			Do(new MoveInterpolationModifier(
				stack,
				Animation.modifyingInterpolation,
				moveSelectedTo,
				this,
				keyframeIndex
			));
			Animation.modifyingInterpolation = moveSelectedTo;
		}
		if (hoveredBase != -1) Main.LocalPlayer.mouseInterface = true;
		if (Main.mouseLeft && Main.mouseLeftRelease && hoveredBase != -1) Animation.modifyingInterpolation = -2;
		width = -1;
		iPos.Y += 16 + 8;
		oneSnippet[0].Color = Color.White;
		if (Animation.modifyingInterpolation == -2) {
			width = FontAssets.ItemStack.Value.MeasureString(CreateLinear is not null ? "Interpolated" : "Stepped").X;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				iPos + new Vector2(-2, -2),
				new(0, 0, (int)width + 4, (1 + (CreateLinear is not null).ToInt()) * 16 + 4),
				new Color(90, 90, 105)
			);
			hoveredBase = -1;
			if (CreateLinear is not null) {
				oneSnippet[0].Text = "Interpolated";
				ChatManager.DrawColorCodedStringWithShadow(
					spriteBatch,
					FontAssets.ItemStack.Value,
					oneSnippet,
					iPos,
					0,
					Vector2.Zero,
					Vector2.One,
					out hoveredBase
				);
				iPos.Y += 16;
			}
			oneSnippet[0].Text = "Stepped";
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.ItemStack.Value,
				oneSnippet,
				iPos,
				0,
				Vector2.Zero,
				Vector2.One,
				out int hoveredStep
			);
			iPos.Y += 16;
			if (hoveredBase != -1) Main.LocalPlayer.mouseInterface = true;
			else if (hoveredStep != -1) Main.LocalPlayer.mouseInterface = true;
			if (Main.mouseLeft && Main.mouseLeftRelease) {
				IInterpolation interpolation = null;
				if (hoveredBase != -1) interpolation = CreateLinear();
				else if (hoveredStep != -1) interpolation = new StepInterpolation<T>();
				if (interpolation is not null) {
					Do(keyframeIndex == -1 ? new SetAnyBaseInterpolation(stack, interpolation) : new SetKeyframeBaseInterpolation(this, keyframeIndex, interpolation));
					Animation.modifyingInterpolation = -1;
				}
			}
		} else if (stack.BaseInterpolation.IsModifiable) {
			for (int i = KeyframeData.modifierTypes.Count - 1; i >= 0; i--) Max(ref width, FontAssets.ItemStack.Value.MeasureString(KeyframeData.modifierTypes[i].name).X);
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				iPos + new Vector2(-2, -2),
				new(0, 0, (int)width + 4, KeyframeData.modifierTypes.Count * 16 + 4),
				new Color(90, 90, 115)
			);
			for (int i = KeyframeData.modifierTypes.Count - 1; i >= 0; i--) {
				oneSnippet[0].Text = KeyframeData.modifierTypes[i].name;
				ChatManager.DrawColorCodedStringWithShadow(
					spriteBatch,
					FontAssets.ItemStack.Value,
					oneSnippet,
					iPos,
					0,
					Vector2.Zero,
					Vector2.One,
					out int hoveredModifier
				);
				if (Main.mouseLeft && Main.mouseLeftRelease && hoveredModifier != -1) {
					Main.mouseLeftRelease = false;
					IInterpolationModifier modifier = KeyframeData.modifierTypes[i].Item2();
					if (Animation.modifyingInterpolation == -1) Do(new AddInterpolationModifier(stack, stack.Count, modifier, this, keyframeIndex));
					else Do(new SetInterpolationModifier(stack, Animation.modifyingInterpolation, modifier, this, keyframeIndex));
				}
				iPos.Y += 16;
			}
		}
		if (Animation.modifyingInterpolation >= stack.Count) Animation.modifyingInterpolation = -1;
		if (stack.BaseInterpolation.IsModifiable) {
			iPos.Y += 8;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				iPos,
				new(0, 0, 150, 150),
				Color.Black
			);
			Rectangle fram = new(0, 0, 1, 150);
			if (oldModifyingInterpolation.TrySet(Animation.modifyingInterpolation)) currentModifier = null;
			if (Animation.modifyingInterpolation >= 0) currentModifier ??= stack[Animation.modifyingInterpolation].Clone();
			Func<float, float> modifyProgress = currentModifier is null ? stack.ModifyProgress : currentModifier.ModifyProgress;
			for (int i = 0; i < 150; i++) {
				fram.Height = (int)(150 - modifyProgress(i / 150f) * 150);
				spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					iPos + new Vector2(i, 0),
					fram,
					new Color(90, 90, 105)
				);
			}
			if (Animation.modifyingInterpolation >= 0) {
				currentModifier.DrawGizmo(spriteBatch, iPos, ref gizmoTracker);
				if (wasDraggingInterpolation.TrySet(gizmoTracker.IsPrevious) && !gizmoTracker.IsPrevious) {
					if (!currentModifier.Equals(stack[Animation.modifyingInterpolation])) {
						Do(new SetInterpolationModifier(stack, Animation.modifyingInterpolation, currentModifier, this, keyframeIndex));
					}
					currentModifier = null;
				}
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
	bool wasDraggingInterpolation;
	IInterpolationModifier currentModifier;
	int oldModifyingInterpolation = -1;
	protected override void DrawKeyframeCreationSidebar(SpriteBatch spriteBatch, float time) {
		Vector2 iPos = new((Main.screenWidth + KeyframeAnimation.TimelineWidth) * 0.5f + 8, 4);
		if (time == 0) {
			if (looped) DrawInterpolationModifierEditor(spriteBatch, loopInterpolation ??= [], iPos, ref Animation.gizmoTracker);
			return;
		}
		oneSnippet[0].Color = Color.White;
		float width = FontAssets.ItemStack.Value.MeasureString(CreateLinear is not null ? "Interpolated" : "Stepped").X;
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			iPos + new Vector2(-2, -2),
			new(0, 0, (int)width + 4, (2 + (CreateLinear is not null).ToInt()) * 16 + 4),
			new Color(90, 90, 105)
		);
		oneSnippet[0].Text = "Insert:";
		ChatManager.DrawColorCodedStringWithShadow(
			spriteBatch,
			FontAssets.ItemStack.Value,
			oneSnippet,
			iPos,
			0,
			Vector2.Zero,
			Vector2.One,
			out _
		);
		iPos.Y += 16;
		int hoveredBase = -1;
		if (CreateLinear is not null) {
			oneSnippet[0].Text = "Interpolated";
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.ItemStack.Value,
				oneSnippet,
				iPos,
				0,
				Vector2.Zero,
				Vector2.One,
				out hoveredBase
			);
			iPos.Y += 16;
		}
		oneSnippet[0].Text = "Stepped";
		ChatManager.DrawColorCodedStringWithShadow(
			spriteBatch,
			FontAssets.ItemStack.Value,
			oneSnippet,
			iPos,
			0,
			Vector2.Zero,
			Vector2.One,
			out int hoveredStep
		);
		if (hoveredBase != -1) Main.LocalPlayer.mouseInterface = true;
		else if (hoveredStep != -1) Main.LocalPlayer.mouseInterface = true;
		if (Main.mouseLeft && Main.mouseLeftRelease) {
			if (hoveredBase != -1) InsertAtCurrentFrame(time, new() { BaseInterpolation = CreateLinear() });
			else if (hoveredStep != -1) InsertAtCurrentFrame(time, new() { BaseInterpolation = new StepInterpolation<T>() });
		}
	}
	protected override void PreDrawKeyframes(SpriteBatch spriteBatch, SelectionType isSelected, float currentTime) {
		if (isSelected.ShowGizmos()) DrawGizmo?.Invoke(spriteBatch, ref currentValue, ref Animation.gizmoTracker, Animation.gizmoBasePosition + GizmoOffset(currentTime));
		if (lastProcessedTime != currentTime) GetCurrentValue(currentTime);
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			new Rectangle((int)Animation.TimelineToScreenPos(0) - 1, (int)Animation.timelinePos.Y - 6, 2, 12),
			isSelected.IsSelected() && currentTime == 0 && !start.Equals(currentValue) ? ModifiedKeyframeColor : Color.Orange
		);
		if (looped) {
			Rectangle loopIndicator = new((int)Animation.TimelineToScreenPos(Animation.totalLength) - 4, (int)Animation.timelinePos.Y - 4, 3, 2);
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				loopIndicator,
				Color.Orange
			);
			loopIndicator.Y += 6;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				loopIndicator,
				Color.Orange
			);
			loopIndicator.X = (int)Animation.TimelineToScreenPos(0) + 2;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				loopIndicator,
				Color.Orange
			);
			loopIndicator.Y -= 6;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				loopIndicator,
				Color.Orange
			);
		}
	}
	protected override bool DrawKeyframe(SpriteBatch spriteBatch, int keyframeIndex, Vector2 position, bool isKeyframeSelected, bool isTimelineSelected) {
		ref Keyframe keyframe = ref this[keyframeIndex];
		float rotation = keyframeIndex == updateKeyframeAnimIndex ? new KeyframeData.SinEaseBoth().ModifyProgress(updateKeyframeAnim.Abs(out int animDir)) * -MathHelper.PiOver2 * animDir : 0;
		Vector2 pos = Animation.timelinePos with { X = Animation.TimelineToScreenPos(keyframe.Time) };
		keyframeDiamond.ResetPositions().Rotate(rotation).Translate(pos);
		keyframeDiamond.FillColor(keyframeIndex == draggingKeyframeIndex ? Color.Lime : Color.Orange);
		if (isKeyframeSelected) {
			if (!currentValue.Equals(keyframe.Target)) keyframeDiamond.FillColor(ModifiedKeyframeColor);
			using (keyframeDiamond.ModificationContext()) {
				keyframeDiamond.Scale(1.75f, keyframeDiamond.TransformedOrigin).Draw(KeyframeAnimation.primitiveBatch);
				keyframeDiamond.Scale(12f / 14f, keyframeDiamond.TransformedOrigin).FillColor(Color.Black).Rotate(rotation * -2, keyframeDiamond.TransformedOrigin).Draw(KeyframeAnimation.primitiveBatch);
			}
		}
		keyframeDiamond.Draw(KeyframeAnimation.primitiveBatch);
		return isTimelineSelected && keyframeDiamond.Contains(Main.MouseScreen);
	}
	protected override void DrawKeyframeEditor(SpriteBatch spriteBatch, int keyframeIndex, ref GizmoTracker gizmoTracker) {
		DrawSidebar(spriteBatch, keyframeIndex, ref gizmoTracker);
	}
	protected override void TryUpdateKeyframe(int keyframeIndex) {
		Do(new SetKeyframeTarget(this, keyframeIndex, currentValue));
	}
	protected override void TryInsertKeyframe(float time) {
		InsertAtCurrentFrame(time);
	}
	protected override void TryPasteKeyframe(float currentTime, int insertIndex) {
		if (currentTime == 0) {
			Do(new ModifyStartValue(this, clipboardKeyframe.Target));
			return;
		}
		base.TryPasteKeyframe(currentTime, insertIndex);
	}
	protected override bool CopyStart(out Keyframe keyframe) {
		keyframe = new(0, start);
		return true;
	}
	protected override void DrawKeyframeDragTarget(SpriteBatch spriteBatch, Vector2 position) =>
		keyframeDiamond.TranslateTo(position).DrawOutline(KeyframeAnimation.primitiveBatch);
	protected override void HandleGizmos() {
		if (Animation.gizmoTracker.JustSelected) resetValue = currentValue;
		else if (Keybindings.CancelGizmo.JustPressed && Animation.gizmoTracker.Cancel()) currentValue = resetValue;
	}
	public override void HandleMultiSelect(IKeyframeSet primary) {
		if (primary is KeyframeSet<T> _primary) currentValue = _primary.currentValue;
	}
	class SetKeyframeTarget(KeyframeSet<T> set, int index, T value) : KeyframeVariableStep<T>(set, index, value) {
		public override string Text => $"Set Keyframe Value: {newValue}";
		protected override ref T Variable => ref set[index].Target;
	}
	class SetKeyframeBaseInterpolation(KeyframeSet<T> set, int index, IInterpolation value) : KeyframeVariableStep<IInterpolation>(set, index, value) {
		readonly InterpolationStack stack = set[index].Interpolation;
		public override string Text => $"Set Keyframe Interpolation: {newValue}";
		protected override ref IInterpolation Variable => ref stack.BaseInterpolation;
	}
	class SetAnyBaseInterpolation(InterpolationStack stack, IInterpolation value) : VariableUndoStep<IInterpolation>(value) {
		public override string Text => $"Set Interpolation: {newValue}";
		protected override ref IInterpolation Variable => ref stack.BaseInterpolation;
	}
	class AddInterpolationModifier(InterpolationStack stack, int index, IInterpolationModifier modifier, KeyframeSet<T> set = null, int keyframeIndex = -1) : UndoStep {
		readonly KeyframeSet<T> set = keyframeIndex == -1 ? null : set;
		public override string Text => $"Add Interpolation Modifier: {modifier}";
		public override void Do() {
			stack.Insert(index, modifier);
			AnimateKeyframeUpdate();
		}
		public override void Undo() {
			stack.RemoveAt(index);
			AnimateKeyframeUpdate(-1);
		}
		void AnimateKeyframeUpdate(int dir = 1) => set?.AnimateKeyframeUpdate(keyframeIndex, dir);
	}
	class RemoveInterpolationModifier(InterpolationStack stack, int index, KeyframeSet<T> set = null, int keyframeIndex = -1) : UndoStep {
		readonly KeyframeSet<T> set = keyframeIndex == -1 ? null : set;
		readonly IInterpolationModifier modifier = stack[index];
		public override string Text => $"Remove Interpolation Modifier: {modifier}";
		public override void Do() {
			stack.RemoveAt(index);
			AnimateKeyframeUpdate();
		}
		public override void Undo() {
			stack.Insert(index, modifier);
			AnimateKeyframeUpdate(-1);
		}
		void AnimateKeyframeUpdate(int dir = 1) => set?.AnimateKeyframeUpdate(keyframeIndex, dir);
	}
	class SetInterpolationModifier(InterpolationStack stack, int index, IInterpolationModifier modifier, KeyframeSet<T> set, int keyframeIndex = -1) : UndoStep {
		readonly IInterpolationModifier oldValue = stack[index];
		public override string Text => $"Replace Interpolation Modifier: {modifier}";
		public override void Do() {
			stack[index] = modifier;
			set.currentModifier = null;
			AnimateKeyframeUpdate();
		}
		public override void Undo() {
			stack[index] = oldValue;
			set.currentModifier = null;
			AnimateKeyframeUpdate(-1);
		}
		void AnimateKeyframeUpdate(int dir = 1) {
			if (keyframeIndex != -1) set.AnimateKeyframeUpdate(keyframeIndex, dir);
		}
	}
	class MoveInterpolationModifier(InterpolationStack stack, int fromIndex, int toIndex, KeyframeSet<T> set = null, int keyframeIndex = -1) : CombinedUndoStep(
		$"Move Interpolation Modifier {stack[fromIndex]}: {fromIndex} --> {toIndex}",
		new RemoveInterpolationModifier(stack, fromIndex, set, keyframeIndex),
		new AddInterpolationModifier(stack, toIndex, stack[fromIndex], set, keyframeIndex)
	) {}
	class ModifyStartValue(KeyframeSet<T> set, T newValue) : UndoStep {
		readonly T oldValue = set.start;
		public override string Text => $"Modify Start Value: {newValue}";
		public override void Do() => set.start = newValue;
		public override void Undo() => set.start = oldValue;
	}
	public delegate void GizmoDrawer(SpriteBatch spriteBatch, ref T value, ref GizmoTracker gizmoTracker, GizmoOffset offset);
	public static GizmoDrawer DrawGizmo;
	public static bool Interpolatable = true;
	public static Func<IInterpolation> CreateLinear;
	public static Func<T, string> ExportValue;
	public interface ITypeHandler : IAutoload<ITypeHandler.Loader> {
		public virtual static bool Interpolatable => true;
		public abstract static void DrawGizmo(SpriteBatch spriteBatch, ref T value, ref GizmoTracker gizmoTracker, GizmoOffset offset);
		public abstract static IInterpolation Linear { get; }
		public abstract static string Export(T value);
		class Loader : IAutoloader {
			static readonly MethodInfo getDrawer = typeof(Loader).GetMethod("GetDrawer");
			static readonly MethodInfo getExporter = typeof(Loader).GetMethod("GetExporter");
			static readonly MethodInfo getInterpolatable = typeof(Loader).GetMethod("GetInterpolatable");
			static readonly MethodInfo checkLoad = typeof(Loader).GetMethod("CheckLoad");
			public static void Autoload(Mod mod, Type type) {
				if (!(bool)checkLoad.MakeGenericMethod(type).Invoke(null, [])) return;
				KeyframeSet<T>.DrawGizmo = (GizmoDrawer)getDrawer.MakeGenericMethod(type).Invoke(null, []);
				ExportValue = (Func<T, string>)getExporter.MakeGenericMethod(type).Invoke(null, []);
				KeyframeSet<T>.Interpolatable = (bool)getInterpolatable.MakeGenericMethod(type).Invoke(null, []);
				if (KeyframeSet<T>.Interpolatable) CreateLinear = type.GetInterfaceProperty(nameof(Linear), false).GetMethod.CreateDelegate<Func<IInterpolation>>();
			}
			public static GizmoDrawer GetDrawer<THandler>() where THandler : ITypeHandler => THandler.DrawGizmo;
			public static Func<T, string> GetExporter<THandler>() where THandler : ITypeHandler => THandler.Export;
			public static bool GetInterpolatable<THandler>() where THandler : ITypeHandler => THandler.Interpolatable;
			public static bool CheckLoad<THandler>() where THandler : ITypeHandler => IsLoaded<THandler>.value.TrySet(true);
			static class IsLoaded<THandler> where THandler : ITypeHandler {
				public static bool value = false;
			}
		}
		public static void EnsureLoaded<THandler>() where THandler : ITypeHandler => Loader.Autoload(ModContent.GetInstance<Origins>(), typeof(THandler));
	}
	static readonly Polygon keyframeDiamond = new(
		new(-4, 0),
		new(0, -4),
		new(4, 0),
		new(0, 4)
	);
	protected override void OnBind() {
		gizmoOffset?.Bind(Animation);
	}
	#endregion
}
public class EventKeyframeSet<TParams> : AKeyframeSet<EventKeyframeSet<TParams>.Keyframe> {
	public delegate void Event(in TParams parameter);
	public record struct Keyframe(float Time, Event Event) : IKeyframe<Keyframe> {
		public Event Event = Event;
		public readonly string Export() {
			StringBuilder builder = new("new(");
			builder.Append(Time);
			builder.Append(Time == (int)Time ? ", " : "f, ");
			builder.Append(Event.Method.Name);
			builder.Append(')');
			return builder.ToString();
		}
		public readonly Keyframe CleanMutableRefs() => this;
	}
	public static Color KeyframeColor { get; } = new(43, 185, 255);
	public static Color ModifiedKeyframeColor { get; } = new(120, 71, 222);
	protected override int KeyframeUpdateAnimationFrames => 27;
	public void Update(in TParams parameters, float lastTime, float currentTime) {
		for (int i = 0; i < keyframes.Count; i++) {
			ref Keyframe keyframe = ref this[i];
			if (keyframe.Time <= lastTime) continue;
			if (keyframe.Time > currentTime) break;
			keyframe.Event(in parameters);
		}
	}
	List<Event> options;
	float currentlyDrawingTime;
	void Insert(float time, Event @event) {
		for (int i = 0; i < keyframes.Count; i++) {
			if (keyframes[i].Time < time) continue;
			if (keyframes[i].Time > time) {
				Do(new CreateKeyframe(this, i, new(time, @event)));
				return;
			}
		}
		Do(new CreateKeyframe(this, keyframes.Count, new(time, @event)));
	}
	protected override void PreDrawKeyframes(SpriteBatch spriteBatch, SelectionType isSelected, float currentTime) {
		currentlyDrawingTime = currentTime;
	}
	protected override bool DrawKeyframe(SpriteBatch spriteBatch, int keyframeIndex, Vector2 position, bool isKeyframeSelected, bool isTimelineSelected) {
		ref Keyframe keyframe = ref this[keyframeIndex];
		float rotation = keyframeIndex == updateKeyframeAnimIndex ? new KeyframeData.SinEaseBoth().ModifyProgress(updateKeyframeAnim.Abs(out int animDir)) * -TwoThirdsPi * animDir : 0;
		Vector2 pos = Animation.timelinePos with { X = Animation.TimelineToScreenPos(keyframe.Time) };
		keyframeTriangle.ResetPositions().Rotate(rotation).Translate(pos);
		keyframeTriangle.FillColor(keyframeIndex == draggingKeyframeIndex ? Color.Lime : KeyframeColor);
		if (keyframe.Time <= currentlyDrawingTime) keyframeTriangle.MultiplyColor(Color.Gray);
		if (isKeyframeSelected) {
			using (keyframeTriangle.ModificationContext()) {
				keyframeTriangle.Scale(2f, keyframeTriangle.TransformedOrigin).Draw(KeyframeAnimation.primitiveBatch);
				keyframeTriangle.Scale(0.8f, keyframeTriangle.TransformedOrigin).FillColor(Color.Black).Rotate(rotation * -2, keyframeTriangle.TransformedOrigin).Draw(KeyframeAnimation.primitiveBatch);
			}
		}
		keyframeTriangle.Draw(KeyframeAnimation.primitiveBatch);
		return isTimelineSelected && keyframeTriangle.Contains(Main.MouseScreen);
	}
	protected override void DrawKeyframeDragTarget(SpriteBatch spriteBatch, Vector2 position) =>
		keyframeTriangle.TranslateTo(position).DrawOutline(KeyframeAnimation.primitiveBatch);
	protected override void DrawKeyframeEditor(SpriteBatch spriteBatch, int keyframeIndex, ref GizmoTracker gizmoTracker) {
		DrawSidebar(spriteBatch, keyframeIndex, this[keyframeIndex].Time);
	}
	protected override void DrawKeyframeCreationSidebar(SpriteBatch spriteBatch, float time) {
		DrawSidebar(spriteBatch, -1, time);
	}
	void DrawSidebar(SpriteBatch spriteBatch, int keyframeIndex, float time) {
		bool addedEvent = false;
		float width = -1;
		Vector2 iPos = new((Main.screenWidth + KeyframeAnimation.TimelineWidth) * 0.5f + 8, 4);
		TextSnippet[] snippets = [
			new()
		];
		for (int i = options.Count - 1; i >= 0; i--) Max(ref width, FontAssets.ItemStack.Value.MeasureString(options[i].Method.Name).X);
		width += 18;
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			iPos - Vector2.One * 2,
			new(0, 0, (int)width + 4, options.Count * 16 + 4),
			new Color(90, 90, 105)
		);
		for (int i = 0; i < options.Count; i++) {
			Event option = options[i];
			snippets[0].Text = option.Method.Name;
			snippets[0].Color = keyframeIndex >= 0 && option == this[keyframeIndex].Event ? Color.Goldenrod : Color.White;
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
			if (Main.mouseLeft && Main.mouseLeftRelease && hoveredModifier != -1 && addedEvent.TrySet(true)) {
				Main.mouseLeftRelease = false;
				if (keyframeIndex == -1) Insert(time, option);
				else Do(new SetKeyframeEvent(this, keyframeIndex, option));
			}
			iPos.Y += 16;
		}
	}
	public override string Export(string indent) {
		StringBuilder builder = new("[");
		for (int i = 0; i < keyframes.Count; i++) {
			builder.Append("\n\t");
			builder.Append(indent);
			builder.Append(keyframes[i].Export());
			if (i + 1 < keyframes.Count) builder.Append(',');
			else builder.Append('\n');
		}
		builder.Append(indent);
		builder.Append(']');
		return builder.ToString();
	}
	public override string ExportType() => $"EventKeyframeSet<{typeof(TParams).Name}>";
	protected override void OnBind() {
		options = [];
		foreach (MethodInfo meth in Animation.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Static)) {
			if (meth.ReturnType != typeof(void)) continue;
			ParameterInfo[] parameters = meth.GetParameters();
			if (parameters.Length != 1) continue;
			if (!parameters[0].ParameterType.IsByRef) continue;
			if (!parameters[0].Attributes.HasFlag(ParameterAttributes.In)) continue;
			options.Add(meth.CreateDelegate<Event>());
		}
	}
	class SetKeyframeEvent(EventKeyframeSet<TParams> set, int index, Event value) : KeyframeVariableStep<Event>(set, index, value) {
		public override string Text => $"Set Keyframe Event: {newValue}";
		protected override ref Event Variable => ref set[index].Event;
	}
	const float TwoThirdsPi = MathHelper.TwoPi / 3;
	static readonly Polygon keyframeTriangle = new(
		5 * Vector2.UnitY,
		5 * Vector2.UnitY.RotatedBy(TwoThirdsPi),
		5 * Vector2.UnitY.RotatedBy(-TwoThirdsPi)
	);
}
public class EnumKeyframeSet<TEnum> : KeyframeSet<AnimatableEnum<TEnum>> where TEnum : struct, Enum {
	public new TEnum CalculateValue(float time) => base.CalculateValue(time);
	public new TEnum GetCurrentValue(float time) => base.GetCurrentValue(time);
	public EnumKeyframeSet() : base() { }
	public EnumKeyframeSet(TEnum startValue, AGizmoOffset gizmoOffset = null) : this(startValue, (false, null), gizmoOffset) { }
	public EnumKeyframeSet(TEnum startValue, (bool looped, InterpolationStack interpolation) loopData, AGizmoOffset gizmoOffset = null) : base(startValue, loopData, gizmoOffset) { }
	protected override void OnBind() {
		base.OnBind();
		ITypeHandler.EnsureLoaded<AnimatableEnum<TEnum>>();
	}
}
public abstract class AKeyframeSet<TKeyframe> : IKeyframeSet, IEnumerable<TKeyframe> where TKeyframe : IKeyframe<TKeyframe> {
	protected float lastProcessedTime = -1;
	protected int updateKeyframeAnimIndex = -1;
	protected float updateKeyframeAnim = 0;
	float draggingKeyframeOffset;
	protected int draggingKeyframeIndex = -1;
	float draggingKeyframeTo = -1;
	public bool looped;
	public List<TKeyframe> keyframes = [];
	public ref TKeyframe this[int index] => ref CollectionsMarshal.AsSpan(keyframes)[index];
	public float Duration => keyframes.Count == 0 ? 0 : keyframes[^1].Time;
	public KeyframeAnimation Animation { get; private set; }
	public void Add(TKeyframe keyframe) => keyframes.Add(keyframe);
	public void RemoveAtIndex(int index) {
		keyframes.RemoveAt(index);
		if (updateKeyframeAnimIndex == index) updateKeyframeAnimIndex = -1;
	}
	protected void AnimateKeyframeUpdate(int i, int dir = 1) {
		updateKeyframeAnimIndex = i;
		updateKeyframeAnim = dir;
	}
	protected virtual int KeyframeUpdateAnimationFrames => 20;
	public void DrawEditorUI(SpriteBatch spriteBatch, SelectionType isSelected, float currentTime) {
		if (isSelected.IsSelected()) {
			if (Keybindings.EditUndo.JustPressed) Undo();
			else if (Keybindings.EditRedo.JustPressed) Redo();
			else if (CanLoop && Keybindings.LoopTimeline.JustPressed) Do(new SetLooped(this, !looped));
			else if (Keybindings.DeleteAllKeyframes.JustPressed && this.keyframes.Count > 0) Do(new DeleteAllKeyframes(this));
		}

		if (history.Length != DebugConfig.Instance.HistoryLength) history = new(DebugConfig.Instance.HistoryLength);
		if (future.Length != DebugConfig.Instance.HistoryLength) future = new(DebugConfig.Instance.HistoryLength);

		if (updateKeyframeAnimIndex != -1 && MathUtils.LinearSmoothing(ref updateKeyframeAnim, 0, 1f / KeyframeUpdateAnimationFrames)) updateKeyframeAnimIndex = -1;
		Span<TKeyframe> keyframes = CollectionsMarshal.AsSpan(this.keyframes);
		bool showCreationSidebar = isSelected.IsSelected();

		int deleteIndex = -1;
		PreDrawKeyframes(spriteBatch, isSelected, currentTime);
		for (int i = 0; i < keyframes.Length; i++) {
			ref TKeyframe keyframe = ref keyframes[i];
			Vector2 pos = Animation.timelinePos with { X = Animation.TimelineToScreenPos(keyframe.Time) };
			bool isKeyframeSelected = isSelected.IsSelected() && keyframe.Time == currentTime;
			bool isKeyframeHovered = DrawKeyframe(spriteBatch, i, pos, isKeyframeSelected, isSelected.IsSelected());
			if (isSelected.IsSelected() && keyframe.Time == currentTime) {
				showCreationSidebar = false;
				if (isSelected.ShowSidebars()) DrawKeyframeEditor(spriteBatch, i, ref Animation.gizmoTracker);
				if (Keybindings.InsertKeyframe.JustPressed) TryUpdateKeyframe(i);
				if (isSelected.CanCopy() && Keybindings.Copy.JustPressed) {
					clipboardKeyframe = keyframe.CleanMutableRefs();
				} else if (isSelected.CanCopy() && Keybindings.Cut.JustPressed) {
					clipboardKeyframe = keyframe.CleanMutableRefs();
					deleteIndex = i;
				} else if (Keybindings.DeleteKeyframe.JustPressed) {
					deleteIndex = i;
				}
			}
			if (isSelected.IsSelected() && Main.mouseLeft && Main.mouseLeftRelease && isKeyframeHovered) {
				draggingKeyframeIndex = i;
				draggingKeyframeOffset = Main.mouseX - pos.X;
			}
			if (i == draggingKeyframeIndex) DrawKeyframeDragTarget(spriteBatch, pos with { X = Animation.TimelineToScreenPos(draggingKeyframeTo) });
		}

		if (currentTime == 0 && isSelected.CanCopy() && Keybindings.Copy.JustPressed && CopyStart(out TKeyframe startKeyframe)) {
			clipboardKeyframe = startKeyframe.CleanMutableRefs();
		}
		if (showCreationSidebar) {
			if (isSelected.ShowSidebars()) DrawKeyframeCreationSidebar(spriteBatch, currentTime);
			if (Keybindings.InsertKeyframe.JustPressed) TryInsertKeyframe(currentTime);
			if (Keybindings.Paste.JustPressed && GetInsertIndex(currentTime) is int insertIndex) {
				TryPasteKeyframe(currentTime, insertIndex);
			}
		}
		if (deleteIndex != -1) Do(new DeleteKeyframe(this, deleteIndex));
		HandleGizmos();
		if (draggingKeyframeIndex != -1) {
			draggingKeyframeTo = Animation.ScreenPosToTimeline(Main.mouseX + draggingKeyframeOffset);
			if (!Animation.usesSubframes) draggingKeyframeTo = float.Round(draggingKeyframeTo);
			if (!Main.mouseLeft) {
				int newIndex = 0;
				for (int i = 0; i < keyframes.Length; i++) {
					if (i == draggingKeyframeIndex) continue;
					if (draggingKeyframeTo < keyframes[i].Time) break;
					if (draggingKeyframeTo == keyframes[i].Time) return;
					newIndex++;
				}
				Do(new MoveKeyframe(this, draggingKeyframeIndex, keyframes[draggingKeyframeIndex].Time, newIndex, draggingKeyframeTo));
				draggingKeyframeIndex = -1;
			}
		}
	}
	protected abstract void PreDrawKeyframes(SpriteBatch spriteBatch, SelectionType isSelected, float currentTime);
	protected abstract bool DrawKeyframe(SpriteBatch spriteBatch, int keyframeIndex, Vector2 position, bool isKeyframeSelected, bool isTimelineSelected);
	protected abstract void DrawKeyframeDragTarget(SpriteBatch spriteBatch, Vector2 position);
	protected abstract void DrawKeyframeEditor(SpriteBatch spriteBatch, int keyframeIndex, ref GizmoTracker gizmoTracker);
	protected virtual bool CopyStart(out TKeyframe keyframe) {
		keyframe = default;
		return false;
	}
	protected virtual void TryUpdateKeyframe(int keyframeIndex) { }
	protected virtual void TryInsertKeyframe(float time) { }
	protected virtual void TryPasteKeyframe(float currentTime, int insertIndex) {
		clipboardKeyframe.Time = currentTime;
		Do(new CreateKeyframe(this, insertIndex, clipboardKeyframe));
	}
	protected abstract void DrawKeyframeCreationSidebar(SpriteBatch spriteBatch, float time);
	protected virtual void HandleGizmos() { }
	public virtual void HandleMultiSelect(IKeyframeSet primary) { }
	public abstract string Export(string indent);
	public virtual string ExportType() => GetType().Name;
	public float PrevKeyframeTime(float currentTime) {
		for (int i = keyframes.Count - 1; i >= 0; i--) {
			TKeyframe keyframe = keyframes[i];
			if (currentTime > keyframe.Time) return keyframe.Time;
		}
		return float.NaN;
	}
	public float NextKeyframeTime(float currentTime) {
		for (int i = 0; i < keyframes.Count; i++) {
			TKeyframe keyframe = keyframes[i];
			if (currentTime < keyframe.Time) return keyframe.Time;
		}
		return float.NaN;
	}
	public int? GetInsertIndex(float time) {
		for (int i = 0; i < keyframes.Count; i++) {
			switch (float.Sign(keyframes[i].Time - time)) {
				case 0:
				return null;
				case 1:
				return i;
			}
		}
		return keyframes.Count;
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
	protected static TKeyframe clipboardKeyframe;
	UndoStep.StepBuffer history = new(10);
	UndoStep.StepBuffer future = new(10);
	protected class CreateKeyframe(AKeyframeSet<TKeyframe> set, int index, TKeyframe keyframe) : UndoStep {
		public override string Text => $"Create Keyframe: {keyframe}";
		public override void Do() {
			set.keyframes.Insert(index, keyframe.CleanMutableRefs());
			set.AnimateKeyframeUpdate(index);
		}
		public override void Undo() => set.RemoveAtIndex(index);
	}
	protected class MoveKeyframe(AKeyframeSet<TKeyframe> set, int fromIndex, float from, int toIndex, float to) : UndoStep {
		public override string Text => $"Move Keyframe from {from} to {to}";
		public override void Do() => Move(fromIndex, toIndex, to);
		public override void Undo() => Move(toIndex, fromIndex, from);
		void Move(int fromIndex, int toIndex, float to) {
			if (fromIndex == toIndex) set[fromIndex].Time = to;
			else {
				Span<TKeyframe> keyframes = CollectionsMarshal.AsSpan(set.keyframes);
				TKeyframe keyframe = keyframes[fromIndex];
				keyframe.Time = to;
				int copyStart = fromIndex;
				int copyEnd = toIndex;
				MinMax(ref copyStart, ref copyEnd);
				int copyLength = copyEnd - copyStart;
				if (fromIndex < toIndex) {
					keyframes.Slice(copyStart + 1, copyLength).CopyTo(keyframes.Slice(copyStart, copyLength));
					keyframes[toIndex] = keyframe;
				} else {
					keyframes.Slice(copyStart, copyLength).CopyTo(keyframes.Slice(copyStart + 1, copyLength));
					keyframes[toIndex] = keyframe;
				}
			}
		}
	}
	protected class DeleteKeyframe(AKeyframeSet<TKeyframe> set, int index) : UndoStep {
		readonly TKeyframe keyframe = set[index];
		public override string Text => $"Delete Keyframe: {keyframe}";
		public override void Do() => set.RemoveAtIndex(index);
		public override void Undo() => set.keyframes.Insert(index, keyframe.CleanMutableRefs());
	}
	protected class DeleteAllKeyframes(AKeyframeSet<TKeyframe> set) : UndoStep {
		readonly List<TKeyframe> oldValue = set.keyframes;
		public override string Text => $"Delete All Keyframes";
		public override void Do() => set.keyframes = [];
		public override void Undo() => set.keyframes = oldValue;
	}
	protected abstract class KeyframeVariableStep<TVar>(AKeyframeSet<TKeyframe> set, int index, TVar value) : VariableUndoStep<TVar>(value) {
		readonly AKeyframeSet<TKeyframe> set = set;
		protected readonly int index = index;
		public override string Text => $"Set Keyframe Value: {newValue}";
		public override void Do() {
			base.Do();
			set.AnimateKeyframeUpdate(index);
		}
		public override void Undo() {
			base.Undo();
			set.AnimateKeyframeUpdate(index, -1);
		}
	}
	protected class SetLooped(AKeyframeSet<TKeyframe> set, bool loop) : UndoStep {
		readonly bool oldValue = set.looped;
		public override string Text => $"Set Timeline Loop: {loop}";
		public override void Do() => set.looped = loop;
		public override void Undo() => set.looped = oldValue;
	}

	IEnumerator<TKeyframe> IEnumerable<TKeyframe>.GetEnumerator() => keyframes.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)keyframes).GetEnumerator();
	public void Bind(KeyframeAnimation animation) {
		Animation = animation;
		OnBind();
	}
	protected virtual void OnBind() { }
	public virtual bool CanLoop => false;
	public bool IsLooped => looped;
}
public interface IKeyframeSet {
	public float Duration { get; }
	public void DrawEditorUI(SpriteBatch spriteBatch, SelectionType isSelected, float currentTime);
	public float PrevKeyframeTime(float currentTime);
	public float NextKeyframeTime(float currentTime);
	public void HandleMultiSelect(IKeyframeSet primary) { }
	public string Export(string indent);
	public string ExportType();
	public void Bind(KeyframeAnimation animation) { }
	public bool CanLoop { get; }
	public bool IsLooped { get; }
}
public interface IKeyframe<TSelf> where TSelf : IKeyframe<TSelf> {
	public float Time { get; set; }
	public TSelf CleanMutableRefs();
}
public interface IInterpolationModifier : IMustBeStruct, IAutoload<IInterpolationModifier.Loader>, IEquatable<IInterpolationModifier> {
	public float ModifyProgress(float progress);
	public string Export() => $"new {GetType().Name}()";
	public void DrawGizmo(SpriteBatch spriteBatch, Vector2 pos, ref GizmoTracker gizmoTracker) { }
	public IInterpolationModifier Clone() => this;
	public virtual static IInterpolationModifier CreateDefault() => null;
	bool IEquatable<IInterpolationModifier>.Equals(IInterpolationModifier other) => GetType() == other.GetType();
	class Loader : IAutoloader {
		static readonly MethodInfo getCustomDefault = typeof(Loader).GetMethod("GetCustomDefault");
		static void IAutoloader.Autoload(Mod mod, Type type) {
			Func<IInterpolationModifier> func = getCustomDefault.MakeGenericMethod(type).Invoke(null, []) as Func<IInterpolationModifier> ?? CreateDefault(type);
			KeyframeData.modifierTypes.Add((func().ToString(), func));
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
		public static Func<IInterpolationModifier> GetCustomDefault<T>() where T : IInterpolationModifier {
			if (((Delegate)T.CreateDefault).Method.DeclaringType == typeof(IInterpolationModifier)) return null;
			return T.CreateDefault;
		}
	}
}
public interface IInterpolationModifier<TSelf> : IInterpolationModifier where TSelf : struct, IInterpolationModifier<TSelf>, IEquatable<TSelf> {
	bool IEquatable<IInterpolationModifier>.Equals(IInterpolationModifier other) => other is TSelf o && ((IEquatable<TSelf>)this).Equals(o);
	IInterpolationModifier IInterpolationModifier.Clone() => (TSelf)this;
}
public static class KeyframeTypes {
	public readonly struct StepInterpolation<T> : KeyframeSet<T>.IInterpolation where T : IEquatable<T> {
		public bool IsModifiable => false;
		public readonly T Interpolate(T prevValue, T nextValue, float progress) => prevValue;
		public readonly override string ToString() => "Stepped";
		public static implicit operator StepInterpolation<T>(KeyframeData.StepInterpolationToken token) => new();
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
		public static void DrawGizmo(SpriteBatch spriteBatch, ref Vector2 value, ref GizmoTracker gizmoTracker, GizmoOffset offset) {
			Vector2 scale = GizmoZoom;
			if (gizmoTracker.IsCurrent && Main.MouseScreen != oldMousePos) {
				Vector2 mult = Vector2.One;
				if (Main.keyState.PressingControl()) mult.X = 0;
				else if (Main.keyState.PressingShift()) mult.Y = 0;
				value += (Main.MouseScreen - oldMousePos) / scale * mult;
			}
			Rectangle rect = new Rectangle(0, 0, (int)(16 * scale.X), (int)(16 * scale.Y)).Recentered(ApplyGizmoZoom(offset.Position));
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
		public static void DrawGizmo(SpriteBatch spriteBatch, ref bool value, ref GizmoTracker gizmoTracker, GizmoOffset offset) {
			Vector2 zoomedPos = ApplyGizmoZoom(offset.Position);
			handle.ResetVertices().Scale(8).Rotate(MathHelper.PiOver4).Translate(zoomedPos);
			bool isHovering = handle.Contains(Main.MouseScreen);

			handle.MultiplyColor(0.5f + isHovering.Mul(0.5f)).DrawOutline();
			if (value) handle.Scale(6f / 8, zoomedPos).Draw(KeyframeAnimation.primitiveBatch);

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
	public static void DrawRotationGizmo(SpriteBatch spriteBatch, ref float value, ref GizmoTracker gizmoTracker, GizmoOffset offset, float size) {
		float scale = GizmoZoom.X;
		Vector2 pos = ApplyGizmoZoom(offset.Position);
		pos = pos.Floor();
		if (gizmoTracker.IsCurrent && Main.MouseScreen != oldMousePos) value = (Main.MouseScreen - pos).ToRotation() + rotationHandleOffset;
		float offsetValue = offset.Rotation;
		Vector2 end = (pos + (offsetValue - MathHelper.PiOver2).ToRotationVector2() * (size * scale)).Floor();
		spriteBatch.DrawLine(Color.White, pos + Main.screenPosition, end + Main.screenPosition);

		rotationHandle.ResetPositions().Scale(4 * scale).Rotate(offsetValue).Translate(end);
		bool isHovering = rotationHandle.Contains(Main.MouseScreen) || gizmoTracker.IsCurrent;

		rotationHandle.ResetColors().MultiplyColor(0.5f + isHovering.Mul(0.5f)).Draw(KeyframeAnimation.primitiveBatch);
		Main.pixelShader.CurrentTechnique.Passes[0].Apply();

		if (gizmoTracker.CheckSetCurrent(isHovering, Keybindings.RotateGizmo)) {
			rotationHandleOffset = value - (Main.MouseScreen - pos).ToRotation();
		}
		gizmoTracker.Advance();
	}
	static float scaleHandleStart;
	static float scaleHandleOffset;
	public static void DrawScaleGizmo(SpriteBatch spriteBatch, ref float value, ref GizmoTracker gizmoTracker, GizmoOffset offset, float size) {
		const float handle_thickness = 2;
		Vector2 pos = ApplyGizmoZoom(offset.Position);
		float dist = (Main.MouseScreen - pos).Length();
		float displayScale = 1;
		if (gizmoTracker.IsCurrent) {
			displayScale = dist / scaleHandleOffset;
			if (Main.MouseScreen != oldMousePos) value = scaleHandleStart * displayScale;
		}

		bool isHovering = gizmoTracker.IsCurrent || dist >= size - handle_thickness && dist <= size + handle_thickness;
		ShaderCircle.Draw(pos, size * displayScale + handle_thickness, Color.White * (0.5f + isHovering.Mul(0.5f)), size * displayScale - handle_thickness);

		if (gizmoTracker.CheckSetCurrent(isHovering, Keybindings.ScaleGizmo)) {
			scaleHandleStart = value;
			scaleHandleOffset = dist;
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
	public static Vector2 ApplyGizmoZoom(Vector2 pos) => (pos - Main.ScreenSize.ToVector2() * 0.5f) * GizmoZoom + Main.ScreenSize.ToVector2() * 0.5f;
	public static Vector2 oldMousePos;
	public static BoneIterator<TBoneChain> Iterate<TBoneChain>(ref this TBoneChain self) where TBoneChain : struct, IBoneChain<TBoneChain>, IEquatable<TBoneChain> {
		return new(ref self, default, default);
	}
	public ref struct BoneIterator<TBoneChain>(ref TBoneChain self, Vector2 startPosition, float startAngle) where TBoneChain : struct, IBoneChain<TBoneChain>, IEquatable<TBoneChain> {
		readonly ref TBoneChain self = ref self;
#pragma warning disable CS9124 // Parameter is captured into the state of the enclosing type and its value is also used to initialize a field, property, or event.
		Vector2 position = startPosition;
		float angle = startAngle;
#pragma warning restore CS9124 // Parameter is captured into the state of the enclosing type and its value is also used to initialize a field, property, or event.
		float lastLength;
		int i = 0;
		public readonly (Vector2 position, float length, float angle) Current => (position, lastLength, angle);
		public readonly void Dispose() { }
		public bool MoveNext() {
			if (i >= TBoneChain.Length) return false;
			PolarVec2 bone = self[i];
			lastLength = bone.R;
			bone.Theta += angle;
			position += (Vector2)bone;
			angle = bone.Theta;
			i++;
			return true;
		}
		public void Reset() {
			position = startPosition;
			angle = startAngle;
			i = 0;
		}
		public readonly BoneIterator<TBoneChain> GetEnumerator() => this;
	}
}
public static class KeyframeData {
	public static List<(string name, Func<IInterpolationModifier>)> modifierTypes = [];
	public record struct Exponential(float Exponent = 2) : IInterpolationModifier<Exponential> {
		public readonly float ModifyProgress(float progress) => float.Pow(progress, Exponent);
		public readonly override string ToString() => Exponent == 0 ? "Exponent" : $"Exponent({Exponent})";
		readonly string IInterpolationModifier.Export() => $"new {nameof(Exponential)}({Exponent}f)";
		static IInterpolationModifier IInterpolationModifier.CreateDefault() => new Exponential(2);
		static readonly Polygon handle = new(
			new(-1, 0),
			new(0, -1),
			new(1, 0),
			new(0, 1)
		);
		static Vector2 handleOffset;
		static float handleX = 0.5f;
		static Vector2 lastMouse;
		void IInterpolationModifier.DrawGizmo(SpriteBatch spriteBatch, Vector2 pos, ref GizmoTracker gizmoTracker) {
			Vector2 mouse = (Main.MouseScreen - pos) / 150;
			float handleY = 1 - float.Pow(handleX, Exponent);
			handle.ResetPositions().Scale(6).Translate(pos + new Vector2(handleX, handleY) * 150);
			bool isHovering = handle.Contains(Main.MouseScreen) || gizmoTracker.IsCurrent;
			handle.ResetColors().MultiplyColor(0.5f + isHovering.Mul(0.5f)).Draw(KeyframeAnimation.primitiveBatch);
			if (gizmoTracker.IsCurrent && lastMouse.TrySet(mouse)) {
				handleX = mouse.X + handleOffset.X;
				Clamp(ref handleX, float.Epsilon, float.BitDecrement(1));
				Exponent = float.Max(float.Log(1 - float.Clamp(mouse.Y - handleOffset.Y, float.Epsilon, float.BitDecrement(1)), handleX), float.Epsilon);
			}
			if (Main.mouseLeft && Main.mouseLeftRelease && gizmoTracker.CheckSetCurrent(isHovering, null)) {
				handleOffset = new Vector2(handleX, handleY) - mouse;
			}
			gizmoTracker.Advance();
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
	public static readonly StepInterpolationToken Stepped = default;
	public struct StepInterpolationToken;
}
public struct GizmoTracker() {
	int usingGizmo = -1;
	int currentIndex;
	bool usingMouseDown;
	public bool JustSelected { readonly get; private set; }
	public readonly bool IsCurrent => usingGizmo == currentIndex;
	public readonly bool IsPrevious => usingGizmo == currentIndex - 1;
	public readonly bool IsInRange(int minInclusive, int maxExclusive) => usingGizmo >= minInclusive && usingGizmo < maxExclusive;
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
	bool IEquatable<TSelf>.Equals(TSelf other) => (TSelf)this == other;
	static KeyframeSet<TSelf>.IInterpolation KeyframeSet<TSelf>.ITypeHandler.Linear { get; } = new Interpolation();
	public readonly struct Interpolation : KeyframeSet<TSelf>.IInterpolation {
		readonly TSelf KeyframeSet<TSelf>.IInterpolation.Interpolate(TSelf prevValue, TSelf nextValue, float progress) =>
			prevValue with { Frame = (int)float.Round(float.Lerp(prevValue.Frame, prevValue.Frame, progress)) };
		public readonly override string ToString() => "Interpolated";
	}
	static void KeyframeSet<TSelf>.ITypeHandler.DrawGizmo(SpriteBatch spriteBatch, ref TSelf value, ref GizmoTracker gizmoTracker, GizmoOffset offset) {
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
			Main.LocalPlayer.mouseInterface = true;
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
public struct AnimatableEnum<TEnum> : IAnimatableEnum<AnimatableEnum<TEnum>, TEnum> where TEnum : struct, Enum {
	public TEnum value;
	public static bool operator ==(AnimatableEnum<TEnum> left, AnimatableEnum<TEnum> right) => AreEqual(left, right);
	public static bool operator !=(AnimatableEnum<TEnum> left, AnimatableEnum<TEnum> right) => !AreEqual(left, right);
	public static implicit operator AnimatableEnum<TEnum>(TEnum value) => Unsafe.BitCast<TEnum, AnimatableEnum<TEnum>>(value);
	public static implicit operator TEnum(AnimatableEnum<TEnum> value) => Unsafe.BitCast<AnimatableEnum<TEnum>, TEnum>(value);
	public override readonly bool Equals(object obj) => obj is AnimatableEnum<TEnum> other && value == other;
	public override int GetHashCode() => value.GetHashCode();
	static readonly Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
	static bool AreEqual(TEnum a, TEnum b) => AsULong(a) == AsULong(b);
	static ulong AsULong(TEnum value) {
		if (underlyingType == typeof(sbyte) || underlyingType == typeof(byte)) return Unsafe.BitCast<TEnum, byte>(value);
		else if (underlyingType == typeof(short) || underlyingType == typeof(ushort)) return Unsafe.BitCast<TEnum, ushort>(value);
		else if (underlyingType == typeof(int) || underlyingType == typeof(uint)) return Unsafe.BitCast<TEnum, uint>(value);
		else if (underlyingType == typeof(long) || underlyingType == typeof(ulong)) return Unsafe.BitCast<TEnum, ulong>(value);
		else throw new InvalidOperationException($"Unsupported enum underlying type: {underlyingType}");
	}
}
public interface IAnimatableEnum<TSelf, TEnum> : KeyframeSet<TSelf>.ITypeHandler, IEquatable<TSelf>, IEqualityOperators<TSelf, TSelf, bool> where TSelf : struct, IAnimatableEnum<TSelf, TEnum> where TEnum : struct, Enum {
	static TEnum[] values = Enum.GetValues<TEnum>();
	static DescriptionAttribute[] descriptions = typeof(TEnum)
		.GetFields(BindingFlags.Public | BindingFlags.Static)
		.Where(field => field.IsLiteral)
		.Select(field => field.GetCustomAttribute<DescriptionAttribute>()).ToArray();
	bool IEquatable<TSelf>.Equals(TSelf other) => (TSelf)this == other;
	static bool KeyframeSet<TSelf>.ITypeHandler.Interpolatable => false;
	static KeyframeSet<TSelf>.IInterpolation KeyframeSet<TSelf>.ITypeHandler.Linear => null;
	static readonly TextSnippet[] snippet = [new()];
	static void KeyframeSet<TSelf>.ITypeHandler.DrawGizmo(SpriteBatch spriteBatch, ref TSelf value, ref GizmoTracker gizmoTracker, GizmoOffset offset) {
		Vector2 pos = new(0, Main.screenHeight);
		int hoveredIndex = -1;
		float maxHeight = 0;
		for (int i = 0; i < values.Length; i++) {
			TEnum current = values[i];
			snippet[0].Text = current.ToString();
			snippet[0].Color = (TSelf)current == value ? Color.Goldenrod : Color.White;
			Vector2 size = ChatManager.GetStringSize(FontAssets.MouseText.Value, snippet, Vector2.One);
			if (pos.X + size.X > Main.screenWidth) {
				pos.X = 0;
				pos.Y -= maxHeight + 4;
				maxHeight = 0;
			}
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.MouseText.Value,
				snippet,
				pos - size * Vector2.UnitY,
				0,
				Vector2.Zero,
				Vector2.One,
				out int isHovered
			);
			if (isHovered != -1) hoveredIndex = i;
			pos.X += size.X + 4;
		}
		if (hoveredIndex != -1) {
			Main.LocalPlayer.mouseInterface = true;
			var aaaaa = typeof(TEnum).GetFields();
			if (descriptions[hoveredIndex] is DescriptionAttribute { Description: string description }) Main.instance.MouseText(description);
			if (Main.mouseLeft && Main.mouseLeftRelease) value = values[hoveredIndex];
		}
	}
	static string KeyframeSet<TSelf>.ITypeHandler.Export(TSelf value) => $"{typeof(TEnum).Name}.{(TEnum)value}";
	public static abstract implicit operator TSelf(TEnum value);
	public static abstract implicit operator TEnum(TSelf value);
}
[AttributeUsage(AttributeTargets.Field)]
public sealed class DescriptionAttribute(string description) : Attribute {
	public readonly string Description = description ?? throw new ArgumentException($"{nameof(description)} may not be null", nameof(description));
}
public record struct PosRotScale(Vector2 Position, float Rotation, float Scale) : KeyframeSet<PosRotScale>.ITypeHandler, IKeyframeOffsetModifier {
	public PosRotScale() : this(default, 0, 1) { }
	public Vector2 Position = Position;
	public float Rotation = Rotation;
	public float Scale = Scale;
	public static KeyframeSet<PosRotScale>.IInterpolation Linear { get; } = new Interpolation();
	public readonly struct Interpolation : KeyframeSet<PosRotScale>.IInterpolation {
		readonly PosRotScale KeyframeSet<PosRotScale>.IInterpolation.Interpolate(PosRotScale prevValue, PosRotScale nextValue, float progress) =>
			new(
				Vector2.Lerp(prevValue.Position, nextValue.Position, progress),
				prevValue.Rotation + GeometryUtils.AngleDif(prevValue.Rotation, nextValue.Rotation, out int dir) * dir * progress,
				float.Lerp(prevValue.Scale, nextValue.Scale, progress)
			);
		public readonly override string ToString() => "Interpolated";
	}
	public static PosRotScale operator +(PosRotScale a, PosRotScale b) => new(a.Position + b.Position.RotatedBy(a.Rotation) * a.Scale, a.Rotation + b.Rotation, a.Scale * b.Scale);
	public static PosRotScale operator +(PosRotScale a, Vector2 b) => new(a.Position + b.RotatedBy(a.Rotation) * a.Scale, a.Rotation, a.Scale);
	public static PosRotScale operator +(Vector2 a, PosRotScale b) => new(a + b.Position, b.Rotation, b.Scale);
	public static void DrawGizmo(SpriteBatch spriteBatch, ref PosRotScale value, ref GizmoTracker gizmoTracker, GizmoOffset offset) {
		Vec2Interpolation.DrawGizmo(spriteBatch, ref value.Position, ref gizmoTracker, offset);
		DrawRotationGizmo(spriteBatch, ref value.Rotation, ref gizmoTracker, offset, 32);
		DrawScaleGizmo(spriteBatch, ref value.Scale, ref gizmoTracker, offset, 20);
	}
	public readonly void ModifyGizmoOffset(ref GizmoOffset gizmoOffset) => gizmoOffset += new GizmoOffset(Position, Rotation);
	public static string Export(PosRotScale value) => value == new PosRotScale() ? "new()" : $"new({Vec2Interpolation.Export(value.Position)}, {value.Rotation}f, {value.Scale}f)";
}
public record struct RotScale(float Rotation, float Scale) : KeyframeSet<RotScale>.ITypeHandler, IKeyframeOffsetModifier {
	public RotScale() : this(0, 1) { }
	public float Rotation = Rotation;
	public float Scale = Scale;
	public static KeyframeSet<RotScale>.IInterpolation Linear { get; } = new Interpolation();
	public readonly struct Interpolation : KeyframeSet<RotScale>.IInterpolation {
		readonly RotScale KeyframeSet<RotScale>.IInterpolation.Interpolate(RotScale prevValue, RotScale nextValue, float progress) =>
			new(
				prevValue.Rotation + GeometryUtils.AngleDif(prevValue.Rotation, nextValue.Rotation, out int dir) * dir * progress,
				float.Lerp(prevValue.Scale, nextValue.Scale, progress)
			);
		public readonly override string ToString() => "Interpolated";
	}
	public static PosRotScale operator +(RotScale a, PosRotScale b) => new(b.Position.RotatedBy(a.Rotation) * a.Scale, a.Rotation + b.Rotation, a.Scale * b.Scale);
	public static PosRotScale operator +(RotScale a, Vector2 b) => new(b.RotatedBy(a.Rotation) * a.Scale, a.Rotation, a.Scale);
	public static void DrawGizmo(SpriteBatch spriteBatch, ref RotScale value, ref GizmoTracker gizmoTracker, GizmoOffset offset) {
		DrawRotationGizmo(spriteBatch, ref value.Rotation, ref gizmoTracker, offset, 32);
		DrawScaleGizmo(spriteBatch, ref value.Scale, ref gizmoTracker, offset, 20);
	}
	public readonly void ModifyGizmoOffset(ref GizmoOffset gizmoOffset) => gizmoOffset += new GizmoOffset(default, Rotation);
	public static string Export(RotScale value) => value == new RotScale() ? "new()" : $"new({value.Rotation}f, {value.Scale}f)";
}
public record struct Percentage(float Value) : KeyframeSet<Percentage>.ITypeHandler {
	public Percentage() : this(1) { }
	public float Value = Value;
	public static KeyframeSet<Percentage>.IInterpolation Linear { get; } = new Interpolation();
	public readonly struct Interpolation : KeyframeSet<Percentage>.IInterpolation {
		readonly Percentage KeyframeSet<Percentage>.IInterpolation.Interpolate(Percentage prevValue, Percentage nextValue, float progress) =>
			new(float.Lerp(prevValue.Value, nextValue.Value, progress));
		public readonly override string ToString() => "Interpolated";
	}
	public static Percentage operator -(Percentage a, float b) => new(a.Value - b);
	public static Percentage operator +(Percentage a, float b) => new(a.Value + b);
	public static Percentage operator *(Percentage a, float b) => new(a.Value * b);
	public static Percentage operator /(Percentage a, float b) => new(a.Value / b);
	public static implicit operator Percentage(float value) => new(value);
	public static implicit operator float(Percentage value) => value.Value;
	public static Color operator *(Color a, Percentage b) => a * b.Value;
	public static void DrawGizmo(SpriteBatch spriteBatch, ref Percentage value, ref GizmoTracker gizmoTracker, GizmoOffset offset) {
		const int height = 48;
		Vector2 scale = GizmoZoom;
		if (gizmoTracker.IsCurrent && Main.mouseY != oldMousePos.Y) {
			value = float.Clamp((offset.Position.Y - Main.mouseY) * 0.5f / height + 0.5f, 0, 1);
		}
		Rectangle rect = new Rectangle(0, 0, (int)(16 * scale.X), (int)(8 * scale.Y)).Recentered(((1 - value) * 2 - 1) * scale * Vector2.UnitY * height + offset.Position);
		bool isHovering = rect.Contains(Main.MouseScreen) || gizmoTracker.IsCurrent;
		Color color = Color.White;
		if (!isHovering) color *= 0.5f;
		Main.spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			offset.Position,
			new Rectangle(0, 0, 2, 2),
			color,
			0,
			Vector2.One,
			new Vector2(2, height),
			0,
		0);
		Main.spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			rect,
			color
		);
		gizmoTracker.CheckSetCurrent(isHovering, Keybindings.GrabGizmo);
		gizmoTracker.Advance();
	}
	public static string Export(Percentage value) => $"{value.Value}f";
}
public record struct Rotation(float Value) : KeyframeSet<Rotation>.ITypeHandler, IKeyframeOffsetModifier {
	public float Value = Value;
	public static KeyframeSet<Rotation>.IInterpolation Linear { get; } = new Interpolation();
	public readonly struct Interpolation : KeyframeSet<Rotation>.IInterpolation {
		readonly Rotation KeyframeSet<Rotation>.IInterpolation.Interpolate(Rotation prevValue, Rotation nextValue, float progress) =>
			new(float.Lerp(prevValue.Value, nextValue.Value, progress));
		public readonly override string ToString() => "Interpolated";
	}
	public static Rotation operator -(Rotation a, float b) => new(a.Value - b);
	public static Rotation operator +(Rotation a, float b) => new(a.Value + b);
	public static Rotation operator *(Rotation a, float b) => new(a.Value * b);
	public static Rotation operator /(Rotation a, float b) => new(a.Value / b);
	public static implicit operator Rotation(float value) => new(value);
	public static implicit operator float(Rotation value) => value.Value;
	public static void DrawGizmo(SpriteBatch spriteBatch, ref Rotation value, ref GizmoTracker gizmoTracker, GizmoOffset offset) {
		DrawRotationGizmo(spriteBatch, ref value.Value, ref gizmoTracker, offset, 32);
	}
	public readonly void ModifyGizmoOffset(ref GizmoOffset gizmoOffset) => gizmoOffset += new GizmoOffset(default, this);
	public static string Export(Rotation value) => $"{value.Value}f";
}
public interface IBoneChain<TSelf> : KeyframeSet<TSelf>.ITypeHandler, IKeyframeOffsetModifier where TSelf : struct, IBoneChain<TSelf>, IEquatable<TSelf> {
	public abstract static int Length { get; }
	public virtual static bool Stretchable => false;
	public abstract ref PolarVec2 this[int index] { [UnscopedRef] get; }
	static KeyframeSet<TSelf>.IInterpolation KeyframeSet<TSelf>.ITypeHandler.Linear { get; } = new Interpolation();
	public readonly struct Interpolation : KeyframeSet<TSelf>.IInterpolation {
		readonly TSelf KeyframeSet<TSelf>.IInterpolation.Interpolate(TSelf prevValue, TSelf nextValue, float progress) {
			for (int i = 0; i < TSelf.Length; i++) {
				ref PolarVec2 output = ref prevValue[i];
				PolarVec2 next = nextValue[i];
				output.Theta = float.Lerp(output.Theta, next.Theta, progress);
				output.R = float.Lerp(output.R, next.R, progress);
			}
			return prevValue;
		}
		public readonly override string ToString() => "Interpolated";
	}
	static readonly Polygon boneHandle = new(
		new(-1, -1),
		new(1, -1),
		new(1, 1),
		new(-1, 1)
	);
	static readonly Polygon boneShape = new(
		new(-1, 0),
		new(-0.8f, -1),
		new(0, 0),
		new(-0.8f, 1)
	);
	static PolarVec2 boneOriginal;
	static float lengthOffset;
	static float rotationOffset;
	static void KeyframeSet<TSelf>.ITypeHandler.DrawGizmo(SpriteBatch spriteBatch, ref TSelf value, ref GizmoTracker gizmoTracker, GizmoOffset offset) {
		float scale = GizmoZoom.X;
		for (int i = TSelf.Length - 1; i >= 0; i--) {
			ref PolarVec2 bone = ref value[i];
			Vector2 pos = ApplyGizmoZoom(offset.Position);
			float offsetRotation = offset.Rotation;
			offset -= new GizmoOffset(bone.Theta.ToRotationVector2() * bone.R, bone.Theta);
			Vector2 basePos = ApplyGizmoZoom(offset.Position);
			if (gizmoTracker.IsCurrent && Main.MouseScreen != oldMousePos) {
				if (Main.keyState.PressingShift()) bone.Theta = boneOriginal.Theta;
				else bone.Theta = (Main.MouseScreen - basePos).ToRotation() + rotationOffset;
				if (TSelf.Stretchable) {
					if (Main.keyState.PressingControl()) bone.R = boneOriginal.R;
					else bone.R = Vector2.Dot(Main.MouseScreen - basePos, (bone.Theta + MathHelper.PiOver2).ToRotationVector2()) + lengthOffset;
				}
			}
			boneShape.ResetVertices().Scale(new Vector2(bone.R, 2 + Math.Clamp(bone.R * 0.1f - 1, 0, 6)) * scale).Rotate(offsetRotation).Translate(pos).Draw(KeyframeAnimation.primitiveBatch);

			boneHandle.ResetPositions().Scale(4 * scale).Rotate(offsetRotation).Translate(pos);
			bool isHovering = boneHandle.Contains(Main.MouseScreen) || gizmoTracker.IsCurrent;

			boneHandle.ResetColors().MultiplyColor(0.5f + isHovering.Mul(0.5f)).Draw(KeyframeAnimation.primitiveBatch);
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();

			if (gizmoTracker.CheckSetCurrent(isHovering, isHovering ? Keybindings.GrabGizmo : null)) {
				boneOriginal = bone;
				rotationOffset = bone.Theta - (Main.MouseScreen - basePos).ToRotation();
				lengthOffset = bone.R - Vector2.Dot(Main.MouseScreen - basePos, (bone.Theta + MathHelper.PiOver2).ToRotationVector2());
			}
			gizmoTracker.Advance();
		}
	}
	void IKeyframeOffsetModifier.ModifyGizmoOffset(ref GizmoOffset gizmoOffset) {
		for (int i = 0; i < TSelf.Length; i++) gizmoOffset += new GizmoOffset(this[i].Theta.ToRotationVector2() * this[i].R, this[i].Theta);
	}
	static string KeyframeSet<TSelf>.ITypeHandler.Export(TSelf value) {
		StringBuilder builder = new();
		builder.Append("new(");
		for (int i = 0; i < TSelf.Length; i++) {
			if (i > 0) builder.Append(", ");
			builder.Append($"new({value[i].R}f, {value[i].Theta}f)");
		}
		builder.Append(')');
		return builder.ToString();
	}
}
public record struct GizmoOffset(Vector2 Position, float Rotation) {
	public static implicit operator GizmoOffset(Vector2 position) => new(position, 0);
	public static GizmoOffset operator +(GizmoOffset a, GizmoOffset b) => new(a.Position + b.Position.RotatedBy(a.Rotation), a.Rotation + b.Rotation);
	public static GizmoOffset operator -(GizmoOffset a, GizmoOffset b) => new(a.Position - b.Position.RotatedBy(a.Rotation - b.Rotation), a.Rotation - b.Rotation);
	public static GizmoOffset operator +(Vector2 a, GizmoOffset b) => new(a + b.Position, b.Rotation);
	public static Vector2 operator -(Vector2 a, GizmoOffset b) => (a - b.Position).RotatedBy(-b.Rotation);
}
public abstract class AGizmoOffset {
	public abstract GizmoOffset GetOffset(float time);
	public abstract string Export();
	public virtual void Bind(KeyframeAnimation Animation) { }
	public static implicit operator AGizmoOffset(Vector2 value) => new VectorOffset(value);
	public static implicit operator AGizmoOffset(GizmoOffset value) => new ConstantOffset(value);
}
public class VectorOffset(Vector2 offset) : AGizmoOffset {
	public override GizmoOffset GetOffset(float time) => new(offset, 0);
	public override string Export() => $"new Vector2({offset.X}f, {offset.Y}f)";
}
public class ConstantOffset(GizmoOffset offset) : AGizmoOffset {
	public override GizmoOffset GetOffset(float time) => offset;
	public override string Export() => $"new GizmoOffset({Vec2Interpolation.Export(offset.Position)}, {Rotation.Export(offset.Rotation)})";
}
public class ChildOfOffset<TValue>(string parentTimelineName, GizmoOffset offset = default) : AGizmoOffset where TValue : IEquatable<TValue> {
	KeyframeSet<TValue> parentTimeline;
	public override GizmoOffset GetOffset(float time) => parentTimeline.GizmoOffset(time) + offset;
	public override void Bind(KeyframeAnimation Animation) {
		parentTimeline = (KeyframeSet<TValue>)Animation.GetByName(parentTimelineName);
	}
	public override string Export() => $"new ChildOfOffset<{typeof(TValue).Name}>(\"{parentTimelineName}\"{(offset == default ? "" : $", new({Vec2Interpolation.Export(offset.Position)}, {Rotation.Export(offset.Rotation)})")})";
}
public class OffsetChildOf<TValue>(string parentTimelineName, GizmoOffset offset) : AGizmoOffset where TValue : IEquatable<TValue> {
	KeyframeSet<TValue> parentTimeline;
	public override GizmoOffset GetOffset(float time) => offset with { Rotation = 0 } + parentTimeline.GizmoOffset(time);
	public override void Bind(KeyframeAnimation Animation) {
		parentTimeline = (KeyframeSet<TValue>)Animation.GetByName(parentTimelineName);
	}
	public override string Export() => $"new OffsetChildOf<{typeof(TValue).Name}>(\"{parentTimelineName}\", new({Vec2Interpolation.Export(offset.Position)}, {Rotation.Export(offset.Rotation)}))";
}
public interface IKeyframeOffsetModifier {
	public void ModifyGizmoOffset(ref GizmoOffset gizmoOffset);
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
public class CombinedUndoStep(string text, params UndoStep[] steps) : UndoStep {
	public override string Text => text;
	public override void Do() {
		for (int i = 0; i < steps.Length; i++) steps[i].Do();
	}
	public override void Undo() {
		for (int i = steps.Length - 1; i >= 0; i--) steps[i].Undo();
	}
}
public abstract class VariableUndoStep<T> : UndoStep {
	protected readonly T oldValue;
	protected readonly T newValue;
	protected VariableUndoStep(T newValue) {
		this.newValue = newValue;
		oldValue = Variable;
	}
	protected abstract ref T Variable { get; }
	public override void Do() => Variable = newValue;
	public override void Undo() => Variable = oldValue;
}