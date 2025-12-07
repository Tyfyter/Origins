using Origins.Dev;
using Origins.Dusts;
using Origins.Gores.NPCs;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using PegasusLib;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Ameballoon : ModItem {
		static short glowmask;
        public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			glowmask = Origins.AddGlowMask(this, "");
			Item.ResearchUnlockCount = 30;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 32;
			Item.shoot = ModContent.ProjectileType<Ameballoon_P>();
			Item.shootSpeed *= 1.75f;
			Item.value = Item.sellPrice(copper: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.glowMask = glowmask;
            Item.ArmorPenetration += 4;
        }
		public override void AddRecipes() {
			Recipe.Create(Type, 20)
			.AddCondition(RecipeConditions.RivenWater)
			.AddIngredient(ModContent.ItemType<Rubber>())
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
	public class Ameballoon_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Ameballoon";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.aiStyle = ProjAIStyleID.GroundProjectile;
			Projectile.penetrate = 1;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.scale *= 0.6f;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 60;
			Projectile.alpha = 150;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = -1;
		}
		public override bool PreKill(int timeLeft) {
			return base.PreKill(timeLeft);
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.NPCDeath1.WithPitch(0.15f), Projectile.Center);
			if (Projectile.owner == Main.myPlayer) {
				PolarVec2 vel = new(4, Main.rand.NextFloat(MathHelper.TwoPi));
				for (int i = Main.rand.Next(12, 16); i-- > 0;) {
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2)vel, ModContent.ProjectileType<Ameballoon_Shrapnel>(), Projectile.damage / 12, Projectile.knockBack, Projectile.owner);
					vel.Theta += Main.rand.NextFloat(0.5f) + 1.618033988749894848204586834f;
					vel.R += Main.rand.NextFloat(0.5f);
				}
			}
			for (int i = 0; i < 5; i++) {
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Glass);
			}
			for (int i = 0; i < 30; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, Gooey_Water_Dust.ID, 0f, -2f, 0, default(Color), 1.1f);
				dust.alpha = 100;
				dust.velocity.X *= 1.5f;
				dust.velocity *= 3f;
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.Kill();
			return false;
		}
		public override Color? GetAlpha(Color lightColor) => Riven_Hive.GetGlowAlpha(lightColor);
	}
	public class Ameballoon_Shrapnel : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Ameballoon_P";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 3600;
			Projectile.aiStyle = ProjAIStyleID.Arrow;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.ArmorPenetration += 25;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.knockBack = 0;
			Projectile.ignoreWater = true;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
		}
		public override void AI() {
			Projectile.rotation -= MathHelper.PiOver2;
		}
		public override void OnKill(int timeLeft) {
			if (timeLeft < 3590) {
				SoundEngine.PlaySound(SoundID.NPCHit18.WithPitch(0.15f).WithVolumeScale(0.5f), Projectile.Center);
				for (int i = Main.rand.Next(6, 12); i-- > 0;) {
					Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity.RotatedByRandom(1.5f) * Main.rand.NextFloat(0f, 1f), ModContent.GoreType<R_Effect_Blood1_Small>());
				}
				Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			}
		}
		public override Color? GetAlpha(Color lightColor) => Riven_Hive.GetGlowAlpha(lightColor);
	}
}
