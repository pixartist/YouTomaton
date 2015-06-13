using DuoCode.Dom;
using DuoCode.Runtime;
using WebGLHelper;

namespace YouTomaton
{
    public class LayerInfo
    {
        public string fragmentShader;
        public Vector3 paintColor;
        public bool enabled;
        public bool paintingEnabled;
        public bool initWithNoise;
        public int repetitions;
        public bool clearEachFrame;
        public LayerInfo(string fragmentShader, Vector3 paintColor, bool enabled, bool paintingEnabled, bool initWithNoise, int repetitions, bool clearEachFrame)
        {
            this.fragmentShader = fragmentShader;
            this.paintColor = paintColor;
            this.enabled = enabled;
            this.paintingEnabled = paintingEnabled;
            this.initWithNoise = initWithNoise;
            this.repetitions = repetitions;
            this.clearEachFrame = clearEachFrame;
        }
        public LayerInfo(PingPongShader shader)
        {
            this.fragmentShader = shader.fragmentCode;
            this.paintColor = shader.paintColor;
            this.enabled = shader.enabled;
            this.paintingEnabled = shader.paintingEnabled;
            this.initWithNoise = shader.initWithNoise;
            this.repetitions = shader.repetitions;
            this.clearEachFrame = shader.clearEachFrame;
        }
    }
}