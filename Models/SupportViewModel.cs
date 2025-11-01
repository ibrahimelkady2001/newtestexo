using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EXOApp.Models
{
    /// <summary>
    /// View Model for the support view
    /// </summary>
    internal class SupportViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand settingsChangeCommand { protected set; get; }
        public ICommand exitCommand { protected set; get; }

        ObservableCollection<HelpItem> helpControlPC = new ObservableCollection<HelpItem>();
        ObservableCollection<HelpItem> helpDoorMechanism = new ObservableCollection<HelpItem>();
        ObservableCollection<HelpItem> helpPod = new ObservableCollection<HelpItem>();
        ObservableCollection<HelpItem> helpReservoir = new ObservableCollection<HelpItem>();
        ObservableCollection<HelpItem> helpPump = new ObservableCollection<HelpItem>();


        ObservableCollection<HelpItem> viewedHelp = new ObservableCollection<HelpItem>();
        public ObservableCollection<HelpItem> ViewedHelp { get { return viewedHelp; } }

        ObservableCollection<SideListItem> sideList = new ObservableCollection<SideListItem>();
        public ObservableCollection<SideListItem> SideList { get { return sideList; } }

        SideListItem currentItem;
        public Action fadeInOutEvent;


        /// <summary>
        /// Contructor for the support view model
        /// </summary>
        public SupportViewModel()
        {
            settingsChangeCommand = new Command(() => settingsChange());
            exitCommand = new Command(async () => await onExit());
            doLists();

        }

        /// <summary>
        /// Fills each category with the related support items
        /// </summary>
        void doLists()
        {
            helpControlPC.Add(createHelpItem("Control PC", 0));
            helpControlPC.Add(createHelpItem("Control PC", 1));
            helpControlPC.Add(createHelpItem("Control PC", 2));
            helpControlPC.Add(createHelpItem("Control PC", 3));
            helpControlPC.Add(createHelpItem("Control PC", 4));

            helpDoorMechanism.Add(createHelpItem("Door Mechanism", 0));
            helpDoorMechanism.Add(createHelpItem("Door Mechanism", 1));
            helpDoorMechanism.Add(createHelpItem("Door Mechanism", 2));
            helpDoorMechanism.Add(createHelpItem("Door Mechanism", 3));
            helpDoorMechanism.Add(createHelpItem("Door Mechanism", 4));
            helpDoorMechanism.Add(createHelpItem("Door Mechanism", 5));
            helpDoorMechanism.Add(createHelpItem("Door Mechanism", 6));

            helpPod.Add(createHelpItem("Pod", 0));
            helpPod.Add(createHelpItem("Pod", 1));
            helpPod.Add(createHelpItem("Pod", 2));
            helpPod.Add(createHelpItem("Pod", 3));
            helpPod.Add(createHelpItem("Pod", 4));
            helpPod.Add(createHelpItem("Pod", 5));
            helpPod.Add(createHelpItem("Pod", 6));

            helpReservoir.Add(createHelpItem("Pump", 0));
            helpReservoir.Add(createHelpItem("Pump", 1));
            helpReservoir.Add(createHelpItem("Pump", 2));

            helpPump.Add(createHelpItem("Reservoir Tanks", 0));



            SideList.Add(new SideListItem(helpControlPC, "Control PC"));
            SideList.Add(new SideListItem(helpDoorMechanism, "Door Mechanism"));
            SideList.Add(new SideListItem(helpPod, "Pod"));
            SideList.Add(new SideListItem(helpReservoir, "Pump"));
            SideList.Add(new SideListItem(helpPump, "Reservoir Tanks"));
            CurrentItem = SideList[0];

            foreach (SideListItem side in SideList)
            {
                side.setSelected(false);
            }
            CurrentItem.setSelected(true);
            viewedHelp = CurrentItem.HelpItems;

        }

        /// <summary>
        /// Creates each support item for the list. The support text and titles are found in SupportTextHelper.
        /// </summary>
        /// <param name="category">Support Category</param>
        /// <param name="parameter">Which entry to get, as specified in SupportTextHelper</param>
        /// <returns></returns>
        HelpItem createHelpItem(string category, int parameter)
        {
            string title;
            string description;
            Helpers.SupportTextHelper.GetSupportInformation(category, parameter, out title, out description);
            return new HelpItem(title, description);
        }

        /// <summary>
        /// Change the active support category
        /// </summary>
        void settingsChange()
        {
            fadeInOutEvent.Invoke();
            foreach (SideListItem side in SideList)
            {
                side.setSelected(false);
            }
            CurrentItem.setSelected(true);
            viewedHelp = CurrentItem.HelpItems;
            PropertyChanged(this, new PropertyChangedEventArgs("ViewedHelp"));

        }

        /// <summary>
        /// Exits the support page and returns to the previous page.
        /// </summary>
        public async Task onExit()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        /// <summary>
        /// Internal class for each help item
        /// </summary>
        internal class HelpItem
        {
            public string Title { get; set; }
            public string Description { get; set; }
            
            /// <summary>
            /// Contructor for help item
            /// </summary>
            public HelpItem(string title, string description)
            {
                Title = title;
                Description = description;
            }

        }


        /// <summary>
        /// Internal class for the side list of categories
        /// </summary>
        internal class SideListItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public ObservableCollection<HelpItem> HelpItems { get; set; }
            public string Title { get; set; }
            bool isEnabled { get; set; }
            Color textColor { get; set; }

            /// <summary>
            /// Contructor for each side list item
            /// </summary>
            public SideListItem(ObservableCollection<HelpItem> helpItems, string title)
            {
                HelpItems = helpItems;
                Title = title;
            }

            /// <summary>
            /// Sets the sideListItem as selected
            /// </summary>
            public void setSelected(bool selected)
            {
                IsEnabled = selected;
                if (selected)
                {
                    TextColor = Colors.Black;
                }
                else
                {
                    TextColor = Colors.White;
                }
            }




            public bool IsEnabled
            {
                set
                {
                    if (isEnabled != value)
                    {
                        isEnabled = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled"));
                        }
                    }
                }
                get
                {
                    return isEnabled;
                }
            }

            public Color TextColor
            {
                set
                {
                    if (textColor != value)
                    {
                        textColor = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("TextColor"));
                        }
                    }
                }
                get
                {
                    return textColor;
                }
            }













        }

        public SideListItem CurrentItem
        {
            set
            {
                if (currentItem != value)
                {
                    currentItem = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("CurrentItem"));
                    }
                }
            }
            get
            {
                return currentItem;
            }
        }

    }
}
