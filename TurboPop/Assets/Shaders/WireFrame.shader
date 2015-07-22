Shader "Custom/WireFrame" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("MainTex(RGB)", 2D) = "white" {}
	}
	SubShader {
		Pass{
			Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
			LOD 200
			ZWrite Off
			Cull off
			Blend SrcAlpha OneMinusSrcAlpha

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
			float4 _Color;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = v.texcoord;

				return o;
			}


			fixed4 frag (v2f i) : COLOR
			{
				return tex2D(_MainTex, i.texcoord) * _Color;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
