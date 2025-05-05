using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Demolitionist;
using Origins.Projectiles;
using System.Collections.Generic;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Audio;
using PegasusLib;

namespace Origins.Tiles.Other {
	public class Omnidirectional_Claymore_Tile : ModTile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;

			// Names
			AddMapEntry(new Color(125, 125, 125), CreateMapEntryName());

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.addTile(Type);
			ID = Type;
			DustType = DustID.Lead;
			RegisterItemDrop(ModContent.ItemType<Omnidirectional_Claymore>());
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Framing.GetTileSafely(i, j);
			tile.TileFrameX = 0;
			return false;
		}
		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;

			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<Omnidirectional_Claymore>();

			if (Main.tile[i, j].TileFrameX / 18 < 1) {
				player.cursorItemIconReversed = true;
			}
		}
		public override bool RightClick(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			tile.TileFrameY = (short)((tile.TileFrameY + (18 * (Main.LocalPlayer.Center.X < i * 16 + 8 ? -1 : 1)) + 90) % 90);
			NetMessage.SendTileSquare(-1, i, j);
			return true;
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			ModContent.GetInstance<Omnidirectional_Claymore_TE_System>().AddTileEntity(new(i, j));
		}
	}
	public class Omnidirectional_Claymore_TE_System : TESystem {
		public HashSet<Point16> projLocations;
		public override void PreUpdateEntities() {
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			projLocations ??= [];
			for (int i = 0; i < tileEntityLocations.Count; i++) {
				Point16 pos = tileEntityLocations[i];
				if (Main.tile[pos.X, pos.Y].TileIsType(Omnidirectional_Claymore_Tile.ID)) {
					if (!projLocations.Contains(pos)) {
						Projectile.NewProjectile(
							Entity.GetSource_None(),
							new Vector2(pos.X + 0.5f, pos.Y + 0.5f) * 16,
							default,
							Omnidirectional_Claymore_Projectile.ID,
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
			Omnidirectional_Claymore_TE_System instance = ModContent.GetInstance<Omnidirectional_Claymore_TE_System>();
			instance.projLocations ??= [];
			return instance.projLocations.Add(pos);
		}
	}
	public class Omnidirectional_Claymore_Projectile : ModProjectile {
		public override string Texture => typeof(Traffic_Cone_Item).GetDefaultTMLName();
		Vector2 LaserStartPoint => Projectile.Center - new Vector2(1, 3);
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.trap = true;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.netImportant = true;
			Projectile.hide = false;
			//Mod.Logger.Info("Spawned ODC projectile on netmode" + Main.netMode);
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			Projectile.timeLeft = 5;
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			Point16 tilePos = Projectile.position.ToTileCoordinates16();
			Tile tile = Main.tile[tilePos.X, tilePos.Y];
			if (tile.TileIsType(Omnidirectional_Claymore_Tile.ID)) {
				if (!Omnidirectional_Claymore_TE_System.RegisterProjPosition(tilePos)) {
					Projectile.Kill();
				} else {
					Vector2 unit = Vector2.UnitX;
					float unitDiag = 0.70710677f;
					switch (tile.TileFrameY / 18) {
						case 0:
						unit = -Vector2.UnitY;
						break;
						case 1:
						unit = new Vector2(-unitDiag, -unitDiag);
						break;
						case 2:
						unit = -Vector2.UnitX;
						break;
						case 3:
						unit = Vector2.UnitX;
						break;
						case 4:
						unit = new Vector2(unitDiag, -unitDiag);
						break;
					}
					Vector2 laserStartPoint = LaserStartPoint;
					Vector2 oldVel = Projectile.velocity;
					Projectile.velocity = unit * CollisionExt.Raymarch(laserStartPoint, unit, 8 * 16);
					if (Projectile.velocity != oldVel) {
						Projectile.netUpdate = true;
					}
					foreach (NPC npc in Main.ActiveNPCs) {
						if (npc.CanBeChasedBy(Projectile) && Collision.CheckAABBvLineCollision2(npc.position, npc.Size, laserStartPoint, laserStartPoint + Projectile.velocity)) {
							Explode();
							return;
						}
					}
					foreach (Player player in Main.ActivePlayers) {
						if (Collision.CheckAABBvLineCollision2(player.position, player.Size, laserStartPoint, laserStartPoint + Projectile.velocity)) {
							Explode();
							return;
						}
					}
				}
			} else {
				Projectile.Kill();
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Vector2 laserStartPoint = LaserStartPoint;
			OriginExtensions.DrawDebugLineSprite(laserStartPoint + Projectile.velocity.SafeNormalize(default) * 2, laserStartPoint + Projectile.velocity, Color.Red, -Main.screenPosition);
			return false;
		}
		void Explode() {
			Point16 tilePos = Projectile.position.ToTileCoordinates16();
			Tile tile = Main.tile[tilePos.X, tilePos.Y];
			if (!tile.TileIsType(Omnidirectional_Claymore_Tile.ID)) {
				Projectile.Kill();
				return;
			}
			tile.HasTile = false;
			NetMessage.SendTileSquare(-1, tilePos.X, tilePos.Y);
			Projectile.NewProjectile(
				Projectile.GetSource_FromAI(),
				LaserStartPoint,
				Projectile.velocity,
				Omnidirectional_Claymore_Explosion.ID,
				Projectile.damage,
				Projectile.knockBack
			);
			Projectile.Kill();
		}
	}
	public class Omnidirectional_Claymore_Explosion : ModProjectile, IIsExplodingProjectile {
		public override string Texture => typeof(Traffic_Cone_Item).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		bool didVisual = false;
		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.trap = true;
			Projectile.friendly = true;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.netImportant = true;
			Projectile.timeLeft = 2;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (Main.netMode == NetmodeID.Server || didVisual) return;
			didVisual = true;
			Vector2 direction = Projectile.velocity.SafeNormalize(-Vector2.UnitY);
			Vector2 spread = GetSpread(direction);
			for (int i = 0; i < 30; i++) {
				Vector2 dustVelocity = (direction * Main.rand.NextFloat(0.8f, 1) + spread * Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
				Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.Smoke,
					dustVelocity.X,
					dustVelocity.Y,
					100,
					default,
					1.5f
				).velocity *= 1.4f;
			}
			for (int i = 0; i < 20; i++) {
				Vector2 dustVelocity = (direction * Main.rand.NextFloat(0.8f, 1) + spread * Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.Torch,
					dustVelocity.X,
					dustVelocity.Y,
					100,
					default,
					3.5f
				);
				dust.noGravity = true;
				dust.velocity *= 7f;
				Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.Torch,
					dustVelocity.X,
					dustVelocity.Y,
					100,
					default,
					1.5f
				).velocity *= 3f;
			}
			SoundEngine.PlaySound(in SoundID.Item62, Projectile.position);
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.position.X);
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.position.X);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 laserStartPoint = Projectile.position;
			Vector2 spread = GetSpread(Projectile.velocity);
			return new Triangle(laserStartPoint, laserStartPoint + Projectile.velocity - spread, laserStartPoint + Projectile.velocity + spread).Intersects(targetHitbox);
		}
		static Vector2 GetSpread(Vector2 velocity) => velocity.RotatedBy(MathHelper.PiOver2) * 0.5f;
		public bool IsExploding() => true;
	}
}
