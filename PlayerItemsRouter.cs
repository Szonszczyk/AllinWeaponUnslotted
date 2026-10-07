using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace AllinWeaponUnslotted;

// SPT runs all matching routers in order. Run after its normal item-data router.
[Injectable(TypePriority = OnLoadOrder.Routers + 1)]
public class PlayerItemsRouter(JsonUtil jsonUtil, PlayerItemTemplates playerItemTemplates)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/client/items",
                // Send relaxed templates to the client, or keep the original when disabled.
                (url, info, sessionId, output, cancellationToken) =>
                    new ValueTask<string>(playerItemTemplates.Response ?? output ?? string.Empty)
            )
        ]
    ) { }
