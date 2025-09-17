using Origins.Dev;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Brainade : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "ThrownExplosive",
			"IsGrenade",
            "ExpendableWeapon"
        ];
        public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.Confused, BuffID.Bleeding];
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 32;
			Item.crit = 4;
			Item.value *= 5;
			Item.shoot = ModContent.ProjectileType<Brainade_P>();
			Item.ammo = ItemID.Grenade;
			Item.value = Item.sellPrice(copper: 50);
			Item.rare = ItemRarityID.Blue;
			Item.ArmorPenetration += 5;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 25)
            .AddIngredient(ItemID.CrimtaneOre)
            .AddIngredient(ItemID.Grenade, 25)
            .AddTile(TileID.Anvils)
            .Register();

			Recipe.Create(Type, 75)
			.AddIngredient(ItemID.CrimtaneBar)
			.AddIngredient(ItemID.Grenade, 75)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Brainade_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Brainade";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 135;
			Projectile.penetrate = -1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			int t = ModContent.ProjectileType<Brain_Blood>();
			for (int i = Main.rand.Next(12); i < 18; i++) Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Main.rand.NextVector2Unit() * 4) + (Projectile.velocity / 8), t, Projectile.damage / 12, 6, Projectile.owner, ai1: -0.5f).scale = 1f;
		}
	}
	public class Brain_Blood : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
			Projectile.width = Projectile.height = 10;
			Projectile.timeLeft = 180;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}
		public override void AI() {
			if (Projectile.ai[1] <= 0/*projectile.timeLeft<168*/) {
				if (Projectile.timeLeft % 3 == 0) {
					Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, Projectile.velocity * -0.25f, 100, new Color(255, 0, 0), Projectile.scale);
					dust.noGravity = false;
					dust.noLight = true;
				}
			} else {
				Projectile.Center = Main.player[Projectile.owner].itemLocation + Projectile.velocity;
				Projectile.ai[1]--;
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.timeLeft > 168 && (Projectile.ai[1] % 1 + 1) % 1 == 0.5f) {
				Projectile.velocity -= oldVelocity - Projectile.velocity;
				return false;
			}
			return true;
		}
		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 7; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, 10, 10, DustID.Blood, 0, 0, 100, new Color(255, 0, 0), 1.25f * Projectile.scale);
				dust.noGravity = true;
				dust.noLight = true;
			}
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = (int)(96 * Projectile.scale);
			Projectile.height = (int)(96 * Projectile.scale);
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.damage = (int)(Projectile.damage * 0.75f);
			Projectile.Damage();
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.timeLeft > 168 && (Projectile.ai[1] % 1 + 1) % 1 == 0.5f) Projectile.penetrate++;
			target.AddBuff(BuffID.Confused, 120);
			target.AddBuff(BuffID.Bleeding, 180);
			Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood, 0, 0, 100, new Color(255, 0, 0), 1.25f * Projectile.scale);
			dust.noGravity = false;
			dust.noLight = true;
		}
		public override bool PreDraw(ref Color lightColor) {
			return false;
		}
	}
}
