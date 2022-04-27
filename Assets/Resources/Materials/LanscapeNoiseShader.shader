// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/LandscapeNoiseShader"
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
        _MainTex ("Texture", 2D) = "white" {}
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
            struct appdata{
                fixed4 vertex   : POSITION;
                fixed2 tex      : TEXCOORD0;
            };

            struct v2f{
                fixed4 pos      : SV_POSITION;
                fixed2 tex      : TEXCOORD0;
                fixed4 col : COLOR;
                float noisy : TEXCOORD1;
            };

            Vector _dronePositions[1000];//globally defined
            float _noiseRadii[1000];//globally defined
            int _displayNoise;//globally defined
            fixed4 _ColorA, _ColorB, _ColorC, _ColorD, _ColorE, _ColorF, _ColorG;
            fixed4 _selCol;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.tex = TRANSFORM_TEX(v.tex, _MainTex);
                float3 wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                fixed4 c = _ColorG;
                float dMin = 100000000;
                o.noisy = 0;
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
                                o.noisy = 1;
                            }
                        }
                    }
                }

                o.col = c;

                return o;
            }


            fixed4 frag(v2f i) : COLOR
            {
                fixed4 c;
                if (i.noisy > 0)
                {
                    c = tex2D(_MainTex,i.tex) * 0.5 + i.col * 0.5;
                }
                else
                {
                    c = tex2D(_MainTex,i.tex);
                }
                return c;
            }
            ENDCG
        }
    }
}
