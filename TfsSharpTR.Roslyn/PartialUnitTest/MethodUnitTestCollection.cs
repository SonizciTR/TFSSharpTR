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

            if (tmp == null)
            {
                this.Add(new MethodUnitTestItem(doc, data));
            }
            else
            {
                tmp.Methods.AddRange(data);
            }
        }

        public int MethodCount
        {
            get
            {
                int cnt = 0;
                foreach (var item in this)
                {
                    cnt += item.Methods.Count;
                }
                return cnt;
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
