using Humanizer;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Tools.Wiring;
using Origins.World.BiomeData;
using PegasusLib.Networking;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Power_Box : ModTile, IAshenPowerConduitTile, IGlowingModTile, IAshenWireTile {
		public override string HighlightTexture => base.Texture + "_Highlight";
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		public TileItem Item { get; protected set; }
		protected int frameHeight;
		public sealed override void Load() {
			Item = new TileItem(this)
			.WithExtraStaticDefaults(item => {
				this.DropTileItem(item);
				item.ResearchUnlockCount = 100;
			})
			.WithExtraDefaults(item => item.mech = true)
			.WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.Wire, 20)
				.AddIngredient<Fortified_Steel_Block1_Item>(10)
				.AddTile<Metal_Presser>()
				.Register();
			}).RegisterItem();
			this.SetupGlowKeys();
			OnLoad();
		}
		public virtual void OnLoad() { }
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public virtual void FancyLightingGlowColor(Tile tile, ref Vector3 color) {/*
			if (tile.TileFrameX >= 18) {
				color.DoFancyGlow(SwitchColor.ToVector3(), tile.TileColor);
				color.DoFancyGlow(tile.TileFrameY >= 18 ? Vector3.Up : Vector3.Right, tile.TileColor);
			}*/
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) GlowTexture = Request<Texture2D>(Texture + "_Glow");
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileBlockLight[Type] = false;
			Main.tileMergeDirt[Type] = false;
			TileID.Sets.DrawTileInSolidLayer[Type] = true;
			AddMapEntry(FromHexRGB(0xBE845A), CreateMapEntryName());

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
			TileObjectData.newTile.SetHeight(2);
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Origin = new(0, 1);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.StyleHorizontal = true;
			ModifyTileObject();
			frameHeight = TileObjectData.newTile.Height * 18;
			TileObjectData.addTile(Type);

			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
			TileID.Sets.HasOutlines[Type] = true;
			AnimationFrameHeight = frameHeight;
		}
		public virtual void ModifyTileObject() => TileObjectData.newTile.RandomStyleRange = 2;
		public override void HitWire(int i, int j) {
			if (Ashen_Wire_Data.HittingAshenWires) UpdatePowerState(i, j, IsPowered(i, j));
		}
		public bool IsPowered(int i, int j) {
			Tile tile = Main.tile[i, j];
			bool inputPower = false;
			Point pos = default;
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			for (int y = 0; !inputPower && y < data.Height; y++) {
				for (int x = 0; !inputPower && x < data.Width; x++) {
					inputPower = Main.tile[left + x, top + y].Get<Ashen_Wire_Data>().AnyPower;
				}
			}
			if (inputPower) {
				inputPower = false;
				using IAshenPowerConduitTile.WalkedConduitOutputs _ = new(left, top, data.Width, data.Height);
				for (int y = 0; !inputPower && y < data.Height; y++) {
					pos.Y = top + y;
					for (int x = 0; !inputPower && x < data.Width; x++) {
						pos.X = left + x;
						inputPower = IAshenPowerConduitTile.FindValidPowerSource(pos, 0)
								|| IAshenPowerConduitTile.FindValidPowerSource(pos, 1)
								|| IAshenPowerConduitTile.FindValidPowerSource(pos, 2);
					}
				}
			}
			return inputPower;
		}
		public void UpdatePowerState(int i, int j, bool powered) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameY >= frameHeight && tile.TileFrameY < frameHeight * 2) return;
			AshenWireTile.DefaultUpdatePowerState(i, j, powered, tile => ref tile.TileFrameY, frameHeight * 2);
			TileObjectData data = TileObjectData.GetTileData(tile);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			for (int y = 0; y < data.Height; y++) {
				for (int x = 0; x < data.Width; x++) {
					Ashen_Wire_Data.SetTilePowered(left + x, top + y, powered);
				}
			}
		}
		public override bool CanExplode(int i, int j) => false;
		/*public override bool CanKillTile(int i, int j, ref bool blockDamaged) {
			Tile tile = Main.tile[i, j];
			return tile.TileFrameX == 0 || tile.TileFrameY == 0;
		}*/
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (frameCounter.CycleUp(8)) frame.CycleUp(4);
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameY < frameHeight * 2) frameYOffset = 0;
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
		public override bool RightClick(int i, int j) {
			new Power_Box_Action(new(i, j)).Perform();
			return true;
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowColor = GlowColor;
			drawData.glowSourceRect = new(drawData.tileFrameX, drawData.tileFrameY, 16, 16);
			drawData.glowTexture = this.GetGlowTexture(drawData.tileCache.TileColor);
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			new Sync_Power_Box_Action(i, j).Perform();
		}
		public record class Power_Box_Action(Point16 Pos) : SyncedAction {
			public override bool ServerOnly => true;
			public Power_Box_Action() : this(Point16.Zero) { }
			public override SyncedAction NetReceive(BinaryReader reader) => this with {
				Pos = new(reader.ReadInt16(), reader.ReadInt16())
			};
			public override void NetSend(BinaryWriter writer) {
				writer.Write((short)Pos.X);
				writer.Write((short)Pos.Y);
			}
			protected override void Perform() {
				(int i, int j) = Pos;
				Tile tile = Main.tile[i, j];
				if (!tile.HasTile) return;
				if (TileLoader.GetTile(tile.TileType) is Power_Box box) {
					bool wasDisabled = tile.TileFrameY >= box.frameHeight && tile.TileFrameY < box.frameHeight * 2;
					TileObjectData data = TileObjectData.GetTileData(tile);
					TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
					for (int x = 0; x < data.Width; x++) {
						for (int y = 0; y < data.Height; y++) {
							ref short useFrame = ref Main.tile[left + x, top + y].TileFrameY;
							useFrame = (short)(useFrame % box.frameHeight + (wasDisabled ? 0 : box.frameHeight));
						}
					}
					if (wasDisabled) {
						box.UpdatePowerState(i, j, box.IsPowered(i, j));
					} else {
						for (int y = 0; y < data.Height; y++) {
							for (int x = 0; x < data.Width; x++) {
								Ashen_Wire_Data.SetTilePowered(left + x, top + y, false);
							}
						}
						if (!NetmodeActive.SinglePlayer) NetMessage.SendTileSquare(-1, left, top, data.Width, data.Height);
					}
				}
			}
		}
		public record class Sync_Power_Box_Action(int I, int J) : SyncedAction {
			public override bool ServerOnly => true;
			public Sync_Power_Box_Action() : this(0, 0) { }
			public override SyncedAction NetReceive(BinaryReader reader) => this with {
				I = reader.ReadInt16(),
				J = reader.ReadInt16(),
			};
			public override void NetSend(BinaryWriter writer) {
				writer.Write((short)I);
				writer.Write((short)J);
			}
			protected override void Perform() {
				if (TileLoader.GetTile(Main.tile[I, J].TileType) is Power_Box @switch) {
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
	}
	public class Power_Box_Wide : Power_Box {
		public override void OnLoad() {
			Item.OnAddRecipes += item => {
				Recipe.Create(item.type)
				.AddIngredient(TileItem.Get<Power_Box>())
				.DisableDecraft()
				.Register();
				Recipe.Create(TileItem.Get<Power_Box>().Type)
				.AddIngredient(item.type)
				.DisableDecraft()
				.Register();
			};
		}
		public override void ModifyTileObject() {
			TileObjectData.newTile.Width = 3;
			TileObjectData.newTile.Origin = new(1, 1);
		}
		public override void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			base.FancyLightingGlowColor(tile, ref color);
		}
	}
}
