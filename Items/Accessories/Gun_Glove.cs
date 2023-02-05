using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Gun_Glove : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Gun Glove");
			Tooltip.SetDefault("Shoots as you swing\n'Ammo not included'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 18);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;

			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.shootSpeed = 5;
			Item.useAmmo = AmmoID.Bullet;
			Item.UseSound = SoundID.Item10;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.gunGlove = true;
			originPlayer.gunGloveItem = Item;
		}
	}
}
