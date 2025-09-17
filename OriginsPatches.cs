using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.NPCs;
using Origins.NPCs.Riven.World_Cracker;
using Origins.NPCs.TownNPCs;
using Origins.Projectiles;
using Origins.Reflection;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Origins.Walls;
using Origins.World.BiomeData;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Personalities;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI.Chat;
using Terraria.Utilities;
using static Origins.OriginExtensions;
using static Mono.Cecil.Cil.OpCodes;
using MC = Terraria.ModLoader.ModContent;
using Origins.Backgrounds;
using Origins.Items.Tools;
using Origins.Tiles;
using System.Runtime.CompilerServices;
using Terraria.ModLoader.Core;
using Origins.Items.Other.Dyes;
using Terraria.GameContent.UI.ResourceSets;
using Origins.Water;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Light;
using Origins.Items;
using System.Runtime.ExceptionServices;
using System.IO;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Magic;
using Terraria.GameContent.Events;
using Origins.Items.Weapons.Summoner.Minions;
using AltLibrary.Common.Hooks;
using Terraria.ModLoader.UI;
using PegasusLib.Graphics;
using PegasusLib;
using Terraria.GameContent.UI;
using Origins.UI.Event;
using Origins.Items.Other.Consumables.Broths;
using Origins.Items.Weapons.Demolitionist;
using PegasusLib.Reflection;
using Terraria.GameContent.Generation;
using Origins.Items.Mounts;
using Terraria.ModLoader.Config;
using Origins.NPCs.Brine;
using Terraria.GameContent.Shaders;
using Origins.NPCs.MiscB.Shimmer_Construct;

namespace Origins {
	public partial class Origins : Mod {
		void ApplyPatches() {
			On_NPC.UpdateCollision += (orig, self) => {
				int realID = self.type;
				if (self.ModNPC is ICustomCollisionNPC shark) {
					if (shark.IsSandshark) self.type = NPCID.SandShark;
					try {
						shark.PreUpdateCollision();
						orig(self);
					} finally {
						shark.PostUpdateCollision();
					}
					self.type = realID;
					return;
				}
				IAltTileCollideNPC tcnpc = self.ModNPC as IAltTileCollideNPC;
				self.type = tcnpc?.CollisionType ?? realID;
				orig(self);
				self.type = realID;
			};
			On_NPC.GetMeleeCollisionData += NPC_GetMeleeCollisionData;
			//On.Terraria.WorldGen.GERunner += OriginSystem.GERunnerHook;
			//On.Terraria.WorldGen.Convert += OriginSystem.ConvertHook;
			OriginSystem worldInstance = MC.GetInstance<OriginSystem>();
			if (worldInstance is not null) {
				worldInstance.defiledResurgenceTiles = [];
				worldInstance.defiledAltResurgenceTiles = [];
			}
			//IL.Terraria.WorldGen.GERunner+=OriginWorld.GERunnerHook;
			On_Main.DrawInterface_Resources_Breath += FixedDrawBreath;
			On_WorldGen.CountTiles += WorldGen_CountTiles;
			On_WorldGen.AddUpAlignmentCounts += WorldGen_AddUpAlignmentCounts;
			Terraria.IO.WorldFile.OnWorldLoad += () => {
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					for (int i = 0; i < Main.maxTilesX; i++) WorldGen.CountTiles(i);
				}
			};

			Terraria.Graphics.Renderers.On_LegacyPlayerRenderer.DrawPlayerInternal += LegacyPlayerRenderer_DrawPlayerInternal;
			On_PlayerDrawLayers.DrawPlayer_TransformDrawData += On_PlayerDrawLayers_DrawPlayer_TransformDrawData;
			On_Projectile.GetWhipSettings += Projectile_GetWhipSettings;
			/*On_Recipe.CollectItemsToCraftWithFrom += (orig, player) => {
				orig(player);
				if (player.InModBiome<Brine_Pool>()) {
					player.adjWater = false;
				}
			};*/
			MonoModHooks.Add(typeof(CommonCode).GetMethod("DropItem", BindingFlags.Public | BindingFlags.Static, [typeof(DropAttemptInfo), typeof(int), typeof(int), typeof(bool)]), (hook_DropItem)CommonCode_DropItem);
			On_ShopHelper.BiomeNameByKey += (On_ShopHelper.orig_BiomeNameByKey orig, string biomeNameKey) => {
				lastBiomeNameKey = biomeNameKey;
				return orig(biomeNameKey);
			};

			On_Language.GetTextValueWith += (On_Language.orig_GetTextValueWith orig, string key, object obj) => {
				if (key.EndsWith("Biome")) {
					try {
						string betterKey = key + "_" + lastBiomeNameKey.Split('.')[^1];
						if (Language.Exists(betterKey)) {
							key = betterKey;
						}/* else if (!Language.Exists(lastBiomeNameKey)) {
							obj = new {
								BiomeName = "the " + lastBiomeNameKey.Split('.')[^1].Replace('_', ' ')
							};
						}*/
					} catch (RuntimeBinderException) { }
				}
				return orig(key, obj);
			};
			On_ShopHelper.IsPlayerInEvilBiomes += (On_ShopHelper.orig_IsPlayerInEvilBiomes orig, ShopHelper self, Player player) => {
				if (ShopMethods._currentNPCBeingTalkedTo?.GetValue(self) is NPC talkNPC) {
					if (talkNPC.type == MC.NPCType<Brine_Fiend>()) {
						IShoppingBiome shoppingBiome = new DungeonBiome();
						if (shoppingBiome.IsInBiome(player)) {
							ShopMethods.AddHappinessReportText(self, "HateBiome", new {
								BiomeName = ShopHelper.BiomeNameByKey(shoppingBiome.NameKey)
							});
						}
						return false;
					}
					if (talkNPC.type == MC.NPCType<Defiled_Effigy>()) {
						List<IShoppingBiome> dangerousBiomes = ShopHelper_EvilBiomes.DangerousBiomes;
						for (int i = 0; i < dangerousBiomes.Count; i++) {
							IShoppingBiome aShoppingBiome = dangerousBiomes[i];
							if (aShoppingBiome == MC.GetInstance<Defiled_Wastelands>()) continue;
							if (aShoppingBiome.IsInBiome(player)) {
								ShopMethods.AddHappinessReportText(self, "HateBiome", new {
									BiomeName = ShopHelper.BiomeNameByKey(aShoppingBiome.NameKey)
								});
								return true;
							}
						}
						return false;
					}
				}
				return orig(self, player);
			};
			On_Main.GetProjectileDesiredShader += (orig, projectile) => {
				if (projectile.TryGetGlobalProjectile(out OriginGlobalProj originGlobalProj) && originGlobalProj.isFromMitosis) {
					return GameShaders.Armor.GetShaderIdFromItemId(ItemID.StardustDye);
				}
				if (projectile.ModProjectile is IShadedProjectile shadedProjectile) {
					return shadedProjectile.Shader;
				}
				return orig(projectile);
			};
			On_TileLightScanner.GetTileLight += TileLightScanner_GetTileLight;
			IL_WorldGen.PlantAlch += WorldGen_PlantAlchIL;
			On_WorldGen.ShakeTree += WorldGen_ShakeTree;
			MonoModHooks.Add(
				typeof(MC).GetMethod("ResizeArrays", BindingFlags.NonPublic | BindingFlags.Static),
				(Action<bool> orig, bool unloading) => {
					orig(unloading);
					if (!unloading) ResizeArrays();
				}
			);
			On_WorldGen.KillWall_CheckFailure += (On_WorldGen.orig_KillWall_CheckFailure orig, bool fail, Tile tileCache) => {
				fail = orig(fail, tileCache);
				if (Main.LocalPlayer.HeldItem.hammer < WallHammerRequirement[tileCache.WallType]) {
					fail = true;
				}
				return fail;
			};
			//Terraria.On_Main.DrawNPCChatButtons += Main_DrawNPCChatButtons;
			On_Player.SetTalkNPC += Player_SetTalkNPC;
			On_Item.CanFillEmptyAmmoSlot += (orig, self) => {
				if (self.ammo == ItemID.Grenade || self.ammo == ItemID.Bomb || self.ammo == ItemID.Dynamite) {
					return false;
				}
				return orig(self);
			};
			On_NPC.AddBuff += (orig, self, type, time, quiet) => {
				Player inflictingPlayer = Main.CurrentPlayer;
				float durationModifier = 1f;
				if (Main.ProjectileUpdateLoopIndex != -1) {
					Projectile projectile = Main.projectile[Main.ProjectileUpdateLoopIndex];
					if (projectile.TryGetOwner(out Player owner)) {
						inflictingPlayer = owner;
						OriginPlayer originPlayer = owner.OriginPlayer();
						if (projectile.IsMinionOrSentryRelated && originPlayer.broth is Savory_Broth) {
							durationModifier *= 1.2f;
						}
					}
				}
				time = (int)(time * durationModifier);
				orig(self, type, time, quiet);
				if (!quiet && type != Headphones_Buff.ID && BuffID.Sets.IsATagBuff[type] && inflictingPlayer.OriginPlayer().summonTagForceCrit) {
					orig(self, Headphones_Buff.ID, 300, quiet);
				}
			};
			On_Player.RollLuck += Player_RollLuck;
			On_TileDrawing.Draw += TileDrawing_Draw;
			On_TileDrawing.DrawTiles_GetLightOverride += TileDrawing_DrawTiles_GetLightOverride;
			On_Player.KillMe += Player_KillMe;// should have no effect, but is necessary for custom death text somehow
			On_WorldGen.PlacePot += WorldGen_PlacePot;
			On_WorldGen.PlaceSmallPile += WorldGen_PlaceSmallPile;
			On_Projectile.ExplodeTiles += Projectile_ExplodeTiles;
			IL_Player.Update += (il) => {
				ILCursor c = new(il);
				int index = -1;
				if (c.TryGotoNext(MoveType.After,
					i => i.MatchLdloc(out index),
					i => i.MatchLdfld<Collision.HurtTile>(nameof(Collision.HurtTile.type)),
					i => i.MatchLdcI4(0),
					i => i.MatchBlt(out _)
				)) {
					c.EmitLdloca(index);
					c.EmitDelegate((in Collision.HurtTile hurtTile) => {
						if (hurtTile.type == TileID.CrimsonThorns) hurtCollisionCrimsonVine = true;
					});
				} else {
					LogError("Could not find GetHurtTile call in Player.Update");
				}
				c = new(il);
				int fallThrough = -1;
				int ignorePlats = -1;
				if (c.TryGotoNext(MoveType.Before,
					i => i.MatchLdarg0(),
					i => i.MatchLdloc(out fallThrough),
					i => i.MatchLdloc(out ignorePlats),
					i => i.MatchCall<Player>(nameof(Player.DryCollision))
				) && c.TryGotoPrev(MoveType.After,
					i => i.MatchCall(typeof(PlayerLoader), nameof(PlayerLoader.PreUpdateMovement))
				)) {
					c.EmitLdarg0();
					c.EmitLdloca(fallThrough);
					c.EmitLdloca(ignorePlats);
					c.EmitDelegate(static (Player player, ref bool fallThrough, ref bool ignorePlats) => {
						if (player.OriginPlayer().noFallThrough.TrySet(false)) {
							fallThrough = false;
							ignorePlats = false;
						}
					});
				} else {
					LogError("Could not find platform fallthrough management in Player.Update");
				}
			};
			On_ShopHelper.GetShoppingSettings += OriginGlobalNPC.ShopHelper_GetShoppingSettings;
			On_Player.HurtModifiers.ToHurtInfo += (On_Player.HurtModifiers.orig_ToHurtInfo orig, ref Player.HurtModifiers self, int damage, int defense, float defenseEffectiveness, float knockback, bool knockbackImmune) => {
				Player.HurtInfo info = orig(ref self, damage, defense, defenseEffectiveness, knockback, knockbackImmune);
				if (OriginPlayer.hitIsSelfDamage && self.DamageSource.TryGetCausingEntity(out Entity entity) && entity is Projectile) {
					float _damage = self.SourceDamage.ApplyTo(damage) * self.IncomingDamageMultiplier.Value;
					float armorPenetration = defense * Math.Clamp(self.ScalingArmorPenetration.Value, 0f, 1f) + self.ArmorPenetration.Value;
					float _defense = Math.Max(defense - armorPenetration, 0f);
					float damageReduction = _defense * defenseEffectiveness;
					_damage = Math.Max(_damage - damageReduction, 0f);
					if (self.FinalDamage.ApplyTo(_damage) <= 0) info.Cancelled = true;
				}
				
				return info;
			};
			On_Player.AddBuff_DetermineBuffTimeToAdd += On_Player_AddBuff_DetermineBuffTimeToAdd;
			On_Player.Update_NPCCollision += On_Player_Update_NPCCollision;
			IL_Main.UpdateWeather += IL_Main_UpdateWeather;
			On_Rain.Update += On_Rain_Update;
			On_Main.DrawRain += On_Main_DrawRain;
			IL_Main.DrawRain += IL_Main_DrawRain;
			IL_PlayerDrawLayers.DrawPlayer_28_ArmOverItemComposite += IL_PlayerDrawLayers_DrawPlayer_28_ArmOverItemComposite;
			IL_Main.DrawSurfaceBG += IL_Main_DrawSurfaceBG;
			On_Player.ItemCheck_UseMiningTools_TryHittingWall += On_Player_ItemCheck_UseMiningTools_TryHittingWall;
			IL_NPCUtils.TargetClosestNonBees += IL_NPCUtils_TargetClosestNonBees;
			On_CommonCode.ModifyItemDropFromNPC += On_CommonCode_ModifyItemDropFromNPC;
			IL_Player.Update += (il) => {
				ILCursor c = new(il);
				c.GotoNext(MoveType.After,
					i => i.MatchCall<Collision>("LavaCollision"),
					i => i.MatchStloc(out int _)
				);
				c.Index--;
				c.EmitDelegate<Func<bool, bool>>((flag) => {
					if (OriginPlayer.forceLavaCollision) {
						OriginPlayer.forceLavaCollision = false;
						return true;
					}
					return flag;
				});

				c.GotoNext(MoveType.After,
					i => i.MatchCall<Collision>("WetCollision")
				);
				c.EmitDelegate<Func<bool, bool>>((flag) => {
					if (OriginPlayer.forceWetCollision) {
						OriginPlayer.forceWetCollision = false;
						return true;
					}
					return flag;
				});

				c.GotoNext(MoveType.After,
					i => i.MatchLdsfld<Collision>("honey"),
					i => i.MatchStloc(out int _)
				); ;
				c.Index--;
				c.EmitDelegate<Func<bool, bool>>((flag) => {
					if (OriginPlayer.forceHoneyCollision) {
						OriginPlayer.forceHoneyCollision = false;
						return true;
					}
					return flag;
				});

				c.GotoNext(MoveType.After,
					i => i.MatchLdsfld<Collision>("shimmer"),
					i => i.MatchStloc(out int _)
				); ;
				c.Index--;
				c.EmitDelegate<Func<bool, bool>>((flag) => {
					if (OriginPlayer.forceShimmerCollision) {
						OriginPlayer.forceShimmerCollision = false;
						return true;
					}
					return flag;
				});
			};
			IL_ShopHelper.ApplyNpcRelationshipEffect += (il) => {
				ILCursor c = new(il);
				if (c.TryGotoNext(MoveType.After,
					i => i.MatchCall(typeof(DefaultInterpolatedStringHandler), nameof(DefaultInterpolatedStringHandler.ToStringAndClear))
				)) {
					c.EmitLdarg1();
					c.EmitDelegate<Func<string, int, string>>((key, npcType) => {
						if (npcType != 0) {
							if (Main.LocalPlayer.talkNPC < 0) return key;
							NPC talkNPC = Main.npc[Main.LocalPlayer.talkNPC];
							string talkNPCName = NPCID.Search.GetName(talkNPC.netID);
							string baseKey = "TownNPCMood_" + talkNPCName;
							string otherNPC = NPCID.Search.GetName(npcType);
							string potentialKey = $"{baseKey}.{key}_{otherNPC}";
							string potentialKey0 = $"Mods.Origins.NPCs.{talkNPCName}.{potentialKey}";
							if (Language.Exists(potentialKey0)) {
								return potentialKey0;
							}
							if (talkNPC.ModNPC is ModNPC modNPC) {
								if (talkNPC.ModNPC.Mod is Origins) return key;
								potentialKey = modNPC.GetLocalizationKey(potentialKey);
							}
							if (Language.Exists(potentialKey)) {
								return potentialKey;
							}
						}
						return key;
					});
				} else {
					LogError("Could not find target IL code in ApplyNpcRelationshipEffect");
				}
			};
			IL_ShopHelper.ApplyBiomeRelationshipEffect += (il) => {
				ILCursor c = new(il);
				if (c.TryGotoNext(MoveType.After,
					i => i.MatchCall(typeof(DefaultInterpolatedStringHandler), nameof(DefaultInterpolatedStringHandler.ToStringAndClear))
				)) {
					c.EmitLdarg1();
					c.EmitDelegate<Func<string, string, string>>((key, biomeName) => {
						if (biomeName is not null) {
							if (Main.LocalPlayer.talkNPC <= -1) return key;
							NPC talkNPC = Main.npc[Main.LocalPlayer.talkNPC];
							string talkNPCName = NPCID.Search.GetName(talkNPC.netID);
							string baseKey = "TownNPCMood_" + talkNPCName;
							string potentialKey = $"{baseKey}.{key}_{biomeName.Replace(".TownNPCDialogueName", "").Split('.')[^1]}";
							string potentialKey0 = $"Mods.Origins.NPCs.{talkNPCName}.{potentialKey}";
							if (Language.Exists(potentialKey0)) {
								return potentialKey0;
							}
							if (talkNPC.ModNPC is ModNPC modNPC) {
								if (talkNPC.ModNPC.Mod is Origins) return key;
								potentialKey = modNPC.GetLocalizationKey(potentialKey);
							}
							if (Language.Exists(potentialKey)) {
								return potentialKey;
							}
						}
						return key;
					});
				} else {
					LogError("Could not find target IL code in ApplyBiomeRelationshipEffect");
				}
			};
			IL_ShopHelper.AddHappinessReportText += (il) => {
				ILCursor c = new(il);
				if (c.TryGotoNext(MoveType.Before,
					i => i.MatchStloc0()
				)) {
					c.EmitLdarg1();
					c.EmitDelegate<Func<string, string, string>>((keyBase, key) => {
						if (key.StartsWith("Mods.")) {
							return "";
						}
						return keyBase + ".";
					});
				} else {
					LogError("Could not find target IL code in AddHappinessReportText");
				}
				c.Index++;
				MethodInfo concat2 = typeof(String).GetMethod(nameof(String.Concat), BindingFlags.Public | BindingFlags.Static, [typeof(String), typeof(String)]);
				while (c.TryGotoNext(MoveType.Before,
				   i => i.MatchStloc0()
				)) {
					c.EmitLdstr(".");
					c.EmitCall(concat2);
					c.Index++;
				}
				if (c.TryGotoNext(MoveType.Before,
				   i => i.MatchLdloc0(),
				   i => i.MatchLdstr("."),
				   i => i.MatchLdarg1(),
				   i => i.MatchCall<String>(nameof(String.Concat))
				)) {
					c.Index++;
					c.Remove();
					c.Index++;
					c.Remove();
					c.EmitCall(concat2);
					/*ILLabel label = c.DefineLabel();
					c.EmitBr(label);
					c.GotoNext(MoveType.After, i => i.MatchCall<String>("Concat"));
					c.EmitLdloc0();
					c.EmitLdarg1();
					c.EmitDelegate<Func<string, string, string>>((keyBase, key) => {
						if (key.StartsWith("Mods.")) {
							return key;
						}
						return keyBase + "." + key;
					});*/
				} else {
					LogError("Could not find target IL code in AddHappinessReportText");
				}
			};
			On_NPC.NPCLoot += (orig, self) => {
				if (self.GetGlobalNPC<OriginGlobalNPC>().transformingThroughDeath) {
					NPCLoader.OnKill(self);
				} else {
					orig(self);
				}
			};
			chanceNumerators = [];
			foreach (Mod mod in ModLoader.Mods) {
				if (mod.Code is null) continue;
				foreach (Type type in from t in AssemblyManager.GetLoadableTypes(mod.Code) where t.IsAssignableTo(typeof(IItemDropRule)) select t) {
					FieldInfo chanceNumerator = type.GetField("chanceNumerator", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (chanceNumerator != null) {
						chanceNumerators.Add(type, new(chanceNumerator));
					}
				}
			}
			On_ItemDropResolver.ResolveRule += (orig, self, rule, info) => {
				currentChanceNumerator = 1;
				if (chanceNumerators.TryGetValue(rule.GetType(), out var chanceNumerator)) {
					currentChanceNumerator = chanceNumerator.GetValue(rule);
				}
				ItemDropAttemptResult result = orig(self, rule, info);
				currentChanceNumerator = 1;
				return result;
			};
			IL_PlayerStatsSnapshot.ctor += IL_PlayerStatsSnapshot_ctor;
			On_PlayerStatsSnapshot.ctor += On_PlayerStatsSnapshot_ctor;
			IL_WaterfallManager.GetAlpha += FixWrongWaterfallAlpha_IL;
			On_FilterManager.BeginCapture += On_FilterManager_BeginCapture;
			On_TileLightScanner.ApplyHellLight += On_TileLightScanner_ApplyHellLight;
			On_Main.DrawBlack += On_Main_DrawBlack;
			On_Item.CloneDefaults += On_Item_CloneDefaults;
			On_Lighting.AddLight_int_int_float_float_float += On_Lighting_AddLight_int_int_float_float_float;
			On_Dust.NewDust += On_Dust_NewDust;
			On_Gore.NewGore_IEntitySource_Vector2_Vector2_int_float += On_Gore_NewGore_IEntitySource_Vector2_Vector2_int_float;
			On_SoundEngine.PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback += On_SoundEngine_PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback;
			On_Main.DrawSocialMediaButtons += (orig, color, upBump) => {
				orig(color, upBump);
				if (loadingWarnings.Count != 0) {
					int titleLinks = Main.tModLoaderTitleLinks.Count;
					Vector2 anchorPosition = new(18f, 18f);
					Rectangle rectangle = new((int)anchorPosition.X, (int)anchorPosition.Y, 30, 30);
					float scaleValue = MathHelper.Lerp(0.5f, 1f, Main.mouseTextColor / 255f);
					ChatManager.DrawColorCodedStringWithShadow(
						Main.spriteBatch,
						FontAssets.DeathText.Value,
						"!",
						rectangle.Left() - FontAssets.DeathText.Value.MeasureString("!") * new Vector2(0f, 0.25f),
						Color.Yellow,
						Color.OrangeRed,
						0,
						Vector2.Zero,
						new Vector2(scaleValue)
					);
					if (rectangle.Contains(Main.mouseX, Main.mouseY)) {
						UICommon.TooltipMouseText(Language.GetText("Mods.Origins.Warnings.WarningPrefaceText") + "\n" + string.Join('\n', loadingWarnings));
					}
				}
			};
			On_Player.FigureOutWhatToPlace += On_Player_FigureOutWhatToPlace;
			On_WorldGen.CheckTight += On_WorldGen_CheckTight;
			On_NPC.SpawnAllowed_ArmsDealer += (orig) => {
				if (orig()) return true;
				int harpoon = MC.ItemType<Harpoon>();
				int scrap = MC.ItemType<Scrap>();
				foreach (Player player in Main.ActivePlayers) {
					for (int j = 0; j < 58; j++) {
						Item item = player.inventory[j];
						if (item != null && item.stack > 0 && (item.ammo == harpoon || item.useAmmo == harpoon || item.ammo == scrap || item.useAmmo == scrap)) {
							return true;
						}
					}
				}
				return false;
			};
			IL_Player.SlopeDownMovement += IL_Player_SlopeDownMovement;
			//IL_Player.Update += IL_Player_SlopeDownMovement;
			On_Collision.StepDown += On_Collision_StepDown;
			IL_Player.CheckDrowning += IL_Player_CheckDrowning;
			On_Projectile.StatusPvP += (orig, self, playerIndex) => {
				orig(self, playerIndex);
				Player player = Main.player[playerIndex];
				player.OriginPlayer().OnHitByAnyProjectile(self);
				if (self.type == Laser_Tag_Laser.ID) Laser_Tag_Laser.OnHitPvP(self, player);
			};
			IL_Sandstorm.EmitDust += IL_Sandstorm_EmitDust;
			IL_Sandstorm.ShouldSandstormDustPersist += IL_Sandstorm_ShouldSandstormDustPersist;
			On_Player.CanNPCBeHitByPlayerOrPlayerProjectile += (orig, self, npc, projectile) => {
				if (projectile is not null && projectile.type == Fresh_Meat_Dog.ID && !self.dontHurtCritters) return true;
				return orig(self, npc, projectile);
			};
			On_NPC.TryTrackingTarget += On_NPC_TryTrackingTarget;
			On_NPC.SetTargetTrackingValues += On_NPC_SetTargetTrackingValues;
			try {
				MonoModHooks.Add(typeof(UICommon).Assembly.GetType("Terraria.ModLoader.UI.Interface").GetMethod("AddMenuButtons", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static), (orig_AddMenuButtons orig, Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons) => {
					orig(main, selectedMenu, buttonNames, buttonScales, ref offY, ref spacing, ref buttonIndex, ref numButtons);
					workshopMenuIndex = buttonIndex - 1;
				});
				IL_Main.DrawMenu += IL_Main_DrawMenu;
			} catch (Exception ex) {
				LogLoadingWarning(Language.GetText("Mods.Origins.Warnings.AddMenuButtonsThing"));
				instance.Logger.Error("Error while hooking into Terraria.ModLoader.UI.Interface.AddMenuButtons: ", ex);
			}
			IL_Player.ItemCheck_ManageRightClickFeatures += IL_Player_ItemCheck_ManageRightClickFeatures;
			IL_NewMultiplayerClosePlayersOverlay.Draw += Laser_Tag_Hud.IL_NewMultiplayerClosePlayersOverlay_Draw;
			MonoModHooks.Add(
				typeof(CombinedHooks).GetMethod(nameof(CombinedHooks.ModifyWeaponDamage), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static),
				(orig_ModifyWeaponDamage orig, Player player, Item item, ref StatModifier damage) => {
					orig(player, item, ref damage);
					damage.Base *= FlatDamageMultiplier[item.type];
					damage.Flat *= FlatDamageMultiplier[item.type];
				}
			);
			//IL_Main.DoDraw += Defiled_Wastelands_Mod_Menu.EnableShaderOnMenu;
			On_ScreenShaderData.Apply += (On_ScreenShaderData.orig_Apply orig, ScreenShaderData self) => {
				try {
					if (self.Shader is not null) orig(self);
				} catch(NullReferenceException) { }
			};
			MonoModHooks.Add(
				typeof(ModLoader).GetMethod("Reload", BindingFlags.NonPublic | BindingFlags.Static),
				(Action orig) => {
					typeof(FilterManager).GetField("_activeFilterCount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Filters.Scene, 0);
					((LinkedList<Filter>)typeof(FilterManager).GetField("_activeFilters", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Filters.Scene)).Clear();
					orig();
				}
			);
			IL_PlayerDrawHelper.SetShaderForData += il => {
				ILCursor c = new(il);
				ILLabel label = c.MarkLabel();
				c.MoveBeforeLabels();
				c.EmitLdarg2();
				c.EmitLdfld(typeof(DrawData).GetField(nameof(DrawData.texture)));
				c.EmitBrtrue(label);
				c.EmitRet();
			};
			On_Player.QuickBuff_ShouldBotherUsingThisBuff += BrothBase.On_Player_QuickBuff_ShouldBotherUsingThisBuff;
			On_Player.AddBuff_RemoveOldMeleeBuffsOfMatchingType += BrothBase.On_Player_AddBuff_RemoveOldMeleeBuffsOfMatchingType;
			On_Main.CalculateWaterStyle += (orig, ignoreFountains) => {
				int chosenStyle = Main.LocalPlayer.CurrentSceneEffect.waterStyle.value;
				if (chosenStyle == ModContent.GetInstance<Riven_Water_Style>().Slot || chosenStyle == ModContent.GetInstance<Brine_Water_Style>().Slot) return chosenStyle;
				return orig(ignoreFountains);
			};
			On_Mount.Dismount += Ravel_Mount.On_Mount_Dismount;
			IL_ShopHelper.ProcessMood += il => {
				ILCursor c = new(il);
				ILLabel label = default;
				if (c.TryGotoNext(MoveType.After,
					i => i.MatchLdloc(out _),
					i => i.MatchLdcI4(3),
					i => i.MatchBle(out label)
				)) {
					c.EmitLdarg2();
					c.EmitDelegate((NPC npc) => {
						if (BirthdayParty.PartyIsUp && IsFineWithCrowdedParties[npc.type]) return true;
						return false;
					});
					c.EmitBrtrue(label);
				} else {
					LogError($"Could not find npcsWithinHouse > 3 comparison in ShopHelper.ProcessMood");
				}
			};
			IL_WorldGen.SpawnThingsFromPot += IL_WorldGen_SpawnThingsFromPot;
			IL_Condition.PlayerCarriesItem += il => {
				ILCursor c = new(il);
				c.GotoNext(MoveType.Before, i => i.MatchNewobj<Condition>());
				c.EmitLdarg0();
				c.EmitDelegate<Func<Func<bool>, int, Func<bool>>>((orig, type) => {
					if (type == ItemID.FlareGun) return () => orig() || Main.LocalPlayer.HasItem(ModContent.ItemType<Flare_Launcher>());
					return orig;
				});
			};
			IL_Player.GetWeaponDamage += il => {
				ILCursor c = new(il);
				c.GotoNext(MoveType.Before, i => i.MatchCall<StatModifier>(nameof(StatModifier.ApplyTo)));
				MonoModMethods.SkipPrevArgument(c);
				c.EmitLdarg1();
				c.EmitDelegate<Modify_ApplyTo>((ref StatModifier modifier, Item item) => {
					modifier = modifier.Scale(DamageBonusScale[item.type]);
					return ref modifier;
				});
			};
			On_Item.TryGetPrefixStatMultipliersForItem += (On_Item.orig_TryGetPrefixStatMultipliersForItem orig, Item self, int rolledPrefix, out float dmg, out float kb, out float spd, out float size, out float shtspd, out float mcst, out int crt) => {
				if (orig(self, rolledPrefix, out dmg, out kb, out spd, out size, out shtspd, out mcst, out crt)) {
					if (dmg != 1 && DamageBonusScale[self.type] == 0) return false;
					dmg = 1 + (dmg - 1) * DamageBonusScale[self.type];
					return true;
				}
				return false;
			};
			On_Player.DoesPickTargetTransformOnKill += (orig, self, hitCounter, damage, x, y, pickPower, bufferIndex, tileTarget) => {
				if (orig(self, hitCounter, damage, x, y, pickPower, bufferIndex, tileTarget)) return true;
				if (hitCounter.AddDamage(bufferIndex, damage, updateAmount: false) >= 100 && TileTransformsOnKill[tileTarget.TileType]) return true;
				return false;
			};
			On_TrackGenerator.IsLocationInvalid += (orig, x, y) => {
				if (orig(x, y)) return true;
				Tile tile = Main.tile[x, y];
				if ((tile.HasTile && TileBlocksMinecartTracks[tile.TileType]) || WallBlocksMinecartTracks[tile.WallType]) return true;
				return false;
			};
			if (ModLoader.GetMod(nameof(BetterDialogue)).Version == new Version(1, 1, 6, 1)) MonoModHooks.Modify(typeof(BetterDialogue.BetterDialogue).Assembly.GetType("BetterDialogue.UI.DialogueCycleButtonUI").GetMethod("Draw", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static), 
				il => new ILCursor(il)
				.EmitLdsfld(typeof(Main).GetField(nameof(Main.editChest)))
				.EmitBrfalse(MonoFuckery.DefineLabel(il, out ILLabel label))
				.EmitRet()
				.MarkLabel(label)
			);
			IL_Player.ItemCheck_OwnerOnlyCode += Eitrite_Gun_Magazine.IL_Player_ItemCheck_OwnerOnlyCode;
			IL_NPC.SpawnNPC_CheckToSpawnUndergroundFairy += Fairy_Lotus.IL_NPC_SpawnNPC_CheckToSpawnUndergroundFairy;
			MonoModHooks.Add(typeof(ModConfig).Assembly.GetType("Terraria.ModLoader.Config.ConfigManager").GetMethod("Save", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static), (Action<ModConfig> orig, ModConfig pendingConfig) => {
				if (pendingConfig is OriginClientConfig clientConfig) OriginClientConfig.forceReloadLanguage = (clientConfig.DebugMenuButton.ForceAprilFools != OriginClientConfig.Instance.DebugMenuButton.ForceAprilFools);
				orig(pendingConfig);
				if (pendingConfig is OriginConfig realPendingConfig) realPendingConfig.SaveToFile();
			});
			MonoModHooks.Add(typeof(ModConfig).Assembly.GetType("Terraria.ModLoader.Config.ConfigManager").GetMethod("Load", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static), (Action<ModConfig> orig, ModConfig pendingConfig) => {
				orig(pendingConfig);
				if (pendingConfig is OriginConfig realPendingConfig) realPendingConfig.LoadFromFile();
			});
			MonoModHooks.Add(typeof(ModConfig).Assembly.GetType("Terraria.ModLoader.Config.ConfigManager").GetMethod("GeneratePopulatedClone", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static), (Func<ModConfig, ModConfig> orig, ModConfig original) => {
				ModConfig config = orig(original);
				if (original is OriginConfig originConfig && config is OriginConfig newConfig) originConfig.CloneTo(newConfig);
				return config;
			});
			On_Player.Hurt_HurtInfo_bool += On_Player_Hurt_HurtInfo_bool;
			On_SoundEngine.PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback += On_SoundEngine_PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback1;
			On_Item.FitsAmmoSlot += (orig, self) => {
				return orig(self) || self.ammo == ItemID.Grenade;
			};
			IL_Player.CheckDrowning += Toxic_Shock_Debuff.IL_Player_CheckDrowning;
			IL_Player.WaterCollision += Abyssal_Anchor.IL_Player_WaterCollision;
			IL_Player.ApplyTouchDamage += Trap_Charm.IL_Player_ApplyTouchDamage;
			/*
			FastFieldInfo<SoundEffect, FAudio.FAudioBuffer> _handle = new("handle", BindingFlags.NonPublic);
			FastFieldInfo<SoundEffect, ushort> _channels = new("channels", BindingFlags.NonPublic);
			FastFieldInfo<SoundEffect, uint> _sampleRate = new("sampleRate", BindingFlags.NonPublic);
			MonoModHooks.Add(typeof(SoundEffect).GetMethod(nameof(SoundEffect.CreateInstance)), (Func<SoundEffect, SoundEffectInstance> orig, SoundEffect self) => {
				FAudio.FAudioBuffer handle = _handle.GetValue(self);
				int sampleRate = (int)_sampleRate.GetValue(self);
				byte[] bytes = new byte[handle.AudioBytes];
				Marshal.Copy(handle.pAudioData, bytes, 0, bytes.Length);
				ushort[] buffer = new ushort[bytes.Length / 2];
				Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);
				for (int i = 0; i < buffer.Length; i++) {
					//buffer[i] = (ushort)(buffer[i] / 2);//(ushort)(buffer[i] * 1.1f);
					//bytes[i] = (byte)(bytes[i] / 2);//Math.Min(buffer[i], (ushort)(ushort.MaxValue / 8));
				}
				Buffer.BlockCopy(buffer, 0, bytes, 0, bytes.Length);
				SoundEffect newEffect = new(bytes, sampleRate, (AudioChannels)_channels.GetValue(self));
				return orig(newEffect);//orig(self);
			});//*/
			On_Player.UpdateJumpHeight += On_Player_UpdateJumpHeight;
			On_Player.AddBuff += On_Player_AddBuff;
			IL_Player.UpdateLifeRegen += Akaliegis.IL_Player_UpdateLifeRegen;
			On_Player.DashMovement += (orig, self) => {
				try {
					processingDash = true;
					orig(self);
				} finally {
					processingDash = false;
				}
			};
			try {
				IL_Player.DashMovement += il => {
					ILCursor c = new(il);
					c.GotoNext(MoveType.After,
						i => i.MatchLdcR4(30f)
					);
					c.EmitLdarg0();
					c.EmitDelegate(static (float damage, Player player) => player?.OriginPlayer()?.dashBaseDamage ?? damage);
				};
			} catch (Exception e) {
				if (Origins.LogLoadingILError("SetDashBaseDamage", e)) throw;
			}
			IL_WaterShaderData.DrawWaves += Brine_Pool_NPC.DisableRipples;
			On_Player.SlopingCollision += (On_Player.orig_SlopingCollision orig, Player self, bool fallThrough, bool ignorePlats) => {
				Debugging.LogFirstRun(self.SlopingCollision);
				float startY = self.position.Y;
				orig(self, fallThrough, ignorePlats);
				if (self.position.Y != startY) self.OriginPlayer().onSlope = true;
			};
			On_Player.UpdateTouchingTiles += (orig, self) => {
				Debugging.LogFirstRun(self.UpdateTouchingTiles);
				orig(self);
			};
			On_PressurePlateHelper.UpdatePlayerPosition += (orig, self) => {
				Debugging.LogFirstRun(PressurePlateHelper.UpdatePlayerPosition);
				if (self.OriginPlayer().weakShimmer) return;
				orig(self);
			};
			try {
				IL_Collision.TileCollision += IL_Collision_TileCollision_OffsetBookcases;
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(IL_Collision_TileCollision_OffsetBookcases), e)) throw;
			}
			/*try {
				IL_ActiveSound.Update += EnablePocketDimensionAmbienceWhenPaused;
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(EnablePocketDimensionAmbienceWhenPaused), e)) throw;
			}*/
			MonoModHooks.Add(typeof(Player).GetProperty(nameof(Player.ShoppingZone_AnyBiome)).GetMethod, (orig_ShoppingZone_AnyBiome orig, Player self) => {
				return orig(self) || self.InModBiome<Defiled_Wastelands>() || self.InModBiome<Riven_Hive>();
			});
		}
		delegate bool orig_ShoppingZone_AnyBiome(Player self);
		delegate bool hook_ShoppingZone_AnyBiome(orig_ShoppingZone_AnyBiome orig, Player self);

		/*static void EnablePocketDimensionAmbienceWhenPaused(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.After,
				i => i.MatchLdsfld<Main>(nameof(Main.gameInactive))
			);
			c.EmitLdarg0();
			c.EmitDelegate(static (bool gameInactive, ActiveSound sound) => gameInactive && !sound.Style.SoundPath.StartsWith("Origins/Sounds/Custom/Ambience"));
		}*/

		internal static void IL_Collision_TileCollision_OffsetBookcases(ILContext il) {
			ILCursor c = new(il);
			int tile = -1;
			int pos = -1;
			int height = -1;
			c.GotoNext(MoveType.After,
				i => i.MatchLdloca(out tile),
				i => i.MatchCall<Tile>("halfBrick"),
				i => i.MatchBrfalse(out _),
				i => i.MatchLdloca(out pos),        //IL_01b8: ldloca.s 12
				i => i.MatchLdflda<Vector2>("Y"),   //IL_01ba: ldflda float32[FNA]Microsoft.Xna.Framework.Vector2::Y
				i => i.MatchDup(),                  //IL_01bf: dup
				i => i.MatchLdindR4(),              //IL_01c0: ldind.r4
				i => i.MatchLdcR4(8),               //IL_01c1: ldc.r4 8
				i => i.MatchAdd(),                  //IL_01c6: add
				i => i.MatchStindR4(),              //IL_01c7: stind.r4
				i => i.MatchLdloc(out height),      //IL_01c8: ldloc.s 15
				i => i.MatchLdcI4(8),               //IL_01ca: ldc.i4.8
				i => i.MatchSub(),                  //IL_01cb: sub
				i => i.MatchStloc(out _)            //IL_01cc: stloc.s 15
			);
			c.MoveAfterLabels();
			c.EmitLdloc(tile);
			c.EmitLdloca(pos);
			c.EmitLdloca(height);
			c.EmitDelegate((Tile tile, ref Vector2 pos, ref int height) => {
				OriginsSets.Tiles.MultitileCollisionOffset.GetIfInRange(tile.TileType)?.Invoke(tile, ref pos.Y, ref height);
			});
		}

		public static bool processingDash = false;
		private static void On_Player_AddBuff(On_Player.orig_AddBuff orig, Player self, int type, int timeToAdd, bool quiet, bool foodHack) {
			if (self.TryGetModPlayer(out Nurse_Assimilation_Dialog assTracker)) assTracker.GotDebuffFromAssimilation[type] = AssimilationDebuff.isUpdatingAssimilation;
			orig(self, type, timeToAdd, quiet, foodHack);
			if (Main.debuff[type]) {
				OriginPlayer originPlayer = self.OriginPlayer();
				if (originPlayer.extremophileSet && originPlayer.extremophileSetTime <= 0) {
					originPlayer.extremophileSetTime++;
				}
			}
		}

		private void On_Player_UpdateJumpHeight(On_Player.orig_UpdateJumpHeight orig, Player self) {
			orig(self);
			if (self.jumpBoost && self.OriginPlayer().heliumTank) {
				Player.jumpHeight = (int)(Player.jumpHeight * 1.2f);
			}
		}

		static bool shouldDoHeliumSound = false;
		static float heliumSoundPitch = 0f;
		private static ReLogic.Utilities.SlotId On_SoundEngine_PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback1(On_SoundEngine.orig_PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback orig, ref SoundStyle style, Vector2? position, SoundUpdateCallback updateCallback) {
			if (shouldDoHeliumSound) {
				SoundStyle styleCopy = style.WithPitchOffset(heliumSoundPitch);
				return orig(ref styleCopy, position, updateCallback);
			} else {
				return orig(ref style, position, updateCallback);
			}
		}
		private static void On_Player_Hurt_HurtInfo_bool(On_Player.orig_Hurt_HurtInfo_bool orig, Player self, Player.HurtInfo info, bool quiet) {
			try {
				if (!self.stoned && !self.frostArmor && !self.boneArmor) {
					OriginPlayer originPlayer = self.OriginPlayer();
					shouldDoHeliumSound = originPlayer.heliumTankSqueak;
					heliumSoundPitch = originPlayer.heliumTankStrength;
				}
				orig(self, info, quiet);
			} finally {
				shouldDoHeliumSound = false;
			}
		}

		delegate ref StatModifier Modify_ApplyTo(ref StatModifier modifier, Item item);
		private static void IL_WorldGen_SpawnThingsFromPot(ILContext il) {
			ILCursor c = new(il);
			int playerLoc = -1;
			int countLoc = -1;
			int typeLoc = -1;
			ILLabel label = null;
			int minConst = -1;
			int maxConst = -1;
			if (c.TryGotoNext(MoveType.After,
				i => i.MatchLdloc(out playerLoc),
				i => i.MatchCallOrCallvirt<Player>("get_" + nameof(Player.ZoneGlowshroom)),
				i => i.MatchBrfalse(out label),
				i => i.MatchLdloc(out countLoc),
				i => i.MatchCallOrCallvirt<Main>("get_" + nameof(Main.rand)),
				i => i.MatchLdcI4(out minConst),
				i => i.MatchLdcI4(out maxConst),
				i => i.MatchCallOrCallvirt<UnifiedRandom>(nameof(UnifiedRandom.Next)),
				i => i.MatchAdd(),
				i => i.MatchStloc(countLoc),
				i => i.MatchLdcI4(ItemID.MushroomTorch),
				i => i.MatchStloc(out typeLoc)
			)) {
				c.GotoLabel(label, MoveType.AfterLabel);
				c.EmitLdloc(playerLoc);
				c.EmitLdloca(countLoc);
				c.EmitLdcI4(minConst);
				c.EmitLdcI4(maxConst);
				c.EmitLdloca(typeLoc);
				c.EmitDelegate((Player player, ref int count, int minBonus, int maxBonus, ref int type) => {
					if (player.InModBiome<Defiled_Wastelands>()) {
						count += Main.rand.Next(minBonus, maxBonus);
						type = ModContent.ItemType<Defiled_Torch>();
					} else if (player.InModBiome<Riven_Hive>()) {
						count += Main.rand.Next(minBonus, maxBonus);
						type = ModContent.ItemType<Riven_Torch>();
					}
				});
			} else {
				LogError($"Could not find mushroom torch drop in WorldGen.SpawnThingsFromPot");
			}
		}

		delegate void orig_ModifyWeaponDamage(Player player, Item item, ref StatModifier damage);
		delegate void hook_ModifyWeaponDamage(orig_ModifyWeaponDamage orig, Player player, Item item, ref StatModifier damage);
		private static void IL_Player_ItemCheck_ManageRightClickFeatures(ILContext il) {
			ILCursor c = new(il);
			if (c.TryGotoNext(MoveType.After,
				i => i.MatchLdsfld<Main>(nameof(Main.myPlayer)),
				i => i.MatchLdarg0(),
				i => i.MatchLdfld<Entity>(nameof(Entity.whoAmI))
			)) {
				c.EmitDelegate((int myPlayer, int whoAmI) => myPlayer == whoAmI || ItemsThatAllowRemoteRightClick[Main.player[whoAmI].HeldItem.type]);
				c.EmitBrfalse(c.Next.Operand as ILLabel);
				c.Remove();
			} else {
				LogError($"Could not find Main.myPlayer == whoAmI comparison in Player.ItemCheck_ManageRightClickFeatures");
			}
		}

		static int workshopMenuIndex = -1;
		delegate void orig_AddMenuButtons(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons);
		delegate void hook_AddMenuButtons(orig_AddMenuButtons orig, Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons);
		static bool updatedHiddenSuggestions = true;
		private void IL_Main_DrawMenu(ILContext il) {
			ILCursor c = new(il);
			int i = -1;
			int l6 = -1;
			int l182 = -1;
			int l20 = -1;
			int l5 = -1;
			int l7 = -1;
			int l183 = -1;
			int l174 = -1;
			int l22 = -1;
			int l19 = -1;
			c.GotoNext(MoveType.After,
				ins => ins.MatchLdsfld<Main>(nameof(Main.spriteBatch)),
				ins => ins.MatchLdsfld(typeof(FontAssets), nameof(FontAssets.DeathText)),
				ins => ins.MatchCallOrCallvirt(out _),
				ins => ins.MatchLdloc(out _),
				ins => ins.MatchLdloc(out i),
				ins => ins.MatchLdelemRef(),
				ins => ins.MatchLdloc(out l6),
				ins => ins.MatchLdloc(out l182),
				ins => ins.MatchAdd(),
				ins => ins.MatchLdloc(out l20),
				ins => ins.MatchLdloc(i),
				ins => ins.MatchLdelemI4(),
				ins => ins.MatchAdd(),
				ins => ins.MatchConvR4(),
				ins => ins.MatchLdloc(out l5),
				ins => ins.MatchLdloc(out l7),
				ins => ins.MatchLdloc(i),
				ins => ins.MatchMul(),
				ins => ins.MatchAdd(),
				ins => ins.MatchLdloc(out l183),
				ins => ins.MatchAdd(),
				ins => ins.MatchConvR4(),
				ins => ins.MatchLdloc(out l174),
				ins => ins.MatchLdfld<Vector2>(nameof(Vector2.Y)),
				ins => ins.MatchLdloc(out l22),
				ins => ins.MatchLdloc(i),
				ins => ins.MatchLdelemR4(),
				ins => ins.MatchMul(),
				ins => ins.MatchAdd(),
				ins => ins.MatchLdloc(out l19),
				ins => ins.MatchLdloc(i),
				ins => ins.MatchLdelemI4(),
				ins => ins.MatchConvR4(),
				ins => ins.MatchAdd(),
				ins => ins.MatchNewobj<Vector2>(),
				ins => ins.MatchLdloc(out _),
				ins => ins.MatchLdcR4(0.0f),
				ins => ins.MatchLdloc(out _),
				ins => ins.MatchLdloc(out _),
				ins => ins.MatchLdcI4(0),
				ins => ins.MatchLdcR4(0.0f),
				ins => ins.MatchCall(typeof(DynamicSpriteFontExtensionMethods), nameof(DynamicSpriteFontExtensionMethods.DrawString))
			);
			
			c.EmitLdloc(i);
			c.EmitLdloc(l6);
			c.EmitLdloc(l182);
			c.EmitLdloc(l20);
			c.EmitLdloc(l5);
			c.EmitLdloc(l7);
			c.EmitLdloc(l183);
			c.EmitLdloc(l174);
			c.EmitLdloc(l22);
			c.EmitLdloc(l19);
			c.EmitDelegate(
				static (int i, int l6, int l182, int[] l20, int l5, int l7, int l183, Vector2 lineOrigin, float[] l22, int[] l19) => {
					if (i != workshopMenuIndex) return;
					if (updatedHiddenSuggestions) {// let players disable recommendations they've seen
						OriginsModIntegrations.compatRecommendations.RemoveAll(loc => DebugConfig.Instance.IgnoredCompatibilitySuggestions.Contains(loc.Key));
						OriginsModIntegrations.conditionalCompatRecommendations.RemoveAll(loc => DebugConfig.Instance.IgnoredCompatibilitySuggestions.Contains(loc.text.Key));
					}
					bool anyErrors = OriginsModIntegrations.compatErrors.Count != 0;
					if (!anyErrors && OriginsModIntegrations.compatRecommendations.Count == 0 && OriginsModIntegrations.conditionalCompatRecommendations.Where(x => x.condition()).Count() == 0) return;
					Vector2 anchorPosition = new(l6 + l182 + l20[i] + lineOrigin.X, l5 + l7 * i + l183 + lineOrigin.Y * l22[i] * 0.5f + l19[i]);
					Rectangle rectangle = new((int)anchorPosition.X, (int)anchorPosition.Y, 30, 30);
					float scaleValue = MathHelper.Lerp(0.5f, 0.75f, Main.mouseTextColor / 255f);
					ChatManager.DrawColorCodedStringWithShadow(
						Main.spriteBatch,
						FontAssets.DeathText.Value,
						"!",
						rectangle.Left() - FontAssets.DeathText.Value.MeasureString("!") * new Vector2(0f, 0.25f),
						Color.Yellow,
						anyErrors ? Color.OrangeRed : Color.DarkGoldenrod,
						0,
						Vector2.Zero,
						new Vector2(scaleValue)
					);
					if (rectangle.Contains(Main.mouseX, Main.mouseY)) {
						string ignoreAll = "";
						if (OriginsModIntegrations.compatRecommendations.Count != 0 || OriginsModIntegrations.conditionalCompatRecommendations.Where(x => x.condition()).Count() != 0) {
							ignoreAll = "\n" + Language.GetTextValue("Mods.Origins.ModCompatNotes.ClearAll");
						}
						UICommon.TooltipMouseText(string.Join("\n",
							OriginsModIntegrations.compatErrors.Select(x => $"[c/ff0000:{x.Value}]")
							.Concat(OriginsModIntegrations.compatRecommendations.Select(x => x.Value))
							.Concat(OriginsModIntegrations.conditionalCompatRecommendations.Where(x => x.condition()).Select(x => x.text.Value))
						) + ignoreAll);
						if (Main.mouseRight && Main.mouseRightRelease) {
							for (int j = 0; j < OriginsModIntegrations.compatRecommendations.Count; j++) {
								DebugConfig.Instance.IgnoredCompatibilitySuggestions.Add(OriginsModIntegrations.compatRecommendations[j].Key);
							}
							for (int j = 0; j < OriginsModIntegrations.conditionalCompatRecommendations.Count; j++) {
								if (OriginsModIntegrations.conditionalCompatRecommendations[j].condition()) {
									DebugConfig.Instance.IgnoredCompatibilitySuggestions.Add(OriginsModIntegrations.conditionalCompatRecommendations[j].text.Key);
								}
							}
							OriginClientConfig.Instance.Save();
							updatedHiddenSuggestions = true;
						}
					}
				}
			);
			updatedHiddenSuggestions = true;
			//*/
		}

		private static void On_NPC_SetTargetTrackingValues(On_NPC.orig_SetTargetTrackingValues orig, NPC self, bool faceTarget, float realDist, int tankTarget) {
			if (!self.HasValidTarget && NPCOnlyTargetInBiome.ContainsKey(self.type)) {
				return;
			}
			orig(self, faceTarget, realDist, tankTarget);
		}

		private static void On_NPC_TryTrackingTarget(On_NPC.orig_TryTrackingTarget orig, NPC self, ref float distance, ref float realDist, ref bool t, ref int tankTarget, int j) {
			if (NPCOnlyTargetInBiome.TryGetValue(self.type, out ModBiome biome) && !Main.player[j].InModBiome(biome)) return;
			orig(self, ref distance, ref realDist, ref t, ref tankTarget, j);
		}

		private void IL_Sandstorm_ShouldSandstormDustPersist(ILContext il) {
			ILCursor c = new(il);
			ILLabel succeed = default;
			c.GotoNext(MoveType.After,
				i => i.MatchLdsfld<Main>(nameof(Main.bgStyle)),
				i => i.MatchLdcI4(2),
				i => i.MatchBeq(out succeed),
				i => i.MatchLdsfld<Main>(nameof(Main.bgStyle)),
				i => i.MatchLdcI4(5),
				i => i.MatchBneUn(out _)
			);
			c.Index--;
			c.EmitBeq(succeed);
			c.EmitDelegate(() => Main.LocalPlayer.InModBiome<Defiled_Wastelands_Desert>() || Main.LocalPlayer.InModBiome<Riven_Hive_Desert>());
			c.EmitBrfalse((ILLabel)c.Next.Operand);
			c.Remove();
		}

		private void IL_Sandstorm_EmitDust(ILContext il) {
			ILCursor c = new(il);
			int local = -1;
			c.GotoNext(MoveType.After,
				i => i.MatchLdloc(out local),
				i => i.MatchLdcI4(213), i => i.MatchLdcI4(196), i => i.MatchLdcI4(197), i => i.MatchLdcI4(180), i => i.MatchNewobj<Color>(),
				i => i.MatchLdsfld<Main>(nameof(Main.SceneMetrics)),
				i => i.MatchLdcI4(TileID.Pearlsand),
				i => i.MatchCallOrCallvirt<SceneMetrics>(nameof(SceneMetrics.GetTileCount)),
				i => i.MatchLdsfld<Main>(nameof(Main.SceneMetrics)),
				i => i.MatchLdcI4(TileID.HallowSandstone),
				i => i.MatchCallOrCallvirt<SceneMetrics>(nameof(SceneMetrics.GetTileCount)),
				i => i.MatchAdd(),
				i => i.MatchLdsfld<Main>(nameof(Main.SceneMetrics)),
				i => i.MatchLdcI4(TileID.HallowHardenedSand),
				i => i.MatchCallOrCallvirt<SceneMetrics>(nameof(SceneMetrics.GetTileCount)),
				i => i.MatchAdd(),
				i => i.MatchConvR8(),
				i => i.MatchCallOrCallvirt<WeightedRandom<Color>>(nameof(WeightedRandom<Color>.Add))
			);
			c.EmitLdloc(local);
			c.EmitDelegate<Action<WeightedRandom<Color>>>(weightedRandom => {
				weightedRandom.Add(new Color(113, 113, 113, 180), Main.SceneMetrics.GetTileCount((ushort)MC.TileType<Defiled_Sand>()) + Main.SceneMetrics.GetTileCount((ushort)MC.TileType<Defiled_Sandstone>()) + Main.SceneMetrics.GetTileCount((ushort)MC.TileType<Hardened_Defiled_Sand>()));
				weightedRandom.Add(new Color(189, 195, 195, 180), Main.SceneMetrics.GetTileCount((ushort)MC.TileType<Silica>()) + Main.SceneMetrics.GetTileCount((ushort)MC.TileType<Brittle_Quartz>()) + Main.SceneMetrics.GetTileCount((ushort)MC.TileType<Quartz>()));
			});
		}

		private void IL_Player_CheckDrowning(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.After,
				i => i.MatchCallOrCallvirt<Collision>(nameof(Collision.DrownCollision))
			);
			c.EmitLdarg0();
			c.EmitDelegate<Func<bool, Player, bool>>((drowning, player) => drowning || player.OriginPlayer().forceDrown);
		}

		private void On_Collision_StepDown(On_Collision.orig_StepDown orig, ref Vector2 position, ref Vector2 velocity, int width, int height, ref float stepSpeed, ref float gfxOffY, int gravDir, bool waterWalk) {
			if (Main.CurrentPlayer.mount.Active && Main.CurrentPlayer.mount.Type == Indestructible_Saddle_Mount.ID) return;
			orig(ref position, ref velocity, width, height, ref stepSpeed, ref gfxOffY, gravDir, waterWalk);
		}

		private void IL_Player_SlopeDownMovement(ILContext il) {
			ILCursor c = new(il);
			ILLabel skip = default;
			c.GotoNext(MoveType.After,
				i => i.MatchLdarg0(),
				i => i.MatchLdfld<Player>(nameof(Player.mount)),
				i => i.MatchCallOrCallvirt<Mount>("get_" + nameof(Mount.Type)),
				i => i.MatchLdcI4(MountID.DarkMageBook),
				i => i.MatchBeq(out skip)
			);
			c.EmitLdarg0();
			c.EmitLdfld(typeof(Player).GetField(nameof(Player.mount)));
			c.EmitCallvirt(typeof(Mount).GetProperty(nameof(Mount.Type)).GetMethod);
			c.EmitCall(typeof(Indestructible_Saddle_Mount).GetProperty(nameof(Indestructible_Saddle_Mount.ID)).GetMethod);
			c.EmitBeq(skip);
		}

		private void On_WorldGen_CheckTight(On_WorldGen.orig_CheckTight orig, int x, int j) {
			if (OriginsGlobalTile.GetStalactiteTexture(x, j, Main.tile[x, j].TileFrameY, out _)) return;
			orig(x, j);
		}

		private void On_Player_FigureOutWhatToPlace(On_Player.orig_FigureOutWhatToPlace orig, Player self, Tile targetTile, Item sItem, out int tileToCreate, out int previewPlaceStyle, out bool? overrideCanPlace, out int? forcedRandom) {
			orig(self, targetTile, sItem, out tileToCreate, out previewPlaceStyle, out overrideCanPlace, out forcedRandom);
			if (TileLoader.GetTile(tileToCreate) is ICustomCanPlaceTile customCanPlaceTile) customCanPlaceTile.CanPlace(self, targetTile, sItem, ref tileToCreate, ref previewPlaceStyle, ref overrideCanPlace, ref forcedRandom);
		}

		static void FCEH(ILContext il) {
			ILCursor c = new(il);
			int msg = -1;
			if (c.TryGotoNext(i => i.MatchLdloc(out msg), i => i.MatchCallOrCallvirt(typeof(Console), nameof(Console.WriteLine))) 
				&& c.TryGotoPrev(i => i.MatchStloc(msg))
				&& c.TryGotoPrev(MoveType.After, i => i.MatchCallOrCallvirt<Exception>("get_" + nameof(Exception.Message)))
				) {
				c.EmitDelegate((string text) => {
					if ((text?.Contains("bytes caused by Origins in HandlePacket") ?? false) || (text?.Contains("bytes caused by ModDemoUtils in HandlePacket") ?? false)) {
						text += $" with packet type {lastPacketType}";
					}
					return text;
				});
			}
		}
		private ReLogic.Utilities.SlotId On_SoundEngine_PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback(On_SoundEngine.orig_PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback orig, ref SoundStyle style, Vector2? position, SoundUpdateCallback updateCallback) {
			if (Strange_Computer.drawingStrangeLine) return ReLogic.Utilities.SlotId.Invalid;
			return orig(ref style, position, updateCallback);
		}

		private int On_Gore_NewGore_IEntitySource_Vector2_Vector2_int_float(On_Gore.orig_NewGore_IEntitySource_Vector2_Vector2_int_float orig, IEntitySource source, Vector2 Position, Vector2 Velocity, int Type, float Scale) {
			if (Strange_Computer.drawingStrangeLine) return 600;
			return orig(source, Position, Velocity, Type, Scale);
		}

		private int On_Dust_NewDust(On_Dust.orig_NewDust orig, Vector2 Position, int Width, int Height, int Type, float SpeedX, float SpeedY, int Alpha, Color newColor, float Scale) {
			if (Strange_Computer.drawingStrangeLine) return 6000;
			return orig(Position, Width, Height, Type, SpeedX, SpeedY, Alpha, newColor, Scale);
		}

		private void On_Lighting_AddLight_int_int_float_float_float(On_Lighting.orig_AddLight_int_int_float_float_float orig, int i, int j, float r, float g, float b) {
			if (Strange_Computer.drawingStrangeLine) return;
			orig(i, j, r, g, b);
		}

		private void On_Item_CloneDefaults(On_Item.orig_CloneDefaults orig, Item self, int TypeToClone) {
			OriginGlobalItem.isOriginsItemCloningDefaults = self?.ModItem?.Mod == this;
			try {
				orig(self, TypeToClone);
			} finally {
				OriginGlobalItem.isOriginsItemCloningDefaults = false;
			}
		}

		private void FixWrongWaterfallAlpha_IL(ILContext il) {
			ILCursor c = new(il);
			ILLabel defaultCase = null;
			if (c.TryGotoNext(MoveType.Before,
				i => i.MatchBr(out defaultCase)
			)) {
				c.GotoLabel(defaultCase, MoveType.AfterLabel);
				c.Prev.MatchBr(out ILLabel end);
				c.EmitLdarg2();
				c.EmitLdarg0();
				c.EmitLdloca(0);
				c.EmitCall(GetType().GetMethod(nameof(FixWrongWaterfallAlpha), BindingFlags.NonPublic | BindingFlags.Static));
				c.EmitBrtrue(end);
			} else {
				LogError($"Could not find default switch case in WaterfallManager.GetAlpha");
			}
		}
		static bool FixWrongWaterfallAlpha(int type, float baseAlpha, ref float alpha) {
			if (type == Riven_Waterfall_Style.ID) {
				alpha = baseAlpha * Riven_Water_Style.GlowValue;
				return true;
			}
			return false;
		}

		private static void On_PlayerStatsSnapshot_ctor(On_PlayerStatsSnapshot.orig_ctor orig, ref PlayerStatsSnapshot self, Player player) {
			orig(ref self, player);
			if (self.AmountOfLifeHearts <= 0) self.AmountOfLifeHearts = 1;
		}
		private void IL_PlayerStatsSnapshot_ctor(ILContext il) {
			ILCursor c = new(il);
			int num = -1;
			if (c.TryGotoNext(MoveType.After,
				i => i.MatchLdloc(out num),
				i => i.MatchLdloc(out _),
				i => i.MatchLdloc(out _),
				i => i.MatchDiv(),
				i => i.MatchConvR4(),
				i => i.MatchAdd(),
				i => i.MatchStloc(num)
			)) {
				c.Index -= 4;
				c.EmitLdcI4(1);
				c.EmitCall(typeof(Math).GetMethod(nameof(Math.Max), [typeof(int), typeof(int)]));
			} else {
				LogError("Could not find potential division by zero in PlayerStatsSnapshot ctor");
			}
		}

		public static int currentChanceNumerator = 1;
		public static Dictionary<Type, FastFieldInfo<IItemDropRule, int>> chanceNumerators;
		private void On_CommonCode_ModifyItemDropFromNPC(On_CommonCode.orig_ModifyItemDropFromNPC orig, NPC npc, int itemIndex) {
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex, 1f);
			}
			orig(npc, itemIndex);
		}

		private void IL_NPCUtils_TargetClosestNonBees(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.Before, ins => ins.MatchLdnull());
			c.Remove();
			c.Emit(Ldarg_0);
			c.EmitDelegate(EmergencyBeeFilter);
		}
		public static NPCUtils.SearchFilter<Player> EmergencyBeeFilter(NPC bee) => (player) => bee.playerInteraction[player.whoAmI] || !player.GetModPlayer<OriginPlayer>().emergencyBeeCanister;
		private static void IL_Main_DrawSurfaceBG(ILContext il) {
			ILCursor c = new(il);
			int index = -1;
			int color = -1;
			int cloudAlpha = -1;
			(OpCode code, object operand)[] cloudYInstructions = null;
			bool DoThing() {
				if (!c.TryGotoNext(MoveType.After,
					i => i.MatchLdsfld<Main>(nameof(Main.cloud)),
					i => i.MatchLdloc(out index),
					i => i.MatchLdelemRef(),
					i => i.MatchLdfld<Cloud>(nameof(Cloud.scale)),
					i => i.MatchLdsfld<Main>(nameof(Main.cloud)),
					i => i.MatchLdloc(index),
					i => i.MatchLdelemRef(),
					i => i.MatchLdfld<Cloud>(nameof(Cloud.spriteDir)),
					i => i.MatchLdcR4(0),
					i => i.MatchCallOrCallvirt<SpriteBatch>(nameof(SpriteBatch.Draw))
				)) return false;
				int afterIndex = c.Index;
				c.Index--;
				//public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
				MonoFuckery.SkipPrevArgument(c);//layerDepth
				MonoFuckery.SkipPrevArgument(c);//effects
				MonoFuckery.SkipPrevArgument(c);//scale
				MonoFuckery.SkipPrevArgument(c);//origin
				MonoFuckery.SkipPrevArgument(c);//rotation
				MonoFuckery.SkipPrevArgument(c);//color
				c.Next.MatchLdloc(out color);
				c.Next.Next.MatchLdloc(out cloudAlpha);
				MonoFuckery.SkipPrevArgument(c);//sourceRectangle
				int positionArg = MonoFuckery.SkipPrevArgument(c);
				c.Index += positionArg;
				c.Index--;
				int positionYArg = MonoFuckery.SkipPrevArgument(c);//position y
				cloudYInstructions = new (OpCode code, object operand)[positionYArg - 11];
				for (int i = 0; i < positionYArg; i++) {
					if (i == 0) {
						cloudYInstructions[0] = (c.Next.OpCode, c.Next.Operand);
					} if (i > 11) {
						cloudYInstructions[i - 11] = (c.Next.OpCode, c.Next.Operand);
					} else {

					}
					c.Index++;
				}
				c.Index = afterIndex;
				c.EmitDelegate(() => {
					if (Main.gameMenu) {
						return MenuLoader.CurrentMenu.MenuBackgroundStyle is Riven_Surface_Background;
					} else {
						return Main.LocalPlayer.InModBiome<Riven_Hive>();
					}
				});
				return true;
			}
			while (DoThing()) {
				ILLabel ifBreak = c.DefineLabel();
				c.Emit(Brfalse, ifBreak);
				c.Emit(Ldloc, index);
				c.Emit(Ldloc, color);
				c.Emit(Ldloc, cloudAlpha);
				foreach ((OpCode code, object operand) in cloudYInstructions) {
					c.Emit(code, operand);
				}
				c.EmitDelegate<Action<int, Color, float, float>>(static (i, color, cloudAlpha, cloudY) => {
					//return;
					if (CloudBottoms is null) LoadCloudBottoms();
					color = new Color(0, 80, 70, 0) * (color.A / 255f);
					Cloud cloud = Main.cloud[i];
					Texture2D texture = CloudBottoms[cloud.type];
					Vector2 halfSize = texture.Size() * 0.5f;
					Main.spriteBatch.Draw(
						texture,
						new Vector2(cloud.position.X, cloudY) + halfSize,
						texture.Bounds,
						color * (cloudAlpha * cloudAlpha) * Riven_Hive.NormalGlowValue.GetValue(),//
						cloud.rotation,
						halfSize,
						cloud.scale,
						cloud.spriteDir,
					0f);
				});
				c.MarkLabel(ifBreak);
			}
		}

		delegate void __DrawCompositeArmorPiece(ref PlayerDrawSet drawinfo, CompositePlayerDrawContext context, DrawData data);
		private void IL_PlayerDrawLayers_DrawPlayer_28_ArmOverItemComposite(ILContext il) {
			ILCursor c = new(il);
			int drawData = -1;
			static void _DrawCompositeArmorPiece(ref PlayerDrawSet drawinfo, CompositePlayerDrawContext context, DrawData data) {
				if (drawinfo.armGlowMask >= 0 && drawinfo.armGlowMask < TextureAssets.GlowMask.Length) {
					data.texture = TextureAssets.GlowMask[drawinfo.armGlowMask].Value;
					data.color = drawinfo.armGlowColor;
					PlayerDrawLayers.DrawCompositeArmorPiece(ref drawinfo, context, data);
				}
			}

			c.GotoNext(MoveType.After,
				i => i.MatchLdarg(0),
				i => i.MatchLdcI4((int)CompositePlayerDrawContext.FrontShoulder),
				i => i.MatchLdloca(out drawData),
				i => i.MatchLdloc(out _),
				i => i.MatchLdloc(out _),
				i => i.MatchLdarg(0),
				i => i.MatchLdfld<PlayerDrawSet>("compFrontShoulderFrame"),
				i => i.MatchNewobj(out _),
				i => i.MatchLdarg(0),
				i => i.MatchLdfld<PlayerDrawSet>("colorArmorBody"),
				i => i.MatchLdloc(out _),
				i => i.MatchLdloc(out _),
				i => i.MatchLdcR4(out _),
				i => i.MatchLdarg(0),
				i => i.MatchLdfld<PlayerDrawSet>("playerEffect"),
				i => i.MatchLdcR4(out _),
				i => i.MatchCall(out _),
				i => i.MatchLdloca(out _),
				i => i.MatchLdarg(0),
				i => i.MatchLdfld<PlayerDrawSet>("cBody"),
				i => i.MatchStfld<DrawData>("shader"),
				i => i.MatchLdloc(out _),
				i => i.MatchCall(typeof(PlayerDrawLayers), "DrawCompositeArmorPiece")
			);
			c.Emit(Ldarg_0);
			c.Emit(Ldc_I4_3);
			c.Emit(Ldloc, drawData);
			c.EmitDelegate<__DrawCompositeArmorPiece>(_DrawCompositeArmorPiece);

			c.GotoNext(MoveType.After,
				i => i.MatchLdarg(0),
				i => i.MatchLdcI4((int)CompositePlayerDrawContext.FrontArm),
				i => i.MatchLdloca(out drawData),
				i => i.MatchLdloc(out _),
				i => i.MatchLdloc(out _),
				i => i.MatchLdarg(0),
				i => i.MatchLdfld<PlayerDrawSet>("compFrontArmFrame"),
				i => i.MatchNewobj(out _),
				i => i.MatchLdarg(0),
				i => i.MatchLdfld<PlayerDrawSet>("colorArmorBody"),
				i => i.MatchLdloc(out _),
				i => i.MatchLdloc(out _),
				i => i.MatchLdcR4(out _),
				i => i.MatchLdarg(0),
				i => i.MatchLdfld<PlayerDrawSet>("playerEffect"),
				i => i.MatchLdcR4(out _),
				i => i.MatchCall(out _),
				i => i.MatchLdloca(out _),
				i => i.MatchLdarg(0),
				i => i.MatchLdfld<PlayerDrawSet>("cBody"),
				i => i.MatchStfld<DrawData>("shader"),
				i => i.MatchLdloc(out _),
				i => i.MatchCall(typeof(PlayerDrawLayers), "DrawCompositeArmorPiece")
			);
			c.Emit(Ldarg_0);
			c.Emit(Ldc_I4_3);
			c.Emit(Ldloc, drawData);
			c.EmitDelegate<__DrawCompositeArmorPiece>(_DrawCompositeArmorPiece);
		}

		private void IL_Main_DrawRain(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.Before, i => i.MatchStloc(6));
			c.EmitDelegate<Func<Color, Color>>(static c => rivenRain ? new Color(0, 255, 195, 175) : c);
		}

		static Rectangle umbrellaHitbox;
		public static bool rainedOnPlayer = false;
		static bool rivenRain = false;
		private void On_Main_DrawRain(On_Main.orig_DrawRain orig, Main self) {
			Player player = Main.LocalPlayer;
			if (player is null || !player.active || player.dead) {
				orig(self);
				return;
			}
			Rectangle playerHitbox = player.Hitbox;
			umbrellaHitbox = default;
			switch (player.HeldItem.type) {
				case ItemID.Umbrella:
				case ItemID.TragicUmbrella:
				if (player.ItemAnimationActive) goto default;
				umbrellaHitbox = playerHitbox;
				float width = 16;
				player.ApplyMeleeScale(ref width);
				umbrellaHitbox.Inflate((int)width, -4);
				umbrellaHitbox.Y -= umbrellaHitbox.Height + 8;
				umbrellaHitbox.X += Main.LocalPlayer.direction * 8;
				break;
				default:
				if (player.armor[0].type == ItemID.UmbrellaHat) {
					umbrellaHitbox = playerHitbox;
					umbrellaHitbox.Inflate(14, -12);
					umbrellaHitbox.Y -= umbrellaHitbox.Height;
				}
				break;
			}
			rainedOnPlayer = false;
			rivenRain = player.InModBiome<Riven_Hive>();
			orig(self);
			if (rainedOnPlayer) player.OriginPlayer().timeSinceRainedOn = 0;
			if (rainedOnPlayer && rivenRain) {
				OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
				bool extraStrength = Main.remixWorld || (Main.masterMode && NPC.npcsFoundForCheckActive[MC.NPCType<World_Cracker_Head>()]);
				originPlayer.GetAssimilation<Riven_Assimilation>().Percent += extraStrength ? 0.01f : 0.003f;
				int duration = extraStrength ? 30 : 15;
				int targetTime = extraStrength ? 480 : 1440;
				OriginPlayer.InflictTorn(player, duration, targetTime, 1);
			}
		}

		private void On_Rain_Update(On_Rain.orig_Update orig, Rain self) {
			orig(self);
			if (rivenRain && Main.remixWorld && Collision.SolidCollision(self.position, 2, 2)) {
				self.active = false;
			}
			int rainSmokeChance = 1000;
			bool splash = false;
			if (umbrellaHitbox.Contains(self.position)) {
				self.active = false;
				splash = true;
				rainSmokeChance /= 5;
			} else if (Main.LocalPlayer.Hitbox.Contains(self.position)) {
				self.active = false;
				rainedOnPlayer = true;
				splash = true;
				rainSmokeChance /= 5;
			}
			if (rivenRain && !self.active && Main.netMode != NetmodeID.Server && Main.rand.Next(rainSmokeChance) < Main.gfxQuality * 100f) {
				Gore.NewGore(Entity.GetSource_None(), self.position, default, GoreID.ChimneySmoke1 + Main.rand.Next(3));
			}
			if (splash) {
				if (Main.rand.Next(100) < Main.gfxQuality * 100f) {
					Dust dust = Main.dust[Dust.NewDust(self.position - self.velocity, 2, 2, Dust.dustWater())];
					dust.position.X -= 2f;
					dust.position.Y += 2f;
					dust.alpha = 38;
					dust.velocity *= 0.1f;
					dust.velocity += -self.velocity * 0.025f;
					dust.scale = 0.6f;
					dust.noGravity = true;
				}
			}
		}

		private void IL_Main_UpdateWeather(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.After, i => i.MatchCall<Main>("get_IsItStorming"));
			c.EmitDelegate<Func<bool, bool>>(static (v) => v || (Main.masterMode && Main.LocalPlayer.InModBiome<Riven_Hive>() && NPC.npcsFoundForCheckActive[MC.NPCType<World_Cracker_Head>()]));
		}

		private void On_Player_Update_NPCCollision(On_Player.orig_Update_NPCCollision orig, Player self) {
			orig(self);
			if (self.TryGetModPlayer(out OriginPlayer originPlayer)) {
				originPlayer.PostHitByNPC();
			}
		}

		private int On_Player_AddBuff_DetermineBuffTimeToAdd(On_Player.orig_AddBuff_DetermineBuffTimeToAdd orig, Player self, int type, int time) {
			int value = orig(self, type, time);
			if (type != BuffID.PotionSickness && self.TryGetModPlayer(out OriginPlayer originPlayer)) {
				float mult = 1f;
				if (originPlayer.plasmaPhial && Main.debuff[type]) {
					mult *= OriginPlayer.plasmaPhialMult;
				}
				if (originPlayer.donorWristband && Main.debuff[type]) {
					mult *= OriginPlayer.donorWristbandMult;
				}
				if (originPlayer.mithrafin && Mithrafin.buffTypes[type]) {
					mult *= OriginPlayer.mithrafinSelfMult;
				}
				value = (int)(value * mult);
			}
			return value;
		}

		private void Player_KillMe(On_Player.orig_KillMe orig, Player self, PlayerDeathReason damageSource, double dmg, int hitDirection, bool pvp) {
			orig(self, damageSource, dmg, hitDirection, pvp);
		}

		#region combat
		private void Projectile_ExplodeTiles(On_Projectile.orig_ExplodeTiles orig, Projectile self, Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ, bool wallSplode) {
			if (self.TryGetGlobalProjectile(out ExplosiveGlobalProjectile global) && global.noTileSplode) return;
			orig(self, compareSpot, radius, minI, maxI, minJ, maxJ, wallSplode);
		}
		#endregion combat
		internal static bool hurtCollisionCrimsonVine;
		internal static bool rollingLotteryTicket;
		private int Player_RollLuck(On_Player.orig_RollLuck orig, Player self, int range) {
			const int lottery_ticket_min_denominator = 50;
			OriginPlayer originPlayer = self.GetModPlayer<OriginPlayer>();
			if (!rollingLotteryTicket && range >= lottery_ticket_min_denominator * currentChanceNumerator && originPlayer.lotteryTicketItem is not null) {
				rollingLotteryTicket = true;
				if (self.RollLuck((int)MathF.Ceiling(MathF.Pow(range, 0.75f) + range * 0.05f)) < currentChanceNumerator) {
					if (Main.netMode == NetmodeID.Server && !originPlayer.lotteryTicketItem.IsAir) {
						ModPacket packet = instance.GetPacket();
						packet.Write(NetMessageType.win_lottery);
						packet.Send(self.whoAmI, -1);
					}
					if (--originPlayer.lotteryTicketItem.stack <= 0) originPlayer.lotteryTicketItem.TurnToAir();
					rollingLotteryTicket = false;
					return 0;
				}
			}
			rollingLotteryTicket = false;
			int roll = orig(self, range);
			int rerollCount = OriginExtensions.RandomRound(Main.rand, originPlayer.brineClover / 4f);
			if (roll >= currentChanceNumerator && rerollCount > 0 && range > 1) {
				Item brineCloverItem = originPlayer.brineCloverItem;
				bool wilt = false;
				while (rerollCount > 0) {
					int newRoll = orig(self, range);
					if (newRoll < roll) {
						roll = newRoll;
						if (newRoll < currentChanceNumerator && range >= 20 * currentChanceNumerator) wilt = true;
					}
					rerollCount--;
				}
				if (wilt) {
					if (Main.netMode == NetmodeID.Server) {
						ModPacket packet = instance.GetPacket();
						packet.Write(NetMessageType.pickle_lottery);
						packet.Send(self.whoAmI, -1);
					}
					int prefix = brineCloverItem.prefix;
					brineCloverItem.SetDefaults((brineCloverItem.ModItem as Brine_Leafed_Clover)?.NextLowerTier ?? ItemID.None);
					brineCloverItem.Prefix(prefix);
				}
			}
			return roll;
		}
		#region quests
		private void Player_SetTalkNPC(On_Player.orig_SetTalkNPC orig, Player self, int npcIndex, bool fromNet) {
			if (self.talkNPC != npcIndex) Questing.Questing.ExitChat();
			orig(self, npcIndex, fromNet);
		}

		#endregion quests
		#region plants
		private void WorldGen_PlantAlchIL(ILContext il) {
			ILCursor c = new(il);
			if (!c.TryGotoNext(
				moveType: MoveType.Before,
				[
					v => v.MatchCall<NetMessage>("SendTileSquare")
				]
			)) return;
			if (!c.TryGotoPrev(
				moveType: MoveType.Before,
				[
					v => v.MatchLdsflda<Main>("tile")
				]
			)) return;
			if (!c.TryFindPrev(
				out ILCursor[] prevs,
				[
					v => v.MatchCall<WorldGen>("PlaceAlch")
				]
			)) return;
			ILCursor c2 = prevs[0];
			c.Index = c2.Index;
			c2.Index++;
			Stack<Instruction> ins = new();
			while (!c2.Next.MatchBneUn(out _)) {
				ins.Push(c2.Next);
				c2.Index--;
			}
			if (!c.TryGotoNext(
				moveType: MoveType.AfterLabel,
				[
					v => v.MatchLdsflda<Main>("tile")
				]
			)) return;
			Instruction instruction;
			while (ins.Count > 0) {
				instruction = ins.Pop();
				switch (instruction.OpCode.Code) {
					case Mono.Cecil.Cil.Code.Ldloc_0:
					case Mono.Cecil.Cil.Code.Ldloc_1:
					case Mono.Cecil.Cil.Code.Ldc_I4_1:
					case Mono.Cecil.Cil.Code.Sub:
					case Mono.Cecil.Cil.Code.Ldc_I4_6:
					c.Emit(instruction.OpCode, instruction.Operand);
					break;
					case Mono.Cecil.Cil.Code.Call:
					MethodBase placeCustomAlch = typeof(Origins).GetMethod(nameof(PlaceCustomAlch), BindingFlags.Public | BindingFlags.Static);
					c.Emit(instruction.OpCode, placeCustomAlch);
					break;
					case Mono.Cecil.Cil.Code.Pop:
					c.Emit(instruction.OpCode, instruction.Operand);
					break;
				}
			}
		}
		public static bool PlaceCustomAlch(int x, int y, int style) {
			if (Framing.GetTileSafely(x, y + 1).BlockType != BlockType.Solid) {
				return false;
			}
			Tile tile = Framing.GetTileSafely(x, y);
			ushort wiltedRose = (ushort)MC.TileType<Wilted_Rose>();
			if (TileObjectData.GetTileData(wiltedRose, 0).AnchorValidTiles.Contains(Main.tile[x, y + 1].TileType) && tile.LiquidAmount <= 0) {
				tile.HasTile = true;
				tile.TileType = wiltedRose;
				tile.TileFrameX = 0;
				tile.TileFrameY = 0;
				return true;
			}
			return false;
		}

		delegate bool orig_ShakeTree(int x, int y, int type, ref bool createLeaves);
		delegate bool hook_ShakeTree(orig_ShakeTree orig, int x, int y, int type, ref bool createLeaves);
		static FastStaticFieldInfo<WorldGen, int> _numTreeShakes;
		static FastStaticFieldInfo<WorldGen, int> _maxTreeShakes;
		static FastStaticFieldInfo<WorldGen, int[]> _treeShakeX;
		static FastStaticFieldInfo<WorldGen, int[]> _treeShakeY;
		private static void WorldGen_ShakeTree(On_WorldGen.orig_ShakeTree orig, int i, int j) {
			_numTreeShakes ??= new("numTreeShakes", BindingFlags.NonPublic);
			_maxTreeShakes ??= new("maxTreeShakes", BindingFlags.NonPublic);
			_treeShakeX ??= new("treeShakeX", BindingFlags.NonPublic);
			_treeShakeY ??= new("treeShakeY", BindingFlags.NonPublic);
			ref int numTreeShakes = ref _numTreeShakes.Value;
			int maxTreeShakes = _maxTreeShakes.Value;
			int[] treeShakeX = _treeShakeX.Value;
			int[] treeShakeY = _treeShakeY.Value;
			WorldGen.GetTreeBottom(i, j, out int x, out int y);
			int tileType = Main.tile[x, y].TileType;
			TreeTypes treeType = WorldGen.GetTreeType(tileType);
			bool edgeC = false;
			for (int k = 0; k < numTreeShakes; k++) {
				if (treeShakeX[k] == x && treeShakeY[k] == y) {
					edgeC = true;
				}
			}
			int origY = y;
			y--;
			while (y > 10 && Main.tile[x, y].HasTile && TileID.Sets.IsShakeable[Main.tile[x, y].TileType]) {
				y--;
			}
			y++;
			if (!WorldGen.IsTileALeafyTreeTop(x, y)) {
				return;
			}
			bool edgeB = numTreeShakes == maxTreeShakes;
			bool edgeA = Collision.SolidTiles(x - 2, x + 2, y - 2, y + 2);
			ITree tree = PlantLoader.GetTree(tileType);
			if (!(edgeA || edgeB || edgeC)) {
				if (PlantLoader_ShakeTree(x, y, tileType, out int vanillaIndex, false)) {
					foreach (TreeShaking.TreeShakeLoot drop in TreeShaking.GetLoot(TreeShaking.ShakeLoot, tree)) {
						Item.NewItem(new EntitySource_ShakeTree(i, j), i * 16, j * 16, 16, 16, drop.Type, WorldGen.genRand.Next(drop.Min, drop.Max));
					}
					treeShakeX[numTreeShakes] = x;
					treeShakeY[numTreeShakes] = origY;
					numTreeShakes++;
				} else {
					switch (vanillaIndex) {
						case 0:
						Projectile.NewProjectile(
							  new EntitySource_ShakeTree(x, y),
							  x * 16,
							  y * 16,
							  Main.rand.Next(-100, 101) * 0.002f,
							  0f,
							  28,
							  0,
							  0f,
							  Main.myPlayer,
							  16f,
							  16f
							);
						break;
						case 7:
						Item.NewItem(new EntitySource_ShakeTree(x, y), x * 16, y * 16, 16, 16, ItemID.RottenEgg, WorldGen.genRand.Next(1, 3));
						break;
						case 8: {
							TileMethods.KillTile_GetItemDrops(i, j, Main.tile[i, j], out int dropItem, out var _, out var _, out var _);
							Item.NewItem(new EntitySource_ShakeTree(x, y), x * 16, y * 16, 16, 16, dropItem, WorldGen.genRand.Next(1, 4));
							break;
						}
						case 9: {
							int dropItem = ItemID.CopperCoin;
							int count = WorldGen.genRand.Next(50, 100);
							if (WorldGen.genRand.NextBool(30)) {
								dropItem = ItemID.GoldCoin;
								count = 1;
								if (WorldGen.genRand.NextBool(5)) {
									count++;
								}
								if (WorldGen.genRand.NextBool(10)) {
									count++;
								}
							} else if (WorldGen.genRand.NextBool(10)) {
								dropItem = ItemID.SilverCoin;
								count = WorldGen.genRand.Next(1, 21);
								if (WorldGen.genRand.NextBool(3)) {
									count += WorldGen.genRand.Next(1, 21);
								}
								if (WorldGen.genRand.NextBool(4)) {
									count += WorldGen.genRand.Next(1, 21);
								}
							}
							Item.NewItem(new EntitySource_ShakeTree(x, y), x * 16, y * 16, 16, 16, dropItem, count);
							break;
						}
					}
				}
			}
			orig(i, j);
			foreach (TreeShaking.TreeShakeLoot drop in TreeShaking.GetLoot(TreeShaking.DryShakeLoot, tree)) {
				Item.NewItem(new EntitySource_ShakeTree(i, j), i * 16, j * 16, 16, 16, drop.Type, WorldGen.genRand.Next(drop.Min, drop.Max));
			}
		}
		internal static bool PlantLoader_ShakeTree(int x, int y, int type, out int index, bool useRealRand = false) {
			//getTreeBottom(i, j, out var x, out var y);
			index = -1;
			UnifiedRandom genRand = useRealRand ? WorldGen.genRand : WorldGen.genRand.Clone();
			TreeTypes treeType = WorldGen.GetTreeType(type);
			if (Main.getGoodWorld && genRand.NextBool(17)) index = 0; // lit Bomb
			if (genRand.NextBool(300) && treeType == TreeTypes.Forest) index = 1;//LivingWoodWand
			if (genRand.NextBool(300) && treeType == TreeTypes.Forest) index = 2;//LeafWand
			if (genRand.NextBool(200) && treeType == TreeTypes.Jungle) index = 3;//LivingMahoganyWand
			if (genRand.NextBool(200) && treeType == TreeTypes.Jungle) index = 4;//LivingMahoganyLeafWand
			if (genRand.NextBool(1000) && treeType == TreeTypes.Forest) index = 5;//EucaluptusSap
			if (genRand.NextBool(7) && (treeType == TreeTypes.Forest || treeType == TreeTypes.Snow || treeType == TreeTypes.Hallowed)) index = 6;//Acorn
			if (genRand.NextBool(8) && treeType == TreeTypes.Mushroom) index = 6;//MushroomGrassSeeds
			if (genRand.NextBool(35) && Main.halloween) index = 7;//RottenEgg
			if (genRand.NextBool(12)) index = 8;//wood? uses KillTile_GetItemDrops
			if (genRand.NextBool(20)) index = 9;//coin
			if (genRand.NextBool(15) && (treeType == TreeTypes.Forest || treeType == TreeTypes.Hallowed)) index = 10;//Bird
			if (genRand.NextBool(50) && treeType == TreeTypes.Hallowed && !Main.dayTime) index = 11;//Fairy
			if (genRand.NextBool(50) && treeType == TreeTypes.Forest && !Main.dayTime) index = 12;//Owl
			if (genRand.NextBool(40) && treeType == TreeTypes.Forest && !Main.dayTime && Main.halloween) index = 13;//Raven
			if (genRand.NextBool(50) && (treeType == TreeTypes.Forest || treeType == TreeTypes.Hallowed)) index = 14;//multiple birds?
			if (genRand.NextBool(40) && treeType == TreeTypes.Jungle) index = 15;//Bee
			if (genRand.NextBool(20) && (treeType == TreeTypes.Palm || treeType == TreeTypes.PalmCorrupt || treeType == TreeTypes.PalmCrimson || treeType == TreeTypes.PalmHallowed) && !WorldGen.IsPalmOasisTree(x)) index = 16;//Seagull2
			if (genRand.NextBool(30) && (treeType == TreeTypes.Crimson || treeType == TreeTypes.PalmCrimson)) index = 17;//LittleCrimera
			if (genRand.NextBool(30) && (treeType == TreeTypes.Corrupt || treeType == TreeTypes.PalmCorrupt)) index = 18;//LittleEater
			if (genRand.NextBool(30) && treeType == TreeTypes.Jungle && !Main.dayTime) index = 19;//JungleBat
			if (genRand.NextBool(40) && treeType == TreeTypes.Jungle) index = 20;//BeeHive
			if (genRand.NextBool(20) && (treeType == TreeTypes.Forest || treeType == TreeTypes.Hallowed) && !Main.raining && !NPC.TooWindyForButterflies && Main.dayTime) index = 21;//Butterfly
			if (genRand.NextBool(15) && treeType == TreeTypes.Forest) index = 22;//Apple, Peach, etc.
			if (genRand.NextBool(15) && treeType == TreeTypes.Snow) index = 23;//Plum, Cherry
			if (genRand.NextBool(15) && treeType == TreeTypes.Jungle) index = 24;//Mango, Pineapple
			if (genRand.NextBool(15) && (treeType == TreeTypes.Palm || treeType == TreeTypes.PalmCorrupt || treeType == TreeTypes.PalmCrimson || treeType == TreeTypes.PalmHallowed) && !WorldGen.IsPalmOasisTree(x)) index = 25;//Coconut, Banana
			if (genRand.NextBool(15) && (treeType == TreeTypes.Corrupt || treeType == TreeTypes.PalmCorrupt)) index = 26;//Elderberry, BlackCurrant
			if (genRand.NextBool(15) && (treeType == TreeTypes.Hallowed || treeType == TreeTypes.PalmHallowed)) index = 27;//Dragonfruit, Starfruit
			if (genRand.NextBool(15) && (treeType == TreeTypes.Crimson || treeType == TreeTypes.PalmCrimson)) index = 28;//BloodOrange, Rambutan
			return index == -1;
		}
		#endregion
		private void TileLightScanner_GetTileLight(On_TileLightScanner.orig_GetTileLight orig, TileLightScanner self, int x, int y, out Vector3 outputColor) {
			orig(self, x, y, out outputColor);
			try {
				Tile tile = Framing.GetTileSafely(x, y);

				if (tile.LiquidType == LiquidID.Water && LoaderManager.Get<WaterStylesLoader>().Get(Main.waterStyle) is IGlowingWaterStyle glowingWaterStyle) {
					glowingWaterStyle.AddLight(ref outputColor, tile.LiquidAmount);
				}
			} catch (Exception) {}
		}

		static int npcScoringRoom = -1;
		static string lastBiomeNameKey;
		#region drop rules
		private static void CommonCode_DropItem(ItemDropper orig, DropAttemptInfo info, int item, int stack, bool scattered = false) {
			(itemDropper ?? orig)(info, item, stack, scattered);
		}
		public static void ResolveRuleWithHandler(IItemDropRule rule, DropAttemptInfo dropInfo, ItemDropper handler) {
			try {
				itemDropper += handler;
				ResolveRule(rule, dropInfo);
			} finally {
				itemDropper = null;
			}
		}
		static event ItemDropper itemDropper;
		#endregion
		private void Projectile_GetWhipSettings(On_Projectile.orig_GetWhipSettings orig, Projectile proj, out float timeToFlyOut, out int segments, out float rangeMultiplier) {
			if (proj.ModProjectile is IWhipProjectile whip) {
				whip.GetWhipSettings(out timeToFlyOut, out segments, out rangeMultiplier);
			} else {
				orig(proj, out timeToFlyOut, out segments, out rangeMultiplier);
			}
			if (OriginConfig.Instance.VanillaWhipScale && proj.TryGetGlobalProjectile(out VanillaWhipScaleSupport scaleSupport)) rangeMultiplier *= scaleSupport.ScaleModifier;
		}
		#region mining
		private void On_Player_ItemCheck_UseMiningTools_TryHittingWall(On_Player.orig_ItemCheck_UseMiningTools_TryHittingWall orig, Player self, Item sItem, int wX, int wY) {
			orig(self, sItem, wX, wY);
			if (sItem.ModItem is C6_Jackhammer && self.altFunctionUse == 2) {
				for (int i = -1; i < 2; i++) {
					for (int j = -1; j < 2; j++) {
						if (i != 0 || j != 0) {
							orig(self, sItem, wX + i, wY + j);
						}
					}
				}
			}
		}
		#endregion
		#region tile counts
		private void WorldGen_CountTiles(On_WorldGen.orig_CountTiles orig, int X) {
			if (X == 0) OriginSystem.UpdateTotalEvilTiles();
			orig(X);
		}

		private void WorldGen_AddUpAlignmentCounts(On_WorldGen.orig_AddUpAlignmentCounts orig, bool clearCounts) {
			int[] tileCounts = WorldGen.tileCounts;
			if (clearCounts) {
				OriginSystem.totalDefiled2 = 0;
				OriginSystem.totalRiven2 = 0;
			}
			OriginSystem.totalDefiled2 += MC.GetInstance<Defiled_Wastelands_Alt_Biome>().SpreadingTiles.Sum(v => tileCounts[v]);
			OriginSystem.totalRiven2 += MC.GetInstance<Riven_Hive_Alt_Biome>().SpreadingTiles.Sum(v => tileCounts[v]);
			//OriginSystem.totalDefiled2 += tileCounts[MC.TileType<Defiled_Stone>()] + tileCounts[MC.TileType<Defiled_Grass>()] + tileCounts[MC.TileType<Defiled_Sand>()] + tileCounts[MC.TileType<Defiled_Ice>()];
			//OriginSystem.totalRiven2 += tileCounts[MC.TileType<Tiles.Riven.Riven_Flesh>()];
			orig(clearCounts);
		}
		#endregion
		#region worldgen
		private bool WorldGen_PlacePot(On_WorldGen.orig_PlacePot orig, int x, int y, ushort type, int style) {
			Tile placedOn = Framing.GetTileSafely(x, y + 1);
			if (PotType.TryGetValue(placedOn.TileType, out var potData)) {
				return orig(x, y, potData.potType, WorldGen.genRand.Next(potData.minStyle, potData.maxStyle));
			}
			return orig(x, y, type, style);
		}
		private bool WorldGen_PlaceSmallPile(On_WorldGen.orig_PlaceSmallPile orig, int i, int j, int X, int Y, ushort type) {
			Tile placedOn = Framing.GetTileSafely(i, j + 1);
			if (PileType.TryGetValue(placedOn.TileType, out var pileData)) {
				return orig(i, j, WorldGen.genRand.Next(pileData.minStyle, pileData.maxStyle), 0, pileData.pileType);
			}
			return orig(i, j, X, Y, type);
		}
		#endregion worldgen
		#region graphics
		private void On_Main_DrawBlack(On_Main.orig_DrawBlack orig, Main self, bool force) {
			if ((OriginPlayer.LocalOriginPlayer?.ZoneVoidProgressSmoothed ?? 0) <= 0) orig(self, force);
		}
		private void On_TileLightScanner_ApplyHellLight(On_TileLightScanner.orig_ApplyHellLight orig, TileLightScanner self, Tile tile, int x, int y, ref Vector3 lightColor) {
			Vector3 value = lightColor;
			orig(self, tile, x, y, ref value);
			if ((OriginPlayer.LocalOriginPlayer?.ZoneVoidProgressSmoothed ?? 0) > 0) {
				lightColor = Vector3.Lerp(value, lightColor, OriginPlayer.LocalOriginPlayer.ZoneVoidProgressSmoothed * 1.5f);
			} else {
				lightColor = value;
			}
		}
		public static RenderTarget2D currentScreenTarget;
		private void On_FilterManager_BeginCapture(On_FilterManager.orig_BeginCapture orig, FilterManager self, RenderTarget2D screenTarget1, Color clearColor) {
			orig(self, screenTarget1, clearColor);
			currentScreenTarget = screenTarget1;
		}
		internal static ShaderLayerTargetHandler shaderOroboros = new();
		public static int drawPlayersWithShader = -1;
		public static int keepPlayerShader = -1;
		internal static int forcePlayerShader = -1;
		internal static bool resetKeepPlayerShader = false;
		private static void On_PlayerDrawLayers_DrawPlayer_TransformDrawData(On_PlayerDrawLayers.orig_DrawPlayer_TransformDrawData orig, ref PlayerDrawSet drawinfo) {
			orig(ref drawinfo);
			if (forcePlayerShader >= 0) {
				for (int i = 0; i < drawinfo.DrawDataCache.Count; i++) {
					if (drawinfo.DrawDataCache[i].shader != keepPlayerShader) drawinfo.DrawDataCache[i] = drawinfo.DrawDataCache[i] with { shader = forcePlayerShader };
				}
			}
		}
		private void LegacyPlayerRenderer_DrawPlayerInternal(Terraria.Graphics.Renderers.On_LegacyPlayerRenderer.orig_DrawPlayerInternal orig, Terraria.Graphics.Renderers.LegacyPlayerRenderer self, Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow, float alpha, float scale, bool headOnly) {
			SpriteBatchState spriteBatchState = Main.spriteBatch.GetState();
			bool shaded = false;
			forcePlayerShader = -1;
			resetKeepPlayerShader = keepPlayerShader == -1;
			try {
				OriginPlayer originPlayer = drawPlayer.GetModPlayer<OriginPlayer>();
				if (drawPlayersWithShader < 0 && originPlayer.VisualRasterizedTime > 0) {
					if (resetKeepPlayerShader) keepPlayerShader = Anti_Gray_Dye.ShaderID;
					forcePlayerShader = Rasterized_Dye.ShaderID;
				} else if (drawPlayersWithShader < 0 && (originPlayer.shineSparkCharge > 0 || originPlayer.shineSparkDashTime > 0)) {
					forcePlayerShader = Shimmer_Dye.ShaderID;
				} else {
					List<VisualEffectPlayer.VisualEffect> effects = drawPlayer.GetModPlayer<VisualEffectPlayer>().effects;
					for (int i = 0; i < effects.Count; i++) {
						if (effects[i].SetForcedShader()) break;
					}
				}
				if (drawPlayersWithShader >= 0) {
					forcePlayerShader = drawPlayersWithShader;
					if (drawPlayersWithShader == coordinateMaskFilterID) {
						coordinateMaskFilter.Shader.Parameters["uOffset"].SetValue(drawPlayer.position);
						coordinateMaskFilter.Shader.Parameters["uScale"].SetValue(1f);
						coordinateMaskFilter.UseColor(new Vector3(originPlayer.tornOffset, originPlayer.tornCurrentSeverity));
					}
					orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, alpha, scale, headOnly);
					return;
				}
				if (originPlayer.amebicVialVisible) {

					const float offset = 2;
					forcePlayerShader = amebicProtectionShaderID;
					int itemAnimation = drawPlayer.itemAnimation;

					amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(offset, 0));
					orig(self, camera, drawPlayer, position + new Vector2(offset, 0), rotation, rotationOrigin, 0.01f, alpha, scale, headOnly);

					amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(-offset, 0));
					orig(self, camera, drawPlayer, position + new Vector2(-offset, 0), rotation, rotationOrigin, 0.01f, alpha, scale, headOnly);

					amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(0, offset));
					orig(self, camera, drawPlayer, position + new Vector2(0, offset), rotation, rotationOrigin, 0.01f, alpha, scale, headOnly);

					amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(0, -offset));
					orig(self, camera, drawPlayer, position + new Vector2(0, -offset), rotation, rotationOrigin, 0.01f, alpha, scale, headOnly);

					forcePlayerShader = -1;
					drawPlayer.itemAnimation = itemAnimation;
				}
				/*int rasterizedTime = originPlayer.rasterizedTime;
				if (rasterizedTime > 0) {
					shaded = true;
					rasterizeShader.Shader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
					rasterizeShader.Shader.Parameters["uOffset"].SetValue(drawPlayer.velocity.WithMaxLength(4) * 0.0625f * rasterizedTime);
					rasterizeShader.Shader.Parameters["uWorldPosition"].SetValue(drawPlayer.position);
					rasterizeShader.Shader.Parameters["uSecondaryColor"].SetValue(new Vector3(40, 1120, 0));
					Main.graphics.GraphicsDevice.Textures[1] = cellNoiseTexture;
					Main.spriteBatch.Restart(SpriteSortMode.Immediate, effect: rasterizeShader.Shader);
				}*/
				orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, alpha, scale, headOnly);
			} finally {
				if (shaded) {
					Main.spriteBatch.Restart(spriteBatchState);
				}
				forcePlayerShader = -1;
				if (resetKeepPlayerShader) keepPlayerShader = -1;
			}
		}

		static bool sonarDrawing = false;
		static bool sonarDrawingNonSolid = false;
		private void TileDrawing_Draw(On_TileDrawing.orig_Draw orig, TileDrawing self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets, int waterStyleOverride) {
			sonarDrawing = false;
			sonarDrawingNonSolid = false;
			orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);
			if (!Main.gamePaused && Main.instance.IsActive && Main.LocalPlayer.GetModPlayer<OriginPlayer>().sonarVisor) {//solidLayer && 
				sonarDrawing = true;
				sonarDrawingNonSolid = !solidLayer;
				tileOutlineShader.Shader.Parameters["uImageSize0"].SetValue(new Vector2(288, 396));//Main.ScreenSize.ToVector2()
				//tileOutlineShader.Shader.Parameters["uScale"].SetValue(2);
				//tileOutlineShader.Shader.Parameters["uColor"].SetValue(new Vector3(1f, 1f, 1f));//new Vector4(0.5f, 0.0625f, 0f, 0f)
				SpriteBatchState state = Main.spriteBatch.GetState();
				Main.spriteBatch.Restart(state, effect: tileOutlineShader.Shader);
				orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);
				Main.spriteBatch.Restart(state);
				sonarDrawing = false;
				sonarDrawingNonSolid = false;
			}
		}
		private Color TileDrawing_DrawTiles_GetLightOverride(On_TileDrawing.orig_DrawTiles_GetLightOverride orig, TileDrawing self, int j, int i, Tile tileCache, ushort typeCache, short tileFrameX, short tileFrameY, Color tileLight) {
			if (sonarDrawing) {
				bool solid = Main.tileSolid[typeCache];
				if (solid || sonarDrawingNonSolid) {
					if (TileDrawing.IsTileDangerous(i, j, Main.LocalPlayer)) {
						return new Color(255, 50, 50);
					}
					if (!solid) {
						return new Color(0, 0, 0, 0);
					}
					if (Main.IsTileSpelunkable(i, j)) {
						return new Color(200, 170, 100);
					}
					if (_IsSolidForSonar(Framing.GetTileSafely(i - 1, j))
						&& _IsSolidForSonar(Framing.GetTileSafely(i, j-1))
						&& _IsSolidForSonar(Framing.GetTileSafely(i + 1, j))
						&& _IsSolidForSonar(Framing.GetTileSafely(i, j + 1))) {
						return new Color(0, 0, 0, 0);
					}
					return new Color(255, 255, 255);
				} else {
					return new Color(0, 0, 0, 0);
				}
			} else if ((SC_Phase_Three_Underlay.alwaysLightAllTiles || SC_Phase_Three_Underlay.ForcedLit(i, j)) && (Filters.Scene["Origins:ShimmerConstructPhase3"].Active || Filters.Scene["Origins:ShimmerConstructPhase3Cheap"].IsVisible())) {
				Color color = orig(self, j, i, tileCache, typeCache, tileFrameX, tileFrameY, tileLight);
				if (color.R == 0 && color.G == 0 && color.B == 0) color.R = 1;
				return color;
			} else {
				return orig(self, j, i, tileCache, typeCache, tileFrameX, tileFrameY, tileLight);
			}
		}
		static bool _IsSolidForSonar(Tile tile) {
			return tile.HasTile && Main.tileSolid[tile.TileType] && tile.BlockType == BlockType.Solid;
		}

		private void TileDrawing_DrawSingleTile(ILContext il) {
			ILCursor c = new ILCursor(il);
			FieldReference tileLight = null;
			if (c.TryGotoNext(o => o.MatchStfld<TileDrawInfo>("tileLight") && o.MatchStfld(out tileLight))) {
				if (c.TryGotoNext(MoveType.After, o => o.MatchRet()) && c.TryGotoNext(MoveType.AfterLabel)) {
					ILLabel label = c.DefineLabel();
					//if (!sonarDrawing) {
					//c.Emit(OpCodes.Ldsfld, typeof(Origins).GetField("sonarDrawing", BindingFlags.NonPublic | BindingFlags.Static));
					//c.Emit(OpCodes.Brfalse, label);

					c.Emit(Ldarg_0);
					c.Emit(Ldarg_S, il.Method.Parameters[6]);
					c.Emit(Ldarg_S, il.Method.Parameters[7]);
					c.Emit(Ldarg_0);
					c.Emit(Ldfld, tileLight);
					c.EmitDelegate((Func<int, int, Color, Color>)((int tileX, int tileY, Color _tileLight) => {
						if (!sonarDrawing) {
							return _tileLight;
						}
						if (TileDrawing.IsTileDangerous(tileX, tileY, Main.LocalPlayer)) {
							return new Color(255, 50, 50);
						} else if (Main.IsTileSpelunkable(tileX, tileY)) {
							return new Color(200, 170, 0);
						} else if (Main.tileSolid[Main.tile[tileX, tileY].TileType]){
							return new Color(200, 200, 200);
						} else {
							return new Color(0, 0, 0, 0);
						}
					}));
					c.Emit(Stfld, tileLight);
					//c.MarkLabel(label);
					//}
				}
			}
		}
		

		private void _TileDrawing_DrawSingleTile(On_TileDrawing.orig_DrawSingleTile orig, TileDrawing self, TileDrawInfo drawData, bool solidLayer, int waterStyleOverride, Vector2 screenPosition, Vector2 screenOffset, int tileX, int tileY) {

			if (TileDrawing.IsTileDangerous(tileX, tileY, Main.LocalPlayer)) {
				drawData.tileLight = new Color(255, 50, 50);
			} else if (Main.IsTileSpelunkable(tileX, tileY)) {
				drawData.tileLight = new Color(200, 170, 0);
			} else {
				drawData.tileLight = new Color(200, 200, 200);
			}
			orig(self, drawData, solidLayer, waterStyleOverride, screenPosition, screenOffset, tileX, tileY);
		}
		#endregion graphics
	}
}
