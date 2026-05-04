// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.06 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.06;sub:START;pass:START;ps:flbk:Mobile/Diffuse,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:False,lmpd:False,lprd:False,rprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:1,culm:0,dpts:2,wrdp:True,dith:0,ufog:False,aust:True,igpj:False,qofs:0,qpre:2,rntp:3,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.6470588,fgcg:0.6470588,fgcb:0.6470588,fgca:1,fgde:0.01,fgrn:217.5,fgrf:1000,ofsf:0,ofsu:0,f2p0:True;n:type:ShaderForge.SFN_Final,id:1,x:34725,y:32621,varname:node_1,prsc:2|diff-1653-OUT,spec-57-OUT,gloss-20-OUT,amspl-2286-OUT,clip-2-A;n:type:ShaderForge.SFN_Tex2d,id:2,x:34130,y:32149,ptovrint:False,ptlb:Diffuse,ptin:_Diffuse,varname:node_9120,prsc:2,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:8,x:33067,y:32731,ptovrint:False,ptlb:Specular,ptin:_Specular,varname:node_5818,prsc:2,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Slider,id:20,x:33476,y:32742,ptovrint:False,ptlb:Gloss,ptin:_Gloss,varname:node_5956,prsc:2,min:0,cur:0.5,max:2;n:type:ShaderForge.SFN_Slider,id:56,x:33476,y:32634,ptovrint:False,ptlb:Specular Force,ptin:_SpecularForce,varname:node_4162,prsc:2,min:0,cur:0,max:5;n:type:ShaderForge.SFN_Multiply,id:57,x:33898,y:32550,varname:node_57,prsc:2|A-8-RGB,B-56-OUT;n:type:ShaderForge.SFN_Cubemap,id:77,x:32948,y:32920,ptovrint:False,ptlb:Cubemap,ptin:_Cubemap,varname:node_823,prsc:2|DIR-7006-OUT;n:type:ShaderForge.SFN_Slider,id:79,x:33588,y:32895,ptovrint:False,ptlb:Reflection 1 Force,ptin:_Reflection1Force,varname:node_3714,prsc:2,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Multiply,id:157,x:33504,y:32969,varname:node_157,prsc:2|A-8-RGB,B-77-RGB;n:type:ShaderForge.SFN_Multiply,id:158,x:33479,y:33140,varname:node_158,prsc:2|A-244-OUT,B-77-RGB;n:type:ShaderForge.SFN_Add,id:232,x:34072,y:32973,varname:node_232,prsc:2|A-325-OUT,B-466-OUT;n:type:ShaderForge.SFN_OneMinus,id:244,x:33251,y:33017,varname:node_244,prsc:2|IN-8-RGB;n:type:ShaderForge.SFN_Slider,id:304,x:33610,y:33417,ptovrint:False,ptlb:Reflection 2 Force,ptin:_Reflection2Force,varname:node_8127,prsc:2,min:0,cur:0,max:3;n:type:ShaderForge.SFN_Multiply,id:325,x:33731,y:32969,varname:node_325,prsc:2|A-157-OUT,B-79-OUT;n:type:ShaderForge.SFN_Multiply,id:466,x:33709,y:33140,varname:node_466,prsc:2|A-158-OUT,B-304-OUT;n:type:ShaderForge.SFN_Tex2d,id:992,x:34211,y:31645,ptovrint:False,ptlb:Colormap,ptin:_Colormap,varname:node_8314,prsc:2,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Color,id:994,x:33969,y:31846,ptovrint:False,ptlb:Car Color,ptin:_CarColor,varname:node_419,prsc:2,glob:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:1580,x:34527,y:31768,varname:node_1580,prsc:2|A-992-RGB,B-994-RGB;n:type:ShaderForge.SFN_Subtract,id:1645,x:34413,y:32105,varname:node_1645,prsc:2|A-2-RGB,B-992-RGB;n:type:ShaderForge.SFN_Add,id:1653,x:34594,y:32026,varname:node_1653,prsc:2|A-1580-OUT,B-1645-OUT;n:type:ShaderForge.SFN_Fresnel,id:1843,x:34231,y:33193,varname:node_1843,prsc:2|EXP-2145-OUT;n:type:ShaderForge.SFN_Vector1,id:2145,x:34024,y:33138,varname:node_2145,prsc:2,v1:0.1;n:type:ShaderForge.SFN_Multiply,id:2286,x:34439,y:33056,varname:node_2286,prsc:2|A-232-OUT,B-1843-OUT;n:type:ShaderForge.SFN_NormalVector,id:5054,x:32561,y:32747,prsc:2,pt:False;n:type:ShaderForge.SFN_Reflect,id:7006,x:32769,y:32920,varname:node_7006,prsc:2|A-4367-OUT,B-5054-OUT;n:type:ShaderForge.SFN_ViewVector,id:7788,x:32382,y:32957,varname:node_7788,prsc:2;n:type:ShaderForge.SFN_Negate,id:4367,x:32583,y:32957,varname:node_4367,prsc:2|IN-7788-OUT;proporder:2-8-56-20-77-79-304-992-994;pass:END;sub:END;*/

Shader "Rune Studios/RVS" {
    Properties {
        _Diffuse ("Diffuse", 2D) = "white" {}
        _Specular ("Specular", 2D) = "white" {}
        _SpecularForce ("Specular Force", Range(0, 5)) = 0
        _Gloss ("Gloss", Range(0, 2)) = 0.5
        _Cubemap ("Cubemap", Cube) = "_Skybox" {}
        _Reflection1Force ("Reflection 1 Force", Range(0, 1)) = 0
        _Reflection2Force ("Reflection 2 Force", Range(0, 3)) = 0
        _Colormap ("Colormap", 2D) = "white" {}
        _CarColor ("Car Color", Color) = (1,1,1,1)
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            Fog {Mode Off}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma exclude_renderers xbox360 ps3 
            #pragma target 2.0
            uniform float4 _LightColor0;
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform sampler2D _Specular; uniform float4 _Specular_ST;
            uniform float _Gloss;
            uniform float _SpecularForce;
            uniform samplerCUBE _Cubemap;
            uniform float _Reflection1Force;
            uniform float _Reflection2Force;
            uniform sampler2D _Colormap; uniform float4 _Colormap_ST;
            uniform float4 _CarColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(i.uv0, _Diffuse));
                clip(_Diffuse_var.a - 0.5);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = _Gloss;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float4 _Specular_var = tex2D(_Specular,TRANSFORM_TEX(i.uv0, _Specular));
                float4 _Cubemap_var = texCUBE(_Cubemap,reflect((-1*viewDirection),i.normalDir));
                float3 specularColor = (_Specular_var.rgb*_SpecularForce);
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow);
                float3 indirectSpecular = (0 + ((((_Specular_var.rgb*_Cubemap_var.rgb)*_Reflection1Force)+(((1.0 - _Specular_var.rgb)*_Cubemap_var.rgb)*_Reflection2Force))*pow(1.0-max(0,dot(normalDirection, viewDirection)),0.1)));
                float3 specular = (directSpecular + indirectSpecular) * specularColor;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 indirectDiffuse = float3(0,0,0);
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float4 _Colormap_var = tex2D(_Colormap,TRANSFORM_TEX(i.uv0, _Colormap));
                float3 diffuse = (directDiffuse + indirectDiffuse) * ((_Colormap_var.rgb*_CarColor.rgb)+(_Diffuse_var.rgb-_Colormap_var.rgb));
/// Final Color:
                float3 finalColor = diffuse + specular;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        Pass {
            Name "ForwardAdd"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            Fog { Color (0,0,0,0) }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma exclude_renderers xbox360 ps3 
            #pragma target 2.0
            uniform float4 _LightColor0;
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform sampler2D _Specular; uniform float4 _Specular_ST;
            uniform float _Gloss;
            uniform float _SpecularForce;
            uniform sampler2D _Colormap; uniform float4 _Colormap_ST;
            uniform float4 _CarColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(i.uv0, _Diffuse));
                clip(_Diffuse_var.a - 0.5);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = _Gloss;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float4 _Specular_var = tex2D(_Specular,TRANSFORM_TEX(i.uv0, _Specular));
                float3 specularColor = (_Specular_var.rgb*_SpecularForce);
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow);
                float3 specular = directSpecular * specularColor;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float4 _Colormap_var = tex2D(_Colormap,TRANSFORM_TEX(i.uv0, _Colormap));
                float3 diffuse = directDiffuse * ((_Colormap_var.rgb*_CarColor.rgb)+(_Diffuse_var.rgb-_Colormap_var.rgb));
/// Final Color:
                float3 finalColor = diffuse + specular;
                return fixed4(finalColor * 1,0);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCollector"
            Tags {
                "LightMode"="ShadowCollector"
            }
            
            Fog {Mode Off}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCOLLECTOR
            #define SHADOW_COLLECTOR_PASS
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcollector
            #pragma exclude_renderers xbox360 ps3 
            #pragma target 2.0
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_COLLECTOR;
                float2 uv0 : TEXCOORD5;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_SHADOW_COLLECTOR(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
/////// Vectors:
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(i.uv0, _Diffuse));
                clip(_Diffuse_var.a - 0.5);
                SHADOW_COLLECTOR_FRAGMENT(i)
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Cull Off
            Offset 1, 1
            
            Fog {Mode Off}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma exclude_renderers xbox360 ps3 
            #pragma target 2.0
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
/////// Vectors:
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(i.uv0, _Diffuse));
                clip(_Diffuse_var.a - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Mobile/Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
