﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace XmlCleaner.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public class RoslynBinding
        {
            private readonly MainViewModel _vm;

            public XDocument InputXml { get; set; }
            public XDocument OutputXml { get; set; }

            public void Print(object o)
            {
                if (o != null)
                    _vm.Output += o + Environment.NewLine;
            }

            public void RemoveTags(string tagName)
            {
                foreach (var node in InputXml.Descendants(tagName))
                    node.Remove();

                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    InputXml.Save(sw);
                    _vm.OutputText = sb.ToString();
                }
            }

            public void RemoveTags(XName tagName)
            {
                foreach (var node in InputXml.Descendants(tagName).ToList())
                    node.Remove();

                _vm.WriteOutput();
            }

            public RoslynBinding(MainViewModel vm)
            {
                _vm = vm;
            }
        }

        #region Roslyn engine

        private readonly ScriptEngine _engine;
        private readonly Session _session;
        private readonly RoslynBinding _bindings;

        #endregion

        #region Backing fields

        private string _statusText;
        private string _statusBackgroundColor;
        private string _statusForegroundColor;
        private string _code;
        private string _inputText;
        private string _outputText;
        private string _output;
        private ReadOnlyArray<CommonDiagnostic> _buildErrors;

        #endregion

        #region Bindable properties

        public string StatusText
        {
            get { return _statusText; }
            set { SetAndNotify(ref _statusText, value); }
        }

        public string StatusBackgroundColor
        {
            get { return _statusBackgroundColor; }
            set { SetAndNotify(ref _statusBackgroundColor, value); }
        }

        public string StatusForegroundColor
        {
            get { return _statusForegroundColor; }
            set { SetAndNotify(ref _statusForegroundColor, value); }
        }

        public string Code
        {
            get { return _code; }
            set { SetAndNotify(ref _code, value); }
        }

        public string InputText
        {
            get { return _inputText; }
            set { SetAndNotify(ref _inputText, value); }
        }

        public string OutputText
        {
            get { return _outputText; }
            set { SetAndNotify(ref _outputText, value); }
        }

        public string Output
        {
            get { return _output; }
            set { SetAndNotify(ref _output, value); }
        }

        public List<Type> Types { get; private set; }

        #endregion

        public ReadOnlyArray<CommonDiagnostic> BuildErrors
        {
            get { return _buildErrors; }
            set { SetAndNotify(ref _buildErrors, value); }
        }

        public MainViewModel()
        {
            SetStatus("Ready");

            // this can be cached
            _engine = new ScriptEngine();
            _engine.AddReference(Assembly.GetEntryAssembly());
            _engine.AddReference("System");
            _engine.AddReference("System.Core");
            _engine.AddReference("System.Collections");
            _engine.AddReference("System.Linq");
            _engine.AddReference("System.Xml");
            _engine.AddReference("System.Xml.Linq");

            Task.Run(() => LoadSymbols());

            _bindings = new RoslynBinding(this);

            _session = _engine.CreateSession(_bindings);
        }

        private void LoadSymbols()
        {
            var types = new List<Type>();
            foreach (var reference in _engine.GetReferences())
            {
                var assembly = Assembly.LoadFile(reference.Display);
                types.AddRange(assembly.GetTypes().Where(type => type.IsPublic));
            }

            types.Sort((l, r) => l.Name.CompareTo(r.Name));

            Types = types;
        }

        public async Task CompileAndRun(string text)
        {
            try
            {
                BuildErrors = new ReadOnlyArray<CommonDiagnostic>();

                SetStatus("Compiling", "Purple");
                _bindings.InputXml = XDocument.Parse(InputText);

                await Task.Run(() => _session.Execute(text));

                WriteOutput();
                SetStatus("Compiling succedded");
            }
            catch (CompilationErrorException ex)
            {
                SetStatus("Build failed: " + ex.Message, "Red");
                BuildErrors = ex.Diagnostics;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                SetStatus("Build failed", "Red");
            }
        }

        internal async Task CreateSyntaxTree(string text)
        {
            await Task.Run(async () =>
            {
                var compiled = SyntaxTree.ParseText(text);
                var newRoot = new Rewriter().Visit(await compiled.GetRootAsync());
                return CompileAndRun(newRoot.ToString());
            });
        }

        private void WriteOutput()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                _bindings.InputXml.Save(sw);
                OutputText = sb.ToString();
            }
        }

        private void SetStatus(string text, string background = "#007ACC", string foreground = "White")
        {
            StatusText = text;
            StatusBackgroundColor = background;
            StatusForegroundColor = foreground;
        }
    }

    public class Rewriter : SyntaxRewriter
    {
        public override SyntaxNode Visit(SyntaxNode node)
        {
            return base.Visit(node);
        }

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            return base.VisitLocalDeclarationStatement(node);
        }
    }
}
