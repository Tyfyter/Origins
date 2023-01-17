using Origins.Journal;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Eccentric_Stone : ModItem, IJournalEntryItem {
        public string IndicatorKey => "Mods.Origins.Journal.Indicator.Whispers";
        public string EntryName => "Origins/" + typeof(Eccentric_Stone_Entry).Name;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Eccentric Stone");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Aglet);
            Item.rare = ItemRarityID.White;
        }
    }
    public class Eccentric_Stone_Entry : JournalEntry {
        public override string TextKey => "Eccentric_Stone";
        public override ArmorShaderData TextShader => null;
    }
}
