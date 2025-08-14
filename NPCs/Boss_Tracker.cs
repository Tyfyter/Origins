using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.NPCs {
	public class Boss_Tracker : ModSystem {
		public static Boss_Tracker Instance => ModContent.GetInstance<Boss_Tracker>();
		public bool downedFiberglassWeaver;
		public bool downedLostDiver;
		public bool downedShimmerConstruct;

		public bool downedChambersiteSentinel;

		public bool downedDefiledMimic;
		public bool downedRivenMimic;
		public bool downedAshenMimic;
		
		public override void Load() {
			{
				DynamicMethod _saveData = new("saveData", typeof(void), [typeof(TagCompound), typeof(Boss_Tracker)], true);
				DynamicMethod _loadData = new("loadData", typeof(void), [typeof(TagCompound), typeof(Boss_Tracker)], true);
				ILGenerator _saveDataGen = _saveData.GetILGenerator();
				ILGenerator _loadDataGen = _loadData.GetILGenerator();
				MethodInfo tagCompound_Set = typeof(TagCompound).GetMethod(nameof(TagCompound.Set));
				MethodInfo tagCompound_TryGet = typeof(TagCompound).GetMethod(nameof(TagCompound.TryGet)).MakeGenericMethod(typeof(bool));
				foreach (FieldInfo field in GetType().GetFields()) {
					if (field.IsStatic) continue;
					if (field.FieldType == typeof(bool)) {
						_saveDataGen.Emit(OpCodes.Ldarg_0);
						_saveDataGen.Emit(OpCodes.Ldstr, field.Name);
						_saveDataGen.Emit(OpCodes.Ldarg_1);
						_saveDataGen.Emit(OpCodes.Ldfld, field);
						_saveDataGen.Emit(OpCodes.Box, field.FieldType);
						_saveDataGen.Emit(OpCodes.Ldc_I4_0);
						_saveDataGen.Emit(OpCodes.Callvirt, tagCompound_Set);

						_loadDataGen.Emit(OpCodes.Ldarg_0);
						_loadDataGen.Emit(OpCodes.Ldstr, field.Name);
						_loadDataGen.Emit(OpCodes.Ldarg_1);
						_loadDataGen.Emit(OpCodes.Ldflda, field);
						_loadDataGen.Emit(OpCodes.Callvirt, tagCompound_TryGet);
						_loadDataGen.Emit(OpCodes.Pop);


					}
				}
				_saveDataGen.Emit(OpCodes.Ret);
				_loadDataGen.Emit(OpCodes.Ret);
				saveData = _saveData.CreateDelegate<Action<TagCompound, Boss_Tracker>>();
				loadData = _loadData.CreateDelegate<Action<TagCompound, Boss_Tracker>>();
			}
			{
				DynamicMethod _netSend = new("netSend", typeof(void), [typeof(BinaryWriter), typeof(Boss_Tracker)], true);
				DynamicMethod _netReceive = new("netReceive", typeof(void), [typeof(BinaryReader), typeof(Boss_Tracker)], true);
				ILGenerator _netSendGen = _netSend.GetILGenerator();
				ILGenerator _netReceiveGen = _netReceive.GetILGenerator();
				MethodInfo writeFlags = typeof(BinaryIO).GetMethod(nameof(BinaryIO.WriteFlags));
				MethodInfo readFlags = typeof(BinaryIO).GetMethod(nameof(BinaryIO.ReadFlags), [typeof(BinaryReader), .. Enumerable.Repeat(typeof(bool).MakeByRefType(), 8)]);
				int index = 0;
				foreach (FieldInfo field in GetType().GetFields()) {
					if (field.IsStatic) continue;
					if (field.FieldType == typeof(bool)) {
						if (index == 0) {
							_netSendGen.Emit(OpCodes.Ldarg_0);
							_netReceiveGen.Emit(OpCodes.Ldarg_0);
						}
						_netSendGen.Emit(OpCodes.Ldarg_1);
						_netSendGen.Emit(OpCodes.Ldfld, field);

						_netReceiveGen.Emit(OpCodes.Ldarg_1);
						_netReceiveGen.Emit(OpCodes.Ldflda, field);
						index = (index + 1) % 8;
						if (index == 0) {
							_netSendGen.Emit(OpCodes.Callvirt, writeFlags);
							_netReceiveGen.Emit(OpCodes.Callvirt, readFlags);
						}
					}
				}
				if (index != 0) {
					FieldInfo dummyField = GetType().GetField(nameof(dummy), BindingFlags.NonPublic | BindingFlags.Static);
					while (index != 0) {
						_netSendGen.Emit(OpCodes.Ldc_I4_0);
						_netReceiveGen.Emit(OpCodes.Ldsflda, dummyField);
						index = (index + 1) % 8;
						if (index == 0) {
							_netSendGen.Emit(OpCodes.Call, writeFlags);
							_netReceiveGen.Emit(OpCodes.Call, readFlags);
						}
					}
				}
				_netSendGen.Emit(OpCodes.Ret);
				_netReceiveGen.Emit(OpCodes.Ret);
				netSend = _netSend.CreateDelegate<Action<BinaryWriter, Boss_Tracker>>();
				netReceive = _netReceive.CreateDelegate<Action<BinaryReader, Boss_Tracker>>();
			}
			{
				DynamicMethod _clearWorld = new("clearWorld", typeof(void), [typeof(Boss_Tracker)], true);
				ILGenerator _clearWorldGen = _clearWorld.GetILGenerator();
				foreach (FieldInfo field in GetType().GetFields()) {
					if (field.IsStatic) continue;
					if (field.FieldType == typeof(bool)) {
						_clearWorldGen.Emit(OpCodes.Ldarg_0);
						_clearWorldGen.Emit(OpCodes.Ldc_I4_0);
						_clearWorldGen.Emit(OpCodes.Stfld, field);
					}
				}
				_clearWorldGen.Emit(OpCodes.Ret);
				clearWorld = _clearWorld.CreateDelegate<Action<Boss_Tracker>>();
			}
			foreach (FieldInfo field in GetType().GetFields()) {
				if (field.FieldType == typeof(bool) && !field.IsStatic) {
					DynamicMethod get = new("get_" + field.Name, typeof(bool), []);
					get.GetILGenerator().Emit(OpCodes.Call, GetType().GetProperty(nameof(Instance)).GetGetMethod());
					get.GetILGenerator().Emit(OpCodes.Ldfld, field);
					get.GetILGenerator().Emit(OpCodes.Ret);
					Conditions.Add(field.Name, new(Language.GetOrRegister("Mods.Origins.Conditions." + field.Name.Replace("downed", "Downed")), get.CreateDelegate<Func<bool>>()));
				}
			}
		}
		public override void Unload() {
			foreach (FieldInfo field in GetType().GetFields()) {
				if (field.IsStatic && field.FieldType.IsClass) field.SetValue(null, null);
			}
		}
		public static Dictionary<string, Condition> Conditions { get; private set; } = [];
		static Action<TagCompound, Boss_Tracker> saveData;
		static Action<TagCompound, Boss_Tracker> loadData;
		static Action<BinaryWriter, Boss_Tracker> netSend;
		static Action<BinaryReader, Boss_Tracker> netReceive;
		static Action<Boss_Tracker> clearWorld;
		static bool dummy;
		public override void SaveWorldData(TagCompound tag) => saveData?.Invoke(tag, this);
		public override void LoadWorldData(TagCompound tag) => loadData?.Invoke(tag, this);
		public override void NetSend(BinaryWriter writer) => netSend(writer, this);
		public override void NetReceive(BinaryReader reader) => netReceive(reader, this);
		public override void ClearWorld() => clearWorld(this);
	}
}
