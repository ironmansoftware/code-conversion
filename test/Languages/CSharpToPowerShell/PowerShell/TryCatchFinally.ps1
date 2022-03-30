function Method
{
	try
	{
		$item = (New-Object -TypeName object)
	}
	catch [Exception]
	{
		$item = (New-Object -TypeName object)
	}
	catch
	{
		$item = (New-Object -TypeName object)
	}
	finally
	{
		$item = (New-Object -TypeName object)
	}
}