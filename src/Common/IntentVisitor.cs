namespace CodeConverter.Common
{
    public class IntentVisitor
    {
		public virtual Node VisitStartProcessIntent(StartProcessIntent intent)
		{
			return intent.Node;
		}

		public virtual Node VisitWriteFileIntent(WriteFileIntent intent)
		{
			return intent.Node;
		}

		public virtual Node VisitWriteHostIntent(WriteHostIntent intent)
		{
			return intent.Node;
		}

		public virtual Node VisitGetServiceIntent(GetServiceIntent intent)
		{
			return intent.Node;
		}

	    public virtual Node VisitGenericIntent(GenericIntent intent)
	    {
		    return intent.Node;
	    }

	}
}
