using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origins.Dev {
	public static class WikiImageExporter {
		static UInt32[] CRCTable;
		static bool crc_table_computed = false;
		static string currentCRCText = "";
		static int currentCRCLength = 0;
		static void make_crc_table() {
			CRCTable = new uint[256];
			uint c;
			short n, k;

			for (n = 0; n < 256; n++) {
				c = (uint)n;
				for (k = 0; k < 8; k++) {
					if ((c & 1) != 0)
						c = 0xedb88320u ^ (c >> 1);
					else
						c >>= 1;
				}
				CRCTable[n] = c;
			}
			crc_table_computed = true;
		}
		static void FinalizeCRC(uint crc32, MemoryStream stream) {
			crc32 ^= 0xFFFFFFFFu;
			stream.Write(ToBytes((uint)crc32));
#if DEBUG
			Origins.instance.Logger.Info(currentCRCLength - 4 + ":" + currentCRCText);
			currentCRCText = "";
			currentCRCLength = 0;
#endif
		}
		static uint ReadUInt(byte[] data, int position) {
			return ((uint)data[position + 0] << 8 * 3) | ((uint)data[position + 1] << 8 * 2) | ((uint)data[position + 2] << 8 * 1) | ((uint)data[position + 3] << 8 * 0);
		}
		static byte[] ToBytes(uint data) {
			return [(byte)(data >> 8 * 3 & 0xFF), (byte)(data >> 8 * 2 & 0xFF), (byte)(data >> 8 * 1 & 0xFF), (byte)(data >> 8 * 0 & 0xFF)];
		}
		static byte[] UShortToBytes(ushort data) {
			return [(byte)(data >> 8 * 1 & 0xFF), (byte)(data >> 8 * 0 & 0xFF)];
		}
		static int NextChunk(int cursor, byte[] buffer, params byte[] targetName) {
			uint targetBytes = 0;
			string chunkName = null;
			if (targetName.Length > 0) {
				chunkName = string.Join("", targetName.Select(c => (char)c));
				if (targetName.Length != 4) {
					throw new ArgumentException($"Target chunk type must be 4 bytes, {chunkName} is {targetName.Length} bytes", nameof(targetName));
				}
				targetBytes = ReadUInt(targetName, 0);
				if (targetBytes == ReadUInt(buffer, cursor + 4)) return cursor;
			}
			start:
			int chunkLength = (int)ReadUInt(buffer, cursor);
			/* length + chunk type + data + crc*/
			cursor += 4 + 4 + chunkLength + 4;
			if (targetName.Length > 0) {
				if (targetBytes != ReadUInt(buffer, cursor + 4)) goto start;
			}
			return cursor;
		}
		static void WriteAndAdvanceCRC(Stream stream, ref uint crc, byte[] buffer, int offset = 0, int length = -1, string name = "") {
#if DEBUG
			currentCRCText += $"\n{name} :";
#endif
			if (!crc_table_computed) make_crc_table();
			if (length == -1) length = buffer.Length - offset;
			stream.Write(buffer, offset, length);
			for (int i = 0; i < length; i++) {
				crc = CRCTable[(crc ^ buffer[i + offset]) & 0xff] ^ (crc >> 8);
				if (currentCRCText.Length < 4) {
					currentCRCText += (char)buffer[i + offset];
				} else {
					string h = buffer[i + offset].ToString("X");
					currentCRCText += " " + (h.Length == 1 ? "0" + h : h);
				}
#if DEBUG
				currentCRCLength++;
#endif
			}
#if DEBUG
			currentCRCText += "|";
#endif
		}
		public static void ExportImage(string name, Texture2D texture) {
			string filePath = Path.Combine(DebugConfig.Instance.WikiSpritesPath, name) + ".png";
			Directory.CreateDirectory(Path.GetDirectoryName(filePath));
			MemoryStream stream = new(texture.Width * texture.Height * 4);
			texture.SaveAsPng(stream, texture.Width, texture.Height);
			texture.Dispose();
			if (File.Exists(filePath)) {
				FileStream fileStream = File.OpenRead(filePath);
				bool isChanged = fileStream.Length != stream.Length;
				for (int i = 0; i < stream.Length && !isChanged; i++) {
					if (stream.ReadByte() != fileStream.ReadByte()) isChanged = true;
				}
				fileStream.Close();
				if (isChanged) {
					fileStream = File.OpenWrite(filePath);
					stream.Position = 0;
					fileStream.Position = 0;
					stream.WriteTo(fileStream);
					fileStream.Close();
				}
			} else {
				FileStream fileStream = File.Create(filePath);
				stream.WriteTo(fileStream);
				fileStream.Close();
			}
			stream.Close();
		}
		public static void ExportAnimatedImage(string name, (Texture2D texture, int frames)[] textures) {
			string filePath = Path.Combine(DebugConfig.Instance.WikiSpritesPath, name) + ".png";
			Directory.CreateDirectory(Path.GetDirectoryName(filePath));
			int seqNum = 0;
			MemoryStream fileBufferStream = new(0);
			for (int i = 0; i < textures.Length; i++) {
				Texture2D texture = textures[i].texture;
				MemoryStream memoryStream = new(0);
				texture.SaveAsPng(memoryStream, texture.Width, texture.Height);
				//texture.SaveAsPng(File.Create(Path.Combine(DebugConfig.Instance.WikiSpritesPath, sprite.name) + $"_frame_{i}.png"), texture.Width, texture.Height);
				//if (texture.Name == "") texture.Dispose();
				byte[] buffer = memoryStream.GetBuffer();
				const int _endSig = 16;
				const int _width = _endSig;
				const int _height = _width + 4;
				const int _bit_depth = _height + 4;
				const int _color_type = _bit_depth + 1;
				//const int _compression = _color_type + 1;
				//const int _filter = _compression + 1;
				//const int _interlace = _filter + 1;
				string test = "";
				string hex = "";
				for (int j = 0; j < buffer.Length; j++) {
					test += (char)buffer[j];
					string h = buffer[j].ToString("X");
					hex += (h.Length == 1 ? "0" + h : h) + "";
					if (j % 16 == 15) {
						//test += '\n';
						//hex += '\n';
					}
				}
				if (buffer[_color_type] != 6) Origins.LogError("invalid color type " + buffer[_color_type]);
				int IDAT_pos = NextChunk(0x08, buffer, "IDAT"u8.ToArray());
				uint IDAT_len = ReadUInt(buffer, IDAT_pos);
				uint crc32 = 0xFFFFFFFFu;
				if (i == 0) {
					fileBufferStream.Write(buffer, 0, IDAT_pos);
					fileBufferStream.Write([0x00, 0x00, 0x00, 0x08], 0, 4);
					WriteAndAdvanceCRC(fileBufferStream, ref crc32, "acTL"u8.ToArray());
					WriteAndAdvanceCRC(fileBufferStream, ref crc32, ToBytes((uint)textures.Length), name: "num_frames");
					WriteAndAdvanceCRC(fileBufferStream, ref crc32, BitConverter.GetBytes((uint)0), name: "num_plays");

					FinalizeCRC((uint)crc32, fileBufferStream);
				}

				fileBufferStream.Write([
					0x00, 0x00, 0x00, 0x1A
				], 0, 4);
				crc32 = 0xFFFFFFFFu;
				WriteAndAdvanceCRC(fileBufferStream, ref crc32, "fcTL"u8.ToArray());
				//BinaryWriter writer = new BinaryWriter(stream, Encoding.Default, true);
				//writer.Flush();
				//writer.Dispose();

				WriteAndAdvanceCRC(fileBufferStream, ref crc32, ToBytes((uint)seqNum++), name: "sequence_number");
				WriteAndAdvanceCRC(fileBufferStream, ref crc32, ToBytes((uint)texture.Width), name: "width");
				WriteAndAdvanceCRC(fileBufferStream, ref crc32, ToBytes((uint)texture.Height), name: "height");
				WriteAndAdvanceCRC(fileBufferStream, ref crc32, BitConverter.GetBytes((uint)0), name: "x_offset");
				WriteAndAdvanceCRC(fileBufferStream, ref crc32, BitConverter.GetBytes((uint)0), name: "y_offset");
				WriteAndAdvanceCRC(fileBufferStream, ref crc32, UShortToBytes((ushort)textures[i].frames), name: "delay_num");
				WriteAndAdvanceCRC(fileBufferStream, ref crc32, UShortToBytes((ushort)60), name: "delay_den");
				WriteAndAdvanceCRC(fileBufferStream, ref crc32, [(byte)1], name: "dispose_op");//APNG_DISPOSE_OP_BACKGROUND
				WriteAndAdvanceCRC(fileBufferStream, ref crc32, [(byte)0], name: "blend_op");//APNG_BLEND_OP_SOURCE
				FinalizeCRC((uint)crc32, fileBufferStream);

				crc32 = 0xFFFFFFFFu;
				if (i != 0) {
					fileBufferStream.Write(ToBytes((uint)(IDAT_len + 4)));
					WriteAndAdvanceCRC(fileBufferStream, ref crc32, [
						/*fdAT          */0x66, 0x64, 0x41, 0x54,
								/*sequence_number*/0x00, 0x00, 0x00, (byte)seqNum++
					]);
				} else {
					fileBufferStream.Write(ToBytes((uint)IDAT_len));
					WriteAndAdvanceCRC(fileBufferStream, ref crc32, "IDAT"u8.ToArray());
				}
				WriteAndAdvanceCRC(fileBufferStream, ref crc32, buffer, IDAT_pos + 4 + 4, (int)IDAT_len);
				FinalizeCRC((uint)crc32, fileBufferStream);

				if (i == textures.Length - 1) {
					int IEND_pos = NextChunk(IDAT_pos, buffer, "IEND"u8.ToArray());
					uint IEND_len = ReadUInt(buffer, IEND_pos);
					fileBufferStream.Write(buffer, IEND_pos, 4 + 4 + (int)IEND_len + 4);
				}
			}
			bool shouldOverwrite = !File.Exists(filePath);
			if (!shouldOverwrite) {
				FileStream oldFileStream = File.OpenRead(filePath);
				if (oldFileStream.Length == fileBufferStream.Length) {
					fileBufferStream.Position = 0;
					shouldOverwrite = false;
					for (int i = 0; i < fileBufferStream.Length; i++) {
						if (fileBufferStream.ReadByte() != oldFileStream.ReadByte()) {
							shouldOverwrite = true;
							break;
						}
					}
				} else {
					shouldOverwrite = true;
				}
				oldFileStream.Close();
			}
			if (shouldOverwrite) {
				fileBufferStream.Position = 0;
				FileStream stream = File.Create(filePath, (int)fileBufferStream.Length);
				fileBufferStream.CopyTo(stream);
			}
			fileBufferStream.Close();
		}
	}
}
