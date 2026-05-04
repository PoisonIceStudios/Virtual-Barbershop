using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace BNG {

    public class IntegrationsEditor : EditorWindow {

        [MenuItem("VRIF/VRIF Integrations")]
        public static void ShowWindow() {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(IntegrationsEditor));
        }

        void OnGUI() {
            GUILayout.Label("VRIF Integration Settings", EditorStyles.boldLabel);

            VRIFSettings.OculusIntegration = EditorGUILayout.Toggle("Oculus Integration", VRIFSettings.OculusIntegration);
            VRIFSettings.SteamVRIntegration = EditorGUILayout.Toggle("SteamVR Integration", VRIFSettings.SteamVRIntegration);
            VRIFSettings.PicoIntegration = EditorGUILayout.Toggle("Pico Integration", VRIFSettings.PicoIntegration);

            GUILayout.Label("", EditorStyles.boldLabel);

            GUILayout.Label("*Enabling an integration will add the appropriate Scripting Define Symbol to your Project Settings for you.", EditorStyles.label);

            GUILayout.Label("*Note : The project will rebuild after toggling an integration.", EditorStyles.boldLabel);

            EditorGUILayout.Separator();

        }
    }

    public class VRIFSettings {
        public static bool OculusIntegration {
            get {
                return EditorPrefs.GetBool("OculusIntegration", false);
            }
            set {
                EditorPrefs.SetBool("OculusIntegration", value);

                if(value) {
                    AddDefineSymbol("OCULUS_INTEGRATION");
                }
                else {
                    RemoveDefineSymbol("OCULUS_INTEGRATION");
                }
            }
        }

        public static bool SteamVRIntegration {
            get {
                return EditorPrefs.GetBool("SteamVRIntegration", false);
            }
            set {
                EditorPrefs.SetBool("SteamVRIntegration", value);

                if (value) {
                    AddDefineSymbol("STEAM_VR_SDK");
                }
                else {
                    RemoveDefineSymbol("STEAM_VR_SDK");
                }
            }
        }

        public static bool PicoIntegration {
            get {
                return EditorPrefs.GetBool("PicoIntegration", false);
            }
            set {
                EditorPrefs.SetBool("PicoIntegration", value);

                if (value) {
                    AddDefineSymbol("PICO_SDK");
                }
                else {
                    RemoveDefineSymbol("PICO_SDK");
                }
            }
        }

        static void AddDefineSymbol(string symbolName) {
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string definesString = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);

            // Already included
            if(definesString.Contains(symbolName)) {
                return;
            }

            List<string> allDefines = definesString.Split(';').ToList();

            allDefines.Add(symbolName);
            string result = string.Join(";", allDefines.ToArray());

            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, result);
        }

        static void RemoveDefineSymbol(string symbolName) {
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string definesString = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);

            if(definesString.Contains(symbolName + ";")) {
                definesString = definesString.Replace(symbolName + ";", "");
            }
            else if (definesString.Contains(symbolName)) {
                definesString = definesString.Replace(symbolName, "");
            }
            else {
                return;
            }

            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, definesString);
        }
    }
}

