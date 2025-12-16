using AltLibrary.Common.AltBiomes;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Reflection;
using Origins.Tiles;
using Origins.Tiles.Banners;
using Origins.Tiles.Dusk;
using Origins.Tiles.Other;
using Origins.UI;
using Origins.Walls;
using PegasusLib;
using PegasusLib.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using ReLogic.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.UI.Chat;
using Terraria.Utilities;
using SetsTiles = Origins.OriginsSets.Tiles;
using SetsWalls = Origins.OriginsSets.Walls;

namespace Origins {
	#region classes
	public class LinkedQueue<T> : ICollection<T> {
		public int Count {
			get { return _items.Count; }
		}

		public void Enqueue(T item) {
			_items.AddLast(item);
		}

		public bool TryDequeue(out T item) {
			if (_items.First is null) {
				item = default;
				return false;
			}
			item = _items.First.Value;
			_items.RemoveFirst();

			return true;
		}
		public bool TryDequeueAs<TCast>(out TCast item) {
			if (TryDequeue(out T _item) && _item is TCast castItem) {
				item = castItem;
				return true;
			}
			item = default;
			return false;
		}
		public TCast DequeueAsOrDefaultTo<TCast>(TCast defaultValue) => TryDequeueAs(out TCast item) ? item : defaultValue;
		public T Dequeue() {
			if (_items.First is null)
				throw new InvalidOperationException("Queue empty.");

			T item = _items.First.Value;
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
			LLNodeEnumerator<T> enumerator = new(_items);
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
	public class DrawAnimationSwitching(Func<bool> @switch, DrawAnimation onFalse, DrawAnimation onTrue) : DrawAnimation {
		public override void Update() {
			if (@switch()) onTrue.Update();
			else onFalse.Update();
		}
		public override Rectangle GetFrame(Texture2D texture, int frameCounterOverride = -1) => @switch() ? onTrue.GetFrame(texture, frameCounterOverride) : onFalse.GetFrame(texture, frameCounterOverride);
	}
	public class NoDrawAnimation : DrawAnimation {
		public readonly static NoDrawAnimation AtAll = new NoDrawAnimation();
		public NoDrawAnimation() {
			Frame = 0;
			FrameCounter = 0;
			FrameCount = 1;
			TicksPerFrame = -1;
		}
		public override void Update() { }
		public override Rectangle GetFrame(Texture2D texture, int frameCounterOverride = -1) => texture.Frame();
	}
	public class DrawAnimationRandom : DrawAnimation {
		public DrawAnimationRandom(int frameCount, int ticksperframe) {
			Frame = Main.rand.Next(frameCount);
			FrameCounter = 0;
			FrameCount = frameCount;
			TicksPerFrame = ticksperframe;
		}
		public override void Update() {
			if (FrameCounter.CycleUp(TicksPerFrame)) {
				RangeRandom random = new(Main.rand, 0, FrameCount);
				random.Multiply(Frame, Frame + 1, 0);
				Frame = random.Get();
			}
		}
		public override Rectangle GetFrame(Texture2D texture, int frameCounterOverride = -1) {
			return texture.Frame(verticalFrames: FrameCount, frameY: Frame);
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
	[Obsolete($"Use PegasusLib.AutoLoadingAsset<T> instead")]
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
			triedLoading = true;
			assetPath = "";
			this.asset = new(asset);
			exists = true;
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
					asset ??= new Ref<Asset<T>>();
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
		public void Wait() {
			LoadAsset();
			asset.Value.Wait();
		}
		public static void Wait(params AutoLoadingAsset<T>[] assets) {
			for (int i = 0; i < assets.Length; i++) assets[i].LoadAsset();
			for (int i = 0; i < assets.Length; i++) assets[i].asset.Value.Wait();
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
		public readonly T a = a;
		public readonly T b = b;

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
		public ItemSlotSet(int headSlot = -2, int bodySlot = -2, int legSlot = -2, int beardSlot = -2, int backSlot = -2, int faceSlot = -2, int neckSlot = -2, int shieldSlot = -2, int wingSlot = -2, int waistSlot = -2, int shoeSlot = -2, int frontSlot = -2, int handOffSlot = -2, int handOnSlot = -2, int balloonSlot = -2) {
			this.headSlot = headSlot;
			this.bodySlot = bodySlot;
			this.legSlot = legSlot;
			this.beardSlot = beardSlot;
			this.backSlot = backSlot;
			this.faceSlot = faceSlot;
			this.neckSlot = neckSlot;
			this.shieldSlot = shieldSlot;
			this.wingSlot = wingSlot;
			this.waistSlot = waistSlot;
			this.shoeSlot = shoeSlot;
			this.frontSlot = frontSlot;
			this.handOffSlot = handOffSlot;
			this.handOnSlot = handOnSlot;
			this.balloonSlot = balloonSlot;
		}
		static void ApplySlot(ref int target, int value) {
			if (value != -2) target = value;
		}
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
		public readonly void Apply(Item item) {
			ApplySlot(ref item.headSlot, headSlot);
			ApplySlot(ref item.bodySlot, bodySlot);
			ApplySlot(ref item.legSlot, legSlot);
			ApplySlot(ref item.beardSlot, beardSlot);
			ApplySlot(ref item.backSlot, backSlot);
			ApplySlot(ref item.faceSlot, faceSlot);
			ApplySlot(ref item.neckSlot, neckSlot);
			ApplySlot(ref item.shieldSlot, shieldSlot);
			ApplySlot(ref item.wingSlot, wingSlot);
			ApplySlot(ref item.waistSlot, waistSlot);
			ApplySlot(ref item.shoeSlot, shoeSlot);
			ApplySlot(ref item.frontSlot, frontSlot);
			ApplySlot(ref item.handOffSlot, handOffSlot);
			ApplySlot(ref item.handOnSlot, handOnSlot);
			ApplySlot(ref item.balloonSlot, balloonSlot);
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
		public readonly void Apply(Player player) {
			ApplySlot(ref player.head, headSlot);
			ApplySlot(ref player.body, bodySlot);
			ApplySlot(ref player.legs, legSlot);
			ApplySlot(ref player.beard, beardSlot);
			ApplySlot(ref player.back, backSlot);
			ApplySlot(ref player.face, faceSlot);
			ApplySlot(ref player.neck, neckSlot);
			ApplySlot(ref player.shield, shieldSlot);
			ApplySlot(ref player.wings, wingSlot);
			ApplySlot(ref player.waist, waistSlot);
			ApplySlot(ref player.shoe, shoeSlot);
			ApplySlot(ref player.front, frontSlot);
			ApplySlot(ref player.handoff, handOffSlot);
			ApplySlot(ref player.handon, handOnSlot);
			ApplySlot(ref player.balloon, balloonSlot);
		}
	}
	public struct PlayerSlotSet {
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
		public int tailSlot;
		public PlayerSlotSet(int headSlot = -2, int bodySlot = -2, int legSlot = -2, int beardSlot = -2, int backSlot = -2, int faceSlot = -2, int neckSlot = -2, int shieldSlot = -2, int wingSlot = -2, int waistSlot = -2, int shoeSlot = -2, int frontSlot = -2, int handOffSlot = -2, int handOnSlot = -2, int balloonSlot = -2, int tailSlot = -2) {
			this.headSlot = headSlot;
			this.bodySlot = bodySlot;
			this.legSlot = legSlot;
			this.beardSlot = beardSlot;
			this.backSlot = backSlot;
			this.faceSlot = faceSlot;
			this.neckSlot = neckSlot;
			this.shieldSlot = shieldSlot;
			this.wingSlot = wingSlot;
			this.waistSlot = waistSlot;
			this.shoeSlot = shoeSlot;
			this.frontSlot = frontSlot;
			this.handOffSlot = handOffSlot;
			this.handOnSlot = handOnSlot;
			this.balloonSlot = balloonSlot;
			this.tailSlot = tailSlot;
		}
		static void ApplySlot(ref int target, int value) {
			if (value != -2) target = value;
		}
		public PlayerSlotSet(Player player) {
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
			tailSlot = player.tail;
		}
		public readonly void Apply(Player player) {
			ApplySlot(ref player.head, headSlot);
			ApplySlot(ref player.body, bodySlot);
			ApplySlot(ref player.legs, legSlot);
			ApplySlot(ref player.beard, beardSlot);
			ApplySlot(ref player.back, backSlot);
			ApplySlot(ref player.face, faceSlot);
			ApplySlot(ref player.neck, neckSlot);
			ApplySlot(ref player.shield, shieldSlot);
			ApplySlot(ref player.wings, wingSlot);
			ApplySlot(ref player.waist, waistSlot);
			ApplySlot(ref player.shoe, shoeSlot);
			ApplySlot(ref player.front, frontSlot);
			ApplySlot(ref player.handoff, handOffSlot);
			ApplySlot(ref player.handon, handOnSlot);
			ApplySlot(ref player.balloon, balloonSlot);
			ApplySlot(ref player.tail, tailSlot);
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
	public abstract class AnimatedModItem : ModItem {
		public abstract DrawAnimation Animation { get; }
		public virtual Color? GetGlowmaskTint(Player player) => null;
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			Texture2D texture = TextureAssets.Item[Item.type].Value;
			spriteBatch.Draw(texture, Item.position - Main.screenPosition, Animation.GetFrame(texture), lightColor, 0f, default, scale, SpriteEffects.None, 0f);
			return false;
		}
	}
	public interface ICustomDrawItem {
		void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin);
		bool DrawOverHand => false;
		bool BackHand => false;
		bool HideNormalDraw => true;
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
	public interface IInteractableNPC {
		bool NeedsSync => true;
		void Interact();
	}
	public interface IOnHitByNPC {
		public void OnHitByNPC(NPC attacker, NPC.HitInfo hit);
	}
	public interface IPostHitPlayer {
		public void PostHitPlayer(Player target, Player.HurtInfo hurtInfo);
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
		bool IsExploding { get; }
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
	public interface IDrawOverArmProjectile {
		DrawData GetDrawData();
	}
	public interface IPreDrawSceneProjectile {
		void PreDrawScene();
	}
	public interface IUnloadable {
		void Unload();
	}
	public interface IMinions {
		public List<int> BossMinions { get; }
	}
	public interface ISpecialFrameTile {
		public void SpecialFrame(int i, int j);
	}
	public interface ISpecialTilePreviewItem {
		public void DrawPreview();
	}
	public interface IComplexMineDamageWall {
		bool CanMine(Player self, Item item, int i, int j);
	}
	internal class SpecialTilePreviewOverlay() : Overlay(EffectPriority.High, RenderLayers.TilesAndNPCs), ILoadable {
		public override void Draw(SpriteBatch spriteBatch) => (Main.LocalPlayer?.HeldItem?.ModItem as ISpecialTilePreviewItem)?.DrawPreview();
		public override void Update(GameTime gameTime) { }
		public override void Activate(Vector2 position, params object[] args) {
			this.Opacity = 1;
			this.Mode = OverlayMode.Active;
		}
		public override void Deactivate(params object[] args) { }
		public override bool IsVisible() => true;
		public static void ForceActive() {
			if (Overlays.Scene[typeof(SpecialTilePreviewOverlay).FullName].Mode != OverlayMode.Active) {
				Overlays.Scene.Activate(typeof(SpecialTilePreviewOverlay).FullName, default);
			}
		}
		public void Load(Mod mod) {
			Overlays.Scene[GetType().FullName] = this;
		}
		public void Unload() { }
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
		public const int ActuallyNone = -1;
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
		public const int Balloon = 56 + 15;
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
		public static Vector2 GetKnockbackFromHit(this NPC.HitInfo hit, bool nerf = true, bool includeDirection = true, float xMult = 1, float yMult = -0.75f) {
			float knockback = hit.Knockback;
			if (nerf) {
				if (knockback > 8f) knockback = 8f + (knockback - 8f) * 0.9f;
				if (knockback > 10f) knockback = 10f + (knockback - 10f) * 0.8f;
				if (knockback > 12f) knockback = 12f + (knockback - 12f) * 0.7f;
				if (knockback > 14f) knockback = 14f + (knockback - 14f) * 0.6f;
				if (knockback > 16f) knockback = 16f;
			}
			if (hit.Crit) knockback *= 1.4f;
			return new(knockback * (includeDirection ? hit.HitDirection : 1) * xMult, knockback * yMult);
		}
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
		}
		public static event Action<Player, int> OnIncreaseMaxBreath;
		public static ref int GetCooldownCounter(this Player player, int cooldownCounter) {
			switch (cooldownCounter) {
				case -1:
				return ref player.immuneTime;
				case 0:
				case 1:
				case 3:
				case 4:
				return ref player.hurtCooldowns[cooldownCounter];
			}
			discard = 0;
			return ref discard;
		}
		static int discard = 0;
		public static Vector2 GetCompositeArmPosition(this Player player, bool back) {
			if (player.gravDir == -1) {
				if (back) {
					float rotation = player.compositeBackArm.rotation - MathHelper.PiOver2;
					Vector2 offset = rotation.ToRotationVector2();
					switch (player.compositeBackArm.stretch) {
						case Player.CompositeArmStretchAmount.Full:
						offset *= new Vector2(10f, 12f);
						break;
						case Player.CompositeArmStretchAmount.None:
						offset *= new Vector2(4f, 6f);
						break;
						case Player.CompositeArmStretchAmount.Quarter:
						offset *= new Vector2(6f, 8f);
						break;
						case Player.CompositeArmStretchAmount.ThreeQuarters:
						offset *= new Vector2(8f, 10f);
						break;
					}
					if (player.direction == -1) {
						offset += new Vector2(-6f, 2f);
					} else {
						offset += new Vector2(6f, 2f);
					}
					return player.MountedCenter + offset;
				} else {
					Vector2 offset = new(-1, 3 * player.direction);
					switch (player.compositeFrontArm.stretch) {
						case Player.CompositeArmStretchAmount.Full:
						offset.X *= 10f;
						break;
						case Player.CompositeArmStretchAmount.None:
						offset.X *= 4f;
						break;
						case Player.CompositeArmStretchAmount.Quarter:
						offset.X *= 6f;
						break;
						case Player.CompositeArmStretchAmount.ThreeQuarters:
						offset.X *= 8f;
						break;
					}
					offset = offset.RotatedBy(player.compositeFrontArm.rotation + MathHelper.PiOver2);
					if (player.direction == -1) {
						offset += new Vector2(4f, 2f);
					} else {
						offset += new Vector2(-4f, 2f);
					}
					return player.MountedCenter + offset;
				}
			} else {
				if (back) {
					return player.GetBackHandPosition(player.compositeBackArm.stretch, player.compositeBackArm.rotation);
				} else {
					return player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation);
				}
			}
		}
		public static bool HasItem(this Item[] collection, Predicate<Item> item) {
			for (int i = 0; i < collection.Length; i++) {
				if ((collection[i]?.stack ?? 0) > 0 && item(collection[i])) return true;
			}
			return false;
		}

		public static bool HasItemInAnyInventory(this Player player, Predicate<Item> item) {
			if (player.inventory.HasItem(item)) return true;
			if (player.armor.HasItem(item)) return true;
			if (player.dye.HasItem(item)) return true;
			if (player.miscEquips.HasItem(item)) return true;
			if (player.miscDyes.HasItem(item)) return true;
			if (player.bank.item.HasItem(item)) return true;
			if (player.bank2.item.HasItem(item)) return true;
			if (player.bank3.item.HasItem(item)) return true;
			if (player.bank4.item.HasItem(item)) return true;
			return false;
		}
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
		public static int RandomRound(this UnifiedRandom random, double value) {
			double amount = value % 1;
			value -= amount;
			if (amount == 0) return (int)value;
			if (random.NextDouble() < amount) {
				value++;
			}
			return (int)value;
		}
		public static int GetGoreSlot(this Mod mod, string name) {
			if (Main.netMode == NetmodeID.Server) return 0;
			if (mod.TryFind(name, out ModGore modGore)) return modGore.Type;
			return mod.TryFind(name.Split('/')[^1], out modGore) ? modGore.Type : 0;
		}
		public static int SpawnGoreByName(this Mod mod, IEntitySource source, Vector2 Position, Vector2 Velocity, string name, float Scale = 1) {
			if (Main.netMode == NetmodeID.Server) return 0;
			return Gore.NewGore(source, Position, Velocity, mod.GetGoreSlot(name), Scale);
		}
		public static int SpawnGoreByType(IEntitySource source, Vector2 Position, Vector2 Velocity, int type, float Scale = 1) {
			if (Main.netMode == NetmodeID.Server) return 0;
			return Gore.NewGore(source, Position, Velocity, type, Scale);
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
		public static bool Contains(this Rectangle area, int x, int y, int xPadding, int yPadding) {
			return (area.X <= x + xPadding) && (x < area.Right + xPadding) && (area.Y <= y + yPadding) && (y < area.Bottom + yPadding);
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
		public static Vector2 RotatedByRandom(this Vector2 vec, double maxRadians, UnifiedRandom rand) {
			return vec.RotatedBy(rand.NextDouble() * maxRadians - rand.NextDouble() * maxRadians);
		}
		public static Vector2 Quantize(this Vector2 vector, float size) {
			return (vector / size).Floor() * size;
		}
		public static Vector2 Normalized(this Vector2 vector, out float magnitude) {
			magnitude = vector.Length();
			if (magnitude > 0) vector /= magnitude;
			return vector;
		}
		public static Vector2 Abs(this Vector2 vector, out Vector2 signs) {
			signs = new(Math.Sign(vector.X), Math.Sign(vector.Y));
			return vector * signs;
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
		public static Vector2 Apply(this Vector2 value, SpriteEffects spriteEffects, Vector2 bounds) {
			if (spriteEffects.HasFlag(SpriteEffects.FlipHorizontally)) value.X = bounds.X - value.X;
			if (spriteEffects.HasFlag(SpriteEffects.FlipVertically)) value.Y = bounds.Y - value.Y;
			return value;
		}
		public static Vector2 TakeAverage(this List<Vector2> vectors) {
			Vector2 sum = default;
			int count = vectors.Count;
			for (int i = 0; i < vectors.Count; i++) {
				sum += vectors[i];
			}
			return count != 0 ? sum / count : sum;
		}
		public static void SwapClear<T>(ref List<T> working, ref List<T> finalized) {
			Utils.Swap(ref working, ref finalized);
			working.Clear();
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
			return new Color(MathHelper.Lerp(median, R, multiplier), MathHelper.Lerp(median, G, multiplier), MathHelper.Lerp(median, B, multiplier), value.A);
		}
		public static T[] BuildArray<T>(int length, params int[] nonNullIndeces) where T : new() {
			T[] o = new T[length];
			for (int i = 0; i < nonNullIndeces.Length; i++) {
				o[nonNullIndeces[i]] = new T();
			}
			return o;
		}
		public static bool Contains<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> value) {
			foreach (TSource element in value) {
				if (source.Contains(element)) return true;
			}
			return false;
		}
		public delegate bool TryGetter<TSource, TResult>(TSource source, out TResult result);
		public static IEnumerable<TResult> TrySelect<TSource, TResult>(this IEnumerable<TSource> source, TryGetter<TSource, TResult> tryGetter) {
			foreach (TSource item in source) {
				if (tryGetter(item, out TResult result)) yield return result;
			}
		}
		public static TResult[] CombineSets<TResult, T1, T2>(this T1[] set1, T2[] set2, Func<T1, T2, TResult> operation) {
			Debugging.Assert(set2.Length == set1.Length, new ArgumentException("Sets must have the same length"));
			TResult[] result = new TResult[set1.Length];
			for (int i = 0; i < result.Length; i++) {
				result[i] = operation(set1[i], set2[i]);
			}
			return result;
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
		public static void DrawFlamethrower(this Projectile projectile, Color color1, Color color2, Color color3, Color color4, bool flag = true, float scale = 1f) {
			const float num = 60f;
			const float num2 = 12f;
			const float fromMax = num + num2;
			Texture2D value = TextureAssets.Projectile[projectile.type].Value;
			float num3 = 0.35f;
			float num4 = 0.7f;
			float num5 = 0.85f;
			float num6 = ((projectile.localAI[0] > num - 10f) ? 0.175f : 0.2f);
			int verticalFrames = 7;
			float num9 = Utils.Remap(projectile.localAI[0], num, fromMax, 1f, 0f);
			float num10 = Math.Min(projectile.localAI[0], 20f);
			float num11 = Utils.Remap(projectile.localAI[0], 0f, fromMax, 0f, 1f);
			float num12 = Utils.Remap(num11, 0.2f, 0.5f, 0.25f, 1f);
			Rectangle rectangle = (flag ? value.Frame(1, verticalFrames, 0, 3) : value.Frame(1, verticalFrames, 0, (int)Utils.Remap(num11, 0.5f, 1f, 3f, 5f)));
			if (num11 >= 1f) return;
			for (int i = 0; i < 2; i++) {
				for (float num13 = 1f; num13 >= 0f; num13 -= num6) {
					Color obj = ((num11 < 0.1f) ? Color.Lerp(Color.Transparent, color1, Utils.GetLerpValue(0f, 0.1f, num11, clamped: true)) : ((num11 < 0.2f) ? Color.Lerp(color1, color2, Utils.GetLerpValue(0.1f, 0.2f, num11, clamped: true)) : ((num11 < num3) ? color2 : ((num11 < num4) ? Color.Lerp(color2, color3, Utils.GetLerpValue(num3, num4, num11, clamped: true)) : ((num11 < num5) ? Color.Lerp(color3, color4, Utils.GetLerpValue(num4, num5, num11, clamped: true)) : ((!(num11 < 1f)) ? Color.Transparent : Color.Lerp(color4, Color.Transparent, Utils.GetLerpValue(num5, 1f, num11, clamped: true))))))));
					float num14 = (1f - num13) * Utils.Remap(num11, 0f, 0.2f, 0f, 1f);
					Vector2 vector = projectile.Center - Main.screenPosition + projectile.velocity * (0f - num10) * num13;
					Color color5 = obj * num14;
					Color color6 = color5;
					if (flag) {
						color6.A = (byte)Math.Min(color5.A + 80f * num14, 255f);
					}
					float num15 = 1f / num6 * (num13 + 1f);
					float num16 = projectile.rotation + num13 * MathHelper.PiOver2 + Main.GlobalTimeWrappedHourly * num15 * 2f;
					float num17 = projectile.rotation - num13 * MathHelper.PiOver2 - Main.GlobalTimeWrappedHourly * num15 * 2f;
					switch (i) {
						case 0:
						Main.EntitySpriteDraw(value, vector + projectile.velocity * (0f - num10) * num6 * 0.5f, rectangle, color6 * num9 * 0.25f, num16 + (float)Math.PI / 4f, rectangle.Size() / 2f, num12 * scale, SpriteEffects.None);
						Main.EntitySpriteDraw(value, vector, rectangle, color6 * num9, num17, rectangle.Size() / 2f, num12 * scale, SpriteEffects.None);
						break;
						case 1:
						Main.EntitySpriteDraw(value, vector + projectile.velocity * (0f - num10) * num6 * 0.2f, rectangle, color5 * num9 * 0.25f, num16 + (float)Math.PI / 2f, rectangle.Size() / 2f, num12 * 0.75f * scale, SpriteEffects.None);
						Main.EntitySpriteDraw(value, vector, rectangle, color5 * num9, num17 + (float)Math.PI / 2f, rectangle.Size() / 2f, num12 * 0.75f * scale, SpriteEffects.None);
						break;
					}
				}
			}
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
		public static List<Vector2> FelisCatusSampling(Vector2 center, float range, int maxCount, float minSpread, float maxSpread) {
			List<Vector2> points = new();
			Queue<Vector2> newPoints = new();
			newPoints.Enqueue(center);
			int retries = 0;
			for (int i = 0; i < maxCount; i++) {
				if (!newPoints.Any()) break;
				Vector2 next = newPoints.Peek() + Vec2FromPolar(Main.rand.NextFloat(MathHelper.TwoPi), Main.rand.NextFloat(minSpread, maxSpread));
				if (!next.IsWithin(center, range) || points.Any((p) => p.DistanceSQ(next) < minSpread * minSpread)) {
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
		readonly ref struct SampleCells(int width, int height) {
			readonly int[,] cells = new int[width, height];
			public readonly int this[int x, int y] {
				get => cells[x, y] - 1;
				set => cells[x, y] = value + 1;
			}
			public readonly int this[Vector2 pos] {
				get => cells[(int)pos.X, (int)pos.Y] - 1;
				set => cells[(int)pos.X, (int)pos.Y] = value + 1;
			}
		}
		public static List<Vector2> PoissonDiskSampling(this UnifiedRandom rand, Rectangle area, float r, int k = 30) {
			float cellSize = r / MathF.Sqrt(2);
			static int Ceil(float value) => (int)float.Ceiling(value);
			SampleCells cells = new(Ceil(area.Width / cellSize), Ceil(area.Height / cellSize));
			Vector2 topLeft = area.TopLeft();
			List<Vector2> samples = [rand.NextVector2FromRectangle(area)];
			cells[(samples[0] - topLeft) / cellSize] = 0;
			List<Vector2> activeList = [samples[0]];
			while (activeList.Count > 0) {
				int index = rand.Next(activeList.Count);
				Vector2 currentSample = activeList[index];
				Vector2 newSample;
				for (int i = 0; i < k; i++) {
					newSample = currentSample + GeometryUtils.Vec2FromPolar(r * (1 + MathF.Sqrt(rand.NextFloat())), rand.NextFloat(MathHelper.TwoPi));
					if (area.Contains(newSample) && cells[(newSample - topLeft) / cellSize] == -1) {
						goto foundPoint;
					}
				}
				// no position found
				activeList.RemoveAt(index);
				continue;
				foundPoint:;
				cells[(newSample - topLeft) / cellSize] = samples.Count;
				samples.Add(newSample);
				activeList.Add(newSample);
			}
			return samples;
		}
		public static List<Vector2> PoissonDiskSampling(this UnifiedRandom rand, Rectangle area, Predicate<Vector2> customShape, float r, int k = 30) {
			float cellSize = r / MathF.Sqrt(2);
			static int Ceil(float value) => (int)float.Ceiling(value);
			SampleCells cells = new(Ceil(area.Width / cellSize), Ceil(area.Height / cellSize));
			Vector2 topLeft = area.TopLeft();
			List<Vector2> samples = [rand.NextVector2FromRectangle(area)];
			cells[(samples[0] - topLeft) / cellSize] = 0;
			List<Vector2> activeList = [samples[0]];
			while (activeList.Count > 0) {
				int index = rand.Next(activeList.Count);
				Vector2 currentSample = activeList[index];
				Vector2 newSample;
				for (int i = 0; i < k; i++) {
					newSample = currentSample + GeometryUtils.Vec2FromPolar(r * (1 + MathF.Sqrt(rand.NextFloat())), rand.NextFloat(MathHelper.TwoPi));
					if (area.Contains(newSample) && customShape(newSample) && cells[(newSample - topLeft) / cellSize] == -1) {
						goto foundPoint;
					}
				}
				// no position found
				activeList.RemoveAt(index);
				continue;
				foundPoint:;
				cells[(newSample - topLeft) / cellSize] = samples.Count;
				samples.Add(newSample);
				activeList.Add(newSample);
			}
			return samples;
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
		public static Projectile GetRelatedProjectile(this Projectile self, int index) {
			if (self.ai[index] >= 0 && self.owner < OriginSystem.projectilesByOwnerAndID.GetLength(0) && self.ai[index] < OriginSystem.projectilesByOwnerAndID.GetLength(1)) {
				return OriginSystem.projectilesByOwnerAndID[self.owner, (int)self.ai[index]];
			}
			return null;
		}
		public static Projectile GetRelatedProjectile_Depreciated(this Projectile self, int index) {
			int projIndex = Projectile.GetByUUID(self.owner, self.ai[index]);
			return Main.projectile.IndexInRange(projIndex) ? Main.projectile[projIndex] : null;
		}
		public static void DoFrames(this NPC self, int counterMax, Range frames) {
			int heightEtBuffer = self.frame.Height;
			self.frameCounter += 1;
			if (self.frameCounter >= counterMax) {
				self.frame.Y += heightEtBuffer;
				self.frameCounter = 0;
			}
			int frameCount = Main.npcFrameCount[self.type];
			if (self.frame.Y >= heightEtBuffer * frames.End.GetOffset(frameCount)) {
				self.frame.Y = heightEtBuffer * frames.Start.GetOffset(frameCount);
			} else if (self.frame.Y < heightEtBuffer * frames.Start.GetOffset(frameCount)) {
				self.frame.Y = heightEtBuffer * (frames.End.GetOffset(frameCount) - 1);
			}
		}
		public static void DoFrames(this NPC self, int counterMax) => self.DoFrames(counterMax, 0..Main.npcFrameCount[self.type]);
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
		public static T GetIfInRange<T>(this T[] array, int index, T fallback = default) {
			if (!array.IndexInRange(index)) return fallback;
			return array[index];
		}
		public static T GetIfInRange<T>(this T[,] array, int i, int j, T fallback = default) {
			if (!array.IndexInRange(i, j)) return fallback;
			return array[i, j];
		}
		public static bool IndexInRange<T>(this T[,] array, int i, int j) {
			return i >= 0 
				&& j >= 0
				&& i < array.GetLength(0)
				&& j < array.GetLength(1);
		}
		public static T GetIfInRange<T>(this List<T> array, int index, T fallback = default) {
			if (!array.IndexInRange(index)) return fallback;
			return array[index];
		}
		public static IEnumerable<int> GetTrueIndexes(this BitArray array) {
			for (int i = 0; i < array.Length; i++) {
				if (array[i]) yield return i;
			}
		}
		public static void Roll<T>(this T[] array, params T[] pushIn) {
			if (pushIn.Length == 0) return;
			for (int i = array.Length - 1; i >= 0; i--) {
				int index = i - pushIn.Length;
				array[i] = array.IndexInRange(index) ? array[index] : pushIn[i];
			}
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
		public static T WithOnFailedConditions<T>(this T rule, IItemDropRule ruleToChain, bool hideLootReport = false) where T : IItemDropRule {
			rule.OnFailedConditions(ruleToChain, hideLootReport);
			return rule;
		}
		public static T WithOnFailedRoll<T>(this T rule, IItemDropRule ruleToChain, bool hideLootReport = false) where T : IItemDropRule {
			rule.OnFailedRoll(ruleToChain, hideLootReport);
			return rule;
		}
		public static T WithOnSuccess<T>(this T rule, IItemDropRule ruleToChain, bool hideLootReport = false) where T : IItemDropRule {
			rule.OnSuccess(ruleToChain, hideLootReport);
			return rule;
		}

		public static ItemDropAttemptResult ResolveRule(IItemDropRule rule, DropAttemptInfo info) {
			if (!rule.CanDrop(info)) {
				ItemDropAttemptResult itemDropAttemptResult = default;
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
		public static void DrawLightningArc(this SpriteBatch spriteBatch, Vector2[] positions, Texture2D texture = null, float scale = 1f, Vector2 offset = default, params (float scale, Color color)[] colors) {
			texture ??= TextureAssets.Extra[33].Value;
			Vector2 size;
			int colorLength = colors.Length;
			DelegateMethods.f_1 = 1;
			Rectangle screenBounds = new(
				0,
				0,
				Main.screenWidth,
				Main.screenHeight
			);
			int extraSize = (int)(Math.Max(texture.Width, texture.Height) * scale);
			screenBounds.Inflate(extraSize, extraSize);
			for (int colorIndex = 0; colorIndex < colorLength; colorIndex++) {
				size = new Vector2(scale) * colors[colorIndex].scale;
				DelegateMethods.c_1 = colors[colorIndex].color;
				for (int i = positions.Length; --i > 0;) {
					Vector2 a = positions[i] + offset;
					Vector2 b = positions[i - 1] + offset;
					if (!Collision.CheckAABBvLineCollision(screenBounds.TopLeft(), screenBounds.Size(), a, b)) continue;
					Utils.DrawLaser(spriteBatch, texture, a, b, size, DelegateMethods.LightningLaserDraw);
				}
			}
		}
		public static void DrawLightningArcBetween(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, float sineMult, float precision = 0.1f, params (float scale, Color color)[] colors) {
			Rectangle screen = new(0, 0, Main.screenWidth, Main.screenHeight);
			if (!screen.Contains(start) && !screen.Contains(end)) {
				return;
			}
			List<Vector2> positions = [];
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
				default,
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
		public static void SetupRubblemakerClone<TItem>(this FlexibleTileWand wand, ModTile tile, params int[] variants) where TItem : ModItem {
			TileObjectData tileObjectData = TileObjectData.GetTileData(tile.Type, 0, 0);
			tileObjectData.RandomStyleRange = 0;
			for (int i = 0; i < tileObjectData.AlternatesCount; i++) {
				TileObjectData.GetTileData(tile.Type, 0, 0).RandomStyleRange = 0;
			}
			tileObjectData.RandomStyleRange = 0;
			wand.AddVariations(ModContent.ItemType<TItem>(), tile.Type, variants);
			tile.RegisterItemDrop(ModContent.ItemType<TItem>());
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
			switch (TileID.Sets.HasSlopeFrames[tile.TileType] ? BlockType.Solid : tile.BlockType) {
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
		public delegate T Lerp<T>(T a, T b, float value);
		public static T Bezier<T>(this Lerp<T> lerp, float progress, params T[] handles) {
			do {
				T[] nextHandles = new T[handles.Length - 1];
				for (int i = 0; i < nextHandles.Length; i++) {
					nextHandles[i] = lerp(handles[i], handles[i + 1], progress);
				}
				handles = nextHandles;
			} while (handles.Length > 1);
			return handles[0];
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
		public static WeightedRandom<int> GetAllPrefixes(Item item, UnifiedRandom rand, params (PrefixCategory category, double weight)[] prefixCategories) {
			WeightedRandom<int> wr = new(rand);
			for (int i = 0; i < prefixCategories.Length; i++) {
				(PrefixCategory category, double weight) = prefixCategories[i];
				foreach (int pre in Item.GetVanillaPrefixes(category)) {
					wr.Add(pre, weight);
				}
				foreach (ModPrefix modPrefix in PrefixLoader.GetPrefixesInCategory(category).Where((ModPrefix x) => x.CanRoll(item))) {
					wr.Add(modPrefix.Type, modPrefix.RollChance(item) * weight);
				}
			}
			return wr;
		}
		public static WeightedRandom<int> GetAllPrefixes(Item item, UnifiedRandom rand, params (PrefixCategory category, bool[] set, double weight)[] prefixCategories) {
			WeightedRandom<int> wr = new(rand);
			for (int i = 0; i < prefixCategories.Length; i++) {
				(PrefixCategory category, bool[] set, double weight) = prefixCategories[i];
				foreach (int pre in Item.GetVanillaPrefixes(category)) {
					if (set[pre]) wr.Add(pre, weight);
				}
				foreach (ModPrefix modPrefix in PrefixLoader.GetPrefixesInCategory(category).Where((ModPrefix x) => x.CanRoll(item))) {
					if (set[modPrefix.Type]) wr.Add(modPrefix.Type, modPrefix.RollChance(item) * weight);
				}
			}
			return wr;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="prefixCategories">Weight functions default to "_ => 1" if null</param>
		/// <returns></returns>
		public static WeightedRandom<int> GetAllPrefixes(Item item, UnifiedRandom rand, params (PrefixCategory category, Func<int, double> weightFunction)[] prefixCategories) {
			WeightedRandom<int> wr = new(rand);
			for (int i = 0; i < prefixCategories.Length; i++) {
				(PrefixCategory category, Func<int, double> weightFunction) = prefixCategories[i];
				weightFunction ??= _ => 1;
				foreach (int pre in Item.GetVanillaPrefixes(category)) {
					double weight = weightFunction(pre);
					if (weight > 0) wr.Add(pre, weight);
				}
				foreach (ModPrefix modPrefix in PrefixLoader.GetPrefixesInCategory(category).Where((ModPrefix x) => x.CanRoll(item))) {
					double weight = weightFunction(modPrefix.Type);
					if (weight > 0) wr.Add(modPrefix.Type, modPrefix.RollChance(item) * weight);
				}
			}
			return wr;
		}
		public static WeightedRandom<int> AccessoryOrSpecialPrefix(this Item item, UnifiedRandom rand, params PrefixCategory[] prefixCategories) {
			(PrefixCategory category, bool[] set, double weight)[] categories = new (PrefixCategory category, bool[] set, double weight)[prefixCategories.Length + 1];
			for (int i = 0; i < prefixCategories.Length; i++) {
				categories[i] = (prefixCategories[i], Origins.SpecialPrefix, 1);
			}
			categories[^1] = (PrefixCategory.Accessory, PrefixID.Sets.Factory.CreateBoolSet(true), 1);
			return GetAllPrefixes(item, rand, categories);
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
					if (FontAssets.MouseText?.IsLoaded ?? false) {
						Texture2D strikeTexture = ModContent.Request<Texture2D>("Origins/Textures/Strikethrough_Font", AssetRequestMode.ImmediateLoad).Value;
						DynamicSpriteFont baseFont = FontAssets.MouseText.Value;
						strikethroughFont = new DynamicSpriteFont(baseFont.CharacterSpacing, baseFont.LineSpacing, baseFont.DefaultCharacter);
						Type dict = _SpriteCharacters.FieldType;
						_SpriteCharacters.SetValue(
							strikethroughFont,
							dict.GetConstructor([typeof(IDictionary<,>).MakeGenericType(dict.GenericTypeArguments)])
							.Invoke([_SpriteCharacters.GetValue(baseFont)])
						);
						object enumerator = dict.GetMethod(nameof(Dictionary<int, int>.GetEnumerator)).Invoke(_SpriteCharacters.GetValue(baseFont), []);
						Type enumType = enumerator.GetType();
						MethodInfo moveNext = enumType.GetMethod(nameof(Dictionary<int, int>.Enumerator.MoveNext));
						PropertyInfo current = enumType.GetProperty(nameof(Dictionary<int, int>.Enumerator.Current));
						PropertyInfo key = typeof(KeyValuePair<,>).MakeGenericType(dict.GenericTypeArguments).GetProperty(nameof(KeyValuePair<int, int>.Key));
						PropertyInfo prop = dict.GetProperty("Item");
						object sfFont = _SpriteCharacters.GetValue(strikethroughFont);

						Type spriteCharacterData = dict.GenericTypeArguments[1];
						ConstructorInfo ctor = spriteCharacterData.GetConstructors()[0];
						FieldInfo glyphField = spriteCharacterData.GetField("Glyph");
						FieldInfo paddingField = spriteCharacterData.GetField("Padding");
						FieldInfo kerningField = spriteCharacterData.GetField("Kerning");
						while ((bool)moveNext.Invoke(enumerator, [])) {
							object[] index = [key.GetValue(current.GetValue(enumerator))];
							object value = prop.GetValue(sfFont, index);
							Rectangle glyph = (Rectangle)glyphField.GetValue(value);
							Rectangle padding = (Rectangle)paddingField.GetValue(value);
							Vector3 kerning = (Vector3)kerningField.GetValue(value);
							padding.X = -4;
							padding.Y = 0;
							padding.Height = 0;
							glyph.X = 0;
							glyph.Y = -8;// 2 - glyph.Height / 2;
							glyph.Width += (int)(kerning.Y + kerning.Z + 4f);
							glyph.Height = 16;
							prop.SetValue(sfFont, ctor.Invoke([
								strikeTexture,
								glyph,
								padding,
								kerning,
							]), index);
						}
						_DefaultCharacterData.SetValue(strikethroughFont, _DefaultCharacterData.GetValue(baseFont));
					} else {
						return FontAssets.MouseText?.Value;
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
		public static void CreateEvilShimmerCycle(this RecipeGroup recipeGroup, int corrupt, int crimson, int defiled, int riven, int ashen) {
			List<string> exceptions = [];
			if (!recipeGroup.ValidItems.Contains(corrupt)) exceptions.Add(Lang.GetItemNameValue(corrupt));
			if (!recipeGroup.ValidItems.Contains(crimson)) exceptions.Add(Lang.GetItemNameValue(crimson));
			if (!recipeGroup.ValidItems.Contains(defiled)) exceptions.Add(Lang.GetItemNameValue(defiled));
			if (!recipeGroup.ValidItems.Contains(riven)) exceptions.Add(Lang.GetItemNameValue(riven));
			if (!recipeGroup.ValidItems.Contains(ashen)) exceptions.Add(Lang.GetItemNameValue(ashen));
			if (exceptions.Count > 0) throw new ArgumentException("Invalid arguments, item(s) not present in recipe group: ", $"[{string.Join(", ", exceptions)}]");

			if (corrupt != crimson) ItemID.Sets.ShimmerTransformToItem[corrupt] = crimson;
			ItemID.Sets.ShimmerTransformToItem[crimson] = defiled;
			ItemID.Sets.ShimmerTransformToItem[defiled] = riven;
			ItemID.Sets.ShimmerTransformToItem[riven] = ashen;
			ItemID.Sets.ShimmerTransformToItem[ashen] = corrupt;
			int last = ashen;
			foreach (int item in recipeGroup.ValidItems) {
				if (item == corrupt || item == crimson || item == defiled || item == riven || item == ashen) continue;
				InsertIntoShimmerCycle(item, last);
				last = item;
			}
		}
		public static void RegisterForUnload(this IUnloadable unloadable) {
			Origins.unloadables.Add(unloadable);
		}
		public static string GetDefaultTMLName(this Type type) => PegasusExt.GetDefaultTMLName(type);
		public static string GetDefaultTMLName(this Type type, string suffix) => PegasusExt.GetDefaultTMLName(type) + suffix;
		public static IEnumerable<T> GetFlags<T>(this T value) where T : struct, Enum {
			T[] possibleFlags = Enum.GetValues<T>();
			for (int i = 0; i < possibleFlags.Length; i++) {
				if (possibleFlags[i].Equals(default(T))) continue;
				if (value.HasFlag(possibleFlags[i])) yield return possibleFlags[i];
			}
		}
		public static IBestiaryInfoElement GetBestiaryFlavorText(this ModNPC npc, bool better = false, bool alt = false) {
			string key = $"Mods.{npc.Mod.Name}.Bestiary.{npc.Name}";
			Language.GetOrRegister(key, () => "bestiary text here");
			if (better || alt) {
				if (alt) {
					string altKey = key + "_Alt";
					Language.GetOrRegister(altKey, () => "alt bestiary text here");
					return new GaslightingFlavorTextBestiaryInfoElement(key, altKey);
				} else return new BetterFlavorTextBestiaryInfoElement(key);
			}
			return new FlavorTextBestiaryInfoElement(key);
		}
		public static FlavorTextBestiaryInfoElement GetBestiaryFlavorText(int npcID) {
			string flavorText = "";
			if (npcID < NPCID.Count) {
				string key = Lang.GetNPCName(npcID).Key;
				key = key.Replace("NPCName.", "");
				string text = "Bestiary_FlavorText.npc_" + key;
				if (Language.Exists(text)) flavorText = text;
			} else {
				// get modded bestiary text, idk how
			}
			return new(flavorText);
		}
		public static void KillsCountTowardsNPC<TOther>(this NPC npc, BestiaryEntry bestiaryEntry) where TOther : ModNPC => npc.KillsCountTowardsNPC(ModContent.NPCType<TOther>(), bestiaryEntry);
		public static void KillsCountTowardsNPC(this NPC npc, int other, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.UIInfoProvider = new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[other], true);
			ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[npc.type] = ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[other];
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
		public static void AddChambersiteTileConversions(this AltBiome biome, int tile) {
			biome.AddTileConversion(tile, ModContent.TileType<Chambersite_Ore>(), extraFunctions: false);

			for (int i = 0; i < SetsTiles.GemTilesToChambersite.Length; i++) {
				if (SetsTiles.GemTilesToChambersite[i]) biome.AddTileConversion(tile, i, oneWay: true, extraFunctions: false);
			}
		}
		public static void AddChambersiteWallConversions(this AltBiome biome, int wall) {
			biome.AddWallConversions(wall, ModContent.WallType<Chambersite_Stone_Wall>());
			biome.AddWallConversions(wall, SetsWalls.GemWallsToChambersite);
		}
		public static void AddChambersiteConversions(this AltBiome biome, int tile, int wall) {
			if (tile != -1) biome.AddChambersiteTileConversions(tile);
			if (wall != -1) biome.AddChambersiteWallConversions(wall);
		}
		public static void AddEvilConversions(this AltBiome biome) {
			for (int i = 0; i < SetsTiles.ExposedGemsToChambersite.Length; i++) {
				if (SetsTiles.ExposedGemsToChambersite[i]) biome.AddTileConversion(ModContent.TileType<Chambersite>(), i, false, false, false);
			}

			biome.AddTileConversion(ModContent.TileType<Bleeding_Obsidian>(), TileID.Obsidian, false, false, false);
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
		public static bool Cooldown(ref this float value, float to = 0, float rate = 1) => value.Cooldown<float>(to, rate);
		public static bool Cooldown(ref this int value, int to = 0, int rate = 1) => value.Cooldown<int>(to, rate);
		public static bool Cooldown<N>(ref this N value, N to, N rate) where N : struct, INumber<N> {
			if (value > to) {
				value -= rate;
				if (value <= to) {
					value = to;
					return true;
				}
			}
			return false;
		}
		public static bool Warmup(ref this float value, float to, float rate = 1) => value.Warmup<float>(to, rate);
		public static bool Warmup(ref this int value, int to, int rate = 1) => value.Warmup<int>(to, rate);
		public static bool Warmup<N>(ref this N value, N to, N rate) where N : struct, INumber<N> {
			if (value < to) {
				value += rate;
				if (value >= to) {
					value = to;
					return true;
				}
			}
			return false;
		}
		public static bool CycleDown(ref this float value, float from, float rate = 1) => value.CycleDown<float>(from, rate);
		public static bool CycleDown(ref this int value, int from, int rate = 1) => value.CycleDown<int>(from, rate);
		public static bool CycleDown<N>(ref this N value, N from, N rate) where N : struct, INumber<N> {
			value -= rate;
			if (value >= N.Zero) {
				value += from;
				return true;
			}
			return false;
		}
		public static bool CycleUp(ref this float value, float to, float rate = 1) => value.CycleUp<float>(to, rate);
		public static bool CycleUp(ref this int value, int to, int rate = 1) => value.CycleUp<int>(to, rate);
		public static bool CycleUp<N>(ref this N value, N to, N rate) where N : struct, INumber<N> {
			value += rate;
			if (value >= to) {
				value -= to;
				return true;
			}
			return false;
		}
		public static void DrawDebugOutline(this Rectangle area, Vector2 offset = default, int dustType = DustID.Torch, Color color = default) {
			Vector2 pos = area.TopLeft() + offset;
			float amt = 20; // as to try to not spawn to many dusts
			for (float c = 0; c < area.Width; c += area.Width / amt) {
				Dust.NewDustPerfect(pos + new Vector2(c, 0), dustType, Vector2.Zero, newColor: color).noGravity = true;
			}
			for (float c = 0; c < area.Height; c += area.Height / amt) {
				Dust.NewDustPerfect(pos + new Vector2(0, c), dustType, Vector2.Zero, newColor: color).noGravity = true;
			}
			for (float c = 0; c < area.Width; c += area.Width / amt) {
				Dust.NewDustPerfect(pos + new Vector2(c, area.Height), dustType, Vector2.Zero, newColor: color).noGravity = true;
			}
			for (float c = 0; c < area.Height; c += area.Height / amt) {
				Dust.NewDustPerfect(pos + new Vector2(area.Width, c), dustType, Vector2.Zero, newColor: color).noGravity = true;
			}
		}
		public static void DrawDebugOutlineSprite(this Rectangle area, Color color, Vector2 offset = default, bool useScreenPos = true) {
			DrawDebugLineSprite(area.TopLeft(), area.TopRight(), color, offset, useScreenPos);
			DrawDebugLineSprite(area.TopLeft(), area.BottomLeft(), color, offset, useScreenPos);
			DrawDebugLineSprite(area.TopRight(), area.BottomRight(), color, offset, useScreenPos);
			DrawDebugLineSprite(area.BottomLeft(), area.BottomRight(), color, offset, useScreenPos);
		}
		public static void DrawDebugOutline(this Triangle area, Vector2 offset = default, int dustType = DustID.Torch, Color color = default) {
			for (float c = 0; c <= 1; c += 0.125f) {
				Dust.NewDustPerfect(offset + Vector2.Lerp(area.a, area.b, c), dustType, Vector2.Zero, newColor: color).noGravity = true;
			}
			for (float c = 0; c <= 1; c += 0.125f) {
				Dust.NewDustPerfect(offset + Vector2.Lerp(area.b, area.c, c), dustType, Vector2.Zero, newColor: color).noGravity = true;
			}
			for (float c = 0; c <= 1; c += 0.125f) {
				Dust.NewDustPerfect(offset + Vector2.Lerp(area.c, area.a, c), dustType, Vector2.Zero, newColor: color).noGravity = true;
			}
		}
		public static void DrawDebugOutlineSprite(this Triangle area, Color color, Vector2 offset = default, bool useScreenPos = true) {
			DrawDebugLineSprite(area.a, area.b, color, offset, useScreenPos);
			DrawDebugLineSprite(area.b, area.c, color, offset, useScreenPos);
			DrawDebugLineSprite(area.c, area.a, color, offset, useScreenPos);
		}
		public static void DrawDebugLine(Vector2 a, Vector2 b, Vector2 offset = default, int dustType = DustID.Torch, Color color = default) {
			for (float c = 0; c <= 1; c += 0.125f) {
				Dust.NewDustPerfect(offset + Vector2.Lerp(a, b, c), dustType, Vector2.Zero, newColor: color).noGravity = true;
			}
		}
		public static void DrawDebugLineSprite(Vector2 a, Vector2 b, Color color, Vector2 offset = default, bool useScreenPos = false) {
			Vector2 diff = b - a;
			Vector2 position = a + offset;
			if (useScreenPos) position -= Main.screenPosition;

			Main.spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				position,
				new Rectangle(0, 0, 2, 2),
				color,
				diff.ToRotation(),
				Vector2.UnitY,
				new Vector2(diff.Length() * 0.5f, 1 * 0.5f),
				0,
			0);
		}
		public static Vector2 DrawDebugTextAbove(this SpriteBatch spritebatch, object obj, Vector2 position, Vector2? origin = null, Color? color = null) {
			string text = obj.ToString();
			DynamicSpriteFont font = FontAssets.ItemStack.Value;
			Vector2 spacing = font.MeasureString(text) / 2;
			Vector2 orig = origin ?? Vector2.Zero;
			orig.Y += spacing.Y;
			return ChatManager.DrawColorCodedStringWithShadow(spritebatch, font, text, position - spacing, color ?? Color.White, 0, orig, Vector2.One);
		}
		public static void DrawConstellationLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, float width = 20, float distort = 20, float waveSpeed = 0.03f) {
			MiscShaderData shader = GameShaders.Misc["Origins:Constellation"];
			shader.UseSaturation(width);
			shader.UseOpacity(distort);

			Asset<Texture2D> space = ModContent.Request<Texture2D>("Origins/Items/Weapons/Ranged/Constellation_Fill");

			Vector2 screenPos = Main.screenPosition / new Vector2(Main.screenWidth, Main.screenHeight);
			Rectangle source = new Rectangle(0, 0, space.Width(), space.Height());

			shader.UseImage0(space);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			int minX = (int)Math.Min(start.X, end.X);
			int minY = (int)Math.Min(start.Y, end.Y);
			int maxX = (int)Math.Max(start.X, end.X);
			int maxY = (int)Math.Max(start.Y, end.Y);

			int offset = (int)20;
			Rectangle dest = new Rectangle(
				minX - offset,
				minY - offset,
				maxX - minX + offset * 2,
				maxY - minY + offset * 2
			);
			float speed = 0.1f;
			shader.UseColor(speed, speed, 0f);
			shader.UseShaderSpecificData(
			new Vector4(screenPos, dest.Width, dest.Height)
			);

			Vector2 uv1 = dest.MapUV(start.ToPoint());
			Vector2 uv2 = dest.MapUV(end.ToPoint());
			Vector4 pack = new Vector4(uv1.X, uv1.Y, uv2.X, uv2.Y);
			shader.Shader.Parameters["uNodePositions"].SetValue(pack);

			shader.Apply();

			DrawData rect = new(
				space.Value,
				dest,
				source,
				Color.White,
				0f,
				Vector2.Zero,
				SpriteEffects.None
			);

			rect.Draw(spriteBatch);

			Main.spriteBatch.Restart();
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
		public static LocalizedText CombineTooltips(params LocalizedText[] parts) {
			switch (parts.Length) {
				case 0:
				return LocalizedText.Empty;

				case 1:
				return parts[0];

				default:
				return Language.GetOrRegister("Mods.Origins.Items.CombineTooltips").WithFormatArgs(parts[0], CombineTooltips(parts[1..]));
			}
		}
		public static LocalizedText CombineWithAnd(params LocalizedText[] parts) {
			if (parts.Length == 2) return Language.GetOrRegister("Mods.Origins.Conditions.And").WithFormatArgs(parts[0], parts[1]);
			return CombineWithAndInternal(parts);
		}
		static LocalizedText CombineWithAndInternal(params LocalizedText[] parts) {
			switch (parts.Length) {
				case 0:
				return LocalizedText.Empty;

				case 1:
				return parts[0];

				case 2:
				return Language.GetOrRegister("Mods.Origins.Conditions.CommaAnd").WithFormatArgs(parts[0], CombineWithAndInternal(parts[1..]));

				default:
				return Language.GetOrRegister("Mods.Origins.Conditions.Comma").WithFormatArgs(parts[0], CombineWithAndInternal(parts[1..]));
			}
		}
		public static LocalizedText GetRandomText(string key) {
			int i = 0;
			while (Language.Exists($"{key}.{i}")) i++;
			return Language.GetText($"{key}.{Main.rand.Next(i)}");
		}
		public static LanguageTree GetLocalizationTree(this ILocalizedModType self)
			=> TextUtils.LanguageTree.Find(self.Mod.GetLocalizationKey($"{self.LocalizationCategory}.{self.Name}"));
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
		public static OriginPlayer OriginPlayer(this Player player) => player.GetModPlayer<OriginPlayer>();
		public static bool DoHoming(this Player player, Func<Entity, bool> selector, bool canPvP = true) {
			bool foundTarget = false;
			foreach (NPC target in Main.ActiveNPCs) {
				if (target.CanBeChasedBy(player)) {
					foundTarget |= selector(target);
				}
			}
			if (!foundTarget && player.hostile && canPvP) {
				foreach (Player target in Main.ActivePlayers) {
					if (!target.dead && target.hostile && target.team != player.team) {
						foundTarget |= selector(target);
					}
				}
			}
			return foundTarget;
		}
		public static void DoCustomCombatText(Rectangle location, Color color, int amount, bool dramatic = false, bool dot = false, bool fromFriendly = true) {
			CombatText.NewText(location, color, amount, dramatic, dot);
			if (Main.netMode != NetmodeID.SinglePlayer) {
				if (fromFriendly) color *= 0.4f;
				ModPacket packet = Origins.instance.GetPacket();
				packet.Write(Origins.NetMessageType.custom_combat_text);

				packet.Write(location.X);
				packet.Write(location.Y);
				packet.Write(location.Width);
				packet.Write(location.Height);

				packet.Write(color.PackedValue);

				packet.Write(amount);

				packet.Write(dramatic);

				packet.Write(dot);
				packet.Send(-1, Main.myPlayer);
			}
		}
		public static ref int BuilderToggleState<TToggle>(this Player player) where TToggle : BuilderToggle {
			return ref player.builderAccStatus[ModContent.GetInstance<TToggle>().Type];
		}
		public static Vector2 ApplyToOrigin(this SpriteEffects spriteEffects, Vector2 origin, Rectangle frame) {
			if (spriteEffects.HasFlag(SpriteEffects.FlipHorizontally)) {
				origin.X = frame.Width - origin.X;
			}
			if (spriteEffects.HasFlag(SpriteEffects.FlipVertically)) {
				origin.Y = frame.Height - origin.Y;
			}
			return origin;
		}
		public static void Deconstruct(this Vector2 vector, out float X, out float Y) {
			X = vector.X;
			Y = vector.Y;
		}
		public static void Deconstruct(this Vector3 vector, out float X, out float Y, out float Z) {
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
		}
		public static void Deconstruct(this Point vector, out int X, out int Y) {
			X = vector.X;
			Y = vector.Y;
		}
		public static void Deconstruct(this Vector4 vector, out Vector2 XY, out Vector2 ZW) {
			XY = vector.XY();
			ZW = vector.ZW();
		}
		public static SpriteBatchState FixedCulling(this SpriteBatchState state) {
			state.rasterizerState.CullMode = CullMode.None;
			return state;
		}
		public static string GetInternalName(this RecipeGroup recipeGroup) {
			foreach (KeyValuePair<string, int> item in RecipeGroup.recipeGroupIDs) {
				if (item.Value == recipeGroup.RegisteredId) return item.Key;
			}
			return null;
		}
		public static void UseOldRenderTargets(this SpriteBatch spriteBatch, RenderTargetBinding[] oldRenderTargets) {
			bool anyOldTargets = (oldRenderTargets?.Length ?? 0) != 0;
			RenderTargetUsage[] renderTargetUsage = [];
			try {
				if (anyOldTargets) {
					renderTargetUsage = new RenderTargetUsage[oldRenderTargets.Length];
					for (int i = 0; i < oldRenderTargets.Length; i++) {
						RenderTarget2D renderTarget = (RenderTarget2D)oldRenderTargets[i].RenderTarget;
						renderTargetUsage[i] = renderTarget.RenderTargetUsage;
						PegasusLib.Graphics.GraphicsMethods.SetRenderTargetUsage(renderTarget, RenderTargetUsage.PreserveContents);
					}
				} else {
					renderTargetUsage = [spriteBatch.GraphicsDevice.PresentationParameters.RenderTargetUsage];
					spriteBatch.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
				}
				spriteBatch.GraphicsDevice.SetRenderTargets(oldRenderTargets);
			} finally {
				if (anyOldTargets) {
					for (int i = 0; i < oldRenderTargets.Length; i++) {
						PegasusLib.Graphics.GraphicsMethods.SetRenderTargetUsage((RenderTarget2D)oldRenderTargets[i].RenderTarget, renderTargetUsage[i]);
					}
				} else {
					spriteBatch.GraphicsDevice.PresentationParameters.RenderTargetUsage = renderTargetUsage[0];
				}
			}
		}
		public static ModUndergroundBackgroundStyle BiomeUGBackground<T>() where T : ModUndergroundBackgroundStyle {
			double num2 = Main.maxTilesY - 330;
			double num3 = (int)((num2 - Main.worldSurface) / 6.0) * 6;
			num2 = Main.worldSurface + num3 - 5.0;
			if ((Main.screenPosition.Y / 16f) > Main.rockLayer + 60 && (Main.screenPosition.Y / 16f) < num2 - 60) {
				return ModContent.GetInstance<T>();
			}
			return null;
		}
		public static string ToRomanNumerals(int number) {
			if (number < 0) return ToRomanNumerals(-number) + "0";
			if (number < 1) return string.Empty;
			if (number >= 1000) return "M" + ToRomanNumerals(number - 1000);
			if (number >= 900) return "CM" + ToRomanNumerals(number - 900);
			if (number >= 500) return "D" + ToRomanNumerals(number - 500);
			if (number >= 400) return "CD" + ToRomanNumerals(number - 400);
			if (number >= 100) return "C" + ToRomanNumerals(number - 100);
			if (number >= 90) return "XC" + ToRomanNumerals(number - 90);
			if (number >= 50) return "L" + ToRomanNumerals(number - 50);
			if (number >= 40) return "XL" + ToRomanNumerals(number - 40);
			if (number >= 10) return "X" + ToRomanNumerals(number - 10);
			if (number >= 9) return "IX" + ToRomanNumerals(number - 9);
			if (number >= 5) return "V" + ToRomanNumerals(number - 5);
			if (number >= 4) return "IV" + ToRomanNumerals(number - 4);
			if (number >= 1) return "I" + ToRomanNumerals(number - 1);
			throw new UnreachableException("Impossible state reached");
		}
	}
	public static class ShopExtensions {
		public static NPCShop InsertAfter<T>(this NPCShop shop, int targetItem, params Condition[] condition) where T : ModItem =>
			shop.InsertAfter(targetItem, ModContent.ItemType<T>(), condition);
		public static NPCShop InsertBefore<T>(this NPCShop shop, int targetItem, params Condition[] condition) where T : ModItem =>
			shop.InsertBefore(targetItem, ModContent.ItemType<T>(), condition);
		public static NPCShop InsertAfter<TAfter, TNew>(this NPCShop shop, params Condition[] condition) where TAfter : ModItem where TNew : ModItem =>
			shop.InsertAfter(ModContent.ItemType<TAfter>(), ModContent.ItemType<TNew>(), condition);
		public static NPCShop InsertBefore<TBefore, TNew>(this NPCShop shop, params Condition[] condition) where TBefore : ModItem where TNew : ModItem =>
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
		public static ReadOnlySpan<Triangle> TileTriangles => tileTriangles.AsSpan();
		public static ReadOnlySpan<Rectangle> TileRectangles => tileRectangles.AsSpan();
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
		public static bool CanRainReach(Vector2 position) {
			if (Main.remixWorld) {
				if (!((position.Y / 16f) > Main.rockLayer + 1024) || !(position.Y / 16f < (Main.maxTilesY - 350))) {
					return false;
				}
			} else if (position.Y > Main.worldSurface * 16.0 + 1024) {
				return false;
			}
			Vector2 dir = new Vector2(Main.windSpeedCurrent * 18f, 14f).Normalized(out _);
			return CollisionExt.CanHitRay(position, position - dir * ((2048 / dir.Y) - 16));
		}
		public static bool OverlapsAnyTiles(this Rectangle area, out List<Point> intersectingTiles, bool fallThrough = true) {
			Rectangle checkArea = area;
			Point topLeft = area.TopLeft().ToTileCoordinates();
			Point bottomRight = area.BottomRight().ToTileCoordinates();
			int minX = Utils.Clamp(topLeft.X, 0, Main.maxTilesX - 1);
			int minY = Utils.Clamp(topLeft.Y, 0, Main.maxTilesY - 1);
			int maxX = Utils.Clamp(bottomRight.X, 0, Main.maxTilesX - 1) - minX;
			int maxY = Utils.Clamp(bottomRight.Y, 0, Main.maxTilesY - 1) - minY;
			int cornerX = area.X - topLeft.X * 16;
			int cornerY = area.Y - topLeft.Y * 16;
			intersectingTiles = [];
			for (int i = 0; i <= maxX; i++) {
				for (int j = 0; j <= maxY; j++) {
					Tile tile = Main.tile[i + minX, j + minY];
					if (fallThrough && Main.tileSolidTop[tile.TileType]) continue;
					if (tile != null && tile.HasSolidTile()) {
						checkArea.X = i * -16 + cornerX;
						checkArea.Y = j * -16 + cornerY;
						if (tile.Slope != SlopeType.Solid) {
							if (tileTriangles[(int)tile.Slope - 1].Intersects(checkArea)) intersectingTiles.Add(new(i + minX, j + minY));
						} else {
							if (tileRectangles[(int)tile.BlockType].Intersects(checkArea)) intersectingTiles.Add(new(i + minX, j + minY));
						}
					}
				}
			}
			return intersectingTiles.Count > 0;
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
						if (tile.HasTile && !tile.IsActuated && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])) {
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
						if (tile.HasTile && !tile.IsActuated && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])) {
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
		/// <summary>
		/// Throws <see cref="ArgumentException"/> if <paramref name="direction"/> is zero
		/// </summary>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		/// <param name="maxLength"></param>
		/// <returns>The distance traveled before a tile was reached, or <paramref name="maxLength"/> if the distance would exceed it</returns>
		/// <exception cref="ArgumentException"></exception>
		public static float Raymarch(Vector2 position, Vector2 direction, Func<Tile, bool?> extraCheck, float maxLength = float.PositiveInfinity) {
			if (direction == Vector2.Zero) throw new ArgumentException($"{nameof(direction)} may not be zero");
			float length = 0;
			Point tilePos = position.ToTileCoordinates();
			Vector2 tileSubPos = (position - tilePos.ToWorldCoordinates(0, 0)) / 16;
			float angle = direction.ToRotation();
			double sin = Math.Sin(angle);
			double cos = Math.Cos(angle);
			double slope = cos == 0 ? Math.CopySign(double.PositiveInfinity, sin) : sin / cos;
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
			if (RaycastStep(tileSubPos, sin, cos) == tileSubPos) {
				DoLoopyThing(tileSubPos.X, out tileSubPos.X, tilePos.X, out tilePos.X, cos);
				DoLoopyThing(tileSubPos.Y, out tileSubPos.Y, tilePos.Y, out tilePos.Y, sin);
			}
			while (length < maxLength) {
				Vector2 next = RaycastStep(tileSubPos, sin, cos);
				if (next == tileSubPos) break;
				Tile tile = Framing.GetTileSafely(tilePos);
				bool doBreak = !WorldGen.InWorld(tilePos.X, tilePos.Y);
				Vector2 diff = next - tileSubPos;
				float dist = diff.Length();
				bool? extraControl = extraCheck(tile);
				if (extraControl == false) break;
				if (!extraControl.HasValue && tile.HasFullSolidTile()) {
					float flope = (float)slope;
					bool doSICalc = true;
					float tileSlope = 0;
					float tileIntercept = 0;
					switch (tile.BlockType) {
						case BlockType.Solid:
						doBreak = true;
						doSICalc = false;
						break;
						case BlockType.HalfBlock:
						if (next.Y > 0.5f) {
							doBreak = true;
							tileSlope = 0;
							tileIntercept = 0.5f;
						}
						break;
						case BlockType.SlopeDownLeft:
						if (next.X == 0 || next.Y == 1) {
							doBreak = true;
							tileSlope = 1;
							tileIntercept = 0;
						}
						break;
						case BlockType.SlopeDownRight:
						if (next.X == 1 || next.Y == 1) {
							doBreak = true;
							tileSlope = -1;
							tileIntercept = 1;
						}
						break;
						case BlockType.SlopeUpLeft:
						if (next.X == 0 || next.Y == 0) {
							doBreak = true;
							tileSlope = -1;
							tileIntercept = 1;
						}
						break;
						case BlockType.SlopeUpRight:
						if (next.X == 1 || next.Y == 0) {
							doBreak = true;
							tileSlope = 1;
							tileIntercept = 0;
						}
						break;
					}
					if (doSICalc) {
						//gets x position of intersection, y position can then be calculated by finding the y position at that x on either line
						float factor = ((tileSubPos.X * -flope + tileSubPos.Y) - tileIntercept) / (tileSlope - flope);
						Vector2 endPoint = new(
							factor,
							tileSlope * factor + tileIntercept
						);
						length += (float)(16 * endPoint.Distance(tileSubPos));
					}
				}
				if (doBreak) break;
				length += dist * 16;
				//Dust.NewDustPerfect(tilePos.ToWorldCoordinates(0, 0) + next * 16, 6, Vector2.Zero).noGravity = true;
				DoLoopyThing(next.X, out next.X, tilePos.X, out tilePos.X, cos);
				DoLoopyThing(next.Y, out next.Y, tilePos.Y, out tilePos.Y, sin);
				tile = Framing.GetTileSafely(tilePos);
				if (!extraControl.HasValue && tile.HasFullSolidTile()) {
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
				if (!extraControl.HasValue && !doBreak && (next.X == 0 || next.X == 1) && (next.Y == 0 || next.Y == 1)) {
					bool IsSolidWithExceptions(int xOff, int yOff, params BlockType[] blockTypes) {
						Tile tile = Framing.GetTileSafely(tilePos.X + xOff, tilePos.Y + yOff);
						if (tile.HasFullSolidTile()) return !blockTypes.Contains(tile.BlockType);
						return false;
					}
					switch ((next.X, next.Y)) {
						case (0, 0):
						if (IsSolidWithExceptions(0, -1, BlockType.SlopeUpRight) && IsSolidWithExceptions(-1, 0, BlockType.SlopeDownLeft, BlockType.HalfBlock)) {
							doBreak = true;
						}
						break;
						case (1, 0):
						if (IsSolidWithExceptions(0, -1, BlockType.SlopeUpLeft) && IsSolidWithExceptions(+1, 0, BlockType.SlopeDownRight, BlockType.HalfBlock)) {
							doBreak = true;
						}
						break;
						case (0, 1):

						if (IsSolidWithExceptions(0, +1, BlockType.SlopeDownRight, BlockType.HalfBlock) && IsSolidWithExceptions(-1, 0, BlockType.SlopeUpLeft)) {
							doBreak = true;
						}
						break;
						case (1, 1):
						if (IsSolidWithExceptions(0, +1, BlockType.SlopeDownLeft, BlockType.HalfBlock) && IsSolidWithExceptions(+1, 0, BlockType.SlopeUpRight)) {
							doBreak = true;
						}
						break;
					}
				}
				if (doBreak) break;
				tileSubPos = next;
			}
			if (length > maxLength) return maxLength;
			return length;
		}
		public static Vector2 RaycastStep(Vector2 pos, double sin, double cos) {
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
					/ ((a.X - b.X) * (point.Y - point2.Y) - (a.Y - b.Y) * (point.X - point2.X));
			progressOnSegment = t;
			if (onlyWithinSegment && (t < 0 || t > 1)) {
				return float.NaN;
			}

			float u = ((a.X - b.X) * (a.Y - point.Y) - (a.Y - b.Y) * (a.X - point.X))
					/ ((a.X - b.X) * (point.Y - point2.Y) - (a.Y - b.Y) * (point.X - point2.X));
			return u;
		}
		/// <summary>
		/// checks if a convex polygon defined by a set of line segments intersects a rectangle
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="hitbox"></param>
		/// <returns></returns>
		public static bool PolygonIntersectsRect((Vector2 start, Vector2 end)[] lines, Rectangle hitbox) {
			Vector2 min = new(float.MaxValue);
			Vector2 max = new(float.MinValue);
			for (int i = 0; i < lines.Length; i++) {
				min = Vector2.Min(min, lines[i].start);
				max = Vector2.Max(max, lines[i].start);
			}
			if (!hitbox.Intersects(OriginExtensions.BoxOf(min, max))) return false;
			int intersections = 0;
			Vector2 rectPos = hitbox.TopLeft();
			Vector2 rectSize = hitbox.Size();
			bool hasSize = hitbox.Width != 0 || hitbox.Height != 0;
			for (int i = 0; i < lines.Length; i++) {
				Vector2 a = lines[i].start;
				Vector2 b = lines[i].end;
				if (hasSize && Collision.CheckAABBvLineCollision2(rectPos, rectSize, a, b)) return true;
				float t = ((a.X - rectPos.X) * (rectPos.Y) - (a.Y - rectPos.Y) * (rectPos.X))
						/ ((a.X - b.X) * (rectPos.Y) - (a.Y - b.Y) * (rectPos.X));
				if (t < 0 || t > 1) continue;

				float u = ((a.X - b.X) * (a.Y - rectPos.Y) - (a.Y - b.Y) * (a.X - rectPos.X))
						/ ((a.X - b.X) * (rectPos.Y) - (a.Y - b.Y) * (rectPos.X));
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
			(Vector2 start, Vector2 end)[] output = new (Vector2 start, Vector2 end)[lines.Length];
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
		public static Rectangle Scaled(this Rectangle rectangle, float by) => new((int)(rectangle.X * by), (int)(rectangle.Y * by), (int)(rectangle.Width * by), (int)(rectangle.Height * by));
		public static Vector2[] Scaled(this Vector2[] vertices, Vector2 scale) {
			Vector2[] output = new Vector2[vertices.Length];
			for (int i = 0; i < vertices.Length; i++) {
				output[i] = vertices[i] * scale;
			}
			return output;
		}
		public static Vector2[] RotatedBy(this Vector2[] vertices, float rotation, Vector2 origin = default) {
			Vector2[] output = new Vector2[vertices.Length];
			for (int i = 0; i < vertices.Length; i++) {
				output[i] = vertices[i].RotatedBy(rotation, origin);
			}
			return output;
		}
		public static bool[,] GeneratePathfindingGrid(Point topLeft, Point bottomRight, int halfExtraWidth, int halfExtraHeight) {
			bool[,] solidity = new bool[bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y];
			for (int i = 0; i < solidity.GetLength(0); i++) {
				for (int j = 0; j < solidity.GetLength(1); j++) {
					for (int x = -halfExtraWidth; x <= halfExtraWidth; x++) {
						for (int y = -halfExtraHeight; y <= halfExtraHeight; y++) {
							Tile tile = Framing.GetTileSafely(topLeft.X + i + x, topLeft.Y + j + y);
							if (tile.TileSolidness() == 2) solidity[i, j] = true;
						}
					}
				}
			}
			return solidity;
		}
		public static Point[] GridBasedPathfinding(bool[,] solidTiles, Point start, Point target, HashSet<Point> alternateEnds = null) {
			alternateEnds ??= [];
			alternateEnds.Add(target);
			PathfindingGrid grid = new(solidTiles);
			PriorityQueue<Point, float> openList = new();
			openList.Enqueue(start, 0);
			grid[start].opened = true;
			const float SQRT2 = 1.4142135623731f;
			while (openList.TryDequeue(out Point pos, out _)) {
				PathfindingNode node = grid[pos];
				if (alternateEnds.Contains(pos)) {
					Stack<Point> stack = new();
					while (node.parent is not null) {
						stack.Push(node.position);
						node = node.parent;
					}
					return stack.ToArray();
				}
				if (node.closed) continue;
				node.closed = true;
				PathfindingNode[] neighbors = grid.GetNeigbors(pos);
				for (int i = 0; i < neighbors.Length; ++i) {
					PathfindingNode neighbor = neighbors[i];

					// get the distance between current node and the neighbor
					// and calculate the next g score
					float ng = node.g + ((neighbor.position.X - node.position.X == 0 || neighbor.position.Y - node.position.Y == 0) ? 1 : SQRT2);

					// check if the neighbor has not been inspected yet, or
					// can be reached with smaller cost from the current node
					if (!neighbor.opened || ng < neighbor.g) {
						static float GetDist(Point a, Point b) {
							int x = a.X - b.X;
							int y = a.Y - b.Y;
							return x * x + y * y;
						}
						neighbor.g = ng;
						neighbor.h ??= GetDist(neighbor.position, target);
						neighbor.f = neighbor.g + neighbor.h.Value;
						neighbor.parent = node;

						if (!neighbor.opened) {
							openList.Enqueue(neighbor.position, neighbor.f);
							neighbor.opened = true;
						} else {
							// the neighbor can be reached with smaller cost.
							// Since its f value has been updated, we have to
							// update its position in the open list
							openList.Enqueue(neighbor.position, neighbor.f);
						}
					}
				}
			}
			return [];
		}
		class PathfindingGrid(bool[,] grid) {
			readonly PathfindingNode[,] nodes = new PathfindingNode[grid.GetLength(0), grid.GetLength(1)];
			public ref PathfindingNode this[Point point] {
				get {
					ref PathfindingNode node = ref nodes[point.X, point.Y];
					node ??= new(point);
					return ref node;
				}
			}
			public PathfindingNode[] GetNeigbors(Point point) {
				PathfindingNode[] output = new PathfindingNode[4];
				int i = 0;
				void TryAdd(Point neighbor) {
					if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= grid.GetLength(0) || neighbor.Y >= grid.GetLength(1)) return;
					if (!grid[neighbor.X, neighbor.Y]) {
						PathfindingNode node = this[neighbor];
						if (!node.closed) output[i++] = node;
					}
				}
				TryAdd(new(point.X, point.Y - 1));
				TryAdd(new(point.X - 1, point.Y));
				TryAdd(new(point.X, point.Y + 1));
				TryAdd(new(point.X + 1, point.Y));
				Array.Resize(ref output, i);
				return output;
			}
		}
		class PathfindingNode(Point position) {
			public readonly Point position = position;
			public float f;
			public float? h;
			public float g;
			public bool closed;
			public bool opened;
			public PathfindingNode parent;
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
			self.useAmmo = ModContent.ItemType<Resizable_Mine_Wood>();
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
			self.ammo = ModContent.ItemType<Resizable_Mine_Wood>();
			self.maxStack = Item.CommonMaxStack;
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
				spriteBatch.Restart(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, RasterizerState.CullNone, null, Main.UIScaleMatrix, DepthStencilState.None);
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
		public static bool AddOtherNPCDialogue(this WeightedRandom<string> random, string thisNPC, int otherNPCType) {
			if (NPC.AnyNPCs(otherNPCType)) {
				random.AddNPCDialogue($"Mods.Origins.NPCs.{thisNPC}.Dialogue.{NPCID.Search.GetName(otherNPCType)}");
				return true;
			}
			return false;
		}
		public static void AddNPCDialogue(this WeightedRandom<string> random, string thisNPC, string key) {
			random.AddNPCDialogue($"Mods.Origins.NPCs.{thisNPC}.Dialogue.{key}");
		}
		public static void AddNPCDialogue(this WeightedRandom<string> random, string key) {
			if (Language.Exists(key)) {
				random.Add(Language.GetTextValue(key));
			} else {
				int i = 1;
				while (Language.Exists(key + i)) {
					random.Add(Language.GetTextValue(key + i));
					i++;
				}
			}
		}
		public static IEnumerable<string> GetGivenName(this ModNPC npc) {
			string key = $"Mods.Origins.NPCs.{npc.Name}.Name";
			if (Language.Exists(key)) {
				yield return Language.GetTextValue(key);
			} else {
				int i = 1;
				while (Language.Exists(key + i)) {
					yield return Language.GetTextValue(key + i);
					i++;
				}
			}
		}
		public static void DoCustomKnockback(this NPC npc, Vector2 velocity, bool fromNet = false) {
			if (npc.velocity == velocity) return;
			npc.velocity = velocity;
			if (!fromNet && Main.netMode != NetmodeID.SinglePlayer) {
				ModPacket packet = Origins.instance.GetPacket();
				packet.Write(Origins.NetMessageType.custom_knockback);
				packet.Write(npc.whoAmI);
				packet.Write(velocity.X);
				packet.Write(velocity.Y);
				packet.Send();
			}
			//Origins.instance.Logger.Info("Custom Knockback:" + velocity);
		}
		public static void SyncCustomKnockback(this NPC npc, bool fromNet = false) {
			DoCustomKnockback(npc, npc.velocity, fromNet);
		}
		public class MultipleUnlockableNPCEntryIcon : IEntryIcon {
			private int _npcNetId;

			private NPC[] _npcCache;

			private bool _firstUpdateDone;

			private Vector2 _positionOffsetCache;

			private string _overrideNameKey;

			public MultipleUnlockableNPCEntryIcon(int npcNetId, string overrideNameKey = null, params float[][] ai) : this(npcNetId, ai) {
				_overrideNameKey = overrideNameKey;
			}
			public MultipleUnlockableNPCEntryIcon(int npcNetId, params float[][] ai) {
				_npcNetId = npcNetId;
				_npcCache = new NPC[ai.Length];
				for (int i = 0; i < _npcCache.Length; i++) {
					_npcCache[i] = new NPC {
						IsABestiaryIconDummy = true
					};
					_npcCache[i].SetDefaults(_npcNetId);
					_firstUpdateDone = false;
					for (int j = 0; j < _npcCache[i].ai.Length && j < ai[i].Length; j++) {
						_npcCache[i].ai[j] = ai[i][j];
					}
				}
			}

			public IEntryIcon CreateClone() {
				return new MultipleUnlockableNPCEntryIcon(_npcNetId, _overrideNameKey, _npcCache.Select(npc => npc.ai).ToArray());
			}

			public void Update(BestiaryUICollectionInfo providedInfo, Rectangle hitbox, EntryIconDrawSettings settings) {
				Vector2 positionOffsetCache = default;
				int? frame = null;
				int? direction = null;
				int? spriteDirection = null;
				bool wet = false;
				float velocityX = 0f;
				Asset<Texture2D> asset = null;
				if (NPCID.Sets.NPCBestiaryDrawOffset.TryGetValue(_npcNetId, out NPCID.Sets.NPCBestiaryDrawModifiers value)) {
					for (int i = 0; i < _npcCache.Length; i++) {
						NPC npc = _npcCache[i];
						npc.rotation = value.Rotation;
						npc.scale = value.Scale;
						if (value.PortraitScale.HasValue && settings.IsPortrait) {
							npc.scale = value.PortraitScale.Value;
						}
						positionOffsetCache = value.Position;
						frame = value.Frame;
						direction = value.Direction;
						spriteDirection = value.SpriteDirection;
						velocityX = value.Velocity;
						wet = value.IsWet;
						if (value.PortraitPositionXOverride.HasValue && settings.IsPortrait) {
							positionOffsetCache.X = value.PortraitPositionXOverride.Value;
						}
						if (value.PortraitPositionYOverride.HasValue && settings.IsPortrait) {
							positionOffsetCache.Y = value.PortraitPositionYOverride.Value;
						}
						if (value.CustomTexturePath != null) {
							asset = ModContent.Request<Texture2D>(value.CustomTexturePath);
						}
					}
				}
				_positionOffsetCache = positionOffsetCache;
				UpdatePosition(settings);
				for (int i = 0; i < _npcCache.Length; i++) {
					NPC npc = _npcCache[i];
					if (NPCID.Sets.TrailingMode[npc.type] != -1) {
						for (int j = 0; j < npc.oldPos.Length; j++) {
							npc.oldPos[i] = npc.position;
						}
					}
					npc.direction = npc.spriteDirection = direction ?? -1;
					if (spriteDirection.HasValue) {
						npc.spriteDirection = spriteDirection.Value;
					}
					npc.wet = wet;
					SimulateFirstHover(velocityX);
					if (!frame.HasValue && (settings.IsPortrait || settings.IsHovered)) {
						npc.velocity.X = npc.direction * velocityX;
						npc.FindFrame();
					} else if (frame.HasValue) {
						npc.FindFrame();
						npc.frame.Y = npc.frame.Height * frame.Value;
					}
				}
			}

			private void UpdatePosition(EntryIconDrawSettings settings) {
				if (!settings.IsPortrait) {
					NPC npc = _npcCache[0];
					if (npc.noGravity) {
						npc.Center = settings.iconbox.Center.ToVector2() + _positionOffsetCache;
					} else {
						npc.Bottom = settings.iconbox.TopLeft() + settings.iconbox.Size() * new Vector2(0.5f, 1f) + new Vector2(0f, -8f) + _positionOffsetCache;
					}
					npc.position = npc.position.Floor();
				} else {
					float totalWidth = 0;
					for (int i = 0; i < _npcCache.Length; i++) {
						NPC npc = _npcCache[i];
						totalWidth += npc.width;
					}
					for (int i = 0; i < _npcCache.Length; i++) {
						NPC npc = _npcCache[i];
						if (npc.noGravity) {
							npc.Center = settings.iconbox.Center.ToVector2() + _positionOffsetCache;
						} else {
							npc.Bottom = settings.iconbox.TopLeft() + settings.iconbox.Size() * new Vector2(0.5f, 1f) + new Vector2(0f, -8f) + _positionOffsetCache;
						}
						npc.position.X += totalWidth * (i - (_npcCache.Length - 1) * 0.5f);
						npc.position = npc.position.Floor();
					}
				}
			}

			private void SimulateFirstHover(float velocity) {
				if (!_firstUpdateDone) {
					_firstUpdateDone = true;
					for (int i = 0; i < _npcCache.Length; i++) {
						NPC npc = _npcCache[i];
						npc.SetFrameSize();
						npc.velocity.X = npc.direction * velocity;
						npc.FindFrame();
					}
				}
			}

			public void Draw(BestiaryUICollectionInfo providedInfo, SpriteBatch spriteBatch, EntryIconDrawSettings settings) {
				UpdatePosition(settings);
				if (!settings.IsPortrait) {
					NPC npc = _npcCache[0];
					Main.instance.DrawNPCDirect(spriteBatch, npc, npc.behindTiles, Vector2.Zero);
				} else {
					for (int i = 0; i < _npcCache.Length; i++) {
						NPC npc = _npcCache[i];
						Main.instance.DrawNPCDirect(spriteBatch, npc, npc.behindTiles, Vector2.Zero);
					}
				}
			}

			public string GetHoverText(BestiaryUICollectionInfo providedInfo) {
				string result = Lang.GetNPCNameValue(_npcNetId);
				if (!string.IsNullOrWhiteSpace(_overrideNameKey)) {
					result = Language.GetTextValue(_overrideNameKey);
				}
				if (GetUnlockState(providedInfo)) {
					return result;
				}
				return "???";
			}

			public bool GetUnlockState(BestiaryUICollectionInfo providedInfo) {
				return providedInfo.UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0;
			}
		}
		/// <summary>
		/// returns true if the entity tried to jump
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public static bool TryJumpOverObstacles(this Entity entity, int direction, bool forceGrounded = false, bool stepsUp = true) {
			direction = Math.Sign(direction);
			int groundTileX = (int)(entity.position.X + entity.width * 0.5f * (1 + direction)) / 16;
			int groundTileY = (int)(entity.position.Y + entity.height + 15) / 16;
			if (forceGrounded || Framing.GetTileSafely(groundTileX, groundTileY).HasSolidTile()) {

				try {
					if (direction < 0) groundTileX--;
					if (direction > 0) groundTileX++;
					groundTileX += (int)entity.velocity.X;
					int jumpHeight = 0;
					for (int j = 0; j < 5; j++) {
						bool blocked = false;
						for (int i = 1; i <= Math.Ceiling(entity.height / 16f) && !blocked; i++) {
							blocked = WorldGen.SolidTile(groundTileX, (groundTileY - i) - j);
						}
						jumpHeight = j;
						if (!blocked) break;
					}
					switch (jumpHeight) {
						case 0:
						return false;

						case 1:
						if (stepsUp) goto case 0;
						entity.velocity.Y = -5.1f;
						break;

						case 2:
						entity.velocity.Y = -7.1f;
						break;

						default:
						case 3:
						entity.velocity.Y = -9.1f;
						break;

						case 4:
						entity.velocity.Y = -10.1f;
						break;

						case 5:
						entity.velocity.Y = -11.1f;
						break;
					}
				} catch {
					entity.velocity.Y = -9.1f;
				}
				return true;
			}
			return false;
		}
		public static void DoJellyfishAI(this NPC npc, float lungeThreshold = 0.2f, float lungeSpeed = 7f, Vector3 glowColor = default, bool canDoZappy = true) {
			bool isZappy = false;
			if (canDoZappy) {
				if (npc.wet && npc.ai[1] == 1f) {
					isZappy = true;
				} else {
					npc.dontTakeDamage = false;
				}

				if (Main.expertMode) {
					if (npc.wet) {
						if (npc.HasValidTarget && Main.player[npc.target].wet && Main.player[npc.target].Center.IsWithin(npc.Center, 150f) && Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height)) {
							if (npc.ai[1] == 0f) {
								npc.ai[2] += 2f;
							} else {
								npc.ai[2] -= 0.25f;
							}
						}

						if (isZappy) {
							npc.dontTakeDamage = true;
							npc.ai[2] += 1f;
							if (npc.ai[2] >= 120f)
								npc.ai[1] = 0f;
						} else {
							npc.ai[2] += 1f;
							if (npc.ai[2] >= 420f) {
								npc.ai[1] = 1f;
								npc.ai[2] = 0f;
							}
						}
					} else {
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
					}
				}
			}

			Lighting.AddLight(npc.Center, glowColor * (isZappy ? 1.5f : 1f));

			if (npc.direction == 0) npc.TargetClosest();

			if (isZappy) return;

			if (npc.wet) {
				Point centerTile = npc.Center.ToTileCoordinates();
				if (Framing.GetTileSafely(centerTile).TopSlope) {
					if (Framing.GetTileSafely(centerTile).LeftSlope) {
						npc.direction = -1;
						npc.velocity.X = Math.Abs(npc.velocity.X) * -1f;
					} else {
						npc.direction = 1;
						npc.velocity.X = Math.Abs(npc.velocity.X);
					}
				} else if (Framing.GetTileSafely(centerTile.X, centerTile.Y + 1).TopSlope) {
					if (Framing.GetTileSafely(centerTile.X, centerTile.Y + 1).LeftSlope) {
						npc.direction = -1;
						npc.velocity.X = Math.Abs(npc.velocity.X) * -1f;
					} else {
						npc.direction = 1;
						npc.velocity.X = Math.Abs(npc.velocity.X);
					}
				}

				if (npc.collideX) {
					npc.velocity.X *= -1f;
					npc.direction *= -1;
				}

				if (npc.collideY) {
					npc.velocity.Y = -npc.velocity.Y;
					npc.directionY = Math.Sign(npc.velocity.Y);
					npc.ai[0] = npc.directionY;
				}

				npc.TargetClosest(false);
				if (npc.HasValidTarget && Main.player[npc.target].wet && Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height)) {
					npc.localAI[2] = 1f;
					npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;
					npc.velocity *= 0.98f;

					if (npc.velocity.X > -lungeThreshold && npc.velocity.X < lungeThreshold && npc.velocity.Y > -lungeThreshold && npc.velocity.Y < lungeThreshold) {

						npc.TargetClosest();
						Vector2 diff = (Main.player[npc.target].MountedCenter - npc.Center);
						float dist = diff.Length();
						npc.velocity = diff * (lungeSpeed / dist);
					}
				} else {
					npc.localAI[2] = 0f;
					npc.velocity.X += npc.direction * 0.02f;
					npc.rotation = npc.velocity.X * 0.4f;
					if (npc.velocity.X < -1f || npc.velocity.X > 1f)
						npc.velocity.X *= 0.95f;

					if (npc.ai[0] == -1f) {
						npc.velocity.Y -= 0.01f;
						if (npc.velocity.Y < -1f) npc.ai[0] = 1f;
					} else {
						npc.velocity.Y += 0.01f;
						if (npc.velocity.Y > 1f) npc.ai[0] = -1f;
					}

					Point tilePos = npc.Center.ToTileCoordinates();
					if (Framing.GetTileSafely(tilePos.X, tilePos.Y - 1).LiquidAmount > 128) {
						if (Framing.GetTileSafely(tilePos.X, tilePos.Y + 1).HasTile || Framing.GetTileSafely(tilePos.X, tilePos.Y + 2).HasTile) {
							npc.ai[0] = -1f;
						}
					} else {
						npc.ai[0] = 1f;
					}

					if (npc.velocity.Y > 1.2 || npc.velocity.Y < -1.2) npc.velocity.Y *= 0.99f;
				}
			} else {
				npc.rotation += npc.velocity.X * 0.1f;
				if (npc.velocity.Y == 0f) {
					npc.velocity.X *= 0.98f;
					if (npc.velocity.X > -0.01 && npc.velocity.X < 0.01) npc.velocity.X = 0f;
				}

				npc.velocity.Y += 0.2f;
				if (npc.velocity.Y > 10f) npc.velocity.Y = 10f;

				npc.ai[0] = 1f;
			}
		}
		public static void DoFlyingAI(this NPC npc, float speed = 4f, float acceleration = 0.02f, float bounciness = 0.4f) {
			if (!npc.HasValidTarget) npc.TargetClosest();

			NPCAimedTarget targetData = npc.GetTargetData();
			bool leave = false;
			if (targetData.Type == NPCTargetType.Player)
				leave = Main.player[npc.target].dead;

			Vector2 npcCenter = (npc.Center / 8f).Floor() * 8;
			Vector2 targetCenter = (targetData.Center / 8f).Floor() * 8;
			Vector2 diff = targetCenter - npcCenter;
			float distanceToTarget = diff.Length();

			if (distanceToTarget == 0f) {
				diff = npc.velocity;
			} else {
				diff *= speed / distanceToTarget;
			}

			if (distanceToTarget > 100f) {
				npc.ai[0] += 1f;
				if (npc.ai[0] > 0f)
					npc.velocity.Y += 0.023f;
				else
					npc.velocity.Y -= 0.023f;

				if (npc.ai[0] < -100f || npc.ai[0] > 100f)
					npc.velocity.X += 0.023f;
				else
					npc.velocity.X -= 0.023f;

				if (npc.ai[0] > 200f)
					npc.ai[0] = -200f;
			}

			if (distanceToTarget < 150f) {
				npc.velocity += diff * 0.007f;
			}

			if (leave) {
				diff = new(npc.direction * speed / 0.5f, speed * -0.5f);
			}
			if (npc.velocity.X < diff.X) {
				npc.velocity.X += acceleration;
			} else if (npc.velocity.X > diff.X) {
				npc.velocity.X -= acceleration;
			}

			if (npc.velocity.Y < diff.Y) {
				npc.velocity.Y += acceleration;
			} else if (npc.velocity.Y > diff.Y) {
				npc.velocity.Y -= acceleration;
			}

			npc.rotation = diff.ToRotation() - MathHelper.PiOver2;

			if (npc.collideX) {
				npc.netUpdate = true;
				npc.velocity.X = npc.oldVelocity.X * -bounciness;
				if (npc.direction == -1 && npc.velocity.X > 0f && npc.velocity.X < 2f)
					npc.velocity.X = 2f;

				if (npc.direction == 1 && npc.velocity.X < 0f && npc.velocity.X > -2f)
					npc.velocity.X = -2f;
			}

			if (npc.collideY) {
				npc.netUpdate = true;
				npc.velocity.Y = npc.oldVelocity.Y * -bounciness;
				if (npc.velocity.Y > 0f && npc.velocity.Y < 1.5f)
					npc.velocity.Y = 2f;

				if (npc.velocity.Y < 0f && npc.velocity.Y > -1.5f)
					npc.velocity.Y = -2f;
			}

			if (npc.wet) {
				if (npc.velocity.Y > 0f)
					npc.velocity.Y *= 0.95f;

				npc.velocity.Y -= 0.3f;
				if (npc.velocity.Y < -2f)
					npc.velocity.Y = -2f;
			}

			if (leave) {
				npc.velocity.Y -= acceleration * 2f;
				npc.EncourageDespawn(10);
			}

			if (((npc.velocity.X > 0f && npc.oldVelocity.X < 0f) || (npc.velocity.X < 0f && npc.oldVelocity.X > 0f) || (npc.velocity.Y > 0f && npc.oldVelocity.Y < 0f) || (npc.velocity.Y < 0f && npc.oldVelocity.Y > 0f)) && !npc.justHit)
				npc.netUpdate = true;
		}
		public static void DrawGlowingNPCPart(this SpriteBatch spriteBatch, Texture2D texture, Texture2D glowTexture, Vector2 position, Rectangle? sourceRectangle, Color color, Color glowColor, float rotation, Vector2 origin, float scale, SpriteEffects effects) {
			spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, 0);
			spriteBatch.Draw(glowTexture, position, sourceRectangle, glowColor, rotation, origin, scale, effects, 0);
		}
		public static Color GetTintColor(this NPC npc, Color baseColor) {
			if (npc.IsABestiaryIconDummy) return baseColor;
			NPCLoader.DrawEffects(npc, ref baseColor);
			return npc.GetNPCColorTintedByBuffs(baseColor);
		}
	}
	public static class TileExtenstions {
		public record class MergeMatcher(int Up, int Down, int Left, int Right, int? UpLeft = null, int? UpRight = null, int? DownLeft = null, int? DownRight = null) {
			public int Match(int up, int down, int left, int right, int upLeft, int upRight, int downLeft, int downRight) {
				if (up != Up) return 0;
				if (down != Down) return 0;
				if (left != Left) return 0;
				if (right != Right) return 0;
				if (upLeft != UpLeft) return 1;
				if (upRight != UpRight) return 1;
				if (upLeft != UpLeft) return 1;
				if (downLeft != DownLeft) return 1;
				if (downRight != DownRight) return 1;
				return 2;
			}
		}
		public static void DoFrameCheck(int i, int j, out int up, out int down, out int left, out int right, out int upLeft, out int upRight, out int downLeft, out int downRight, params (int tileType, int frameType)[] map) {
			void FixFraming(out int frame, int x, int y, params BlockType[] invalidBlockTypes) {
				Tile tile = Framing.GetTileSafely(x, y);
				frame = -1;
				if (!tile.HasTile || invalidBlockTypes.Contains(tile.BlockType)) return;
				for (int i = 0; i < map.Length; i++) {
					if (tile.TileType == map[i].tileType) {
						frame = map[i].frameType;
						return;
					}
				}
			}
			FixFraming(out up, i, j - 1, BlockType.SlopeUpLeft, BlockType.SlopeUpRight);
			FixFraming(out down, i, j + 1, BlockType.HalfBlock, BlockType.SlopeDownLeft, BlockType.SlopeDownRight);
			FixFraming(out left, i - 1, j, BlockType.HalfBlock, BlockType.SlopeUpLeft, BlockType.SlopeDownLeft);
			FixFraming(out right, i + 1, j, BlockType.HalfBlock, BlockType.SlopeUpRight, BlockType.SlopeDownRight);
			switch (Framing.GetTileSafely(i, j).BlockType) {
				case BlockType.HalfBlock:
				up = -1;
				left = -1;
				right = -1;
				break;
				case BlockType.SlopeDownLeft:
				up = -1;
				right = -1;
				break;
				case BlockType.SlopeDownRight:
				up = -1;
				left = -1;
				break;
				case BlockType.SlopeUpLeft:
				down = -1;
				right = -1;
				break;
				case BlockType.SlopeUpRight:
				down = -1;
				left = -1;
				break;
			}

			FixFraming(out upLeft, i - 1, j - 1);
			FixFraming(out upRight, i + 1, j - 1);
			FixFraming(out downLeft, i - 1, j + 1);
			FixFraming(out downRight, i + 1, j + 1);
		}
		public static void DoFraming(int i, int j, bool resetFrame, int up, int down, int left, int right, int upLeft, int upRight, int downLeft, int downRight, params (MergeMatcher match, Point first, Point offset)[] frames) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (resetFrame) tile.TileFrameNumber = WorldGen.genRand.Next(0, 3);
			int frameNumber = tile.TileFrameNumber;
			Point bestMatch = default;
			int matchQuality = 0;
			for (int k = 0; k < frames.Length; k++) {
				int currentQuality = frames[k].match.Match(up, down, left, right, upLeft, upRight, downLeft, downRight);
				if (currentQuality > matchQuality) {
					bestMatch = frames[k].first;
					bestMatch = bestMatch.OffsetBy(frames[k].offset.X * frameNumber, frames[k].offset.Y * frameNumber);
					matchQuality = currentQuality;
					if (matchQuality >= 2) break;
				}
			}
			if (matchQuality == 0) return;
			tile.TileFrameX = (short)(bestMatch.X * 18);
			tile.TileFrameY = (short)(bestMatch.Y * 18);
		}
		public static (MergeMatcher match, Point first, Point offset)[] ExtraTileBlending {
			get {
				Point right = new(1, 0);
				Point down = new(0, 1);
				Point single = new(0, 0);
				const int NONE = -1;
				const int ROCK = 1;
				const int _MUD = 2;
				return [
					(new(ROCK, ROCK, NONE, ROCK), new Point(0, 0), down),
					(new(NONE, ROCK, ROCK, ROCK), new Point(1, 0), right),
					(new(ROCK, ROCK, ROCK, ROCK), new Point(1, 1), right),
					(new(ROCK, NONE, ROCK, ROCK), new Point(1, 2), right),
					(new(ROCK, ROCK, ROCK, NONE), new Point(4, 0), down),
					(new(ROCK, ROCK, NONE, NONE), new Point(5, 0), down),
					(new(NONE, ROCK, NONE, NONE), new Point(6, 0), right),
					(new(ROCK, ROCK, ROCK, ROCK, UpLeft: NONE, UpRight: NONE), new Point(6, 1), right),
					(new(ROCK, ROCK, ROCK, ROCK, DownLeft: NONE, DownRight: NONE), new Point(6, 2), right),
					(new(ROCK, NONE, NONE, NONE), new Point(6, 3), right),
					(new(NONE, NONE, NONE, ROCK), new Point(9, 0), down),
					(new(ROCK, ROCK, ROCK, ROCK, UpLeft: NONE, DownLeft: NONE), new Point(10, 0), down),
					(new(ROCK, ROCK, ROCK, ROCK, UpRight: NONE, DownRight: NONE), new Point(11, 0), down),
					(new(NONE, NONE, ROCK, NONE), new Point(12, 0), down),
					(new(NONE, _MUD, ROCK, ROCK), new Point(13, 0), right),
					(new(_MUD, NONE, ROCK, ROCK), new Point(13, 1), right),
					(new(ROCK, ROCK, NONE, _MUD), new Point(13, 2), right),
					(new(ROCK, ROCK, _MUD, NONE), new Point(13, 3), right),
					(new(NONE, ROCK, NONE, ROCK), new Point(0, 3), new Point(2, 0)),
					(new(NONE, ROCK, ROCK, NONE), new Point(1, 3), new Point(2, 0)),
					(new(ROCK, NONE, NONE, ROCK), new Point(0, 4), new Point(2, 0)),
					(new(ROCK, NONE, ROCK, NONE), new Point(1, 4), new Point(2, 0)),
					(new(NONE, NONE, NONE, NONE), new Point(9, 3), right),
					(new(NONE, NONE, ROCK, ROCK), new Point(6, 4), right),
					(new(ROCK, ROCK, ROCK, ROCK, DownRight: _MUD), new Point(0, 5), new Point(0, 2)),
					(new(ROCK, ROCK, ROCK, ROCK, DownLeft: _MUD), new Point(1, 5), new Point(0, 2)),
					(new(ROCK, ROCK, ROCK, ROCK, UpRight: _MUD), new Point(0, 6), new Point(0, 2)),
					(new(ROCK, ROCK, ROCK, ROCK, UpLeft: _MUD), new Point(1, 5), new Point(0, 2)),
					(new(_MUD, ROCK, _MUD, ROCK), new Point(2, 5), new Point(0, 2)),
					(new(_MUD, ROCK, ROCK, _MUD), new Point(3, 5), new Point(0, 2)),
					(new(ROCK, _MUD, _MUD, ROCK), new Point(2, 6), new Point(0, 2)),
					(new(ROCK, _MUD, ROCK, _MUD), new Point(3, 6), new Point(0, 2)),
					(new(ROCK, _MUD, NONE, ROCK), new Point(4, 5), down),
					(new(ROCK, _MUD, ROCK, NONE), new Point(5, 5), down),
					(new(_MUD, ROCK, NONE, ROCK), new Point(4, 8), down),
					(new(_MUD, ROCK, ROCK, NONE), new Point(5, 8), down),
					(new(NONE, _MUD, NONE, NONE), new Point(6, 5), down),
					(new(_MUD, NONE, NONE, NONE), new Point(6, 8), down),
					(new(ROCK, _MUD, NONE, NONE), new Point(7, 5), down),
					(new(_MUD, ROCK, NONE, NONE), new Point(7, 8), down),
					(new(ROCK, _MUD, ROCK, ROCK), new Point(8, 5), right),
					(new(_MUD, ROCK, ROCK, ROCK), new Point(8, 6), right),
					(new(ROCK, ROCK, ROCK, _MUD), new Point(8, 7), down),
					(new(ROCK, ROCK, _MUD, ROCK), new Point(9, 7), down),
					(new(_MUD, ROCK, _MUD, _MUD), new Point(11, 5), down),
					(new(_MUD, _MUD, _MUD, ROCK), new Point(12, 5), down),
					(new(ROCK, _MUD, _MUD, _MUD), new Point(11, 8), down),
					(new(_MUD, _MUD, ROCK, _MUD), new Point(12, 8), down),
					(new(ROCK, ROCK, _MUD, _MUD), new Point(10, 7), down),
					(new(_MUD, _MUD, ROCK, ROCK), new Point(8, 10), right),
					(new(NONE, ROCK, _MUD, ROCK), new Point(0, 11), right),
					(new(NONE, ROCK, ROCK, _MUD), new Point(3, 11), right),
					(new(ROCK, NONE, _MUD, ROCK), new Point(0, 12), right),
					(new(ROCK, NONE, ROCK, _MUD), new Point(3, 12), right),
					(new(_MUD, _MUD, _MUD, _MUD), new Point(6, 11), right),
					(new(NONE, NONE, _MUD, _MUD), new Point(9, 11), right),
					(new(_MUD, _MUD, NONE, NONE), new Point(6, 12), down),
					(new(NONE, NONE, _MUD, NONE), new Point(0, 13), right),
					(new(NONE, NONE, NONE, _MUD), new Point(3, 13), right),
					(new(NONE, NONE, _MUD, ROCK), new Point(0, 14), right),
					(new(NONE, NONE, ROCK, _MUD), new Point(3, 14), right),

					(new(NONE, ROCK, NONE, _MUD), new Point(0, 3), new Point(2, 0)),
					(new(NONE, ROCK, _MUD, NONE), new Point(1, 3), new Point(2, 0)),
					(new(ROCK, NONE, NONE, _MUD), new Point(0, 4), new Point(2, 0)),
					(new(ROCK, NONE, _MUD, NONE), new Point(1, 4), new Point(2, 0)),
					(new(NONE, _MUD, NONE, ROCK), new Point(0, 3), new Point(2, 0)),
					(new(NONE, _MUD, ROCK, NONE), new Point(1, 3), new Point(2, 0)),
					(new(_MUD, NONE, NONE, ROCK), new Point(0, 4), new Point(2, 0)),
					(new(_MUD, NONE, ROCK, NONE), new Point(1, 4), new Point(2, 0)),

					(new(_MUD, NONE, _MUD, _MUD), new Point(14, 8), single),
					(new(ROCK, NONE, _MUD, _MUD), new Point(15, 8), single),
					(new(_MUD, NONE, _MUD, ROCK), new Point(13, 9), single),
					(new(NONE, ROCK, _MUD, _MUD), new Point(14, 9), single),
					(new(NONE, _MUD, _MUD, _MUD), new Point(15, 9), single),
					(new(NONE, _MUD, _MUD, ROCK), new Point(13, 10), single),
					(new(_MUD, _MUD, _MUD, NONE), new Point(14, 10), single),
					(new(_MUD, ROCK, _MUD, NONE), new Point(15, 10), single),
					(new(NONE, _MUD, _MUD, NONE), new Point(13, 11), single),
					(new(ROCK, _MUD, _MUD, NONE), new Point(14, 11), single),
					(new(_MUD, NONE, _MUD, NONE), new Point(15, 11), single),
					(new(_MUD, NONE, ROCK, _MUD), new Point(13, 12), single),
					(new(NONE, _MUD, ROCK, _MUD), new Point(14, 12), single),
					(new(_MUD, _MUD, ROCK, NONE), new Point(15, 12), single),
					(new(_MUD, NONE, NONE, _MUD), new Point(13, 13), single),
					(new(_MUD, ROCK, NONE, _MUD), new Point(14, 13), single),
					(new(_MUD, _MUD, NONE, _MUD), new Point(15, 13), single),
					(new(_MUD, _MUD, NONE, ROCK), new Point(13, 14), single),
					(new(NONE, _MUD, NONE, _MUD), new Point(14, 14), single),
					(new(ROCK, _MUD, NONE, _MUD), new Point(15, 14), single),
				];
			}
		}
		public static void DoFraming(int i, int j, bool resetFrame, (int tileType, int frameType)[] map, params (MergeMatcher match, Point first, Point offset)[] frames) {
#if DEBUG && false
			List<int> types = [-1];
			for (int k = 0; k < map.Length; k++) {
				if (!types.Contains(map[k].frameType)) types.Add(map[k].frameType);
			}
			List<(int up, int down, int left, int right)> missingConfigurations = [];
			for (int l = 0; l < types.Count; l++) {
				for (int r = 0; r < types.Count; r++) {
					for (int u = 0; u < types.Count; u++) {
						for (int d = 0; d < types.Count; d++) {
							bool foundMatch = false;
							for (int k = 0; k < frames.Length && !foundMatch; k++) {
								if (frames[k].match.Match(types[u], types[d], types[l], types[r], -1, -1, -1, -1) > 0) {
									foundMatch = true;
								}
							}
							if (!foundMatch) missingConfigurations.Add((types[u], types[d], types[l], types[r]));
						}
					}
				}
			}
			if (missingConfigurations.Count > 0) {
				Framing_Tester.type = Framing.GetTileSafely(i, j).TileType;
				Dictionary<int, int> rev = new() {
					[-1] = -1
				};
				for (int k = 0; k < map.Length && rev.Count < types.Count; k++) {
					rev.TryAdd(map[k].frameType, map[k].tileType);
				}
				Framing_Tester.missingConfigurations = missingConfigurations.Select(v => (rev[v.up], rev[v.down], rev[v.left], rev[v.right])).ToList();
				Framing_Tester.allConfigurations = frames.Select(v => {
					List<(Vector2, int)> extras = [];
					if (v.match.UpLeft.HasValue) extras.Add((new(-1, -1), rev[v.match.UpLeft.Value]));
					if (v.match.UpRight.HasValue) extras.Add((new(1, -1), rev[v.match.UpRight.Value]));
					if (v.match.DownLeft.HasValue) extras.Add((new(-1, 1), rev[v.match.DownLeft.Value]));
					if (v.match.DownRight.HasValue) extras.Add((new(1, 1), rev[v.match.DownRight.Value]));
					return (rev[v.match.Up], rev[v.match.Down], rev[v.match.Left], rev[v.match.Right], extras);
				}).ToList();
				List<(Vector2, int)> empty = [];
				Framing_Tester.allConfigurations.AddRange(Framing_Tester.missingConfigurations.Select(v => (v.up, v.down, v.left, v.right, empty)));
			}
#endif
			DoFrameCheck(i, j, out int up, out int down, out int left, out int right, out int upLeft, out int upRight, out int downLeft, out int downRight, map);
			DoFraming(i, j, resetFrame, up, down, left, right, upLeft, upRight, downLeft, downRight, frames);
		}
		public static bool TryPlace(int i, int j, int type, int style = 0, int dir = 0, int? forcedRandom = null, bool cut = true) {
			return CanActuallyPlace(i, j, type, style, dir, out TileObject objectData, forcedRandom: forcedRandom, checkStay: true, cut: cut) && TileObject.Place(objectData);
		}
		public static bool CanActuallyPlace(int i, int j, int type, int style, int dir, out TileObject objectData, bool onlyCheck = false, int? forcedRandom = null, bool checkStay = false, bool cut = true) {
			if (TileObject.CanPlace(i, j, type, style, dir, out objectData, onlyCheck, forcedRandom, checkStay)) {
				TileObjectData tileData = TileObjectData.GetTileData(type, objectData.style, objectData.alternate);

				int left = i - tileData.Origin.X;
				int top = j - tileData.Origin.Y;
				for (int y = 0; y < tileData.Height; y++) {
					for (int x = 0; x < tileData.Width; x++) {
						Tile tileSafely = Framing.GetTileSafely(left + x, top + y);
						if (tileSafely.HasTile && !(cut && (Main.tileCut[tileSafely.TileType] || TileID.Sets.BreakableWhenPlacing[tileSafely.TileType]))) {
							return false;
						}
					}
				}
				/*for (int y = 0; y < tileData.Height; y++) {
					tileData.AnchorLeft.
				}
				for (int x = 0; x < tileData.Width; x++) { 

				}*/
				return true;
			}
			return false;
		}
		public static void ForcePlace(int i, int j, int type, int style, int dir, bool onlyCheck = false, int? forcedRandom = null, bool checkStay = false) {
			if (TileObject.CanPlace(i, j, type, style, dir, out TileObject objectData, onlyCheck, forcedRandom, checkStay)) {
				TileObjectData tileData = TileObjectData.GetTileData(type, objectData.style);

				int left = i - tileData.Origin.X;
				int top = j - tileData.Origin.Y;
				for (int y = 0; y < tileData.Height; y++) {
					for (int x = 0; x < tileData.Width; x++) {
						Tile tileSafely = Framing.GetTileSafely(left + x, top + y);
						tileSafely.HasTile = false;
					}
				}
				TileObject.Place(objectData);
			}
		}
		public static bool HasSolidFace(this Tile tile, TileSide side) {
			if (tile.BlockType == BlockType.Solid) return true;
			switch (side) {
				case TileSide.Top:
				return tile.BlockType is BlockType.SlopeUpLeft or BlockType.SlopeUpRight;

				case TileSide.Bottom:
				return tile.BlockType is BlockType.HalfBlock or BlockType.SlopeDownLeft or BlockType.SlopeDownRight;

				case TileSide.Left:
				return tile.BlockType is BlockType.SlopeDownLeft or BlockType.SlopeUpLeft;

				case TileSide.Right:
				return tile.BlockType is BlockType.SlopeDownRight or BlockType.SlopeUpRight;

				default:
				throw new ArgumentException($"Invalid tile side {side}", nameof(side));
			}
		}
		public enum TileSide {
			Top,
			Bottom,
			Left,
			Right
		}
		public static void SetToType(this Tile tile, ushort type, byte? paint = null) {
			tile.HasTile = true;
			tile.TileType = type;
			tile.TileColor = paint ?? PaintID.None;
		}
	}
	public static class ProjectileExtensions {
		public static void DoBoomerangAI(this Projectile projectile, Entity owner, float returnSpeed = 9f, float returnAcceleration = 0.4f, bool doSound = true) {
			if (doSound && projectile.soundDelay == 0) {
				projectile.soundDelay = 8;
				SoundEngine.PlaySound(SoundID.Item7, projectile.position);
			}
			if (projectile.ai[0] == 0f) {
				projectile.ai[1] += 1f;

				if (projectile.ai[1] >= 30f) {
					projectile.ai[0] = 1f;
					projectile.ai[1] = 0f;
					projectile.netUpdate = true;
				}
			} else {
				projectile.tileCollide = false;

				Vector2 offset = owner.Center - projectile.Center;
				float distance = offset.Length();
				if (distance > 3000f) {
					projectile.Kill();
					return;
				}

				offset *= returnSpeed / distance;
				if (projectile.velocity.X < offset.X) {
					projectile.velocity.X += returnAcceleration;
					if (projectile.velocity.X < 0f && offset.X > 0f)
						projectile.velocity.X += returnAcceleration;
				} else if (projectile.velocity.X > offset.X) {
					projectile.velocity.X -= returnAcceleration;
					if (projectile.velocity.X > 0f && offset.X < 0f)
						projectile.velocity.X -= returnAcceleration;
				}

				if (projectile.velocity.Y < offset.Y) {
					projectile.velocity.Y += returnAcceleration;
					if (projectile.velocity.Y < 0f && offset.Y > 0f)
						projectile.velocity.Y += returnAcceleration;
				} else if (projectile.velocity.Y > offset.Y) {
					projectile.velocity.Y -= returnAcceleration;
					if (projectile.velocity.Y > 0f && offset.Y < 0f)
						projectile.velocity.Y -= returnAcceleration;
				}

				if (Main.myPlayer == projectile.owner) {
					if (projectile.Hitbox.Intersects(owner.Hitbox))
						projectile.Kill();
				}
			}
		}
		public static void BulletShimmer(this Projectile projectile) {
			if (projectile.shimmerWet) {
				int x = (int)(projectile.Center.X / 16f);
				int y = (int)(projectile.position.Y / 16f);
				if (WorldGen.InWorld(x, y) && Main.tile[x, y] != null && Main.tile[x, y].LiquidAmount == byte.MaxValue && Main.tile[x, y].LiquidType == LiquidID.Shimmer && WorldGen.InWorld(x, y - 1) && Main.tile[x, y - 1] != null && Main.tile[x, y - 1].LiquidAmount > 0 && Main.tile[x, y - 1].LiquidType == LiquidID.Shimmer) {
					projectile.Kill();
				} else if (projectile.velocity.Y > 0f) {
					projectile.velocity.Y *= -1f;
					projectile.netUpdate = true;
					if (projectile.timeLeft > 600)
						projectile.timeLeft = 600;

					projectile.timeLeft -= 60;
					projectile.shimmerWet = false;
					projectile.wet = false;
				}
			}
		}
		public static bool IsLocallyOwned(this Projectile projectile) => projectile.owner == Main.myPlayer;

		public static void FillWhipControlPoints(this Projectile proj, Vector2 playerArmPosition, List<Vector2> controlPoints, int useTimeMax, float useTime, float? useTimeForSize = null) {
			useTimeForSize ??= useTimeMax;
			int timeToFlyOut = useTimeMax * proj.MaxUpdates;
			int segments = proj.WhipSettings.Segments;
			float rangeMultiplier = proj.WhipSettings.RangeMultiplier;
			float num = useTime / timeToFlyOut;
			float num2 = 0.5f;
			float num3 = 1f + num2;
			float num4 = MathHelper.Pi * 10f * (1f - num * num3) * (-proj.spriteDirection) / segments;
			float num5 = num * num3;
			float num6 = 0f;
			if (num5 > 1f) {
				num6 = (num5 - 1f) / num2;
				num5 = MathHelper.Lerp(1f, 0f, num6);
			}
			float num7 = (useTimeMax * 2) * num;
			float num8 = proj.velocity.Length() * num7 * num5 * rangeMultiplier / segments;
			float num9 = 1f;
			Vector2 vector = playerArmPosition;
			float num10 = -MathHelper.PiOver2;
			Vector2 vector2 = vector;
			float num11 = MathHelper.PiOver2 + MathHelper.PiOver2 * proj.spriteDirection;
			Vector2 vector3 = vector;
			float num12 = MathHelper.PiOver2;
			controlPoints.Add(playerArmPosition);
			for (int i = 0; i < segments; i++) {
				float num13 = i / (float)segments;
				float num14 = num4 * num13 * num9;
				Vector2 vector4 = vector + num10.ToRotationVector2() * num8;
				Vector2 vector5 = vector3 + num12.ToRotationVector2() * (num8 * 2f);
				Vector2 vector6 = vector2 + num11.ToRotationVector2() * (num8 * 2f);
				float num15 = 1f - num5;
				float num16 = 1f - num15 * num15;
				Vector2 value = Vector2.Lerp(vector5, vector4, num16 * 0.9f + 0.1f);
				Vector2 vector7 = Vector2.Lerp(vector6, value, num16 * 0.7f + 0.3f);
				Vector2 spinningpoint = playerArmPosition + (vector7 - playerArmPosition) * new Vector2(1f, num3);
				float num17 = num6;
				num17 *= num17;
				Vector2 item = spinningpoint.RotatedBy(proj.rotation + 4.712389f * num17 * proj.spriteDirection, playerArmPosition);
				controlPoints.Add(item);
				num10 += num14;
				num12 += num14;
				num11 += num14;
				vector = vector4;
				vector3 = vector5;
				vector2 = vector6;
			}
		}
	}
	public static class ContentExtensions {
		public static LocalizedText[] GetChildren(this LanguageTree languageTree) => languageTree.Values.Select(tree => tree.value).ToArray();
		public static IEnumerable<LanguageTree> GetDescendants(this LanguageTree languageTree, bool includeSelf = false) {
			if (includeSelf && languageTree.value.Key != languageTree.value.Value) yield return languageTree;
			foreach (LanguageTree branch in languageTree.Values) {
				foreach (LanguageTree item in branch.GetDescendants(true)) {
					yield return item;
				}
			}
		}
		public static LocalizedText SelectFrom(this LanguageTree languageTree, params object[] formatArgs) => Main.rand.Next(languageTree.GetChildren()).WithFormatArgs(formatArgs);
		public static string SelectFromFormatArg(this LanguageTree languageTree, object format, UnifiedRandom rand = null) => (rand ?? Main.rand)
			.Next(languageTree.Values
				.Select(tree => tree.value)
				.Where(text => text.CanFormatWith(format))
			.ToArray())
			.FormatWith(format);
		public static void AddBanner(this ModNPC self, int killsRequired = 50) {
			self.Mod.AddContent(new Banner(self, killsRequired));
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
				if (dropRule.ChainedRules.Count != 0 && dropRule.ChainedRules.Select(c => c.RuleToChain).FindDropRule(predicate) is T foundRule) return foundRule;
				if (ruleChildFinders.TryGetValue(dropRule.GetType(), out var ruleChildFinder) && ruleChildFinder(dropRule).FindDropRule(predicate) is T foundRule2) return foundRule2;
			}
			return null;
		}
		public static void Add(this OneFromRulesRule rule, params IItemDropRule[] rules) {
			Array.Resize(ref rule.options, rule.options.Length + rules.Length);
			for (int i = 1; i <= rules.Length; i++) {
				rule.options[^i] = rules[^i];
			}
		}
		public static void Add(this AlwaysAtleastOneSuccessDropRule rule, params IItemDropRule[] rules) {
			Array.Resize(ref rule.rules, rule.rules.Length + rules.Length);
			for (int i = 1; i <= rules.Length; i++) {
				rule.rules[^i] = rules[^i];
			}
		}
		public static void SubstituteKeybind(this List<TooltipLine> tooltips, ModKeybind keybind) {
			InputMode inputMode = InputMode.Keyboard;
			switch (PlayerInput.CurrentInputMode) {
				case InputMode.XBoxGamepad or InputMode.XBoxGamepadUI:
				inputMode = InputMode.XBoxGamepad;
				break;
			}
			string substitution = keybind.GetAssignedKeys(inputMode).FirstOrDefault() ?? Language.GetOrRegister("Mods.Origins.Generic.UnboundKey").Format(keybind.DisplayName);
			foreach (TooltipLine line in tooltips) {
				line.Text = line.Text.Replace("<key>", substitution);
			}
			if (OriginsModIntegrations.GoToKeybindKeybindPressed) {
				OriginsModIntegrations.GoToKeybind(keybind);
			}
		}
		public static string SubstituteKeybind(this string line, ModKeybind keybind) {
			InputMode inputMode = InputMode.Keyboard;
			switch (PlayerInput.CurrentInputMode) {
				case InputMode.XBoxGamepad or InputMode.XBoxGamepadUI:
				inputMode = InputMode.XBoxGamepad;
				break;
			}
			string substitution = keybind.GetAssignedKeys(inputMode).FirstOrDefault() ?? Language.GetOrRegister("Mods.Origins.Generic.UnboundKey").Format(keybind.DisplayName);
			line = line.Replace("<key>", substitution);
			if (OriginsModIntegrations.GoToKeybindKeybindPressed) {
				OriginsModIntegrations.GoToKeybind(keybind);
			}
			return line;
		}
		public static float DifficultyDamageMultiplier {
			get {
				if (Main.GameModeInfo.IsJourneyMode) {
					CreativePowers.DifficultySliderPower power = CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
					if (power.GetIsUnlocked()) {
						return power.StrengthMultiplierToGiveNPCs;
					}
				}
				return Main.GameModeInfo.EnemyDamageMultiplier;
			}
		}
		public static Vector2 MapUV(this Rectangle rect, Point point) {
			float U = (point.X - rect.Left) / (float)(rect.Right - rect.Left);
			float V = (point.Y - rect.Bottom) / (float)(rect.Top - rect.Bottom);

			return new Vector2(
				MathHelper.Clamp(U, 0, 1),
				MathHelper.Clamp(1f - V, 0, 1)
			);
		}
		public static bool IsWithin(this Entity a, Entity b, float range) => a.Center.Clamp(b.Hitbox).IsWithin(b.Center.Clamp(a.Hitbox), range);
		public static bool IsWithin(this Rectangle hitbox, Vector2 position, float range) => position.IsWithin(position.Clamp(hitbox), range);
		public static bool IsWithinRectangular(this Entity a, Entity b, Vector2 range) => a.Center.Clamp(b.Hitbox).IsWithinRectangular(b.Center.Clamp(a.Hitbox), range);
		public static bool IsWithinRectangular(this Vector2 a, Vector2 b, Vector2 range) => Abs(a - b).Between(Vector2.Zero, Abs(range));
		static Vector2 Abs(Vector2 v) => new(Math.Abs(v.X), Math.Abs(v.Y));
		public static void GetDisplayedDayTime(out string hours, out string minutes, out string seconds, out string half) {
			// Get current weird time
			double time = Main.time;
			if (!Main.dayTime) {
				// if it's night add this number
				time += 54000.0;
			}

			// Divide by seconds in a day * 24
			time = (time / 86400.0) * 24.0;
			// Dunno why we're taking 19.5. Something about hour formatting
			time = time - 7.5 - 12.0;
			// Format in readable time
			if (time < 0.0) {
				time += 24.0;
			}

			int intTime = (int)time;
			// Get the decimal points of time.
			double deltaTime = time - intTime;
			seconds = ((int)(deltaTime * 60.0 * 60.0) % 60).ToString("0#");
			// multiply them by 60. Minutes, probably
			deltaTime = (int)(deltaTime * 60.0);
			minutes = deltaTime.ToString();
			if (deltaTime < 10.0) {
				// if deltaTime is eg "1" (which would cause time to display as HH:M instead of HH:MM)
				minutes = "0" + minutes;
			}
			if (OriginClientConfig.Instance.TwentyFourHourTime) {
				half = "";
				hours = intTime.ToString("0#");
			} else {
				if (intTime >= 12) {
					half = " " + Language.GetTextValue("GameUI.TimePastMorning");
					// This is for AM/PM time rather than 24hour time
					if (intTime > 0) intTime -= 12;
				} else {
					half = " " + Language.GetTextValue("GameUI.TimeAtMorning");
				}

				if (intTime == 0) {
					// 0AM = 12AM
					intTime = 12;
				}
				hours = intTime.ToString();
			}
		}
	}
	public static class NetmodeActive {
		public static bool SinglePlayer => Main.netMode == NetmodeID.SinglePlayer;
		public static bool MultiplayerClient => Main.netMode == NetmodeID.MultiplayerClient;
		public static bool Server => Main.netMode == NetmodeID.Server;
	}
	// Convenience methods that really only exist to make things quicker and are likely to be used in places without a shared base class from Origins
	public static class GlobalUtils {
		public static Color FromHexRGB(uint hex) => FromHexRGBA((hex << 8) | 0x000000ffu);
		public static Color FromHexRGBA(uint hex) => new() {
			PackedValue = ((hex & 0xff000000u) >> 24) | ((hex & 0x00ff0000u) >> 8) | ((hex & 0x0000ff00u) << 8) | ((hex & 0x000000ffu) << 24),
		};
		public static void Min<T>(ref T current, T @new) where T : IComparisonOperators<T, T, bool> {
			if (current > @new) current = @new;
		}
		public static void Max<T>(ref T current, T @new) where T : IComparisonOperators<T, T, bool> {
			if (current < @new) current = @new;
		}
		public static void MinMax<T>(ref T min, ref T max) where T : IComparisonOperators<T, T, bool> {
			if (min > max) Utils.Swap(ref min, ref max);
		}
	}
}
