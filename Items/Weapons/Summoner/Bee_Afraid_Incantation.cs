using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.NPCs;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Bee_Afraid_Incantation : ModItem, ICustomDrawItem {
		private Asset<Texture2D> _smolTexture;
		public Texture2D SmolTexture => (_smolTexture ??= this.GetSmallTexture())?.Value;
		public override void SetStaticDefaults() {
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.Poisoned];
		}
		public override void SetDefaults() {
			Item.damage = 12;
			Item.DamageType = DamageClasses.Incantation;
			Item.noMelee = true;
			Item.width = 22;
			Item.height = 34;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.shoot = ModContent.ProjectileType<Bee_Afraid_Incantation_P>();
			Item.shootSpeed = 0f;
			Item.mana = 18;
			Item.knockBack = 0.8f;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item97;
			Item.channel = true;
			Item.holdStyle = ItemHoldStyleID.HoldLamp;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.BeeWax, 10)
			.AddIngredient(ItemID.Book, 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override void UseItemFrame(Player player) => Incantations.HoldItemFrame(player);
		public override void HoldItemFrame(Player player) => Incantations.HoldItemFrame(player);
		public bool BackHand => true;
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Incantations.DrawInHand(
				SmolTexture,
				ref drawInfo,
				lightColor
			);
		}
	}
	public class Bee_Afraid_Incantation_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Summoner/Minions/Amoeba_Bubble";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 6;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.ignoreWater = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.timeLeft = 90;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[0] = -1;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (Projectile.ai[0] == 0) {
				player.TryCancelChannel(Projectile);
				Projectile.Kill();
				return;
			}

			if (Projectile.ai[0] == -1 && Main.myPlayer == Projectile.owner) {
				Projectile.ai[0] = 0;
				for (int i = 0; i < 7; i++) {
					Projectile.NewProjectile(
						Terraria.Entity.InheritSource(Projectile),
						Projectile.position,
						Projectile.velocity + Main.rand.NextVector2Circular(3, 3),
						player.beeType() == ProjectileID.Bee ? Bee_Afraid_Incantation_Bee.ID : Bee_Afraid_Incantation_Big_Bee.ID,
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner,
						Projectile.whoAmI
					);
					Projectile.ai[0]++;
				}
			}
			if (player.channel) {
				if (Main.myPlayer == Projectile.owner && Main.MouseWorld != Projectile.position) {
					Projectile.position = Main.MouseWorld;
					Projectile.netUpdate = true;
				}
				Projectile.timeLeft = 5;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			return false;
		}
	}
	public class Bee_Afraid_Incantation_Bee : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bee;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bee);
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 5;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.ignoreWater = false;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.hide = false;
			Projectile.timeLeft = 180;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 6;
			Projectile.extraUpdates = 0;
			Projectile.alpha = 0;
		}
		public override void AI() {
			const float grouping_factor = 0.005f;
			const float spreading_factor = 0.05f;
			const float sheeping_factor = 0.05f;
			//const float glass_door_factor = 1f;// for when I get around to block avoidance
			const float control_weight = 30f;
			const float anger_weight = 8f;
			const float helping_weight = 8f;

			Vector2 swarmCenter = default;
			Vector2 magnetism = default;
			Vector2 swarmVelocity = default;
			float totalWeight = 0;

			bool freeBoid = true;


			Player player = Main.player[Projectile.owner];
			float targetDist = 16 * 12;
			targetDist *= targetDist;
			Vector2 targetCenter = Projectile.Center;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (isPriorityTarget) return;
				if (npc.CanBeChasedBy()) {
					Vector2 diff = npc.Center - Projectile.Center;
					float dist = diff.LengthSquared();
					if (dist < targetDist && Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height)) {
						targetDist = dist;
						targetCenter = npc.Center;
						foundTarget = true;
					}
				}
			}
			Vector2 enemyDir = default;
			if (Projectile.ai[0] != -1) {
				Projectile target = Main.projectile[(int)Projectile.ai[0]];
				if (target.active && target.type == Bee_Afraid_Incantation_P.ID) {
					freeBoid = false;
					Projectile.timeLeft = 300 + Main.rand.Next(60);
					swarmCenter = target.position * control_weight;
					Vector2 dir = Projectile.DirectionTo(target.position); 
					swarmVelocity = dir * 8 * control_weight;
					magnetism = dir * 0.25f;
					totalWeight += control_weight;
					if (player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm)) {
						swarmCenter += targetCenter * helping_weight;
						enemyDir = Projectile.DirectionTo(targetCenter);
						swarmVelocity += enemyDir * 8 * helping_weight;
						magnetism = enemyDir * 0.25f;
						totalWeight += helping_weight;
					}
				} else {
					Projectile.ai[0] = -1;
					Projectile.penetrate = 5;
				}
			} else {
				if (player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm)) {
					swarmCenter = targetCenter * anger_weight;
					enemyDir = Projectile.DirectionTo(targetCenter);
					swarmVelocity = enemyDir * 8 * anger_weight;
					magnetism = enemyDir * 0.25f;
					totalWeight += anger_weight;
				}
			}
			const float max_free_boid_range = 16 * 4;
			const float i_must_bee_traveling_on_now = 8;
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (other.type != Type || other.ai[0] != Projectile.ai[0]) continue;
				float distSQ = other.DistanceSQ(Projectile.Center);
				if (freeBoid && distSQ > max_free_boid_range * max_free_boid_range) continue;

				swarmCenter += other.Center;
				swarmVelocity += other.velocity;
				totalWeight += 1;
				if (distSQ <= i_must_bee_traveling_on_now * i_must_bee_traveling_on_now) magnetism += (Projectile.Center - other.Center) * spreading_factor;
				//float blockDist = CollisionExtensions.
			}

			if (totalWeight > 0) {
				Projectile.velocity =
					(Projectile.velocity +
					((swarmCenter / totalWeight) - Projectile.Center) * grouping_factor + 
					magnetism + 
					(swarmVelocity / totalWeight) * sheeping_factor)
					.WithMaxLength(9)
				;
			}
			if (enemyDir != default) {
				Vector2 norm = Projectile.velocity.SafeNormalize(default);
				float dot = Vector2.Dot(enemyDir, norm);
				if (dot < 0) dot *= -0.9f;
				Projectile.velocity -= Projectile.velocity * (1 - MathF.Pow(dot, 3)) * 0.05f;
			}

			Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
			Projectile.rotation = Projectile.velocity.X * 0.1f;
			Projectile.frameCounter++;
			if (++Projectile.frameCounter >= 3) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= 3) Projectile.frame = 0;
			}

		}
		public override void OnKill(int timeLeft) {
			if (Projectile.ai[0] != -1) {
				Projectile target = Main.projectile[(int)Projectile.ai[0]];
				if (target.active && target.type == Bee_Afraid_Incantation_P.ID) {
					target.ai[0]--;
					target.netUpdate = true;
				}
			}
			for (int i = 0; i < 6; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Bee, Projectile.velocity.X, Projectile.velocity.Y, 50);
				dust.noGravity = true;
				dust.scale = 1f;
			}

		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Bee_Incantation_Buff.ID, 240);
			target.AddBuff(BuffID.Poisoned, 180);
			if (target.life > 0 && target.CanBeChasedBy()) Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
			if (Projectile.ai[0] == -1) Projectile.damage = (int)(Projectile.damage * 0.90);
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = -oldVelocity.X;
			if (Projectile.velocity.Y != oldVelocity.Y) Projectile.velocity.Y = -oldVelocity.Y;
			return false;
		}
	}
	public class Bee_Afraid_Incantation_Big_Bee : Bee_Afraid_Incantation_Bee {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GiantBee;
		public new static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GiantBee);
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 7;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.ignoreWater = false;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.hide = false;
			Projectile.timeLeft = 180;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 6;
			Projectile.extraUpdates = 0;
			Projectile.alpha = 0;
		}
	}
	public class Bee_Incantation_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().beeAfraidDebuff = true;
		}
	}
}
