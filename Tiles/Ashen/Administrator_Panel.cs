using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
	public class Administrator_Panel : OriginTile, IGlowingModTile, IAshenWireTile {
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
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (tile.TileFrameX < 18 * 2) return;
			switch (tile.TileFrameY) {
				case 18:
				color.DoFancyGlow(new Vector3(0.54901963f, 0.10980393f, 0.7137255f), tile.TileColor);
				break;
				case 18 * 2:
				color.DoFancyGlow(new Vector3(0.2352941f, 0.04705883f, 0.3058824f), tile.TileColor);
				break;
			}
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
			TileObjectData.newTile.SetHeight(4, 18);
			TileObjectData.newTile.Origin = new Point16(TileObjectData.newTile.Width / 2 - 1, TileObjectData.newTile.Height - 1);
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.addTile(Type);
			DustType = Ashen_Biome.DefaultTileDust;
			AnimationFrameHeight = 36;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => Main.tile[i, j].TileFrameX >= 18 * 2 && OriginsModIntegrations.CheckAprilFools();
		public override bool RightClick(int i, int j) {
			if (Main.tile[i, j].TileFrameX >= 18 * 2 && OriginsModIntegrations.CheckAprilFools()) {
				new Nuke_Launch_Program(Main.LocalPlayer).Perform();
				return true;
			}
			return false;
		}
		public override void HitWire(int i, int j) {
			UpdatePowerState(i, j, AshenWireTile.DefaultIsPowered(i, j));
		}
		public void UpdatePowerState(int i, int j, bool powered) {
			bool wasPowered = Main.tile[i, j].TileFrameX >= 18 * 2;
			if (powered == wasPowered) return;
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			for (int x = 0; x < data.Width; x++) {
				for (int y = 0; y < data.Height; y++) {
					Tile tile = Main.tile[left + x, top + y];
					tile.TileFrameX = (short)(tile.TileFrameX % (18 * 2) + (powered ? 2 * 18 : 0));
				}
			}
			if (!NetmodeActive.SinglePlayer) NetMessage.SendTileSquare(-1, left, top, data.Width, data.Height);
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX < 18 * 2) frameYOffset = 0;
			frameXOffset = frameYOffset;
			frameYOffset = 0;
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
			const string base_key = "Mods.Origins.Status_Messages.Nuke_Launch_Program.";
			public Nuke_Launch_Program() : this(default(Player)) { }
			public override SyncedAction NetReceive(BinaryReader reader) => this with {
				Player = Main.player[reader.ReadByte()]
			};
			public override void NetSend(BinaryWriter writer) {
				writer.Write((byte)Player.whoAmI);
			}
			protected override bool ShouldPerform => Countdown == 0;
			/// <summary>
			/// <paramref name="delayAfter"/> is ignored if <paramref name="typeOut"/> is false
			/// </summary>
			static void DisplayMessage(string message, bool typeOut = true, int delayAfter = 15) {
				if (typeOut) {
					const int delay = 2;
					const int speed = 1;
					Countdown = (message.Length - 1) * speed + delay + delayAfter;
					message = GetText("TypingSnippet", delay, speed, message);
				}
				Main.NewText(GetText("Cursor") + message, FromHexRGB(0xFFB18C));
			}
			static string GetText(string key, params object[] substitutions) => Language.GetTextValue(base_key + key, substitutions);
			public static void Update() {
				if (Countdown == 0) return;

				switch (CountdownStage) {
					case 0:
					if (--Countdown == 0) {
						++CountdownStage;
						DisplayMessage(GetText("Destination"));
					}
					break;
					case 1:
					if (--Countdown == 0) {
						++CountdownStage;
						DisplayMessage(Main.worldName);
					}
					break;
					case 2:
					if (--Countdown == 0) {
						++CountdownStage;
						DisplayMessage(GetText("InitCD"));
					}
					break;
					case 3:
					if (--Countdown == 0) {
						++CountdownStage;
						Countdown = 300;
						DisplayMessage("5", false);
					}
					break;
					case 4:
					switch (--Countdown) {
						case 240:
						case 180:
						case 120:
						case 60:
						DisplayMessage((Countdown / 60).ToString(), false);
						break;
					}
					break;
				}
			}
			protected override void Perform() {
				CountdownStage = 0;
				DisplayMessage(GetText("Start", Player.name));
			}
		}
	}
}
