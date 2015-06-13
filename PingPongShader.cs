using DuoCode.Dom;
using DuoCode.Runtime;
using WebGLHelper;
using System;
namespace YouTomaton
{
    using GL = WebGLRenderingContext;
    public class PingPongShader
    {
        /// <summary>
        /// uniforms:
        /// 
        /// </summary>
        private int id;
        private int at = 0;
        private Shader shader;
        private Surface[] targets;
        private uint bSize;
        public string lastError;
        public bool enabled;
        public bool paintingEnabled;
        public bool initWithNoise;
        public int repetitions = 1;
        public bool clearEachFrame;
        public Vector3 paintColor;
        public string fragmentCode = "";
        public PingPongShader(uint defaultSize, int id)
        {
            this.paintColor = new Vector3(1, 1, 1);
            this.enabled = false;
            this.paintingEnabled = true;
            this.initWithNoise = false;
            this.clearEachFrame = false;
            this.id = id;
            this.bSize = defaultSize;
            this.targets = new Surface[2];
            this.targets[0] = new Surface(Program.Context, bSize, bSize, GL.FLOAT, GL.NEAREST);
            this.targets[1] = new Surface(Program.Context, bSize, bSize, GL.FLOAT, GL.NEAREST);
        }
        public bool Compile(string prefix, Texture initTex, Shader copyShader, out string message)
        {
            bool success = true;
            message = "Success";
            if (fragmentCode != null)
            {
                var oldShader = shader;
                try
                {
                   
                    shader = Shader.CreateShaderProgramFromStrings(Main.Context, Shaders.VertexShader, prefix + Environment.NewLine + fragmentCode);
                    if(initWithNoise)
                    {
                        targets[0].BindFramebuffer();
                        Main.Context.viewport(0, 0, (int)bSize, (int)bSize);
                        Main.Context.clearColor(0, 0, 0, 0);
                        Main.Context.clear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT);
                        initTex.Bind(Main.Context);
                        Main.DrawQuad(copyShader);
                        targets[1].BindFramebuffer();
                        Main.Context.viewport(0, 0, (int)bSize, (int)bSize);
                        Main.Context.clearColor(0, 0, 0, 0);
                        Main.Context.clear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT);
                        initTex.Bind(Main.Context);
                        Main.DrawQuad(copyShader);
                        Main.Context.bindFramebuffer(GL.FRAMEBUFFER, null);
                    }
                }
                catch(Exception e)
                {
                    shader = oldShader;
                    message = e.Message;
                    success = false;
                }
            }
            else
            {
                shader = null;
                message = "No code provided";
                success = false;
            }
            return success;
        }
        public void Flip(Shader s)
        {
            at = 1 - at;
            s.SetInt("uSampler", 0);
            targets[1 - at].Bind(GL.TEXTURE0);
            targets[at].BindFramebuffer();
            Main.Context.viewport(0, 0, (int)bSize, (int)bSize);
            Main.Context.clearColor(0, 0, 0, 0);
            Main.Context.clear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT);
            Main.DrawQuad(s);
            Main.Context.bindFramebuffer(GL.FRAMEBUFFER, null);
            
        }
        public void Tick(float time)
        {
            if (shader != null && Main.layers != null && repetitions > 0 && enabled)
            {
                Vector2 mp = Main.Screen2Shader(Input.MousePosition, bSize);
                Main.Context.viewport(0, 0, (int)bSize, (int)bSize);
                for (int i = 0; i < repetitions; i++)
                {
                    shader.Use();
                    at = 1 - at;
                    shader.SetInt("layer" + id, id);
                    targets[1 - at].Bind(GL.TEXTURE0 + id);
                    targets[at].BindFramebuffer();

                    int l = 0;
                    for (l = 0; l < Main.MaxLayers; l++)
                    {
                        if (l != id)
                        {
                            if (Main.layers[l] != null && Main.layers[l].enabled)
                            {
                                shader.SetInt("layer" + l, l);
                                Main.layers[l].BindTexture(GL.TEXTURE0 + l);
                            }
                            else
                            {
                                Main.Context.activeTexture(GL.TEXTURE0 + l);
                                Main.Context.bindTexture(GL.TEXTURE_2D, null);
                            }
                        }
                    }
                    shader.SetInt("noise", l);
                    Main.noiseTex.Bind(Main.Context, GL.TEXTURE0 + l);
                    shader.SetFloat("uTime", time);
                    shader.SetFloat("uResolution", bSize, bSize);
                    Main.Context.clearColor(0, 0, 0, 0);

                    Main.Context.clear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT);
                    Main.DrawQuad(shader);
                    Main.Context.bindFramebuffer(GL.FRAMEBUFFER, null);
                }
            }
        }
        public void BindTexture(int target)
        {
            targets[at].Bind(target);
        }
        public void Clear()
        {
            targets[0].Clear();
            targets[1].Clear();
        }
        public void LoadFrom(LayerInfo info)
        {
            repetitions = 1;
            clearEachFrame = false;
            fragmentCode = info.fragmentShader;
            enabled = info.enabled;
            paintColor = info.paintColor;
            paintingEnabled = info.paintingEnabled;
            initWithNoise = info.initWithNoise;
            if(Js.undefined != info.repetitions)
                repetitions = info.repetitions;
            if (Js.undefined != clearEachFrame)
                clearEachFrame = info.clearEachFrame;
        }
    }
}
