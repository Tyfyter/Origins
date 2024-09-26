using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items;
using Origins.Items.Accessories;
using Origins.Items.Armor.Riptide;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Dyes;
using Origins.Items.Other.Fish;
using Origins.Items.Pets;
using Origins.Items.Tools;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.Journal;
using Origins.NPCs;
using Origins.Questing;
using Origins.Reflection;
using Origins.Tiles.Brine;
using Origins.Tiles.Other;
using Origins.Water;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Origins.OriginExtensions;

namespace Origins {
	public partial class OriginPlayer : ModPlayer {
		public static Dictionary<Guid, int> playersByGuid;
		public const float explosive_defense_factor = 2f;
		public static OriginPlayer LocalOriginPlayer { get; internal set; }
		public override void PreUpdateMovement() {
			Origins.hurtCollisionCrimsonVine = false;
			if (riptideLegs && Player.wet) {
				Player.velocity *= 1.0048f;
				Player.ignoreWater = true;
			}
			if (riptideSet && !Player.mount.Active) {
				Player.dashType = 0;
				Player.dashTime = 0;
				const int riptideDashDuration = 12;
				const float riptideDashSpeed = 9f;
				if (dashDirection != 0 && (Player.velocity.X * dashDirection < riptideDashSpeed)) {
					Player.dashDelay = -1;
					Player.dash = 2;
					riptideDashTime = riptideDashDuration * dashDirection;
					Player.timeSinceLastDashStarted = 0;
					int gravDir = Math.Sign(Player.gravity);
					if (Player.velocity.Y * gravDir > Player.gravity * gravDir) {
						Player.velocity.Y = Player.gravity;
					}
					Projectile.NewProjectile(
						Player.GetSource_Misc("riptide_dash"),
						Player.Center + new Vector2(Player.width * dashDirection, 0),
						new Vector2(dashDirection * riptideDashSpeed, 0),
						Riptide_Dash_P.ID,
						25,
						riptideDashSpeed + 3,
						Player.whoAmI
					);
				}
				if (riptideDashTime != 0) {
					Player.velocity.X = riptideDashSpeed * Math.Sign(riptideDashTime);
					riptideDashTime -= Math.Sign(riptideDashTime);
					dashDelay = 25;
				}
			}
			if (meatScribeItem is not null && meatDashCooldown <= 0) {
				Player.dashType = 0;
				Player.dashTime = 0;
				const float meatDashTotalSpeed = 12f;
				const float meatDashSpeed = meatDashTotalSpeed / Scribe_of_the_Meat_God_P.max_updates;
				if (dashDirection != 0 && (Player.velocity.X * dashDirection < meatDashTotalSpeed)) {
					Player.dashDelay = -1;
					Player.dash = 2;
					Player.timeSinceLastDashStarted = 0;
					int gravDir = Math.Sign(Player.gravity);
					if (Player.velocity.Y * gravDir > Player.gravity * gravDir) {
						Player.velocity.Y = Player.gravity;
					}
					Projectile.NewProjectile(
						Player.GetSource_Misc("meat"),
						Player.Center + new Vector2(Player.width * dashDirection, 0),
						new Vector2(dashDirection * meatDashSpeed, 0),
						meatScribeItem.shoot,
						meatScribeItem.damage,
						meatDashSpeed * Scribe_of_the_Meat_God_P.max_updates + meatScribeItem.knockBack,
						Player.whoAmI
					);
					SoundEngine.PlaySound(SoundID.NPCDeath10.WithVolumeScale(0.75f), Player.position);
					dashDelay = Scribe_of_the_Meat_God_P.dash_duration + 6;
					meatDashCooldown = 120 + Scribe_of_the_Meat_God_P.dash_duration;
				}
			}
			if (shineSpark && ((loversLeapDashTime <= 0 && shineSparkCharge > 0) || shineSparkDashTime > 0)) {
				Player.dashType = 0;
				Player.dashTime = 0;
				const int shineSparkDuration = 90;
				const float shineSparkSpeed = 16f;
				if (shineSparkDashTime > 0) {
					Player.velocity = shineSparkDashDirection * shineSparkDashSpeed;
					if (collidingX || collidingY) {
						shineSparkDashTime = 1;
						Collision.HitTiles(Player.position, Player.velocity, Player.width, Player.height);
					}
					shineSparkDashTime--;
					dashDelay = 30;
					loversLeapDashTime = 0;
					Player.velocity.Y -= Player.gravity * Player.gravDir * 0.1f;
				} else {
					if (dashDirection != 0 || dashDirectionY != 0) {
						shineSparkDashTime = shineSparkDuration;
						shineSparkDashSpeed = shineSparkSpeed;
						Player.timeSinceLastDashStarted = 0;
						shineSparkDashDirection = new((Player.controlRight ? 1 : 0) - (Player.controlLeft ? 1 : 0), (Player.controlDown ? 1 : 0) - (Player.controlUp ? 1 : 0));
						shineSparkDashDirection.Normalize();
						if (collidingY && oldYSign > 0) Player.position.Y -= 1;
					}
				}
			} else if (loversLeap) {
				Player.dashType = 0;
				Player.dashTime = 0;
				const int loversLeapDuration = 6;
				const float loversLeapSpeed = 12f;
				if (collidingX || collidingY) {
					if ((dashDirection != 0 && (Player.velocity.X * dashDirection < loversLeapSpeed)) || (dashDirectionY != 0 && (Player.velocity.Y * dashDirectionY < loversLeapSpeed))) {
						//Player.dashDelay = -1;
						//Player.dash = 2;
						loversLeapDashTime = loversLeapDuration;
						loversLeapDashSpeed = loversLeapSpeed;
						Player.timeSinceLastDashStarted = 0;
						if (dashDirectionY == -1) {
							loversLeapDashDirection = 0;
						} else if (dashDirectionY == 1) {
							loversLeapDashDirection = 1;
						} else if (dashDirection == 1) {
							loversLeapDashDirection = 2;
						} else if (dashDirection == -1) {
							loversLeapDashDirection = 3;
						}
					}
				}
				if (loversLeapDashTime > 0) {
					if (loversLeapDashTime > 1) {
						switch (loversLeapDashDirection) {
							case 0:
							Player.velocity.Y = -loversLeapDashSpeed;
							break;
							case 1:
							Player.velocity.Y = loversLeapDashSpeed;
							break;
							case 2:
							Player.velocity.X = loversLeapDashSpeed;
							break;
							case 3:
							Player.velocity.X = -loversLeapDashSpeed;
							break;
						}
						loversLeapDashTime--;
						dashDelay = 30;
					}
					if ((loversLeapDashTime == 1 || loversLeapDashDirection == 2 || loversLeapDashDirection == 3) && loversLeapDashSpeed > 0) {
						bool bounce = false;
						bool canBounce = true;
						if (Math.Abs(Player.velocity.X) > Math.Abs(Player.velocity.Y)) {
							loversLeapDashDirection = Math.Sign(Player.velocity.X) == 1 ? 2 : 3;
						} else {
							loversLeapDashDirection = Math.Sign(Player.velocity.Y) == 1 ? 1 : 0;
						}
						Rectangle loversLeapHitbox = default;
						int hitDirection = 0;
						switch (loversLeapDashDirection) {
							case 0:
							canBounce = false;
							break;
							case 1:
							loversLeapHitbox = Player.Hitbox;
							loversLeapHitbox.Inflate(4, 4);
							loversLeapHitbox.Offset(0, 8);
							break;
							case 2:
							loversLeapHitbox = new Rectangle((int)(Player.position.X + Player.width), (int)(Player.position.Y - 4), 8, Player.height + 8);
							hitDirection = 1;
							break;
							case 3:
							loversLeapHitbox = new Rectangle((int)(Player.position.X - 8), (int)(Player.position.Y - 4), 8, Player.height + 8);
							hitDirection = -1;
							break;
						}
						if (canBounce) {
							for (int i = 0; i < Main.maxNPCs; i++) {
								NPC npc = Main.npc[i];
								if (npc.active && !npc.dontTakeDamage) {
									if (!npc.friendly || (npc.type == NPCID.Guide && Player.killGuide) || (npc.type == NPCID.Clothier && Player.killClothier)) {
										if (loversLeapHitbox.Intersects(npc.Hitbox)) {
											bounce = true;
											NPC.HitModifiers modifiers = npc.GetIncomingStrikeModifiers(loversLeapItem.DamageType, hitDirection);

											Player.ApplyBannerOffenseBuff(npc, ref modifiers);
											if (npc.life > 5) {
												Player.OnHit(npc.Center.X, npc.Center.Y, npc);
											}
											modifiers.ArmorPenetration += Player.GetWeaponArmorPenetration(loversLeapItem);
											CombinedHooks.ModifyPlayerHitNPCWithItem(Player, loversLeapItem, npc, ref modifiers);

											NPC.HitInfo strike = modifiers.ToHitInfo(Player.GetWeaponDamage(loversLeapItem), Main.rand.Next(100) < Player.GetWeaponCrit(loversLeapItem), Player.GetWeaponKnockback(loversLeapItem), damageVariation: true, Player.luck);
											NPCKillAttempt attempt = new(npc);
											int dmgDealt = npc.StrikeNPC(strike);

											CombinedHooks.OnPlayerHitNPCWithItem(Player, loversLeapItem, npc, in strike, dmgDealt);
											PlayerMethods.ApplyNPCOnHitEffects(Player, loversLeapItem, Item.GetDrawHitbox(loversLeapItem.type, Player), strike.SourceDamage, strike.Knockback, npc.whoAmI, strike.SourceDamage, dmgDealt);
											int bannerID = Item.NPCtoBanner(npc.BannerID());
											if (bannerID >= 0) {
												Player.lastCreatureHit = bannerID;
											}
											if (Main.netMode != NetmodeID.SinglePlayer) {
												NetMessage.SendStrikeNPC(npc, in strike);
											}
											if (Player.accDreamCatcher && !npc.HideStrikeDamage) {
												Player.addDPS(dmgDealt);
											}
											if (attempt.DidNPCDie()) {
												Player.OnKillNPC(ref attempt, loversLeapItem);
											}
											Player.GiveImmuneTimeForCollisionAttack(4);
										}
									}
								}
							}
							if (bounce) {
								loversLeapDashDirection ^= 1;
								loversLeapDashTime = 2;
								loversLeapDashSpeed = Math.Min(loversLeapDashSpeed - 0.5f, 9f);
							}
							switch (loversLeapDashDirection) {
								case 2:
								case 3:
								if (bounce) {
									Player.velocity.Y -= 4;
									loversLeapDashSpeed -= 2f;
								}
								break;
								case 0:
								case 1:
								if (collidingX || collidingY) {
									loversLeapDashTime = 0;
								}
								break;
							}
						}
					} else if(loversLeapDashTime == 1) {
						loversLeapDashTime = 0;
					}
				}
			}
			if (rebreather && Player.breath < Player.breathMax) {
				if (Player.breathCD == 0 || rebreatherCounting) {
					rebreatherCounting = true;
					const float maxCount = 8f;
					const float maxSpeed = 16f;
					float speed = Math.Min(Player.velocity.Length() / maxSpeed, 1);
					if ((rebreatherCount += speed) >= maxCount) {
						rebreatherCounting = false;
						rebreatherCount -= maxCount;
						Player.breath++;
					}
				}
			}
			if (hookTarget >= 0) {//ropeVel.HasValue&&
				Player.fallStart = (int)(Player.position.Y / 16f);
				Projectile projectile = Main.projectile[hookTarget];
				if (projectile.type == Amoeba_Hook_P.ID) {
					Vector2 diff = Player.Center - projectile.Center;
					Vector2 normDiff = diff.SafeNormalize(default);
					float dot = Vector2.Dot(normDiff, Player.velocity.SafeNormalize(default));
					Player.velocity = Vector2.Lerp(normDiff * -16, Player.velocity, 0.85f + dot * 0.1f);
					if (diff.LengthSquared() > 64) {
						Player.GoingDownWithGrapple = true;
					}
					Player.RefreshMovementAbilities();
				}
			}
			if (changeSize) {
				Player.position.X -= (targetWidth - Player.width) / 2;
				Player.position.Y -= targetHeight - Player.height;
				Player.width = targetWidth;
				Player.height = targetHeight;
			}
			oldXSign = Math.Sign(Player.velocity.X);
			oldYSign = Math.Sign(Player.velocity.Y);
			//endCustomMovement:
			hookTarget = -1;
			/*int directionX = Math.Sign(Player.velocity.X);
			int directionY = Math.Sign(Player.velocity.Y);
			int vine = ModContent.TileType<Brineglow_Vine>();
			foreach (Point item in Collision.GetTilesIn(Player.TopLeft, Player.BottomRight)) {
				Tile tile = Framing.GetTileSafely(item);
				if (tile.HasTile && tile.TileType == vine) {
					ref short windSpeed = ref tile.Get<TileExtraVisualData>().TileFrameX;
					windSpeed = (short)Math.Clamp(windSpeed + Player.velocity.X, -128, 128);
				}
			}*/
		}
		public override void PreUpdate() {
			if (corruptionAssimilation > 0) {
				Player.AddBuff(ModContent.BuffType<Corrupt_Assimilation_Debuff>(), 5);
			}
			if (crimsonAssimilation > 0) {
				Player.AddBuff(ModContent.BuffType<Crimson_Assimilation_Debuff>(), 5);
			}
			if (defiledAssimilation > 0) {
				Player.AddBuff(ModContent.BuffType<Defiled_Assimilation_Debuff>(), 5);
			}
			if (rivenAssimilation > 0) {
				Player.AddBuff(ModContent.BuffType<Riven_Assimilation_Debuff>(), 5);
			}
			if (rivenWet) {
				Player.gravity = 0.25f;
			}
			if (ravel && spiderRavel) {
				ceilingRavel = false;
				if (collidingX) {
					Player.gravity = 0;
					Player.velocity.Y *= 0.9f;
					if (Player.controlUp) {
						Player.velocity.Y -= 0.35f;
					}
					if (Player.controlDown) {
						Player.velocity.Y += 0.35f;
					}
				} else {
					bool colliding = false;
					float halfSpiderWidth = Player.width / 2 - 1;
					float halfSpiderHeight = Player.height / 2 + 4;
					for (int i = -1; i < 2; i++) {
						Tile currentTile = Main.tile[(Player.Center - Player.velocity + new Vector2(halfSpiderWidth * i, -halfSpiderHeight)).ToTileCoordinates()];
						if (currentTile.HasTile && Main.tileSolid[currentTile.TileType] && !Main.tileSolidTop[currentTile.TileType]) {
							colliding = true;
							break;
						}
					}
					if (colliding) {
						ceilingRavel = true;
						spiderRavelTime = 10;
					}
					if (Player.controlDown) {
						spiderRavelTime = 0;
					}
					if (spiderRavelTime > 0 && Player.controlUp) {
						Player.gravity = 0f;
						Player.velocity.Y -= 0.35f;
						for (int i = -1; i < 2; i++) {
							Tile currentTile = Main.tile[(Player.Center - Player.velocity + new Vector2(9 * i, -30)).ToTileCoordinates()];
							if (currentTile.HasTile && Main.tileSolid[currentTile.TileType] && !Main.tileSolidTop[currentTile.TileType]) {
								Player.velocity.Y -= 1;
								break;
							}
						}
						Collision.StepUp(ref Player.position, ref Player.velocity, Player.width, Player.height, ref Player.stepSpeed, ref Player.gfxOffY, -1, true);
					}
				}
			}
		}
		public override void PostUpdate() {
			heldProjectile = -1;
			if (rasterizedTime > 0) {
				Player.velocity = Vector2.Lerp(Player.velocity, Player.oldVelocity, rasterizedTime * 0.06f);
				Player.position = Vector2.Lerp(Player.position, Player.oldPosition, rasterizedTime * 0.06f);
			}
			Player.oldVelocity = Player.velocity;
			rivenWet = false;
			if ((Player.wet || WaterCollision(Player.position, Player.width, Player.height)) && !(Player.lavaWet || Player.honeyWet)) {
				ModWaterStyle waterStyle = LoaderManager.Get<WaterStylesLoader>().Get(Main.waterStyle);
				if (waterStyle is Riven_Water_Style) {
					rivenWet = true;
					/*if (GameModeData.ExpertMode) {
						int duration = 432;
						int targetTime = 1440;
						float targetSeverity = 0f;
					} else if (GameModeData.MasterMode) {
						int duration = 676;
						int targetTime = 1440;
						float targetSeverity = 0f;
					} else if (GameModeData.NormalMode) {
						int duration = 188;
						int targetTime = 1440;
						float targetSeverity = 0f;
					} else if (GameModeData.Creative) {
						int duration = 188;
						int targetTime = 1440;
						float targetSeverity = 0.08f;
					}*/
					InflictTorn(Player, 188, 750, 1f, true);
					Player.velocity *= 0.95f;
					rivenAssimilation += 0.001f; // This value x60 for every second, remember 100% is the max assimilation. This should be 6% every second resulting in 16.67 seconds of total time to play in Riven Water
				} else if (waterStyle is Brine_Water_Style) {
					Player.AddBuff(Toxic_Shock_Debuff.ID, 300);
				}
			}

			if (shineSpark) {
				if (shineSparkDashTime > 0) {
					shineSparkCharge = 0;
				} else {
					const int max_shinespark_charge = 50;
					const int shinespark_trigger_charge = 60 * 2 * -1;
					int dir = shineSparkCharge > 0 ? 1 : -1;
					bool isCharging = Math.Abs(Player.velocity.X) > 7;
					if (isCharging) {
						if (collidingY) shineSparkCharge += dir;
						if (shineSparkCharge < shinespark_trigger_charge || shineSparkCharge > max_shinespark_charge) shineSparkCharge = max_shinespark_charge;
					} else if (shineSparkCharge != 0) {
						shineSparkCharge -= dir;
					}
				}
			}
		}
		public override void OnRespawn() {
			oldGravDir = Player.gravDir;
			if (hasProtOS) {
				Protomind.PlayRandomMessage(Protomind.QuoteType.Respawn, protOSQuoteCooldown, Player.Top);
			}
		}
		public override void UpdateDead() {
			timeSinceLastDeath = -1;
			tornCurrentSeverity = 0;
			tornTarget = 0f;
			mojoFlaskCount = mojoFlaskCountMax;

			corruptionAssimilation = 0;
			crimsonAssimilation = 0;
			defiledAssimilation = 0;
			rivenAssimilation = 0;

			selfDamageRally = 0;
			blastSetCharge = 0;
		}
		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			health = StatModifier.Default;
			mana = StatModifier.Default;
			mana.Base += quantumInjectors * Quantum_Injector.mana_per_use;
			if (tornCurrentSeverity > 0) {
				health *= 1 - tornCurrentSeverity;
				if (tornCurrentSeverity >= 1) {
					Player.KillMe(new KeyedPlayerDeathReason() {
						Key = "Mods.Origins.DeathMessage.Torn_" + Main.rand.Next(5)
					}, 1, 0);
				}
			}
		}
		public override void PostUpdateBuffs() {
			if (Player.whoAmI == Main.myPlayer) {
				foreach (var quest in Quest_Registry.Quests) {
					if (quest.PreUpdateInventoryEvent is not null) {
						quest.PreUpdateInventoryEvent();
					}
				}
			}
			if (mojoInjection) Mojo_Injection.UpdateEffect(this);
		}
		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			if (hasPotatOS) {
				Potato_Battery.PlayRandomMessage(Potato_Battery.QuoteType.Death, potatOSQuoteCooldown, Player.Top);
			}
			if (hasProtOS) {
				Protomind.PlayRandomMessage(Protomind.QuoteType.Death, protOSQuoteCooldown, Player.Top);
			}
			if (talkingPet != -1) {
				Projectile pet = Main.projectile[talkingPet];
				if (pet.type == Chew_Toy.projectileID) {
					Chee_Toy_Messages.Instance.PlayRandomMessage(Chee_Toy_Message_Types.Death, pet.Top);
				}
			}
		}
		public override void ProcessTriggers(TriggersSet triggersSet) {
			releaseTriggerSetBonus = !controlTriggerSetBonus;
			controlTriggerSetBonus = Origins.SetBonusTriggerKey.Current;
			if (controlTriggerSetBonus && releaseTriggerSetBonus) {
				TriggerSetBonus();
			}
			if (Player.controlDown && Player.releaseDown) {
				doubleTapDown = doubleTapDownTimer < 15;
				doubleTapDownTimer = 0;
			} else doubleTapDown = false;
		}
		public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback) {
			if (Player.InModBiome<Brine_Pool>()) {
				Player.adjWater = false;
			}
			List<Item> items = new();
			Dictionary<int, Item> substituteItems = new();
			int switchblade = ModContent.ItemType<Switchblade_Shortsword>();
			Item currentItem;
			for (int i = 0; i < Player.inventory.Length; i++) {
				currentItem = Player.inventory[i];
				if (currentItem.type == switchblade) {
					substituteItems.Add(items.Count, currentItem);
					items.Add(new Item(ModContent.ItemType<Switchblade_Broadsword>(), currentItem.stack, currentItem.prefix));
				}
			}
			itemConsumedCallback = (item, index) => {
				if (substituteItems.TryGetValue(index, out Item consumed)) {
					consumed.stack = item.stack;
					if (consumed.stack <= 0) consumed.TurnToAir();
				}
			};
			return items;
		}
		public override bool CanSellItem(NPC vendor, Item[] shopInventory, Item item) {
			if (item.prefix == ModContent.PrefixType<Imperfect_Prefix>()) return false;
			return true;
		}
		public override void PostSellItem(NPC vendor, Item[] shopInventory, Item item) {
			if (vendor.type == NPCID.Demolitionist && item.type == ModContent.ItemType<Peat_Moss_Item>()) {
				OriginSystem originWorld = ModContent.GetInstance<OriginSystem>();
				if (originWorld.peatSold < 999) {
					if (item.stack >= 999 - originWorld.peatSold) {
						item.stack -= 999 - originWorld.peatSold;
						originWorld.peatSold = 999;
						int nextSlot = 0;
						for (; ++nextSlot < shopInventory.Length && !shopInventory[nextSlot].IsAir;) ;
						if (nextSlot < shopInventory.Length) shopInventory[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Grenade>());
						if (nextSlot < shopInventory.Length) shopInventory[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Bomb>());
						if (nextSlot < shopInventory.Length) shopInventory[nextSlot].SetDefaults(ModContent.ItemType<Impact_Dynamite>());
					} else {
						originWorld.peatSold += item.stack;
						item.TurnToAir();
					}
					if (Main.netMode != NetmodeID.SinglePlayer) {
						ModPacket packet = Mod.GetPacket();
						packet.Write(Origins.NetMessageType.sync_peat);
						packet.Write((short)OriginSystem.Instance.peatSold);
						packet.Send(-1, Player.whoAmI);
					}
				}
			}
		}
		public bool DisplayJournalTooltip(IJournalEntryItem journalItem) {
			if (!journalUnlocked) {
				return true;
			}
			bool unlockedEntry = unlockedJournalEntries.Contains(journalItem.EntryName);
			if (Origins.InspectItemKey.JustPressed) {
				if (!unlockedEntry) unlockedJournalEntries.Add(journalItem.EntryName);
				if (OriginClientConfig.Instance.OpenJournalOnUnlock) {
					Origins.OpenJournalEntry(journalItem.EntryName);
				}
				return false;
			}
			return !unlockedEntry;
		}

		public override void SaveData(TagCompound tag) {
			if (eyndumCore is not null) {
				tag.Add("EyndumCore", eyndumCore.Value);
			}
			tag.Add("MimicSetSelection", mimicSetChoices);
			tag.Add("journalUnlocked", journalUnlocked);
			if (journalDye is not null) {
				tag.Add("JournalDye", journalDye);
			}
			if (unlockedJournalEntries is not null) {
				tag.Add("UnlockedJournalEntries", unlockedJournalEntries.ToList());
			}
			if (startedQuests is not null) {
				tag.Add("UnlockedQuests", startedQuests.ToList());
			}
			TagCompound questsTag = new TagCompound();
			foreach (var quest in Quest_Registry.Quests) {
				if (!quest.SaveToWorld) {
					TagCompound questTag = new TagCompound();
					quest.SaveData(questTag);
					if (questTag.Count > 0) questsTag.Add(quest.FullName, questTag);
				}
			}
			if (questsTag.Count > 0) {
				tag.Add("Quests", questsTag);
			}
			tag.Add("TimeSinceLastDeath", timeSinceLastDeath);
			tag.Add("corruptionAssimilation", corruptionAssimilation);
			tag.Add("crimsonAssimilation", crimsonAssimilation);
			tag.Add("defiledAssimilation", defiledAssimilation);
			tag.Add("rivenAssimilation", rivenAssimilation);
			tag.Add("mojoInjection", mojoInjection);
			tag.Add("GUID", guid.ToByteArray());
		}
		public override void LoadData(TagCompound tag) {
			if (tag.SafeGet<Item>("EyndumCore") is Item eyndumCoreItem) {
				eyndumCore = new Ref<Item>(eyndumCoreItem);
			}
			if (tag.SafeGet<int>("MimicSetSelection") is int mimicSetSelection) {
				mimicSetChoices = mimicSetSelection;
			}
			if (tag.SafeGet<Item>("JournalDye") is Item journalDyeItem) {
				journalDye = journalDyeItem;
			}
			if (tag.SafeGet<List<string>>("UnlockedJournalEntries") is List<string> journalEntries) {
				unlockedJournalEntries = journalEntries.ToHashSet();
			}
			if (tag.SafeGet<List<string>>("UnlockedQuests") is List<string> unlockedQuests) {
				startedQuests = unlockedQuests.ToHashSet();
			}
			if (tag.ContainsKey("journalUnlocked")) {
				journalUnlocked = tag.Get<bool>("journalUnlocked");
			}
			questsTag = tag.SafeGet<TagCompound>("Quests");
			if (tag.SafeGet<int>("TimeSinceLastDeath") is int timeSinceLastDeath) {
				this.timeSinceLastDeath = timeSinceLastDeath;
			}
			corruptionAssimilation = tag.SafeGet<float>("corruptionAssimilation");
			crimsonAssimilation = tag.SafeGet<float>("crimsonAssimilation");
			defiledAssimilation = tag.SafeGet<float>("defiledAssimilation");
			rivenAssimilation = tag.SafeGet<float>("rivenAssimilation");
			mojoInjection = tag.SafeGet<bool>("mojoInjection");
			if (tag.TryGet("GUID", out byte[] guidBytes)) {
				guid = new Guid(guidBytes);
			} else {
				guid = Guid.NewGuid();
			}
		}
		TagCompound questsTag;
		public override void OnEnterWorld() {
			questsTag ??= new TagCompound();
			TagCompound worldQuestsTag = ModContent.GetInstance<OriginSystem>().questsTag ?? new TagCompound();
			Origins.instance.Logger.Debug(worldQuestsTag.ToString());
			foreach (var quest in Quest_Registry.Quests) {
				if (!quest.SaveToWorld) {
					quest.LoadData(questsTag.SafeGet<TagCompound>(quest.FullName) ?? new TagCompound());
				} else {
					quest.LoadData(worldQuestsTag.SafeGet<TagCompound>(quest.FullName) ?? new TagCompound());
				}
			}
			netInitialized = false;
		}
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			FishingLoot.Pool.CatchFish(Player, attempt, ref itemDrop, ref npcSpawn, ref sonar, ref sonarPosition);
		}
		public override void GetDyeTraderReward(List<int> rewardPool) {
			rewardPool.AddRange([
				ModContent.ItemType<Amber_Dye>(),
				ModContent.ItemType<High_Contrast_Dye>(),
				ModContent.ItemType<Rasterized_Dye>(),// temp
			]);
		}
		public override bool CanUseItem(Item item) {
			if (ravel) {
				return false;
			}
			return true;
		}
		public override bool PreItemCheck() {
			collidingX = oldXSign != 0 && Player.velocity.X == 0;
			collidingY = oldYSign != 0 && Player.velocity.Y == 0;
			if (disableUseItem) {
				itemUseOldDirection = Player.direction;
				return false;
			}
			ItemChecking = true;
			if (Player.HeldItem.ModItem is C6_Jackhammer or Miter_Saw && Player.controlUseTile) {
				if (Player.ItemAnimationEndingOrEnded) {
					Player.direction = itemUseOldDirection;
				} else if (Player.altFunctionUse == 2) {
					Player.controlUseItem = true;
				}
			}
			itemUseOldDirection = Player.direction;
			if (focusPotion) {
				if (Player.ItemAnimationJustStarted) {
					focusPotionThisUse = Player.CheckMana(Focus_Potion.GetManaCost(Player.HeldItem), true);
					Player.manaRegenDelay = (int)Player.maxRegenDelay;
				} else if (Player.ItemAnimationEndingOrEnded) {
					focusPotionThisUse = false;
				}
			} else {
				focusPotionThisUse = false;
			}
			return true;
		}
		public override void PostItemCheck() {
			ItemChecking = false;
		}
		public void InflictAssimilation(byte assimilationType, float assimilationAmount) {
			switch (assimilationType) {
				case 0:
				CorruptionAssimilation += assimilationAmount;
				break;
				case 1:
				CrimsonAssimilation += assimilationAmount;
				break;
				case 2:
				DefiledAssimilation += assimilationAmount;
				break;
				case 3:
				RivenAssimilation += assimilationAmount;
				break;
			}
			if (Main.netMode == NetmodeID.SinglePlayer || Player.whoAmI == Main.myPlayer) return;
			ModPacket packet = Origins.instance.GetPacket();
			packet.Write(Origins.NetMessageType.inflict_assimilation);
			packet.Write((byte)Player.whoAmI);
			packet.Write(assimilationType);
			packet.Write(assimilationAmount);
			packet.Send(Player.whoAmI, Main.myPlayer);
		}
		public void ResetLaserTag() {
			if (!Laser_Tag_Console.LaserTagGameActive) laserTagRespawnDelay = 0;
			laserTagVestActive = false;
			laserTagPoints = 0;
			laserTagHits = 0;
			laserTagHP = 0;
		}
	}
}
