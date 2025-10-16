using Origins.Dev;
using Origins.Layers;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Shotgun_Glove : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override void SetStaticDefaults() {
            Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask<HandsOff_Glow_Layer>(Item.handOnSlot, Texture + "_HandsOn_Glow");
		}
        public override void SetDefaults() {
			Item.DefaultToAccessory(24, 18);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.shootSpeed = 5;
			Item.useAmmo = AmmoID.Bullet;
			Item.UseSound = SoundID.Item36;
        }
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.gunGlove = true;
			originPlayer.gunGloveItem = Item;
		}
		public override bool RangedPrefix() => false;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = 3; i-- > 0;) {
				Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.5f), type, damage, knockback, player.whoAmI);
			}
			return false;
		}
	}
}
