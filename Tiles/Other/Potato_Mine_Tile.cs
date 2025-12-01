using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Origins.Items.Weapons.Demolitionist;
using Origins.Projectiles;
using Origins.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using ThoriumMod.Projectiles;

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
			TileID.Sets.PressurePlate[Type] = 0;

			// Names
			AddMapEntry(new Color(150, 115, 45), CreateMapEntryName());

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.addTile(Type);
			ID = Type;
		}
		public override void HitSwitch(int i, int j) {
			WiringMethods.HitWireSingle(i, j);
			Wiring.TripWire(i, j, 1, 1);
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
		public override void HitWire(int i, int j) {
			Projectile.NewProjectile(
				Entity.GetSource_None(),
				new Vector2(i + 0.5f, j + 0.5f) * 16,
				default,
				Potato_Mine_Projectile.ID,
				50,
				6,
				Owner: Main.myPlayer
			);
			Tile tile = Main.tile[i, j];
			tile.HasTile = false;
		}
	}
	public class Potato_Mine_Projectile : ExplosionProjectile {
		public static int ID { get; private set; }
		public override DamageClass DamageType => DamageClasses.Explosive;
		public override int Size => 96;
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.timeLeft = 5;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.trap = true;
			Projectile.friendly = true;
			Projectile.hostile = true;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
		}
	}
}
