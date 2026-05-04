using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("TimerDiGuasto", "Guasto")]
	public class ES3UserType_RasoioElettrico : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_RasoioElettrico() : base(typeof(RasoioElettrico)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (RasoioElettrico)obj;
			
			writer.WriteProperty("TimerDiGuasto", instance.TimerDiGuasto, ES3Type_float.Instance);
			writer.WriteProperty("Guasto", instance.Guasto, ES3Type_bool.Instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (RasoioElettrico)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "TimerDiGuasto":
						instance.TimerDiGuasto = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "Guasto":
						instance.Guasto = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_RasoioElettricoArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_RasoioElettricoArray() : base(typeof(RasoioElettrico[]), ES3UserType_RasoioElettrico.Instance)
		{
			Instance = this;
		}
	}
}