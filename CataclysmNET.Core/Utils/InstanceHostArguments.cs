using System.CommandLine;
using System.Diagnostics.CodeAnalysis;

namespace CataclysmNET.Core.Utils;

public class InstanceHostArguments
{
    private static readonly Option<string> InstanceTypeOption = new("--instanceType")
    {
        Required = true
    };

    private static readonly Option<int> Parameter1Option = new("--param1")
    {
        Required = true
    };

    private static readonly Option<int> Parameter2Option = new("--param2")
    {
        Required = true
    };

    private static readonly RootCommand CommandParser = new()
    {
        Options = { InstanceTypeOption, Parameter1Option, Parameter2Option }
    };

    public string InstanceType { get; private set; } = string.Empty;
    public int Parameter1 { get; private set; }
    public int Parameter2 { get; private set; }

    public static bool TryParse(string[] args, [NotNullWhen(true)] out InstanceHostArguments? arguments)
    {
        arguments = null;

        ParseResult parseResult = CommandParser.Parse(args);
        if (parseResult.Errors.Count > 0)
            return false;

        arguments = new InstanceHostArguments()
        {
            InstanceType = parseResult.GetRequiredValue(InstanceTypeOption),
            Parameter1 = parseResult.GetRequiredValue(Parameter1Option),
            Parameter2 = parseResult.GetRequiredValue(Parameter2Option)
        };

        return true;
    }
}
