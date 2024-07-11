using Microsoft.Xna.Framework;
using Origins.Items;
using Origins.Items.Armor.Amber;
using Origins.Items.Weapons.Demolitionist;
using Origins.Projectiles.Weapons;
using Origins.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Projectiles {
	public class ExplosiveGlobalProjectile : GlobalProjectile {
		public bool isHoming = false;
		public bool magicTripwire = false;
		public bool magicTripwireTripped = false;
		public bool noTileSplode = false;
		public bool acridHandcannon = false;
		public bool novaCascade = false;
		public bool novaSwarm = false;
		public bool hasAmber = false;
		public bool fromDeath = false;
		public StatModifier selfDamageModifier = StatModifier.Default;
		public StatModifier modifierBlastRadius = StatModifier.Default;
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;
		public static List<Vector2> explodingProjectiles = [];
		public static List<Vector2> nextExplodingProjectiles = [];
		public override void Unload() {
			explodingProjectiles = null;
			nextExplodingProjectiles = null;
		}
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.DamageType.CountsAsClass(DamageClasses.Explosive) || GetVanillaExplosiveType(entity) > 0;
		}
		public override void SetDefaults(Projectile projectile) {
			isHoming = false;
		}
		public override void AI(Projectile projectile) {
			if (isHoming && !projectile.minion) {
				float targetWeight = 300;
				Vector2 targetPos = default;
				bool foundTarget = false;
				for (int i = 0; i < 200; i++) {
					NPC currentNPC = Main.npc[i];
					if (currentNPC.CanBeChasedBy(this)) {
						Vector2 currentPos = currentNPC.Center;
						float num21 = Math.Abs(projectile.Center.X - currentPos.X) + Math.Abs(projectile.Center.Y - currentPos.Y);
						if (num21 < targetWeight && Collision.CanHit(projectile.position, projectile.width, projectile.height, currentNPC.position, currentNPC.width, currentNPC.height)) {
							targetWeight = num21;
							targetPos = currentPos;
							foundTarget = true;
						}
					}
				}

				if (foundTarget) {

					float scaleFactor = 16f * Origins.HomingEffectivenessMultiplier[projectile.type];

					Vector2 targetVelocity = (targetPos - projectile.Center).SafeNormalize(-Vector2.UnitY) * scaleFactor;
					projectile.velocity = Vector2.Lerp(projectile.velocity, targetVelocity, 0.083333336f);
				}
			}
			if (novaSwarm) {
				const float force = 1;
				float angle = projectile.velocity.ToRotation();
				projectile.rotation = angle + MathHelper.PiOver2;
				float targetOffset = 0.9f;
				float targetAngle = 1;
				NPC target;
				float dist = 641;
				for (int i = 0; i < Main.npc.Length; i++) {
					target = Main.npc[i];
					if (!target.CanBeChasedBy()) continue;
					Vector2 toHit = (projectile.Center.Clamp(target.Hitbox.Add(target.velocity)) - projectile.Center);
					if (!Collision.CanHitLine(projectile.Center + projectile.velocity, 1, 1, projectile.Center + toHit, 1, 1)) continue;
					float tdist = toHit.Length();
					float ta = (float)Math.Abs(Tyfyter.Utils.GeometryUtils.AngleDif(toHit.ToRotation(), angle, out _));
					if (tdist <= dist && ta <= targetOffset) {
						targetAngle = ((target.Center + target.velocity) - projectile.Center).ToRotation();
						targetOffset = ta;
						dist = tdist;
					}
				}
				if (dist < 641) projectile.velocity = (projectile.velocity + new Vector2(force, 0).RotatedBy(targetAngle)).SafeNormalize(Vector2.Zero) * projectile.velocity.Length();
			}
			if (magicTripwire && Origins.MagicTripwireRange[projectile.type] > 0) {
				int magicTripwireRange = Origins.MagicTripwireRange[projectile.type];
				Rectangle magicTripwireHitbox = new(
					(int)projectile.Center.X - magicTripwireRange,
					(int)projectile.Center.Y - magicTripwireRange,
					magicTripwireRange * 2,
					magicTripwireRange * 2
				);
				bool tripped = false;
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy() && magicTripwireHitbox.Intersects(npc.Hitbox)) {
						tripped = true;
					}
				}
				if (tripped) {
					magicTripwireTripped = true;
				} else if (magicTripwireTripped) {
					Explode(projectile, detonationStyle: Origins.MagicTripwireDetonationStyle[projectile.type]);
				}
			}
			/*if (IsExploding(projectile)) {
				//nextExplodingProjectiles.Add(projectile.Center);
			}*/
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			Player player = Main.player[projectile.owner];
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (originPlayer.novaSet && Origins.HomingEffectivenessMultiplier[projectile.type] != 0) {
				isHoming = true;
			}
			if (originPlayer.explosiveFuseTime != StatModifier.Default) {
				projectile.timeLeft = (int)(originPlayer.explosiveFuseTime.ApplyTo(projectile.timeLeft));
			}
			if (originPlayer.magicTripwire) {
				magicTripwire = true;
			}
			if (originPlayer.pincushion) {
				noTileSplode = true;
			}
			if (originPlayer.amberSet) {
				hasAmber = true;
				if (!IsExploding(projectile)) {
					Amber_Debuff_Shard.SpawnDusts(projectile);
				}
			}
			if (source is EntitySource_Death) fromDeath = true;
			else if (source is EntitySource_ItemUse itemUse) {
				if (PrefixLoader.GetPrefix(itemUse.Item.prefix) is IBlastRadiusPrefix brPrefix) {
					modifierBlastRadius = brPrefix.BlastRadius();
				}
				if (PrefixLoader.GetPrefix(itemUse.Item.prefix) is ISelfDamagePrefix sdPrefix) {
					selfDamageModifier = selfDamageModifier.CombineWith(sdPrefix.SelfDamage());
				}
				acridHandcannon = itemUse.Item.type == Acrid_Handcannon.ID;
				novaCascade = itemUse.Item.type == Nova_Cascade.ID;
				novaSwarm = itemUse.Item.type == Nova_Swarm.ID;
				if (novaSwarm) projectile.scale *= Nova_Swarm.rocket_scale;
			} else if (source is EntitySource_Parent sourceParent && sourceParent.Entity is Projectile parent && parent.TryGetGlobalProjectile(out ExplosiveGlobalProjectile parentGlobal)) {
				modifierBlastRadius = parentGlobal.modifierBlastRadius;
				selfDamageModifier = selfDamageModifier.CombineWith(parentGlobal.modifierBlastRadius);
				novaCascade = parentGlobal.novaCascade;
				novaSwarm = parentGlobal.novaSwarm;
				if (!novaSwarm) projectile.scale *= Nova_Swarm.rocket_scale;
				noTileSplode = parentGlobal.noTileSplode;
			}
		}
		public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox) {
			OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
			if (IsExploding(projectile)) {
				nextExplodingProjectiles.Add(projectile.Center);
				if (modifierBlastRadius != StatModifier.Default) {
					StatModifier modifier = modifierBlastRadius.Scale(additive: 0.5f, multiplicative: 0.5f);
					hitbox.Inflate((int)(modifier.ApplyTo(hitbox.Width) - hitbox.Width), (int)(modifier.ApplyTo(hitbox.Height) - hitbox.Height));
				}
				if (originPlayer.explosiveBlastRadius != StatModifier.Default) {
					StatModifier modifier = originPlayer.explosiveBlastRadius.Scale(additive: 0.5f, multiplicative: 0.5f);
					hitbox.Inflate((int)(modifier.ApplyTo(hitbox.Width) - hitbox.Width), (int)(modifier.ApplyTo(hitbox.Height) - hitbox.Height));
				}
			}
			switch (projectile.type) {
				case ProjectileID.Bomb:
				case ProjectileID.StickyBomb:
				case ProjectileID.Dynamite:
				case ProjectileID.StickyDynamite:
				case ProjectileID.BombFish:
				case ProjectileID.DryBomb:
				case ProjectileID.WetBomb:
				case ProjectileID.LavaBomb:
				case ProjectileID.HoneyBomb:
				if (hitbox.Width < 32) {
					hitbox = default;
				}
				break;
			}
		}
		public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox) {
			if (projectile.type == ProjectileID.ScarabBomb) {
				if (projectile.timeLeft <= 1) {
					Point scarabBombDigDirectionSnap = ProjectileMethods.GetScarabBombDigDirectionSnap8(projectile);
					bool axisAligned = scarabBombDigDirectionSnap.X == 0 || scarabBombDigDirectionSnap.Y == 0;
					projHitbox.Inflate((48 - projHitbox.Width) / 2, (48 - projHitbox.Height) / 2);
					for (int i = axisAligned ? 21 : 15; i-->0;) {
						if (projHitbox.Intersects(targetHitbox)) return true;
						projHitbox.X += scarabBombDigDirectionSnap.X * 16;
						projHitbox.Y += scarabBombDigDirectionSnap.Y * 16;
					}
					return false;
				} else {
					return false;
				}
			}
			return null;
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			bitWriter.WriteBit(isHoming);
			bitWriter.WriteBit(magicTripwire);
			bitWriter.WriteBit(magicTripwireTripped);
			bitWriter.WriteBit(noTileSplode);
			bitWriter.WriteBit(hasAmber);
			bitWriter.WriteBit(fromDeath);
			bitWriter.WriteBit(novaSwarm);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			isHoming = bitReader.ReadBit();
			magicTripwire = bitReader.ReadBit();
			magicTripwireTripped = bitReader.ReadBit();
			noTileSplode = bitReader.ReadBit();
			if (bitReader.ReadBit()) {
				if (!hasAmber) Amber_Debuff_Shard.SpawnDusts(projectile);
				hasAmber = true;
			} else {
				hasAmber = false;
			}
			fromDeath = bitReader.ReadBit();
			if (bitReader.ReadBit()) {
				if (!novaSwarm) projectile.scale *= Nova_Swarm.rocket_scale;
				novaSwarm = true;
			} else {
				if (novaSwarm) projectile.scale /= Nova_Swarm.rocket_scale;
				novaSwarm = false;
			}
		}
		public static bool IsExploding(Projectile projectile) {
			if (!projectile.CountsAsClass(DamageClasses.Explosive)) return false;
			if (projectile.ModProjectile is IIsExplodingProjectile explodingProjectile) {
				return explodingProjectile.IsExploding();
			}
			switch (projectile.type) {
				case ProjectileID.VolatileGelatinBall:
				case ProjectileID.DD2ExplosiveTrapT1:
				case ProjectileID.DD2ExplosiveTrapT2:
				case ProjectileID.DD2ExplosiveTrapT3:
				return false;

				case ProjectileID.FireWhipProj:
				case ProjectileID.Volcano:
				case ProjectileID.DD2ExplosiveTrapT1Explosion:
				case ProjectileID.DD2ExplosiveTrapT2Explosion:
				case ProjectileID.DD2ExplosiveTrapT3Explosion:
				return true;

				default:
				return projectile.timeLeft <= 3 || projectile.penetrate == 0;
			}
		}
		public static void Explode(Projectile projectile, int delay = 0, int detonationStyle = -1) {
			if (projectile.ModProjectile is IIsExplodingProjectile explodingProjectile) {
				explodingProjectile.Explode(delay);
			} else {
				if (detonationStyle == -1) {
					switch (projectile.type) {
						case ProjectileID.Grenade:
						case ProjectileID.BouncyGrenade:
						case ProjectileID.StickyGrenade:
						case ProjectileID.PartyGirlGrenade:
						case ProjectileID.Beenade:
						case ProjectileID.Bomb:
						case ProjectileID.BouncyBomb:
						case ProjectileID.StickyBomb:
						case ProjectileID.Dynamite:
						case ProjectileID.BouncyDynamite:
						case ProjectileID.StickyDynamite:
						case ProjectileID.BombFish:
						case ProjectileID.DryBomb:
						case ProjectileID.WetBomb:
						case ProjectileID.LavaBomb:
						case ProjectileID.HoneyBomb:
						case ProjectileID.ScarabBomb:
						case ProjectileID.MolotovCocktail:
						detonationStyle = 1;
						break;

						case ProjectileID.RocketI:
						case ProjectileID.RocketII:
						case ProjectileID.RocketIII:
						case ProjectileID.RocketIV:
						case ProjectileID.MiniNukeRocketI:
						case ProjectileID.MiniNukeRocketII:
						case ProjectileID.ClusterRocketI:
						case ProjectileID.ClusterRocketII:
						case ProjectileID.DryRocket:
						case ProjectileID.WetRocket:
						case ProjectileID.LavaRocket:
						case ProjectileID.HoneyRocket:

						case ProjectileID.ProximityMineI:
						case ProjectileID.ProximityMineII:
						case ProjectileID.ProximityMineIII:
						case ProjectileID.ProximityMineIV:
						case ProjectileID.MiniNukeMineI:
						case ProjectileID.MiniNukeMineII:
						case ProjectileID.ClusterMineI:
						case ProjectileID.ClusterMineII:
						case ProjectileID.DryMine:
						case ProjectileID.WetMine:
						case ProjectileID.LavaMine:
						case ProjectileID.HoneyMine:

						case ProjectileID.GrenadeI:
						case ProjectileID.GrenadeII:
						case ProjectileID.GrenadeIII:
						case ProjectileID.GrenadeIV:
						case ProjectileID.MiniNukeGrenadeI:
						case ProjectileID.MiniNukeGrenadeII:
						case ProjectileID.ClusterGrenadeI:
						case ProjectileID.ClusterGrenadeII:
						case ProjectileID.DryGrenade:
						case ProjectileID.WetGrenade:
						case ProjectileID.LavaGrenade:
						case ProjectileID.HoneyGrenade:
						detonationStyle = 2;
						break;

						case ProjectileID.RocketSnowmanI:
						case ProjectileID.RocketSnowmanII:
						case ProjectileID.RocketSnowmanIII:
						case ProjectileID.RocketSnowmanIV:
						case ProjectileID.MiniNukeSnowmanRocketI:
						case ProjectileID.MiniNukeSnowmanRocketII:
						case ProjectileID.ClusterSnowmanRocketI:
						case ProjectileID.ClusterSnowmanRocketII:
						case ProjectileID.DrySnowmanRocket:
						case ProjectileID.WetSnowmanRocket:
						case ProjectileID.LavaSnowmanRocket:
						case ProjectileID.HoneySnowmanRocket:

						case ProjectileID.RocketFireworkBlue:
						case ProjectileID.RocketFireworkGreen:
						case ProjectileID.RocketFireworkRed:
						case ProjectileID.RocketFireworkYellow:

						case ProjectileID.Celeb2Rocket:
						case ProjectileID.Celeb2RocketExplosive:
						case ProjectileID.Celeb2RocketLarge:
						case ProjectileID.Celeb2RocketExplosiveLarge:

						case ProjectileID.ElectrosphereMissile:

						case ProjectileID.ClusterFragmentsI:
						case ProjectileID.ClusterFragmentsII:
						case ProjectileID.ClusterSnowmanFragmentsI:
						case ProjectileID.ClusterSnowmanFragmentsII:
						case ProjectileID.HellfireArrow:
						case ProjectileID.Stynger:
						case ProjectileID.StyngerShrapnel:
						case ProjectileID.JackOLantern:
						default:
						detonationStyle = 0;
						break;
					}
				}
				switch (detonationStyle) {
					case 1:
					delay += 3;
					goto default;

					case 2:
					projectile.velocity = Vector2.Zero;
					delay += 3;
					goto default;

					default:
					if (projectile.timeLeft > delay) projectile.timeLeft = delay;
					break;
				}
			}
		}
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			if (novaCascade) {
				int type = ModContent.ProjectileType<Nova_Cascade_Explosion>();
				if (projectile.type != type) {
					Projectile.NewProjectile(
						projectile.GetSource_OnHit(target),
						projectile.Center,
						default,
						type,
						projectile.damage,
						projectile.knockBack
					);
				}
			}
		}
		public override void OnKill(Projectile projectile, int timeLeft) {
			if (!fromDeath && projectile.owner == Main.myPlayer) {
				if (acridHandcannon) {
					const int count = 6;
					const float portion = MathHelper.TwoPi / count;
					int projType = ModContent.ProjectileType<Acid_Shot>();
					for (int i = 0; i < count; i++) {
						Projectile.NewProjectile(
							projectile.GetSource_Death(),
							projectile.Center,
							new Vector2(6, 0).RotatedByRandom(i * portion + Main.rand.NextFloat(-0.1f, 0.1f)),
							projType,
							projectile.damage / 3,
							projectile.knockBack / 4
						);
					}
				}
				if (novaCascade) {
					int type = ModContent.ProjectileType<Nova_Cascade_Explosion>();
					if (projectile.type != type) {
						Projectile.NewProjectile(
							projectile.GetSource_Death(),
							projectile.Center,
							default,
							type,
							projectile.damage,
							projectile.knockBack
						);
					}
				}
				if (hasAmber) {
					const int count = 6;
					const float portion = MathHelper.TwoPi / count;
					int projType = ModContent.ProjectileType<Amber_Shard>();
					for (int i = 0; i < count; i++) {
						Projectile.NewProjectile(
							projectile.GetSource_Death(),
							projectile.Center,
							new Vector2(6, 0).RotatedByRandom(i * portion + Main.rand.NextFloat(-0.1f, 0.1f)),
							projType,
							projectile.damage / 4,
							projectile.knockBack / 8
						);
					}
				}
			}
		}
		public static int GetVanillaExplosiveType(Projectile projectile) {
			switch (projectile.type) {
				case ProjectileID.Grenade:
				case ProjectileID.BouncyGrenade:
				case ProjectileID.StickyGrenade:
				case ProjectileID.PartyGirlGrenade:
				case ProjectileID.Beenade:
				case ProjectileID.Bomb:
				case ProjectileID.BouncyBomb:
				case ProjectileID.StickyBomb:
				case ProjectileID.Dynamite:
				case ProjectileID.BouncyDynamite:
				case ProjectileID.StickyDynamite:
				case ProjectileID.BombFish:
				case ProjectileID.DryBomb:
				case ProjectileID.WetBomb:
				case ProjectileID.LavaBomb:
				case ProjectileID.HoneyBomb:
				case ProjectileID.ScarabBomb:
				case ProjectileID.MolotovCocktail:
				return 1;

				case ProjectileID.RocketI:
				case ProjectileID.RocketII:
				case ProjectileID.RocketIII:
				case ProjectileID.RocketIV:
				case ProjectileID.MiniNukeRocketI:
				case ProjectileID.MiniNukeRocketII:
				case ProjectileID.ClusterRocketI:
				case ProjectileID.ClusterRocketII:
				case ProjectileID.DryRocket:
				case ProjectileID.WetRocket:
				case ProjectileID.LavaRocket:
				case ProjectileID.HoneyRocket:

				case ProjectileID.ProximityMineI:
				case ProjectileID.ProximityMineII:
				case ProjectileID.ProximityMineIII:
				case ProjectileID.ProximityMineIV:
				case ProjectileID.MiniNukeMineI:
				case ProjectileID.MiniNukeMineII:
				case ProjectileID.ClusterMineI:
				case ProjectileID.ClusterMineII:
				case ProjectileID.DryMine:
				case ProjectileID.WetMine:
				case ProjectileID.LavaMine:
				case ProjectileID.HoneyMine:

				case ProjectileID.GrenadeI:
				case ProjectileID.GrenadeII:
				case ProjectileID.GrenadeIII:
				case ProjectileID.GrenadeIV:
				case ProjectileID.MiniNukeGrenadeI:
				case ProjectileID.MiniNukeGrenadeII:
				case ProjectileID.ClusterGrenadeI:
				case ProjectileID.ClusterGrenadeII:
				case ProjectileID.DryGrenade:
				case ProjectileID.WetGrenade:
				case ProjectileID.LavaGrenade:
				case ProjectileID.HoneyGrenade:

				case ProjectileID.RocketSnowmanI:
				case ProjectileID.RocketSnowmanII:
				case ProjectileID.RocketSnowmanIII:
				case ProjectileID.RocketSnowmanIV:
				case ProjectileID.MiniNukeSnowmanRocketI:
				case ProjectileID.MiniNukeSnowmanRocketII:
				case ProjectileID.ClusterSnowmanRocketI:
				case ProjectileID.ClusterSnowmanRocketII:
				case ProjectileID.DrySnowmanRocket:
				case ProjectileID.WetSnowmanRocket:
				case ProjectileID.LavaSnowmanRocket:
				case ProjectileID.HoneySnowmanRocket:

				case ProjectileID.RocketFireworkBlue:
				case ProjectileID.RocketFireworkGreen:
				case ProjectileID.RocketFireworkRed:
				case ProjectileID.RocketFireworkYellow:

				case ProjectileID.Celeb2Rocket:
				case ProjectileID.Celeb2RocketExplosive:
				case ProjectileID.Celeb2RocketLarge:
				case ProjectileID.Celeb2RocketExplosiveLarge:

				case ProjectileID.ElectrosphereMissile:

				case ProjectileID.ClusterFragmentsI:
				case ProjectileID.ClusterFragmentsII:
				case ProjectileID.ClusterSnowmanFragmentsI:
				case ProjectileID.ClusterSnowmanFragmentsII:
				case ProjectileID.HellfireArrow:
				case ProjectileID.Stynger:
				case ProjectileID.StyngerShrapnel:
				case ProjectileID.JackOLantern:
				return 2;

				case ProjectileID.Volcano:
				return 3;

				case ProjectileID.FireWhipProj:
				return 4;

				case ProjectileID.DD2ExplosiveTrapT1:
				case ProjectileID.DD2ExplosiveTrapT1Explosion:
				case ProjectileID.DD2ExplosiveTrapT2:
				case ProjectileID.DD2ExplosiveTrapT2Explosion:
				case ProjectileID.DD2ExplosiveTrapT3:
				case ProjectileID.DD2ExplosiveTrapT3Explosion:
				return 5;

				default:
				return 0;
			}
		}
		internal static void SetupMagicTripwireRanges(int[] magicTripwireRange, int[] magicTripwireDetonationStyle) {
			magicTripwireRange[ProjectileID.Grenade] = 32;
			magicTripwireRange[ProjectileID.BouncyGrenade] = 32;
			magicTripwireRange[ProjectileID.StickyGrenade] = 32;
			magicTripwireRange[ProjectileID.PartyGirlGrenade] = 32;
			magicTripwireRange[ProjectileID.Beenade] = 32;
			magicTripwireRange[ProjectileID.Bomb] = 64;
			magicTripwireRange[ProjectileID.BouncyBomb] = 64;
			magicTripwireRange[ProjectileID.StickyBomb] = 64;
			magicTripwireRange[ProjectileID.Dynamite] = 64;
			magicTripwireRange[ProjectileID.BouncyDynamite] = 64;
			magicTripwireRange[ProjectileID.StickyDynamite] = 64;
			magicTripwireRange[ProjectileID.BombFish] = 64;
			magicTripwireRange[ProjectileID.DryBomb] = 12;
			magicTripwireRange[ProjectileID.WetBomb] = 12;
			magicTripwireRange[ProjectileID.LavaBomb] = 12;
			magicTripwireRange[ProjectileID.HoneyBomb] = 12;
			magicTripwireRange[ProjectileID.ScarabBomb] = 0;
			magicTripwireRange[ProjectileID.MolotovCocktail] = 64;

			magicTripwireRange[ProjectileID.RocketI] = 32;
			magicTripwireRange[ProjectileID.RocketII] = 32;
			magicTripwireRange[ProjectileID.RocketIII] = 64;
			magicTripwireRange[ProjectileID.RocketIV] = 64;
			magicTripwireRange[ProjectileID.MiniNukeRocketI] = 64;
			magicTripwireRange[ProjectileID.MiniNukeRocketII] = 64;
			magicTripwireRange[ProjectileID.ClusterRocketI] = 32;
			magicTripwireRange[ProjectileID.ClusterRocketII] = 32;
			magicTripwireRange[ProjectileID.DryRocket] = 32;
			magicTripwireRange[ProjectileID.WetRocket] = 32;
			magicTripwireRange[ProjectileID.LavaRocket] = 32;
			magicTripwireRange[ProjectileID.HoneyRocket] = 32;

			magicTripwireRange[ProjectileID.ProximityMineI] = 32;
			magicTripwireRange[ProjectileID.ProximityMineII] = 32;
			magicTripwireRange[ProjectileID.ProximityMineIII] = 64;
			magicTripwireRange[ProjectileID.ProximityMineIV] = 64;
			magicTripwireRange[ProjectileID.MiniNukeMineI] = 64;
			magicTripwireRange[ProjectileID.MiniNukeMineII] = 64;
			magicTripwireRange[ProjectileID.ClusterMineI] = 32;
			magicTripwireRange[ProjectileID.ClusterMineII] = 32;
			magicTripwireRange[ProjectileID.DryMine] = 32;
			magicTripwireRange[ProjectileID.WetMine] = 32;
			magicTripwireRange[ProjectileID.LavaMine] = 32;
			magicTripwireRange[ProjectileID.HoneyMine] = 32;

			magicTripwireRange[ProjectileID.GrenadeI] = 32;
			magicTripwireRange[ProjectileID.GrenadeII] = 32;
			magicTripwireRange[ProjectileID.GrenadeIII] = 64;
			magicTripwireRange[ProjectileID.GrenadeIV] = 64;
			magicTripwireRange[ProjectileID.MiniNukeGrenadeI] = 64;
			magicTripwireRange[ProjectileID.MiniNukeGrenadeII] = 64;
			magicTripwireRange[ProjectileID.ClusterGrenadeI] = 32;
			magicTripwireRange[ProjectileID.ClusterGrenadeII] = 32;
			magicTripwireRange[ProjectileID.DryGrenade] = 32;
			magicTripwireRange[ProjectileID.WetGrenade] = 32;
			magicTripwireRange[ProjectileID.LavaGrenade] = 32;
			magicTripwireRange[ProjectileID.HoneyGrenade] = 32;

			magicTripwireRange[ProjectileID.RocketSnowmanI] = 32;
			magicTripwireRange[ProjectileID.RocketSnowmanII] = 32;
			magicTripwireRange[ProjectileID.RocketSnowmanIII] = 64;
			magicTripwireRange[ProjectileID.RocketSnowmanIV] = 64;
			magicTripwireRange[ProjectileID.MiniNukeSnowmanRocketI] = 64;
			magicTripwireRange[ProjectileID.MiniNukeSnowmanRocketII] = 64;
			magicTripwireRange[ProjectileID.ClusterSnowmanRocketI] = 32;
			magicTripwireRange[ProjectileID.ClusterSnowmanRocketII] = 32;
			magicTripwireRange[ProjectileID.DrySnowmanRocket] = 32;
			magicTripwireRange[ProjectileID.WetSnowmanRocket] = 32;
			magicTripwireRange[ProjectileID.LavaSnowmanRocket] = 32;
			magicTripwireRange[ProjectileID.HoneySnowmanRocket] = 32;

			magicTripwireRange[ProjectileID.RocketFireworkBlue] = 64;
			magicTripwireRange[ProjectileID.RocketFireworkGreen] = 64;
			magicTripwireRange[ProjectileID.RocketFireworkRed] = 64;
			magicTripwireRange[ProjectileID.RocketFireworkYellow] = 64;

			magicTripwireRange[ProjectileID.Celeb2Rocket] = 64;
			magicTripwireRange[ProjectileID.Celeb2RocketExplosive] = 64;
			magicTripwireRange[ProjectileID.Celeb2RocketLarge] = 64;
			magicTripwireRange[ProjectileID.Celeb2RocketExplosiveLarge] = 64;

			magicTripwireRange[ProjectileID.ElectrosphereMissile] = 32;

			magicTripwireRange[ProjectileID.HellfireArrow] = 8;
			magicTripwireRange[ProjectileID.Stynger] = 12;
			magicTripwireRange[ProjectileID.JackOLantern] = 32;

			
			magicTripwireDetonationStyle[ProjectileID.RocketSnowmanI] = 0;
			magicTripwireDetonationStyle[ProjectileID.RocketSnowmanII] = 0;
			magicTripwireDetonationStyle[ProjectileID.RocketSnowmanIII] = 0;
			magicTripwireDetonationStyle[ProjectileID.RocketSnowmanIV] = 0;
			magicTripwireDetonationStyle[ProjectileID.MiniNukeSnowmanRocketI] = 0;
			magicTripwireDetonationStyle[ProjectileID.MiniNukeSnowmanRocketII] = 0;
			magicTripwireDetonationStyle[ProjectileID.ClusterSnowmanRocketI] = 0;
			magicTripwireDetonationStyle[ProjectileID.ClusterSnowmanRocketII] = 0;
			magicTripwireDetonationStyle[ProjectileID.DrySnowmanRocket] = 0;
			magicTripwireDetonationStyle[ProjectileID.WetSnowmanRocket] = 0;
			magicTripwireDetonationStyle[ProjectileID.LavaSnowmanRocket] = 0;
			magicTripwireDetonationStyle[ProjectileID.HoneySnowmanRocket] = 0;

			magicTripwireDetonationStyle[ProjectileID.RocketFireworkBlue] = 0;
			magicTripwireDetonationStyle[ProjectileID.RocketFireworkGreen] = 0;
			magicTripwireDetonationStyle[ProjectileID.RocketFireworkRed] = 0;
			magicTripwireDetonationStyle[ProjectileID.RocketFireworkYellow] = 0;

			magicTripwireDetonationStyle[ProjectileID.Celeb2Rocket] = 0;
			magicTripwireDetonationStyle[ProjectileID.Celeb2RocketExplosive] = 0;
			magicTripwireDetonationStyle[ProjectileID.Celeb2RocketLarge] = 0;
			magicTripwireDetonationStyle[ProjectileID.Celeb2RocketExplosiveLarge] = 0;

			magicTripwireDetonationStyle[ProjectileID.ElectrosphereMissile] = 0;

			magicTripwireDetonationStyle[ProjectileID.ClusterFragmentsI] = 0;
			magicTripwireDetonationStyle[ProjectileID.ClusterFragmentsII] = 0;
			magicTripwireDetonationStyle[ProjectileID.ClusterSnowmanFragmentsI] = 0;
			magicTripwireDetonationStyle[ProjectileID.ClusterSnowmanFragmentsII] = 0;
			magicTripwireDetonationStyle[ProjectileID.HellfireArrow] = 0;
			magicTripwireDetonationStyle[ProjectileID.Stynger] = 0;
			magicTripwireDetonationStyle[ProjectileID.StyngerShrapnel] = 0;
			magicTripwireDetonationStyle[ProjectileID.JackOLantern] = 0;

			magicTripwireDetonationStyle[ProjectileID.Grenade] = 1;
			magicTripwireDetonationStyle[ProjectileID.BouncyGrenade] = 1;
			magicTripwireDetonationStyle[ProjectileID.StickyGrenade] = 1;
			magicTripwireDetonationStyle[ProjectileID.PartyGirlGrenade] = 1;
			magicTripwireDetonationStyle[ProjectileID.Beenade] = 1;
			magicTripwireDetonationStyle[ProjectileID.Bomb] = 1;
			magicTripwireDetonationStyle[ProjectileID.BouncyBomb] = 1;
			magicTripwireDetonationStyle[ProjectileID.StickyBomb] = 1;
			magicTripwireDetonationStyle[ProjectileID.Dynamite] = 1;
			magicTripwireDetonationStyle[ProjectileID.BouncyDynamite] = 1;
			magicTripwireDetonationStyle[ProjectileID.StickyDynamite] = 1;
			magicTripwireDetonationStyle[ProjectileID.BombFish] = 1;
			magicTripwireDetonationStyle[ProjectileID.DryBomb] = 1;
			magicTripwireDetonationStyle[ProjectileID.WetBomb] = 1;
			magicTripwireDetonationStyle[ProjectileID.LavaBomb] = 1;
			magicTripwireDetonationStyle[ProjectileID.HoneyBomb] = 1;
			magicTripwireDetonationStyle[ProjectileID.ScarabBomb] = 1;
			magicTripwireDetonationStyle[ProjectileID.MolotovCocktail] = 1;
			
			magicTripwireDetonationStyle[ProjectileID.RocketI] = 2;
			magicTripwireDetonationStyle[ProjectileID.RocketII] = 2;
			magicTripwireDetonationStyle[ProjectileID.RocketIII] = 2;
			magicTripwireDetonationStyle[ProjectileID.RocketIV] = 2;
			magicTripwireDetonationStyle[ProjectileID.MiniNukeRocketI] = 2;
			magicTripwireDetonationStyle[ProjectileID.MiniNukeRocketII] = 2;
			magicTripwireDetonationStyle[ProjectileID.ClusterRocketI] = 2;
			magicTripwireDetonationStyle[ProjectileID.ClusterRocketII] = 2;
			magicTripwireDetonationStyle[ProjectileID.DryRocket] = 2;
			magicTripwireDetonationStyle[ProjectileID.WetRocket] = 2;
			magicTripwireDetonationStyle[ProjectileID.LavaRocket] = 2;
			magicTripwireDetonationStyle[ProjectileID.HoneyRocket] = 2;

			magicTripwireDetonationStyle[ProjectileID.ProximityMineI] = 2;
			magicTripwireDetonationStyle[ProjectileID.ProximityMineII] = 2;
			magicTripwireDetonationStyle[ProjectileID.ProximityMineIII] = 2;
			magicTripwireDetonationStyle[ProjectileID.ProximityMineIV] = 2;
			magicTripwireDetonationStyle[ProjectileID.MiniNukeMineI] = 2;
			magicTripwireDetonationStyle[ProjectileID.MiniNukeMineII] = 2;
			magicTripwireDetonationStyle[ProjectileID.ClusterMineI] = 2;
			magicTripwireDetonationStyle[ProjectileID.ClusterMineII] = 2;
			magicTripwireDetonationStyle[ProjectileID.DryMine] = 2;
			magicTripwireDetonationStyle[ProjectileID.WetMine] = 2;
			magicTripwireDetonationStyle[ProjectileID.LavaMine] = 2;
			magicTripwireDetonationStyle[ProjectileID.HoneyMine] = 2;

			magicTripwireDetonationStyle[ProjectileID.GrenadeI] = 2;
			magicTripwireDetonationStyle[ProjectileID.GrenadeII] = 2;
			magicTripwireDetonationStyle[ProjectileID.GrenadeIII] = 2;
			magicTripwireDetonationStyle[ProjectileID.GrenadeIV] = 2;
			magicTripwireDetonationStyle[ProjectileID.MiniNukeGrenadeI] = 2;
			magicTripwireDetonationStyle[ProjectileID.MiniNukeGrenadeII] = 2;
			magicTripwireDetonationStyle[ProjectileID.ClusterGrenadeI] = 2;
			magicTripwireDetonationStyle[ProjectileID.ClusterGrenadeII] = 2;
			magicTripwireDetonationStyle[ProjectileID.DryGrenade] = 2;
			magicTripwireDetonationStyle[ProjectileID.WetGrenade] = 2;
			magicTripwireDetonationStyle[ProjectileID.LavaGrenade] = 2;
			magicTripwireDetonationStyle[ProjectileID.HoneyGrenade] = 2;
		}
		public static void DoExplosion(Projectile projectile, int size, bool dealSelfDamage = true, SoundStyle? sound = null, int fireDustAmount = 20, int smokeDustAmount = 30, int smokeGoreAmount = 2, int fireDustType = DustID.Torch) {
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = size;
			projectile.height = size;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
			if (dealSelfDamage) DealSelfDamage(projectile);
			if (fireDustType == -1) fireDustAmount = 0;
			ExplosionVisual(projectile, true, sound: sound, fireDustAmount: fireDustAmount, smokeDustAmount: smokeDustAmount, smokeGoreAmount: smokeGoreAmount, fireDustType: fireDustType);
		}
		public static void ExplosionVisual(Projectile projectile, bool applyHitboxModifiers, bool adjustDustAmount = false, SoundStyle? sound = null, int fireDustAmount = 20, int smokeDustAmount = 30, int smokeGoreAmount = 2, bool debugOutline = false, int fireDustType = DustID.Torch) {
			if (applyHitboxModifiers) {
				Rectangle hitbox = projectile.Hitbox;
				float baseWidth = hitbox.Width;
				ProjectileLoader.ModifyDamageHitbox(projectile, ref hitbox);
				if (adjustDustAmount) {
					float dustMult = hitbox.Width / baseWidth;
					fireDustAmount = (int)(fireDustAmount * dustMult);
					smokeDustAmount = (int)(smokeDustAmount * dustMult);
				}
				ExplosionVisual(hitbox, sound, fireDustAmount, smokeDustAmount, smokeGoreAmount, debugOutline, fireDustType);
			} else {
				ExplosionVisual(projectile.Hitbox, sound, fireDustAmount, smokeDustAmount, smokeGoreAmount, debugOutline, fireDustType);
			}
		}
		public static void ExplosionVisual(Rectangle area, SoundStyle? sound = null, int fireDustAmount = 20, int smokeDustAmount = 30, int smokeGoreAmount = 2, bool debugOutline = false, int fireDustType = DustID.Torch) {
			Vector2 center = area.Center.ToVector2();
			if (sound.HasValue) {
				SoundEngine.PlaySound(in sound, center);
			}
			Vector2 topLeft = area.TopLeft();
			for (int i = 0; i < smokeDustAmount; i++) {
				Dust.NewDustDirect(
					topLeft,
					area.Width,
					area.Height,
					DustID.Smoke,
					0f,
					0f,
					100,
					default,
					1.5f
				).velocity *= 1.4f;
			}
			for (int i = 0; i < fireDustAmount; i++) {
				Dust dust = Dust.NewDustDirect(
					topLeft,
					area.Width,
					area.Height,
					fireDustType,
					0f,
					0f,
					100,
					default,
					3.5f
				);
				dust.noGravity = true;
				dust.velocity *= 7f;
				Dust.NewDustDirect(
					topLeft,
					area.Width,
					area.Height,
					fireDustType,
					0f,
					0f,
					100,
					default,
					1.5f
				).velocity *= 3f;
			}
			for (int i = 0; i < smokeGoreAmount; i++) {
				float velocityMult = 0.4f * (i + 1);
				Gore gore = Gore.NewGoreDirect(null, center, default, Main.rand.Next(61, 64));
				gore.velocity *= velocityMult;
				gore.velocity.X += 1f;
				gore.velocity.Y += 1f;
				gore = Gore.NewGoreDirect(null, center, default, Main.rand.Next(61, 64));
				gore.velocity *= velocityMult;
				gore.velocity.X -= 1f;
				gore.velocity.Y += 1f;
				gore = Gore.NewGoreDirect(null, center, default, Main.rand.Next(61, 64));
				gore.velocity *= velocityMult;
				gore.velocity.X += 1f;
				gore.velocity.Y -= 1f;
				gore = Gore.NewGoreDirect(null, center, default, Main.rand.Next(61, 64));
				gore.velocity *= velocityMult;
				gore.velocity.X -= 1f;
				gore.velocity.Y -= 1f;
			}
			if (debugOutline) {
				area.DrawDebugOutline();
			}
		}
		public static void DealSelfDamage(Projectile projectile, int cooldownCounter = -1) {
			if (projectile.owner == Main.myPlayer) {
				Player player = Main.LocalPlayer;
				if (player.active && !player.dead && !player.immune) {
					Rectangle projHitbox = projectile.Hitbox;
					ProjectileLoader.ModifyDamageHitbox(projectile, ref projHitbox);
					Rectangle playerHitbox = new((int)player.position.X, (int)player.position.Y, player.width, player.height);
					if (projHitbox.Intersects(playerHitbox)) {
						double damageDealt = player.Hurt(
							PlayerDeathReason.ByProjectile(Main.myPlayer, projectile.whoAmI),
							Main.DamageVar(projectile.damage, -player.luck),
							Math.Sign(player.Center.X - projectile.Center.X),
							out Player.HurtInfo info,
							true,
							cooldownCounter: cooldownCounter
						);
						if (projectile.ModProjectile is ISelfDamageEffectProjectile selfDamageEffectProjectile) selfDamageEffectProjectile.OnSelfDamage(player, info, damageDealt);
					}
				}
			}
		}
	}
	public abstract class ExplosionProjectile : ModProjectile, IIsExplodingProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Sonorous_Shredder_P";
		public abstract DamageClass DamageType { get; }
		public abstract int Size { get; }
		public virtual bool DealsSelfDamage => true;
		public virtual SoundStyle? Sound => SoundID.Item62;
		public virtual int FireDustAmount => 20;
		public virtual int SmokeDustAmount => 30;
		public virtual int SmokeGoreAmount => 2;
		public virtual int SelfDamageCooldownCounter => ImmunityCooldownID.General;
		public override void SetDefaults() {
			Projectile.DamageType = DamageType;
			Projectile.width = Size;
			Projectile.height = Size;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.hide = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: Sound, fireDustAmount: FireDustAmount, smokeDustAmount: SmokeDustAmount, smokeGoreAmount: SmokeGoreAmount);
				Projectile.ai[0] = 1;
			}
			if (DealsSelfDamage) ExplosiveGlobalProjectile.DealSelfDamage(Projectile, SelfDamageCooldownCounter);
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding() => true;
	}
}
