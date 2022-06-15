// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Omni/Unlit/Texture"
{
	Properties
	{
		_Texture("Texture", 2D) = "white" {}
		_OverlayColor("Overlay Color", Color) = (0.07843138,0.1882353,0.2666667,1)
		_OverlayPower("Overlay Power", Range( 0 , 1)) = 0
		_OverlayPosition("Overlay Position", Range( 0 , 1)) = 0
		_OverlayDistribution("Overlay Distribution", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
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
			

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			uniform sampler2D _Texture;
			uniform float4 _Texture_ST;
			uniform float4 _OverlayColor;
			uniform float _OverlayPosition;
			uniform float _OverlayDistribution;
			uniform float _OverlayPower;
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				
				v.vertex.xyz +=  float3(0,0,0) ;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				fixed4 finalColor;
				float2 uv_Texture = i.ase_texcoord.xy * _Texture_ST.xy + _Texture_ST.zw;
				float4 tex2DNode219 = tex2D( _Texture, uv_Texture );
				float2 uv243 = i.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float smoothstepResult270 = smoothstep( _OverlayPosition , ( _OverlayPosition + _OverlayDistribution ) , uv243.y);
				float4 lerpResult242 = lerp( _OverlayColor , tex2DNode219 , saturate( smoothstepResult270 ));
				float4 lerpResult272 = lerp( tex2DNode219 , lerpResult242 , _OverlayPower);
				
				
				finalColor = lerpResult272;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=15800
-90;316;1612;889;1434.408;483.3795;1.6;True;False
Node;AmplifyShaderEditor.RangedFloatNode;226;-568.3766,368.4377;Float;False;Property;_OverlayDistribution;Overlay Distribution;4;0;Create;True;0;0;False;0;0;0.035;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;267;-569.8573,286.0373;Float;False;Property;_OverlayPosition;Overlay Position;3;0;Create;True;0;0;False;0;0;0.035;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;268;-239.3896,358.7838;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;243;-347.149,155.7778;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;270;-90.82445,270.9519;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;273;266.552,267.1224;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;219;118.1124,-65.48505;Float;True;Property;_Texture;Texture;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;241;166.679,-242.4207;Float;False;Property;_OverlayColor;Overlay Color;1;0;Create;True;0;0;False;0;0.07843138,0.1882353,0.2666667,1;0.490196,0.5333333,0.5882353,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;275;139.7195,359.384;Float;False;Property;_OverlayPower;Overlay Power;2;0;Create;True;0;0;False;0;0;0.035;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;242;430.3724,22.815;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;272;629.1016,-0.9277954;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.5;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;276;787.3335,-248.0417;Float;False;True;2;Float;ASEMaterialInspector;0;1;Omni/Unlit/Texture;0770190933193b94aaa3065e307002fa;0;0;Unlit;2;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;268;0;267;0
WireConnection;268;1;226;0
WireConnection;270;0;243;2
WireConnection;270;1;267;0
WireConnection;270;2;268;0
WireConnection;273;0;270;0
WireConnection;242;0;241;0
WireConnection;242;1;219;0
WireConnection;242;2;273;0
WireConnection;272;0;219;0
WireConnection;272;1;242;0
WireConnection;272;2;275;0
WireConnection;276;0;272;0
ASEEND*/
//CHKSM=91D17D6EF85D6F1F135517D4DD45920020132427