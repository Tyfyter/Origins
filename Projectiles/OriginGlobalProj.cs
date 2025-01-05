using AltLibrary.Common.AltBiomes;
using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Graphics;
using Origins.Items;
using Origins.Items.Accessories;
using Origins.Items.Armor.Felnum;
using Origins.Items.Other.Dyes;
using Origins.Items.Tools;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Ranged;
using Origins.NPCs;
using Origins.Questing;
using Origins.Reflection;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

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
		public bool ownerSafe = false;
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
		public Vector2? weakpointAnalyzerTarget = default;
		public Vector2 extraGravity = default;
		public bool shouldUnmiss = false;
		public int unmissTarget = -1;
		public Vector2 unmissTargetPos = default;
		public int unmissAnimation = 0;
		public static Dictionary<int, Action<OriginGlobalProj, Projectile, string[]>> itemSourceEffects;
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
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			felnumBonus = MainReflection._currentPlayerOverride.GetValue() is null ? 0 : Main.CurrentPlayer.OriginPlayer().felnumShock;
			string[] contextArgs = source?.Context?.Split(';') ?? [];
			if (projectile.aiStyle is ProjAIStyleID.Explosive or ProjAIStyleID.Bobber or ProjAIStyleID.GolfBall && projectile.originalDamage < projectile.damage)
				projectile.originalDamage = projectile.damage;
			if (contextArgs.Contains(nameof(OriginPlayer.weakpointAnalyzer))) {
				weakpointAnalyzerTarget = Main.MouseWorld;
			}
			if (source is EntitySource_ItemUse itemUseSource) {
				if (itemSourceEffects.TryGetValue(itemUseSource.Item.type, out var itemSourceEffect)) itemSourceEffect(this, projectile, contextArgs);
				OriginPlayer originPlayer = itemUseSource.Player.GetModPlayer<OriginPlayer>();
				if (itemUseSource.Item.ModItem is IElementalItem elementalItem && (elementalItem.Element & Elements.Fiberglass) != 0 && originPlayer.entangledEnergy) {
					fiberglassLifesteal = true;
				}
				Prefix = itemUseSource.Item.prefix;
				ModPrefix projPrefix = PrefixLoader.GetPrefix(Prefix);

				if (projPrefix is IOnSpawnProjectilePrefix spawnPrefix) {
					spawnPrefix.OnProjectileSpawn(projectile, source);
				}

				fromItemType = itemUseSource.Item.type;
				if (fromItemType == Neural_Network.ID) {
					neuralNetworkEffect = true;
				}
				if (!contextArgs.Contains(multishot_context) && !contextArgs.Contains(no_multishot_context)) {
					int bocShadows = 0;
					if (originPlayer.weakpointAnalyzer && projectile.CountsAsClass(DamageClass.Ranged) && projectile.aiStyle != ProjAIStyleID.HeldProjectile) {
						bocShadows = 2;
					} else if (originPlayer.controlLocus && projectile.aiStyle != ProjAIStyleID.HeldProjectile) {
						if (projectile.CountsAsClass(DamageClasses.Explosive)) bocShadows = 2;
						if (projectile.CountsAsClass(DamageClass.Ranged)) bocShadows = 5;
					}
					EntitySource_ItemUse multishotSource = null;
					int ammoID = ItemID.None;
					if (bocShadows > 0) {// separate if statement for future multishot sources
						multishotSource = itemUseSource.WithContext(source.Context, multishot_context, nameof(OriginPlayer.weakpointAnalyzer));
						if (itemUseSource is EntitySource_ItemUse_WithAmmo sourceWAmmo) {
							ammoID = sourceWAmmo.AmmoItemIdUsed;
						}
					}
					if (bocShadows > 0) {
						for (int i = bocShadows; i-- > 0;) {
							float rot = MathHelper.TwoPi * ((i + 1f) / (bocShadows + 1f)) + Main.rand.NextFloat(-0.3f, 0.3f);
							Vector2 _position = projectile.position.RotatedBy(rot, Main.MouseWorld);
							Vector2 _velocity = projectile.velocity.RotatedBy(rot);
							bool free = itemUseSource.Player.IsAmmoFreeThisShot(itemUseSource.Item, new(ammoID), projectile.type);
							int _damage = free ? projectile.damage : 0;
							Projectile.NewProjectile(multishotSource, _position, _velocity, projectile.type, _damage, projectile.knockBack, projectile.owner, projectile.ai[0], projectile.ai[1], projectile.ai[2]);
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
					Prefix = parentGlobalProjectile.Prefix;
					neuralNetworkEffect = parentGlobalProjectile.neuralNetworkEffect;
					neuralNetworkHit = parentGlobalProjectile.neuralNetworkHit;
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
					if (parentNPC.GetGlobalNPC<OriginGlobalNPC>().soulhideWeakenedDebuff) {
						projectile.damage = (int)(projectile.damage * (1f - OriginGlobalNPC.soulhideWeakenAmount));
					}
				}
			}
			if (Strange_Computer.drawingStrangeLine) {
				Strange_Computer.projectiles.Add(projectile.whoAmI);
			}
		}
		public override void PostAI(Projectile projectile) {
			if (projectile.aiStyle is ProjAIStyleID.Explosive or ProjAIStyleID.Bobber or ProjAIStyleID.GolfBall)
				projectile.damage = projectile.originalDamage;
			if (!OriginPlayer.ShouldApplyFelnumEffectOnShoot(projectile)) felnumBonus = Main.player[projectile.owner].OriginPlayer().felnumShock;
			if (shouldUnmiss) {
				if (Origins.MagicTripwireRange[projectile.type] >= 0) {
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
						if (npc.CanBeChasedBy() && magicTripwireHitbox.Intersects(npc.Hitbox)) {
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
					if (tripper != -1) {
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
						if (target is not null) {
							unmissTargetPos = target.Center - projectile.velocity * 10;
							if (++unmissAnimation == 8) {
								shouldUnmiss = false;
								(unmissTargetPos, projectile.Center) = (projectile.Center, unmissTargetPos);
							}
						} else {
							unmissTarget = -1;
							if (unmissAnimation > 0) unmissAnimation--;
						}
					}
				}
			} else if (unmissAnimation > 0) {
				unmissAnimation--;
			}
		}
		public override void AI(Projectile projectile) {
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
							Mitosis_P.aiVariableResets[projectile.type][0] ? 0 : projectile.ai[0],
							Mitosis_P.aiVariableResets[projectile.type][1] ? 0 : projectile.ai[1],
							Mitosis_P.aiVariableResets[projectile.type][2] ? 0 : projectile.ai[2]
						);
						duplicated.rotation += 0.25f;

						projectile.velocity = projectile.velocity.RotatedBy(-0.25f);
						projectile.rotation -= 0.25f;
						hasUsedMitosis = true;
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
			if (felnumBonus > Felnum_Helmet.shock_damage_divisor) {
				if (!ProjectileID.Sets.IsAWhip[projectile.type]) {
					Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity.RotatedByRandom(0.1) * 0.5f, Scale: 0.5f);
				}
			}
			if (viperEffect && projectile.extraUpdates != 19) {
				Lighting.AddLight(projectile.Center, 0, 0.75f * projectile.scale, 0.3f * projectile.scale);
				Dust dust = Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity.RotatedByRandom(0.1f) * -0.25f, 100, new Color(0, 255, 0), projectile.scale / 2);
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
		}
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			if (fiberglassLifesteal) {
				Projectile.NewProjectile(
					projectile.GetSource_OnHit(target),
					target.Center,
					default,
					ModContent.ProjectileType<Entangled_Energy_Lifesteal>(),
					(int)MathF.Ceiling(damageDone / 10f),
					0,
					projectile.owner
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
			if (neuralNetworkEffect) {
				neuralNetworkHit = true;
				if (target.CanBeChasedBy(projectile)) {
					int buffType = ModContent.BuffType<Neural_Network_Buff>();
					Main.player[projectile.owner].AddBuff(buffType, 1);
				}
			}
		}
		public override bool CanHitPlayer(Projectile projectile, Player target) {
			return ownerSafe ? target.whoAmI != projectile.owner : true;
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
				Main.player[projectile.owner].ClearBuff(ModContent.BuffType<Neural_Network_Buff>());
			}
		}
		public override Color? GetAlpha(Projectile projectile, Color lightColor) {
			if (weakpointAnalyzerTarget is Vector2 targetPos) {
				Color baseColor = projectile.ModProjectile?.GetAlpha(lightColor) ?? lightColor;
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

			binaryWriter.Write(Prefix);
			if (OriginPlayer.ShouldApplyFelnumEffectOnShoot(projectile)) binaryWriter.Write(felnumBonus);
			binaryWriter.Write((sbyte)updateCountBoost);

			bitWriter.WriteBit(weakpointAnalyzerTarget.HasValue);
			if (weakpointAnalyzerTarget.HasValue) binaryWriter.WriteVector2(weakpointAnalyzerTarget.Value);

			bitWriter.WriteBit(extraGravity != default);
			if (extraGravity != default) binaryWriter.WriteVector2(extraGravity);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			viperEffect = bitReader.ReadBit();
			isFromMitosis = bitReader.ReadBit();
			shouldUnmiss = bitReader.ReadBit();

			Prefix = binaryReader.ReadInt32();
			if (OriginPlayer.ShouldApplyFelnumEffectOnShoot(projectile)) felnumBonus = binaryReader.ReadSingle();
			SetUpdateCountBoost(projectile, binaryReader.ReadSByte());

			if (bitReader.ReadBit()) weakpointAnalyzerTarget = binaryReader.ReadVector2();

			if (bitReader.ReadBit()) extraGravity = binaryReader.ReadVector2();
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
}
