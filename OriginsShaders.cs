using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Core.Shaders;
using ReLogic.Content;
using System.Reflection;
using Terraria.ModLoader;

namespace Origins {
	public partial class Origins : Mod {
		public static class Shaders {
			public static AdvancedArmorShaderData Overbrighten { get; } = new(Request("Overbrighten"), "Overbrighten",
				Parameter.uColor
			);
			private static Asset<Effect> Request(string file) {
				if (ModContent.GetInstance<Origins>() is not Origins mod) return null;
				return mod.Assets.Request<Effect>("Effects/" + file);
			}
		}
		private void LoadShaders() {
			_ = Shaders.Overbrighten;
			//RuntimeHelpers.RunClassConstructor(typeof(Shaders).TypeHandle);
			ConstructorInfo typeInitializer = typeof(Shaders).TypeInitializer;
			if (typeInitializer != null) {
				object value = typeInitializer.GetType().GetProperty("Invoker", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(typeInitializer);
				FieldInfo field = value.GetType().GetField("_invocationFlags", BindingFlags.Instance | BindingFlags.NonPublic);
				object fieldObj = value;
				uint newFlagValue = 0u;
				uint oldFlagValue = (uint)field.GetValue(fieldObj);
				field.SetValue(fieldObj, newFlagValue);
				typeInitializer.Invoke(null, null);
				field.SetValue(fieldObj, oldFlagValue | newFlagValue);
			}
			foreach (FieldInfo field in typeof(Shaders).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
				if (field.FieldType.IsAssignableTo(typeof(AdvancedArmorShaderData))) ((AdvancedArmorShaderData)field.GetValue(null)).Register();
			}
		}
	}
}
