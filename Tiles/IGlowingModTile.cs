using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Tiles {
    public interface IGlowingModTile {
        Texture2D GlowTexture { get; }
        Color GlowColor { get; }
    }
}
