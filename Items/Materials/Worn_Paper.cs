using Origins.Journal;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Materials {
	public abstract class Worn_Paper : ModItem, IJournalEntrySource {
		public override string Texture => typeof(Worn_Paper).GetDefaultTMLName();
		public string PaperName => Name.Replace("Worn_Paper_", "");
		public override LocalizedText Tooltip => LocalizedText.Empty;
		public string EntryName => $"{Mod.Name}/{PaperName}";
		[field: CloneByReference]
		public JournalEntry Entry { get; private set; }
		public override void Load() {
			Mod.AddContent(Entry = new Worn_Paper_Entry(this));
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Gray;
			Item.width = 16;
			Item.height = 16;
			Item.maxStack = Item.CommonMaxStack;
		}
		public override bool CanPickup(Player player) {
			return true;
		}
		public override bool OnPickup(Player player) {
			OriginPlayer oP = player.OriginPlayer();
			if (!oP.unlockedJournalEntries.Contains(EntryName)) {
				oP.UnlockJournalEntry(EntryName);
				return false;
			}
			return true;
		}
	}
	[Autoload(false)]
	public class Worn_Paper_Entry(Worn_Paper paper) : JournalEntry {
		public override string Name => paper.PaperName;
		public override string TextKey => paper.PaperName;
		public override LocalizedText DisplayName => paper.DisplayName;
		public override JournalSortIndex SortIndex => new(nameof(Worn_Paper), 0);
	}
	public class Worn_Paper_Loose_Wheel : Worn_Paper { }
	public class Worn_Paper_Self_Preservation : Worn_Paper { }
	public class Worn_Paper_Smog_Test : Worn_Paper { }
	public class Worn_Paper_The_Packing_Slip : Worn_Paper { }
	public class Worn_Paper_They_Found_Us : Worn_Paper { }
	public class Worn_Paper_Mean_Baby : Worn_Paper { }
	public class Worn_Paper_Workplace_Safety_Concern: Worn_Paper { }
}
