using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items;
using Origins.Items.Armor.Amber;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Projectiles.Weapons;
using Origins.Reflection;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace Origins.Projectiles {
	public class ExplosiveGlobalProjectile : GlobalProjectile {
		public bool isHoming = false;
		public bool magicTripwire = false;
		public bool magicTripwireTripped = false;
		public bool noTileSplode = false;
		public bool acridHandcannon = false;
		public bool novaCascade = false;
		public bool novaSwarm = false;
		public bool scrapCompactor = false;
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
				bool foundTarget = Main.player[projectile.owner].DoHoming((target) => {
					Vector2 currentPos = target.Center;
					float dist = Math.Abs(projectile.Center.X - currentPos.X) + Math.Abs(projectile.Center.Y - currentPos.Y);
					if (target is Player) dist *= 2.5f;
					if (dist < targetWeight && Collision.CanHit(projectile.position, projectile.width, projectile.height, target.position, target.width, target.height)) {
						targetWeight = dist;
						targetPos = currentPos;
						return true;
					}
					return false;
				});

				if (foundTarget) {
					float scaleFactor = 16f;
					float lerpValue = 0.083333336f;
					switch (OriginsSets.Projectiles.HomingEffectivenessMultiplier[projectile.type]) {
						default:
						lerpValue *= Origins.HomingEffectivenessMultiplier[projectile.type];
						break;
						case 1:
						scaleFactor *= Origins.HomingEffectivenessMultiplier[projectile.type];
						break;
					}

					Vector2 targetVelocity = (targetPos - projectile.Center).SafeNormalize(-Vector2.UnitY) * scaleFactor;
					projectile.velocity = Vector2.Lerp(projectile.velocity, targetVelocity, lerpValue);
				}
			}
			if (novaSwarm) {
				const float force = 1;
				float angle = projectile.velocity.ToRotation();
				projectile.rotation = angle + MathHelper.PiOver2;
				float targetOffset = 0.9f;
				float targetAngle = 1;
				float dist = 641;

				bool foundTarget = Main.player[projectile.owner].DoHoming((target) => {
					Vector2 toHit = (projectile.Center.Clamp(target.Hitbox.Add(target.velocity)) - projectile.Center);
					if (!Collision.CanHitLine(projectile.Center + projectile.velocity, 1, 1, projectile.Center + toHit, 1, 1)) return false;
					float tdist = toHit.Length();
					float ta = (float)Math.Abs(GeometryUtils.AngleDif(toHit.ToRotation(), angle, out _));
					if (target is Player) {
						tdist *= 2.5f;
						ta *= 2.5f;
					}
					if (tdist <= dist && ta <= targetOffset) {
						targetAngle = ((target.Center + target.velocity) - projectile.Center).ToRotation();
						targetOffset = ta;
						dist = tdist;
						return true;
					}
					return false;
				});
				if (foundTarget) projectile.velocity = (projectile.velocity + new Vector2(force, 0).RotatedBy(targetAngle)).SafeNormalize(Vector2.Zero) * projectile.velocity.Length();
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
				for (int i = 0; i < Main.maxNPCs && !tripped; i++) {
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy() && magicTripwireHitbox.Intersects(npc.Hitbox)) {
						tripped = true;
					}
				}
				if (!tripped) {
					Player owner = Main.player[projectile.owner];
					if (owner.hostile) {
						foreach (Player player in Main.ActivePlayers) {
							if (!player.dead && player.hostile && player.team != owner.team && magicTripwireHitbox.Intersects(player.Hitbox)) {
								tripped = true;
								break;
							}
						}
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
			if (OriginsSets.Projectiles.ApplyLifetimeModifiers[projectile.type] && originPlayer.explosiveFuseTime != StatModifier.Default) {
				projectile.timeLeft = (int)originPlayer.explosiveFuseTime.ApplyTo(projectile.timeLeft);
			}
			if (originPlayer.magicTripwire) {
				magicTripwire = true;
			}
			if (originPlayer.pincushion) {
				noTileSplode = true;
			}
			if (originPlayer.scrapCompactor) {
				scrapCompactor = true;
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
					modifierBlastRadius = modifierBlastRadius.CombineWith(brPrefix.BlastRadius());
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
				selfDamageModifier = selfDamageModifier.CombineWith(parentGlobal.selfDamageModifier);
				novaCascade = parentGlobal.novaCascade;
				novaSwarm = parentGlobal.novaSwarm;
				if (novaSwarm) projectile.scale *= Nova_Swarm.rocket_scale;
				noTileSplode = parentGlobal.noTileSplode;
			}
		}
		public override void CutTiles(Projectile projectile) {
			if (IsExploding(projectile)) {
				OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
				Rectangle hitbox = projectile.Hitbox;
				bool modifiedBlastRadius = false;
				if (modifierBlastRadius != StatModifier.Default) {
					StatModifier modifier = modifierBlastRadius.Scale(additive: 0.5f, multiplicative: 0.5f);
					hitbox.Inflate((int)(modifier.ApplyTo(hitbox.Width) - hitbox.Width), (int)(modifier.ApplyTo(hitbox.Height) - hitbox.Height));
					modifiedBlastRadius = true;
				}
				if (originPlayer.explosiveBlastRadius != StatModifier.Default) {
					StatModifier modifier = originPlayer.explosiveBlastRadius.Scale(additive: 0.5f, multiplicative: 0.5f);
					hitbox.Inflate((int)(modifier.ApplyTo(hitbox.Width) - hitbox.Width), (int)(modifier.ApplyTo(hitbox.Height) - hitbox.Height));
					modifiedBlastRadius = true;
				}
				if (modifiedBlastRadius) {
					int minX = hitbox.X / 16;
					int maxX = (hitbox.X + hitbox.Width) / 16 + 1;
					int minY = hitbox.Y / 16;
					int maxY = (hitbox.Y + hitbox.Height) / 16 + 1;

					if (minX < 0) minX = 0;
					if (maxX > Main.maxTilesX) maxX = Main.maxTilesX;
					if (minY < 0) minY = 0;
					if (maxY > Main.maxTilesY) maxY = Main.maxTilesY;

					bool[] tileCutIgnorance = Main.player[projectile.owner].GetTileCutIgnorance(allowRegrowth: false, projectile.trap);
					for (int i = minX; i < maxX; i++) {
						for (int j = minY; j < maxY; j++) {
							if (Main.tile[i, j] != null && Main.tileCut[Main.tile[i, j].TileType] && !tileCutIgnorance[Main.tile[i, j].TileType] && WorldGen.CanCutTile(i, j, TileCuttingContext.AttackProjectile)) {
								WorldGen.KillTile(i, j);
								if (Main.netMode != NetmodeID.SinglePlayer) {
									NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);
								}
							}
						}
					}
				}
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
			bitWriter.WriteBit(scrapCompactor);
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
			scrapCompactor = bitReader.ReadBit();
		}
		public static bool IsExploding(Projectile projectile, bool isHitting = false) {
			if (projectile.type == Shimmer_Guardian_Shard.ID) {
				return projectile.ai[0] == 0 && projectile.velocity != Vector2.Zero && projectile.ai[1] <= 50;
			}
			if (!projectile.CountsAsClass(DamageClasses.Explosive)) return false;
			if (projectile.ModProjectile is IIsExplodingProjectile explodingProjectile) {
				return explodingProjectile.IsExploding;
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

				case ProjectileID.ExplosiveBullet:
				if (isHitting) return true;
				goto default;

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
					int projType = ModContent.ProjectileType<Brine_Droplet>();
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
				int shrapnelShardType = ModContent.ProjectileType<Impeding_Shrapnel_Shard>();
				if (scrapCompactor && projectile.damage > 0 && projectile.type != shrapnelShardType) {
					SoundEngine.PlaySound(Origins.Sounds.ShrapnelFest, projectile.Center);
					for (int i = 3; i-- > 0;) {
						Vector2 v = Main.rand.NextVector2Unit() * 4;
						Projectile.NewProjectile(
							projectile.GetSource_Death(),
							projectile.Center + v * 8,
							v,
							shrapnelShardType,
							projectile.damage / 2,
							projectile.knockBack / 4,
							projectile.owner,
							ai2: 0.5f
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
				case ProjectileID.ExplosiveBullet:
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
		public static void DoExplosion(Projectile projectile, int size, bool dealSelfDamage = true, SoundStyle? sound = null, int fireDustAmount = 20, int smokeDustAmount = 30, int smokeGoreAmount = 2, int fireDustType = DustID.Torch, bool hostile = false, bool alsoFriendly = false) {
			projectile.friendly = !hostile || alsoFriendly;
			projectile.hostile = hostile;
			projectile.penetrate = -1;
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = size;
			projectile.height = size;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
			if (dealSelfDamage && !hostile) DealSelfDamage(projectile);
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
					if (projectile.Colliding(projHitbox, playerHitbox)) {
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
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Sonorous_Shredder_P";
		public abstract DamageClass DamageType { get; }
		public abstract int Size { get; }
		public virtual bool Hostile => false;
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
			Projectile.friendly = !Hostile;
			Projectile.hostile = Hostile;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.hide = true;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: Sound, fireDustAmount: FireDustAmount, smokeDustAmount: SmokeDustAmount, smokeGoreAmount: SmokeGoreAmount);
				Projectile.ai[0] = 1;
			}
			if (!Hostile && DealsSelfDamage) ExplosiveGlobalProjectile.DealSelfDamage(Projectile, SelfDamageCooldownCounter);
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding => true;
	}
}
