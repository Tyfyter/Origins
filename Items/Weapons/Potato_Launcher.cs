using Microsoft.Xna.Framework;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Consumables.Food;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons {
    public class Potato_Launcher : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Potato Launcher");
			// Tooltip.SetDefault("Uses potatoes for ammo\n'Time to make some mash...'");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FlintlockPistol);
			Item.damage = 25;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.CountsAsClass(DamageClass.Generic);
			Item.useAmmo = ModContent.ItemType<Potato>();
			Item.shoot = ModContent.ProjectileType<Potato_P>();
			Item.knockBack = 2f;
			Item.shootSpeed = 12f;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(silver: 65);
			Item.rare = ItemRarityID.Blue;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemOpen or EntitySource_Loot) {
				Item.NewItem(source, Item.position, ModContent.ItemType<Potato>(), Main.rand.Next(40, 51));
			}
		}
	}
	public class Potato_P : ModProjectile {
		public override string Texture => "Origins/Items/Other/Consumables/Food/Potato";
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Potato");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SnowBallFriendly);
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.scale = 0.6f;
		}
	}
}
