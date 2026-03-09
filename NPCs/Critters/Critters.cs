using Origins.Dev;
using Origins.Items.Other.Critters;
using Origins.Tiles.Other;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Critters {
	#region Base Classes
	public abstract class Critter_Mod_NPC<TCritter, TItem> : Critter_Mod_NPC where TCritter : ModItem where TItem : ModItem {
		public override int CritterItem => ModContent.ItemType<TCritter>();
		public override int ShimmeredNPC => 0;
		public override int ShimmeredItem => ModContent.ItemType<TItem>();
	}
	public abstract class Critter_Mod_NPC<TCritter> : Critter_Mod_NPC where TCritter : ModItem {
		public override int CritterItem => ModContent.ItemType<TCritter>();
	}
	public abstract class Critter_Mod_NPC : Glowing_Mod_NPC, IItemObtainabilityProvider {
		public abstract int CritterItem { get; }
		public virtual int ShimmeredNPC => NPCID.Shimmerfly;
		public virtual int ShimmeredItem => 0;
		public override void SetStaticDefaults() {
			Main.npcCatchable[Type] = true;
			NPCID.Sets.CountsAsCritter[Type] = true;
			if (ShimmeredItem > 0) NPCID.Sets.ShimmerTransformToItem[Type] = ShimmeredItem;
			if (ShimmeredNPC != 0) NPCID.Sets.ShimmerTransformToNPC[Type] = ShimmeredNPC;
		}
		public override void SetDefaults() {
			NPC.catchItem = CritterItem;
		}
		public IEnumerable<int> ProvideItemObtainability() => [CritterItem];
	}
	#endregion
	public class Amoeba_Buggy : Critter_Mod_NPC<Amoeba_Buggy_Item>, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 18, 12);
		public int AnimationFrames => 4;
		public int FrameDuration => 8;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = new(0, 22),
				PortraitPositionYOverride = 42
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.BlueDragonfly);
			base.SetDefaults();
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type
			];
		}
		public override void FindFrame(int frameHeight) {
			NPC.frameCounter += 0.75f;
			NPC.frameCounter += NPC.velocity.LengthSquared() / 16;
			NPC.spriteDirection = Math.Sign(NPC.velocity.X);
			if (NPC.frameCounter >= 7) {
				NPC.frameCounter = 0;
				NPC.frame.Height = 48 / Main.npcFrameCount[NPC.type];
				if ((NPC.frame.Y += NPC.frame.Height) / NPC.frame.Height >= Main.npcFrameCount[Type]) {
					NPC.frame.Y = 0;
				}
			}
		}
	}
	public class Bug : Critter_Mod_NPC<Bug_Item>, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 20, 12);
		public int AnimationFrames => 2;
		public int FrameDuration => 8;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 2;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.GoldWaterStrider);
			base.SetDefaults();
		}
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter >= 7) {
				NPC.frameCounter = 0;
				NPC.frame.Height = 24 / Main.npcFrameCount[NPC.type];
				if ((NPC.frame.Y += NPC.frame.Height) / NPC.frame.Height >= Main.npcFrameCount[Type]) {
					NPC.frame.Y = 0;
				}
				if (NPC.velocity.X != 0) NPC.spriteDirection = Math.Sign(NPC.velocity.X);
			}
		}
		public override void AI() {
			if (NPC.frameCounter == 0) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					if (i == NPC.whoAmI) continue;
					NPC other = Main.npc[i];
					if (other.active && NPC.Hitbox.Intersects(other.Hitbox)) {
						other.spriteDirection *= -1;
					}
				}
				for (int i = 0; i < Main.maxPlayers; i++) {
					Player target = Main.player[i];
					if (target.active && NPC.Hitbox.Intersects(target.Hitbox)) {
						target.gravDir *= -1;
						target.AddBuff(BuffID.Gravitation, 30);
					}
				}
			}
			if (Main.rand.NextBool(100)) {
				int tries = 0;
				float range = 16 * 10;
				Rectangle checkbox = NPC.Hitbox;
				while (++tries < 1000) {
					Vector2 pos = NPC.position + new Vector2(Main.rand.NextFloat(-range, range), Main.rand.NextFloat(-range, range));
					checkbox.X = (int)pos.X;
					checkbox.Y = (int)pos.Y;
					if (!checkbox.OverlapsAnyTiles()) {
						NPC.Teleport(pos, -1);
						break;
					}
				}
			}
		}
	}
	public class Cicada_3301 : Critter_Mod_NPC<Cicada_3301_Item>, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 20, 13);
		public int AnimationFrames => 2;
		public int FrameDuration => 8;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 2;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Buggy);
			base.SetDefaults();
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
		}
		public override void FindFrame(int frameHeight) {
			if (Main.rand.NextBool(350)) {
				SoundEngine.PlaySound(Origins.Sounds.Amalgamation.WithPitch(1).WithVolumeScale(0.5f), NPC.Center);
			}
			NPC.spriteDirection = Math.Sign(NPC.velocity.X);
			if (++NPC.frameCounter >= 7) {
				NPC.frameCounter = 0;
				NPC.frame.Height = 26 / Main.npcFrameCount[NPC.type];
				if ((NPC.frame.Y += NPC.frame.Height) / NPC.frame.Height >= Main.npcFrameCount[Type]) {
					NPC.frame.Y = 0;
				}
			}
		}
	}
	public class Chambersite_Bunny : Critter_Mod_NPC<Chambersite_Bunny_Item, Chambersite_Item>, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 48, 40);
		public int AnimationFrames => 7;
		public int FrameDuration => 8;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override string GlowTexturePath => $"Terraria/Images/Glow_{GlowMaskID.GemBunny}";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 7;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers {
				Velocity = 0.25f
			};
			NPCID.Sets.SpecificDebuffImmunity[Type] = NPCID.Sets.SpecificDebuffImmunity[NPCID.GemBunnyAmethyst];
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.GemBunnyAmethyst);
			base.SetDefaults();
			AnimationType = NPCID.GemBunnyAmethyst;
			AIType = NPCID.GemBunnyAmethyst;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns
			);
		}/* "borrowed" code for testing
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!Main.hardMode || !NPCExtensions.GemCritterSpawn(spawnInfo)) {
				return 0f;
			}
			return 0.045f;
		}*/
	}
	public class Chambersite_Squirrel: Critter_Mod_NPC<Chambersite_Squirrel_Item, Chambersite_Item>, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 50, 32);
		public int AnimationFrames => 6;
		public int FrameDuration => 8;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override string GlowTexturePath => $"Terraria/Images/Glow_{GlowMaskID.GemSquirrel}";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 6;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers {
				Velocity = 0.25f
			};
			NPCID.Sets.SpecificDebuffImmunity[Type] = NPCID.Sets.SpecificDebuffImmunity[NPCID.GemSquirrelAmethyst];
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.GemSquirrelAmethyst);
			base.SetDefaults();
			AnimationType = NPCID.GemSquirrelAmethyst;
			AIType = NPCID.GemSquirrelAmethyst;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns
			);
		}/* "borrowed" code for testing
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!Main.hardMode || !NPCExtensions.GemCritterSpawn(spawnInfo)) {
				return 0f;
			}
			return 0.045f;
		}*/
	}
	public class Peppered_Moth : Critter_Mod_NPC<Peppered_Moth_Item>, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 18, 12);
		public int AnimationFrames => 4;
		public int FrameDuration => 8;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = new(2, 3),
				PortraitPositionYOverride = 1
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.BlueDragonfly);
			base.SetDefaults();
			SpawnModBiomes = [
				ModContent.GetInstance<Ashen_Biome>().Type
			];
		}
		public override void FindFrame(int frameHeight) {
			NPC.frameCounter += 0.75f;
			NPC.frameCounter += NPC.velocity.LengthSquared() / 16;
			NPC.spriteDirection = Math.Sign(NPC.velocity.X);
			NPC.DoFrames(7);
		}
	}
}
