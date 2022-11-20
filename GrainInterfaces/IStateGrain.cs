namespace GrainInterfaces;

public interface IStateGrain : Orleans.IGrainWithIntegerKey
{
    Task<double> Get();

    Task Set(double value);
}