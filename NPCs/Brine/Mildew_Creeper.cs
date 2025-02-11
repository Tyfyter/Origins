using CalamityMod.Items.Weapons.Magic;
using CalamityMod.NPCs.ExoMechs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.Items.Armor.Defiled;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons.Magic;
using Origins.Misc;
using Origins.NPCs.Defiled;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Misc.Physics;

namespace Origins.NPCs.Brine {
	public class Mildew_Creeper : Brine_Pool_NPC, IMeleeCollisionDataNPC {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = Vector2.UnitY * -16,
				PortraitPositionYOverride = -32
			};
		}
		public override void SetDefaults() {
			NPC.aiStyle = -1;
			NPC.lifeMax = 500;
			NPC.defense = 24;
			NPC.damage = 58;
			NPC.width = 26;
			NPC.height = 26;
			NPC.friendly = false;
			NPC.HitSound = SoundID.Item127;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 1f;
			NPC.value = 5000;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Brine_Pool>().Type
			];
		}
		// Mildew Creepers can and will attack anything
		public override bool CanHitNPC(NPC target) => true;
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			//npcLoot.Add(ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Mildew_Item>(), 1, 1, 7));
			//npcLoot.Add(ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Mildew_Incantation>(), 23));
			npcLoot.Add(ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Mildew_Heart>(), 40));
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Brine_Pool.SpawnRates.EnemyRate(spawnInfo, Brine_Pool.SpawnRates.Creeper);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				this.GetBestiaryFlavorText()
			]);
		}
		public void GetMeleeCollisionData(Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect, ref float knockbackMult) {
			if (npcRect.Intersects(victimHitbox)) return;
			if (chain?.links is null) return;
			npcRect = new(0, 0, 24, 24);
			damageMultiplier = 0.5f;
			for (int i = 0; i < chain.links.Length; i++) {
				npcRect.X = (int)(chain.links[i].position.X - 12);
				npcRect.Y = (int)(chain.links[i].position.Y - 12);
				if (npcRect.Intersects(victimHitbox)) return;
			}
		}
		public override bool CheckTargetLOS(Vector2 target) => true;
		Physics.Chain chain;
		public override void AI() {
			DoTargeting();
			if (chain is null) {
				Gravity[] gravity = [
					new ConstantGravity(Vector2.UnitY * -0.006f),
					new ConstantGravity(directions[(int)NPC.ai[3]] * -0.008f),
				];
				List<Chain.Link> links = [];
				Vector2 anchor = NPC.Center;
				anchor.Y += 8;
				const float spring = 0.5f;
				for (int i = 0; i < 15; i++) {
					links.Add(new(anchor, default, 16f, gravity, drag: 0.93f, spring: spring));
				}
				links.Add(new(anchor, default, 20f, [new NPCTargetGravity(this)], drag: 0.93f, spring: spring));
				chain = new Physics.Chain() {
					anchor = new WorldAnchorPoint(anchor),
					links = links.ToArray()
				};
			}
			if (NPC.justHit) {
				chain.links[^1].velocity = NPC.velocity;
			} else {
				NPC.velocity = chain.links[^1].velocity;
			}
			NPC.Center = chain.links[^1].position;
			chain.Update();
			NPC.Center = chain.links[^1].position;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (chain is null) {
				if (NPC.IsABestiaryIconDummy) {
					Gravity[] gravity = [
						new ConstantGravity(Vector2.UnitY * -0.01f)
					];
					List<Chain.Link> links = [];
					Vector2 anchor = NPC.Center;
					anchor.Y += 80;
					const int count = 5;
					const float spring = 0.5f;
					for (int i = 0; i < count; i++) {
						links.Add(new(anchor - Vector2.UnitY * 16 * i, default, 16f, gravity, drag: 0.93f, spring: spring));
					}
					links.Add(new(anchor - Vector2.UnitY * 16 * count, default, 20f, [new NPCTargetGravity(this)], drag: 0.93f, spring: spring));
					chain = new Physics.Chain() {
						anchor = new WorldAnchorPoint(anchor),
						links = links.ToArray()
					};
					NPC.Center = chain.links[^1].position;
				} else {
					return false;
				}
			}
			Color GetColor(Vector2 position) {
				if (NPC.IsABestiaryIconDummy) return Color.White;
				Color npcColor = Lighting.GetColor(position.ToTileCoordinates());
				NPCLoader.DrawEffects(NPC, ref npcColor);
				return NPC.GetNPCColorTintedByBuffs(npcColor);
			}
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				chain.anchor.WorldPosition - screenPos,
				new Rectangle(10, 34, 16, 24),
				GetColor(chain.anchor.WorldPosition),
				(chain.links[1].position - chain.anchor.WorldPosition).ToRotation() - MathHelper.PiOver2,
				new(8, 4),
				1,
				SpriteEffects.None
			);
			for (int i = 0; i < chain.links.Length - 1; i++) {
				Vector2 position = chain.links[i].position;
				Main.EntitySpriteDraw(
					texture,
					position - screenPos,
					new Rectangle(10, 34, 16, 24),
					GetColor(position),
					(chain.links[i + 1].position - position).ToRotation() - MathHelper.PiOver2,
					new(8, 4),
					1,
					SpriteEffects.None
				);
			}
			Main.EntitySpriteDraw(
				texture,
				chain.anchor.WorldPosition - screenPos,
				new Rectangle(0, 60, 36, 18),
				GetColor(chain.anchor.WorldPosition),
				directions[(int)NPC.ai[3]].ToRotation() - MathHelper.PiOver2,
				new(18, 9),
				1,
				SpriteEffects.None
			);
			drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
			Main.EntitySpriteDraw(
				texture,
				NPC.Center - screenPos,
				new Rectangle(0, 0, 36, 32),
				drawColor,
				(chain.links[^2].position - NPC.Center).ToRotation() - MathHelper.PiOver2,
				new(18, 16),
				1,
				SpriteEffects.None
			);
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Origins.instance.SpawnGoreByName(
					NPC.GetSource_Death(),
					chain.anchor.WorldPosition,
					Vector2.Zero,
					$"Gores/NPC/{nameof(Mildew_Creeper)}_Gore_3"
				);
				Origins.instance.SpawnGoreByName(
					NPC.GetSource_Death(),
					chain.anchor.WorldPosition,
					Vector2.Zero,
					$"Gores/NPC/{nameof(Mildew_Creeper)}_Gore_2"
				);
				for (int i = 0; i < chain.links.Length - 1; i++) {
					Origins.instance.SpawnGoreByName(
						NPC.GetSource_Death(),
						chain.links[i].position,
						chain.links[i].velocity,
						$"Gores/NPC/{nameof(Mildew_Creeper)}_Gore_2"
					);
				}
				Origins.instance.SpawnGoreByName(
					NPC.GetSource_Death(),
					NPC.Center,
					NPC.velocity,
					$"Gores/NPC/{nameof(Mildew_Creeper)}_Gore_1"
				);
			}
		}
		Vector2[] directions = [
			Vector2.UnitX,
			-Vector2.UnitX,
			Vector2.UnitY,
			-Vector2.UnitY
		];
		public override int SpawnNPC(int tileX, int tileY) {
			tileY = OriginGlobalNPC.GetAerialSpawnPosition(tileX, tileY, this, (spawnY) => CanSpawnInPosition(tileX, spawnY));
			if (tileY == -1) return Main.maxNPCs;

			const float offsetLen = 0;
			Vector2 basePos = new(tileX * 16 + 8, tileY * 16);
			float dist = 800;
			int directionIndex = 2;
			Vector2 bestPosition = basePos + directions[directionIndex] * (dist - offsetLen);
			for (int i = 0; i < directions.Length; i++) {
				float newDist = CollisionExtensions.Raycast(basePos, directions[i], dist);
				if (newDist < dist) {
					dist = newDist;
					bestPosition = basePos + directions[i] * (dist - offsetLen);
					directionIndex = i;
				}
			}
			return NPC.NewNPC(null, (int)bestPosition.X, (int)bestPosition.Y, NPC.type, ai3: directionIndex);
		}
		public class NPCTargetGravity(Mildew_Creeper npc) : Gravity {
			public override Vector2 Acceleration => (npc.TargetPos == default) ? Vector2.Zero : ((npc.TargetPos - npc.NPC.Center).SafeNormalize(default) * 0.3f);
		}
	}

}
