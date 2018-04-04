using Selenium.Umbraco;

namespace Selenium.Interfaces
{
	public interface IUmbraco
	{
		Sections Sections { get; }

		void Login(string userName, string password);
	}
}