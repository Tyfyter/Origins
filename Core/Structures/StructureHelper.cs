using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Origins.Items.Other.Testing;
using Origins.Tiles.Dev;
using Origins.UI;
using ReLogic.Content;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.Utilities.FileBrowser;
using static Origins.Core.Structures.DeserializedStructure;

namespace Origins.Core.Structures {
	public class StructureHelperUI : UIState {
		string structurePath;
		public Structure structure;
		Stack<ViewState> viewStack = new();
		ViewState CurrentView => viewStack.TryPeek(out ViewState result) ? result : null;

		Dictionary<string, List<(IRoom room, char entrance)>[]> connectionLookup;
		public override bool ContainsPoint(Vector2 point) {
			return CurrentView?.room is null && base.ContainsPoint(point);
		}
		static StructureHelperUI instance;
		public override void OnInitialize() {
			instance = this;
			refreshButton = new(TextureAssets.Flame) {
				Left = new(-16, 0),
				Top = new(8, 0.1f),
				HAlign = 0.75f + 0.25f * 0.5f
			};
			refreshButton.OnLeftClick += (_, _) => {
				while (viewStack.Count > 0) viewStack.Pop();
				RefreshStructure();
			};
			refreshButton.OnUpdate += _ => {
				if (refreshButton.IsMouseHovering) Main.instance.MouseText("Reload Structure");
			};
			Append(refreshButton);

			selectFileButton = new(TextureAssets.Camera[6]) {
				Left = new(16, 0),
				Top = new(8, 0.1f),
				HAlign = 0.25f * 0.5f
			};
			selectFileButton.OnLeftClick += (_, _) => {
				if (FileBrowser.OpenFilePanel("Select Structure", "json") is not string select) return;
				string sourcePath = Path.Combine(Program.SavePathShared, "ModSources");
				string relativePath = Path.GetRelativePath(sourcePath, select);
				if (relativePath.StartsWith('.')) return;
				while (viewStack.Count > 0) viewStack.Pop();
				structurePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
				RefreshStructure();
			};
			selectFileButton.OnUpdate += _ => {
				if (selectFileButton.IsMouseHovering) Main.instance.MouseText("Select Structure");
			};
			Append(selectFileButton);

			deselectRoomButton = new(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Button_Back")) {
				Left = new(48, 0),
				Top = new(8, 0.1f),
				HAlign = 0.25f * 0.5f
			};
			deselectRoomButton.OnLeftClick += (_, _) => {
				viewStack.Pop();
			};
			deselectRoomButton.OnUpdate += _ => {
				if (deselectRoomButton.IsMouseHovering) Main.instance.MouseText(Lang.menu[5].Value);
			};
			Append(deselectRoomButton);

			text = new("") {
				Left = new(0, 0),
				Top = new(8, 0.1f),
				HAlign = 0.5f
			};
			Append(text);
		}
		void RefreshStructure() {
			structure = Load(structurePath);
			RefreshRoomList();
			connectionLookup = structure.Rooms.GenerateConnectionLookup();
		}
		public override void Update(GameTime gameTime) {
			for (int i = 0; i < Elements.Count; i++) Elements[i].IgnoresMouseInteraction = true;
			foreach (UIElement element in ActiveElements()) {
				element.IgnoresMouseInteraction = false;
				element.Update(gameTime);
			}
		}
		IEnumerable<UIElement> ActiveElements() {
			yield return selectFileButton;
			if (viewStack.Count > 0) {
				if (CurrentView.room is null) {
					yield return CurrentView.roomList;
				} else {
					text.SetText(CurrentView.room.Identifier);
					yield return text;
				}
				yield return deselectRoomButton;
			} else {
				if (structure is not null) {
					text.SetText(structurePath);
					yield return text;
					yield return refreshButton;
					yield return roomList;
				}
			}
		}
		UIList roomList;
		UIText text;
		UIImageButton refreshButton;
		UIImageButton selectFileButton;
		UIImageButton deselectRoomButton;
		public override void Draw(SpriteBatch spriteBatch) {
			Vector2 bgStart = Main.ScreenSize.ToVector2() * new Vector2(0.25f * 0.5f, 0.1f);
			Vector2 bgSize = new(Main.screenWidth * 0.75f, Main.screenHeight * 0.8f);
			ConfigElement.DrawPanel2(
				spriteBatch,
				bgStart,
				TextureAssets.SettingsPanel.Value,
				bgSize.X,
				bgSize.Y,
				Color.DeepSkyBlue * 0.8f
			);
			if (Main.MouseScreen.IsWithinRectangular(bgStart + bgSize * 0.5f, bgSize * 0.5f)) Main.LocalPlayer.mouseInterface = true;
			foreach (UIElement element in ActiveElements()) element.Draw(spriteBatch);
			ViewState currentView = CurrentView;
			if (currentView?.room is not null) {
				Point basePos = Main.ScreenSize;
				basePos.X /= 2;
				basePos.Y /= 2;
				Rectangle dest = new(0, 0, 16, 16);
				for (int index = 0; index < currentView.layers.Length; index++) {
					if (currentView.selectedLayer != null && currentView.selectedLayer != currentView.layers[index].name) continue;
					bool[,] map = currentView.layers[index].map;
					Color color = currentView.layers[index].color;
					for (int y = 0; y < map.GetLength(1); y++) {
						for (int x = 0; x < map.GetLength(0); x++) {
							if (!map[x, y]) continue;
							dest.X = (int)(basePos.X + (x - map.GetLength(0) * 0.5f) * 16);
							dest.Y = (int)(basePos.Y + (y - map.GetLength(1) * 0.5f) * 16);
							currentView.layers[index].source.Draw(spriteBatch, dest, color, map, x, y, currentView.layers[index].name);
						}
					}
				}
				for (int i = 0; i < currentView.overlays.Count; i++) currentView.overlays[i].Draw(spriteBatch, basePos);
			}
		}

		public void RefreshRoomList() {
			if (roomList is not null) RemoveChild(roomList);
			roomList = [];
			FillRoomList(roomList);
			Append(roomList);
		}
		public void FillRoomList(UIList roomList, IEnumerable<IRoom> rooms = null) {
			rooms ??= structure.Rooms;
			float height = 0;
			foreach (IRoom room in rooms) {
				UITextPanel<string> element = new(room.Identifier);
				element.OnLeftClick += (_, _) => {
					viewStack.Push(new(room));
				};
				element.Width.Set(0, 1);
				roomList.Add(element);
				height += element.Height.Pixels + 4;
			}
			roomList.MinHeight.Set(0, 1);
			roomList.Height.Set(height, 0);
			roomList.Width.Set(0, 0.4f);
			roomList.HAlign = 0.5f;
			roomList.Top.Set(32, 0.1f);
		}
		public class ViewState {
			public IRoom room;
			public (string name, Color color, bool[,] map, SerializableTileDescriptor source)[] layers;
			public List<StructureOverlay> overlays;
			public UIList roomList;
			public string selectedLayer;
			public float scroll;
			public ViewState(IRoom room) {
				this.room = room;
				Dictionary<string, (HashSet<char> set, Color color, SerializableTileDescriptor source)> layers = [];
				foreach ((char key, TileDescriptor value) in room.Key) {
					if (value.Parts is null) continue;
					for (int i = 0; i < value.Parts.Length; i++) {
						if (!SerializableTileDescriptor.TryGet(instance.structure.Mod, value.Parts[i], out SerializableTileDescriptor source, out string[] parameters)) continue;
						foreach ((string _layer, Color color) in source.GetDisplayLayers(parameters)) {
							string layer = $"{source.Name}_{_layer}";
							if (!layers.TryGetValue(layer, out (HashSet<char> set, Color color, SerializableTileDescriptor source) data)) layers[layer] = data = ([], color, source);
							data.set.Add(key);
						}
					}
				}
				string[] map = room.Map.Split('\n', StringSplitOptions.RemoveEmptyEntries);
				{
					(string name, Color color, bool[,] map, SerializableTileDescriptor source)[] _layers = new (string name, Color color, bool[,] map, SerializableTileDescriptor source)[layers.Count];
					int index = 0;
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
				{
					overlays = [];
					for (int y = 0; y < map.Length; y++) {
						for (int x = 0; x < map[y].Length; x++) {
							if (room.SocketKey.TryGetValue(map[y][x], out RoomSocket socket) && socket.Direction != 0) {
								overlays.Add(new SocketOverlay(
									new(
										(x - map[y].Length * 0.5f) * 16,
										(y - map.Length * 0.5f) * 16
									),
									socket
								));
							}
						}
					}
				}
			}
			public ViewState(IEnumerable<IRoom> roomList) {
				this.roomList = [];
				instance.FillRoomList(this.roomList, roomList);
				instance.Append(this.roomList);
			}
			~ViewState() {
				roomList?.Clear();
				roomList?.Remove();
			}
		}
		public abstract class StructureOverlay {
			public abstract void Draw(SpriteBatch spriteBatch, Point basePos);
		}
		public class SocketOverlay(Vector2 pos, RoomSocket socket) : StructureOverlay {
			readonly Vector2 pos = pos;
			readonly RoomSocket socket = socket;
			static readonly Asset<Texture2D>[] arrows = [
				TextureAssets.ScrollRightButton,
				TextureAssets.ScrollLeftButton,
				TextureAssets.CraftUpButton,
				TextureAssets.CraftDownButton
			];
			public override void Draw(SpriteBatch spriteBatch, Point basePos) {
				if (socket.Direction == 0) return;
				Texture2D texture = arrows[socket.Direction.Index()].Value;
				Vector2 pos = this.pos + basePos.ToVector2();
				bool hovered = Main.MouseScreen.IsWithinRectangular(pos + texture.Size() * 0.5f, texture.Size() * 0.5f);
				if (hovered) {
					UICommon.TooltipMouseText(socket.Key);
					if (Main.mouseLeft && Main.mouseLeftRelease && instance.connectionLookup.TryGetValue(socket.Key, out List<(IRoom room, char entrance)>[] _connections)) {
						instance.viewStack.Push(new(_connections[socket.Direction.Index()].Select(v => v.room)));
					}
				}
				spriteBatch.Draw(
					texture,
					pos,
					Color.White * (hovered ? 1 : 0.5f)
				);
			}
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
		static Task task;
		public static void CopyStructure() {
			shouldCancel = false;
			if (task is not null && !task.IsCompleted) return;
			task =
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
				Dictionary<char, string> key = [];
				Dictionary<char, RoomSocket> socketKey = [];
				for (int j = min.Y; j <= max.Y; j++) {
					StringBuilder builder = new();
					Structure_Helper_HUD.HighlightTile.Y = j;
					for (int i = min.X; i <= max.X; i++) {
						Structure_Helper_HUD.HighlightTile.X = i;
						Tile tile = Main.tile[i, j];
						CurrentTileData = SerializableTileDescriptor.Serialize(tile);
						SpinWait.SpinUntil(() => StructureCommand.ReadyForNewAss);
						if (tile.TileType == ModContent.TileType<Room_Socket_Marker>()) {
							string pattern = CurrentTileData;
							char? ch = null;
							RoomSocket socket = new(null, default);
							switch (tile.TileFrameX / 18) {
								case 1:
								CurrentTileData = "Right";
								socket = socket with { Direction = Direction.Right };
								break;
								case 2:
								CurrentTileData = "Left";
								socket = socket with { Direction = Direction.Left };
								break;
								case 3:
								CurrentTileData = "Down";
								socket = socket with { Direction = Direction.Down };
								break;
								case 4:
								CurrentTileData = "Up";
								socket = socket with { Direction = Direction.Up };
								break;
								default:
								CurrentTileData = "None";
								socket = null;
								break;
							}
							StructureCommand.OnAss += (c, socketKey) => {
								if (Associations.ContainsValue(c)) return false;
								if (string.IsNullOrWhiteSpace(socketKey)) return false;
								socket = socket with { Key = socketKey };
								ch = c;
								return true;
							};
							Main.QueueMainThreadAction(() => {
								Main.NewText($"New socket, use /ass to set its map and socket keys");
								Main.OpenPlayerChat();
								Main.chatText = "/ass ";
							});
							SpinWait.SpinUntil(() => ShouldCancel || ch.HasValue);
							if (ShouldCancel) {
								Associations.Clear();
								Structure_Helper_HUD.HighlightTile = default;
								return;
							}
							builder.Append(ch.Value);
							key.Add(ch.Value, pattern);
							if (socket is not null) socketKey.Add(ch.Value, socket);
						} else {
							if (!Associations.ContainsKey(CurrentTileData)) {
								StructureCommand.OnAss += (c, _) => !Associations.ContainsValue(c) && Associations.TryAdd(CurrentTileData, c);
								Main.QueueMainThreadAction(() => {
									Main.NewText($"New tile pattern {CurrentTileData}, use /ass to set its key");
									Main.OpenPlayerChat();
									Main.chatText = "/ass ";
								});
								SpinWait.SpinUntil(() => ShouldCancel || Associations.ContainsKey(CurrentTileData));
							}
							if (ShouldCancel) {
								Associations.Clear();
								Structure_Helper_HUD.HighlightTile = default;
								return;
							}
							builder.Append(Associations[CurrentTileData]);
						}
					}
					lines[j - min.Y] = builder.ToString();
				}
				Structure_Helper_HUD.HighlightTile = default;
				foreach ((string pattern, char c) in Associations) {
					key.Add(c, pattern);
				}
				RoomDescriptor descriptor = new() {
					Map = lines,
					Key = key,
					SocketKey = socketKey
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
		public static Point HighlightTile;
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
			if (HighlightTile != default) {
				spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					new Rectangle(HighlightTile.X * 16 - (int)Main.screenPosition.X, HighlightTile.Y * 16 - (int)Main.screenPosition.Y, 16, 16),
					Main.DiscoColor.MultiplyRGBA(new Color(0.5f, 0.5f, 0.5f, 0.4f))
				);
			}
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
					spriteBatch.Draw(TextureAssets.MagicPixel.Value, result, new Color(0.8f, 0.8f, 0.8f, 0f) * 0.15f);
					for (int i = 0; i < 2; i++) {
						spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(result.X, result.Y + ((i == 1) ? result.Height : -2), result.Width, 2), Color.White);
						spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(result.X + ((i == 1) ? result.Width : -2), result.Y, 2, result.Height), Color.White);
					}
				}
				if (Main.MouseWorld.X >= min.X * 16 && Main.MouseWorld.Y >= min.Y * 16 && Main.MouseWorld.X <= (max.X + 1) * 16 && Main.MouseWorld.Y <= (max.Y + 1) * 16) {
					Main.LocalPlayer.mouseInterface = true;
					UICommon.TooltipMouseText("Left click to copy");
					if (!Main.LocalPlayer.ItemAnimationJustStarted && Main.mouseLeft && Main.mouseLeftRelease) Structure_Helper_Item.CopyStructure();
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
		public static event Func<char, string, bool> OnAss = null;
		public static bool ReadyForNewAss => OnAss is null;
		public override void Action(CommandCaller caller, string input, string[] args) {
			if (args[0] == "cancel") {
				Structure_Helper_Item.ShouldCancel = true;
				return;
			}
			if (args[0].Length != 1) {
				caller.Reply(args[0] + "is not a single character in UTF-16");
				return;
			}
			if (OnAss?.Invoke(args[0][0], args.GetIfInRange(1)) ?? false) {
				caller.Reply($"Successfully added {args[0][0]}<--->{Structure_Helper_Item.CurrentTileData}");
				OnAss = null;
			} else {
				caller.Reply($"Could not add {args[0][0]}<--->{Structure_Helper_Item.CurrentTileData}");
			}
		}
	}
}
