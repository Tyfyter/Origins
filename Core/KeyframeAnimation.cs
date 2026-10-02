using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace Origins.Core; 
public abstract class KeyframeAnimation {
	public static int TimelineWidth => 600;
	public Rectangle CurrentTimeline => new Rectangle(0, 0, TimelineWidth, 8).Recentered(timelinePos);
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
	public int draggingGizmo = -1;
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
	public void DrawEditorUI(SpriteBatch spriteBatch, ref float currentTime) => DrawEditorUI(spriteBatch, new(ref currentTime));
	public void DrawEditorUI(SpriteBatch spriteBatch, ref int currentTime) => DrawEditorUI(spriteBatch, new(ref currentTime));
	public void DrawEditorUI(SpriteBatch spriteBatch, Time currentTime) {
		timelinePos.X = Main.screenWidth * 0.5f;
		timelinePos.Y = 16;
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
			if (hoveredSnippet != -1 && Main.mouseLeft && Main.mouseLeftRelease) {
				selectedIndex = i;
				modifyingInterpolation = -1;
			}
			if (Main.mouseLeft && CurrentTimeline.Contains(Main.MouseScreen)) {
				currentTime.Value = (int)float.Round(ScreenPosToTimeline(Main.MouseScreen.X));
			}
			DrawTimeline(spriteBatch, i == selectedIndex);
			timelinePos.Y += 16;
		}
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			new Rectangle((int)TimelineToScreenPos(currentTime) - 1, 0, 2, (int)timelinePos.Y),
			Color.White
		);
		timelinePos.Y = 16;
		for (int i = 0; i < keyframeSets.Count; i++) {
			keyframeSets[i].DrawEditorUI(spriteBatch, this, i == selectedIndex, currentTime);
			timelinePos.Y += 16;
		}
		if (lastTime.TrySet(currentTime)) {
			modifyingInterpolation = -1;
			draggingGizmo = -1;
		}
		if (!Main.mouseLeft) {
			draggingInterpolation = false;
			draggingGizmo = -1;
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
}
