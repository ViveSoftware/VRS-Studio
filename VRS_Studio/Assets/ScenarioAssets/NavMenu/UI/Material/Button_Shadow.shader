// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Button_Shadow"
{
	Properties
	{
		[HDR]_Color("Color", Color) = (0.009433955,0.0175594,0.03773582,0)
		_Opacity("Opacity", Range( 0 , 1)) = 0.6
		_size("size", Range( 0 , 0.45)) = 0.45
		_Blur("Blur", Range( 0 , 0.15)) = 0.1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Overlay+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color;
		uniform float _Opacity;
		uniform float _size;
		uniform float _Blur;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Emission = _Color.rgb;
			float2 CenteredUV15_g7 = ( i.uv_texcoord - float2( 0.5,0.5 ) );
			float2 break17_g7 = CenteredUV15_g7;
			float2 appendResult23_g7 = (float2(( length( CenteredUV15_g7 ) * 0.5 * 2.0 ) , ( atan2( break17_g7.x , break17_g7.y ) * ( 1.0 / 6.28318548202515 ) * 1.0 )));
			float clampResult61 = clamp( ( _size + ( ( _size - appendResult23_g7.x ) / _Blur ) ) , 0.0 , 1.0 );
			o.Alpha = ( _Opacity * clampResult61 );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
383.2;532.8;1470;643;1018.643;230.4024;1.158836;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;54;-1982.157,-205.5546;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;62;-1758.973,-43.27761;Inherit;False;Constant;_Vector2;Vector 2;7;0;Create;True;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FunctionNode;55;-1502.86,-136.5966;Inherit;False;Polar Coordinates;-1;;7;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;0.5;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;57;-1257.92,-140.2906;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;56;-1295.62,110.7334;Inherit;False;Property;_size;size;2;0;Create;True;0;0;False;0;False;0.45;0.1;0;0.45;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;63;-966.246,-217.2785;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-973.871,236.9409;Inherit;False;Property;_Blur;Blur;3;0;Create;True;0;0;False;0;False;0.1;0.09;0;0.15;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;59;-631.9828,115.9133;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;60;-607.1537,-127.0727;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;61;-361.0268,180.1714;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-367.5,-16;Inherit;False;Property;_Opacity;Opacity;1;0;Create;True;0;0;False;0;False;0.6;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;64;-53.46753,54.80762;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-172.5,-268;Inherit;False;Property;_Color;Color;0;1;[HDR];Create;True;0;0;False;0;False;0.009433955,0.0175594,0.03773582,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;168,-202;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Button_Shadow;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Transparent;;Overlay;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;4;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;55;1;54;0
WireConnection;55;2;62;0
WireConnection;57;0;55;0
WireConnection;63;0;56;0
WireConnection;63;1;57;0
WireConnection;59;0;63;0
WireConnection;59;1;58;0
WireConnection;60;0;56;0
WireConnection;60;1;59;0
WireConnection;61;0;60;0
WireConnection;64;0;2;0
WireConnection;64;1;61;0
WireConnection;0;2;1;0
WireConnection;0;9;64;0
ASEEND*/
//CHKSM=089DF450098DAB51984186D76FD7594CE692EE6B