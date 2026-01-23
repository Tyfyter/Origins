using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.NPCs.Ashen.Boss;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Heat_Vent : OriginTile, IComplexMineDamageTile, IGlowingModTile {
		public static int ID { get; private set; }
		TileItem item;
		public override void Load() {
			Mod.AddContent(item = new(this, true));
			this.SetupGlowKeys();
		}
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (ShouldGlow(tile)) color.DoFancyGlow(new(0.912f, 0.579f, 0f), tile.TileColor);
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileHammer[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;

			// Names
			AddMapEntry(new Color(220, 220, 220), CreateMapEntryName());

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Height = 6;
			TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height).ToArray();
			TileObjectData.newTile.Origin = new Point16(0, 5);
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.StyleWrapLimit = 3;
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.addTile(Type);
			ID = Type;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}

		public void MinePower(int i, int j, int minePower, ref int damage) {
			if (minePower < 55 && Main.tile[i, j].TileFrameY >= 18 * 6 * 2) damage = 0;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (ShouldGlow(Main.tile[i, j])) {
				r = 0.0912f;
				g = 0.0579f;
				b = 0f;
			}
		}
		public static bool ShouldGlow(Tile tile) {
			int frameY = (tile.TileFrameY / 18) % 6;
			if (tile.TileFrameX <= 2 * 18 && frameY == 0) return false;
			if (frameY >= 4) return false;
			return tile.TileFrameY < (18 * 6 * 2);
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (!fail) {
				Tile tile = Main.tile[i, j];
				if (tile.TileFrameY < 18 * 6 * 2) {
					TileObjectData data = TileObjectData.GetTileData(tile);
					TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
					for (int x = 0; x < data.Width; x++) {
						for (int y = 0; y < data.Height; y++) {
							tile = Main.tile[left + x, top + y];
							tile.TileFrameY += 18 * 6;
						}
					}
					fail = true;
					if (tile.TileFrameY >= 18 * 6 * 2) {
						CheckVent(left, top);
					}
				}
			}
		}
		public static void CheckVent(int i, int j) {
			if (WorldGen.generatingWorld) {
				return;
			}
			int x = Main.tile[i, j].TileFrameX != 0 ? i - 1 : i;
			int y = Main.tile[i, j].TileFrameY != 0 && Main.tile[i, j].TileFrameY != 36 ? j - 1 : j;
			if (Main.netMode != NetmodeID.MultiplayerClient && !WorldGen.noTileActions) {

				float fx = x * 16;
				float fy = y * 16;
				float distance = float.PositiveInfinity;
				int plr = 0;
				for (int pindex = 0; pindex < 255; pindex++) {
					float currentDist = Math.Abs(Main.player[pindex].position.X - fx) + Math.Abs(Main.player[pindex].position.Y - fy);
					if (currentDist < distance) {
						plr = pindex;
						distance = currentDist;
					}
				}

				DropAttemptInfo dropInfo = default;
				dropInfo.player = Main.player[plr];
				dropInfo.IsExpertMode = Main.expertMode;
				dropInfo.IsMasterMode = Main.masterMode;
				dropInfo.IsInSimulation = false;
				dropInfo.rng = Main.rand;
				Origins.ResolveRuleWithHandler(WorldGen.shadowOrbSmashed ? Ashen_Biome.OrbDropRule : Ashen_Biome.FirstOrbDropRule, dropInfo, (info, item, stack, _) => {
					Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 32, 32, item, stack, pfix: -1);
				});
				WorldGen.shadowOrbSmashed = true;
				WorldGen.shadowOrbCount++;
				LocalizedText localizedText = Language.GetText("Mods.Origins.Status_Messages.Heat_Vent_" + int.Min(WorldGen.shadowOrbCount, 3));
				List<Point16> tileEntityLocations = ModContent.GetInstance<Heat_Vent_TE_System>().tileEntityLocations;
				object[] formatArgs = [
					tileEntityLocations.IndexOf(new(i, j + 5)),
					00
				];
				if (Main.netMode == NetmodeID.SinglePlayer) {
					Main.NewText(localizedText.Format(formatArgs), 220, 90, 0);
				} else if (Main.netMode == NetmodeID.Server) {
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey(localizedText.Key, formatArgs), new Color(220, 90, 0));
				}
				AchievementsHelper.NotifyProgressionEvent(7);

				if (WorldGen.shadowOrbCount >= 3) {
					new Spawn_Trenchmaker_Action(dropInfo.player).Perform();
					WorldGen.shadowOrbCount = 0;
				}
			}
			SoundEngine.PlaySound(SoundID.NPCDeath1, new Vector2(i * 16, j * 16));
			WorldGen.destroyObject = false;
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			ModContent.GetInstance<Heat_Vent_TE_System>().AddTileEntity(new(i, j));
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
	}
	public class Heat_Vent_TE_System : TESystem {
		HashSet<Point16> processedLocations;
		List<Rectangle> hitboxes;
		public const string biome_name = "Origins:BeaconLight";
		public override void PreUpdateEntities() {
			processedLocations ??= [];
			hitboxes ??= [];
			for (int i = 0; i < tileEntityLocations.Count; i++) {
				Point16 pos = tileEntityLocations[i];
				Tile tile = Main.tile[pos];
				if (tile.TileIsType(Heat_Vent.ID) && processedLocations.Add(pos)) {
					if (tile.TileFrameY < 18 * 6 * 2) hitboxes.Add(new(pos.X * 16, pos.Y * 16 - 16 * 5, 32, 16 * 4));
				} else {
					tileEntityLocations.RemoveAt(i--);
				}
			}
			processedLocations.Clear();
			if (!NetmodeActive.Server && hitboxes.Any(Main.LocalPlayer.Hitbox.Intersects)) {
				Main.LocalPlayer.AddBuff(BuffID.OnFire3, 90);
			}
			foreach (NPC npc in Main.ActiveNPCs) {
				if (hitboxes.Any(npc.Hitbox.Intersects)) {
					int index = npc.FindBuffIndex(BuffID.OnFire3);
					if (index >= 0) {
						if (npc.buffTime[index] < 90) npc.buffTime[index] = 90;
					} else {
						npc.AddBuff(BuffID.OnFire3, 90);
					}
				}
			}
			hitboxes.Clear();
		}
	}
}
