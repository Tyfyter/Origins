using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Journal;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Defiled_Spirit : ModItem, ICustomWikiStat, IJournalEntrySource {
		static short glowmask;
        public string[] Categories => [
            WikiCategories.ThrownExplosive,
            WikiCategories.ExpendableWeapon
        ];
		public string EntryName => "Origins/" + typeof(Defiled_Spirit_Entry).Name;
		public class Defiled_Spirit_Entry : JournalEntry {
			public override string TextKey => "Defiled_Spirit";
			public override JournalSortIndex SortIndex => new("The_Defiled", 2);
		}
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Snowball);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
			Item.damage = 12;
			Item.shoot = ModContent.ProjectileType<Defiled_Spirit_P>();
			Item.shootSpeed = 10;
			Item.knockBack = 0;
			Item.value = Item.sellPrice(copper: 40);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
            Item.ArmorPenetration += 12;
        }
	}
	public class Defiled_Spirit_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Defiled_Spirit_P";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			Origins.MagicTripwireRange[Type] = 32;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SnowBallFriendly);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
			Projectile.penetrate = 1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (++Projectile.frameCounter > 3) {
				Projectile.frame = (Projectile.frame + 1) % 3;
				Projectile.frameCounter = 0;
			}
			if (Main.rand.NextBool(3)) Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Asphalt);
			const int HalfSpriteWidth = 50 / 2;

			int HalfProjWidth = Projectile.width / 2;

			// Vanilla configuration for "hitbox towards the end"
			if (Projectile.spriteDirection == 1) {
				DrawOriginOffsetX = -(HalfProjWidth - HalfSpriteWidth);
				DrawOffsetX = (int)-DrawOriginOffsetX * 2;
				DrawOriginOffsetY = 0;
			} else {
				DrawOriginOffsetX = (HalfProjWidth - HalfSpriteWidth);
				DrawOffsetX = 0;
				DrawOriginOffsetY = 0;
			}
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			int t = ModContent.ProjectileType<Return_To_Sender_Thorns>();
			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, t, Projectile.damage, 6, Projectile.owner, ai0: 1f).scale = 1f;
			Projectile.Damage();
			SoundEngine.PlaySound(SoundID.Item46.WithVolume(0.66f), Projectile.Center);
			for (int i = 0; i < 18; i++) {
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Asphalt);
			}
		}
	}
}
