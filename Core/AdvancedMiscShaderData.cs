using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Core.Shaders;
using Origins.Reflection;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Core { 
	public class AdvancedMiscShaderData(Asset<Effect> shader, string passName, params Parameter[] parameters) : MiscShaderData(shader, passName), IAdvancedShaderData<AdvancedMiscShaderData> {
		Parameter[] IAdvancedShaderData<AdvancedMiscShaderData>.Parameters => parameters;
		public override void Apply(DrawData? drawData = null) {
			using (SkipShaderApply _ = new()) base.Apply(drawData);
			this.ApplyParameters();
			((ShaderData)this).Apply();
		}
		public AdvancedMiscShaderData Clone(params Parameter[] newParameters) => new(ShaderDataMethods._asset.GetValue(this), ShaderDataMethods._passName.GetValue(this), newParameters.Union(parameters).ToArray());
	}
	public class AdvancedArmorShaderData : ArmorShaderData, IAdvancedShaderData<AdvancedArmorShaderData> {
		private readonly Parameter[] parameters;
		readonly Task loadTask;
		public AdvancedArmorShaderData(Asset<Effect> shader, string passName, params Parameter[] parameters) : base(shader, passName) {
			if (shader is null) return;
			this.parameters = parameters;
			if (Main.dedServ) return;
			loadTask = Task.Run(shader.Wait).ContinueWith(_ => {
				for (int i = 0; i < parameters.Length; i++) parameters[i].Bind(this);
			});
		}
		public int ShaderID {
			get {
				if (field <= 0) field = ShaderDataMethods._shaderData.GetValue(GameShaders.Armor).IndexOf(this);
				if (field == -1 && !registering) throw new InvalidOperationException("Shader has not been registered");
				return field;
			}
			private set;
		}
		Parameter[] IAdvancedShaderData<AdvancedArmorShaderData>.Parameters {
			get {
				loadTask.Wait();
				return parameters;
			}
		}

		public override void Apply(Entity entity, DrawData? drawData = null) {
			loadTask.Wait();
			using (SkipShaderApply _ = new()) base.Apply(entity, drawData);
			this.ApplyParameters();
			Apply();
		}
		public AdvancedArmorShaderData Clone(params Parameter[] newParameters) => new(ShaderDataMethods._asset.GetValue(this), ShaderDataMethods._passName.GetValue(this), newParameters.Union(parameters).ToArray());
		[ThreadStatic]
		static bool registering;
		public AdvancedArmorShaderData Register() {
			using (registering.ScopedOverride(true)) {
				switch (ShaderID) {
					case 0:
					case -1:
					ShaderID = ShaderDataMethods.RegisterArmorShader(this);
					return Clone();

					default:
					return this;
				}
			}
		}
	}
	public interface IAdvancedShaderData<T> where T : IAdvancedShaderData<T> {
		Parameter[] Parameters { get; }
		public T Clone(params Parameter[] newParameters);
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
		public static void Apply(this MiscShaderData shaderData, DrawData? drawData = null, params Span<Parameter> parameters) {
			using (SkipShaderApply _ = new()) shaderData.Apply(drawData);
			for (int i = 0; i < parameters.Length; i++) parameters[i].Apply(shaderData.Shader.Parameters);
			((ShaderData)shaderData).Apply();
		}
		public static void Apply(this ArmorShaderData shaderData, Entity entity, DrawData? drawData = null, params Span<Parameter> parameters) {
			using (SkipShaderApply _ = new()) shaderData.Apply(entity, drawData);
			for (int i = 0; i < parameters.Length; i++) parameters[i].Apply(shaderData.Shader.Parameters);
			shaderData.Apply();
		}
		public static void ApplyParameters<T>(this T shaderData) where T : ShaderData, IAdvancedShaderData<T> {
			Parameter[] customParameters = shaderData.Parameters;
			EffectParameterCollection parameterCollection = shaderData.Shader.Parameters;
			for (int i = 0; i < customParameters.Length; i++) customParameters[i].Apply(parameterCollection);
		}
		public static void CreateParameter(this ShaderData shader, ref Parameter parameter, string name, Parameter.Val value) {
			if (parameter.Value == value && parameter.name == name) return;
			if (parameter.name == name) {
				parameter = parameter.For(shader, value);
				return;
			}
			if (shader.Shader.Parameters[name] is null) throw new KeyNotFoundException($"Shader {shader} does not contain parameter {name}");
			parameter = new Parameter(name, value);
			parameter.Bind(shader);
		}
		public static void BindParameters(this ShaderData shader, params Span<Parameter> value) {
			for (int i = 0; i < value.Length; i++) value[i].Bind(shader);
		}
	}
	namespace Shaders {
		public readonly struct Parameter(string name, Parameter.Val value) : IEquatable<Parameter> {
#pragma warning disable IDE1006 // Naming Styles
			public static Parameter uColor => new("uColor", Vector3.One);
			public static Parameter uSecondaryColor => new("uSecondaryColor", Vector3.One);
			public static Parameter uOpacity => new("uOpacity", 1);
			public static Parameter uSaturation => new("uSaturation", 1);

#pragma warning restore IDE1006 // Naming Styles

			public readonly string name = name;
			public Val Value { get; init; } = value;
			readonly Ref<ShaderData> shader = new();
			readonly Ref<EffectParameterCollection> collection = new();
			readonly Ref<EffectParameter> cachedParameter = new();
			public Parameter(string name, params bool[] value) : this(name, value.Length == 1 ? (Val)value[0] : (Val)value) { }
			public Parameter(string name, params int[] value) : this(name, value.Length == 1 ? (Val)value[0] : (Val)value) { }
			public Parameter(string name, params Matrix[] value) : this(name, value.Length == 1 ? (Val)value[0] : (Val)value) { }
			public Parameter(string name, params Quaternion[] value) : this(name, value.Length == 1 ? (Val)value[0] : (Val)value) { }
			public Parameter(string name, params float[] value) : this(name, value.Length == 1 ? (Val)value[0] : (Val)value) { }
			public Parameter(string name, params Vector2[] value) : this(name, value.Length == 1 ? (Val)value[0] : (Val)value) { }
			public Parameter(string name, params Vector3[] value) : this(name, value.Length == 1 ? (Val)value[0] : (Val)value) { }
			public Parameter(string name, params Vector4[] value) : this(name, value.Length == 1 ? (Val)value[0] : (Val)value) { }
			public bool Fits(EffectParameterCollection parameters) => collection.Value == parameters;
			public void Apply(EffectParameterCollection parameters) {
				if (shader.Value is not null && collection.Value != shader.Value.Shader.Parameters && parameters == shader.Value.Shader.Parameters) {
					collection.Value = null;
					cachedParameter.Value = null;
				}
				collection.Value ??= parameters;
				if (!Fits(parameters)) throw new ArgumentException("Cannot change parameter collection", nameof(parameters));
				Value.Apply(cachedParameter.Value ??= parameters[name]);
			}
			public Parameter For(ShaderData shader, Val value) {
				if (Main.dedServ) return this;
				if (Value == value && Fits(shader.Shader.Parameters)) return this;
				if (Fits(shader.Shader.Parameters)) {
					if (shader.Shader.Parameters[name] is null) throw new KeyNotFoundException($"Shader {shader} does not contain parameter {name}");
					return this with {
						Value = value
					};
				}
				Parameter param = new(name, value);
				param.Bind(shader);
				return param;
			}
			public void Bind(ShaderData shader) {
				if (Main.dedServ) return;
				if (collection.Value is null) {
					this.shader.Value = shader;
					collection.Value = shader.Shader.Parameters;
					cachedParameter.Value = shader.Shader.Parameters[name] ?? throw new KeyNotFoundException($"Shader {shader} does not contain parameter {name}");
				} else if (!Fits(shader.Shader.Parameters)) {
					throw new InvalidOperationException("Attempting to re-bind shader parameter");
				}
			}

			public override int GetHashCode() => name.GetHashCode();
			bool IEquatable<Parameter>.Equals(Parameter other) => name == other.name;
			public override bool Equals(object obj) => obj is Parameter parameter && name == parameter.name;
			public static bool operator ==(Parameter left, Parameter right) => left.Equals(right);
			public static bool operator !=(Parameter left, Parameter right) => !(left == right);
			public override string ToString() => $"{name}: {Value}";
			//Replace with union once C# gets them
			public readonly struct Val {
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
				public static implicit operator Val(bool value) => new() { InnerValue = value };
				public static implicit operator Val(bool[] value) => new() { InnerValue = value };
				public static implicit operator Val(int value) => new() { InnerValue = value };
				public static implicit operator Val(int[] value) => new() { InnerValue = value };
				public static implicit operator Val(Matrix value) => new() { InnerValue = value };
				public static implicit operator Val(Matrix[] value) => new() { InnerValue = value };
				public static implicit operator Val(Quaternion value) => new() { InnerValue = value };
				public static implicit operator Val(Quaternion[] value) => new() { InnerValue = value };
				public static implicit operator Val(float value) => new() { InnerValue = value };
				public static implicit operator Val(float[] value) => new() { InnerValue = value };
				public static implicit operator Val(string value) => new() { InnerValue = value };
				public static implicit operator Val(Texture value) => new() { InnerValue = value };
				public static implicit operator Val(Vector2 value) => new() { InnerValue = value };
				public static implicit operator Val(Vector2[] value) => new() { InnerValue = value };
				public static implicit operator Val(Vector3 value) => new() { InnerValue = value };
				public static implicit operator Val(Vector3[] value) => new() { InnerValue = value };
				public static implicit operator Val(Vector4 value) => new() { InnerValue = value };
				public static implicit operator Val(Vector4[] value) => new() { InnerValue = value };
				public override string ToString() => InnerValue.ToString();
				public override int GetHashCode() => InnerValue.GetHashCode() ^ 1;
				public override bool Equals(object obj) => obj is Val other && InnerValue.Equals(other.InnerValue);
				public static bool operator ==(Val left, Val right) => left.Equals(right);
				public static bool operator !=(Val left, Val right) => !(left == right);
			}
		}
	}
}