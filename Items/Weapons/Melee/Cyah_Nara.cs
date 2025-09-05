using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Cyah_Nara : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Sword"
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Katana);
			Item.damage = 18;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = false;
			Item.noMelee = false;
			Item.width = 46;
			Item.height = 52;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 19;
			Item.useAnimation = 19;
			Item.knockBack = 0f;
			Item.shoot = ProjectileID.None;
			Item.value = Item.sellPrice(silver: 2);
			Item.rare = ItemRarityID.Blue;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item71.WithPitch(1f);
		}
	}
}
