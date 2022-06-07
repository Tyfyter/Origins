using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Chat;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Origins.NPCs;
using Terraria.Localization;
using Terraria.GameContent.Achievements;

namespace Origins.Projectiles.Misc {
    public class Defiled_Wastelands_Signal : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public Point Target {
            get => new Point((int)Projectile.localAI[0], (int)Projectile.localAI[1]);
            set {
                Projectile.localAI[0] = value.X;
                Projectile.localAI[1] = value.Y;
            }
        }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Nerve Impulse");
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
        public override void AI() {
            int x = (int)(Projectile.position.X / 16);
            int y = (int)(Projectile.position.Y / 16);
            WorldGen.TileFrame(x, y, true);
            Framing.WallFrame(x, y, true);
            Lighting.AddLight(Projectile.Center, 0.15f, 0.15f, 0.15f);
            if (x == Target.X && y == Target.Y) {
                Projectile.Kill();
            }
            if (Target == default) {
                List<Point> hearts = ModContent.GetInstance<OriginSystem>().Defiled_Hearts;
                Point current = default;
                float dist = float.PositiveInfinity;
                for (int i = hearts.Count - 1; i >= 0; i--) {
                    float cdist = Projectile.DistanceSQ(hearts[i].ToVector2() * 16);
                    if (cdist < dist) {
                        current = hearts[i];
                        dist = cdist;
                    }
                }
                Target = current;
            } else {
                Projectile.velocity = Projectile.DirectionTo(Target.ToVector2() * 16 + new Vector2(8)) * 8;
            }
        }
        public override void Kill(int timeLeft) {
            if ((int)Projectile.ai[0] == 1) {
				WorldGen.shadowOrbCount++;
				if (WorldGen.shadowOrbCount >= 3) {
					WorldGen.shadowOrbCount = 0;
					NPC.SpawnOnPlayer((int)Projectile.ai[1], NPCID.Frog);
				} else {
					LocalizedText localizedText = Lang.misc[10];
					if (WorldGen.shadowOrbCount == 2) {
						localizedText = Lang.misc[11];
					}
					if (Main.netMode == NetmodeID.SinglePlayer) {
						Main.NewText(localizedText.ToString(), 50, byte.MaxValue, 130);
					} else if (Main.netMode == NetmodeID.Server) {
						ChatHelper.BroadcastChatMessage(NetworkText.FromKey(localizedText.Key), new Color(50, 255, 130));
					}
				}
				AchievementsHelper.NotifyProgressionEvent(7);
            } else {
				Main.player[(int)Projectile.ai[1]].GetModPlayer<OriginPlayer>().rapidSpawnFrames = 5;
            }
        }
    }
}
