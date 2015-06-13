using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YouTomaton
{
    public static class Shaders
    {
        public const string Prex =
@"
#ifdef GL_FRAGMENT_PRECISION_HIGH
precision highp float;
#else
precision mediump float;
#endif";
        

        public const string VertexShader =
Prex + @"
attribute vec2 aTextureCoord;
attribute vec2 aVertexPosition;

varying vec2 vTextureCoord;

void main(void) {
    gl_Position = vec4(aVertexPosition, 0.0, 1.0);
    vTextureCoord = aTextureCoord;
}";
        public const string FinalVertexShader =
Prex + @"
attribute vec2 aVertexPosition;
attribute vec2 aTextureCoord;
varying vec2 vTextureCoord;
uniform vec2 uContentSize;
uniform vec2 uResolution;
uniform vec2 uPosition;
void main(void) {
    gl_Position = vec4(aVertexPosition, 0.0, 1.0);
    vTextureCoord = aTextureCoord * (uResolution / uContentSize) + uPosition / uResolution;
}";
        public const string PaintShader =
Prex + @"
uniform sampler2D uSampler;
uniform vec2 uResolution;
uniform highp vec2 uPainter;
uniform highp float uRadius;
uniform vec4 uPaintColor;
varying vec2 vTextureCoord;

float ls(vec2 f)
{
    return f.x * f.x + f.y * f.y;
}

void main(void) {

    vec2 pp1 = mod(uPainter, vec2(1.0));
    vec2 pp0 = pp1 + vec2(vTextureCoord.x > 0.5 ? 1 : -1, vTextureCoord.y > 0.5 ? 1 : -1);
    vec2 pp2 = vec2(pp0.x, pp1.y);
    vec2 pp3 = vec2(pp1.x, pp0.y);
    float rs = uRadius * uRadius;
    vec4 val = texture2D(uSampler, vTextureCoord);
    vec2 g = vec2(0.5) / uResolution;
    if(uRadius > g.x * 2.0)
    {
        if(ls(vTextureCoord - pp0) <= rs || ls(vTextureCoord - pp1) <= rs || ls(vTextureCoord - pp2) <= rs || ls(vTextureCoord - pp3) <= rs)
        {
            val = uPaintColor;
        }
    }
    else if(uRadius > 0.0)
    {
            
        if(pp1.x - g.x < vTextureCoord.x && pp1.x + g.x >= vTextureCoord.x && pp1.y - g.y < vTextureCoord.y && pp1.y + g.y >= vTextureCoord.y)
        {
            val = uPaintColor;
        }
    }
    gl_FragColor = val;
}
";
        public const string FragmentSimple =
            @"
varying vec2 vTextureCoord;
uniform sampler2D source;
void main(void) {
    gl_FragColor = texture2D(source, vTextureCoord);
}";
        public const string Frag0 = 
        @"

varying vec2 vTextureCoord;
uniform vec2 uResolution;
uniform sampler2D layer0;
uniform sampler2D layer1;
uniform sampler2D layer2;
uniform sampler2D layer3;
uniform sampler2D layer4;
uniform sampler2D layer5;
uniform sampler2D layer6;
uniform sampler2D layer7;
uniform sampler2D noiseTex;
uniform float uTime;
int CountNeighbours(sampler2D sampler)
{
    int a = 0;
    vec2 step = vec2(1.0) / uResolution.x;
    for(int x = -1; x < 2; x++)
    {
        for(int y = -1; y < 2; y++)
        {
            if(x != 0 || y != 0)
            {
                vec4 n = texture2D(sampler, vTextureCoord + vec2(x,y)*step);
                if(n.r > 0.0)
                    a++;    
            }  
        } 
    }
    return a;
}
void main(void) {

    int n = CountNeighbours(layer0);
    vec4 self = texture2D(layer0, vTextureCoord);
    if(self.r > 0.0)
    {
        if(n < 2 || n > 3)
        {
            self = vec4(0.0);
        }
    }
    else
    {
        if(n == 3)
        {
            self = vec4(1.0);
        }
    }
    gl_FragColor = self;
}";
        public const string Frag1 =
@"

varying vec2 vTextureCoord;
uniform vec2 uResolution;
uniform sampler2D layer0;
uniform sampler2D layer1;
uniform sampler2D layer2;
uniform sampler2D layer3;
uniform sampler2D layer4;
uniform sampler2D layer5;
uniform sampler2D layer6;
uniform sampler2D layer7;
uniform sampler2D noiseTex;
uniform float uTime;
const float s2 = 1.41421356;

void main(void) {

    float a = 1.2;
    float b = 1.0;
    float c = 1.0;
    float f = 0.3;
    a *= f;
    b *= f;
    c *= f;
    vec4 total = vec4(0.0);
    vec2 step = vec2(1.0) / uResolution.x;
    
    for(int x = -1; x < 2; x++)
    {
        for(int y = -1; y < 2; y++)
        {
            total += texture2D(layer1, vTextureCoord + vec2(x,y)*step);   
        } 
    }
    total /= 9.0;
    total += vec4(0.5);
    vec3 n = vec3(total.r + pow(total.r, 3.0) *(a * total.g - c * total.b),
                  total.g + pow(total.g, 3.0) *(b * total.b - a * total.r),
                  total.b + pow(total.b, 3.0) *(c * total.r - b * total.g));
    gl_FragColor = vec4(clamp(n - vec3(0.5), vec3(0.0),vec3(1.0)),1.0);
}";
    }
}