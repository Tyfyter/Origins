using AltLibrary.Common.Systems;
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Tiles.Riven;
using Origins.World;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Utilities;
using static Origins.Items.Other.Testing.Blood_Mushroom_Soup;

namespace Origins.Items.Other.Testing {
	public class Blood_Mushroom_Soup : TestingItem {
		public static int mode;
		static int ModeCount => modes.Count;
		LinkedQueue<object> parameters = new LinkedQueue<object>();
		Vector2 basePosition;
		public static List<WorldgenTestingMode> modes = [];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 0;
			modes = new TopoSort<WorldgenTestingMode>(modes,
				mode => mode.Order.After is WorldgenTestingMode v ? [v] : [],
				mode => mode.Order.Before is WorldgenTestingMode v ? [v] : []
			).Sort();

		}
		public override void SetDefaults() {
			Item.width = 16;
			Item.height = 26;
			Item.value = 25000;
			Item.rare = ItemRarityID.Green;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useAnimation = 10;
			Item.useTime = 10;
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool? UseItem(Player player) {
			if (Main.myPlayer == player.whoAmI) {
				if (player.altFunctionUse == 2) {
					parameters.Clear();
					bool loopGuard = true;
					skipContinue:
					if (player.controlSmart) {
						mode = (mode + ModeCount - 1) % ModeCount;
					} else {
						mode = (mode + 1) % ModeCount;
					}
					if (loopGuard) {
						loopGuard = false;
						if (modes[mode] is Continue_Worldgen_Testing_Mode) goto skipContinue;
					}
				} else {
					if (player.controlSmart) {
						modes[mode].Apply(parameters);
					} else if (player.controlDown) {
						if (parameters.Count > 0) parameters.RemoveAt(parameters.Count - 1);
					} else {
						Point mousePos = new((int)(Main.MouseScreen.X / 16), (int)(Main.MouseScreen.Y / 16));
						int mousePacked = mousePos.X + (Main.screenWidth / 16) * mousePos.Y;
						double mousePackedDouble = (Main.MouseScreen.X / 16d + (Main.screenWidth / 16d) * Main.MouseScreen.Y / 16d) / 16d;
						Tile mouseTile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
						Vector2 diffFromPlayer = Main.MouseWorld - Main.LocalPlayer.MountedCenter;
						modes[mode].SetParameter(parameters, mousePos, mousePacked, mousePackedDouble, mouseTile, diffFromPlayer);
					}
				}
				return true;
			}
			return false;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (Main.LocalPlayer.HeldItem.type == Item.type) {
				Point mousePos = new((int)(Main.MouseScreen.X / 16), (int)(Main.MouseScreen.Y / 16));
				int mousePacked = mousePos.X + (Main.screenWidth / 16) * mousePos.Y;
				double mousePackedDouble = (Main.MouseScreen.X / 16d + (Main.screenWidth / 16d) * Main.MouseScreen.Y / 16d) / 16d;
				Tile mouseTile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
				PlayerInput.SetZoom_MouseInWorld();
				Vector2 diffFromPlayer = Main.MouseWorld - Main.LocalPlayer.MountedCenter;
				Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, modes[mode].GetMouseText(parameters.Count, mousePos, mousePacked, mousePackedDouble, mouseTile, diffFromPlayer), Main.MouseScreen.X, Math.Max(Main.MouseScreen.Y - 24, 18), Colors.RarityNormal, Color.Black, new Vector2(0f));
				if (Main.LocalPlayer.controlLeft && Main.LocalPlayer.controlRight && Main.LocalPlayer.controlUp && Main.LocalPlayer.controlDown) {
					int O = 0;
					int OwO = 0 / O;
				}
				PlayerInput.SetZoom_UI();
			}
		}
		/*
		void SetParameter() {
			switch (((Mode)mode, parameters.Count)) {
				case (BrinePool_Opening, 0):
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				Apply();
				break;
				break;
				case (SpreadRivenGrass, 0):
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				Apply();
				break;
				case (BrinePool_Start, 0):
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				Apply();
				break;
				case (BrinePool_SmallCave, 0):
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				basePosition = new Vector2(Player.tileTargetX, Player.tileTargetY);
				break;
				case (BrinePool_SmallCave, 2):
				parameters.Enqueue((new Vector2(Player.tileTargetX, Player.tileTargetY) - basePosition).Length() / 35f);
				break;
				case (BrinePool_SmallCave, 3):
				parameters.Enqueue((new Vector2(Player.tileTargetX, Player.tileTargetY) - basePosition) / 10f);
				break;
				case (FiberglassStart, 0):
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				Apply();
				break;
				case (WFCTest, 0):
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				break;
				case (WFCTest, 1):
				parameters.Enqueue(Player.tileTargetY);
				break;
				case (WFCTest, 2):
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				Apply();
				break;
				case (WFCTest, 3):
				parameters.Enqueue(Player.tileTargetY);
				Apply();
				break;
				case (DefiledFissure, 0):
				parameters.Enqueue(new Point(Player.tileTargetX, Player.tileTargetY));
				Apply();
				break;
				case (DefiledVeinRunner, 0):
				parameters.Enqueue(new Point(Player.tileTargetX, Player.tileTargetY));
				break;
				case (DefiledVeinRunner, 1):
				parameters.Enqueue(Math.Sqrt(mousePackedDouble / 16));
				break;
				case (DefiledVeinRunner, 2):
				parameters.Enqueue(Main.MouseWorld);
				break;
				case (DefiledVeinRunner, 3):
				parameters.Enqueue(mousePackedDouble);
				break;
				case (DefiledVeinRunner, 4):
				parameters.Enqueue(Math.Sqrt(mousePackedDouble / 16));
				break;
				case (DefiledVeinRunner, 5):
				parameters.Enqueue(Main.LocalPlayer.controlUp ? 0 : diffFromPlayer.ToRotation());
				break;
				case (DefiledVeinRunner, 6):
				parameters.Enqueue(Main.MouseScreen.Y > Main.screenHeight / 2f);
				break;
				case (DefiledVeinRunner, 7):
				Apply();
				break;
				case (DefiledRibs, 0):
				parameters.Enqueue(Main.MouseWorld.X / 16);
				parameters.Enqueue(Main.MouseWorld.Y / 16);
				Apply();
				break;
				case (StartRivenHive, 0):
				case (RivenHiveCave_Old, 0):
				case (BrinePool_Old, 0):
				case (StartDefiled, 0):
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				Apply();
				break;
				case (VeinRunner_Branching, 0):
				case (VeinRunner, 0):
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				break;
				case (VeinRunner_Branching, 1):
				case (VeinRunner, 1):
				parameters.Enqueue(Player.tileTargetY);
				break;
				case (VeinRunner_Branching, 2):
				case (VeinRunner, 2):
				parameters.Enqueue(Math.Sqrt(mousePackedDouble / 16));
				break;
				case (VeinRunner_Branching, 3):
				case (VeinRunner, 3):
				parameters.Enqueue(diffFromPlayer / 16);
				break;
				case (VeinRunner_Branching, 4):
				case (VeinRunner, 4):
				parameters.Enqueue(mousePackedDouble);
				break;
				case (VeinRunner_Branching, 5):
				case (VeinRunner, 5):
				parameters.Enqueue(Main.LocalPlayer.controlUp ? 0 : diffFromPlayer.ToRotation());
				break;
				case (VeinRunner_Branching, 6):
				case (VeinRunner, 6):
				parameters.Enqueue(Main.MouseScreen.Y > Main.screenHeight / 2f);
				break;
				case (VeinRunner_Branching, 7):
				parameters.Enqueue((byte)((mousePacked / 16) % 256));
				break;

				case (Continue, 1):
				Apply();
				break;
			}
		}
		string GetMouseText() {
			Point mousePos = new Point((int)(Main.MouseScreen.X / 16), (int)(Main.MouseScreen.Y / 16));
			int mousePacked = mousePos.X + (Main.screenWidth / 16) * mousePos.Y;
			double mousePackedDouble = (Main.MouseScreen.X / 16d + (Main.screenWidth / 16d) * Main.MouseScreen.Y / 16d) / 16d;
			Vector2 diffFromPlayer = Main.MouseWorld - Main.LocalPlayer.MountedCenter;
			switch (((Mode)mode, parameters.Count)) {
				case (BrinePool_Opening, 0):
				return $"place brine pool opening: {Player.tileTargetX}, {Player.tileTargetY}";
				case (RavelHole, 0):
				return $"ravel hole start point: {Player.tileTargetX}, {Player.tileTargetY}";
				case (RavelHole, 1):
				return $"ravel hole end point: {Player.tileTargetX}, {Player.tileTargetY}";
				case (SpreadRivenGrass, 0):
				return $"spread riven grass: {Player.tileTargetX}, {Player.tileTargetY}";
				case (BrinePool_Start, 0):
				return $"place brine pool start: {Player.tileTargetX}, {Player.tileTargetY}";
				case (BrinePool_SmallCave, 0):
				return $"place brine cave start: {Player.tileTargetX}, {Player.tileTargetY}";
				case (BrinePool_SmallCave, 2):
				return $"brine cave scale: {(new Vector2(Player.tileTargetX, Player.tileTargetY) - basePosition).Length() / 35f}";
				case (BrinePool_SmallCave, 3):
				return $"brine cave stretch: {(new Vector2(Player.tileTargetX, Player.tileTargetY) - basePosition) / 10f}";
				case (FiberglassStart, 0):
				return "start fiberglass undergrowth";
				case (WFCTest, 0):
				return "place WFC test point 1";
				case (WFCTest, 2):
				return "place WFC test point 2";
				case (DefiledFissure, 0):
				return "place defiled fissure";
				case (DefiledVeinRunner, 0):
				return "defiled vein position";
				case (DefiledVeinRunner, 1):
				return "defiled vein strength: " + Math.Sqrt(mousePackedDouble / 16);
				case (DefiledVeinRunner, 2):
				return "defiled vein target";
				case (DefiledVeinRunner, 3):
				return "defiled vein length: " + mousePackedDouble;
				case (DefiledVeinRunner, 4):
				return "defiled vein wall thickness: " + Math.Sqrt(mousePackedDouble / 16);
				case (DefiledVeinRunner, 5):
				return "defiled vein twist: " + (Main.LocalPlayer.controlUp ? 0 : (double)diffFromPlayer.ToRotation());
				case (DefiledVeinRunner, 6):
				return "defiled vein twist randomization: " + (Main.MouseScreen.Y > Main.screenHeight / 2f);
				case (DefiledVeinRunner, 7):
				return "start defiled vein";
				case (DefiledRibs, 0):
				return "place defiled stone ring";
				case (StartDefiled, 0):
				return "place defiled start";
				case (BrinePool_Old, 0):
				return "place brine pool";
				case (RivenHiveCave_Old, 0):
				return "place riven cave";
				case (StartRivenHive, 0):
				return "place riven start";
				case (VeinRunner_Branching, 0):
				case (VeinRunner, 0):
				return $"i,j: {Player.tileTargetX}, {Player.tileTargetY}";
				case (VeinRunner_Branching, 1):
				case (VeinRunner, 1):
				return $"j: {Player.tileTargetY}";
				case (VeinRunner_Branching, 2):
				case (VeinRunner, 2):
				return $"strength: {mousePackedDouble / 16}";
				case (VeinRunner_Branching, 3):
				case (VeinRunner, 3):
				return $"speed: {diffFromPlayer / 16}";
				case (VeinRunner_Branching, 4):
				case (VeinRunner, 4):
				return $"length: {mousePackedDouble}";
				case (VeinRunner_Branching, 5):
				case (VeinRunner, 5):
				return $"twist: {(Main.LocalPlayer.controlUp ? 0 : (double)diffFromPlayer.ToRotation())}";
				case (VeinRunner_Branching, 6):
				case (VeinRunner, 6):
				return $"random twist: {Main.MouseScreen.Y > Main.screenHeight / 2f}";
				case (VeinRunner_Branching, 7):
				return $"branch count (optional): {(byte)((mousePacked / 16) % 256)}";

				case (Continue, 1):
				return $"continue";
				//return $":{}";
			}
			return "";
		}
		void Apply() {
			switch ((Mode)mode) {
				case VeinRunner_Branching: {
					int i = (int)parameters.Dequeue();
					int j = (int)parameters.Dequeue();
					double strength = (double)parameters.Dequeue();
					Vector2 speed = (Vector2)parameters.Dequeue();
					Stack<((Vector2, Vector2), byte)> veins = new Stack<((Vector2, Vector2), byte)>();
					double length = (double)parameters.Dequeue();
					float twist = (float)parameters.Dequeue();
					bool twistRand = (bool)parameters.Dequeue();
					veins.Push(((new Vector2(i, j), speed), (parameters.Count > 0 ? (byte)parameters.Dequeue() : (byte)10)));
					((Vector2 p, Vector2 v) v, byte count) curr;
					(Vector2 p, Vector2 v) ret;
					byte count;
					while (veins.Count > 0) {
						curr = veins.Pop();
						count = curr.count;
						ret = GenRunners.VeinRunner(
							i: (int)curr.v.p.X,
							j: (int)curr.v.p.Y,
							strength: strength,
							speed: curr.v.v,
							length: length,
							twist: twist,
							randomtwist: twistRand);
						if (count > 0 && Main.rand.NextBool(3)) {
							veins.Push(((ret.p, ret.v.RotatedBy(Main.rand.NextBool() ? -1 : 1)), (byte)Main.rand.Next(--count)));
						}
						if (count > 0) {
							veins.Push(((ret.p, ret.v.RotatedByRandom(0.05)), --count));
						}
					}
					break;
				}
				case StartRivenHive:
				Riven_Hive.Gen.StartHive((int)parameters.Dequeue(), (int)parameters.Dequeue());
				break;
				case RivenHiveCave_Old:
				Riven_Hive.Gen.HiveCave_Old((int)parameters.Dequeue(), (int)parameters.Dequeue());
				break;
				case BrinePool_Old:
				Brine_Pool.Gen.BrineStart_Old((int)parameters.Dequeue(), (int)parameters.Dequeue());
				break;
				case StartDefiled:
				Defiled_Wastelands.Gen.StartDefiled((int)parameters.Dequeue(), (int)parameters.Dequeue());
				break;
				case DefiledRibs: {
					Vector2 a = new Vector2((float)parameters.Dequeue(), (float)parameters.Dequeue());
					Defiled_Wastelands.Gen.DefiledRibs((int)a.X, (int)a.Y);
					for (int i = (int)a.X - 1; i < (int)a.X + 3; i++) {
						for (int j = (int)a.Y - 2; j < (int)a.Y + 2; j++) {
							Main.tile[i, j].SetActive(false);
						}
					}
					TileObject.CanPlace((int)a.X, (int)a.Y, (ushort)ModContent.TileType<Tiles.Defiled.Defiled_Heart>(), 0, 1, out var data);
					TileObject.Place(data);
					break;
				}
				case RemoveTree:
				break;
				case DefiledVeinRunner: {
					Point pos = (Point)parameters.Dequeue();
					double strength = (double)parameters.Dequeue();
					Vector2 speed = (((Vector2)parameters.Dequeue()) - pos.ToVector2() * 16).SafeNormalize(Vector2.UnitY);
					double length = (double)parameters.Dequeue();
					double wallThickness = (double)parameters.Dequeue();
					float twist = (float)parameters.Dequeue();
					bool twistRand = (bool)parameters.Dequeue();
					Defiled_Wastelands.Gen.DefiledVeinRunner(
						pos.X, pos.Y,
						strength,
						speed,
						length,
						(ushort)ModContent.TileType<Tiles.Defiled.Defiled_Stone>(),
						(float)wallThickness,
						twist,
						twistRand,
						(ushort)ModContent.WallType<Walls.Defiled_Stone_Wall>()
					);
					break;
				}
				case DefiledFissure: {
					ushort stoneID = (ushort)ModContent.TileType<Tiles.Defiled.Defiled_Stone>();
					ushort fissureID = (ushort)ModContent.TileType<Tiles.Defiled.Defiled_Fissure>();
					Point pos = (Point)parameters.Dequeue();
					for (int oY = 1; oY < 2; oY++) {
						for (int o = 0; o > -5; o = o > 0 ? -o : -o + 1) {
							Point p = pos;
							int loop = 0;
							for (; !Main.tile[p.X + o - 1, p.Y + oY].HasTile || !Main.tile[p.X + o, p.Y + oY].HasTile; p.Y++) {
								if (++loop > 10) {
									break;
								}
							}
							WorldGen.KillTile(p.X + o - 1, p.Y - 1);
							WorldGen.KillTile(p.X + o, p.Y - 1);
							WorldGen.KillTile(p.X + o - 1, p.Y);
							WorldGen.KillTile(p.X + o, p.Y);
							WorldGen.PlaceTile(p.X + o - 1, p.Y + 1, stoneID);
							WorldGen.PlaceTile(p.X + o, p.Y + 1, stoneID);
							WorldGen.SlopeTile(p.X + o - 1, p.Y + 1, SlopeID.None);
							WorldGen.SlopeTile(p.X + o, p.Y + 1, SlopeID.None);
							if (TileObject.CanPlace(p.X + o, p.Y, fissureID, 0, 0, out TileObject to)) {
								WorldGen.Place2x2(p.X + o, p.Y, fissureID, 0);
								break;
							}
						}
					}
					break;
				}
				case WFCTest: {
					int x1 = (int)parameters.Dequeue();
					int y1 = (int)parameters.Dequeue();
					int x2 = (int)parameters.Dequeue();
					int y2 = (int)parameters.Dequeue();
					for (int i = x1; i < x2; i++) {
						for (int j = y1; j < y2; j++) {
							Framing.GetTileSafely(i, j).ResetToType(TileID.StoneSlab);
						}
					}
					WorldGen.RangeFrame(x1, y1, x2, y2);
					x2 -= x1;
					if (x2 < 0) {
						x1 += x2;
						x2 = -x2;
					}
					x2++;
					y2 -= y1;
					if (y2 < 0) {
						y1 += y2;
						y2 = -y2;
					}
					y2++;
					ushort mask_none = 0b011;
					ushort mask_full = 0b101;
					WaveFunctionCollapse.Generator<BlockType> generator = new(x2, y2, cellTypes: [
						new(new(BlockType.SlopeDownLeft,  mask_none, mask_full, mask_full, mask_none), 1),
						new(new(BlockType.SlopeDownRight, mask_none, mask_none, mask_full, mask_full), 1),
						new(new(BlockType.SlopeUpLeft,    mask_full, mask_full, mask_none, mask_none), 1),
						new(new(BlockType.SlopeUpRight,   mask_full, mask_none, mask_none, mask_full), 1)
					]);
					/*mode = -1;
					parameters.Enqueue((Func<bool>)(() => {
						return generator.CollapseStepWith(
							(int i, int j, BlockType type) => {
								Main.NewText(type);
								Framing.GetTileSafely(i + x1, j + y1).BlockType = type;
							}
						);
					}));* /
					//generator.Force(0, 0, new(BlockType.SlopeDownLeft, mask_none, mask_full, mask_full, mask_none));
					retry:
					try {
						generator.Collapse();
					} catch (Exception) {
						goto retry;
					}
					for (int i = 0; i < x2; i++) {
						for (int j = 0; j < y2; j++) {
							Framing.GetTileSafely(i + x1, j + y1).BlockType = generator.GetActual(i, j);
							if (Framing.GetTileSafely(i + x1, j + y1).BlockType == (BlockType)6) {
								Framing.GetTileSafely(i + x1, j + y1).HasTile = false;
							}
						}
					}
					break;
				}
				case FiberglassStart: {
					Fiberglass_Undergrowth.Gen.FiberglassStart((int)parameters.Dequeue(), (int)parameters.Dequeue());
					break;
				}
				case BrinePool_SmallCave: {
					Brine_Pool.Gen.SmallCave(
						(int)parameters.Dequeue(),
						(int)parameters.Dequeue(),
						(float)parameters.Dequeue(),
						(Vector2)parameters.Dequeue()
					);
					break;
				}
				case BrinePool_Start: {
					Brine_Pool.Gen.BrineStart((int)parameters.Dequeue(), (int)parameters.Dequeue());
					break;
				}
				case SpreadRivenGrass: {
					Riven_Hive.Gen.SpreadRivenGrass((int)parameters.Dequeue(), (int)parameters.Dequeue());
					break;
				}
				case BrinePool_Opening: {
					GenRunners.OpeningRunner(
						(int)parameters.Dequeue(), (int)parameters.Dequeue(),
						Main.rand.NextFloat(4, 6),
						Main.rand.NextFloat(0.95f, 1.2f),
						-Vector2.UnitY.RotatedByRandom(0.15f),
						75
					);
					break;
				}
				case StartLimestone: {
					Limestone_Cave.Gen.StartLimestone((int)parameters.Dequeue(), (int)parameters.Dequeue());
					break;
				}
}
		}
		*/
	}
	public abstract class WorldgenTestingMode : ILoadable {
		public record struct SortOrder(WorldgenTestingMode After = null, WorldgenTestingMode Before = null) {
			public static SortOrder New => new(ModContent.GetInstance<Remove_Tree_Testing_Mode>(), ModContent.GetInstance<Continue_Worldgen_Testing_Mode>());
			public static SortOrder Default => new(ModContent.GetInstance<Continue_Worldgen_Testing_Mode>());
		}
		bool gotOrder;
		SortOrder order;
		internal SortOrder Order {
			get {
				if (!gotOrder) {
					gotOrder = true;
					order = SortPosition;
				}
				return order;
			}
		}
		public virtual SortOrder SortPosition => SortOrder.Default;
		public abstract string GetMouseText(int parameterCount, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer);
		public abstract void SetParameter(LinkedQueue<object> parameters, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer);
		public abstract void Apply(LinkedQueue<object> parameters);
		public void Load(Mod mod) {
			modes.Add(this);
		}
		public void Unload() { }
	}
	public class Structure_Testing_Mode : WorldgenTestingMode {
		public override SortOrder SortPosition => SortOrder.New;
		public override string GetMouseText(int parameterCount, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) => "Place Structure";
		public override void SetParameter(LinkedQueue<object> parameters, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {
			parameters.Enqueue(Player.tileTargetX);
			parameters.Enqueue(Player.tileTargetY);
			Apply(parameters);
		}
		public override void Apply(LinkedQueue<object> parameters) {
			ModContent.GetInstance<TestStructure>().Generate((int)parameters.Dequeue(), (int)parameters.Dequeue());
		}
	}
	public class List_Worldgen_Testing_Mode : WorldgenTestingMode {
		public override string GetMouseText(int parameterCount, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) => "List World Generation Sources";
		public override void SetParameter(LinkedQueue<object> parameters, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {
			Apply(parameters);
		}
		public override void Apply(LinkedQueue<object> parameters) {
			System.Reflection.FieldInfo field = typeof(SystemLoader).GetField("HookModifyWorldGenTasks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			System.Reflection.FieldInfo defaultInstances = field.FieldType.GetField("defaultInstances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			ReadOnlySpan<ModSystem> readOnlySpan = (ModSystem[])defaultInstances.GetValue(field.GetValue(null));
			for (int i = 0; i < readOnlySpan.Length; i++) {
				Main.NewText(readOnlySpan[i]);
			}
		}
	}
	public class Area_Analysis_Testing_Mode : WorldgenTestingMode {
		public override string GetMouseText(int parameterCount, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) => "Analyze";
		public override void SetParameter(LinkedQueue<object> parameters, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {
			parameters.Enqueue(Player.tileTargetX);
			parameters.Enqueue(Player.tileTargetY);
			Apply(parameters);
		}
		public override void Apply(LinkedQueue<object> parameters) {
			int x = (int)parameters.Dequeue();
			int y = (int)parameters.Dequeue();
			Point[] directions = [
				new(1, 0), new(-1, 0),
				new(2, 0), new(-2, 0),
				new(0, 1), new(0, -1)
			];
			ushort fleshBlockType = (ushort)ModContent.TileType<Spug_Flesh>();

			if (AreaAnalysis.March(x, y, directions, pos => Math.Abs(pos.Y - y) < 20 && Framing.GetTileSafely(pos).TileIsType(fleshBlockType), a => a.MaxX - a.MinX >= 100).Broke) {
				Framing.GetTileSafely(x, y).TileType = TileID.AmberGemspark;
			}
		}
	}
	public class Shelf_Coral_Testing_Mode : WorldgenTestingMode {
		public override string GetMouseText(int parameterCount, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {
			StringBuilder text = new("Shelf Coral placement: ");
			Point coralPos = new(Player.tileTargetX, Player.tileTargetY);
			Shelf_Coral shelfCoral = ModContent.GetInstance<Shelf_Coral>();
			for (int k = 0; k < shelfCoral.patterns.Length; k++) {
				if (shelfCoral.patterns[k].pattern.Matches(coralPos) && (shelfCoral.patterns[k].extraChecks?.Invoke(Player.tileTargetX, Player.tileTargetY, true) ?? true)){
					text.Append(shelfCoral.patterns[k].name);
					return text.ToString();
				}
			}
			text.Append("None");
			return text.ToString();
		}
		public override void SetParameter(LinkedQueue<object> parameters, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {
			parameters.Enqueue(Player.tileTargetX);
			parameters.Enqueue(Player.tileTargetY);
			Apply(parameters);
		}
		public override void Apply(LinkedQueue<object> parameters) {
			Point coralPos = new((int)parameters.Dequeue(), (int)parameters.Dequeue());
			if (TileExtenstions.CanActuallyPlace(coralPos.X, coralPos.Y, ModContent.TileType<Shelf_Coral>(), 0, 0, out TileObject objectData, onlyCheck: false) && TileObject.Place(objectData)) {
				TileObjectData.CallPostPlacementPlayerHook(coralPos.X, coralPos.Y, ModContent.TileType<Shelf_Coral>(), objectData.style, 0, objectData.alternate, objectData);
			}
		}
	}
	public class Start_Riven_Testing_Mode : WorldgenTestingMode {
		public override string GetMouseText(int parameterCount, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) => "Start Riven Hive";
		public override void SetParameter(LinkedQueue<object> parameters, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {
			parameters.Enqueue(Player.tileTargetX);
			parameters.Enqueue(Player.tileTargetY);
			Apply(parameters);
		}
		public override void Apply(LinkedQueue<object> parameters) {
			WorldBiomeGeneration.ChangeRange.ResetRange();
			Riven_Hive.Gen.StartHive((int)parameters.Dequeue(), (int)parameters.Dequeue());
		}
	}
	public class Carver_Testing_Mode : WorldgenTestingMode {
		public override string GetMouseText(int parameterCount, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {
			Carver.DoCarve(
				GetFilter(out Vector2 posMin, out Vector2 posMax),
				pos => {
					Dust.NewDustPerfect(pos * 16 + Vector2.One * 8, 29, Vector2.Zero).noGravity = true;
					return 1;
				},
				posMin,
				posMax,
				0
			);
			posMin = new(MathF.Floor(posMin.X), MathF.Floor(posMin.Y));
			posMax = new(MathF.Ceiling(posMax.X), MathF.Ceiling(posMax.Y));
			Dust.NewDustPerfect(posMin * 16 + Vector2.One * 8, 6, Vector2.Zero).noGravity = true;
			Dust.NewDustPerfect(posMax * 16 + Vector2.One * 8, 6, Vector2.Zero).noGravity = true;
			return "";
		}
		public override void SetParameter(LinkedQueue<object> parameters, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {
			Apply(parameters);
		}
		static int seed = 0;
		static Carver.Filter GetFilter(out Vector2 posMin, out Vector2 posMax) {
			posMin = new(float.PositiveInfinity);
			posMax = new(float.NegativeInfinity);
			PlayerInput.SetZoom_MouseInWorld();
			UnifiedRandom genRand = new(seed);

			return Carver.Climb(Main.MouseWorld / 16, pos => {
				if (!OriginExtensions.IsTileReplacable((int)pos.X, (int)pos.Y)) return false;
				Tile tile = Framing.GetTileSafely(pos.ToPoint());
				return tile.HasTile && TileID.Sets.Falling[tile.TileType];
			}, ref posMin, ref posMax);/*Carver.PointyLemon(// tweak to change the shape and size of the barnacled areas
				Main.MouseWorld / 16,
				scale: genRand.Next(5, 15),
				rotation: genRand.NextFloat(0, MathHelper.TwoPi),
				aspectRatio: 1.1f,
				0.5f,
				ref posMin,
				ref posMax
			);*/
		}
		public override void Apply(LinkedQueue<object> parameters) {
			ushort oreID = (ushort)ModContent.TileType<Amoeba_Fluid>();
			Carver.DoCarve(
				GetFilter(out Vector2 posMin, out Vector2 posMax),
				pos => {
					Tile tile = Framing.GetTileSafely(pos.ToPoint());
					tile.HasTile = true;
					tile.TileType = oreID;
					return 1;
				},
				posMin,
				posMax,
				0
			);
			posMin = new(MathF.Floor(posMin.X), MathF.Floor(posMin.Y));
			posMax = new(MathF.Ceiling(posMax.X), MathF.Ceiling(posMax.Y));
			WorldGen.RangeFrame((int)posMin.X, (int)posMin.Y, (int)posMax.X, (int)posMax.Y);
			seed++;
		}
	}
	public class Auto_Slope_Testing_Mode : WorldgenTestingMode {
		public override string GetMouseText(int parameterCount, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) => "Auto Slope";
		public override void SetParameter(LinkedQueue<object> parameters, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {
			parameters.Enqueue(Player.tileTargetX);
			parameters.Enqueue(Player.tileTargetY);
			Apply(parameters);
		}
		public override void Apply(LinkedQueue<object> parameters) {
			GenRunners.AutoSlope((int)parameters.Dequeue(), (int)parameters.Dequeue(), true);
		}
	}
	public class Ravel_Hole_Testing_Mode : WorldgenTestingMode {
		public override string GetMouseText(int parameterCount, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) =>
			$"ravel hole {(parameterCount > 0 ? "end" : "start")} point: {Player.tileTargetX}, {Player.tileTargetY}";
		public override void SetParameter(LinkedQueue<object> parameters, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {
			parameters.Enqueue(Main.MouseWorld);
			if (parameters.Count >= 2) Apply(parameters);
		}
		public override void Apply(LinkedQueue<object> parameters) {
			Defiled_Wastelands.Gen.RavelConnection((Vector2)parameters.Dequeue(), (Vector2)parameters.Dequeue());
		}
	}
	public class Start_Limestone_Testing_Mode : WorldgenTestingMode {
		public override string GetMouseText(int parameterCount, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) => "Start Limestone Cave";
		public override void SetParameter(LinkedQueue<object> parameters, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {
			parameters.Enqueue(Player.tileTargetX);
			parameters.Enqueue(Player.tileTargetY);
			Apply(parameters);
		}
		public override void Apply(LinkedQueue<object> parameters) {
			Limestone_Cave.Gen.StartLimestone((int)parameters.Dequeue(), (int)parameters.Dequeue());
		}
	}
	public class Spike_Testing_Mode : WorldgenTestingMode {
		public override string GetMouseText(int parameterCount, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {
			switch (parameterCount) {
				case 0:
				return $"i,j: {Player.tileTargetX}, {Player.tileTargetY}";
				case 1:
				return $"j: {Player.tileTargetY}";
				case 2:
				return $"strength: {mousePackedDouble / 16}";
				case 3:
				return $"speed: {diffFromPlayer / 16}";
				case 4:
				return $"decay: {mousePackedDouble / 64}";
				case 5:
				return $"twist: {(Main.LocalPlayer.controlUp ? 0 : (double)diffFromPlayer.ToRotation())}";
				case 6:
				return "twist randomization: " + (Main.MouseScreen.Y > Main.screenHeight / 2f);
				case 7:
				return "smooth: " + (Main.MouseScreen.Y > Main.screenHeight / 2f);
				case 8:
				return "cutoff strength: " + mousePackedDouble / 16;
			}
			return "place";
		}
		public override void SetParameter(LinkedQueue<object> parameters, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {
			switch (parameters.Count) {
				case 0:
				parameters.Enqueue(Player.tileTargetX);
				parameters.Enqueue(Player.tileTargetY);
				return;
				case 1:
				parameters.Enqueue(Player.tileTargetY);
				return;
				case 2:
				parameters.Enqueue(Math.Sqrt(mousePackedDouble / 16));
				return;
				case 3:
				parameters.Enqueue(diffFromPlayer / 16);
				return;
				case 4:
				parameters.Enqueue(mousePackedDouble / 64);
				return;
				case 5:
				parameters.Enqueue(Main.LocalPlayer.controlUp ? 0 : diffFromPlayer.ToRotation());
				return;
				case 6:
				parameters.Enqueue(Main.MouseScreen.Y > Main.screenHeight / 2f);
				return;
				case 7:
				parameters.Enqueue(Main.MouseScreen.Y > Main.screenHeight / 2f);
				return;
				case 8:
				parameters.Enqueue(mousePackedDouble / 16);
				return;
			}
			Apply(parameters);
		}
		public override void Apply(LinkedQueue<object> parameters) {
			GenRunners.SmoothSpikeRunner((int)parameters.Dequeue(), (int)parameters.Dequeue(),
				parameters.DequeueAsOrDefaultTo(1.0),
				TileID.AccentSlab,
				parameters.DequeueAsOrDefaultTo(-Vector2.UnitY),
				parameters.DequeueAsOrDefaultTo(0.5),
				parameters.DequeueAsOrDefaultTo(0f),
				parameters.DequeueAsOrDefaultTo(false),
				parameters.DequeueAsOrDefaultTo(true),
				parameters.DequeueAsOrDefaultTo(0.0)
			);
		}
	}
	public class Remove_Tree_Testing_Mode : WorldgenTestingMode {
		public override SortOrder SortPosition => new();
		public override string GetMouseText(int parameterCount, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) => "Remove Tree";
		public override void SetParameter(LinkedQueue<object> parameters, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {
			parameters.Enqueue(Main.MouseWorld.ToTileCoordinates());
			Apply(parameters);
		}
		public override void Apply(LinkedQueue<object> parameters) {
			Point treeLoc = (Point)parameters.Dequeue();
			//Main.NewText(treeLoc);
			OriginSystem.RemoveTree(treeLoc.X, treeLoc.Y);
		}
	}
	public class Continue_Worldgen_Testing_Mode : WorldgenTestingMode {
		public override SortOrder SortPosition => new(ModContent.GetInstance<Remove_Tree_Testing_Mode>());
		public override string GetMouseText(int parameterCount, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) => "Continue";
		public override void SetParameter(LinkedQueue<object> parameters, Point mousePos, int mousePacked, double mousePackedDouble, Tile mouseTile, Vector2 diffFromPlayer) {}
		public override void Apply(LinkedQueue<object> parameters) {
			Func<bool> function = (Func<bool>)parameters.Dequeue();
			if (function()) {
				parameters.Enqueue(function);
			} else {
				mode = 0;
			}
		}
	}
}
