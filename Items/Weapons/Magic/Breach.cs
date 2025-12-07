using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Journal;
using Origins.Projectiles;
using PegasusLib;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Magic {
	public class Breach : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string[] Categories => [
			"OtherMagic"
		];
		public string EntryName => "Origins/" + typeof(Breach_Entry).Name;
		public class Breach_Entry : JournalEntry {
			public override string TextKey => "Breach";
			public override JournalSortIndex SortIndex => new("The_Defiled", 16);
		}
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[Type] = Type;
			OriginExtensions.InsertIntoShimmerCycle(ModContent.ItemType<Missing_File>(), Type);
		}
		public override void SetDefaults() {
			Item.DefaultToMagicWeapon(ModContent.ProjectileType<Breach_P>(), 27, 9);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Item.damage = 57;
			Item.crit = 5;
			Item.knockBack = 6f;
			Item.mana = 26;
			Item.width = 28;
			Item.height = 30;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Yellow;
			Item.ArmorPenetration = 8;
		}
		public static void SpawnManaStar(Projectile projectile, double damageDealt) {
			if (projectile.owner == Main.myPlayer) {
				int item = Item.NewItem(
					Main.LocalPlayer.GetSource_OnHurt(PlayerDeathReason.ByProjectile(Main.myPlayer, projectile.whoAmI)),
					Main.LocalPlayer.MountedCenter,
					ModContent.ItemType<Manasynk_Pickup>(),
					(int)(damageDealt * 1.34f)
				);
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
				}
			}
		}
	}
	public class Breach_P : ModProjectile, ISelfDamageEffectProjectile {
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 40;
			Main.projFrames[Type] = 3;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.AmberBolt);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.penetrate = -1;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.alpha = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void AI() {
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Asphalt, 0, -1, 0, default, 1.2f);
			if (++Projectile.frameCounter > 5) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) {
					Projectile.frame = 0;
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.timeLeft > 0) {
				Projectile.Kill();
			}
			target.AddBuff(Buffs.Rasterized_Debuff.ID, 20);
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, sound: SoundID.Item116, fireDustAmount: 0, smokeDustAmount: 15, smokeGoreAmount: 0);
			for (int i = 0; i < 3; i++) Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, default, Breach_P_Debris.ID);
			if (Projectile.owner == Main.myPlayer) {
				int count = Main.rand.Next(7, 10);
				int type = ModContent.ProjectileType<Breach_Wisp>();
				for (int i = 0; i < count; i++) {
					Vector2 velocity = Main.rand.NextVector2Circular(1, 1);
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						Projectile.Center + Main.rand.NextVector2Circular(64, 64),
						velocity,
						type,
						Projectile.damage / 2,
						1f
					);
				}
			}
		}
		public void OnSelfDamage(Player player, Player.HurtInfo info, double damageDealt) {
			Breach.SpawnManaStar(Projectile, damageDealt);
		}
	}
	public class Breach_Wisp : ModProjectile, ISelfDamageEffectProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SpiritFlame);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.hide = false;
			Projectile.aiStyle = 0;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 180;
			Projectile.localAI[1] = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 5;
			Projectile.localAI[2] = Main.rand.Next(30);
		}
		public override void AI() {
			if (Main.rand.NextBool(6)) Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Asphalt, 0, 0, 0, default, 0.75f);
			if (Projectile.localAI[2] > 0f) {
				Projectile.localAI[2]--;
				Projectile.timeLeft--;
			}
			if (Projectile.localAI[0] > 0f) {
				Projectile.localAI[0]--;
			}
			if (Projectile.alpha > 0) {
				Projectile.alpha -= 5;
				if (Projectile.alpha < 0) Projectile.alpha = 0;
			}
			if (Projectile.localAI[0] == 0f && Projectile.owner == Main.myPlayer) {
				Projectile.localAI[0] = 5f;
				float currentTargetDist = Projectile.ai[0] > 0 ? Main.npc[(int)Projectile.ai[0] - 1].Distance(Projectile.Center) : 0;
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC targetOption = Main.npc[i];
					if (targetOption.CanBeChasedBy()) {
						float newTargetDist = targetOption.Distance(Projectile.Center);
						bool selectNew = Projectile.ai[0] <= 0f || currentTargetDist > newTargetDist;
						if (selectNew && (newTargetDist < 240f)) {
							Projectile.ai[0] = i + 1;
							currentTargetDist = newTargetDist;
						}
					}
				}
				if (Projectile.ai[0] > 0f) {
					Projectile.timeLeft = 300 - Main.rand.Next(120);
					Projectile.netUpdate = true;
				}
			}

			int target = (int)Projectile.ai[0] - 1;
			if (target >= 0) {
				if (Main.npc[target].active) {
					if (Projectile.Distance(Main.npc[target].Center) > 1f) {
						Vector2 dir = Projectile.DirectionTo(Main.npc[target].Center);
						if (dir.HasNaNs()) {
							dir = Vector2.UnitY;
						}
						float angle = dir.ToRotation();
						PolarVec2 velocity = (PolarVec2)Projectile.velocity;
						const float targetVel = 6;
						bool changed = false;
						if (velocity.R != targetVel) {
							OriginExtensions.LinearSmoothing(ref velocity.R, targetVel, (targetVel - 0.5f) * 0.1f);
							changed = true;
						}
						if (velocity.Theta != angle) {
							OriginExtensions.AngularSmoothing(ref velocity.Theta, angle, 0.2f);
							changed = true;
						}
						if (changed) {
							Projectile.velocity = (Vector2)velocity;
						}
					}
					return;
				}
				Projectile.ai[0] = 0f;
				Projectile.netUpdate = true;
			} else {
				PolarVec2 velocity = (PolarVec2)Projectile.velocity;
				const float targetVel = 1;
				bool changed = false;
				if (velocity.R != targetVel) {
					OriginExtensions.LinearSmoothing(ref velocity.R, targetVel / 3f, (targetVel - 0.5f) * 0.1f);
					changed = true;
				}

				if (velocity.Theta != Projectile.localAI[1]) {
					OriginExtensions.AngularSmoothing(ref velocity.Theta, Projectile.localAI[1], (targetVel - 0.5f) * 0.03f);
					changed = true;
				} else {
					Projectile.localAI[1] = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
				}

				if (changed) {
					Projectile.velocity = (Vector2)velocity;
				}
			}
			if (++Projectile.frameCounter > 5) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) {
					Projectile.frame = 0;
				}
			}
			Projectile.spriteDirection = 1;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.timeLeft > 0) {
				Projectile.Kill();
			}
			target.AddBuff(Buffs.Rasterized_Debuff.ID, 5);
		}
		public override void OnKill(int timeLeft) {
			if (timeLeft > 0) {
				ExplosiveGlobalProjectile.DoExplosion(Projectile, 32, sound: SoundID.Item46.WithPitchRange(0.2f, 0.3f), fireDustAmount: 0, smokeDustAmount: 5, smokeGoreAmount: 0);
			} else {
				Gore.NewGorePerfect(
					Projectile.GetSource_Death(),
					Projectile.position,
					Projectile.velocity * 2,
					Breach_P_Debris.ID
				);
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			const float fade_out_time = 10f;
			if (Projectile.timeLeft < fade_out_time) {
				Main.instance.LoadGore(Breach_P_Debris.ID);
				Texture2D texture = TextureAssets.Gore[Breach_P_Debris.ID].Value;
				Rectangle frame = texture.Frame(verticalFrames: 3, frameY: Projectile.frame);
				Main.EntitySpriteDraw(
					texture,
					Projectile.Center - Main.screenPosition,
					frame,
					lightColor,
					Projectile.rotation,
					frame.Size() * 0.5f,
					Projectile.scale,
					SpriteEffects.None
				);
				lightColor *= Projectile.timeLeft / fade_out_time;
			}
			return true;
		}
		public void OnSelfDamage(Player player, Player.HurtInfo info, double damageDealt) {
			Breach.SpawnManaStar(Projectile, damageDealt);
		}
	}
	public class Breach_P_Debris : ModGore {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void OnSpawn(Gore gore, IEntitySource source) {
			gore.Frame = new(1, 3, 0, (byte)Main.rand.Next(3));
		}
		public override bool Update(Gore gore) {
			gore.timeLeft -= 3;
			if (++gore.alpha > 255) gore.timeLeft = 0;
			return true;
		}
	}
}
