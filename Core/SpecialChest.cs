using Humanizer;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Tiles;
using Origins.Tiles.Dev;
using PegasusLib.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using static Origins.Core.SpecialChest;

namespace Origins.Core {
	public class SpecialChest : ILoadable {
		public const int chestID = -8479;
		public static void OpenChest(int x, int y) {
			Tile tile = Main.tile[x, y];
			int style = TileObjectData.GetTileStyle(tile);
			if (style >= 0) TileUtils.GetMultiTileTopLeft(x, y, TileObjectData.GetTileData(tile.TileType, style), out x, out y);
			if (ModContent.GetInstance<SpecialChestSystem>().tileEntities.ContainsKey(new((short)x, (short)y))) {
				Main.LocalPlayer.chest = chestID;
				Main.playerInventory = true;
				Main.editChest = false;
				Main.npcChatText = "";
				Main.ClosePlayerChat();
				Main.chatText = "";
				Main.LocalPlayer.chestX = x;
				Main.LocalPlayer.chestY = y;
				ModContent.GetInstance<SpecialChestSystem>().chestUI.SetState(new SpecialChestUI((short)x, (short)y));
			}
		}
		static bool sendChestSync = false;
		public static void MarkConsumedItem() => sendChestSync = true;
		void ILoadable.Load(Mod mod) {
			IL_ChestUI.Draw += IL_ChestUI_Draw;
			IL_ItemSlot.OverrideHover_ItemArray_int_int += IL_ItemSlot_OverrideHover_ItemArray_int_int;
			IL_ItemSlot.LeftClick_SellOrTrash += IL_ItemSlot_LeftClick_SellOrTrash;
			On_Player.HandleBeingInChestRange += On_Player_HandleBeingInChestRange;
			On_Recipe.CollectItems_ItemArray_int += On_Recipe_CollectItems_ItemArray_int;
			On_Recipe.Create += On_Recipe_Create;
			static void On_Player_HandleBeingInChestRange(On_Player.orig_HandleBeingInChestRange orig, Player self) {
				if (self.chest == chestID && ModContent.GetInstance<SpecialChestSystem>().tileEntities.TryGetValue(new(self.chestX, self.chestY), out ChestData chest)) {
					chest.HandleBeingInChestRange(self);
					return;
				}
				orig(self);
			}
			static void IL_ItemSlot_LeftClick_SellOrTrash(ILContext il) {
				ILCursor c = new(il);
				while (c.TryGotoNext(MoveType.After,
					i => i.MatchLdsfld<Main>(nameof(Main.player)),
					i => i.MatchLdsfld<Main>(nameof(Main.myPlayer)),
					i => i.MatchLdelemRef(),
					i => i.MatchLdfld<Player>(nameof(Player.chest)),
					i => i.MatchLdcI4(-1),
					i => i.MatchCeq()
				)) {
					c.EmitLdsfld(typeof(Main).GetField(nameof(Main.player)));
					c.EmitLdsfld(typeof(Main).GetField(nameof(Main.myPlayer)));
					c.EmitLdelemRef();
					c.EmitLdfld(typeof(Player).GetField(nameof(Player.chest)));
					c.EmitLdcI4(chestID);
					c.EmitCeq();
					c.EmitOr();
				}
			}

			static void IL_ItemSlot_OverrideHover_ItemArray_int_int(ILContext il) {
				ILCursor c = new(il);
				ILLabel label = default;
				while (c.TryGotoNext(MoveType.After,
					i => i.MatchLdsfld<Main>(nameof(Main.player)),
					i => i.MatchLdsfld<Main>(nameof(Main.myPlayer)),
					i => i.MatchLdelemRef(),
					i => i.MatchLdfld<Player>(nameof(Player.chest)),
					i => i.MatchLdcI4(-1),
					i => i.MatchBeq(out label)
				)) {
					c.EmitLdsfld(typeof(Main).GetField(nameof(Main.player)));
					c.EmitLdsfld(typeof(Main).GetField(nameof(Main.myPlayer)));
					c.EmitLdelemRef();
					c.EmitLdfld(typeof(Player).GetField(nameof(Player.chest)));
					c.EmitLdcI4(chestID);
					c.EmitBeq(label);
				}
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
					if (player.chest == chestID && SpecialChestSystem.TryGetChest(player.chestX, player.chestY) is ChestData chest) {
						new Set_Special_Chest_Action(new(player.chestX, player.chestY), chest).Perform();
					}
				}
			}
		}

		class SpecialChestCraftingPlayer : ModPlayer {
			public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback) {
				if (Player.chest == chestID && SpecialChestSystem.TryGetChest(Player.chestX, Player.chestY) is ChestData chest) {
					itemConsumedCallback = chest.CraftWithItem;
					return chest.Items(true);
				}
				return base.AddMaterialsForCrafting(out itemConsumedCallback);
			}
		}
		public static string MapChestName(string name, int i, int j) {
			TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out int left, out int top);
			string renamed = SpecialChestSystem.TryGetChest(left, top)?.MapName ?? "";
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
			protected abstract bool IsValidSpot(Point position);
			public virtual void UpdateUI(SpecialChestUI.SpecialChestElement ui) { }
			public virtual int ItemCount => Items().Length;
			public virtual string MapName => "";
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
			public abstract record class Storage_Container_Data(Item[] Inventory) : ChestData() {
				public abstract int Capacity { get; }
				public abstract int Width { get; }
				public abstract int Height { get; }
				public Storage_Container_Data() : this([]) {
					Item[] inventory = Inventory;
					Array.Resize(ref inventory, Capacity);
					for (int i = 0; i < inventory.Length; i++) inventory[i] ??= new();
					Inventory = inventory;
				}
				public override Item[] Items(bool forCrafting = false) => Inventory;
				protected override ChestData LoadData(TagCompound tag) => this with {
					Inventory = tag.SafeGet("Items", new List<Item>() { new(), new(), new() }).ToArray()
				};
				protected override void SaveData(TagCompound tag) => tag["Items"] = Inventory.ToList();
				internal override ChestData NetReceive(BinaryReader reader) => this with {
					Inventory = reader.ReadCompressedItemArray()
				};
				internal override void NetSend(BinaryWriter writer) {
					writer.WriteCompressedItemArray(Inventory);
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
			}
			public sealed record class Invalid : ChestData {
				protected override bool IsValidSpot(Point position) => false;
				public override Item[] Items(bool forCrafting = false) => [];
				protected override ChestData LoadData(TagCompound tag) => this;
				protected override void SaveData(TagCompound tag) { }
				internal override ChestData NetReceive(BinaryReader reader) => this;
				internal override void NetSend(BinaryWriter writer) { }
			}
		}
		public static void SyncToPlayer(int player) => ModContent.GetInstance<SpecialChestSystem>().SyncToPlayer(player);
		class SpecialChestSystem : ModSystem {
			public static ChestData TryGetChest(int i, int j) => ModContent.GetInstance<SpecialChestSystem>().tileEntities.TryGetValue(new(i, j), out ChestData chest) ? chest : null;
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
				ModContent.GetInstance<SpecialChestSystem>().tileEntities[Position] = Data;
			}
		}
		public class SpecialChestUI(short x, short y) : UIState {
			public override void OnInitialize() {
				if (ModContent.GetInstance<SpecialChestSystem>().tileEntities.TryGetValue(new(x, y), out ChestData contents)) {
					Append(new SpecialChestElement(contents));
				}
			}
			public class SpecialChestElement(ChestData data) : UIElement {
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
				public override void Update(GameTime gameTime) {
					base.Update(gameTime);
					data.UpdateUI(this);
					scrollbar.Left.Pixels = data.ItemCount > 40 ? 0 : -Main.screenWidth;
					scrollbar.SetView(56 * 4, MathF.Ceiling(data.ItemCount / 10f) * 56);
				}
				protected override void DrawSelf(SpriteBatch spriteBatch) {
					float inventoryScale = Main.inventoryScale;
					Main.inventoryScale = 0.755f;
					Player player = Main.LocalPlayer;
					int viewPos = scrollbar is null ? 0 : (int)(scrollbar.ViewPosition / 56);
					Item[] items = data.Items();
					bool didAnything = false;
					for (int i = 0; i < 10; i++) {
						for (int j = 0; j < 4; j++) {
							int slot = i + (j + viewPos) * 10;
							int xPos = (int)(73f + (i * 56) * Main.inventoryScale);
							int yPos = (int)(Main.instance.invBottom + (j * 56) * Main.inventoryScale);
							Item item = slot >= items.Length ? new() : items[slot].Clone();
							if (!PlayerInput.IgnoreMouseInterface && Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, xPos, yPos, TextureAssets.InventoryBack.Width() * Main.inventoryScale, TextureAssets.InventoryBack.Height() * Main.inventoryScale)) {
								player.mouseInterface = true;
								didAnything |= data.InteractWithItem(item, slot);
							}
							data.DrawItemSlot(spriteBatch, new(xPos, yPos), item, slot);
						}
					}
					if (didAnything) new Set_Special_Chest_Action(new(Main.LocalPlayer.chestX, Main.LocalPlayer.chestY), data).Perform();
					Main.inventoryScale = inventoryScale;
				}
				public override void ScrollWheel(UIScrollWheelEvent evt) {
					if (scrollbar is null) return;
					int viewPos = (int)(scrollbar.ViewPosition / 56);
					scrollbar.ViewPosition = (viewPos - evt.ScrollWheelValue / 120) * 56;
				}
			}
		}
	}
}
