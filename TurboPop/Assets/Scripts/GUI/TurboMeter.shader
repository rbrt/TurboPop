Shader "Custom/TurboMeter" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Percentage ("Percentage", Float) = 0
	}
	SubShader {
		Pass{
			Tags { "RenderType"="Opaque" }
			LOD 200

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float _Percentage;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = v.texcoord;

				return o;
			}


			fixed4 frag (v2f i) : COLOR
			{
				if (i.texcoord.x <= _Percentage){
					return tex2D(_MainTex, i.texcoord);
				}
				else {
					return tex2D(_MainTex, i.texcoord) * .15;
				}

			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
