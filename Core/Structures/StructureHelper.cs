using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Primitives;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils;
using Newtonsoft.Json;
using Origins.Graphics;
using Origins.Items.Other.Testing;
using Origins.Tiles.Dev;
using Origins.UI;
using PegasusLib;
using PegasusLib.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.Utilities.FileBrowser;
using static Origins.Core.Structures.DescriptorParameter;
using static Origins.Core.Structures.DeserializedStructure;
using static Origins.Core.Structures.StructureCommand;
using static Origins.Core.Structures.StructureHelperUI;

namespace Origins.Core.Structures; 
public class StructureHelperUI : UIState {
	struct Move : IMoveToPegFlag;
	string structurePath;
	public Structure structure;
	readonly Stack<ViewState> viewStack = new();
	ViewState CurrentView => viewStack.TryPeek(out ViewState result) ? result : null;

	Dictionary<string, List<(ARoom room, char entrance)>[]> connectionLookup;
	public override bool ContainsPoint(Vector2 point) {
		return CurrentView?.room is null && base.ContainsPoint(point);
	}
	static StructureHelperUI instance;
	static AutoLoadingTexture saveIcon = "Origins/UI/Floppy_Disk";
	static string reopenPath;
	public override void OnInitialize() {
		instance = this;
		refreshButton = new(TextureAssets.Flame) {
			Left = new(-16, 0),
			Top = new(8, 0.1f),
			HAlign = 0.75f + 0.25f * 0.5f
		};
		refreshButton.OnLeftClick += (_, _) => {
			while (viewStack.Count > 0) viewStack.Pop();
			ReloadStructure();
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
		selectFileButton.OnLeftClick += OpenStructureSelect;
		selectFileButton.OnUpdate += _ => {
			if (selectFileButton.IsMouseHovering) Main.instance.MouseText("Select Structure");
		};
		Append(selectFileButton);

		saveIcon.Wait();
		saveFileButton = new(saveIcon) {
			Left = new(48, 0),
			Top = new(8, 0.1f),
			HAlign = 0.25f * 0.5f
		};
		saveFileButton.OnLeftClick += (_, _) => {
			if (structure is not DeserializedStructure @struct) return;
			string path = Path.Combine(Path.Combine(Program.SavePathShared, "ModSources"), structurePath.Replace('/', Path.DirectorySeparatorChar));
			if (!path.EndsWith(".json")) return;
			string sourcePath = Path.Combine(Program.SavePathShared, "ModSources");
			string relativePath = Path.GetRelativePath(sourcePath, path);
			if (relativePath.StartsWith('.')) return;
			string newData = @struct.Export();
			using (StreamWriter writer = File.CreateText(path)) {
				writer.Write(newData);
			}
			while (viewStack.Count > 0) viewStack.Pop();
			ReloadStructure();
		};
		saveFileButton.OnUpdate += _ => {
			if (saveFileButton.IsMouseHovering) Main.instance.MouseText(Lang.inter[47].Value);
		};
		Append(saveFileButton);

		Asset<Texture2D> favorited = Main.Assets.Request<Texture2D>("Images/UI/ButtonFavoriteActive");
		Asset<Texture2D> notFavorited = Main.Assets.Request<Texture2D>("Images/UI/ButtonFavoriteInactive");
		keepStructureButton = new(notFavorited) {
			Left = new(82, 0),
			Top = new(10, 0.1f),
			HAlign = 0.25f * 0.5f
		};
		keepStructureButton.OnLeftClick += (_, _) => {
			if (reopenPath == structurePath) reopenPath = null;
			else reopenPath = structurePath;
		};
		keepStructureButton.OnUpdate += _ => {
			if (structurePath == reopenPath) keepStructureButton.SetImage(favorited);
			else keepStructureButton.SetImage(notFavorited);
			if (keepStructureButton.IsMouseHovering) Main.instance.MouseText("Auto-reopen");
		};
		Append(keepStructureButton);

		deselectRoomButton = new(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Button_Back")) {
			Left = new(108, 0),
			Top = new(10, 0.1f),
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

		inputFeedback = new("", 1) {
			Left = new(0, 0),
			Top = new(-16, 0.1f),
			Width = new(0, 1),
			HAlign = 0.5f
		};
		Append(inputFeedback);

		textInput = new() {
			Left = new(0, 0),
			Top = new(8, 0.1f),
			Width = new(0, 1),
			HAlign = 0.5f
		};
		textInput.OnSubmit += inputValue => {
			if (serializationContext.state == SerializationContext.SerializationState.Socket) {
				textInput.IsWritingText = false;
				textInput2.IsWritingText = true;
				return;
			}
			if (Associate(new ToastCommandCaller(toastHandler), inputValue?.Split(' '))) textInput.Clear();
		};
		textInput.OnExit += IngameFancyUI.Close;
		Append(textInput);

		textInput2 = new() {
			Left = new(0, 0),
			Top = new(62, 0.1f),
			Width = new(0, 1),
			HAlign = 0.5f
		};
		textInput2.OnSubmit += inputValue => {
			string key = ((ITextInputContainer)textInput).TextDisplay;
			if (key?.Length != 1) {
				textInput.IsWritingText = true;
				textInput2.IsWritingText = false;
				return;
			}
			if (Associate(new ToastCommandCaller(toastHandler), [
				key,
				inputValue,
				""//$"optional,{RoomSocket.OffsetType.InverseOffset}"
			])) textInput2.Clear();
		};
		textInput2.OnExit += IngameFancyUI.Close;
		Append(textInput2);
		Append(socketSelector = new() {
			Left = new(4, 0),
			Width = new(0, 0.4f),
			Height = new(-204, 1),
			VAlign = 1
		});

		Append(globalKeyToggle = new() {
			Left = new(16, 0),
			Top = new(58, 0.1f),
			HAlign = 0f
		});

		UIElement region = new() {
			Left = new(0, 0.25f * 0.5f),
			Top = new(0, 0.1f),
			Width = new(0, 0.75f),
			Height = new(0, 0.8f),
			IgnoresMouseInteraction = true
		};
		Append(region);
		region.Append(toastHandler = new());
		Append(visibilityToggle = new() {
			Left = new(4, 0.25f * 0.5f),
			Top = new(-4, -0.1f),
			VAlign = 1
		});
		Append(roomListWrapper = new() {
			Top = new(32, 0.1f),
			Width = new(0, 0.4f),
			Height = new(-33, 0.8f),
			HAlign = 0.5f,
			OverflowHidden = true
		});
		if (!string.IsNullOrWhiteSpace(reopenPath)) SelectStructure(reopenPath);
	}
	private void OpenStructureSelect(UIMouseEvent _, UIElement __) {
		if (FileBrowser.OpenFilePanel("Select Structure", "json") is not string select) return;
		string sourcePath = Path.Combine(Program.SavePathShared, "ModSources");
		string relativePath = Path.GetRelativePath(sourcePath, select);
		if (relativePath.StartsWith('.')) return;
		SelectStructure(relativePath);
	}

	private void SelectStructure(string relativePath) {
		while (viewStack.Count > 0) viewStack.Pop();
		structurePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
		ReloadStructure();
	}

	void ReloadStructure() {
		structure = Load(structurePath);
		OnRoomsChanged();
		serializationContext ??= new();
		serializationContext.globalKeyToggle = globalKeyToggle;
		serializationContext.socketInput = textInput2;
		serializationContext.globalKey.Clear();
		serializationContext.globalSocketKey.Clear();
		if (structure is DeserializedStructure des/*Like, the roaring knight?*/) {
			serializationContext.globalKey.AddRange(des.globalTileDescriptors);
			foreach ((char key, RoomSocket socket) in des.Rooms.OfType<DeserializedRoom>().SelectMany(r => r.SocketKey)) {
				serializationContext.globalSocketKey[socket.Key] |= 1 << (int)socket.Direction;
			}
		}
	}
	void OnRoomsChanged() {
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
	public class UITextCommandCaller(UIText uiText) : CommandCaller {
		public CommandType CommandType => CommandType.Chat;
		public Player Player => Main.LocalPlayer;
		public void Reply(string text, Color color = default) {
			uiText.SetText(text);
			if (color == default)
				color = Color.White;
			uiText.TextColor = color;
		}
	}
	public class ToastCommandCaller(ToastHandler uiText) : CommandCaller {
		public CommandType CommandType => CommandType.Chat;
		public Player Player => Main.LocalPlayer;
		public void Reply(string text, Color color = default) => uiText.Push(text, color);
	}
	IEnumerable<UIElement> ActiveElements() {
		yield return visibilityToggle;
		if (!Visible) yield break;
		yield return toastHandler;
		if (!StructureCommand.ReadyForNewAss) {
			textInput.IsWritingText = true;
			yield return inputFeedback;
			yield return textInput;
			switch (serializationContext.state) {
				case SerializationContext.SerializationState.Tile:
				yield return globalKeyToggle;
				break;
				case SerializationContext.SerializationState.Socket:
				yield return textInput2;
				socketSelector.serializationContext = serializationContext;
				yield return socketSelector;
				break;
			}
			yield break;
		}
		textInput.IsWritingText = false;
		yield return selectFileButton;
		if (structurePath is not null && structure is DeserializedStructure) {
			yield return saveFileButton;
			yield return keepStructureButton;
		}
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
				yield return roomListWrapper;
			}
		}
	}
	UIElement roomListWrapper;
	UIList roomList;
	UIText text;
	UIText inputFeedback;
	UIInputPanel textInput;
	UIInputPanel textInput2;
	UIImageButton refreshButton;
	UIImageButton selectFileButton;
	UIImageButton saveFileButton;
	UIImageButton keepStructureButton;
	UIImageButton deselectRoomButton;
	public ToastHandler toastHandler;
	GlobalKeyToggle globalKeyToggle;
	SocketKeySelector socketSelector;
	UIImageButtonVisibility visibilityToggle;
	public bool Visible => visibilityToggle.Visible;
	public bool CanModifyBounds => !Visible && serializationContext.state == SerializationContext.SerializationState.Inactive;
	SerializationContext serializationContext = new();
	public override void Draw(SpriteBatch spriteBatch) {
		if (!Visible) {
			using (spriteBatch.OverrideState(transformMatrix: Main.GameViewMatrix.ZoomMatrix)) {
				ModContent.GetInstance<Structure_Helper_HUD>().Draw(spriteBatch);
			}
			visibilityToggle.Draw(spriteBatch);
			return;
		}
		if (!StructureCommand.ReadyForNewAss) {
			SpriteBatchState state = spriteBatch.GetState();
			spriteBatch.Restart(state, transformMatrix: Main.GameViewMatrix.ZoomMatrix);
			ModContent.GetInstance<Structure_Helper_HUD>().Draw(spriteBatch);
			spriteBatch.Restart(state);
			foreach (UIElement element in ActiveElements()) element.Draw(spriteBatch);
			return;
		}
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
		foreach (UIElement element in ActiveElements()) element.Draw(spriteBatch);
		ViewState currentView = CurrentView;
		if (currentView?.room is not null) {
			List<(string text, StructureLayer layer, StructureOverlay[] overlays)> parts = [];
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			for (int i = 0; i < currentView.layers.Length; i++) {
				foreach (bool item in currentView.layers[i].Map) if (item) goto any;
				continue;
				any:
				parts.Add((currentView.layers[i].Name, currentView.layers[i], null));
			}
			foreach (IGrouping<string, StructureOverlay> item in currentView.overlays.GroupBy(o => o.Name)) {
				parts.Add((item.Key, null, item.ToArray()));
			}
			Point basePos = Main.ScreenSize;
			basePos.X /= 2;
			basePos.Y /= 2;
			Rectangle dest = new(0, 0, 16, 16);
			for (int index = 0; index < currentView.layers.Length; index++) {
				if (currentView.selectedLayer != null && currentView.selectedLayer != currentView.layers[index].Name) continue;
				currentView.layers[index].Draw(spriteBatch, basePos);
			}
			for (int i = 0; i < currentView.overlays.Count; i++) currentView.overlays[i].Draw(spriteBatch, basePos);
			Vector2 layerListPos = new(bgStart.X + bgSize.X - 16, bgStart.Y + 48);
			bool hoverSelected = false;
			for (int i = 0; i < parts.Count; i++) {
				Vector2 size = font.MeasureString(parts[i].text);
				ChatManager.DrawColorCodedStringWithShadow(
					spriteBatch,
					font,
					parts[i].text,
					layerListPos - size * Vector2.UnitX,
					Color.White,
					0,
					default,
					Vector2.One
				);
				if (!hoverSelected && Main.MouseScreen.Between(layerListPos - size * Vector2.UnitX, layerListPos + size * Vector2.UnitY)) {
					hoverSelected = true;
					Vector2 layersSize = new(currentView.layers[0].Map.GetLength(0) * 16, currentView.layers[0].Map.GetLength(1) * 16);
					spriteBatch.Draw(
						TextureAssets.MagicPixel.Value,
						new Rectangle((int)(basePos.X - layersSize.X * 0.5f), (int)(basePos.Y - layersSize.Y * 0.5f), (int)layersSize.X, (int)layersSize.Y),
						Color.DeepSkyBlue * 0.5f
					);
					if (parts[i].layer is not null) {
						parts[i].layer.Draw(spriteBatch, basePos);
					} else {
						for (int j = 0; j < currentView.overlays.Count; j++) currentView.overlays[j].Draw(spriteBatch, basePos);
					}
				}
				layerListPos.Y += size.Y + 4;
			}
			Vector2 weightListPos = new(bgStart.X + 16, bgStart.Y + 48);
			for (int i = 0; i < currentView.weights.Count; i++) {
				string name = currentView.weights[i].ToString();
				Vector2 size = font.MeasureString(name);
				ChatManager.DrawColorCodedStringWithShadow(
					spriteBatch,
					font,
					name,
					weightListPos,
					Color.White,
					0,
					default,
					Vector2.One
				);
				if (Main.mouseLeft && Main.mouseLeftRelease && Main.MouseScreen.Between(weightListPos, weightListPos + size)) {

				}
				weightListPos.Y += size.Y + 4;
			}
		}
		if (Main.MouseScreen.IsWithinRectangular(bgStart + bgSize * 0.5f, bgSize * 0.5f)) Main.LocalPlayer.mouseInterface = true;
	}

	public void RefreshRoomList() {
		if (roomList is not null) roomListWrapper.RemoveChild(roomList);
		roomList = new() {
			Width = new(0, 1f),
			Height = new(0, 1f),
			MaxHeight = new(0, 1f),
			MinHeight = new(0, 1f),
			HAlign = 0.5f,
			OverflowHidden = true
		};
		roomList.SetScrollbar(new UIScrollbar());
		FillRoomList(roomList);
		roomListWrapper.Append(roomList);
	}
	public void FillRoomList(UIList roomList, IEnumerable<ARoom> rooms = null) {
		rooms ??= structure.Rooms;
		float height = 0;
		foreach (ARoom room in rooms) {
			UITextPanel<string> element = new(room.Identifier);
			element.OnLeftClick += (@event, _) => {
				if (@event.Target == element) viewStack.Push(new(structure, room));
			};
			element.Width.Set(0, 1);
			UIConditionalButton deleteButton = new(TextureAssets.MapDeath, () => ItemSlot.ShiftInUse ? null : "Must hold shift to delete rooms");
			deleteButton.OnLeftClick += (_, _) => {
				structure.RemoveRoom(room);
				OnRoomsChanged();
			};
			deleteButton.HAlign = 1f;
			deleteButton.VAlign = 0.5f;
			deleteButton.Top.Set(0, -0.25f);
			element.Append(deleteButton);
			roomList.Add(element);
			height += element.Height.Pixels + 4;
		}
		{
			UITextPanel<string> element = new("New room");
			element.OnLeftClick += (_, _) => {
				if (Structure_Helper_Item.leftClick is not Point leftClick || Structure_Helper_Item.rightClick is not Point rightClick) {
					toastHandler.Push($"Must specify bounds before adding room");
					return;
				}
				Task.Run(() => {
					Structure_Helper_Item.ShouldCancel = false;
					string roomName = null;
					StructureCommand.OnAss += (c, _, _) => {
						roomName = c;
						return true;
					};
					Main.QueueMainThreadAction(() => {
						inputFeedback.SetText($"Creating room, input a name");
						inputFeedback.TextColor = Color.White;
					});
					SpinWait.SpinUntil(() => {
						Structure_Helper_Item.leftClick = leftClick;
						Structure_Helper_Item.rightClick = rightClick;
						return Structure_Helper_Item.ShouldCancel || roomName != null;
					});
					if (Structure_Helper_Item.ShouldCancel) {
						Structure_Helper_Item.ShouldCancel = false;
						return;
					}
					Structure_Helper_Item.ExportRoom(new UITextCommandCaller(inputFeedback), serializationContext).ContinueWith(_descriptor => {
						RoomDescriptor descriptor = _descriptor.Result;
						if (descriptor is null) {
							toastHandler.Push("Canceled");
							return;
						}
						descriptor.Identifier = roomName;
						structure.AddOrReplaceRoom(new DeserializedRoom(structure.Mod, descriptor));
						OnRoomsChanged();
					});
				});
			};
			element.Width.Set(0, 1);
			roomList.Add(element);
			height += element.Height.Pixels + 4;
		}
	}
	public override void OnDeactivate() {
		StructureCommand.Cancel();
	}
	public record StructureLayer(string Name, Color Color, bool[,] Map, SerializableTileDescriptor Source) {
		public void Draw(SpriteBatch spriteBatch, Point basePos) {
			Rectangle dest = new(0, 0, 16, 16);
			for (int y = 0; y < Map.GetLength(1); y++) {
				for (int x = 0; x < Map.GetLength(0); x++) {
					if (!Map[x, y]) continue;
					dest.X = (int)(basePos.X + (x - Map.GetLength(0) * 0.5f) * 16);
					dest.Y = (int)(basePos.Y + (y - Map.GetLength(1) * 0.5f) * 16);
					Source.Draw(spriteBatch, dest, Color, Map, x, y, Name);
				}
			}
		}
	}
	public class ViewState {
		public readonly ARoom room;
		public StructureLayer[] layers;
		public List<StructureOverlay> overlays;
		public List<WeightUI> weights;
		public UIList roomList;
		public string selectedLayer;
		public float scroll;
		public ViewState(Structure structure, ARoom room) {
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
				StructureLayer[] _layers = new StructureLayer[layers.Count];
				int index = 0;
				foreach ((string name, (HashSet<char> set, Color color, SerializableTileDescriptor source) data) in layers.OrderBy(k => k.Key)) {
					bool[,] layer = new bool[map[0].Length, map.Length];
					for (int y = 0; y < map.Length; y++) {
						for (int x = 0; x < map[y].Length; x++) {
							layer[x, y] = data.set.Contains(map[y][x]);
						}
					}
					_layers[index++] = new(name, data.color, layer, data.source);
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
			if (room is DeserializedRoom { weight: WeightDescriptor dWeight }) {
				weights = [..dWeight.Parts.Select(p => new WeightUI(structure, p))];
			} else {
				weights = [];
			}
		}
		public ViewState(IEnumerable<ARoom> roomList) {
			this.roomList = [];
			instance.FillRoomList(this.roomList, roomList);
			instance.Append(this.roomList);
		}
		~ViewState() {
			roomList?.Clear();
			roomList?.Remove();
		}
		public readonly struct WeightUI {
			public readonly WeightDescriptor.Kind source;
			public readonly ParameterElement[] parameters;
			static readonly Regex constantRegex = new("^\\d*.?\\d*$");
			public WeightUI(Structure structure, string part) {
				if (constantRegex.IsMatch(part)) {
					parameters = [ModContent.GetInstance<FloatParameter>().CreateElement(structure, part)];
					return;
				}
				WeightDescriptor.Kind.TryGet(structure.Mod, part, out source, out string[] para);
				parameters = new ParameterElement[para.Length];
				int i = 0;
				foreach (Instance param in source.Parameters) {
					parameters[i] = param.Parameter.CreateElement(structure, para[i]);
					i++;
				}
			}
			public override string ToString() => source is null ? parameters[0].GetParameterValue() : $"{source.FullName}({string.Join(',', parameters.Select(p => p.GetParameterValue()))})";
		}
	}
	public abstract class StructureOverlay {
		public virtual string Name => GetType().Name;
		public abstract void Draw(SpriteBatch spriteBatch, Point basePos);
	}
	public class SocketOverlay(Vector2 pos, RoomSocket socket) : StructureOverlay {
		public override string Name => "Sockets";
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
			bool hovered = !Main.LocalPlayer.mouseInterface && Main.MouseScreen.IsWithinRectangular(pos + texture.Size() * 0.5f, texture.Size() * 0.5f);
			if (hovered) {
				Main.LocalPlayer.mouseInterface = true;
				UICommon.TooltipMouseText(socket.Key);
				if (Main.mouseLeft && Main.mouseLeftRelease && instance.connectionLookup.TryGetValue(socket.Key, out List<(ARoom room, char entrance)>[] _connections)) {
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
	public class GlobalKeyToggle : UIElement {
		readonly Asset<Texture2D> toggleButton = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");
		public bool Value { get; set; }
		public override void OnInitialize() {
			Width.Set(0, 1);
			Height.Set(16, 0);
			UIImageButtonFramed button = new(toggleButton);
			button.OnUpdate += _ => button.SetFrame(toggleButton.Frame(2, frameX: Value.ToInt()));
			button.OnLeftClick += (_, _) => Value = !Value;
			Append(button);
			Append(new UIText("Add as global tile key?") { Left = new(16, 0) });
		}
	}
	public class SocketKeySelector : UIElement {
		static readonly RoomSocket.OffsetType[] offsetTypes = Enum.GetValues<RoomSocket.OffsetType>();
		public SerializationContext serializationContext;
		readonly TextSnippet[] snippet = [new()];
		public bool optional = false;
		public RoomSocket.OffsetType offsetType = RoomSocket.OffsetType.NormalOffset;
		public override void Draw(SpriteBatch spriteBatch) {
			Vector2 position = GetDimensions().Position();
			float startY = position.Y;
			StringBuilder builder = new();
			float width = 0;
			float lineHeight = FontAssets.ItemStack.Value.LineSpacing + 4;
			foreach ((string name, int _dirs) in serializationContext.globalSocketKey) {
				builder.Clear();
				builder.Append(name);
				builder.Append(" (");
				bool comma = false;
				for (int i = 1; i <= 4; i++) {
					if ((_dirs & (1 << i)) != 0) {
						if (!comma.TrySet(true)) builder.Append(", ");
						builder.Append(((Direction)i).ToString());
					}
				}
				builder.Append(')');
				if (DrawLine(spriteBatch, ref position, startY, builder.ToString(), ref width, lineHeight)) {
					ITextInputContainer container = serializationContext.socketInput;
					container.Text.Clear();
					container.Text.Append(name);
					container.Submit();
				}
			}
			position.Y = startY;
			position.X += width + 16;
			width = 0;
			if (DrawLine(spriteBatch, ref position, startY, $"{(optional ? "☑" : "☐")} optional?", ref width, lineHeight)) {
				optional = !optional;
			}
			for (int i = 0; i < offsetTypes.Length; i++) {
				RoomSocket.OffsetType current = offsetTypes[i];
				if (DrawLine(spriteBatch, ref position, startY, $"{(current == offsetType ? "⚫" : "⚪")} {current}", ref width, lineHeight)) {
					offsetType = offsetTypes[i];
				}
			}
		}

		bool DrawLine(SpriteBatch spriteBatch, ref Vector2 position, float startY, string text, ref float width, float lineHeight) {
			snippet[0].Text = text;
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.ItemStack.Value,
				snippet,
				position,
				0,
				default,
				Vector2.One,
				out int isHovered
			);
			position.Y += lineHeight;
			Max(ref width, ChatManager.GetStringSize(FontAssets.ItemStack.Value, snippet, Vector2.One).X + 4);
			if (position.Y + lineHeight > Main.screenHeight) {
				position.Y = startY;
				position.X += width;
				width = 0;
			}
			return isHovered != -1 && Main.mouseLeft && Main.mouseLeftRelease;
		}
	}
}
public class Structure_Helper_Item : TestingItem {
	public override string Texture => "Terraria/Images/Item_" + ItemID.WireKite;
	internal static Point? leftClick;
	internal static Point? rightClick;
	static bool shouldCancel = false;
	internal static bool ShouldCancel {
		get => shouldCancel || Main.gameMenu;
		set => shouldCancel = value;
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
			if (Main.InGameUI.CurrentState is StructureHelperUI { CanModifyBounds: false }) return false;
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
	static Task<RoomDescriptor> task;
	public static void CopyStructure() {
		shouldCancel = false;
		if (task is not null && !task.IsCompleted) return;
		if (!leftClick.HasValue || !rightClick.HasValue) return;
		task = ExportRoom();
		task?.ContinueWith(_descriptor => {
			RoomDescriptor descriptor = _descriptor.Result;
			if (descriptor is null) {
				Main.NewText("Canceled");
				return;
			}
			Platform.Get<IClipboard>().Value = RoomExtensions.SerializeObject(descriptor);
			Main.NewText("Copied room to clipboard");
		});
	}
	static readonly Regex socketOptionParser = new($"(?:(optional)|({string.Join('|', Enum.GetNames<RoomSocket.OffsetType>())})|.+?)+", RegexOptions.Compiled);
	public static Task<RoomDescriptor> ExportRoom(CommandCaller caller = null, SerializationContext context = null) {
		if (!leftClick.HasValue || !rightClick.HasValue) return null;
		static void NewText(CommandCaller caller, string text, Color color = default) {
			if (caller is null) Main.NewText(text, color);
			else caller.Reply(text, color);
		}
		context ??= new();
		return Task.Run<RoomDescriptor>(() => {
			if (Structure_Helper_Item.leftClick is not Point leftClick || Structure_Helper_Item.rightClick is not Point rightClick) return null;
			Dictionary<char, string> globalKey = context.globalKey;
			FungibleSet<string> globalSocketKey = context.globalSocketKey;
			Associations.Clear();
			Associations.Add("Empty", ' ');
			Associations.Add("Void", '.');
			foreach (KeyValuePair<char, string> item in globalKey) {
				if (item.Key is ' ' or '.') continue;
				Associations.TryAdd(item.Value, item.Key);
			}
			Point min = leftClick;
			Point max = rightClick;
			MinMax(ref min.X, ref max.X);
			MinMax(ref min.Y, ref max.Y);
			string[] lines = new string[max.Y + 1 - min.Y];
			Dictionary<char, string> key = [];
			Dictionary<char, RoomSocket> socketKey = [];
			SerializableTileDescriptor.CurrentlySerializingMod = context.mod;
			for (int j = min.Y; j <= max.Y; j++) {
				StringBuilder builder = new();
				Structure_Helper_HUD.HighlightTile.Y = j;
				for (int i = min.X; i <= max.X; i++) {
					Structure_Helper_HUD.HighlightTile.X = i;
					Tile tile = Main.tile[i, j];
					CurrentTileData = SerializableTileDescriptor.Serialize(tile);
					SpinWait.SpinUntil(() => StructureCommand.ReadyForNewAss);
					if (Room_Socket_Marker.IsSocketMarker(tile)) {
						context.state = SerializationContext.SerializationState.Socket;
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
						StructureCommand.OnAss += (_c, socketKey, parameters) => {
							char c = AsChar(_c);
							if (Associations.ContainsValue(c)) return false;
							if (socket is null) {
								ch = c;
								return true;
							}
							if (string.IsNullOrWhiteSpace(socketKey)) return false;
							Match match = socketOptionParser.Match(parameters);
							_ = Enum.TryParse(match.Groups[2].Value, out RoomSocket.OffsetType offsetType);
							socket = socket with {
								Key = socketKey,
								Optional = match.Groups[1].Success,
								Offset = offsetType,
							};
							ch = c;
							return true;
						};
						Main.QueueMainThreadAction(() => {
							string format;
							if (caller is StructureHelperUI.UITextCommandCaller or StructureHelperUI.ToastCommandCaller) {
								format = "New socket, input {0} to set its {1}";
							} else {
								format = "New socket, use /ass {0} to set its {1}";
								Main.OpenPlayerChat();
								Main.chatText = "/ass ";
							}
							if (socket is null) {
								NewText(caller, string.Format(format, "<k>", "map key"));
							} else {
								NewText(caller, string.Format(format, "<k> <socket>", "map and socket keys"));
							}
						});
						SpinWait.SpinUntil(() => {
							Structure_Helper_Item.leftClick = leftClick;
							Structure_Helper_Item.rightClick = rightClick;
							return ShouldCancel || ch.HasValue;
						});
						if (ShouldCancel) {
							Associations.Clear();
							Structure_Helper_HUD.HighlightTile = default;
							context.state = SerializationContext.SerializationState.Inactive;
							return null;
						}
						builder.Append(ch.Value);
						key.Add(ch.Value, pattern);
						if (socket?.Key is not null) {
							socketKey.Add(ch.Value, socket);
							globalSocketKey[socket.Key] |= 1 << (int)socket.Direction;
						}
					} else {
						if (!Associations.ContainsKey(CurrentTileData)) {
							context.state = SerializationContext.SerializationState.Tile;
							StructureCommand.OnAss += (_c, _, _) => {
								char c = AsChar(_c);
								bool global = context.ShouldAddGlobal;
								if (global && globalKey.ContainsKey(c)) return false;
								if (Associations.ContainsValue(c) || !Associations.TryAdd(CurrentTileData, c)) return false;
								if (global) globalKey.Add(c, CurrentTileData);
								return true;
							};
							Main.QueueMainThreadAction(() => {
								string format;
								if (caller is StructureHelperUI.UITextCommandCaller or StructureHelperUI.ToastCommandCaller) {
									format = "New tile pattern {0}, input {1} to set its {2}";
								} else {
									format = "New tile pattern {0}, use /ass {1} to set its {2}";
									Main.OpenPlayerChat();
									Main.chatText = "/ass ";
								}
								NewText(caller, string.Format(format, CurrentTileData, "<k>", "map key"));
							});
							SpinWait.SpinUntil(() => {
								Structure_Helper_Item.leftClick = leftClick;
								Structure_Helper_Item.rightClick = rightClick;
								return ShouldCancel || Associations.ContainsKey(CurrentTileData);
							});
						}
						if (ShouldCancel) {
							Associations.Clear();
							Structure_Helper_HUD.HighlightTile = default;
							context.state = SerializationContext.SerializationState.Inactive;
							return null;
						}
						builder.Append(Associations[CurrentTileData]);
					}
					context.state = SerializationContext.SerializationState.Processing;
				}
				lines[j - min.Y] = builder.ToString();
			}
			Structure_Helper_HUD.HighlightTile = default;
			foreach ((string pattern, char c) in Associations) {
				key.Add(c, pattern);
			}
			Associations.Clear();
			Structure_Helper_Item.leftClick = null;
			Structure_Helper_Item.rightClick = null;
			context.state = SerializationContext.SerializationState.Inactive;
			SerializableTileDescriptor.CurrentlySerializingMod = null;
			return new() {
				Map = lines,
				Key = key,
				SocketKey = socketKey
			};
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
		MinMax(ref min.X, ref max.X);
		MinMax(ref min.Y, ref max.Y);
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
			return;
		}
	}
}
public class StructureCommand : ModCommand {
	public override CommandType Type => CommandType.Chat;
	public override string Command => "ass";
	public override string Usage => "/ass cancel or as described when prompted to use this command";
	public override string Description => "";
	public override bool IsCaseSensitive => true;
	public static event Func<string, string, string, bool> OnAss = null;
	public static bool ReadyForNewAss => OnAss is null;
	public static void Cancel() {
		Structure_Helper_Item.ShouldCancel = true;
		OnAss = null;
	}
	public static bool Associate(CommandCaller caller, string[] args) {
		if (args.Length <= 0) return false;
		if (args[0] == "cancel") {
			Cancel();
			return true;
		}
		if (OnAss?.Invoke(args[0], args.GetIfInRange(1), args.GetIfInRange(2)) ?? false) {
			OnAss = null;
			caller.Reply($"Successfully added {args[0]} <---> {Structure_Helper_Item.CurrentTileData}");
			return true;
		} else {
			caller.Reply($"Could not add {args[0]} <---> {Structure_Helper_Item.CurrentTileData}");
			return false;
		}
	}
	public override void Load() {
		MonoModHooks.Add(
			typeof(JsonTextWriter).GetConstructor([typeof(TextWriter)]),
			(Action<JsonTextWriter, TextWriter> orig, JsonTextWriter self, TextWriter writer) => {
				orig(self, writer);
				self.IndentChar = '\t';
				self.Indentation = 1;
			}
		);
	}
	public override void Action(CommandCaller caller, string input, string[] args) {
		Associate(caller, args);
	}
	public static char AsChar(string arg) {
		if (arg.Length != 1) throw new UsageException($"\" {arg} \" is not a single character in UTF-16", Color.Red);
		return arg[0];
	}
}
public class UIInputPanel(float textScale = 1, bool large = false) : UITextPanel<StringBuilder>(new(), textScale, large), ITextInputContainer {
	public int CursorIndex { get; set; }
	public bool IsWritingText { get; set; }
	StringBuilder ITextInputContainer.Text => _text;
	string lastSearch = "";
	TextInputContainerExtensions.StringDrawer drawer;
	Color lastColor;
	public void Clear() {
		_text.Clear();
		CursorIndex = 0;
	}
	public event Action<string> OnSubmit;
	public event Action OnExit;
	public void Reset() {
		IsWritingText = false;
		Clear();
		_text.Append(lastSearch);
		OnExit?.Invoke();
	}
	void ITextInputContainer.Submit() => OnSubmit?.Invoke(lastSearch = _text.ToString());
	public void OnSubmitOnce(Func<string, bool> action) {
		void onSubmit(string inputText) {
			if (action(inputText)) OnSubmit -= onSubmit;
		}
	}
	public override void Draw(SpriteBatch spriteBatch) {
		if (IsWritingText) {
			this.ProcessInput(out bool typed);
			if (typed) SetText(_text);
		}
		DrawSelf(spriteBatch);
		if (IsWritingText) {
			CalculatedStyle innerDimensions = GetInnerDimensions();
			Vector2 pos = innerDimensions.Position();
			if (_isLarge)
				pos.Y += 10f * _textScale * _textScale;
			else
				pos.Y += 4f * _textScale;

			pos.X += (innerDimensions.Width - _textSize.X) * TextHAlign;
			if (lastColor.TrySet(TextColor) || drawer is null) drawer = TextInputContainerExtensions.BasicStringDrawer(TextColor);
			TextInputContainerExtensions.DrawInputTextCursor(
				PegasusConfig.Instance.preferredTextCursor,
				Text,
				CursorIndex,
				spriteBatch,
				pos,
				FontAssets.MouseText.Value,
				drawer,
				_textScale
			);
		}
		DrawChildren(spriteBatch);
	}
}
public class UIConditionalButton(Asset<Texture2D> texture, Func<string> condition) : UIImageButton(texture) {
	public override bool ContainsPoint(Vector2 point) {
		if (!base.ContainsPoint(point)) return false;
		if (condition() is string reason) {
			Main.instance.MouseText(reason);
			return false;
		}
		return true;
	}
}
public class UIImageButtonFramed(Asset<Texture2D> texture) : UIElement() {
	Rectangle frame;
	public void SetFrame(Rectangle frame) {
		//Non-short-circuiting or because it needs to do both even if width succeeds
		this.frame = frame;
		if (Width.TrySet(new(frame.Width, 0f)) | Height.TrySet(new(frame.Height, 0f))) Recalculate();
	}
	protected override void DrawSelf(SpriteBatch spriteBatch) {
		CalculatedStyle dimensions = GetDimensions();
		spriteBatch.Draw(texture.Value, dimensions.ToRectangle(), frame, Color.White);
	}
}
public class UIImageButtonVisibility(bool visible = true) : UIImageButton(GetImage(visible)) {
	static readonly Asset<Texture2D> toggleOn = Main.Assets.Request<Texture2D>("Images/Inventory_Tick_On");
	static readonly Asset<Texture2D> toggleOff = Main.Assets.Request<Texture2D>("Images/Inventory_Tick_Off");
	public bool Visible { get; private set; } = visible;
	public override void LeftClick(UIMouseEvent evt) {
		base.LeftClick(evt);
		Visible = !Visible;
		SetImage(GetImage(Visible));
	}
	static Asset<Texture2D> GetImage(bool visible) => visible ? toggleOn : toggleOff;
}
public class SerializationContext {
	public SerializationState state;
	public Dictionary<char, string> globalKey = [];
	public FungibleSet<string> globalSocketKey = [];
	public StructureHelperUI.GlobalKeyToggle globalKeyToggle;
	public UIInputPanel socketInput;
	public Mod mod;
	public bool ShouldAddGlobal => globalKeyToggle?.Value ?? false;
	public enum SerializationState {
		Inactive,
		Processing,
		Tile,
		Socket
	}
}
