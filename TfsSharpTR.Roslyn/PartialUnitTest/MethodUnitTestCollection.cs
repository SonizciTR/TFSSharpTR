using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Roslyn.PartialUnitTest
{
    public class MethodUnitTestCollection : List<MethodUnitTestItem>
    {
        public void AddDistinct(Document doc, List<MethodDeclarationSyntax> data)
        {
            Add(doc, data);
        }

        public void Add(Document doc, List<MethodDeclarationSyntax> data)
        {
            var tmp = this.FirstOrDefault(x => x.Doc.Id == doc.Id);

            if(tmp == null)
            {
                tmp.Methods.AddRange(data);
            }
            else
            {
                this.Add(new MethodUnitTestItem(doc, data));
            }
        }
    }

    public class MethodUnitTestItem
    {
        public Document Doc { get; set; }
        public List<MethodDeclarationSyntax> Methods { get; set; }

        public MethodUnitTestItem()
        {
        }

        public MethodUnitTestItem(Document document, List<MethodDeclarationSyntax> methods)
        {
            Doc = document;
            Methods = methods;
        }
    }
}
