// Versione leggera per Meta Quest basata su Complex.shader (AkilliMum)
//
// Differenze rispetto a Complex:
//  - _FULLMIRROR hardcodato ON (non e una variabile, e una shader keyword)
//  - Solo target GLES3 / Vulkan (Quest non supporta target 4.5)
//  - Rimossi: ShadowCaster, DepthNormals, Meta, Universal2D pass
//  - Rimossi: NormalMap, Parallax, Detail, ClearCoat, Wave, Ripple, Refraction, Mask
//  - Smoothness rimasto come unica opzione visiva
//  - Riusa LitInput.hlsl e LitForwardPass.hlsl originali (gia testati e funzionanti)

Shader "AkilliMum/URP/Mirrors/PBR_Quest"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}

        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.9
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0

        _ReceiveShadows("Receive Shadows", Float) = 1.0

        // Compatibilità LitInput.hlsl (devono esistere anche se non usate)
        [HideInInspector] _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [HideInInspector] _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [HideInInspector] _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0
        [HideInInspector] _SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
        [HideInInspector] _SpecGlossMap("Specular", 2D) = "white" {}
        [HideInInspector] _MetallicGlossMap("Metallic", 2D) = "white" {}
        [HideInInspector] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [HideInInspector] _EnvironmentReflections("Environment Reflections", Float) = 1.0
        [HideInInspector] _BumpScale("Scale", Float) = 1.0
        [HideInInspector] _BumpMap("Normal Map", 2D) = "bump" {}
        [HideInInspector] _Parallax("Scale", Range(0.005, 0.08)) = 0.005
        [HideInInspector] _ParallaxMap("Height Map", 2D) = "black" {}
        [HideInInspector] _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        [HideInInspector] _OcclusionMap("Occlusion", 2D) = "white" {}
        [HideInInspector] _EmissionColor("Color", Color) = (0,0,0)
        [HideInInspector] _EmissionMap("Emission", 2D) = "white" {}
        [HideInInspector] _DetailMask("Detail Mask", 2D) = "white" {}
        [HideInInspector] _DetailAlbedoMapScale("Scale", Range(0.0, 2.0)) = 1.0
        [HideInInspector] _DetailAlbedoMap("Detail Albedo x2", 2D) = "linearGrey" {}
        [HideInInspector] _DetailNormalMapScale("Scale", Range(0.0, 2.0)) = 1.0
        [HideInInspector] _DetailNormalMap("Normal Map", 2D) = "bump" {}
        [HideInInspector] _ClearCoat("Clear Coat", Float) = 0.0
        [HideInInspector] _ClearCoatMap("Clear Coat Map", 2D) = "white" {}
        [HideInInspector] _ClearCoatMask("Clear Coat Mask", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _ClearCoatSmoothness("Clear Coat Smoothness", Range(0.0, 1.0)) = 1.0
        [HideInInspector] _WorkflowMode("WorkflowMode", Float) = 1.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

        // ── Mirror properties (assegnate da MirrorManager via script) ────────
        [HideInInspector] _IsMultiPass("IsMultiPass Mobile", Float) = 0.
        [HideInInspector] _LeftOrCenterTexture("Left or Center Texture", 2D) = "white" {}
        [HideInInspector] _RightTexture("Right Texture", 2D) = "white" {}
        [HideInInspector] _ReflectionIntensity("Reflection Intensity", Range(0.0, 1.0)) = 0.9
        [HideInInspector] _DisableGI("Disable GI", Float) = 1.
        [HideInInspector] _UseFresnel("Fresnel Like Reflection", Float) = 0.
        [HideInInspector] _LODLevel("Mip Level", Range(0, 10)) = 0
        [HideInInspector] _WorkType("Work Type", Float) = 1.
        [HideInInspector] _DeviceType("Device Type", Float) = 1.
        [HideInInspector] _MixBlackColor("Mix Black Color", Float) = 0.
        [HideInInspector] _ClipUV("Clip UV", int) = 99
        [HideInInspector] _ClipEye("Clip Eye", int) = 0
        [HideInInspector] _ClipPercentage("Clipping Percentage", int) = 20
        [HideInInspector] _RefractionTex("Refraction", 2D) = "bump" {}
        [HideInInspector] _ReflectionRefraction("Reflection Refraction", Float) = 0.0
        [HideInInspector] _EnableDepthBlur("Enable Depth Blur", Float) = -1.0
        [HideInInspector] _EnableSimpleDepth("Enable Simple Depth", Float) = -1.0
        [HideInInspector] _SimpleDepthCutoff("Simple Depth Cutoff", Range(0.0, 50.0)) = 0.5
        [HideInInspector] _LeftOrCenterDepthTexture("Left or Center Depth Texture", 2D) = "white" {}
        [HideInInspector] _RightDepthTexture("Right Depth Texture", 2D) = "white" {}
        [HideInInspector] _NearClip("Near Clip", Float) = 0.3
        [HideInInspector] _FarClip("Far Clip", Float) = 1000
        [HideInInspector] _MaskTex("Mask", 2D) = "white" {}
        [HideInInspector] _MaskCutoff("Mask Cutoff", Range(0.0, 1.0)) = 0.5
        [HideInInspector] _MaskEdgeDarkness("Mask Edge Darkness", Range(1.0, 50.0)) = 1.
        [HideInInspector] _MaskTiling("Mask Tiling", Vector) = (1,1,1,1)
        [HideInInspector] _WaveNoiseTex("Wave Noise Tex", 2D) = "white" {}
        [HideInInspector] _WaveSize("Wave Size", float) = 12.0
        [HideInInspector] _WaveDistortion("Wave Distortion", Float) = 0.02
        [HideInInspector] _WaveSpeed("Wave Speed", Float) = 3.0
        [HideInInspector] _RippleTex("Ripple", 2D) = "bump" {}
        [HideInInspector] _RippleSize("Ripple Size", Float) = 2.0
        [HideInInspector] _RippleRefraction("Ripple Refraction", Float) = 0.02
        [HideInInspector] _RippleDensity("Ripple Density", Float) = 1.0
        [HideInInspector] _RippleSpeed("Ripple Speed", Float) = 0.3
        [HideInInspector] _EnableLocallyCorrection("Enable Locally Correction", Float) = 0.
        [HideInInspector] _BBoxMin("BBox Min", Vector) = (0,0,0,0)
        [HideInInspector] _BBoxMax("BBox Max", Vector) = (0,0,0,0)
        [HideInInspector] _EnviCubeMapPos("CubeMap Pos", Vector) = (0,0,0,0)
        [HideInInspector] _EnableRotation("Enable Rotation", Float) = 0
        [HideInInspector] _EnviRotation("Environment Rotation", Vector) = (0,0,0,0)
        [HideInInspector] _EnviPosition("Environment Position", Vector) = (0,0,0,0)
    }

    SubShader
    {
        // Target GLES3 — compatibile con Meta Quest (Vulkan/GLES3)
        Tags
        {
            "RenderType"          = "Opaque"
            "RenderPipeline"      = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector"     = "True"
            "ShaderModel"         = "2.0"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore vulkan
            #pragma target 2.0

            // ── _FULLMIRROR hardcodato ON ────────────────────────────────
            // Non e una variabile float: e una shader keyword che attiva
            // il percorso "specchio puro" in LitForwardPass.hlsl
            #define _FULLMIRROR

            // Feature disabilitate per alleggerire lo shader su Quest
            // (nessuna normalmap, parallax, detail, clearcoat, wave, ripple)

            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "LitInput.hlsl"
            #include "LitForwardPass.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore vulkan
            #pragma target 2.0
            #pragma multi_compile_instancing

            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "LitInput.hlsl"
            #include "DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/Lit"
    CustomEditor "AkilliMum.SRP.Mirror.ComplexEditor"
}
