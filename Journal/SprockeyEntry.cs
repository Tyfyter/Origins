using Origins.Items.Mounts.Star_Soldier;
using ReLogic.Graphics;
using System;
using Terraria;

namespace Origins.Journal; 
public abstract class SprockeyEntry : JournalEntry {
	public override DynamicSpriteFont FontOverride => Star_Soldier.Font;
	public override JournalSortIndex SortIndex => new("Wire_Tutorial", 1);
	public override void OnUnlock() {
		string[] lines = FullText.Value.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		for (int i = 0; i < lines.Length; i++) {
			PopupText.NewText(new AdvancedPopupRequest() {
				Text = lines[i],
				DurationInFrames = 1200 * lines.Length,
				Color = new Color(255, 250, 242)
			}, Main.LocalPlayer.Top);
		}
	}
}
