function Method
{
	if (1 -eq 2)
	{
		[int]$variable = 1;
	}
	elseif ("xyz" -eq (New-Object -TypeName Object))
	{
		[int]$variable = 2;
	}
	else
	{
		[int]$variable = 3;
	}
}