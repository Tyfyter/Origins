using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Reflection;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Core {
	public class AdvancedMiscShaderData(Asset<Effect> shader, string passName, params AdvancedMiscShaderData.Parameter[] parameters) : MiscShaderData(shader, passName) {
		public override void Apply(DrawData? drawData = null) {
			using (SkipShaderApply _ = new()) base.Apply(drawData);
			for (int i = 0; i < parameters.Length; i++) parameters[i].Apply(Shader.Parameters);
			((ShaderData)this).Apply();
		}
		public readonly struct Parameter(string name, Parameter.Value value) : IEquatable<Parameter> {
			public readonly string name = name;
			readonly Value value = value;
			public Parameter(string name, params bool[] value) : this(name, (Value)value) { }
			public Parameter(string name, params int[] value) : this(name, (Value)value) { }
			public Parameter(string name, params Matrix[] value) : this(name, (Value)value) { }
			public Parameter(string name, params Quaternion[] value) : this(name, (Value)value) { }
			public Parameter(string name, params float[] value) : this(name, (Value)value) { }
			public Parameter(string name, params Vector2[] value) : this(name, (Value)value) { }
			public Parameter(string name, params Vector3[] value) : this(name, (Value)value) { }
			public Parameter(string name, params Vector4[] value) : this(name, (Value)value) { }
			public void Apply(EffectParameterCollection parameters) => value.Apply(parameters[name]);
			public override int GetHashCode() => name.GetHashCode();
			bool IEquatable<Parameter>.Equals(Parameter other) => name == other.name;
			public override bool Equals(object obj) => obj is Parameter parameter && name == parameter.name;
			public static bool operator ==(Parameter left, Parameter right) => left.Equals(right);
			public static bool operator !=(Parameter left, Parameter right) => !(left == right);
			public readonly struct Value {
				object InnerValue { get; init; }
				public void Apply(EffectParameter parameter) {
					switch (InnerValue) {
						case bool value:
						parameter.SetValue(value);
						break;
						case bool[] value:
						parameter.SetValue(value);
						break;
						case int value:
						parameter.SetValue(value);
						break;
						case int[] value:
						parameter.SetValue(value);
						break;
						case Matrix value:
						parameter.SetValue(value);
						break;
						case Matrix[] value:
						parameter.SetValue(value);
						break;
						case Quaternion value:
						parameter.SetValue(value);
						break;
						case Quaternion[] value:
						parameter.SetValue(value);
						break;
						case float value:
						parameter.SetValue(value);
						break;
						case float[] value:
						parameter.SetValue(value);
						break;
						case string value:
						parameter.SetValue(value);
						break;
						case Texture value:
						parameter.SetValue(value);
						break;
						case Vector2 value:
						parameter.SetValue(value);
						break;
						case Vector2[] value:
						parameter.SetValue(value);
						break;
						case Vector3 value:
						parameter.SetValue(value);
						break;
						case Vector3[] value:
						parameter.SetValue(value);
						break;
						case Vector4 value:
						parameter.SetValue(value);
						break;
						case Vector4[] value:
						parameter.SetValue(value);
						break;
						default:
						throw new InvalidOperationException($"{InnerValue} ({InnerValue.GetType()}) is not a valid effect parameter state");
					}
				}
				public static implicit operator Value(bool value) => new() { InnerValue = value };
				public static implicit operator Value(bool[] value) => new() { InnerValue = value };
				public static implicit operator Value(int value) => new() { InnerValue = value };
				public static implicit operator Value(int[] value) => new() { InnerValue = value };
				public static implicit operator Value(Matrix value) => new() { InnerValue = value };
				public static implicit operator Value(Matrix[] value) => new() { InnerValue = value };
				public static implicit operator Value(Quaternion value) => new() { InnerValue = value };
				public static implicit operator Value(Quaternion[] value) => new() { InnerValue = value };
				public static implicit operator Value(float value) => new() { InnerValue = value };
				public static implicit operator Value(float[] value) => new() { InnerValue = value };
				public static implicit operator Value(string value) => new() { InnerValue = value };
				public static implicit operator Value(Texture value) => new() { InnerValue = value };
				public static implicit operator Value(Vector2 value) => new() { InnerValue = value };
				public static implicit operator Value(Vector2[] value) => new() { InnerValue = value };
				public static implicit operator Value(Vector3 value) => new() { InnerValue = value };
				public static implicit operator Value(Vector3[] value) => new() { InnerValue = value };
				public static implicit operator Value(Vector4 value) => new() { InnerValue = value };
				public static implicit operator Value(Vector4[] value) => new() { InnerValue = value };
			}
		}
	}
	internal struct SkipShaderApply : IDisposable {
		public SkipShaderApply() => Handler.active++;
		readonly void IDisposable.Dispose() => Handler.active.Cooldown();
		class Handler : ILoadable {
			public static int active;
			static void Handle(ILContext context) {
				ILCursor c = new(context);
				ILLabel skip = c.DefineLabel();
				c.EmitCall(((Delegate)IsActive).Method);
				c.EmitBrfalse(skip);
				c.EmitRet();
				c.MarkLabel(skip);
			}
			static bool IsActive() => active != 0;
			void ILoadable.Load(Mod mod) {
				Origins.DoILEdit(new ShaderData(default(Asset<Effect>), null).Apply, Handle);
			}
			void ILoadable.Unload() {}
		}
	}
	public static class ShaderExtensions {
		public static void Apply(this MiscShaderData shaderData, DrawData? drawData = null, params AdvancedMiscShaderData.Parameter[] parameters) {
			using (SkipShaderApply _ = new()) shaderData.Apply(drawData);
			for (int i = 0; i < parameters.Length; i++) parameters[i].Apply(shaderData.Shader.Parameters);
			((ShaderData)shaderData).Apply();
		}
	}
}
