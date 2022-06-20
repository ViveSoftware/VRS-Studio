// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "UILayout_FrameBSide"
{
	Properties
	{
		_Color("Color", Color) = (0.5019608,0.5176471,0.5568628,0)
		_ColorB("ColorB", Color) = (0.345098,0.3647059,0.3960784,0)
		_Inner_op("Inner_op", Range( 0 , 1)) = 1
		_FrameColor("FrameColor", Color) = (0.5960785,0.5960785,0.5960785,0)
		_Overall_Gralevel("Overall_Gralevel", Range( 0.35 , 1)) = 0.43
		_Overall_Blur("Overall_Blur", Range( 0 , 1)) = 0.3882353
		[HDR]_OuterRimColor("OuterRimColor", Color) = (1,1,1,0)
		_OuterInRimLight("OuterInRimLight", Float) = 6.38
		_OutRim_Width("OutRim_Width", Range( 0 , 1)) = 0.4463888
		_OutRim_WidthB("OutRim_WidthB", Range( 0 , 10)) = 5
		_OutRim_Blur("OutRim_Blur", Range( 0 , 1)) = 0.1631206
		_OuterRim_power("OuterRim_power", Range( 0 , 2)) = 2
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float2 uv2_texcoord2;
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float4 _Color;
		uniform float4 _ColorB;
		uniform float4 _FrameColor;
		uniform float4 _OuterRimColor;
		uniform float _OuterRim_power;
		uniform float _OutRim_Width;
		uniform float _OutRim_Blur;
		uniform float _OuterInRimLight;
		uniform float _OutRim_WidthB;
		uniform float _Overall_Gralevel;
		uniform float _Overall_Blur;
		uniform float _Inner_op;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float smoothstepResult13 = smoothstep( 0.84 , 0.92 , i.uv2_texcoord2.y);
			float4 lerpResult16 = lerp( _Color , _ColorB , smoothstepResult13);
			float4 color27 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float4 color23 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float clampResult98 = clamp( ( 0.5 + ( ( 0.5 - i.uv2_texcoord2.x ) / 0.0 ) ) , 0.0 , 1.0 );
			float4 lerpResult25 = lerp( color27 , color23 , clampResult98);
			float4 lerpResult26 = lerp( lerpResult16 , _FrameColor , lerpResult25);
			o.Albedo = lerpResult26.rgb;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV76 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode76 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV76, _OuterRim_power ) );
			float fresnelNdotV65 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode65 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV65, 1.901694 ) );
			float clampResult72 = clamp( ( 1.0 - ( _OutRim_Width + ( ( _OutRim_Width - fresnelNode65 ) / _OutRim_Blur ) ) ) , 0.0 , 1.0 );
			float fresnelNdotV77 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode77 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV77, _OutRim_WidthB ) );
			float4 lerpResult80 = lerp( ( _OuterRimColor * fresnelNode76 ) , ( clampResult72 * _OuterRimColor * _OuterInRimLight ) , fresnelNode77);
			float4 OuterRim83 = lerpResult80;
			float4 lerpResult99 = lerp( color23 , OuterRim83 , clampResult98);
			o.Emission = lerpResult99.rgb;
			float4 color5 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float4 color3 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float clampResult91 = clamp( ( _Overall_Gralevel + ( ( _Overall_Gralevel - i.uv2_texcoord2.y ) / _Overall_Blur ) ) , 0.0 , 1.0 );
			float4 lerpResult6 = lerp( color5 , color3 , clampResult91);
			float4 lerpResult30 = lerp( ( lerpResult6 * _Inner_op ) , ( 1.0 * lerpResult6 ) , lerpResult25);
			o.Alpha = lerpResult30.r;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
34;673;1502;727;2872.826;-1052.677;1.003788;True;True
Node;AmplifyShaderEditor.RangedFloatNode;63;-3023.807,2755.168;Inherit;False;Constant;_InnerGlow_RangeB;InnerGlow_RangeB;9;0;Create;True;0;0;False;0;False;1.901694;3.76139;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-2631.523,2505.71;Inherit;False;Property;_OutRim_Width;OutRim_Width;8;0;Create;True;0;0;False;0;False;0.4463888;0.4463888;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;65;-2715.676,2651.703;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;67;-2289.083,2613.48;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-2452.745,2916.128;Inherit;False;Property;_OutRim_Blur;OutRim_Blur;10;0;Create;True;0;0;False;0;False;0.1631206;0.1631206;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;68;-2058.936,2721.174;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;90;-2393.03,1209.598;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;85;-2469.487,1349.439;Inherit;False;Property;_Overall_Gralevel;Overall_Gralevel;4;0;Create;True;0;0;False;0;False;0.43;0;0.35;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;69;-1827.001,2677.056;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;86;-2087.477,1339.807;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;92;-2797.01,254.3746;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;82;-1576.355,2081.713;Inherit;False;Property;_OuterRim_power;OuterRim_power;11;0;Create;True;0;0;False;0;False;2;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;70;-1668.541,2637.922;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;93;-2824.733,391.262;Inherit;False;Constant;_half;half;4;0;Create;True;0;0;False;0;False;0.5;0.3008071;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-2375.903,1442.966;Inherit;False;Property;_Overall_Blur;Overall_Blur;5;0;Create;True;0;0;False;0;False;0.3882353;0.2854134;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;73;-1400.06,2255.31;Inherit;False;Property;_OuterRimColor;OuterRimColor;6;1;[HDR];Create;True;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;94;-2779.883,487.7425;Inherit;False;Constant;_Float5;Float 5;5;0;Create;True;0;0;False;0;False;0;0.2854134;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;88;-1903.132,1324.025;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;95;-2491.457,384.5826;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;71;-1251.231,2883.803;Inherit;False;Property;_OuterInRimLight;OuterInRimLight;7;0;Create;True;0;0;False;0;False;6.38;0.97;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;76;-873.7737,2249.583;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-1284.016,3092.97;Inherit;False;Property;_OutRim_WidthB;OutRim_WidthB;9;0;Create;True;0;0;False;0;False;5;5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;72;-1456.315,2608.534;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;89;-1904.001,1484.437;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;96;-2307.112,368.8005;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-690.8617,1990.169;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-339.0208,2690.825;Inherit;True;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;77;-888.7467,2949.588;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;8.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;-1986.928,991.5125;Inherit;False;Constant;_Color0;Color 0;2;0;Create;True;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;91;-1710.41,1160.34;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;3;-1992.106,803.8121;Inherit;False;Constant;_Color1;Color 1;1;0;Create;True;0;0;False;0;False;0,0,0,0;0.1058824,0.6901961,0.9019608,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;97;-2307.981,529.2136;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-2125.064,-442.4566;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;80;-77.69855,2393.023;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1012.69,-321.4936;Inherit;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;False;0;False;0.92;0.87;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-1005.743,-203.5685;Inherit;False;Constant;_Float1;Float 1;6;0;Create;True;0;0;False;0;False;0.84;0.76;-1;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;9;-1287.327,-844.7578;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;False;0.5019608,0.5176471,0.5568628,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;27;-2770.982,-251.1732;Inherit;False;Constant;_Color3;Color 3;2;0;Create;True;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;6;-1500.826,969.0857;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;13;-1292.853,-394.485;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;98;-2114.39,205.1165;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-1104.323,1257.164;Inherit;False;Property;_Inner_op;Inner_op;2;0;Create;True;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-1184.275,647.9878;Inherit;False;Constant;_frame_op;frame_op;9;0;Create;True;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;83;249.8793,2386.862;Inherit;False;OuterRim;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;14;-1282.121,-627.7997;Inherit;False;Property;_ColorB;ColorB;1;0;Create;True;0;0;False;0;False;0.345098,0.3647059,0.3960784,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-2787.897,-65.0023;Inherit;False;Constant;_Color4;Color 4;5;0;Create;True;0;0;False;0;False;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;24;-1252.374,-128.9568;Inherit;False;Property;_FrameColor;FrameColor;3;0;Create;True;0;0;False;0;False;0.5960785,0.5960785,0.5960785,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;84;-1776.132,250.9982;Inherit;True;83;OuterRim;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;16;-864.6982,-580.4862;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;25;-2021.442,-96.56355;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-799.3729,710.6185;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-783.0468,1081.016;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;99;-1333.109,219.3924;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;26;-659.4086,-78.56024;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;30;-349.6165,349.8521;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-3287.407,1195.659;Inherit;False;Constant;_Gralevel;Gralevel;6;0;Create;True;0;0;False;0;False;0.71;0.71;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1;-3260.403,1291.945;Inherit;False;Constant;_blur;blur;7;0;Create;True;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;22;-3612.803,120.5254;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-3256.363,1034.34;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;4;-2882.079,1105.044;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;21;-4048.799,327.8255;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;19;-3253.62,183.1198;Inherit;False;Constant;_Float2;Float 2;3;0;Create;True;0;0;False;0;False;0.512278;0.87;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-3246.674,301.0449;Inherit;False;Constant;_Float3;Float 3;6;0;Create;True;0;0;False;0;False;0.508697;0.76;-1;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;UILayout_FrameBSide;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;65;3;63;0
WireConnection;67;0;64;0
WireConnection;67;1;65;0
WireConnection;68;0;67;0
WireConnection;68;1;66;0
WireConnection;69;0;64;0
WireConnection;69;1;68;0
WireConnection;86;0;85;0
WireConnection;86;1;90;2
WireConnection;70;0;69;0
WireConnection;88;0;86;0
WireConnection;88;1;87;0
WireConnection;95;0;93;0
WireConnection;95;1;92;1
WireConnection;76;3;82;0
WireConnection;72;0;70;0
WireConnection;89;0;85;0
WireConnection;89;1;88;0
WireConnection;96;0;95;0
WireConnection;96;1;94;0
WireConnection;78;0;73;0
WireConnection;78;1;76;0
WireConnection;79;0;72;0
WireConnection;79;1;73;0
WireConnection;79;2;71;0
WireConnection;77;3;75;0
WireConnection;91;0;89;0
WireConnection;97;0;93;0
WireConnection;97;1;96;0
WireConnection;80;0;78;0
WireConnection;80;1;79;0
WireConnection;80;2;77;0
WireConnection;6;0;5;0
WireConnection;6;1;3;0
WireConnection;6;2;91;0
WireConnection;13;0;10;2
WireConnection;13;1;12;0
WireConnection;13;2;11;0
WireConnection;98;0;97;0
WireConnection;83;0;80;0
WireConnection;16;0;9;0
WireConnection;16;1;14;0
WireConnection;16;2;13;0
WireConnection;25;0;27;0
WireConnection;25;1;23;0
WireConnection;25;2;98;0
WireConnection;31;0;28;0
WireConnection;31;1;6;0
WireConnection;18;0;6;0
WireConnection;18;1;17;0
WireConnection;99;0;23;0
WireConnection;99;1;84;0
WireConnection;99;2;98;0
WireConnection;26;0;16;0
WireConnection;26;1;24;0
WireConnection;26;2;25;0
WireConnection;30;0;18;0
WireConnection;30;1;31;0
WireConnection;30;2;25;0
WireConnection;22;0;21;1
WireConnection;22;1;20;0
WireConnection;22;2;19;0
WireConnection;4;0;7;2
WireConnection;4;1;1;0
WireConnection;4;2;2;0
WireConnection;0;0;26;0
WireConnection;0;2;99;0
WireConnection;0;9;30;0
ASEEND*/
//CHKSM=D97BDBDC2F1C7A43E41BE76896C9703CD9135120