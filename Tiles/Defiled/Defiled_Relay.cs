using Microsoft.Extensions.Primitives;
using Microsoft.Xna.Framework;
using Origins.Dev;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
	public class Defiled_Relay : ModTile {
		public static string message;
		public static int messageIndex = 0;
		public static int messageTimer = 0;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileHammer[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Height = 4;
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16];
			//TileObjectData.newTile.AnchorBottom = new AnchorData();
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
			if (++frameCounter >= 9) {
				frameCounter = 0;
				frame = (frame + 1) % 6;
			}
		}
		public static void DisplayMessage(string key) {
			string text = Regex.Replace(Language.GetOrRegister(key).Value, "", " ");
			StringBuilder builder = new();
			for (int i = 0; i < text.Length; i++) {
				string charKey = "MorseLookup." + text[i].ToString().ToUpperInvariant();
				if (Language.Exists(charKey)) {
					builder.Append(Language.GetOrRegister(charKey).Value);
				} else {
					return;
				}
			}
			message = builder.ToString();
			messageIndex = 0;
			Main.tileFrameCounter[ModContent.TileType<Defiled_Relay>()] = 0;
		}
		public override bool CanKillTile(int i, int j, ref bool blockDamaged) {
			Player player = Main.LocalPlayer;
			DisplayMessage("please stop");
			if (player.HeldItem.hammer >= 45) {
				return true;
			}
			Projectile.NewProjectile(WorldGen.GetItemSource_FromTileBreak(i, j), new Vector2((i + 1) * 16, (j + 1) * 16), Vector2.Zero, ModContent.ProjectileType<Projectiles.Misc.Defiled_Wastelands_Signal>(), 0, 0, ai0: 0, ai1: Main.myPlayer);
			return false;
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			World.BiomeData.Defiled_Wastelands.CheckFissure(i, j, Type);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = g = b = 0.3f;
		}
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
