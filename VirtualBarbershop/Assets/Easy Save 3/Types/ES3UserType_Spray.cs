using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("SprayAttivo", "Quantita")]
	public class ES3UserType_Spray : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Spray() : base(typeof(BNG.Spray)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (BNG.Spray)obj;
			
			writer.WriteProperty("SprayAttivo", instance.SprayAttivo, ES3Type_bool.Instance);
			writer.WriteProperty("Quantita", instance.Quantita, ES3Type_float.Instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (BNG.Spray)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "SprayAttivo":
						instance.SprayAttivo = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "Quantita":
						instance.Quantita = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_SprayArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_SprayArray() : base(typeof(BNG.Spray[]), ES3UserType_Spray.Instance)
		{
			Instance = this;
		}
	}
}