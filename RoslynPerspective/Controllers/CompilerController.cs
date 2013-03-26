using System;
using System.Text;
using System.Web.Http;
using Roslyn.Compilers.CSharp;

namespace RoslynPerspective.Controllers
{
    public class CompilerStatus
    {
        public string Status { get; set; }
    }

    public class CompilerController : ApiController
    {
        public string Post([FromBody] string data)
        {
            if (data == null)
                return "Must send some data";

            try
            {
                var tree = SyntaxTree.ParseText(data);
                var visitor = new Visitor();
                visitor.Visit(tree.GetRoot());
                return visitor.Tree;
            }
            catch (Exception e)
            {
                return string.Format("Exception caught: '{0}'", e);
            }
        }
    }

    internal class Visitor : SyntaxRewriter
    {
        private int _indent;
        private readonly StringBuilder _sb = new StringBuilder();

        public string Tree
        {
            get { return _sb.ToString(); }
        }

        public override SyntaxNode Visit(SyntaxNode node)
        {
            if (node == null)
                return base.Visit(null);

            _sb.AppendLine(new string(' ', _indent * 2) + node.Kind.ToString());
            _indent++;

            var node2 = base.Visit(node);
            _indent--;
            return node2;
        }
    }
}
