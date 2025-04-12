using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework.Graphics;
using Origins.NPCs.Brine;
using Origins.Tiles;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.Defiled {
	//Very important comment, do not delete: 4265737420706F6E79
	public class Defiled_Chrysalis : ModNPC, IDefiledEnemy {
		public override string Texture => "Terraria/Images/NPC_0";
		public float Mana { get; set; }
		static Asset<Texture2D>[] textures;
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			textures = new Asset<Texture2D>[3];
			for (int i = 0; i < textures.Length; i++) textures[i] = ModContent.Request<Texture2D>("Origins/Gores/NPCs/DF_Effect_Medium" + (i + 1));
		}
		public override void SetDefaults() {
			NPC.lifeMax = 1000;
			NPC.SuperArmor = true;
			NPC.noGravity = true;
			NPC.immortal = true;
			NPC.knockBackResist = 0;
			NPC.behindTiles = true;
		}
		NPC referenceNPC;
		ChunkData[] chunks;
		int animationTimeMax = 0;
		public override void OnSpawn(IEntitySource source) {
			if (NPC.ai[0] == 0) NPC.ai[0] = ModContent.NPCType<Defiled_Cyclops>();
			if (NPC.ai[1] != 0) {
				animationTimeMax = (int)NPC.ai[1];
				NPC.ai[1] = 0;
			}
			if (NPC.ai[2] == 0) NPC.ai[2] = Main.rand.Next();
			NPC.netUpdate = true;
		}
		public override void AI() {
			if (NPC.ai[0] <= 0) {
				NPC.active = false;
				return;
			}
			if (referenceNPC is null) {
				referenceNPC = new();
				referenceNPC.SetDefaults((int)NPC.ai[0]);
				if (animationTimeMax == 0) {
					animationTimeMax = (int)MathF.Pow(referenceNPC.width * referenceNPC.height, 0.333f) * 10;
				}
				Vector2 bottom = NPC.Bottom;
				NPC.Size = referenceNPC.Size;
				NPC.Bottom = bottom;
			}
			if (chunks is null) {
				UnifiedRandom rand = new((int)NPC.ai[2]);
				List<Vector2> vectors;
				if (referenceNPC.ModNPC is IDefiledEnemy defiledEnemy && defiledEnemy.GetCustomChrysalisShape(NPC) is (Rectangle startArea, Predicate<Vector2> customShape)) {
					vectors = Main.rand.PoissonDiskSampling(startArea, customShape, 12);
				} else {
					vectors = Main.rand.PoissonDiskSampling(NPC.Hitbox, 12);
				}
				chunks = new ChunkData[vectors.Count];
				for (int i = 0; i < chunks.Length; i++) {
					chunks[i] = new(rand, vectors[i], animationTimeMax);
				}
			}
			if (++NPC.ai[1] > animationTimeMax) {
				NPC.Transform(referenceNPC.netID);
				IEntitySource source = NPC.GetSource_FromThis();
				Vector2 from = new(NPC.position.X + NPC.width  * 0.5f, NPC.position.Y + NPC.height * 0.75f);
				for (int i = 0; i < chunks.Length; i++) {
					chunks[i].Explode(source, from);
				}
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			for (int i = 0; i < chunks.Length; i++) {
				chunks[i].Draw((int)NPC.ai[1]);
			}
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(animationTimeMax);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			animationTimeMax = reader.ReadInt32();
		}
		public readonly struct ChunkData {
			readonly Vector2 basePos;
			readonly Vector2 groundPos;
			readonly int animationTimeStart;
			readonly int animationTimeEnd;
			readonly int texture;
			public ChunkData(UnifiedRandom rand, Vector2 basePos, int animationTimeMax) {
				this.basePos = basePos;
				texture = rand.Next(textures.Length);
				int tries = 100;
				float dist = 0;
				const float max_dist = 16 * 10;
				while (tries > 0) {
					Vector2 direction = rand.NextFloat(MathHelper.TwoPi).ToRotationVector2();
					dist = CollisionExt.Raymarch(basePos, direction, max_dist);
					Vector2 endPoint = basePos + direction * dist;
					Tile tileSafely = Framing.GetTileSafely(endPoint + direction);
					if ((tileSafely.HasTile && (TileLoader.GetTile(tileSafely.TileType) is IDefiledTile || tries <= 15)) || --tries <= 0) {
						groundPos = endPoint + direction * 16;
						break;
					}
				}
				animationTimeStart = rand.Next(animationTimeMax - 20);
				animationTimeEnd = animationTimeStart + (int)((dist / max_dist + rand.NextFloat()) * 0.5f * 20);
			}
			public readonly void Draw(int progress) {
				Vector2 pos = Vector2.Lerp(groundPos, basePos, Utils.Remap(progress, animationTimeStart, animationTimeEnd, 0, 1));
				Main.spriteBatch.Draw(
					textures[texture].Value,
					pos - Main.screenPosition,
					null,
					Lighting.GetColor(pos.ToTileCoordinates()),
					0,
					textures[texture].Size() * 0.5f,
					1,
					SpriteEffects.None,
				0);
			}
			public readonly void Explode(IEntitySource source, Vector2 from) {
				Origins.instance.SpawnGoreByName(
					source,
					basePos,
					(basePos - from).SafeNormalize(default).RotatedByRandom(0.1f) * Main.rand.NextFloat(1, 4),
					"Gores/NPCs/DF_Effect_Medium" + (texture + 1)
				);
			}
		}
	}
}
