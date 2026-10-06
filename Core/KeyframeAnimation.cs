using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics.Primitives;
using PegasusLib.Graphics;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace Origins.Core; 
public abstract class KeyframeAnimation {
	public static int TimelineWidth => 600;
	public Rectangle CurrentTimeline => new Rectangle(0, 0, TimelineWidth, 12).Recentered(timelinePos);
	public int selectedIndex = 0;
	public Vector2 timelinePos;
	public float totalLength;
	public Vector2 gizmoBasePosition;
	public bool usesSubframes;
	readonly List<IKeyframeSet> keyframeSets;
	readonly List<TextSnippet[]> keyframeSetNames;
	float lastTime;
	public int modifyingInterpolation = -1;
	public bool draggingInterpolation = false;
	public GizmoTracker gizmoTracker = new();
	protected KeyframeAnimation() {
		keyframeSets = [];
		keyframeSetNames = [];
		foreach (FieldInfo field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)) {
			if (!field.FieldType.IsAssignableTo(typeof(IKeyframeSet))) continue;
			IKeyframeSet item = (IKeyframeSet)field.GetValue(this);
			keyframeSets.Add(item);
			keyframeSetNames.Add([new(field.Name)]);
			Max(ref totalLength, item.Duration);
		}
	}
	public void DrawEditorUI(SpriteBatch spriteBatch, ref float currentTime, ref PlayingState animationControls) => DrawEditorUI(spriteBatch, new(ref currentTime), ref animationControls);
	public void DrawEditorUI(SpriteBatch spriteBatch, ref int currentTime, ref PlayingState animationControls) => DrawEditorUI(spriteBatch, new(ref currentTime), ref animationControls);
	public void DrawEditorUI(SpriteBatch spriteBatch, Time currentTime, ref PlayingState animationControls) {
		gizmoTracker.ResetCurrent();
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
			if (DrawButton(buttonPos, "Next Keyframe", out buttonColor)) {
				float nextKeyframe = keyframeSets[selectedIndex].NextKeyframeTime(currentTime);
				if (!float.IsNaN(nextKeyframe)) currentTime.Value = nextKeyframe;
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
			if (DrawButton(buttonPos, "Previous Keyframe", out buttonColor)) {
				float nextKeyframe = keyframeSets[selectedIndex].PrevKeyframeTime(currentTime);
				if (!float.IsNaN(nextKeyframe)) currentTime.Value = nextKeyframe;
			}
			playTriangle.Scale(new Vector2(-1, 1), playTriangle.TransformedOrigin).TranslateTo(buttonPos + new Vector2(icon_scale * 0.85f, 0)).FillColor(buttonColor).Draw(primitiveBatch);
			keyframeDiamond.FillColor(buttonColor).TranslateTo(buttonPos - new Vector2(icon_size * 0.4f, 0)).Draw(primitiveBatch);

			// go to start
			buttonPos = timelinePos - buttonOffset * 2;
			if (DrawButton(buttonPos, "Restart", out buttonColor)) {
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
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.ItemStack.Value,
				keyframeSetNames[i],
				timelinePos - new Vector2(TimelineWidth * 0.5f + 4, 0) - ChatManager.GetStringSize(FontAssets.ItemStack.Value, keyframeSetNames[i], Vector2.One) * new Vector2(1, 0.5f),
				0,
				Vector2.Zero,
				Vector2.One,
				out int hoveredSnippet
			);
			if (hoveredSnippet != -1) {
				Main.LocalPlayer.mouseInterface = true;
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					selectedIndex = i;
					modifyingInterpolation = -1;
				}
			}
			DrawTimeline(spriteBatch, i == selectedIndex);
			timelinePos.Y += 16;
		}
		Rectangle extendAnimationHandle = new((int)((Main.screenWidth + TimelineWidth) * 0.5f), minTimelineY.Y - 2, 2, (int)(timelinePos.Y - minTimelineY.Y - 2));
		bool hoverExtend = extendAnimationHandle.Contains(Main.MouseScreen);
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			extendAnimationHandle,
			hoverExtend || gizmoTracker.IsCurrent ? Color.Orange : Color.Orange * 0.5f
		);
		gizmoTracker.CheckSetCurrent(hoverExtend, null);
		if (gizmoTracker.IsCurrent) {
			float diff = extendAnimationHandle.Contains(Main.MouseScreen) ? 0 : (Main.mouseX - extendAnimationHandle.Center().X);
			if (diff != 0) {
				totalLength += diff * (totalLength / TimelineWidth) / 30f;
				if (diff < 0) {
					float minLength = 1;
					for (int i = 0; i < keyframeSets.Count; i++) Max(ref minLength, keyframeSets[i].Duration);
					Max(ref totalLength, minLength);
				}
			}
		} else if (CurrentTimeline.Including(minTimelineY).Contains(Main.MouseScreen)) {
			Main.LocalPlayer.mouseInterface = true;
			if (Main.mouseLeft) currentTime.Value = (int)float.Round(ScreenPosToTimeline(Main.mouseX));
		}
		gizmoTracker.Advance();
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			new Rectangle((int)TimelineToScreenPos(currentTime) - 1, minTimelineY.Y - 2, 2, (int)(timelinePos.Y - minTimelineY.Y - 2)),
			Color.White
		);
		timelinePos.Y = 32;
		for (int i = 0; i < keyframeSets.Count; i++) {
			keyframeSets[i].DrawEditorUI(spriteBatch, this, i == selectedIndex, currentTime);
			timelinePos.Y += 16;
		}
		spriteBatch.Restart(spriteBatch.GetState());
		Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
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
			return hovered && Main.mouseLeft && Main.mouseLeftRelease;
		}
	}
	public void DrawTimeline(SpriteBatch spriteBatch, bool isSelected) {
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			CurrentTimeline,
			Color.Gray * (isSelected ? 1 : 0.5f)
		);
	}
	public float TimelineToScreenPos(float time) => Main.screenWidth * 0.5f + TimelineWidth * (time / totalLength - 0.5f);
	public float ScreenPosToTimeline(float x) => ((x - Main.screenWidth * 0.5f) / TimelineWidth + 0.5f) * totalLength;
	public string Export() {
		StringBuilder builder = new();
		for (int i = 0; i < keyframeSets.Count; i++) {
			builder.Append("\tpublic ");
			builder.Append(keyframeSets[i].ExportType());
			builder.Append(' ');
			builder.Append(keyframeSetNames[i][0].Text);
			builder.Append(" = ");
			builder.Append(keyframeSets[i].Export());
			builder.Append(';');
			builder.Append('\n');
		}
		return builder.ToString();
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
	}

	const float button_scale = 10;
	const float half_bri_base = 0.8660254f;
	const float icon_scale = button_scale / (half_bri_base * 2);
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
}