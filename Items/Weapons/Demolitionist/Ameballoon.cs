using Origins.Dusts;
using Origins.Gores.NPCs;
using Origins.Items.Materials;
using Origins.NPCs.Riven;
using Origins.Projectiles;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Ameballoon : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Origins.AddGlowMask(this, "");
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
		public override void SetStaticDefaults() {
			Hand_Grenade_Launcher.AltFireAction[Type] = (player, source, position, velocity, type, damage, knockback) => {
				position += velocity.SafeNormalize(Vector2.Zero);
				Projectile.NewProjectileDirect(source, position, velocity * 0.6f, ModContent.ProjectileType<Ameballoon_Bubble>(), damage, knockback, player.whoAmI);
			};
		}
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
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, Gooey_Water_Dust.ID, 0f, -2f, Scale: 1.1f);
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
	public class Ameballoon_Bubble : ModProjectile, IBroken {
		public override string Texture => typeof(Amoebeye_P).GetDefaultTMLName();
		public static string BrokenReason => "needs balancing, needs sound on bouncing";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ThrownExplosive;
			Projectile.timeLeft = 10 * 60;
			Projectile.width = 38;
			Projectile.height = 38;
			Projectile.friendly = true;
		}
		public override void AI() {
			#region Behavior
			if (Projectile.ai[0].Warmup(0.5f * 60)) {
				Projectile.ai[0] = 0;
				Projectile.scale -= 0.05f;
				int shrapnelDmg = (int)(Projectile.damage / 5f);
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.UnitY * 0.1f, ModContent.ProjectileType<Ameballoon_Shrapnel>(), shrapnelDmg, Projectile.knockBack * 0.3f, Projectile.owner);
				Projectile.ai[1]++;
				if (Projectile.scale < 0.5f) // I only want it to last for 10 seconds OR until its scale gets below this value
					Projectile.Kill();
			}

			const int range = 16 * 16;
			const int boss_range = 16 * 27;
			const float boss_ratio = range / (float)boss_range;
			float targetWeight = range;
			Vector2 targetPos = default;
			bool foundTarget = Main.player[Projectile.owner].DoHoming((target) => {
				Vector2 currentPos = Projectile.Center.Clamp(target.Hitbox);
				float dist = Projectile.Center.Distance(currentPos);
				if (target is Player) dist *= 1.5f;
				if (target is NPC npc && (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type])) dist *= boss_ratio;
				if (dist < targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height)) {
					targetWeight = dist;
					targetPos = currentPos;
					return true;
				}
				return false;
			});

			if (foundTarget) {
				Vector2 targetVelocity = (targetPos - Projectile.Center).Normalized(out _);

				targetVelocity *= 2f / Projectile.scale;
				float speed = Projectile.velocity.Length();
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.17999999f / Projectile.scale).Normalized(out float newSpeed) * float.Max(speed, newSpeed);
			}
			#endregion

			#region Visuals
			const int HalfSpriteWidth = 58 / 2;
			const int HalfSpriteHeight = 58 / 2;
			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);

			if (++Projectile.frameCounter > 6) {
				Projectile.frame = (Projectile.frame + 1) % 4;
				Projectile.frameCounter = 0;
			}
			Projectile.spriteDirection = 1;
			#endregion
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			// TODO: add sound on bounce
			if (oldVelocity.X != Projectile.velocity.X) Projectile.velocity.X = -oldVelocity.X;
			if (oldVelocity.Y != Projectile.velocity.Y) Projectile.velocity.Y = -oldVelocity.Y;
			return false;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 1 - Projectile.ai[1] / 10;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.SourceDamage *= 1 - Projectile.ai[1] / 10;
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, (int)(128 * Projectile.scale), sound: SoundID.NPCDeath1.WithPitch(0.15f), fireDustAmount: 0, smokeDustAmount: 8, smokeGoreAmount: 2);
			if (Projectile.owner == Main.myPlayer) {
				PolarVec2 vel = new(4, Main.rand.NextFloat(MathHelper.TwoPi));
				for (int i = (int)(Main.rand.Next(12, 16) + (4 * Main.gfxQuality)); i-- > 0;) {
					Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, (Vector2)vel, Main.rand.Next(R_Effect_Blood1.GoreIDs));
					vel.Theta += Main.rand.NextFloat(0.5f) + 1.618033988749894848204586834f;
					vel.R += Main.rand.NextFloat(0.5f);
				}
			}
			for (int i = 0; i < 10; i++) {
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Glass);
			}
			for (int i = 0; i < 30; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, Gooey_Water_Dust.ID, 0f, -2f, Scale: 1.1f);
				dust.alpha = 100;
				dust.velocity.X *= 1.5f;
				dust.velocity *= 3f;
			}
		}
		public override Color? GetAlpha(Color lightColor) => Riven_Hive.GetGlowAlpha(lightColor);
	}
}
