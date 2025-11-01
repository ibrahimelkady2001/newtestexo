using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace EXOApp.Models
{
    class BaseModel : INotifyPropertyChanged
    {
        /// <summary>
        ///  This Function is unused
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// This function is unused
        /// </summary>
        /// <param name="propertyName">Unused</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
