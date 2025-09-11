using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.Gores.NPCs {
	public class R_Effect_Blood1 : ModGore {
		public static List<int> GoreIDs { get; private set; } = [];
		AutoLoadingAsset<Texture2D> normalTexture;
		AutoLoadingAsset<Texture2D> afTexture;
		public override void SetStaticDefaults() {
			normalTexture = Texture;
			afTexture = Texture + "_AF";
			normalTexture.LoadAsset();
			afTexture.LoadAsset();
			GoreIDs.Add(Type);
		}
		public override void OnSpawn(Gore gore, IEntitySource source) {
			TextureAssets.Gore[Type] = OriginsModIntegrations.CheckAprilFools() ? afTexture : normalTexture;
		}
		public override void Unload() {
			GoreIDs = null;
		}
		public override Color? GetAlpha(Gore gore, Color lightColor) {
			return new Color(255, 255, 255, 0);
		}
		public override bool Update(Gore gore) {
			gore.velocity.Y += 0.2f;
			gore.rotation += gore.velocity.X * 0.05f;
			gore.rotation += gore.velocity.X * 0.1f;
			gore.alpha += 2 * GoreID.Sets.DisappearSpeedAlpha[Type];
			/*Dust.NewDustPerfect(gore.AABBRectangle.TopLeft(), 6, Vector2.Zero).noGravity = true;
			Dust.NewDustPerfect(gore.AABBRectangle.TopRight(), 6, Vector2.Zero).noGravity = true;
			Dust.NewDustPerfect(gore.AABBRectangle.BottomLeft(), 6, Vector2.Zero).noGravity = true;
			Dust.NewDustPerfect(gore.AABBRectangle.BottomRight(), 6, Vector2.Zero).noGravity = true;*/
			Vector2 newVelocity = Collision.TileCollision(gore.position, gore.velocity, gore.AABBRectangle.Width, gore.AABBRectangle.Height, true, true);
			if (gore.velocity != newVelocity) {
				Splatter(gore, newVelocity);
				//gore.velocity = Vector2.Zero;
			} else if (gore.alpha > 10) {
				for (int i = 0; i < Main.maxPlayers; i++) {
					Player player = Main.player[i];
					if (player.active && gore.AABBRectangle.Intersects(player.Hitbox)) {
						Splatter(gore, newVelocity, player);
						break;
					}
				}
				if (gore.active) {
					for (int i = 0; i < Main.maxNPCs; i++) {
						NPC npc = Main.npc[i];
						if (npc.active && gore.AABBRectangle.Intersects(npc.Hitbox)) {
							Splatter(gore, newVelocity, Main.npc[i]);
							break;
						}
					}
				}
			}
			gore.position += gore.velocity;
			return false;
		}
		public static void Splatter(Gore gore, Vector2 newVelocity, Entity collisionEntity = null) {
			if (OriginClientConfig.Instance.ExtraGooeyRivenGores) {
				Vector2 dustSpawnPosition = gore.position + newVelocity + gore.velocity.SafeNormalize(default) * 8;
				object dustCustomData = null;
				if (collisionEntity is Player player) {
					dustCustomData = (player, (dustSpawnPosition - player.Center) * new Vector2(player.direction, 1));
				} else if (collisionEntity is NPC npc) {
					dustCustomData = (npc, (dustSpawnPosition - npc.Center) * new Vector2(npc.direction, npc.directionY), npc.rotation);
				}
				for (int i = Main.rand.Next(3, 6); i-- > 0;) {
					Dust.NewDustPerfect(
						dustSpawnPosition,
						Main.rand.Next(Riven_Blood_Coating.IDs),
						gore.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0f, 1f) * 0.5f
					).customData = dustCustomData;
				}
			}
			if (gore.velocity.X != newVelocity.X) {
				gore.velocity.X *= -0.5f;
			}
			if (gore.velocity.Y != newVelocity.Y) {
				gore.velocity.Y *= -0.5f;
			}
			//gore.velocity *= 1.5f;
			for (int i = Main.rand.Next(6, 12); i-- > 0;) {
				Gore.NewGore(Entity.GetSource_None(), gore.position, gore.velocity.RotatedByRandom(1.5f) * Main.rand.NextFloat(0f, 1f), ModContent.GoreType<R_Effect_Blood1_Small>());
			}
			SoundEngine.PlaySound(SoundID.NPCDeath1.WithVolumeScale(0.5f), gore.position);
			gore.active = false;
		}
	}
	public class R_Effect_Blood2 : R_Effect_Blood1 { }
	public class R_Effect_Blood3 : R_Effect_Blood1 { }
	public class R_Effect_Blood1_Small : ModGore {
		public override string Texture => "Origins/Gores/NPCs/R_Effect_Blood1";
		AutoLoadingAsset<Texture2D> normalTexture;
		AutoLoadingAsset<Texture2D> afTexture;
		public override void SetStaticDefaults() {
			GoreID.Sets.DisappearSpeedAlpha[Type] = 5;
			ChildSafety.SafeGore[Type] = true;
			normalTexture = Texture;
			afTexture = Texture + "_AF";
			normalTexture.LoadAsset();
			afTexture.LoadAsset();
		}
		public override void OnSpawn(Gore gore, IEntitySource source) {
			TextureAssets.Gore[Type] = OriginsModIntegrations.CheckAprilFools() ? afTexture : normalTexture;
			gore.scale *= 0.5f;
		}
		public override bool Update(Gore gore) {
			gore.position += gore.velocity;
			gore.scale *= 0.96f;
			gore.scale -= 0.02f;
			gore.velocity *= 0.9f;
			//gore.alpha += 2 * GoreID.Sets.DisappearSpeedAlpha[Type];
			if (gore.scale < 0) gore.active = false;
			return false;
		}
		public override Color? GetAlpha(Gore gore, Color lightColor) {
			return new Color(255, 255, 255, 0);
		}
	}
	public class Riven_Blood_Coating : ModDust {
		internal static Stack<int> cachedDusts;
		internal static bool anyActive;
		public static ScreenTarget SlimeTarget { get; private set; }
		public static int[] IDs { get; private set; }
		public override string Texture => "Origins/Gores/NPCs/R_Effect_Blood1";
		public override bool Update(Dust dust) {
			Lighting.AddLight(dust.position, 0.0175f, 0.05f, 0.0375f);
			if (dust.customData is null) {
				dust.position += dust.velocity;
				dust.velocity *= 0.75f;
			} else if (dust.customData is bool) {
				dust.position += dust.velocity;
				dust.velocity.X *= 0.98f;
				dust.velocity.Y += 0.08f;
			} else if (dust.customData is (Player player, Vector2 playerOffset)) {
				dust.position = player.Center - playerOffset * new Vector2(player.direction, 1);
				dust.alpha += (int)(Math.Min(player.velocity.LengthSquared() / 64, 2.5f) + 0.5f);
				if (!player.Hitbox.Intersects(new Rectangle((int)dust.position.X, (int)dust.position.Y, 8, 8))) {
					dust.velocity = player.velocity;
					dust.customData = false;
				}
			} else if (dust.customData is (NPC npc, Vector2 npcOffset, float rotation)) {
				if (npc.rotation != rotation) {
					npcOffset = npcOffset.RotatedBy((npc.rotation - rotation) * npc.direction);
					dust.customData = (npc, npcOffset, npc.rotation);
				}
				dust.position = npc.Center - npcOffset * new Vector2(npc.direction, npc.directionY);
				dust.alpha += (int)(Math.Min(npc.velocity.LengthSquared() / 64, 1.5f) + 0.5f);
				if (!npc.Hitbox.Intersects(new Rectangle((int)dust.position.X, (int)dust.position.Y, 8, 8))) {
					dust.velocity = npc.velocity;
					dust.customData = false;
				}
			}
			if (Main.rand.NextBool(5)) dust.alpha++;
			return false;
		}
		public override void Load() {
			if (Main.dedServ || cachedDusts is not null) return;
			cachedDusts = new();
			SlimeTarget = new(
				MaskAura,
				() => {
					if (Main.gameMenu) anyActive = false;
					bool isActive = anyActive;
					anyActive = false;
					return isActive && Lighting.NotRetro;
				},
				0
			);
			On_Main.DrawInfernoRings += Main_DrawInfernoRings;
		}
		private void Main_DrawInfernoRings(On_Main.orig_DrawInfernoRings orig, Main self) {
			orig(self);
			if (Main.dedServ) return;
			if (Lighting.NotRetro) DrawAura(Main.spriteBatch);
		}
		public override void SetStaticDefaults() {
			IDs = [
				ModContent.DustType<Riven_Blood_Coating>(),
				ModContent.DustType<Riven_Blood_Coating2>(),
				ModContent.DustType<Riven_Blood_Coating3>()
			];
		}
		public override void Unload() {
			cachedDusts = null;
			SlimeTarget = null;
			IDs = null;
		}
		static void MaskAura(SpriteBatch spriteBatch) {
			if (Main.dedServ) return;
			while (cachedDusts.Count != 0) {
				Dust dust = Main.dust[cachedDusts.Pop()];
				float dustAlpha = dust.alpha / 255f;
				dustAlpha *= dustAlpha;
				if (dust.type >= DustID.Count && DustLoader.GetDust(dust.type) is ModDust modDust) spriteBatch.Draw(
					modDust.Texture2D.Value,
					(dust.position - Main.screenPosition),
					Color.White * (1 - dustAlpha)
				);
				
			}
		}
		static void DrawAura(SpriteBatch spriteBatch) {
			if (Main.dedServ) return;
			string biomeName = "Origins:RivenBloodCoating";
			if (OriginsModIntegrations.CheckAprilFools()) biomeName = "Origins:ChineseRivenBloodCoating";
			Main.LocalPlayer.ManageSpecialBiomeVisuals(biomeName, anyActive, Main.LocalPlayer.Center);
			if (anyActive) {
				ScreenShaderData shader = Filters.Scene[biomeName].GetShader();
				shader.UseImage(SlimeTarget.RenderTarget, 1, SamplerState.PointClamp);
				shader.UseIntensity(Riven_Hive.NormalGlowValue.GetValue() / 1.5f);
			}
		}
		public override bool PreDraw(Dust dust) {
			cachedDusts.Push(dust.dustIndex);
			anyActive = true;
			return false;
		}
	}
	public class Riven_Blood_Coating2 : Riven_Blood_Coating {
		public override string Texture => "Origins/Gores/NPCs/R_Effect_Blood2";
		public override void Load() {}
		public override void Unload() {}
	}
	public class Riven_Blood_Coating3 : Riven_Blood_Coating {
		public override string Texture => "Origins/Gores/NPCs/R_Effect_Blood3";
		public override void Load() { }
		public override void Unload() { }
	}
}
