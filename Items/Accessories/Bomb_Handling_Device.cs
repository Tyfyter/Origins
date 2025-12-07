using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Items.Darksteel;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Bomb_Handling_Device : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"ExplosiveBoostAcc"
		];
        static short glowmask;
        public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
        }
        public override void SetDefaults() {
			Item.DefaultToAccessory(38, 20);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
            Item.glowMask = glowmask;
        }
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().bombHandlingDevice = true;
			player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed += 0.25f;
		}
	}
}
