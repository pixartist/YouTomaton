<?
$onload="Init()";
$title = "Index";

include("session.php");
include("header.php");
?>
    <div id="content">
        <div id="canvasContainer"><div id="simControl"><button id="run">Run</button><button id="clear">Clear</button><button id="save" onclick="saveProgram(document.getElementById('programName').value);">Save</button><input type="text" id="programName"></input><button id="import">Import</button></div></div>
        <div id="controls">
            <span class="btn" id="0">Layer 0</span>
            <span class="btn" id="1">Layer 1</span>
            <span class="btn" id="2">Layer 2</span>
            <span class="btn" id="3">Layer 3</span>
            <span class="btn" id="4">Layer 4</span>
            <span class="btn" id="5">Layer 5</span>
            <span class="btn" id="6">Layer 6</span>
            <span class="btn" id="7">Layer 7</span>
            <span class="btn" id="-1">Final</span>
            <div id="code">
                <div><input type="checkbox" id="enabled">Enabled <input type="checkbox" id="initNoise">Init with noise <input type="checkbox" id="painting" checked/>Painting enabled, color: <input type="color" id="colorSelect" /><input type="text" id="repetition" value="1" /> Repetitions  <input type="hidden" id="selectedLayer" value="-2"/> <input type="checkbox" id="clearEachFrame"/>Clear Each Frame</div>
                <div id="fragmentCodeEditor"><input type="hidden" id="fragmentCodeGet" /></div>
                <div><pre id="console"></pre></div>
            </div>
        </div>
    </div>
<?include("footer.php");?>
