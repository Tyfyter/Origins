using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.Items.Tools;
using Origins.Items.Weapons.Magic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins {
	public partial class OriginPlayer : ModPlayer {
		public override void PostUpdateEquips() {
			if (eyndumSet) {
				ApplyEyndumSetBuffs();
				if (eyndumCore?.Value?.ModItem is ModItem equippedCore) {
					equippedCore.UpdateEquip(Player);
				}
			}
			Player.buffImmune[Rasterized_Debuff.ID] = Player.buffImmune[BuffID.Cursed];
			if (tornDebuff) {
				LinearSmoothing(ref tornCurrentSeverity, tornTarget, tornSeverityRate);
			}
			if (explosiveArtery) {
				if (explosiveArteryCount == -1) {
					explosiveArteryCount = CombinedHooks.TotalUseTime(explosiveArteryItem.useTime, Player, explosiveArteryItem);
				}
				if (explosiveArteryCount > 0) {
					explosiveArteryCount--;
				} else {
					const float maxDist = 512 * 512;
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
			if (soulhideSet) {
				const float maxDistTiles = 10f * 16;
				const float maxDistTiles2 = 15f * 16;
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC currentTarget = Main.npc[i];
					if (currentTarget.CanBeChasedBy()) {
						float distSquared = (currentTarget.Center - Player.MountedCenter).LengthSquared();
						if (distSquared < maxDistTiles * maxDistTiles) {
							currentTarget.AddBuff(Weak_Shadowflame_Debuff.ID, 5);
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
				const float maxDist = 256 * 256;
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
					case 13://???
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
			}
		}
		public override void PostUpdateMiscEffects() {
			if (cryostenHelmet) {
				if (Player.statLife != Player.statLifeMax2 && (int)Main.time % (cryostenLifeRegenCount > 0 ? 5 : 15) == 0) {
					for (int i = 0; i < 10; i++) {
						int num6 = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Frost);
						Main.dust[num6].noGravity = true;
						Main.dust[num6].velocity *= 0.75f;
						int num7 = Main.rand.Next(-40, 41);
						int num8 = Main.rand.Next(-40, 41);
						Main.dust[num6].position.X += num7;
						Main.dust[num6].position.Y += num8;
						Main.dust[num6].velocity.X = -num7 * 0.075f;
						Main.dust[num6].velocity.Y = -num8 * 0.075f;
					}
				}
			}
			if (tendonSet) {
				Player.moveSpeed *= Math.Min((Player.statLife / 167) + 1, 1.65f);
				Player.jumpSpeedBoost += Math.Min(Player.statLife / 167, 5);
				Player.runAcceleration *= Math.Min((Player.statLife / 167) + 1, 1.85f);
			}
			if (Main.myPlayer == Player.whoAmI && protozoaFood && protozoaFoodCooldown <= 0 && Player.ownedProjectileCounts[Mini_Protozoa_P.ID] < Player.maxMinions) {
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
			if (statSharePercent != 0f) {
				foreach (DamageClass damageClass in DamageClasses.All) {
					if (damageClass == DamageClass.Generic) {
						continue;
					}
					Player.GetArmorPenetration(DamageClass.Generic) += Player.GetArmorPenetration(damageClass) * statSharePercent;
					Player.GetArmorPenetration(damageClass) -= Player.GetArmorPenetration(damageClass) * statSharePercent;

					Player.GetDamage(DamageClass.Generic) = Player.GetDamage(DamageClass.Generic).CombineWith(Player.GetDamage(damageClass).Scale(statSharePercent));
					Player.GetDamage(damageClass) = Player.GetDamage(damageClass).Scale(1f - statSharePercent);

					Player.GetAttackSpeed(DamageClass.Generic) += (Player.GetAttackSpeed(damageClass) - 1) * statSharePercent;
					Player.GetAttackSpeed(damageClass) -= (Player.GetAttackSpeed(damageClass) - 1) * statSharePercent;

					float crit = Player.GetCritChance(damageClass) * statSharePercent;
					Player.GetCritChance(DamageClass.Generic) += crit;
					Player.GetCritChance(damageClass) -= crit;
				}
			}
			if (hookTarget >= 0) {
				Projectile projectile = Main.projectile[hookTarget];
				if (projectile.type == Amoeba_Hook_Projectile.ID) {
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
					Player.endurance += 0.15f;
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
				Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Ash, Alpha: 100).noGravity = true;
				if (Player.immuneTime % 15 == 1) {
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
			oldGravDir = Player.gravDir;
		}
		public override void UpdateDyes() {
			if (Ravel_Mount.RavelMounts.Contains(Player.mount.Type)) {
				for (int i = Player.SupportedSlotsArmor; i < Player.SupportedSlotsArmor + Player.SupportedSlotsAccs; i++) {
					if (Player.armor[i].ModItem is Ravel) {
						Player.cMount = Player.dye[i].dye;
					}
					if (Player.armor[i + 10].ModItem is Ravel) {
						Player.cMount = Player.dye[i].dye;
						break;
					}
				}
			}
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
			Player.pickSpeed *= (Player.pickSpeed - 1) * 0.5f;
			Player.aggro += Player.aggro / 2;
			Player.blockRange += Player.blockRange / 2;
			#endregion
		}
		public void TriggerSetBonus() {
			if (setAbilityCooldown > 0) return;
			switch (setActiveAbility) {
				case 1: {
					Vector2 speed = Vector2.Normalize(Main.MouseWorld - Player.MountedCenter) * 14;
					int type = ModContent.ProjectileType<Infusion_P>();
					for (int i = -5; i < 6; i++) {
						Projectile.NewProjectile(Player.GetSource_FromThis(), Player.MountedCenter + speed.RotatedBy(MathHelper.PiOver2) * i * 0.25f + speed * (5 - Math.Abs(i)) * 0.75f, speed, type, 40, 7, Player.whoAmI);
					}
					setAbilityCooldown = 30;
					if (Player.manaRegenDelay < 60) Player.manaRegenDelay = 60;
				}
				break;
				case 2: {
					if (Player.CheckMana((int)(40 * Player.manaCost), true)) {
						setAbilityCooldown = 1800;
						Player.AddBuff(Mimic_Buff.ID, 600);
						Player.AddBuff(BuffID.Heartreach, 30);
					}
				}
				break;
				case 3:
				break;
				default:
				break;
			}
		}
		public override void UpdateLifeRegen() {
			if (cryostenHelmet) Player.lifeRegenCount += cryostenLifeRegenCount > 0 ? 180 : 1;
			if (focusCrystal) {
				float factor = Player.dpsDamage / 200f;
				int rounded = (int)factor;
				Player.lifeRegenCount += rounded;
				Player.lifeRegenTime += (int)((factor - rounded) * 50);
			}
		}
		public void SetMimicSetChoice(int level, int choice) {
			mimicSetChoices = (mimicSetChoices & ~(3 << level * 2)) | ((choice & 3) << level * 2);
		}
		public int GetMimicSetChoice(int level) {
			return (mimicSetChoices >> level * 2) & 3;
		}
	}
}
