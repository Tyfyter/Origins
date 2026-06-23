using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Reflection;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Graphics.Shaders;

namespace Origins.Reflection {
	public class ShaderDataMethods : ReflectionLoader {
		public static FastFieldInfo<ShaderData, string> _passName;
		public static FastFieldInfo<ShaderData, Asset<Effect>> _asset;
		public static FastFieldInfo<ArmorShaderDataSet, List<ArmorShaderData>> _shaderData;
		public static FastFieldInfo<ArmorShaderDataSet, int> _shaderDataCount;
		static class Lock {
			public static readonly object _lock = new();
		}
		public static int RegisterArmorShader(ArmorShaderData shaderData) {
			lock (Lock._lock) {
				_shaderDataCount.SetValue(GameShaders.Armor, _shaderDataCount.GetValue(GameShaders.Armor) + 1);
				List<ArmorShaderData> armorShaderDatas = _shaderData.GetValue(GameShaders.Armor);
				lock (armorShaderDatas) armorShaderDatas.Add(shaderData);
				return _shaderDataCount.GetValue(GameShaders.Armor);
			}
		}
	}
}