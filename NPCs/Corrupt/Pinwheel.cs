using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using PegasusLib;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.NPCs.Corrupt {
	public class Pinwheel : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 30, 30);
		public int AnimationFrames => 6;
		public int FrameDuration => 3;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 6;
			CorruptGlobalNPC.NPCTypes.Add(Type);
			AssimilationLoader.AddNPCAssimilation<Corrupt_Assimilation>(Type, 0.03f);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 380;
			NPC.defense = 15;
			NPC.damage = 16;
			NPC.width = 30;
			NPC.height = 30;
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
			NPC.direction = 1;
			NPC.ai[0] = DigTime;
			//NPC.ai[2] = 60;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.RottenChunk, 3));
			npcLoot.Add(ItemDropRule.Common(ItemID.DemoniteOre, 14));
		}
		public bool wasCollideY = false;
		public override bool PreAI() {
			AIType = NPCID.DesertGhoul;
			float bladeOffsetMax = NPC.frame.Size().Y / 2 / 16;
			NPC.ai[2] = 0.3f;
			if (NPC.collideY) {
				NPC.ai[2] = 0.3f;
				if (!wasCollideY && NPC.ai[1] < bladeOffsetMax) if (MathUtils.LinearSmoothing(ref NPC.ai[1], bladeOffsetMax, NPC.ai[2])) wasCollideY = true;
				else return false;
				NPC.velocity.X += NPC.ai[2] * NPC.direction;
			} else {
				wasCollideY = false;
				NPC.ai[2] = 0.15f;
				if (NPC.ai[1] < bladeOffsetMax) MathUtils.LinearSmoothing(ref NPC.ai[1], 0, NPC.ai[2]);
			}
			NPC.rotation += (1f / NPC.width) * NPC.velocity.X; // I love radians
			return true;
		}
		public override void AI() {
			float bladeOffsetMax = NPC.frame.Size().Y / 2 / 16;
			NPCAimedTarget target = NPC.GetTargetData();
			#region Movement
			/*if (NPC.collideX) {
				if (NPC.ai[2]-- < 0) {
					NPC.ai[2] = 60;
					NPC.direction |= 1;
				}
			}
			if (!target.Invalid) NPC.direction = Math.Sign(Math.Abs(NPC.position.DirectionTo(target.Position).X));*/
			#endregion Movement
			#region Dig up Dust
			if (Math.Abs(NPC.velocity.X) > 0f && NPC.collideY) {
				Tile tile = Framing.GetTileSafely((NPC.Bottom + Vector2.UnitY).ToTileCoordinates());

				if (tile.HasSolidTile()) {
					int extra = 0;
					Vector2 vel = new(0.1f * -NPC.velocity.X, -2.5f);
					if (NPC.ai[0]-- <= 0) {
						extra = 20;
						int type = Lingering_Shadowflame.ShadeIDs[Main.rand.Next(Lingering_Shadowflame.ShadeIDs.Count)];
						if (Main.hardMode) type = Lingering_Shadowflame.CursedIDs[Main.rand.Next(Lingering_Shadowflame.CursedIDs.Count)];
						Vector2 pos = NPC.BottomLeft;
						if (NPC.velocity.X < 0) pos = NPC.BottomRight;

						//Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, vel, type, 10, 0);
					}
					for (int i = 0; i < 28 + extra; i++) {
						Collision.HitTiles(tile.GetTilePosition().ToVector2(), vel, 0, 0);
					}
				}
			}
			if (NPC.ai[0] <= 0f) NPC.ai[0] = DigTime;
			#endregion Dig up Dust
			//wasCollideY = NPC.collideY;
		}
		public override void FindFrame(int frameHeight) {
			NPC.DoFrames(Main.rand.Next(3,6));
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Vector2 offset = NPC.IsABestiaryIconDummy ? default : new(0, NPC.ai[1]);
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
			string debugText = $"Color: {drawColor}";
			if (!NPC.IsABestiaryIconDummy) debugText += $"\nVel: {NPC.velocity}\nWasCollideY: {wasCollideY}\nTarget: {NPC.GetTargetData().Invalid}, {NPC.GetTargetData().Type}\nAi: {NPC.ai[0]}, {NPC.ai[1]}, {NPC.ai[2]}, {NPC.ai[3]}";
			Vector2 origin = new(0, NPC.frame.Size().Y);
			if (NPC.IsABestiaryIconDummy) origin.Y /= 2;
			OriginExtensions.DrawDebugTextAbove(spriteBatch, debugText, NPC.Top + offset - screenPos, origin);
			NPC.Hitbox.DrawDebugOutlineSprite(Color.White, -screenPos, false);
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
			Projectile.friendly = false;
			Projectile.hostile = true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Flame, FlameTime);
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(Flame, FlameTime);
		}
		public override bool PreDraw(ref Color lightColor) {
			Projectile.friendly = false;
			Projectile.hostile = true;
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
