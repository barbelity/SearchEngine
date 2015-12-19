using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Model
{
	class Term
	{


			   public string type { get; set; }
			   public string termString { get; set; }
			   // <docName,positions> example positions="4,7,8,"
			   public Dictionary<string, StringBuilder> d_locations { get; set; }

			   public Term(string type, string termString)
			   {
				   d_locations = new Dictionary<string, StringBuilder>();
				   this.type = type;
				   this.termString = termString;
			   }

			   public override int GetHashCode()
			   {
				   return termString.GetHashCode();
			   }

			   internal void addPosition(string docNme, int p)
			   {
				   if (!d_locations.ContainsKey(docNme))
				   {
					   d_locations[docNme] = new StringBuilder(p + ",");

				   }
				   else
				   {
					   d_locations[docNme].Append(p + ",");
				   }

			   }
			   public override string ToString()
			   {
				   string tfCount = "";
				    
				   StringBuilder tempAns = new StringBuilder();
				   foreach (var pair in d_locations)
				   {
					   tfCount = pair.Value.ToString();
					   tempAns.Append(pair.Key + ";" + tfCount.Count(x => x == ',') + ":");
					   tempAns.Append(pair.Value);
					   tempAns.Append('|');
					   tfCount += pair.Key + ";";
					   
				   }
				   return tempAns.ToString();
			   }
		   }
}