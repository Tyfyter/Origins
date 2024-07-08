using Microsoft.Xna.Framework;
using Origins.Dusts;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Tyfyter.Utils;
using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
	public class Ameballoon : ModItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => new string[] {
            "ThrownExplosive",
            "SpendableWeapon"
        };
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this, "");
			Item.ResearchUnlockCount = 30;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 25;
			Item.width = 20;
			Item.height = 22;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.shoot = ModContent.ProjectileType<Ameballoon_P>();
			Item.shootSpeed = 8.75f;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(copper: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.glowMask = glowmask;
            Item.ArmorPenetration += 1;
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 20);
			recipe.AddCondition(
			   Language.GetOrRegister("Mods.Origins.Conditions.RivenWater"),
			   () => Main.LocalPlayer.adjWater && Main.LocalPlayer.InModBiome<Riven_Hive>()
			);
			recipe.AddIngredient(ModContent.ItemType<Rubber>());
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
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
		}
		public override bool PreKill(int timeLeft) {
			return base.PreKill(timeLeft);
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.NPCDeath1.WithPitch(0.15f), Projectile.Center);
			PolarVec2 vel = new PolarVec2(4, Main.rand.NextFloat(MathHelper.TwoPi));

			for (int i = Main.rand.Next(12, 16); i-- > 0;) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2)vel, ModContent.ProjectileType<Ameballoon_Shrapnel>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
				vel.Theta += Main.rand.NextFloat(0.5f) + 1.618033988749894848204586834f;
				vel.R += Main.rand.NextFloat(0.5f);

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
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.ignoreWater = true;
		}
		public override void AI() {
			Projectile.rotation -= MathHelper.PiOver2;
		}
		public override void OnKill(int timeLeft) {
			if (timeLeft < 3590) SoundEngine.PlaySound(SoundID.NPCHit18.WithPitch(0.15f).WithVolumeScale(0.5f), Projectile.Center);
		}
		public override Color? GetAlpha(Color lightColor) => Riven_Hive.GetGlowAlpha(lightColor);
	}
}
