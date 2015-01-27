﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ProtoBuf;

namespace Salar.BoisBenchmark.Objects
{
	[Serializable]
	[ProtoContract]
	[DataContract]
	public class SpecialCollections
	{
		public static SpecialCollections CreateObject()
		{
			var result =
				new SpecialCollections
					{
						ObsCollection = new ObservableCollection<string>()
							                {
								                "2","1","3"
							                },

						// Uncomment this part before using this test for benchmark
						NameValue = new NameValueCollection()
										{
											{"1","one"},
											{"2","two"},
											{"0","zero"}
										}
					};
			return result;
		}

		[ProtoMember(1)]
		public ObservableCollection<string> ObsCollection { get; set; }
		[ProtoMember(2)]
		public NameValueCollection NameValue { get; set; }
	}
}
