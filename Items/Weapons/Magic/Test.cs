using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
    public class Test : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Vilethorn);
			Item.damage = 16;
			Item.crit = 4;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.useTime = 6;
			Item.useAnimation = 24;
			Item.shootSpeed = 3;
			Item.mana = 2;
			Item.knockBack = 1;
			Item.shoot = ModContent.ProjectileType<Test2>();
			Item.useAmmo = AmmoID.None;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Blue;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = velocity.RotatedByRandom(0.25f);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.itemAnimation != 0 && !player.CheckMana(Item, pay: true)) {
				return false;
			}
			SoundEngine.PlaySound(SoundID.Item43, position);
			return true;
		}
	}
	public class Test2 : ModProjectile {
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bee);
			Projectile.friendly = true;
			Projectile.penetrate = 2;
			Projectile.timeLeft = 360;
		}
		public override void AI() {
			Dust dust = Dust.NewDustDirect(Projectile.Center, -11, 0, DustID.GoldFlame, 0, 0, 255, new Color(20, 200, 30), 1f);
			dust.noGravity = false;
			dust.velocity *= 3f;
		}
	}
}
