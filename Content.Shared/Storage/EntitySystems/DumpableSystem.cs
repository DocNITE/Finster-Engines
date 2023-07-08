using Content.Shared.Disposal;
using Content.Shared.Disposal.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Placeable;
using Content.Shared.Storage.Components;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared.Storage.EntitySystems;

public sealed class DumpableSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDisposalUnitSystem _disposalUnitSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();
        _xformQuery = GetEntityQuery<TransformComponent>();
        SubscribeLocalEvent<DumpableComponent, AfterInteractEvent>(OnAfterInteract, after: new[]{ typeof(SharedEntityStorageSystem) });
        SubscribeLocalEvent<DumpableComponent, GetVerbsEvent<AlternativeVerb>>(AddDumpVerb);
        SubscribeLocalEvent<DumpableComponent, GetVerbsEvent<UtilityVerb>>(AddUtilityVerbs);
        SubscribeLocalEvent<DumpableComponent, DumpableDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(EntityUid uid, DumpableComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Handled)
            return;

        if (!HasComp<SharedDisposalUnitComponent>(args.Target) && !HasComp<PlaceableSurfaceComponent>(args.Target))
            return;

        StartDoAfter(uid, args.Target.Value, args.User, component);
        args.Handled = true;
    }

    private void AddDumpVerb(EntityUid uid, DumpableComponent dumpable, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!TryComp<SharedStorageComponent>(uid, out var storage) || storage.StoredEntities == null || storage.StoredEntities.Count == 0)
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                StartDoAfter(uid, args.Target, args.User, dumpable);//Had multiplier of 0.6f
            },
            Text = Loc.GetString("dump-verb-name"),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/drop.svg.192dpi.png")),
        };
        args.Verbs.Add(verb);
    }

    private void AddUtilityVerbs(EntityUid uid, DumpableComponent dumpable, GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!TryComp<SharedStorageComponent>(uid, out var storage) || storage.StoredEntities == null || storage.StoredEntities.Count == 0)
            return;

        if (HasComp<SharedDisposalUnitComponent>(args.Target))
        {
            UtilityVerb verb = new()
            {
                Act = () =>
                {
                    StartDoAfter(uid, args.Target, args.User, dumpable);
                },
                Text = Loc.GetString("dump-disposal-verb-name", ("unit", args.Target)),
                IconEntity = uid
            };
            args.Verbs.Add(verb);
        }

        if (HasComp<PlaceableSurfaceComponent>(args.Target))
        {
            UtilityVerb verb = new()
            {
                Act = () =>
                {
                    StartDoAfter(uid, args.Target, args.User, dumpable);
                },
                Text = Loc.GetString("dump-placeable-verb-name", ("surface", args.Target)),
                IconEntity = uid
            };
            args.Verbs.Add(verb);
        }
    }

    public void StartDoAfter(EntityUid storageUid, EntityUid? targetUid, EntityUid userUid, DumpableComponent dumpable)
    {
        if (!TryComp<SharedStorageComponent>(storageUid, out var storage) || storage.StoredEntities == null)
            return;

        float delay = storage.StoredEntities.Count * (float) dumpable.DelayPerItem.TotalSeconds * dumpable.Multiplier;

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(userUid, delay, new DumpableDoAfterEvent(), storageUid, target: targetUid, used: storageUid)
        {
            BreakOnTargetMove = true,
            BreakOnUserMove = true,
            NeedHand = true
        });
    }

    private void OnDoAfter(EntityUid uid, DumpableComponent component, DoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || !TryComp<SharedStorageComponent>(uid, out var storage) || storage.StoredEntities == null)
            return;

        Queue<EntityUid> dumpQueue = new();
        foreach (var entity in storage.StoredEntities)
        {
            dumpQueue.Enqueue(entity);
        }

        foreach (var entity in dumpQueue)
        {
            var transform = Transform(entity);
            _container.AttachParentToContainerOrGrid(transform);
            _transformSystem.SetLocalPositionRotation(transform, transform.LocalPosition + _random.NextVector2Box() / 2, _random.NextAngle());
        }

        if (args.Args.Target == null)
            return;

        if (HasComp<SharedDisposalUnitComponent>(args.Args.Target.Value))
        {
            foreach (var entity in dumpQueue)
            {
                _disposalUnitSystem.DoInsertDisposalUnit(args.Args.Target.Value, entity, args.Args.User);
            }
            return;
        }

        if (HasComp<PlaceableSurfaceComponent>(args.Args.Target.Value))
        {
            var targetPos = _xformQuery.GetComponent(args.Args.Target.Value).LocalPosition;

            foreach (var entity in dumpQueue)
            {
                _transformSystem.SetLocalPosition(entity, targetPos + _random.NextVector2Box() / 4);
            }
        }
    }
}