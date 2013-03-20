using System.Linq;
using System.Collections.Generic;
using Roslyn.Compilers.CSharp;

namespace SyntaxTreeVisualizer
{
    public class EnumerableSyntaxNode : IEnumerable<EnumerableSyntaxNode>
    {
        private readonly SyntaxNode _syntaxNode;
        private List<EnumerableSyntaxNode> _children;

        public SyntaxNode SyntaxNode { get { return _syntaxNode; } }
        public string Text { get { return _syntaxNode.ToString(); } }

        public List<EnumerableSyntaxNode> Children
        {
            get
            {
                if (_children == null)
                    _children = _syntaxNode.ChildNodes().Select(n => new EnumerableSyntaxNode(n)).ToList();

                return _children;
            }
        }


        public EnumerableSyntaxNode(SyntaxNode syntaxNode)
        {
            _syntaxNode = syntaxNode;
        }

        public IEnumerator<EnumerableSyntaxNode> GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Children.GetEnumerator();
        }
    }
}
