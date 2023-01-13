Shader "Qualcomm/Instanced/InstancedColorSurfaceShader"
{
    Properties
    {
        [MainColor] _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }

    // URP
    SubShader
    {
    	PackageRequirements
        {
        	"com.unity.render-pipelines.universal" : ""
        }
    	
        Tags
        {
            "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" "RenderType" = "Opaque"
        }

        Pass
        {
            Tags
            {
                "LightMode" = "DepthOnly"
            }
        }

        HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		CBUFFER_START(UnityPerMaterial)
		float4 _MainTex_ST;
		CBUFFER_END

		UNITY_INSTANCING_BUFFER_START(Props)
           UNITY_DEFINE_INSTANCED_PROP(half4, _Color)
        UNITY_INSTANCING_BUFFER_END(Props)
		ENDHLSL

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment
            #pragma multi_compile_instancing
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

			struct Attributes {
				float4 positionOS	: POSITION;
				float4 normalOS		: NORMAL;
				float2 uv		    : TEXCOORD0;
				float4 color		: COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings {
				float4 positionCS 					: SV_POSITION;
				float2 uv		    				: TEXCOORD0;
				float3 positionWS					: TEXCOORD1;
				half3 normalWS						: TEXCOORD2;
				float4 color						: COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

			void InitalizeSurfaceData(Varyings IN, out SurfaceData surfaceData){
				surfaceData = (SurfaceData)0;
				half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
				half4 diffuse = baseMap * UNITY_ACCESS_INSTANCED_PROP(Props, _Color) * IN.color;
				surfaceData.albedo = diffuse.rgb;
			}

			void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData) {
				inputData = (InputData)0;
				inputData.positionWS = input.positionWS;
				inputData.normalWS = input.normalWS;
			}

			Varyings LitPassVertex(Attributes IN) {
				Varyings OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
				VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz);
				OUT.positionCS = positionInputs.positionCS;
				OUT.positionWS = positionInputs.positionWS;
				OUT.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
				OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
				OUT.color = IN.color;
				return OUT;
			}

			half4 LitPassFragment(Varyings IN) : SV_Target {
				UNITY_SETUP_INSTANCE_ID(IN);
				SurfaceData surfaceData;
				InitalizeSurfaceData(IN, surfaceData);
				InputData inputData;
				InitializeInputData(IN, surfaceData.normalTS, inputData);
				half4 color = UniversalFragmentBlinnPhong(inputData, surfaceData);
				color.rgb = color.rgb;
				return color;
			}
			ENDHLSL
        }
    }

    //Built-in
    SubShader
    {
        Tags
        {
            "Queue" = "Geometry" "RenderType" = "Opaque"
        }

        CGPROGRAM
        #pragma surface surf Lambert
        #pragma target 3.0
        
        struct Input
        {
            float2 uv_MainTex;
        };
        
        sampler2D _MainTex;

        UNITY_INSTANCING_BUFFER_START(Props)
           UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
        UNITY_INSTANCING_BUFFER_END(Props)
        
        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
}