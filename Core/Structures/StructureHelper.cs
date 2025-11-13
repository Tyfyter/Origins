using CalamityMod.Items.SummonItems;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Origins.Items.Other.Testing;
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
using Terraria.GameContent.UI.Chat;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using static Origins.Core.Structures.DeserializedStructure;

namespace Origins.Core.Structures {
	public class StructureHelperUI : UIState {
		public Structure structure;
		public IRoom room;
		public (string name, Color color, bool[,] map)[] layers;
		public string selectedLayer = null;
		float scroll = 0;
		bool reset = false;
		public void SetRoom(IRoom room) {
			this.room = room;
			Dictionary<string, (HashSet<char> set, Color color)> layers = [];
			foreach ((char key, TileDescriptor value) in room.Key) {
				if (value.Parts is null) continue;
				for (int i = 0; i < value.Parts.Length; i++) {
					if (!SerializableTileDescriptor.TryGet(structure.Mod, value.Parts[i], out SerializableTileDescriptor source, out string[] parameters)) continue;
					foreach ((string _layer, Color color) in source.GetDisplayLayers(parameters)) {
						string layer = $"{source.Name}_{_layer}";
						if (!layers.TryGetValue(layer, out (HashSet<char> set, Color color) data)) layers[layer] = data = ([], color);
						data.set.Add(key);
					}
				}
			}
			{
				(string name, Color color, bool[,] map)[] _layers = new (string name, Color color, bool[,] map)[layers.Count];
				int index = 0;
				string[] map = room.Map.Split('\n', StringSplitOptions.RemoveEmptyEntries);
				foreach ((string name, (HashSet<char> set, Color color) data) in layers.OrderBy(k => k.Key)) {
					bool[,] layer = new bool[map[0].Length, map.Length];
					for (int y = 0; y < map.Length; y++) {
						for (int x = 0; x < map[y].Length; x++) {
							layer[x, y] = data.set.Contains(map[y][x]);
						}
					}
					_layers[index++] = (name, data.color, layer);
				}
				this.layers = _layers;
			}
		}
		public override bool ContainsPoint(Vector2 point) {
			return room is null && base.ContainsPoint(point);
		}
		public override void Draw(SpriteBatch spriteBatch) {
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
							spriteBatch.Draw(
								TextureAssets.MagicPixel.Value,
								dest,
								color
							);
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
		static Point leftClick;
		static Point rightClick;
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
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool? UseItem(Player player) {
			if (Main.myPlayer == player.whoAmI) {
				if (player.altFunctionUse == 2) {
					rightClick.X = Player.tileTargetX;
					rightClick.Y = Player.tileTargetY;
				} else {
					leftClick.X = Player.tileTargetX;
					leftClick.Y = Player.tileTargetY;
				}
				return true;
			}
			return false;
		}
		public override void RightClick(Player player) {
			Task.Run(() => {
				Associations.Clear();
				Associations.Add("Empty", ' ');
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
							SpinWait.SpinUntil(() => Associations.ContainsKey(CurrentTileData));
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
			});
		}
		public override bool CanRightClick() => true;
		public override bool ConsumeItem(Player player) => false;
	}
	public class StructureCommand : ModCommand {
		public override CommandType Type => CommandType.Chat;
		public override string Command => "ass";
		public override string Usage => "/ass <char>";
		public override string Description => "";
		public override void Action(CommandCaller caller, string input, string[] args) {
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
