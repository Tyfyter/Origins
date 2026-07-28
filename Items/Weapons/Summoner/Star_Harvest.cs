using Origins.Dev;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Projectiles;
using Origins.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Summoner {
	public class Star_Harvest : ModItem, ICustomWikiStat {
		public static int BaseMaxHealth => 100;
		public static int CellsPerSplit(int maxLife) => Main.rand.RandomRound(maxLife / 33f);
		public static int MaxCellsPerSlot(int maxLife) => maxLife / 16;
		public static int HurtImmuneTime => 5; //unaffected by speed modifiers, how many frames small cells will be immune to attacks for after being hit
		public static int BaseChildhoodDuration => 120; //affected by speed modifiers
		public static int ChildImmuneTime => 15; //unaffected by speed modifiers, how many frames small cells will be immune to attacks and unable to attack for
		public static int ChildViolenceTime => 15; //affected by speed modifiers, how many frames small cells will attack for before growing up
		public static float BaseSpeed => 15;
		public static float Inertia => 30;
		
		public override void SetStaticDefaults() {
			ItemID.Sets.StaffMinionSlotsRequired[Type] = 0;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 100;
			Item.DamageType = DamageClass.Summon;
			Item.knockBack = 1;
			Item.mana = 20;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.RaiseLamp;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.Red;
			Item.UseSound = SoundID.Item117;
			Item.buffType = Star_Harvest_Buff.ID;
			Item.shoot = Star_Cell_Tracker.ID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Star_Harvest_Buff.ID, 2);
			ModPrefix prefix = PrefixLoader.GetPrefix(Item.prefix);
			FungibleSet<int> openSlots = Star_Cell.CountKin(
				prefix,
				player.whoAmI,
				-1,
				(int)((prefix as ArtifactMinionPrefix)?.MaxLifeModifier ?? StatModifier.Default).ApplyTo(BaseMaxHealth)
			);
			if (openSlots.Count > 0) {
				using ScopedOverride<int> _ = Star_Cell.spawnOnTracker.ScopedOverride(openSlots.MinBy(t => t.Value).Key);
				player.SpawnMinionOnCursor(source, player.whoAmI, Star_Cell.ID, Item.damage, knockback);
			} else {
				using ScopedOverride<float> _ = ItemID.Sets.StaffMinionSlotsRequired[Type].ScopedOverride(1);
				PlayerMethods.FreeUpPetsAndMinions(player, Item);
				player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);
			}
			return false;
		}
	}
	public class Star_Harvest_Buff : MinionBuff {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StardustCellMinion;
		public override IEnumerable<int> ProjectileTypes() => [
			Star_Cell.ID,
			Small_Star_Cell.ID,
			Star_Cell_Tracker.ID
		];
		public static int ID { get; private set; }
		public override bool ShowSlots => true;
		protected override void SetBuffFlag(Player player) => player.OriginPlayer().starCellArtifact = true;
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Star_Cell : MinionBase, IArtifactMinion {
		public static int ID { get; private set; }
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StardustCellMinion;
		public int MaxLife { get; set; }
		public float Life { get; set; }
		public override Rectangle RestRegion => new Rectangle(0, 0, 48, 48).Recentered(Owner.MountedCenter - new Vector2(Owner.direction * 64, 0));
		public float SacrificeAvoidance => float.PositiveInfinity;
		public override void SetStaticDefaults() {
			this.SetIDProp();
			Main.projFrames[Type] = 4;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Type] = true;
			OriginsSets.Projectiles.NoMultishot[Type] = true;
			OriginsSets.Projectiles.ReducedDeathHealEffectChance[Type] = 1f / 8;
		}

		public override void SetDefaults() {
			OriginsSets.Projectiles.ReducedDeathHealEffectChance[Type] = 1f / 8;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
			Projectile.netImportant = true;
			MaxLife = Star_Harvest.BaseMaxHealth;
		}
		internal static int spawnOnTracker = -1;
		public override void OnSpawn(IEntitySource source) {
			if (spawnOnTracker >= 0) Projectile.ai[2] = spawnOnTracker;
		}
		/// <summary>
		/// Return early if this returns true
		/// </summary>
		protected bool CheckTracker() {
			if (Projectile.GetRelatedProjectile(2) is not { active: true, ModProjectile: Star_Cell_Tracker }) {
				Projectile.ai[2] = -1;
				Projectile.Kill();
				return true;
			}
			return false;
		}
		protected override void BasicAI() {
			float overlapVelocity = 0.08f;
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (other != Projectile && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
					if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
					else Projectile.velocity.X += overlapVelocity;

					if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
					else Projectile.velocity.Y += overlapVelocity;
				}
			}
			base.BasicAI();
		}
		public override void AI() {
			if (CheckTracker()) return;
			BasicAI();
			Projectile.localAI[0].Cooldown();
			if (Projectile.localAI[0] <= 0) Projectile.localAI[0] = this.GetHurtByHostiles(skipNPCs: true).Mul(Star_Harvest.HurtImmuneTime);
		}
		public override void MoveTowardsTarget() {
			Rectangle restRegion = RestRegion;
			if (!targetingData.HasTarget && Projectile.Hitbox.Intersects(restRegion)) {
				Projectile.velocity *= 0.99f;
				return;
			}
			Projectile.rotation += Projectile.direction * 0.3f;
			Vector2 targetCenter = targetingData.HasTarget ? targetingData.Center : restRegion.Center();
			Vector2 direction = (targetCenter - Projectile.Center).Normalized(out float distance);
			float speed = Star_Harvest.BaseSpeed * SpeedModifier;
			speed += distance / 100f;
			float inertia = Star_Harvest.Inertia + 1;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction * speed) / inertia;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			int targetDamage = target.damage;
			if (target.aiStyle == NPCAIStyleID.Celestial_Pillar) targetDamage = 80;
			if (targetDamage > 0) this.DamageArtifactMinion(target.life > 0 ? targetDamage : (targetDamage / 2), new NPCDamageSource(target));
			hit.Knockback = 1;
			hit.HitDirection *= -1;
			Projectile.velocity = hit.GetKnockbackFromHit();
		}
		public void OnHurt(int damage, bool fromDoT) {
			if (Life > 0 && !fromDoT) {
				int quarterWidth = Projectile.width / 4;
				for (int i = 0; i < 10.0 + damage / 10.0; i++) {
					Dust dust = Dust.NewDustDirect(Projectile.Center - Vector2.One * (quarterWidth + 1), quarterWidth * 2, quarterWidth * 2, DustID.Vortex);
					Vector2 direction = Vector2.Normalize(dust.position - Projectile.Center);
					dust.position = Projectile.Center + direction * quarterWidth * Projectile.scale - new Vector2(4f);
					if (i < 30) {
						dust.velocity = direction * dust.velocity.Length() * 2f;
					} else {
						dust.velocity = 2f * direction * Main.rand.Next(45, 91) / 10f;
					}
					dust.noGravity = true;
					dust.scale = 0.7f + Main.rand.NextFloat();
				}
			}
		}
		public override void OnKill(int timeLeft) {
			int quarterWidth = Projectile.width / 4;
			for (int i = 0; i < 60; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.Center - Vector2.One * (quarterWidth + 1), quarterWidth * 2, quarterWidth * 2, DustID.Vortex);
				Vector2 direction = Vector2.Normalize(dust.position - Projectile.Center);
				dust.position = Projectile.Center + direction * quarterWidth * Projectile.scale - new Vector2(4f);
				if (i < 30) {
					dust.velocity = direction * dust.velocity.Length() * 2f;
				} else {
					dust.velocity = 2f * direction * Main.rand.Next(45, 91) / 10f;
				}
				dust.noGravity = true;
				dust.scale = 0.7f;
			}
			if (!Projectile.IsLocallyOwned() || ArtifactMinionSystem.IsDismissingMinion || Projectile.type != ID) return;
			if (Projectile.GetRelatedProjectile(2) is not { active: true, ModProjectile: Star_Cell_Tracker }) return;
			int count = Star_Harvest.CellsPerSplit(MaxLife);
			foreach ((int tracker, int capacity) in CountKin().OrderBy(p => p.Value)) {
				for (int i = capacity; i > 0 && (count--) > 0; i--) {
					Projectile.NewProjectile(
						Projectile.GetSource_Death(),
						Projectile.Center,
						Main.rand.NextVector2Circular(1, 1),
						Small_Star_Cell.ID,
						Projectile.originalDamage,
						Projectile.knockBack,
						ai2: tracker
					);
				}
			}
			if (count > 0) {
				Projectile.NewProjectile(
					Projectile.GetSource_Death(),
					Projectile.Center,
					default,
					Star_Cell_Tracker.ID,
					Projectile.originalDamage,
					Projectile.knockBack,
					ai1: count
				);
			}
		}
		protected bool CheckTracked() {
			if (GetProjectile(Projectile.owner, (int)Projectile.ai[2]) is not { active: true, ModProjectile: Star_Cell_Tracker }) {
				Projectile.Kill();
				return false;
			}
			return true;
		}
		static readonly FungibleSet<int> availableCounts = new();
		protected FungibleSet<int> CountKin() => CountKin(Projectile.GetGlobalProjectile<OriginGlobalProj>().prefix, Projectile.owner, Projectile.whoAmI, MaxLife);
		public static FungibleSet<int> CountKin(ModPrefix prefix, int owner, int ignoreProjectile, int maxLife) {
			availableCounts.Clear();
			foreach (Projectile minion in Main.ActiveProjectiles) {
				if (minion.owner != owner || minion.whoAmI == ignoreProjectile) continue;
				if (minion.GetGlobalProjectile<OriginGlobalProj>().prefix != prefix) continue;
				switch (minion.ModProjectile) {
					case Star_Cell:
					int tracker = (int)minion.ai[2];
					if (availableCounts[tracker] == 0) availableCounts[tracker] = Star_Harvest.MaxCellsPerSlot(maxLife);
					availableCounts[tracker]--;
					break;
					case Star_Cell_Tracker:
					if (availableCounts[minion.identity] == 0) availableCounts[minion.identity] = Star_Harvest.MaxCellsPerSlot(maxLife);
					break;
				}
			}
			return availableCounts;
		}

		#region boilerplate
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => fallThrough = true;
		public override ref bool HasBuff(Player player) => ref player.OriginPlayer().starCellArtifact;
		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => true;
		#endregion
	}
	public class Small_Star_Cell : Star_Cell {
		public new static int ID { get; private set; }
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StardustCellMinionShot;
		public override bool SkipTargeting =>  Projectile.ai[0] < Star_Harvest.BaseChildhoodDuration - Star_Harvest.ChildViolenceTime;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			OriginsSets.Projectiles.NoMultishot[Type] = true;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.width = 18;
			Projectile.height = 18;
			MaxLife = Star_Harvest.BaseMaxHealth / 2;
		}
		public override void MoveTowardsTarget() {
			if (SkipTargeting) Projectile.velocity *= 0.99f;
			else base.MoveTowardsTarget();
			Projectile.ai[1]++;
			Projectile.rotation += Projectile.velocity.X * 0.1f;
			Projectile.scale = 1f + 0.2f * MathHelper.Clamp(Projectile.ai[0] / Star_Harvest.BaseChildhoodDuration, 0f, 1f);
			if (Projectile.ai[0].Warmup(Star_Harvest.BaseChildhoodDuration, SpeedModifier)) {
				Projectile.SpawnProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center,
					Projectile.velocity,
					Star_Cell.ID,
					Projectile.originalDamage,
					Projectile.knockBack,
					ai2: Projectile.ai[2]
				);
				Projectile.active = false;
				float dustCount = Main.rand.Next(10, 21);
				float randomOffset = Main.rand.NextFloat(MathHelper.TwoPi);
				float vector = 12f / 2f;
				for (float i = 0f; i < dustCount; i += 1f) {
					Dust dust = Main.dust[Dust.NewDust(Projectile.Center, 0, 0, DustID.Vortex)];
					Vector2 direction = Vector2.UnitY.RotatedBy(i * MathHelper.TwoPi / dustCount + randomOffset);
					dust.position = Projectile.Center + direction * vector;
					dust.velocity = direction;
					dust.noGravity = true;
					dust.scale = 0.6f + Main.rand.NextFloat() * 1.8f;
					dust.velocity *= dust.scale;
					dust.fadeIn = Main.rand.NextFloat() * 2f;
				}
			} else {
				Vector2 center = Projectile.Center;
				int dustCount = (int)(Projectile.ai[0] * 2 / Star_Harvest.BaseChildhoodDuration);
				for (int i = 0; i < dustCount + 1; i++) {
					if (Main.rand.NextBool(2)) {
						Vector2 direction = Main.rand.NextVector2Unit();
						Vector2 position = center + direction * (6f - (dustCount * 2));
						Dust dust = Dust.NewDustDirect(position - Vector2.One * 4f, 8, 8, DustID.Electric, Projectile.velocity.X / 2f, Projectile.velocity.Y / 2f);
						dust.position -= new Vector2(2f);
						dust.velocity = direction * 1.5f * (10f - dustCount * 2f) / 10f;
						dust.noGravity = true;
						dust.scale = 0.3f + (i % 2) * 0.15f;
						dust.customData = this;
					}
				}
			}
		}
		public override void AI() {
			if (CheckTracker()) return;
			BasicAI();
			Projectile.localAI[0].Cooldown();
			if (MinionContactDamage() && Projectile.localAI[0] <= 0) Projectile.localAI[0] = this.GetHurtByHostiles(skipNPCs: true).Mul(Star_Harvest.HurtImmuneTime);
		}
		public override bool MinionContactDamage() => Projectile.ai[1] > Star_Harvest.ChildImmuneTime;
		public override void OnKill(int timeLeft) {
			base.OnKill(timeLeft);
		}
	}
	public class Star_Cell_Tracker : MinionBase, IArtifactMinion {
		public static int ID { get; private set; }
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StardustCellMinion;
		public override bool SkipTargeting => true;
		public override bool AutomaticRotationAndDirection => false;
		public int MaxLife { get; set; }
		public float Life { get => MaxLife; set { } }
		public float SacrificeAvoidance => Projectile.localAI[0] * Projectile.localAI[1];
		public override void SetStaticDefaults() {
			this.SetIDProp();
			MinionBuff.SkipInCount[Type] = true;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			OriginsSets.Projectiles.ReducedDeathEffectChance[Type] = 0;
		}
		public override void SetDefaults() {
			Projectile.netImportant = true;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.timeLeft = 60;
			Projectile.aiStyle = ProjAIStyleID.DesertTigerBall;
			Projectile.hide = true;
			MaxLife = Star_Harvest.BaseMaxHealth;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse) Projectile.ai[0] = 1;
		}
		public override bool PreAI() {
			if (Projectile.localAI[0] == 0) {
				int prefix = Projectile.GetGlobalProjectile<OriginGlobalProj>().Prefix;
				ItemMethods.TryGetPrefixStatMultipliers(ModContent.GetInstance<Star_Harvest>().Item, prefix, out float dmg, out float kb, out float spd, out float size, out float shtspd, out float mcst, out int crt);
				float valueMult = 1f * dmg * (2f - spd) * (2f - mcst) * size * kb * shtspd * (1f + crt * 0.02f);
				PrefixLoader.GetPrefix(prefix)?.ModifyValue(ref valueMult);
				Projectile.localAI[0] = valueMult;
			}
			if (Projectile.IsLocallyOwned() && (Projectile.ai[0] != 0 || Projectile.ai[1] != 0)) {
				for (int i = 0; i < Projectile.ai[0]; i++)
					Projectile.NewProjectile(
						Projectile.GetSource_Death(),
						Projectile.Center,
						default,
						Star_Cell.ID,
						Projectile.originalDamage,
						Projectile.knockBack,
						ai2: Projectile.identity
					);
				for (int i = 0; i < Projectile.ai[1]; i++)
					Projectile.NewProjectile(
						Projectile.GetSource_Death(),
						Projectile.Center,
						Main.rand.NextVector2Circular(1, 1),
						Small_Star_Cell.ID,
						Projectile.originalDamage,
						Projectile.knockBack,
						ai2: Projectile.identity
					);
				Projectile.ai[0] = 0;
				Projectile.ai[1] = 0;
				Projectile.netUpdate = true;
			}
			return base.PreAI();
		}
		public override void MoveTowardsTarget() {
			if (!Projectile.IsLocallyOwned()) return;
			int count = 0;
			int maxCells = Star_Harvest.MaxCellsPerSlot(MaxLife);
			foreach (Projectile minion in Main.ActiveProjectiles) {
				if (minion.owner != Projectile.owner) continue;
				if (minion.ModProjectile is not Star_Cell) continue;
				if (minion.ai[2] == Projectile.identity) {
					if (++count > maxCells) {
						minion.ai[2] = -1;
						minion.Kill();
					}
				}
			}
			Projectile.localAI[1] = count / (float)maxCells;
			if (count == 0) Projectile.Kill();
		}
		public override void OnKill(int timeLeft) {
			foreach (Projectile minion in Main.ActiveProjectiles) {
				if (minion.owner != Projectile.owner) continue;
				if (minion.ModProjectile is not Star_Cell) continue;
				if (minion.ai[2] == Projectile.identity) {
					minion.ai[2] = -1;
					minion.Kill();
				}
			}
		}
		public override ref bool HasBuff(Player player) => ref player.OriginPlayer().starCellArtifact;
		public override bool? CanCutTiles() => false;
	}
}
