using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Selenium.Umbraco
{
	public interface ISections
	{
		ISectionItem Section(string name);
		ISectionItem Section(Sections.SectionEnum section);
	}

	public class Sections : List<SectionItem>, ISections
	{
		public enum SectionEnum
		{
			Content,
			Media,
			Settings,
			Developer,
			Users,
			Members,
			Forms,
			Translation
		}

		public ISectionItem Section(string name)
		{
			return Find(item => string.Equals(item.Title, name, StringComparison.InvariantCultureIgnoreCase));
		}

		public ISectionItem Section(SectionEnum section)
		{
			return Find(item => string.Equals(item.Title, section.ToString(), StringComparison.InvariantCultureIgnoreCase));
		}
	}
}