using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Other.Consumables;
using Origins.Items.Weapons.Ranged;
using Origins.LootConditions;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using PegasusLib;
using Origins.Tiles.Other;
using Origins.Walls;
using Terraria.GameContent.Bestiary;
using System.Linq;
using Terraria.Localization;

namespace Origins.NPCs.Fiberglass {
	public class Enchanted_Fiberglass_Slime : ModNPC {
		readonly Color[] oldColor = new Color[10];
		readonly int[] oldFrame = new int[10];
		readonly bool[] oldGlass = new bool[10];
		public static void AddSpawns(SpawnPool pool) {
			Enchanted_Fiberglass_Slime[] slimes = pool.Mod.GetContent<Enchanted_Fiberglass_Slime>().ToArray();
			for (int i = 0; i < slimes.Length; i++) {
				pool.AddSpawn(slimes[i].Type, 1f / slimes.Length);
			}
		}
		public override LocalizedText DisplayName => Mod.GetLocalization($"{LocalizationCategory}.{nameof(Enchanted_Fiberglass_Slime)}.{nameof(DisplayName)}");
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 2;
			NPCID.Sets.TrailCacheLength[NPC.type] = 10;
			NPCID.Sets.TrailingMode[NPC.type] = 3;
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Slime;
			NPC.damage = 10;
			NPC.life = NPC.lifeMax = 57;
			NPC.defense = 10;
			NPC.width = 24;
			NPC.height = 18;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.knockBackResist = 0.4f;
			NPC.alpha = 100;
			NPC.value = 500f;
			AnimationType = NPCID.BlueSlime;
			SpawnModBiomes = [
				ModContent.GetInstance<Fiberglass_Undergrowth>().Type
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				new FlavorTextBestiaryInfoElement($"Mods.Origins.Bestiary.{nameof(Enchanted_Fiberglass_Slime)}")
			]);
		}
		public override void AI() {
			NPC.TargetClosest();
			NPC.hide = false;
			if (!NPC.Center.IsWithin(NPC.targetRect.Center(), 16 * 12)) {
				ushort wallType = (ushort)ModContent.WallType<Fiberglass_Wall>();
				Rectangle hitbox = NPC.Hitbox;
				hitbox.Inflate(2, 2);
				Rectangle tile = new(0, 0, 16, 16);
				foreach (Point pos in Collision.GetTilesIn(NPC.position + Vector2.One * 8, NPC.BottomRight - Vector2.One * 8)) {
					ushort currentWallType = Framing.GetTileSafely(pos).WallType;
					if (currentWallType != wallType) {
						if (currentWallType != WallID.None) return;
						tile.X = pos.X * 16;
						tile.Y = pos.Y * 16;
						if (hitbox.Intersects(tile)) return;
					}
				}
				NPC.hide = true;
			}
			NPC.chaseable = !NPC.hide;
		}
		public override bool? CanBeHitByItem(Player player, Item item) => NPC.hide ? false : null;
		public override bool? CanBeHitByProjectile(Projectile projectile) => NPC.hide ? false : null;
		public override bool CanBeHitByNPC(NPC attacker) => !NPC.hide;
		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			if (NPC.hide) npcHitbox = default;
			return false;
		}
		public override void PostAI() {
			for (int i = NPC.oldPos.Length - 1; i > 0; i--) {
				oldFrame[i] = oldFrame[i - 1];
				oldColor[i] = oldColor[i - 1] * 0.9f;
				oldGlass[i] = oldGlass[i - 1];
			}
			NPC.frame.Height = 72 / Main.npcFrameCount[NPC.type];
			oldFrame[0] = NPC.frame.Y / NPC.frame.Height;
			oldGlass[0] = NPC.hide;
		}
		public override void DrawBehind(int index) {
			if (NPC.hide) Main.instance.DrawCacheNPCsMoonMoon.Add(index);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Shaped_Glass>(), 25));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Fiberglass_Item>(), 10, 3, 8));
			npcLoot.Add(ItemDropRule.Common(ItemID.Vine, 7));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Fiberglass_Shard>(), 1, 1, 7));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (hit.Damage > NPC.life * 2f) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, $"Gores/NPCs/FG{(Main.rand.NextBool() ? 1 : 3)}_Gore");
			}
			if (NPC.life <= 0) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG1_Gore");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG3_Gore");
			} else if (hit.Damage > NPC.lifeMax * 0.5f) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, $"Gores/NPCs/FG{(Main.rand.NextBool() ? 1 : 3)}_Gore");
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Rectangle frame = NPC.frame;
			Color glassColor = Color.Lerp(Color.White, new(18, 36, 44), NPC.hide ? 0 : 0.3f);
			for (int i = NPC.oldPos.Length - 1; i > 0; i--) {
				if (oldGlass[i]) {
					frame.Y = frame.Height * oldFrame[i];
					Color color = oldColor[i].MultiplyRGBA(glassColor) * (1 - i / NPC.oldPos.Length) * 0.25f;
					spriteBatch.Draw(
						TextureAssets.Npc[NPC.type].Value,
						NPC.oldPos[i] + NPC.Size * new Vector2(0.5f, 1) + Vector2.UnitY * (NPC.gfxOffY + 4) - Main.screenPosition,
						frame,
						color,
						NPC.oldRot[i],
						frame.Size() * new Vector2(0.5f, 1),
						NPC.scale,
						oldFrame[i] != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
					0);
				}
			}
			oldColor[0] = drawColor * NPC.Opacity;
			return true;
		}
	}
	public class Enchanted_Fiberglass_Slime_1 : Enchanted_Fiberglass_Slime {
		public override void HitEffect(NPC.HitInfo hit) {
			base.HitEffect(hit);
			if (NPC.life <= 0) {
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(8 * NPC.direction, -6).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Fiberglass_Slime_Gore3")
				);
			}
		}
	}
	public class Enchanted_Fiberglass_Slime_2 : Enchanted_Fiberglass_Slime {
	}
	public class Enchanted_Fiberglass_Slime_3 : Enchanted_Fiberglass_Slime {
		public override void HitEffect(NPC.HitInfo hit) {
			base.HitEffect(hit);
			if (NPC.life <= 0) {
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(4 * NPC.direction, 0).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Fiberglass_Slime_Gore1")
				);
			}
		}
	}
}
