using UnityEngine;

[RequireComponent(typeof(PlayerInteractionDetector))]
[RequireComponent(typeof(PlayerCondition))]
public class PlayerHeldInteraction : MonoBehaviour
{
    private PlayerInteractionDetector _interactionDetector;
    private PlayerCondition _condition;

    private IHoldInteractable _currentHoldInteractable;
    private InteractionContext _currentContext;
    private PlayerMovement _movement;
    private bool _isHolding;

    public bool IsHolding => _isHolding;

    private void Awake()
    {
        _interactionDetector = GetComponent<PlayerInteractionDetector>();
        _condition = GetComponent<PlayerCondition>();
        _movement = GetComponent<PlayerMovement>();
    }

    private void Reset()
    {
        _interactionDetector = GetComponent<PlayerInteractionDetector>();
        _condition = GetComponent<PlayerCondition>();
        _movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if(!_isHolding || _currentHoldInteractable == null) return;

        if (_condition.IsDead)
        {
            CancelHold();
            return;
        }

        bool isCompleted = _currentHoldInteractable.TickHold(_currentContext, Time.deltaTime);

        if (!isCompleted) return;

        FinishHold();
    }

    public bool TryBeginHold()
    {
        if (_isHolding) return false;

        if (_condition == null || !_condition.CanInteract || !_condition.CanReceiveInput) return false;

        if(!_interactionDetector.TryGetBestInteractable(out IInteractable interactable)) return false;

        if(interactable is not IHoldInteractable holdInteractable) return false;

        InteractionContext context = _interactionDetector.CreateInteractionContext();

        if (!holdInteractable.CanBeginHold(context)) return false;

        _currentHoldInteractable = holdInteractable;
        _currentContext = context;
        _isHolding = true;

        _condition.SetMovementBlocked(true);
        _movement.SetBuoyancyEnabled(false);

        _currentHoldInteractable.BeginHold(_currentContext);

        return true;
    }

    public void CancelHold()
    {
        if (!_isHolding) return;

        if(_currentHoldInteractable != null)
        {
            _currentHoldInteractable.CancelHold(_currentContext);
        }

        FinishHold();
    }

    private void FinishHold()
    {
        _condition.SetMovementBlocked(false);
        _movement.SetBuoyancyEnabled(true);

        _currentHoldInteractable = null;
        _currentContext = default;
        _isHolding = false;
    }
}
