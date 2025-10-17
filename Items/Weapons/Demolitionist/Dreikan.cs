using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Dreikan : ModItem, ICustomWikiStat {
		public string[] Categories => [
			nameof(WeaponTypes.OtherExplosive)
		];
		public override void SetStaticDefaults() {
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				global.SetUpdateCountBoost(proj, global.UpdateCountBoost + 2);
			});
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SniperRifle);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 48;
			Item.crit = 20;
			Item.useAnimation = 33;
			Item.useTime = 11;
			Item.width = 96;
			Item.height = 24;
			Item.shoot = ModContent.ProjectileType<Dreikan_Shot>();
			Item.reuseDelay = 6;
			//Item.scale = 0.75f;
			Item.value = Item.sellPrice(gold: 5);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SoulofNight, 30)
			.AddIngredient(ModContent.ItemType<Hallowed_Cleaver>())
			.AddIngredient(ModContent.ItemType<Phoenum>(), 15)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-16, 2);
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Item40, player.itemLocation);
			SoundEngine.PlaySound(SoundID.Item36.WithVolume(0.75f), player.itemLocation);
			return null;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == ProjectileID.Bullet) type = Item.shoot;
		}
	}
	public class Dreikan_Shot : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_286";

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ExplosiveBullet);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			AIType = ProjectileID.ExplosiveBullet;
			Projectile.light = 0;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, 0.5f, 0.35f, 0.05f);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Daybreak, 30);
			target.immune[Projectile.owner] = 5;
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
			for (int i = 0; i < 7; i++) {
				Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default(Color), 1.5f);
			}
			for (int i = 0; i < 3; i++) {
				int num568 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default(Color), 2.5f);
				Main.dust[num568].noGravity = true;
				Dust dust1 = Main.dust[num568];
				Dust dust2 = dust1;
				dust2.velocity *= 3f;
				num568 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default(Color), 1.5f);
				dust1 = Main.dust[num568];
				dust2 = dust1;
				dust2.velocity *= 2f;
			}
			Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), new Vector2(Projectile.position.X - 10f, Projectile.position.Y - 10f), default(Vector2), Main.rand.Next(61, 64));
			Gore gore2 = gore;
			gore2.velocity *= 0.3f;
			gore.velocity.X += Main.rand.Next(-10, 11) * 0.05f;
			gore.velocity.Y += Main.rand.Next(-10, 11) * 0.05f;
			if (Projectile.owner == Main.myPlayer) {
				Projectile.localAI[1] = -1f;
				Projectile.maxPenetrate = 0;
				Projectile.position.X += Projectile.width / 2;
				Projectile.position.Y += Projectile.height / 2;
				Projectile.width = 80;
				Projectile.height = 80;
				Projectile.position.X -= Projectile.width / 2;
				Projectile.position.Y -= Projectile.height / 2;
				Projectile.Damage();
			}
		}
	}
	public class Dreikan_Crit_Type : CritType<Dreikan> {
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) {
			return target.TryGetGlobalNPC(out Dreikan_Crit_NPC npc) && projectile.TryGetGlobalProjectile(out Dreikan_Crit_Projectile proj) && npc.IncrementHit(proj.shotNumber);
		}
		public override float CritMultiplier(Player player, Item item) => 3f;
		class Dreikan_Crit_Projectile : CritGlobalProjectile {
			public int shotNumber = 0;
			public override bool IsLoadingEnabled(Mod mod) => ModEnabled;
			public override void OnSpawn(Projectile projectile, IEntitySource source) {
				if (source is EntitySource_ItemUse itemUse && itemUse.Item.ModItem is Dreikan) {
					shotNumber = itemUse.Player.ItemUsesThisAnimation;
				}
			}
		}
		class Dreikan_Crit_NPC : CritGlobalNPC {
			int hitTimer = 0;
			int hitNumber = 0;
			public override bool InstancePerEntity => true;
			public override bool IsLoadingEnabled(Mod mod) => ModEnabled;
			public override void ResetEffects(NPC npc) {
				if (hitTimer.Cooldown()) {
					hitNumber = 0;
				}
			}
			public bool IncrementHit(int number) {
				if (number == hitNumber + 1) {
					hitNumber++;
					hitTimer = 30;
					if (hitNumber == 3) {
						hitNumber = 0;
						return true;
					}
				} else if (number == 1) {
					hitNumber = 1;
					hitTimer = 30;
				} else if (number != hitNumber || hitTimer <= 20) {
					hitNumber = 0;
					hitTimer = 0;
				}
				return false;
			}
		}
	}
}
