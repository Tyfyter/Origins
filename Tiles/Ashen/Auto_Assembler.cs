using Microsoft.Xna.Framework.Graphics;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen;
public class Auto_Assembler : ModTile, IAshenWireTile {
	public static int SpawnRate => 180;
	public override void Load() {
		new TileItem(this)
		.WithExtraStaticDefaults(this.DropTileItem)
		.RegisterItem();
		AddGrounded(1, 14, 14);
		AddGrounded(2, 22, 10);
		AddGrounded(3, 22, 18);
		AddGrounded(4, 44, 22);
		AddGrounded(5, 30, 14);
		AddGrounded(6, 40, 20);
		AddGrounded(7, 20, 20);
		AddHanging(1, 38, 36);
		AddHanging(3, 42, 32);
		AddHanging(4, 38, 28);
		AddHanging(5, 50, 24);
	}

	public override void SetStaticDefaults() {
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLighted[Type] = true;
		TileID.Sets.DrawTileInSolidLayer[Type] = true;
		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
		TileObjectData.newTile.SetHeight(3);
		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.newAlternate.AnchorTop = new(AnchorType.SolidBottom, 0, 3);
		TileObjectData.addAlternate(1);
		TileObjectData.addTile(Type);
		AddMapEntry(new Color(194, 69, 12), CreateMapEntryName());
		DustType = Ashen_Biome.DefaultTileDust;
	}
	public override void PlaceInWorld(int i, int j, Item item) {
		int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
		ModContent.GetInstance<Auto_Assembler_TE>().AddTileEntity(TileObjectData.TopLeft(i, j) + new Point16(1, 1 - (style != 0).ToDirectionInt()), new());
	}
	static int GetFacingDirection(int i, int j) {
		Tile tile = Main.tile[i, j];
		int style = TileObjectData.GetTileStyle(tile);
		int dir = 0;
		i -= (tile.TileFrameX / 18) % 3;
		j -= tile.TileFrameY / 18;
		if (style != 0) {
			j -= 1;
		} else {
			j += 3;
		}
		for (int x = 0; x < 3; x++) {
			if (!WorldGen.InWorld(i + x, j)) continue;
			tile = Main.tile[i + x, j];
			if (!tile.HasTile) continue;
			dir += TileID.Sets.ConveyorDirection[tile.TileType];
		}
		return dir * (style == 0).ToDirectionInt();
	}
	bool isDrawingFlipped;
	public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
		isDrawingFlipped = GetFacingDirection(i, j) < 0;
		if (isDrawingFlipped) spriteEffects ^= SpriteEffects.FlipHorizontally;
	}
	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
		if (isDrawingFlipped) tileFrameX += (short)(36 - (tileFrameX % (3 * 18)) * 2);
	}
	void IAshenWireTile.UpdatePowerState(int i, int j, bool powered) { }
	void IAshenWireTile.HitWire(int i, int j) { }
	class Auto_Assembler_TE : TESystem<Auto_Assembler_TE.Data> {
		protected override bool IsValidTile(Tile tile) => tile.TileIsType<Auto_Assembler>();
		public class Data() : ITileEntityData {
			int timer;
			public void Update(Point16 position) {
				if (NetmodeActive.MultiplayerClient) return;
				if (AshenWireTile.DefaultIsPowered(position.X, position.Y) && timer.CycleUp(SpawnRate) && GetFacingDirection(position.X, position.Y) != 0) {
					TileID.Sets.DrawTileInSolidLayer[Main.tile[position].TileType] = true;
					int style = TileObjectData.GetTileStyle(Main.tile[position]);
					Point spawnPos = new(position.X * 16, position.Y * 16);
					int spawnType;
					if (style != 0) {
						spawnType = Main.rand.Next(hanging).Type;
						spawnPos.Y += ContentSamples.NpcsByNetId[spawnType].height;
					} else {
						spawnType = Main.rand.Next(grounded).Type;
						spawnPos.Y += 16;
					}
					NPC.NewNPC(
						new EntitySource_TileUpdate(position.X, position.Y),
						spawnPos.X,
						spawnPos.Y,
						spawnType
					);
				}
			}

			void ITileEntityData.SaveTE(TagCompound tag) {
				tag[nameof(timer)] = timer;
			}
			static Data ITileEntityData.LoadTE(TagCompound tag) {
				Data data = new();
				tag.TryGet(nameof(timer), out data.timer);
				return data;
			}
			void ITileEntityData.NetSend(BinaryWriter writer) { }
			static Data ITileEntityData.NetReceive(BinaryReader reader, Data existing) => existing ?? new Data();
			public bool IsDirty { get; set; }
		}
	}
	void AddGrounded(int num, int width, int height) {
		Auto_Assembler_Item_Grounded instance = new(typeof(Auto_Assembler_Item_Grounded).GetDefaultTMLName(num.ToString()), width, height);
		Mod.AddContent(instance);
		grounded.Add(instance);
	}
	void AddHanging(int num, int width, int height) {
		Auto_Assembler_Item_Hanging instance = new(typeof(Auto_Assembler_Item_Hanging).GetDefaultTMLName(num.ToString()), width, height);
		Mod.AddContent(instance);
		hanging.Add(instance);
	}
	static List<Auto_Assembler_Item_Grounded> grounded = [];
	static List<Auto_Assembler_Item_Hanging> hanging = [];
}
[Autoload(false)]
public class Auto_Assembler_Item_Grounded(string texture, int width, int height) : ModNPC {
	public override string Texture => texture;
	public override string Name { get; } = texture.Split('/')[^1];
	protected override bool CloneNewInstances => true;
	public override void SetStaticDefaults() {
		//NPCID.Sets.ConveyorBeltCollision[Type] = true;
	}
	public override void SetDefaults() {
		NPC.lifeMax = 10;
		NPC.width = width;
		NPC.height = height;
		NPC.defense = 10;
		NPC.CanBeReplacedByOtherNPCs = true;
		NPC.behindTiles = true;
		NPC.HitSound = SoundID.NPCHit4;
	}
	public override bool PreHoverInteract(bool mouseIntersects) => false;
	public override void PostAI() {
		int type = ModContent.TileType<Conveyor_Scooper>();
		Rectangle hitbox = NPC.Hitbox;
		int xDiff = (int)(NPC.position.X - NPC.oldPosition.X);
		hitbox.X += Math.Abs(xDiff);
		if (xDiff < 0) hitbox.X += Math.Abs(xDiff);
		hitbox.Width -= Math.Abs(xDiff) * 2;
		hitbox.Y -= (int)(2 * NPC.GravityMultiplier.Value);
		foreach (Point item in hitbox.IterateTilesIn()) {
			if (!Main.tile[item].TileIsType(type)) return;
		}
		NPC.active = false;
	}
	public override void FindFrame(int frameHeight) {
		DrawOffsetY = -4;
		Collision.StepConveyorBelt(NPC, NPC.GravityMultiplier.Value);
	}
}
[Autoload(false)]
public class Auto_Assembler_Item_Hanging(string texture, int width, int height) : Auto_Assembler_Item_Grounded(texture, width, height) {
	public override void AI() {
		switch ((int)NPC.ai[0]) {
			case 0:
			NPC.GravityMultiplier = MultipliableFloat.One * -1;
			if (NPC.collideY) NPC.ai[0] = 1;
			break;
			case 1:
			NPC.GravityMultiplier = MultipliableFloat.One * -1;
			if (!NPC.collideY) NPC.ai[0] = 2;
			break;
			case 2:
			if (NPC.collideY && !NetmodeActive.MultiplayerClient) NPC.StrikeInstantKill();
			break;
		}
	}
}