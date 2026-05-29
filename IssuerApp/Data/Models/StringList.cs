using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace IssuerApp.Data.Models
{
    [ModelBinder]
    public class StringList : List<string>
    {
        public StringList() : base()
        {
        }
        public StringList(IEnumerable<string> collection) : base(collection)
        {
        }
    }
}
