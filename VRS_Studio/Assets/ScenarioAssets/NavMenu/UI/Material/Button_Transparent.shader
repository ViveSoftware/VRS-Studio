// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Button_Transparent"
{
	Properties
	{
		[HDR]_RimColor("RimColor", Color) = (1,1,1,0)
		[HDR]_Color("Color", Color) = (1,1,1,0)
		_RimLight1("RimLight", Float) = 6.38
		_OutRim_Width1("OutRim_Width", Range( 0 , 1)) = 0.4463888
		_OutRim_WidthB1("OutRim_WidthB", Range( 0 , 10)) = 5
		_OutRim_Blur1("OutRim_Blur", Range( 0 , 1)) = 0.1631206
		_power("power", Range( 0 , 2)) = 2
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend OneMinusDstColor One
		
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float4 _Color;
		uniform float _power;
		uniform float _OutRim_Width1;
		uniform float _OutRim_Blur1;
		uniform float4 _RimColor;
		uniform float _RimLight1;
		uniform float _OutRim_WidthB1;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV1 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode1 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV1, _power ) );
			float fresnelNdotV10 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode10 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV10, 1.901694 ) );
			float clampResult26 = clamp( ( 1.0 - ( _OutRim_Width1 + ( ( _OutRim_Width1 - fresnelNode10 ) / _OutRim_Blur1 ) ) ) , 0.0 , 1.0 );
			float fresnelNdotV34 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode34 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV34, _OutRim_WidthB1 ) );
			float4 lerpResult37 = lerp( ( _Color * fresnelNode1 ) , ( clampResult26 * _RimColor * _RimLight1 ) , fresnelNode34);
			o.Emission = ( lerpResult37 + float4( 0,0,0,0 ) ).rgb;
			o.Alpha = fresnelNode1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 worldPos : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
0;440;1297;579;2639.793;219.7983;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;6;-3727.066,597.3482;Inherit;False;Constant;_InnerGlow_RangeB1;InnerGlow_RangeB;9;0;Create;True;0;0;False;0;False;1.901694;3.76139;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-3334.782,347.8908;Inherit;False;Property;_OutRim_Width1;OutRim_Width;4;0;Create;True;0;0;False;0;False;0.4463888;0.4463888;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;10;-3418.935,493.883;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-3156.004,758.3085;Inherit;False;Property;_OutRim_Blur1;OutRim_Blur;6;0;Create;True;0;0;False;0;False;0.1631206;0.1631206;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;13;-2992.342,455.6606;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;16;-2762.195,563.3543;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;18;-2530.26,519.2361;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-2279.614,-76.10645;Inherit;False;Property;_power;power;7;0;Create;True;0;0;False;0;False;2;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;20;-2371.8,480.1023;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-1954.49,725.9835;Inherit;False;Property;_RimLight1;RimLight;3;0;Create;True;0;0;False;0;False;6.38;0.97;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;26;-2159.574,450.7148;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;22;-2192.553,217.1444;Inherit;False;Property;_RimColor;RimColor;1;1;[HDR];Create;True;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-1842.362,-326.4083;Inherit;False;Property;_Color;Color;2;1;[HDR];Create;True;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;28;-1987.275,935.1506;Inherit;False;Property;_OutRim_WidthB1;OutRim_WidthB;5;0;Create;True;0;0;False;0;False;5;5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;1;-1577.033,91.76366;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;34;-1592.006,791.7685;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;8.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-1394.121,-167.6506;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-1042.28,533.0054;Inherit;True;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;37;-780.9578,235.2039;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;39;-410.9435,-127.326;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Button_Transparent;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;5;4;False;-1;1;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;3;6;0
WireConnection;13;0;9;0
WireConnection;13;1;10;0
WireConnection;16;0;13;0
WireConnection;16;1;14;0
WireConnection;18;0;9;0
WireConnection;18;1;16;0
WireConnection;20;0;18;0
WireConnection;26;0;20;0
WireConnection;1;3;41;0
WireConnection;34;3;28;0
WireConnection;5;0;4;0
WireConnection;5;1;1;0
WireConnection;29;0;26;0
WireConnection;29;1;22;0
WireConnection;29;2;23;0
WireConnection;37;0;5;0
WireConnection;37;1;29;0
WireConnection;37;2;34;0
WireConnection;39;0;37;0
WireConnection;0;2;39;0
WireConnection;0;9;1;0
ASEEND*/
//CHKSM=F267477AF2EF44237EA965AAAC8E9728158B82A8