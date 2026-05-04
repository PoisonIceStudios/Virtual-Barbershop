using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("ValoreDiGuasto", "rotto")]
	public class ES3UserType_Rasoio : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Rasoio() : base(typeof(Rasoio)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (Rasoio)obj;
			
			writer.WriteProperty("ValoreDiGuasto", instance.ValoreDiGuasto, ES3Type_int.Instance);
			writer.WriteProperty("rotto", instance.rotto, ES3Type_bool.Instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (Rasoio)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "ValoreDiGuasto":
						instance.ValoreDiGuasto = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "rotto":
						instance.rotto = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_RasoioArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_RasoioArray() : base(typeof(Rasoio[]), ES3UserType_Rasoio.Instance)
		{
			Instance = this;
		}
	}
}