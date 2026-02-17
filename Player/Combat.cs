using Origins.Buffs;
using Origins.Items;
using Origins.Items.Accessories;
using Origins.Items.Armor.Felnum;
using Origins.Items.Armor.Necromancer;
using Origins.Items.Other.Consumables;
using Origins.Items.Pets;
using Origins.Items.Tools;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Items.Weapons.Demolitionist;
using Origins.NPCs;
using Origins.NPCs.Brine;
using Origins.NPCs.Crimson;
using Origins.NPCs.Defiled;
using Origins.Projectiles;
using Origins.Questing;
using PegasusLib.ID;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins {
	public partial class OriginPlayer : ModPlayer {
		#region stats
		bool focusPotionThisUse = false;
		public override float UseAnimationMultiplier(Item item) {
			if (eitriteGunMagazine && item.useTime != item.useAnimation && item.IsAGun()) {
				return (item.useAnimation + item.useTime) / (float)item.useAnimation;
			}
			return 1f;
		}
		public override float UseSpeedMultiplier(Item item) {
			float speed = 1f;
			if (item.IsAGun()) speed += gunSpeedBonus;
			if (item.damage > 0 && retaliatoryTendrilStrength > 0) speed += retaliatoryTendrilStrength;
			return speed;
		}
		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			Debugging.LogFirstRun(ModifyWeaponDamage);
			if (entangledEnergy) {
				damage.Flat += Entangled_Energy.DamageBonus(Player.statLifeMax2 - Player.statLife);
			}
			if (Origins.ArtifactMinion[item.shoot]) damage = damage.CombineWith(artifactDamage);
			if (focusCrystal) {
				damage *= 1 + (focusCrystalTime / 360f);
			}
			if (focusPotion && (focusPotionThisUse || Player.CheckMana(Focus_Potion.GetManaCost(Player.HeldItem), false))) {
				damage *= 1 + Focus_Potion.bonus_multiplicative;
				damage.Flat += Focus_Potion.bonus_additive;
			}
			if ((item.CountsAsClass(DamageClass.Ranged) || item.CountsAsClass(DamageClasses.Explosive)) && LuckyHatSetActive) {
				float mult = (90f / Math.Max(item.useAnimation, 8)) * 1.3f;
				if (mult > 1) damage *= mult;
			}
			if (attackFromLunaticDuplicate) damage *= 0.333f;
			Debugging.LogFirstRun($"{nameof(ModifyWeaponDamage)} (after)");
		}
		public override void ModifyWeaponCrit(Item item, ref float crit) {
			if (rubyReticle) {
				crit += Player.GetWeaponDamage(item) * 0.15f;
			}
			if (blastSetActive && item.CountsAsClass<Explosive>()) {
				crit += Player.GetCritChance<Explosive>() * 0.5f;
			}
		}
		public override void ModifyManaCost(Item item, ref float reduce, ref float mult) {
			if (Origins.ArtifactMinion[item.shoot]) {
				mult *= artifactManaCost;
			}
			if (wishingGlassActive) mult *= 0;
			necromanaUsedThisUse = false;
		}
		public override bool CanConsumeAmmo(Item weapon, Item ammo) {
			if (wishingGlassActive) return false;
			if (ammo.CountsAsClass(DamageClasses.Explosive)) {
				if (endlessExplosives && Main.rand.NextBool(15, 100)) return false;
				if (controlLocus && Main.rand.NextBool(12, 100)) return false;
			} else if (ammo.CountsAsClass(DamageClass.Ranged)) {
				if (weakpointAnalyzer && Main.rand.NextBool(8, 100)) return false;
				if (controlLocus && Main.rand.NextBool(12, 100)) return false;
			}
			return true;
		}
		public override bool? CanAutoReuseItem(Item item) {
			if (destructiveClaws && item.CountsAsClass(DamageClasses.Explosive)) return true;
			return null;
		}
		public static bool ShouldApplyFelnumEffectOnShoot(Projectile projectile) =>
			(!projectile.CountsAsClass(DamageClass.Melee) &&
			!projectile.CountsAsClass(DamageClass.Summon) &&
			!ProjectileID.Sets.IsAWhip[projectile.type] &&
			!Origins.DamageModOnHit[projectile.type] &&
			projectile.aiStyle != ProjAIStyleID.WaterJet) ||
			Origins.ForceFelnumShockOnShoot[projectile.type];
		public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (advancedImaging) {
				velocity *= 1.38f;
			}
			StatModifier velocityModifier = projectileSpeedBoost;
			if (item.CountsAsClass(DamageClasses.Explosive)) {
				velocityModifier = velocityModifier.CombineWith(explosiveProjectileSpeed);
				if (item.useAmmo == 0 && item.CountsAsClass(DamageClass.Throwing)) {
					velocityModifier = velocityModifier.CombineWith(explosiveThrowSpeed);
				}
			}
			if (item.CountsAsClass(DamageClass.Throwing)) {
				velocityModifier = velocityModifier.CombineWith(thrownProjectileSpeed);
			}
			if (item.CountsAsClass(DamageClasses.Incantation)) {
				velocityModifier = velocityModifier.CombineWith(incantationProjSpeedBoost);
			}
			float speed = velocity.Length();
			if (speed != 0 && velocityModifier != StatModifier.Default) {
				velocity *= velocityModifier.ApplyTo(speed) / speed;
			}
			if (item.shoot > ProjectileID.None && felnumShock >= Felnum_Helmet.shock_damage_divisor * 2) {
				Projectile p = new();
				p.SetDefaults(type);
				if (!ShouldApplyFelnumEffectOnShoot(p)) return;
				usedFelnumShock = true;
				SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), position);
			}
		}
		#endregion
		public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (item.CountsAsClass(DamageClasses.Explosive)) {
				if (bleedingObsidianSet) {
					Fraction dmg = new(2, 2);
					int count = (madHand ? 1 : 0) + (Main.rand.NextBool(2) ? 1 : 0);
					dmg.D += count;
					damage *= dmg;
					double rot = Main.rand.NextBool(2) ? -0.1 : 0.1;
					for (int i = count; i-- > 0;) {
						Vector2 _velocity = velocity.RotatedBy(rot);
						if (ItemLoader.Shoot(item, Player, source, position, _velocity, type, damage, knockback)) {
							Projectile.NewProjectile(source, position, _velocity, type, damage, knockback, Player.whoAmI);
						}
						rot = -rot;
					}
				} else if (novaSet) {
					Fraction dmg = new(3, 3);
					int count = (madHand ? 1 : 0) + (Main.rand.NextBool(4) ? 0 : 1);
					dmg.D += count;
					damage *= dmg;
					double rot = Main.rand.NextBool(2) ? -0.1 : 0.1;
					for (int i = count; i-- > 0;) {
						Vector2 _velocity = velocity.RotatedBy(rot);
						if (ItemLoader.Shoot(item, Player, source, position, _velocity, type, damage, knockback)) {
							Projectile.NewProjectile(source, position, _velocity, type, damage, knockback, Player.whoAmI);
						}
						rot = -rot;
					}
				}
				if (madHand) {
					Fraction dmg = new(2, 3);
					int count = (Main.rand.NextBool(2) ? 1 : 0);
					dmg.D += count;
					damage *= dmg;
					double rot = Main.rand.NextBool(2) ? -0.1 : 0.1;
					for (int i = count; i-- > 0;) {
						Vector2 _velocity = velocity.RotatedBy(rot);
						if (ItemLoader.Shoot(item, Player, source, position, _velocity, type, damage, knockback)) {
							Projectile.NewProjectile(source, position, _velocity, type, damage, knockback, Player.whoAmI);
						}
						rot = -rot;
					}
				}
			}
			return true;
		}
		public override void MeleeEffects(Item item, Rectangle hitbox) {
			if (flaskBile) {
				Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.BloodWater, newColor: Color.Black);
			} else if (flaskSalt) {
				Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.GoldFlame, newColor: Color.Lime);
			}
			DoGunGlove();
		}
		public override void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight) {
			if (projectile.CountsAsClass(DamageClass.Melee) || ProjectileID.Sets.IsAWhip[projectile.type]) {//flasks
				if (flaskBile) {
					Dust.NewDust(boxPosition, boxWidth, boxHeight, DustID.BloodWater, newColor: Color.Black);
				} else if (flaskSalt) {
					Dust.NewDust(boxPosition, boxWidth, boxHeight, DustID.GoldFlame, newColor: Color.Lime);
				}
			}
		}
		public void DoGunGlove() {
			if (gunGlove && gunGloveCooldown <= 0) {
				if (Player.PickAmmo(gunGloveItem, out int projToShoot, out float speed, out int damage, out float knockback, out int usedAmmoItemId)) {
					int manaCost = Player.GetManaCost(gunGloveItem);
					if (CombinedHooks.CanShoot(Player, gunGloveItem) && Player.CheckMana(manaCost, true)) {
						if (manaCost > 0) {
							Player.manaRegenDelay = (int)Player.maxRegenDelay;
						}
						Vector2 position = Player.itemLocation;
						float rotation = Player.direction == 1 ? Player.itemRotation : Player.itemRotation + MathHelper.Pi;
						if (compositeFrontArmWasEnabled) {
							rotation = Player.compositeFrontArm.rotation + MathHelper.PiOver2;
							position = Player.GetCompositeArmPosition(false);
						}
						Vector2 velocity = Vec2FromPolar(rotation, speed);

						if (gunGloveItem.UseSound is not null) SoundEngine.PlaySound(gunGloveItem.UseSound, position);

						if (Main.myPlayer == Player.whoAmI) {
							CombinedHooks.ModifyShootStats(Player, gunGloveItem, ref position, ref velocity, ref projToShoot, ref damage, ref knockback);
							EntitySource_ItemUse_WithAmmo source = (EntitySource_ItemUse_WithAmmo)Player.GetSource_ItemUse_WithPotentialAmmo(gunGloveItem, usedAmmoItemId);
							if (CombinedHooks.Shoot(Player, gunGloveItem, source, position, velocity, projToShoot, damage, knockback)) {
								Projectile.NewProjectile(source, position, velocity, projToShoot, damage, knockback, Player.whoAmI);
								SoundEngine.PlaySound(gunGloveItem.UseSound, position);
							}
						}
					}
					int useTime = CombinedHooks.TotalUseTime(gunGloveItem.useTime, Player, gunGloveItem);
					gunGloveCooldown = useTime + Main.rand.Next(-useTime / 5, useTime / 5);
				}
			}
		}
		#region dealing
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (modifiers.DamageType.CountsAsClass(DamageClasses.Explosive)) {
				modifiers.DefenseEffectiveness *= explosive_defense_factor;
			}
			if (Origins.processingDash) {
				for (int i = 0; i < dashHitDebuffs.Count; i++) {
					(int type, Range duration) = dashHitDebuffs[i];
					target.AddBuff(type, Main.rand.Next(duration.Start.Value, duration.End.Value + 1));
				}
			}
			if (fiberglassHelmet == true) {
				modifiers.CritDamage.Base += 10;
			}
		}
		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)/* tModPorter If you don't need the Item, consider using ModifyHitNPC instead */ {
			//enemyDefense = NPC.GetDefense;
			if (felnumShock >= Felnum_Helmet.shock_damage_divisor * 2) {
				modifiers.SourceDamage.Flat += (int)((felnumShock * Origins.DamageBonusScale[item.type]) / Felnum_Helmet.shock_damage_divisor);
				usedFelnumShock = true;
				SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), target.Center);
			}
		}
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			if (proj.IsMinionOrSentryRelated) {
				if (rivenSet) modifiers.SourceDamage *= rivenMult;
				broth?.ModifyMinionHit(proj, target, ref modifiers);
			}
		}
		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			if (entangledEnergy && Player.potionDelay > 0) {
				Projectile.NewProjectile(
					Player.GetSource_OnHit(target),
					target.Center,
					default,
					ModContent.ProjectileType<Entangled_Energy_Lifesteal>(),
					damageDone,
					0,
					Player.whoAmI,
					ai1: 1
				);
			}
			if (item.CountsAsClass(DamageClass.Melee)) {//flasks
				if (flaskBile) {
					target.AddBuff(Rasterized_Debuff.ID, Rasterized_Debuff.duration);
				}
				if (flaskSalt) {
					OriginGlobalNPC.InflictTorn(target, 300, 180, Salt_Flask.TornSeverity, this);
				}
			}
			if (futurephones) {
				target.AddBuff(Futurephones_Buff.ID, 300);
				Player.MinionAttackTargetNPC = target.whoAmI;
			}
			if (LocalOriginPlayer.priorityMail && Player.whoAmI == Main.myPlayer) {
				OriginGlobalNPC originGlobalNPC = target.GetGlobalNPC<OriginGlobalNPC>();
				originGlobalNPC.priorityMailTime = originGlobalNPC.prevPriorityMailTime;
			}
			if (faithBeads) {
				target.AddBuff(Cavitation_Debuff.ID, 90);
				target.AddBuff(BuffID.Wet, 180);
				if (target.wet && Main.myPlayer == Player.whoAmI && faithBeadsItem is not null) {
					Projectile.NewProjectile(
						Player.GetSource_Accessory(faithBeadsItem),
						target.Center,
						default,
						faithBeadsItem.shoot,
						Player.GetWeaponDamage(faithBeadsItem),
						Player.GetWeaponKnockback(faithBeadsItem)
					);
				}
			}
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			if (proj.IsMinionOrSentryRelated) {
				broth?.OnMinionHit(proj, target, hit, damageDone);
				if (mildewSet) {
					target.AddBuff(Toxic_Shock_Debuff.ID, 90);
					if (mildewSet && venomFang) {
						target.AddBuff(Toxic_Shock_Strengthen_Debuff.ID, 2);
					}
				}
			}
			if (proj.CountsAsClass(DamageClass.Melee) || ProjectileID.Sets.IsAWhip[proj.type]) {//flasks
				if (flaskBile) {
					target.AddBuff(Rasterized_Debuff.ID, Rasterized_Debuff.duration);
				}
				if (flaskSalt) {
					OriginGlobalNPC.InflictTorn(target, 300, 180, Salt_Flask.TornSeverity, this);
				}
			}
			if (futurephones && !proj.IsMinionOrSentryRelated) {
				target.AddBuff(Futurephones_Buff.ID, 300);
				Player.MinionAttackTargetNPC = target.whoAmI;
			}
			if (LocalOriginPlayer.priorityMail && Player.whoAmI == Main.myPlayer && !proj.IsMinionOrSentryRelated) {
				OriginGlobalNPC originGlobalNPC = target.GetGlobalNPC<OriginGlobalNPC>();
				originGlobalNPC.priorityMailTime = originGlobalNPC.prevPriorityMailTime;
			}
			if (faithBeads) {
				target.AddBuff(Cavitation_Debuff.ID, 90);
				target.AddBuff(BuffID.Wet, 180);
				if (Main.rand.NextBool(faithBeadsItem.useAnimation, faithBeadsItem.reuseDelay)) {
					if (target.wet && Main.myPlayer == Player.whoAmI && faithBeadsItem is not null && proj.type != faithBeadsItem.shoot) {
						Projectile.NewProjectile(
							Player.GetSource_Accessory(faithBeadsItem),
							target.Center,
							default,
							faithBeadsItem.shoot,
							Player.GetWeaponDamage(faithBeadsItem),
							Player.GetWeaponKnockback(faithBeadsItem)
						);
					}
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (hit.Crit && !OriginsSets.NPCs.TargetDummies[target.type]) {
				if (dimStarlight && dimStarlightCooldown < 1) {
					int item = Item.NewItem(Player.GetSource_OnHit(target, "Accessory"), target.position, target.width, target.height, ItemID.Star);
					dimStarlightCooldown = 300;
					if (Main.netMode == NetmodeID.MultiplayerClient) {
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
					}
				}
			}
			if (symbioteSkull) {
				OriginGlobalNPC.InflictTorn(target, Main.rand.Next(50, 110), 60, Symbiote_Skull.TornSeverity, this);
			}
			if (acridSet) {
				target.AddBuff(Toxic_Shock_Debuff.ID, 300);
			}
			if (venomFang) {
				target.AddBuff(Toxic_Shock_Debuff.ID, 180);
				if (venomFang && acridSet) {
					target.AddBuff(Toxic_Shock_Strengthen_Debuff.ID, 2);
				}
			}
			if (mithrafin) {
				target.AddBuff(Mithrafin_Poison_Extend_Debuff.ID, 2);
			}
			if (lightningRing) {
				target.AddBuff(ModContent.BuffType<Mini_Static_Shock_Debuff>(), 180);
			}
			if (messyLeech) {
				target.AddBuff(BuffID.Bleeding, 480);
			}
			if (magmaLeech) {
				target.AddBuff(BuffID.Bleeding, 480);
				target.AddBuff(BuffID.OnFire3, Main.rand.Next(120, 361));
			}
			if (hit.DamageType.CountsAsClass<Explosive>()) {
				if (dangerBarrel) {
					target.AddBuff(BuffID.OnFire, Main.rand.Next(120, 361));
				}
				if (scavengerSet) {
					OriginGlobalNPC.InflictImpedingShrapnel(target, 300);
				}
			}
			if (faithBeads) {
				target.AddBuff(Cavitation_Debuff.ID, 90);
				target.AddBuff(BuffID.Wet, 180);
			}
			if (target.life <= 0) {
				if (target.ModNPC is Cannihound) {
					Player.AddBuff(ModContent.BuffType<Cannihound_Lure_Debuff>(), 5 * 60);
				}
				foreach (Quest quest in Quest_Registry.Quests) {
					if (quest.KillEnemyEvent is not null) {
						quest.KillEnemyEvent(target);
					}
				}

				if (!string.IsNullOrWhiteSpace(OriginsSets.NPCs.JournalEntries[target.type])) UnlockJournalEntry(OriginsSets.NPCs.JournalEntries[target.type]);
				if (necroSet) {
					necroSetAmount += target.lifeMax;
				}
				if (necroSet2) {
					int type = ModContent.ProjectileType<Unsatisfied_Soul>();
					int damage = (int)Player.GetTotalDamage(DamageClass.Summon).ApplyTo(target.lifeMax / 4) + 40;
					int knockback = (int)Player.GetTotalKnockback(DamageClass.Summon).ApplyTo(4);
					for (int i = Main.rand.RandomRound(Player.maxMinions * 0.5f); i > 0; i--) {
						Projectile.NewProjectile(
							target.GetSource_Death(),
							target.Center,
							Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(8, 12),
							type,
							damage,
							knockback
						);
					}
				}
			}
			if (hasPotatOS && Main.rand.NextBool(10)) {
				Potato_Battery.PlayRandomMessage(
					Potato_Battery.QuoteType.Combat,
					potatOSQuoteCooldown,
					Player.Top
				);
			}
			if (hasProtOS) {
				if (target.life <= 0 && target.townNPC) {
					Protomind.PlayRandomMessage(
						Protomind.QuoteType.Kill_Villager,
						protOSQuoteCooldown,
						Player.Top
					);
				} else if (Main.rand.NextBool(10)) {
					Protomind.PlayRandomMessage(
						Protomind.QuoteType.Combat,
						protOSQuoteCooldown,
						Player.Top
					);
				}
			}
			if (talkingPet != -1) {
				Projectile pet = Main.projectile[talkingPet];
				if (pet.type == Chew_Toy.projectileID) {
					if (Main.rand.NextBool(10)) {
						Chee_Toy_Messages.Instance.PlayRandomMessage(
							Chee_Toy_Message_Types.Combat,
							pet.Top,
							new Vector2(Math.Sign(target.Center.X - Player.Center.X) * 7f, -2f + Main.rand.NextFloat() * -2f)
						);
					}
				}
			}
			if (cryostenSet) {
				target.AddBuff(BuffID.Frostburn, Main.rand.Next(300, 601));
			}
			if (retaliatoryTendrilCharge > 0) {
				retaliatoryTendrilCharge -= hit.Damage;
				if (retaliatoryTendrilCharge <= 0) {
					retaliatoryTendrilCharge = 0;
					retaliatoryTendrilStrength = 0;
				}
			}
			OriginGlobalNPC originGlobalNPC = target.GetGlobalNPC<OriginGlobalNPC>();
			if (originGlobalNPC.amnesticRose && !OriginsSets.NPCs.TargetDummies[target.type]) {
				float mana = 1 + MathF.Pow(damageDone * 0.1f, 0.5f);
				if (target.ModNPC is IDefiledEnemy defiledEnemy) {
					mana = Math.Min(mana, defiledEnemy.Mana);
				}
				if (mana > 0) {
					Projectile.NewProjectile(
						Player.GetSource_OnHit(target),
						target.Center,
						Vector2.Zero,
						ModContent.ProjectileType<Defiled_Prefix_Orb>(),
						0,
						0,
						ai0: mana
					);
				}
				if (hit.Crit || target.life <= 0) {
					for (int i = int.Min(Main.rand.Next(3), Main.rand.Next(3)); i >= 0; i--) {
						Projectile.NewProjectile(
							Player.GetSource_OnHit(target),
							Main.rand.NextVector2FromRectangle(target.Hitbox),
							target.velocity + Main.rand.NextVector2Circular(2, 2),
							originGlobalNPC.amnesticRoseGooProj,
							0,
							0,
							ai0: mana
						);
					}
				}
			}

			if (bombRack is not null) {
				OriginGlobalNPC.InflictImpedingShrapnel(target, Bomb_Rack.DebuffTime);
			}

			if (dangerTime < maxDangerTime) dangerTime = maxDangerTime;
		}
		#endregion
		#region receiving
		public override void UpdateBadLifeRegen() {
			if (scrapBarrierDebuff) {
				if (Player.lifeRegen > 0) Player.lifeRegen = 0;
				if (Player.lifeRegenCount > 0) Player.lifeRegenCount = 0;
			}
			if (plasmaPhial && Player.bleed) {
				Player.lifeRegen -= 12;
			}
			if (cursedCrown && Player.onFire) {
				Player.lifeRegen -= 8;
			}
			if (cavitationDebuff) {
				Player.lifeRegen -= 33;
			}
			if (tetanus) {
				Player.lifeRegen -= Tetanus_Debuff.DPS;
			}
			if (miasma) {
				if (Player.breath <= 0) {
					Player.lifeRegenTime = 0f;
					Player.breath = 0;
					if (Main.rand.NextBool(10)) Player.statLife -= 1;
					if (Player.statLife <= 0) {
						Player.statLife = 0;
						Player.KillMe(PlayerDeathReason.ByCustomReason(TextUtils.LanguageTree.Find("Mods.Origins.DeathMessage.Miasma").SelectFrom(Player.name).ToNetworkText()), 10.0, 0);
					}
				} else if (Player.breathCD.Warmup(Player.breathCDMax + Player.breathCDMax / 2)) {
					Player.breathCD = 0;
					Player.breath--;
				}
			}

			if (staticShock || miniStaticShock || staticShockDamage) {
				if (Player.lifeRegen > 0) {
					Player.lifeRegen = 0;
				}
				int damageMult = 1 + Player.wet.ToInt() + ((staticShock || miniStaticShock) && staticShockDamage).ToInt();
				Player.lifeRegen -= 9 * damageMult;
			}

			if (oiled && (Player.onFire || Player.onFire2 || Player.onFire3 || Player.onFrostBurn || Player.onFrostBurn2)) {
				Player.lifeRegen -= 50;
			}

			if (Player.lifeRegen < 0 && dangerTime < maxDangerTime) dangerTime = maxDangerTime;
		}
		public override bool CanBeHitByProjectile(Projectile proj) {
			if (shimmerShieldDashTime != 0 && OriginsSets.Projectiles.CanBeDeflected[proj.type]) {
				Vector2 direction = Vector2.UnitX * Math.Sign(shimmerShieldDashTime);
				if (proj.TryGetGlobalProjectile(out HostileGlobalProjectile hostileProjectile) && hostileProjectile.TryGetOwnerPosition(out Vector2? ownerPosition) == HostileGlobalProjectile.OwnerState.Alive) {
					direction = proj.DirectionTo(ownerPosition ?? direction);
				} else if (proj.friendly && proj.TryGetOwner(out Player attacker)) {
					direction = proj.DirectionTo(attacker.MountedCenter);
				}
				proj.reflected = true;
				float damageMult = Shimmer_Shield.ReflectionDamageMultiplier;
				if (proj.hostile) damageMult *= Shimmer_Shield.ReflectionHostileDamageMultiplier;
				proj.damage = Main.rand.RandomRound(proj.damage * damageMult);
				proj.hostile = false;
				proj.friendly = true;
				float speed = Math.Max(12f / proj.MaxUpdates, proj.velocity.Length());
				proj.velocity = direction * speed;
				proj.owner = Player.whoAmI;
				proj.netUpdate = true;
				return false;
			}
			return true;
		}
		public StatModifier GetSelfDamageModifier() {
			StatModifier currentExplosiveSelfDamage = explosiveSelfDamage;
			if (minerSet) {
				currentExplosiveSelfDamage -= 0.2f;
				currentExplosiveSelfDamage = currentExplosiveSelfDamage.CombineWith(
					Player.GetDamage(DamageClasses.Explosive).GetInverse()
				);
			}
			return currentExplosiveSelfDamage;
		}
		public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers) {
			if (trapCharm && proj.trap) {
				modifiers.SourceDamage /= 2;
				Player.buffImmune[BuffID.Poisoned] = true;
			}
			if (rubberBody) modifiers.SourceDamage *= 0.9f;
			if (proj.GetGlobalProjectile<OriginGlobalProj>().weakpointAnalyzerFake) modifiers.Cancel();
			hitIsSelfDamage = false;
			if (proj.owner == Player.whoAmI && !proj.hostile && proj.CountsAsClass(DamageClasses.Explosive)) {
				hitIsSelfDamage = true;
				float damageMult = Main.GameModeInfo.EnemyDamageMultiplier;
				if (Main.GameModeInfo.IsJourneyMode) {
					Terraria.GameContent.Creative.CreativePowers.DifficultySliderPower power = Terraria.GameContent.Creative.CreativePowerManager.Instance.GetPower<Terraria.GameContent.Creative.CreativePowers.DifficultySliderPower>();
					if (power.GetIsUnlocked()) {
						damageMult = power.StrengthMultiplierToGiveNPCs;
					}
				}
				modifiers.SourceDamage /= damageMult;
				StatModifier currentExplosiveSelfDamage = GetSelfDamageModifier();

				if (proj.TryGetGlobalProjectile(out ExplosiveGlobalProjectile global)) currentExplosiveSelfDamage = currentExplosiveSelfDamage.CombineWith(global.selfDamageModifier);
				const float min_self_destruct_mult = 0.1f;
				if (proj.type == ModContent.ProjectileType<Self_Destruct_Explosion>()) {
					modifiers.ScalingArmorPenetration += 1;
					if (currentExplosiveSelfDamage.ApplyTo(proj.damage) < proj.damage * min_self_destruct_mult) currentExplosiveSelfDamage = new StatModifier(1, min_self_destruct_mult);
				}
				if (currentExplosiveSelfDamage.ApplyTo(proj.damage) <= 0) modifiers.Cancel();

				modifiers.FinalDamage = modifiers.FinalDamage.CombineWith(currentExplosiveSelfDamage);
				if (Player.mount.Active && Player.mount.Type == ModContent.MountType<Trash_Lid_Mount>()) {
					Vector2 diff = proj.Center - Player.MountedCenter;
					if (diff.Y > 24 && diff.Y > Math.Abs(diff.X)) {
						modifiers.SourceDamage *= 0;
						modifiers.SourceDamage.Flat = 0;
						modifiers.FinalDamage *= 0;
						modifiers.FinalDamage.Flat = 1;
						//modifiers.Knockback *= 0;
						modifiers.HitDirectionOverride = 0;
						modifiers.DisableSound();
					}
				}
			}
			if (shineSparkDashTime > 0) {
				modifiers.FinalDamage *= 0;
				modifiers.FinalDamage.Flat = -ushort.MaxValue;
				proj.Kill();
			}
		}
		public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo) {
			if (proj.owner == Player.whoAmI && proj.friendly && proj.CountsAsClass(DamageClasses.Explosive)) {
				if (Player.mount.Active && Player.mount.Type == ModContent.MountType<Trash_Lid_Mount>()) {
					Vector2 diff = proj.Center - Player.MountedCenter;
					if (diff.Y > 24 && diff.Y > Math.Abs(diff.X)) {
						Player.velocity -= diff.SafeNormalize(default) * hurtInfo.Knockback * 750 / (diff.Length() + 128);
						SoundEngine.PlaySound(SoundID.Item148.WithPitchRange(-0.7f, -0.6f), Player.MountedCenter + new Vector2(0, 24));
					}
				}
			}
			OnHitByAnyProjectile(proj);
		}
		public void OnHitByAnyProjectile(Projectile proj) {
			if (proj.type == ProjectileID.UnholyWater) {
				InflictAssimilation<Corrupt_Assimilation>(0.075f);
				proj.Kill();
			} else if (proj.type == ProjectileID.BloodWater) {
				InflictAssimilation<Crimson_Assimilation>(0.075f);
				proj.Kill();
			} else if (proj.type == ModContent.ProjectileType<White_Water_P>()) {
				InflictAssimilation<Defiled_Assimilation>(0.075f);
				proj.Kill();
			} else if (proj.type == ModContent.ProjectileType<Gooey_Water_P>()) {
				InflictAssimilation<Riven_Assimilation>(0.075f);
				proj.Kill();
			}
		}
		public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers) {
			if (shineSparkDashTime > 0) {
				modifiers.FinalDamage *= 0;
				modifiers.FinalDamage.Flat = int.MinValue;
				npc.SimpleStrikeNPC(Player.GetWeaponDamage(loversLeapItem) * 15, Player.direction, true, 12, DamageClass.Generic);
			}
		}
		public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) {
			if (shimmerShieldDashTime != 0 && NPCID.Sets.ProjectileNPC[npc.type]) {
				if (npc.immune[Player.whoAmI] == 0) {
					npc.SimpleStrikeNPC(1, Player.direction);
					npc.immune[Player.whoAmI] = 6;
				}
				return false;
			}
			if (emergencyBeeCanister && (npc.type == NPCID.Bee || npc.type == NPCID.BeeSmall)) return npc.playerInteraction[Player.whoAmI];
			return true;
		}
		public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
			if (!Player.noKnockback && hurtInfo.Damage != 0) {
				Player.velocity.X *= MeleeCollisionNPCData.knockbackMult;
			}
			MeleeCollisionNPCData.knockbackMult = 1f;
		}
		public void PostHitByNPC() {
			if (preHitBuffs is not null && lastHitEnemy >= 0) {
				NPC npc = Main.npc[lastHitEnemy];
				for (int i = 0; i < Player.MaxBuffs; i++) {
					int buffType = Player.buffType[i];
					if (Main.debuff[buffType] && !BuffID.Sets.NurseCannotRemoveDebuff[buffType] && !preHitBuffs.Contains(new Point(buffType, Player.buffTime[i]))) {
						if (noU) {
							bool immune = npc.buffImmune[buffType];
							npc.buffImmune[buffType] = false;
							npc.AddBuff(buffType, Player.buffTime[i]);
							npc.buffImmune[buffType] = immune;

							Player.DelBuff(i--);
						}
					}
				}
			}
		}
		public override bool FreeDodge(Player.HurtInfo info) {
			if (allManaDamage) {
				Player.CheckMana(manaDamageToTake, true);
				Player.AddBuff(ModContent.BuffType<Mana_Buffer_Debuff>(), 192);
				PlayerLoader.PostHurt(Player, info);
				return true;
			}
			if (Player.whoAmI == Main.myPlayer && !(protomindItem?.IsAir ?? true)) {
				for (int i = 0; i < 200; i++) {
					if (!Main.npc[i].active || Main.npc[i].friendly) {
						continue;
					}
					int num2 = 300 + info.Damage * 2;
					if (Main.rand.Next(500) < num2) {
						float dist = (Main.npc[i].Center - Player.Center).Length();
						float chance = Main.rand.Next(200 + info.Damage / 2, 301 + info.Damage * 2);
						if (chance > 500f) {
							chance = 500f + (chance - 500f) * 0.75f;
						}
						if (chance > 700f) {
							chance = 700f + (chance - 700f) * 0.5f;
						}
						if (chance > 900f) {
							chance = 900f + (chance - 900f) * 0.25f;
						}
						if (dist < chance) {
							float num4 = Main.rand.Next(90 + info.Damage / 3, 300 + info.Damage / 2);
							Main.npc[i].AddBuff(BuffID.Confused, (int)num4);
						}
					}
				}
				Projectile.NewProjectile(
					Player.GetSource_Accessory(protomindItem),
					Player.Center + new Vector2(Main.rand.Next(-40, 40), Main.rand.Next(-60, -20)),
					Player.velocity * 0.3f,
					protomindItem.shoot,
					0,
					0f,
					Player.whoAmI
				);
				if (Main.rand.NextBool(6) && Player.FindBuffIndex(BuffID.BrainOfConfusionBuff) == -1) {
					Player.BrainOfConfusionDodge();
					return true;
				}
			}
			return false;
		}
		public override bool ConsumableDodge(Player.HurtInfo info) {
			if (info.DamageSource.SourcePlayerIndex == Player.whoAmI && info.DamageSource.SourceProjectileLocalIndex != -1) {
				Projectile sourceProjectile = Main.projectile[info.DamageSource.SourceProjectileLocalIndex];
				if (sourceProjectile.owner == Player.whoAmI && sourceProjectile.CountsAsClass(DamageClasses.Explosive)) {
					if (resinShield) {
						resinShieldCooldown = (int)explosiveFuseTime.Scale(5).ApplyTo(198);
						if (Player.shield == Resin_Shield.ShieldID) {
							for (int i = Main.rand.Next(4, 8); i-- > 0;) {
								Dust.NewDust(Player.MountedCenter + new Vector2(12 * Player.direction - 6, -12), 8, 32, DustID.GemAmber, Player.direction * 2, Alpha: 100);
							}
						}
						return true;
					}
				}
			}
			if (extremophileSet && Main.rand.Next(99) < extremophileSetHits * 10) {
				extremophileSetHits = 0;
				Player.SetImmuneTimeForAllTypes(60);
				return true;
			}
			if (blizzardwalkerActiveTime >= Blizzardwalkers_Jacket.max_active_time) {
				blizzardwalkerActiveTime = 0;
				Player.SetImmuneTimeForAllTypes(60);
				for (int i = 0; i < 30; i++) Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Asphalt, 0f, -3f, 0, default, 1.4f).noGravity = true;
				SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack);
				return true;
			}
			return false;
		}
		bool allManaDamage = false;
		int manaDamageToTake = 0;
		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (fullSend && modifiers.DamageSource.SourceOtherIndex == OtherDeathReasonID.Fall) {
				modifiers.FinalDamage *= 0.14f - fullSendHorseshoeBonus.Mul(0.07f);
			}
			if (Player.HasBuff(Toxic_Shock_Debuff.ID) && Main.rand.Next(Player.HasBuff(Toxic_Shock_Strengthen_Debuff.ID) ? 6 : 12) < 3) {
				modifiers.FinalDamage *= 1.5f;
			}
			allManaDamage = false;
			manaDamageToTake = 0;
			if (manaShielding > 0) {
				if (manaShielding > 1) manaShielding = 1;
				modifiers.ModifyHurtInfo += (ref Player.HurtInfo info) => {
					float manaDamage = info.Damage;
					float costMult = 3;
					float costMult3 = (float)Math.Pow(manaShielding, Player.manaCost);
					if (Player.magicCuffs) {
						costMult = 1;
						Player.magicCuffs = false;
					}
					if (Player.statMana < manaDamage * costMult * manaShielding) {
						manaDamage = Player.statMana / (costMult * manaShielding);
					}
					if (manaDamage * costMult * manaShielding >= 1f) {
						Player.ManaEffect((int)-(manaDamage * costMult * manaShielding));
					}
					int damage = (int)(info.Damage - (manaDamage * costMult3));
					if (damage <= 0) allManaDamage = true;
					info.Damage = damage;
					info.Dodgeable = true;
					manaDamageToTake = (int)Math.Floor(manaDamage * costMult * manaShielding);
				};
			}
			if (refactoringPieces) {
				modifiers.SourceDamage *= 0.9f;
			}
			if (toxicShock) {
				Player.lifeRegen -= 15;
				modifiers.ScalingArmorPenetration += 0.1f;
			}
			if (ashenKBReduction) {
				modifiers.Knockback -= 0.15f;
			}
			if (slagBucketCursed && Player.statLife > Player.statLifeMax2 * 0.8f) {
				modifiers.FinalDamage *= 0.6f;
			}
			if (scrapBarrierCursed && Player.statLife > Player.statLifeMax2 * 0.9f) {
				modifiers.FinalDamage *= 0.5f;
			}
		}
		public override void OnHurt(Player.HurtInfo info) {
			if (crystalHeart) crystalHeartCounter += info.Damage;
		}
		public override void PostHurt(Player.HurtInfo info) {
			lifeRegenTimeSinceHit = 0;
			timeSinceHit = 0;
			pacemakerTime = 0;
			if (extremophileSet) extremophileSetHits++;
			extremophileSetTime = 0;
			if (manaDamageToTake > 0) {
				Player.CheckMana(manaDamageToTake, true);
				Player.AddBuff(ModContent.BuffType<Mana_Buffer_Debuff>(), 50);
			}
			bool isSelfDamage = false;
			if (info.DamageSource.SourcePlayerIndex == Player.whoAmI) {
				isSelfDamage = true;
				selfDamageRally = info.Damage;
			}
			if (retaliatoryTendril && !isSelfDamage && info.Damage > retaliatoryTendrilItem.useAnimation) {
				float strength = info.Damage * retaliatoryTendrilItem.knockBack;
				if (strength > retaliatoryTendrilStrength) {
					retaliatoryTendrilCharge = retaliatoryTendrilItem.useTime;
					retaliatoryTendrilStrength = strength;
				} else {
					retaliatoryTendrilStrength += strength;
				}
				if (retaliatoryTendrilStrength > retaliatoryTendrilItem.reuseDelay * 0.01f) retaliatoryTendrilStrength = retaliatoryTendrilItem.reuseDelay * 0.01f;
			}
			if (info.PvP && info.CooldownCounter == ImmunityCooldownID.WrongBugNet) {
				Player.hurtCooldowns[ImmunityCooldownID.WrongBugNet] = Player.longInvince ? 10 : 6;
			}
			if (guardedHeart) {
				Player.AddBuff(Guarded_Heart_Buff.ID, 60 * 8);
			}
			if (razorwire) {
				const float maxDist = 240 * 240;
				double totalDamage = info.Damage;
				List<(int id, float weight)> targets = new();
				NPC npc;
				for (int i = 0; i < Main.maxNPCs; i++) {
					npc = Main.npc[i];
					if (npc.CanBeChasedBy(razorwireItem)) {
						Vector2 currentPos = npc.Hitbox.ClosestPointInRect(Player.MountedCenter);
						Vector2 diff = currentPos - Player.MountedCenter;
						float dist = diff.LengthSquared();
						if (dist > maxDist) continue;
						float currentWeight = (1.5f - Vector2.Dot(npc.velocity, diff.SafeNormalize(default))) * (dist / maxDist);
						if (totalDamage / 3 > npc.life) {
							currentWeight = 0;
						}
						if (targets.Count >= 3) {
							for (int j = 0; j < 3; j++) {
								if (targets[j].weight < currentWeight) {
									targets.Insert(j, (i, currentWeight));
									break;
								}
							}
						} else {
							targets.Add((i, currentWeight));
						}
					}
				}
				for (int i = 0; i < 3; i++) {
					if (i >= targets.Count) break;
					Vector2 currentPos = Main.npc[targets[i].id].Hitbox.ClosestPointInRect(Player.MountedCenter);
					Projectile.NewProjectile(
						Player.GetSource_Accessory(razorwireItem),
						Player.MountedCenter,
						(currentPos - Player.MountedCenter).WithMaxLength(12),
						razorwireItem.shoot,
						(int)(totalDamage / Math.Min(targets.Count, 3)) + 1,
						10,
						Player.whoAmI
					);
				}
			}
			if (retributionShield) {
				const float maxDist = 240 * 240;
				double totalDamage = info.Damage * (1 - retributionShieldItem.knockBack);
				List<(int id, float weight)> targets = new();
				NPC npc;
				for (int i = 0; i < Main.maxNPCs; i++) {
					npc = Main.npc[i];
					if (npc.CanBeChasedBy(retributionShieldItem)) {
						Vector2 currentPos = npc.Hitbox.ClosestPointInRect(Player.MountedCenter);
						Vector2 diff = currentPos - Player.MountedCenter;
						float dist = diff.LengthSquared();
						if (dist > maxDist) continue;
						float currentWeight = (1.5f - Vector2.Dot(npc.velocity, diff.SafeNormalize(default))) * (dist / maxDist);
						if (totalDamage / 3 > npc.life) {
							currentWeight = 0;
						}
						if (targets.Count >= 3) {
							for (int j = 0; j < 3; j++) {
								if (targets[j].weight < currentWeight) {
									targets.Insert(j, (i, currentWeight));
									break;
								}
							}
						} else {
							targets.Add((i, currentWeight));
						}
					}
				}
				for (int i = 0; i < 3; i++) {
					if (i >= targets.Count) break;
					Vector2 currentPos = Main.npc[targets[i].id].Hitbox.ClosestPointInRect(Player.MountedCenter);
					Projectile.NewProjectile(
						Player.GetSource_Accessory(retributionShieldItem),
						Player.MountedCenter,
						(currentPos - Player.MountedCenter).WithMaxLength(12),
						retributionShieldItem.shoot,
						(int)(totalDamage / Math.Min(targets.Count, 3)) + 1,
						10,
						Player.whoAmI
					);
				}
			}
			if (barkShieldItem?.ModItem is Bark_Shield barkShield) {
				barkShield.durability -= barkShield.CalculateDamage(info);
				Min(ref barkShield.durability, barkShield.MaxDurability);
				if (barkShield.durability <= 0) {
					barkShield.Break(Player, info);
					barkShieldItem.TurnToAir(true);
				}
			}
			if (!(flakJacketItem?.IsAir ?? true)) {
				int damage = Player.GetWeaponDamage(flakJacketItem);
				float knockback = Player.GetWeaponKnockback(flakJacketItem);
				Player.SpawnProjectile(
					Player.GetSource_Accessory_OnHurt(flakJacketItem, info.DamageSource),
					Player.MountedCenter,
					Vector2.Zero,
					flakJacketItem.shoot,
					damage,
					knockback
				);
			}
			if (fullSend && info.DamageSource.SourceOtherIndex == OtherDeathReasonID.Fall) {
				const float maxDist = 240 * 240;
				double totalDamage = info.SourceDamage * (0.86 + fullSendHorseshoeBonus.Mul(0.07f));
				Vector2 center = Player.MountedCenter;
				List<(int id, float weight)> targets = new();
				NPC npc;
				for (int i = 0; i < Main.maxNPCs; i++) {
					npc = Main.npc[i];
					if (npc.type == NPCID.TargetDummy || npc.CanBeChasedBy(info.DamageSource)) {
						Vector2 currentPos = npc.Hitbox.ClosestPointInRect(center);
						Vector2 diff = currentPos - center;
						float dist = diff.LengthSquared();
						if (dist > maxDist) continue;
						float currentWeight = (1.5f - Vector2.Dot(npc.velocity, diff.SafeNormalize(default))) * (dist / maxDist);
						if (totalDamage / 3 > npc.life) {
							currentWeight = 0;
						}
						if (npc.type == NPCID.TargetDummy) currentWeight -= 2;
						if (targets.Count >= 1) {
							for (int j = 0; j < 3 && j < targets.Count; j++) {
								if (targets[j].weight < currentWeight) {
									targets.Insert(j, (i, currentWeight));
									break;
								}
							}
						} else {
							targets.Add((i, currentWeight));
						}
					}
				}
				NPC.HitInfo hit = new() {
					Damage = Main.rand.RandomRound(totalDamage / 3),
					DamageType = DamageClass.Summon,
					Knockback = 4
				};
				for (int i = 0; i < 3; i++) {
					if (i >= targets.Count) break;
					npc = Main.npc[targets[i].id];
					hit.HitDirection = Math.Sign(npc.Center.X - center.X);
					npc.StrikeNPC(hit);
					if (Main.netMode != NetmodeID.SinglePlayer) NetMessage.SendStrikeNPC(npc, hit);
				}
			}
			if (cinderSealItem is not null) {
				cinderSealItem.ModItem.Shoot(
					Player,
					Player.GetSource_ItemUse_WithPotentialAmmo(cinderSealItem, ItemID.None) as EntitySource_ItemUse_WithAmmo,
					Player.Center,
					Vector2.Zero,
					cinderSealItem.shoot,
					Player.GetWeaponDamage(cinderSealItem),
					Player.GetWeaponKnockback(cinderSealItem)
				);
				if (Main.rand.NextBool(5)) {
					Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Ash);
					dust.noGravity = true;
					dust.velocity *= 0.1f;
				}
			}
			if (unsoughtOrgan) {
				const float maxDist = 240 * 240;
				double totalDamage = info.Damage;
				List<(int id, float weight)> targets = new();
				NPC npc;
				for (int i = 0; i < Main.maxNPCs; i++) {
					npc = Main.npc[i];
					if (npc.active && npc.damage > 0 && !npc.friendly) {
						Vector2 currentPos = npc.Hitbox.ClosestPointInRect(Player.MountedCenter);
						Vector2 diff = currentPos - Player.MountedCenter;
						float dist = diff.LengthSquared();
						if (dist > maxDist) continue;
						float currentWeight = (1.5f - Vector2.Dot(npc.velocity, diff.SafeNormalize(default))) * (dist / maxDist);
						if (totalDamage / 3 > npc.life) {
							currentWeight = 0;
						}
						if (targets.Count >= 3) {
							for (int j = 0; j < 3; j++) {
								if (targets[j].weight < currentWeight) {
									targets.Insert(j, (i, currentWeight));
									break;
								}
							}
						} else {
							targets.Add((i, currentWeight));
						}
					}
				}
				for (int i = 0; i < 3; i++) {
					if (i >= targets.Count) break;
					Vector2 currentPos = Main.npc[targets[i].id].Hitbox.ClosestPointInRect(Player.MountedCenter);
					Projectile.NewProjectile(
						Player.GetSource_Accessory(unsoughtOrganItem),
						Player.MountedCenter,
						(currentPos - Player.MountedCenter).WithMaxLength(12),
						unsoughtOrganItem.shoot,
						(int)(totalDamage / targets.Count) + 1,
						10,
						Player.whoAmI
					);
				}
			}
			if (bombCharminIt && Player.whoAmI == info.DamageSource.SourcePlayerIndex) {
				bombCharminItLifeRegenCount += info.Damage;
			}
			if (Player.whoAmI == info.DamageSource.SourcePlayerIndex && OriginsModIntegrations.CheckAprilFools()) {
				const int damage_required = 400;
				aprilFoolsRubberDynamiteTracker += info.Damage;
				while (aprilFoolsRubberDynamiteTracker >= damage_required) {
					aprilFoolsRubberDynamiteTracker -= damage_required;
					Player.QuickSpawnItem(Player.GetSource_OnHurt(info.DamageSource), ModContent.ItemType<Rubber_Dynamite>());
				}
			}
			if (coreGenerator && !isSelfDamage) {
				int ammoType = coreGeneratorItem.useAmmo;
				try {
					coreGeneratorItem.useAmmo = ModContent.ItemType<Resizable_Mine_Wood>();
					if (Player.PickAmmo(coreGeneratorItem, out int projToShoot, out float speed, out int damage, out float knockback, out int usedAmmoItemId)) {
						int manaCost = Player.GetManaCost(coreGeneratorItem);
						if (CombinedHooks.CanShoot(Player, coreGeneratorItem) && Player.CheckMana(manaCost, true)) {
							if (manaCost > 0) {
								Player.manaRegenDelay = (int)Player.maxRegenDelay;
							}
							Vector2 position = Player.MountedCenter;
							Vector2 velocity = new(coreGeneratorItem.shootSpeed, 0);

							CombinedHooks.ModifyShootStats(Player, coreGeneratorItem, ref position, ref velocity, ref projToShoot, ref damage, ref knockback);
							EntitySource_ItemUse_WithAmmo source = (EntitySource_ItemUse_WithAmmo)Player.GetSource_ItemUse_WithPotentialAmmo(coreGeneratorItem, usedAmmoItemId);
							if (CombinedHooks.Shoot(Player, coreGeneratorItem, source, position, velocity, projToShoot, damage, knockback)) {
								Projectile.NewProjectile(source, position, velocity, projToShoot, damage, knockback, Player.whoAmI);
								SoundEngine.PlaySound(coreGeneratorItem.UseSound, position);
							}
						}
						gunGloveCooldown = CombinedHooks.TotalUseTime(coreGeneratorItem.useTime, Player, coreGeneratorItem);
					}
				} finally {
					coreGeneratorItem.useAmmo = ammoType;
				}
			}
			if (decorativeAshes is not null && !isSelfDamage) {
				for (int i = Decorative_Ashes.ThornsCount; i > 0; i--) {
					Player.SpawnProjectile(
						Player.GetSource_Accessory(decorativeAshes),
						Decorative_Ashes.GetRocketShootPosition(Player),
						Decorative_Ashes.GetRocketShootVelocity(Player),
						decorativeAshes.shoot,
						Player.GetWeaponDamage(decorativeAshes),
						Player.GetWeaponKnockback(decorativeAshes),
						-1f
					);
				}
			}
			if (info.DamageSource.SourceNPCIndex > -1 && Main.npc[info.DamageSource.SourceNPCIndex].type == ModContent.NPCType<Brine_Latcher>()) {
				if (info.CooldownCounter == -1) {
					Player.immuneTime /= 3;
				} else {
					Player.hurtCooldowns[info.CooldownCounter] /= 3;
				}
			}

			preHitBuffs = [];
			for (int i = 0; i < Player.MaxBuffs; i++) {
				preHitBuffs.Add(new Point(Player.buffType[i], Player.buffTime[i]));
			}
			lastHitEnemy = info.DamageSource.SourceNPCIndex;
			if (thornsVisualProjType >= 0) {
				Projectile.NewProjectile(
					Player.GetSource_Misc("thorns_visual"),
					Player.position,
					default,
					thornsVisualProjType,
					0,
					0,
					Player.whoAmI
				);
			}
			if (Origins.hurtCollisionCrimsonVine) {
				Origins.hurtCollisionCrimsonVine = false;
				GetAssimilation<Crimson_Assimilation>().Percent += 0.03f;
			}
			if (blastSet) {
				blastSetCharge += info.Damage * blast_set_charge_gain;
				if (blastSetCharge > blast_set_charge_max) blastSetCharge = blast_set_charge_max;
			}
			if (slagBucketCursed && Player.statLife > Player.statLifeMax2 * 0.5f) {
				Player.AddBuff(ModContent.BuffType<Slag_Bucket_Debuff>(), 4 * 60);
			}
			if (scrapBarrierCursed && Player.statLife > Player.statLifeMax2 * 0.5f) {
				Player.AddBuff(ModContent.BuffType<Scrap_Barrier_Debuff>(), 3 * 60);
			}


			if (Player.whoAmI == Main.myPlayer && info.DamageSource.TryGetCausingEntity(out Entity sourceEntity)) {
				Entity entity = sourceEntity;
				if (entity is Projectile proj) {
					if (proj.ModProjectile is IPostHitPlayer postHitPlayer) postHitPlayer.PostHitPlayer(Player, info);
				} else if (entity is NPC npc) {
					if (npc.ModNPC is IPostHitPlayer postHitPlayer) postHitPlayer.PostHitPlayer(Player, info);
				}
			}

			if (dangerTime < maxDangerTime) dangerTime = maxDangerTime;
		}
		#endregion
		/// <param name="target">the potential target</param>
		/// <param name="targetPriorityMultiplier"></param>
		/// <param name="isPriorityTarget">whether or not this npc is a "priority" target (i.e. a manually selected target)</param>
		/// <param name="foundTarget">whether or not a target has already been found</param>
		public delegate void Minion_Selector(NPC target, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget);
		public bool GetMinionTarget(Minion_Selector selector, bool noGuaranteedPriority = false) {
			bool foundTarget = false;
			if (Player.MinionAttackTargetNPC > -1) selector(Main.npc[Player.MinionAttackTargetNPC], 1f, true, ref foundTarget);
			if (asylumWhistleTarget > -1) selector(Main.npc[asylumWhistleTarget], 1f, true, ref foundTarget);
			if (!foundTarget || noGuaranteedPriority) {
				foreach (NPC target in Main.ActiveNPCs) if (target.CanBeChasedBy()) selector(target, 1f, false, ref foundTarget);
			}
			return foundTarget;
		}
		internal static float hitOriginalDamage = 0;
		internal static bool hitIsSelfDamage = false;
		public static void InflictTorn(Player player, int duration, int targetTime = 180, float targetSeverity = 0.3f, bool hideTimeLeft = false) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			int buffIndex = player.FindBuffIndex(Torn_Debuff.ID);
			if (buffIndex < 0 || (targetSeverity.CompareTo(originPlayer.tornTarget - Mojo_Injection.healing * 4) + (duration.CompareTo(player.buffTime[buffIndex]) & 1) > 0)) {
				bool shouldAdd = false;
				if (buffIndex < 0) {
					int decayIndex = player.FindBuffIndex(Torn_Decay_Debuff.ID);
					if (decayIndex >= 0) {
						player.buffType[decayIndex] = Torn_Debuff.ID;
						player.buffTime[decayIndex] = duration;
					} else {
						originPlayer.tornOffset = new Vector2(Main.rand.NextFloat());
						shouldAdd = true;
					}
				}
				if (shouldAdd) {
					player.AddBuff(Torn_Debuff.ID, duration);
				}
				originPlayer.tornSeverityRate = targetSeverity / targetTime;
				originPlayer.tornTarget = targetSeverity;
				originPlayer.hideTornTime = hideTimeLeft;
			}
		}
	}
}
