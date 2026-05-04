using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("LocaleCorrente", "LocaleAcquistato")]
	public class ES3UserType_CambiaLocale : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_CambiaLocale() : base(typeof(CambiaLocale)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (CambiaLocale)obj;
			
			writer.WriteProperty("LocaleCorrente", instance.LocaleCorrente, ES3Type_int.Instance);
			writer.WriteProperty("LocaleAcquistato", instance.LocaleAcquistato, ES3Type_boolArray.Instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (CambiaLocale)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "LocaleCorrente":
						instance.LocaleCorrente = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "LocaleAcquistato":
						instance.LocaleAcquistato = reader.Read<System.Boolean[]>(ES3Type_boolArray.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_CambiaLocaleArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_CambiaLocaleArray() : base(typeof(CambiaLocale[]), ES3UserType_CambiaLocale.Instance)
		{
			Instance = this;
		}
	}
}