using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Projectiles {
	public class FriendlyGlobalProjectile : GlobalProjectile {
		public bool forceMinionShot;
		public bool forceSentryShot;
		public DamageClass DamageTypeOverride {
			get => field;
			set {
				if (field == value) return;
				field = value;
				revertDamageClass = value is null;
			}
		}
		bool revertDamageClass = false;
		public override bool InstancePerEntity => true;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.friendly;
		public override void Load() {
			On_Projectile.Damage += On_Projectile_Damage;
		}
		void On_Projectile_Damage(On_Projectile.orig_Damage orig, Projectile self) {
			if (self.TryGetGlobalProjectile(this, out FriendlyGlobalProjectile result)) {
				/*using ScopedOverride<bool> m = result.forceMinionShot ? ProjectileID.Sets.MinionShot[self.type].ScopedOverride(true) : default;
				using ScopedOverride<bool> s = result.forceSentryShot ? ProjectileID.Sets.SentryShot[self.type].ScopedOverride(true) : default;*/
				using ScopedOverride<bool> m = ProjectileID.Sets.MinionShot[self.type].ScopedOverride(result.forceMinionShot || ProjectileID.Sets.MinionShot[self.type]);
				using ScopedOverride<bool> s = ProjectileID.Sets.SentryShot[self.type].ScopedOverride(result.forceSentryShot || ProjectileID.Sets.SentryShot[self.type]);
				orig(self);
			} else {
				orig(self);
			}
		}
		public override bool? CanDamage(Projectile projectile) {
			if (DamageTypeOverride is not null) {
				projectile.DamageType = DamageTypeOverride;
			} else if (revertDamageClass) {
				projectile.DamageType = ContentSamples.ProjectilesByType[projectile.type].DamageType;
			}
			return null;
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			if (DamageTypeOverride is not null) {
				bitWriter.WriteBit(true);
				WriteType(binaryWriter, DamageTypeOverride.Type, DamageClassLoader.DamageClassCount);
			} else {
				bitWriter.WriteBit(false);
			}
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			DamageTypeOverride = bitReader.ReadBit() ? DamageClassLoader.GetDamageClass(ReadType(binaryReader, DamageClassLoader.DamageClassCount)) : null;
		}
		static void WriteType(BinaryWriter writer, int type, int count) {
			if (count < byte.MaxValue) writer.Write((byte)type);
			else if (count < ushort.MaxValue) writer.Write((ushort)type);
			else writer.Write((int)type);
		}
		static int ReadType(BinaryReader reader, int count) {
			if (count < byte.MaxValue) return reader.ReadByte();
			if (count < ushort.MaxValue) return reader.ReadUInt16();
			return reader.ReadInt32();
		}
	}
}
