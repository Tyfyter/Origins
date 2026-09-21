using Origins.Items.Mounts.Star_Soldier;
using Origins.Items.Tools.Wiring;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen;
public class Auto_Assembler : ModTile, IAshenWireTile {
	const short dir_size = 2 * 3 * 18;
	public static int SpawnRate => 180;
	public override void Load() {
		new TileItem(this)
		.WithExtraStaticDefaults(item => ItemID.Sets.DisableAutomaticPlaceableDrop[item.type] = true)
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
		TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
		TileObjectData.newTile.SetHeight(3);
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
		ModContent.GetInstance<Auto_Assembler_TE>().AddTileEntity(TileObjectData.TopLeft(i, j) + new Point16(1, 1 - ((style & 1) != 0).ToDirectionInt()), new());
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
	class Auto_Assembler_TE : TESystem<Auto_Assembler_TE.Data> {
		protected override bool IsValidTile(Tile tile) => tile.TileIsType<Auto_Assembler>();
		public class Data() : ITileEntityData {
			int timer;
			public void Update(Point16 position) {
				if (NetmodeActive.MultiplayerClient) return;
				if (AshenWireTile.DefaultIsPowered(position.X, position.Y) && timer.CycleUp(SpawnRate) && GetFacingDirection(position.X, position.Y) != 0) {
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
					SoundEngine.PlaySound(Origins.Sounds.MetalDoorOpen.WithPitch(0.5f).WithVolume(0.7f));
					SoundEngine.PlaySound(Origins.Sounds.SmallSawEnd.WithPitch(-0.6f).WithVolume(0.2f));
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
	static readonly List<Auto_Assembler_Item_Grounded> grounded = [];
	static readonly List<Auto_Assembler_Item_Hanging> hanging = [];
}
[Autoload(false)]
public class Auto_Assembler_Item_Grounded(string texture, int width, int height) : ModNPC, IPlatformNPC {
	public override string Texture => texture;
	public override string Name { get; } = texture.Split('/')[^1];
	protected override bool CloneNewInstances => true;
	public Vector2 PlatformOffset => default;
	public float PlatformWidth => NPC.width;
	public Vector2 OldPlatformPosition { get; set; }
	public override void SetStaticDefaults() {
		//NPCID.Sets.ConveyorBeltCollision[Type] = true;
		Star_Soldier_UI.Sets.OverrideDontDakeDamageTargeting[Type] = true;
	}
	public override void SetDefaults() {
		NPC.lifeMax = 10;
		NPC.width = width;
		NPC.height = height;
		NPC.defense = 10;
		NPC.knockBackResist = 0;
		NPC.CanBeReplacedByOtherNPCs = true;
		NPC.behindTiles = true;
		NPC.chaseable = false;
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
		if (NPC.collideY) NPC.velocity.X *= 0.93f;
		foreach (Point item in hitbox.IterateTilesIn()) {
			if (!Main.tile[item].TileIsType(type)) return;
		}
		NPC.active = false;
	}
	public override void FindFrame(int frameHeight) {
		DrawOffsetY = -4;
		float x = NPC.position.X;
		Collision.StepConveyorBelt(NPC, Math.Sign(NPC.GravityMultiplier.Value));
		if (NPC.collideY && NPC.position.X == x) {
			int assembler = ModContent.TileType<Auto_Assembler>();
			int scooper = ModContent.TileType<Conveyor_Scooper>();
			foreach (Point pos in NPC.Hitbox.IterateTilesIn()) {
				Tile tile = Main.tile[pos];
				if (tile.HasTile && (tile.TileType == assembler || tile.TileType == scooper)) {
					Vector2 movement = Collision.TileCollision(
						NPC.position,
						new Vector2(Auto_Assembler.GetFacingDirection(pos.X, pos.Y) * 2.5f, 0),
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
	public virtual bool CanStandOnPlatform(Player player) => true;
	public override void HitEffect(NPC.HitInfo hit) {
		if (NPC.life <= 0) {
			Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore1");
			Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore2");
			Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore3");
			Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore4");
			for (int i = 0; i < 7; i++) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
			}
		} else if (Main.rand.NextBool(5)) {
			Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
		}
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
	public override void HitEffect(NPC.HitInfo hit) {
		if (NPC.life <= 0) {
			Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore1");
			Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore2");
			Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore3");
			Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore4");
			for (int i = 0; i < 7; i++) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
			}
		} else if (Main.rand.NextBool(5)) {
			Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
		}
	}
}