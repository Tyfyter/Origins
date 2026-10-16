using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Graphics.Primitives;
using Origins.UI;
using PegasusLib.Graphics;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace Origins.Core.Animation; 
public abstract class KeyframeAnimation {
	public static int TimelineWidth => 600;
	static float leftBound;
	public Rectangle CurrentTimeline => new Rectangle(0, 0, TimelineWidth, 12).Recentered(timelinePos);
	public int selectedIndex = 0;
	public Vector2 timelinePos;
	public float MinimumLength { get => field; set => Max(ref totalLength, field = value); } = 1;
	public float totalLength;
	public Vector2 gizmoBasePosition;
	public bool usesSubframes;
	Func<KeyframeAnimation, bool> needsReset;
	readonly List<IKeyframeSet> keyframeSets;
	readonly List<string> keyframeSetNames;
	readonly bool[] selected;
	bool exportAsCtor = false;
	static readonly TextSnippet[] snippet = [new()];
	float lastTime;
	public int modifyingInterpolation = -1;
	public bool draggingInterpolation = false;
	public GizmoTracker gizmoTracker = new();
	protected KeyframeAnimation() {
		keyframeSets = [];
		keyframeSetNames = [];
		Setup();
		selected = new bool[keyframeSets.Count];
	}

	private void Setup() {
		keyframeSets.Clear();
		keyframeSetNames.Clear();
		totalLength = MinimumLength;
		DynamicMethod method = default;
		ILGenerator gen = default;
		Label fail = default;
		if (needsReset is null) {
			method = new(nameof(needsReset), typeof(bool), [typeof(KeyframeAnimation)], true);
			gen = method.GetILGenerator();
			gen.DeclareLocal(GetType());
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Castclass, GetType());
			gen.Emit(OpCodes.Stloc_0);
			gen.Emit(OpCodes.Ldc_I4_1);
			fail = gen.DefineLabel();
		}
		foreach (FieldInfo field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)) {
			if (!field.FieldType.IsAssignableTo(typeof(IKeyframeSet))) continue;
			if (field.GetCustomAttribute<ShowIfTrueAttribute>()?.ShouldShow(this) == false) continue;
			if (needsReset is null) {
				gen.Emit(OpCodes.Ldloc_0);
				gen.Emit(OpCodes.Ldfld, field);
				gen.Emit(OpCodes.Ldloc_0);
				gen.Emit(OpCodes.Ldfld, typeof(KeyframeAnimation).GetField(nameof(keyframeSets), BindingFlags.NonPublic | BindingFlags.Instance));
				gen.Emit(OpCodes.Ldc_I4, keyframeSets.Count);
				gen.Emit(OpCodes.Callvirt, typeof(List<IKeyframeSet>).GetMethod("get_Item"));
				gen.Emit(OpCodes.Bne_Un, fail);
			}
			IKeyframeSet item = (IKeyframeSet)field.GetValue(this);
			keyframeSets.Add(item);
			keyframeSetNames.Add(field.Name);
			item.Bind(this);
			Max(ref totalLength, item.Duration);
			exportAsCtor |= field.FieldType.DeclaringType != GetType();
		}
		if (needsReset is null) {
			gen.Emit(OpCodes.Pop);
			gen.Emit(OpCodes.Ldc_I4_0);
			gen.MarkLabel(fail);
			gen.Emit(OpCodes.Ret);
			needsReset = method.CreateDelegate<Func<KeyframeAnimation, bool>>();
		}
	}

	public string GetName(IKeyframeSet keyframeSet) => keyframeSetNames[keyframeSets.IndexOf(keyframeSet)];
	public IKeyframeSet GetByName(string name) => keyframeSets[keyframeSetNames.IndexOf(name)];
	public void DrawEditorUI(SpriteBatch spriteBatch, ref float currentTime, ref PlayingState animationControls) => DrawEditorUI(spriteBatch, new(ref currentTime), ref animationControls);
	public void DrawEditorUI(SpriteBatch spriteBatch, ref int currentTime, ref PlayingState animationControls) => DrawEditorUI(spriteBatch, new(ref currentTime), ref animationControls);
	public void DrawEditorUI(SpriteBatch spriteBatch, Time currentTime, ref PlayingState animationControls) {
		if (needsReset is not null) {
			if (needsReset(this)) Setup();
			needsReset = null;
		}
		leftBound = float.PositiveInfinity;
		gizmoTracker.ResetCurrent();
		float hoveredTime = ScreenPosToTimeline(Main.mouseX);
		if (hoveredTime > totalLength) {
			if (usesSubframes) hoveredTime = totalLength;
			else hoveredTime = (int)totalLength;
		}
		float hoveredTimeQuantized = usesSubframes ? hoveredTime : float.Round(hoveredTime);
		timelinePos.X = Main.screenWidth * 0.5f;
		timelinePos.Y = 12;
		{
			const float icon_size = icon_scale * 2;
			const float button_size = button_scale * 2;
			Vector2 buttonOffset = new(button_size + 2, 0);
			buttonRect.ResetPositions();
			playTriangle.ResetPositions();
			keyframeDiamond.ResetPositions();
			pauseRect.ResetPositions();
			loopSymbol.ResetPositions();
			copyRect.ResetPositions();
			Vector2 buttonPos = timelinePos;
			// pause/play
			if (DrawButton(buttonPos, animationControls.playing ? "Pause" : "Play", out Color buttonColor)) animationControls.playing = !animationControls.playing;
			if (animationControls.playing) {
				pauseRect.FillColor(buttonColor).TranslateTo(buttonPos + new Vector2(icon_size * -0.333f, 0)).Draw(primitiveBatch);
				pauseRect.Translate(new Vector2(icon_size * 0.666f, 0)).Draw(primitiveBatch);
			} else {
				playTriangle.TranslateTo(buttonPos).FillColor(buttonColor).Draw(primitiveBatch);
			}

			// go to next keyframe
			buttonPos = timelinePos + buttonOffset;
			if (DrawButton(buttonPos, "Next Keyframe", out buttonColor) || Keybindings.NextKeyframeAnimation.JustPressed) {
				float nextKeyframe = keyframeSets[selectedIndex].NextKeyframeTime(currentTime);
				if (!float.IsNaN(nextKeyframe)) currentTime.Value = nextKeyframe;
				else currentTime.Value = totalLength;
			}
			playTriangle.Scale(0.95f, playTriangle.TransformedOrigin).TranslateTo(buttonPos - new Vector2(icon_scale * 0.85f, 0)).FillColor(buttonColor).Draw(primitiveBatch);
			keyframeDiamond.Scale(0.75f).FillColor(buttonColor).TranslateTo(buttonPos + new Vector2(icon_size * 0.4f, 0)).Draw(primitiveBatch);

			// loop
			buttonPos = timelinePos + buttonOffset * 2;
			if (DrawButtonColored(buttonPos, "Loop", animationControls.forceLoop ? Color.CornflowerBlue : Color.Gray, out buttonColor)) {
				animationControls.forceLoop = !animationControls.forceLoop;
			}
			loopSymbol.TranslateTo(buttonPos).FillColor(buttonColor).Draw(primitiveBatch);

			// go to last keyframe
			buttonPos = timelinePos - buttonOffset;
			if (DrawButton(buttonPos, "Previous Keyframe", out buttonColor) || Keybindings.PreviousKeyframeAnimation.JustPressed) {
				float nextKeyframe = keyframeSets[selectedIndex].PrevKeyframeTime(currentTime);
				if (!float.IsNaN(nextKeyframe)) currentTime.Value = nextKeyframe;
				else currentTime.Value = 0;
			}
			playTriangle.Scale(new Vector2(-1, 1), playTriangle.TransformedOrigin).TranslateTo(buttonPos + new Vector2(icon_scale * 0.85f, 0)).FillColor(buttonColor).Draw(primitiveBatch);
			keyframeDiamond.FillColor(buttonColor).TranslateTo(buttonPos - new Vector2(icon_size * 0.4f, 0)).Draw(primitiveBatch);

			// go to start
			buttonPos = timelinePos - buttonOffset * 2;
			if (DrawButton(buttonPos, "Restart", out buttonColor) || Keybindings.RestartAnimation.JustPressed) {
				currentTime.Value = 0;
			}
			playTriangle.Translate(-buttonOffset - new Vector2(icon_size * 0.1f, 0)).FillColor(buttonColor).Draw(primitiveBatch);
			pauseRect.Scale(0.85f).FillColor(buttonColor).TranslateTo(buttonPos - new Vector2(icon_size * 0.4f, 0)).Draw(primitiveBatch);

			// export to clipboard
			buttonPos = timelinePos + (new Vector2(TimelineWidth, 0) - buttonOffset) * 0.5f;
			if (DrawButton(buttonPos, "Export To Clipboard", out buttonColor)) {
				Platform.Get<IClipboard>().Value = Export();
			}
			copyRect.TranslateTo(buttonPos - new Vector2(button_size * 0.05f)).FillColor(buttonColor).Draw(primitiveBatch);
			copyRect.Translate(new Vector2(button_size * 0.075f)).FillColor(buttonColor.MultiplyRGB(Color.Gray)).Draw(primitiveBatch);
			copyRect.Translate(new Vector2(button_size * 0.025f)).FillColor(buttonColor).Draw(primitiveBatch);
		}
		timelinePos.Y += 20;
		Point minTimelineY = CurrentTimeline.TopLeft().ToPoint();
		for (int i = 0; i < keyframeSets.Count; i++) {
			snippet[0].Text = keyframeSetNames[i];
			Vector2 pos = timelinePos - new Vector2(TimelineWidth * 0.5f + 4, 0) - ChatManager.GetStringSize(FontAssets.ItemStack.Value, keyframeSetNames[i], Vector2.One) * new Vector2(1, 0.5f);
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.ItemStack.Value,
				snippet,
				pos,
				0,
				Vector2.Zero,
				Vector2.One,
				out int hoveredSnippet
			);
			Min(ref leftBound, pos.X);
			if (hoveredSnippet != -1) {
				Main.LocalPlayer.mouseInterface = true;
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					if (keyframeSets[i].GetType() != keyframeSets.GetIfInRange(selectedIndex)?.GetType() || !Main.keyState.PressingShift()) Array.Clear(selected);
					selectedIndex = i;
					selected[i] = true;
					modifyingInterpolation = -1;
				}
			}
			DrawTimeline(spriteBatch, GetSelectionType(i));
			timelinePos.Y += 16;
		}
		Rectangle extendAnimationHandle = new((int)((Main.screenWidth + TimelineWidth) * 0.5f), minTimelineY.Y - 2, 2, (int)(timelinePos.Y - minTimelineY.Y - 2));
		bool hoverExtend = extendAnimationHandle.Contains(Main.MouseScreen);
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			extendAnimationHandle,
			hoverExtend || gizmoTracker.IsCurrent ? Color.Orange : Color.Orange * 0.5f
		);
		DrawString(spriteBatch, usesSubframes ? $"{totalLength:0.0}" : $"{(int)totalLength}", new((Main.screenWidth + TimelineWidth) * 0.5f, timelinePos.Y), origin: Vector2.UnitX);
		gizmoTracker.CheckSetCurrent(hoverExtend, null);
		bool isTimelineHovered = CurrentTimeline.Including(minTimelineY).Contains(Main.MouseScreen);
		bool hoveringTimelineControl = hoverExtend;
		if (gizmoTracker.IsCurrent) {
			float diff = extendAnimationHandle.Contains(Main.MouseScreen) ? 0 : Main.mouseX - extendAnimationHandle.Center().X;
			if (diff != 0) {
				totalLength += diff * (totalLength / TimelineWidth) / 30f;
				if (diff < 0) {
					float minLength = 1;
					for (int i = 0; i < keyframeSets.Count; i++) Max(ref minLength, keyframeSets[i].Duration);
					Max(ref totalLength, minLength);
				}
			}
		}
		gizmoTracker.Advance();

		if (animationControls.loopStart.HasValue) {
			extendAnimationHandle = new((int)TimelineToScreenPos(animationControls.loopStart.Value) - 2, minTimelineY.Y - 2, 4, (int)(timelinePos.Y - minTimelineY.Y - 2));
			hoverExtend = gizmoTracker.IsCurrent || !hoveringTimelineControl && extendAnimationHandle.Contains(Main.MouseScreen);
			Color bluePortal = new(64, 160, 255);
			if (!hoverExtend) bluePortal *= 0.5f;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				extendAnimationHandle,
				bluePortal
			);
			gizmoTracker.CheckSetCurrent(hoverExtend, null);
			hoveringTimelineControl |= hoverExtend;
			if (gizmoTracker.IsCurrent) animationControls.loopStart = hoveredTimeQuantized;
			else if (hoverExtend && Main.mouseRight && Main.mouseRightRelease) animationControls.loopStart = null;
			gizmoTracker.Advance();
		}
		if (animationControls.loopEnd.HasValue) {
			extendAnimationHandle = new((int)TimelineToScreenPos(animationControls.loopEnd.Value) - 2, minTimelineY.Y - 2, 4, (int)(timelinePos.Y - minTimelineY.Y - 2));
			hoverExtend = gizmoTracker.IsCurrent || !hoveringTimelineControl && extendAnimationHandle.Contains(Main.MouseScreen);
			Color orangePortal = new(255, 160, 32);
			if (!hoverExtend) orangePortal *= 0.5f;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				extendAnimationHandle,
				orangePortal
			);
			gizmoTracker.CheckSetCurrent(hoverExtend, null);
			hoveringTimelineControl |= hoverExtend;
			if (gizmoTracker.IsCurrent) animationControls.loopEnd = hoveredTimeQuantized;
			else if (hoverExtend && Main.mouseRight && Main.mouseRightRelease) {
				if (!animationControls.loopStart.HasValue) animationControls.loopStart = animationControls.loopEnd;
				animationControls.loopEnd = null;
			}
			gizmoTracker.Advance();
		}
		if (!hoveringTimelineControl && isTimelineHovered) {
			Main.LocalPlayer.mouseInterface = true;
			if (Main.mouseLeft) currentTime.Value = hoveredTimeQuantized;
		}
		if (!hoveringTimelineControl && isTimelineHovered && Main.mouseRight && Main.mouseRightRelease) {
			switch ((animationControls.loopStart, animationControls.loopEnd)) {
				case (null, null):
				animationControls.loopEnd = hoveredTimeQuantized;
				break;
				case (null, float):
				animationControls.loopStart = hoveredTimeQuantized;
				break;
				case (float, null):
				animationControls.loopEnd = hoveredTimeQuantized;
				break;
				case (float start, float):
				if (hoveredTimeQuantized < start) animationControls.loopStart = hoveredTimeQuantized;
				else animationControls.loopEnd = hoveredTimeQuantized;
				break;
			}
			if (animationControls.loopEnd.HasValue && animationControls.loopStart.HasValue && animationControls.loopStart > animationControls.loopEnd) Utils.Swap(ref animationControls.loopStart, ref animationControls.loopEnd);
		}
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			new Rectangle((int)TimelineToScreenPos(currentTime) - 1, minTimelineY.Y - 2, 2, (int)(timelinePos.Y - minTimelineY.Y - 2)),
			Color.White
		);
		timelinePos.Y = 32;
		for (int i = 0; i < keyframeSets.Count; i++) {
			keyframeSets[i].DrawEditorUI(spriteBatch, GetSelectionType(i), currentTime);
			timelinePos.Y += 16;
		}
		for (int i = 0; i < keyframeSets.Count; i++) {
			if (GetSelectionType(i) == SelectionType.Secondary) keyframeSets[i].HandleMultiSelect(keyframeSets[selectedIndex]);
		}
		spriteBatch.Restart(spriteBatch.GetState());
		Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
		Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		primitiveBatch.Flush();
		if (lastTime.TrySet(currentTime)) modifyingInterpolation = -1;
		if (!Main.mouseLeft) draggingInterpolation = false;
		if (modifyingInterpolation == -1) draggingInterpolation = false;
		gizmoTracker.Update();
		KeyframeTypes.oldMousePos = Main.MouseScreen;

		static bool DrawButton(Vector2 position, string tooltip, out Color color) => DrawButtonColored(position, tooltip, Color.Gray, out color);
		static bool DrawButtonColored(Vector2 position, string tooltip, Color baseColor, out Color color) {
			buttonRect.TranslateTo(position);
			bool hovered = buttonRect.Contains(Main.MouseScreen);
			color = Color.White;
			if (!hovered) color = Color.LightGray;
			else UICommon.TooltipMouseText(tooltip);
			buttonRect.FillColor(color.MultiplyRGBA(baseColor)).Draw(primitiveBatch);
			if (hovered) Main.LocalPlayer.mouseInterface = true;
			return hovered && Main.mouseLeft && Main.mouseLeftRelease;
		}
	}

	public void DrawTimeline(SpriteBatch spriteBatch, SelectionType isSelected) {
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			CurrentTimeline,
			Color.Gray * (isSelected.IsSelected() ? 1 : 0.5f)
		);
	}
	public float TimelineToScreenPos(float time) => Main.screenWidth * 0.5f + TimelineWidth * (time / totalLength - 0.5f);
	public float ScreenPosToTimeline(float x) => ((x - Main.screenWidth * 0.5f) / TimelineWidth + 0.5f) * totalLength;
	public string Export() {
		StringBuilder builder = new();
		bool isLooped = keyframeSets.Any(s => s.IsLooped);
		bool exportAsCtor = this.exportAsCtor || isLooped;
		for (int i = 0; i < keyframeSets.Count; i++) {
			if (!exportAsCtor) {
				builder.Append("\tpublic ");
				builder.Append(keyframeSets[i].ExportType());
				builder.Append(' ');
			}
			builder.Append(keyframeSetNames[i]);
			builder.Append(" = ");
			builder.Append(keyframeSets[i].Export(exportAsCtor ? "\t\t" : "\t"));
			builder.Append(';');
			if (i < keyframeSets.Count - 1) builder.Append('\n');
		}
		if (isLooped) {
			builder.Append('\n');
			builder.Append(nameof(MinimumLength));
			builder.Append(" = ");
			builder.Append(usesSubframes ? totalLength : float.Round(totalLength));
			builder.Append("f;");
		}
		return builder.ToString();
	}
	public static bool DrawSwapper<TAnimation>(SpriteBatch spriteBatch, IEnumerable<TAnimation> animations, out TAnimation selected, TAnimation current = null) where TAnimation : KeyframeAnimation {
		selected = null;
		float maxWidth = 0f;
		foreach (TAnimation animation in animations) Max(ref maxWidth, ChatManager.GetStringSize(FontAssets.ItemStack.Value, animation.ToString(), Vector2.One).X);
		Vector2 pos = new(leftBound - maxWidth - 16, 70);
		Min(ref leftBound, pos.X);
		Color lowColor = Color.White;
		Color lowShadowColor = Color.Black;
		Color highColor = Color.White;
		Color highShadowColor = Color.Black;
		if (current is not null) {
			lowColor *= 0.5f;
			lowShadowColor *= 0.5f;
		}
		foreach (TAnimation animation in animations) {
			if (DrawString(spriteBatch, animation.ToString(), pos, animation == current ? highColor : lowColor, animation == current ? highShadowColor : lowShadowColor)) {
				Main.LocalPlayer.mouseInterface = true;
				selected = animation;
			}
			pos.Y += 16;
		}
		if (Main.mouseLeft && Main.mouseLeftRelease && selected is not null) return true;
		selected = null;
		return false;
	}
	static bool DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color? color = null, Color? shadowColor = null, Vector2 origin = default) {
		snippet[0].Text = text;
		ChatManager.DrawColorCodedStringWithShadow(
			spriteBatch,
			FontAssets.ItemStack.Value,
			snippet,
			position - origin * ChatManager.GetStringSize(FontAssets.ItemStack.Value, snippet, Vector2.One),
			0,
			color ?? Color.White,
			shadowColor ?? Color.Black,
			default,
			Vector2.One,
			out int h
		);
		return h != -1;
	}
	public SelectionType GetSelectionType(int index) {
		if (index == selectedIndex) {
			if (selected.Count(true) <= 1) return SelectionType.Alone;
			return SelectionType.Primary;
		}
		if (selected[index]) return SelectionType.Secondary;
		return SelectionType.None;
	}
	[StructLayout(LayoutKind.Explicit, Pack = 0)]
	public readonly ref struct Time {
		[FieldOffset(0)]
		readonly ref float f;
		[FieldOffset(0)]
		readonly ref int i;
		[FieldOffset(8)]
		readonly Kind kind;
		public readonly float Value {
			get {
				switch (kind) {
					case Kind.Float:
					return f;
					case Kind.Int:
					return i;
					default:
					throw new InvalidOperationException();
				}
			}
			set {
				switch (kind) {
					case Kind.Float:
					f = value;
					break;
					case Kind.Int:
					i = (int)float.Round(value);
					break;
					default:
					throw new InvalidOperationException();
				}
			}
		}
		public Time(ref float value) {
			f = ref value;
			kind = Kind.Float;
		}
		public Time(ref int value) {
			i = ref value;
			kind = Kind.Int;
		}
		public static implicit operator float(Time value) => value.Value;
		enum Kind : byte {
			Invalid,
			Float,
			Int
		}
	}
	public struct PlayingState {
		public bool playing;
		public bool forceLoop;
		public bool? overrideAnimatorMode;
		public float? loopStart;
		public float? loopEnd;
		public void Play(Time time, float duration, float speed) {
			if (overrideAnimatorMode ?? DebugConfig.Instance.AnimatorMode) {
				if (Keybindings.PlayPauseAnimation.JustPressed) playing = !playing;
				if (Keybindings.LoopAnimation.JustPressed) forceLoop = !forceLoop;
				if (playing) {
					if (forceLoop) {
						speed = speed.Abs(out int dir);
						float start = loopStart ?? 0;
						duration = (loopEnd ?? duration) - start;
						float t = time.Value - start;
						switch (dir) {
							case 1:
							t.CycleUp(duration + 1, speed);
							break;
							case -1:
							t.CycleDownWithZero(duration, speed);
							break;
						}
						time.Value = t + start;
					} else {
						time.Value += speed;
					}
				}
			} else {
				time.Value += speed;
			}
		}
		public void Play(ref float time, KeyframeAnimation animation, float speed = 1) => Play(new Time(ref time), animation.totalLength, speed);
		public void Play(ref int time, KeyframeAnimation animation, float speed = 1) => Play(new Time(ref time), animation.totalLength, speed);
	}
	const float button_scale = 10;
	const float half_tri_base = 0.8660254f;
	const float icon_scale = button_scale / (half_tri_base * 2);
	static readonly Polygon buttonRect = new(
		new(-button_scale, -button_scale),
		new(button_scale, -button_scale),
		new(button_scale, button_scale),
		new(-button_scale, button_scale)
	);
	static readonly Polygon playTriangle = new(
		new(icon_scale, 0),
		new(icon_scale * -0.5f, 5f),
		new(icon_scale * -0.5f, -5f)
	);
	static readonly Polygon pauseRect = new(
		new(-icon_scale / 3, -icon_scale),
		new(icon_scale / 3, -icon_scale),
		new(icon_scale / 3, icon_scale),
		new(-icon_scale / 3, icon_scale)
	);
	static readonly Polygon keyframeDiamond = new(
		new(-icon_scale, 0),
		new(0, -icon_scale),
		new(icon_scale, 0),
		new(0, icon_scale)
	);
	static readonly Polygon loopSymbol = Polygon.Import(icon_scale * 2,
		Polygon.Alignment.Center,
		new(-icon_scale, 0),
		new(icon_scale * 0.5f, 5f),
		new(icon_scale * 0.25f, 0),
		new(icon_scale * 0.5f, -5f)
	);
	static readonly Polygon copyRect = new(
		new(-button_scale * 0.25f, -button_scale * 0.375f),
		new(button_scale * 0.25f, -button_scale * 0.375f),
		new(button_scale * 0.25f, button_scale * 0.375f),
		new(-button_scale * 0.25f, button_scale * 0.375f)
	);
	public static readonly Polygon.PrimitiveBatch primitiveBatch = new();
	[AttributeUsage(AttributeTargets.Field, Inherited = false)]
	protected sealed class ShowIfTrueAttribute(string propertyName) : Attribute {
		public bool ShouldShow(object instance) => (bool)instance.GetType().GetProperty(propertyName).GetValue(instance);
	}
}
public class Animation_Editor_UI : SwitchableUIState {
	static uint setTickCount;
	public static Action<SpriteBatch> DrawEditorUI {
		get {
			unchecked {
				if (setTickCount == OriginSystem.gameTickCount || setTickCount + 1 == OriginSystem.gameTickCount) return field;
			}
			return null;
		}
		set {
			if (!DebugConfig.Instance.AnimatorMode) return;
			field = value;
			setTickCount = OriginSystem.gameTickCount;
		}
	}
	public override void AddToList() => OriginSystem.Instance.DevToolHUD.AddState(this);
	public override bool IsActive() => DrawEditorUI is not null;
	public Animation_Editor_UI() : base() {
		OverrideSamplerState = SamplerState.PointClamp;
	}
	protected override void DrawSelf(SpriteBatch spriteBatch) => DrawEditorUI?.Invoke(spriteBatch);
}
public enum SelectionType {
	None,
	Alone,
	Primary,
	Secondary
}
public static class SelectionExt {
	public static bool IsSelected(this SelectionType selectionType) => selectionType is not SelectionType.None;
	public static bool ShowGizmos(this SelectionType selectionType) => selectionType is SelectionType.Alone or SelectionType.Primary;
	public static bool CanCopy(this SelectionType selectionType) => selectionType is SelectionType.Alone or SelectionType.Primary;
	public static bool ShowSidebars(this SelectionType selectionType) => selectionType is SelectionType.Alone;
}