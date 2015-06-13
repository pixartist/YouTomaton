<?

session_start();



$servername = "localhost";
$username = "db11030402-yt";
$password = "Krawall86YT";

// Create connection
$conn = mysql_connect($servername, $username, $password);

// Check connection
mysql_select_db("db11030402-1", $conn);
$user = -1;
$error = null;
if (!empty($_POST) && isset($_POST['password']) && strlen($_POST['password']) > 0)
{
	$mail = empty($_POST['mail']) ? null : mysql_real_escape_string($_POST['mail']);
	$password = empty($_POST['password']) ? null : sha1($_POST['password']);

	$query = sprintf("SELECT Id,Pw FROM YTUser WHERE Mail='%s'", mysql_real_escape_string($mail));

	// Perform Query
	$result = mysql_query($query);
	if (!$result) 
	{
		$error = "Query failed";
	}
	else
	{
		if(mysql_num_rows($result) < 1)
		{
			$error = "User not found";
		}
		else
		{
			$row = mysql_fetch_assoc($result);
			if ($password == $row['Pw']) 
			{
				$_SESSION['authenticated'] = true;
				$_SESSION['user'] = $row['Id'];
			}
			else 
			{
				$error = 'Incorrect password'.$password;
			}
		}
	}
}
$loggedIn = isset($_SESSION['authenticated']) && isset($_SESSION['user']) && $_SESSION['authenticated'] == true;
if($loggedIn)
{
	$user = $_SESSION['user'];
}
function readProgram($id)
{
	$query = sprintf("SELECT JSON from YTProgram WHERE Id='%s'", mysql_real_escape_string($id));
	$result = mysql_query($query);
	if (!$result) 
	{
		$error = "Query failed";
	}
	else
	{
		if(mysql_num_rows($result) < 1)
		{
			$error = "Program not found";
		}
		else
		{
			$row = mysql_fetch_assoc($result);
			return $row['JSON'];
		}
	}
}
function writeProgram($name, $data, $image)
{
	
	global $loggedIn;
	global $user;
	$error = "";
	if($loggedIn && $user >= 0)
	{
		$query = sprintf("SELECT Id FROM YTProgram WHERE Author='%s' AND Name='%s'", mysql_real_escape_string($user), mysql_real_escape_string($name));
		$result = mysql_query($query);
		if(mysql_num_rows($result) > 0)
		{
			$row = mysql_fetch_assoc($result);
			$query = sprintf("UPDATE YTProgram SET JSON='%s', Image='%s' WHERE Id='%s'", mysql_real_escape_string($data), mysql_real_escape_string($image),  $row['Id']);
			$result = mysql_query($query);
			if (!$result) 
			{
				$error = "Update query failed";
			}
		}
		else
		{
			$query = sprintf("INSERT INTO YTProgram (Name, JSON, Author, Image) VALUES ('%s', '%s', '%s', '%s')", mysql_real_escape_string($name), mysql_real_escape_string($data), mysql_real_escape_string($user), mysql_real_escape_string($image));
			$result = mysql_query($query);
			if (!$result) 
			{
				$error = "Save query failed";
			}
		}
	}
	else
	{
		$error = "Login to save";
	}
	if(isset($error) && strlen($error) > 0)
		echo $error.": ". mysql_error();
}
?>