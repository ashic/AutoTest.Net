﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoTest.TestRunners.Shared;
using AutoTest.TestRunners.Shared.Options;
using AutoTest.TestRunners.Shared.Results;
using AutoTest.TestRunners.Shared.Logging;
using AutoTest.TestRunners.Shared.AssemblyAnalysis;
using AutoTest.TestRunners.Shared.Communication;

namespace AutoTest.TestRunners.NUnit
{
    public class Runner : IAutoTestNetTestRunner
    {
        private ITestFeedbackProvider _channel = null;

        public string Identifier { get { return "NUnit"; } }

        public void SetLogger(ILogger logger)
        {
        }

        public void SetLiveFeedbackChannel(ITestFeedbackProvider channel)
        {
            _channel = channel;
        }

        public bool IsTest(string assembly, string member)
        {
            var locator = new SimpleTypeLocator(assembly);
            var method = locator.LocateMethod(member);
            if (method == null)
                return false;
            return (method.Attributes.Contains("NUnit.Framework.TestAttribute") || method.Attributes.Contains("NUnit.Framework.TestCaseAttribute")) &&
                !method.IsAbstract;
        }

        public bool ContainsTestsFor(string assembly, string member)
        {
            var locator = new SimpleTypeLocator(assembly);
            var cls = locator.LocateClass(member);
            if (cls == null)
                return false;
            return !cls.IsAbstract && cls.Attributes.Contains("NUnit.Framework.TestFixtureAttribute");
        }

        public bool ContainsTestsFor(string assembly)
        {
            var parser = new AssemblyReader();
            return parser.GetReferences(assembly).Contains("nunit.framework");
        }

        public bool Handles(string identifier)
        {
            return identifier.ToLower().Equals(Identifier.ToLower());
        }

        public IEnumerable<TestResult> Run(RunSettings settings)
        {
            var runner = new NUnitRunner(_channel);
            runner.Initialize();
            var parser = new NUnitOptionsParser(settings);
            parser.Parse();
            var results = new List<TestResult>();
            foreach (var option in parser.Options)
                results.AddRange(runner.Execute(option));
            return results;
        }
    }
}
