using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using PegasusLib;
using PegasusLib.Reflection;

namespace Origins.Reflection {
	public class MusicMethods : ReflectionLoader {
		public static FastStaticFieldInfo<MusicLoader, Dictionary<string, int>> musicByPath;
		public static FastStaticFieldInfo<MusicLoader, Dictionary<string, string>> musicExtensions;
	}
}
