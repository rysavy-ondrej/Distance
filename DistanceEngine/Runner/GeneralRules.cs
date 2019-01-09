using Distance.Runtime;
using NRules.Fluent.Dsl;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distance.Engine
{
    public class EmitEvents : Rule
    {
        public override void Define()
        {
            DistanceEvent @event = null;
            When()
                .Match(() => @event);

            Then()
                .Do(ctx => ctx.Event(@event));
        }
    }
}
