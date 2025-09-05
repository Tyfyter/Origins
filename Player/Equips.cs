using Mono.Cecil;
using Origins.Buffs;
using Origins.Dusts;
using Origins.Items;
using Origins.Items.Accessories;
using Origins.Items.Armor.Aetherite;
using Origins.Items.Armor.Chambersite;
using Origins.Items.Mounts;
using Origins.Items.Other;
using Origins.Items.Tools;
using Origins.Items.Weapons.Magic;
using Origins.Layers;
using Origins.Projectiles;
using PegasusLib;
using PegasusLib.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins {
	public partial class OriginPlayer : ModPlayer {
		public override void UpdateEquips() {
			Debugging.LogFirstRun(UpdateEquips);
		}
		public override void PostUpdateEquips() {
			Debugging.LogFirstRun(PostUpdateEquips);
			if (bugZapper && tornCurrentSeverity > 0) {
				Player.statDefense *= 1.15f + tornCurrentSeverity;
			}
			if (donorWristband) {
				Player.pStone = false;
			}
			if (eyndumSet) {
				ApplyEyndumSetBuffs();
				if (eyndumCore?.Value?.ModItem is ModItem equippedCore) {
					equippedCore.UpdateEquip(Player);
				}
			}
			Player.buffImmune[Rasterized_Debuff.ID] = Player.buffImmune[BuffID.Cursed];
			if (tornDebuff) {
				LinearSmoothing(ref tornCurrentSeverity, tornTarget, tornTarget > tornCurrentSeverity ? tornSeverityRate : tornSeverityDecayRate);
			}
			if (soulhideSet) {
				const float maxDistTiles = 10f * 16;
				const float maxDistTiles2 = 15f * 16;
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC currentTarget = Main.npc[i];
					if (currentTarget.CanBeChasedBy()) {
						float distSquared = (currentTarget.Center - Player.MountedCenter).LengthSquared();
						if (distSquared < maxDistTiles * maxDistTiles) {
							currentTarget.AddBuff(Shadefire_Debuff.ID, 5);
						}
						if (distSquared < maxDistTiles2 * maxDistTiles2) {
							currentTarget.AddBuff(Soulhide_Weakened_Debuff.ID, 5);
							if (Main.rand.NextBool(6)) {
								Vector2 pos = Main.rand.NextVector2FromRectangle(currentTarget.Hitbox);
								Vector2 dir = (Player.MountedCenter - pos).SafeNormalize(default) * 4;
								Dust dust = Dust.NewDustPerfect(
									pos,
									DustID.Shadowflame,
									dir,
									120,
									Color.Red,
									1.25f
								);
								dust.noGravity = true;
								if (Main.rand.NextBool(4)) {
									dust.noGravity = false;
									dust.scale *= 0.5f;
								}
							}
						}
					}
				}
			}
			if (lousyLiverCount > 0) {
				float maxDist = lousyLiverRange * lousyLiverRange;
				List<(NPC target, float dist)> targets = new(lousyLiverCount);
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC currentTarget = Main.npc[i];
					if (currentTarget.CanBeChasedBy()) {
						float dist = (currentTarget.Center - Player.MountedCenter).LengthSquared();
						if (dist < maxDist) {
							int index = Math.Min(targets.Count, lousyLiverCount) + 1;
							for (; index-- > 0;) {
								if (index == 0 || targets[index - 1].dist < dist) {
									targets.Insert(index, (currentTarget, dist));
									break;
								}
							}
						}
					}
				}
				for (int i = 0; i < Math.Min(targets.Count, lousyLiverCount); i++) {
					for (int j = 0; j < lousyLiverDebuffs.Count; j++) {
						targets[i].target.AddBuff(lousyLiverDebuffs[j].id, lousyLiverDebuffs[j].duration);
					}
				}
				explosiveArteryCount = -1;
			}
			if (solarPanel) {
				static Vector3 DoTileCalcs(int x, int top, Vector3 sunLight, Vector3 waterFactor) {
					for (int y = top; y > 0; y--) {
						Tile tile = Framing.GetTileSafely(x, y);
						if (tile.HasUnactuatedTile && Main.tileBlockLight[tile.TileType]) {
							sunLight = Vector3.Zero;
						} else if (tile.LiquidAmount > 0) {
							switch (tile.LiquidType) {
								case 0:
								sunLight *= waterFactor;
								sunLight -= waterFactor * 0.01f;
								break;
								case 1:
								sunLight = Vector3.Zero;
								break;
								case 2:
								sunLight *= new Vector3(0.25f, 0.25f, 0f);
								sunLight -= new Vector3(0.0025f, 0.0025f, 0f);
								break;
							}
						} else if (y > Main.worldSurface) {
							sunLight *= new Vector3(0.999f, 0.999f, 0.999f);
							sunLight -= new Vector3(0.001f, 0.001f, 0.001f);
						}
						if (sunLight.X < 0) {
							sunLight.X = 0;
						}
						if (sunLight.Y < 0) {
							sunLight.Y = 0;
						}
						if (sunLight.Z < 0) {
							sunLight.Z = 0;
						}
						if (sunLight == Vector3.Zero) {
							break;
						}
					}
					return sunLight;
				}
				Vector3 sunLight = new Vector3(1f, 1f, 0.67f);
				float sunFactor = 0;
				int top = (int)Player.TopLeft.Y / 16;
				Vector3 waterFactor = Vector3.One;
				switch (Main.waterStyle) {
					case WaterStyleID.Purity:
					case WaterStyleID.Lava:
					case WaterStyleID.Underground:
					case WaterStyleID.Cavern:
					waterFactor = new Vector3(0.88f, 0.96f, 1.015f);
					break;

					case WaterStyleID.Corrupt:
					waterFactor = new Vector3(0.94f, 0.85f, 1.01f) * 0.91f;
					break;
					case WaterStyleID.Jungle:
					waterFactor = new Vector3(0.84f, 0.95f, 1.015f) * 0.91f;
					break;
					case WaterStyleID.Hallow:
					waterFactor = new Vector3(0.90f, 0.86f, 1.01f) * 0.91f;
					break;
					case WaterStyleID.Snow:
					waterFactor = new Vector3(0.64f, 0.99f, 1.01f) * 0.91f;
					break;
					case WaterStyleID.Desert:
					waterFactor = new Vector3(0.93f, 0.83f, 0.98f) * 0.91f;
					break;
					case WaterStyleID.Bloodmoon:
					waterFactor = new Vector3(1f, 0.88f, 0.84f) * 0.91f;
					break;
					case WaterStyleID.Crimson:
					waterFactor = new Vector3(0.83f, 1f, 1f) * 0.91f;
					break;
					case WaterStyleID.UndergroundDesert:
					waterFactor = new Vector3(0.95f, 0.98f, 0.85f) * 0.91f;
					break;
					case 13:
					waterFactor = new Vector3(0.9f, 1f, 1.02f) * 0.91f;
					break;

					case WaterStyleID.Honey:
					waterFactor = new Vector3(0f);
					break;

					default:
					if (LoaderManager.Get<WaterStylesLoader>().Get(Main.waterStyle) is ModWaterStyle waterStyle) waterStyle.LightColorMultiplier(ref waterFactor.X, ref waterFactor.Y, ref waterFactor.Z);
					break;
				}
				waterFactor /= 1.015f;

				sunFactor += DoTileCalcs((int)(Player.TopLeft.X + 1) / 16, top, sunLight, waterFactor).Length() / 1.56f;
				sunFactor += DoTileCalcs((int)Player.Top.X / 16, top, sunLight, waterFactor).Length() / 1.56f;
				sunFactor += DoTileCalcs((int)(Player.TopRight.X - 1) / 16, top, sunLight, waterFactor).Length() / 1.56f;

				Player.manaRegenCount += (int)(sunFactor * 12);
				//Player.chatOverhead.NewMessage("" + (int)(sunFactor * 12), 5);
			}
			if (blizzardwalkerJacket) {
				if (blizzardwalkerActiveTime < Blizzardwalkers_Jacket.max_active_time) {
					bool blizzardwalkerDanger = false;
					if (Player.nearbyActiveNPCs > 0) {
						const int range = 16 * 15;
						const int rangeSQ = range * range;
						const int bossRange = 16 * 45;
						const int bossRangeSQ = bossRange * bossRange;
						for (int i = 0; i < Main.maxNPCs; i++) {
							NPC npc = Main.npc[i];
							if (npc.active && npc.damage > 0 && npc.npcSlots > 0 && Player.DistanceSQ(npc.Center) <= (npc.boss ? bossRangeSQ : rangeSQ)) {
								blizzardwalkerDanger = true;
							}
						}
					}
					if (blizzardwalkerDanger) {
						const int decayRate = 6;
						if (blizzardwalkerActiveTime > decayRate) {
							blizzardwalkerActiveTime -= decayRate;
						} else {
							blizzardwalkerActiveTime = 0;
						}
					} else {
						blizzardwalkerActiveTime++;
					}
				} else {
					if ((int)Main.timeForVisualEffects % 5 == 0) Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Asphalt, 0f, -3f, 0, default, 1.4f).noGravity = true;
				}
			}
			if (scrapCompactor) {
				if (++scrapCompactorTimer % 15 == 0 && Player.nearbyActiveNPCs > 0) {
					const int range = 16 * 20;
					const int rangeSQ = range * range;
					foreach (NPC npc in Main.ActiveNPCs) {
						if (npc.life <= npc.lifeMax * (npc.boss ? 0.1f : 0.4f) && npc.HasBuff<Impeding_Shrapnel_Debuff>() && Player.DistanceSQ(npc.Center) <= rangeSQ) {
							npc.StrikeNPC(new NPC.HitInfo() {
								Damage = (int)(npc.lifeMax * (npc.boss ? 0.005f : 0.05f))
							});
						}
					}
				}
			}
			if (Player.miscEquips[3]?.ModItem is Ravel ravel) {
				ravel.UpdateEquip(Player);
			}
			if (Player.gemCount == 9) {
				ownedLargeGems.Clear();
				for (int i = 0; i <= 58; i++) {
					if (ModLargeGem.GemTextures[Player.inventory[i].type] is not null && !ownedLargeGems.Contains(Player.inventory[i].type)) {
						ownedLargeGems.Add(Player.inventory[i].type);
					}
				}
				ownedLargeGems.Sort();

				if (Player.trashItem?.IsAir != true && ModLargeGem.GemTextures[Player.trashItem.type] is not null) Player.trashItem.SetDefaults();
			}
		}
		public override void PostUpdateMiscEffects() {
			Debugging.LogFirstRun(PostUpdateMiscEffects);
			if (oldCryostenHelmet) {
				if (Player.statLife != Player.statLifeMax2) {
					bool buffed = cryostenLifeRegenCount > 0;
					int visualTime = (int)Main.timeForVisualEffects % (buffed ? 15 : 60);
					if (visualTime == 0 || visualTime == (buffed ? 2 : 8)) {
						float fadeIn = buffed ? 0.9f : 1.5f;
						float offsetMult = buffed ? 0.5f : 1;
						for (int i = 0; i < 4; i++) {
							Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Frost, Scale: 0.5f);
							dust.noGravity = true;
							dust.velocity *= 0.75f;
							float xOffset = Main.rand.NextFloat(-40, 40) * offsetMult;
							float yOffset = Main.rand.NextFloat(-40, 40) * offsetMult;
							dust.position.X += xOffset;
							dust.position.Y += yOffset;
							dust.velocity.X = -xOffset * 0.075f;
							dust.velocity.Y = -yOffset * 0.075f;
							dust.fadeIn = fadeIn;
						}
					}
				}
			}
			if (tendonSet) {
				Player.moveSpeed *= Math.Min((Player.statLife / 167) + 1, 1.65f);
				Player.jumpSpeedBoost += Math.Min(Player.statLife / 167, 5);
				Player.runAcceleration *= Math.Min((Player.statLife / 167) + 1, 1.375f);
			}
			if (explosiveArtery) {
				if (explosiveArteryCount == -1) {
					explosiveArteryCount = CombinedHooks.TotalUseTime(explosiveArteryItem.useTime, Player, explosiveArteryItem);
				}
				if (explosiveArteryCount > 0) {
					explosiveArteryCount--;
				} else {
					const float maxDist = 384 * 384;
					for (int i = 0; i < Main.maxNPCs; i++) {
						NPC currentTarget = Main.npc[i];
						if (Main.rand.NextBool(explosiveArteryItem.useAnimation, explosiveArteryItem.reuseDelay)) {
							if (currentTarget.CanBeChasedBy() && currentTarget.HasBuff(BuffID.Bleeding)) {
								Vector2 diff = currentTarget.Center - Player.MountedCenter;
								if (diff.LengthSquared() < maxDist) {
									Projectile.NewProjectileDirect(
										Player.GetSource_Accessory(explosiveArteryItem),
										currentTarget.Center,
										new Vector2(Math.Sign(diff.X), 0),
										explosiveArteryItem.shoot,
										Player.GetWeaponDamage(explosiveArteryItem),
										Player.GetWeaponKnockback(explosiveArteryItem),
										Player.whoAmI
									);
								}
							}
						}
					}
					explosiveArteryCount = -1;
				}
			}
			if (cursedVoice && Main.myPlayer == Player.whoAmI) {
				Player.AddBuff(cursedVoiceItem.buffType, 5);
				if (cursedVoiceCooldown <= 0) {
					if (Player.MouthPosition is Vector2 mouthPosition && Keybindings.ForbiddenVoice.JustPressed && Player.CheckMana(cursedVoiceItem.mana, true)) {
						Player.manaRegenDelay = (int)Player.maxRegenDelay;
						Projectile.NewProjectileDirect(
							Player.GetSource_Accessory(cursedVoiceItem),
							mouthPosition,
							Vector2.Zero,
							cursedVoiceItem.shoot,
							Player.GetWeaponDamage(cursedVoiceItem),
							Player.GetWeaponKnockback(cursedVoiceItem),
							Player.whoAmI
						);
						cursedVoiceCooldown = cursedVoiceCooldownMax = CombinedHooks.TotalUseTime(cursedVoiceItem.useTime, Player, cursedVoiceItem);
					}
				} else {
					int index = Player.FindBuffIndex(cursedVoiceItem.buffType);
					int time = cursedVoiceCooldown + 59;
					if (index != -1 && Player.buffTime[index] < time) Player.buffTime[index] = time;
				}
			}
			if (goldenLotus && Main.myPlayer == Player.whoAmI) {
				if (Keybindings.GoldenLotus.JustPressed) {
					int projType = goldenLotusItem.shoot;
					if (Player.ownedProjectileCounts[projType] <= 0) {
						goldenLotusProj = Projectile.NewProjectile(
							Player.GetSource_Accessory(goldenLotusItem),
							Player.Center,
							Vector2.Zero,
							projType,
							0,
							0,
							Player.whoAmI
						);
					} else {
						Projectile goldenLotusFairy = Main.projectile[goldenLotusProj];
						goldenLotusFairy.ai[0] = -1;
						for (int i = 0; i < Main.chest.Length; i++) {
							Chest chest = Main.chest[i];
							if (chest is null) continue;
							if (Player.tileTargetX >= chest.x && Player.tileTargetX <= chest.x + 1 && Player.tileTargetY >= chest.y && Player.tileTargetY <= chest.y + 1) {
								goldenLotusFairy.ai[0] = -2;
								goldenLotusFairy.ai[1] = i;
								break;
							}
						}
						goldenLotusFairy.netUpdate = true;
					}
				}
			}
			if (WishingGlass && wishingGlassCooldown <= 0 && Player.GetModPlayer<SyncedKeybinds>().WishingGlass.JustPressed) {
				if (wishingGlassVisible) {
					Wishing_Glass_Layer.StartAnimation(ref wishingGlassAnimation);
				} else {
					int dustType = ModContent.DustType<Following_Shimmer_Dust>();
					for (int i = 0; i < 20; i++) {
						Dust dust = Dust.NewDustDirect(
							Player.position,
							Player.width,
							Player.height,
							dustType,
							0f,
							0f,
							100,
							default,
							2.5f
						);
						dust.noGravity = true;
						dust.velocity *= 7f;
						dust.velocity += Player.velocity * 0.75f;
						dust.customData = new Following_Shimmer_Dust.FollowingDustSettings(Player);

						dust = Dust.NewDustDirect(
							Player.position,
							Player.width,
							Player.height,
							dustType,
							0f,
							0f,
							100,
							default,
							1.5f
						);
						dust.velocity *= 3f;
						dust.velocity += Player.velocity * 0.75f;
						dust.customData = new Following_Shimmer_Dust.FollowingDustSettings(Player);
					}
				}
				if (Main.myPlayer == Player.whoAmI) {
					Player.AddBuff(ModContent.BuffType<Wishing_Glass_Buff>(), 8 * 60);
				}
			}
			if (Main.myPlayer == Player.whoAmI && protozoaFood && protozoaFoodCooldown <= 0 && Player.ownedProjectileCounts[Mini_Protozoa_P.ID] < Player.maxMinions && Player.CheckMana(protozoaFoodItem, pay: true)) {
				//Player.manaRegenDelay = (int)Player.maxRegenDelay;
				Item item = protozoaFoodItem;
				int damage = Player.GetWeaponDamage(item);
				Projectile.NewProjectileDirect(
					Player.GetSource_Accessory(item),
					Player.Center,
					OriginExtensions.Vec2FromPolar(Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi), Main.rand.NextFloat(1, 8)),
					Mini_Protozoa_P.ID,
					damage,
					Player.GetWeaponKnockback(item),
					Player.whoAmI
				).originalDamage = damage;
				protozoaFoodCooldown = item.useTime;
			}
			if (Main.myPlayer == Player.whoAmI && (strangeToothItem?.IsAir == false) && strangeToothCooldown <= 0 && Player.ownedProjectileCounts[Strange_Tooth_Minion.ID] < Player.maxMinions) {
				Item item = strangeToothItem;
				int damage = Player.GetWeaponDamage(item);
				Player.AddBuff(Strange_Tooth_Buff.ID, 5);
				Projectile.NewProjectileDirect(
					Player.GetSource_Accessory(item),
					Player.Center,
					OriginExtensions.Vec2FromPolar(Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi), Main.rand.NextFloat(1, 8)),
					Strange_Tooth_Minion.ID,
					damage,
					Player.GetWeaponKnockback(item),
					Player.whoAmI
				).originalDamage = damage;
				strangeToothCooldown = item.useTime;
			}
			if (statSharePercent != 0f) {
				foreach (DamageClass damageClass in DamageClasses.All) {
					if (damageClass == DamageClass.Generic) {
						continue;
					}
					float currentStatSharePercent = statSharePercent;
					if (OriginConfig.Instance.StatShareRatio.TryGetValue(new(damageClass.FullName), out float multiplier)) currentStatSharePercent *= multiplier;
					Player.GetArmorPenetration(DamageClass.Generic) += Player.GetArmorPenetration(damageClass) * currentStatSharePercent;
					Player.GetArmorPenetration(damageClass) -= Player.GetArmorPenetration(damageClass) * currentStatSharePercent;

					Player.GetDamage(DamageClass.Generic) = Player.GetDamage(DamageClass.Generic).CombineWith(Player.GetDamage(damageClass).Scale(currentStatSharePercent));
					Player.GetDamage(damageClass) = Player.GetDamage(damageClass).Scale(1f - currentStatSharePercent);

					Player.GetAttackSpeed(DamageClass.Generic) += (Player.GetAttackSpeed(damageClass) - 1) * currentStatSharePercent;
					Player.GetAttackSpeed(damageClass) -= (Player.GetAttackSpeed(damageClass) - 1) * currentStatSharePercent;

					float crit = Player.GetCritChance(damageClass) * currentStatSharePercent;
					if (damageClass is ExplosivePlus) crit += 2;
					Player.GetCritChance(DamageClass.Generic) += crit;
					Player.GetCritChance(damageClass) -= crit;
				}
			}
			if (hookTarget >= 0) {
				Projectile projectile = Main.projectile[hookTarget];
				if (projectile.type == Amoeba_Hook_P.ID) {
					if (projectile.ai[1] < 5 || Player.controlJump || (Player.Center - projectile.Center).LengthSquared() > 2304) {
						Player.GoingDownWithGrapple = true;
					}
				}
			}
			if (graveDanger) {
				if (Player.difficulty != 2) {
					//1 minute (3600 ticks), decays over the latter 45 seconds (2700 ticks)
					float factor = Math.Min((3600 - timeSinceLastDeath) / 2700f, 1);
					if (factor > 0) {
						Player.statDefense += (int)MathF.Ceiling(Player.statDefense * factor * 0.25f);
						if (Main.rand.NextFloat(1.25f) < factor + 0.1f) {
							Dust dust = Dust.NewDustDirect(Player.position - new Vector2(8, 0), Player.width + 16, Player.height, DustID.Smoke, newColor: new Color(0.1f, 0.1f, 0.2f));
							dust.velocity *= 0.4f;
							dust.alpha = 150;
						}
					}
				} else {
					Player.endurance += (1 - Player.endurance) * 0.15f;
				}
			}
			if (hasProtOS) {
				if (!Player.noFallDmg && Player.equippedWings == null) {
					float distance = (Player.position.Y / 16 - Player.fallStart) * Player.gravDir;
					float extraFall = Player.extraFall;
					float mult = 10f;
					if (Player.stoned) {
						extraFall = 2;
						mult = 20;
					}
					if ((distance - extraFall) * mult * (1 - Player.endurance) > Player.statLife) {
						Protomind.PlayRandomMessage(Protomind.QuoteType.Falling, protOSQuoteCooldown, Player.Top);
					}
				}
			}
			if (hasProtOS && Player.gravDir != oldGravDir) {
				Protomind.PlayRandomMessage(Protomind.QuoteType.Respawn, protOSQuoteCooldown, Player.Top);
			}

			if (cinderSealItem?.ModItem is not null && cinderSealCount > 0 && Player.immuneTime > 0) {
				for (int i = 0; i < cinderSealCount; i++) {
					Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Ash).noGravity = true;
				}
				if (Player.immuneTime % cinderSealItem.useTime == 1) {
					cinderSealCount--;
					cinderSealItem.ModItem.Shoot(
						Player,
						Player.GetSource_ItemUse_WithPotentialAmmo(cinderSealItem, ItemID.None) as Terraria.DataStructures.EntitySource_ItemUse_WithAmmo,
						Player.Center,
						Vector2.Zero,
						cinderSealItem.shoot,
						Player.GetWeaponDamage(cinderSealItem),
						Player.GetWeaponKnockback(cinderSealItem)
					);

				}
			}
			if (slagBucket && Player.onFire) {
				Player.onFire = false;
				Player.lifeRegenCount += 4;
				if (Main.rand.NextBool(4)) {
					Dust dust18 = Dust.NewDustDirect(Player.position - Vector2.One * 2, Player.width + 4, Player.height + 4, DustID.Torch, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default, 3f);
					dust18.noGravity = true;
					dust18.velocity *= 1.8f;
					dust18.velocity.Y -= 0.5f;
				}
			}
			if (emergencyBeeCanister && Player.honeyWet) Player.ignoreWater = true;
			if (staticShock) Static_Shock_Debuff.ProcessShocking(Player, miniStaticShock ? 7 : 5);
			else if (miniStaticShock) Static_Shock_Debuff.ProcessShocking(Player, 2);
			if (mildewHeart) {
				float speed = 0.25f * mildewHeartRegenMult;
				if (Player.statLife <= 0) {
					Player.lifeRegenCount = 0;
					speed = 0.81f;
					if (Player.whoAmI == Main.myPlayer) {
						if (mildewHealth <= 0) {
							Player.KillMe(lastMildewDeathReason, 9999, 0, lastMildewDeathPvP);
						}
					} else {
						Player.statLife = 1;
					}
				} else if (Player.statLife < mildewHealth) {
					Player.lifeRegenCount += 24;
					speed = 0.1f;
				}
				MathUtils.LinearSmoothing(ref mildewHealth, Math.Min(Player.statLifeMax2 * 0.65f, Player.statLife), speed);
			} else {
				if (mildewHealth > 0 && Player.statLife <= 0 && Player.whoAmI == Main.myPlayer) {
					Player.KillMe(lastMildewDeathReason, 9999, 0, lastMildewDeathPvP);
				}
				mildewHealth = 0;
			}
			if (necromancyPrefixMana > Player.statManaMax2 * Necromantic_Prefix.MaxManaMultiplier) {
				necromancyPrefixMana *= 0.97f;
				Player.manaRegenBonus += (int)((necromancyPrefixMana - Player.statManaMax2) * 0.01f);
				Player.manaRegenDelay = 0;
				if (necromancyPrefixMana < Player.statManaMax2 * Necromantic_Prefix.MaxManaMultiplier) necromancyPrefixMana = Player.statManaMax2 * Necromantic_Prefix.MaxManaMultiplier;
			}
		}
		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource) {
			if (mildewHeart) {
				if (tornCurrentSeverity >= 1 || Player.statLifeMax2 <= 0) {
					mildewHealth = 0;
				}
				if (Player.whoAmI != Main.myPlayer) return damage >= 9999;
				if (mildewHealth > 0) {
					lastMildewDeathReason = damageSource;
					lastMildewDeathPvP = pvp;
				}
				return mildewHealth <= 0;
			}
			return true;
		}
		public override void UpdateDyes() {
			Debugging.LogFirstRun(UpdateDyes);
		}
		public void ApplyEyndumSetBuffs() {
			#region movement
			float speedMult = (Player.moveSpeed - 1) * 0.5f;
			Player.runAcceleration += (Player.runAcceleration / Player.moveSpeed) * speedMult;
			Player.maxRunSpeed += (Player.maxRunSpeed / Player.moveSpeed) * speedMult;
			Player.extraFall += Player.extraFall / 2;
			Player.wingTimeMax += Player.wingTimeMax / 2;
			Player.jumpSpeedBoost += Player.jumpSpeedBoost * 0.5f;
			if (Player.spikedBoots == 1) Player.spikedBoots = 2;
			#endregion
			#region defense
			Player.statLifeMax2 += (Player.statLifeMax2 - Player.statLifeMax) / 2;
			Player.statDefense += Player.statDefense / 2;
			Player.endurance += Player.endurance * 0.5f;
			Player.lifeRegen += Player.lifeRegen / 2;
			Player.thorns += Player.thorns * 0.5f;
			Player.lavaMax += Player.lavaMax / 2;
			#endregion
			#region damage
			foreach (DamageClass damageClass in DamageClasses.All) {
				Player.GetArmorPenetration(damageClass) += Player.GetArmorPenetration(damageClass) * 0.5f;

				Player.GetDamage(damageClass) = Player.GetDamage(damageClass).Scale(1.5f);

				Player.GetAttackSpeed(damageClass) += (Player.GetAttackSpeed(damageClass) - 1) * 0.5f;
			}

			Player.arrowDamage = Player.arrowDamage.Scale(1.5f);
			Player.bulletDamage = Player.bulletDamage.Scale(1.5f);
			Player.specialistDamage = Player.specialistDamage.Scale(1.5f);

			explosiveBlastRadius = explosiveBlastRadius.Scale(1.5f);

			//explosiveDamage += (explosiveDamage - 1) * 0.5f;
			//explosiveThrowSpeed += (explosiveThrowSpeed - 1) * 0.5f;
			//explosiveSelfDamage += (explosiveSelfDamage - 1) * 0.5f;
			#endregion
			#region resources
			Player.statManaMax2 += (Player.statManaMax2 - Player.statManaMax) / 2;
			Player.manaCost += (Player.manaCost - 1) * 0.5f;
			Player.maxMinions += (Player.maxMinions - 1) / 2;
			Player.maxTurrets += (Player.maxTurrets - 1) / 2;
			Player.manaRegenBonus += Player.manaRegenBonus / 2;
			Player.manaRegenDelayBonus += Player.manaRegenDelayBonus / 2;
			#endregion
			#region utility
			Player.wallSpeed += (Player.wallSpeed - 1) * 0.5f;
			Player.tileSpeed += (Player.tileSpeed - 1) * 0.5f;
			Player.pickSpeed += (Player.pickSpeed - 1) * 0.5f;
			Player.aggro += Player.aggro / 2;
			Player.blockRange += Player.blockRange / 2;
			#endregion
		}
		public void TriggerSetBonus(bool fromNet = false) {
			if (setAbilityCooldown > 0 || Player.DeadOrGhost) return;
			if (!fromNet && Main.netMode != NetmodeID.SinglePlayer) {

			}
			switch (setActiveAbility) {
				case 1: {
					if (fromNet) break;
					Vector2 speed = Vector2.Normalize(Main.MouseWorld - Player.MountedCenter) * 14;
					int type = ModContent.ProjectileType<Infusion_P>();
					for (int i = -5; i < 6; i++) {
						Projectile.NewProjectile(Player.GetSource_FromThis(), Player.MountedCenter + speed.RotatedBy(MathHelper.PiOver2) * i * 0.25f + speed * (5 - Math.Abs(i)) * 0.75f, speed, type, 40, 7, Player.whoAmI);
					}
					setAbilityCooldown = 30;
					if (Player.manaRegenDelay < 60) Player.manaRegenDelay = 60;
					break;
				}

				case 2: {
					if (Player.CheckMana((int)(40 * Player.manaCost), true)) {
						setAbilityCooldown = 1800;
						Player.AddBuff(Mimic_Buff.ID, 600);
						Player.AddBuff(BuffID.Heartreach, 30);
					}
					break;
				}

				case 3:
				break;

				case SetActiveAbility.blast_armor: {
					blastSetActive = true;
					break;
				}

				case SetActiveAbility.aetherite_armor: {
					setAbilityCooldown = 600;
					Player.SpawnProjectile(null,
						Player.MountedCenter,
						Vector2.Zero,
						ModContent.ProjectileType<Aetherite_Aura_P>(),
						0,
						0
					);
					break;
				}

				case SetActiveAbility.chambersite_armor: {
					setAbilityCooldown = 1200;
					int dmg = 90;
					Player.SpawnProjectile(null,
						Player.MountedCenter,
						Vector2.Zero,
						Chambersite_Commander_Sentinel.ID,
						dmg,
						1
					).originalDamage = dmg;
					Player.AddBuff(Chambersite_Commander_Sentinel_Buff.ID, 900);
					break;
				}

				default:
				break;
			}
		}
		public override void UpdateLifeRegen() {
			Debugging.LogFirstRun(UpdateLifeRegen);
			if (extremophileSet && Player.lifeRegen < 0) {
				Player.lifeRegen -= Main.rand.RandomRound(Player.lifeRegen * 0.333f);
			}
			if (oldCryostenHelmet) Player.lifeRegenCount += cryostenLifeRegenCount > 0 ? 60 : 1;
			if (bombCharminItLifeRegenCount > 0) {
				Player.lifeRegenCount += bombCharminItStrength;
				const float offsetMult = 1;
				const float fadeIn = 0;
				Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Asphalt, Scale: 0.75f);
				dust.noGravity = true;
				dust.velocity *= 0.75f;
				float xOffset = Main.rand.NextFloat(-40, 40) * offsetMult;
				float yOffset = Main.rand.NextFloat(-40, 40) * offsetMult;
				dust.position.X += xOffset;
				dust.position.Y += yOffset;
				dust.velocity.X = -xOffset * 0.075f;
				dust.velocity.Y = -yOffset * 0.075f;
				dust.fadeIn = fadeIn;
			}
			if (focusCrystal) {
				float factor = Player.dpsDamage / 200f;
				int rounded = (int)factor;
				Player.lifeRegenCount += rounded;
				Player.lifeRegenTime += (int)((factor - rounded) * 50);
			}
			if (primordialSoup) {
				Player.lifeRegenCount += (int)(tornCurrentSeverity * 18);
			}
			if (bugZapper) {
				Player.lifeRegenCount += (int)(tornCurrentSeverity * 22);
			}
		}
		public override void GetHealLife(Item item, bool quickHeal, ref int healValue) {
			if (entangledEnergy) healValue -= healValue / 4;
		}
		public void SetMimicSetChoice(int level, int choice) {
			mimicSetChoices = (mimicSetChoices & ~(3 << level * 2)) | ((choice & 3) << level * 2);
		}
		public int GetMimicSetChoice(int level) {
			return (mimicSetChoices >> level * 2) & 3;
		}
	}
	public static class SetActiveAbility {
		public const int blast_armor = 4;
		public const int aetherite_armor = 5;
		public const int chambersite_armor = 6;
	}
}
