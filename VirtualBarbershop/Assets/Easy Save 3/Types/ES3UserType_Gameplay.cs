using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("VittoriaOneTime")]
	public class ES3UserType_Gameplay : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Gameplay() : base(typeof(Gameplay)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (Gameplay)obj;
			
			writer.WriteProperty("VittoriaOneTime", instance.VittoriaOneTime, ES3Type_bool.Instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (Gameplay)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "VittoriaOneTime":
						instance.VittoriaOneTime = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_GameplayArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_GameplayArray() : base(typeof(Gameplay[]), ES3UserType_Gameplay.Instance)
		{
			Instance = this;
		}
	}
}