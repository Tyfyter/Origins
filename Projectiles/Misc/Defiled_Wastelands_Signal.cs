using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
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
            get => new Point((int)projectile.localAI[0], (int)projectile.localAI[1]);
            set {
                projectile.localAI[0] = value.X;
                projectile.localAI[1] = value.Y;
            }
        }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Nerve Impulse");
		}
        public override void SetDefaults() {
            projectile.aiStyle = 0;
            projectile.timeLeft = 600;
            projectile.extraUpdates = 0;
            projectile.width = projectile.height = 2;
            projectile.penetrate = -1;
            projectile.light = 0;
            projectile.tileCollide = false;
        }
        public override void AI() {
            int x = (int)(projectile.position.X / 16);
            int y = (int)(projectile.position.Y / 16);
            WorldGen.TileFrame(x, y, true);
            Framing.WallFrame(x, y, true);
            if (x == Target.X && y == Target.Y) {
                projectile.Kill();
            }
            if (Target == default) {
                List<Point> hearts = ModContent.GetInstance<World.OriginWorld>().Defiled_Hearts;
                Point current = default;
                float dist = float.PositiveInfinity;
                for (int i = hearts.Count - 1; i >= 0; i--) {
                    float cdist = projectile.DistanceSQ(hearts[i].ToVector2() * 16);
                    if (cdist < dist) {
                        current = hearts[i];
                        dist = cdist;
                    }
                }
                Target = current;
            } else {
                projectile.velocity = projectile.DirectionTo(Target.ToVector2() * 16 + new Vector2(8)) * 8;
            }
        }
        public override void Kill(int timeLeft) {
            if ((int)projectile.ai[0] == 1) {
				WorldGen.shadowOrbCount++;
				if (WorldGen.shadowOrbCount >= 3) {
					WorldGen.shadowOrbCount = 0;
					NPC.SpawnOnPlayer((int)projectile.ai[1], NPCID.Frog);
				} else {
					LocalizedText localizedText = Lang.misc[10];
					if (WorldGen.shadowOrbCount == 2) {
						localizedText = Lang.misc[11];
					}
					if (Main.netMode == NetmodeID.SinglePlayer) {
						Main.NewText(localizedText.ToString(), 50, byte.MaxValue, 130);
					} else if (Main.netMode == NetmodeID.Server) {
						NetMessage.BroadcastChatMessage(NetworkText.FromKey(localizedText.Key), new Color(50, 255, 130));
					}
				}
				AchievementsHelper.NotifyProgressionEvent(7);
            } else {
				Main.player[(int)projectile.ai[1]].GetModPlayer<OriginPlayer>().rapidSpawnFrames = 5;
            }
        }
    }
}
