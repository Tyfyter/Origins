using Microsoft.Xna.Framework.Graphics;
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
	public class Beacon_Light : OriginTile {
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

			// Names
			AddMapEntry(new Color(220, 220, 220), CreateMapEntryName());

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.addTile(Type);
			ID = Type;
			DustType = Ashen_Biome.DefaultTileDust;
			RegisterItemDrop(item.Type);
		}
		public override void MouseOver(int i, int j) {
			return;
			Player player = Main.LocalPlayer;

			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<Omnidirectional_Claymore>();

			if (Main.tile[i, j].TileFrameX / 18 < 1) {
				player.cursorItemIconReversed = true;
			}
		}
		public override bool RightClick(int i, int j) {
			return false;
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			ModContent.GetInstance<Beacon_Light_TE_System>().AddTileEntity(new(i, j));
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
				if (Main.tile[pos].TileIsType(Beacon_Light.ID) && processedLocations.Add(pos)) {
					hitboxes.Add(new(pos.X * 16 - 32, pos.Y * 16 - 16, 64, 64));
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
		internal static bool RegisterProjPosition(Point16 pos) {
			Beacon_Light_TE_System instance = ModContent.GetInstance<Beacon_Light_TE_System>();
			instance.processedLocations ??= [];
			return instance.processedLocations.Add(pos);
		}
		public class Beacon_Light_Overlay() : Overlay(EffectPriority.High, RenderLayers.ForegroundWater) {
			public override void Draw(SpriteBatch spriteBatch) {
				List<Point16> tileEntityLocations = ModContent.GetInstance<Beacon_Light_TE_System>().tileEntityLocations;
				SpriteBatchState state = spriteBatch.GetState();
				spriteBatch.Restart(state, samplerState: SamplerState.LinearClamp);
				Color color = new(255, 123, 72);
				const float range = 16 * 2640; // a mile
				for (int i = 0; i < tileEntityLocations.Count; i++) {
					Vector2 pos = tileEntityLocations[i].ToWorldCoordinates(0, 0);
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
