using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Tools.Wiring;
using Origins.Items.Weapons.Demolitionist;
using Origins.World.BiomeData;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Beacon_Light : OriginTile, IComplexMineDamageTile, IAshenWireTile {
		public static int ID { get; private set; }
		TileItem item;
		public override void Load() {
			Mod.AddContent(item = new(this));
		}
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;

			// Names
			AddMapEntry(FromHexRGB(0xFFB18C), item.DisplayName);

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.addTile(Type);
			ID = Type;
			DustType = Ashen_Biome.DefaultTileDust;
			RegisterItemDrop(item.Type);
		}
		public void MinePower(int i, int j, int minePower, ref int damage) {
			if (minePower < 55 && Main.tile[i, j].TileFrameX >= 18 * 2 * 2) damage = 0;
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			ModContent.GetInstance<Beacon_Light_TE_System>().AddTileEntity(new(i, j));
		}
		public void UpdatePowerState(int i, int j, bool powered) {
			if (Main.tile[i, j].TileFrameX < 18 * 2 * 2) AshenWireTile.DefaultUpdatePowerState(i, j, powered, tile => ref tile.TileFrameX, 18 * 2, true);
		}
		public override void HitWire(int i, int j) {
			UpdatePowerState(i, j, AshenWireTile.DefaultIsPowered(i, j));
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (!fail) {
				Tile tile = Main.tile[i, j];
				if (tile.TileFrameX < 18 * 2 * 2) {
					TileObjectData data = TileObjectData.GetTileData(tile);
					TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
					for (int x = 0; x < data.Width; x++) {
						for (int y = 0; y < data.Height; y++) {
							tile = Main.tile[left + x, top + y];
							tile.TileFrameX %= 18 * 2;
							tile.TileFrameX += 18 * 2 * 2;
						}
					}
					fail = true;
				}
			}
		}
	}
	public class Beacon_Light_TE_System : TESystem {
		HashSet<Point16> processedLocations;
		List<Rectangle> hitboxes;
		public const string biome_name = "Origins:BeaconLight";
		public override void Load() {
			Overlays.Scene[biome_name] = new Beacon_Light_Overlay();
		}
		public override void PreUpdateEntities() {
			processedLocations ??= [];
			hitboxes ??= [];
			for (int i = 0; i < tileEntityLocations.Count; i++) {
				Point16 pos = tileEntityLocations[i];
				Tile tile = Main.tile[pos];
				if (tile.TileIsType(Beacon_Light.ID) && processedLocations.Add(pos)) {
					if (tile.TileFrameX < 18 * 2) hitboxes.Add(new(pos.X * 16 - 32, pos.Y * 16 - 16, 64, 64));
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
		public override void PostDrawTiles() {
			bool inZone = tileEntityLocations.Count > 0;
			if (inZone != (Overlays.Scene[biome_name].Mode != OverlayMode.Inactive)) {
				if (inZone)
					Overlays.Scene.Activate(biome_name, default);
				else
					Overlays.Scene[biome_name].Deactivate();
			}
		}
		public class Beacon_Light_Overlay() : Overlay(EffectPriority.High, RenderLayers.ForegroundWater) {
			public override void Draw(SpriteBatch spriteBatch) {
				List<Point16> tileEntityLocations = ModContent.GetInstance<Beacon_Light_TE_System>().tileEntityLocations;
				SpriteBatchState state = spriteBatch.GetState();
				spriteBatch.Restart(state, samplerState: SamplerState.LinearClamp);
				Color color = new(255, 123, 72);
				const float range = 16 * 2640; // a mile
				for (int i = 0; i < tileEntityLocations.Count; i++) {
					Point16 tilePos = tileEntityLocations[i];
					Tile tile = Main.tile[tilePos];
					if (tile.TileFrameX >= 18 * 2) continue;
					Vector2 pos = tilePos.ToWorldCoordinates(0, 0);
					float obscureFactor = 1f;
					foreach (Player player in Main.ActivePlayers) {
						Rectangle rect = player.Hitbox;
						rect.Inflate(-4, -4);
						float distSQ = pos.Clamp(rect).DistanceSQ(pos);
						if (distSQ <= 0) {
							obscureFactor = 0;
							break;
						}
						if (distSQ < 4 * 4) {
							obscureFactor = Math.Min(float.Sqrt(distSQ) / 4, obscureFactor);
						}
					}
					if (obscureFactor <= 0) continue;
					float scaleFactor = float.Pow(1 - pos.Distance(pos.Clamp(Main.Camera.ScaledPosition, Main.Camera.ScaledPosition + Main.Camera.ScaledSize)) / range, 1.5f);
					if (scaleFactor < 0.5f) scaleFactor *= scaleFactor + 0.5f;
					if (scaleFactor < 0.1f) continue;
					if (scaleFactor > 1) scaleFactor = 1;
					scaleFactor *= obscureFactor;
					Color currentColor = color * scaleFactor;
					Flare_Launcher_Glow_P.DrawGlow(
						pos,
						currentColor,
						scale: 5 * scaleFactor
					);
				}
				spriteBatch.Restart(state);
			}
			public override void Update(GameTime gameTime) { }
			public override void Activate(Vector2 position, params object[] args) { }
			public override void Deactivate(params object[] args) { }
			public override bool IsVisible() => true;
		}
	}
}
