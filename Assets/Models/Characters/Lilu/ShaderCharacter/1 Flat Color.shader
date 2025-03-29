// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Shantanu Bhadoria/Basic/1 Flat Color" {
	Properties {
		_Color("Color",Color) = (1.0,1.0,1.0,1.0)
	}
	SubShader{
		Pass {
			CGPROGRAM
			
			// Pragmas
			#pragma vertex vert
			#pragma fragment frag
			
			// User Defined Variables
			uniform float4 _Color;
			
			// Base Input Structs
			struct vertexInput {
				float4 vertex : POSITION;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
			};
			
			// Vertex Function
			vertexOutput vert(vertexInput v) {
				vertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;			
			}
			
			// Fragment Function
			float4 frag(vertexOutput i) : COLOR {
				return _Color;
			}			
			
			ENDCG
		}
	}
	// Fallback
	//Fallback "Diffuse"
}
