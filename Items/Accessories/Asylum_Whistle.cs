using Origins.Items.Materials;
using Origins.Journal;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Asylum_Whistle : ModItem, IJournalEntryItem {
        public string IndicatorKey => "Mods.Origins.Journal.Indicator.Whispers";
        public string EntryName => "Origins/"+typeof(Asylum_Whistle_Entry).Name;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Asylum Whistle");
            Tooltip.SetDefault("");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().asylumWhistle = true;
        }
    }
	public class Asylum_Whistle_Entry : JournalEntry {
        public override string TextKey => "Mods.Origins.Journal.Asylum_Whistle";
        public override ArmorShaderData TextShader => null;
	}
}
