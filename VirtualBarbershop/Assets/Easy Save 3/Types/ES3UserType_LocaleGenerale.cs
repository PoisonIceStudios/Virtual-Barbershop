using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("SoldiTotali", "Qualita", "Interni", "Progresso", "ClientiTotali", "GiorniTotali")]
	public class ES3UserType_LocaleGenerale : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_LocaleGenerale() : base(typeof(LocaleGenerale)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (LocaleGenerale)obj;
			
			writer.WriteProperty("SoldiTotali", instance.SoldiTotali, ES3Type_int.Instance);
			writer.WriteProperty("Qualita", instance.Qualita, ES3Type_int.Instance);
			writer.WriteProperty("Interni", instance.Interni, ES3Type_int.Instance);
			writer.WriteProperty("Progresso", instance.Progresso, ES3Type_int.Instance);
			writer.WriteProperty("ClientiTotali", instance.ClientiTotali, ES3Type_int.Instance);
			writer.WriteProperty("GiorniTotali", instance.GiorniTotali, ES3Type_int.Instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (LocaleGenerale)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "SoldiTotali":
						instance.SoldiTotali = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Qualita":
						instance.Qualita = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Interni":
						instance.Interni = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Progresso":
						instance.Progresso = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "ClientiTotali":
						instance.ClientiTotali = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "GiorniTotali":
						instance.GiorniTotali = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_LocaleGeneraleArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_LocaleGeneraleArray() : base(typeof(LocaleGenerale[]), ES3UserType_LocaleGenerale.Instance)
		{
			Instance = this;
		}
	}
}