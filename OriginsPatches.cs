using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.NPCs;
using Origins.NPCs.MiscE;
using Origins.NPCs.TownNPCs;
using Origins.Projectiles;
using Origins.Questing;
using Origins.Tiles.Brine;
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
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI.Chat;
using Terraria.UI.Gamepad;
using Terraria.Utilities;
using static Origins.OriginExtensions;
using MC = Terraria.ModLoader.ModContent;

namespace Origins {
	public partial class Origins : Mod {
		void ApplyPatches() {
			On.Terraria.NPC.UpdateCollision += (orig, self) => {
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
			On.Terraria.NPC.GetMeleeCollisionData += NPC_GetMeleeCollisionData;
			//On.Terraria.WorldGen.GERunner += OriginSystem.GERunnerHook;
			//On.Terraria.WorldGen.Convert += OriginSystem.ConvertHook;
			Defiled_Tree.Load();
			OriginSystem worldInstance = MC.GetInstance<OriginSystem>();
			if (!(worldInstance is null)) {
				worldInstance.defiledResurgenceTiles = new List<(int, int)> { };
				worldInstance.defiledAltResurgenceTiles = new List<(int, int, ushort)> { };
			}
			//IL.Terraria.WorldGen.GERunner+=OriginWorld.GERunnerHook;
			On.Terraria.Main.DrawInterface_Resources_Breath += FixedDrawBreath;
			On.Terraria.WorldGen.CountTiles += WorldGen_CountTiles;
			On.Terraria.WorldGen.AddUpAlignmentCounts += WorldGen_AddUpAlignmentCounts;
			Terraria.IO.WorldFile.OnWorldLoad += () => {
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					for (int i = 0; i < Main.maxTilesX; i++) WorldGen.CountTiles(i);
				}
			};
			//AltLibrary hooks Lang.GetDryadWorldStatusDialog, so ours is probably not necessary
			//On.Terraria.Lang.GetDryadWorldStatusDialog += Lang_GetDryadWorldStatusDialog;
			HookEndpointManager.Add(typeof(TileLoader).GetMethod("MineDamage", BindingFlags.Public | BindingFlags.Static), (hook_MinePower)MineDamage);

			On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.DrawPlayerInternal += LegacyPlayerRenderer_DrawPlayerInternal;
			On.Terraria.Projectile.GetWhipSettings += Projectile_GetWhipSettings;
			On.Terraria.Recipe.Condition.RecipeAvailable += (On.Terraria.Recipe.Condition.orig_RecipeAvailable orig, Recipe.Condition self, Recipe recipe) => {
				if (self == Recipe.Condition.NearWater && Main.LocalPlayer.InModBiome<Brine_Pool>()) {
					return false;
				}
				return orig(self, recipe);
			};
			HookEndpointManager.Add(typeof(CommonCode).GetMethod("DropItem", BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(DropAttemptInfo), typeof(int), typeof(int), typeof(bool) }), (hook_DropItem)CommonCode_DropItem);
			On.Terraria.WorldGen.ScoreRoom += (On.Terraria.WorldGen.orig_ScoreRoom orig, int ignoreNPC, int npcTypeAskingToScoreRoom) => {
				npcScoringRoom = npcTypeAskingToScoreRoom;
				orig(ignoreNPC, npcTypeAskingToScoreRoom);
			};
			On.Terraria.WorldGen.GetTileTypeCountByCategory += (On.Terraria.WorldGen.orig_GetTileTypeCountByCategory orig, int[] tileTypeCounts, TileScanGroup group) => {
				if (group == TileScanGroup.TotalGoodEvil) {
					int defiledTiles = tileTypeCounts[MC.TileType<Defiled_Stone>()] + tileTypeCounts[MC.TileType<Defiled_Grass>()] + tileTypeCounts[MC.TileType<Defiled_Sand>()] + tileTypeCounts[MC.TileType<Defiled_Ice>()];
					int rivenTiles = tileTypeCounts[MC.TileType<Tiles.Riven.Riven_Flesh>()];

					if (npcScoringRoom == MC.NPCType<Acid_Freak>()) {
						return orig(tileTypeCounts, TileScanGroup.Hallow);
					}

					return orig(tileTypeCounts, group) - (defiledTiles + rivenTiles);
				}
				return orig(tileTypeCounts, group);
			};
			On.Terraria.GameContent.ShopHelper.BiomeNameByKey += (On.Terraria.GameContent.ShopHelper.orig_BiomeNameByKey orig, string biomeNameKey) => {
				lastBiomeNameKey = biomeNameKey;
				return orig(biomeNameKey);
			};

			On.Terraria.Localization.Language.GetTextValueWith += (On.Terraria.Localization.Language.orig_GetTextValueWith orig, string key, object obj) => {
				if (key.EndsWith("Biome")) {
					try {
						string betterKey = key + "_" + lastBiomeNameKey.Split('.')[^1];
						if (Language.Exists(betterKey)) {
							key = betterKey;
						} else if (!Language.Exists(lastBiomeNameKey)) {
							obj = new {
								BiomeName = "the " + lastBiomeNameKey.Split('.')[^1].Replace('_', ' ')
							};
						}
					} catch (RuntimeBinderException) { }
				}
				return orig(key, obj);
			};

			On.Terraria.GameContent.ShopHelper.IsPlayerInEvilBiomes += (On.Terraria.GameContent.ShopHelper.orig_IsPlayerInEvilBiomes orig, ShopHelper self, Player player) => {
				bool retValue = false;
				IShoppingBiome[] orig_dangerousBiomes = dangerousBiomes.GetValue(self);
				try {
					IShoppingBiome[] _dangerousBiomes;
					if (Main.npc[player.talkNPC].type == MC.NPCType<Acid_Freak>()) {
						_dangerousBiomes = new IShoppingBiome[] { orig_dangerousBiomes[2] };
					} else {
						_dangerousBiomes = orig_dangerousBiomes.WithLength(orig_dangerousBiomes.Length + 2);
						_dangerousBiomes[^2] = new Defiled_Wastelands();
						_dangerousBiomes[^1] = new Riven_Hive();
					}
					dangerousBiomes.SetValue(self, _dangerousBiomes);
					retValue = orig(self, player);
				} finally {
					dangerousBiomes.SetValue(self, orig_dangerousBiomes);
				}
				return retValue;
			};
			On.Terraria.Main.GetProjectileDesiredShader += (orig, index) => {
				if (Main.projectile[index].TryGetGlobalProjectile(out OriginGlobalProj originGlobalProj) && originGlobalProj.isFromMitosis) {
					return GameShaders.Armor.GetShaderIdFromItemId(ItemID.StardustDye);
				}
				if (Main.projectile[index].ModProjectile is IShadedProjectile shadedProjectile) {
					return shadedProjectile.Shader;
				}
				return orig(index);
			};
			On.Terraria.Graphics.Light.TileLightScanner.GetTileLight += TileLightScanner_GetTileLight;
			//On.Terraria.GameContent.UI.Elements.UIWorldListItem.GetIcon += UIWorldListItem_GetIcon;
			//On.Terraria.GameContent.UI.Elements.UIGenProgressBar.DrawSelf += UIGenProgressBar_DrawSelf;
			/*HookEndpointManager.Add(typeof(PlantLoader).GetMethod("ShakeTree", BindingFlags.Public | BindingFlags.Static), 
                (hook_ShakeTree)((orig_ShakeTree orig, int x, int y, int type, ref bool createLeaves) => {
					if (orig(x, y, type, ref createLeaves)) {
                        PlantLoader_ShakeTree(x, y, type, ref createLeaves);
                        return true;
					}
                    return false;
                })
            );*/
			IL.Terraria.WorldGen.PlantAlch += WorldGen_PlantAlchIL;
			On.Terraria.WorldGen.PlantAlch += WorldGen_PlantAlch;
			On.Terraria.WorldGen.ShakeTree += WorldGen_ShakeTree;
			HookEndpointManager.Add(
				typeof(MC).GetMethod("ResizeArrays", BindingFlags.NonPublic | BindingFlags.Static),
				(Action<bool> orig, bool unloading) => {
					orig(unloading);
					if (!unloading) Origins.ResizeArrays();
				}
			);
			On.Terraria.WorldGen.KillWall_CheckFailure += (On.Terraria.WorldGen.orig_KillWall_CheckFailure orig, bool fail, Tile tileCache) => {
				fail = orig(fail, tileCache);
				if (Main.LocalPlayer.HeldItem.hammer < Origins.WallHammerRequirement[tileCache.WallType]) {
					fail = true;
				}
				return fail;
			};
			On.Terraria.Main.DrawNPCChatButtons += Main_DrawNPCChatButtons;
			On.Terraria.Player.SetTalkNPC += Player_SetTalkNPC;
			On.Terraria.Item.CanFillEmptyAmmoSlot += (orig, self) => {
				if (self.ammo == ItemID.Grenade || self.ammo == ItemID.Bomb || self.ammo == ItemID.Dynamite) {
					return false;
				}
				return orig(self);
			};
			On.Terraria.NPC.AddBuff += (orig, self, type, time, quiet) => {
				orig(self, type, time, quiet);
				if (!quiet && type != Headphones_Buff.ID && BuffID.Sets.IsAnNPCWhipDebuff[type] && Main.LocalPlayer.GetModPlayer<OriginPlayer>().summonTagForceCrit) {
					orig(self, Headphones_Buff.ID, 300, quiet);
				}
			};
			On.Terraria.Player.RollLuck += Player_RollLuck;
			On.Terraria.GameContent.Drawing.TileDrawing.Draw += TileDrawing_Draw;
			On.Terraria.GameContent.Drawing.TileDrawing.DrawTiles_GetLightOverride += TileDrawing_DrawTiles_GetLightOverride;
			IL.Terraria.NPC.StrikeNPC += NPC_StrikeNPC;
		}
		#region combat
		delegate void _ModifyNPCDefense(NPC npc, ref int defense);
		private static void NPC_StrikeNPC(ILContext il) {
			ILCursor c = new(il);
			if (c.TryGotoNext(op => op.MatchLdarg(0), op => op.MatchLdfld<NPC>("ichor"), op => op.MatchBrfalse(out _))) {
				c.Emit(OpCodes.Ldarg_0);
				c.Emit(OpCodes.Ldloca_S, (byte)2);
				c.EmitDelegate<_ModifyNPCDefense>(ModifyNPCDefense);
			}
		}
		public static void ModifyNPCDefense(NPC npc, ref int defense) {
			if (npc.ichor && CrimsonGlobalNPC.NPCTypes.Contains(npc.type)) {
				defense += 5;
			}
			if (npc.GetGlobalNPC<OriginGlobalNPC>().barnacleBuff) {
				defense += (int)(defense * 0.25f);
			}
			if (npc.HasBuff(Toxic_Shock_Debuff.ID)) {
				defense -= (int)(defense * 0.2f);
			}
		}
		#endregion combat
		private int Player_RollLuck(On.Terraria.Player.orig_RollLuck orig, Player self, int range) {
			OriginPlayer originPlayer = self.GetModPlayer<OriginPlayer>();
			int roll = orig(self, range);
			int rerollCount = OriginExtensions.RandomRound(Main.rand, originPlayer.brineClover / 4f);
			if (rerollCount > 0 && range > 1) {
				Item brineCloverItem = originPlayer.brineCloverItem;
				bool wilt = false;
				while (rerollCount > 0) {
					int newRoll = orig(self, range);
					if (newRoll < roll) {
						roll = newRoll;
						if (newRoll == 0 && range >= 20) wilt = true;
					}
					rerollCount--;
				}
				if (wilt) {
					int prefix = brineCloverItem.prefix;
					brineCloverItem.SetDefaults((brineCloverItem.ModItem as Brine_Leafed_Clover)?.NextLowerTier ?? 0);
					brineCloverItem.Prefix(prefix);
				}
			}
			return roll;
		}
		#region quests
		private void Player_SetTalkNPC(On.Terraria.Player.orig_SetTalkNPC orig, Player self, int npcIndex, bool fromNet) {
			orig(self, npcIndex, fromNet);
			if (npcIndex == -1) {
				npcChatQuestSelected = false;
				npcChatQuestListSelected = false;
				npcChatQuestIndexSelected = -1;
			}
		}

		static bool npcChatQuestsFocus = false;
		static bool npcChatQuestListSelected = false;
		static int npcChatQuestListFocus = -1;
		static bool npcChatQuestListBackFocus = false;
		static int npcChatQuestIndexSelected = -1;
		public static bool npcChatQuestSelected = false;
		private void Main_DrawNPCChatButtons(On.Terraria.Main.orig_DrawNPCChatButtons orig, int superColor, Color chatColor, int numLines, string focusText, string focusText3) {
			Player player = Main.LocalPlayer;
			Quest quest = null;
			List<Quest> startableQuests = new();
			NPC talkNPC = Main.npc[player.talkNPC];
			foreach (var currentQuest in Quest_Registry.Quests) {
				if (currentQuest.HasDialogue(talkNPC)) {
					quest = currentQuest;
					break;
				} else if (currentQuest.HasStartDialogue(talkNPC)) {
					startableQuests.Add(currentQuest);
				}
			}
			if (npcChatQuestListSelected) {
				DrawQuestList(startableQuests, 130 + numLines * 30, superColor);
				return;
			}
			orig(superColor, chatColor, numLines, focusText, focusText3);

			bool hasQuest = quest is not null;
			if (hasQuest || startableQuests.Any()) {
				DynamicSpriteFont font = FontAssets.MouseText.Value;
				float x = 180 + (Main.screenWidth - 800) / 2;
				x += ChatManager.GetStringSize(font, focusText, new Vector2(0.9f)).X + 30f;
				x += ChatManager.GetStringSize(font, Lang.inter[52].Value, new Vector2(0.9f)).X + 30f;
				if (!string.IsNullOrWhiteSpace(focusText3)) x += ChatManager.GetStringSize(font, focusText3, new Vector2(0.9f)).X + 30f;
				x += ChatManager.GetStringSize(font, Language.GetTextValue("UI.NPCCheckHappiness"), new Vector2(0.9f)).X + 30f;
				Vector2 position = new Vector2(x, 130 + numLines * 30);

				string textValue = hasQuest ? quest.GetDialogue() : Language.GetTextValue("Mods.Origins.Interface.Quests");
				Vector2 scale = new Vector2(0.9f);
				Vector2 stringSize = ChatManager.GetStringSize(font, textValue, scale);
				Color baseColor = new Color(superColor, (int)(superColor / 1.1), superColor / 2, superColor);

				if (Main.MouseScreen.Between(position, position + stringSize * scale) && !PlayerInput.IgnoreMouseInterface) {
					player.mouseInterface = true;
					player.releaseUseItem = false;
					scale *= 1.2f;
					if (!npcChatQuestsFocus) {
						SoundEngine.PlaySound(SoundID.MenuTick);
					}
					npcChatQuestsFocus = true;
				} else {
					if (npcChatQuestsFocus) {
						SoundEngine.PlaySound(SoundID.MenuTick);
					}
					npcChatQuestsFocus = false;
				}
				ChatManager.DrawColorCodedStringWithShadow(
					Main.spriteBatch,
					font,
					textValue,
					position + stringSize * 0.5f,
					baseColor,
					(!npcChatQuestsFocus) ? Color.Black : Color.Brown,
					0f,
					stringSize * 0.5f,
					scale
				);
				UILinkPointNavigator.SetPosition(2503, position + stringSize * 0.5f);
				UILinkPointNavigator.Shortcuts.NPCCHAT_ButtonsRight2 = true;
				if (npcChatQuestsFocus && !PlayerInput.IgnoreMouseInterface && Main.mouseLeft && Main.mouseLeftRelease) {
					Main.mouseLeftRelease = false;
					player.releaseUseItem = false;
					player.mouseInterface = true;
					SoundEngine.PlaySound(SoundID.MenuTick);
					if (hasQuest) {
						quest.OnDialogue();
					} else {
						npcChatQuestListSelected = true;
						Main.npcChatText = "beans, idk I just need to put something here or the menu will close";// go back to quest list dialogue
					}
				}
			}
		}
		void DrawQuestList(List<Quest> quests, float y, int superColor) {
			Player player = Main.LocalPlayer;
			int maxWidth = TextureAssets.ChatBack.Width();
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			float x = 180 + (Main.screenWidth - 800) / 2;
			float startX = x;
			int index = 0;
			int hoveredIndex = -1;
			bool selectedSpecificQuest = npcChatQuestIndexSelected != -1;

			string backTextValue = Language.GetTextValue("UI.Back");
			Vector2 backScale = new Vector2(0.9f);
			Vector2 backSize = ChatManager.GetStringSize(font, backTextValue, backScale);
			maxWidth -= (int)(backSize.X + 30);

			foreach (Quest quest in quests) {
				if (selectedSpecificQuest && index != npcChatQuestIndexSelected) continue;
				Vector2 position = new Vector2(x, y);
				string textValue = quest.GetDialogue();
				Vector2 scale = new Vector2(0.9f);
				Vector2 stringSize = ChatManager.GetStringSize(font, textValue, scale);
				if (x + stringSize.X - startX > maxWidth) break;
				Color baseColor = new Color(superColor, (int)(superColor / 1.1), superColor / 2, superColor);

				if (Main.MouseScreen.Between(position, position + stringSize * scale) && !PlayerInput.IgnoreMouseInterface) {
					player.mouseInterface = true;
					player.releaseUseItem = false;
					scale *= 1.2f;
					hoveredIndex = index;
				}
				ChatManager.DrawColorCodedStringWithShadow(
					Main.spriteBatch,
					font,
					textValue,
					position + stringSize * 0.5f,
					baseColor,
					(hoveredIndex != index) ? Color.Black : Color.Brown,
					0f,
					stringSize * 0.5f,
					scale
				);
				//UILinkPointNavigator.SetPosition(2503, position + stringSize * 0.5f);
				//UILinkPointNavigator.Shortcuts.NPCCHAT_ButtonsRight2 = true;
				if (hoveredIndex == index && !PlayerInput.IgnoreMouseInterface && Main.mouseLeft && Main.mouseLeftRelease) {
					Main.mouseLeftRelease = false;
					player.releaseUseItem = false;
					player.mouseInterface = true;
					SoundEngine.PlaySound(SoundID.MenuTick);
					quest.OnDialogue();
					npcChatQuestIndexSelected = index;
				}
				x += ChatManager.GetStringSize(font, textValue, new Vector2(0.9f)).X + 30f;
			}

			Vector2 backPosition = new Vector2(x, y);
			Color backBaseColor = new Color(superColor, (int)(superColor / 1.1), superColor / 2, superColor);

			if (Main.MouseScreen.Between(backPosition, backPosition + backSize * backScale) && !PlayerInput.IgnoreMouseInterface) {
				player.mouseInterface = true;
				player.releaseUseItem = false;
				backScale *= 1.2f;
				if (!npcChatQuestListBackFocus) {
					SoundEngine.PlaySound(SoundID.MenuTick);
				}
				npcChatQuestListBackFocus = true;
			} else {
				if (npcChatQuestListBackFocus) {
					SoundEngine.PlaySound(SoundID.MenuTick);
				}
				npcChatQuestListBackFocus = false;
			}
			ChatManager.DrawColorCodedStringWithShadow(
				Main.spriteBatch,
				font,
				backTextValue,
				backPosition + backSize * 0.5f,
				backBaseColor,
				(!npcChatQuestListBackFocus) ? Color.Black : Color.Brown,
				0f,
				backSize * 0.5f,
				backScale
			);
			//UILinkPointNavigator.SetPosition(2503, position + stringSize * 0.5f);
			//UILinkPointNavigator.Shortcuts.NPCCHAT_ButtonsRight2 = true;
			if (npcChatQuestListBackFocus && !PlayerInput.IgnoreMouseInterface && Main.mouseLeft && Main.mouseLeftRelease) {
				Main.mouseLeftRelease = false;
				player.releaseUseItem = false;
				player.mouseInterface = true;
				SoundEngine.PlaySound(SoundID.MenuTick);
				if (selectedSpecificQuest) {
					npcChatQuestIndexSelected = -1;
					npcChatQuestSelected = false;
					Main.npcChatText = "beans, idk I just need to put something here or the menu will close";// go back to quest list dialogue
				} else {
					npcChatQuestListSelected = false;
					Main.npcChatText = Main.npc[player.talkNPC].GetChat();// go back to normal dialogue
				}
			}
			if (hoveredIndex != npcChatQuestListFocus) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				npcChatQuestListFocus = hoveredIndex;
			}
		}
		#endregion quests
		#region plants
		private void WorldGen_PlantAlchIL(ILContext il) {
			ILCursor c = new ILCursor(il);
			if (!c.TryGotoNext(
				moveType: MoveType.Before,
				new Func<Instruction, bool>[] {
					v => v.MatchCall<NetMessage>("SendTileSquare")
				}
			)) return;
			if (!c.TryGotoPrev(
				moveType: MoveType.Before,
				new Func<Instruction, bool>[] {
					v => v.MatchLdsflda<Main>("tile")
				}
			)) return;
			if (!c.TryFindPrev(
				out ILCursor[] prevs,
				new Func<Instruction, bool>[] {
					v => v.MatchCall<WorldGen>("PlaceAlch")
				}
			)) return;
			ILCursor c2 = prevs[0];
			c.Index = c2.Index;
			c2.Index++;
			Stack<Instruction> ins = new Stack<Instruction>();
			while (!c2.Next.MatchBneUn(out _)) {
				ins.Push(c2.Next);
				c2.Index--;
			}
			if (!c.TryGotoNext(
				moveType: MoveType.AfterLabel,
				new Func<Instruction, bool>[] {
					v => v.MatchLdsflda<Main>("tile")
				}
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
					MethodBase placeCustomAlch = typeof(Origins).GetMethod("PlaceCustomAlch", BindingFlags.Public | BindingFlags.Static);
					c.Emit(instruction.OpCode, placeCustomAlch);
					break;
					case Mono.Cecil.Cil.Code.Pop:
					c.Emit(instruction.OpCode, instruction.Operand);
					break;
				}
			}
		}
		private void WorldGen_PlantAlch(On.Terraria.WorldGen.orig_PlantAlch orig) {
			orig();
			//if (!WorldGen.genRand.NextBool(10)) return;
			int x = WorldGen.genRand.Next(20, Main.maxTilesX - 20);
			int y = WorldGen.genRand.Next((int)Main.worldSurface - 250, Main.UnderworldLayer);
			while (y < Main.maxTilesY - 20 && (Main.tile[x, y].HasTile || Main.tile[x, y].WallType == WallID.None)) {
				y++;
			}
			int wallType = MC.WallType<Riven_Flesh_Wall>();
			while (y > 20 && !Main.tile[x, y - 1].HasTile && Main.tile[x, y - 1].WallType == wallType) {
				y--;
			}
			Tile tile = Framing.GetTileSafely(x, y);
			if (tile.WallType == MC.WallType<Riven_Flesh_Wall>()) {
				Tile left = Framing.GetTileSafely(x - 1, y);
				Tile right = Framing.GetTileSafely(x + 1, y);
				Tile up = Framing.GetTileSafely(x, y - 1);
				static int GetConnections(Tile tile, int dir) {
					if (tile.HasTile) {
						if (tile.TileType == MC.TileType<Wrycoral>()) return 2;
						if (tile.TileType == MC.TileType<Riven_Flesh>()) {
							switch (tile.BlockType) {
								case BlockType.Solid:
								return 1;
								case BlockType.HalfBlock:
								return dir == 2 ? 1 : 0;
								case BlockType.SlopeDownLeft:
								return dir == 1 ? 0 : 1;
								case BlockType.SlopeDownRight:
								return dir == 0 ? 0 : 1;
								case BlockType.SlopeUpLeft:
								return dir == 2 ? 1 : 0;
								case BlockType.SlopeUpRight:
								return dir == 1 ? 1 : 0;
							}
						}
					}
					return 0;
				}
				int connections = GetConnections(left, 0) + GetConnections(right, 1) + GetConnections(up, 2);
				if (Main.rand.Next(connections) > (connections / 3)) {
					tile.ResetToType((ushort)MC.TileType<Wrycoral>());
					WorldGen.SquareTileFrame(x, y);
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
			ushort brineClover = (ushort)MC.TileType<Brine_Leaf_Clover_Tile>();
			if (TileObject.CanPlace(x, y, brineClover, 0, 0, out TileObject objectData, false)) {
				objectData.style = 0;
				objectData.alternate = 0;
				objectData.random = 0;
				TileObject.Place(objectData);
				return true;
			}
			return false;
		}

		delegate bool orig_ShakeTree(int x, int y, int type, ref bool createLeaves);
		delegate bool hook_ShakeTree(orig_ShakeTree orig, int x, int y, int type, ref bool createLeaves);
		delegate void GetTreeBottom(int i, int j, out int x, out int y);
		GetTreeBottom _getTreeBottom;
		static GetTreeBottom getTreeBottom => instance._getTreeBottom ??=
			typeof(WorldGen).GetMethod("GetTreeBottom", BindingFlags.NonPublic | BindingFlags.Static)
			.CreateDelegate<GetTreeBottom>(null);
		private void WorldGen_ShakeTree(On.Terraria.WorldGen.orig_ShakeTree orig, int i, int j) {
			getTreeBottom(i, j, out var x, out var y);
			int num = y;
			int tileType = Main.tile[x, y].TileType;
			TreeTypes treeType = WorldGen.GetTreeType(tileType);
			if (treeType == TreeTypes.None) {
				return;
			}
			bool edgeC = false;
			for (int k = 0; k < WorldGen.numTreeShakes; k++) {
				if (WorldGen.treeShakeX[k] == x && WorldGen.treeShakeY[k] == y) {
					edgeC = true;
				}
			}
			y--;
			while (y > 10 && Main.tile[x, y].HasTile && TileID.Sets.IsShakeable[Main.tile[x, y].TileType]) {
				y--;
			}
			y++;
			if (!WorldGen.IsTileALeafyTreeTop(x, y)) {
				return;
			}
			bool edgeB = WorldGen.numTreeShakes == WorldGen.maxTreeShakes;
			bool edgeA = Collision.SolidTiles(x - 2, x + 2, y - 2, y + 2);
			if (PlantLoader_ShakeTree(x, y, tileType, edgeA || edgeB || edgeC)) {
				if (WorldGen.genRand.NextBool(20)) {
					switch (WorldGen.genRand.Next(2)) {
						case 0:
						Item.NewItem(new EntitySource_ShakeTree(i, j), i * 16, j * 16, 16, 16, MC.ItemType<Tree_Sap>(), WorldGen.genRand.Next(1, 3));
						break;
						case 1:
						Item.NewItem(new EntitySource_ShakeTree(i, j), i * 16, j * 16, 16, 16, MC.ItemType<Bark>(), WorldGen.genRand.Next(1, 3));
						break;
					}
				}
			}
			orig(i, j);
		}
		static bool PlantLoader_ShakeTree(int x, int y, int type, bool useRealRand = false) {
			//getTreeBottom(i, j, out var x, out var y);
			UnifiedRandom genRand = useRealRand ? WorldGen.genRand : WorldGen.genRand.Clone();
			TreeTypes treeType = WorldGen.GetTreeType(type);
			if (Main.getGoodWorld && genRand.NextBool(15)) return false;
			if (genRand.NextBool(300) && treeType == TreeTypes.Forest) return false;
			if (genRand.NextBool(300) && treeType == TreeTypes.Forest) return false;
			if (genRand.NextBool(200) && treeType == TreeTypes.Jungle) return false;
			if (genRand.NextBool(200) && treeType == TreeTypes.Jungle) return false;
			if (genRand.NextBool(1000) && treeType == TreeTypes.Forest) return false;
			if (genRand.NextBool(7) && (treeType == TreeTypes.Forest || treeType == TreeTypes.Snow || treeType == TreeTypes.Hallowed)) return false;
			if (genRand.NextBool(8) && treeType == TreeTypes.Mushroom) return false;
			if (genRand.NextBool(35) && Main.halloween) return false;
			if (genRand.NextBool(12)) return false;
			if (genRand.NextBool(20)) return false;
			if (genRand.NextBool(15) && (treeType == TreeTypes.Forest || treeType == TreeTypes.Hallowed)) return false;
			if (genRand.NextBool(50) && treeType == TreeTypes.Hallowed && !Main.dayTime) return false;
			if (genRand.NextBool(50) && treeType == TreeTypes.Forest && !Main.dayTime) return false;
			if (genRand.NextBool(40) && treeType == TreeTypes.Forest && !Main.dayTime && Main.halloween) return false;
			if (genRand.NextBool(50) && (treeType == TreeTypes.Forest || treeType == TreeTypes.Hallowed)) return false;
			if (genRand.NextBool(40) && treeType == TreeTypes.Jungle) return false;
			if (genRand.NextBool(20) && (treeType == TreeTypes.Palm || treeType == TreeTypes.PalmCorrupt || treeType == TreeTypes.PalmCrimson || treeType == TreeTypes.PalmHallowed) && !WorldGen.IsPalmOasisTree(x)) return false;
			if (genRand.NextBool(30) && (treeType == TreeTypes.Crimson || treeType == TreeTypes.PalmCrimson)) return false;
			if (genRand.NextBool(30) && (treeType == TreeTypes.Corrupt || treeType == TreeTypes.PalmCorrupt)) return false;
			if (genRand.NextBool(30) && treeType == TreeTypes.Jungle && !Main.dayTime) return false;
			if (genRand.NextBool(40) && treeType == TreeTypes.Jungle) return false;
			if (genRand.NextBool(20) && (treeType == TreeTypes.Forest || treeType == TreeTypes.Hallowed) && !Main.raining && !NPC.TooWindyForButterflies && Main.dayTime) return false;
			if (genRand.NextBool(15) && treeType == TreeTypes.Forest) return false;
			if (genRand.NextBool(15) && treeType == TreeTypes.Snow) return false;
			if (genRand.NextBool(15) && treeType == TreeTypes.Jungle) return false;
			if (genRand.NextBool(15) && (treeType == TreeTypes.Palm || treeType == TreeTypes.PalmCorrupt || treeType == TreeTypes.PalmCrimson || treeType == TreeTypes.PalmHallowed) && !WorldGen.IsPalmOasisTree(x)) return false;
			if (genRand.NextBool(15) && (treeType == TreeTypes.Corrupt || treeType == TreeTypes.PalmCorrupt)) return false;
			if (genRand.NextBool(15) && (treeType == TreeTypes.Hallowed || treeType == TreeTypes.PalmHallowed)) return false;
			if (genRand.NextBool(15) && (treeType == TreeTypes.Crimson || treeType == TreeTypes.PalmCrimson)) return false;
			return true;
		}
		#endregion
		private void TileLightScanner_GetTileLight(On.Terraria.Graphics.Light.TileLightScanner.orig_GetTileLight orig, Terraria.Graphics.Light.TileLightScanner self, int x, int y, out Vector3 outputColor) {
			orig(self, x, y, out outputColor);
			Tile tile = Main.tile[x, y];

			if (tile.LiquidType == LiquidID.Water && LoaderManager.Get<WaterStylesLoader>().Get(Main.waterStyle) is IGlowingWaterStyle glowingWaterStyle) {
				glowingWaterStyle.AddLight(ref outputColor, tile.LiquidAmount);
			}
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
				OriginExtensions.ResolveRule(rule, dropInfo);
			} finally {
				itemDropper = null;
			}
		}
		static event ItemDropper itemDropper;
		#endregion
		private void Projectile_GetWhipSettings(On.Terraria.Projectile.orig_GetWhipSettings orig, Projectile proj, out float timeToFlyOut, out int segments, out float rangeMultiplier) {
			if (proj.ModProjectile is IWhipProjectile whip) {
				whip.GetWhipSettings(out timeToFlyOut, out segments, out rangeMultiplier);
			} else {
				orig(proj, out timeToFlyOut, out segments, out rangeMultiplier);
			}
		}
		private string Lang_GetDryadWorldStatusDialog(On.Terraria.Lang.orig_GetDryadWorldStatusDialog orig) {
			const int good = 1;
			const int evil = 2;
			const int blood = 4;
			const int defiled = 8;
			const int riven = 16;
			string text = "";
			int tGood = WorldGen.tGood;
			int tEvil = WorldGen.tEvil;
			int tBlood = WorldGen.tBlood;
			int tDefiled = OriginSystem.tDefiled;
			int tRiven = OriginSystem.tRiven;
			int tBad = tEvil + tBlood + tDefiled + tRiven;
			if (tDefiled == 0 && tRiven == 0) {
				return orig();
			}
			int tHas = (tGood > 0 ? good : 0) | (tEvil > 0 ? evil : 0) | (tBlood > 0 ? blood : 0) | (tDefiled > 0 ? defiled : 0) | (tRiven > 0 ? riven : 0);
			switch (tHas & (good | evil | blood)) {
				case good | evil | blood:
				text = Language.GetTextValue("DryadSpecialText.WorldStatusAll", Main.worldName, tGood, tEvil, tBlood);
				break;
				case good | evil:
				text = Language.GetTextValue("DryadSpecialText.WorldStatusHallowCorrupt", Main.worldName, tGood, tEvil, tBlood);
				break;
				case good | blood:
				text = Language.GetTextValue("DryadSpecialText.WorldStatusHallowCrimson", Main.worldName, tGood, tEvil, tBlood);
				break;
				case evil | blood:
				text = Language.GetTextValue("DryadSpecialText.WorldStatusCorruptCrimson", Main.worldName, tGood, tEvil, tBlood);
				break;
				case evil:
				text = Language.GetTextValue("DryadSpecialText.WorldStatusCorrupt", Main.worldName, tGood, tEvil, tBlood);
				break;
				case blood:
				text = Language.GetTextValue("DryadSpecialText.WorldStatusCrimson", Main.worldName, tGood, tEvil, tBlood);
				break;
				case good:
				text = Language.GetTextValue("DryadSpecialText.WorldStatusHallow", Main.worldName, tGood, tEvil, tBlood);
				break;
				case 0:
				text = Language.GetTextValue("DryadSpecialText.WorldStatusPure", Main.worldName, tGood, tEvil, tBlood);
				break;
			}
			//temp fix, unlocalized and never grammatically correct
			if (tDefiled > 0) text += $" and {tDefiled}% defiled wastelands";
			if (tRiven > 0) text += $" and {tRiven}% riven";
			string str = (tGood * 1.2 >= tBad && tGood * 0.8 <= tBad) ?
				Language.GetTextValue("DryadSpecialText.WorldDescriptionBalanced") : ((tGood >= tBad) ?
				Language.GetTextValue("DryadSpecialText.WorldDescriptionFairyTale") : ((tBad > tGood + 20) ?
				Language.GetTextValue("DryadSpecialText.WorldDescriptionGrim") : ((tBad <= 10) ?
				Language.GetTextValue("DryadSpecialText.WorldDescriptionClose") :
				Language.GetTextValue("DryadSpecialText.WorldDescriptionWork"))));
			return text + " " + str;
		}
		#region mining power
		private delegate void orig_MinePower(int minePower, ref int damage);
		private delegate void hook_MinePower(orig_MinePower orig, int minePower, ref int damage);
		private void MineDamage(orig_MinePower orig, int minePower, ref int damage) {
			ModTile modTile = MC.GetModTile(Main.tile[Player.tileTargetX, Player.tileTargetY].TileType);
			if (modTile is IComplexMineDamageTile damageTile) {
				damageTile.MinePower(Player.tileTargetX, Player.tileTargetY, minePower, ref damage);
			} else {
				orig(minePower, ref damage);
			}
		}
		#endregion
		#region tile counts
		private void WorldGen_CountTiles(On.Terraria.WorldGen.orig_CountTiles orig, int X) {
			if (X == 0) OriginSystem.UpdateTotalEvilTiles();
			orig(X);
		}

		private void WorldGen_AddUpAlignmentCounts(On.Terraria.WorldGen.orig_AddUpAlignmentCounts orig, bool clearCounts) {
			int[] tileCounts = WorldGen.tileCounts;
			if (clearCounts) {
				OriginSystem.totalDefiled2 = 0;
				OriginSystem.totalRiven2 = 0;
			}
			OriginSystem.totalDefiled2 += tileCounts[MC.TileType<Defiled_Stone>()] + tileCounts[MC.TileType<Defiled_Grass>()] + tileCounts[MC.TileType<Defiled_Sand>()] + tileCounts[MC.TileType<Defiled_Ice>()];
			OriginSystem.totalRiven2 += tileCounts[MC.TileType<Tiles.Riven.Riven_Flesh>()];
			orig(clearCounts);
		}
		#endregion
		#region graphics
		private void LegacyPlayerRenderer_DrawPlayerInternal(On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.orig_DrawPlayerInternal orig, Terraria.Graphics.Renderers.LegacyPlayerRenderer self, Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow, float alpha, float scale, bool headOnly) {
			bool shaded = false;
			try {
				OriginPlayer originPlayer = drawPlayer.GetModPlayer<OriginPlayer>();
				if (originPlayer.amebicVialVisible) {
					PlayerShaderSet shaderSet = new PlayerShaderSet(drawPlayer);
					new PlayerShaderSet(amebicProtectionShaderID).Apply(drawPlayer);
					int playerHairDye = drawPlayer.hairDye;
					drawPlayer.hairDye = amebicProtectionHairShaderID;

					const float offset = 2;
					int itemAnimation = drawPlayer.itemAnimation;
					amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(offset, 0));
					orig(self, camera, drawPlayer, position + new Vector2(offset, 0), rotation, rotationOrigin, 0.01f, alpha, scale, headOnly);

					amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(-offset, 0));
					orig(self, camera, drawPlayer, position + new Vector2(-offset, 0), rotation, rotationOrigin, 0.01f, alpha, scale, headOnly);

					amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(0, offset));
					orig(self, camera, drawPlayer, position + new Vector2(0, offset), rotation, rotationOrigin, 0.01f, alpha, scale, headOnly);

					amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(0, -offset));
					orig(self, camera, drawPlayer, position + new Vector2(0, -offset), rotation, rotationOrigin, 0.01f, alpha, scale, headOnly);
					shaderSet.Apply(drawPlayer);
					drawPlayer.hairDye = playerHairDye;
					drawPlayer.itemAnimation = itemAnimation;
				}
				int rasterizedTime = originPlayer.rasterizedTime;
				if (rasterizedTime > 0) {
					shaded = true;
					rasterizeShader.Shader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
					rasterizeShader.Shader.Parameters["uOffset"].SetValue(drawPlayer.velocity.WithMaxLength(4) * 0.0625f * rasterizedTime);
					rasterizeShader.Shader.Parameters["uWorldPosition"].SetValue(drawPlayer.position);
					rasterizeShader.Shader.Parameters["uSecondaryColor"].SetValue(new Vector3(40, 1120, 0));
					Main.graphics.GraphicsDevice.Textures[1] = cellNoiseTexture;
					Main.spriteBatch.Restart(SpriteSortMode.Immediate, effect: rasterizeShader.Shader);
				}
				orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, alpha, scale, headOnly);
			} finally {
				if (shaded) {
					Main.spriteBatch.Restart();
				}
			}
		}

		static bool sonarDrawing = false;
		static bool sonarDrawingNonSolid = false;
		private void TileDrawing_Draw(On.Terraria.GameContent.Drawing.TileDrawing.orig_Draw orig, TileDrawing self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets, int waterStyleOverride) {
			sonarDrawing = false;
			sonarDrawingNonSolid = false;
			orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);
			if (!Main.gamePaused && Main.instance.IsActive && Main.LocalPlayer.GetModPlayer<OriginPlayer>().sonarVisor) {//solidLayer && 
				sonarDrawing = true;
				sonarDrawingNonSolid = !solidLayer;
				tileOutlineShader.Shader.Parameters["uImageSize0"].SetValue(Main.ScreenSize.ToVector2());
				//tileOutlineShader.Shader.Parameters["uScale"].SetValue(2);
				//tileOutlineShader.Shader.Parameters["uColor"].SetValue(new Vector3(1f, 1f, 1f));//new Vector4(0.5f, 0.0625f, 0f, 0f)
				SpriteBatchState state = Main.spriteBatch.GetState();
				Main.spriteBatch.Restart(state with {
					effect = tileOutlineShader.Shader
				});
				orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);
				Main.spriteBatch.Restart(state);
				sonarDrawing = false;
				sonarDrawingNonSolid = false;
			}
		}
		private Color TileDrawing_DrawTiles_GetLightOverride(On.Terraria.GameContent.Drawing.TileDrawing.orig_DrawTiles_GetLightOverride orig, TileDrawing self, int j, int i, Tile tileCache, ushort typeCache, short tileFrameX, short tileFrameY, Color tileLight) {
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
			} else {
				return orig(self, i, j, tileCache, typeCache, tileFrameX, tileFrameY, tileLight);
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

					c.Emit(OpCodes.Ldarg_0);
					c.Emit(OpCodes.Ldarg_S, il.Method.Parameters[6]);
					c.Emit(OpCodes.Ldarg_S, il.Method.Parameters[7]);
					c.Emit(OpCodes.Ldarg_0);
					c.Emit(OpCodes.Ldfld, tileLight);
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
					c.Emit(OpCodes.Stfld, tileLight);
					//c.MarkLabel(label);
					//}
				}
			}
		}
		

		private void _TileDrawing_DrawSingleTile(On.Terraria.GameContent.Drawing.TileDrawing.orig_DrawSingleTile orig, TileDrawing self, TileDrawInfo drawData, bool solidLayer, int waterStyleOverride, Vector2 screenPosition, Vector2 screenOffset, int tileX, int tileY) {

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
