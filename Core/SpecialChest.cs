using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Reflection;
using PegasusLib.Networking;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.UI;
using Terraria.UI.Chat;
using static Origins.Core.SpecialChest.SpecialChestUI;

namespace Origins.Core {
	public class SpecialChest : ILoadable {
		public const int chestID = -8479;
		public static ChestData CurrentChest { get; internal set; }
		public static void OpenChest(int x, int y) {
			Tile tile = Main.tile[x, y];
			int style = TileObjectData.GetTileStyle(tile);
			if (style >= 0) TileUtils.GetMultiTileTopLeft(x, y, TileObjectData.GetTileData(tile.TileType, style), out x, out y);
			Player player = Main.LocalPlayer;
			if (player.chest == chestID && player.chestX == x && player.chestY == y) {
				player.chest = -1;
				if (OriginsSets.Tiles.ChestSoundOverride[tile.TileType].close == default) SoundEngine.PlaySound(SoundID.MenuClose);
				CurrentChest = default;
				return;
			}
			if (TryGetChest(x, y) is ChestData chest) {
				player.chest = chestID;
				Main.playerInventory = true;
				Main.editChest = false;
				Main.npcChatText = "";
				Main.ClosePlayerChat();
				Main.chatText = "";
				player.chestX = x;
				player.chestY = y;
				ModContent.GetInstance<SpecialChestSystem>().chestUI.SetState(new SpecialChestUI());
				Recipe.FindRecipes();
				CurrentChest = chest;
			}
		}
		static bool sendChestSync = false;
		public static void MarkConsumedItem() => sendChestSync = true;
		public static void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(tile), out int left, out int top);
			player.cursorItemIconID = -1;
			string name = TryGetChest(left, top)?.GivenName;
			if ((name?.Length ?? 0) > 0) {
				player.cursorItemIconText = name;
			} else {
				player.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(tile.TileType);
				player.cursorItemIconText = "";
			}
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
		}
		public static ChestData TryGetChest(int i, int j) {
			Player player = Main.LocalPlayer;
			if (player.chest == chestID && player.chestX == i && player.chestY == j) return CurrentChest;
			return ModContent.GetInstance<SpecialChestSystem>().tileEntities.TryGetValue(new(i, j), out ChestData chest) ? chest : null;
		}
		void ILoadable.Load(Mod mod) {
			IL_ChestUI.Draw += IL_ChestUI_Draw;
			On_ItemSlot.OverrideHover_ItemArray_int_int += On_ItemSlot_OverrideHover_ItemArray_int_int;
			On_ItemSlot.OverrideLeftClick += On_ItemSlot_OverrideLeftClick;
			On_ItemSlot.LeftClick_SellOrTrash += On_ItemSlot_LeftClick_SellOrTrash;
			On_Player.HandleBeingInChestRange += On_Player_HandleBeingInChestRange;
			On_Recipe.CollectItems_ItemArray_int += On_Recipe_CollectItems_ItemArray_int;
			On_Recipe.Create += On_Recipe_Create;
			On_Chest.PutItemInNearbyChest += On_Chest_PutItemInNearbyChest;
			static bool On_ItemSlot_LeftClick_SellOrTrash(On_ItemSlot.orig_LeftClick_SellOrTrash orig, Item[] inv, int context, int slot) {
				if (Main.LocalPlayer.chest == chestID) return false;
				return orig(inv, context, slot);
			}
			static void On_Player_HandleBeingInChestRange(On_Player.orig_HandleBeingInChestRange orig, Player self) {
				if (self.chest == chestID && TryGetChest(self.chestX, self.chestY) is ChestData chest) {
					chest.HandleBeingInChestRange(self);
					return;
				}
				orig(self);
			}

			static void On_ItemSlot_OverrideHover_ItemArray_int_int(On_ItemSlot.orig_OverrideHover_ItemArray_int_int orig, Item[] inv, int context, int slot) {
				if (Main.LocalPlayer.chest == chestID && CurrentChest.OverrideHover(inv, context, slot)) return;
				orig(inv, context, slot);
			}
			static bool On_ItemSlot_OverrideLeftClick(On_ItemSlot.orig_OverrideLeftClick orig, Item[] inv, int context, int slot) {
				if (Main.LocalPlayer.chest == chestID && CurrentChest.OverrideLeftClick(inv, context, slot)) return true;
				return orig(inv, context, slot);
			}
			static void IL_ChestUI_Draw(ILContext il) {
				ILCursor c = new(il);
				ILLabel label = default;
				c.GotoNext(MoveType.After,
					i => i.MatchLdfld<Player>(nameof(Player.chest)),
					i => i.MatchLdcI4(-1),
					i => i.MatchBeq(out label)
				);
				c.EmitDelegate(() => Main.LocalPlayer.chest == chestID);
				c.EmitBrtrue(label);
			}
			static void On_Recipe_CollectItems_ItemArray_int(On_Recipe.orig_CollectItems_ItemArray_int orig, Item[] currentInventory, int slotCap) {
				if (currentInventory is null) return;
				orig(currentInventory, slotCap);
			}
			static void On_Recipe_Create(On_Recipe.orig_Create orig, Recipe self) {
				sendChestSync = false;
				orig(self);
				if (sendChestSync.TrySet(false)) {
					Player player = Main.LocalPlayer;
					if (player.chest == chestID && TryGetChest(player.chestX, player.chestY) is ChestData chest) {
						new Set_Special_Chest_Action(new(player.chestX, player.chestY), chest).Perform();
					}
				}
			}
			static Item On_Chest_PutItemInNearbyChest(On_Chest.orig_PutItemInNearbyChest orig, Item item, Vector2 position) {
				foreach ((Point16 chestPosition, ChestData chest) in ModContent.GetInstance<SpecialChestSystem>().tileEntities) {
					chest.ReceiveNearbyQuickStack(chestPosition, item, position);
					if (item.IsAir) return item;
				}
				return orig(item, position);
			}
		}

		class SpecialChestCraftingPlayer : ModPlayer {
			public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback) {
				if (Player.chest == chestID && TryGetChest(Player.chestX, Player.chestY) is ChestData chest) {
					itemConsumedCallback = chest.CraftWithItem;
					return chest.Items(true);
				}
				return base.AddMaterialsForCrafting(out itemConsumedCallback);
			}
		}
		public static string MapChestName(string name, int i, int j) {
			TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out int left, out int top);
			string renamed = TryGetChest(left, top)?.GivenName ?? "";
			if (renamed == "") {
				return name;
			} else {
				return name + ": " + renamed;
			}
		}
		void ILoadable.Unload() { }
		public abstract record class ChestData : ILoadable {
			static readonly List<ChestData> chestDatas = [];
			static readonly Dictionary<Type, int> specialChestIDsByType = [];
			static readonly Dictionary<string, int> specialChestIDsByTypeName = [];
			protected ChestData() {
				specialChestIDsByType?.TryGetValue(GetType(), out type);
			}
			private int type;
			public void Load(Mod mod) {
				if (mod.Side != ModSide.Both) throw new InvalidOperationException("ChestDatas can only be added by Both-side mods");
				type = chestDatas.Count;
				chestDatas.Add(this);
				specialChestIDsByType.Add(GetType(), type);
				specialChestIDsByTypeName.Add(GetType().Name, type);
				mod.Logger.Info($"{nameof(ChestData)} Loading {GetType().Name}");
			}
			public void Unload() { }
			public static ChestData Read(BinaryReader reader) {
				return chestDatas[ReadType(reader)].NetReceive(reader);
			}
			public void Write(BinaryWriter packet) {
				WriteType(packet);
				NetSend(packet);
			}
			public abstract Item[] Items(bool forCrafting = false);
			internal abstract ChestData NetReceive(BinaryReader reader);
			internal abstract void NetSend(BinaryWriter writer);
			void WriteType(BinaryWriter writer) {
				if (chestDatas.Count < byte.MaxValue) writer.Write((byte)type);
				else if (chestDatas.Count < ushort.MaxValue) writer.Write((ushort)type);
				else writer.Write((int)type);
			}
			static int ReadType(BinaryReader reader) {
				if (chestDatas.Count < byte.MaxValue) return reader.ReadByte();
				if (chestDatas.Count < ushort.MaxValue) return reader.ReadUInt16();
				return reader.ReadInt32();
			}
			public TagCompound Save() {
				TagCompound data = new();
				SaveData(data);
				return new() {
					["Type"] = GetType().Name,
					["Data"] = data
				};
			}
			public static ChestData Load(TagCompound tag) {
				if (!tag.TryGet("Type", out string type) || !tag.TryGet("Data", out TagCompound data)) throw new ArgumentException($"Malformed ChestData {tag}", nameof(tag));
				if (!specialChestIDsByTypeName.TryGetValue(type, out int chestID)) return new Invalid();
				return chestDatas[chestID].LoadData(data);
			}
			protected abstract void SaveData(TagCompound tag);
			protected abstract ChestData LoadData(TagCompound tag);
			protected internal abstract bool IsValidSpot(Point position);
			public virtual void UpdateUI(SpecialChestUI.SpecialChestElement ui) { }
			public virtual int ItemCount => Items().Length;
			public virtual string GivenName => "";
			public virtual void HandleBeingInChestRange(Player player) {
				if (!player.IsInInteractionRange(2, 2)) {
					if (player.chest != -1) {
						SoundEngine.PlaySound(SoundID.MenuClose);
					}
					player.chest = -1;
					Recipe.FindRecipes();
				}
			}
			public virtual bool InteractWithItem(Item item, int slot) {
				ItemSlot.OverrideHover(ref item, ItemSlot.Context.ChestItem);
				bool isLeft = Main.mouseLeftRelease && Main.mouseLeft;
				bool didSomething = false;
				if (!Main.LocalPlayer.ItemAnimationActive && slot <= ItemCount) {
					Item clone = item.Clone();
					ItemSlot.LeftClick(ref item, ItemSlot.Context.BankItem);
					ItemSlot.RightClick(ref item, ItemSlot.Context.BankItem);
					didSomething = item.IsNetStateDifferent(clone);
					if (didSomething) {
						Items()[slot] = item;
						Recipe.FindRecipes();
						SoundEngine.PlaySound(SoundID.Grab);
					}
				}
				ItemSlot.MouseHover(ref item, ItemSlot.Context.ChestItem);
				return didSomething;
			}
			public virtual void DrawItemSlot(SpriteBatch spriteBatch, Vector2 position, Item item, int slot) {
				ItemSlot.Draw(spriteBatch, ref item, slot >= ItemCount ? ItemSlot.Context.GoldDebug : ItemSlot.Context.ChestItem, position);
			}
			/// <summary>
			/// Called when an item from the <see cref="ChestData"/> is consumed via crafting
			/// Remember to call <see cref="MarkConsumedItem"/> if the item being consumed should produce changes visible to other players, such as if this is a container
			/// </summary>
			public virtual void CraftWithItem(Item item, int index) => MarkConsumedItem();
			public virtual bool CanDestroy() => true;
			/// <summary>
			/// Return <see cref="true"/> to skip the vanilla code
			/// </summary>
			public virtual bool OverrideHover(Item[] inv, int context, int slot) => false;
			public virtual bool OverrideLeftClick(Item[] inv, int context, int slot) => false;
			public virtual Item ReceiveNearbyQuickStack(Point16 chestPosition, Item item, Vector2 playerPosition) => item;
			public virtual IEnumerable<SpecialChestButton> Buttons { get; }
			public delegate void ItemTransferHandler(Item item, int countTransfered);
			public Item ReceiveQuickStack(Item item, ItemTransferHandler onTransfer) {
				Item[] items = Items();
				bool hasMatchingItem = false;
				bool hasEmptySlot = false;
				for (int j = 0; j < items.Length; j++) {
					if (items[j].IsAir) {
						hasEmptySlot = true;
					} else if (item.type == items[j].type && ItemLoader.CanStack(items[j], item)) {
						hasMatchingItem = true;
						if (ItemLoader.TryStackItems(items[j], item, out int numTransfered)) {
							onTransfer?.Invoke(item, numTransfered);
							if (item.stack <= 0) {
								item.SetDefaults();
								return item;
							}
						}
					}
				}
				if (!hasMatchingItem || !hasEmptySlot || item.stack <= 0) return item;
				for (int k = 0; k < items.Length; k++) {
					if (items[k].IsAir) {
						onTransfer?.Invoke(item, item.stack);
						items[k] = item.Clone();
						item.SetDefaults();
						return item;
					}
				}
				return item;
			}
			public bool TryPlacingInChest(Item item, bool justCheck) {
				Item[] items = Items();
				bool sync = false;
				bool success = false;
				for (int i = 0; i < items.Length; i++) {
					if (items[i].stack >= items[i].maxStack || item.type != items[i].type || !ItemLoader.CanStack(items[i], item)) {
						continue;
					}
					int depositedCount = item.stack;
					if (item.stack + items[i].stack > items[i].maxStack) {
						depositedCount = items[i].maxStack - items[i].stack;
					}
					if (justCheck) {
						success = success || depositedCount > 0;
						break;
					}
					ItemLoader.StackItems(items[i], item, out _);
					SoundEngine.PlaySound(SoundID.Grab);
					if (item.stack <= 0) {
						item.SetDefaults();
						sync = true;
						break;
					}
					if (items[i].type == ItemID.None) {
						items[i] = item.Clone();
						item.SetDefaults();
					}
					sync = true;
				}
				if (item.stack > 0) {
					for (int i = 0; i < items.Length; i++) {
						if (items[i].stack != 0) {
							continue;
						}
						if (justCheck) {
							success = true;
							break;
						}
						SoundEngine.PlaySound(SoundID.Grab);
						items[i] = item.Clone();
						item.SetDefaults();
						ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(items[i], 0, 3));
						sync = true;
						break;
					}
				}
				if (sync && !justCheck) {
					Player player = Main.LocalPlayer;
					ModContent.GetInstance<SpecialChestSystem>().netDirtyChests.Add(new(player.chestX, player.chestY));
				}
				return success;
			}
			public sealed record class Invalid : ChestData {
				protected internal override bool IsValidSpot(Point position) => false;
				public override Item[] Items(bool forCrafting = false) => [];
				protected override ChestData LoadData(TagCompound tag) => this;
				protected override void SaveData(TagCompound tag) { }
				internal override ChestData NetReceive(BinaryReader reader) => this;
				internal override void NetSend(BinaryWriter writer) { }
			}
		}
		public abstract record class Storage_Container_Data(Item[] Inventory) : ChestData(), RenameButton.IRenamable {
			public abstract int Capacity { get; }
			public abstract int Width { get; }
			public abstract int Height { get; }
			public string RenamableName { get; set; }
			public override string GivenName => RenamableName;
			public override IEnumerable<SpecialChestButton> Buttons => [
				ModContent.GetInstance<LootAllButton>(),
				ModContent.GetInstance<DepositAllButton>(),
				ModContent.GetInstance<QuickStackButton>(),
				ModContent.GetInstance<RestockButton>(),
				ModContent.GetInstance<SortButton>(),
				..RenameButton.Buttons,
			];
			public Storage_Container_Data() : this([]) {
				Item[] inventory = Inventory;
				Array.Resize(ref inventory, Capacity);
				for (int i = 0; i < inventory.Length; i++) inventory[i] ??= new();
				Inventory = inventory;
			}
			public override Item[] Items(bool forCrafting = false) => Inventory;
			protected override ChestData LoadData(TagCompound tag) => this with {
				Inventory = tag.SafeGet("Items", Enumerable.Repeat(0, Capacity).Select(_ => new Item()).ToList()).ToArray(),
				RenamableName = tag.SafeGet<string>("Name")
			};
			protected override void SaveData(TagCompound tag) {
				tag["Items"] = Inventory.ToList();
				if (!string.IsNullOrEmpty(RenamableName)) tag["Name"] = RenamableName;
			}
			internal override ChestData NetReceive(BinaryReader reader) => this with {
				Inventory = reader.ReadCompressedItemArray(),
				RenamableName = reader.ReadString()
			};
			internal override void NetSend(BinaryWriter writer) {
				writer.WriteCompressedItemArray(Inventory);
				writer.Write(RenamableName);
			}
			public override void HandleBeingInChestRange(Player player) {
				if (!player.IsInInteractionRange(Width, Height)) {
					if (player.chest != -1) {
						SoundEngine.PlaySound(SoundID.MenuClose);
					}
					player.chest = -1;
					Recipe.FindRecipes();
				}
			}
			public override bool CanDestroy() {
				Item[] items = Items();
				for (int i = 0; i < items.Length; i++) {
					if (items[i]?.IsAir == false) return false;
				}
				return true;
			}
			public override bool OverrideHover(Item[] inv, int context, int slot) {
				switch (context) {
					case ItemSlot.Context.InventoryItem:
					case ItemSlot.Context.InventoryCoin:
					case ItemSlot.Context.InventoryAmmo:
					if (ItemSlot.ShiftInUse) {
						if (TryPlacingInChest(inv[slot], true)) Main.cursorOverride = CursorOverrideID.InventoryToChest;
						return true;
					}
					break;
				}
				return false;
			}
			public override bool OverrideLeftClick(Item[] inv, int context, int slot) {
				if (ItemSlot.ShiftInUse && PlayerLoader.ShiftClickSlot(Main.LocalPlayer, inv, context, slot)) return true;
				if (Main.cursorOverride == 9) {
					TryPlacingInChest(inv[slot], false);
					ModContent.GetInstance<SpecialChestSystem>().ConsumeNetDirtyChests();
					return true;
				}
				return false;
			}
			public override Item ReceiveNearbyQuickStack(Point16 chestPosition, Item item, Vector2 playerPosition) {
				Rectangle chestRect = new(chestPosition.X * 16, chestPosition.Y * 16, Width * 16, Height * 16);
				if (!playerPosition.IsWithin(playerPosition.Clamp(chestRect), 600f)) return item;

				ReceiveQuickStack(item, (item, countTransfered) => {
					if (countTransfered > 0) {
						Chest.VisualizeChestTransfer(playerPosition, chestRect.Center(), item, countTransfered);
						ModContent.GetInstance<SpecialChestSystem>().netDirtyChests.Add(chestPosition);
					}
				});
				return item;
			}
		}
		public static void SyncToPlayer(int player) => ModContent.GetInstance<SpecialChestSystem>().SyncToPlayer(player);
		class SpecialChestSystem : ModSystem {
			public UserInterface chestUI = new();
			public override void UpdateUI(GameTime gameTime) {
				if (chestUI.CurrentState is not null && (!Main.playerInventory || Main.LocalPlayer.chest != chestID)) {
					chestUI.SetState(null);
				}
				chestUI.Update(gameTime);
			}
			public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
				int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
				if (inventoryIndex != -1) {//error prevention & null check
					layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
						"Origins: Special Chest UI",
						delegate {
							chestUI?.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
							return true;
						},
						InterfaceScaleType.UI) { Active = Main.playerInventory }
					);
				}
			}

			public Dictionary<Point16, ChestData> tileEntities = [];
			public HashSet<Point16> netDirtyChests = [];
			public void SyncToPlayer(int player) {
				foreach ((Point16 position, ChestData data) in tileEntities) {
					new Set_Special_Chest_Action(position, data).Send(player);
				}
			}
			public override void SaveWorldData(TagCompound tag) {
				tag[$"{nameof(tileEntities)}"] = tileEntities.Select(kvp => new TagCompound {
					["key"] = kvp.Key,
					["data"] = kvp.Value.Save()
				}).ToList();
			}
			public override void LoadWorldData(TagCompound tag) {
				TagCompound invalidChestData = new() {
					["Type"] = nameof(ChestData.Invalid)
				};
				tileEntities = tag
					.SafeGet<List<TagCompound>>($"{nameof(tileEntities)}", [])
					.Select(t => (t.SafeGet<Point16>("key"), ChestData.Load(t.SafeGet("data", invalidChestData))))
					.ToDictionary();
				KeyValuePair<Point16, ChestData>[] entries = tileEntities.ToArray();
				for (int i = 0; i < entries.Length; i++) {
					if (!entries[i].Value.IsValidSpot(entries[i].Key.ToPoint())) {
						ModContent.GetInstance<Origins>().Logger.Warn($"Attempted to load Special Chest {entries[i].Value} at invalid position {entries[i].Key}");
						((ICollection<KeyValuePair<Point16, ChestData>>)tileEntities).Remove(entries[i]);
					}
				}
			}
			public void ConsumeNetDirtyChests() {
				if (!NetmodeActive.SinglePlayer) {
					foreach (Point16 chest in netDirtyChests) {
						new Set_Special_Chest_Action(chest, tileEntities[chest]).Send();
					}
				}
				netDirtyChests.Clear();
			}
			public override void PreUpdateEntities() => ConsumeNetDirtyChests();
		}
		class PreventDestruction : GlobalTile {
			public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged) {
				Tile tile = Main.tile[i, j];
				int style = TileObjectData.GetTileStyle(tile);
				if (style >= 0) TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(tile.TileType, style), out i, out j);
				return TryGetChest(i, j)?.CanDestroy() ?? true;
			}
			public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
				new Destroy_Special_Chest_Action(new(i, j)).Perform();
			}
		}
		public record class Set_Special_Chest_Action(Point16 Position, ChestData Data) : SyncedAction {
			public Set_Special_Chest_Action() : this(default, default) { }
			public override SyncedAction NetReceive(BinaryReader reader) => this with {
				Position = new(reader.ReadInt16(), reader.ReadInt16()),
				Data = ChestData.Read(reader)
			};
			public override void NetSend(BinaryWriter writer) {
				writer.Write((short)Position.X);
				writer.Write((short)Position.Y);
				Data.Write(writer);
			}
			protected override void Perform() {
				if (!Data.IsValidSpot(Position.ToPoint())) {
					ModContent.GetInstance<Origins>().Logger.Warn($"Attempted to place Special Chest {Data} at invalid position {Position}");
					return;
				}
				ModContent.GetInstance<SpecialChestSystem>().tileEntities[Position] = Data;
				Player localPlayer = Main.LocalPlayer;
				if (localPlayer.chest == chestID && localPlayer.chestX == Position.X && localPlayer.chestY == Position.Y) CurrentChest = Data;
			}
		}
		public record class Destroy_Special_Chest_Action(Point16 Position) : SyncedAction {
			protected override bool ShouldPerform => ModContent.GetInstance<SpecialChestSystem>().tileEntities.ContainsKey(Position);
			public Destroy_Special_Chest_Action() : this(Point16.Zero) { }
			public override SyncedAction NetReceive(BinaryReader reader) => this with {
				Position = new(reader.ReadInt16(), reader.ReadInt16())
			};
			public override void NetSend(BinaryWriter writer) {
				writer.Write((short)Position.X);
				writer.Write((short)Position.Y);
			}
			protected override void Perform() {
				Dictionary<Point16, ChestData> tileEntities = ModContent.GetInstance<SpecialChestSystem>().tileEntities;
				if (!tileEntities.TryGetValue(Position, out ChestData chest)) return;
				if (!chest.CanDestroy()) {
					ModContent.GetInstance<Origins>().Logger.Warn($"Attempted to destroy Special Chest {chest} at position {Position}, but it cannot be destroyed");
					return;
				}
				tileEntities.Remove(Position);
				Player localPlayer = Main.LocalPlayer;
				if (localPlayer.chest == chestID && localPlayer.chestX == Position.X && localPlayer.chestY == Position.Y) CurrentChest = null;
			}
		}
		public class SpecialChestUI() : UIState {
			public static int inputCursorIndex;
			public static StringBuilder inputText = new();
			public static IInputTextTaker inputTextTaker;
			public static bool InputtingText => inputTextTaker is not null;
			public override void OnInitialize() {
				Append(new SpecialChestElement());
			}
			public static void SubmitText() {
				inputTextTaker?.Submit(inputText.ToString());
				inputTextTaker = null;
			}
			public static void CancelText() {
				inputTextTaker?.Cancel();
				inputTextTaker = null;
			}
			public class SpecialChestElement : UIElement, ITextInputContainer {
				public int CursorIndex { get => inputCursorIndex; set => inputCursorIndex = value; }
				public StringBuilder Text => inputText;
				UIScrollbar scrollbar;
				public override void OnInitialize() {
					Left.Set(51f, 0);
					Top.Set(Main.instance.invBottom, 0);
					Width.Set(560f * 0.755f + 22, 0);
					Height.Set(224f * 0.755f, 0);

					scrollbar = new();
					scrollbar.Left.Set(0, 0);
					scrollbar.Top.Set(10, 0);
					scrollbar.Height.Set(-20, 1);
					Append(scrollbar);
					scrollbar.OnUpdate += element => {
						if (element.IsMouseHovering) Main.LocalPlayer.mouseInterface = true;
					};
				}
				public void Submit() => SubmitText();
				public void Reset() => CancelText();
				public override void Update(GameTime gameTime) {
					if (Main.LocalPlayer.chest != chestID || CurrentChest is null) return;
					base.Update(gameTime);
					CurrentChest.UpdateUI(this);
					scrollbar.Left.Pixels = CurrentChest.ItemCount > 40 ? 0 : -Main.screenWidth;
					scrollbar.SetView(56 * 4, MathF.Ceiling(CurrentChest.ItemCount / 10f) * 56);
				}
				protected override void DrawSelf(SpriteBatch spriteBatch) {
					Player player = Main.LocalPlayer;
					if (player.chest != chestID || CurrentChest is null) return;
					float inventoryScale = Main.inventoryScale;
					Main.inventoryScale = 0.755f;
					{
						Vector2 position = new(504f, Main.instance.invBottom);
						string name = CurrentChest.GivenName;
						if (SpecialChestUI.InputtingText) {
							this.ProcessInput(out bool typed);
							name = Text.ToString();
						} else {
							CursorIndex = Math.Max(name?.Length ?? 1, 1) - 1;
							Text.Clear();
							if (string.IsNullOrEmpty(name)) {
								Tile tile = Main.tile[player.chestX, player.chestY];
								name = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY);
							}
						}
						name ??= "";

						Vector2 offset = new(0, 2);
						bool blink = Main.timeForVisualEffects % 40 < 20;
						if (SpecialChestUI.InputtingText && blink) {
							spriteBatch.DrawString(
								FontAssets.MouseText.Value,
								"|",
								position + FontAssets.MouseText.Value.MeasureString(Text.ToString()[..CursorIndex]) * Vector2.UnitX * 1 + offset * new Vector2(0.5f, 1),
								Color.White * (blink ? 1 : 0.5f),
								0,
								new(0, 0),
								1,
								SpriteEffects.None,
							0);
						}

						Color color = Color.White * (1f - (255f - Main.mouseTextColor) / 255f * 0.5f);
						color.A = byte.MaxValue;
						ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, name, position, color, 0f, Vector2.Zero, Vector2.One, -1f, 1.5f);
					}

					int buttonY = Main.instance.invBottom + 40;
					foreach (SpecialChestButton button in CurrentChest.Buttons) {
						if (button.Draw(spriteBatch, 506, buttonY)) buttonY += 26;
					}

					int viewPos = scrollbar is null ? 0 : (int)(scrollbar.ViewPosition / 56);
					Item[] items = CurrentChest.Items();
					bool didAnything = false;
					for (int i = 0; i < 10; i++) {
						for (int j = 0; j < 4; j++) {
							int slot = i + (j + viewPos) * 10;
							int xPos = (int)(73f + (i * 56) * Main.inventoryScale);
							int yPos = (int)(Main.instance.invBottom + (j * 56) * Main.inventoryScale);
							Item item = slot >= items.Length ? new() : items[slot].Clone();
							if (!PlayerInput.IgnoreMouseInterface && Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, xPos, yPos, TextureAssets.InventoryBack.Width() * Main.inventoryScale, TextureAssets.InventoryBack.Height() * Main.inventoryScale)) {
								player.mouseInterface = true;
								didAnything |= CurrentChest.InteractWithItem(item, slot);
							}
							CurrentChest.DrawItemSlot(spriteBatch, new(xPos, yPos), item, slot);
						}
					}
					if (didAnything) new Set_Special_Chest_Action(new(Main.LocalPlayer.chestX, Main.LocalPlayer.chestY), CurrentChest).Perform();
					Main.inventoryScale = inventoryScale;
				}
				public override void ScrollWheel(UIScrollWheelEvent evt) {
					if (scrollbar is null) return;
					int viewPos = (int)(scrollbar.ViewPosition / 56);
					scrollbar.ViewPosition = (viewPos - evt.ScrollWheelValue / 120) * 56;
				}
			}
			public interface IInputTextTaker {
				void Submit(string text);
				void Cancel();
			}
		}
		public abstract class SpecialChestButton : ModType, ILocalizedModType {
			public string LocalizationCategory => "ChestButton";
			protected sealed override void Register() { }
			public virtual LocalizedText Text => this.GetLocalization(nameof(Text));
			public virtual bool CanDisplay => !SpecialChestUI.InputtingText;
			public abstract bool Click();
			bool hovered = false;
			float scale = 0.75f;
			public bool Draw(SpriteBatch spriteBatch, int x, int y) {
				if (!CanDisplay) {
					hovered = false;
					MathUtils.LinearSmoothing(ref scale, 0.75f, 0.05f);
					return false;
				}
				string text = Text.Value;
				Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text);
				Color color = Color.White * 0.97f * (1f - (255f - Main.mouseTextColor) / 255f * 0.5f);
				color.A = byte.MaxValue;
				x += (int)(textSize.X * scale / 2f);
				bool isHovered;
				if (hovered) {
					isHovered = Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, x - textSize.X * 0.5f - 10f, y - 12, textSize.X + 16f, 24f);
				} else {
					isHovered = Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, x - textSize.X * 0.5f, y - 12, textSize.X, 24f);
				}
				if (isHovered) color = Main.OurFavoriteColor;
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, new Vector2(x, y), color, 0f, textSize / 2f, new Vector2(scale), -1f, 1.5f);
				if (isHovered) {
					if (hovered.TrySet(true)) SoundEngine.PlaySound(SoundID.MenuTick);
					MathUtils.LinearSmoothing(ref scale, 1f, 0.05f);
					if (PlayerInput.IgnoreMouseInterface) return true;
					Main.LocalPlayer.mouseInterface = true;
					if (Main.mouseLeft && Main.mouseLeftRelease) {
						if (Click()) ModContent.GetInstance<SpecialChestSystem>().netDirtyChests.Add(new(Main.LocalPlayer.chestX, Main.LocalPlayer.chestY));
					}
				} else {
					hovered = false;
					MathUtils.LinearSmoothing(ref scale, 0.75f, 0.05f);
				}
				return true;
			}
		}
		public class LootAllButton : SpecialChestButton {
			public override LocalizedText Text => Language.GetText("LegacyInterface.29");
			public override bool Click() {
				bool didAnything = false;
				Item[] items = CurrentChest.Items();
				GetItemSettings lootAllSettingsRegularChest = GetItemSettings.LootAllSettingsRegularChest;
				for (int i = 0; i < items.Length; i++) {
					if (items[i].IsAir) continue;
					didAnything = true;
					items[i].position = Main.LocalPlayer.Center;
					items[i] = Main.LocalPlayer.GetItem(Main.myPlayer, items[i], lootAllSettingsRegularChest);
				}
				return didAnything;
			}
		}
		public class DepositAllButton : SpecialChestButton {
			public override LocalizedText Text => Language.GetText("LegacyInterface.30");
			public override bool Click() {
				bool didAnything = false;
				for (int i = 10; i < Main.InventoryItemSlotsCount; i++) {
					if (Main.LocalPlayer.inventory[i].favorited) continue;
					didAnything |= CurrentChest.TryPlacingInChest(Main.LocalPlayer.inventory[i], false);
				}
				return didAnything;
			}
		}
		public class QuickStackButton : SpecialChestButton {
			public override LocalizedText Text => Language.GetText("LegacyInterface.31");
			public override bool Click() {
				bool didAnything = false;
				for (int i = 10; i < Main.InventoryItemSlotsCount + Main.InventoryCoinSlotsCount + Main.InventoryAmmoSlotsCount; i++) {
					if (Main.LocalPlayer.inventory[i].favorited) continue;
					CurrentChest.ReceiveQuickStack(Main.LocalPlayer.inventory[i], (_, _) => didAnything = true);
				}
				return didAnything;
			}
		}
		public class RestockButton : SpecialChestButton {
			public override LocalizedText Text => Language.GetText("LegacyInterface.82");
			public override bool Click() {
				bool didAnything = false;
				Item[] items = CurrentChest.Items();
				List<Item> stacksToRestock = [];
				for (int i = 0; i < Main.InventoryItemSlotsCount + Main.InventoryCoinSlotsCount + Main.InventoryAmmoSlotsCount; i++) {
					Item item = Main.LocalPlayer.inventory[i];
					if (item.IsAir || item.stack >= item.maxStack) continue;
					stacksToRestock.Add(item);
				}
				for (int i = 0; i < items.Length; i++) {
					if (items[i].IsAir) continue;
					for (int j = stacksToRestock.Count - 1; j >= 0; j--) {
						if (stacksToRestock[j].type != items[i].type) continue;
						didAnything |= ItemLoader.TryStackItems(stacksToRestock[j], items[i], out int countTransferred) && countTransferred > 0;
						if (stacksToRestock[j].stack >= stacksToRestock[j].maxStack) stacksToRestock.RemoveAt(j);
					}
				}
				return didAnything;
			}
		}
		public class SortButton : SpecialChestButton {
			public override LocalizedText Text => Language.GetText("LegacyInterface.122");
			public override bool Click() {
				ItemSortingMethods.Sort(CurrentChest.Items());
				return true;
			}
		}
		public class RenameButton : SpecialChestButton, IInputTextTaker {
			public static IEnumerable<SpecialChestButton> Buttons => [
				ModContent.GetInstance<RenameButton>(),
				ModContent.GetInstance<SaveRenameButton>(),
				ModContent.GetInstance<CancelRenameButton>(),
			];
			public override LocalizedText Text => Language.GetText("LegacyInterface.61");
			public void Submit(string text) {
				string inputText = SpecialChestUI.inputText.ToString();
				if (CurrentChest.GivenName != inputText && CurrentChest is IRenamable renamable) {
					renamable.RenamableName = inputText;
				}
			}
			public void Cancel() { }
			public override bool Click() {
				SpecialChestUI.inputTextTaker = this;
				string name = CurrentChest.GivenName;
				if (string.IsNullOrEmpty(name)) {
					Player player = Main.LocalPlayer;
					Tile tile = Main.tile[player.chestX, player.chestY];
					name = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY);
				}
				SpecialChestUI.inputText.Clear();
				SpecialChestUI.inputText.Append(name);
				SpecialChestUI.inputCursorIndex = name.Length;
				return false;
			}
			public interface IRenamable {
				string RenamableName { get; set; }
			}
			public class SaveRenameButton : SpecialChestButton {
				public override LocalizedText Text => Language.GetText("LegacyInterface.47");
				public override bool CanDisplay => SpecialChestUI.inputTextTaker is RenameButton;
				public override bool Click() {
					string oldName = CurrentChest.GivenName;
					SpecialChestUI.SubmitText();
					return CurrentChest.GivenName != oldName;
				}
			}
			public class CancelRenameButton : SpecialChestButton {
				public override LocalizedText Text => Language.GetText("LegacyInterface.63");
				public override bool CanDisplay => SpecialChestUI.inputTextTaker is RenameButton;
				public override bool Click() {
					SpecialChestUI.CancelText();
					return false;
				}
			}
		}
	}
}
