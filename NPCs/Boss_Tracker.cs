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
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.NPCs {
	public class Boss_Tracker : ModSystem {
		public bool downedFiberglassWeaver;
		
		public override void Load() {
			DynamicMethod _saveData = new("saveData", typeof(void), [typeof(TagCompound), typeof(Boss_Tracker)], true);
			DynamicMethod _loadData = new("loadData", typeof(void), [typeof(TagCompound), typeof(Boss_Tracker)], true);
			ILGenerator _saveDataGen = _saveData.GetILGenerator();
			ILGenerator _loadDataGen = _loadData.GetILGenerator();
			MethodInfo tagCompound_Set = typeof(TagCompound).GetMethod(nameof(TagCompound.Set));
			MethodInfo tagCompound_TryGet = typeof(TagCompound).GetMethod(nameof(TagCompound.TryGet)).MakeGenericMethod(typeof(bool));
			foreach (FieldInfo field in GetType().GetFields()) {
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
		public override void Unload() {
			foreach (FieldInfo field in GetType().GetFields()) {
				if (field.IsStatic && field.FieldType.IsClass) field.SetValue(null, null);
			}
		}
		static Action<TagCompound, Boss_Tracker> saveData;
		static Action<TagCompound, Boss_Tracker> loadData;
		static Func<Boss_Tracker, List<BitsByte>> netSend;
		static Func<BinaryReader, List<BitsByte>> netReceive;
		public override void SaveWorldData(TagCompound tag) => saveData?.Invoke(tag, this);
		public override void LoadWorldData(TagCompound tag) => loadData?.Invoke(tag, this);
		public override void NetSend(BinaryWriter writer) {
			writer.WriteFlags(
				downedFiberglassWeaver
			);
		}
		public override void NetReceive(BinaryReader reader) {
			reader.ReadFlags(
				out downedFiberglassWeaver
			);
		}
	}
}
