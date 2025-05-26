using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using PegasusLib;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class Ichor_Harpoon : ModItem {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.damage = 13;
			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;
			Item.maxStack = 99;
			Item.shoot = Ichor_Harpoon_P.ID;
			Item.ammo = Harpoon.ID;
			Item.value = Item.sellPrice(silver: 5);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			CreateRecipe(5)
			.AddRecipeGroup(RecipeGroupID.IronBar, 5)
			.AddIngredient(ItemID.Ichor)
			.AddTile(TileID.Anvils)
			.Register();

			CreateRecipe(5)
			.AddIngredient<Harpoon>(5)
			.AddIngredient(ItemID.Ichor)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Ichor_Harpoon_P : Harpoon_P {
		public static new int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void AI() {//still needs its own AI override since it has unique AI functionality
			if (Projectile.ai[0] == 1 && Projectile.penetrate >= 0 && Projectile.aiStyle != 1) {
				Projectile.aiStyle = 1;
				Projectile.velocity = Projectile.oldVelocity;
				Projectile.tileCollide = true;
				Vector2 diff = Main.player[Projectile.owner].itemLocation - Projectile.Center;
				SoundEngine.PlaySound(SoundID.Item10, Projectile.Center + diff / 2);
				float len = diff.Length() / 16;
				diff /= len;
				Projectile.SpawnProjectile(null, Projectile.Center, default, Ichor_Harpoon_Droplet.ID, Projectile.damage, 0, len, diff.X, diff.Y);
			}
			if (Projectile.penetrate == 1) {
				Projectile.penetrate--;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Ichor, Main.rand.Next(9 * 60, 12 * 60 + 1));
		}
	}
	public class Ichor_Harpoon_Droplet : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CursedDartFlame;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.CursedDartFlame);
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.penetrate = -1;
			Projectile.alpha = 255;
			Projectile.aiStyle = -1;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 20;
		}
		Hitbox[] hitboxes;
		public override void AI() {
			if (hitboxes is null) {
				hitboxes = new Hitbox[(int)Projectile.ai[0]];
				Vector2 pos = Projectile.Center;
				Vector2 diff = new(Projectile.ai[1], Projectile.ai[2]);
				for (int i = 0; i < hitboxes.Length; i++) {
					hitboxes[i] = new(pos);
					pos += diff;
				}
			}
			Projectile.active = false;
			for (int i = 0; i < hitboxes.Length; i++) {
				Projectile.active |= hitboxes[i].active;
				hitboxes[i].Update(Projectile.timeLeft);
			}
		}
		int hitIndex = -1;
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (hitboxes is null) return false;
			hitIndex = -1;
			for (int i = 0; i < hitboxes.Length; i++) {
				if (hitboxes[i].active && targetHitbox.Intersects(hitboxes[i].hitbox)) {
					hitIndex = i;
					return true;
				}
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Ichor, Main.rand.Next(6 * 60, 8 * 60 + 1));
			if (hitIndex != -1) hitboxes[hitIndex].active = false;
		}
		struct Hitbox(Vector2 vector) {
			public Rectangle hitbox = new((int)vector.X - 6, (int)vector.Y - 6, 12, 12);
			float yOffset = 0;
			float ySpeed = 0;
			public bool active = true;
			public void Update(int time) {
				if (!active) return;
				if (time % 3 == 0 && hitbox.OverlapsAnyTiles()) {
					active = false;
				}
				ySpeed += 0.12f;
				ySpeed *= 0.99f;
				yOffset += ySpeed;
				while (yOffset >= 1) {
					hitbox.Y += 1;
					yOffset -= 1;
				}
				Dust dust = Dust.NewDustDirect(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.Ichor, 0f, 0f);
				dust.position.X -= 2f;
				dust.position.Y += 2f;
				dust.scale += Main.rand.NextFloat(0.5f);
				dust.noGravity = true;
				dust.velocity *= 0.1f;
				dust.velocity.Y -= 0.3f;
				if (Main.rand.NextBool(2)) {
					dust = Dust.NewDustDirect(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.Ichor, 0f, 0f);
					dust.position.X -= 2f;
					dust.position.Y += 2f;
					dust.scale += Main.rand.NextFloat(0.5f);
					dust.noGravity = true;
					dust.velocity *= 0.1f;
				}
			}
		}
	}
}
