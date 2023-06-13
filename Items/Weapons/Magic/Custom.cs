using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Custom : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Startillery");
			Tooltip.SetDefault("Fires an arcing starshot that explodes on impact");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThrowingKnife);
			Item.damage = 48;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Item.mana = 3;
			Item.noUseGraphic = true;
			Item.useTime = 6;
			Item.useAnimation = 6;
			Item.knockBack = 3;
			Item.autoReuse = true;
			Item.consumable = false;
			Item.maxStack = 16;
			Item.shoot = ModContent.ProjectileType<HDisc>();
			Item.shootSpeed = 32f;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = Origins.Sounds.DeepBoom.WithPitchRange(1.7f, 2f);
		}
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < Item.stack;
		}
	}
	public class HDisc : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_288";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Starshot");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.LightDisc);
			Projectile.friendly = true;
			Projectile.hide = false;
			Projectile.alpha = 0;
		}
	}
}
