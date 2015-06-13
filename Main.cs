using DuoCode.Dom;
using DuoCode.Runtime;
using WebGLHelper;
using System;
using System.Collections.Generic;
namespace YouTomaton
{
    
    using GL = WebGLRenderingContext;
    public struct LayerInfo
    {
        public readonly string fragmentShader;
        public readonly Vector3 paintColor;
        public readonly bool enabled;
        public readonly bool paintingEnabled;
        public readonly bool initWithNoise;
        public readonly int repetitions;
        public readonly bool clearEachFrame;
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
    public struct ProgramInfo
    {
        public readonly LayerInfo layer0;
        public readonly LayerInfo layer1;
        public readonly LayerInfo layer2;
        public readonly LayerInfo layer3;
        public readonly LayerInfo layer4;
        public readonly LayerInfo layer5;
        public readonly LayerInfo layer6;
        public readonly LayerInfo layer7;
        public readonly string finalShader;
        public ProgramInfo(PingPongShader[] layers, string finalShader)
        {
            layer0 = new LayerInfo(layers[0]);
            layer1 = new LayerInfo(layers[1]);
            layer2 = new LayerInfo(layers[2]);
            layer3 = new LayerInfo(layers[3]);
            layer4 = new LayerInfo(layers[4]);
            layer5 = new LayerInfo(layers[5]);
            layer6 = new LayerInfo(layers[6]);
            layer7 = new LayerInfo(layers[7]);
            this.finalShader = finalShader;
        }
    }
    class Main : Program
    {
        public static int MaxLayers = 8;
        public static PingPongShader[] layers;
        public static Texture noiseTex;
        public static string finalShaderFragment;
        public static string lastError;
        public static float zoom;
        public static Vector2 position;
        public static bool paused;
        public static Uint8Array CurrentImage
        {
            get
            {
                Uint8Array buf = new Uint8Array(Width * Height * 4);
                Context.readPixels(0, 0, Width, Height, GL.RGBA, GL.UNSIGNED_BYTE, buf);
                return buf;
            }
        }
        public static dynamic CurrentProgram
        {
            get
            {
                instance.Compile(null);
                return Global.JSON.stringify(new ProgramInfo(layers, finalShaderFragment));
            }
            set
            {

                ProgramInfo i = value;
                layers[0].LoadFrom(i.layer0);
                layers[1].LoadFrom(i.layer1);
                layers[2].LoadFrom(i.layer2);
                layers[3].LoadFrom(i.layer3);
                layers[4].LoadFrom(i.layer4);
                layers[5].LoadFrom(i.layer5);
                layers[6].LoadFrom(i.layer6);
                layers[7].LoadFrom(i.layer7);
                
                instance.SelectedLayer = -2;
                instance.SelectLayer(-1);
                instance.FragmentCode = i.finalShader;
                instance.Compile(null);
            }
        }
        private static Main instance;
        private float time;
        private JsObject editor;
        private Shader finalShader, paintShader, copyShader;
        private const int defaultSize = 1024;
        private readonly float[] quad = new float[] { -1, -1, 1, -1, -1, 1, -1, 1, 1, -1, 1, 1 };
        private readonly float[] texCoords = new float[] { 0, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 1 };
        private GLBufferF vBuffer, tBuffer;
        private Dictionary<string, HTMLElement> elements;
        private bool canRender = false;
        private float brushRadius = 0.02f;
        private bool canPaint;
        private int colorMode;
        private string FragmentCode
        {
            get
            {
                return editor.member("getValue").invoke();
            }
            set
            {
                editor.member("setValue").invoke(value);
                
            }
        }
        private HTMLPreElement ErrorConsole
        {
            get
            {
                return Id<HTMLPreElement>("console");
            }
        }
        private HTMLInputElement ColorSelector
        {
            get
            {
                return Id<HTMLInputElement>("colorSelect");
            }
        }
        private HTMLInputElement EnabledCheckbox
        {
            get
            {
                return Id<HTMLInputElement>("enabled");
            }
        }
        private HTMLInputElement PaintingEnabledCheckbox
        {
            get
            {
                return Id<HTMLInputElement>("painting");
            }
        }
        private HTMLInputElement ClearEachFrameCheckbox
        {
            get
            {
                return Id<HTMLInputElement>("clearEachFrame");
            }
        }
        private HTMLInputElement RepetitionsTextbox
        {
            get
            {
                return Id<HTMLInputElement>("repetition");
            }
        }
        private HTMLInputElement InitWithNoiseCheckbox
        {
            get
            {
                return Id<HTMLInputElement>("initNoise");
            }
        }
        private int SelectedLayer
        {
            get
            {
                return int.Parse(Id<HTMLInputElement>("selectedLayer").value);
            }
            set
            {
                Id<HTMLInputElement>("selectedLayer").value = value.ToString();
            }
        }
        private float ShaderMX
        {
            get
            {
                return (Input.MousePosition.x) / (defaultSize * zoom) + position.x / (float)Global.window.innerWidth;
            }
        }
        private float ShaderMY
        {
            get
            {
                return (Height - Input.MousePosition.y) / (defaultSize * zoom) + position.y / (float)Global.window.innerHeight;
            }
        }
        public Main() : base()
        {
            elements = new Dictionary<string, HTMLElement>();
            instance = this;
        }
        public static void Run(JsObject editor,HTMLCanvasElement canvas, WebGLRenderingContext context) // HTML body.onload event entry point, see index.html
        {
            var m = new Main();
            m.editor = editor;
            canvas.id = "canvas";
            m.Id("canvasContainer").appendChild(canvas);
            Run(canvas, context, m);

        }
        public override void OnCreate()
        {
            Global.window.onresize = Resize;
            noiseTex = new Texture(Context, "noise.png");
            var ext = Context.getExtension("OES_texture_float");
            canPaint = false;
            zoom = 1;
            position = new Vector2(0, 0);
            paused = false;
            layers = new PingPongShader[MaxLayers];
            for (int i = 0; i < MaxLayers; i++)
            {
                layers[i] = new PingPongShader(defaultSize, i);
            }
            layers[0].fragmentCode = Shaders.Frag0;
            layers[1].fragmentCode = Shaders.Frag1;
            layers[0].enabled = true;
            paintShader = Shader.CreateShaderProgramFromStrings(Context, Shaders.VertexShader, Shaders.PaintShader);
            copyShader = Shader.CreateShaderProgramFromStrings(Context, Shaders.FinalVertexShader, Shaders.PaintShader);
            finalShaderFragment = @"
uniform sampler2D layer0;
uniform sampler2D layer1;
uniform sampler2D layer2;
uniform sampler2D layer3;
uniform sampler2D layer4;
uniform sampler2D layer5;
uniform sampler2D layer6;
uniform sampler2D layer7;
varying vec2 vTextureCoord;
void main(void) {
    gl_FragColor = texture2D(layer0, vTextureCoord);
}";
            SetCanvasSize(defaultSize, defaultSize);
            vBuffer = new GLBufferF();
            vBuffer.components = 2;
            vBuffer.SetData(quad);
            vBuffer.UpdateBuffer();

            tBuffer = new GLBufferF();
            tBuffer.components = 2;
            tBuffer.SetData(texCoords);
            tBuffer.UpdateBuffer();
            SetupControls();
            Context.enable(GL.BLEND);
            Context.blendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
            Context.disable(GL.DEPTH_TEST);
            Resize(null);
        }
        private void SetupControls()
        {
            for(int i = -1; i < MaxLayers; i++)
            {
                Id(i.ToString()).onclick = SelectLayer;
            }
            

            ColorSelector.onchange = ColorChanged;
            //ColorSelector.value = "#ffffff";

            PaintingEnabledCheckbox.onclick = TogglePaintingEnabled;
            ClearEachFrameCheckbox.onclick = ToggleClearEachFrame;
            InitWithNoiseCheckbox.onclick = ToggleInitWithNoise;
            RepetitionsTextbox.onchange = ChangeRepetitions;
           // PaintingEnabledCheckbox.click();

            EnabledCheckbox.onclick = ToggleLayer;
            //EnabledCheckbox.click();
            
            Id<HTMLButtonElement>("run").onclick = Compile;
            Click("clear",
            () =>
            {
                for(int i = 0; i < MaxLayers; i++)
                {
                    layers[i].Clear();
                }
                
            });
            Canvas.onmouseenter = (e) => { canPaint = true; return true; };
            Canvas.onmouseleave = (e) => { canPaint = false; return true; };
            Canvas.onselectstart = (e) => { return false; };
            Canvas.onmousedown = (e) => { return false; };
            Id("import").onclick = Import;
            Id("0").click();
            Id("run").click();
        }
        private dynamic Resize(Event e)
        {
            int wi = (int)(Global.window.innerWidth * 0.6);
            int hi = (int)(Global.window.innerHeight);
            string w = wi + "px";
            Id("canvasContainer").style.setProperty("width", w);
            Id("controls").style.setProperty("margin-left", w);
            Class("CodeMirror cm-s-default").style.setProperty("height", (hi / 2) + "px");
            Id("console").style.setProperty("height", (int)(hi*0.25)+ "px");
            SetCanvasSize(wi-60, hi-160);
            return true;
        }
        private dynamic Import(MouseEvent e)
        {
            Id<HTMLInputElement>("selectedLayer").value = "-2";
            string json = Global.window.prompt("Paste Json", "");
            System.Console.WriteLine("parsing: " + json);
            ProgramInfo inf = Global.JSON.parse(json);
            layers[0].LoadFrom(inf.layer0);
            layers[1].LoadFrom(inf.layer1);
            layers[2].LoadFrom(inf.layer2);
            layers[3].LoadFrom(inf.layer3);
            layers[4].LoadFrom(inf.layer4);
            layers[5].LoadFrom(inf.layer5);
            layers[6].LoadFrom(inf.layer6);
            layers[7].LoadFrom(inf.layer7);
            FragmentCode = inf.finalShader;
            Id("0").click();
            Id("run").click();
            return true;
        }
        private dynamic Compile(MouseEvent e)
        {
            if (SelectedLayer >= 0 && layers[SelectedLayer] != null)
            {
                layers[SelectedLayer].fragmentCode = FragmentCode;
            }
            else if (SelectedLayer == -1)
            {
                finalShaderFragment = FragmentCode;
                //set final shader
            }
            
            var oldFinal = finalShader;
            try
            {
                
                
                for (int i = 0; i < MaxLayers; i++)
                {
                    if(layers[i].enabled)
                    {
                        layers[i].lastError = "Compiled successfully";
                        string msg;
                        if(!layers[i].Compile(Shaders.Prex, noiseTex, copyShader, out msg))
                        {
                            layers[i].lastError = msg;
                            
                            Id(i.ToString()).style.backgroundColor = "red";
                            System.Console.WriteLine("Failed to compile layer " + i + ": " + msg);
                        }
                        if (i == SelectedLayer)
                        {
                            ErrorConsole.textContent = layers[i].lastError;
                        }
                    }
                }
                lastError = "Compiled successfully";
                // System.Console.WriteLine("Compiling: vert: " + VertexShader + Environment.NewLine + "frag: " + Prex + finalShaderFragment);
                finalShader = Shader.CreateShaderProgramFromStrings(Context, Shaders.FinalVertexShader, Shaders.Prex + finalShaderFragment);
                canRender = true;
                
            }
            catch(Exception ex)
            {
                lastError = ex.Message;
                Id("-1").style.backgroundColor = "red";
                
                System.Console.WriteLine("Failed to compile final shader: " + ex.Message);
                finalShader = oldFinal;
            }
            if (-1 == SelectedLayer)
            {
                ErrorConsole.textContent = lastError;
            }
            time = 0;
            return true;
        }
        
        private void SetColor(int layer)
        {
            float r = Js.parseInt(ColorSelector.value.Substring(1, 2), 16) / 255f;
            float g = Js.parseInt(ColorSelector.value.Substring(3, 2), 16) / 255f;
            float b = Js.parseInt(ColorSelector.value.Substring(5, 2), 16) / 255f;
            layers[layer].paintColor = new Vector3(r, g, b);
        }
        private string GetColor(int layer)
        {
            return "#" + ((int)(layers[layer].paintColor.x * 255)).ToString("X2") + ((int)(layers[layer].paintColor.y * 255)).ToString("X2") + ((int)(layers[layer].paintColor.z * 255)).ToString("X2");
        }
        private dynamic ColorChanged(Event e)
        {

            SetColor(SelectedLayer);
            return true;
        }
        private dynamic ToggleInitWithNoise(MouseEvent e)
        {
            if (SelectedLayer == -1)
            {
                InitWithNoiseCheckbox.checked_ = true;
            }
            else if (SelectedLayer >= 0)
            {
                layers[SelectedLayer].initWithNoise = InitWithNoiseCheckbox.checked_;
                //FragmentCode.disabled = !layers[id].enabled;
            }
            return true;
        }
        private dynamic TogglePaintingEnabled(MouseEvent e)
        {
            if (SelectedLayer == -1)
            {
                PaintingEnabledCheckbox.checked_ = true;
            }
            else if (SelectedLayer >= 0)
            {
                layers[SelectedLayer].paintingEnabled = PaintingEnabledCheckbox.checked_;
                //FragmentCode.disabled = !layers[id].enabled;
            }
            return true;
        }
        private dynamic ToggleClearEachFrame(MouseEvent e)
        {
            if (SelectedLayer == -1)
            {
                ClearEachFrameCheckbox.checked_ = true;
            }
            else if (SelectedLayer >= 0)
            {
                layers[SelectedLayer].clearEachFrame = ClearEachFrameCheckbox.checked_;
                //FragmentCode.disabled = !layers[id].enabled;
            }
            return true;
        }
        private dynamic ChangeRepetitions(Event e)
        {
            if (SelectedLayer == -1)
            {
                RepetitionsTextbox.value = "1";
            }
            else if(SelectedLayer >= 0)
            {
                SetRepetitions(SelectedLayer, RepetitionsTextbox.value);
            }
            return true;
        }
        private void SetRepetitions(int layer, string s)
        {
            int rep ;
            if(int.TryParse(s, out rep))
            {
                layers[layer].repetitions = rep;
            }
        }
        private dynamic ToggleLayer(MouseEvent e)
        {

            if (SelectedLayer == -1)
            {
                EnabledCheckbox.checked_ = true;
            }
            else if(SelectedLayer >= 0)
            {
                layers[SelectedLayer].enabled = EnabledCheckbox.checked_;
                //FragmentCode.disabled = !layers[id].enabled;
            }
            return true;
        }
        private dynamic SelectLayer(int id)
        {
            if (SelectedLayer >= -1)
            {
                if (SelectedLayer >= 0)
                {
                    layers[SelectedLayer].fragmentCode = FragmentCode;
                    layers[SelectedLayer].paintingEnabled = PaintingEnabledCheckbox.checked_;
                    layers[SelectedLayer].clearEachFrame = ClearEachFrameCheckbox.checked_;
                    layers[SelectedLayer].initWithNoise = InitWithNoiseCheckbox.checked_;
                    SetRepetitions(SelectedLayer, RepetitionsTextbox.value);
                    SetColor(SelectedLayer);
                }
                else
                {
                    finalShaderFragment = FragmentCode;

                    //set final shader
                }
                Id(SelectedLayer.ToString()).style.backgroundColor = "#303030";
            }
            SelectedLayer = id;

            Id(id.ToString()).style.backgroundColor = "#505050";
            if (SelectedLayer >= 0)
            {
                EnabledCheckbox.checked_ = layers[SelectedLayer].enabled;
                PaintingEnabledCheckbox.checked_ = layers[SelectedLayer].paintingEnabled;
                ClearEachFrameCheckbox.checked_ = layers[SelectedLayer].clearEachFrame;
                RepetitionsTextbox.value = layers[SelectedLayer].repetitions.ToString();
                InitWithNoiseCheckbox.checked_ = layers[SelectedLayer].initWithNoise;
                FragmentCode = layers[SelectedLayer].fragmentCode;
                ColorSelector.value = GetColor(SelectedLayer);
                ErrorConsole.textContent = layers[SelectedLayer].lastError;
                //FragmentCode.disabled = !layers[id].enabled;
            }
            else if (SelectedLayer == -1)
            {
                EnabledCheckbox.checked_ = true;
                //FragmentCode.disabled = false;
                PaintingEnabledCheckbox.checked_ = false;
                ClearEachFrameCheckbox.checked_ = false;
                InitWithNoiseCheckbox.checked_ = false;
                RepetitionsTextbox.value = "1";
                FragmentCode = finalShaderFragment;
                ErrorConsole.textContent = lastError;
                //load final shader
            }

            return true;
        }
        private dynamic SelectLayer(MouseEvent e)
        {

            SelectLayer(int.Parse(((HTMLSpanElement)e.srcElement).id));
            return true;
        }
        public override void OnRender(double t, float dt)
        {
            time += dt;
            if(canRender)
            {
                for(int i = 0; i < MaxLayers; i++)
                {
                    layers[i].Tick(time);
                    if (layers[i].paintingEnabled && canPaint && Input.GetButtonState(MouseButton.Left) && !Input.IsKeyPressed(KeyCode.Shift))
                    {
                        paintShader.Use();
                        
                        paintShader.SetFloat("uResolution", (int)(Canvas.width), (int)(Canvas.height));
                        paintShader.SetFloat("uPainter", ShaderMX, ShaderMY);
                        paintShader.SetFloat("uRadius", brushRadius);
                        paintShader.SetFloat("uPaintColor", Input.IsKeyPressed(KeyCode.Ctrl) ? new float[] { 0, 0, 0, 0 } : new float[] { layers[i].paintColor.x, layers[i].paintColor.y, layers[i].paintColor.z, 1 });
                        layers[i].Flip(paintShader);
                    }
                    
                }
                
                Context.viewport(0, 0, (int)(Canvas.width), (int)(Canvas.height));
                finalShader.Use();
                Context.bindFramebuffer(GL.FRAMEBUFFER, null);
                Context.clearColor(0,0,0,0);
                Context.clear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT);
                
                
                for (int l = 0; l < Main.MaxLayers; l++)
                {
                    if (layers[l] != null && layers[l].enabled)
                    {
                        finalShader.SetInt("layer" + l, l);
                        layers[l].BindTexture(GL.TEXTURE0 + l);
                    }
                    else
                    {
                        Context.activeTexture(GL.TEXTURE0 + l);
                        Context.bindTexture(GL.TEXTURE_2D, null);
                    }

                }
                finalShader.SetFloat("uResolution", (int)(Canvas.width), (int)(Canvas.height));
                finalShader.SetFloat("uContentSize", defaultSize * zoom, defaultSize * zoom);
                finalShader.SetFloat("uPosition", position.x, position.y);
                DrawQuad(finalShader);
                for (int i = 0; i < MaxLayers; i++)
                {
                    if (layers[i].clearEachFrame)
                    {
                        layers[i].Clear();
                    }
                }
            }
        }
        public static void DrawQuad(Shader s)
        {
            s.SetBuffer("aVertexPosition", ((Main)Instance).vBuffer);
            s.SetBuffer("aTextureCoord", ((Main)Instance).tBuffer);
            Context.drawArrays(GL.TRIANGLES, 0, 6);
        }
        private NodeList ClassElements(string name)
        {
            return Global.document.getElementsByClassName(name);
        }
        private HTMLElement Class(string name)
        {
            var els = ClassElements(name);
            if(els.length < 1)
                return null;
            return els.item(0).As<HTMLElement>();
        }
        private T Id<T>(string name) where T : HTMLElement
        {

            HTMLElement element;
            if(!elements.TryGetValue(name, out element))
            {
                 element = (T)Global.document.getElementById(name);
                 elements.Add(name, element);
            }
            return (T)element;
        }
        private HTMLElement Id(string name)
        {
            return Id<HTMLElement>(name);
        }
        private T Tag<T>(string name) where T : HTMLElement
        {

            return (T)Global.document.getElementsByTagName(name)[0];
        }
        private T Tag<T>(HTMLElement parent, string name) where T : HTMLElement
        {
            return (T)parent.getElementsByTagName(name)[0];
        }
        private void Click(string id, Action action)
        {
            Id(id).onclick = (x) => { action(); return true; };
        }
        private void Click(HTMLElement element, Action action)
        {
            element.onclick = (x) => { action(); return true; };
        }
        public static Vector2 Screen2Shader(Vector2 screen, uint bSize)
        {
            return new Vector2((Input.MousePosition.x) / (bSize * zoom) + position.x / Width, (Height - Input.MousePosition.y) / (bSize * zoom) + position.y / Height);
        }
        public static T Make<T>(HTMLElement parent, string tag) where T:HTMLElement
        {
            T element = Global.document.createElement(tag);
            parent.appendChild(element);
            return element;
        }
        public static HTMLElement Make(HTMLElement parent, string tag)
        {
            HTMLElement element = Global.document.createElement(tag);
            parent.appendChild(element);
            return element;
        }

    }
}
