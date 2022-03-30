class NewClass : BaseClass, IBaseInterface
{
	NewClass ([string]$test)
	{
		$this.Process = $test
	}
	[string] Test([string]$value)
	{
		$this.Process = $value
		return "Hello"
	}
	static [void] StuffSetter([string]$value)
	{
		$NewClass.stuff = $value
	}
	[string] $Process
	[string] hidden $Process2
	[string] hidden $test
	static [string] $stuff
}