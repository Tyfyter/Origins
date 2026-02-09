using Origins.Core;
using Origins.Items.Tools.Wiring;
using Origins.Items.Weapons.Ammo;
using Origins.Tiles.Ashen.Hanging_Scrap;
using Origins.World.BiomeData;
using PegasusLib.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Origins.Tiles.Ashen.Cargo_Elevator_Door_TE_System;

namespace Origins.Tiles.Ashen {
	public class Cargo_Elevator_Door : OriginTile, IComplexMineDamageTile, IMultiTypeMultiTile, IAshenWireTile {
		public TileItem Item { get; protected set; }
		public override void Load() {
			Mod.AddContent(Item = new TileItem(this).WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.AdamantiteBar, 2)
				.AddIngredient(ModContent.ItemType<Scrap>(), 30)
				.AddTile(ModContent.TileType<Metal_Presser>())
				.Register();
				Recipe.Create(item.type)
				.AddIngredient(ItemID.TitaniumBar, 2)
				.AddIngredient(ModContent.ItemType<Scrap>(), 30)
				.AddTile(ModContent.TileType<Metal_Presser>())
				.Register();
			}));
		}
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileSolid[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.DrawsWalls[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;
			HitSound = SoundID.Tink;

			// Names
			if (this is not Cargo_Elevator_Door_Open) AddMapEntry(FromHexRGB(0x3C3A45), CreateMapEntryName());

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = 11;
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.AnchorBottom = new();
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.addTile(Type);
			DustType = Ashen_Biome.DefaultTileDust;
			MineResist = 8;
			if (this is not Cargo_Elevator_Door_Open) RegisterItemDrop(Item.Type);
		}
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
		public override bool CanExplode(int i, int j) => false;
		public override bool Slope(int i, int j) => false;
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.001f;
			g = 0.001f;
			b = 0.001f;
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX >= 11 * 18) return false;
			TileObjectData data = TileObjectData.GetTileData(tile);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			Door_Animation animation = Cargo_Elevator_Door_TE_System.GetAnimation(new(left, top));
			if (animation.IsAnimating) return false;
			if (AshenWireTile.DefaultIsPowered(i, j)) return false;
			return true;
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (!fail) fail = AshenWireTile.DefaultIsPowered(i, j);
		}
		public void UpdatePowerState(int i, int j, bool powered) => AshenWireTile.DefaultUpdatePowerState(i, j, powered, tile => ref tile.TileFrameX, 18 * 11);
		public override void HitWire(int i, int j) {
			UpdatePowerState(i, j, AshenWireTile.DefaultIsPowered(i, j));
			if (!Ashen_Wire_Data.HittingAshenWires) Toggle(i, j);
		}
		public override bool RightClick(int i, int j) => Toggle(i, j);
		public override void PlaceInWorld(int i, int j, Item item) {
			FrameSurrounding(i, j, out int left, out int top);
			ModContent.GetInstance<Cargo_Elevator_Door_TE_System>().AddTileEntity(new(left, top));
		}
		public static bool Toggle(int i, int j, bool actuallyDo = true) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX >= 11 * 18) return false;
			TileObjectData data = TileObjectData.GetTileData(tile);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			Door_Animation animation = Cargo_Elevator_Door_TE_System.GetAnimation(new(left, top));
			if (animation.IsAnimating) {
				switch (animation.TargetFrame) {
					case Door_Animation.locked_frame:
					return false;
					case Door_Animation.closed_frame:
					if (animation.frame >= Door_Animation.closed_frame) return false;
					break;
					case Door_Animation.open_frame:
					return false;
				}
			}
			if (actuallyDo) new Cargo_Elevator_Door_Action(new(left, top), animation.TargetFrame != Door_Animation.open_frame).Perform();
			return true;
		}

		public bool IsValidTile(Tile tile, int left, int top) {
			if (tile.HasTile && (tile.TileType == ModContent.TileType<Cargo_Elevator_Door>() || tile.TileType == ModContent.TileType<Cargo_Elevator_Door_Open>())) return true;
			return false;
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			tileFrameX %= 18 * 11;
		}
	}
	public class Cargo_Elevator_Door_Open : Cargo_Elevator_Door {
		public override string Texture => typeof(Cargo_Elevator_Door).GetDefaultTMLName();
		public override void Load() { }
		public override void SetStaticDefaults() {
			Item = ModContent.GetInstance<Cargo_Elevator_Door>().Item;
			base.SetStaticDefaults();
			Main.tileSolidTop[Type] = true;
			OriginsSets.Tiles.MultitileCollisionOffset[Type] = OffsetBookcaseCollision;
		}
		public override void PostSetDefaults() {
			Main.tileBlockLight[Type] = false;
			Main.tileNoSunLight[Type] = true;
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
			openDoors ??= [];
			foreach (Point16 pos in tileEntityLocations) {
				Tile tile = Main.tile[pos];
				TileObjectData data = TileObjectData.GetTileData(tile);
				if (data is null) continue;
				TileUtils.GetMultiTileTopLeft(pos.X, pos.Y, data, out int left, out int top);
				if (pos.X == left && pos.Y == top) {
					Door_Animation animation = GetAnimation(pos);
					if (tile.TileFrameX >= 11 * 18) {
						animation.TargetFrame = Door_Animation.locked_frame;
						animation.frame = Door_Animation.locked_frame;
					} else if (tile.TileFrameY == Door_Animation.open_frame * data.Height * 18) {
						animation.TargetFrame = Door_Animation.open_frame;
						animation.frame = Door_Animation.open_frame;
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
			public int TargetFrame = closed_frame;
			public int frame = 0;
			public int frameCounter = 0;
			public bool IsAnimating => frame != TargetFrame;
			public const int locked_frame = 0;
			public const int closed_frame = 7;
			public const int open_frame = 18;

			readonly HashSet<Point> leftClosing = [];
			readonly HashSet<Point> rightClosing = [];
			public void Update(Point16 position) {
				if ((Main.tile[position].TileFrameX >= 11 * 18) != (TargetFrame == locked_frame)) {
					if (TargetFrame == locked_frame) {
						TargetFrame = closed_frame;
					} else {
						TargetFrame = locked_frame;
					}
				}
				if (!IsAnimating) return;
				if (++frameCounter > 4) {
					frameCounter = 0;
					TileObjectData data = TileObjectData.GetTileData(Main.tile[position]);
					TileUtils.GetMultiTileTopLeft(position.X, position.Y, data, out int left, out int top);
					if (TargetFrame == open_frame && frame == 11) {
						for (int x = 0; x < data.Width; x++) {
							for (int y = 0; y < data.Height; y++) {
								if (Main.tile[left + x, top + y].Get<Hanging_Scrap_Data>().HasScrap) {

									TargetFrame = closed_frame;
									break;
								}
							}
						}
					}
					frame += TargetFrame.CompareTo(frame);
					ushort closed = (ushort)ModContent.TileType<Cargo_Elevator_Door>();
					ushort open = (ushort)ModContent.TileType<Cargo_Elevator_Door_Open>();
					leftClosing.Clear();
					rightClosing.Clear();
					for (int x = 0; x < data.Width; x++) {
						for (int y = 0; y < data.Height; y++) {
							Tile tile = Main.tile[left + x, top + y];
							tile.TileFrameY = (short)(frame * 3 * 18 + y * 18);
							if (tile.TileType.TrySet(Cargo_Elevator_Door.IsSolid(left + x, top + y) ? closed : open)) {
								if (TargetFrame != open_frame) {
									if (x < (data.Width + 1) / 2) leftClosing.Add(new(left + x, top + y));
									if (x > (data.Width - 2) / 2) rightClosing.Add(new(left + x, top + y));
								} else if (!NetmodeActive.MultiplayerClient && tile.TileType == open && tile.LiquidAmount > 0 && !WorldGen.noLiquidCheck) {
									Liquid.AddWater(left + x, top + y);
								}
							}
						}
					}
				}
				if (TargetFrame != open_frame && frame > 5) {
					ushort closed = (ushort)ModContent.TileType<Cargo_Elevator_Door>();
					ushort open = (ushort)ModContent.TileType<Cargo_Elevator_Door_Open>();
					Vector2? GetPushDirection(Rectangle hitbox) {
						if (!CollisionExt.OverlapsAnyTiles(hitbox, out List<Point> positions)) return null;
						int left = int.Sign(positions.Any(leftClosing.Contains).ToInt());
						int right = int.Sign(positions.Any(rightClosing.Contains).ToInt());
						for (int i = 0; i < positions.Count; i++) {
							Point pos = positions[i];
							switch ((leftClosing.Contains(pos), rightClosing.Contains(pos))) {
								case (true, true):
								float diff = hitbox.Center().X - (pos.X * 16 + 8);
								if (Math.Abs(diff) < 4) {
									left = 1;
									right = 1;
								} else if (diff > 0) {
									right = 1;
								} else {
									left = 1;
								}
								break;
								case (true, false):
								left = 1;
								break;
								case (false, true):
								right = 1;
								break;
							}
						}
						if (left == 0 && right == 0) return null;
						return Vector2.UnitX * (left - right);
					}
					if (!NetmodeActive.Server) {
						Player player = Main.LocalPlayer;
						if (!player.shimmering) {
							int tryCount = 0;
							while (++tryCount < 4 && GetPushDirection(player.Hitbox) is Vector2 push) {
								if (push == default) {
									if (frameCounter == 0) {
										player.Hurt(
											PlayerDeathReason.ByCustomReason(TextUtils.LanguageTree.Find("Mods.Origins.DeathMessage.Crushed").SelectFrom(player.name).ToNetworkText()),
											player.statLife,
											0,
											cooldownCounter: -2,
											dodgeable: false,
											knockback: 18,
											scalingArmorPenetration: 1
										);
									}
									break;
								}
								player.position += push * 4;
							}
						}
					}
					if (!NetmodeActive.MultiplayerClient) {
						foreach (NPC npc in Main.ActiveNPCs) {
							if (npc.noTileCollide || npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type]) continue;
							int tryCount = 0;
							while (++tryCount < 4 && GetPushDirection(npc.Hitbox) is Vector2 push) {
								if (push == default) {
									if (frameCounter == 0) {
										npc.SimpleStrikeNPC(npc.life, 0, true, 18, noPlayerInteraction: true);
									}
									break;
								} else {
									npc.position += push * 4;
									npc.netUpdate = true;
								}
							}
							/*if (npc.Hitbox.Intersects(hitbox)) {
								npc.SimpleStrikeNPC(npc.life / (frame + 1), 0, true, 18, noPlayerInteraction: true);
							}*/
						}
					}
				}
				leftClosing.Clear();
				rightClosing.Clear();
			}
		}
		public record class Cargo_Elevator_Door_Action(Point16 Position, bool Open) : SyncedAction {
			protected override bool ShouldPerform => (GetAnimation(Position).TargetFrame == Door_Animation.open_frame) != Open;
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
			protected override void Perform() => GetAnimation(Position).TargetFrame = Open ? Door_Animation.open_frame : Door_Animation.closed_frame;
		}
	}
}
