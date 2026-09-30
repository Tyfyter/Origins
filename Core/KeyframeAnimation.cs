using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Reflection;
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

	protected KeyframeAnimation() {
		keyframeSets = [];
		keyframeSetNames = [];
		foreach (FieldInfo field in GetType().GetFields()) {
			if (!field.FieldType.IsAssignableTo(typeof(IKeyframeSet))) continue;
			IKeyframeSet item = (IKeyframeSet)field.GetValue(this);
			keyframeSets.Add(item);
			keyframeSetNames.Add([new(field.Name)]);
			Max(ref totalLength, item.Duration);
		}
	}
	public void DrawEditorUI(SpriteBatch spriteBatch, ref int currentTime) {
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
			if (hoveredSnippet != -1 && Main.mouseLeft && Main.mouseLeftRelease) selectedIndex = i;
			if (Main.mouseLeft && CurrentTimeline.Contains(Main.MouseScreen)) {
				currentTime = (int)float.Round(ScreenPosToTimeline(Main.MouseScreen.X));
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
	}
	public void DrawEditorUI(SpriteBatch spriteBatch, ref float currentTime) {
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
			if (hoveredSnippet != -1 && Main.mouseLeft && Main.mouseLeftRelease) selectedIndex = i;
			if (Main.mouseLeft && CurrentTimeline.Contains(Main.MouseScreen)) {
				currentTime = ScreenPosToTimeline(Main.MouseScreen.X);
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
}
