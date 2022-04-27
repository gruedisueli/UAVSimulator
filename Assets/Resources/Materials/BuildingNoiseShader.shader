// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/BuildingNoiseShader"
{
        Properties
    {
        _ColorA ("Color A", Color) = (1,0,0,1)
        _ColorB ("Color B", Color) = (1,1,0,1)
        _ColorC ("Color C", Color) = (0,0,1,1)
        _ColorD ("Color D", Color) = (0,0,1,1)
        _ColorE ("Color E", Color) = (0,0,1,1)
        _ColorF ("Color F", Color) = (0,0,1,1)
        _ColorG ("Color G", Color) = (1,1,1,1)
        //[NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
         Tags {
             "RenderType" = "Opaque"
        //"RenderType" = "Transparent"
        //"Queue" = "Transparent"
        }
 
        LOD 100
        //Blend SrcAlpha OneMinusSrcAlpha
 
        //ZWrite Off
        //ZTest Always
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : TEXCOORD0;
            };

            Vector _dronePositions[1000];//globally defined
            float _noiseRadii[1000];//globally defined
            int _displayNoise;//globally defined
            float4 _ColorA, _ColorB, _ColorC, _ColorD, _ColorE, _ColorF, _ColorG;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float4 c = _ColorG;
                float dMin = 100000000;
                if (_displayNoise == 1)
                {
                    for(int i = 0; i < 1000; i++)
                    {
                        float r = _noiseRadii[i];
                        Vector p = _dronePositions[i];
                        float dX = p[0] - wPos[0];
                        float dY = p[1] - wPos[1];
                        float dZ = p[2] - wPos[2];
                        float d = sqrt(pow(dX, 2) + pow(dY, 2) + pow(dZ, 2));
                        if (d < dMin)
                        {
                            dMin = d;
                            if (d < r)
                            {
                                //interpolate Color
                                float ratio = sqrt(d/r);
                                if (ratio < 0.16)
                                {
                                    c = _ColorA;
                                }
                                else if (ratio < 0.32)
                                {
                                    c = _ColorB;
                                }
                                else if (ratio < 0.5)
                                {
                                    c = _ColorC;
                                }
                                else if (ratio < 0.66)
                                {
                                    c = _ColorD;
                                }
                                else if (ratio < 0.82)
                                {
                                    c = _ColorE;
                                }
                                else
                                {
                                    c = _ColorF;
                                }
                            }
                        }
                    }
                }

                o.color = c;
                

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
