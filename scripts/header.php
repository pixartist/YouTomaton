<!DOCTYPE html>
<html>
	<head>
		<link href="style/style.css" rel="stylesheet" type="text/css">
		<link rel="stylesheet" href="style/codemirror.css">
		<!-- assembly references (ordered by dependency) -->
		<script type="text/javascript" src="mscorlib.js"></script>
		<script type="text/javascript" src="lib/toBlob.js"></script>
		<script type="text/javascript" src="lib/jquery-2.1.3.min.js"></script>
		<script type="text/javascript" src="lib/codemirror.js"></script>
		<script type="text/javascript" src="lib/glsl.js"></script>
		<script type="text/javascript" src="lib/glMatrix-0.9.5.min.js"></script>
		<script type="text/javascript" src="WebGLHelper.js"></script>
		<script type="text/javascript" src="YouTomaton.js"></script>
		<script type="text/javascript">
			var loggedIn = <? if(!isset($loggedIn) || !$loggedIn) { echo "false"; } else { echo "true"; } ?>;
			var mirror;
			function Init()
			{
				mirror = CodeMirror.fromTextArea(document.getElementById("fragmentCodeGet"),
									   {
										   lineNumbers: true,
										   matchBrackets: true,
										   indentWithTabs: false,
										   tabSize: 4,
										   indentUnit: 4,
										   mode: "text/x-glsl",
										   foldGutter: true,
										   gutters: ["CodeMirror-linenumbers", "CodeMirror-foldgutter"],
									   });
				//mirror.on("change", function (instance, ev) { document.getElementById("fragmentCodeGet").value = mirror.doc.getValue(); });
				var canvas = document.createElement("canvas");
				var context = canvas.getContext("webgl", { premultipliedAlpha:false, preserveDrawingBuffer:true});
				YouTomaton.Main.Run(mirror, canvas, context);
				<?
				if(isset($_GET['program']))
				{
					echo "YouTomaton.Main.set_CurrentProgram(".readProgram($_GET['program']).");";
				}
				if(isset($_GET['n']))
				{
					?>
					var tb = document.getElementById('programName');
					if(tb != undefined)
					{
						tb.value="<? echo $_GET['n'] ?>";
					}
					<?
				}
				?>

			}
			function saveProgram(name)
			{
				if(!loggedIn)
				{
					alert("You need to log in to save");
				}
				else
				{
					if(name.length < 1)
					{
						alert("Please give your program a name");
					}
					else
					{
						var data = new FormData();
							data.append('name', name);
							data.append('program', YouTomaton.Main.get_CurrentProgram());
							data.append('image', "");
							$.ajax({
								type: "Post",
								url: "save.php",
								data: data,
								processData: false,
								contentType: false,
								success: function (result) {
									if(result != undefined && result.length > 0)
										alert(result);
								}
							});
						
					}
				}
			}
		</script>
		<title><?echo("YouTomaton - " . $title);?></title>
	</head>
	<body <?if(isset($onload)) { echo "onload=" . $onload; }?>>
	<div id="content">
	<div id="header"><div id="actions"><a href="index.php">Home</a><a href="editor.php">Create</a></div>
	<?
	if(isset($loggedIn) && $loggedIn)
	{?>
	<a href="logout.php">Logout</a>
	<?}else{?>
	<div id="login"><form action="<?echo $_SERVER['PHP_SELF'];?>" method="post"><input type="text" name="mail" /><input type="text" name="password" /><input type="submit" value="login" /> <?echo $error;?></form></div>
	<?}?>
	</div>