function Method
{
	[bool]$eq = 1 -eq 2
	[bool]$notEq = 1 -ne 2
	[bool]$or = 1 -eq 2 -or 2 -eq 1
	[bool]$and = 1 -eq 2 -and 2 -eq 1
	[bool]$gt = 1 -gt 2
	[bool]$lt = 1 -lt 2
	[bool]$ge = 1 -ge 2
	[bool]$le = 1 -le 2
	[int]$plus = 1 + 1
	[int]$minus = 1 - 1
	[int]$bor = 1 -bor 1
}