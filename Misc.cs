using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using SysDraw = System.Drawing;
using Bitmap = System.Drawing.Bitmap;
using Microsoft.Xna.Framework.Audio;
using Terraria.ID;
using System.Runtime.CompilerServices;

namespace Origins {
    public class DrawAnimationManual : DrawAnimation {
	    public DrawAnimationManual(int frameCount) {
		    Frame = 0;
		    FrameCounter = 0;
		    FrameCount = frameCount;
	    }

	    public override void Update() {}

	    public override Rectangle GetFrame(Texture2D texture) {
		    return texture.Frame(FrameCount, 1, Frame, 0);
	    }
    }
    public interface IAnimatedItem {
        DrawAnimation Animation { get; }
    }
    public interface IElementalItem {
        ushort element { get; }
    }
    public static class Elements {
        public const ushort Fire = 1;
        public const ushort Earth = 2;
        public const ushort Acid = 4;
        public const ushort Ice = 4;
    }
    public static class OriginExtensions {
        public static Func<float, int, Vector2> drawPlayerItemPos;
        public static void PlaySound(string Name, Vector2 Position, float Volume = 1f, float PitchVariance = 1f, float? Pitch = null){
            if (Main.dedServ || string.IsNullOrEmpty(Name)) return;
            var sound = Origins.instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/" + Name);
            SoundEffectInstance SEI = Main.PlaySound(sound.WithVolume(Volume).WithPitchVariance(PitchVariance), Position);
            if(Pitch.HasValue)SEI.Pitch = Pitch.Value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 DrawPlayerItemPos(float gravdir, int itemtype) {
            return drawPlayerItemPos(gravdir, itemtype);
        }
        public static Vector2 GetLoSLength(Vector2 pos, Vector2 unit, int maxSteps, out int totalSteps) {
            return GetLoSLength(pos, new Point(1,1), unit, new Point(1,1), maxSteps, out totalSteps);
        }
        public static Vector2 GetLoSLength(Vector2 pos, Point size1, Vector2 unit, Point size2, int maxSteps, out int totalSteps) {
            Vector2 origin = pos;
            totalSteps = 0;
            while (Collision.CanHit(origin, size1.X, size1.Y, pos+unit, size2.X, size2.Y) && totalSteps<maxSteps) {
                totalSteps++;
                pos += unit;
            }
            return pos;
        }
        public static Vector2 Clamp(this Vector2 value, Vector2 min, Vector2 max) {
            return new Vector2(MathHelper.Clamp(value.X,min.X,max.X), MathHelper.Clamp(value.Y,min.Y,max.Y));
        }
        public static Vector2 Clamp(this Vector2 value, Rectangle area) {
            return new Vector2(MathHelper.Clamp(value.X,area.X,area.Right), MathHelper.Clamp(value.Y,area.Y,area.Bottom));
        }
        public static double AngleDif(double alpha, double beta) {
            double phi = Math.Abs(beta - alpha) % (Math.PI*2);       // This is either the distance or 360 - distance
            double distance = phi > Math.PI ? (Math.PI*2) - phi : phi;
            return distance;
        }
        public static void FixedUseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) {
            float xoffset = 10f;
            float yoffset = 24f;
            byte stage = 3;
            if (player.itemAnimation < player.itemAnimationMax * 0.333) {
                stage = 1;
				if (item.width >= 92) xoffset = 38f;
				else if (item.width >= 64) xoffset = 28f;
				else if (item.width >= 52) xoffset = 24f;
				else if (item.width > 32) xoffset = 14f;
			} else if (player.itemAnimation < player.itemAnimationMax * 0.666) {
                stage = 2;
				if (item.width >= 92) xoffset = 38f;
				else if (item.width >= 64) xoffset = 28f;
				else if (item.width >= 52) xoffset = 24f;
				else if (item.width > 32) xoffset = 18f;
				yoffset = 10f;
				if (item.height >= 64) yoffset = 14f;
				else if (item.height >= 52) yoffset = 12f;
				else if (item.height > 32) yoffset = 8f;
			} else {
				xoffset = 6f;
				if (item.width >= 92) xoffset = 38f;
				else if (item.width >= 64) xoffset = 28f;
				else if (item.width >= 52) xoffset = 24f;
				else if (item.width >= 48) xoffset = 18f;
				else if (item.width > 32) xoffset = 14f;
				yoffset = 10f;
				if (item.height >= 64) yoffset = 14f;
				else if (item.height >= 52) yoffset = 12f;
				else if (item.height > 32) yoffset = 8f;
			}
			hitbox.X = (int)(player.itemLocation.X = player.position.X + player.width * 0.5f + (item.width * 0.5f - xoffset) * player.direction);
			hitbox.Y = (int)(player.itemLocation.Y = player.position.Y + yoffset + player.mount.PlayerOffsetHitbox);
            hitbox.Width = (int)(item.width*item.scale);
            hitbox.Height = (int)(item.height*item.scale);
		    if (player.direction == -1) hitbox.X -= hitbox.Width;
		    if (player.gravDir == 1f) hitbox.Y -= hitbox.Height;
            switch(stage) {
                case 1:
				if (player.direction == -1) hitbox.X -= (int)(hitbox.Width * 1.4 - hitbox.Width);
				hitbox.Width = (int)(hitbox.Width * 1.4);
				hitbox.Y += (int)(hitbox.Height * 0.5 * player.gravDir);
				hitbox.Height = (int)(hitbox.Height * 1.1);
                break;
                case 3:
				if (player.direction == 1) hitbox.X -= (int)(hitbox.Width * 1.2);
				hitbox.Width *= 2;
				hitbox.Y -= (int)((hitbox.Height * 1.4 - hitbox.Height) * player.gravDir);
				hitbox.Height = (int)(hitbox.Height * 1.4);
                break;
            }
        }
        public static void DrawLine(this SpriteBatch spriteBatch, Color color, Vector2 start, Vector2 end, int thickness = 2) {
            Vector2 drawOrigin = new Vector2(1, 1);
            Rectangle drawRect = new Rectangle(
                (int)Math.Round(start.X - Main.screenPosition.X),
                (int)Math.Round(start.Y - Main.screenPosition.Y),
                (int)Math.Round((end - start).Length()),
                thickness);

            spriteBatch.Draw(Origins.instance.GetTexture("Projectiles/Pixel"), drawRect, null, color, (end - start).ToRotation(), Vector2.Zero, SpriteEffects.None, 0);
        }
        public static Bitmap ToBitmap(this Texture2D tex) {
            int[] data = new int[tex.Width * tex.Height];
            tex.GetData(0, new Rectangle(0,0,tex.Width,tex.Height), data, 0, tex.Width * tex.Height);
            Bitmap bitmap = new Bitmap(tex.Width, tex.Height);
            for (int x = 0; x < tex.Width; x++){
                for (int y = 0; y < tex.Height; y++){
                    SysDraw.Color bitmapColor = SysDraw.Color.FromArgb(data[y * tex.Width + x]);
                    bitmap.SetPixel(x, y, bitmapColor);
                }
            }
            return bitmap;
        }
    }
}
