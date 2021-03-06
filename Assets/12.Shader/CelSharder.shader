Shader "Unlit/CelShader"
{
  	Properties
  	{
  		_OutlineColor ("Outline color", Color) = (1,0.5,0,1)
  		_OutlineWidth ("Outlines width", Range (0.0, 5.0)) = 0.1
      _Band_Tex("Band LUT", 2D) = "white" {}
      _Color("Main Tex Color", Color) = (1,1,1,1)
      _MainTex("Main Texture", 2D) = "white" {}
      _BumpMap("NormalMap", 2D) = "bump" {}
      _Outline_Bold("Outline Bold", Range(0, 1)) = 0.1
  	}

  	CGINCLUDE
  	#include "UnityCG.cginc"

  	struct appdata
  	{
  		float4 vertex : POSITION;
  		float3 normal : NORMAL;
  	};

  	struct v2f
  	{
  		float4 pos : POSITION;
  	};

  	uniform float _OutlineWidth;
  	uniform float4 _OutlineColor;

  	ENDCG

  	SubShader
  	{
  		Tags{ "Queue" = "Transparent+1" "IgnoreProjector" = "True" }

  		Pass
  		{
  			ZWrite Off
  			Cull Front

  			CGPROGRAM

  			#pragma vertex vert
  			#pragma fragment frag

  			v2f vert(appdata v)
  			{


  				v2f o;
  				o.pos = UnityObjectToClipPos(v.vertex);

  				float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
  				float2 offset = TransformViewToProjection(norm.xy);
  				o.pos.xy += offset * o.pos.z * _OutlineWidth;

  				return o;

  			}

  			half4 frag(v2f i) : COLOR
  			{
  				return _OutlineColor;
  			}

  			ENDCG
  		}

        cull back
        CGPROGRAM

        #pragma surface surf _BandedLighting

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Band_Tex;
            float2 uv_BumpMap;
        };

        float4 _Outlinecolor;

        struct SurfaceOutputCustom
        {
            fixed3 Albedo;
            fixed3 Normal;
            fixed3 Emission;
            half Specular;
            fixed Gloss;
            fixed Alpha;

            float3 BandLUT;
        };

        sampler2D _MainTex;
        sampler2D _Band_Tex;
        sampler2D _BumpMap;

        float4 _Color;

        void surf(Input IN, inout SurfaceOutputCustom o)
        {
            float4 fMainTex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = fMainTex.rgb;
            o.Alpha = 1.0f;

            float4 fBandLUT = tex2D(_Band_Tex, IN.uv_Band_Tex);
            o.BandLUT = fBandLUT.rgb;

            float3 fNormalTex = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            o.Normal = fNormalTex;

        }


        float4 Lighting_BandedLighting(SurfaceOutputCustom s, float3 lightDir, float3 viewDir, float atten)
        {
            float3 fBandedDiffuse;
            float fNDotL = dot(s.Normal, lightDir) * 0.5f + 0.5f;

            float fBandNum = 3.0f;
            fBandedDiffuse = ceil(fNDotL * fBandNum) / fBandNum;

            fBandedDiffuse = tex2D(_Band_Tex, float2(fNDotL, 0.5f)).rgb;



            float3 fSpecularColor;
            float3 fHalfVector = normalize(lightDir + viewDir);
            float fHDotN = saturate(dot(fHalfVector, s.Normal));
            float fPowedHDotN = pow(fHDotN, 100000000000000000000000000000000000000.0f);
            fSpecularColor = fPowedHDotN * 1.0f;



            //! 최종 컬러 출력
            float4 fFinalColor;
            fFinalColor.rgb = ((s.Albedo * _Color) + fSpecularColor) *
                                 fBandedDiffuse * _LightColor0.rgb * atten;
            fFinalColor.a = s.Alpha;

            return fFinalColor;
        }


        ENDCG
    }
}
