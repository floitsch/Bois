﻿using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Polenter.Serialization;
using ProtoBuf;
using Salar.Bon;
using Salar.BonBenchmark.Objects;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace Salar.BonBenchmark
{
	public partial class frmTest : Form
	{
		public frmTest()
		{
			InitializeComponent();
		}

		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.Run(new frmTest());
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			ClearLog();
		}

		private void ClearLog()
		{
			txtLog.Text = "";
		}

		private void frmTest_Load(object sender, EventArgs e)
		{

		}

		private void frmTest_Shown(object sender, EventArgs e)
		{
			RunTheTests(1);
		}
		private void btnBenchmark_Click(object sender, EventArgs e)
		{
			RunTheTests(2000);
		}


		void RunTheTests(int count)
		{
			ClearLog();
			Initialize();

			Log("BasicTypes benchmark---------- repeat count: " + count);
			var benchobj1 = BasicTypes.CreateObject();

			BonBenchmark(benchobj1, count, 2);

			ProtoBufNetBenchmark(benchobj1, count);

			NetSerializerBenchmark(benchobj1, count);

			SharpSerializerBenchmark(benchobj1, count);

			BinaryFormatterBenchmark(benchobj1, count);

			BsonBenchmark(benchobj1, count);

			JsonNetBenchmark(benchobj1, count);



			Log("");
			Log("HierarchyObject benchmark---------- repeat count: " + count);

			var benchobj2 = HierarchyObject.CreateObject();
			BonBenchmark(benchobj2, count, 2);

			ProtoBufNetBenchmark(benchobj2, count);

			NetSerializerBenchmark(benchobj2, count);

			SharpSerializerBenchmark(benchobj2, count);

			BinaryFormatterBenchmark(benchobj2, count);

			BsonBenchmark(benchobj2, count);

			JsonNetBenchmark(benchobj2, count);

			Log("");
			Log("Collections benchmark---------- repeat count: " + count);

			var benchobj3 = Collections.CreateObject();
			BonBenchmark(benchobj3, count, 2);

			ProtoBufNetBenchmark(benchobj3, count);

			NetSerializerBenchmark(benchobj3, count);

			SharpSerializerBenchmark(benchobj3, count);

			BinaryFormatterBenchmark(benchobj3, count);

			BsonBenchmark(benchobj3, count);

			JsonNetBenchmark(benchobj3, count);

			Log("");
			Log("SpecialCollections benchmark---------- repeat count: " + count);

			var benchobj4 = SpecialCollections.CreateObject();
			BonBenchmark(benchobj4, count, 2);

			ProtoBufNetBenchmark(benchobj4, count);

			NetSerializerBenchmark(benchobj4, count);

			SharpSerializerBenchmark(benchobj4, count);

			BinaryFormatterBenchmark(benchobj4, count);

			BsonBenchmark(benchobj4, count);

			JsonNetBenchmark(benchobj4, count);
		}


		private void Log(string text)
		{
			txtLog.Text = txtLog.Text + text + "\r\n";
		}


		static string ToString(TimeSpan ts)
		{
			return string.Format(" {0} s\t=\t{1}s, {2} ms", ts.TotalMilliseconds / 1000, ts.Seconds, ts.Milliseconds);
		}

		private void Initialize()
		{
			BonTypeCache.Initialize<BasicTypes>();
			BonTypeCache.Initialize<HierarchyObject>();
			if (!_netSerializer)
			{
				try
				{
					NetSerializer.Serializer.Initialize(
						new Type[]
							{
								typeof (BasicTypes),
								typeof (HierarchyObject),
								typeof (SpecialCollections),
								typeof (Collections),
							});
					_netSerializer = true;
				}
				catch (Exception ex)
				{
					Log("Failed to initialize NetSerializer: " + ex.Message);
				}
			}
		}

		private void BonBenchmark<T>(T obj, int count, int which)
		{
			try
			{
				Stopwatch sw;
				//-----------------------------------
				var bonSerializer = new BonSerializer();
				var bonMem = new MemoryStream();

				bonSerializer.Serialize(obj, bonMem);
				bonMem.Seek(0, SeekOrigin.Begin);
				bonSerializer.Deserialize<T>(bonMem);

				if (which != 0)
				{
					using (var mem = new MemoryStream())
					{
						sw = Stopwatch.StartNew();
						for (int i = 0; i < count; i++)
						{
							bonSerializer.Serialize(obj, mem);
							mem.SetLength(0);
						}
					}
					sw.Stop();
					Log("bonSerializer.Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + bonMem.Length);
				}

				if (which != 1)
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						bonMem.Seek(0, SeekOrigin.Begin);
						bonSerializer.Deserialize<T>(bonMem);
					}
					sw.Stop();
					Log("bonDeserializer.Deserialize		took: " + ToString(sw.Elapsed));
				}
			}
			catch (Exception ex)
			{
				Log("Bon failed, " + ex.Message);
			}
		}

		private void ProtoBufNetBenchmark<T>(T obj, int count)
		{
			try
			{
				Stopwatch sw;
				//-----------------------------------
				var pbuffMem = new MemoryStream();
				Serializer.Serialize(pbuffMem, obj);

				using (var mem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						Serializer.Serialize(mem, obj);
						mem.SetLength(0);
					}
				}
				sw.Stop();
				Log("protobuf-net Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + pbuffMem.Length);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					pbuffMem.Seek(0, SeekOrigin.Begin);
					Serializer.Deserialize<T>(pbuffMem);
				}
				sw.Stop();
				Log("protobuf-net Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("ProtocolBuffer failed, " + ex.Message);
			}
		}

		private bool _netSerializer = false;
		private void NetSerializerBenchmark<T>(T obj, int count)
		{
			try
			{

				Stopwatch sw;
				//-----------------------------------

				var pbuffMem = new MemoryStream();
				if (!_netSerializer)
				{
					NetSerializer.Serializer.Initialize(new Type[] { typeof(T) });
					_netSerializer = true;
				}
				NetSerializer.Serializer.Serialize(pbuffMem, obj);

				using (var mem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						NetSerializer.Serializer.Serialize(mem, obj);
						mem.SetLength(0);
					}
				}
				sw.Stop();
				Log("NetSerializer.Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + pbuffMem.Length);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					pbuffMem.Seek(0, SeekOrigin.Begin);
					NetSerializer.Serializer.Deserialize(pbuffMem);
				}
				sw.Stop();
				Log("NetSerializer.Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("NetSerializer failed, " + ex.Message);
			}
		}


		private void SharpSerializerBenchmark<T>(T obj, int count)
		{
			try
			{

				long initlength = 0;
				Stopwatch sw;
				//-----------------------------------
				var sharper = new SharpSerializer(new SharpSerializerBinarySettings(BinarySerializationMode.SizeOptimized));

				var mem = new MemoryStream();
				sharper.Serialize(obj, mem);
				initlength = mem.Length;

				mem.Seek(0, SeekOrigin.Begin);
				sharper.Deserialize(mem);


				using (var sharperMem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						sharper.Serialize(obj, sharperMem);
					}
				}
				sw.Stop();
				Log("SharpSerializer Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + initlength);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					mem.Seek(0, SeekOrigin.Begin);
					sharper.Deserialize(mem);
				}
				sw.Stop();
				Log("SharpSerializer Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("SharpSerializer failed, " + ex.Message);
			}
		}

		private void BinaryFormatterBenchmark<T>(T obj, int count)
		{
			try
			{
				long initlength = 0;
				Stopwatch sw;
				//-----------------------------------
				var binf = new BinaryFormatter();

				var mem = new MemoryStream();
				binf.Serialize(mem, obj);
				initlength = mem.Length;

				mem.Seek(0, SeekOrigin.Begin);
				binf.Deserialize(mem);


				using (var sharperMem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						binf.Serialize(sharperMem, obj);
					}
				}
				sw.Stop();
				Log("BinaryFormatter Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + initlength);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					mem.Seek(0, SeekOrigin.Begin);
					binf.Deserialize(mem);
				}
				sw.Stop();
				Log("BinaryFormatter Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("BinaryFormatter failed, " + ex.Message);
			}
		}

		private void BsonBenchmark<T>(T obj, int count)
		{
			try
			{
				Stopwatch sw;
				//-----------------------------------
				var jsonNet = new JsonSerializer();
				jsonNet.NullValueHandling = NullValueHandling.Ignore;
				var mainMem = new MemoryStream();
				var bsonWriter = new BsonWriter(mainMem);
				jsonNet.Serialize(bsonWriter, obj);
				mainMem.Seek(0, SeekOrigin.Begin);

				var bsonReader = new BsonReader(mainMem);


				using (var tbsonMem = new MemoryStream())
				using (var tbsonWriter = new BsonWriter(tbsonMem))
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						jsonNet.Serialize(tbsonWriter, obj);
						tbsonMem.SetLength(0);
					}
				}
				sw.Stop();
				Log("BSON bson.Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + mainMem.Length);

				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					bsonReader = new BsonReader(mainMem);
					mainMem.Seek(0, SeekOrigin.Begin);
					jsonNet.Deserialize<T>(bsonReader);
				}
				sw.Stop();
				Log("BSON bson.Deserialize		took: " + ToString(sw.Elapsed));

			}
			catch (Exception ex)
			{
				Log("BSON failed, " + ex.Message);
			}
		}

		private void JsonNetBenchmark<T>(T obj, int count)
		{
			try
			{

				Stopwatch sw;
				//-----------------------------------
				var jsonNet = new JsonSerializer();
				jsonNet.NullValueHandling = NullValueHandling.Ignore;

				var strWriter = new StringWriter();
				var jsonWriter = new JsonTextWriter(strWriter);
				jsonNet.Serialize(jsonWriter, obj);
				var initJsonString = strWriter.ToString();

				var strReader = new StringReader(initJsonString);

				var bsonReader = new JsonTextReader(strReader);


				using (var tbsonMem = new StringWriter())
				using (var tbsonWriter = new JsonTextWriter(tbsonMem))
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						jsonNet.Serialize(tbsonWriter, obj);
					}
				}
				sw.Stop();
				Log("Json.NET Serialize			took: " + ToString(sw.Elapsed) + "  data-size: " + initJsonString.Length);

				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					bsonReader = new JsonTextReader(strReader);
					jsonNet.Deserialize<T>(bsonReader);
				}
				sw.Stop();
				Log("Json.NET Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("Json.Net failed, " + ex.Message);
			}
		}



	}
}