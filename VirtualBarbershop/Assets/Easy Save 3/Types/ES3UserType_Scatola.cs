using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("DragObjectRight", "DragObjectLeft", "antaRight", "antaLeft", "dragObjectRight_MinZ", "dragObjectRight_MaxZ", "antaRightClosed", "antaRightOpen", "rightMinLimit", "antaRightOpenFixed", "dragObjectLeft_MinZ", "dragObjectLeft_MaxZ", "antaLeftClosed", "antaLeftOpen", "leftMinLimit", "antaLeftOpenFixed", "currentDistanceRight", "currentDistanceLeft", "isClosed", "rightClosed", "leftClosed")]
	public class ES3UserType_Scatola : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Scatola() : base(typeof(Scatola)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (Scatola)obj;
			
			writer.WritePropertyByRef("DragObjectRight", instance.DragObjectRight);
			writer.WritePropertyByRef("DragObjectLeft", instance.DragObjectLeft);
			writer.WritePrivateFieldByRef("antaRight", instance);
			writer.WritePrivateFieldByRef("antaLeft", instance);
			writer.WritePrivateField("dragObjectRight_MinZ", instance);
			writer.WriteProperty("dragObjectRight_MaxZ", instance.dragObjectRight_MaxZ, ES3Type_float.Instance);
			writer.WritePrivateField("antaRightClosed", instance);
			writer.WriteProperty("antaRightOpen", instance.antaRightOpen, ES3Type_float.Instance);
			writer.WritePrivateField("rightMinLimit", instance);
			writer.WritePrivateField("antaRightOpenFixed", instance);
			writer.WritePrivateField("dragObjectLeft_MinZ", instance);
			writer.WriteProperty("dragObjectLeft_MaxZ", instance.dragObjectLeft_MaxZ, ES3Type_float.Instance);
			writer.WritePrivateField("antaLeftClosed", instance);
			writer.WriteProperty("antaLeftOpen", instance.antaLeftOpen, ES3Type_float.Instance);
			writer.WritePrivateField("leftMinLimit", instance);
			writer.WritePrivateField("antaLeftOpenFixed", instance);
			writer.WritePrivateField("currentDistanceRight", instance);
			writer.WritePrivateField("currentDistanceLeft", instance);
			writer.WriteProperty("isClosed", instance.isClosed, ES3Type_bool.Instance);
			writer.WritePrivateField("rightClosed", instance);
			writer.WritePrivateField("leftClosed", instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (Scatola)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "DragObjectRight":
						instance.DragObjectRight = reader.Read<UnityEngine.Transform>(ES3Type_Transform.Instance);
						break;
					case "DragObjectLeft":
						instance.DragObjectLeft = reader.Read<UnityEngine.Transform>(ES3Type_Transform.Instance);
						break;
					case "antaRight":
					instance = (Scatola)reader.SetPrivateField("antaRight", reader.Read<UnityEngine.GameObject>(), instance);
					break;
					case "antaLeft":
					instance = (Scatola)reader.SetPrivateField("antaLeft", reader.Read<UnityEngine.GameObject>(), instance);
					break;
					case "dragObjectRight_MinZ":
					instance = (Scatola)reader.SetPrivateField("dragObjectRight_MinZ", reader.Read<System.Single>(), instance);
					break;
					case "dragObjectRight_MaxZ":
						instance.dragObjectRight_MaxZ = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "antaRightClosed":
					instance = (Scatola)reader.SetPrivateField("antaRightClosed", reader.Read<System.Single>(), instance);
					break;
					case "antaRightOpen":
						instance.antaRightOpen = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "rightMinLimit":
					instance = (Scatola)reader.SetPrivateField("rightMinLimit", reader.Read<System.Single>(), instance);
					break;
					case "antaRightOpenFixed":
					instance = (Scatola)reader.SetPrivateField("antaRightOpenFixed", reader.Read<System.Single>(), instance);
					break;
					case "dragObjectLeft_MinZ":
					instance = (Scatola)reader.SetPrivateField("dragObjectLeft_MinZ", reader.Read<System.Single>(), instance);
					break;
					case "dragObjectLeft_MaxZ":
						instance.dragObjectLeft_MaxZ = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "antaLeftClosed":
					instance = (Scatola)reader.SetPrivateField("antaLeftClosed", reader.Read<System.Single>(), instance);
					break;
					case "antaLeftOpen":
						instance.antaLeftOpen = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "leftMinLimit":
					instance = (Scatola)reader.SetPrivateField("leftMinLimit", reader.Read<System.Single>(), instance);
					break;
					case "antaLeftOpenFixed":
					instance = (Scatola)reader.SetPrivateField("antaLeftOpenFixed", reader.Read<System.Single>(), instance);
					break;
					case "currentDistanceRight":
					instance = (Scatola)reader.SetPrivateField("currentDistanceRight", reader.Read<System.Single>(), instance);
					break;
					case "currentDistanceLeft":
					instance = (Scatola)reader.SetPrivateField("currentDistanceLeft", reader.Read<System.Single>(), instance);
					break;
					case "isClosed":
						instance.isClosed = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "rightClosed":
					instance = (Scatola)reader.SetPrivateField("rightClosed", reader.Read<System.Boolean>(), instance);
					break;
					case "leftClosed":
					instance = (Scatola)reader.SetPrivateField("leftClosed", reader.Read<System.Boolean>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_ScatolaArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_ScatolaArray() : base(typeof(Scatola[]), ES3UserType_Scatola.Instance)
		{
			Instance = this;
		}
	}
}