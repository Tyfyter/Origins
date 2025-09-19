using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.World.BiomeData;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace Origins.Tiles.Defiled {
	public class Defiled_Relay : ModTile, IGlowingModTile {
		public static string message;
		public static int messageIndex = 0;
		public static int messageTimer = 0;
		public static int ID { get; private set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White * GlowValue;
		public float GlowValue => GetGlowValue(Main.tileFrame[Type]);
		public static float GetGlowValue(int frame) => frame switch {
			6 or 7 => 0.3f,
			8 => 0.5f,
			9 => 0.38f,
			_ => 0.24f
		};
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = Vector3.Max(color, Vector3.One * GlowValue);
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			}
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileHammer[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
			TileID.Sets.GeneralPlacementTiles[Type] = false;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Height = 4;
			TileObjectData.newTile.Origin = new(0, 3);
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16];
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(40, 40, 40), CreateMapEntryName());
			//disableSmartCursor = true;
			AdjTiles = [TileID.ShadowOrbs];
			ID = Type;
			AnimationFrameHeight = 720 / 10;
			HitSound = Origins.Sounds.DefiledIdle;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			const int dit = 5;
			const int dah_mult = 3;
			if (message is not null) {
				if (messageIndex >= message.Length) {
					message = null;
					return;
				}
				bool state = true;
				int duration = 1;
				switch (message[messageIndex]) {
					case '-':
					duration = dah_mult;
					break;
					case ' ':
					state = false;
					break;
				}
				messageTimer++;
				if (state) {
					if (messageTimer < 4) {
						frame = 6 + messageTimer / 2;
					} else if (messageTimer / dit > duration) {
						frame = 9;
					} else {
						frame = 8;
					}
				} else {
					if (++frameCounter >= 9) {
						frameCounter = 0;
						frame = (frame + 1) % 6;
					}
				}
				if (messageTimer / dit > duration) {
					messageTimer = 0;
					messageIndex++;
				}
				return;
			}
			if (++messageTimer > Main.rand.Next(20 * 60, 60 * 60 * 3)) {// a minute to 10 minutes
				DisplayMessage("Idle");
			}
			if (++frameCounter >= 9) {
				frameCounter = 0;
				frame = (frame + 1) % 6;
			}
		}
		public static string GetDialogueKey(string key) {
			WeightedRandom<string> random = new();
			if (Language.Exists(key)) {
				random.Add(key);
			} else {
				int i = 1;
				while (Language.Exists(key + i)) {
					random.Add(key + i);
					i++;
				}
			}
			return random;
		}
		public static void DisplayMessage(string key, bool fromNet = false) {
			if (!fromNet && !NetmodeActive.SinglePlayer && Origins.instance.NetID >= 0 && key != "Idle") {
				ModPacket packet = Origins.instance.GetPacket();
				packet.Write(Origins.NetMessageType.defiled_relay_message);
				packet.Write(key);
				packet.Send();
			}
			key = GetDialogueKey("Mods.Origins.DefiledRelaySignals." + key);
			DisplayText(Language.GetTextValue(key));
		}
		public static void DisplayText(string text) {
			text = text.ToUpperInvariant();
			switch (text) {
				case "SOS":
				message = "...---...";
				messageIndex = 0;
				messageTimer = 0;
				return;
			}
			text = Regex.Replace(text, "(?:<(\\w+)>)|", " ");
			StringBuilder builder = new();
			for (int i = 0; i < text.Length; i++) {
				string charKey = "MorseLookup." + text[i].ToString();
				if (Language.Exists(charKey)) {
					builder.Append(Language.GetOrRegister(charKey).Value);
				} else {
					return;
				}
			}
			message = builder.ToString();
			messageIndex = 0;
			messageTimer = 0;
			Main.tileFrameCounter[ModContent.TileType<Defiled_Relay>()] = 0;
		}
		public override bool CanKillTile(int i, int j, ref bool blockDamaged) => Main.LocalPlayer.HeldItem.hammer >= 45;
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (fail && !effectOnly) {
				if (CanKillTile(i, j, ref fail)) {
					DisplayMessage("BeingDestroyed");
				} else {
					DisplayMessage("Security");
				}
				PegasusLib.TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Framing.GetTileSafely(i, j)), out i, out j);
				Projectile.NewProjectile(WorldGen.GetItemSource_FromTileBreak(i, j), new Vector2((i + 1) * 16, (j + 2) * 16), Vector2.Zero, ModContent.ProjectileType<Projectiles.Misc.Defiled_Wastelands_Signal>(), 0, 0, ai0: 0, ai1: Main.myPlayer);
			}
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Defiled_Wastelands.CheckFissure(i, j, Type);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = g = b = GlowValue;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Defiled_Relay_Item : ModItem, ICustomWikiStat, IItemObtainabilityProvider {
		public IEnumerable<int> ProvideItemObtainability() => new int[] { Type };
		public override string Texture => "Origins/Tiles/Defiled/Defiled_Relay";
		public override void SetStaticDefaults() {
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
		}

		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<Defiled_Relay>();
		}
		public bool ShouldHavePage => false;
	}
}
