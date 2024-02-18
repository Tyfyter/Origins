using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Playtimes_Over : ModItem, ICustomWikiStat {
        [AutoloadEquip(EquipType.HandsOff)]
        public string[] Categories => new string[] {
            "Vitality",
            "Torn",
            "TornSource"
        };
        static short glowmask;
        public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
        }
        public override void SetDefaults() {
            Item.DefaultToAccessory(38, 20);
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ItemRarityID.LightPurple;
            Item.glowMask = glowmask;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Bug_Zapper>());
            recipe.AddIngredient(ModContent.ItemType<Fleshy_Figurine>());
            recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
        }
        public override void UpdateEquip(Player player) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            if (!originPlayer.taintedFlesh2) originPlayer.tornStrengthBoost.Flat += 0.13f * (1 + originPlayer.tornCurrentSeverity);
            originPlayer.taintedFlesh2 = true;
            originPlayer.symbioteSkull = true;
            originPlayer.bugZapper = true;
        }
    }
}
