using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("premiRiscattati")]
	public class ES3UserType_ComputerOS : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_ComputerOS() : base(typeof(ComputerOS)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (ComputerOS)obj;
			
			writer.WritePrivateField("premiRiscattati", instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (ComputerOS)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "premiRiscattati":
					instance = (ComputerOS)reader.SetPrivateField("premiRiscattati", reader.Read<System.Boolean[]>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_ComputerOSArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_ComputerOSArray() : base(typeof(ComputerOS[]), ES3UserType_ComputerOS.Instance)
		{
			Instance = this;
		}
	}
}