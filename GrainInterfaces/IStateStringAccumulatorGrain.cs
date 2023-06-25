namespace GrainInterfaces;

public interface IStateStringAccumulatorGrain : Orleans.IGrainWithIntegerKey
{
    Task<string> Add(string value);
}