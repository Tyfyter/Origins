using MonoMod.Cil;
using Origins.Core;
using Origins.Items.Tools.Wiring;
using Origins.World.BiomeData;
using PegasusLib;
using PegasusLib.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Origins.Tiles.Ashen.Cargo_Elevator_Door_TE_System;

namespace Origins.Tiles.Ashen {
	public class Cargo_Elevator_Door : OriginTile, IComplexMineDamageTile, IMultiTypeMultiTile {
		public TileItem Item { get; protected set; }
		public override void Load() {
			Mod.AddContent(Item = new(this));
		}
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileSolid[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.DrawsWalls[Type] = true;
			TileID.Sets.HasOutlines[Type] = false;

			// Names
			if (this is not Cargo_Elevator_Door_Open) AddMapEntry(new Color(220, 220, 220), CreateMapEntryName());

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = 11;
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.AnchorBottom = new();
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.addTile(Type);
			DustType = Ashen_Biome.DefaultTileDust;
			RegisterItemDrop(Item.Type);
		}
		// TODO: implement
		public static bool IsSolid(int i, int j) {
			Tile tile = Main.tile[i, j];
			int distFromCenter = Math.Abs(tile.TileFrameX / 18 - 5);
			switch (tile.TileFrameY / (18 * 3)) {
				case 12:
				case 13:
				if (distFromCenter <= 0) return false;
				break;
				case 14:
				case 15:
				if (distFromCenter <= 1) return false;
				break;
				case 16:
				case 17:
				if (distFromCenter <= 2) return false;
				break;
				case 18:
				if (distFromCenter <= 3) return false;
				break;
			}
			return true;
		}
		public override bool Slope(int i, int j) => false;
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.001f;
			g = 0.001f;
			b = 0.001f;
		}
		public static bool IsPowered(int i, int j) {
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			for (int x = 0; x < data.Width; x++) {
				for (int y = 0; y < data.Height; y++) {
					if (Main.tile[left + x, top + y].Get<Ashen_Wire_Data>().AnyPower) return true;
				}
			}
			return false;
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (!fail) fail = IsPowered(i, j);
		}
		public override void HitWire(int i, int j) {
			bool powered = IsPowered(i, j);
			bool wasPowered = Main.tile[i, j].TileFrameX >= 18 * 4;
			if (powered != wasPowered) {
				TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
				TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
				for (int x = 0; x < data.Width; x++) {
					for (int y = 0; y < data.Height; y++) {
						Tile tile = Main.tile[left + x, top + y];
						tile.TileFrameX = (short)(tile.TileFrameX % (18 * 4) + (powered ? 4 * 18 : 0));
					}
				}
				if (!NetmodeActive.SinglePlayer) NetMessage.SendTileSquare(-1, left, top, data.Width, data.Height);
			} else {
				Toggle(i, j);
			}
		}
		public override bool RightClick(int i, int j) => Toggle(i, j);
		public override void PlaceInWorld(int i, int j, Item item) {
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			ModContent.GetInstance<Cargo_Elevator_Door_TE_System>().AddTileEntity(new(left, top));
		}
		public static bool Toggle(int i, int j, bool actuallyDo = true) {
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			Door_Animation animation = Cargo_Elevator_Door_TE_System.GetAnimation(new(left, top));
			if (animation.IsAnimating) return false;
			if (actuallyDo) new Cargo_Elevator_Door_Action(new(left, top), !animation.TargetOpen).Perform();
			return true;
		}

		public bool IsValidTile(Tile tile) => tile.TileType == ModContent.TileType<Cargo_Elevator_Door>() || tile.TileType == ModContent.TileType<Cargo_Elevator_Door_Open>();
	}
	public class Cargo_Elevator_Door_Open : Cargo_Elevator_Door {
		public override string Texture => typeof(Cargo_Elevator_Door).GetDefaultTMLName();
		public override void Load() {}
		public override void SetStaticDefaults() {
			Item = ModContent.GetInstance<Cargo_Elevator_Door>().Item;
			base.SetStaticDefaults();
			Main.tileSolidTop[Type] = true;
			OriginsSets.Tiles.MultitileCollisionOffset[Type] = OffsetBookcaseCollision;
		}
		static void OffsetBookcaseCollision(Tile tile, ref float y, ref int height) {
			if ((tile.TileFrameY / 18) % 3 != 0) height = -1600;
		}
	}
	public class Cargo_Elevator_Door_TE_System : TESystem {
		Dictionary<Point16, Door_Animation> openDoors;
		public override void PreUpdateEntities() {
			openDoors ??= [];
			for (int i = 0; i < tileEntityLocations.Count; i++) {
				Point16 pos = tileEntityLocations[i];
				Tile tile = Main.tile[pos];
				if (tile.HasTile && tile.TileType == ModContent.TileType<Cargo_Elevator_Door>()) {
					GetAnimation(pos).Update(pos);
				} else {
					tileEntityLocations.RemoveAt(i--);
				}
			}
			foreach (Point16 item in openDoors.Keys.Except(tileEntityLocations).ToArray()) {
				openDoors.Remove(item);
			}
		}
		public override void LoadWorldData(TagCompound tag) {
			base.LoadWorldData(tag);
			int closed = ModContent.TileType<Cargo_Elevator_Door>();
			int open = ModContent.TileType<Cargo_Elevator_Door_Open>();
			openDoors ??= [];
			foreach (Point16 pos in tileEntityLocations) {
				Tile tile = Main.tile[pos];
				TileObjectData data = TileObjectData.GetTileData(tile);
				TileUtils.GetMultiTileTopLeft(pos.X, pos.Y, data, out int left, out int top);
				if (pos.X == left && pos.Y == top) {
					if (tile.TileType == open) {
						Door_Animation animation = GetAnimation(pos);
						animation.TargetOpen = true;
						animation.frame = Door_Animation.max_frame;
					} else if (tile.TileType == closed) {
						Door_Animation animation = GetAnimation(pos);
						animation.TargetOpen = false;
						animation.frame = 0;
					}
				}
			}
		}
		public static Door_Animation GetAnimation(Point16 position) {
			Dictionary<Point16, Door_Animation> openDoors = ModContent.GetInstance<Cargo_Elevator_Door_TE_System>().openDoors;
			openDoors.TryAdd(position, new());
			return openDoors[position];
		}
		public class Door_Animation {
			public bool TargetOpen = false;
			public int frame = 0;
			public int frameCounter = 0;
			public bool IsAnimating => frame != TargetOpen.Mul(max_frame);
			public const int max_frame = 18;
			public void Update(Point16 position) {
				if (TargetOpen && Main.tile[position].TileFrameX >= 4 * 18) TargetOpen = false;
				if (!IsAnimating) return;
				if (++frameCounter > 4) {
					frameCounter = 0;
					frame += TargetOpen.ToDirectionInt();
					TileObjectData data = TileObjectData.GetTileData(Main.tile[position]);
					TileUtils.GetMultiTileTopLeft(position.X, position.Y, data, out int left, out int top);
					ushort closed = (ushort)ModContent.TileType<Cargo_Elevator_Door>();
					ushort open = (ushort)ModContent.TileType<Cargo_Elevator_Door_Open>();
					for (int x = 0; x < data.Width; x++) {
						for (int y = 0; y < data.Height; y++) {
							Tile tile = Main.tile[left + x, top + y];
							tile.TileFrameY = (short)(frame * 3 * 18 + y * 18);
							tile.TileType = Cargo_Elevator_Door.IsSolid(left + x, top + y) ? closed : open;
						}
					}
					/*if (!TargetOpen && frame < 4 && !NetmodeActive.MultiplayerClient) {
						Rectangle hitbox = new(left * 16, top * 16, 16 * 2, 16 * 3);
						foreach (Player player in Main.ActivePlayers) {
							if (player.shimmering) continue;
							if (player.Hitbox.Intersects(hitbox)) {
								player.Hurt(
									PlayerDeathReason.ByCustomReason(TextUtils.LanguageTree.Find("Mods.Origins.DeathMessage.Crushed").SelectFrom(player.name).ToNetworkText()),
									player.statLife / (frame + 1),
									0,
									cooldownCounter: -2,
									dodgeable: false,
									knockback: 18,
									scalingArmorPenetration: 1
								);
							}
						}
						foreach (NPC npc in Main.ActiveNPCs) {
							if (npc.noTileCollide || npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type]) continue;
							if (npc.Hitbox.Intersects(hitbox)) {
								npc.SimpleStrikeNPC(npc.life / (frame + 1), 0, true, 18, noPlayerInteraction: true);
							}
						}
					}*/
				}
			}
		}
		public record class Cargo_Elevator_Door_Action(Point16 Position, bool Open) : SyncedAction {
			protected override bool ShouldPerform => GetAnimation(Position).TargetOpen != Open;
			public Cargo_Elevator_Door_Action() : this(default, default) { }
			public override SyncedAction NetReceive(BinaryReader reader) => this with {
				Position = new(reader.ReadInt16(), reader.ReadInt16()),
				Open = reader.ReadBoolean()
			};
			public override void NetSend(BinaryWriter writer) {
				writer.Write((short)Position.X);
				writer.Write((short)Position.Y);
				writer.Write(Open);
			}
			protected override void Perform() => GetAnimation(Position).TargetOpen = Open;
		}
	}
}
