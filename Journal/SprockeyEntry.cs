using Origins.Items.Mounts.Star_Soldier;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Journal; 
public abstract class SprockeyEntry : JournalEntry {
	public override DynamicSpriteFont FontOverride => Star_Soldier.Font;
	public override JournalSortIndex SortIndex => new("Wire_Tutorial", 100);
	public override void OnUnlock() {
		switch (Main.LocalPlayer.OriginPlayer().sprockeyHelpRate) {
			case UI.Snippets.Sprockey_Help_Rate_Handler.Options.Never:
			return;
		}
		string[] lines = FullText.Value.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		for (int i = 0; i < lines.Length; i++) {
			PopupText.NewText(new AdvancedPopupRequest() {
				Text = lines[i],
				DurationInFrames = 1200 * lines.Length,
				Color = new Color(255, 250, 242)
			}, Main.LocalPlayer.Top);
		}
	}
	public static void AddEntryOnUse<TEntry>(ModItem item) where TEntry : SprockeyEntry => AddEntryOnUse<TEntry>(item.Item);
	public static void AddEntryOnUse<TEntry>(Item item) where TEntry : SprockeyEntry => AddEntryOnUse<TEntry>(item.type);
	public static void AddEntryOnUse<TEntry>(int type) where TEntry : SprockeyEntry => AddJournalEntry<TEntry>(ref OriginsSets.Items.JournalEntriesOnUse[type]);
}
public class Any_Powerable_Entry : SprockeyEntry {
	public override JournalSortIndex SortIndex => base.SortIndex with { Part = 1 };
}
public class Transistor_Entry : SprockeyEntry { }
public class Delay_Component_Entry : SprockeyEntry { }
public class Logic_Components_Entry : SprockeyEntry { }
public class Radio_Component_Entry : SprockeyEntry { }
public class Edge_Detector_Entry : SprockeyEntry { }
public class Mechanical_Key_Node_Entry : SprockeyEntry { }
public class Gas_Generator_Entry : SprockeyEntry { }
public class Solar_Battery_Entry : SprockeyEntry { }
public class Wind_Turbine_Entry : SprockeyEntry { }
public class Wave_Energy_Converter_Entry : SprockeyEntry { }
public class White_Wire_Upgrade_Entry : SprockeyEntry { }
public class Logic_Component_Upgrade_Entry : SprockeyEntry { }
