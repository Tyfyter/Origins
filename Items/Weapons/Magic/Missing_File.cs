using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Magic {
    public class Missing_File : ModItem, ICustomWikiStat {
        public string[] Categories => new string[] {
            "HardmodeOtherMagic"
        };
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CrystalVileShard);
			Item.shoot = ModContent.ProjectileType<Potato_P>();
			Item.damage = 64;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.shootSpeed = 12;
			Item.useStyle = ItemHoldStyleID.HoldUp;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Yellow;
			Item.mana = 18;
		}
	}
}
