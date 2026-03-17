using CalamityMod.Items.Accessories;
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Items.Tools.Liquids;
using Origins.World.BiomeData;
using ReLogic.Utilities;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.UI;
using static Origins.Core.SpecialChest;

namespace Origins.Tiles.Ashen {
	public class Oil_Derrick : ModSpecialChest, IComplexMineDamageTile, IMultiTypeMultiTile {
		readonly Sound ambientSound = EnvironmentSounds.Register<Sound>();
		public override void Load() => new TileItem(this).WithExtraStaticDefaults(this.DropTileItem).RegisterItem();
		public override bool IsMultitile => false;
		public override void ModifyTileData() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Width = 8;
			TileObjectData.newTile.SetHeight(7);
			TileObjectData.newTile.SetOriginBottomCenter();
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.newTile.HookPlaceOverride = MultiTypeMultiTile.PlaceWhereTrue(IsPart);
			TileObjectData.newTile.AnchorBottom = new(AnchorType.SolidTile, TileObjectData.newTile.Width - 1, 1);
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.addAlternate(1);
			this.SetAnimationHeight();
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(40, 30, 18), this.GetTileItem().DisplayName);
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
		public override bool CanExplode(int i, int j) => false;
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (frameCounter.CycleUp(8)) frame.CycleUp(7);
		}
		public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects) {
			position.Y += 2;
			if (frame.X >= 144) {
				frame.X = 18 * (8 * 2 - 1) - frame.X;
				spriteEffects ^= SpriteEffects.FlipHorizontally;
			}
			if (!validPlacement && frame.X == 18 * 3 && frame.Y == 18 * 1 && Main.tile[i, j].HasTile) {
				Point offset = (Main.screenPosition - new Vector2(Main.offScreenRange)).ToPoint();
				spriteBatch.Draw(
					TextureAssets.Cd.Value,
					new Rectangle(i * 16 - offset.X, j * 16 - offset.Y, 16, 16),
					null,
					color
				);
			}
			frame.Y = (frame.Y + 7 * 18 * 4) % (7 * 18 * 8);
			return base.PreDrawPlacementPreview(i, j, spriteBatch, ref frame, ref position, ref color, validPlacement, ref spriteEffects);
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = 2;
			if (tileFrameX >= 144) tileFrameX = (short)(18 * (8 * 2 - 1) - tileFrameX);
		}
		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			base.SetSpriteEffects(i, j, ref spriteEffects);
			if (Main.tile[i, j].TileFrameX >= 144) spriteEffects ^= SpriteEffects.FlipHorizontally;
		}
		public override void NearbyEffects(int i, int j, bool closer) {
			if (closer) return;
			ambientSound.TrySetNearest(new(i * 16 + 8, j * 16 + 8));
		}
		class Sound : AEnvironmentSound {
			SlotId droning;
			public override void UpdateSound(Vector2 position) {
				int type = ModContent.TileType<Oil_Derrick>();
				float mult = 1 / float.Max(position.DistanceSQ(Main.Camera.Center) / (16 * 20 * 16 * 20), 1);
				droning.PlaySoundIfInactive(Origins.Sounds.RadioBroadcaster.WithPitch(0.5f), position, playingSound => {
					if (GetPosition() is not Vector2 pos) return false;
					playingSound.Volume = 0.2f / float.Max(pos.DistanceSQ(Main.Camera.Center) / (16 * 20 * 16 * 20), 1);
					return true;
				});
				if (Main.tileFrame[type] == 1) SoundEngine.PlaySound(SoundID.Item108.WithPitch(-1.4f).WithVolume(0.32f * mult), position);
				if (Main.rand.NextBool(150)) SoundEngine.PlaySound(SoundID.Item148.WithPitch(-0.5f).WithVolume(0.48f * mult), position);
			}
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Main.tile[i, j];
			if (!IsPart(tile.TileFrameX / 18, tile.TileFrameY / 18, TileObjectData.GetTileStyle(tile))) {
				tile.HasTile = false;
				return false;
			}
			return true;
		}
		public static bool IsPart(int i, int j, int style) {
			if (style > 0) i = 7 - i;
			switch (i, j) {
				case (0, 0):
				case (0, 1):
				case (0, 2):
				case (0, 3):
				case (0, 6):

				case (1, 0):
				case (1, 1):
				case (1, 2):

				case (2, 0):
				case (2, 1):
				case (2, 2):

				case (3, 0):
				case (3, 1):

				case (4, 0):
				case (5, 0):
				case (7, 0):
				return false;
			}
			return true;
		}

		public bool IsValidTile(Tile tile, int left, int top, int style) {
			(int i, int j) = tile.GetTilePosition();
			i -= left;
			j -= top;
			if (IsPart(i, j, style)) {
				if (tile.TileType != Type) return false;
				Tile topLeft = Main.tile[left, top];
				return tile.TileFrameX == topLeft.TileFrameX + i * 18 && tile.TileFrameY == topLeft.TileFrameY + j * 18;
			}
			return true;
		}
		public bool CanBlockPlacement(Tile tile, int left, int top, int style) {
			if (!tile.HasTile) return false;
			Point pos = tile.GetTilePosition();
			return IsPart(pos.X - left, pos.Y - top, style);
		}
		public bool ShouldBreak(int x, int y, int left, int top, int style) => IsPart(x - left, y - top, style);
		public override void ModifyTELocation(ref int x, ref int y, int originalX, int originalY) {
			x += 3;
			y += 6;
		}
		public override SpecialChest.ChestData CreateChestData() => new Oil_Derrick_Container_Data();
		public record class Oil_Derrick_Container_Data() : ChestData() {
			public static int TimePerBucket => 60 * 10;
			public Item[] Inventory { get; init; } = OriginExtensions.BuildFullArray<Item>(2);
			public int oilTime;
			public override bool CanReceiveNearbyQuickStack(int x, int y) {
				return false;
			}
			public override Item[] Items(bool forCrafting = false) => Inventory;
			public override void UpdateChest(Point16 position) {
				Item output = Inventory[1];
				if (output.stack >= output.maxStack) return;
				if (++oilTime >= TimePerBucket) {
					Item input = Inventory[0];
					if (input.IsAir) return;
					oilTime = 0;
					if (--input.stack <= 0) input.TurnToAir();
					if (output.IsAir) output.SetDefaults(ModContent.ItemType<Oil_Bucket>());
					else output.stack++;
				}
			}
			protected override ChestData LoadData(TagCompound tag) => this with {
				Inventory = tag.SafeGet("Items", Enumerable.Repeat(0, 2).Select(_ => new Item()).ToList()).ToArray()
			};
			protected override void SaveData(TagCompound tag) {
				tag["Items"] = Inventory.ToList();
			}
			internal override ChestData NetReceive(BinaryReader reader) => this with {
				Inventory = reader.ReadCompressedItemArray()
			};
			internal override void NetSend(BinaryWriter writer) {
				writer.WriteCompressedItemArray(Inventory);
			}
			public override void ModifySlotPosition(Item item, int slot, ref int xPos, ref int yPos) => xPos += (int)(slot * 56 * Main.inventoryScale);
			public override void DrawItemSlot(SpriteBatch spriteBatch, Vector2 position, Item item, int slot) {
				if (slot > 1) return;
				Item air = new();
				Item icon = item;
				Color itemColor = default;
				if (slot == 0) {
					ItemSlot.Draw(spriteBatch, ref air, ItemSlot.Context.ChestItem, position);
					if (item.IsAir) {
						icon = new(ItemID.EmptyBucket);
						itemColor = new(130, 62, 102, 100);
					}
				} else {
					ItemSlot.Draw(spriteBatch, ref air, ItemSlot.Context.ShopItem, position);
					if (item.IsAir) {
						icon = new(ModContent.ItemType<Oil_Bucket>());
						itemColor = new(110, 128, 54, 100);
					}
					air.SetDefaults(ItemID.ArrowSign);
					float brightness = 1 - oilTime / (float)TimePerBucket;
					ItemSlot.Draw(spriteBatch, ref air, ItemSlot.Context.ChatItem, position - Vector2.UnitX * 56 * Main.inventoryScale, new(brightness, brightness, brightness, 1));
				}
				ItemSlot.Draw(spriteBatch, ref icon, ItemSlot.Context.ChatItem, position, itemColor);
			}
			public override bool InteractWithItem(Item item, int slot) {
				switch (slot) {
					case 0:
					if (!Main.mouseItem.IsAir && Main.mouseItem.type != ItemID.EmptyBucket) return false;
					return base.InteractWithItem(item, slot);
					case 1:
					if (!Main.mouseItem.IsAir && Main.mouseItem.type != ModContent.ItemType<Oil_Bucket>()) return false;
					return base.InteractWithItem(item, slot);
				}
				return false;
			}
			protected internal override bool IsValidSpot(Point position) => Main.tile[position].TileIsType(ModContent.TileType<Oil_Derrick>());
		}
	}
}
