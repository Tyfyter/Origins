using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Items.Tools.Wiring;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen;
public class Large_Auto_Assembler : ModTile, IAshenWireTile {
	const short dir_size = 2 * 4 * 18;
	public static int SpawnRate => 180;
	public override void Load() {
		new TileItem(this)
		.WithExtraStaticDefaults(item => ItemID.Sets.DisableAutomaticPlaceableDrop[item.type] = true)
		.RegisterItem();
		AddGrounded(1, 22, 108);
		AddGrounded(2, 54, 44);
		AddGrounded(3, 34, 78);
		AddGrounded(4, 32, 110);
		AddGrounded(5, 38, 108);
		AddGrounded(6, 30, 70);
		AddGrounded(7, 30, 70);
		AddHanging(1, 14, 110);
		AddHanging(2, 14, 98);
		AddHanging(3, 48, 104);
		AddHanging(4, 58, 90);
		AddHanging(5, 36, 86);
		int tmLeg = hanging.Count;
		AddHanging(6, 30, 96);
		Large_Auto_Assembler_Item_Hanging afLeg = new(typeof(Large_Auto_Assembler_Item_Hanging).GetDefaultTMLName("6_AF"), 30, 100);
		Mod.AddContent(afLeg);
		AprilFoolsAssetSwitcher<Large_Auto_Assembler_Item_Hanging>.Add(() => ref CollectionsMarshal.AsSpan(hanging)[tmLeg], afLeg);
	}

	public override void SetStaticDefaults() {
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLighted[Type] = true;
		TileID.Sets.DrawTileInSolidLayer[Type] = true;
		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
		TileObjectData.newTile.Width = 4;
		TileObjectData.newTile.SetHeight(7);
		TileObjectData.newTile.SetOriginBottomCenter();
		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
		TileObjectData.addAlternate(2);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.newAlternate.Origin = new(1, 0);
		TileObjectData.newAlternate.AnchorTop = new(AnchorType.SolidBottom, 0, 3);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
		TileObjectData.addAlternate(1);
		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.newAlternate.Origin = new(1, 0);
		TileObjectData.newAlternate.AnchorTop = new(AnchorType.SolidBottom, 0, 3);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
		TileObjectData.addAlternate(3);
		TileObjectData.addTile(Type);
		AddMapEntry(new Color(194, 69, 12), CreateMapEntryName());
		DustType = Ashen_Biome.DefaultTileDust;
	}
	public override void PlaceInWorld(int i, int j, Item item) {
		int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
		ModContent.GetInstance<Large_Auto_Assembler_TE>().AddTileEntity(TileObjectData.TopLeft(i, j) + new Point16(1, ((style & 1) == 0).Mul(6)), new());
	}
	public static int GetFacingDirection(int i, int j) => (Main.tile[i, j].TileFrameX < dir_size).ToDirectionInt();
	void IAshenWireTile.UpdatePowerState(int i, int j, bool powered) { }
	public override void HitWire(int i, int j) {
		if (!Ashen_Wire_Data.HittingAshenWires) {
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			for (int y = 0; y < data.Height; y++) {
				for (int x = 0; x < data.Width; x++) {
					Tile tile = Main.tile[left + x, top + y];
					if (tile.TileType != Type) continue;
					tile.TileFrameX += dir_size;
					tile.TileFrameX %= dir_size * 2;
				}
			}
		}
	}
	class Large_Auto_Assembler_TE : TESystem<Large_Auto_Assembler_TE.Data> {
		protected override bool IsValidTile(Tile tile) => tile.TileIsType<Large_Auto_Assembler>();
		public class Data() : ITileEntityData {
			int timer;
			public void Update(Point16 position) {
				if (NetmodeActive.MultiplayerClient) return;
				if (AshenWireTile.DefaultIsPowered(position.X, position.Y) && timer.CycleUp(SpawnRate)) {
					//if no players are within this range, skip spawning the item
					const float max_dist = 16 * 250;
					Vector2 worldCoords = position.ToWorldCoordinates();
					foreach (Player player in Main.ActivePlayers) {
						if (player.WithinRange(worldCoords, max_dist)) goto playerInRange;
					}
					return;
					playerInRange:
					int style = TileObjectData.GetTileStyle(Main.tile[position]);
					Point spawnPos = new(position.X * 16 + 8, position.Y * 16);
					int spawnType;
					if ((style & 1) != 0) {
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
					SoundEngine.PlaySound(Origins.Sounds.MetalBoxOpen.WithPitch(-1f).WithVolume(0.5f));
					SoundEngine.PlaySound(Origins.Sounds.MetalDoorOpen.WithPitch(0.3f).WithVolume(1f));
					SoundEngine.PlaySound(Origins.Sounds.SawEnd.WithPitch(-0.8f).WithVolume(0.15f));
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
	Large_Auto_Assembler_Item_Grounded AddGrounded(int num, int width, int height) {
		Large_Auto_Assembler_Item_Grounded instance = new(typeof(Large_Auto_Assembler_Item_Grounded).GetDefaultTMLName(num.ToString()), width, height);
		Mod.AddContent(instance);
		grounded.Add(instance);
		return instance;
	}
	Large_Auto_Assembler_Item_Hanging AddHanging(int num, int width, int height) {
		Large_Auto_Assembler_Item_Hanging instance = new(typeof(Large_Auto_Assembler_Item_Hanging).GetDefaultTMLName(num.ToString()), width, height);
		Mod.AddContent(instance);
		hanging.Add(instance);
		return instance;
	}
	static readonly List<Large_Auto_Assembler_Item_Grounded> grounded = [];
	static readonly List<Large_Auto_Assembler_Item_Hanging> hanging = [];
}
[Autoload(false)]
public class Large_Auto_Assembler_Item_Grounded(string texture, int width, int height) : Auto_Assembler_Item_Grounded(texture, width, height), IPlatformNPC {
	public static float SmoothingDist => 8;
	public override void PostAI() {
		if (NPC.ai[2] == 1) {
			NPC.ai[1] += 1f / (15 + NPC.width);
			NPC.scale = float.Pow(Math.Min(0.85f, 26f / NPC.width), NPC.ai[1]);
			if (NPC.ai[1] >= 1) NPC.active = false;
			return;
		}
		int type = ModContent.TileType<Large_Conveyor_Scooper>();
		Rectangle hitbox = NPC.Hitbox;
		int xDiff = (int)(NPC.position.X - NPC.oldPosition.X);
		hitbox.X += Math.Abs(xDiff);
		if (xDiff < 0) hitbox.X += Math.Abs(xDiff);
		hitbox.Width -= Math.Abs(xDiff) * 2;
		hitbox.Y -= (int)(2 * NPC.GravityMultiplier.Value);
		hitbox.Height -= 2;
		if (NPC.collideY) NPC.velocity.X *= 0.93f;
		int dir = 0;
		foreach (Point pos in hitbox.IterateTilesIn()) {
			Tile tile = Main.tile[pos];
			if (!tile.TileIsType(type)) return;
			int scoopDir = Large_Auto_Assembler.GetFacingDirection(pos.X, pos.Y);
			switch (dir * scoopDir) {
				case 0:
				dir = scoopDir;
				break;
				case -1:
				return;
			}
		}
		float center = TileObjectData.TopLeft(hitbox.Center().ToTileCoordinates16()).ToWorldCoordinates(0, 0).X + 32 - NPC.width * 0.5f;
		center += dir * 2;
		if ((NPC.position.X - (center - dir * SmoothingDist)) * dir >= 0) {
			NPC.position.X = center;
			NPC.ai[1] = 0;
			NPC.ai[2] = 1;
			NPC.direction = dir;
		}
	}
	public override void FindFrame(int frameHeight) {
		DrawOffsetY = -4;
		if (NPC.ai[2] == 1) return;
		float x = NPC.position.X;
		Collision.StepConveyorBelt(NPC, Math.Sign(NPC.GravityMultiplier.Value));
		if (NPC.collideY && NPC.position.X == x) {
			int assembler = ModContent.TileType<Large_Auto_Assembler>();
			int scooper = ModContent.TileType<Large_Conveyor_Scooper>();
			CollisionExtensions.TileOrder order = default;
			Point16 centerPos = NPC.Center.ToTileCoordinates16();
			if (Main.tile[centerPos].TileIsType(assembler) && Large_Auto_Assembler.GetFacingDirection(centerPos.X, centerPos.Y) > 0) order = CollisionExtensions.TileOrder.DescX;
			foreach (Point pos in NPC.Hitbox.IterateTilesIn(order)) {
				Tile tile = Main.tile[pos];
				if (tile.HasTile && (tile.TileType == assembler || tile.TileType == scooper)) {
					NPC.spriteDirection = Large_Auto_Assembler.GetFacingDirection(pos.X, pos.Y);
					Vector2 movement = Collision.TileCollision(
						NPC.position,
						new Vector2(NPC.spriteDirection * 2.5f, 0),
						NPC.width,
						NPC.height,
						false,
						false,
						Math.Sign(NPC.GravityMultiplier.Value)
					);
					NPC.position += movement;
					break;
				}
			}
		}
	}
	public override bool SpecialOnKill() {
		NPCLoader.OnKill(NPC);
		return true;
	}
	public override Color? GetAlpha(Color drawColor) {
		if (NPC.ai[2] == 1) {
			float alpha = 1 - NPC.ai[1];
			float brightness = alpha * alpha * alpha * alpha;
			return new((int)(brightness * drawColor.R), (int)(brightness * drawColor.G), (int)(brightness * drawColor.B), (int)(drawColor.A * alpha));
		}
		return drawColor;
	}
	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
		drawColor = GetAlpha(drawColor) ?? drawColor;
		Texture2D texture = TextureAssets.Npc[Type].Value;
		Vector2 originPos = new(0.5f, 0);
		if (NPC.GravityMultiplier.Value > 0) originPos.Y = 1;
		screenPos.X += float.Pow(SmoothingDist, 1 - NPC.ai[1]) * NPC.direction;
		spriteBatch.Draw(
			texture,
			NPC.position + NPC.Size * originPos - screenPos,
			NPC.frame,
			drawColor,
			0,
			NPC.frame.Size() * originPos,
			NPC.scale,
			NPC.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
		0);
		return false;
	}
}
[Autoload(false)]
public class Large_Auto_Assembler_Item_Hanging(string texture, int width, int height) : Large_Auto_Assembler_Item_Grounded(texture, width, height) {
	public override void SetStaticDefaults() {
		base.SetStaticDefaults();
		Main.npcFrameCount[Type] = 9;
	}
	public override void SetDefaults() {
		base.SetDefaults();
		NPC.lavaImmune = true;
	}
	public override void AI() {
		NPC.frame.Y = NPC.frame.Height * (int)(NPC.frameCounter * (Main.npcFrameCount[Type] - 1));
		OriginExtensions.LinearSmoothing(ref NPC.frameCounter, NPC.lavaWet.ToInt(), 1f / (NPC.lavaWet ? 30 : 270));
		if (NPC.ai[2] == 1) {
			NPC.GravityMultiplier = MultipliableFloat.One * -1;
			return;
		}
		switch ((int)NPC.ai[0]) {
			case 0:
			NPC.GravityMultiplier = MultipliableFloat.One * -1;
			if (NPC.collideY) NPC.ai[0] = 1;
			break;
			case 1:
			NPC.GravityMultiplier = MultipliableFloat.One * -1;
			if (!NPC.collideY) {
				Rectangle hitbox = NPC.Hitbox;
				hitbox.Height = 1;
				hitbox.Y -= hitbox.Height;
				foreach (Point item in hitbox.IterateTilesIn()) {
					if (Main.tile[item].HasFullSolidTile()) return;
				}
				NPC.ai[0] = 2;
				NPC.GravityMultiplier = MultipliableFloat.One;
			} else {
				NPC.ai[1] = 0;
				if (Math.Abs(NPC.position.X - NPC.oldPosition.X) < 0.1f) {
					NPC.ai[0] = 2;
					NPC.GravityMultiplier = MultipliableFloat.One;
				}
			}
			break;
			case 2:
			if ((NPC.collideY || NPC.velocity.Y == 0) && !NetmodeActive.MultiplayerClient) NPC.StrikeInstantKill();
			break;
		}
	}
	public override bool CanStandOnPlatform(Player player) => NPC.ai[0] == 2;
}