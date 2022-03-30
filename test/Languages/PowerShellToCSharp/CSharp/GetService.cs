void Method()
{
	System.ServiceProcess.ServiceController.GetServices.FirstOrDefault(m => m.Name == "myService");
	System.ServiceProcess.ServiceController.GetServices.FirstOrDefault(m => m.Name == "myService");
}