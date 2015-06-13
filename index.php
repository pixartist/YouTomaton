<? 
$title = "Index";
include("session.php");

include("header.php");?>
<div id="toplist"><?
$query = "SELECT p.Id as Id, p.Name, p.Image, u.Nick FROM YTProgram p, YTUser u WHERE p.Author = u.Id";

	// Perform Query
	$result = mysql_query($query);
	if (!$result) 
	{
		echo "ERROR ". mysql_error();
	}
	else
	{
		while($row = mysql_fetch_array($result))
		{
			echo '<div id="thumb"><img src="'.$row["Image"].'" width="100" height="100"><a href="editor.php?program='.$row["Id"].'&n='.$row["Name"].'">'.$row["Name"].'</a> by '.$row["Nick"].'<br> </div>';
		}
	}
?>
</div>

<?include("footer.php");?>