using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("HeldItem")]
	public class ES3UserType_SnapZone : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_SnapZone() : base(typeof(BNG.SnapZone)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (BNG.SnapZone)obj;
			
			writer.WritePropertyByRef("HeldItem", instance.HeldItem);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (BNG.SnapZone)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "HeldItem":
						instance.HeldItem = reader.Read<BNG.Grabbable>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_SnapZoneArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_SnapZoneArray() : base(typeof(BNG.SnapZone[]), ES3UserType_SnapZone.Instance)
		{
			Instance = this;
		}
	}
}