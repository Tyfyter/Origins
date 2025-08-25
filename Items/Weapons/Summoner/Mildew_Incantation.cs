using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Misc;
using Origins.NPCs;
using Origins.NPCs.Brine;
using Origins.Projectiles;
using PegasusLib;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Misc.Physics;

namespace Origins.Items.Weapons.Summoner {
	public class Mildew_Incantation : ModItem, ICustomDrawItem {
		private Asset<Texture2D> _smolTexture;
		public Texture2D SmolTexture => (_smolTexture ??= this.GetSmallTexture())?.Value;
		public override void SetStaticDefaults() {
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [Toxic_Shock_Debuff.ID, Slow_Debuff.ID];
		}
		public override void SetDefaults() {
			Item.damage = 50;
			Item.DamageType = DamageClasses.Incantation;
			Item.noMelee = true;
			Item.width = 22;
			Item.height = 34;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 26;
			Item.useAnimation = 26;
			Item.shoot = ModContent.ProjectileType<Mildew_Incantation_P>();
			Item.shootSpeed = 10f;
			Item.mana = 14;
			Item.knockBack = 1f;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item8;
			Item.holdStyle = ItemHoldStyleID.HoldLamp;
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
	public class Mildew_Incantation_P : ModProjectile {
		public override void SetStaticDefaults() {
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.ignoreWater = true;
			Projectile.friendly = true;
			Projectile.timeLeft = 90;
			Projectile.alpha = 50;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
		}
		public override void OnKill(int timeLeft) {
			if (Projectile.penetrate == 0) {
				ExplosiveGlobalProjectile.DoExplosion(
					Projectile,
					40,
					false,
					SoundID.NPCDeath1,
					0,
					0,
					0
				);
				for (int i = 0; i < 14; i++) {
					Vector2 offset = Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2();
					Dust dust = Dust.NewDustPerfect(
						Projectile.Center + offset * Main.rand.NextFloat(10, 30),
						DustID.Bone,
						Projectile.oldVelocity * 0.2f - offset * 2,
						100,
						Scale: 1.2f
					);
					dust.noGravity = true;
					dust.velocity *= 1.2f;
				}
			} else {
				SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
				for (int i = 0; i < 7; i++) {
					Dust dust = Dust.NewDustDirect(
						Projectile.position - Vector2.One * 2,
						Projectile.width + 4,
						Projectile.height + 4,
						DustID.Bone,
						Projectile.oldVelocity.X * 0.4f,
						Projectile.oldVelocity.Y * 0.4f,
						100,
						Scale: 1.2f
					);
					dust.noGravity = true;
					dust.velocity *= 1.2f;
					dust.velocity.Y -= 0.5f;
				}
				if (Projectile.owner == Main.myPlayer) {
					Vector2[] directions = [
						Vector2.UnitX,
						-Vector2.UnitX,
						Vector2.UnitY,
						-Vector2.UnitY
					];
					const float offsetLen = 0;
					Vector2 basePos = Projectile.Center;
					float dist = 48;
					int directionIndex = 2;
					Vector2 bestPosition = basePos;
					for (int i = 0; i < directions.Length; i++) {
						float newDist = CollisionExt.Raymarch(basePos, directions[i], dist);
						if (newDist < dist) {
							dist = newDist;
							bestPosition = basePos + directions[i] * (dist - offsetLen);
							directionIndex = i;
						}
					}
					Projectile.NewProjectile(
						Projectile.GetSource_Death(),
						bestPosition,
						Vector2.Zero,
						ModContent.ProjectileType<Mildew_Incantation_Creeper>(),
						Projectile.damage / 2,
						Projectile.knockBack,
						ai2: directionIndex
					);
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Mildew_Incantation_Buff.ID, 240);
			target.AddBuff(Toxic_Shock_Debuff.ID, Main.rand.Next(180, 301));
			if (target.life > 0) Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			_ = Projectile.penetrate;
			return true;
		}
	}
	public class Mildew_Incantation_Creeper : ModProjectile {
		public override string Texture => typeof(Mildew_Creeper).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
			Projectile.width = 26;
			Projectile.height = 26;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.scale = 0.85f;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 30;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[0] = -1;
		}
		public override bool? CanHitNPC(NPC target) {
			if (Projectile.ai[0] != -1) return Projectile.ai[0] == target.whoAmI;
			if (OriginsSets.NPCs.TargetDummies[target.type]) return false;
			return base.CanHitNPC(target);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Mildew_Incantation_Buff.ID, 240);
			target.AddBuff(Slow_Debuff.ID, 31);
			if (target.life > 0) Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
			Projectile.ai[0] = target.whoAmI;
			if (Projectile.timeLeft > 61) Projectile.timeLeft = 61;
		}
		Physics.Chain chain;
		public override void AI() {
			if (Projectile.ai[0] == -1) {
				float maxDistanceFromAnchor = 200f * 200f;
				float distanceFromTarget = 2000f * 2000f;
				void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
					if (chain is null) return;
					if (npc.CanBeChasedBy()) {
						float between = Vector2.DistanceSquared(npc.Center, Projectile.Center);
						// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
						if (between < distanceFromTarget && Vector2.DistanceSquared(npc.Center, chain.anchor.WorldPosition) < maxDistanceFromAnchor) {
							distanceFromTarget = between;
							Projectile.ai[1] = npc.whoAmI;
							foundTarget = true;
						}
					}
				}
				if (!Main.player[Projectile.owner].OriginPlayer().GetMinionTarget(targetingAlgorithm)) Projectile.ai[1] = -1;
			} else {
				Projectile.ai[1] = Projectile.ai[0];
				if (!Main.npc.IndexInRange((int)Projectile.ai[1]) || !Main.npc[(int)Projectile.ai[1]].active) {
					Projectile.Kill();
				}
			}
			if (chain is null) {
				Gravity[] gravity = [
					new ConstantGravity(Vector2.UnitY * -0.006f),
					new ConstantGravity(directions[(int)Projectile.ai[2]] * -0.008f),
				];
				List<Chain.Link> links = [];
				Vector2 anchor = Projectile.Center;
				anchor.Y += 8;
				const float spring = 0.5f;
				for (int i = 0; i < 15; i++) {
					links.Add(new(anchor, default, 13.6f, gravity, drag: 0.93f, spring: spring));
				}
				links.Add(new(anchor, default, 17f, [new ProjectileTargetGravity(this)], drag: 0.93f, spring: spring));
				chain = new Physics.Chain() {
					anchor = new WorldAnchorPoint(anchor),
					links = links.ToArray()
				};
			}
			Projectile.velocity = chain.links[^1].velocity;
			Projectile.Center = chain.links[^1].position;
			chain.Update();
			Projectile.Center = chain.links[^1].position;
		}
		Vector2[] directions = [
			Vector2.UnitX,
			-Vector2.UnitX,
			Vector2.UnitY,
			-Vector2.UnitY
		];
		public override bool PreDraw(ref Color lightColor) {
			if (chain is null) return false;
			
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				chain.anchor.WorldPosition - Main.screenPosition,
				new Rectangle(10, 34, 16, 24),
				Lighting.GetColor(chain.anchor.WorldPosition.ToTileCoordinates()),
				(chain.links[1].position - chain.anchor.WorldPosition).ToRotation() - MathHelper.PiOver2,
				new(8, 4),
				Projectile.scale,
				SpriteEffects.None
			);
			for (int i = 0; i < chain.links.Length - 1; i++) {
				Vector2 position = chain.links[i].position;
				Main.EntitySpriteDraw(
					texture,
					position - Main.screenPosition,
					new Rectangle(10, 34, 16, 24),
					Lighting.GetColor(position.ToTileCoordinates()),
					(chain.links[i + 1].position - position).ToRotation() - MathHelper.PiOver2,
					new(8, 4),
					Projectile.scale,
					SpriteEffects.None
				);
			}
			Main.EntitySpriteDraw(
				texture,
				chain.anchor.WorldPosition - Main.screenPosition,
				new Rectangle(0, 60, 36, 18),
				Lighting.GetColor(chain.anchor.WorldPosition.ToTileCoordinates()),
				directions[(int)Projectile.ai[2]].ToRotation() - MathHelper.PiOver2,
				new(18, 9),
				Projectile.scale,
				SpriteEffects.None
			);
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				new Rectangle(0, 0, 36, 32),
				lightColor,
				(chain.links[^2].position - Projectile.Center).ToRotation() - MathHelper.PiOver2,
				new(18, 16),
				Projectile.scale,
				SpriteEffects.None
			);
			return false;
		}
		public override void OnKill(int timeLeft) {
			Main.gore[Origins.instance.SpawnGoreByName(
				Projectile.GetSource_Death(),
				chain.anchor.WorldPosition,
				Vector2.Zero,
				$"Gores/NPC/{nameof(Mildew_Creeper)}_Gore_3"
			)].scale = Projectile.scale;
			Main.gore[Origins.instance.SpawnGoreByName(
				Projectile.GetSource_Death(),
				chain.anchor.WorldPosition,
				Vector2.Zero,
				$"Gores/NPC/{nameof(Mildew_Creeper)}_Gore_2"
			)].scale = Projectile.scale;
			for (int i = 0; i < chain.links.Length - 1; i++) {
				Main.gore[Origins.instance.SpawnGoreByName(
					Projectile.GetSource_Death(),
					chain.links[i].position,
					chain.links[i].velocity,
					$"Gores/NPC/{nameof(Mildew_Creeper)}_Gore_2"
				)].scale = Projectile.scale;
			}
			Main.gore[Origins.instance.SpawnGoreByName(
				Projectile.GetSource_Death(),
				Projectile.Center,
				Projectile.velocity,
				$"Gores/NPC/{nameof(Mildew_Creeper)}_Gore_1"
			)].scale = Projectile.scale;
		}
		public class ProjectileTargetGravity(Mildew_Incantation_Creeper proj) : Gravity {
			public override Vector2 Acceleration {
				get {
					if (!Main.npc.IndexInRange((int)proj.Projectile.ai[1])) return Vector2.Zero;
					return (Main.npc[(int)proj.Projectile.ai[1]].Center - proj.Projectile.Center).WithMaxLength(proj.Projectile.ai[0] == -1 ? 0.6f : 0.9f);
				}
			}
		}
	}
	public class Mildew_Incantation_Buff : ModBuff, ICustomWikiStat {
		public bool CanExportStats => false;
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().mildewIncantationDebuff = true;
		}
	}
}
