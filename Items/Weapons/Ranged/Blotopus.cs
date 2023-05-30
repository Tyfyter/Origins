using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
	public class Blotopus : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Blotopus");
			Tooltip.SetDefault("Fires a stream of needles that bleed enemies\n20% chance not consume ammo");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Musket);
			Item.damage = 4;
			Item.width = 64;
			Item.height = 22;
			Item.useTime = 6;
			Item.useAnimation = 6;
			Item.shoot = ModContent.ProjectileType<Blotopus_P>();
			Item.useAmmo = AmmoID.Bullet;
			Item.knockBack = 0;
			Item.shootSpeed = 7f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item2;
			Item.autoReuse = true;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == ProjectileID.Bullet) type = Item.shoot;
			velocity = velocity.RotatedByRandom(0.1f);
		}
		public override Vector2? HoldoutOffset() {
			return Vector2.Zero;
		}
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return !Main.rand.NextBool(5);
		}
	}
	public class Blotopus_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Ranged/Blotopus_P";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Bloody Needle");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.alpha = 0;
		}
		public override void AI() {
			Projectile.rotation -= MathHelper.PiOver2;
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			target.AddBuff(BuffID.Bleeding, Main.rand.Next(119, 241));
		}
	}
}
