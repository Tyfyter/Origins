using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Demolitionist;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Origins.Tiles.Other {
    public class Potato_Mine_Tile : ModTile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;

			// Names
			AddMapEntry(new Color(150, 115, 45), CreateMapEntryName());

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.addTile(Type);
			ID = Type;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Framing.GetTileSafely(i, j);
			tile.TileFrameX = 0;
			tile.TileFrameY = 0;
			return false;
		}
        public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;

			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<Potato_Mine>();

			if (Main.tile[i, j].TileFrameX / 18 < 1) {
				player.cursorItemIconReversed = true;
			}
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			ModContent.GetInstance<Potato_Mine_TE_System>().AddTileEntity(new(i, j));
		}
	}
	public class Potato_Mine_TE_System : TESystem {
		public HashSet<Point16> projLocations;
		public override void PreUpdateEntities() {
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			projLocations ??= [];
			for (int i = 0; i < tileEntityLocations.Count; i++) {
				Point16 pos = tileEntityLocations[i];
				if (Main.tile[pos.X, pos.Y].TileIsType(Potato_Mine_Tile.ID)) {
					if (!projLocations.Contains(pos)) {
						Projectile.NewProjectile(
							Entity.GetSource_None(),
							new Vector2(pos.X + 0.5f, pos.Y + 0.5f) * 16,
							default,
							Potato_Mine_Projectile.ID,
							50,
							6,
							Owner: Main.myPlayer
						);
					}
				} else {
					tileEntityLocations.RemoveAt(i);
					i--;
					continue;
				}
			}
			projLocations.Clear();
		}
		internal static bool RegisterProjPosition(Point16 pos) {
			Potato_Mine_TE_System instance = ModContent.GetInstance<Potato_Mine_TE_System>();
			instance.projLocations ??= new();
			return instance.projLocations.Add(pos);
		}
	}
	public class Potato_Mine_Projectile : ModProjectile {
		public override string Texture => typeof(Traffic_Cone_Item).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.trap = true;
			Projectile.friendly = true;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.netImportant = true;
			Projectile.hide = true;
			Projectile.ArmorPenetration += 6;
		}
		public override void AI() {
			Projectile.timeLeft = 5;
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			Point16 tilePos = Projectile.position.ToTileCoordinates16();
			if (Main.tile[tilePos.X, tilePos.Y].TileIsType(Potato_Mine_Tile.ID)) {
				if (!Potato_Mine_TE_System.RegisterProjPosition(tilePos)) {
					Projectile.Kill();
				}
			} else {
				Projectile.Kill();
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Explode();
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.immune = true;
			target.AddImmuneTime(info.CooldownCounter, 2);
			Explode();
		}
		void Explode() {
			Point16 tilePos = Projectile.position.ToTileCoordinates16();
			Tile tile = Main.tile[tilePos.X, tilePos.Y];
			if (!tile.TileIsType(Potato_Mine_Tile.ID)) {
				Projectile.Kill();
				return;
			}
			tile.HasTile = false;
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62);
			Projectile.Damage();
			Projectile.Kill();
		}
	}
}
