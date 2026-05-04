using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("isClosed")]
	public class ES3UserType_BoxController : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_BoxController() : base(typeof(BoxController)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (BoxController)obj;
			
			writer.WritePrivateField("isClosed", instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (BoxController)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "isClosed":
					instance = (BoxController)reader.SetPrivateField("isClosed", reader.Read<System.Boolean>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_BoxControllerArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_BoxControllerArray() : base(typeof(BoxController[]), ES3UserType_BoxController.Instance)
		{
			Instance = this;
		}
	}
}