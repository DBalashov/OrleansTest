// using GrainInterfaces;
// using Microsoft.Extensions.Logging;
// using Orleans;
// using Orleans.Runtime;
//
// namespace Grains;
//
// public class StateGrain : Orleans.Grain, IStateGrain
// {
//     readonly ILogger        logger;
//     readonly IClusterClient client;
//
//     readonly IPersistentState<UCState> state;
//
//     public StateGrain(ILogger<StateGrain> logger, IClusterClient client, [PersistentState("state")] IPersistentState<UCState> state)
//     {
//         this.client = client;
//         this.logger = logger;
//         this.state  = state;
//     }
//
//     public override void Participate(IGrainLifecycle lifecycle)
//     {
//         logger.LogInformation("Initialize: {0}", IdentityString);
//         base.Participate(lifecycle);
//     }
//
//     public override Task OnActivateAsync()
//     {
//         logger.LogInformation("Activate: {0}", IdentityString);
//         return base.OnActivateAsync();
//     }
//
//     public override Task OnDeactivateAsync()
//     {
//         logger.LogInformation("Deactivate: {0}", IdentityString);
//         return base.OnDeactivateAsync();
//     }
//
//     public async Task<string> Get()
//     {
//         return state.State.Value;
//     }
//
//     public async Task Set(string value)
//     {
//         state.State.Value    = value;
//         state.State.Modified = DateTime.UtcNow;
//         await state.WriteStateAsync();
//     }
// }
//
// public class UCState
// {
//     public string   Value    { get; set; }
//     public DateTime Modified { get; set; }
// }