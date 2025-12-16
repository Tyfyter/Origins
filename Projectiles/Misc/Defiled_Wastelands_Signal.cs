using System.Collections.Generic;
using Terraria;
using Terraria.Chat;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Localization;
using Terraria.GameContent.Achievements;
using Terraria.Audio;
using Terraria.DataStructures;
using Origins.NPCs.Defiled.Boss;
using Origins.Tiles.Defiled;
using Origins.Tiles;
using System.Collections.ObjectModel;

namespace Origins.Projectiles.Misc {
	public class Defiled_Wastelands_Signal : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public Point Target {
			get => new((int)Projectile.localAI[0], (int)Projectile.localAI[1]);
			set {
				Projectile.localAI[0] = value.X;
				Projectile.localAI[1] = value.Y;
			}
		}
		
		public override void SetDefaults() {
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 600;
			Projectile.extraUpdates = 0;
			Projectile.width = Projectile.height = 2;
			Projectile.penetrate = -1;
			Projectile.light = 0;
			Projectile.tileCollide = false;
		}
		public override void OnSpawn(IEntitySource source) {
			base.OnSpawn(source);
		}
		public override void AI() {
			int x = (int)(Projectile.position.X / 16);
			int y = (int)(Projectile.position.Y / 16);
			WorldGen.TileFrame(x, y, true);
			Framing.WallFrame(x, y, true);
			Lighting.AddLight(Projectile.Center, 0.45f, 0.45f, 0.45f);
			Dust dust = Dust.NewDustDirect(Projectile.position, -0, 0, DustID.WhiteTorch, 0, 0, 125, new Color(80, 80, 80), 0.6f);
			dust.noGravity = true;
			if (x == Target.X && y == Target.Y) {
				Projectile.Kill();
			}
			if (Target == default) {
				ReadOnlyCollection<Point16> hearts = TESystem.GetLocations<Defiled_Heart_TE_System>();
				Point16 current = default;
				float dist = float.PositiveInfinity;
				for (int i = hearts.Count - 1; i >= 0; i--) {
					float cdist = Projectile.DistanceSQ(hearts[i].ToVector2() * 16);
					if (cdist < dist) {
						current = hearts[i];
						dist = cdist;
					}
				}
				Target = current.ToPoint();
				if (Target == default) Projectile.timeLeft -= 9;
			} else {
				Projectile.velocity = Projectile.DirectionTo(Target.ToVector2() * 16 + new Vector2(8)) * 8;
			}
		}
		public override void OnKill(int timeLeft) {
			if ((int)Projectile.ai[0] == 2) {
				WorldGen.shadowOrbCount = 2;
				Projectile.ai[0] = 1;
			}
			if ((int)Projectile.ai[0] == 1) {
				Color color = Color.Lerp(new Color(50, 255, 130), new Color(222, 222, 222), WorldGen.shadowOrbCount / 2f);

				WorldGen.shadowOrbCount++;
				string textKey = "Mods.Origins.Status_Messages.Defiled_Fissure_" + WorldGen.shadowOrbCount;

				if (Main.netMode == NetmodeID.SinglePlayer) {
					Main.NewText(NetworkText.FromKey(textKey).ToString(), color);
				} else if (Main.netMode == NetmodeID.Server) {
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey(textKey), color);
				}
				if (WorldGen.shadowOrbCount >= 3) {
					WorldGen.shadowOrbCount = 0;
					SoundEngine.PlaySound(Origins.Sounds.DefiledKill.WithPitch(0f).WithVolume(1f), Projectile.Center);
					Defiled_Amalgamation.spawnDA = true;
				}
			} else {
				Main.player[(int)Projectile.ai[1]].GetModPlayer<OriginPlayer>().rapidSpawnFrames = 5;
			}
		}
	}
}
