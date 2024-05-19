using System.Security.Cryptography.X509Certificates;

namespace SymmetricDifferenceFinder;
public class Program
{

	public static void Main(string[] args)
	{
		Tests.BasicRetrievalTests.TestMassagers();
		Tests.BasicRetrievalTests.TestMassagersConflict();
	}
}
