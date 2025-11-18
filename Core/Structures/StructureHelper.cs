using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Origins.Items.Other.Testing;
using Origins.UI;
using PegasusLib;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using static Origins.Core.Structures.DeserializedStructure;

namespace Origins.Core.Structures {
	public class StructureHelperUI : UIState {
		public Structure structure;
		public IRoom room;
		public (string name, Color color, bool[,] map, SerializableTileDescriptor source)[] layers;
		public string selectedLayer = null;
		float scroll = 0;
		bool reset = false;
		public void SetRoom(IRoom room) {
			this.room = room;
			Dictionary<string, (HashSet<char> set, Color color, SerializableTileDescriptor source)> layers = [];
			foreach ((char key, TileDescriptor value) in room.Key) {
				if (value.Parts is null) continue;
				for (int i = 0; i < value.Parts.Length; i++) {
					if (!SerializableTileDescriptor.TryGet(structure.Mod, value.Parts[i], out SerializableTileDescriptor source, out string[] parameters)) continue;
					foreach ((string _layer, Color color) in source.GetDisplayLayers(parameters)) {
						string layer = $"{source.Name}_{_layer}";
						if (!layers.TryGetValue(layer, out (HashSet<char> set, Color color, SerializableTileDescriptor source) data)) layers[layer] = data = ([], color, source);
						data.set.Add(key);
					}
				}
			}
			{
				(string name, Color color, bool[,] map, SerializableTileDescriptor source)[] _layers = new (string name, Color color, bool[,] map, SerializableTileDescriptor source)[layers.Count];
				int index = 0;
				string[] map = room.Map.Split('\n', StringSplitOptions.RemoveEmptyEntries);
				foreach ((string name, (HashSet<char> set, Color color, SerializableTileDescriptor source) data) in layers.OrderBy(k => k.Key)) {
					bool[,] layer = new bool[map[0].Length, map.Length];
					for (int y = 0; y < map.Length; y++) {
						for (int x = 0; x < map[y].Length; x++) {
							layer[x, y] = data.set.Contains(map[y][x]);
						}
					}
					_layers[index++] = (name, data.color, layer, data.source);
				}
				this.layers = _layers;
			}
		}
		public override bool ContainsPoint(Vector2 point) {
			return room is null && base.ContainsPoint(point);
		}
		public override void Draw(SpriteBatch spriteBatch) {
			if (Main.LocalPlayer.controlDown && Main.LocalPlayer.releaseDown) structure = null;
			structure ??= DeserializedStructure.Load("Origins/World/Structures/TestStructure");
			if (reset) scroll = 0;
			if (room is null) {
				if (Elements.Count <= 0) {
					RemoveAllChildren();
					UIList list = [];
					float height = 0;
					for (int i = 0; i < structure.Rooms.Count; i++) {
						IRoom room = structure.Rooms[i];
						UITextPanel<string> element = new(room.Identifier);
						element.OnLeftClick += (_, _) => {
							SetRoom(room);
							reset = true;
						};
						element.Width.Set(0, 1);
						list.Add(element);
						height += element.Height.Pixels + 4;
					}
					list.MinHeight.Set(0, 1);
					list.Height.Set(height, 0);
					list.Width.Set(0, 0.5f);
					list.VAlign = 0.5f;
					Append(list);
				}
				if (Elements[0].Top.Pixels.TrySet(-scroll)) {
					Elements[0].Recalculate();
				}
				base.Draw(spriteBatch);
			} else {
				Point basePos = Main.ScreenSize;
				basePos.X /= 2;
				basePos.Y /= 2;
				Rectangle dest = new(0, 0, 16, 16);
				for (int index = 0; index < layers.Length; index++) {
					if (selectedLayer != null && selectedLayer != layers[index].name) continue;
					bool[,] map = layers[index].map;
					Color color = layers[index].color;
					for (int y = 0; y < map.GetLength(1); y++) {
						for (int x = 0; x < map.GetLength(0); x++) {
							if (!map[x, y]) continue;
							dest.X = (int)(basePos.X + (x - map.GetLength(0) * 0.5f) * 16);
							dest.Y = (int)(basePos.Y + (y - map.GetLength(1) * 0.5f) * 16);
							layers[index].source.Draw(spriteBatch, dest, color, map, x, y, layers[index].name);
						}
					}
				}
			}
			reset = false;
		}
	}
	public class Structure_Helper_Item : TestingItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.WireKite;
#if !DEBUG
		public override bool IsLoadingEnabled(Mod mod) => DebugConfig.Instance.ForceEnableDebugItems;
#endif
		internal static Point? leftClick;
		internal static Point? rightClick;
		static bool shouldCancel = false;
		internal static bool ShouldCancel {
			get => shouldCancel || Main.gameMenu;
			set => shouldCancel |= value;
		}
		public static Dictionary<string, char> Associations { get; private set; } = [];
		public static string CurrentTileData { get; private set; }
		public override void SetDefaults() {
			Item.width = 16;
			Item.height = 26;
			Item.value = 25000;
			Item.color = new(255, 255, 180);
			Item.rare = ItemRarityID.Green;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useAnimation = 10;
			Item.useTime = 10;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool? UseItem(Player player) {
			if (Main.myPlayer == player.whoAmI) {
				if (player.altFunctionUse == 2) {
					rightClick = new(Player.tileTargetX, Player.tileTargetY);
				} else {
					leftClick = new(Player.tileTargetX, Player.tileTargetY);
				}
				return true;
			}
			return false;
		}
		public override void RightClick(Player player) {
			IngameFancyUI.OpenUIState(new StructureHelperUI());
		}
		public static void CopyStructure() {
			shouldCancel = false;
			Task.Run(() => {
				if (Structure_Helper_Item.leftClick is not Point leftClick || Structure_Helper_Item.rightClick is not Point rightClick) return;
				Associations.Clear();
				Associations.Add("Empty", ' ');
				Associations.Add("Void", '.');
				Point min = leftClick;
				Point max = rightClick;
				Min(ref min.X, rightClick.X);
				Min(ref min.Y, rightClick.Y);
				Max(ref max.X, leftClick.X);
				Max(ref max.Y, leftClick.Y);
				string[] lines = new string[max.Y + 1 - min.Y];
				for (int j = min.Y; j <= max.Y; j++) {
					StringBuilder builder = new();
					for (int i = min.X; i <= max.X; i++) {
						CurrentTileData = SerializableTileDescriptor.Serialize(Main.tile[i, j]);
						if (!Associations.ContainsKey(CurrentTileData)) {
							Main.QueueMainThreadAction(() => Main.NewText($"New tile pattern {CurrentTileData}, use /ass to set its key"));
							SpinWait.SpinUntil(() => ShouldCancel || Associations.ContainsKey(CurrentTileData));
						}
						if (ShouldCancel) {
							Associations.Clear();
							return;
						}
						builder.Append(Associations[CurrentTileData]);
					}
					lines[j - min.Y] = builder.ToString();
				}
				RoomDescriptor descriptor = new() {
					Map = lines,
					Key = Associations.Select(x => new KeyValuePair<char, string>(x.Value, x.Key)).ToDictionary(),
				};
				Associations.Clear();

				Platform.Get<IClipboard>().Value = JsonConvert.SerializeObject(descriptor, new JsonSerializerSettings {
					Formatting = Formatting.Indented,
					DefaultValueHandling = DefaultValueHandling.Ignore,
					ObjectCreationHandling = ObjectCreationHandling.Replace,
					NullValueHandling = NullValueHandling.Ignore
				});
				Main.NewText("Copied room to clipboard");
				Structure_Helper_Item.leftClick = null;
				Structure_Helper_Item.rightClick = null;
			});
		}
		public override bool CanRightClick() => true;
		public override bool ConsumeItem(Player player) => false;
	}
	public class Structure_Helper_HUD : SwitchableUIState {
		public override void AddToList() => OriginSystem.Instance.ItemUseHUD.AddState(this);
		public override bool IsActive() => Main.LocalPlayer.HeldItem.ModItem is Structure_Helper_Item;
		public override InterfaceScaleType ScaleType => InterfaceScaleType.Game;
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			DrawRegion(spriteBatch);
			Vector2 vector = Main.screenPosition + new Vector2(30f);
			Vector2 max = vector + new Vector2(Main.screenWidth, Main.screenHeight) - new Vector2(60f);
			if (Main.mapFullscreen) {
				vector -= Main.screenPosition;
				max -= Main.screenPosition;
			}
			bool hasLeft = Structure_Helper_Item.leftClick.HasValue;
			bool hasRight = Structure_Helper_Item.rightClick.HasValue;
			Point leftClick = Structure_Helper_Item.leftClick.GetValueOrDefault();
			Point rightClick = Structure_Helper_Item.rightClick.GetValueOrDefault();
			int flip = Math.Sign(Main.LocalPlayer.gravDir);
			if (hasLeft) {
				int specialMode = 0;
				float rotation;
				Vector2 drawPosition;
				Vector2 leftPos = leftClick.ToVector2() * 16f;
				if (!hasRight) {
					specialMode = 1;
					leftPos += Vector2.One * 8f;
					rotation = (-leftPos + Main.ReverseGravitySupport(new Vector2(Main.mouseX, Main.mouseY)) + Main.screenPosition).ToRotation() * flip;

					drawPosition = Vector2.Clamp(leftPos, vector, max);
					if (drawPosition != leftPos)
						rotation = (leftPos - drawPosition).ToRotation();
				} else {
					Vector2 vector4 = new((leftClick.X > rightClick.X).ToInt() * 16, (leftClick.Y > rightClick.Y).ToInt() * 16);
					leftPos += vector4;
					drawPosition = Vector2.Clamp(leftPos, vector, max);
					rotation = (rightClick.ToVector2() * 16f + new Vector2(16f) - vector4 - drawPosition).ToRotation();
					if (drawPosition != leftPos) {
						rotation = (leftPos - drawPosition).ToRotation();
						specialMode = 1;
					}
					rotation *= flip;
				}
				Utils.DrawCursorSingle(spriteBatch, Color.White, rotation - (float)Math.PI / 2f, 1f, Main.ReverseGravitySupport(drawPosition - Main.screenPosition), 4, specialMode);
			}
			if (hasRight) {
				int specialMode = 0;
				float rotation;
				Vector2 drawPosition;
				Vector2 rightPos = rightClick.ToVector2() * 16f;
				if (!hasLeft) {
					specialMode = 1;
					rightPos += Vector2.One * 8f;
					rotation = (-rightPos + Main.ReverseGravitySupport(new Vector2(Main.mouseX, Main.mouseY)) + Main.screenPosition).ToRotation() * flip;

					drawPosition = Vector2.Clamp(rightPos, vector, max);
					if (drawPosition != rightPos)
						rotation = (rightPos - drawPosition).ToRotation();
				} else {
					Vector2 vector4 = new((leftClick.X <= rightClick.X).ToInt() * 16, (leftClick.Y <= rightClick.Y).ToInt() * 16);
					rightPos += vector4;
					drawPosition = Vector2.Clamp(rightPos, vector, max);
					rotation = (rightClick.ToVector2() * 16f + new Vector2(16f) - vector4 - drawPosition).ToRotation();
					if (drawPosition != rightPos) {
						rotation = (rightPos - drawPosition).ToRotation();
						specialMode = 1;
					}
					rotation *= flip;
				}
				Utils.DrawCursorSingle(spriteBatch, Color.White, rotation - (float)Math.PI / 2f, 1f, Main.ReverseGravitySupport(drawPosition - Main.screenPosition), 5, specialMode);
			}
		}
		static void DrawRegion(SpriteBatch spriteBatch) {
			if (Structure_Helper_Item.leftClick is not Point leftClick || Structure_Helper_Item.rightClick is not Point rightClick) return;
			Point min = leftClick;
			Point max = rightClick;
			Min(ref min.X, rightClick.X);
			Min(ref min.Y, rightClick.Y);
			Max(ref max.X, leftClick.X);
			Max(ref max.Y, leftClick.Y);
			if (!Main.mapFullscreen) {
				Rectangle value = Main.ReverseGravitySupport(new Rectangle(min.X * 16, min.Y * 16, (max.X + 1 - min.X) * 16, (max.Y + 1 - min.Y) * 16));
				Rectangle value2 = Main.ReverseGravitySupport(new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth + 1, Main.screenHeight + 1));
				Rectangle.Intersect(ref value2, ref value, out Rectangle result);
				if (result.Width != 0 && result.Height != 0) {
					result.Offset(-value2.X, -value2.Y);
					spriteBatch.Draw(TextureAssets.MagicPixel.Value, result, new Color(0.8f, 0.8f, 0.8f, 0f) * 0.3f);
					for (int i = 0; i < 2; i++) {
						spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(result.X, result.Y + ((i == 1) ? result.Height : -2), result.Width, 2), Color.White);
						spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(result.X + ((i == 1) ? result.Width : -2), result.Y, 2, result.Height), Color.White);
					}
				}
				if (Main.MouseWorld.IsWithinRectangular(min.ToVector2() * 16, ((max - min).ToVector2() + Vector2.One) * 16)) {
					Main.LocalPlayer.mouseInterface = true;
					UICommon.TooltipMouseText("Left click to copy");
					if (Main.mouseLeft && Main.mouseLeftRelease) Structure_Helper_Item.CopyStructure();
				}
				return;
			}
		}
	}
	public class StructureCommand : ModCommand {
		public override CommandType Type => CommandType.Chat;
		public override string Command => "ass";
		public override string Usage => "/ass <char> or /ass cancel";
		public override string Description => "";
		public override void Action(CommandCaller caller, string input, string[] args) {
			if (args[0] == "cancel") {
				Structure_Helper_Item.ShouldCancel = true;
				return;
			}
			if (args[0].Length != 1) {
				caller.Reply(args[0] + "is not a single character in UTF-16");
				return;
			}
			if (!Structure_Helper_Item.Associations.ContainsValue(args[0][0]) && Structure_Helper_Item.Associations.TryAdd(Structure_Helper_Item.CurrentTileData, args[0][0])) {
				caller.Reply($"Successfully added {args[0][0]}<--->{Structure_Helper_Item.CurrentTileData}");
			} else {
				caller.Reply($"Could not add {args[0][0]}<--->{Structure_Helper_Item.CurrentTileData}");
			}
		}
	}
}
