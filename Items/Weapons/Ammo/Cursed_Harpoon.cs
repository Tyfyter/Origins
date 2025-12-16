using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
namespace Origins.Items.Weapons.Ammo {
	public class Cursed_Harpoon : ModItem {
		public static int ID { get; private set; }
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.damage = 20;
			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;
			Item.maxStack = 99;
			Item.shoot = Cursed_Harpoon_P.ID;
			Item.ammo = Harpoon.ID;
			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 5)
			.AddRecipeGroup(RecipeGroupID.IronBar, 5)
			.AddIngredient(ItemID.CursedFlame)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(Type, 5)
			.AddIngredient(ModContent.ItemType<Harpoon>(), 5)
			.AddIngredient(ItemID.CursedFlame)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Cursed_Harpoon_P : Harpoon_P {
		public static new int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void AI() {//still needs its own AI override since it has unique AI functionality
			if (Projectile.ai[0] == 1 && Projectile.penetrate >= 0) {
				Projectile.aiStyle = ProjAIStyleID.Arrow;
				Projectile.velocity = Projectile.oldVelocity;
				Projectile.tileCollide = true;
				Vector2 diff = Main.player[Projectile.owner].itemLocation - Projectile.Center;
				SoundEngine.PlaySound(SoundID.Item10, Projectile.Center + diff / 2);
				float len = diff.Length() * 0.125f;
				diff /= len;
				Vector2 pos = Projectile.Center;
				for (int i = 0; i < len; i++) {
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, default, Cursed_Harpoon_Flame.ID, Projectile.damage, 0, Projectile.owner, i * 0.15f);
					pos += diff;
				}
			}
			if (Projectile.penetrate == 1) {
				Projectile.penetrate--;
			}
		}
	}
	public class Cursed_Harpoon_Flame : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CursedDartFlame;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Cursed Harpoon");
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.CursedDartFlame);
			Projectile.friendly = false;
			Projectile.alpha = 255;
			Projectile.aiStyle = 0;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.timeLeft = 10 + (int)Projectile.ai[0];
		}
		public override bool PreAI() {
			return Projectile.timeLeft <= 10;
		}
		public override void AI() {
			Projectile.friendly = true;
			Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CursedTorch, 0f, 0f, 100);
			dust.position.X -= 2f;
			dust.position.Y += 2f;
			dust.scale += Main.rand.Next(50) * 0.01f;
			dust.noGravity = true;
			dust.velocity.Y -= 2f;
			if (Main.rand.NextBool(2)) {
				Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CursedTorch, 0f, 0f, 100);
				dust2.position.X -= 2f;
				dust2.position.Y += 2f;
				dust2.scale += 0.3f + Main.rand.Next(50) * 0.01f;
				dust2.noGravity = true;
				dust2.velocity *= 0.1f;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.CursedInferno, Main.rand.Next(270, 360));
		}
	}
}
