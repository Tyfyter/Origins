using AltLibrary.Common.AltBiomes;
using AltLibrary.Common.Systems;
using Fargowiltas.Items.Tiles;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoMod.Cil;
using Origins.Buffs;
using Origins.CrossMod.Fargos.Items;
using Origins.Dev;
using Origins.Items;
using Origins.Items.Accessories;
using Origins.Items.Armor.Amber;
using Origins.Items.Armor.Other;
using Origins.Items.Materials;
using Origins.Items.Other;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Consumables.Broths;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Other.Fish;
using Origins.Items.Pets;
using Origins.Items.Vanity.BossMasks;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.NPCs;
using Origins.NPCs.Ashen.Boss;
using Origins.NPCs.Brine;
using Origins.NPCs.Brine.Boss;
using Origins.NPCs.Corrupt;
using Origins.NPCs.Crimson;
using Origins.NPCs.Defiled;
using Origins.NPCs.Defiled.Boss;
using Origins.NPCs.Dungeon;
using Origins.NPCs.Fiberglass;
using Origins.NPCs.MiscB;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.NPCs.MiscE;
using Origins.NPCs.Riven;
using Origins.NPCs.Riven.World_Cracker;
using Origins.Tiles;
using Origins.Tiles.Ashen;
using Origins.Tiles.BossDrops;
using Origins.Tiles.Brine;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using PegasusLib.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Items;
using ThoriumMod.Items.Darksteel;
using ThoriumMod.Items.Donate;
using ThoriumMod.Items.MeleeItems;
using ThoriumMod.Items.Misc;
using ThoriumMod.Items.Painting;
using ThoriumMod.Projectiles.Bard;
using static Origins.OriginsSets.Items;
using static Origins.OriginSystem;
using static Terraria.ModLoader.ModContent;

namespace Origins {
	public class OriginsModIntegrations : ILoadable {
		private static OriginsModIntegrations instance;
		Mod wikiThis;
		public static Mod WikiThis { get => instance.wikiThis; set => instance.wikiThis = value; }
		Mod epikV2;
		public static Mod EpikV2 { get => instance.epikV2; set => instance.epikV2 = value; }
		Asset<Texture2D> phaseIndicator;
		public static Asset<Texture2D> PhaseIndicator { get => instance.phaseIndicator; set => instance.phaseIndicator = value; }
		Mod herosMod;
		public static Mod HEROsMod { get => instance.herosMod; set => instance.herosMod = value; }
		Mod thorium;
		public static Mod Thorium { get => instance.thorium; set => instance.thorium = value; }
		Mod fancyLighting;
		public static Mod FancyLighting { get => instance.fancyLighting; set => instance.fancyLighting = value; }
		Mod fargosMutant;
		public static Mod FargosMutant { get => instance.fargosMutant; set => instance.fargosMutant = value; }
		Mod avalon;
		public static Mod Avalon { get => instance.avalon; set => instance.avalon = value; }
		Func<bool> checkAprilFools;
		public static Func<bool> CheckAprilFools {
			get => OriginClientConfig.Instance.DebugMenuButton.ForceAprilFools ?
				() => true :
				instance.checkAprilFools ??= ModLoader.TryGetMod("HolidayLib", out Mod HolidayLib) ? HolidayLibCheckAprilFools(HolidayLib) : DefaultCheckAprilFools;
			private set => instance.checkAprilFools = value;
		}
		Func<object[], object> holidayForceChanged;
		public static void HolidayForceChanged() => instance.holidayForceChanged([]);
		public static Condition AprilFools => new("Mods.Origins.Conditions.AprilFools", () => CheckAprilFools());
		public static Condition NotAprilFools => new(Language.GetOrRegister("Mods.Origins.Conditions.Not").WithFormatArgs(AprilFools.Description), () => !CheckAprilFools());
		ModKeybind goToKeybindKeybind;
		public static bool GoToKeybindKeybindPressed => instance.goToKeybindKeybind?.JustPressed ?? false;
		Action<ModKeybind> goToKeybind;
		Action<string> searchKeybinds;
		public static void GoToKeybind(ModKeybind keybind) {
			instance.goToKeybind?.Invoke(keybind);
		}
		public static void SearchKeybind(string text) {
			instance.searchKeybinds?.Invoke(text);
		}
		delegate void _drawingAOMap(ref bool value);
		_drawingAOMap drawingAOMap;
		public static bool DrawingAOMap {
			get {
				bool value = false;
				instance?.drawingAOMap?.Invoke(ref value);
				return value;
			}
		}
		public void Load(Mod mod) {
			instance = this;
			if (!Main.dedServ && ModLoader.TryGetMod("Wikithis", out wikiThis)) {
				WikiThis.Call("AddModURL", Origins.instance, "tyfyter.github.io/OriginsWiki/{}");
			}
			if (ModLoader.TryGetMod("ThoriumMod", out instance.thorium)) {
				LoadThorium();
			}
			if (ModLoader.TryGetMod("HolidayLib", out Mod HolidayLib)) {
				checkAprilFools = (Func<bool>)HolidayLib.Call("GETACTIVELOOKUP", "April fools");
				HolidayLib.Call("ADDHOLIDAY", "April fools", () => OriginSystem.Instance.ForceAF.ToInt());
				holidayForceChanged = (Func<object[], object>)HolidayLib.Call("GETFUNC", "HOLIDAYFORCECHANGED");
			} else {
				checkAprilFools = DefaultCheckAprilFools;
				holidayForceChanged = _ => -1;
			}
			ModLoader.TryGetMod("Avalon", out instance.avalon);
		}
		static Func<bool> HolidayLibCheckAprilFools(Mod HolidayLib) => (Func<bool>)HolidayLib.Call("GETACTIVELOOKUP", "April fools");
		static bool DefaultCheckAprilFools() => (DateTime.Today.Month == 4 && DateTime.Today.Day == 1) || Instance.ForceAF;
		static class BCSpriteConstructor {
			public static List<(int part, Vector2 pos, float rot, SpriteEffects effects)> parts = [];
			public static int partNum = 0;
			public static Vector2 partPos = default;
			public static float partRot = 0;
			public static SpriteEffects partEffects = SpriteEffects.None;
			public static MouseState oldMouse = default;
			public static KeyboardState oldKeyboard = default;
			public static void SetupArrangement(int maxParts, Vector2 center) {
				if (!DebugConfig.Instance.DebugMode) return;
				MouseState currentMouse = Mouse.GetState();
				KeyboardState currentKB = Keyboard.GetState();
				parts.RemoveAll(p => p.part < 0 || p.part > maxParts);
				if (partNum < 0) partNum = maxParts;
				if (partNum > maxParts) partNum = 0;
				if (Main.hasFocus) {
					partPos = Main.MouseScreen;
					int scrollDiff = currentMouse.ScrollWheelValue - oldMouse.ScrollWheelValue;
					if (scrollDiff != 0) {
						if (currentMouse.RightButton == ButtonState.Pressed) {
							partRot += scrollDiff * 0.001f;
						} else {
							partNum += scrollDiff / 120;
							if (partNum < 0) partNum = maxParts;
							if (partNum > maxParts) partNum = 0;
						}
					}
					if (currentMouse.XButton1 == ButtonState.Pressed && oldMouse.XButton1 == ButtonState.Released) partEffects ^= SpriteEffects.FlipHorizontally;
					if (currentMouse.XButton2 == ButtonState.Pressed && oldMouse.XButton2 == ButtonState.Released) partEffects ^= SpriteEffects.FlipVertically;
					if (currentMouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released) parts.Add((partNum, partPos, partRot, partEffects));
					if (parts.Count > 0 && currentKB.IsKeyDown(Keys.Back) && oldKeyboard.IsKeyUp(Keys.Back)) parts.RemoveAt(parts.Count - 1);
					if (parts.Count > 0 && currentKB.IsKeyDown(Keys.End) && oldKeyboard.IsKeyUp(Keys.End)) {
						Origins.instance.Logger.Info(string.Join('\n', parts.Select(p => $"({p.part}, new Vector2({p.pos.X - center.X}, {p.pos.Y - center.Y}) + center, {p.rot}f, SpriteEffects.{p.effects})")));
					}
				}
				oldMouse = currentMouse;
				oldKeyboard = currentKB;
			}
		}
		public static void PostSetupContent(Mod mod) {
			if (ModLoader.TryGetMod("BossChecklist", out Mod bossChecklist)) {
				static Func<bool> IfEvil<T>() where T : AltBiome {
					AltBiome biome = GetInstance<T>();
					return () => Main.drunkWorld || WorldBiomeManager.GetWorldEvil(true) == biome || ModLoader.HasMod("BothEvils");
				}
				/*Asset<Texture2D> glowTexture = Request<Texture2D>(typeof(Trenchmaker).GetDefaultTMLName() + "_Glow");
				Asset<Texture2D> armTexture = Request<Texture2D>(typeof(Trenchmaker).GetDefaultTMLName() + "_Arm");
				Asset<Texture2D> pistonTexture = Request<Texture2D>(typeof(Trenchmaker).GetDefaultTMLName() + "_Leg_Piston");
				Asset<Texture2D> hipTexture = Request<Texture2D>(typeof(Trenchmaker).GetDefaultTMLName() + "_Hip");
				Asset<Texture2D> hipGlowTexture = Request<Texture2D>(typeof(Trenchmaker).GetDefaultTMLName() + "_Hip_Glow");
				Asset<Texture2D> thighTexture = Request<Texture2D>(typeof(Trenchmaker).GetDefaultTMLName() + "_Thigh");
				Asset<Texture2D> calfTexture = Request<Texture2D>(typeof(Trenchmaker).GetDefaultTMLName() + "_Calf");
				Asset<Texture2D> footTexture = Request<Texture2D>(typeof(Trenchmaker).GetDefaultTMLName() + "_Foot");*/
				bossChecklist.Call("LogBoss",
					mod,
					"Trenchmaker",
					3f,
					() => NPC.downedBoss2,
					new List<int> { NPCType<Trenchmaker>(), NPCType<Fearmaker>() },
					new Dictionary<string, object> {
						["availability"] = IfEvil<Ashen_Alt_Biome>(),
						["spawnItems"] = ItemType<Distress_Beacon>(),
						["spawnInfo"] = Language.GetOrRegister("Mods.Origins.NPCs.Trenchmaker.BossChecklistIntegration.SpawnCondition"),
						["collectibles"] = new List<int> {
							RelicTileBase.ItemType<Trenchmaker_Relic>(),
							TrophyTileBase.ItemType<Trenchmaker_Trophy>(),
							ItemType<Trenchmaker_Mask>(),
							//ItemType<Fleshy_Globe>(),
						}/*,
						["customPortrait"] = (SpriteBatch spriteBatch, Rectangle area, Color color) => {
						}*/
					}
				);
				bossChecklist.Call("LogBoss",
					mod,
					nameof(Defiled_Amalgamation).Replace("_", ""),
					3f,
					() => NPC.downedBoss2,
					NPCType<Defiled_Amalgamation>(),
					new Dictionary<string, object> {
						["availability"] = IfEvil<Defiled_Wastelands_Alt_Biome>(),
						["spawnInfo"] = Language.GetOrRegister("Mods.Origins.NPCs.Defiled_Amalgamation.BossChecklistIntegration.SpawnCondition"),
						["spawnItems"] = ItemType<Nerve_Impulse_Manipulator>(),
						["collectibles"] = new List<int> {
							RelicTileBase.ItemType<Defiled_Amalgamation_Relic>(),
							TrophyTileBase.ItemType<Defiled_Amalgamation_Trophy>(),
							ItemType<Defiled_Amalgamation_Mask>(),
							ItemType<Blockus_Tube>(),
						}
					}
				);
				Asset<Texture2D> wcHeadTexture = Request<Texture2D>(typeof(World_Cracker_Head).GetDefaultTMLName());
				Asset<Texture2D> wcBodyTexture = Request<Texture2D>(typeof(World_Cracker_Body).GetDefaultTMLName());
				Asset<Texture2D> wcTailTexture = Request<Texture2D>(typeof(World_Cracker_Tail).GetDefaultTMLName());
				Asset<Texture2D> wcHeadArmorTexture = Request<Texture2D>("Origins/NPCs/Riven/World_Cracker/World_Cracker_Head_Armor");
				Asset<Texture2D> wcArmorTexture = Request<Texture2D>("Origins/NPCs/Riven/World_Cracker/World_Cracker_Armor");
				bossChecklist.Call("LogBoss",
					mod,
					"WorldCracker",
					3f,
					() => NPC.downedBoss2,
					new List<int> { NPCType<World_Cracker_Head>(), NPCType<World_Cracker_Body>(), NPCType<World_Cracker_Tail>() },
					new Dictionary<string, object> {
						["availability"] = IfEvil<Riven_Hive_Alt_Biome>(),
						["spawnItems"] = ItemType<Sus_Ice_Cream>(),
						["spawnInfo"] = Language.GetOrRegister("Mods.Origins.NPCs.World_Cracker_Head.BossChecklistIntegration.SpawnCondition"),
						["collectibles"] = new List<int> {
							RelicTileBase.ItemType<World_Cracker_Relic>(),
							TrophyTileBase.ItemType<World_Cracker_Trophy>(),
							ItemType<World_Cracker_Mask>(),
							ItemType<Fleshy_Globe>(),
						},
						["customPortrait"] = (SpriteBatch spriteBatch, Rectangle area, Color color) => {
							Vector2 center = area.Center();
							if (CheckAprilFools()) {
								void DrawSegment(Rectangle frame, Vector2 position, Texture2D baseTexture, int @switch) {
									switch (@switch) {
										case 0:
										spriteBatch.Draw(
											baseTexture,
											position,
											null,
											color,
											MathHelper.PiOver2,
											baseTexture.Size() * 0.5f,
											1,
											0,
										0);
										break;
										case 1:
										Vector2 halfSize = frame.Size() / 2;
										spriteBatch.Draw(
											wcArmorTexture.Value,
											position,
											frame,
											color,
											MathHelper.PiOver2,
											halfSize,
											1,
											0,
										0);
										break;
									}
								}
								Vector2 diff = new(0, 48);
								for (int j = 0; j < 2; j++) {
									DrawSegment(new Rectangle(168, 0, 52, 56), center + diff * 3, wcTailTexture.Value, j);
									for (int i = 3; i-- > -2;) {
										DrawSegment(new Rectangle(104, 60 * Math.Abs(i % 2), 62, 58), center + diff * i, wcBodyTexture.Value, j);
									}
									DrawSegment(new Rectangle(0, 0, 102, 58), center + diff * -3, wcHeadTexture.Value, j);
								}
								return;
							}
							DrawData MakeData(int part, Vector2 pos, float rot, SpriteEffects effects = SpriteEffects.None) {
								Texture2D texture;
								Rectangle? frame = null;
								switch (part) {
									case 0:
									texture = wcHeadTexture.Value;
									frame = texture.Frame(verticalFrames: 4);
									break;

									case 1 or 2 or 3:
									texture = wcBodyTexture.Value;
									frame = texture.Frame(3, frameX: part - 1);
									break;

									case 4:
									texture = wcTailTexture.Value;
									break;

									default:
									return default;
								}
								Vector2 origin = texture.Size() * 0.5f;
								if (frame.HasValue) origin = frame.Value.Size() * 0.5f;
								return new DrawData(texture, pos, frame, Color.White, rot, origin, 1, effects);
							}
							/*
							BCSpriteConstructor.SetupArrangement(4, center);
							DrawData[] datas = [
								..BCSpriteConstructor.parts.Select(d => MakeData(d.part, d.pos, d.rot, d.effects)),
								MakeData(BCSpriteConstructor.partNum, BCSpriteConstructor.partPos, BCSpriteConstructor.partRot, BCSpriteConstructor.partEffects)
							];/*/
							DrawData[] datas = [
								MakeData(0, new Vector2(-91, -142) + center, -0.120000005f, SpriteEffects.None),
								MakeData(1, new Vector2(2, -139) + center, 0.36f, SpriteEffects.None),
								MakeData(2, new Vector2(68, -86) + center, 1.08f, SpriteEffects.None),
								MakeData(1, new Vector2(94, -1) + center, 1.5600001f, SpriteEffects.None),
								MakeData(2, new Vector2(64, 75) + center, 2.2799997f, SpriteEffects.None),
								MakeData(3, new Vector2(-12, 125) + center, 2.7599993f, SpriteEffects.None),
								MakeData(4, new Vector2(-104, 138) + center, 3.119999f, SpriteEffects.None)
							];//*/
							for (int i = 0; i < datas.Length; i++) {
								datas[i].Draw(spriteBatch);
							}
							for (int i = 0; i < datas.Length; i++) {
								if (datas[i].texture == wcHeadTexture.Value) {
									datas[i].texture = wcHeadArmorTexture.Value;
									Rectangle frame = wcHeadArmorTexture.Frame(4, 3);
									datas[i].sourceRect = frame;
									datas[i].origin = frame.Size() * 0.5f + new Vector2(15, 8).Apply(datas[i].effect, default);
								} else if (datas[i].texture == wcBodyTexture.Value) {
									datas[i].texture = wcArmorTexture.Value;
									Rectangle frame = wcArmorTexture.Frame(3, 3, frameX: datas[i].sourceRect.Value.X / wcBodyTexture.Frame(3).Width);
									datas[i].sourceRect = frame;
									datas[i].origin = frame.Size() * 0.5f;
								} else continue;
								datas[i].Draw(spriteBatch);
							}
						}
					}
				);
				Asset<Texture2D> fwTexture = Request<Texture2D>("Origins/UI/Fiberglass_Weaver_Preview");
				Asset<Texture2D> fwAFTexture = Request<Texture2D>(typeof(Fiberglass_Weaver).GetDefaultTMLName() + "_AF");
				bossChecklist.Call("LogBoss",
					mod,
					nameof(Fiberglass_Weaver).Replace("_", ""),
					4.7f,
					() => Boss_Tracker.Instance.downedFiberglassWeaver,
					NPCType<Fiberglass_Weaver>(),
					new Dictionary<string, object> {
						["spawnInfo"] = Language.GetOrRegister("Mods.Origins.NPCs.Fiberglass_Weaver.BossChecklistIntegration.SpawnCondition"),
						["spawnItems"] = ItemType<Shaped_Glass>(),
						["collectibles"] = new List<int> {
							RelicTileBase.ItemType<Fiberglass_Weaver_Relic>(),
							TrophyTileBase.ItemType<Fiberglass_Weaver_Trophy>(),
							ItemType<Fiberglass_Weaver_Head>()
						},
						["customPortrait"] = (SpriteBatch spriteBatch, Rectangle area, Color color) => {
							SpriteBatchState state = spriteBatch.GetState();
							spriteBatch.Restart(state, samplerState: SamplerState.PointClamp);
							try {
								Texture2D tex = fwTexture.Value;
								if (CheckAprilFools()) tex = fwAFTexture.Value;

								spriteBatch.Draw(tex, area.Center(), null, color, 0, tex.Size() * 0.5f, 2, SpriteEffects.None, 0);
							} finally {
								spriteBatch.Restart(state);
							}
						}
					}
				);
				Asset<Texture2D> ldTexture = Request<Texture2D>("Origins/NPCs/Brine/Boss/Rock_Bottom");
				bossChecklist.Call("LogBoss",
					mod,
					nameof(Lost_Diver).Replace("_", ""),
					7.3f,
					() => Boss_Tracker.Instance.downedLostDiver,
					new List<int> {
						NPCType<Lost_Diver>(),
						NPCType<Lost_Diver_Transformation>(),
						NPCType<Mildew_Carrion>()
					},
					new Dictionary<string, object> {
						["spawnInfo"] = Language.GetOrRegister("Mods.Origins.NPCs.Lost_Diver.BossChecklistIntegration.SpawnCondition"),
						["spawnItems"] = ItemType<Lost_Picture_Frame>(),
						["collectibles"] = new List<int> {
							RelicTileBase.ItemType<Lost_Diver_Relic>(),
							TrophyTileBase.ItemType<Lost_Diver_Trophy>(),
							ItemType<Lost_Diver_Helmet>(),
							ItemType<Lost_Diver_Chest>(),
							ItemType<Lost_Diver_Greaves>()
						},
						["overrideHeadTextures"] = GetInstance<Lost_Diver>().BossHeadTexture,
						["customPortrait"] = (SpriteBatch spriteBatch, Rectangle area, Color color) => {
							SpriteBatchState state = spriteBatch.GetState();
							spriteBatch.Restart(state, samplerState: SamplerState.PointClamp);
							try {
								spriteBatch.Draw(ldTexture.Value, area.Center(), ldTexture.Value.Frame(1, 15, 0, 4), color, 0, ldTexture.Value.Frame(1, 15, 0, 4).Size() * 0.5f, 2, SpriteEffects.None, 0);
							} finally {
								spriteBatch.Restart(state);
							}
						}
					}
				);
				Asset<Texture2D> scTexture = Request<Texture2D>(typeof(Shimmer_Construct).GetDefaultTMLName());
				Asset<Texture2D> scAFTexture = Request<Texture2D>(typeof(Shimmer_Construct).GetDefaultTMLName() + "_AF");
				bossChecklist.Call("LogBoss",
					mod,
					nameof(Shimmer_Construct).Replace("_", ""),
					6.91f,
					() => Boss_Tracker.Instance.downedShimmerConstruct,
					NPCType<Shimmer_Construct>(),
					new Dictionary<string, object> {
						["spawnInfo"] = Language.GetOrRegister("Mods.Origins.NPCs.Shimmer_Construct.BossChecklistIntegration.SpawnCondition"),
						["collectibles"] = new List<int> {
							RelicTileBase.ItemType<Shimmer_Construct_Relic>(),
							TrophyTileBase.ItemType<Shimmer_Construct_Trophy>()
						},
						["customPortrait"] = (SpriteBatch spriteBatch, Rectangle area, Color color) => {
							SpriteBatchState state = spriteBatch.GetState();
							spriteBatch.Restart(state, samplerState: SamplerState.PointClamp);
							try {
								Texture2D tex = scTexture.Value;
								Rectangle frame = new(0, 996, 134, 166);
								float rot = 0;
								if (CheckAprilFools()) {
									tex = scAFTexture.Value;
									frame = new(0, 0, 134, 166);
									rot = MathHelper.Pi;
								}

								spriteBatch.Draw(tex, area.Center(), frame, color, rot, frame.Size() * 0.5f, 1, SpriteEffects.None, 0);
							} finally {
								spriteBatch.Restart(state);
							}
						}
					}
				);
				bossChecklist.Call("LogMiniBoss",
					mod,
					nameof(Chambersite_Sentinel).Replace("_", ""),
					7.2f,
					() => Boss_Tracker.Instance.downedChambersiteSentinel,
					NPCType<Chambersite_Sentinel>(),
					new Dictionary<string, object> {
						["spawnInfo"] = Language.GetOrRegister("Mods.Origins.NPCs.Chambersite_Sentinel.BossChecklistIntegration.SpawnCondition"),
						["overrideHeadTextures"] = "Origins/Textures/EmptySprite"
					}
				);
			}
			if (ModLoader.TryGetMod("Fargowiltas", out instance.fargosMutant)) {
				FargosMutant.Call("AddIndestructibleTileType", TileType<Fortified_Steel_Block1>());
				FargosMutant.Call("AddIndestructibleTileType", TileType<Fortified_Steel_Block2>());
				FargosMutant.Call("AddIndestructibleTileType", TileType<Fortified_Steel_Block3>());

				FargosMutant.Call("AddEvilAltar", Ashen_Altar.ID);
				FargosMutant.Call("AddEvilAltar", Defiled_Altar.ID);
				FargosMutant.Call("AddEvilAltar", Riven_Altar.ID);

				FargosMutant.Call("AddStat", (int)ItemID.Bomb, () => Language.GetTextValueWith("Mods.Origins.CrossMod.ExplosiveDamage", Math.Round(Main.LocalPlayer.GetTotalDamage(DamageClasses.Explosive).Additive * Main.LocalPlayer.GetTotalDamage(DamageClasses.Explosive).Multiplicative * 100f - 100f)));
				FargosMutant.Call("AddStat", (int)ItemID.Bomb, () => Language.GetTextValueWith("Mods.Origins.CrossMod.ExplosiveCritical", (int)Main.LocalPlayer.GetTotalCritChance(DamageClasses.Explosive)));

				FargosMutant.Call("AddPermUpgrade", ItemType<Mojo_Injection>(), () => Main.LocalPlayer.OriginPlayer().mojoInjection);
				FargosMutant.Call("AddPermUpgrade", ItemType<Crown_Jewel>(), () => Main.LocalPlayer.OriginPlayer().crownJewel);

				FargosMutant.Call("AddSummon", 3, ItemType<Distress_Beacon>(), () => NPC.downedBoss2, Item.buyPrice(gold: 10));
				FargosMutant.Call("AddSummon", 3, ItemType<Nerve_Impulse_Manipulator>(), () => NPC.downedBoss2, Item.buyPrice(gold: 10));
				FargosMutant.Call("AddSummon", 3, ItemType<Sus_Ice_Cream>(), () => NPC.downedBoss2, Item.buyPrice(gold: 10));
				FargosMutant.Call("AddSummon", 4.7, ItemType<Shaped_Glass>(), () => Boss_Tracker.Instance.downedFiberglassWeaver, Item.buyPrice(gold: 15));
				FargosMutant.Call("AddSummon", 7.3, ItemType<Lost_Picture_Frame>(), () => Boss_Tracker.Instance.downedLostDiver, Item.buyPrice(gold: 22));
				FargosMutant.Call("AddSummon", 6.8, ItemType<Aether_Orb>(), () => Boss_Tracker.Instance.downedShimmerConstruct, Item.buyPrice(gold: 18));
			}

			void AddModdedNPCAssimilation<TDebuff>(string name, AssimilationAmount assimilationAmount, HashSet<int> set = null) where TDebuff : AssimilationDebuff {
				if (NPCID.Search.TryGetId(name, out int id)) {
					set?.Add(id);
					if (assimilationAmount != default) AssimilationLoader.AddNPCAssimilation<TDebuff>(id, assimilationAmount);
				}
			}
			void AddModdedProjectileAssimilation<TDebuff>(string name, AssimilationAmount assimilationAmount, string creditNPC) where TDebuff : AssimilationDebuff {
				if (ProjectileID.Search.TryGetId(name, out int id)) {
					AssimilationLoader.AddProjectileAssimilation<TDebuff>(id, assimilationAmount);
					if (NPCID.Search.TryGetId(creditNPC, out id)) {
						BiomeNPCGlobals.assimilationDisplayOverrides.TryAdd(id, []);
						BiomeNPCGlobals.assimilationDisplayOverrides[id][GetInstance<TDebuff>().AssimilationType] = assimilationAmount;
					}
				}
			}
			void AddModdedDebuffAssimilation<TDebuff>(string name, AssimilationAmount assimilationAmount) where TDebuff : AssimilationDebuff {
				if (BuffID.Search.TryGetId(name, out int id)) {
					AssimilationLoader.AddDebuffAssimilation<TDebuff>(id, assimilationAmount);
				}
			}
			AddModdedProjectileAssimilation<Corrupt_Assimilation>("ThoriumMod/VileSpit", 0.06f, "ThoriumMod/VileFloater");
			AddModdedProjectileAssimilation<Corrupt_Assimilation>("ThoriumMod/VileSpitCold", 0.05f, "ThoriumMod/ChilledSpitter");
			AddModdedNPCAssimilation<Corrupt_Assimilation>("SpiritMod/Masticator", 0.06f, CorruptGlobalNPC.NPCTypes);
			AddModdedProjectileAssimilation<Corrupt_Assimilation>("SpiritMod/CorruptVomitProj", 0.08f, "SpiritMod/Masticator");
			AddModdedNPCAssimilation<Corrupt_Assimilation>("SpiritMod/Vilemoth", 0.04f, CorruptGlobalNPC.NPCTypes);
			AddModdedNPCAssimilation<Corrupt_Assimilation>("SpiritMod/VileWasp", 0.04f, CorruptGlobalNPC.NPCTypes);
			AddModdedNPCAssimilation<Corrupt_Assimilation>("SpiritMod/Teratoma", 0.05f, CorruptGlobalNPC.NPCTypes);
			AddModdedProjectileAssimilation<Corrupt_Assimilation>("SpiritMod/TeratomaProj", 0.07f, "SpiritMod/Teratoma");
			AddModdedNPCAssimilation<Corrupt_Assimilation>("SpiritMod/PurpleClubberfish", 0.05f, CorruptGlobalNPC.NPCTypes);
			AddModdedNPCAssimilation<Corrupt_Assimilation>("SpiritMod/Toxikarp", 0.03f, CorruptGlobalNPC.NPCTypes);
			AddModdedDebuffAssimilation<Corrupt_Assimilation>("SpiritMod/Crystallization", 0.01f / 60f);

			AddModdedNPCAssimilation<Crimson_Assimilation>("SpiritMod/Spewer", default, CrimsonGlobalNPC.NPCTypes);
			AddModdedProjectileAssimilation<Crimson_Assimilation>("SpiritMod/VomitProj", 0.08f, "SpiritMod/Spewer");
			AddModdedNPCAssimilation<Crimson_Assimilation>("SpiritMod/CrimsonTrapper", default, CrimsonGlobalNPC.NPCTypes);
			AddModdedProjectileAssimilation<Crimson_Assimilation>("SpiritMod/ArterialBloodClump", 0.08f, "SpiritMod/CrimsonTrapper");
			AddModdedNPCAssimilation<Crimson_Assimilation>("SpiritMod/Bladetongue", default, CrimsonGlobalNPC.NPCTypes);

			if (ModLoader.TryGetMod("RecipeBrowser", out Mod recipeBrowser)) {
				Type ArmorSetFeatureHelper = recipeBrowser.Code.GetType("RecipeBrowser.UIElements.ArmorSetFeatureHelper");
				MethodInfo CalculateArmorSets = ArmorSetFeatureHelper?.GetMethod("CalculateArmorSets", BindingFlags.NonPublic | BindingFlags.Static);
				if (CalculateArmorSets is not null) {
					try {
						MonoModHooks.Modify(CalculateArmorSets, il => {
							ILCursor c = new(il);
							c.GotoNext(MoveType.After,
								i => i.MatchStsfld(ArmorSetFeatureHelper, "armorSetSlots")
							);
							c.EmitLdsfld(ArmorSetFeatureHelper.GetField("sets", BindingFlags.NonPublic | BindingFlags.Static));
							c.EmitDelegate<Action<List<Tuple<Item, Item, Item, string, int>>>>(sets => {
								int hatType = ItemType<Lucky_Hat>();
								bool firstHat = true;
								sets.RemoveAll(i => {
									if (i.Item1.type == hatType) {
										if (firstHat) {
											/*string annything = Language.GetTextValue("Mods.Origins.CrossMod.RecipeBrowserAnyArmor");
											i.Item2.SetNameOverride(annything);
											i.Item3.SetNameOverride(annything);*/
											firstHat = false;
											return false;
										}
										return true;
									}
									return false;
								});
							});
						});
					} catch (Exception ex) {
						if (Origins.LogLoadingILError("RecipeBrowser_CalculateArmorSets_Fix", ex)) throw;
					}
				}
			}
			if (ModLoader.TryGetMod("ColoredDamageTypesRedux", out Mod coloredRedux)) {
				coloredRedux.Call("AddToPreset", "ColoredDamageTypesRedux/DefaultColorData", DamageClasses.Explosive.FullName, new Color(255, 121, 27), new Color(255, 100, 0));
				coloredRedux.Call("AddToPreset", "ColoredDamageTypesRedux/PillarsPreset", DamageClasses.Explosive.FullName, new Color(234, 56, 103), new Color(235, 0, 59));
			}
			if (instance.thorium is not null) {
				int ExplosivePierce = 28;
				instance.thorium.Call("TerrariumArmorAddClassFocus",
					DamageClasses.Explosive,
					new Action<Player>((plr) => plr.GetArmorPenetration<Explosive>() += ExplosivePierce),
					new Color(235, 0, 59),
					"",
					DamageClasses.Explosive.GetLocalization("ThoriumMod_TerrariumArmorAddClassFocus.Description").WithFormatArgs(ExplosivePierce)
				);

				instance.thorium.Call("BirdFeederAddFruitsToBiome", "Evil", (int[])[ItemType<Bileberry>(), ItemType<Prickly_Pear>(), ItemType<Petrified_Prickly_Pear>(), ItemType<Pawpaw>(), ItemType<Periven>()]);
				instance.thorium.Call("BirdFeederAddPotionsToBiome", "Evil", (int[])[ItemType<Fervor_Potion>(), ItemType<Protean_Potion>(), ItemType<Ambition_Potion>()]);

				instance.thorium.Call("AddFlailProjectileID", ProjectileType<Depth_Charge_P>());
				instance.thorium.Call("AddFlailProjectileID", ProjectileType<Depth_Charge_P_Alt>());
				instance.thorium.Call("AddFlailProjectileID", ProjectileType<Depth_Charge_Explosion>());

				instance.thorium.Call("AddGemStoneTileID", TileType<Chambersite_Ore>());
				instance.thorium.Call("AddGemStoneTileID", TileType<Chambersite_Ore_Ebonstone>());
				instance.thorium.Call("AddGemStoneTileID", TileType<Chambersite_Ore_Crimstone>());
				instance.thorium.Call("AddGemStoneTileID", TileType<Chambersite_Ore_Defiled_Stone>());
				instance.thorium.Call("AddGemStoneTileID", TileType<Chambersite_Ore_Riven_Flesh>());
				instance.thorium.Call("AddGemStoneTileID", TileType<Chambersite_Ore_Tainted_Stone>());

				instance.thorium.Call("AddZombieRepellentNPCID", NPCType<Conehead_Zombie>());
				instance.thorium.Call("AddZombieRepellentNPCID", NPCType<Graveshield_Zombie>());
				instance.thorium.Call("AddZombieRepellentNPCID", NPCType<Buckethead_Zombie>());
				instance.thorium.Call("AddSkeletonRepellentNPCID", NPCType<Cellarkeep>());
				instance.thorium.Call("AddSkeletonRepellentNPCID", NPCType<Catacomb_Clearer>());
				instance.thorium.Call("AddFishRepellentNPCID", NPCType<Bottomfeeder>());
				instance.thorium.Call("AddFishRepellentNPCID", NPCType<Shattered_Goldfish>());
				instance.thorium.Call("AddFishRepellentNPCID", NPCType<Shotgunfish>());
				instance.thorium.Call("AddFishRepellentNPCID", NPCType<Sea_Dragon>());
				instance.thorium.Call("AddFishRepellentNPCID", NPCType<Carpalfish>());
				instance.thorium.Call("AddFishRepellentNPCID", NPCType<Brine_Serpent_Head>());

				instance.thorium.Call("AddPlayerDoTBuffID", BuffType<Cavitation_Debuff>());
				instance.thorium.Call("AddPlayerDoTBuffID", BuffType<Mini_Static_Shock_Debuff>());
				instance.thorium.Call("AddPlayerDoTBuffID", BuffType<Static_Shock_Debuff>());
				instance.thorium.Call("AddPlayerDoTBuffID", BuffType<Static_Shock_Damage_Debuff>());
				instance.thorium.Call("AddPlayerDoTBuffID", BuffType<Shadefire_Debuff>());
				instance.thorium.Call("AddPlayerDoTBuffID", BuffType<On_Even_More_Fire>());

				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Rasterized_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Toxic_Shock_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Toxic_Shock_Strengthen_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Lousy_Liver_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Impeding_Shrapnel_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Maelstrom_Buff_Damage>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Maelstrom_Buff_Zap>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Blind_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Slow_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Electrified_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Silenced_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Soulhide_Weakened_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Mithrafin_Poison_Extend_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Scrap_Barrier_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Slag_Bucket_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Amber_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Cannihound_Lure_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Defiled_Asphyxiator_Debuff>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Defiled_Asphyxiator_Debuff_2>());
				instance.thorium.Call("AddPlayerStatusBuffID", BuffType<Defiled_Asphyxiator_Debuff_3>());
			}
			if (ModLoader.TryGetMod("InfoSlot", out Mod infoSlot)) {
				infoSlot.Call("AddInfoItem", ItemType<Eitrite_Watch>());
			}
		}
		public static void LateLoad() {
			if (ModLoader.TryGetMod("ControllerConfigurator", out Mod controllerConfigurator) && controllerConfigurator.Call("GETGOTOKEYBINDKEYBIND") is ModKeybind keybind) {
				instance.goToKeybindKeybind = keybind;
				instance.goToKeybind = (keybind) => controllerConfigurator.Call("OPENKEYBINDSTOSEARCH", keybind);
				instance.searchKeybinds = (text) => controllerConfigurator.Call("OPENKEYBINDSTOSEARCH", text);
			}
			if (ModLoader.TryGetMod("GlobalLootViewer", out Mod globalLootViewer)) {
				globalLootViewer.Call("IgnoreConditionWhenHighLighting", DropConditions.PlayerInteraction);
			}
			if (ModLoader.TryGetMod("EpikV2", out instance.epikV2)) {
				EpikV2.Call("AddModEvilBiome", GetInstance<Ashen_Biome>());
				EpikV2.Call("AddModEvilBiome", GetInstance<Defiled_Wastelands>());
				EpikV2.Call("AddModEvilBiome", GetInstance<Riven_Hive>());
				/*EpikV2.Call("AddBiomeKey",
					ItemType<Defiled_Biome_Keybrand>(),
					ItemType<Defiled_Key>(),
					TileType<Defiled_Dungeon_Chest>(),
					36,
					ItemID.CorruptionKey
				);*///just here so it can eventually be used
			}
			if (ModLoader.TryGetMod("HEROsMod", out instance.herosMod)) {
				HEROsMod.Call(
					"AddItemCategory",
					"Explosive",
					"Weapons",
					(Predicate<Item>)((Item i) => i.CountsAsClass<Explosive>())
				);
			}
			if (ModLoader.TryGetMod("FancyLighting", out instance.fancyLighting)) {
				instance.LoadFancyLighting();
			} else {
				compatRecommendations.Add(Language.GetText("Mods.Origins.ModCompatNotes.AddFancyLighting"));
			}

			conditionalCompatRecommendations.Add((() => !Lighting.NotRetro, Language.GetText("Mods.Origins.ModCompatNotes.RetroBad")));

			if (ModLoader.TryGetMod("ModDemoUtils", out Mod modDemoUtils)) {
				ItemWikiProvider itemWikiProvider = new();
				modDemoUtils.Call("RegisterDemo", Origins.instance, "Tyfyter/Origins");
				modDemoUtils.Call("AddStatProvider", Origins.instance, (Item item) => {
					return itemWikiProvider.GetStats(item.ModItem).First().Item2;
				});
			}

			if (ModLoader.TryGetMod("CalamityOverhaul", out Mod calamityOverhaul)) {
				MethodInfo HasPwoerEffect = calamityOverhaul.Code.GetType("CalamityOverhaul.Common.Effects.EffectLoader")?.GetMethod("HasPwoerEffect", BindingFlags.NonPublic | BindingFlags.Instance);
				if (HasPwoerEffect is not null) {
					MonoModHooks.Modify(HasPwoerEffect, il => {
						ILCursor c = new(il);
						ILLabel label = c.MarkLabel();
						c.MoveBeforeLabels();
						c.EmitDelegate(() => Main.gameMenu);
						c.EmitBrfalse(label);
						c.EmitLdcI4(0);
						c.EmitRet();
					});
				}
			}
			if (ModLoader.TryGetMod("Aequus", out Mod aequus)) {
				MethodInfo GetTotalStats = aequus.Code.GetType("Aequus.Common.DamageClasses.DamageClassStatFloat")?.GetMethod("GetTotalStats", BindingFlags.Public | BindingFlags.Instance, [typeof(DamageClass)]);
				if (GetTotalStats is not null) {
					MonoModHooks.Modify(GetTotalStats, il => {
						ILCursor c = new(il);
						if (c.TryGotoNext(MoveType.Before,
							i => i.MatchLdsfld(aequus.Code.GetType("Aequus.Common.DamageClasses.AequusDamageClasses"), "DamageClasses")
						)) {
							c.Remove();
							c.Index++;
							c.Remove();
							c.EmitCall(typeof(DamageClassLoader).GetMethod(nameof(DamageClassLoader.GetDamageClass)));
						}
					});
				}

				foreach (ModItem itm in aequus.GetContent<ModItem>()) {
					if (itm.GetType().Namespace == "Aequus.Content.Tiles.Paintings")
						PaintingsNotFromVendor[itm.Type] = true;
				}
			}
			if (ModLoader.TryGetMod("SpiritMod", out Mod spiritMod)) {
				if (spiritMod.Code.GetType("SpiritMod.GlobalClasses.Items.GlyphGlobalItem")?.GetMethod("CanBeAppliedTo", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) is MethodInfo CanBeAppliedTo) {
					MonoModHooks.Add(CanBeAppliedTo, (Func<Player, Item, bool> orig, Player player, Item item) => {
						if (PrefixLoader.GetPrefix(item.prefix) is ICanReforgePrefix canReforgePrefix && !canReforgePrefix.CanReforge(item)) return false;
						return orig(player, item);
					});
				}

				foreach (ModItem itm in spiritMod.GetContent<ModItem>()) {
					if (itm.GetType().Namespace == "SpiritMod.Items.Placeable.Furniture.Paintings")
						PaintingsNotFromVendor[itm.Type] = true;
				}
			}
			if (ModLoader.HasMod("ferventarms")) compatRecommendations.Add(Language.GetText("Mods.Origins.ModCompatNotes.FerventArms"));
			if (ModLoader.TryGetMod("Munchies", out Mod munchies)) {
				munchies.Call("AddSingleConsumable",
					Origins.instance,
					"1.4.2",
					GetInstance<Mojo_Injection>(),
					"player",
					() => Main.LocalPlayer.OriginPlayer().mojoInjection
				);
				munchies.Call("AddSingleConsumable",
					Origins.instance,
					"1.4.2",
					GetInstance<Crown_Jewel>(),
					"player",
					() => Main.LocalPlayer.OriginPlayer().crownJewel
				);
			}
			if (ModLoader.TryGetMod("miningcracks_take_on_luiafk", out Mod luiafk)) {
				OriginsSets.NPCs.TargetDummies[luiafk.Find<ModNPC>("Deeps").Type] = true;
			}

			conditionalCompatRecommendations.Add((
				() => !(ModLoader.HasMod("ShopExtender") || ModLoader.HasMod("ShopExpander")),
				Language.GetText("Mods.Origins.ModCompatNotes.ToManyItems" + (ModLoader.HasMod("AlchemistNPC") ? "Alch" : string.Empty))));
		}
		public static void AddRecipes() {
			if (instance.thorium is not null) AddThoriumRecipes();
			if (instance.fargosMutant is not null) AddFargosRecipes();
		}
		public static void AddRecipeGroups() {
			if (ModLoader.TryGetMod("MagicStorage", out _)) AddMagicStorageGroups();
			if (instance.thorium is not null) AddThoriumRecipeGroups();
			if (instance.fargosMutant is not null) AddFargosGroups();
		}
		public static void PostAddRecipes() {
		}
		public void Unload() {
			instance = null;
			conditionalCompatRecommendations = null;
			compatRecommendations = null;
			compatErrors = null;
		}
		public static List<(Func<bool> condition, LocalizedText text)> conditionalCompatRecommendations = [];
		public static List<LocalizedText> compatRecommendations = [];
		public static List<LocalizedText> compatErrors = [];
		[JITWhenModsEnabled("FancyLighting")]
		void LoadFancyLighting() {
			try {
				Type smoothLightingType = fancyLighting.GetType().Assembly.GetType("FancyLighting.SmoothLighting");
				//MethodInfo[] methods = smoothLightingType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				MonoModHooks.Add(
					smoothLightingType.GetMethod("TileShine", BindingFlags.NonPublic | BindingFlags.Static),
					(hook_TileShine)((orig_TileShine orig, ref Vector3 color, Tile tile) => {
						orig(ref color, tile);
						if (tile.HasTile) {
							if (TileLoader.GetTile(tile.TileType) is IGlowingModTile glowingTile) glowingTile.FancyLightingGlowColor(tile, ref color);
							switch (tile.TileType) {
								case TileID.DyePlants:
								if (tile.TileFrameX == 204 || tile.TileFrameX == 202) goto case TileID.Cactus;
								break;
								case TileID.Cactus: {
									Point pos = tile.GetTilePosition();
									WorldGen.GetCactusType(pos.X, pos.Y, tile.TileFrameX, tile.TileFrameY, out int sandType);
									if (PlantLoader.Get<ModCactus>(80, sandType) is IGlowingModPlant glowingPlant) {
										glowingPlant.FancyLightingGlowColor(tile, ref color);
									}
									break;
								}
								case TileID.VanityTreeSakura:
								case TileID.VanityTreeYellowWillow:
								break;
								default: {
									if (OriginExtensions.GetTreeType(tile) is IGlowingModTile glowingTree) {
										glowingTree.FancyLightingGlowColor(tile, ref color);
									}
									break;
								}
							}
						}
					})
				);
				/*
				int wallShineCount = 0;
				foreach (var item in smoothLightingType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
					MethodInfo wallShineMethod = null;
					if (item.GetMethod("<CalculateSmoothLightingReach>b__0", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) is MethodInfo meth0) {
						wallShineMethod = meth0;
					} else if (item.GetMethod("<CalculateSmoothLightingHiDef>b__0", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) is MethodInfo meth1) {
						wallShineMethod = meth1;
					}
					if (wallShineMethod is not null && false) {
						MonoModHooks.Modify(wallShineMethod, (il) => {
							ILCursor c = new(il);
							c.GotoNext(MoveType.Before,
								i => i.MatchCall("FancyLighting.Util.VectorToColor", "Assign")
							);
							c.EmitLdloc(4);
							c.EmitDelegate<Func<Vector3, Tile, Vector3>>((color, tile) => {
								if (WallLoader.GetWall(tile.WallType) is IGlowingModWall glowingWall) glowingWall.FancyLightingGlowColor(tile, ref color);
								return color;
							});
						});
						wallShineCount++;
					}
				}
				if (wallShineCount == 0) {
					Origins.LogLoadingWarning(Language.GetText("Mods.Origins.Warnings.FancyLightingWallShineDelegateMissing"));
				}
				//*/
				Type ambientOcclusionType = fancyLighting.GetType().Assembly.GetType("FancyLighting.AmbientOcclusion");
				//MethodInfo[] methods = smoothLightingType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				bool drawingAO = false;
				MonoModHooks.Add(
					ambientOcclusionType.GetMethod("ApplyAmbientOcclusion", BindingFlags.NonPublic | BindingFlags.Instance),
					(Func<Func<object, RenderTarget2D, bool, bool, RenderTarget2D>, object, RenderTarget2D, bool, bool, RenderTarget2D>)((Func<object, RenderTarget2D, bool, bool, RenderTarget2D> orig, object self, RenderTarget2D wallTarget, bool doDraw, bool updateWallTarget) => {
						drawingAO = true;
						RenderTarget2D value;
						try {
							value = orig(self, wallTarget, doDraw, updateWallTarget);
						} catch (Exception) {
							drawingAO = false;
							throw;
						}
						drawingAO = false;
						return value;
					})
				);
				drawingAOMap += (ref bool value) => value |= drawingAO;
			} catch (Exception e) {
				Origins.LogError("Exception thrown while loading Fancy Lighting Integration:", e);
				FancyLighting = null;
#if DEBUG
				throw;
#endif
			}
			/*for (int i = 0; i < OriginTile.IDs.Count; i++) {
				if (OriginTile.IDs[i] is IGlowingModTile glowingTile) {
					glowingTiles[OriginTile.IDs[i].Type] = true;
					glowingTileColors[OriginTile.IDs[i].Type] = glowingTile.GlowColor;
				}
			}*/
		}
		delegate void orig_TileShine(ref Vector3 color, Tile tile);
		delegate void hook_TileShine(orig_TileShine orig, ref Vector3 color, Tile tile);
		[JITWhenModsEnabled("ThoriumMod")]
		static void LoadThorium() {
			MonoModHooks.Add(
				typeof(BardItem).GetMethod("SetDefaults", BindingFlags.Public | BindingFlags.Instance),
				(Action<Action<BardItem>, BardItem>)([JITWhenModsEnabled("ThoriumMod")] (orig, self) => {
					orig(self);
					if (self is IBardDamageClassOverride classOverride) {
						self.Item.DamageType = classOverride.DamageType;
					}
				})
			);
			MonoModHooks.Add(
				typeof(BardProjectile).GetMethod("SetDefaults", BindingFlags.Public | BindingFlags.Instance),
				(Action<Action<BardProjectile>, BardProjectile>)([JITWhenModsEnabled("ThoriumMod")] (orig, self) => {
					orig(self);
					if (self is IBardDamageClassOverride classOverride) {
						self.Projectile.DamageType = classOverride.DamageType;
					}
				})
			);
			if (typeof(ThoriumPlayer).GetField(nameof(ThoriumPlayer.breathOverMax)) is not null) OriginExtensions.OnIncreaseMaxBreath += [JITWhenModsEnabled("ThoriumMod")] (player, _) => player.GetModPlayer<ThoriumPlayer>().breathOverMax = true;
			(string name, float? assimilation)[] thoriumNPCs = [
				("TheInnocent", 0.02f),
				("FrostWormHead", null),
				("FrostWormBody", null),
				("FrostWormTail", null),
				("SnowEater", 0.03f),
				("TheStarved", 0.10f),
				("HorrificCharger", 0.02f),
				("VileFloater", null),
				("ChilledSpitter", null),
				("Freezer", 0.09f),
				("SoulCorrupter", null),
			];
			for (int i = 0; i < thoriumNPCs.Length; i++) {
				(string name, float? assimilation) = thoriumNPCs[i];
				if (Thorium.TryFind(name, out ModNPC npc)) {
					CorruptGlobalNPC.NPCTypes.Add(npc.Type);
					if (assimilation.HasValue) AssimilationLoader.AddNPCAssimilation<Corrupt_Assimilation>(npc.Type, assimilation.Value);
				} else {
					Origins.LogError($"Could not find npc \"{name}\" in Thorium");
				}
			}

			thoriumNPCs = [
				("LivingHemorrhage", null),
				("Clot", null),
				("Coolmera", null),
				("FrozenFace", null),
				("BlisterPod", null),
				("Blister", 0.04f),
				("Coldling", null),
				("FrozenGross", null),
				("EpiDermon", null)
			];
			for (int i = 0; i < thoriumNPCs.Length; i++) {
				(string name, float? assimilation) = thoriumNPCs[i];
				if (Thorium.TryFind(name, out ModNPC npc)) {
					CrimsonGlobalNPC.NPCTypes.Add(npc.Type);
					if (assimilation.HasValue) AssimilationLoader.AddNPCAssimilation<Crimson_Assimilation>(npc.Type, assimilation.Value);
				} else {
					Origins.LogError($"Could not find npc \"{name}\" in Thorium");
				}
			}
		}
		[JITWhenModsEnabled("ThoriumMod")]
		static void AddThoriumRecipes() {
			ModLargeGem.AddCrossModLargeGem(GetInstance<LargeOpal>(), "ThoriumMod/Items/Misc/LargeOpal_Glow");
			ModLargeGem.AddCrossModLargeGem(GetInstance<LargeAquamarine>(), "ThoriumMod/Items/Misc/LargeAquamarine_Glow");
			ModLargeGem.AddCrossModLargeGem(GetInstance<LargePrismite>(), "ThoriumMod/Items/Misc/LargePrismite_Glow");

			Recipe.Create(ItemType<Asylum_Whistle>())
			.AddIngredient<aDarksteelAlloy>(15)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemType<Bomb_Handling_Device>())
			.AddIngredient<aDarksteelAlloy>(15)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemType<Ashen_Dungeon_Chest_Item>(), 10)
			.AddIngredient<Ashen_Key>()
			.AddIngredient(ItemID.Chest, 10)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemType<Brine_Dungeon_Chest_Item>(), 10)
			.AddIngredient<Brine_Key>()
			.AddIngredient(ItemID.Chest, 10)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemType<Defiled_Dungeon_Chest_Item>(), 10)
			.AddIngredient<Defiled_Key>()
			.AddIngredient(ItemID.Chest, 10)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemType<Riven_Dungeon_Chest_Item>(), 10)
			.AddIngredient<Riven_Key>()
			.AddIngredient(ItemID.Chest, 10)
			.AddTile(TileID.Anvils)
			.Register();

			foreach (ModItem itm in instance.thorium.GetContent<ModItem>()) {
				if (itm is not BlankPainting && itm.GetType().Namespace == "ThoriumMod.Items.Painting")
					PaintingsNotFromVendor[itm.Type] = true;
			}
			PaintingsNotFromVendor[ItemType<GrayDPaintingItem>()] = true;
		}
		[JITWhenModsEnabled("ThoriumMod")]
		static void AddThoriumRecipeGroups() {
			RecipeGroup.recipeGroups[GemPhasebladeRecipeGroupID].ValidItems.Add(ItemType<CyanPhaseblade>());
			RecipeGroup.recipeGroups[GemPhasebladeRecipeGroupID].ValidItems.Add(ItemType<PinkPhaseblade>());
		}
		[JITWhenModsEnabled(nameof(Fargowiltas))]
		static void SetFargosStaticDefaults() {
			OriginsSets.NPCs.TargetDummies[NPCType<Fargowiltas.NPCs.SuperDummy>()] = true;

			PaintingsNotFromVendor[ItemType<EchPainting>()] = true;
			PaintingsNotFromVendor[ItemType<WiresPainting>()] = true;
		}
		[JITWhenModsEnabled(nameof(Fargowiltas))]
		static void AddFargosRecipes() {
			#region Keys
			Recipe.Create(ItemType<Ashen_Key>())
				.AddRecipeGroup("Origins:AnyAshenBanner", 10)
				.AddCondition(Condition.Hardmode)
				.AddTile(TileID.Solidifier)
				.DisableDecraft()
				.Register();
			Recipe.Create(ItemType<Brine_Key>())
			   .AddRecipeGroup("Origins:AnyBrineBanner", 10)
			   .AddCondition(Condition.Hardmode)
			   .AddTile(TileID.Solidifier)
			   .DisableDecraft()
			   .Register();
			Recipe.Create(ItemType<Defiled_Key>())
				.AddRecipeGroup("Origins:AnyDefiledBanner", 10)
				.AddCondition(Condition.Hardmode)
				.AddTile(TileID.Solidifier)
				.DisableDecraft()
				.Register();
			Recipe.Create(ItemType<Riven_Key>())
				.AddRecipeGroup("Origins:AnyRivenBanner", 10)
				.AddCondition(Condition.Hardmode)
				.AddTile(TileID.Solidifier)
				.DisableDecraft()
				.Register();

			Recipe.Create(ItemType<Ashen_Torch_Item>()) // temp result until the actual ashen dungeon weapon is made
				.AddIngredient(ItemType<Ashen_Key>())
				.AddCondition(Condition.DownedPlantera)
				.AddTile(TileID.MythrilAnvil)
				.DisableDecraft()
				.Register();
			Recipe.Create(ItemType<The_Foot>())
				.AddIngredient(ItemType<Brine_Key>())
				.AddCondition(Condition.DownedPlantera)
				.AddTile(TileID.MythrilAnvil)
				.DisableDecraft()
				.Register();
			Recipe.Create(ItemType<Missing_File>())
				.AddIngredient(ItemType<Defiled_Key>())
				.AddCondition(Condition.DownedPlantera)
				.AddTile(TileID.MythrilAnvil)
				.DisableDecraft()
				.Register();
			Recipe.Create(ItemType<Plasma_Cutter>())
				.AddIngredient(ItemType<Riven_Key>())
				.AddCondition(Condition.DownedPlantera)
				.AddTile(TileID.MythrilAnvil)
				.DisableDecraft()
				.Register();
			#endregion
			#region Misc
			Recipe.Create(ItemID.MeatGrinder)
				.AddRecipeGroup("Origins:AnyAshenBanner", 5)
				.AddCondition(Condition.Hardmode)
				.AddTile(TileID.Solidifier)
				.DisableDecraft()
				.Register();
			Recipe.Create(ItemID.MeatGrinder)
				.AddRecipeGroup("Origins:AnyDefiledBanner", 5)
				.AddCondition(Condition.Hardmode)
				.AddTile(TileID.Solidifier)
				.DisableDecraft()
				.Register();
			Recipe.Create(ItemID.MeatGrinder)
				.AddRecipeGroup("Origins:AnyRivenBanner", 5)
				.AddCondition(Condition.Hardmode)
				.AddTile(TileID.Solidifier)
				.DisableDecraft()
				.Register();
			#endregion Misc
			#region Crate Recipes
			static void CrateRecipe(int result, int resultAmount = 1, int crate = 0, int crateHard = 0, int crateAmount = 1, int extraItem = 0, params Condition[] conditions) {
				if (crate > 0) {
					Recipe r = Recipe.Create(result, resultAmount)
					.AddIngredient(crate, crateAmount);
					if (extraItem > 0) r.AddIngredient(extraItem);
					r.AddTile(TileID.WorkBenches);
					foreach (Condition con in conditions) r.AddCondition(con);
					r.DisableDecraft()
					.Register();
				}
				if (crateHard > 0) {
					Recipe r = Recipe.Create(result, resultAmount)
					.AddIngredient(crateHard, crateAmount);
					if (extraItem > 0) r.AddIngredient(extraItem);
					r.AddTile(TileID.WorkBenches);
					foreach (Condition con in conditions) r.AddCondition(con);
					r.DisableDecraft()
					.Register();
				}
			}

			CrateRecipe(ItemType<Cyah_Nara>(), crate: ItemID.WoodenCrate, crateHard: ItemID.WoodenCrateHard, crateAmount: 3);
			CrateRecipe(ItemType<Bang_Snap>(), 50, ItemID.WoodenCrate, ItemID.WoodenCrateHard);
			CrateRecipe(ItemType<Woodsprite_Staff>(), crate: ItemID.WoodenCrate, crateHard: ItemID.WoodenCrateHard, crateAmount: 3);

			CrateRecipe(ItemType<Boiler>(), crate: ItemID.LavaCrate, crateHard: ItemID.LavaCrateHard, extraItem: ItemID.ShadowKey);
			CrateRecipe(ItemType<Firespit>(), crate: ItemID.LavaCrate, crateHard: ItemID.LavaCrateHard, extraItem: ItemID.ShadowKey);
			CrateRecipe(ItemType<Dragons_Breath>(), crate: ItemID.LavaCrate, crateHard: ItemID.LavaCrateHard, crateAmount: 3, extraItem: ItemID.ShadowKey);
			CrateRecipe(ItemType<Hand_Grenade_Launcher>(), crate: ItemID.LavaCrate, crateHard: ItemID.LavaCrateHard, extraItem: ItemID.ShadowKey);

			CrateRecipe(ItemType<Cryostrike>(), crate: ItemID.FrozenCrate, crateHard: ItemID.FrozenCrateHard);

			CrateRecipe(ItemType<Bomb_Charm>(), crate: ItemID.GoldenCrate, crateHard: ItemID.GoldenCrateHard);
			CrateRecipe(ItemType<Beginners_Tome>(), crate: ItemID.GoldenCrate, crateHard: ItemID.GoldenCrateHard, crateAmount: 3);
			CrateRecipe(ItemType<Nitro_Crate>(), crate: ItemID.GoldenCrate, crateHard: ItemID.GoldenCrateHard, crateAmount: 3);
			CrateRecipe(ItemType<Broken_Terratotem>(), crate: ItemID.GoldenCrate, crateHard: ItemID.GoldenCrateHard);
			CrateRecipe(ItemType<Magic_Tripwire>(), crate: ItemID.GoldenCrate, crateHard: ItemID.GoldenCrateHard, crateAmount: 3);
			CrateRecipe(ItemType<Trap_Charm>(), crate: ItemID.GoldenCrate, crateHard: ItemID.GoldenCrateHard, crateAmount: 3);

			CrateRecipe(ItemType<Tones_Of_Agony>(), crate: ItemID.DungeonFishingCrate, crateHard: ItemID.DungeonFishingCrateHard, extraItem: ItemID.GoldenKey);
			CrateRecipe(ItemType<Asylum_Whistle>(), crate: ItemID.DungeonFishingCrate, crateHard: ItemID.DungeonFishingCrateHard, crateAmount: 3, extraItem: ItemID.GoldenKey);
			CrateRecipe(ItemType<Bomb_Launcher>(), crate: ItemID.DungeonFishingCrate, crateHard: ItemID.DungeonFishingCrateHard, extraItem: ItemID.GoldenKey);
			CrateRecipe(ItemType<Bomb_Handling_Device>(), crate: ItemID.DungeonFishingCrate, crateHard: ItemID.DungeonFishingCrateHard, extraItem: ItemID.GoldenKey);

			CrateRecipe(ItemType<Desert_Crown>(), crate: ItemID.OasisCrate, crateHard: ItemID.OasisCrateHard);

			CrateRecipe(ItemType<Messy_Leech>(), crate: ItemID.JungleFishingCrate, crateHard: ItemID.JungleFishingCrateHard);

			CrateRecipe(ItemType<Huff_Puffer_Bait>(), crate: ItemType<Residual_Crate>(), crateHard: ItemType<Basic_Crate>(), crateAmount: 3);

			CrateRecipe(ItemType<Knee_Slapper>(), crateHard: ItemType<Bilious_Crate>(), crateAmount: 3);
			CrateRecipe(ItemType<Manasynk>(), crate: ItemType<Chunky_Crate>(), crateHard: ItemType<Bilious_Crate>(), crateAmount: 3);
			CrateRecipe(ItemType<Kruncher>(), crate: ItemType<Chunky_Crate>(), crateHard: ItemType<Bilious_Crate>(), crateAmount: 3);
			CrateRecipe(ItemType<Dim_Starlight>(), crate: ItemType<Chunky_Crate>(), crateHard: ItemType<Bilious_Crate>(), crateAmount: 3);
			CrateRecipe(ItemType<Monolith_Rod>(), crate: ItemType<Chunky_Crate>(), crateHard: ItemType<Bilious_Crate>(), crateAmount: 3);
			CrateRecipe(ItemType<Krakram>(), crate: ItemType<Chunky_Crate>(), crateHard: ItemType<Bilious_Crate>(), crateAmount: 3);
			CrateRecipe(ItemType<Suspicious_Looking_Pebble>(), crate: ItemType<Chunky_Crate>(), crateHard: ItemType<Bilious_Crate>(), crateAmount: 3);

			CrateRecipe(ItemType<Scabcoral_Lyre>(), crateHard: ItemType<Festering_Crate>(), crateAmount: 3);
			CrateRecipe(ItemType<Ocotoral_Bud>(), crate: ItemType<Crusty_Crate>(), crateHard: ItemType<Festering_Crate>(), crateAmount: 3);
			CrateRecipe(ItemType<Riven_Splitter>(), crate: ItemType<Crusty_Crate>(), crateHard: ItemType<Festering_Crate>(), crateAmount: 3);
			CrateRecipe(ItemType<Amebolize_Incantation>(), crate: ItemType<Crusty_Crate>(), crateHard: ItemType<Festering_Crate>(), crateAmount: 3);
			CrateRecipe(ItemType<Splitsplash>(), crate: ItemType<Crusty_Crate>(), crateHard: ItemType<Festering_Crate>(), crateAmount: 3);
			CrateRecipe(ItemType<Riverang>(), crate: ItemType<Crusty_Crate>(), crateHard: ItemType<Festering_Crate>(), crateAmount: 3);
			CrateRecipe(ItemType<Amoeba_Toy>(), crate: ItemType<Crusty_Crate>(), crateHard: ItemType<Festering_Crate>(), crateAmount: 3);
			CrateRecipe(ItemType<Primordial_Soup>(), crate: ItemType<Crusty_Crate>(), crateHard: ItemType<Festering_Crate>(), crateAmount: 3);
			#endregion

			Recipe.Create(ItemType<Surveysprout_Item>(), 5)
			.AddIngredient(ItemID.HerbBag)
			.AddTile(TileID.WorkBenches)
			.Register();
			Recipe.Create(ItemType<Wilting_Rose_Item>(), 5)
			.AddIngredient(ItemID.HerbBag)
			.AddTile(TileID.WorkBenches)
			.Register();
			Recipe.Create(ItemType<Wrycoral_Item>(), 5)
			.AddIngredient(ItemID.HerbBag)
			.AddTile(TileID.WorkBenches)
			.Register();

			SetFargosStaticDefaults();
		}
		public static void AddItemsToGroup(RecipeGroup group, params bool[] items) {
			for (int i = 0; i < items.Length; i++) {
				if (items[i]) group.ValidItems.Add(i);
			}
		}
		public static RecipeGroup GetGroup(string key) => RecipeGroup.recipeGroups[RecipeGroup.recipeGroupIDs[key]];
		[JITWhenModsEnabled("MagicStorage")]
		static void AddMagicStorageGroups() {
			AddItemsToGroup(GetGroup("MagicStorage:AnySnowBiomeBlock"), ModCompatSets.AnySnowBiomeTiles);
			AddItemsToGroup(GetGroup("MagicStorage:AnyDemonAltar"), ModCompatSets.AnyFakeDemonAltars);
			AddItemsToGroup(GetGroup("MagicStorage:AnyChest"), ModCompatSets.AnyChests);
			AddItemsToGroup(GetGroup("MagicStorage:AnyWorkBench"), ModCompatSets.AnyWorkBenches);
			AddItemsToGroup(GetGroup("MagicStorage:AnySink"), ModCompatSets.AnySinks);
			AddItemsToGroup(GetGroup("MagicStorage:AnyTable"), ModCompatSets.AnyTables);
			AddItemsToGroup(GetGroup("MagicStorage:AnyBookcase"), ModCompatSets.AnyBookcases);
			AddItemsToGroup(GetGroup("MagicStorage:AnyCampfire"), ModCompatSets.AnyCampfires);
		}
		public interface IAshenEnemy { } // move into an ashen global npc file when made
		[JITWhenModsEnabled(nameof(Fargowiltas))]
		static void AddFargosGroups() {
			static int GetBanner(int npc, bool item = false) {
				int banner = Item.NPCtoBanner(npc);
				if (item) return Item.BannerToItem(banner);
				else return banner;
			}
			int[] bannedBanners = [
				GetBanner(NPCID.PigronHallow),
				GetBanner(NPCID.DesertGhoul),
				GetBanner(NPCID.Zombie)
			];

			RecipeGroup brineGroup = new(() => Language.GetTextValue("Mods.Origins.RecipeGroups.AnyBanner", Language.GetTextValue("Mods.Origins.Biomes.Brine_Pool.DisplayName")), [ItemID.IronPickaxe]);
			RecipeGroup ashenGroup = new(() => Language.GetTextValue("Mods.Origins.RecipeGroups.AnyBanner", Language.GetTextValue("Mods.Origins.Biomes.Ashen_Biome.DisplayName")), [ItemID.IronPickaxe]);
			RecipeGroup defiledGroup = new(() => Language.GetTextValue("Mods.Origins.RecipeGroups.AnyBanner", Language.GetTextValue("Mods.Origins.Biomes.Defiled_Wastelands.DisplayName")), [ItemID.IronPickaxe]);
			RecipeGroup rivenGroup = new(() => Language.GetTextValue("Mods.Origins.RecipeGroups.AnyBanner", Language.GetTextValue("Mods.Origins.Biomes.Riven_Hive.DisplayName")), [ItemID.IronPickaxe]);
			brineGroup.ValidItems.Remove(ItemID.IronPickaxe);
			ashenGroup.ValidItems.Remove(ItemID.IronPickaxe);
			defiledGroup.ValidItems.Remove(ItemID.IronPickaxe);
			rivenGroup.ValidItems.Remove(ItemID.IronPickaxe);
			bool AllowedBanner(int type, RecipeGroup group) {
				int banner = GetBanner(type);
				bool ban = banner > 0;
				if (!(GetBanner(type) > 0)) return false;
				if (bannedBanners.Contains(GetBanner(type))) return false;
				return !group.ValidItems.Contains(GetBanner(type, true));
			}
			for (int i = 0; i < NPCID.Sets.AllNPCs.Length; i++) {
				NPC npc = ContentSamples.NpcsByNetId[i];

				switch (npc?.ModNPC) {
					case IBrinePoolNPC: {
						if (AllowedBanner(i, brineGroup))
							brineGroup.ValidItems.Add(GetBanner(i, true));
						break;
					}
					case IAshenEnemy: {
						if (AllowedBanner(i, ashenGroup))
							ashenGroup.ValidItems.Add(GetBanner(i, true));
						break;
					}
					case IDefiledEnemy: {
						if (AllowedBanner(i, defiledGroup))
							defiledGroup.ValidItems.Add(GetBanner(i, true));
						break;
					}
					case IRivenEnemy: {
						if (AllowedBanner(i, rivenGroup))
							rivenGroup.ValidItems.Add(GetBanner(i, true));
						break;
					}
				}
			}
			brineGroup.IconicItemId = brineGroup.ValidItems.First();
			ashenGroup.IconicItemId = ashenGroup.ValidItems.First();
			defiledGroup.IconicItemId = defiledGroup.ValidItems.First();
			rivenGroup.IconicItemId = rivenGroup.ValidItems.First();
			RecipeGroup.RegisterGroup("Origins:AnyBrineBanner", brineGroup);
			RecipeGroup.RegisterGroup("Origins:AnyAshenBanner", ashenGroup);
			RecipeGroup.RegisterGroup("Origins:AnyDefiledBanner", defiledGroup);
			RecipeGroup.RegisterGroup("Origins:AnyRivenBanner", rivenGroup);

			bool BannerCheck(int type, RecipeGroup group) {
				if (!(GetBanner(type) > 0)) return false;
				if (bannedBanners.Contains(GetBanner(type))) return false;

				NPC npc = ContentSamples.NpcsByNetId[type];
				if (npc?.ModNPC is null) return false;
				return npc.ModNPC.Mod is Origins && !group.ValidItems.Contains(GetBanner(type, true));
			}
			RecipeGroup corrupt = GetGroup("Fargowiltas:AnyCorrupts");
			RecipeGroup crimson = GetGroup("Fargowiltas:AnyCrimsons");
			foreach (int npc in CorruptGlobalNPC.NPCTypes) {
				if (BannerCheck(npc, corrupt)) {
					int item = GetBanner(npc, true);
					corrupt.ValidItems.Add(item);
				}
			}
			foreach (int npc in CrimsonGlobalNPC.NPCTypes) {
				if (BannerCheck(npc, crimson)) {
					int item = GetBanner(npc, true);
					crimson.ValidItems.Add(item);
				}
			}
		}
	}
	public interface ICustomWikiDestination {
		string WikiPageName { get; }
	}
	[ExtendsFromMod("ThoriumMod")]
	public class OriginsThoriumPlayer : ModPlayer {
		public bool altEmpowerment = false;
		public override void ResetEffects() {
			altEmpowerment = false;
		}
	}
	public interface IBardDamageClassOverride {
		DamageClass DamageType { get; }
	}
	[ReinitializeDuringResizeArrays]
	public class ModCompatSets {
		public static bool[] AnySnowBiomeTiles { get; } = ItemID.Sets.Factory.CreateBoolSet();
		public static bool[] AnyFakeDemonAltars { get; } = ItemID.Sets.Factory.CreateBoolSet();
		public static bool[] AnyChests { get; } = ItemID.Sets.Factory.CreateBoolSet();
		public static bool[] AnyWorkBenches { get; } = ItemID.Sets.Factory.CreateBoolSet();
		public static bool[] AnySinks { get; } = ItemID.Sets.Factory.CreateBoolSet();
		public static bool[] AnyTables { get; } = ItemID.Sets.Factory.CreateBoolSet();
		public static bool[] AnyBookcases { get; } = ItemID.Sets.Factory.CreateBoolSet();
		public static bool[] AnyCampfires { get; } = ItemID.Sets.Factory.CreateBoolSet();
	}
}
