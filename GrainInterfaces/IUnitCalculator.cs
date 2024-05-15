using Orleans.Placement;

namespace GrainInterfaces;

public interface IUnitCalculator : IGrainWithIntegerKey
{
    Task<double> Calculate(int value);
    
    IAsyncEnumerable<string> Powers(int value);
    
    Task<double> Multi(Parms p);
}

[GenerateSerializer]
public record Parms(double X, double Y);