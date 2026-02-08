using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Other.Consumables;
using Origins.Items.Tools.Wiring;
using Origins.World.BiomeData;
using PegasusLib.Networking;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public abstract class Mechanical_Key_Node : ModTile, IAshenPowerConduitTile, IGlowingModTile, IAshenWireTile {
		public Mechanical_Key_Node_Item Item { get; private set; }
		public abstract int KeyType { get; }
		public override string HighlightTexture => typeof(Mechanical_Key_Node).GetDefaultTMLName("_Highlight");
		public virtual Color SwitchColor => FromHexRGB(0x7a391a);
		public virtual Color MapColor => SwitchColor;
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		public sealed override void Load() {
			Mod.AddContent(Item = new(this));
			this.SetupGlowKeys();
		}
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (tile.TileFrameX >= 18) {
				color.DoFancyGlow(SwitchColor.ToVector3(), tile.TileColor);
				color.DoFancyGlow(tile.TileFrameY >= 18 ? Vector3.Up : Vector3.Right, tile.TileColor);
			}
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			}
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileBlockLight[Type] = false;
			Main.tileMergeDirt[Type] = false;
			TileID.Sets.DrawTileInSolidLayer[Type] = true;
			AddMapEntry(MapColor, CreateMapEntryName());

			MinPick = 65;
			MineResist = 2;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
			TileID.Sets.HasOutlines[Type] = true;
			RegisterItemDrop(Item.Type);
		}
		public override void HitWire(int i, int j) {
			if (Ashen_Wire_Data.HittingAshenWires) UpdatePowerState(i, j, IsPowered(i, j));
		}
		public bool IsPowered(int i, int j) {
			Tile tile = Main.tile[i, j];
			Point pos = new(i, j);
			bool inputPower = false;
			if (tile.Get<Ashen_Wire_Data>().AnyPower) {
				using IAshenPowerConduitTile.WalkedConduitOutput _ = new(pos);
				inputPower = IAshenPowerConduitTile.FindValidPowerSource(pos, 0)
						|| IAshenPowerConduitTile.FindValidPowerSource(pos, 1)
						|| IAshenPowerConduitTile.FindValidPowerSource(pos, 2);
			}
			return inputPower;
		}
		public void UpdatePowerState(int i, int j, bool powered) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX.TrySet(powered.Mul<short>(18))) {
				if (tile.TileFrameY != 0) Ashen_Wire_Data.SetTilePowered(i, j, powered);
				NetMessage.SendData(MessageID.TileSquare, Main.myPlayer, -1, null, i, j, 1, 1);
			}
		}
		public override bool CanExplode(int i, int j) => false;
		/*public override bool CanKillTile(int i, int j, ref bool blockDamaged) {
			Tile tile = Main.tile[i, j];
			return tile.TileFrameX == 0 || tile.TileFrameY == 0;
		}*/
		public bool HasKey(Player player) => player.HasItemInInventoryOrOpenVoidBag(KeyType);
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => HasKey(settings.player);
		public override bool RightClick(int i, int j) {
			if (!HasKey(Main.LocalPlayer)) return false;
			new Mechanical_Switch_Action(new(i, j)).Perform();
			return true;
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowColor = GlowColor;
			drawData.glowSourceRect = new(drawData.tileFrameX, drawData.tileFrameY, 16, 16);
			drawData.glowTexture = this.GetGlowTexture(drawData.tileCache.TileColor);
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			new Sync_Mechanical_Switch_Action(i, j).Perform();
		}
		public record class Mechanical_Switch_Action(Point16 Pos) : SyncedAction {
			public override bool ServerOnly => true;
			public Mechanical_Switch_Action() : this(Point16.Zero) { }
			public override SyncedAction NetReceive(BinaryReader reader) => this with {
				Pos = new(reader.ReadInt16(), reader.ReadInt16())
			};
			public override void NetSend(BinaryWriter writer) {
				writer.Write((short)Pos.X);
				writer.Write((short)Pos.Y);
			}
			protected override void Perform() {
				if (TileLoader.GetTile(Main.tile[Pos].TileType) is Mechanical_Key_Node) {
					Tile tile = Main.tile[Pos];
					tile.TileFrameY ^= 18;
					bool inputPower = tile.TileFrameX != 0 && tile.TileFrameY != 0;
					using (IAshenPowerConduitTile.WalkedConduitOutput _ = new(Pos.ToPoint())) {
						if (inputPower) inputPower = IAshenPowerConduitTile.FindValidPowerSource(Pos.ToPoint(), 0)
								|| IAshenPowerConduitTile.FindValidPowerSource(Pos.ToPoint(), 1)
								|| IAshenPowerConduitTile.FindValidPowerSource(Pos.ToPoint(), 2);
					}
					Ashen_Wire_Data.SetTilePowered(Pos.X, Pos.Y, inputPower);
					NetMessage.SendData(MessageID.TileSquare, Main.myPlayer, -1, null, Pos.X, Pos.Y, 1, 1);
				}
			}
		}
		public record class Sync_Mechanical_Switch_Action(int I, int J) : SyncedAction {
			public override bool ServerOnly => true;
			public Sync_Mechanical_Switch_Action() : this(0, 0) { }
			public override SyncedAction NetReceive(BinaryReader reader) => this with {
				I = reader.ReadInt16(),
				J = reader.ReadInt16(),
			};
			public override void NetSend(BinaryWriter writer) {
				writer.Write((short)I);
				writer.Write((short)J);
			}
			protected override void Perform() {
				if (TileLoader.GetTile(Main.tile[I, J].TileType) is Mechanical_Key_Node @switch) {
					@switch.HitWire(I, J);
				}
			}
		}
		public bool ShouldCountAsPowerSource(Point position, int forWireType) {
			using IAshenPowerConduitTile.WalkedConduitOutput _ = new(position);
			bool powered = false;
			for (int i = 0; !powered && i < 3; i++) {
				powered = i != forWireType && IAshenPowerConduitTile.FindValidPowerSource(position, i);
			}
			return powered;
		}

		public class Mechanical_Key_Node_Item(Mechanical_Key_Node tile) : ModItem {
			static AutoLoadingAsset<Texture2D> overlay = typeof(Mechanical_Key_Node_Item).GetDefaultTMLName("_Color");
			public override string Name => tile.Name + "_Item";
			public override string Texture => GetType().GetDefaultTMLName();
			protected override bool CloneNewInstances => true;
			public override void SetStaticDefaults() {
				Item.ResearchUnlockCount = 100;
			}
			public override void SetDefaults() {
				Item.DefaultToPlaceableTile(tile.Type);
				Item.mech = true;
			}
			public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
				spriteBatch.Draw(overlay, position, frame, tile.SwitchColor.MultiplyRGBA(drawColor), 0, origin, scale, SpriteEffects.None, 0);
			}
			public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
				Vector2 origin = overlay.Value.Size() * 0.5f;
				Rectangle originalFrame;
				{
					Texture2D texture = TextureAssets.Item[Type].Value;
					if (Main.itemAnimations[Type] != null) {
						originalFrame = Main.itemAnimations[Type].GetFrame(texture, Main.itemFrameCounter[whoAmI]);
					} else {
						originalFrame = texture.Frame();
					}
				}
				alphaColor = alphaColor.MultiplyRGBA(tile.SwitchColor);
				Vector2 vector2 = new((Item.width / 2) - originalFrame.Width * 0.5f, Item.height - originalFrame.Height);
				Vector2 position = Item.position - Main.screenPosition + originalFrame.Size() * 0.5f + vector2;
				spriteBatch.Draw(overlay, position, null, alphaColor, rotation, origin, scale, SpriteEffects.None, 0f);
				if (Item.shimmered) {
					spriteBatch.Draw(overlay, position, null, alphaColor with { A = 0 }, rotation, origin, scale, SpriteEffects.None, 0f);
				}
			}
		}
	}
	[LegacyName("Blue_Mechanical_Key_Node")]
	public class Mechanical_Key_Node_Blue : Mechanical_Key_Node {
		public override Color SwitchColor => new Color(0, 80, 240);
		public override int KeyType => ItemType<Mechanical_Key_Blue>();
	}
	[LegacyName("Green_Mechanical_Key_Node")]
	public class Mechanical_Key_Node_Green : Mechanical_Key_Node {
		public override Color SwitchColor => new Color(16, 240, 0);
		public override int KeyType => ItemType<Mechanical_Key_Green>();
	}
	[LegacyName("Orange_Mechanical_Key_Node")]
	public class Mechanical_Key_Node_Orange : Mechanical_Key_Node {
		public override Color SwitchColor => new Color(255, 81, 0);
		public override int KeyType => ItemType<Mechanical_Key_Orange>();
	}
	[LegacyName("Purple_Mechanical_Switch", "Purple_Mechanical_Key_Node")]
	public class Mechanical_Key_Node_Purple : Mechanical_Key_Node {
		public override Color SwitchColor => new Color(109, 10, 145);
		public override int KeyType => ItemType<Mechanical_Key_Purple>();
	}
	[LegacyName("Yellow_Mechanical_Key_Node")]
	public class Mechanical_Key_Node_Yellow : Mechanical_Key_Node {
		public override Color SwitchColor => new Color(255, 179, 0);
		public override int KeyType => ItemType<Mechanical_Key_Yellow>();
	}
}
