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
using SysDraw = System.Drawing;
using Bitmap = System.Drawing.Bitmap;
using Microsoft.Xna.Framework.Audio;
using Terraria.ID;
using System.Runtime.CompilerServices;
using static Origins.Items.OriginGlobalItem;
using System.Reflection;
using Terraria.Utilities;
using System.Collections;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Origins {
    public class LinkedQueue<T> : ICollection<T> {
        public int Count {
            get { return _items.Count; }
        }

        public void Enqueue(T item) {
            _items.AddLast(item);
        }

        public T Dequeue() {
            if(_items.First is null)
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
            while(Count > 0) yield return Dequeue();
        }

        public IEnumerable<LinkedListNode<T>> GetNodeEnumerator() {
            IEnumerator<LinkedListNode<T>> enumerator = new LLNodeEnumerator<T>(_items);
            yield return enumerator.Current;
            while(enumerator.MoveNext()) yield return enumerator.Current;
        }

        public T[] ToArray() {
            return _items.ToArray();
        }
        #region ICollection implementation
        [Obsolete]
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
        private LinkedList<T> _items = new LinkedList<T>();
    }
    public struct LLNodeEnumerator<T> : IEnumerator<LinkedListNode<T>>{
        internal static FieldInfo LLVersion;
        private LinkedList<T> list;
        private LinkedListNode<T> current;
        private int version;
        private int index;
        public LLNodeEnumerator(LinkedList<T> list) {
            this.list = list;
            version = list.GetVersion();
            current = list.First;
            index = 0;
        }

        public LinkedListNode<T> Current => current;

        object IEnumerator.Current => current;

        public void Dispose() {}

        public bool MoveNext() {
            VersionCheck();
            current = current.Next;
            return !(current is null);
        }

        public void Reset() {
            VersionCheck();
            current = list.First;
        }
        void VersionCheck() {
            if (version != list.GetVersion()) {
                throw new InvalidOperationException("Collection has been modified");
            }
        }
    }
    public struct PolarVec2 {
        public float R;
        public float Theta;
        public PolarVec2(float r, float theta) {
            R = r;
            Theta = theta;
        }
        public static explicit operator Vector2(PolarVec2 pv) {
            return new Vector2((float)(pv.R*Math.Cos(pv.Theta)),(float)(pv.R*Math.Sin(pv.Theta)));
        }
        public static explicit operator PolarVec2(Vector2 vec) {
            return new PolarVec2(vec.Length(), vec.ToRotation());
        }
    }
    /// <summary>
    /// Because it's a little more convenient than an extension method for every number type
    /// </summary>
    public struct Fraction {
        public static Fraction Half => new Fraction(1, 2);
        public static Fraction Third => new Fraction(1, 3);
        public static Fraction Quarter => new Fraction(1, 4);
        public static Fraction Fifth => new Fraction(1, 5);
        public static Fraction Sixth => new Fraction(1, 6);
        public int numerator;
        public int denominator;
        public int N {get=>numerator;set=>numerator=value;}
        public int D {get=>denominator;set=>denominator=value;}
        public Fraction(int numerator, int denominator) {
            this.numerator = numerator;
            this.denominator = denominator;
        }
        public static Fraction operator +(Fraction f1, Fraction f2) {
            return new Fraction((f1.N * f2.D) + (f2.N * f1.D), f1.D*f2.D);
        }
        public static Fraction operator -(Fraction f1, Fraction f2) {
            return new Fraction((f1.N * f2.D) - (f2.N * f1.D), f1.D*f2.D);
        }
        public static Fraction operator *(Fraction f1, Fraction f2) {
            return new Fraction(f1.N * f2.N, f1.D * f2.D);
        }
        public static Fraction operator /(Fraction f1, Fraction f2) {
            return new Fraction(f1.N * f2.D, f1.D * f2.N);
        }
        public static Fraction operator *(Fraction frac, int i) {
            return new Fraction(frac.N*i, frac.D);
        }
        public static int operator *(int i, Fraction frac) {
            return (i * frac.N) / frac.D;
        }
        public static Fraction operator /(Fraction frac, int i) {
            return new Fraction(frac.N, frac.D*i);
        }
        public static int operator /(int i, Fraction frac) {
            return (i * frac.D) / frac.N;
        }
        public static explicit operator float(Fraction frac) {
            return frac.N/(float)frac.D;
        }
        public static explicit operator double(Fraction frac) {
            return frac.N/(double)frac.D;
        }
        public override string ToString() {
            return N+"/"+D;
        }
    }
    public class DrawAnimationManual : DrawAnimation {
	    public DrawAnimationManual(int frameCount) {
		    Frame = 0;
		    FrameCounter = 0;
		    FrameCount = frameCount;
            TicksPerFrame = -1;
	    }

	    public override void Update() {}

	    public override Rectangle GetFrame(Texture2D texture) {
            if(TicksPerFrame==-1)FrameCounter = 0;
		    return texture.Frame(FrameCount, 1, Frame, 0);
	    }
    }
    public abstract class IAnimatedItem : ModItem{
        public abstract DrawAnimation Animation { get; }
        public virtual Color? GlowmaskTint { get => null; }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
            Texture2D texture = Main.itemTexture[item.type];
            spriteBatch.Draw(texture, item.position-Main.screenPosition, Animation.GetFrame(texture), lightColor, 0f, default(Vector2), scale, SpriteEffects.None, 0f);
            return false;
        }
    }
    public interface ICustomDrawItem {
        void DrawInHand(Texture2D itemTexture, PlayerDrawInfo drawInfo, Vector2 itemCenter, Vector4 lightColor, Vector2 drawOrigin);
    }
    public interface ITileCollideNPC {
        int CollisionType { get; }
    }
    public interface IMeleeCollisionDataNPC {
        void GetMeleeCollisionData(Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect, ref float knockbackMult);
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
    public static class Elements {
        public const ushort Fire = 1;
        public const ushort Earth = 2;
        public const ushort Acid = 4;
        public const ushort Ice = 4;
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
		/// Moves directly towards player, gaining momentum. Emits blue-ish particles.
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
    }
    public static class OriginExtensions {
        public static Func<float, int, Vector2> drawPlayerItemPos;
        public static void PlaySound(string Name, Vector2 Position, float Volume = 1f, float PitchVariance = 1f, float? Pitch = null){
            if (Main.netMode==NetmodeID.Server || string.IsNullOrEmpty(Name)) return;
            var sound = Origins.instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/" + Name);
            SoundEffectInstance SEI = Main.PlaySound(sound.WithVolume(Volume).WithPitchVariance(PitchVariance), Position);
            if(Pitch.HasValue)SEI.Pitch = Pitch.Value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public static Vector2 Clamp(this Vector2 value, Rectangle area) {
            return new Vector2(MathHelper.Clamp(value.X,area.X,area.Right), MathHelper.Clamp(value.Y,area.Y,area.Bottom));
        }
        public static Rectangle Add(this Rectangle a, Vector2 b) {
            return new Rectangle(a.X+(int)b.X,a.Y+(int)b.Y,a.Width,a.Height);
        }
        public static double AngleDif(double alpha, double beta) {
            double phi = Math.Abs(beta - alpha) % (Math.PI*2);       // This is either the distance or 360 - distance
            double distance = ((phi > Math.PI)^(alpha>beta)) ? (Math.PI*2) - phi : phi;
            return distance;
        }
        public static float AngleDif(float alpha, float beta, out int dir) {
            float phi = Math.Abs(beta - alpha) % MathHelper.TwoPi;       // This is either the distance or 360 - distance
            dir = ((phi > MathHelper.Pi)^(alpha>beta))?-1:1;
            float distance = phi > MathHelper.Pi ? MathHelper.TwoPi - phi : phi;
            return distance;
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
            hitbox.Width = (int)(item.width*item.scale);
            hitbox.Height = (int)(item.height*item.scale);
		    if (player.direction == -1) hitbox.X -= hitbox.Width;
		    if (player.gravDir == 1f) hitbox.Y -= hitbox.Height;
            switch(stage) {
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
            Vector2 drawOrigin = new Vector2(1, 1);
            Rectangle drawRect = new Rectangle(
                (int)Math.Round(start.X - Main.screenPosition.X),
                (int)Math.Round(start.Y - Main.screenPosition.Y),
                (int)Math.Round((end - start).Length()),
                thickness);

            spriteBatch.Draw(Origins.instance.GetTexture("Projectiles/Pixel"), drawRect, null, color, (end - start).ToRotation(), Vector2.Zero, SpriteEffects.None, 0);
        }
        public static Bitmap ToBitmap(this Texture2D tex) {
            int[] data = new int[tex.Width * tex.Height];
            tex.GetData(0, new Rectangle(0,0,tex.Width,tex.Height), data, 0, tex.Width * tex.Height);
            Bitmap bitmap = new Bitmap(tex.Width, tex.Height);
            for (int x = 0; x < tex.Width; x++){
                for (int y = 0; y < tex.Height; y++){
                    SysDraw.Color bitmapColor = SysDraw.Color.FromArgb(data[y * tex.Width + x]);
                    bitmap.SetPixel(x, y, bitmapColor);
                }
            }
            return bitmap;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LinearSmoothing(ref float smoothed, float target, float rate) {
            if(target!=smoothed) {
                if(Math.Abs(target-smoothed)<rate) {
                    smoothed = target;
                } else {
                    if(target>smoothed) {
                        smoothed+=rate;
                    }else if(target<smoothed) {
                        smoothed-=rate;
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LinearSmoothing(ref Vector2 smoothed, Vector2 target, float rate) {
            if(target!=smoothed) {
                Vector2 diff = (target-smoothed);
                if((target-smoothed).Length()<rate) {
                    smoothed = target;
                } else {
                    diff.Normalize();
                    smoothed+=diff*rate;
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LerpSmoothing(ref float smoothed, float target, float rate, float snap) {
            if(target!=smoothed) {
                if(Math.Abs(target-smoothed)<snap) {
                    smoothed = target;
                } else {
                    smoothed = MathHelper.Lerp(smoothed, target, rate);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LerpSmoothing(ref Vector2 smoothed, Vector2 target, float rate, float snap) {
            if(target!=smoothed) {
                Vector2 diff = (target-smoothed);
                if((target-smoothed).Length()<snap) {
                    smoothed = target;
                } else {
                    smoothed = Vector2.Lerp(smoothed, target, rate);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AngularSmoothing(ref float smoothed, float target, float rate) {
            if(target!=smoothed) {
                float diff = AngleDif(smoothed, target, out int dir);
                diff = Math.Abs(diff);
                float aRate = Math.Abs(rate);
                if(diff<aRate) {
                    smoothed = target;
                } else {
                    smoothed+=rate*dir;
                }
            }
        }
        public static void AngularSmoothing(ref float smoothed, float target, float rate, out bool equal) {
            equal = true;
            if(target!=smoothed) {
                float diff = AngleDif(smoothed, target, out int dir);
                diff = Math.Abs(diff);
                float aRate = Math.Abs(rate);
                if(diff<=aRate) {
                    smoothed = target;
                } else {
                    smoothed+=rate*dir;
                    equal = false;
                }
            }
        }
        public static int GetWeaponCrit(this Player player, Item item) {
            int crit = 4+item.crit;
            if(item.melee) {
                crit+=player.meleeCrit-4;
                if(player.HeldItem.melee)crit-=player.HeldItem.crit;
            }
            if(item.ranged) {
                crit+=player.rangedCrit-4;
                if(player.HeldItem.ranged)crit-=player.HeldItem.crit;
            }
            if(item.magic) {
                crit+=player.magicCrit-4;
                if(player.HeldItem.magic)crit-=player.HeldItem.crit;
            }
            /*if(IsExplosive(item)) {
                crit+=player.GetModPlayer<OriginPlayer>().explosiveCrit-4;
                if(IsExplosive(player.HeldItem))crit-=player.HeldItem.crit;
            }*/
            return crit;
        }
        public static Vector2 TakeAverage(this List<Vector2> vectors) {
            Vector2 sum = default;
            int count = vectors.Count;
            for(int i = 0; i < vectors.Count; i++) {
                sum+=vectors[i];
            }
            return count!=0 ? sum/count : sum;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Vec2FromPolar(float theta, float magnitude = 1f) {
            return new Vector2((float)(magnitude*Math.Cos(theta)),(float)(magnitude*Math.Sin(theta)));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NormDot(Vector2 a, Vector2 b) {
            return (Vector2.Normalize(a)*Vector2.Normalize(b)).Sum();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(this Vector2 a) {
            return a.X+a.Y;
        }
        public static T[] BuildArray<T>(int length, params int[] nonNullIndeces) where T : new() {
            T[] o = new T[length];
            for(int i = 0; i < nonNullIndeces.Length; i++) {
                o[nonNullIndeces[i]] = new T();
            }
            return o;
        }
        public static Vector2 OldPos(this Projectile self, int index) {
            return index==-1 ?self.position:self.oldPos[index];
        }
        public static float OldRot(this Projectile self, int index) {
            return index==-1 ?self.rotation:self.oldRot[index];
        }
        public static Vector2 OldPos(this NPC self, int index) {
            return index==-1 ?self.position:self.oldPos[index];
        }
        public static float OldRot(this NPC self, int index) {
            return index==-1 ?self.rotation:self.oldRot[index];
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
        public static void CloneFrame(this NPC self, int type, int frameHeight) {
            int t = self.type;
            self.type = NPCID.BigMimicCrimson;
            self.VanillaFindFrame(frameHeight);
            self.type = t;
        }
        internal static MethodInfo memberwiseClone;
        internal static FieldInfo inext;
        internal static FieldInfo inextp;
        internal static FieldInfo SeedArray;
        internal static void initExt() {
            memberwiseClone = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic|BindingFlags.Instance);
            inext = typeof(UnifiedRandom).GetField("inext", BindingFlags.NonPublic|BindingFlags.Instance);
            inextp = typeof(UnifiedRandom).GetField("inextp", BindingFlags.NonPublic|BindingFlags.Instance);
            SeedArray = typeof(UnifiedRandom).GetField("SeedArray", BindingFlags.NonPublic|BindingFlags.Instance);
        }
        internal static void unInitExt() {
            memberwiseClone = null;
            inext = null;
            inextp = null;
            SeedArray = null;
        }
        public static UnifiedRandom Clone(this UnifiedRandom r) {
            UnifiedRandom o = (UnifiedRandom)memberwiseClone.Invoke(r, BindingFlags.NonPublic, null, null, null);
            SeedArray.SetValue(o, ((int[])SeedArray.GetValue(r)).Clone(), BindingFlags.NonPublic, null, null);
            return o;
        }
        public static ModRecipe Clone(this Recipe r, Mod mod) {
            ModRecipe o = new ModRecipe(mod);

            o.createItem = new Item();
            o.createItem.SetDefaults(r.createItem.type);
            o.requiredItem = r.requiredItem.Select(CloneByID).ToArray();
            o.requiredTile = r.requiredTile.ToArray();
            o.acceptedGroups = r.acceptedGroups.ToList();

            o.anyFragment = r.anyFragment;
            o.anyIronBar = r.anyIronBar;
            o.anyPressurePlate = r.anyPressurePlate;
            o.anySand = r.anySand;
            o.anyWood = r.anyWood;

            o.alchemy = r.alchemy;

            o.needWater = r.needWater;
            o.needLava = r.needLava;
            o.needHoney = r.needHoney;
            o.needSnowBiome = r.needSnowBiome;

            return o;
        }
        public static string Stringify(this Recipe r) {
            ItemID.Search.TryGetName(r.createItem.type, out string resultName);
            return $"result: {resultName} " +
                $"alchemy: {r.alchemy} " +
                $"required Items: {string.Join(", ",r.requiredItem.Select((i) => { ItemID.Search.TryGetName(i.type, out string name); return name; }))} " +
                $"required Tiles: {string.Join(", ",r.requiredTile.Select((i) => { TileID.Search.TryGetName(i, out string name); return name; }))}";
        }
        public static Item CloneByID(this Item item) {
            Item v = new Item();
            v.SetDefaults(item.type);
            return v;
        }
        public static Item ItemFromType(int type) {
            Item v = new Item();
            v.SetDefaults(type);
            return v;
        }
        public static int GetVersion<T>(this LinkedList<T> ll) {
            if(LLNodeEnumerator<T>.LLVersion is null)LLNodeEnumerator<T>.LLVersion = typeof(LinkedList<T>).GetField("version", BindingFlags.NonPublic|BindingFlags.Instance);
            return (int)LLNodeEnumerator<T>.LLVersion.GetValue(ll);
        }
    }
}
