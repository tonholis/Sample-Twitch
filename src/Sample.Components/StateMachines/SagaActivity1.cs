namespace Sample.Components.StateMachines
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using GreenPipes;


    public class SagaActivity1 : Activity<OrderState, OrderSubmitted>
    {
        public SagaActivity1(Dependency dependency)
        {
            
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope(nameof(SagaActivity1));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderState, OrderSubmitted> context, Behavior<OrderState, OrderSubmitted> next)
        {
            Console.WriteLine($"I was here at {nameof(SagaActivity1)}");
            await next.Execute(context);
        }

        public async Task Faulted<TException>(BehaviorExceptionContext<OrderState, OrderSubmitted, TException> context, Behavior<OrderState, OrderSubmitted> next)
            where TException : Exception
        {
            await next.Faulted(context);
        }
    }
}