using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("position")]
	public class ES3UserType_RectTransform : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_RectTransform() : base(typeof(UnityEngine.RectTransform)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (UnityEngine.RectTransform)obj;
			
			writer.WriteProperty("position", instance.position, ES3Type_Vector3.Instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (UnityEngine.RectTransform)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "position":
						instance.position = reader.Read<UnityEngine.Vector3>(ES3Type_Vector3.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_RectTransformArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_RectTransformArray() : base(typeof(UnityEngine.RectTransform[]), ES3UserType_RectTransform.Instance)
		{
			Instance = this;
		}
	}
}