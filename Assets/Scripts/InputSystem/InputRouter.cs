using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InputSystem
{
    /// <summary>
    /// Routes input to contexts ordered by Priority (highest first).
    /// Contexts can be pushed and removed in any order.
    /// </summary>
    public class InputRouter
    {
        // priority queue of active contexts
        private static readonly List<IInputContext> Contexts = new();
        private static readonly Dictionary<GameAction, IInputContext> Owners = new();
        private static readonly List<IInputContext> Snapshot = new();

        public static IInputContext Top => Contexts.Count > 0 ? Contexts[^1] : null;
        public static bool IsActive(IInputContext ctx) => Contexts.Contains(ctx);

        public static void Dispatch(ActionEvent e)
        {
            // new events identify the owner context
            if (e.Phase == Phase.Started)
            {
                // a snapshot is necessary since a handler may push or remove contexts
                Snapshot.Clear();
                Snapshot.AddRange(Contexts);
                for (int i = Snapshot.Count - 1; i >= 0; i--)
                {
                    var ctx = Snapshot[i];
                    if (!Contexts.Contains(ctx)) continue;

                    // event matched by a context
                    if (ctx.Handle(e))
                    {
                        // pointer move event has no ownership
                        // a context that removed itself can't own
                        if (e.Action != GameAction.PointerMove && Contexts.Contains(ctx))
                            Owners[e.Action] = ctx;
                        return;
                    }
                    // event blocked by a context
                    if ((ctx.BlockedActions & e.Action) != 0) return;
                }
                return;
            }

            // ongoing events reuse the current owner context
            if (Owners.TryGetValue(e.Action, out var owner))
            {
                if (e.Phase == Phase.Released) Owners.Remove(e.Action);
                owner.Handle(e);
            }
        }

        public static void Push(IInputContext ctx)
        {
            if (Contexts.Contains(ctx))
            {
                Debug.LogWarning($"Push: {ctx} is already active");
                return;
            }

            int index = Contexts.FindLastIndex(c => c.Priority <= ctx.Priority) + 1;
            var below = new HashSet<IInputContext>(Contexts.Take(index));
            CancelWhere((action, owner) => below.Contains(owner) && (ctx.BlockedActions & action) != 0);

            Contexts.Insert(index, ctx);
        }

        public static void Remove(IInputContext ctx)
        {
            int index = Contexts.IndexOf(ctx);
            if (index < 0)
            {
                Debug.LogWarning($"Remove: {ctx} is not active");
                return;
            }

            CancelWhere((_, owner) => owner == ctx);
            Contexts.RemoveAt(index);
        }

        private static void CancelWhere(Func<GameAction, IInputContext, bool> match)
        {
            var toCancel = Owners.Where(kv => match(kv.Key, kv.Value)).ToList();
            foreach (var (action, owner) in toCancel)
            {
                Owners.Remove(action);
                owner.Handle(new ActionEvent { Action = action, Phase = Phase.Canceled });
            }
        }
    }
}
