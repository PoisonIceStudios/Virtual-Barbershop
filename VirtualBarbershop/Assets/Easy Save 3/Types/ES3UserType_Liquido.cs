using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("Quantita")]
	public class ES3UserType_Liquido : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Liquido() : base(typeof(BNG.Liquido)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (BNG.Liquido)obj;
			
			writer.WriteProperty("Quantita", instance.Quantita, ES3Type_float.Instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (BNG.Liquido)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
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


	public class ES3UserType_LiquidoArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_LiquidoArray() : base(typeof(BNG.Liquido[]), ES3UserType_Liquido.Instance)
		{
			Instance = this;
		}
	}
}