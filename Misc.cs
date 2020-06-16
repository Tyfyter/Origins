using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;

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
}
