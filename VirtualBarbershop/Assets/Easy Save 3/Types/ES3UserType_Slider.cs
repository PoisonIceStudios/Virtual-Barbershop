using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("value")]
	public class ES3UserType_Slider : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Slider() : base(typeof(UnityEngine.UI.Slider)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (UnityEngine.UI.Slider)obj;
			
			writer.WriteProperty("value", instance.value, ES3Type_float.Instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (UnityEngine.UI.Slider)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "value":
						instance.value = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_SliderArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_SliderArray() : base(typeof(UnityEngine.UI.Slider[]), ES3UserType_Slider.Instance)
		{
			Instance = this;
		}
	}
}