using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class OxygenGaugeView : MonoBehaviour
{
    [SerializeField] private OxygenViewModel _viewModel;
    [SerializeField] private Slider _oxygenSlider;
    [SerializeField] private TMP_Text _oxygenText;

    private void OnEnable()
    {
        if (_viewModel == null)
        {
            return;
        }

        // ViewModel의 표시 상태 변경 이벤트를 구독한다.
        _viewModel.ViewStateChanged += HandleViewStateChanged;
    }

    private void OnDisable()
    {
        if (_viewModel == null)
        {
            return;
        }

        // 비활성화 시 구독을 해제한다.
        _viewModel.ViewStateChanged -= HandleViewStateChanged;
    }

    private void HandleViewStateChanged(OxygenViewState viewState)
    {
        // View는 전달받은 표시 상태를 화면에 반영만 한다.
        if (_oxygenSlider != null)
        {
            _oxygenSlider.value = viewState.Ratio;
        }

        if (_oxygenText != null)
        {
            _oxygenText.text = viewState.AmountText;
        }
    }
}