ConnectionOptions connectionOptions = new ConnectionOptions()
ManagementScope scope = new ManagementScope("\\\\" + "myComputer" + "\\" + "root\\virtualization", connectionOptions);
scope.Connect();
ObjectQuery objectQuery = new ObjectQuery("select " + string.Join(',', new [] { "Description", "Status" }) + " from " + "Msvm_ComputerSystem" + " where " + "name like 'test'");
ObjectQuery query2 = objectQuery;
using (var searcher = new ManagementObjectSearcher(scope, query, options))
{
	searcher.Get();
}