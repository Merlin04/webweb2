using System;
using BlazorEditor.Data;

namespace BlazorEditor.Shared
{
    public class OMCSService
    {
        private dynamic _currentlySelected = new NonSpecificItem();
        public dynamic CurrentlySelected
        {
            get => _currentlySelected;
            set
            {
                if (_currentlySelected.GetType() != value.GetType() || _currentlySelected != value)
                {
                    _currentlySelected = value;
                    Notify?.Invoke();

                }
            }
        }

        public event Action Notify;
        
    }
}