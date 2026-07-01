using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Common.Events;

/// <summary>
/// MediatR adapter that wraps a plain domain event (which lives in the Domain layer
/// and has no MediatR dependency) into an <see cref="INotification"/> so it can be
/// published through the mediator and handled by application-level subscribers.
/// </summary>
/// <typeparam name="TDomainEvent">The type of the wrapped domain event.</typeparam>
public class DomainEventNotification<TDomainEvent> : INotification
{
    /// <summary>
    /// Gets the wrapped domain event instance.
    /// </summary>
    public TDomainEvent DomainEvent { get; }

    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}
