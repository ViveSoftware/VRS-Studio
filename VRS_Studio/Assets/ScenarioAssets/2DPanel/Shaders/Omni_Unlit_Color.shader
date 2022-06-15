// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Omni/Unlit/Color"
{
	Properties
	{
		[Enum(Filled,0,Outlined,1)]_ColorType("Color Type", Float) = 0
		_Normal("Normal", Color) = (0.3372549,0.8509804,0.9647059,1)
		_Highlighted("Highlighted", Color) = (0,0.7019608,0.8901961,1)
		_Pressed("Pressed", Color) = (0.3372549,0.8509804,0.9647059,1)
		_Active("Active", Color) = (0.3372549,0.8509804,0.9647059,1)
		_ActiveHighlighted("ActiveHighlighted", Color) = (0,0.7019608,0.8901961,1)
		_ActivePressed("ActivePressed", Color) = (0.3372549,0.8509804,0.9647059,1)
		_Error("Error", Color) = (0.8313726,0.2039216,0.1882353,1)
		_Disabled("Disabled", Color) = (0.9058824,0.9176471,0.9411765,1)
		[IntRange][Enum(Normal,0,Highlighted,1,Pressed,2,Active,3,ActiveHighlighted,4,ActivePressed,5)]_State1("State 1", Range( 0 , 7)) = 0
		[IntRange][Enum(Normal,0,Highlighted,1,Pressed,2,Active,3,ActiveHighlighted,4,ActivePressed,5)]_State2("State 2", Range( 0 , 7)) = 1
		_StateTween("State Tween", Range( 1 , 2)) = 1
		[Enum(None,0,Error,1,Disabled,2)]_StateFail("State Fail", Float) = 0
		_StateFailOverrideTween("State Fail Override Tween", Range( 0 , 1)) = 1
		_OutlinePosition("Outline Position", Range( 0 , 0.01)) = 0.00199
		_Gradient("Gradient", Color) = (0.8470588,0.972549,0.9568627,1)
		[KeywordEnum(XPositve,XNegative,YPositive,YNegative)] _GradientDirection("Gradient Direction", Float) = 1
		_GradientIntensity("Gradient Intensity", Range( 0 , 1)) = 0
		_GradientPosition("Gradient Position", Range( -1 , 1)) = 0
		_GradientDistribution("Gradient Distribution", Range( 0 , 1)) = 0
		_DepthColor("Depth Color", Color) = (0.3372549,0.8509804,0.9647059,1)
		_DepthIntensity("Depth Intensity", Range( 0 , 1)) = 0
		_DepthPosition("Depth Position", Range( 0 , 0.01)) = 0.002
		_DepthDistribution("Depth Distribution", Range( 0 , 0.01)) = 0.004
		_AOColor("AO Color", Color) = (0.4745098,0.7411765,0.9490196,1)
		_AOIntensity("AO Intensity", Range( 0 , 1)) = 0
		_AODistribution("AO Distribution", Range( 0 , 1)) = 0
		_AOXOffset("AO X Offset", Float) = 0
		_AOYOffset("AO Y Offset", Float) = 0
		_AOWidth("AO Width", Float) = 0
		_AOHeight("AO Height", Float) = 0
		_SheenColor("Sheen Color", Color) = (0.172549,0.227451,0.3137255,1)
		_SheenPower("Sheen Power", Range( 0 , 1)) = 0.3
	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		

		Pass
		{
			Name "Unlit"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#pragma shader_feature _GRADIENTDIRECTION_XPOSITVE _GRADIENTDIRECTION_XNEGATIVE _GRADIENTDIRECTION_YPOSITIVE _GRADIENTDIRECTION_YNEGATIVE


			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float3 ase_normal : NORMAL;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			uniform float _ColorType;
			uniform float _State1;
			uniform float _State2;
			uniform float _StateTween;
			uniform float _StateFail;
			uniform float4 _Disabled;
			uniform float4 _Error;
			uniform float _StateFailOverrideTween;
			uniform float4 _Pressed;
			uniform float4 _Highlighted;
			uniform float4 _Normal;
			uniform float4 _ActivePressed;
			uniform float4 _ActiveHighlighted;
			uniform float4 _Active;
			uniform float4 _Gradient;
			uniform float _GradientPosition;
			uniform float _GradientDistribution;
			uniform float _GradientIntensity;
			uniform float4 _AOColor;
			uniform float _AOWidth;
			uniform float _AOXOffset;
			uniform float _AOHeight;
			uniform float _AOYOffset;
			uniform float _AODistribution;
			uniform float _AOIntensity;
			uniform float _OutlinePosition;
			uniform float4 _DepthColor;
			uniform float _DepthPosition;
			uniform float _DepthDistribution;
			uniform float _DepthIntensity;
			uniform float4 _SheenColor;
			uniform float _SheenPower;
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.ase_texcoord1.xyz = ase_worldPos;
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord2.xyz = ase_worldNormal;
				
				o.ase_texcoord = v.vertex;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;
				o.ase_texcoord2.w = 0;
				
				v.vertex.xyz +=  float3(0,0,0) ;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				fixed4 finalColor;
				float4 _OutlinedFillPressed = float4(1,1,1,1);
				float4 _OutlinedFillHighlighted = float4(0.8901961,0.9372549,0.9882353,1);
				float4 _OutlinedFillNormal = float4(1,1,1,1);
				float4 ifLocalVar336 = 0;
				if( _State1 > 1.0 )
				ifLocalVar336 = _OutlinedFillPressed;
				else if( _State1 == 1.0 )
				ifLocalVar336 = _OutlinedFillHighlighted;
				else if( _State1 < 1.0 )
				ifLocalVar336 = _OutlinedFillNormal;
				float4 ifLocalVar337 = 0;
				if( _State1 > 4.0 )
				ifLocalVar337 = _OutlinedFillPressed;
				else if( _State1 == 4.0 )
				ifLocalVar337 = _OutlinedFillHighlighted;
				else if( _State1 < 4.0 )
				ifLocalVar337 = _OutlinedFillNormal;
				float4 ifLocalVar338 = 0;
				if( _State2 > 1.0 )
				ifLocalVar338 = _OutlinedFillPressed;
				else if( _State2 == 1.0 )
				ifLocalVar338 = _OutlinedFillHighlighted;
				else if( _State2 < 1.0 )
				ifLocalVar338 = _OutlinedFillNormal;
				float4 ifLocalVar335 = 0;
				if( _State2 > 4.0 )
				ifLocalVar335 = _OutlinedFillPressed;
				else if( _State2 == 4.0 )
				ifLocalVar335 = _OutlinedFillHighlighted;
				else if( _State2 < 4.0 )
				ifLocalVar335 = _OutlinedFillNormal;
				float4 lerpResult341 = lerp( (( _State1 >= 0.0 && _State1 <= 2.0 ) ? ifLocalVar336 :  ifLocalVar337 ) , (( _State2 >= 0.0 && _State2 <= 2.0 ) ? ifLocalVar338 :  ifLocalVar335 ) , ( _StateTween - 1.0 ));
				float4 ifLocalVar343 = 0;
				if( _StateFail > 1.0 )
				ifLocalVar343 = _Disabled;
				else if( _StateFail == 1.0 )
				ifLocalVar343 = _Error;
				else if( _StateFail < 1.0 )
				ifLocalVar343 = lerpResult341;
				float4 lerpResult344 = lerp( lerpResult341 , ifLocalVar343 , _StateFailOverrideTween);
				float4 ifLocalVar290 = 0;
				if( _State1 > 1.0 )
				ifLocalVar290 = _Pressed;
				else if( _State1 == 1.0 )
				ifLocalVar290 = _Highlighted;
				else if( _State1 < 1.0 )
				ifLocalVar290 = _Normal;
				float4 ifLocalVar291 = 0;
				if( _State1 > 4.0 )
				ifLocalVar291 = _ActivePressed;
				else if( _State1 == 4.0 )
				ifLocalVar291 = _ActiveHighlighted;
				else if( _State1 < 4.0 )
				ifLocalVar291 = _Active;
				float4 ifLocalVar299 = 0;
				if( _State2 > 1.0 )
				ifLocalVar299 = _Pressed;
				else if( _State2 == 1.0 )
				ifLocalVar299 = _Highlighted;
				else if( _State2 < 1.0 )
				ifLocalVar299 = _Normal;
				float4 ifLocalVar302 = 0;
				if( _State2 > 4.0 )
				ifLocalVar302 = _ActivePressed;
				else if( _State2 == 4.0 )
				ifLocalVar302 = _ActiveHighlighted;
				else if( _State2 < 4.0 )
				ifLocalVar302 = _Active;
				float4 lerpResult244 = lerp( (( _State1 >= 0.0 && _State1 <= 2.0 ) ? ifLocalVar290 :  ifLocalVar291 ) , (( _State2 >= 0.0 && _State2 <= 2.0 ) ? ifLocalVar299 :  ifLocalVar302 ) , ( _StateTween - 1.0 ));
				float4 ifLocalVar307 = 0;
				if( _StateFail > 1.0 )
				ifLocalVar307 = _Disabled;
				else if( _StateFail == 1.0 )
				ifLocalVar307 = _Error;
				else if( _StateFail < 1.0 )
				ifLocalVar307 = lerpResult244;
				float4 lerpResult308 = lerp( lerpResult244 , ifLocalVar307 , _StateFailOverrideTween);
				float4 ifLocalVar358 = 0;
				if( _ColorType > 0.0 )
				ifLocalVar358 = lerpResult344;
				else if( _ColorType == 0.0 )
				ifLocalVar358 = lerpResult308;
				#if defined(_GRADIENTDIRECTION_XPOSITVE)
				float staticSwitch232 = i.ase_texcoord.xyz.x;
				#elif defined(_GRADIENTDIRECTION_XNEGATIVE)
				float staticSwitch232 = ( i.ase_texcoord.xyz.x * -1.0 );
				#elif defined(_GRADIENTDIRECTION_YPOSITIVE)
				float staticSwitch232 = i.ase_texcoord.xyz.y;
				#elif defined(_GRADIENTDIRECTION_YNEGATIVE)
				float staticSwitch232 = ( i.ase_texcoord.xyz.y * -1.0 );
				#else
				float staticSwitch232 = ( i.ase_texcoord.xyz.x * -1.0 );
				#endif
				float4 lerpResult165 = lerp( ifLocalVar358 , _Gradient , saturate( ( ( staticSwitch232 + _GradientPosition ) / _GradientDistribution ) ));
				float4 lerpResult212 = lerp( ifLocalVar358 , lerpResult165 , _GradientIntensity);
				float3 appendResult275 = (float3(( _AOWidth + _AOXOffset ) , ( _AOHeight + _AOYOffset ) , 1.0));
				float4 lerpResult161 = lerp( lerpResult212 , _AOColor , saturate( ( ( length( ( i.ase_texcoord.xyz / appendResult275 ) ) + ( 0.5 * -1.0 ) ) / _AODistribution ) ));
				float4 lerpResult264 = lerp( lerpResult212 , lerpResult161 , _AOIntensity);
				float4 lerpResult195 = lerp( lerpResult264 , lerpResult308 , saturate( ( ( i.ase_texcoord.xyz.z + _OutlinePosition ) / 0.0 ) ));
				float4 ifLocalVar360 = 0;
				if( _ColorType > 0.0 )
				ifLocalVar360 = lerpResult195;
				else if( _ColorType == 0.0 )
				ifLocalVar360 = lerpResult264;
				float4 lerpResult325 = lerp( ifLocalVar360 , _DepthColor , saturate( ( ( i.ase_texcoord.xyz.z + _DepthPosition ) / _DepthDistribution ) ));
				float4 lerpResult346 = lerp( ifLocalVar360 , lerpResult325 , _DepthIntensity);
				float3 ase_worldPos = i.ase_texcoord1.xyz;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_worldNormal = i.ase_texcoord2.xyz;
				float fresnelNdotV226 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode226 = ( 0.0 + _SheenPower * pow( 1.0 - fresnelNdotV226, 1.0 ) );
				float4 lerpResult222 = lerp( lerpResult346 , _SheenColor , fresnelNode226);
				
				
				finalColor = lerpResult222;
				return finalColor;
			}
			ENDCG
		}
	}
	
	
	
}
/*ASEBEGIN
Version=15800
24;648;917;784;6666.541;2040.613;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;310;-5979.56,-2897.898;Float;False;2060.671;1156.277;;23;235;236;42;238;239;237;291;295;290;287;300;299;302;298;308;307;306;309;245;244;284;241;240;Filled Colors;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;155;-4438.248,-343.9894;Float;False;1683.429;523.4005;;20;204;276;282;280;279;283;281;156;275;278;250;252;247;206;251;231;158;264;265;161;XY AO;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;331;-6161.17,-1653.43;Float;False;Constant;_OutlinedFillNormal;Outlined Fill Normal;32;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;329;-6168.17,-1480.43;Float;False;Constant;_OutlinedFillHighlighted;Outlined Fill Highlighted;33;0;Create;True;0;0;False;0;0.8901961,0.9372549,0.9882353,1;0.9058824,0.9176471,0.9411765,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;238;-5908.969,-2111.124;Float;False;Property;_ActiveHighlighted;ActiveHighlighted;5;0;Create;True;0;0;False;0;0,0.7019608,0.8901961,1;0,0.7019608,0.8901961,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;42;-5908.969,-2831.124;Float;False;Property;_Normal;Normal;1;0;Create;True;0;0;False;0;0.3372549,0.8509804,0.9647059,1;1,1,1,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;328;-6161.17,-1301.43;Float;False;Constant;_OutlinedFillPressed;Outlined Fill Pressed;34;0;Create;True;0;0;False;0;1,1,1,1;0.9490196,0.9568627,0.972549,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;237;-5908.969,-2287.124;Float;False;Property;_Active;Active;4;0;Create;True;0;0;False;0;0.3372549,0.8509804,0.9647059,1;0.3372548,0.8509804,0.9647059,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;239;-5908.969,-1935.123;Float;False;Property;_ActivePressed;ActivePressed;6;0;Create;True;0;0;False;0;0.3372549,0.8509804,0.9647059,1;0.3372548,0.8509804,0.9647059,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;236;-5908.969,-2479.124;Float;False;Property;_Pressed;Pressed;3;0;Create;True;0;0;False;0;0.3372549,0.8509804,0.9647059,1;0.9490196,0.9568627,0.972549,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;187;-4190.3,-1131.544;Float;False;1395.16;510.613;;13;213;212;165;166;171;173;174;172;232;175;163;313;314;Color Gradient;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;235;-5908.969,-2655.124;Float;False;Property;_Highlighted;Highlighted;2;0;Create;True;0;0;False;0;0,0.7019608,0.8901961,1;0.9058824,0.9176471,0.9411765,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;298;-5572.969,-2287.124;Float;False;Property;_State2;State 2;10;2;[IntRange];[Enum];Create;True;6;Normal;0;Highlighted;1;Pressed;2;Active;3;ActiveHighlighted;4;ActivePressed;5;0;False;0;1;1;0;7;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;287;-5620.969,-2655.124;Float;False;Property;_State1;State 1;9;2;[IntRange];[Enum];Create;True;6;Normal;0;Highlighted;1;Pressed;2;Active;3;ActiveHighlighted;4;ActivePressed;5;0;False;0;0;0;0;7;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;163;-4172.3,-1058.544;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;290;-5252.969,-2591.124;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;1;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;282;-4412.887,85.31733;Float;False;Property;_AOYOffset;AO Y Offset;28;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;299;-5236.969,-2223.124;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;1;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;276;-4412.887,-10.68288;Float;False;Property;_AOHeight;AO Height;30;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;291;-5252.969,-2415.124;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;4;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;280;-4412.887,-106.683;Float;False;Property;_AOXOffset;AO X Offset;27;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;302;-5236.969,-2047.124;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;4;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;279;-4396.887,-234.6829;Float;False;Property;_AOWidth;AO Width;29;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;338;-5499.845,-1239.316;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;1;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;337;-5515.845,-1431.316;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;4;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;245;-5028.969,-2095.124;Float;False;Property;_StateTween;State Tween;11;0;Create;True;0;0;False;0;1;1;1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;335;-5499.845,-1063.316;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;4;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;336;-5515.845,-1607.316;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;1;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;283;-4236.887,-10.68288;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;281;-4236.887,-122.683;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCCompareWithRange;339;-5292.152,-1602.28;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;2;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;313;-4000.231,-921.0586;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCCompareWithRange;295;-5044.969,-2655.124;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;2;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCCompareWithRange;300;-5028.969,-2271.124;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;2;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;284;-4740.969,-2095.124;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;314;-4092.532,-830.0585;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCCompareWithRange;340;-5276.152,-1218.279;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;2;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;342;-5043.002,-979.7194;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;341;-4867.001,-1027.719;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PosVertexDataNode;156;-4141.393,-290.2598;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;241;-4644.968,-2319.124;Float;False;Property;_Error;Error;7;0;Create;True;0;0;False;0;0.8313726,0.2039216,0.1882353,1;1,1,1,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;175;-3918.3,-779.5433;Float;False;Property;_GradientPosition;Gradient Position;18;0;Create;True;0;0;False;0;0;0.01;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;240;-4660.968,-2495.124;Float;False;Property;_Disabled;Disabled;8;0;Create;True;0;0;False;0;0.9058824,0.9176471,0.9411765,1;0.9058824,0.9176471,0.9411765,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;306;-4580.968,-2607.124;Float;False;Property;_StateFail;State Fail;12;1;[Enum];Create;True;3;None;0;Error;1;Disabled;2;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;275;-4092.887,-122.683;Float;False;FLOAT3;4;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;232;-3906.3,-1067.544;Float;False;Property;_GradientDirection;Gradient Direction;16;0;Create;True;0;0;False;0;0;1;3;True;;KeywordEnum;4;XPositve;XNegative;YPositive;YNegative;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;244;-4564.968,-2143.124;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;343;-4675.098,-1379.117;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;1;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;309;-4676.968,-1983.124;Float;False;Property;_StateFailOverrideTween;State Fail Override Tween;13;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;172;-3630.3,-907.5436;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;174;-3774.3,-699.5435;Float;False;Property;_GradientDistribution;Gradient Distribution;19;0;Create;True;0;0;False;0;0;0.015;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;307;-4324.967,-2447.124;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;1;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;250;-3948.889,-90.68301;Float;False;Constant;_AODiameter;AO Diameter;22;0;Create;True;0;0;False;0;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;278;-3900.89,-186.683;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;357;-4062.363,-1546.56;Float;False;Property;_ColorType;Color Type;0;1;[Enum];Create;True;2;Filled;0;Outlined;1;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;344;-4467.098,-1235.117;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;308;-4116.968,-2303.124;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LengthOpNode;252;-3756.89,-170.683;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;173;-3486.3,-907.5436;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;206;-3756.89,-90.68301;Float;False;2;2;0;FLOAT;-1;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;166;-3422.3,-1083.544;Float;False;Property;_Gradient;Gradient;15;0;Create;True;0;0;False;0;0.8470588,0.972549,0.9568627,1;0.3372549,0.8509804,0.9647059,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;358;-3241.729,-1630.39;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;247;-3612.89,-106.683;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;197;-2649.07,-35.10356;Float;False;925.8951;332.3866;;6;194;191;190;189;195;345;Outlined Colors;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;204;-3756.89,5.316979;Float;False;Property;_AODistribution;AO Distribution;26;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;171;-3326.3,-907.5436;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;213;-3294.3,-795.5435;Float;False;Property;_GradientIntensity;Gradient Intensity;17;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;251;-3484.89,-106.683;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;165;-3150.3,-1067.544;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;190;-2611.349,184.4798;Float;False;Property;_OutlinePosition;Outline Position;14;0;Create;True;0;0;False;0;0.00199;0.002;0;0.01;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;189;-2529.502,34.94289;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;231;-3356.89,-106.683;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;158;-3452.89,-282.6829;Float;False;Property;_AOColor;AO Color;24;0;Create;True;0;0;False;0;0.4745098,0.7411765,0.9490196,1;0.8392157,0.8392157,0.8392157,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;212;-2974.3,-1067.544;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;317;-1588.259,-172.4024;Float;False;1092.929;439.1526;;10;346;347;325;323;324;322;320;321;318;319;Depth;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;191;-2314.121,120.49;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.00165;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;265;-3212.89,-138.683;Float;False;Property;_AOIntensity;AO Intensity;25;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;319;-1476.259,-44.40269;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;345;-2193.413,157.9439;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;318;-1556.259,99.59731;Float;False;Property;_DepthPosition;Depth Position;22;0;Create;True;0;0;False;0;0.002;0.002;0;0.01;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;161;-3100.89,-282.6829;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;264;-2924.889,-282.6829;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;194;-2099.34,90.95805;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;321;-1412.259,179.5974;Float;False;Property;_DepthDistribution;Depth Distribution;23;0;Create;True;0;0;False;0;0.004;0.0001;0;0.01;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;320;-1268.26,83.59731;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.00165;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;322;-1140.26,83.59731;Float;False;2;0;FLOAT;0;False;1;FLOAT;0.0001;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;195;-1891.352,22.01201;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;360;-2061.113,-1352.734;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;323;-1092.26,-92.40273;Float;False;Property;_DepthColor;Depth Color;20;0;Create;True;0;0;False;0;0.3372549,0.8509804,0.9647059,1;0.7019608,0.7450981,0.8,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;324;-996.2601,83.59731;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;217;-480,-176;Float;False;751.1213;414.1694;;4;226;229;223;222;Sheen;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;229;-432,112;Float;False;Property;_SheenPower;Sheen Power;32;0;Create;True;0;0;False;0;0.3;0.3;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;325;-836.26,-92.40273;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;347;-1119.467,186.6187;Float;False;Property;_DepthIntensity;Depth Intensity;21;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;223;-160,-128;Float;False;Property;_SheenColor;Sheen Color;31;0;Create;True;0;0;False;0;0.172549,0.227451,0.3137255,1;0.1725489,0.2274509,0.3137254,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;346;-657.2666,-12.2815;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;226;-144,48;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;222;112,-128;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;286;328.6713,-122.7065;Float;False;True;2;Float;;0;1;Omni/Unlit/Color;0770190933193b94aaa3065e307002fa;0;0;Unlit;2;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;290;0;287;0
WireConnection;290;2;236;0
WireConnection;290;3;235;0
WireConnection;290;4;42;0
WireConnection;299;0;298;0
WireConnection;299;2;236;0
WireConnection;299;3;235;0
WireConnection;299;4;42;0
WireConnection;291;0;287;0
WireConnection;291;2;239;0
WireConnection;291;3;238;0
WireConnection;291;4;237;0
WireConnection;302;0;298;0
WireConnection;302;2;239;0
WireConnection;302;3;238;0
WireConnection;302;4;237;0
WireConnection;338;0;298;0
WireConnection;338;2;328;0
WireConnection;338;3;329;0
WireConnection;338;4;331;0
WireConnection;337;0;287;0
WireConnection;337;2;328;0
WireConnection;337;3;329;0
WireConnection;337;4;331;0
WireConnection;335;0;298;0
WireConnection;335;2;328;0
WireConnection;335;3;329;0
WireConnection;335;4;331;0
WireConnection;336;0;287;0
WireConnection;336;2;328;0
WireConnection;336;3;329;0
WireConnection;336;4;331;0
WireConnection;283;0;276;0
WireConnection;283;1;282;0
WireConnection;281;0;279;0
WireConnection;281;1;280;0
WireConnection;339;0;287;0
WireConnection;339;3;336;0
WireConnection;339;4;337;0
WireConnection;313;0;163;2
WireConnection;295;0;287;0
WireConnection;295;3;290;0
WireConnection;295;4;291;0
WireConnection;300;0;298;0
WireConnection;300;3;299;0
WireConnection;300;4;302;0
WireConnection;284;0;245;0
WireConnection;314;0;163;1
WireConnection;340;0;298;0
WireConnection;340;3;338;0
WireConnection;340;4;335;0
WireConnection;342;0;245;0
WireConnection;341;0;339;0
WireConnection;341;1;340;0
WireConnection;341;2;342;0
WireConnection;275;0;281;0
WireConnection;275;1;283;0
WireConnection;232;1;163;1
WireConnection;232;0;314;0
WireConnection;232;2;163;2
WireConnection;232;3;313;0
WireConnection;244;0;295;0
WireConnection;244;1;300;0
WireConnection;244;2;284;0
WireConnection;343;0;306;0
WireConnection;343;2;240;0
WireConnection;343;3;241;0
WireConnection;343;4;341;0
WireConnection;172;0;232;0
WireConnection;172;1;175;0
WireConnection;307;0;306;0
WireConnection;307;2;240;0
WireConnection;307;3;241;0
WireConnection;307;4;244;0
WireConnection;278;0;156;0
WireConnection;278;1;275;0
WireConnection;344;0;341;0
WireConnection;344;1;343;0
WireConnection;344;2;309;0
WireConnection;308;0;244;0
WireConnection;308;1;307;0
WireConnection;308;2;309;0
WireConnection;252;0;278;0
WireConnection;173;0;172;0
WireConnection;173;1;174;0
WireConnection;206;0;250;0
WireConnection;358;0;357;0
WireConnection;358;2;344;0
WireConnection;358;3;308;0
WireConnection;247;0;252;0
WireConnection;247;1;206;0
WireConnection;171;0;173;0
WireConnection;251;0;247;0
WireConnection;251;1;204;0
WireConnection;165;0;358;0
WireConnection;165;1;166;0
WireConnection;165;2;171;0
WireConnection;231;0;251;0
WireConnection;212;0;358;0
WireConnection;212;1;165;0
WireConnection;212;2;213;0
WireConnection;191;0;189;3
WireConnection;191;1;190;0
WireConnection;345;0;191;0
WireConnection;161;0;212;0
WireConnection;161;1;158;0
WireConnection;161;2;231;0
WireConnection;264;0;212;0
WireConnection;264;1;161;0
WireConnection;264;2;265;0
WireConnection;194;0;345;0
WireConnection;320;0;319;3
WireConnection;320;1;318;0
WireConnection;322;0;320;0
WireConnection;322;1;321;0
WireConnection;195;0;264;0
WireConnection;195;1;308;0
WireConnection;195;2;194;0
WireConnection;360;0;357;0
WireConnection;360;2;195;0
WireConnection;360;3;264;0
WireConnection;324;0;322;0
WireConnection;325;0;360;0
WireConnection;325;1;323;0
WireConnection;325;2;324;0
WireConnection;346;0;360;0
WireConnection;346;1;325;0
WireConnection;346;2;347;0
WireConnection;226;2;229;0
WireConnection;222;0;346;0
WireConnection;222;1;223;0
WireConnection;222;2;226;0
WireConnection;286;0;222;0
ASEEND*/
//CHKSM=CF6FEC3E7F6B24B55564039E0FB5C8D34D579A67