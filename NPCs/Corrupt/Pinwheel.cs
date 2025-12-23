using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Dusts;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Corrupt {
	public class Pinwheel : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 30, 30);
		public int AnimationFrames => 6;
		public int FrameDuration => 3;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 6;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			CorruptGlobalNPC.NPCTypes.Add(Type);
			AssimilationLoader.AddNPCAssimilation<Corrupt_Assimilation>(Type, 0.03f);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Fighter;
			AIType = NPCID.GoblinScout;
			NPC.lifeMax = 85;
			NPC.defense = 6;
			NPC.damage = 16;
			NPC.width = 38;
			NPC.height = 38;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0;
			NPC.value = 61;
			NPC.behindTiles = true;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!spawnInfo.Player.ZoneCorrupt || spawnInfo.Player.ZoneSkyHeight) return 0;
			return 0.07f * (spawnInfo.Player.position.Y / 16 > Main.rockLayer ? 2 : 1);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption
			);
		}
		public float DigTime = 60 * 0.5f;
		public override void OnSpawn(IEntitySource source) {
			NPC.aiStyle = NPCAIStyleID.Fighter; // don't remove me, for some reason aiStyle is "-1" without me
			NPC.localAI[0] = DigTime;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.RottenChunk, 3));
			npcLoot.Add(ItemDropRule.Common(ItemID.DemoniteOre, 25));
		}
		public bool wasCollideY = false;
		public override bool PreAI() {
			float bladeOffsetMax = NPC.frame.Size().Y / 16; // how much of the blade goes into the ground
			float bladeAllowedOut = bladeOffsetMax * 0.3f; // how deep the blade doesn't need to be in the ground before requiring to stop the burrow
			float acc = 1.2f;
			#region Movement
			if (NPC.collideY) {
				if (!wasCollideY || NPC.localAI[1] < bladeOffsetMax) {
					NPC.netUpdate = true;
					if (MathUtils.LinearSmoothing(ref NPC.localAI[1], bladeOffsetMax, acc / 4)) { // the value after "acc / " is the speed multiplier for burrowing the blade
						wasCollideY = true;
					} 
					if (NPC.localAI[1] <= -bladeAllowedOut) {
						NPC.rotation += ((acc * 3) / NPC.width) * NPC.direction;
						if (!NPC.collideX) NPC.velocity.X *= 0.1f * NPC.direction;
						int width = 60 - 40;
						for (int i = -width; i <= width; i += width / 16) {
							if (i % 4 != 0) continue;
							Vector2 pos = NPC.Bottom;
							pos.X += i;
							Tile tile = Framing.GetTileSafely(pos.ToTileCoordinates());
							if (tile.HasSolidTile() && Main.rand.NextBool(30)) Collision.HitTiles(tile.GetTilePosition().ToWorldCoordinates(), new(0, -2.5f), 0, 0);
						}
						return false;
					}
				}
				if (!NPC.collideX) NPC.velocity.X += acc * NPC.direction;
			} else {
				if (NPC.localAI[1] > -bladeOffsetMax) MathUtils.LinearSmoothing(ref NPC.localAI[1], -bladeOffsetMax, acc / 6); // the value after "acc / " is the speed multiplier for unburrowing the blade
				wasCollideY = NPC.localAI[1] > -bladeAllowedOut;
				NPC.netUpdate = true;
			}
			NPC.rotation += (1f / NPC.width) * NPC.velocity.X; // I love radians
			#endregion Movement
			return true;
		}
		public override void AI() {
			#region Dig up Dust
			if (Math.Abs(NPC.velocity.X) > 0.07f && NPC.collideY) {
				Vector2 pos = NPC.velocity.X < 0 ? NPC.BottomRight : NPC.BottomLeft;
				Tile tile = Framing.GetTileSafely((pos + Vector2.UnitY).ToTileCoordinates());

				if (tile.HasSolidTile()) {
					NPC.netUpdate = true;
					int extra = 0;
					Vector2 vel = new(0.6f * -NPC.velocity.X, -2.5f);
					if (NPC.localAI[0]-- <= 0) {
						extra = 2;
						int type = Lingering_Shadowflame.ShadeIDs[Main.rand.Next(Lingering_Shadowflame.ShadeIDs.Count)];
						int dmg = 10;
						if (Main.hardMode && Main.expertMode) {// enemies only scale with progress in expert mode and higher
							type = Lingering_Shadowflame.CursedIDs[Main.rand.Next(Lingering_Shadowflame.CursedIDs.Count)];
							dmg = 20;
						}
						Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, vel, type, dmg, 0);
						NPC.localAI[0] = DigTime;
					}
					if (Main.rand.NextBool(10) || extra > 0) {
						vel.Y *= 5;
						for (int i = 0; i < 1 + extra; i++) {
							Collision.HitTiles(tile.GetTilePosition().ToWorldCoordinates(), vel, 0, 0);
							Tile tile1 = Framing.GetTileSafely((NPC.Bottom + Vector2.UnitY).ToTileCoordinates());
							if (tile1.HasSolidTile()) Collision.HitTiles(tile1.GetTilePosition().ToWorldCoordinates(), vel, 0, 0);
						}
					}
				}
			}
			#endregion Dig up Dust
		}
		public override void SendExtraAI(BinaryWriter writer) {
			for (int i = 0; i < NPC.localAI.Length; i++) {
				writer.Write(NPC.localAI[i]);
			}
			writer.Write(wasCollideY);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			for (int i = 0; i < NPC.localAI.Length; i++) {
				NPC.localAI[i] = reader.ReadSingle();
			}
			wasCollideY = reader.ReadBoolean();
		}
		public override void FindFrame(int frameHeight) {
			NPC.DoFrames(Main.rand.Next(3, 6)); // meant to simulate random blinking times
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Vector2 offset = new(0, NPC.localAI[1]);
			if (NPC.IsABestiaryIconDummy) {
				offset.Y = NPC.frame.Size().Y / 16;
				drawColor = Color.White;
			}
			Main.EntitySpriteDraw(
				texture,
				NPC.Center + offset - screenPos,
				NPC.frame,
				drawColor,
				NPC.rotation,
				NPC.frame.Size() * 0.5f,
				NPC.scale,
				SpriteEffects.None
			);
			/*string debugText = ""; // debugging code
			if (NPC.IsABestiaryIconDummy) {
				debugText = $"Color: {drawColor}\n       Offset: {offset}\n            Rot: {NPC.rotation}";
			} else {
				debugText = $"AI: {NPC.aiStyle}, {AIType}, S/Dir: {NPC.spriteDirection}, {NPC.direction}\nOffset: {offset}\nVel: {NPC.velocity}, {Math.Abs(NPC.velocity.X)}, {Math.Abs(NPC.velocity.Y)}\nWasCollideY: {wasCollideY}\nTarget: {NPC.HasValidTarget}, {NPC.GetTargetData().Invalid}, {NPC.GetTargetData().Type}\nAI: {NPC.ai[0]}, {NPC.ai[1]}, {NPC.ai[2]}, {NPC.ai[3]}\nLAI: {NPC.localAI[0]}, {NPC.localAI[1]}, {NPC.localAI[2]}, {NPC.localAI[3]}";
			}
			OriginExtensions.DrawDebugTextAbove(spriteBatch, debugText, NPC.Top + offset - screenPos);*/
			//NPC.Hitbox.DrawDebugOutlineSprite(Color.White, -screenPos, false);
			return false;
		}
	}
	public abstract class Lingering_Shadowflame : ModProjectile {
		public abstract int BaseProj { get; }
		public override string Texture => $"Terraria/Images/Projectile_{BaseProj}";
		public virtual bool Hardmode => false;
		public static List<int> ShadeIDs = [];
		public static List<int> CursedIDs = [];
		public virtual int Flame => ModContent.BuffType<Shadefire_Debuff>();
		public virtual int FlameTime => 60;
		public override void SetStaticDefaults() {
			if (Hardmode) CursedIDs.Add(Type);
			else ShadeIDs.Add(Type);
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(BaseProj);
			Projectile.timeLeft = 60 * 3;
		}
		public override void AI() {
			Color color = Hardmode ? Color.LimeGreen : Color.MediumPurple;
			Lighting.AddLight(Projectile.Center, color.ToVector3() * 1.2f);
			Dust.NewDustDirect(Projectile.TopLeft, Projectile.width, Projectile.height, ModContent.DustType<Flare_Dust>(), 0, -3, 0, color);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Flame, FlameTime);
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(Flame, FlameTime);
		}
		public override bool PreDraw(ref Color lightColor) {
			lightColor = lightColor.MultiplyRGB(Hardmode ? Color.LimeGreen : Color.MediumPurple);
			return true;
		}
	}
	public class Lingering_Shadowflame1 : Lingering_Shadowflame {
		public override int BaseProj => ProjectileID.GreekFire1;
	}
	public class Lingering_Shadowflame2 : Lingering_Shadowflame {
		public override int BaseProj => ProjectileID.GreekFire2;
	}
	public class Lingering_Shadowflame3 : Lingering_Shadowflame {
		public override int BaseProj => ProjectileID.GreekFire3;
	}
	public class Lingering_Cursedflame1 : Lingering_Shadowflame {
		public override int BaseProj => ProjectileID.GreekFire1;
		public override bool Hardmode => true;
		public override int Flame => BuffID.CursedInferno;
	}
	public class Lingering_Cursedflame2 : Lingering_Cursedflame1 {
		public override int BaseProj => ProjectileID.GreekFire2;
	}
	public class Lingering_Cursedflame3 : Lingering_Cursedflame1 {
		public override int BaseProj => ProjectileID.GreekFire3;
	}
}
