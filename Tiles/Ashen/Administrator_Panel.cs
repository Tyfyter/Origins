using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Items.Tools.Wiring;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using PegasusLib.Networking;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Administrator_Panel : OriginTile, IGlowingModTile {
		public override void Load() {
			new TileItem(this)
			.WithExtraStaticDefaults(this.DropTileItem)
			.WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddRecipeGroup(ALRecipeGroups.CopperBars)
				.AddIngredient(ModContent.ItemType<Scrap>(), 6)
				.AddIngredient(ItemID.Glass, 3)
				.AddIngredient(ItemID.Wire, 8)
				.AddTile(ModContent.TileType<Metal_Presser>())
				.Register();
			}).RegisterItem();
			this.SetupGlowKeys();
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			HitSound = SoundID.Tink;

			// Names
			AddMapEntry(FromHexRGB(0x280834));

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.SetHeight(4, 16);
			TileObjectData.newTile.Origin = new Point16(TileObjectData.newTile.Width / 2 - 1, TileObjectData.newTile.Height - 1);
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.addTile(Type);
			DustType = Ashen_Biome.DefaultTileDust;
			AnimationFrameHeight = 72;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => Main.tile[i, j].TileFrameY >= 18 * 4 && OriginsModIntegrations.CheckAprilFools();
		public override bool RightClick(int i, int j) {
			if (Main.tile[i, j].TileFrameY >= 18 * 4 && OriginsModIntegrations.CheckAprilFools()) {
				new Nuke_Launch_Program(Main.LocalPlayer).Perform();
				return true;
			}
			return false;
		}
		public static bool IsPowered(int i, int j) {
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			for (int x = 0; x < data.Width; x++) {
				for (int y = 0; y < data.Height; y++) {
					if (Main.tile[left + x, top + y].Get<Ashen_Wire_Data>().AnyPower) return true;
				}
			}
			return false;
		}
		public override void HitWire(int i, int j) {
			bool powered = IsPowered(i, j);
			bool wasPowered = Main.tile[i, j].TileFrameY >= 18 * 4;
			if (powered != wasPowered) {
				UpdatePowerState(i, j, powered);
			}
		}
		public static void UpdatePowerState(int i, int j, bool powered) {
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			for (int x = 0; x < data.Width; x++) {
				for (int y = 0; y < data.Height; y++) {
					Tile tile = Main.tile[left + x, top + y];
					tile.TileFrameY = (short)(tile.TileFrameY % (18 * 4) + (powered ? 4 * 18 : 0));
				}
			}
			if (!NetmodeActive.SinglePlayer) NetMessage.SendTileSquare(-1, left, top, data.Width, data.Height);
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameY < 18 * 4) frameYOffset = 0;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 5) {
				frameCounter = 0;
				frame = ++frame % 2;
			}
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;

		public record class Nuke_Launch_Program(Player Player) : SyncedAction {
			private static int Countdown = 0;
			private static int CountdownStage = 0;
			public Nuke_Launch_Program() : this(default(Player)) { }
			public override SyncedAction NetReceive(BinaryReader reader) => this with {
				Player = Main.player[reader.ReadByte()]
			};
			public override void NetSend(BinaryWriter writer) {
				writer.Write((byte)Player.whoAmI);
			}
			public override bool ServerOnly => true;
			protected override bool ShouldPerform => Countdown == 0;
			static void DisplayMessage(string key, params object[] substitutions) {
				WorldGen.BroadcastText(NetworkText.FromKey("Mods.Origins.Status_Messages.Nuke_Launch_Program." + key, substitutions), FromHexRGB(0xFFB18C));
			}
			public static void Update() {
				if (Countdown == 0) return;

				Debugging.ChatOverhead($"CD: {Countdown}, CDS: {CountdownStage}");
				switch (CountdownStage) {
					case 0:
					if (--Countdown == 0) {
						++CountdownStage;
						Countdown = (19 - 1) * 5 + 10;
						DisplayMessage("Destination");
					}
					break;
					case 1:
					if (--Countdown == 0) {
						++CountdownStage;
						Countdown = (Main.worldName.Length - 1) * 5 + 10;
						DisplayMessage("DestName", Main.worldName);
					}
					break;
					case 2:
					if (--Countdown == 0) {
						++CountdownStage;
						Countdown = (22 - 1) * 5 + 10;
						DisplayMessage("InitCD");
					}
					break;
					case 3:
					if (--Countdown == 0) {
						++CountdownStage;
						Countdown = 300;
						DisplayMessage("5");
					}
					break;
					case 4:
					switch (--Countdown) {
						case 240:
						case 180:
						case 120:
						case 60:
						DisplayMessage((Countdown / 60).ToString());
						break;
					}
					break;
				}
			}
			protected override void Perform() {
				Countdown = (Player.name.Length + 39 - 1) * 5 + 10;
				CountdownStage = 0;
				DisplayMessage("Start", Player.name);
			}
		}
	}
}
