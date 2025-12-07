using Origins.Buffs;
using Origins.Dev;
using Origins.Layers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOff)]
	public class Playtimes_Over : ModItem, ICustomWikiStat, ITornSource {
		public static float TornSeverity => 0.1f;
		float ITornSource.Severity => TornSeverity;
		public string[] Categories => [
            "Vitality",
            "Torn",
            "TornSource"
        ];
        static short glowmask;
        public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask<HandsOff_Glow_Layer>(Item.handOffSlot, Texture + "_HandsOff_Glow");
		}
        public override void SetDefaults() {
            Item.DefaultToAccessory(38, 20);
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ItemRarityID.LightPurple;
            Item.glowMask = glowmask;
        }
        public override void AddRecipes() {
            Recipe.Create(Type)
            .AddIngredient(ModContent.ItemType<Bug_Trapper>())
            .AddIngredient(ModContent.ItemType<Fleshy_Figurine>())
            .AddTile(TileID.TinkerersWorkbench)
			.Register();
        }
        public override void UpdateEquip(Player player) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            if (!originPlayer.taintedFlesh2) originPlayer.tornStrengthBoost.Flat += 0.18f * (1 + originPlayer.tornCurrentSeverity);
            originPlayer.taintedFlesh2 = true;
            originPlayer.symbioteSkull = true;
            originPlayer.bugZapper = true;
        }
    }
}
