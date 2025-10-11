using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace WadMaker.Tests.TestHelpers;

/// <summary>
/// Attribute to set StaticFlags properties before a test and reset them after.
/// Usage: [WithStaticFlags(Property1 = value1, Property2 = value2, ...)]
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class WithStaticFlagsAttribute : NUnitAttribute, IWrapSetUpTearDown
{
    public LegacyFlags Flags { get; }

  
    public WithStaticFlagsAttribute(LegacyFlags flags)
    {
        Flags = flags;
    }

    public TestCommand Wrap(TestCommand command)
    {
        return new StaticFlagsCommand(command, this);
    }

    private class StaticFlagsCommand : DelegatingTestCommand
    {
        private readonly WithStaticFlagsAttribute _attribute;
        private bool? _originalExampleFlag;

        public StaticFlagsCommand(TestCommand innerCommand, WithStaticFlagsAttribute attribute)
            : base(innerCommand)
        {
            _attribute = attribute;
        }

        public override TestResult Execute(TestExecutionContext context)
        {         
            Legacy.Flags = _attribute.Flags;    
            try
            {
                return innerCommand.Execute(context);
            }
            finally
            {
                Legacy.Flags = LegacyFlags.None;
            }
        }
    }
}