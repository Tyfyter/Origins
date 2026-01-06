using AltLibrary.Common.AltBiomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Achievements;
using Origins.Buffs;
using Origins.Graphics;
using Origins.Items;
using Origins.Items.Accessories;
using Origins.Items.Armor.Felnum;
using Origins.Items.Materials;
using Origins.Items.Other.Dyes;
using Origins.Items.Tools;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.NPCs;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.NPCs.MiscE;
using Origins.Projectiles.Weapons;
using Origins.Questing;
using Origins.Reflection;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace Origins.Projectiles {
	public class OriginGlobalProj : GlobalProjectile, IDrawProjectileEffect {
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;

		public const string multishot_context = "multishot";
		public const string no_multishot_context = "noMultishot";
		public int fromItemType = -1;
		//bool init = true;
		public float felnumBonus = 0;
		public bool viperEffect = false;
		public int killLink = -1;
		int updateCountBoost = 0;
		public int UpdateCountBoost => updateCountBoost;
		public float extraBossDamage = 0f;
		public static int killLinkNext = -1;
		public bool isFromMitosis = false;
		public bool hasUsedMitosis = false;
		public int mitosisTimeLeft = 3600;
		public bool fiberglassLifesteal = false;
		public ModPrefix prefix;
		public int Prefix {
			get => prefix?.Type ?? 0;
			set {
				prefix = value == 0 ? null : PrefixLoader.GetPrefix(value);
			}
		}
		public bool neuralNetworkEffect = false;
		public bool neuralNetworkHit = false;
		public bool crawdadNetworkEffect = false;
		public Vector2? weakpointAnalyzerTarget = default;
		public bool weakpointAnalyzerFake = false;
		public Vector2 extraGravity = default;
		public bool shouldUnmiss = false;
		public bool[] alreadyUnmissed = new bool[Main.maxNPCs];
		public int unmissTarget = -1;
		public Vector2 unmissTargetPos = default;
		public int unmissAnimation = 0;
		public bool isUnmissing = false;
		public bool laserBow = false;
		public bool astoxoEffect = false;
		public bool weakShimmer = false;
		public static Dictionary<int, Action<OriginGlobalProj, Projectile, string[]>> itemSourceEffects;
		public Vector2[] oldPositions = [];
		public OwnerMinionKey ownerMinion = null;
		public bool magicHairSprayEffect = false;
		public override void Load() {
			itemSourceEffects = [];
		}
		public override void Unload() {
			itemSourceEffects = null;
		}
		public override void SetDefaults(Projectile projectile) {
			if (killLinkNext != -1) {
				killLink = killLinkNext;
				//sync killLink ids
				Main.projectile[killLink].GetGlobalProjectile<OriginGlobalProj>().killLink = projectile.whoAmI;
				killLinkNext = -1;
			}
			switch (ExplosiveGlobalProjectile.GetVanillaExplosiveType(projectile)) {
				case 1:
				projectile.DamageType = DamageClasses.ThrownExplosive;
				break;

				case 2:
				projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
				break;

				case 3:
				projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
				break;

				case 4:
				projectile.DamageType = DamageClasses.Explosive;
				break;

				case 5:
				projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Summon];
				break;
			}
			if (projectile.ModProjectile?.Mod is Origins) {
				ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[projectile.type] = true;
			}
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			felnumBonus = MainReflection._currentPlayerOverride.GetValue() is null ? 0 : Main.CurrentPlayer.OriginPlayer().felnumShock;
			string[] contextArgs = source?.Context?.Split(';') ?? [];
			if (projectile.aiStyle is ProjAIStyleID.Explosive or ProjAIStyleID.Bobber or ProjAIStyleID.GolfBall && projectile.originalDamage < projectile.damage)
				projectile.originalDamage = projectile.damage;
			if (contextArgs.Contains(nameof(OriginPlayer.weakpointAnalyzer))) {
				weakpointAnalyzerTarget = Main.MouseWorld;
				weakpointAnalyzerFake = contextArgs.Contains("fake");
				if (OriginsSets.Projectiles.WeakpointAnalyzerSpawnAction[projectile.type] is Action<Projectile, int> action) {
					const string clone_prefix = $"{nameof(OriginPlayer.weakpointAnalyzer)}_clone";
					for (int i = 0; i < contextArgs.Length; i++) {
						if (contextArgs[i].StartsWith(clone_prefix) && int.TryParse(contextArgs[i][clone_prefix.Length..], out int cloneIndex)) {
							action(projectile, cloneIndex);
							break;
						}
					}
				}
			}
			if (projectile.friendly && projectile.TryGetOwner(out Player player) && player.OriginPlayer().weakShimmer) {
				weakShimmer = true;
				if ((ProjectileID.Sets.FallingBlockTileItem[projectile.type]?.ItemType ?? 0) != ItemID.None) weakShimmer = false;
			}
			if (source is EntitySource_ItemUse itemUseSource) {
				if (itemSourceEffects.TryGetValue(itemUseSource.Item.type, out Action<OriginGlobalProj, Projectile, string[]> itemSourceEffect)) itemSourceEffect(this, projectile, contextArgs);
				OriginPlayer originPlayer = itemUseSource.Player.GetModPlayer<OriginPlayer>();
				if (originPlayer.entangledEnergy) {
					fiberglassLifesteal = true;
				}
				Prefix = itemUseSource.Item.prefix;
				ModPrefix projPrefix = PrefixLoader.GetPrefix(Prefix);

				if (projPrefix is IOnSpawnProjectilePrefix spawnPrefix) {
					spawnPrefix.OnProjectileSpawn(projectile, source);
				}
				if (itemUseSource is EntitySource_ItemUse_WithAmmo withAmmo) {
					if (withAmmo.AmmoItemIdUsed == ModContent.ItemType<Magic_Hair_Spray>()) magicHairSprayEffect = true;
				}

				fromItemType = itemUseSource.Item.type;
				if (fromItemType == Neural_Network.ID) {
					neuralNetworkEffect = true;
				}
				if (!contextArgs.Contains(multishot_context) && !contextArgs.Contains(no_multishot_context) && !OriginsSets.Projectiles.NoMultishot[projectile.type]) {
					EntitySource_ItemUse multishotSource;
					if (!itemUseSource.Item.accessory || itemUseSource.Item.useStyle != ItemUseStyleID.None) {
						int bocShadows = 0;
						float bocShadowDamageChance = 0.08f;
						if (originPlayer.weakpointAnalyzer && projectile.CountsAsClass(DamageClass.Ranged)) {
							bocShadows = 2;
						}
						if (originPlayer.controlLocus) {
							if (projectile.CountsAsClass(DamageClasses.Explosive)) bocShadows = 2;
							if (projectile.CountsAsClass(DamageClass.Ranged)) bocShadows = 2;
							bocShadowDamageChance = Math.Max(bocShadowDamageChance, 0.12f);
						}
						if (bocShadows > 0 && projectile.damage > 0) {
							multishotSource = itemUseSource.WithContext(source.Context, multishot_context, nameof(OriginPlayer.weakpointAnalyzer));
							for (int i = bocShadows; i-- > 0;) {
								EntitySource_ItemUse currentSource = multishotSource.WithContext(multishotSource.Context, $"{nameof(OriginPlayer.weakpointAnalyzer)}_clone{i}");
								float rot = MathHelper.TwoPi * ((i + 1f) / (bocShadows + 1f)) + Main.rand.NextFloat(-0.3f, 0.3f);
								Vector2 _position = projectile.position.RotatedBy(rot, Main.MouseWorld);
								Vector2 _velocity = projectile.velocity.RotatedBy(rot);
								int _damage = projectile.damage;
								if (Main.rand.NextFloat(1) >= bocShadowDamageChance) {
									currentSource = currentSource.WithContext(currentSource.Context, "fake");
								}
								Projectile.NewProjectile(currentSource, _position, _velocity, projectile.type, _damage, projectile.knockBack, projectile.owner, projectile.ai[0], projectile.ai[1], projectile.ai[2]);
							}
						}
					}
					if (originPlayer.emergencyBeeCanister && (projectile.type is ProjectileID.Bee or ProjectileID.GiantBee) && Main.rand.NextBool(3)) {
						multishotSource = itemUseSource.WithContext(source.Context, multishot_context, nameof(OriginPlayer.emergencyBeeCanister));
						Projectile.NewProjectile(multishotSource, projectile.position, projectile.velocity.RotatedByRandom(0.2f), projectile.type, projectile.damage, projectile.knockBack, projectile.owner, projectile.ai[0], projectile.ai[1], projectile.ai[2]);
					}
				}
			} else if (source is EntitySource_Parent source_Parent) {
				if (source_Parent.Entity is Projectile parentProjectile) {
					if (parentProjectile.type == ModContent.ProjectileType<Mitosis_P>()) {
						isFromMitosis = true;
						projectile.alpha = 100;
						if (projectile.minion) {
							mitosisTimeLeft = Mitosis_P.minion_duplicate_duration;
							projectile.minionSlots = 0;
						}
					}
					OriginGlobalProj parentGlobalProjectile = parentProjectile.GetGlobalProjectile<OriginGlobalProj>();
					ownerMinion = parentProjectile.minion || parentProjectile.sentry ? new(parentProjectile.type, parentProjectile.owner, parentProjectile.identity) : parentGlobalProjectile.ownerMinion;
					Prefix = parentGlobalProjectile.Prefix;
					neuralNetworkEffect = parentGlobalProjectile.neuralNetworkEffect;
					neuralNetworkHit = parentGlobalProjectile.neuralNetworkHit;
					crawdadNetworkEffect = parentGlobalProjectile.crawdadNetworkEffect;
					fiberglassLifesteal = parentGlobalProjectile.fiberglassLifesteal;
					weakpointAnalyzerFake = parentGlobalProjectile.weakpointAnalyzerFake;
					magicHairSprayEffect = parentGlobalProjectile.magicHairSprayEffect;
					if (OriginPlayer.ShouldApplyFelnumEffectOnShoot(projectile)) felnumBonus = parentGlobalProjectile.felnumBonus;

					ModPrefix projPrefix = PrefixLoader.GetPrefix(Prefix);

					if (projPrefix is IOnSpawnProjectilePrefix spawnPrefix) {
						spawnPrefix.OnProjectileSpawn(projectile, source);
					}
					if (parentProjectile.friendly && parentProjectile.owner == Main.myPlayer && (projectile.type is ProjectileID.Bee or ProjectileID.GiantBee)) {
						OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
						if (originPlayer.emergencyBeeCanister && Main.rand.NextBool(3)) {
							Projectile.NewProjectile(
								source.CloneWithContext(OriginExtensions.MakeContext(source.Context, multishot_context, nameof(OriginPlayer.emergencyBeeCanister))),
								projectile.position,
								projectile.velocity.RotatedByRandom(0.2f),
								projectile.type,
								projectile.damage,
								projectile.knockBack,
								projectile.owner,
								projectile.ai[0], projectile.ai[1], projectile.ai[2]
							);
						}
					}
				} else if (source_Parent.Entity is NPC parentNPC) {
					OriginGlobalNPC globalNPC = parentNPC.GetGlobalNPC<OriginGlobalNPC>();
					if (globalNPC.soulhideWeakenedDebuff) {
						projectile.damage = (int)(projectile.damage * (1f - OriginGlobalNPC.soulhideWeakenAmount));
					}
					if (globalNPC.silencedDebuff && projectile.damage > 0 && !projectile.netImportant) {
						projectile.timeLeft = 1;
					}
				}
			}
			if (Strange_Computer.drawingStrangeLine) {
				Strange_Computer.projectiles.Add(projectile.whoAmI);
			}
		}
		public override void PostAI(Projectile projectile) {
			if (projectile.minion) weakShimmer = Main.player.GetIfInRange(projectile.owner)?.OriginPlayer()?.weakShimmer ?? false;
			if (projectile.aiStyle is ProjAIStyleID.Explosive or ProjAIStyleID.Bobber or ProjAIStyleID.GolfBall)
				projectile.damage = projectile.originalDamage;
			if (!OriginPlayer.ShouldApplyFelnumEffectOnShoot(projectile)) felnumBonus = Main.player[projectile.owner].OriginPlayer().felnumShock;
			if (shouldUnmiss) {
				int magicTripwireRange = Origins.MagicTripwireRange[projectile.type];
				if (magicTripwireRange == 0) magicTripwireRange = 48;
				Rectangle magicTripwireHitbox = new(
					(int)projectile.Center.X - magicTripwireRange,
					(int)projectile.Center.Y - magicTripwireRange,
					magicTripwireRange * 2,
					magicTripwireRange * 2
				);
				int tripper = -1;
				foreach (NPC npc in Main.ActiveNPCs) {
					if (!alreadyUnmissed[npc.whoAmI] && npc.CanBeChasedBy() && magicTripwireHitbox.Intersects(npc.Hitbox)) {
						tripper = npc.whoAmI;
						break;
					}
				}
				if (tripper == -1) {
					Player owner = Main.player[projectile.owner];
					if (owner.hostile) {
						foreach (Player player in Main.ActivePlayers) {
							if (!player.dead && player.hostile && player.team != owner.team && magicTripwireHitbox.Intersects(player.Hitbox)) {
								tripper = player.whoAmI + Main.maxNPCs + 1;
								break;
							}
						}
					}
				}

				if (tripper != -1 && !isUnmissing) {
					unmissTarget = tripper;
				} else if (unmissTarget != -1) {
					Entity target = null;
					if (unmissTarget > Main.maxNPCs + 1) {
						int translatedTarget = unmissTarget - (Main.maxNPCs + 1);
						if (Main.player.IndexInRange(translatedTarget)) {
							Player playerTarget = Main.player[translatedTarget];
							if (playerTarget.active && !playerTarget.dead && playerTarget.hostile && playerTarget.team != Main.player[projectile.owner].team) {
								target = playerTarget;
							}
						}
					} else {
						if (Main.npc.IndexInRange(unmissTarget) && Main.npc[unmissTarget].CanBeChasedBy(projectile)) {
							target = Main.npc[unmissTarget];
						}
					}
					if (target is not null && !isUnmissing) {
						unmissTargetPos = target.Center - projectile.velocity * 10;
						if (++unmissAnimation >= 8) {
							isUnmissing = true;
							alreadyUnmissed[target.whoAmI] = true;
							//shouldUnmiss = false;
							(unmissTargetPos, projectile.Center) = (projectile.Center, unmissTargetPos);
						}
					} else {
						isUnmissing = false;
						unmissTarget = -1;
						if (unmissAnimation > 0) unmissAnimation--;
					}
				}
			} else if (unmissAnimation > 0) {
				unmissAnimation--;
			}
			{
				bool mildewArmor = false;
				if (projectile.friendly && !OriginsSets.Projectiles.NoMildewSetTrail[projectile.type] && projectile.TryGetOwner(out Player owner)) {
					mildewArmor = owner.OriginPlayer().mildewSet;
				}
				int neededTrailLength = 0;
				if (mildewArmor) neededTrailLength = Math.Max(neededTrailLength, 30);
				if (neededTrailLength != 0) {
					if (oldPositions.Length != neededTrailLength) Array.Resize(ref oldPositions, neededTrailLength);
					Rectangle mildewHitbox = new(0, 0, 8, 8);
					for (int i = oldPositions.Length - 1; i > 0; i--) {
						oldPositions[i] = oldPositions[i - 1];
						if (mildewArmor) {
							mildewHitbox.X = (int)oldPositions[i].X - 4;
							mildewHitbox.Y = (int)oldPositions[i].Y - 4;
							foreach (NPC npc in Main.ActiveNPCs) {
								if (npc.chaseable && !npc.immortal && !npc.friendly && mildewHitbox.Intersects(npc.Hitbox)) {
									npc.AddBuff(Toxic_Shock_Debuff.ID, 60);
								}
							}
						}
					}
					oldPositions[0] = projectile.Center;
				}
			}
			if (weakpointAnalyzerFake) {
				projectile.timeLeft -= 2;
			}
			if (magicHairSprayEffect) {
				float size = 48 * projectile.scale;
				if (projectile.ModProjectile?.Mod is not Origins) {
					size = 48 * Utils.Remap(Utils.Remap(projectile.localAI[0], 0f, 72, 0f, 1f), 0.2f, 0.5f, 0.25f, 1f);
				}
				if (Main.rand.NextFloat(250) < size) {
					ParticleOrchestrator.RequestParticleSpawn(
						true,
						ParticleOrchestraType.PrincessWeapon,
						new() {
							PositionInWorld = projectile.Center + Main.rand.NextVector2Circular(size, size)
						}
					);
				}
			}
		}
		public override void AI(Projectile projectile) {
			if (prefix is IProjectileAIPrefix projectileAIPrefix) {
				projectileAIPrefix.ProjectileAI(projectile);
			}
			if (!isFromMitosis && !hasUsedMitosis && projectile.owner == Main.myPlayer && !ProjectileID.Sets.IsAWhip[projectile.type] && projectile.type != ModContent.ProjectileType<Mitosis_P>()) {
				for (int i = 0; i < Mitosis_P.mitosises.Count; i++) {
					if (projectile.Colliding(projectile.Hitbox, Main.projectile[Mitosis_P.mitosises[i]].Hitbox)) {
						Projectile duplicated = Projectile.NewProjectileDirect(
							Main.projectile[Mitosis_P.mitosises[i]].GetSource_FromThis(),
							projectile.Center,
							projectile.velocity.RotatedBy(0.25f),
							projectile.type,
							projectile.damage,
							projectile.knockBack,
							projectile.owner,
							OriginsSets.Projectiles.DuplicationAIVariableResets[projectile.type].first ? 0 : projectile.ai[0],
							OriginsSets.Projectiles.DuplicationAIVariableResets[projectile.type].second ? 0 : projectile.ai[1],
							OriginsSets.Projectiles.DuplicationAIVariableResets[projectile.type].third ? 0 : projectile.ai[2]
						);
						duplicated.rotation += 0.25f;

						projectile.velocity = projectile.velocity.RotatedBy(-0.25f);
						projectile.rotation -= 0.25f;
						hasUsedMitosis = true;
						ModContent.GetInstance<Cloning_Factory>().Condition.Value++;
						if (projectile.minion) {
							mitosisTimeLeft = Mitosis_P.minion_duplicate_duration;
						}
					}
					
				}
			} else {
				if (isFromMitosis) {
					Main.player[projectile.owner].ownedProjectileCounts[projectile.type]--;
					if (--mitosisTimeLeft <= 0) projectile.active = false;
				}
				if (hasUsedMitosis && projectile.minion && --mitosisTimeLeft <= 0) {
					hasUsedMitosis = false;
				}
			}
			if (!projectile.ownerHitCheck && projectile.damage > 0 && OriginsSets.Projectiles.CanBeDeflected[projectile.type]) {
				for (int i = 0; i < The_Bird_Swing.reflectors.Count; i++) {
					Projectile reflector = Main.projectile[The_Bird_Swing.reflectors[i]];
					Rectangle hitbox = reflector.Hitbox;
					hitbox.Inflate(16, 16);
					if (projectile.WithinRange(reflector.Center, 10 * 16) && projectile.Colliding(projectile.Hitbox, hitbox)) {
						projectile.reflected = true;
						if (projectile.hostile) projectile.damage *= 3;
						projectile.hostile = false;
						projectile.friendly = true;
						float speed = Math.Max(12f / projectile.MaxUpdates, projectile.velocity.Length());
						projectile.velocity = reflector.velocity * speed;
						projectile.owner = reflector.owner;
					}
				}
			}
			if (felnumBonus > Felnum_Helmet.shock_damage_divisor) {
				if (!ProjectileID.Sets.IsAWhip[projectile.type]) {
					Dust.NewDustPerfect(projectile.Center, DustID.Electric, projectile.velocity.RotatedByRandom(0.1) * 0.5f, Scale: 0.5f);
				}
			}
			if (viperEffect && projectile.extraUpdates != 19) {
				Lighting.AddLight(projectile.Center, 0, 0.75f * projectile.scale, 0.3f * projectile.scale);
				Dust dust = Dust.NewDustPerfect(projectile.Center, DustID.Electric, projectile.velocity.RotatedByRandom(0.1f) * -0.25f, 100, new Color(0, 255, 0), projectile.scale / 2);
				dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
				dust.noGravity = true;
				dust.noLight = true;
			}
			switch (projectile.aiStyle) {
				case ProjAIStyleID.Harpoon:
				if (projectile.ai[1] >= 10f) goto default;
				break;
				default:
				projectile.velocity += extraGravity;
				break;
			}
			if (laserBow && projectile.timeLeft % 10 == 0) {
				Projectile.NewProjectile(
					projectile.GetSource_FromAI(),
					projectile.Center,
					Vector2.Zero,
					ModContent.ProjectileType<Brine_Droplet>(),
					projectile.damage / 3,
					projectile.knockBack,
					projectile.owner
				);
			}
		}
		public override bool PreAI(Projectile projectile) {
			if (weakpointAnalyzerTarget.HasValue && OriginsSets.Projectiles.WeakpointAnalyzerAIReplacement[projectile.type] is Func<Projectile, bool> fakeAI) {
				if (!fakeAI(projectile)) return false;
			}
			return true;
		}
		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) {
			if (viperEffect) {
				for (int i = 0; i < target.buffType.Length; i++) {
					if (Main.debuff[target.buffType[i]] && target.buffType[i] != Toxic_Shock_Debuff.ID) {
						modifiers.SetCrit();
						break;
					}
				}
			}
			if (target.boss && extraBossDamage != 0f) {
				modifiers.SourceDamage *= 1 + extraBossDamage;
			}
			if (felnumBonus > Felnum_Helmet.shock_damage_divisor * 2) {
				modifiers.SourceDamage.Base += (felnumBonus * (fromItemType >= 0 ? Origins.DamageBonusScale[fromItemType] : 1)) / Felnum_Helmet.shock_damage_divisor;
				if (!OriginPlayer.ShouldApplyFelnumEffectOnShoot(projectile)) {
					Main.player[projectile.owner].OriginPlayer().usedFelnumShock = true;
					SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), projectile.Center);
				}
			}
			if (prefix is IModifyHitNPCPrefix onHitNPCPrefix) {
				onHitNPCPrefix.ModifyHitNPC(projectile, target, ref modifiers);
			}
		}
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			if (fiberglassLifesteal && Main.player[projectile.owner].potionDelay > 0) {
				Projectile.NewProjectile(
					projectile.GetSource_OnHit(target),
					target.Center,
					Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(4, 8),
					ModContent.ProjectileType<Entangled_Energy_Lifesteal>(),
					damageDone,
					0,
					projectile.owner,
					ai1: projectile.ownerHitCheck.ToInt()
				);
			}
			if (prefix is IOnHitNPCPrefix onHitNPCPrefix) {
				onHitNPCPrefix.OnHitNPC(projectile, target, hit, damageDone);
			}
			if (viperEffect) {
				if (hit.Crit || Main.rand.Next(0, 9) == 0) {
					target.AddBuff(Toxic_Shock_Debuff.ID, 450);
				}
			}
			if (crawdadNetworkEffect) {
				ref int crawdadNetworkCount = ref Main.player[projectile.owner].OriginPlayer().crawdadNetworkCount;
				if (++crawdadNetworkCount >= 7) {
					crawdadNetworkCount = 0;
					if (projectile.owner == Main.myPlayer) {
						Projectile.NewProjectile(
							projectile.GetSource_OnHit(target),
							projectile.Center,
							projectile.velocity,
							ModContent.ProjectileType<Crawdaddys_Revenge_P>(),
							projectile.damage,
							projectile.knockBack
						);
					}
				}
			}
			if (neuralNetworkEffect) {
				neuralNetworkHit = true;
				if (target.CanBeChasedBy(projectile)) {
					Player player = Main.player[projectile.owner];
					player.AddBuff(ModContent.BuffType<Neural_Network_Buff>(), 1);
					player.OriginPlayer().neuralNetworkMisses = 0;
				}
			}
			switch (projectile.type) {
				case ProjectileID.ThunderStaffShot:
				if (OriginConfig.Instance.ThunderStaff && Main.rand.NextBool()) Static_Shock_Debuff.Inflict(target, Main.rand.Next(120, 210));
				break;
				case ProjectileID.ThunderSpear:
				if (OriginConfig.Instance.ThunderSpear && Main.rand.NextBool()) Static_Shock_Debuff.Inflict(target, Main.rand.Next(120, 210));
				break;
				case ProjectileID.ThunderSpearShot:
				if (OriginConfig.Instance.ThunderSpear && Main.rand.NextBool(3)) Static_Shock_Debuff.Inflict(target, Main.rand.Next(90, 180));
				break;
			}
			if (magicHairSprayEffect) {
				new Extend_Deuffs_Action(target, 30).Perform();
				ParticleOrchestrator.RequestParticleSpawn(
					false,
					ParticleOrchestraType.PaladinsHammer,
					new() {
						PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox)
					}
				);
			}
		}
		public override bool? CanHitNPC(Projectile projectile, NPC target) {
			if (weakpointAnalyzerFake) return false;
			return base.CanHitNPC(projectile, target);
		}
		public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info) {
			if (BiomeNPCGlobals.ProjectileAssimilationAmounts.TryGetValue(projectile.type, out Dictionary<int, AssimilationAmount> assimilationValues)) {
				foreach (KeyValuePair<int, AssimilationAmount> value in assimilationValues) {
					target.GetAssimilation(value.Key).Percent += value.Value.GetValue(projectile, target);
				}
			}
		}
		public override bool PreKill(Projectile projectile, int timeLeft) {
			if (felnumBonus > Felnum_Helmet.shock_damage_divisor && projectile.type == ProjectileID.WaterGun) {//projectile.aiStyle==60
				OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
				int type = ModContent.ProjectileType<Felnum_Shock_Grenade_Shock>();
				int damage = (int)(felnumBonus / Felnum_Helmet.shock_damage_divisor);
				for (int i = 0; i < 5; i++) {
					Projectile.NewProjectileDirect(projectile.GetSource_FromThis(), projectile.Center, Vector2.Zero, type, damage, projectile.knockBack, projectile.owner);
				}
				originPlayer.usedFelnumShock = true;
				SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), projectile.Center);
			}
			return true;
		}
		public override void OnKill(Projectile projectile, int timeLeft) {
			if (killLink != -1 && projectile.penetrate == 0) {
				Main.projectile[killLink].active = false;
				killLink = -1;
			}
			if (neuralNetworkEffect && !neuralNetworkHit) {
				neuralNetworkEffect = false;
				Player player = Main.player[projectile.owner];
				if (projectile.owner == Main.myPlayer && ++player.OriginPlayer().neuralNetworkMisses >= 4) {
					player.ClearBuff(ModContent.BuffType<Neural_Network_Buff>());
				}
			}
			if (astoxoEffect) Astoxo.DoEffect(projectile);
			if (projectile.type == ProjectileID.Boulder && Main.noTrapsWorld && Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(3)) {
				Vector2 pos = projectile.Bottom;
				NPC.NewNPC(
					projectile.GetSource_Death(),
					(int)pos.X,
					(int)pos.Y,
					ModContent.NPCType<Boulder_Mimic>()
				);
			}
		}
		static bool getAlphaRecursionLock = false;
		public override Color? GetAlpha(Projectile projectile, Color lightColor) {
			if (getAlphaRecursionLock) return null;
			if (weakpointAnalyzerTarget is Vector2 targetPos) {
				Color baseColor;
				try {
					getAlphaRecursionLock = true;
					baseColor = projectile.GetAlpha(lightColor);
				} finally {
					getAlphaRecursionLock = false;
				}
				float distSQ = projectile.DistanceSQ(targetPos);
				const float range = 128;
				const float rangeSQ = range * range;
				return baseColor * MathHelper.Min(1f / (((distSQ * distSQ) / (rangeSQ * rangeSQ)) + 1), 1);
			}
			return null;
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			bitWriter.WriteBit(viperEffect);
			bitWriter.WriteBit(isFromMitosis);
			bitWriter.WriteBit(shouldUnmiss);
			bitWriter.WriteBit(laserBow);

			binaryWriter.Write(Prefix);
			if (OriginPlayer.ShouldApplyFelnumEffectOnShoot(projectile)) binaryWriter.Write(felnumBonus);
			binaryWriter.Write((sbyte)updateCountBoost);

			bitWriter.WriteBit(weakpointAnalyzerTarget.HasValue);
			if (weakpointAnalyzerTarget.HasValue) binaryWriter.WriteVector2(weakpointAnalyzerTarget.Value);

			bitWriter.WriteBit(extraGravity != default);
			if (extraGravity != default) binaryWriter.WriteVector2(extraGravity);

			bitWriter.WriteBit(ownerMinion is not null);
			if (ownerMinion is not null) {
				binaryWriter.Write(ownerMinion.Type);
				binaryWriter.Write((byte)ownerMinion.Owner);
				binaryWriter.Write(ownerMinion.Identity);
			}

			bitWriter.WriteBit(weakShimmer);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			viperEffect = bitReader.ReadBit();
			isFromMitosis = bitReader.ReadBit();
			shouldUnmiss = bitReader.ReadBit();
			laserBow = bitReader.ReadBit();

			Prefix = binaryReader.ReadInt32();
			if (OriginPlayer.ShouldApplyFelnumEffectOnShoot(projectile)) felnumBonus = binaryReader.ReadSingle();
			SetUpdateCountBoost(projectile, binaryReader.ReadSByte());

			if (bitReader.ReadBit()) weakpointAnalyzerTarget = binaryReader.ReadVector2();

			if (bitReader.ReadBit()) extraGravity = binaryReader.ReadVector2();

			if (bitReader.ReadBit()) ownerMinion = new(binaryReader.ReadInt32(), binaryReader.ReadByte(), binaryReader.ReadInt32());

			weakShimmer = bitReader.ReadBit();
		}
		public void SetUpdateCountBoost(Projectile projectile, int newUpdateCountBoost) {
			projectile.extraUpdates += newUpdateCountBoost - updateCountBoost;
			updateCountBoost = newUpdateCountBoost;
		}

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
					int d = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, dustType, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 0, color);
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
		public override bool? CanCutTiles(Projectile projectile) {
			if (projectile.TryGetOwner(out Player owner) && owner.HasBuff<Weak_Shimmer_Debuff>()) return false;
			return null;
		}
		public override bool PreDraw(Projectile projectile, ref Color lightColor) {
			if (projectile.friendly && projectile.TryGetOwner(out Player owner) && owner.OriginPlayer().mildewSet) {
				Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Mildew_Whip_P>()].Value;
				int frameSeed = projectile.type + projectile.whoAmI;
				for (int i = 0; i < oldPositions.Length - 1; i++) {
					if (i < 30) {
						if (oldPositions[i + 1] == Vector2.Zero) break;
						frameSeed = DrawMildewVine(texture, oldPositions[i], oldPositions[i + 1], frameSeed,
							new Rectangle(18, 32, 14, 16),
							new Rectangle(18, 62, 10, 12),
							new Rectangle(18, 90, 12, 14)
						);
					}
				}
			}
			return true;
		}
		static int DrawMildewVine(Texture2D texture, Vector2 a, Vector2 b, int frameSeed, params Rectangle[] frames) {
			if (frames.Length == 0) throw new ArgumentException("Must have frames", nameof(frames));
			float rotation = (b - a).ToRotation() + MathHelper.PiOver2;
			Rectangle frame;
			float length = (b - a).Length();
			/*if (length > 12) {
				frame = frames[new FastRandom(++frameSeed).Next(frames.Length)];
				Main.EntitySpriteDraw(
					texture,
					a - Main.screenPosition,
					frame,
					Lighting.GetColor(a.ToTileCoordinates()),
					rotation,
					frame.Size() * 0.5f,
					1,
					(SpriteEffects)new FastRandom(++frameSeed).Next(4)
				);
				frame = frames[new FastRandom(++frameSeed).Next(frames.Length)];
				Main.EntitySpriteDraw(
					texture,
					b - Main.screenPosition,
					frame,
					Lighting.GetColor(a.ToTileCoordinates()),
					rotation,
					frame.Size() * 0.5f,
					1,
					(SpriteEffects)new FastRandom(++frameSeed).Next(4)
				);
			}*/
			Vector2 dir = (b - a) / length;
			while (length > 0) {
				frame = frames[new FastRandom(++frameSeed).Next(frames.Length)];
				Main.EntitySpriteDraw(
					texture,
					a - Main.screenPosition,
					frame,
					Lighting.GetColor(a.ToTileCoordinates()),
					rotation,
					frame.Size() * 0.5f * Vector2.UnitX,
					1,
					(SpriteEffects)new FastRandom(++frameSeed).Next(4)
				);
				a += dir * (frame.Height - 2);
				length -= frame.Height - 2;
			}
			return frameSeed;
		}
		public override void PostDraw(Projectile projectile, Color lightColor) {
			if (felnumBonus > Felnum_Helmet.shock_damage_divisor * 2 && ProjectileID.Sets.IsAWhip[projectile.type]) {
				List<Vector2> controlPoints = [];
				Projectile.FillWhipControlPoints(projectile, controlPoints);
				for (int i = 1; i <= controlPoints.Count - 2; i+=2) {
					Main.spriteBatch.DrawLightningArcBetween(controlPoints[^i] - Main.screenPosition, controlPoints[^(i + 2)] - Main.screenPosition, Main.rand.NextFloat(-4, 4));
				}
				if (controlPoints.Count % 2 == 1) {
					Main.spriteBatch.DrawLightningArcBetween(controlPoints[0] - Main.screenPosition, controlPoints[1] - Main.screenPosition, Main.rand.NextFloat(-4, 4));
				}
			}
		}
		public void PrepareToDrawProjectile(Projectile projectile) {
			if (unmissAnimation > 0) {
				Origins.shaderOroboros.Capture();
			}
		}
		public void FinishDrawingProjectile(Projectile projectile) {
			if (unmissAnimation > 0) {
				Vector2 velocity = projectile.velocity;
				try {
					projectile.velocity = unmissTargetPos - projectile.Center;
					Origins.shaderOroboros.Stack(GameShaders.Armor.GetSecondaryShader(Rasterized_Dye.ShaderID, null), projectile);
				} finally {
					projectile.velocity = velocity;
				}
				Origins.shaderOroboros.Release();
			}
		}
	}
	public record OwnerMinionKey(int Type, int Owner, int Identity);
}
