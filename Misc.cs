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
    public static class OriginExtensions {
        public static Func<float, int, Vector2> drawPlayerItemPos;
        public static void PlaySound(string Name, Vector2 Position, float Volume = 1f, float PitchVariance = 1f){
            if (Main.dedServ || string.IsNullOrEmpty(Name)) return;
            var sound = Origins.instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/" + Name);
            Main.PlaySound(sound.WithVolume(Volume).WithPitchVariance(PitchVariance), Position);
        }
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
    }
}
