using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;
using System.Runtime.CompilerServices;
using System.Reflection;
using Terraria.Utilities;
using System.Collections;
using Terraria.ModLoader.IO;
using Tyfyter.Utils;
using Origins.Tiles;
using ReLogic.Content;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader.Exceptions;
using ReLogic.Reflection;
using Terraria.Localization;
using ReLogic.Graphics;
using System.Reflection.Emit;
using Terraria.GameContent.Personalities;
using Terraria.Map;
using Origins.Reflection;
using Terraria.GameContent.Bestiary;
using System.Diagnostics.CodeAnalysis;
using AltLibrary.Common.AltBiomes;
using Origins.Walls;
using Terraria.GameContent.Drawing;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Tiles.Banners;
using Terraria.ObjectData;
using static System.Net.Mime.MediaTypeNames;
using Terraria.ModLoader.Utilities;

namespace Origins {
	#region classes
	public class LinkedQueue<T> : ICollection<T> {
		public int Count {
			get { return _items.Count; }
		}

		public void Enqueue(T item) {
			_items.AddLast(item);
		}

		public T Dequeue() {
			if (_items.First is null)
				throw new InvalidOperationException("Queue empty.");

			var item = _items.First.Value;
			_items.RemoveFirst();

			return item;
		}
		//convenient shorthand that's incompatible with the vs debugger
		/*# region shorthand
		/// <summary>
		/// shorthand for Dequeue
		/// </summary>
		public T DQ => Dequeue();
		/// <summary>
		/// shorthand for Enqueue
		/// </summary>
		public void NQ(T value) => Enqueue(value);
		/// <summary>
		/// shorthand for Dequeue/Enqueue
		/// </summary>
		public T Q {
			get => Dequeue();
			set => Enqueue(value);
		}
		# endregion*/
		public bool Remove(T item) {
			return _items.Remove(item);
		}

		public void RemoveAt(int index) {
			_items.Remove(GetNodeEnumerator().Skip(index).First());
		}

		public IEnumerable<T> GetEnumerator() {
			while (Count > 0) yield return Dequeue();
		}

		public IEnumerable<LinkedListNode<T>> GetNodeEnumerator() {
			IEnumerator<LinkedListNode<T>> enumerator = new LLNodeEnumerator<T>(_items);
			yield return enumerator.Current;
			while (enumerator.MoveNext()) yield return enumerator.Current;
		}

		public T[] ToArray() {
			return [.. _items];
		}
		#region ICollection implementation
		[Obsolete("Use Enqueue")]
		public void Add(T item) {
			Enqueue(item);
		}

		public void Clear() {
			_items.Clear();
		}

		public bool Contains(T item) {
			return _items.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex) {
			_items.CopyTo(array, arrayIndex);
		}

		[Obsolete]
		IEnumerator<T> IEnumerable<T>.GetEnumerator() {
			throw new NotImplementedException();
		}

		[Obsolete]
		IEnumerator IEnumerable.GetEnumerator() {
			throw new NotImplementedException();
		}

		public bool IsReadOnly => false;
		#endregion
		private readonly LinkedList<T> _items = new();
	}
	public struct LLNodeEnumerator<T>(LinkedList<T> list) : IEnumerator<LinkedListNode<T>> {
		internal static FieldInfo LLVersion;
		private readonly LinkedList<T> list = list;
		private LinkedListNode<T> current = list.First;
		private readonly int version = list.GetVersion();

		public readonly LinkedListNode<T> Current => current;

		readonly object IEnumerator.Current => current;

		public readonly void Dispose() { }

		public bool MoveNext() {
			VersionCheck();
			current = current.Next;
			return current is not null;
		}

		public void Reset() {
			VersionCheck();
			current = list.First;
		}

		readonly void VersionCheck() {
			if (version != list.GetVersion()) {
				throw new InvalidOperationException("Collection has been modified");
			}
		}
	}
	/// <summary>
	/// Because it's a little more convenient than an extension method for every number type
	/// </summary>
	public struct Fraction(int numerator, int denominator) {
		public static Fraction Half => new(1, 2);
		public static Fraction Third => new(1, 3);
		public static Fraction Quarter => new(1, 4);
		public static Fraction Fifth => new(1, 5);
		public static Fraction Sixth => new(1, 6);
		public int numerator = numerator;
		public int denominator = denominator;
		public int N { readonly get => numerator; set => numerator = value; }
		public int D { readonly get => denominator; set => denominator = value; }

		public static Fraction operator +(Fraction f1, Fraction f2) {
			return new Fraction((f1.N * f2.D) + (f2.N * f1.D), f1.D * f2.D);
		}
		public static Fraction operator -(Fraction f1, Fraction f2) {
			return new Fraction((f1.N * f2.D) - (f2.N * f1.D), f1.D * f2.D);
		}
		public static Fraction operator *(Fraction f1, Fraction f2) {
			return new Fraction(f1.N * f2.N, f1.D * f2.D);
		}
		public static Fraction operator /(Fraction f1, Fraction f2) {
			return new Fraction(f1.N * f2.D, f1.D * f2.N);
		}
		public static Fraction operator *(Fraction frac, int i) {
			return new Fraction(frac.N * i, frac.D);
		}
		public static int operator *(int i, Fraction frac) {
			return (i * frac.N) / frac.D;
		}
		public static Fraction operator /(Fraction frac, int i) {
			return new Fraction(frac.N, frac.D * i);
		}
		public static int operator /(int i, Fraction frac) {
			return (i * frac.D) / frac.N;
		}
		public static explicit operator float(Fraction frac) {
			return frac.N / (float)frac.D;
		}
		public static explicit operator double(Fraction frac) {
			return frac.N / (double)frac.D;
		}
		public override readonly string ToString() {
			return N + "/" + D;
		}
	}
	public class FastFieldInfo<TParent, T> {
		public readonly FieldInfo field;
		Func<TParent, T> getter;
		Action<TParent, T> setter;
		public FastFieldInfo(string name, BindingFlags bindingFlags, bool init = false) {
			field = typeof(TParent).GetField(name, bindingFlags | BindingFlags.Instance);
			if (field is null) throw new InvalidOperationException($"No such instance field {name} exists");
			if (init) {
				getter = CreateGetter();
				setter = CreateSetter();
			}
		}
		public FastFieldInfo(FieldInfo field, bool init = false) {
			if (field.IsStatic) throw new InvalidOperationException($"field {field.Name} is static");
			this.field = field;
			if (init) {
				getter = CreateGetter();
				setter = CreateSetter();
			}
		}
		public T GetValue(TParent parent) {
			return (getter ??= CreateGetter())(parent);
		}
		public void SetValue(TParent parent, T value) {
			(setter ??= CreateSetter())(parent, value);
		}
		private Func<TParent, T> CreateGetter() {
			if (field.FieldType != typeof(T)) throw new InvalidOperationException($"type of {field.Name} does not match provided type {typeof(T)}");
			string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
			DynamicMethod getterMethod = new(methodName, typeof(T), [typeof(TParent)], true);
			ILGenerator gen = getterMethod.GetILGenerator();

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldfld, field);
			gen.Emit(OpCodes.Ret);

			return (Func<TParent, T>)getterMethod.CreateDelegate(typeof(Func<TParent, T>));
		}
		private Action<TParent, T> CreateSetter() {
			if (field.FieldType != typeof(T)) throw new InvalidOperationException($"type of {field.Name} does not match provided type {typeof(T)}");
			string methodName = field.ReflectedType.FullName + ".set_" + field.Name;
			DynamicMethod setterMethod = new(methodName, null, [typeof(TParent), typeof(T)], true);
			ILGenerator gen = setterMethod.GetILGenerator();

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Stfld, field);
			gen.Emit(OpCodes.Ret);
			
			return (Action<TParent, T>)setterMethod.CreateDelegate(typeof(Action<TParent, T>));
		}
	}
	public class FastStaticFieldInfo<TParent, T>(string name, BindingFlags bindingFlags, bool init = false) : FastStaticFieldInfo<T>(typeof(TParent), name, bindingFlags, init) {
	}
	public class FastStaticFieldInfo<T> {
		public readonly FieldInfo field;
		RefGetter refGetter;
		Func<T> getter;
		Action<T> setter;
		public FastStaticFieldInfo(Type type, string name, BindingFlags bindingFlags, bool init = false) {
			field = type.GetField(name, bindingFlags | BindingFlags.Static);
			if (field is null) throw new InvalidOperationException($"No such static field {name} exists");
			if (init) {
				getter = CreateGetter();
				setter = CreateSetter();
			}
		}
		public FastStaticFieldInfo(FieldInfo field, bool init = false) {
			if (!field.IsStatic) throw new InvalidOperationException($"field {field.Name} is not static");
			this.field = field;
			if (init) {
				getter = CreateGetter();
				setter = CreateSetter();
			}
		}
		public ref T Value => ref (refGetter ??= CreateRefGetter())();
		public T GetValue() {
			return (getter ??= CreateGetter())();
		}
		public void SetValue(T value) {
			(setter ??= CreateSetter())(value);
		}
		private Func<T> CreateGetter() {
			if (field.FieldType != typeof(T)) throw new InvalidOperationException($"type of {field.Name} does not match provided type {typeof(T)}");
			string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
			DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(T), Array.Empty<Type>(), true);
			ILGenerator gen = getterMethod.GetILGenerator();

			gen.Emit(OpCodes.Ldsfld, field);
			gen.Emit(OpCodes.Ret);

			return (Func<T>)getterMethod.CreateDelegate(typeof(Func<T>));
		}
		private Action<T> CreateSetter() {
			if (field.FieldType != typeof(T)) throw new InvalidOperationException($"type of {field.Name} does not match provided type {typeof(T)}");
			string methodName = field.ReflectedType.FullName + ".set_" + field.Name;
			DynamicMethod setterMethod = new DynamicMethod(methodName, null, [typeof(T)], true);
			ILGenerator gen = setterMethod.GetILGenerator();

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Stsfld, field);
			gen.Emit(OpCodes.Ret);

			return (Action<T>)setterMethod.CreateDelegate(typeof(Action<T>));
		}
		private delegate ref T RefGetter();
		private RefGetter CreateRefGetter() {
			if (field.FieldType != typeof(T)) throw new InvalidOperationException($"type of {field.Name} does not match provided type {typeof(T)}");
			string methodName = field.ReflectedType.FullName + ".getref_" + field.Name;
			DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(T).MakeByRefType(), [], true);
			ILGenerator gen = getterMethod.GetILGenerator();

			gen.Emit(OpCodes.Ldsflda, field);
			gen.Emit(OpCodes.Ret);

			return getterMethod.CreateDelegate<RefGetter>();
		}
		public static explicit operator T(FastStaticFieldInfo<T> fastFieldInfo) {
			return fastFieldInfo.GetValue();
		}
	}
	public class DrawAnimationManual : DrawAnimation {
		public DrawAnimationManual(int frameCount) {
			Frame = 0;
			FrameCounter = 0;
			FrameCount = frameCount;
			TicksPerFrame = -1;
		}

		public override void Update() { }

		public override Rectangle GetFrame(Texture2D texture, int frameCounterOverride = -1) {
			if (TicksPerFrame == -1) FrameCounter = 0;
			if (frameCounterOverride != -1) {
				return texture.Frame(FrameCount, 1, (frameCounterOverride / TicksPerFrame) % FrameCount, 0);
			}
			return texture.Frame(FrameCount, 1, Frame, 0);
		}
	}
	public class DrawAnimationDelegated : DrawAnimation {
		Func<Texture2D, Rectangle> frame;
		public DrawAnimationDelegated(Func<Texture2D, Rectangle> frameMethod) {
			Frame = 0;
			FrameCounter = 0;
			FrameCount = 1;
			TicksPerFrame = -1;
			frame = frameMethod;
		}
		public override void Update() { }
		public override Rectangle GetFrame(Texture2D texture, int frameCounterOverride = -1) {
			if (TicksPerFrame == -1) FrameCounter = 0;
			return frame(texture);
		}
	}
	public readonly struct AutoCastingAsset<T> where T : class {
		public bool IsLoaded => asset?.IsLoaded ?? false;
		public T Value => asset?.Value;
		public readonly Asset<T> asset;
		AutoCastingAsset(Asset<T> asset) {
			this.asset = asset;
		}
		public static implicit operator AutoCastingAsset<T>(Asset<T> asset) => new(asset);
		public static implicit operator T(AutoCastingAsset<T> asset) => asset.Value;
	}
	public struct AutoLoadingAsset<T> : IUnloadable where T : class {
		public readonly bool IsLoaded => asset.Value?.IsLoaded ?? false;
		public T Value {
			get {
				LoadAsset();
				return asset.Value?.Value;
			}
		}
		public bool Exists {
			get {
				LoadAsset();
				return exists;
			}
		}
		bool exists;
		bool triedLoading;
		string assetPath;
		Ref<Asset<T>> asset;
		AutoLoadingAsset(Asset<T> asset) {
			triedLoading = false;
			assetPath = "";
			this.asset = new(asset);
			exists = false;
			this.RegisterForUnload();
		}
		AutoLoadingAsset(string asset) {
			triedLoading = false;
			assetPath = asset;
			this.asset = new(null);
			exists = false;
			this.RegisterForUnload();
		}
		public void Unload() {
			assetPath = null;
			asset = null;
		}
		public void LoadAsset() {
			if (!triedLoading) {
				triedLoading = true;
				if (assetPath is null) {
					asset.Value = Asset<T>.Empty;
				} else {
					if (!Main.dedServ) {
						exists = ModContent.RequestIfExists(assetPath, out Asset<T> foundAsset);
						asset.Value = exists ? foundAsset : Asset<T>.Empty;
					} else {
						asset.Value = Asset<T>.Empty;
					}
				}
			}
		}
		public static implicit operator AutoLoadingAsset<T>(Asset<T> asset) => new(asset);
		public static implicit operator AutoLoadingAsset<T>(string asset) => new(asset);
		public static implicit operator T(AutoLoadingAsset<T> asset) => asset.Value;
		public static implicit operator AutoCastingAsset<T>(AutoLoadingAsset<T> asset) {
			asset.LoadAsset();
			return asset.asset.Value;
		}
		public static implicit operator Asset<T>(AutoLoadingAsset<T> asset) {
			asset.LoadAsset();
			return asset.asset.Value;
		}
	}
	public readonly struct UnorderedTuple<T>(T a, T b) : IEquatable<UnorderedTuple<T>> {
		readonly T a = a;
		readonly T b = b;

		public bool Equals(UnorderedTuple<T> other) {
			return other == this;
		}
		public override bool Equals(object obj) {
			return obj is UnorderedTuple<T> other && Equals(other);
		}
		public static bool operator ==(UnorderedTuple<T> a, UnorderedTuple<T> b) {
			return (EqualityComparer<T>.Default.Equals(a.a, b.a) && EqualityComparer<T>.Default.Equals(a.b, b.b))
				|| (EqualityComparer<T>.Default.Equals(a.a, b.b) && EqualityComparer<T>.Default.Equals(a.b, b.a));
		}
		public static bool operator !=(UnorderedTuple<T> a, UnorderedTuple<T> b) {
			return (!EqualityComparer<T>.Default.Equals(a.a, b.a) || !EqualityComparer<T>.Default.Equals(a.b, b.b))
				&& (!EqualityComparer<T>.Default.Equals(a.a, b.b) || !EqualityComparer<T>.Default.Equals(a.b, b.a));
		}
		public static implicit operator UnorderedTuple<T>((T a, T b) v) => new(v.a, v.b);
		public override int GetHashCode() {
			return a.GetHashCode() + b.GetHashCode();
		}
	}
	public class KeyedPlayerDeathReason : PlayerDeathReason {
		public string Key { get => SourceCustomReason; set => SourceCustomReason = value; }
	}
	public struct PlayerShaderSet {
		public int cHead;
		public int cBody;
		public int cLegs;
		public int cHandOn;
		public int cHandOff;
		public int cBack;
		public int cFront;
		public int cShoe;
		public int cWaist;
		public int cShield;
		public int cNeck;
		public int cFace;
		public int cFaceHead;
		public int cFaceFlower;
		public int cBalloon;
		public int cWings;
		public int cBalloonFront;
		public int cCarpet;
		public int cFloatingTube;
		public int cBackpack;
		public int cTail;
		public int cShieldFallback;
		public int cGrapple;
		public int cMount;
		public int cMinecart;
		public int cPet;
		public int cLight;
		public int cYorai;
		public int cPortableStool;
		public int cUnicornHorn;
		public int cAngelHalo;
		public int cBeard;
		public int cMinion;
		public int cLeinShampoo;
		public PlayerShaderSet(Player player) {
			cHead = player.cHead;
			cBody = player.cBody;
			cLegs = player.cLegs;
			cHandOn = player.cHandOn;
			cHandOff = player.cHandOff;
			cBack = player.cBack;
			cFront = player.cFront;
			cShoe = player.cShoe;
			cWaist = player.cWaist;
			cShield = player.cShield;
			cNeck = player.cNeck;
			cFace = player.cFace;
			cFaceHead = player.cFaceHead;
			cFaceFlower = player.cFaceFlower;
			cBalloon = player.cBalloon;
			cWings = player.cWings;
			cBalloonFront = player.cBalloonFront;
			cCarpet = player.cCarpet;
			cFloatingTube = player.cFloatingTube;
			cBackpack = player.cBackpack;
			cTail = player.cTail;
			cShieldFallback = player.cShieldFallback;
			cGrapple = player.cGrapple;
			cMount = player.cMount;
			cMinecart = player.cMinecart;
			cPet = player.cPet;
			cLight = player.cLight;
			cYorai = player.cYorai;
			cPortableStool = player.cPortableStool;
			cUnicornHorn = player.cUnicornHorn;
			cAngelHalo = player.cAngelHalo;
			cBeard = player.cBeard;
			cMinion = player.cMinion;
			cLeinShampoo = player.cLeinShampoo;
		}
		public PlayerShaderSet(int shader) {
			cHead = shader;
			cBody = shader;
			cLegs = shader;
			cHandOn = shader;
			cHandOff = shader;
			cBack = shader;
			cFront = shader;
			cShoe = shader;
			cWaist = shader;
			cShield = shader;
			cNeck = shader;
			cFace = shader;
			cFaceHead = shader;
			cFaceFlower = shader;
			cBalloon = shader;
			cWings = shader;
			cBalloonFront = shader;
			cCarpet = shader;
			cFloatingTube = shader;
			cBackpack = shader;
			cTail = shader;
			cShieldFallback = shader;
			cGrapple = shader;
			cMount = shader;
			cMinecart = shader;
			cPet = shader;
			cLight = shader;
			cYorai = shader;
			cPortableStool = shader;
			cUnicornHorn = shader;
			cAngelHalo = shader;
			cBeard = shader;
			cMinion = shader;
			cLeinShampoo = shader;
		}
		public void Apply(Player player) {
			player.cHead = cHead;
			player.cBody = cBody;
			player.cLegs = cLegs;
			player.cHandOn = cHandOn;
			player.cHandOff = cHandOff;
			player.cBack = cBack;
			player.cFront = cFront;
			player.cShoe = cShoe;
			player.cWaist = cWaist;
			player.cShield = cShield;
			player.cNeck = cNeck;
			player.cFace = cFace;
			player.cFaceHead = cFaceHead;
			player.cFaceFlower = cFaceFlower;
			player.cBalloon = cBalloon;
			player.cWings = cWings;
			player.cBalloonFront = cBalloonFront;
			player.cCarpet = cCarpet;
			player.cFloatingTube = cFloatingTube;
			player.cBackpack = cBackpack;
			player.cTail = cTail;
			player.cShieldFallback = cShieldFallback;
			player.cGrapple = cGrapple;
			player.cMount = cMount;
			player.cMinecart = cMinecart;
			player.cPet = cPet;
			player.cLight = cLight;
			player.cYorai = cYorai;
			player.cPortableStool = cPortableStool;
			player.cUnicornHorn = cUnicornHorn;
			player.cAngelHalo = cAngelHalo;
			player.cBeard = cBeard;
			player.cMinion = cMinion;
			player.cLeinShampoo = cLeinShampoo;
		}
	}
	public struct ItemSlotSet {
		public int headSlot;
		public int bodySlot;
		public int legSlot;
		public int beardSlot;
		public int backSlot;
		public int faceSlot;
		public int neckSlot;
		public int shieldSlot;
		public int wingSlot;
		public int waistSlot;
		public int shoeSlot;
		public int frontSlot;
		public int handOffSlot;
		public int handOnSlot;
		public int balloonSlot;
		public ItemSlotSet(Item item) {
			headSlot = item.headSlot;
			bodySlot = item.bodySlot;
			legSlot = item.legSlot;
			beardSlot = item.beardSlot;
			backSlot = item.backSlot;
			faceSlot = item.faceSlot;
			neckSlot = item.neckSlot;
			shieldSlot = item.shieldSlot;
			wingSlot = item.wingSlot;
			waistSlot = item.waistSlot;
			shoeSlot = item.shoeSlot;
			frontSlot = item.frontSlot;
			handOffSlot = item.handOffSlot;
			handOnSlot = item.handOnSlot;
			balloonSlot = item.balloonSlot;
		}
		public void Apply(Item item) {
			item.headSlot = headSlot;
			item.bodySlot = bodySlot;
			item.legSlot = legSlot;
			item.beardSlot = beardSlot;
			item.backSlot = backSlot;
			item.faceSlot = faceSlot;
			item.neckSlot = neckSlot;
			item.shieldSlot = shieldSlot;
			item.wingSlot = wingSlot;
			item.waistSlot = waistSlot;
			item.shoeSlot = shoeSlot;
			item.frontSlot = frontSlot;
			item.handOffSlot = handOffSlot;
			item.handOnSlot = handOnSlot;
			item.balloonSlot = balloonSlot;
		}
		public ItemSlotSet(Player player) {
			headSlot = player.head;
			bodySlot = player.body;
			legSlot = player.legs;
			beardSlot = player.beard;
			backSlot = player.back;
			faceSlot = player.face;
			neckSlot = player.neck;
			shieldSlot = player.shield;
			wingSlot = player.wings;
			waistSlot = player.waist;
			shoeSlot = player.shoe;
			frontSlot = player.front;
			handOffSlot = player.handoff;
			handOnSlot = player.handon;
			balloonSlot = player.balloon;
		}
		public void Apply(Player player) {
			player.head = headSlot;
			player.body = bodySlot;
			player.legs = legSlot;
			player.beard = beardSlot;
			player.back = backSlot;
			player.face = faceSlot;
			player.neck = neckSlot;
			player.shield = shieldSlot;
			player.wings = wingSlot;
			player.waist = waistSlot;
			player.shoe = shoeSlot;
			player.front = frontSlot;
			player.handoff = handOffSlot;
			player.handon = handOnSlot;
			player.balloon = balloonSlot;
		}
	}
	public class GeneratorCache<TKey, TValue>(Func<TKey, TValue> generator) : IReadOnlyDictionary<TKey, TValue> {
		readonly Dictionary<TKey, TValue> cache = new();
		readonly Func<TKey, TValue> generator = generator;

		public GeneratorCache(Func<TKey, TValue> generator, params TKey[] pregenerate) : this(generator) {
			for (int i = 0; i < pregenerate.Length; i++) {
				_ = this[pregenerate[i]];
			}
		}
		public TValue this[TKey key] {
			get {
				if (!cache.TryGetValue(key, out TValue value)) {
					value = generator(key);
					cache.Add(key, value);
				}
				return value;
			}
		}
		public IEnumerable<TKey> Keys => cache.Keys;
		public IEnumerable<TValue> Values => cache.Values;
		public int Count => cache.Count;
		public bool ContainsKey(TKey key) {
			return cache.ContainsKey(key);
		}
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
			return cache.GetEnumerator();
		}
		public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
			return cache.TryGetValue(key, out value);
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return cache.GetEnumerator();
		}
	}
	public record SpriteBatchState(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix transformMatrix = default);
	public abstract class AnimatedModItem : ModItem {
		public abstract DrawAnimation Animation { get; }
		public virtual Color? GetGlowmaskTint(Player player) => null;
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			Texture2D texture = TextureAssets.Item[Item.type].Value;
			spriteBatch.Draw(texture, Item.position - Main.screenPosition, Animation.GetFrame(texture), lightColor, 0f, default(Vector2), scale, SpriteEffects.None, 0f);
			return false;
		}
	}
	public interface ICustomDrawItem {
		void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin);
		bool DrawOverHand => false;
		bool BackHand => false;
	}
	public interface IAltTileCollideNPC {
		int CollisionType { get; }
	}
	public interface ICustomCollisionNPC {
		bool IsSandshark => false;
		void PreUpdateCollision();
		void PostUpdateCollision();
	}
	public interface IMeleeCollisionDataNPC {
		void GetMeleeCollisionData(Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect, ref float knockbackMult);
	}
	interface IComplexMineDamageTile {
		void MinePower(int i, int j, int minePower, ref int damage) {
		}
	}
	public interface IWhipProjectile {
		void GetWhipSettings(out float timeToFlyOut, out int segments, out float rangeMultiplier);
	}
	public static class MeleeCollisionNPCData {
		public static float knockbackMult = 1f;
	}
	public interface IElementalItem {
		ushort Element { get; }
	}
	public interface IElementalProjectile {
		ushort Element { get; }
	}
	public interface IGlowingWaterStyle {
		public void AddLight(ref Vector3 color, byte liquidAmount);
	}
	public interface IShadedProjectile {
		public int Shader { get; }
	}
	interface IIsExplodingProjectile {
		void Explode(int delay = 0) {
			if (this is ModProjectile modProjectile) {
				if (modProjectile.Projectile.timeLeft > delay) modProjectile.Projectile.timeLeft = delay;
			}
		}
		bool IsExploding();
	}
	interface ISelfDamageEffectProjectile {
		void OnSelfDamage(Player player, Player.HurtInfo info, double damageDealt);
	}
	interface ICustomRespawnArtifact {
		void Respawn();
	}
	public interface ICustomCanPlaceTile {
		public void CanPlace(Player self, Tile targetTile, Item sItem, ref int tileToCreate, ref int previewPlaceStyle, ref bool? overrideCanPlace, ref int? forcedRandom);
	}
	interface ILoadExtraTextures {
		void LoadTextures();
	}
	interface IItemObtainabilityProvider {
		IEnumerable<int> ProvideItemObtainability();
	}
	public interface IUnloadable {
		void Unload();
	}
	public static class Elements {
		public const ushort Fire = 1;
		public const ushort Earth = 2;
		public const ushort Acid = 4;
		public const ushort Ice = 8;
		public const ushort Fiberglass = 16;
	}
	public static class SlopeID {
		public const byte None = 0;
		public const byte BottomLeft = 1;
		public const byte BottomRight = 2;
		public const byte TopLeft = 3;
		public const byte TopRight = 4;
	}
	public static class NPCAIStyleID {
		///<summary>
		///Doesn't move.
		///</summary>
		public const int None = 0;
		///<summary>
		/// Hops in one direction, slides on slopes, floats in water, follows player if damaged or it's nighttime. Grasshoppers, on the other hand, flees from nearby players.
		///</summary>
		public const int Slime = 1;
		///<summary>
		/// Flies, follows player, bounces off walls in an arc.
		///</summary>
		public const int Demon_Eye = 2;
		///<summary>
		/// Will walk, jump over holes, follow player. It will try to line up vertically first. If it fails to reach its target, it will back up a bit, then re-attempt. It is the most common AI in Terraria.
		///</summary>
		public const int Fighter = 3;
		///<summary>
		/// Alternates between trying to stay above the player and summoning Servants of Cthulhu, and charging at the player occasionally. Spins when at low health, and begins exclusively charging at the player. Always looks at player.
		///</summary>
		public const int Eye_of_Cthulhu = 4;
		///<summary>
		/// Flies, looks at player, follows player.
		///</summary>
		public const int Flying = 5;
		///<summary>
		/// Burrows in ground (and/or air), passes through blocks, follows player. Truffle Worm instead flees from players.
		///</summary>
		public const int Worm = 6;
		///<summary>
		/// "Walks semi-randomly, jumps over holes. As of the 1.3 update, all Town NPCs talk to each other and the player, and each Town NPC has a different weapon to defend themselves when an enemy is nearby."
		///</summary>
		public const int Passive = 7;
		///<summary>
		/// Casts spells at player, stays stationary, warps after three casts, warps if falling. It stops casting spells if damaged, then warps if left alone.
		///</summary>
		public const int Caster = 8;
		///<summary>
		/// Travels in a direct line towards player, going through blocks. Used by casters.
		///</summary>
		public const int Spell = 9;
		///<summary>
		/// Tries to drift towards or around player, often stays a little bit out of reach once having touched the player.
		///</summary>
		public const int Cursed_Skull = 10;
		///<summary>
		/// Tries to stay above player, spins and moves towards player occasionally, enrages during the daytime.
		///</summary>
		public const int Head = 11;
		///<summary>
		/// Follows the Skeletron head, flails, waves, and attempts to damage player.
		///</summary>
		public const int Skeletron_Hand = 12;
		///<summary>
		/// Extends on a vine towards player, looks at player. Dies if not rooted to a block.
		///</summary>
		public const int Plant = 13;
		///<summary>
		/// Spasmodically flies towards player.
		///</summary>
		public const int Bat = 14;
		///<summary>
		/// Hops towards the player and releases Blue Slimes when damaged. Teleports to the player when the player is out of reach; releases Spiked Slimes when damaged in Expert Mode.
		///</summary>
		public const int King_Slime = 15;
		///<summary>
		/// Swims back and forth and moves towards the player if they are in water, except for Goldfish, Gold Goldfish and Pupfish.
		///</summary>
		public const int Swimming = 16;
		///<summary>
		/// Stands still until player gets within five blocks of AI or it is damaged. AI then acts similarly to the Flying AI.
		///</summary>
		public const int Vulture = 17;
		///<summary>
		/// Floats back and forth, swims towards player in small bursts if player is in water.
		///</summary>
		public const int Jellyfish = 18;
		///<summary>
		/// Looks at player, climbs overlapping blocks, shoots falling sand at nearby players.
		///</summary>
		public const int Antlion = 19;
		///<summary>
		/// Swings in a circle from a pivot point on a chain.
		///</summary>
		public const int Spike_Ball = 20;
		///<summary>
		/// Moves along walls, floors and closed doors.
		///</summary>
		public const int Blazing_Wheel = 21;
		///<summary>
		/// Similar to the Fighter AI, floats over the ground instead of jumping.
		///</summary>
		public const int Hovering = 22;
		///<summary>
		/// Doesn't adhere to gravity or tile collisions. Spins several times, then heads straight towards the player. Any damage cancels its attack and forces it to spin again.
		///</summary>
		public const int Flying_Weapon = 23;
		///<summary>
		/// Stands still until player gets nearby, then flies away. Avoids walls and obstacles and changes direction if one is in the way.
		///</summary>
		public const int Bird = 24;
		///<summary>
		/// Stands still until player approaches or attacks, then leaps towards them with varying heights.
		///</summary>
		public const int Mimic = 25;
		///<summary>
		/// Slowly gains speed while moving, jumps over obstacles.
		///</summary>
		public const int Unicorn = 26;
		///<summary>
		/// Wall of Flesh Mouth with Wall image attached. Traverses the world horizontally. Also spawns with two Wall of Flesh Eyes which share its health.
		///</summary>
		public const int Wall_of_Flesh = 27;
		///<summary>
		/// Bound to an entity, watches player, and shoots projectiles. The more damaged it is, the more it shoots.
		///</summary>
		public const int Wall_of_Flesh_Eye = 28;
		///<summary>
		/// Similar to the Plant AI, but attached to an entity.
		///</summary>
		public const int The_Hungry = 29;
		///<summary>
		/// Alternates between attempting to stay diagonally above player while shooting projectiles slowly, and attempting to stay beside player and shooting projectiles very rapidly.
		///</summary>
		public const int Retinazer = 30;
		///<summary>
		/// Alternates between shooting projectiles and staying beside player, and charging toward player.
		///</summary>
		public const int Spazmatism = 31;
		///<summary>
		/// Same as Head AI.
		///</summary>
		public const int Skeletron_Prime_Head = 32;
		///<summary>
		/// Occasionally charges at the player, heads directly towards player when enraged.
		///</summary>
		public const int Prime_Saw = 33;
		///<summary>
		/// Occasionally charges at the player, rapidly charges when enraged.
		///</summary>
		public const int Prime_Vice = 34;
		///<summary>
		/// Fires bombs upwards, aims directly at player when enraged.
		///</summary>
		public const int Prime_Cannon = 35;
		///<summary>
		/// Fires projectiles at player, shoots very rapidly when enraged.
		///</summary>
		public const int Prime_Laser = 36;
		///<summary>
		/// Similar to Worm AI except it will shoot projectiles from the body and tail and is unable to "fly" in air. It will also release Probes.
		///</summary>
		public const int The_Destroyer = 37;
		///<summary>
		/// Jumps and runs towards the player, similar to Fighter AI.
		///</summary>
		public const int Snowman = 38;
		///<summary>
		/// Crawls a bit, then leaps towards player. Will indefinitely leap towards the player in water.
		///</summary>
		public const int Tortoise = 39;
		///<summary>
		/// Capable of climbing background walls as well as through platforms. Used only when crawling on a wall. Without a wall listed enemies seamlessly transform into their walking variants, which use Fighter AI.
		///</summary>
		public const int Spider = 40;
		///<summary>
		/// Jumps high and attempts to land on the player. It can strafe to the sides in midair.
		///</summary>
		public const int Herpling = 41;
		///<summary>
		/// Turns into a Nymph when a player gets too close.
		///</summary>
		public const int Lost_Girl = 42;
		///<summary>
		/// Alternates between attempting to stay above player while firing projectiles downwards or spawning bees, and charging back and forth very rapidly.
		///</summary>
		public const int Queen_Bee = 43;
		///<summary>
		/// Flies straight towards player.
		///</summary>
		public const int Flying_Fish = 44;
		///<summary>
		/// Jumps towards player every few seconds, shoots lasers.
		///</summary>
		public const int Golem_Body = 45;
		///<summary>
		/// Bound to an entity.
		///</summary>
		public const int Golem_Head = 46;
		///<summary>
		/// Flies towards player, returns when hit.
		///</summary>
		public const int Golem_Fist = 47;
		///<summary>
		/// Attempts to fly back and forth, shoots projectiles at player.
		///</summary>
		public const int Free_Golem_Head = 48;
		///<summary>
		/// Attempts to stay directly above the player, and fires projectiles downwards.
		///</summary>
		public const int Angry_Nimbus = 49;
		///<summary>
		/// Drifts downwards while following player, destroyed on contact.
		///</summary>
		public const int Spore = 50;
		///<summary>
		/// Clings to nearby blocks, chases player, fires projectiles and spiky balls that bounce around.
		///</summary>
		public const int Plantera = 51;
		///<summary>
		/// Moves forward briefly before latching onto a block.
		///</summary>
		public const int Plantera_Hook = 52;
		///<summary>
		/// Acts very similar to the Plant AI, only bound to a certain entity.
		///</summary>
		public const int Plantera_Tentacle = 53;
		///<summary>
		/// Doesn't adhere to gravity or tile collisions, and teleports occasionally in first form. Once all Creepers are killed, it will begin rapidly teleporting and moving towards the player.
		///</summary>
		public const int Brain_of_Cthulhu = 54;
		///<summary>
		/// Circles around an entity, charging at player.
		///</summary>
		public const int Creeper = 55;
		///<summary>
		/// Moves directly towards player, gaining momentum. Emits blueish particles.
		///</summary>
		public const int Dungeon_Spirit = 56;
		///<summary>
		/// Moves towards player, stops, and fires projectiles straight at the player.
		///</summary>
		public const int Mourning_Wood = 57;
		///<summary>
		/// Very similar to the Head AI. Spawns with two Pumpking Scythes.
		///</summary>
		public const int Pumpking = 58;
		///<summary>
		/// Very similar to the Skeletron Hand AI.
		///</summary>
		public const int Pumpking_Scythe = 59;
		///<summary>
		/// Flies around, shooting a barrage of ice-based projectiles.
		///</summary>
		public const int Ice_Queen = 60;
		///<summary>
		/// Moves across the ground, stops moving, and launches many different types of projectiles.
		///</summary>
		public const int Santank = 61;
		///<summary>
		/// Attempts to fly around the player, shooting bullets rapidly if they are in sight.
		///</summary>
		public const int Elf_Copter = 62;
		///<summary>
		/// Flies towards the player at high speed.
		///</summary>
		public const int Flocko = 63;
		///<summary>
		/// Flies slowly in any direction and occasionally glows.
		///</summary>
		public const int Firefly = 64;
		///<summary>
		/// Flies slowly in any direction.
		///</summary>
		public const int Butterfly = 65;
		///<summary>
		/// Moves along the ground, pause for a bit, then continue moving. Will avoid walls and moves in the other direction if it has hit one.
		///</summary>
		public const int Passive_Worm = 66;
		///<summary>
		/// Acts similarly to the Passive Worm AI, but climbs up walls instead of moving away.
		///</summary>
		public const int Snail = 67;
		///<summary>
		/// Swims on water and walks on land. Will fly away when a player is nearby, but only while swimming. Lands after flying for a while.
		///</summary>
		public const int Duck = 68;
		///<summary>
		/// Rams player multiple times before summoning entities. In second form, it flies in circles and summons entities.
		///</summary>
		public const int Duke_Fishron = 69;
		///<summary>
		/// Flies through the air, chases player, and disappears after a period of time. Explodes and deals damage if it succeeds in reaching player.
		///</summary>
		public const int Detonating_Bubble = 70;
		///<summary>
		/// Follows an arcing path, dies when it touches a wall or player.
		///</summary>
		public const int Sharkron = 71;
		///<summary>
		/// Created by the Martian Officer, absorbs all damage dealt to the officer.
		///</summary>
		public const int Bubble_Shield = 72;
		///<summary>
		/// Built by Martian Engineers on the ground.
		///</summary>
		public const int Tesla_Turret = 73;
		///<summary>
		/// Travels through blocks, charges at the player.
		///</summary>
		public const int Corite = 74;
		///<summary>
		/// Has a Rider and a corresponding mount, if one is destroyed the other keeps attacking.
		///</summary>
		public const int Rider = 75;
		///<summary>
		/// Flies around the player, fires a Death Ray straight down, can only be damaged if all four turrets have been destroyed.
		///</summary>
		public const int Martian_Saucer = 76;
		///<summary>
		/// Positions behind the player, invulnerable until the Moon Lord's head and hands are defeated.
		///</summary>
		public const int Moon_Lord_Core = 77;
		///<summary>
		/// Flees to the left and right of the player, opens and closes, fires Phantasmal Spheres, Phantasmal Eyes, and Phantasmal Bolts. Spawns True Eye of Cthulhu when health is depleted.
		///</summary>
		public const int Moon_Lord_Hand = 78;
		///<summary>
		/// Flies above the player, shoots a Phantasmal Deathray and then some Phantasmal Bolts, spawns True Eye of Cthulhu when defeated.
		///</summary>
		public const int Moon_Lord_Head = 79;
		///<summary>
		/// Floats at a constant height, turns red and files away if the player gets near it, triggers the Martian Madness event if it gets off screen.
		///</summary>
		public const int Martian_Probe = 80;
		///<summary>
		/// Flies around the Moon Lord, shoots Phantasmal Sphere, Phantasmal Deathray, Phantasmal Eye and Phantasmal Bolts at the player.
		///</summary>
		public const int True_Eye_of_Cthulhu = 81;
		///<summary>
		/// Travels from the player to the Moon Lord's head, heals part of the Moon Lord for 1000 HP if it reaches the mouth.
		///</summary>
		public const int Moon_Leech_Clot = 82;
		///<summary>
		/// Never moves, causes the Lunatic Cultist to spawn when all are killed.
		///</summary>
		public const int Lunatic_Devote = 83;
		///<summary>
		/// Teleports around the player, fires shadow fireballs, ice mist, fireballs and lighting orbs. Creates duplicates of itself and summons Phantasm Dragons and Ancient Visions, triggers Lunar events when defeated.
		///</summary>
		public const int Lunatic_Cultist = 84;
		///<summary>
		/// Flies around the player, slowly floats towards them. Usually sticks to the player.
		///</summary>
		public const int Star_Cell = 85;
		///<summary>
		/// Flies around the player, does not adhere to gravity or tile collisions.
		///</summary>
		public const int Ancient_Vision = 86;
		///<summary>
		/// Passive until approached by the player. Attacks by jumping, dashing rapidly, and jumping into the air and slamming down on the player, ignoring block collision. Periodically "shuts" and becomes immune to damage and reflects projectiles in Expert Mode.
		///</summary>
		public const int Biome_Mimic = 87;
		///<summary>
		/// Flies through blocks, lunges at the player, occasionally lays a Mothron Egg.
		///</summary>
		public const int Mothron = 88;
		///<summary>
		/// Doesn't move, spawns Baby Mothrons after a while.
		///</summary>
		public const int Mothron_Egg = 89;
		///<summary>
		/// Spawns from Mothron Eggs, flies, lunges at the player.
		///</summary>
		public const int Baby_Mothron = 90;
		///<summary>
		/// Floats towards the player, passing through tiles if the player is far enough. Drops to the ground when hurt in Expert mode.
		///</summary>
		public const int Granite_Elemental = 91;
		///<summary>
		/// Stationary, recoils when damaged.
		///</summary>
		public const int Target_Dummy = 92;
		///<summary>
		/// Floats about the player, is defeated when all 4 cannons are destroyed.
		///</summary>
		public const int Flying_Dutchman = 93;
		///<summary>
		/// Bobs up and down in place, can only be damaged when 100 / 150 of its event enemies are defeated. Triggers Moon Lord to spawn when all 4 are defeated.
		///</summary>
		public const int Celestial_Pillar = 94;
		///<summary>
		/// Remains stationary, eventually grows into a full sized star cell.
		///</summary>
		public const int Small_Star_Cell = 95;
		///<summary>
		/// Floats around the player, summons 3 orbiting minions, fires projectiles at the player.
		///</summary>
		public const int Flow_Invader = 96;
		///<summary>
		/// Floats around the player, summons 3 orbiting minions, charges at the player and fires laser while teleporting.
		///</summary>
		public const int Nebula_Floater = 97;
		///<summary>
		/// Stays still and shoots fire.
		///</summary>
		public const int Unknown_1 = 98;
		///<summary>
		/// Spawned from the top of the Solar Pillar. Shot upward on spawn and gradually falls down to the ground, destroyed on contact with blocks or the player.
		///</summary>
		public const int Solar_Fragment = 99;
		///<summary>
		/// Fired in spreads of five by the Lunatic Cultist.
		///</summary>
		public const int Ancient_Light = 100;
		///<summary>
		/// Spawned from Lunatic Cultist in Expert Mode, fires 4 projectiles in a "+" shape when defeated/summoned.
		///</summary>
		public const int Ancient_Doom = 101;
		///<summary>
		/// Floats around, moving towards the player, and occasionally spawns three 'sandnados'. Gets faster the more it is damaged.
		///</summary>
		public const int Sand_Elemental = 102;
		///<summary>
		/// Hides under sand (and its variants), occasionally dashes at the player.
		///</summary>
		public const int Sand_Shark = 103;
		///<summary>
		/// Currently, no NPCs follow this AI. Its specifics are unknown.
		///</summary>
		public const int Unknown_2 = 104;
		///<summary>
		/// Stays still, targeted by Old One's Army monsters, ends the Old One's Army event if health reaches 0.
		///</summary>
		public const int Eternia_Crystal = 105;
		///<summary>
		/// Spawns Etherian enemies.
		///</summary>
		public const int Mysterious_Portal = 106;
		///<summary>
		/// Moves towards the Eternia Crystal to attack it, and floats through blocks if it can't reach the crystal.
		///</summary>
		public const int Attacker = 107;
		///<summary>
		/// Flies above the Eternia Crystal, pauses, and then dives for it. The cycle then repeats.
		///</summary>
		public const int Flying_Attacker = 108;
		///<summary>
		/// Slowly moves towards the Eternia Crystal. Occasionally summons multiple Old One's Skeletons around it and heals enemies.
		///</summary>
		public const int Dark_Mage = 109;
		///<summary>
		/// AI behaves similarly to the Duke Fishron AI. Shoots fireballs and rarely sweeps over the crystal with Flame Breath.
		///</summary>
		public const int Betsy = 110;
		///<summary>
		/// AI behaves similarly to the Hovering AI. Shoots lightning below it repeatedly.
		///</summary>
		public const int Etherian_Lightning_Bug = 111;
		///<summary>
		/// Moves towards the player, floats around them if no treasure is nearby, moves towards nearby treasure if detected, then waits for the player to go near it, despawns when on top of treasure.
		///</summary>
		public const int Fairy = 112;
		///<summary>
		/// Spawns attached to an entity, floats up and down along with attached entity, rapidly flies up if attached entity dies, dies if it hits a block.
		///</summary>
		public const int Windy_Balloon = 113;
		///<summary>
		/// Flies in bursts in random directions, flies away at fast speeds if a player approaches.
		///</summary>
		public const int Dragonfly = 114;
		///<summary>
		/// Alternates between flying slowly in any direction and walking back and forth on the ground.
		///</summary>
		public const int Ladybug = 115;
		///<summary>
		/// Glides on water and hops on land.
		///</summary>
		public const int Water_Strider = 116;
		///<summary>
		/// Cycles between firing blood projectiles at the player, charging and spinning around the player, and summoning Blood Squids.
		///</summary>
		public const int Dreadnautilus = 117;
		///<summary>
		/// Swims in bursts in random directions.
		///</summary>
		public const int Seahorse = 118;
		///<summary>
		/// Doesn't move, shoots projectiles at nearby players, but only in the wind’s direction.
		///</summary>
		public const int Angry_Dandelion = 119;
		///<summary>
		/// Hovers slightly above the player, cycles between several projectile attacks and charging the player. Disappears and reappears at half health, and starts cycling through attacks faster. Enrages during the daytime.
		///</summary>
		public const int Empress_of_Light = 120;
		///<summary>
		/// Cycles between performing hops, leaping towards the player to pound them, and firing Regal Gel. Grows wings at half health, and begins flying towards the player's position, firing Regal Gel and attempting to pound the player when above them. Spawns Crystal Slimes, Heavenly Slimes, and Bouncy Slimes when damaged.
		///</summary>
		public const int Queen_Slime = 121;
		public const int Pirate_Curse = 122;
	}
	public static class ChestID {
		public const int Normal = 0;
		public const int Gold = 1;
		public const int LockedGold = 2;
		public const int Shadow = 3;
		public const int LockedShadow = 4;
		public const int Barrel = 5;
		public const int TrashCan = 6;
		public const int Ebonwood = 7;
		public const int RichMahogany = 8;
		public const int Pearlwood = 9;
		public const int Ivy = 10;
		public const int Ice = 11;
		public const int LivingWood = 12;
		public const int Skyware = 13;
		public const int Shadewood = 14;
		public const int Web = 15;
		public const int Lihzahrd = 16;
		public const int Water = 17;
		public const int Jungle = 18;
		public const int Corruption = 19;
		public const int Crimson = 20;
		public const int Hallow = 21;
		public const int Frozen = 22;
		public const int LockedJungle = 23;
		public const int LockedCorruption = 24;
		public const int LockedCrimson = 25;
		public const int LockedHallow = 26;
		public const int LockedFrozen = 27;
		public const int Dynasty = 28;
		public const int Honey = 29;
		public const int Steampunk = 30;
		public const int Palm = 31;
		public const int Mushroom = 32;
		public const int Boreal = 33;
		public const int Slime = 34;
		public const int GreenDungeon = 35;
		public const int LockedGreenDungeon = 36;
		public const int PinkDungeon = 37;
		public const int LockedPinkDungeon = 38;
		public const int BlueDungeon = 39;
		public const int LockedBlueDungeon = 40;
		public const int Bone = 41;
		public const int Cactus = 42;
		public const int Flesh = 43;
		public const int Obsidian = 44;
		public const int Pumpkin = 45;
		public const int Spooky = 46;
		public const int Glass = 47;
		public const int Martian = 48;
		public const int Meteorite = 49;
		public const int Granite = 50;
		public const int Marble = 51;
		public const int Crystal = 52;
		public const int Golden = 53;

		public const int Crystal2 = 56 + 0;
		public const int Golden2 = 56 + 1;
		public const int Spider = 56 + 2;
		public const int Lesion = 56 + 3;
		public const int DeadMan = 56 + 4;
		public const int Solar = 56 + 5;
		public const int Vortex = 56 + 6;
		public const int Nebula = 56 + 7;
		public const int Stardust = 56 + 8;
		public const int Golf = 56 + 9;
		public const int Sandstone = 56 + 10;
		public const int Bamboo = 56 + 11;
		public const int Desert = 56 + 12;
		public const int LockedDesert = 56 + 13;
		public const int Reef = 56 + 14;
		public const int Baloon = 56 + 15;
		public const int AshWood = 56 + 16;
		public static readonly IdDictionary Search = IdDictionary.Create(typeof(ChestID), typeof(int));
	}
	#endregion
	public delegate void hook_DropItem(ItemDropper orig, DropAttemptInfo info, int item, int stack, bool scattered = false);
	public delegate void ItemDropper(DropAttemptInfo info, int item, int stack, bool scattered = false);
	public static class OriginExtensions {
		public static Func<float, int, Vector2> drawPlayerItemPos;
		#region sound
		public static SoundStyle WithPitch(this SoundStyle soundStyle, float pitch) {
			soundStyle.Pitch = pitch;
			return soundStyle;
		}
		public static SoundStyle WithPitchVarience(this SoundStyle soundStyle, float pitchVarience) {
			soundStyle.PitchVariance = pitchVarience;
			return soundStyle;
		}
		public static SoundStyle WithPitchRange(this SoundStyle soundStyle, float min, float max) {
			//soundStyle.PitchRange = (min, max);
			return soundStyle with {
				Pitch = (min + max) / 2,
				PitchVariance = max - min
			};
		}
		public static SoundStyle WithVolume(this SoundStyle soundStyle, float volume) {
			soundStyle.Volume = volume;
			return soundStyle;
		}
		#endregion sound
		#region combat mechanics

		#endregion
		public static StatModifier Scale(this StatModifier statModifier, float additive = 1f, float multiplicative = 1f, float flat = 1f, float @base = 1f) {
			return new StatModifier(
				(statModifier.Additive - 1) * additive + 1,
				(statModifier.Multiplicative - 1) * multiplicative + 1,
				statModifier.Flat * flat,
				statModifier.Base * @base
			);
		}
		public static StatModifier ScaleMatrix(this StatModifier statModifier,
			(float additive, float multiplicative) additive,
			(float additive, float multiplicative) multiplicative,
			(float flat, float @base) flat,
			(float flat, float @base) @base
			) {
			return new StatModifier(
				((statModifier.Additive - 1) * additive.additive + 1) * ((statModifier.Multiplicative - 1) * additive.multiplicative + 1),
				((statModifier.Additive - 1) * multiplicative.additive + 1) * ((statModifier.Multiplicative - 1) * multiplicative.multiplicative + 1),
				(statModifier.Flat * flat.flat) + (statModifier.Base * flat.@base),
				(statModifier.Flat * @base.flat) + (statModifier.Base * @base.@base)
			);
		}
		public static StatModifier GetInverse(this StatModifier statModifier) {
			return new StatModifier(1f / statModifier.Multiplicative, 1f / statModifier.Additive, -statModifier.Base, -statModifier.Flat);
		}
		public static Vector2 GetKnockbackFromHit(this NPC.HitInfo hit, float xMult = 1, float yMult = -0.1f) => new Vector2(hit.Knockback * hit.HitDirection, -0.1f * hit.Knockback);
		public static void ApplyBuffTimeModifier(this Player player, float mult, bool[] set, bool invert = false) {
			for (int i = 0; i < Player.MaxBuffs; i++) {
				if (set[player.buffType[i]]) {
					if (invert) {
						player.buffTime[i] = (int)(player.buffTime[i] / mult);
					} else {
						player.buffTime[i] = (int)(player.buffTime[i] * mult);
					}
				}
			}
		}
		public static void ApplyBuffTimeModifier(this Player player, float mult, int type, bool invert = false) {
			for (int i = 0; i < Player.MaxBuffs; i++) {
				if (player.buffType[i] == type) {
					if (invert) {
						player.buffTime[i] = (int)(player.buffTime[i] / mult);
					} else {
						player.buffTime[i] = (int)(player.buffTime[i] * mult);
					}
					break;
				}
			}
		}
		public static void ApplyBuffTimeAccessory(this Player player, bool old, bool current, float mult, bool[] set) {
			if (current && !old) {
				player.ApplyBuffTimeModifier(mult, set);
			} else if (!current && old) {
				player.ApplyBuffTimeModifier(mult, set, true);
			}
		}
		public static void ApplyBuffTimeAccessory(this Player player, bool old, bool current, float mult, int type) {
			if (current && !old) {
				player.ApplyBuffTimeModifier(mult, type);
			} else if (!current && old) {
				player.ApplyBuffTimeModifier(mult, type, true);
			}
		}
		public static void AddMaxBreath(this Player player, int amount) {
			player.breathMax += amount;
			OnIncreaseMaxBreath?.Invoke(player, amount);
			//OriginsModIntegrations.breathOverMax
		}
		public static event Action<Player, int> OnIncreaseMaxBreath;
		#region spritebatch
		public static void Restart(this SpriteBatch spriteBatch, SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null, DepthStencilState depthStencilState = null) {
			spriteBatch.End();
			spriteBatch.Start(
				sortMode,
				blendState ?? BlendState.AlphaBlend,
				samplerState ?? SamplerState.LinearClamp,
				rasterizerState ?? Main.Rasterizer,
				effect,
				transformMatrix ?? Main.GameViewMatrix.TransformationMatrix,
				depthStencilState ?? DepthStencilState.None
			);
		}
		public static void Start(this SpriteBatch spriteBatch, SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null, DepthStencilState depthStencilState = null) {
			spriteBatch.Begin(
				sortMode,
				blendState ?? BlendState.AlphaBlend,
				samplerState ?? SamplerState.LinearClamp,
				depthStencilState ?? DepthStencilState.None,
				rasterizerState ?? Main.Rasterizer,
				effect,
				transformMatrix ?? Main.GameViewMatrix.TransformationMatrix
			);
		}
		private static FastFieldInfo<SpriteBatch, SpriteSortMode> _sortMode;
		internal static FastFieldInfo<SpriteBatch, SpriteSortMode> sortMode => _sortMode ??= new("sortMode", BindingFlags.NonPublic);
		private static FastFieldInfo<SpriteBatch, Effect> _customEffect;
		internal static FastFieldInfo<SpriteBatch, Effect> customEffect => _customEffect ??= new("customEffect", BindingFlags.NonPublic);
		private static FastFieldInfo<SpriteBatch, Matrix> _transformMatrix;
		internal static FastFieldInfo<SpriteBatch, Matrix> transformMatrix => _transformMatrix ??= new("transformMatrix", BindingFlags.NonPublic);
		public static SpriteBatchState GetState(this SpriteBatch spriteBatch) {
			return new SpriteBatchState(
				sortMode.GetValue(spriteBatch),
				spriteBatch.GraphicsDevice.BlendState,
				spriteBatch.GraphicsDevice.SamplerStates[0],
				spriteBatch.GraphicsDevice.DepthStencilState,
				spriteBatch.GraphicsDevice.RasterizerState,
				customEffect.GetValue(spriteBatch),
				transformMatrix.GetValue(spriteBatch)
			);
		}
		public static void Restart(this SpriteBatch spriteBatch, SpriteBatchState spriteBatchState, SpriteSortMode? sortMode = null, BlendState blendState = null, SamplerState samplerState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null, DepthStencilState depthStencilState = null) {
			spriteBatch.End();
			spriteBatch.Start(spriteBatchState, sortMode ?? spriteBatchState.sortMode, blendState ?? spriteBatchState.blendState, samplerState ?? spriteBatchState.samplerState, rasterizerState ?? spriteBatchState.rasterizerState, effect ?? spriteBatchState.effect, transformMatrix ?? spriteBatchState.transformMatrix, depthStencilState ?? spriteBatchState.depthStencilState);
		}
		public static void Start(this SpriteBatch spriteBatch, SpriteBatchState spriteBatchState, SpriteSortMode? sortMode = null, BlendState blendState = null, SamplerState samplerState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null, DepthStencilState depthStencilState = null) {
			spriteBatch.Begin(sortMode ?? spriteBatchState.sortMode, blendState ?? spriteBatchState.blendState, samplerState ?? spriteBatchState.samplerState, depthStencilState ?? spriteBatchState.depthStencilState, rasterizerState ?? spriteBatchState.rasterizerState, effect ?? spriteBatchState.effect, transformMatrix ?? spriteBatchState.transformMatrix);
		}
		#endregion
		public static int RandomRound(this UnifiedRandom random, float value) {
			float amount = value % 1;
			value -= amount;
			if (amount == 0) return (int)value;
			if (random.NextFloat() < amount) {
				value++;
			}
			return (int)value;
		}
		public static int GetGoreSlot(this Mod mod, string name) {
			if (Main.netMode == NetmodeID.Server) return 0;
			if (mod.TryFind(name, out ModGore modGore)) return modGore.Type;
			return mod.TryFind(name.Split('/', 3)[^1], out modGore) ? modGore.Type : 0;
		}
		public static int SpawnGoreByName(this Mod mod, IEntitySource source, Vector2 Position, Vector2 Velocity, string name, float Scale = 1) {
			if (Main.netMode == NetmodeID.Server) return 0;
			return Gore.NewGore(source, Position, Velocity, mod.GetGoreSlot(name), Scale);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 DrawPlayerItemPos(float gravdir, int itemtype) {
			return drawPlayerItemPos(gravdir, itemtype);
		}
		#region line of sight
		public static Vector2 GetLoSLength(Vector2 pos, Vector2 unit, int maxSteps, out int totalSteps) {
			return GetLoSLength(pos, new Point(1, 1), unit, new Point(1, 1), maxSteps, out totalSteps);
		}
		public static Vector2 GetLoSLength(Vector2 pos, Point size1, Vector2 unit, Point size2, int maxSteps, out int totalSteps) {
			Vector2 origin = pos;
			totalSteps = 0;
			while (Collision.CanHit(origin, size1.X, size1.Y, pos + unit, size2.X, size2.Y) && totalSteps < maxSteps) {
				totalSteps++;
				pos += unit;
			}
			return pos;
		}
		#endregion
		public static Rectangle Add(this Rectangle a, Vector2 b) {
			return new Rectangle(a.X + (int)b.X, a.Y + (int)b.Y, a.Width, a.Height);
		}
		public static Rectangle Recentered(this Rectangle a, Vector2 b) {
			return new Rectangle((int)b.X - a.Width / 2, (int)b.Y - a.Height / 2, a.Width, a.Height);
		}
		public static Vector4 FrameToUV(this Rectangle frame, Vector2 save) {
			return new Vector4(frame.X / save.X, frame.Y / save.Y, frame.Width / save.X, frame.Height / save.Y);
		}
		public static Vector4 UVFrame(this Asset<Texture2D> frame, int horizontalFrames = 1, int verticalFrames = 1, int frameX = 0, int frameY = 0, int sizeOffsetX = 0, int sizeOffsetY = 0) {
			return frame.Value.UVFrame(horizontalFrames, verticalFrames, frameX, frameY, sizeOffsetX, sizeOffsetY);
		}
		public static Vector4 UVFrame(this Texture2D frame, int horizontalFrames = 1, int verticalFrames = 1, int frameX = 0, int frameY = 0, int sizeOffsetX = 0, int sizeOffsetY = 0) {
			Vector2 sizeOffset = new Vector2(sizeOffsetX, sizeOffsetY) / frame.Size();
			Vector2 frameSize = new(1f / horizontalFrames, 1f / verticalFrames);
			return new Vector4(
				frameX * frameSize.X,
				frameY * frameSize.Y,
				frameSize.X + sizeOffset.X,
				frameSize.Y + sizeOffset.Y
			);
		}
		public static Rectangle Frame(this AutoLoadingAsset<Texture2D> asset, int horizontalFrames = 1, int verticalFrames = 1, int frameX = 0, int frameY = 0, int sizeOffsetX = 0, int sizeOffsetY = 0) {
			return asset.Value.Frame(horizontalFrames, verticalFrames, frameX, frameY, sizeOffsetX, sizeOffsetY);
		}
		public static float AngleDif(float alpha, float beta, out int dir) {
			float phi = Math.Abs(beta - alpha) % MathHelper.TwoPi;       // This is either the distance or 360 - distance
			dir = ((phi > MathHelper.Pi) ^ (alpha > beta)) ? -1 : 1;
			float distance = phi > MathHelper.Pi ? MathHelper.TwoPi - phi : phi;
			return distance;
		}
		public static Vector2 RotatedByRandom(this Vector2 vec, double maxRadians, UnifiedRandom rand) {
			return vec.RotatedBy(rand.NextDouble() * maxRadians - rand.NextDouble() * maxRadians);
		}
		public static void FixedUseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) {
			float xoffset = 10f;
			float yoffset = 24f;
			byte stage = 3;
			if (player.itemAnimation < player.itemAnimationMax * 0.333) {
				stage = 1;
				if (item.width >= 92) xoffset = 38f; else if (item.width >= 64) xoffset = 28f; else if (item.width >= 52) xoffset = 24f; else if (item.width > 32) xoffset = 14f;
			} else if (player.itemAnimation < player.itemAnimationMax * 0.666) {
				stage = 2;
				if (item.width >= 92) xoffset = 38f; else if (item.width >= 64) xoffset = 28f; else if (item.width >= 52) xoffset = 24f; else if (item.width > 32) xoffset = 18f;
				yoffset = 10f;
				if (item.height >= 64) yoffset = 14f; else if (item.height >= 52) yoffset = 12f; else if (item.height > 32) yoffset = 8f;
			} else {
				xoffset = 6f;
				if (item.width >= 92) xoffset = 38f; else if (item.width >= 64) xoffset = 28f; else if (item.width >= 52) xoffset = 24f; else if (item.width >= 48) xoffset = 18f; else if (item.width > 32) xoffset = 14f;
				yoffset = 10f;
				if (item.height >= 64) yoffset = 14f; else if (item.height >= 52) yoffset = 12f; else if (item.height > 32) yoffset = 8f;
			}
			hitbox.X = (int)(player.itemLocation.X = player.position.X + player.width * 0.5f + (item.width * 0.5f - xoffset) * player.direction);
			hitbox.Y = (int)(player.itemLocation.Y = player.position.Y + yoffset + player.mount.PlayerOffsetHitbox);
			hitbox.Width = (int)(item.width * item.scale);
			hitbox.Height = (int)(item.height * item.scale);
			if (player.direction == -1) hitbox.X -= hitbox.Width;
			if (player.gravDir == 1f) hitbox.Y -= hitbox.Height;
			switch (stage) {
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
			Rectangle drawRect = new Rectangle(
				(int)Math.Round(start.X - Main.screenPosition.X),
				(int)Math.Round(start.Y - Main.screenPosition.Y),
				(int)Math.Round((end - start).Length()),
				thickness);

			spriteBatch.Draw(Origins.instance.Assets.Request<Texture2D>("Projectiles/Pixel").Value, drawRect, null, color, (end - start).ToRotation(), Vector2.Zero, SpriteEffects.None, 0);
		}
		#region smoothing
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool LinearSmoothing(ref float smoothed, float target, float rate) {
			if (target != smoothed) {
				if (Math.Abs(target - smoothed) < rate) {
					smoothed = target;
				} else {
					if (target > smoothed) {
						smoothed += rate;
					} else if (target < smoothed) {
						smoothed -= rate;
					}
					return false;
				}
			}
			return true;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool LinearSmoothing(ref Vector2 smoothed, Vector2 target, float rate) {
			if (target != smoothed) {
				Vector2 diff = (target - smoothed);
				if ((target - smoothed).Length() < rate) {
					smoothed = target;
				} else {
					diff.Normalize();
					smoothed += diff * rate;
					return false;
				}
			}
			return true;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool LerpSmoothing(ref float smoothed, float target, float rate, float snap) {
			if (target != smoothed) {
				if (Math.Abs(target - smoothed) < snap) {
					smoothed = target;
				} else {
					smoothed = MathHelper.Lerp(smoothed, target, rate);
					return false;
				}
			}
			return true;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool LerpSmoothing(ref Vector2 smoothed, Vector2 target, float rate, float snap) {
			if (target != smoothed) {
				Vector2 diff = (target - smoothed);
				if ((target - smoothed).Length() < snap) {
					smoothed = target;
				} else {
					smoothed = Vector2.Lerp(smoothed, target, rate);
					return false;
				}
			}
			return true;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool AngularSmoothing(ref float smoothed, float target, float rate) {
			if (target != smoothed) {
				float diff = GeometryUtils.AngleDif(smoothed, target, out int dir);
				diff = Math.Abs(diff);
				float aRate = Math.Abs(rate);
				if (diff < aRate) {
					smoothed = target;
				} else {
					smoothed += rate * dir;
					return false;
				}
			}
			return true;
		}
		public static void AngularSmoothing(ref float smoothed, float target, float rate, out bool equal) {
			equal = true;
			if (target != smoothed) {
				float diff = GeometryUtils.AngleDif(smoothed, target, out int dir);
				diff = Math.Abs(diff);
				float aRate = Math.Abs(rate);
				if (diff <= aRate) {
					smoothed = target;
				} else {
					smoothed += rate * dir;
					equal = false;
				}
			}
		}
		#endregion
		#region vectors
		public static Vector2 Clamp(this Vector2 value, Vector2 min, Vector2 max) {
			return new Vector2(MathHelper.Clamp(value.X, min.X, max.X), MathHelper.Clamp(value.Y, min.Y, max.Y));
		}
		public static Vector2 Clamp(this Vector2 value, Rectangle area) {
			return new Vector2(MathHelper.Clamp(value.X, area.X, area.Right), MathHelper.Clamp(value.Y, area.Y, area.Bottom));
		}
		public static Vector2 TakeAverage(this List<Vector2> vectors) {
			Vector2 sum = default;
			int count = vectors.Count;
			for (int i = 0; i < vectors.Count; i++) {
				sum += vectors[i];
			}
			return count != 0 ? sum / count : sum;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 Vec2FromPolar(float theta, float magnitude = 1f) {
			return new Vector2((float)(magnitude * Math.Cos(theta)), (float)(magnitude * Math.Sin(theta)));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float NormDot(Vector2 a, Vector2 b) {
			return (Vector2.Normalize(a) * Vector2.Normalize(b)).Sum();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float NormDotWithPriorityMult(Vector2 a, Vector2 b, float priorityMult) {
			return (Vector2.Dot(Vector2.Normalize(a), Vector2.Normalize(b)) - 1) * priorityMult + 1;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sum(this Vector2 a) {
			return a.X + a.Y;
		}
		public static Vector2 WithMaxLength(this Vector2 vector, float length) {
			if (length <= 0) return Vector2.Zero;
			float pLength = vector.LengthSquared();
			return pLength > length * length ? Vector2.Normalize(vector) * length : vector;
		}
		public static Vector2 LerpEquals(ref Vector2 value, Vector2 value2, float amount) {
			return value = Vector2.Lerp(value, value2, amount);
		}
		#endregion
		public static Color Desaturate(this Color value, float multiplier) {
			float R = value.R / 255f;
			float G = value.G / 255f;
			float B = value.B / 255f;
			float median = (Math.Min(Math.Min(R, G), B) + Math.Max(Math.Max(R, G), B)) / 2f;
			return new Color(MathHelper.Lerp(median, R, multiplier), MathHelper.Lerp(median, G, multiplier), MathHelper.Lerp(median, B, multiplier));
		}
		public static T[] BuildArray<T>(int length, params int[] nonNullIndeces) where T : new() {
			T[] o = new T[length];
			for (int i = 0; i < nonNullIndeces.Length; i++) {
				o[nonNullIndeces[i]] = new T();
			}
			return o;
		}
		public static Vector2 OldPos(this Projectile self, int index) {
			return index == -1 ? self.position : self.oldPos[index];
		}
		public static float OldRot(this Projectile self, int index) {
			return index == -1 ? self.rotation : self.oldRot[index];
		}
		public static Vector2 OldPos(this NPC self, int index) {
			return index == -1 ? self.position : self.oldPos[index];
		}
		public static float OldRot(this NPC self, int index) {
			return index == -1 ? self.rotation : self.oldRot[index];
		}
		public static float GetRotation(this Entity self) {
			if (self is NPC npc) return npc.rotation;
			if (self is Projectile projectile) return projectile.rotation;
			if (self is Player player) return player.fullRotation;
			return 0f;
		}
		public static float GetOldRotation(this Entity self) {
			if (self is NPC npc) return npc.oldRot.Length >= 1 ? npc.oldRot[0] : npc.rotation;
			if (self is Projectile projectile) return projectile.oldRot.Length >= 1 ? projectile.oldRot[0] : projectile.rotation;
			if (self is Player player) return player.fullRotation;
			return 0f;
		}
		//named for the author of https://www.reddit.com/r/learnmath/comments/rrz697/topology_efficiently_create_a_set_of_random/
		public static List<Vector2> FelisCatusSampling(Rectangle area, int maxCount, float minSpread, float maxSpread) {
			List<Vector2> points = new();
			Queue<Vector2> newPoints = new();
			newPoints.Enqueue(area.Center.ToVector2());
			int retries = 0;
			for (int i = 0; i < maxCount; i++) {
				if (!newPoints.Any()) break;
				Vector2 next = newPoints.Peek() + Vec2FromPolar(Main.rand.NextFloat(MathHelper.TwoPi), Main.rand.NextFloat(minSpread, maxSpread));
				if (!area.Contains(next) || points.Any((p) => p.DistanceSQ(next) < minSpread * minSpread)) {
					if (++retries > 20) {
						retries = 0;
						newPoints.Dequeue();
					}
				} else {
					retries = 0;
					points.Add(next);
					newPoints.Enqueue(next);
				}
			}
			return points;
		}
		public static Recipe AddRecipeGroupWithItem(this Recipe recipe, int recipeGroupId, int showItem, int stack = 1) {
			if (!RecipeGroup.recipeGroups.ContainsKey(recipeGroupId)) {
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 1);
				defaultInterpolatedStringHandler.AppendLiteral("A recipe group with the ID ");
				defaultInterpolatedStringHandler.AppendFormatted(recipeGroupId);
				defaultInterpolatedStringHandler.AppendLiteral(" does not exist.");
				throw new RecipeException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			recipe.AddIngredient(showItem, stack);
			recipe.acceptedGroups.Add(recipeGroupId);
			return recipe;
		}
		public static bool IsTileReplacable(int x, int y) {
			Tile tile = Main.tile[x, y];
			return !tile.HasTile || (TileID.Sets.CanBeClearedDuringGeneration[tile.TileType] && WorldGen.CanKillTile(x, y));
		}
		public static void SpreadWall(int x, int y, ushort wallType, Dictionary<ushort, bool> replacables) {
			if (!WorldGen.InWorld(x, y)) {
				return;
			}
			int count = 0;
			Stack<Point> positions = new Stack<Point>();
			Stack<Point> nextPositions = new Stack<Point>();
			HashSet<Point> oldPositions = new HashSet<Point>();
			void AddPosition(Point newPosition) {
				if (!oldPositions.Contains(newPosition)) {
					nextPositions.Push(newPosition);
				}
			}
			nextPositions.Push(new Point(x, y));
			while (nextPositions.Count > 0) {
				while (nextPositions.Count > 0) positions.Push(nextPositions.Pop());
				while (positions.Count > 0) {
					Point position = positions.Pop();
					if (!WorldGen.InWorld(position.X, position.Y, 1)) {
						continue;
					}
					oldPositions.Add(position);
					Tile tile = Main.tile[position.X, position.Y];
					if (tile.WallType != wallType) {
						if (!WorldGen.SolidTile(position.X, position.Y)) {
							if (tile.WallType == WallID.None) {
								continue;
							}
							count++;
							if (count >= WorldGen.maxWallOut2) {
								continue;
							}
							AddPosition(new Point(position.X - 1, position.Y));
							AddPosition(new Point(position.X + 1, position.Y));
							AddPosition(new Point(position.X, position.Y - 1));
							AddPosition(new Point(position.X, position.Y + 1));
							AddPosition(new Point(position.X - 1, position.Y - 1));
							AddPosition(new Point(position.X + 1, position.Y - 1));
							AddPosition(new Point(position.X - 1, position.Y + 1));
							AddPosition(new Point(position.X + 1, position.Y + 1));
							AddPosition(new Point(position.X - 2, position.Y));
							AddPosition(new Point(position.X + 2, position.Y));
						}
						if (replacables.TryGetValue(tile.WallType, out bool isReplacable) && isReplacable) tile.WallType = wallType;
					}
				}
			}
		}
		public static bool IsDevName(string name, int dev = 0) {
			if (dev is 0 or 1) {//Tyfyter
				return name is "Jennifer" or "Asher";
			} else if (dev is 0 or 2) {//Chee

			}//add more here
			return false;
		}
		public static void SetToType(this Projectile self, int type) {
			float[] ai = self.ai;
			Vector2 pos = self.Center;
			int dmg = self.damage;
			float kb = self.knockBack;
			int id = self.identity;
			int owner = self.owner;
			self.SetDefaults(type);
			self.ai = ai;
			self.Center = pos;
			self.damage = dmg;
			self.knockBack = kb;
			self.identity = id;
			self.owner = owner;
		}
		[Obsolete("No longer incompatible with multiplayer, but just use AnimationType")]
		public static void CloneFrame(this NPC self, int type, int frameHeight) {
			int t = self.type;
			self.type = type;
			self.position += self.netOffset;
			self.VanillaFindFrame(frameHeight, false, type);
			self.type = t;
		}
		public static void DoFrames(this NPC self, int counterMax) {
			int heightEtBuffer = self.frame.Height;
			self.frameCounter += 1;
			if (self.frameCounter >= counterMax) {
				self.frame.Y += heightEtBuffer;
				self.frameCounter = 0;
				if (self.frame.Y >= heightEtBuffer * Main.npcFrameCount[self.type]) {
					self.frame.Y = 0;
				}
			}
		}
		public static string Get2ndPersonReference(this Player self, string args = "") {
			return Language.GetTextValue($"Mods.Origins.Words.2ndref{args}{(self.Male ? "male" : "female")}");
		}
		public static string GetCooldownText(int time) {
			return ((time / 60 < 60)
				? Language.GetTextValue("Mods.Origins.Items.GenericTooltip.SecondCooldown", Math.Round(time / 60.0))
				: Language.GetTextValue("Mods.Origins.Items.GenericTooltip.MinuteCooldown", Math.Round((time / 60) / 60.0)));
		}
		#region UnifiedRandom FieldInfos
		private static FieldInfo _inext;
		internal static FieldInfo Inext => _inext ??= typeof(UnifiedRandom).GetField("inext", BindingFlags.NonPublic | BindingFlags.Instance);
		private static FieldInfo _inextp;
		internal static FieldInfo Inextp => _inextp ??= typeof(UnifiedRandom).GetField("inextp", BindingFlags.NonPublic | BindingFlags.Instance);
		private static FieldInfo _seedArray;
		internal static FieldInfo SeedArray => _seedArray ??= typeof(UnifiedRandom).GetField("SeedArray", BindingFlags.NonPublic | BindingFlags.Instance);
		#endregion
		private static FastFieldInfo<ShopHelper, IShoppingBiome[]> _dangerousBiomes;
		internal static FastFieldInfo<ShopHelper, IShoppingBiome[]> dangerousBiomes => _dangerousBiomes ??= new("_dangerousBiomes", BindingFlags.NonPublic);
		internal static void initExt() {
			CollisionExtensions.Load();
		}
		private static FastStaticFieldInfo<Color[]> _colorLookup;
		private static FastStaticFieldInfo<Color[]> MapColorLookup => _colorLookup ??= new(typeof(MapHelper), "colorLookup", BindingFlags.NonPublic);
		public static Color GetTileMapColor(int type) {
			return MapColorLookup.GetValue()[type];
		}
		public static Color GetWallMapColor(int type) {
			if (Main.netMode == NetmodeID.Server) return Color.Transparent;
			return MapColorLookup.GetValue()[MapHelper.wallLookup[type]];
		}
		internal static void unInitExt() {
			_inext = null;
			_inextp = null;
			_seedArray = null;
			_dangerousBiomes = null;
			_defaultCharacterData = null;
			_spriteCharacters = null;
			strikethroughFont = null;
			_idToSlot = null;
			_colorLookup = null;
			CollisionExtensions.Unload();
			OnIncreaseMaxBreath = null;
		}
		public static UnifiedRandom Clone(this UnifiedRandom r) {
			UnifiedRandom o = new UnifiedRandom();
			Inext.SetValue(o, (int)Inext.GetValue(r));
			Inextp.SetValue(o, (int)Inextp.GetValue(r));
			SeedArray.SetValue(o, ((int[])SeedArray.GetValue(r)).ToArray());
			return o;
		}
		public static string Stringify(this Recipe r) {
			ItemID.Search.TryGetName(r.createItem.type, out string resultName);
			return $"result: {resultName} " +
				//$"alchemy: {r.alchemy} " +
				$"required Items: {string.Join(", ", r.requiredItem.Select((i) => { ItemID.Search.TryGetName(i.type, out string name); return name; }))} " +
				$"required Tiles: {string.Join(", ", r.requiredTile.Select((i) => { TileID.Search.TryGetName(i, out string name); return name; }))}";
		}
		public static T[] WithLength<T>(this T[] input, int length) {
			T[] output = new T[length];
			if (length > input.Length) {
				length = input.Length;
			}
			for (int i = 0; i < length; i++) {
				output[i] = input[i];
			}
			return output;
		}
		public static Rectangle BoxOf(Vector2 a, Vector2 b, float buffer) {
			return BoxOf(a, b, new Vector2(buffer));
		}
		public static Rectangle BoxOf(Vector2 a, Vector2 b, Vector2 buffer = default) {
			Vector2 position = Vector2.Min(a, b) - buffer;
			Vector2 dimensions = (Vector2.Max(a, b) + buffer) - position;
			return new Rectangle((int)position.X, (int)position.Y, (int)dimensions.X, (int)dimensions.Y);
		}
		public static bool CanBeHitBy(this NPC npc, Player player, Item item, bool checkImmortal = true) {
			if (!npc.active || (checkImmortal && npc.immortal) || npc.dontTakeDamage) {
				return false;
			}
			bool itemCanHitNPC = ItemLoader.CanHitNPC(item, player, npc) ?? true;
			if (!itemCanHitNPC) {
				return false;
			}
			bool canBeHitByItem = NPCLoader.CanBeHitByItem(npc, player, item) ?? true;
			if (!canBeHitByItem) {
				return false;
			}
			bool playerCanHitNPC = PlayerLoader.CanHitNPC(player, npc);
			if (!playerCanHitNPC) {
				return false;
			}
			bool playerCanHitNPCWithItem = PlayerLoader.CanHitNPCWithItem(player, item, npc) ?? true;
			if (!playerCanHitNPCWithItem) {
				return false;
			}
			if (npc.friendly) {
				switch (npc.type) {
					case NPCID.Guide:
					return player.killGuide;
					case NPCID.Clothier:
					return player.killClothier;
					default:
					return false;
				}
			}
			return true;
		}
		#region drop rules
		public static IItemDropRule WithOnFailedConditions(this IItemDropRule rule, IItemDropRule ruleToChain, bool hideLootReport = false) {
			rule.OnFailedConditions(ruleToChain, hideLootReport);
			return rule;
		}
		public static IItemDropRule WithOnFailedRoll(this IItemDropRule rule, IItemDropRule ruleToChain, bool hideLootReport = false) {
			rule.OnFailedRoll(ruleToChain, hideLootReport);
			return rule;
		}
		public static IItemDropRule WithOnSuccess(this IItemDropRule rule, IItemDropRule ruleToChain, bool hideLootReport = false) {
			rule.OnSuccess(ruleToChain, hideLootReport);
			return rule;
		}

		public static ItemDropAttemptResult ResolveRule(IItemDropRule rule, DropAttemptInfo info) {
			if (!rule.CanDrop(info)) {
				ItemDropAttemptResult itemDropAttemptResult = default(ItemDropAttemptResult);
				itemDropAttemptResult.State = ItemDropAttemptResultState.DoesntFillConditions;
				ItemDropAttemptResult itemDropAttemptResult2 = itemDropAttemptResult;
				ResolveRuleChains(rule, info, itemDropAttemptResult2);
				return itemDropAttemptResult2;
			}
			ItemDropAttemptResult itemDropAttemptResult3 = (rule as INestedItemDropRule)?.TryDroppingItem(info, ResolveRule) ?? rule.TryDroppingItem(info);
			ResolveRuleChains(rule, info, itemDropAttemptResult3);
			return itemDropAttemptResult3;
		}
		private static void ResolveRuleChains(IItemDropRule rule, DropAttemptInfo info, ItemDropAttemptResult parentResult) {
			ResolveRuleChains(ref info, ref parentResult, rule.ChainedRules);
		}
		private static void ResolveRuleChains(ref DropAttemptInfo info, ref ItemDropAttemptResult parentResult, List<IItemDropRuleChainAttempt> ruleChains) {
			if (ruleChains == null) {
				return;
			}
			for (int i = 0; i < ruleChains.Count; i++) {
				IItemDropRuleChainAttempt itemDropRuleChainAttempt = ruleChains[i];
				if (itemDropRuleChainAttempt.CanChainIntoRule(parentResult)) {
					ResolveRule(itemDropRuleChainAttempt.RuleToChain, info);
				}
			}
		}
		#endregion
		public static int GetVersion<T>(this LinkedList<T> ll) {
			if (LLNodeEnumerator<T>.LLVersion is null) LLNodeEnumerator<T>.LLVersion = typeof(LinkedList<T>).GetField("version", BindingFlags.NonPublic | BindingFlags.Instance);
			return (int)LLNodeEnumerator<T>.LLVersion.GetValue(ll);
		}

		public static int GetNearestPlayerFrame(Player player) {
			float rot = player.itemRotation * player.direction;
			if (rot < -0.75) {
				if (player.gravDir == -1f) {
					return 4;
				}
				return 2;
			}
			if (rot > 0.6) {
				if (player.gravDir == -1f) {
					return 2;
				}
				return 4;
			}
			return 3;
		}

		public static int GetNearestPlayerFrame(float angle, int direction, float gravDir = 1) {
			float rot = angle * direction;
			if (rot < -0.75) {
				if (gravDir == -1f) {
					return 4;
				}
				return 2;
			}
			if (rot > 0.6) {
				if (gravDir == -1f) {
					return 2;
				}
				return 4;
			}
			return 3;
		}

		public static int GetNearestPlayerFrame(float angle, float gravDir = 1) {
			double rot = Math.Sin(angle);
			if (rot < -0.15) {
				if (gravDir == -1f) {
					return 4;
				}
				return 2;
			}
			if (rot > 0.15) {
				if (gravDir == -1f) {
					return 2;
				}
				return 4;
			}
			return 3;
		}

		public static bool Contains(this Rectangle area, Vector2 point) {
			return area.Contains((int)point.X, (int)point.Y);
		}
		#region drawing
		public static void DrawLightningArc(this SpriteBatch spriteBatch, Vector2[] positions, Texture2D texture = null, float scale = 1f, params (float scale, Color color)[] colors) {
			if (texture is null) {
				texture = TextureAssets.Extra[33].Value;
			}
			Vector2 size;
			int colorLength = colors.Length;
			DelegateMethods.f_1 = 1;
			for (int colorIndex = 0; colorIndex < colorLength; colorIndex++) {
				size = new Vector2(scale) * colors[colorIndex].scale;
				DelegateMethods.c_1 = colors[colorIndex].color;
				for (int i = positions.Length; --i > 0;) {
					Utils.DrawLaser(spriteBatch, texture, positions[i], positions[i - 1], size, DelegateMethods.LightningLaserDraw);
				}
			}
		}
		public static void DrawLightningArcBetween(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, float sineMult, float precision = 0.1f, params (float scale, Color color)[] colors) {
			Rectangle screen = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
			if (!screen.Contains(start) && !screen.Contains(end)) {
				return;
			}
			List<Vector2> positions = new List<Vector2>();
			Vector2 normal = (end - start).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * (sineMult + Math.Sign(sineMult));
			for (float i = 0; i < 1f; i += precision) {
				positions.Add(Vector2.Lerp(start, end, i) + (normal * (float)Math.Sin(i * Math.PI) * Main.rand.NextFloat(0.75f, 1.25f)));
			}
			positions.Add(end);
			if (colors is null || colors.Length == 0) {
				colors = [
					(0.15f, new Color(80, 204, 219, 0) * 0.5f),
					(0.1f, new Color(80, 251, 255, 0) * 0.5f),
					(0.05f, new Color(200, 255, 255, 0) * 0.5f)
				];
			}
			spriteBatch.DrawLightningArc(
				positions.ToArray(),
				null,
				1.333f,
				colors
			);
		}
		public static void DrawGrappleChain(Vector2 startPos, Vector2 endPos, Texture2D texture, Rectangle[] frames, Color lightColor, bool useX = false, int dye = 0, Action<Vector2> action = null) {
			Vector2 center = endPos;
			Vector2 distToProj = startPos - endPos;
			float projRotation = distToProj.ToRotation() - (useX ? 0 : MathHelper.PiOver2);
			float distance = distToProj.Length();
			distToProj.Normalize();
			int frame = 0;
			while (distance > 8f && !float.IsNaN(distance)) {
				center += distToProj * (useX ? frames[frame].Width : frames[frame].Height);
				distance = (startPos - center).Length();
				Color drawColor = lightColor;
				if (action is not null) action(center);
				DrawData data = new DrawData(texture,
					center - Main.screenPosition,
					frames[frame],
					drawColor,
					projRotation,
					frames[frame].Size() * 0.5f,
					Vector2.One,
					SpriteEffects.None, 
				0);
				data.shader = dye;
				Main.EntitySpriteDraw(data);
				if (frame >= frames.Length) frame = 0;
			}
		}
		#endregion drawing
		public static Rectangle MoveToWithin(this Rectangle value, Rectangle area) {
			Rectangle output = value;
			if (output.Width > area.Width) {
				output.Width = area.Width;
			}
			if (output.Height > area.Height) {
				output.Height = area.Height;
			}
			output.X = Math.Min(Math.Max(output.X, area.X), (area.X + area.Width) - output.Width);
			output.Y = Math.Min(Math.Max(output.Y, area.Y), (area.Y + area.Height) - output.Height);
			return output;
		}
		public static Vector2 NextVectorIn(this UnifiedRandom random, Rectangle area) {
			return area.TopLeft() + new Vector2(Main.rand.Next(area.Width), Main.rand.Next(area.Height));
		}
		#region tiles
		public static void SetActive(this Tile tile, bool active) {
			tile.HasTile = active;
		}
		public static void SetHalfBlock(this Tile tile, bool halfBlock) {
			tile.IsHalfBlock = halfBlock;
		}
		public static void SetSlope(this Tile tile, SlopeType slope) {
			tile.Slope = slope;
		}
		public static void SetLiquidType(this Tile tile, int liquidType) {
			tile.LiquidType = liquidType;
		}
		public static bool HasSolidTile(this Tile tile) {
			return tile.HasUnactuatedTile && Main.tileSolid[tile.TileType];
		}
		public static bool HasFullSolidTile(this Tile tile) {
			return tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
		}
		public static int TileSolidness(this Tile tile) {
			if (!tile.HasTile) return 0;
			if (!Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType] || tile.IsActuated) return 1;
			return 2;
		}
		/// <summary>
		/// checks if a tile is active and is the provided type
		/// </summary>
		public static bool TileIsType(this Tile self, int type) {
			return self.HasTile && self.TileType == type;
		}
		#endregion
		public static T SafeGet<T>(this TagCompound self, string key, T fallback = default) {
			return self.TryGet(key, out T output) ? output : fallback;
		}
		public static void DrawTileGlow(this IGlowingModTile self, int i, int j, SpriteBatch spriteBatch) {
			if (self.GlowTexture.Value is null) {
				return;
			}
			DrawTileGlow(self.GlowTexture, self.GlowColor, i, j, spriteBatch);
		}
		public static void DrawTileGlow(Texture2D glowTexture, Color glowColor, int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];
			if (!TileDrawing.IsVisible(tile)) return;
			Vector2 offset = new(Main.offScreenRange, Main.offScreenRange);
			if (Main.drawToScreen) {
				offset = Vector2.Zero;
			}
			int posYFactor = -2;
			int flatY = 0;
			int kScaleY = 2;
			int flatX = 14;
			int kScaleX = -2;
			Vector2 position = new Vector2(i * 16f, j * 16f) + offset - Main.screenPosition;
			switch (tile.BlockType) {
				case BlockType.Solid:
				spriteBatch.Draw(glowTexture, position, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), glowColor, 0f, default, 1f, SpriteEffects.None, 0f);
				break;
				case BlockType.HalfBlock:
				spriteBatch.Draw(glowTexture, position + new Vector2(0, 8), new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 4), glowColor, 0f, default, 1f, SpriteEffects.None, 0f);
				spriteBatch.Draw(glowTexture, position + new Vector2(0, 12), new Rectangle(144, 66, 16, 4), glowColor, 0f, default, 1f, SpriteEffects.None, 0f);
				break;
				case BlockType.SlopeDownLeft://1
				posYFactor = 0;
				kScaleY = 0;
				flatX = 0;
				kScaleX = 2;
				goto case BlockType.SlopeUpRight;
				case BlockType.SlopeDownRight://2
				posYFactor = 0;
				kScaleY = 0;
				flatX = 14;
				kScaleX = -2;
				goto case BlockType.SlopeUpRight;
				case BlockType.SlopeUpLeft://3
				flatX = 0;
				kScaleX = 2;
				goto case BlockType.SlopeUpRight;

				case BlockType.SlopeUpRight://4
				for (int k = 0; k < 8; k++) {
					Main.spriteBatch.Draw(
						glowTexture,
						position + new Vector2(flatX + kScaleX * k, k * 2 + posYFactor * k),
						new Rectangle(tile.TileFrameX + flatX + kScaleX * k, tile.TileFrameY + flatY + kScaleY * k, 2, 16 - 2 * k),
						glowColor,
						0f,
						Vector2.Zero,
						1f,
						0,
						0f
					);
				}
				break;
			}
		}
		public static void DrawChestGlow(this IGlowingModTile self, int i, int j, SpriteBatch spriteBatch) {
			if (self.GlowTexture.Value is null) {
				return;
			}
			Tile tile = Main.tile[i, j];
			Vector2 vector = new Vector2(Main.offScreenRange, Main.offScreenRange);
			if (Main.drawToScreen) {
				vector = Vector2.Zero;
			}
			Point key = new Point(i, j);
			if (tile.TileFrameX % 36 != 0) {
				key.X--;
			}
			if (tile.TileFrameY % 36 != 0) {
				key.Y--;
			}
			int frameOffset = Main.chest[Chest.FindChest(key.X, key.Y)].frame * 38;
			spriteBatch.Draw(self.GetGlowTexture(tile.TileColor), (new Vector2(i * 16f, j * 16f) + vector) - Main.screenPosition, new Rectangle(tile.TileFrameX, tile.TileFrameY + frameOffset, 16, 16), self.GlowColor, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
		}
		public static Point GetTilePosition(this Tile tile) {
			uint id = TileMethods.TileId.GetValue(tile);
			return new Point((int)(id / Main.tile.Height), (int)(id % Main.tile.Height));
		}
		public static ITree GetTreeType(Tile tile) {
			if (!tile.HasTile || tile.TileType is TileID.VanityTreeSakura or TileID.VanityTreeYellowWillow || !TileID.Sets.IsATreeTrunk[tile.TileType]) return null;
			Point pos = tile.GetTilePosition();
			return GetTreeType(pos.X, pos.Y);
		}
		public static ITree GetTreeType(int i, int j) {
			Tile tile = Main.tile[i, j];
			if (!tile.HasTile || tile.TileType is TileID.VanityTreeSakura or TileID.VanityTreeYellowWillow || !TileID.Sets.IsATreeTrunk[tile.TileType]) return null;
			WorldGen.GetTreeBottom(i, j, out var x, out var y);
			return PlantLoader.GetTree(Main.tile[x, y].TileType);
		}
		public static Point OffsetBy(this Point self, int x = 0, int y = 0) {
			return new Point(self.X + x, self.Y + y);
		}
		public static byte PackToByte<T>(this (T h, T g, T f, T e, T d, T c, T b, T a) value, Func<T, bool> method) {
			return (byte)((method(value.h) ? 128 : 0) |
				(method(value.g) ? 64 : 0) |
				(method(value.f) ? 32 : 0) |
				(method(value.e) ? 16 : 0) |
				(method(value.d) ? 8 : 0) |
				(method(value.c) ? 4 : 0) |
				(method(value.b) ? 2 : 0) |
				(method(value.a) ? 1 : 0));
		}
		static Dictionary<int, Dictionary<EquipType, int>> _idToSlot;
		public static int GetEquipSlot(int itemType, EquipType equipType) {
			return _idToSlot[itemType][equipType];
		}
		public static bool WaterCollision(Vector2 Position, int Width, int Height) {
			int minX = Utils.Clamp((int)(Position.X / 16f) - 1, 0, Main.maxTilesX - 1);
			int maxX = Utils.Clamp((int)((Position.X + Width) / 16f) + 2, 0, Main.maxTilesX - 1);
			int minY = Utils.Clamp((int)(Position.Y / 16f) - 1, 0, Main.maxTilesY - 1);
			int maxY = Utils.Clamp((int)((Position.Y + Height) / 16f) + 2, 0, Main.maxTilesY - 1);
			Vector2 pos = default;
			Tile tile;
			for (int i = minX; i < maxX; i++) {
				for (int j = minY; j < maxY; j++) {
					tile = Framing.GetTileSafely(i, j);
					if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Water) {
						pos.X = i * 16;
						pos.Y = j * 16;
						float airAmount = (256 - tile.LiquidAmount) / 32f;
						pos.Y += airAmount * 2f;
						int surfaceOffset = 16 - (int)(airAmount * 2f);
						if (Position.X + Width > pos.X && Position.X < pos.X + 16f && Position.Y + Height > pos.Y && Position.Y < pos.Y + surfaceOffset) {
							return true;
						}
					}
				}
			}
			return false;
		}
		public static WeightedRandom<int> GetAllPrefixes(Item item, UnifiedRandom rand, params PrefixCategory[] prefixCategories) {
			WeightedRandom<int> wr = new(rand);
			for (int i = 0; i < prefixCategories.Length; i++) {
				PrefixCategory category = prefixCategories[i];
				foreach (int pre in Item.GetVanillaPrefixes(category)) {
					wr.Add(pre);
				}
				foreach (ModPrefix modPrefix in PrefixLoader.GetPrefixesInCategory(category).Where((ModPrefix x) => x.CanRoll(item))) {
					wr.Add(modPrefix.Type, modPrefix.RollChance(item));
				}
			}
			return wr;
		}
		#region font
		static FieldInfo _spriteCharacters;
		static FieldInfo _SpriteCharacters => _spriteCharacters ??= typeof(DynamicSpriteFont).GetField("_spriteCharacters", BindingFlags.NonPublic | BindingFlags.Instance);
		static FieldInfo _defaultCharacterData;
		static FieldInfo _DefaultCharacterData => _defaultCharacterData ??= typeof(DynamicSpriteFont).GetField("_defaultCharacterData", BindingFlags.NonPublic | BindingFlags.Instance);
		static DynamicSpriteFont strikethroughFont;
		public static DynamicSpriteFont StrikethroughFont {
			get {
				if (strikethroughFont is null) {
					if (FontAssets.MouseText.IsLoaded) {
						DynamicSpriteFont baseFont = FontAssets.MouseText.Value;
						strikethroughFont = new DynamicSpriteFont(-2, baseFont.LineSpacing, baseFont.DefaultCharacter);
						_SpriteCharacters.SetValue(strikethroughFont, _SpriteCharacters.GetValue(baseFont));
						_DefaultCharacterData.SetValue(strikethroughFont, _DefaultCharacterData.GetValue(baseFont));
					} else {
						return FontAssets.MouseText.Value;
					}
				}
				return strikethroughFont;
			}
		}
		#endregion
		/// <summary>
		/// inserts an item into a shimmer cycle, will not work to add an item after the last item of a cycle that is not complete yet
		/// </summary>
		/// <param name="type"></param>
		/// <param name="after"></param>
		public static void InsertIntoShimmerCycle(int type, int after) {
			ItemID.Sets.ShimmerTransformToItem[type] = ItemID.Sets.ShimmerTransformToItem[after];
			ItemID.Sets.ShimmerTransformToItem[after] = type;
		}
		public static void RegisterForUnload(this IUnloadable unloadable) {
			Origins.unloadables.Add(unloadable);
		}
		public static string GetDefaultTMLName(this Type type) => (type.Namespace + "." + type.Name).Replace('.', '/');
		public static IEnumerable<T> GetFlags<T>(this T value) where T : struct, Enum {
			T[] possibleFlags = Enum.GetValues<T>();
			for (int i = 0; i < possibleFlags.Length; i++) {
				if (possibleFlags[i].Equals(default(T))) continue;
				if (value.HasFlag(possibleFlags[i])) yield return possibleFlags[i];
			}
		}
		public static FlavorTextBestiaryInfoElement GetBestiaryFlavorText(this ModNPC npc) {
			Language.GetOrRegister($"Mods.{npc.Mod.Name}.Bestiary.{npc.Name}", () => "bestiary text here");
			return new FlavorTextBestiaryInfoElement($"Mods.{npc.Mod.Name}.Bestiary.{npc.Name}");
		}
		public static string MakeContext(params string[] args) {
			return new StringBuilder().AppendJoin(';', args.Where(a => !string.IsNullOrWhiteSpace(a))).ToString();
		}
		public static EntitySource_ItemUse WithContext(this EntitySource_ItemUse itemUseSource, params string[] args) {
			string bocContext = MakeContext(args);
			if (itemUseSource is EntitySource_ItemUse_WithAmmo sourceWAmmo) {
				return new EntitySource_ItemUse_WithAmmo(sourceWAmmo.Player, sourceWAmmo.Item, sourceWAmmo.AmmoItemIdUsed, bocContext);
			} else {
				return new EntitySource_ItemUse(itemUseSource.Player, itemUseSource.Item, bocContext);
			}
		}
		static GeneratorCache<Type, Func<IEntitySource, string, IEntitySource>> cloneWithContexts = new(GenerateCloneWithContext);
		public static IEntitySource CloneWithContext(this IEntitySource source, string context) {
			return cloneWithContexts[source.GetType()](source, context);
		}
		static Func<IEntitySource, string, IEntitySource> GenerateCloneWithContext(Type sourceType) {
			string methodName = sourceType.FullName + ".CloneWithContext";
			DynamicMethod getterMethod = new(methodName, typeof(IEntitySource), [typeof(IEntitySource), typeof(string)], true);
			ILGenerator gen = getterMethod.GetILGenerator();

			ConstructorInfo ctor = sourceType.GetConstructors()[0];
			Dictionary<string, MemberInfo> vars = sourceType.GetMembers().Where(m => m is FieldInfo or PropertyInfo).ToDictionary(m => m.Name.ToUpperInvariant());
			foreach (ParameterInfo param in ctor.GetParameters()) {
				string name = param.Name.ToUpperInvariant();
				if (name == "CONTEXT") {
					gen.Emit(OpCodes.Ldarg_1);
					continue;
				}
				if (vars.TryGetValue(name, out MemberInfo mem)) {
					if (mem is PropertyInfo prop && param.ParameterType == prop?.PropertyType) {
						gen.Emit(OpCodes.Ldarg_0);
						gen.Emit(OpCodes.Call, prop.GetMethod);
						continue;
					} else if (mem is FieldInfo field && param.ParameterType == field?.FieldType) {
						gen.Emit(OpCodes.Ldarg_0);
						gen.Emit(OpCodes.Ldfld, field);
						continue;
					}
				}
				gen.Emit(OpCodes.Ldnull);
			}
			gen.Emit(OpCodes.Newobj, ctor);
			gen.Emit(OpCodes.Ret);

			return getterMethod.CreateDelegate<Func<IEntitySource, string, IEntitySource>>();
		}
		public static bool Matches(this Recipe recipe, (int id, int? count)? result, int[] tiles, params (int id, int? count)[] ingredients) {
			static bool ItemMatches(Item item, (int id, int? count) pattern) {
				if (item.type == pattern.id) {
					return !pattern.count.HasValue || item.stack == pattern.count;
				}
				return false;
			}
			if (result.HasValue && !ItemMatches(recipe.createItem, result.Value)) return false;
			if (ingredients is not null) {
				if (recipe.requiredItem.Count == ingredients.Length) {
					for (int i = 0; i < ingredients.Length; i++) {
						(int id, int? count) ingredient = ingredients[i];
						if (!recipe.requiredItem.Any(req => ItemMatches(req, ingredient))) return false;
					}
				} else {
					return false;
				}
			}
			if (tiles is not null) {
				if (recipe.requiredTile.Count == tiles.Length) {
					for (int i = 0; i < ingredients.Length; i++) {
						if (!recipe.requiredTile.Contains(tiles[i])) return false;
					}
				} else {
					return false;
				}
			}
			return true;
		}
		public static void SpawnBossOn(this Player player, int type) {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				NPC.SpawnOnPlayer(player.whoAmI, type);
			} else {
				ModPacket packet = Origins.instance.GetPacket();
				packet.Write(Origins.NetMessageType.spawn_boss_on_player);
				packet.Write((ushort)player.whoAmI);
				packet.Write(type);
				packet.Send();
			}
		}
		public static void AddChambersiteConversions(this AltBiome biome, int tile, int wall) {
			biome.AddWallConversions(wall, ModContent.WallType<Chambersite_Stone_Wall>());
			biome.GERunnerWallConversions.Add(WallID.AmethystUnsafe, wall);
			biome.GERunnerWallConversions.Add(WallID.TopazUnsafe, wall);
			biome.GERunnerWallConversions.Add(WallID.SapphireUnsafe, wall);
			biome.GERunnerWallConversions.Add(WallID.EmeraldUnsafe, wall);
			biome.GERunnerWallConversions.Add(WallID.RubyUnsafe, wall);
			biome.GERunnerWallConversions.Add(WallID.DiamondUnsafe, wall);
		}
		public static float SpecificTilesEnemyRate(this NPCSpawnInfo spawnInfo, HashSet<int> tiles, bool hardmode = false) {
			if (hardmode && !Main.hardMode) return 0f;
			if (tiles.Contains(spawnInfo.SpawnTileType)) {
				return 1f;
			}
			return 0f;
		}
		//couldn't resist giving this a stupidly long name
		public static void FadeOutOldProjectilesAtLimit(HashSet<int> types, int limit, int minTimeLeft = 0) {
			redo:
			int count = 0;
			int index = -1;
			int timeLeft = int.MaxValue;
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				if (projectile.owner == Main.myPlayer && projectile.timeLeft > minTimeLeft && types.Contains(projectile.type)) {
					count++;
					if (projectile.timeLeft < timeLeft) {
						index = projectile.whoAmI;
						timeLeft = projectile.timeLeft;
					}
				}
			}
			if (count >= limit) {
				Main.projectile[index].timeLeft = minTimeLeft;
				if (count > limit) goto redo;
			}
		}
		public static void DrawDebugOutline(this Rectangle area, Vector2 offset = default, int dustType = DustID.Torch) {
			Vector2 pos = area.TopLeft() + offset;
			for (int c = 0; c < area.Width; c += 2) {
				Dust.NewDustPerfect(pos + new Vector2(c, 0), dustType, Vector2.Zero).noGravity = true;
			}
			for (int c = 0; c < area.Height; c += 2) {
				Dust.NewDustPerfect(pos + new Vector2(0, c), dustType, Vector2.Zero).noGravity = true;
			}
			for (int c = 0; c < area.Width; c += 2) {
				Dust.NewDustPerfect(pos + new Vector2(c, area.Height), dustType, Vector2.Zero).noGravity = true;
			}
			for (int c = 0; c < area.Height; c += 2) {
				Dust.NewDustPerfect(pos + new Vector2(area.Width, c), dustType, Vector2.Zero).noGravity = true;
			}
		}
		public static void DrawDebugOutline(this Triangle area, Vector2 offset = default, int dustType = DustID.Torch) {
			for (float c = 0; c <= 1; c += 0.125f) {
				Dust.NewDustPerfect(offset + Vector2.Lerp(area.a, area.b, c), dustType, Vector2.Zero).noGravity = true;
			}
			for (float c = 0; c <= 1; c += 0.125f) {
				Dust.NewDustPerfect(offset + Vector2.Lerp(area.b, area.c, c), dustType, Vector2.Zero).noGravity = true;
			}
			for (float c = 0; c <= 1; c += 0.125f) {
				Dust.NewDustPerfect(offset + Vector2.Lerp(area.c, area.a, c), dustType, Vector2.Zero).noGravity = true;
			}
		}
		public static void DrawDebugLine(Vector2 a, Vector2 b, Vector2 offset = default, int dustType = DustID.Torch) {
			for (float c = 0; c <= 1; c += 0.125f) {
				Dust.NewDustPerfect(offset + Vector2.Lerp(a, b, c), dustType, Vector2.Zero).noGravity = true;
			}
		}
		public static void DrawDebugLineSprite(Vector2 a, Vector2 b, Color color, Vector2 offset = default) {
			Vector2 diff = b - a;
			Main.spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				a + offset,
				new Rectangle(0, 0, 2, 2),
				color,
				diff.ToRotation(),
				Vector2.UnitY,
				new Vector2(diff.Length() * 0.5f, 1 * 0.5f),
				0,
			0);
		}
		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, Func<TValue> fallback) {
			if (self.TryGetValue(key, out TValue value)) return value;
			value = fallback();
			self.Add(key, value);
			return value;
		}
		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key) where TValue : new() {
			if (self.TryGetValue(key, out TValue value)) return value;
			value = new();
			self.Add(key, value);
			return value;
		}
		public static bool TryGetText(string key, [MaybeNullWhen(false)] out LocalizedText text) {
			if (Language.Exists(key)) {
				text = Language.GetText(key);
				return true;
			}
			text = null;
			return false;
		}
		public static void GetMultiTileTopLeft(int i, int j, TileObjectData data, out int left, out int top) {
			Tile tile = Main.tile[i, j];
			int innerFrameY = tile.TileFrameY % data.CoordinateFullHeight;
			int frameI = (tile.TileFrameX % data.CoordinateFullWidth) / (data.CoordinateWidth + data.CoordinatePadding);
			int frameJ = 0;
			while (innerFrameY >= data.CoordinateHeights[frameJ] + data.CoordinatePadding) {
				innerFrameY -= data.CoordinateHeights[frameJ] + data.CoordinatePadding;
				frameJ++;
			}
			top = j - frameJ;
			left = i - frameI;
		}
	}
	public static class ShopExtensions {
		public static NPCShop InsertAfter<T>(this NPCShop shop, int targetItem, params Condition[] condition) where T : ModItem =>
			shop.InsertAfter(targetItem, ModContent.ItemType<T>(), condition);
		public static NPCShop InsertBefore<T>(this NPCShop shop, int targetItem, params Condition[] condition) where T : ModItem =>
			shop.InsertBefore(targetItem, ModContent.ItemType<T>(), condition);
		public static NPCShop InsertAfter<TAfter, TNew>(this NPCShop shop, params Condition[] condition) where TAfter : ModItem where TNew : ModItem  =>
			shop.InsertAfter(ModContent.ItemType<TAfter>(), ModContent.ItemType<TNew>(), condition);
		public static NPCShop InsertBefore<TBefore, TNew>(this NPCShop shop, params Condition[] condition) where TBefore : ModItem where TNew : ModItem  =>
			shop.InsertBefore(ModContent.ItemType<TBefore>(), ModContent.ItemType<TNew>(), condition);
	}
	public static class ConditionExtensions {
		public static Condition CommaAnd(this Condition a, Condition b) {
			return new Condition(
				Language.GetOrRegister("Mods.Origins.Conditions.Comma").WithFormatArgs(a.Description, b.Description),
				() => a.Predicate() && b.Predicate()
			);
		}
		public static Condition And(this Condition a, Condition b) {
			return new Condition(
				Language.GetOrRegister("Mods.Origins.Conditions.And").WithFormatArgs(a.Description, b.Description),
				() => a.Predicate() && b.Predicate()
			);
		}
		public static Condition CommaOr(this Condition a, Condition b) {
			return new Condition(
				Language.GetOrRegister("Mods.Origins.Conditions.Comma").WithFormatArgs(a.Description, b.Description),
				() => a.Predicate() || b.Predicate()
			);
		}
		public static Condition Or(this Condition a, Condition b) {
			return new Condition(
				Language.GetOrRegister("Mods.Origins.Conditions.Or").WithFormatArgs(a.Description, b.Description),
				() => a.Predicate() || b.Predicate()
			);
		}
		public static Condition Not(this Condition value) {
			return new Condition(
				Language.GetOrRegister("Mods.Origins.Conditions.Not").WithFormatArgs(value.Description),
				() => !value.Predicate()
			);
		}
	}
	public static class CollisionExtensions {
		static Triangle[] tileTriangles;
		static Rectangle[] tileRectangles;
		public static void Load() {
			Vector2 topLeft = Vector2.Zero * 16;
			Vector2 topRight = Vector2.UnitX * 16;
			Vector2 bottomLeft = Vector2.UnitY * 16;
			Vector2 bottomRight = Vector2.One * 16;
			tileTriangles = [
				new Triangle(topLeft, bottomLeft, bottomRight),
				new Triangle(topRight, bottomLeft, bottomRight),
				new Triangle(topLeft, topRight, bottomLeft),
				new Triangle(topLeft, topRight, bottomRight)
			];
			tileRectangles = [
				new Rectangle(0, 0, 16, 16),
				new Rectangle(0, 8, 16, 8)
			];
		}
		public static void Unload() {
			tileTriangles = null;
			tileRectangles = null;
		}
		public static bool OverlapsAnyTiles(this Rectangle area, bool fallThrough = true, bool drawDebug = false) {
			Rectangle checkArea = area;
			Point topLeft = area.TopLeft().ToTileCoordinates();
			Point bottomRight = area.BottomRight().ToTileCoordinates();
			int minX = Utils.Clamp(topLeft.X, 0, Main.maxTilesX - 1);
			int minY = Utils.Clamp(topLeft.Y, 0, Main.maxTilesY - 1);
			int maxX = Utils.Clamp(bottomRight.X, 0, Main.maxTilesX - 1) - minX;
			int maxY = Utils.Clamp(bottomRight.Y, 0, Main.maxTilesY - 1) - minY;
			int cornerX = area.X - topLeft.X * 16;
			int cornerY = area.Y - topLeft.Y * 16;
			if (drawDebug) {
				bool colliding = false;
				for (int i = 0; i <= maxX; i++) {
					for (int j = 0; j <= maxY; j++) {
						Tile tile = Main.tile[i + minX, j + minY];
						Dust.NewDustPerfect(new Vector2((i + minX) * 16 + 8, (j + minY) * 16 + 8), 6, Vector2.Zero).noGravity = true;
						if (fallThrough && Main.tileSolidTop[tile.TileType]) continue;
						checkArea.X = i * -16 + cornerX;
						checkArea.Y = j * -16 + cornerY;
						//checkArea.DrawDebugOutline(new Vector2((i + minX) * 16, (j + minY) * 16), DustID.WaterCandle);
						if (tile != null && tile.HasSolidTile()) {
							if (tile.Slope != SlopeType.Solid) {
								Triangle draw = tileTriangles[(int)tile.Slope - 1];
								if (draw.Intersects(checkArea)) {
									draw.DrawDebugOutline(new Vector2((i + minX) * 16, (j + minY) * 16));
									colliding = true;
								}
							} else {
								Rectangle draw = tileRectangles[(int)tile.BlockType];
								if (draw.Intersects(checkArea)) {
									draw.DrawDebugOutline(new Vector2((i + minX) * 16, (j + minY) * 16));
									colliding = true;
								}
							}
						}
					}
				}
				return colliding;
			} else {
				for (int i = 0; i <= maxX; i++) {
					for (int j = 0; j <= maxY; j++) {
						Tile tile = Main.tile[i + minX, j + minY];
						if (fallThrough && Main.tileSolidTop[tile.TileType]) continue;
						if (tile != null && tile.HasSolidTile()) {
							checkArea.X = i * -16 + cornerX;
							checkArea.Y = j * -16 + cornerY;
							if (tile.Slope != SlopeType.Solid) {
								if (tileTriangles[(int)tile.Slope - 1].Intersects(checkArea)) return true;
							} else {
								if (tileRectangles[(int)tile.BlockType].Intersects(checkArea)) return true;
							}
						}
					}
				}
			}
			return false;
		}
		public static bool CanHitRay(Vector2 position, Vector2 target) {
			Vector2 diff = target - position;
			float length = diff.Length();
			return Raycast(position, diff, length) == length;
		}
		public static float Raycast(Vector2 position, Vector2 direction, float maxLength = float.PositiveInfinity) {
			if (direction == Vector2.Zero) throw new ArgumentException($"{nameof(direction)} may not be zero");
			float length = 0;
			Point tilePos = position.ToTileCoordinates();
			Vector2 tileSubPos = (position - tilePos.ToWorldCoordinates(0, 0)) / 16;
			float angle = direction.ToRotation();
			//OriginExtensions.DrawDebugLine(Vector2.Zero, GeometryUtils.Vec2FromPolar(16, angle), position, 27);
			double sin = Math.Sin(angle);
			double cos = Math.Cos(angle);
			static void DoLoopyThing(float currentSubPos, out float newSubPos, int currentTilePos, out int newTilePos, double direction) {
				newTilePos = currentTilePos;
				if (currentSubPos == 0 && direction < 0) {
					newSubPos = 1;
					newTilePos--;
				} else if (currentSubPos == 1 && direction > 0) {
					newSubPos = 0;
					newTilePos++;
				} else {
					newSubPos = currentSubPos;
				}
			}
			while (length < maxLength) {
				Vector2 next = RaycastStep(tileSubPos, sin, cos);
				if (next == tileSubPos) break;
				Tile tile = Framing.GetTileSafely(tilePos);
				bool doBreak = false;
				Vector2 diff = next - tileSubPos;
				float dist = diff.Length();
				if (tile.HasFullSolidTile()) {
					switch (tile.BlockType) {
						case BlockType.Solid:
						doBreak = true;
						break;
						case BlockType.HalfBlock:
						if (next.Y > 0.5f) {
							doBreak = true;
							//length += dist * (next.Y - 0.5f) * 8f;
						}
						break;
						case BlockType.SlopeDownLeft:
						break;
						case BlockType.SlopeDownRight:
						break;
						case BlockType.SlopeUpLeft:
						break;
						case BlockType.SlopeUpRight:
						break;
					}
				}
				if (doBreak) break;
				length += dist * 16;
				//Dust.NewDustPerfect(tilePos.ToWorldCoordinates(0, 0) + next * 16, 6, Vector2.Zero).noGravity = true;
				DoLoopyThing(next.X, out next.X, tilePos.X, out tilePos.X, cos);
				DoLoopyThing(next.Y, out next.Y, tilePos.Y, out tilePos.Y, sin);
				tile = Framing.GetTileSafely(tilePos);
				if (tile.HasFullSolidTile()) {
					switch (tile.BlockType) {
						case BlockType.Solid:
						doBreak = true;
						break;
						case BlockType.HalfBlock:
						if (next.Y > 0.5f) doBreak = true;
						break;
						case BlockType.SlopeDownLeft:
						if (next.X == 0 || next.Y == 1) doBreak = true;
						break;
						case BlockType.SlopeDownRight:
						if (next.X == 1 || next.Y == 1) doBreak = true;
						break;
						case BlockType.SlopeUpLeft:
						if (next.X == 0 || next.Y == 0) doBreak = true;
						break;
						case BlockType.SlopeUpRight:
						if (next.X == 1 || next.Y == 0) doBreak = true;
						break;
					}
				}
				if (!doBreak && (next.X == 0 || next.X == 1) && (next.Y == 0 || next.Y == 1)) {

				}
				if (doBreak) break;
				tileSubPos = next;
			}
			if (length > maxLength) return maxLength;
			return length;
		}
		static Vector2 RaycastStep(Vector2 pos, double sin, double cos) {
			if (cos == 0) return new(pos.X, sin > 0 ? 1 : 0);
			if (sin == 0) return new(cos > 0 ? 1 : 0, pos.Y);
			double slope = sin / cos;
			int xVlaue = cos > 0 ? 1 : 0;
			double yIntercept = pos.Y - slope * (pos.X - xVlaue);
			if (yIntercept >= 0 && yIntercept <= 1) return new Vector2(xVlaue, (float)yIntercept);
			int yVlaue = sin > 0 ? 1 : 0;
			double xIntercept = (pos.Y - yVlaue) / -slope + pos.X;
			return new Vector2((float)xIntercept, yVlaue);
		}
		public static Vector2 GetCenterProjectedPoint(Rectangle rect, Vector2 a) {
			Vector2 b = rect.Center();
			float s = (a.Y - b.Y) / (a.X - b.X);
			float v = s * rect.Width / 2;
			if (-rect.Height / 2 <= v && v <= rect.Height / 2) {
				if (a.X > b.X) {
					return new(rect.Right, b.Y + v);
				} else {
					return new(rect.Left, b.Y - v);
				}
			} else {
				v = (rect.Height / 2) / s;
				if (a.Y > b.Y) {
					return new(b.X + v, rect.Bottom);
				} else {
					return new(b.X - v, rect.Top);
				}
			}
		}
		/// <summary>
		/// returns the signed distance between a line segment and 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="point"></param>
		/// <param name="progressOnSegment"></param>
		/// <param name="onlyWithinSegment"></param>
		/// <returns></returns>
		public static float GetEdgeSignedDistance(Vector2 a, Vector2 b, Vector2 point, out float progressOnSegment, bool onlyWithinSegment = true) {
			Vector2 normal = b - a;
			normal.Normalize();
			normal = new(normal.Y, -normal.X);
			Vector2 point2 = point - normal;

			float t = ((a.X - point.X) * (point.Y - point2.Y) - (a.Y - point.Y) * (point.X - point2.X))
					/ ((a.X - b.X)     * (point.Y - point2.Y) - (a.Y - b.Y)     * (point.X - point2.X));
			progressOnSegment = t;
			if (onlyWithinSegment && (t < 0 || t > 1)) {
				return float.NaN;
			}

			float u = ((a.X - b.X) * (a.Y - point.Y)      - (a.Y - b.Y) * (a.X - point.X))
					/ ((a.X - b.X) * (point.Y - point2.Y) - (a.Y - b.Y) * (point.X - point2.X));
			return u;
		}
		/// <summary>
		/// checks if a convex polygon defined by a set of line segments intersects a rectangle
		/// uses <see cref="GetEdgeSignedDistance">
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="hitbox"></param>
		/// <returns></returns>
		public static bool PolygonIntersectsRect((Vector2 start, Vector2 end)[] lines, Rectangle hitbox) {
			int intersections = 0;
			Vector2 rectPos = hitbox.TopLeft();
			Vector2 rectSize = hitbox.Size();
			bool hasSize = hitbox.Width != 0 || hitbox.Height != 0;
			for (int i = 0; i < lines.Length; i++) {
				Vector2 a = lines[i].start;
				Vector2 b = lines[i].end;
				if (hasSize && Collision.CheckAABBvLineCollision2(rectPos, rectSize, a, b)) return true;
				float t = ((a.X - rectPos.X) * (rectPos.Y) - (a.Y - rectPos.Y) * (rectPos.X))
						/ ((a.X - b.X)       * (rectPos.Y) - (a.Y - b.Y)       * (rectPos.X));
				if (t < 0 || t > 1) continue;

				float u = ((a.X - b.X) * (a.Y - rectPos.Y) - (a.Y - b.Y) * (a.X - rectPos.X))
						/ ((a.X - b.X) * (rectPos.Y)       - (a.Y - b.Y) * (rectPos.X));
				if (u > 0 && u < 1) intersections++;
			}
			return intersections % 2 == 1;
		}
		/*public static bool PolygonIntersectsRect((Vector2 start, Vector2 end)[] lines, Rectangle hitbox) {
			float maxDist = 0;
			Vector2 rectPos = hitbox.TopLeft();
			Vector2 rectSize = hitbox.Size();
			Vector2 rectCenter = rectPos + rectSize * 0.5f;
			for (int i = 0; i < lines.Length; i++) {
				if (Collision.CheckAABBvLineCollision2(rectPos, rectSize, lines[i].start, lines[i].end)) return true;
				float dist = GetEdgeSignedDistance(lines[i].start, lines[i].end, rectCenter, out _, false);
				if (dist > maxDist) maxDist = dist;
			}
			return maxDist <= 0;
		}*/
		public static (Vector2 start, Vector2 end)[] FlipLines((Vector2 start, Vector2 end)[] lines) {
			var output = new (Vector2 start, Vector2 end)[lines.Length];
			for (int i = 0; i < lines.Length; i++) {
				output[i] = (lines[i].end, lines[i].start);
			}
			return output;
		}
		public static N Mul<N>(this bool flag, N value) where N : System.Numerics.INumber<N> {
			return flag ? value : N.Zero;
		}
		public static N Mul<N>(this N value, bool flag) where N : System.Numerics.INumber<N> {
			return flag ? value : N.Zero;
		}
	}
	public static class ItemExtensions {
		public static void CloneDefaultsKeepSlots(this Item self, int type) {
			ItemSlotSet slots = new(self);
			self.CloneDefaults(type);
			slots.Apply(self);
		}
		public static void DefaultToLauncher(this Item self, int damage, int useTime, int width, int height, bool autoReuse = false) {
			self.damage = damage;
			self.useTime = useTime;
			self.useAnimation = useTime;
			self.width = width;
			self.height = height;
			self.autoReuse = autoReuse;
			self.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			self.useStyle = ItemUseStyleID.Shoot;
			self.UseSound = SoundID.Item61;
			self.noMelee = true;
		}
		public static void DefaultToCanisterLauncher(this Item self, int damage, int useTime, float shootSpeed, int width, int height, bool autoReuse = false) {
			self.damage = damage;
			self.width = width;
			self.height = height;
			self.useTime = useTime;
			self.useAnimation = useTime;
			self.shootSpeed = shootSpeed;
			self.autoReuse = autoReuse;
			self.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			self.useAmmo = ModContent.ItemType<Resizable_Mine_One>();
			self.useStyle = ItemUseStyleID.Shoot;
			self.UseSound = SoundID.Item61;
			self.noMelee = true;
		}
		public static void DefaultToCanisterLauncher<T>(this Item self, int damage, int useTime, float shootSpeed, int width, int height, bool autoReuse = false) where T : ModProjectile {
			self.DefaultToCanisterLauncher(damage, useTime, shootSpeed, width, height, autoReuse);
			self.shoot = ModContent.ProjectileType<T>();
		}
		public static void DefaultToCanister(this Item self, int damage, int width = 16, int height = 28) {
			self.damage = damage;
			self.width = width;
			self.height = height;
			self.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			self.ammo = ModContent.ItemType<Resizable_Mine_One>();
			self.maxStack = 999;
			self.consumable = true;
		}
	}
	public static class NPCExtensions {
		public static void CopyBanner<TOther>(this ModNPC self) where TOther : ModNPC {
			int type = ModContent.NPCType<TOther>();
			self.Banner = type;
			self.BannerItem = BannerGlobalNPC.NPCToBannerItem[type];
		}
		public static NPCID.Sets.NPCBestiaryDrawModifiers HideInBestiary => new() {
			Hide = true
		};
		public static NPCID.Sets.NPCBestiaryDrawModifiers BestiaryWalkLeft => new() {
			Velocity = 1f
		};
		/// <summary>
		/// distinct from <see cref="HideInBestiary"/> so it can be easily found and removed when the NPC is implemented
		/// </summary>
		public static NPCID.Sets.NPCBestiaryDrawModifiers HideInBestiaryUnimplemented => new() {
			Hide = true
		};
		static RenderTarget2D renderTarget;
		public static void DrawBestiaryIcon(SpriteBatch spriteBatch, int type, Rectangle within, bool hovering = false, DrawData? stencil = null, Blend stencilColorBlend = Blend.SourceAlpha) {
			BestiaryEntry bestiaryEntry = BestiaryDatabaseNPCsPopulator.FindEntryByNPCID(type);
			if (bestiaryEntry?.Icon is not null) {
				if (renderTarget is not null && (renderTarget.Width != Main.screenWidth || renderTarget.Height != Main.screenHeight)) {
					renderTarget.Dispose();
					renderTarget = null;
				}
				renderTarget ??= new(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
				Rectangle screenPos = new((Main.screenWidth) / 2, (Main.screenHeight) / 2, (int)(within.Width), (int)(within.Height));
				screenPos.X -= screenPos.Width / 2;
				screenPos.Y -= screenPos.Height / 2;
				//within.Width 
				BestiaryUICollectionInfo info = new() {
					OwnerEntry = bestiaryEntry,
					UnlockState = BestiaryEntryUnlockState.CanShowDropsWithDropRates_4
				};
				EntryIconDrawSettings settings = new() {
					iconbox = screenPos,
					IsHovered = hovering,
					IsPortrait = false
				};
				bestiaryEntry.Icon.Update(info, screenPos, settings);
				SpriteBatchState state = spriteBatch.GetState();
				spriteBatch.Restart(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, Main.Rasterizer, null, Main.UIScaleMatrix, DepthStencilState.None);
				Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
				Main.graphics.GraphicsDevice.Clear(Color.Transparent);
				bestiaryEntry.Icon.Draw(info, spriteBatch, settings);
				if (stencil is not null) {
					DrawData stencilValue = stencil.Value;
					spriteBatch.Restart(spriteBatch.GetState(), blendState: new BlendState() {
						ColorSourceBlend = Blend.One,
						AlphaSourceBlend = Blend.One,
						ColorDestinationBlend = stencilColorBlend,
						AlphaDestinationBlend = Blend.SourceAlpha
					});
					stencilValue.position += screenPos.Center() / Main.UIScale;
					stencilValue.scale *= 2 / Main.UIScale;
					stencilValue.Draw(spriteBatch);
				}
				spriteBatch.Restart(state);
				RenderTargetUsage renderTargetUsage = Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage;
				Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
				Main.graphics.GraphicsDevice.SetRenderTarget(null);
				Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = renderTargetUsage;
				//origin.Y -= 8;
				screenPos = new(
					(int)((screenPos.X - screenPos.Width * 0.5f)),
					(int)((screenPos.Y - screenPos.Height * 0.5f)),
					(screenPos.Width * 2),
					(screenPos.Height * 2)
				);
				within.Width = (int)(within.Width / Main.UIScale);
				within.Height = (int)(within.Height / Main.UIScale);
				spriteBatch.Draw(renderTarget, within, screenPos, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
			}
		}
		public static bool HasRightDungeonWall(this NPCSpawnInfo spawnInfo, DungeonWallType wallType) {
			if (spawnInfo.Player.RollLuck(7) == 0) {
				return Main.rand.NextBool(3);
			} else {
				ushort wall = Main.tile[spawnInfo.SpawnTileX, spawnInfo.SpawnTileY].WallType;
				ushort wall2 = Main.tile[spawnInfo.SpawnTileX, spawnInfo.SpawnTileY - 1].WallType;
				switch (wallType) {
					case DungeonWallType.Brick:
					return wall is 7 or 8 or 9 || wall2 is 7 or 8 or 9;
					case DungeonWallType.Slab:
					return wall is 94 or 96 or 98 || wall2 is 94 or 96 or 98;
					case DungeonWallType.Tile:
					return wall is 95 or 97 or 99 || wall2 is 95 or 97 or 99;
				}
			}
			return true;
		}
		public enum DungeonWallType {
			Brick,
			Slab,
			Tile
		}
	}
	public static class ContentExtensions {
		public static void AddBanner(this ModNPC self) {
			self.Mod.AddContent(new Banner(self));
			BannerGlobalNPC.NPCTypesWithBanners.Add(self.GetType());
		}
		public static Dictionary<Type, Func<IItemDropRule, IEnumerable<IItemDropRule>>> ruleChildFinders = new() {
			[typeof(AlwaysAtleastOneSuccessDropRule)] = r => ((AlwaysAtleastOneSuccessDropRule)r).rules,
			[typeof(DropBasedOnExpertMode)] = r => [((DropBasedOnExpertMode)r).ruleForNormalMode, ((DropBasedOnExpertMode)r).ruleForExpertMode],
			[typeof(DropBasedOnMasterAndExpertMode)] = r => [((DropBasedOnMasterAndExpertMode)r).ruleForDefault, ((DropBasedOnMasterAndExpertMode)r).ruleForExpertmode, ((DropBasedOnMasterAndExpertMode)r).ruleForMasterMode],
			[typeof(DropBasedOnMasterMode)] = r => [((DropBasedOnMasterMode)r).ruleForDefault, ((DropBasedOnMasterMode)r).ruleForMasterMode],
			[typeof(FewFromRulesRule)] = r => ((FewFromRulesRule)r).options,
			[typeof(OneFromRulesRule)] = r => ((OneFromRulesRule)r).options,
			[typeof(SequentialRulesNotScalingWithLuckRule)] = r => ((SequentialRulesNotScalingWithLuckRule)r).rules,
			[typeof(SequentialRulesRule)] = r => ((SequentialRulesRule)r).rules,
		};
		public static T FindDropRule<T>(this IEnumerable<IItemDropRule> dropRules, Predicate<T> predicate) where T : class, IItemDropRule {
			foreach (var dropRule in dropRules) {
				if (dropRule is T rule && predicate(rule)) return rule;
				if (dropRule.ChainedRules.Select(c => c.RuleToChain).FindDropRule(predicate) is T foundRule) return foundRule;
				if (ruleChildFinders.TryGetValue(dropRules.GetType(), out var ruleChildFinder)) return ruleChildFinder(dropRule).FindDropRule(predicate);
			}
			return null;
		}
	}
}
