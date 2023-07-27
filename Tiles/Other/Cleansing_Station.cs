using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.NPCs.Defiled;
using Origins.NPCs.MiscE;
using Origins.NPCs.Riven;
using Origins.World.BiomeData;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Other {
    public class Cleansing_Station : ModTile, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => CanUse(Main.LocalPlayer) ? Color.White : Color.Transparent;
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = Mod.Assets.Request<Texture2D>("Tiles/Other/Cleansing_Station_Glow");
			}
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 3, 0);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(6, 157, 44), Language.GetText("Cleansing Station"));
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return CanUse(Main.LocalPlayer);
		}
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ModContent.ItemType<Cleansing_Station_Item>());
		}
		static bool CanUse(Player player) {
			const float maxDist = 25 * 16;
			return !(
				player.ZoneCorrupt
				|| player.ZoneCrimson
				|| player.InModBiome<Defiled_Wastelands>()
				|| player.InModBiome<Riven_Hive>()
				|| Main.npc.Any(
					npc =>
						npc.active
						&& npc.lifeMax > 5
						&& !npc.friendly
						&& npc.DistanceSQ(player.Center) < maxDist * maxDist
						&& (
							npc.TryGetGlobalNPC<CorruptGlobalNPC>(out _)
							|| npc.TryGetGlobalNPC<CrimsonGlobalNPC>(out _)
							|| npc.TryGetGlobalNPC<DefiledGlobalNPC>(out _)
							|| npc.TryGetGlobalNPC<RivenGlobalNPC>(out _)
						)
					)
			);
		}
		public override void MouseOver(int i, int j) {
			if (!CanUse(Main.LocalPlayer)) return;
			Main.LocalPlayer.cursorItemIconEnabled = true;
			Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<Cleansing_Station_Item>();
		}
		public override bool RightClick(int i, int j) {
			if (!CanUse(Main.LocalPlayer)) {
				Main.NewText(Language.GetTextValue("Mods.Origins.Status_Messages.Cleansing_Station_Invalid_Location"));
				return false;
			}
			OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
			originPlayer.mojoFlaskCount = originPlayer.mojoFlaskCountMax;
			float assimilationTotal = originPlayer.CorruptionAssimilation + originPlayer.CrimsonAssimilation + originPlayer.DefiledAssimilation + originPlayer.RivenAssimilation;
			if (assimilationTotal > 0) {
				for (int k = 0; k < 3 + 6 * assimilationTotal; k++) {
					Vector2 pos = new Vector2(i + Main.rand.NextFloat(1), j + Main.rand.NextFloat(1)) * 16;
					Vector2 dir = ((Main.LocalPlayer.MountedCenter - pos) / 24).WithMaxLength(8);
					Dust dust = Dust.NewDustDirect(pos, 0, 0, DustID.Phantasmal, dir.X, dir.Y);
					dust.velocity += dir;
					dust.noGravity = true;
					dust.fadeIn = 1.5f;
					dust.color = Color.Blue;
				}
			}
			for (int k = 0; k < 2; k++) {
				Vector2 pos = new Vector2(i + Main.rand.NextFloat(1), j + Main.rand.NextFloat(1)) * 16;
				Vector2 dir = ((Main.LocalPlayer.MountedCenter - pos) / 24).WithMaxLength(8);
				Dust dust = Dust.NewDustDirect(pos, 0, 0, DustID.Phantasmal, dir.X, dir.Y);
				dust.velocity += dir;
				dust.noGravity = true;
				dust.fadeIn = 1.5f;
				dust.color = Color.Blue;
			}
			originPlayer.CorruptionAssimilation = 0;
			originPlayer.CrimsonAssimilation = 0;
			originPlayer.DefiledAssimilation = 0;
			originPlayer.RivenAssimilation = 0;
			return true;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0f;
			g = 0.05f;
			b = 0.055f;
		}
	}
	public class Cleansing_Station_Item : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Cleansing Station");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.BoneTable);
			Item.value = Item.buyPrice(gold: 2);
			Item.createTile = ModContent.TileType<Cleansing_Station>();
			Item.placeStyle = 0;
		}
	}
}
