//using AltLibrary.Common.AltBiomes;
using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Ranged;
using Origins.NPCs;
using Origins.Questing;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Projectiles {
	public class OriginGlobalProj : GlobalProjectile {
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;
		public int fromItemType = -1;
		//bool init = true;
		public bool felnumEffect = false;
		public bool viperEffect = false;
		public bool ownerSafe = false;
		public int killLink = -1;
		public float godHunterEffect = 0f;
		public static bool felnumEffectNext = false;
		public static bool viperEffectNext = false;
		public static bool hostileNext = false;
		public static int killLinkNext = -1;
		public static int extraUpdatesNext = -1;
		public static float godHunterEffectNext = 0f;
		public bool isFromMitosis = false;
		public bool hasUsedMitosis = false;
		public int mitosisTimeLeft = 3600;
		public bool fiberglassLifesteal = false;
		public int prefix;
		public override void SetDefaults(Projectile projectile) {
			if (hostileNext) {
				projectile.hostile = true;
				hostileNext = false;
			}
			felnumEffect = felnumEffectNext;
			felnumEffectNext = false;
			if (killLinkNext != -1) {
				killLink = killLinkNext;
				//sync killLink ids
				Main.projectile[killLink].GetGlobalProjectile<OriginGlobalProj>().killLink = projectile.whoAmI;
				killLinkNext = -1;
			}
			if (viperEffectNext) {
				viperEffect = true;
				projectile.extraUpdates += 2;
				viperEffectNext = false;
			}
			if (extraUpdatesNext != 0) {
				projectile.extraUpdates += extraUpdatesNext;
				extraUpdatesNext = 0;
			}
			if (godHunterEffectNext != 0) {
				godHunterEffect = godHunterEffectNext;
				godHunterEffectNext = 0;
			}
			switch (ExplosiveGlobalProjectile.GetVanillaExplosiveType(projectile)) {
				case 1:
				projectile.DamageType = DamageClasses.ThrownExplosive;
				break;

				case 2:
				projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
				break;
			}
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (projectile.aiStyle is ProjAIStyleID.Explosive or ProjAIStyleID.Bobber or ProjAIStyleID.GolfBall && projectile.originalDamage < projectile.damage)
				projectile.originalDamage = projectile.damage;
			if (source is EntitySource_ItemUse itemUseSource) {
				if (itemUseSource.Item.ModItem is IElementalItem elementalItem && (elementalItem.Element & Elements.Fiberglass) != 0 &&
					itemUseSource.Entity is Player player && player.GetModPlayer<OriginPlayer>().entangledEnergy) {
					fiberglassLifesteal = true;
				}
				prefix = itemUseSource.Item.prefix;
				ModPrefix projPrefix = PrefixLoader.GetPrefix(prefix);

				if (projPrefix is IOnSpawnProjectilePrefix spawnPrefix) {
					spawnPrefix.OnProjectileSpawn(projectile, source);
				}

				fromItemType = itemUseSource.Item.type;
			} else if (source is EntitySource_Parent source_Parent) {
				if (source_Parent.Entity is Projectile parentProjectile) {
					if (parentProjectile.type == ModContent.ProjectileType<Amoeba_Bubble>()) {
						isFromMitosis = true;
						projectile.alpha = 100;
						if (projectile.minion) {
							mitosisTimeLeft = 300;
							projectile.minionSlots = 0;
						}
					}
					OriginGlobalProj parentGlobalProjectile = parentProjectile.GetGlobalProjectile<OriginGlobalProj>();
					prefix = parentGlobalProjectile.prefix;

					ModPrefix projPrefix = PrefixLoader.GetPrefix(prefix);

					if (projPrefix is IOnSpawnProjectilePrefix spawnPrefix) {
						spawnPrefix.OnProjectileSpawn(projectile, source);
					}
				} else if (source_Parent.Entity is NPC parentNPC) {
					if (parentNPC.GetGlobalNPC<OriginGlobalNPC>().soulhideWeakenedDebuff) {
						projectile.damage = (int)(projectile.damage * (1f - OriginGlobalNPC.soulhideWeakenAmount));
					}
				}
			}
		}
		public override void PostAI(Projectile projectile) {
			if (projectile.aiStyle is ProjAIStyleID.Explosive or ProjAIStyleID.Bobber or ProjAIStyleID.GolfBall)
				projectile.damage = projectile.originalDamage;
		}
		public override void AI(Projectile projectile) {
			switch (projectile.aiStyle) {
				case -1:
				projectile.rotation = projectile.velocity.ToRotation();
				break;
			}
			if (isFromMitosis) {
				Main.player[projectile.owner].ownedProjectileCounts[projectile.type]--;
				if (--mitosisTimeLeft <= 0) projectile.active = false;
			}
			if (hasUsedMitosis && projectile.minion && --mitosisTimeLeft <= 0) {
				hasUsedMitosis = false;
			}
			if (felnumEffect) {
				if (!ProjectileID.Sets.IsAWhip[projectile.type]) {
					if (projectile.CountsAsClass(DamageClass.Melee) || projectile.CountsAsClass(DamageClass.Summon)) {
						if (Main.player[projectile.owner].GetModPlayer<OriginPlayer>().felnumShock > 19)
							Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity.RotatedByRandom(0.1) * 0.5f, Scale: 0.5f);
					} else {
						Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity.RotatedByRandom(0.1) * 0.5f, Scale: 0.5f);
					}
				} else {

				}
			}
			if (viperEffect && projectile.extraUpdates != 19) {
				Lighting.AddLight(projectile.Center, 0, 0.75f * projectile.scale, 0.3f * projectile.scale);
				Dust dust = Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity.RotatedByRandom(0.1f) * -0.25f, 100, new Color(0, 255, 0), projectile.scale / 2);
				dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
				dust.noGravity = true;
				dust.noLight = true;
			}
		}
		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) {
			//this is actually how vanilla does projectile crits, which might explain why there are no vanilla multiclass weapons, since a 4% crit chance with a 4-class weapon would crit ~15% of the time
			if (viperEffect) {
				for (int i = 0; i < target.buffType.Length; i++) {
					if (Main.debuff[target.buffType[i]] && target.buffType[i] != Toxic_Shock_Debuff.ID) {
						modifiers.SetCrit();
						break;
					}
				}
			}
			if (target.boss && godHunterEffect != 0f) {
				modifiers.SourceDamage *= 1 + godHunterEffect;
			}
		}
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			if (fiberglassLifesteal) {
				Projectile.NewProjectile(
					projectile.GetSource_OnHit(target),
					target.Center,
					default,
					ModContent.ProjectileType<Entangled_Energy_Lifesteal>(),
					damageDone / 10,
					0,
					projectile.owner
				);
			}
			if (target.life <= 0 && prefix == ModContent.PrefixType<Imperfect_Prefix>()) {
				if (fromItemType == ModContent.ItemType<Shardcannon>()) {
					ModContent.GetInstance<Shardcannon_Quest>().UpdateKillCount();
				}
			}
			if (viperEffect) {
				if (hit.Crit || Main.rand.Next(0, 9) == 0) {
					target.AddBuff(Toxic_Shock_Debuff.ID, 450);
				}
			}
		}
		public override bool CanHitPlayer(Projectile projectile, Player target) {
			return ownerSafe ? target.whoAmI != projectile.owner : true;
		}
		public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info) {

		}
		public override bool PreKill(Projectile projectile, int timeLeft) {
			if (felnumEffect && projectile.type == ProjectileID.WaterGun) {//projectile.aiStyle==60
				OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
				Projectile.NewProjectileDirect(projectile.GetSource_FromThis(), projectile.Center, Vector2.Zero, ModContent.ProjectileType<Felnum_Shock_Grenade_Shock>(), (int)(originPlayer.felnumShock / 2.5f), projectile.knockBack, projectile.owner).timeLeft = 1;
				originPlayer.felnumShock = 0;
				SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), projectile.Center);
			}
			return true;
		}
		public override void Kill(Projectile projectile, int timeLeft) {
			if (killLink != -1 && projectile.penetrate == 0) {
				Main.projectile[killLink].active = false;
				killLink = -1;
			}
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			binaryWriter.Write(prefix);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			prefix = binaryReader.ReadInt32();
		}

#if false ///TODO: find a way
		public static void ClentaminatorAI<TBiome>(Projectile projectile, int dustType, Color color) where TBiome : AltBiome {
			if (projectile.owner == Main.myPlayer) {
				AltLibrary.Core.ALConvert.SimulateSolution<TBiome>(projectile);
			}
			if (projectile.timeLeft > 133) {
				projectile.timeLeft = 133;
			}
			if (projectile.ai[0] > 7f) {
				float scale = 1f;
				switch (projectile.ai[0]) {
					case 8f:
					scale = 0.2f;
					break;
					case 9f:
					scale = 0.4f;
					break;
					case 10f:
					scale = 0.6f;
					break;
					case 11f:
					scale = 0.8f;
					break;
				}
				projectile.ai[0]++;
				for (int num354 = 0; num354 < 1; num354++) {
					int d = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, dustType, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100, color);
					Main.dust[d].noGravity = true;
					Dust dust1 = Main.dust[d];
					Dust dust2 = dust1;
					dust2.scale *= 1.75f;
					Main.dust[d].velocity.X *= 2f;
					Main.dust[d].velocity.Y *= 2f;
					dust1 = Main.dust[d];
					dust2 = dust1;
					dust2.scale *= scale;
				}
			} else {
				projectile.ai[0]++;
			}
			projectile.rotation += 0.3f * projectile.direction;
		}
#endif
	}
}
